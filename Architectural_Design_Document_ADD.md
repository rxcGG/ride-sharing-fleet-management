# Architectural Design Document

## Система Ride Sharing & Fleet Management

Студент: Єгоров Ігор Антонович<br>
Група: 3ПР ПІ<br>
Тег у репозиторії: lr6<br>
Репозиторій: https://github.com/rxcGG/ride-sharing-fleet-management

### 1. Вступ та цілі системи

Пасажир замовляє поїздку, водій її виконує, а оператор бачить стан і звернення. Навчальний стенд показує роботу ядра поїздок, API та обробку подій. [Опис системи](docs/1_system_description.md).

### 2. Архітектурні обмеження та NFR (ISO/IEC 25010)

Демонстраційний процес працює на .NET 10. Стан поїздок і черг зберігається в пам’яті. [Початкові вимоги та критерії перевірки](docs/2_nfr_specification.md).

| Вимога | Рішення або зв’язок із розгортанням |
| --- | --- |
| Пошук водія до 2 с при навантаженні | [ADR-0002](docs/adrs/0002-database-and-storage-strategy.md) передбачає географічний індекс у повній системі. |
| Повторний запит не створює другу поїздку | [Сценарії якості](docs/3_quality_scenarios.md) передбачають ключ ідемпотентності запиту. У стенді перевіряється окремий випадок повторної доставки події через EventId. |
| Доступ до поїздки лише для її учасників | [ADR-0003](docs/adrs/0003-authentication-and-security-framework.md) визначає ролі й перевірку доступу. Стенд доступний через loopback. |
| Підтвердження й збереження SOS | [Діаграма сценарію SOS](docs/diagrams/sequence_sos.png) описує шлях оповіщення та зберігання сигналу для оператора. |
| Модульні тести для правил поїздки | Сценарії викликають порти, що можна підміняти в [модульних тестах](tests/RideShare.Tests/CompleteTripHandlerTests.cs). |

### 3. Контекст та межі системи

[Діаграма C4 Level 1](docs/diagrams/context_booking.png) показує пасажира, водія, оператора й зовнішні інтеграції. Картографічний і платіжний сервіси належать до контексту повної системи. Локальний запит створення поїздки виконується без з’єднання з ними.

### 4. Архітектурна стратегія та ADR

- [ADR-0001](docs/adrs/0001-architectural-style-selection.md) описує модульний моноліт і межі ядра.
- [ADR-0002](docs/adrs/0002-database-and-storage-strategy.md) визначає сховища повної системи.
- [ADR-0003](docs/adrs/0003-authentication-and-security-framework.md) визначає вхід і доступ.
- [ADR-0004](docs/adrs/0004-deployment-topology.md) пояснює вибір одного контейнера для демонстрації.

### 5. Структурні діаграми

[C4 Level 2](docs/diagrams/container_model.png) описує повну систему. На навчальному стенді контейнер backend-api поєднує HTTP/gRPC, ядро та дві фонові підписки. [Діаграма компонентів](docs/diagrams/component_trip_api.png) і [класи поїздки](docs/diagrams/domain_classes.png) показують внутрішні залежності.

### 6. Динаміка взаємодії

[Послідовність замовлення поїздки](docs/diagrams/sequence_request_trip.png), [контракт OpenAPI](api/openapi.yaml) і [контракт gRPC](api/service.proto) задають загальну модель взаємодії. У демонстрації POST /trips передає запит до CreateTripHandler. Після збереження він видає TripRequestedEvent через IEventPublisher. Журнал і сповіщення читають свої черги окремо. [Каталог подій і перевірка повтору](docs/event_driven_report.md).

### 7. Фізична архітектура

[Діаграма розгортання](docs/diagrams/deployment.png) і [джерело PlantUML](docs/diagrams/deployment.puml) узгоджені з [docker-compose.yml](docker-compose.yml). Docker запускає backend-api з артефактом RideShare.WebAPI.dll. HTTP/JSON працює через TCP 8080, gRPC через HTTP/2 на TCP 8081. Черги і сховище знаходяться всередині цього процесу.

```bash
DEMO_REPEAT_EVENTS=true docker compose up --build -d
python3 tools/verify_events.py --compose --url http://127.0.0.1:8080
docker compose down
```

### 8. Компроміси та ризики

Один контейнер спрощує демонстрацію і перевірку. Після перезапуску дані в пам’яті зникають. Для трьох екземплярів споживача потрібні мережевий брокер, спільне довговічне сховище оброблених EventId та атомарний запис результату.
