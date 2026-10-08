# UhilTaxi Backend

Backend системи UhilTaxi для керування замовленнями таксі та поїздками. API підтримує ролі клієнта, водія й адміністратора, авторизацію, тарифи та промокоди.

Репозиторій містить серверну частину. Запланований фронтенд на Vue не входить до цього репозиторію.

## Реалізовані можливості

- Реєстрація клієнтів, вхід, оновлення токенів, вихід і керування власним профілем.
- Адміністративне керування клієнтами, водіями, тарифами та промокодами.
- Оцінка вартості, створення, призначення та скасування замовлень; історія зміни статусів.
- Прийняття замовлення водієм, прибуття, початок і завершення поїздки з розрахунком фактичної вартості.
- Перевірка ролей, активності користувачів і доступу до власних ресурсів.

Цикл замовлення: `pending → accepted → driver_arriving → in_progress → completed`. Скасувати замовлення можна до початку поїздки.

Для прийняття замовлення водію потрібні відкрита зміна й активне авто відповідного класу. API керування змінами та автопарком ще потребує реалізації. Контролери платежів, відгуків, звітів та інших запланованих модулів також містять заготовки.

Оцінка маршруту використовує `MockMapsService`: відстань між координатами з коефіцієнтом 1,25 та середню швидкість 30 км/год. Інтеграція з дорожніми картами ще не реалізована.

## Технології та вимоги

| Компонент | Технологія |
| --- | --- |
| API | C#, .NET 10, ASP.NET Core |
| Доступ до даних | Entity Framework Core 9, Pomelo MySQL |
| База даних | MySQL 8.4 |
| Авторизація | JWT access token та refresh token у HttpOnly cookie |
| Документація API | Swagger / OpenAPI |
| Тести | xUnit, Python 3 для HTTP/MySQL перевірок |

Для запуску в контейнерах потрібні Git, Docker із плагіном Compose та GNU Make. Для локальної збірки й модульних тестів додатково потрібен .NET SDK 10. Для інтеграційних тестів — Python 3; додаткові Python-пакети не потрібні.

## Структура

```text
src/
  UhilTaxi.Api/             HTTP API, авторизація, Swagger
  UhilTaxi.Application/     Сервіси, контракти, валідація
  UhilTaxi.Domain/          Сутності, статуси, бізнес-правила
  UhilTaxi.Infrastructure/  EF Core, репозиторії, інтеграції
database/
  init.sql                 Початкова схема MySQL
  optional_promocodes.sql  Доповнення для старих баз без промокодів
docs/
  orders-trips.md           Контракти та правила orders/trips
tests/
  UhilTaxi.Tests/           Модульні тести
  orders_trips_integration.py
```

## Швидкий запуск через Docker

Клонуйте гілку розробки:

```bash
git clone --branch develop https://github.com/bevzick/UhilTaxi-backend.git
cd UhilTaxi-backend
cp .env.example .env
```

Якщо `.env` уже існує, збережіть його налаштування. У новому `.env` замініть `MYSQL_PASSWORD`, `MYSQL_ROOT_PASSWORD` і `JWT_KEY`. Ключ JWT має бути криптографічно випадковим і містити щонайменше 32 байти UTF-8. Наприклад, згенеруйте його та скопіюйте результат у `JWT_KEY`:

```bash
openssl rand -base64 48
```

Для локального `dotnet run` також оновіть пароль у `ConnectionStrings__Default`. Файл `.env` виключений із Git; не додавайте його до комітів.

Перевірте конфігурацію та запустіть API разом із MySQL:

```bash
docker compose config --quiet
make up
make ps
curl --fail http://localhost:8080/health
```

За типовими налаштуваннями:

- API: `http://localhost:8080`.
- Swagger: `http://localhost:8080/swagger`.
- OpenAPI JSON: `http://localhost:8080/swagger/v1/swagger.json`.
- MySQL: `localhost:3306`.

Swagger доступний у середовищі `Development`. `/health` перевіряє доступність HTTP-сервера; підключення до БД перевіряйте через роботу API або інтеграційні тести.

### Основні змінні `.env`

| Змінна | Призначення |
| --- | --- |
| `MYSQL_DATABASE` | `uhiltaxi`; ця назва також задана в `database/init.sql` |
| `MYSQL_USER` | Користувач БД для API |
| `MYSQL_PASSWORD`, `MYSQL_ROOT_PASSWORD` | Паролі користувача та root MySQL |
| `MYSQL_PORT` | Порт MySQL на хості; типовий — `3306` |
| `API_PORT` | Порт Docker API на хості; типовий — `8080` |
| `ASPNETCORE_ENVIRONMENT` | `Development` для локальної розробки |
| `JWT_KEY` | Секрет підпису JWT |
| `JWT_ISSUER`, `JWT_AUDIENCE` | Очікувані видавець та аудиторія токена |
| `JWT_ACCESS_MINUTES`, `JWT_REFRESH_DAYS` | Строки дії access і refresh токенів |
| `ConnectionStrings__Default` | Підключення до БД для запуску API на хості |
| `ADMIN_SEED_ENABLED` | Увімкнення створення першого адміністратора в Development |

У Docker API підключається до MySQL через ім'я сервісу `mysql`; Compose формує connection string із `MYSQL_*`. Для запуску API на хості використовуйте `localhost`, значення `MYSQL_PORT` та той самий пароль у `ConnectionStrings__Default`.

Після зміни `.env` виконайте `make up`, щоб Compose застосував нові параметри контейнерів. Звичайний `make restart` не оновлює їхні змінні середовища.

### Перший адміністратор

Самостійна реєстрація створює клієнта. Для початкового адміністратора в локальному `Development`:

1. Установіть `ADMIN_SEED_ENABLED=true` у `.env`.
2. Заповніть `ADMIN_SEED_PHONE`, `ADMIN_SEED_PASSWORD`, `ADMIN_SEED_FIRST_NAME` і `ADMIN_SEED_LAST_NAME`; email необов'язковий.
3. Використовуйте телефон у міжнародному форматі та пароль щонайменше з 12 символів.
4. Запустіть `make up`. Схема БД має бути створена до запуску API.
5. Після створення адміністратора поверніть `ADMIN_SEED_ENABLED=false` і знову виконайте `make up`.

Якщо адміністратор уже існує, повторне створення пропускається. Якщо телефон або email зайняті іншим користувачем, виберіть вільні значення.

## Запуск API без Docker-збірки

MySQL можна залишити в контейнері, а API запускати через .NET SDK. Спочатку налаштуйте `.env`, як описано вище, з підключенням `ConnectionStrings__Default` до MySQL на хості.

```bash
docker compose up -d mysql
dotnet restore UhilTaxi.slnx
dotnet build UhilTaxi.slnx --no-restore
dotnet run --project src/UhilTaxi.Api/UhilTaxi.Api.csproj --launch-profile http
```

HTTP-профіль використовує `Development` і порт `5069`. Swagger буде доступний за адресою `http://localhost:5069/swagger`. API читає `.env` із кореня репозиторію; змінні середовища процесу мають пріоритет.

## Перевірки

Модульні тести:

```bash
dotnet test UhilTaxi.slnx
```

Інтеграційні тести для API, запущеного через `make up`:

```bash
python3 tests/orders_trips_integration.py
```

Для локального HTTP-профілю:

```bash
UHILTAXI_TEST_URL=http://localhost:5069 python3 tests/orders_trips_integration.py
```

Скрипт потребує доступу до Docker-контейнера MySQL, створює унікальні тестові дані й видаляє їх після перевірок. Запускайте його на окремій базі для розробки. Типова назва контейнера — `uhiltaxi-mysql`; для іншої назви задайте `UHILTAXI_MYSQL_CONTAINER`.

Перевірки охоплюють реєстрацію, промокоди, цикл замовлення та поїздки, права доступу, розрахунок вартості й конкурентне прийняття замовлень.

## Основні API

JSON використовує `snake_case`. Захищені маршрути потребують заголовка `Authorization: Bearer <access_token>` та активного користувача. Клієнти й водії мають доступ до власних ресурсів; адміністратор — до адміністративних маршрутів. Помилки повертаються у форматі ProblemDetails.

| Група | Префікс |
| --- | --- |
| Реєстрація, вхід, refresh, logout | `/api/v1/auth` |
| Власний профіль | `/api/v1/me` |
| Публічний список активних тарифів | `/api/v1/tariffs` |
| Замовлення та поїздки клієнта | `/api/v1/orders`, `/api/v1/trips` |
| Замовлення та поїздки водія | `/api/v1/driver/orders`, `/api/v1/driver/trips` |
| Клієнти та водії для адміністратора | `/api/v1/admin/clients`, `/api/v1/admin/drivers` |
| Тарифи та промокоди для адміністратора | `/api/v1/admin/tariffs`, `/api/v1/admin/promocodes` |
| Замовлення та поїздки для адміністратора | `/api/v1/admin/orders`, `/api/v1/admin/trips` |

Детальні контракти, приклади запитів та бізнес-правила описані в [документації orders/trips](docs/orders-trips.md). Повний перелік реалізованих маршрутів дивіться у Swagger.

## Корисні команди

| Команда | Дія |
| --- | --- |
| `make help` | Список команд |
| `make up` | Збірка й запуск контейнерів |
| `make down` | Видалення контейнерів зі збереженням тому БД |
| `make stop` / `make start` | Зупинка / запуск наявних контейнерів |
| `make logs-api` / `make logs-db` | Перегляд логів API / MySQL |
| `make ps` | Стан контейнерів |
| `make build` / `make rebuild` | Збірка API / збірка без кешу |
| `make db-shell` / `make api-shell` | Консоль MySQL / контейнера API |

`make reset` видаляє контейнери **та том із усіма даними БД**. `make config` друкує підсумкову конфігурацію разом із секретами; для перевірки без такого виводу використовуйте `docker compose config --quiet`.

## Якщо запуск не вдається

- **Помилка JWT:** перевірте наявність `.env` і довжину `JWT_KEY`, потім виконайте `make up`.
- **Порт зайнятий:** змініть `API_PORT` або `MYSQL_PORT`. Для API на хості також оновіть порт у `ConnectionStrings__Default`.
- **MySQL відхиляє пароль:** зміна `.env` не змінює пароль користувача в уже створеному томі. Узгодьте налаштування з наявною БД.
- **У старій БД немає таблиць:** MySQL виконує `database/init.sql` автоматично лише під час першої ініціалізації порожнього тому. Для наявної бази застосуйте необхідні SQL-зміни окремо, попередньо створивши резервну копію. У проєкті наразі немає EF-міграцій.
- **Помилка збірки `DiscountType`:** у поточному `develop` тип промокоду узгоджено з enum. Переконайтеся, що ваша гілка містить актуальні зміни `develop`.
- **HTTP 403 під час завантаження .NET Docker-образу:** перевірте мережевий доступ до Microsoft Container Registry та CDN. У хмарному середовищі причиною може бути обмеження проксі; локальний запуск через SDK дозволяє перевірити API без збірки Docker-образу.

Логи для діагностики: `make logs-api` та `make logs-db`. Налаштування Compose призначені для локальної розробки; перед розгортанням налаштуйте HTTPS, керування секретами та доступ до БД.

## Робота з гілками

- `main` — основна гілка для стабільних змін.
- `develop` — інтеграція розробки.
- `feature/*` — окремі функції та документація.

Нову feature створюйте від актуального `develop`:

```bash
git switch develop
git pull --ff-only origin develop
git switch -c feature/my-feature
# Внесіть зміни та перевірте їх.
git add <files>
git commit -m "feat: describe the change"
git push -u origin feature/my-feature
```

Після публікації відкрийте pull request із feature-гілки в `develop`.
