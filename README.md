# Ride Sharing and Fleet Management

Індивідуальний навчальний проєкт для лабораторних робіт № 1–4 з дисципліни «Проєктування архітектури, специфікація та стандарти інженерії». Обрано варіант 6: сервіс поїздок і керування автопарком.

## Призначення

Система з'єднує пасажирів із водіями, підбирає водія поблизу, показує стан поїздки та передає оплату зовнішньому платіжному сервісу. Водій може керувати доступністю та отримувати замовлення. Оператор бачить поїздки, звернення та повідомлення про небезпеку.

У цій роботі підготовлено опис системи, вимоги якості, сценарії перевірки, карту архітектурних ракурсів і три початкові записи ADR.

## Лабораторні роботи 2–4

- ЛР2: C4 та UML-діаграми у `diagrams/`, їхні PNG у `docs/diagrams/`, звіт у `docs/architecture_diagrams_report.md`.
- ЛР3: REST контракт OpenAPI у `api/openapi.yaml`, gRPC контракт у `api/service.proto`, згенеровані C# стаби у `api/generated/csharp/`. Звіт: `docs/api_specification_report.md`.
- ЛР4: модульне ядро в `src/Domain`, `src/Application`, `src/Infrastructure`, `src/WebAPI`; модульні тести в `tests/RideShare.Tests`. Звіт: `docs/clean_architecture_report.md`.

## Перевірка

```powershell
dotnet build RideShare.sln
dotnet test tests/RideShare.Tests/RideShare.Tests.csproj
dotnet run --project tools/PayloadSize/PayloadSize.csproj
```

Щоб перевірити OpenAPI через Prism, спочатку запустіть у першому терміналі `tools/prism/node_modules/.bin/prism.cmd mock api/openapi.yaml --errors -p 4010`, а в іншому виконайте `tools/test_prism.ps1`. JSON результати та знімок збережені у `docs/evidence/`.

## Межі системи

Система відповідає за облікові записи, пошук поїздки, підбір водія, стани поїздки, тариф, сповіщення та журнал подій. Зовнішніми залишаються картографічний сервіс, провайдер оплати та провайдер входу. Дані банківських карток система не зберігає.

## Структура

- `docs/1_system_description.md` описує призначення системи, її межі та зацікавлені сторони.
- `docs/2_nfr_specification.md` містить вимоги якості з вимірюваними критеріями.
- `docs/3_quality_scenarios.md` містить п'ять сценаріїв якості.
- `docs/4_viewpoints_map.md` пов'язує архітектурні ракурси, вигляди й аудиторії.
- `docs/adrs/0001-architectural-style-selection.md` фіксує вибір стилю системи.
- `docs/adrs/0002-database-and-storage-strategy.md` фіксує рішення щодо зберігання даних.
- `docs/adrs/0003-authentication-and-security-framework.md` фіксує підхід до входу й доступу.
