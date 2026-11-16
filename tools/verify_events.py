"""Перевірка від HTTP-запиту до обробки події поїздки."""
import argparse
import json
import os
import re
import subprocess
import tempfile
import time
from contextlib import contextmanager
from pathlib import Path
from urllib.error import URLError
from urllib.request import Request, urlopen

ROOT = Path(__file__).resolve().parents[1]
APP = ROOT / "src/WebAPI/bin/Debug/net10.0/RideShare.WebAPI.dll"


@contextmanager
def local_api(url, fail):
    env = dict(os.environ, Demo__RepeatEvents="true",
               Demo__FailNotifications=str(fail).lower(),
               Logging__Console__FormatterOptions__SingleLine="true")
    with tempfile.TemporaryDirectory() as folder:
        log_path = Path(folder) / "api.log"
        with log_path.open("wb") as log:
            process = subprocess.Popen(["dotnet", str(APP), "--urls", url],
                                       cwd=ROOT, env=env, stdout=log, stderr=subprocess.STDOUT)

        def output():
            return log_path.read_text(encoding="utf-8", errors="replace")

        try:
            yield output
        finally:
            if process.poll() is None:
                process.terminate()
            process.wait(timeout=10)


@contextmanager
def compose_api():
    def output():
        return subprocess.check_output(["docker", "compose", "logs", "--no-color", "backend-api"],
                                       cwd=ROOT, encoding="utf-8")
    yield output


def verify(url, output, fail):
    for _ in range(60):
        try:
            with urlopen(url + "/health", timeout=2) as health:
                if json.load(health)["status"] == "ok":
                    break
        except (URLError, ConnectionError, TimeoutError):
            time.sleep(0.5)
    else:
        raise RuntimeError("API не відповів.\n" + output())

    request = Request(url + "/trips", headers={"Content-Type": "application/json"},
                      data=json.dumps({"passengerId": "de64a263-eb9a-42ec-babc-80eace412006",
                                       "pickup": {"latitude": 49.42, "longitude": 26.98},
                                       "dropoff": {"latitude": 49.43, "longitude": 26.99},
                                       "fareEstimate": 100}).encode())
    with urlopen(request, timeout=5) as response:
        trip = json.load(response)
        if response.status != 201 or trip["status"] != "REQUESTED":
            raise RuntimeError(f"Неочікувана відповідь API: {response.status}, {trip}")
        print(f"POST /trips → HTTP {response.status}; стан {trip['status']}; поїздка {trip['id']}")

    for _ in range(30):
        logs = output()
        ids = re.findall(r"Опубліковано TripRequestedEvent ([\w-]+)", logs)
        journal = [line for line in logs.splitlines() if "Журнал: записів " in line and trip["id"] in line]
        notices = [line for line in logs.splitlines() if "Сповіщення: записів " in line and trip["id"] in line]
        for event_id in set(ids):
            duplicate = f"повтор {event_id} пропущено"
            failure = f"помилка обробки {event_id}"
            if duplicate not in logs or len(journal) != 1 or ids.count(event_id) != 2:
                continue
            if fail and (logs.count(failure) != 2 or notices):
                continue
            if not fail and (logs.count(duplicate) != 2 or len(notices) != 1):
                continue
            for line in logs.splitlines():
                if event_id in line or line in journal or line in notices:
                    print(line.strip())
            print("Перевірку пройдено: журнал змінено один раз; " +
                  ("збій сповіщень не завадив запиту і журналу." if fail else
                   "сповіщення підготовлено один раз."))
            return
        time.sleep(0.5)
    raise RuntimeError("Подію або її повтор не підтверджено.\n" + output())


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--url", default="http://127.0.0.1:5088")
    parser.add_argument("--compose", action="store_true")
    parser.add_argument("--fail-notifications", action="store_true")
    args = parser.parse_args()
    api = compose_api() if args.compose else local_api(args.url, args.fail_notifications)
    with api as output:
        verify(args.url, output, args.fail_notifications)


if __name__ == "__main__":
    main()
