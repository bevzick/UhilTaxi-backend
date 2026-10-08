# Підключення фронтенду UhilTaxi

## Адреса API

Для Vite на `http://localhost:5173` і Docker API на `http://localhost:8080` базова адреса запитів — `http://localhost:8080/api/v1`. Наприклад, вхід: `POST /api/v1/auth/login`, реєстрація: `POST /api/v1/auth/register`.

У конфігурації HTTP-клієнта фронтенду задайте цю базову адресу. Якщо клієнт додає `/api` самостійно, узгодьте префікс, щоб не дублювати його. Маршрути `/api/auth/login` та `/api/auth/register` у backend відсутні.

## CORS у backend

У `.env`:

```dotenv
CORS_ALLOWED_ORIGINS=http://localhost:5173,http://127.0.0.1:5173
```

Після зміни виконайте `make up`. Для запуску без Docker API читає цю змінну з `.env`; перезапустіть процес. Альтернатива — змінна середовища `Cors__AllowedOrigins`, яка має пріоритет над `.env`.

Дозволені origins задаються через кому, без шляху, кінцевого `/` та `*`. Схема, хост і порт мають збігатися з адресою сторінки в браузері. Якщо Vite вибрав інший порт, явно додайте його до списку. Порожнє значення вимикає доступ з інших origins. У Development без перевизначення дозволено localhost і 127.0.0.1 на порту 5173; для production задайте адреси свого фронтенду явно.

Політика підтримує preflight `OPTIONS`, заголовки JSON/Bearer і credentials для refresh-cookie. У клієнті використовуйте `credentials: 'include'` для Fetch або `withCredentials: true` для Axios:

```javascript
const response = await fetch('http://localhost:8080/api/v1/auth/refresh', {
  method: 'POST',
  credentials: 'include',
});
```

Використовуйте однаковий хост для фронтенду та API: наприклад, `localhost` для обох. Refresh-cookie має `SameSite=Strict`, тому cross-site розгортання потребує окремого рішення; CORS не змінює політику cookie. Для production використовуйте HTTPS і розміщення в межах одного site або reverse proxy.

## CSP у фронтенді

Повідомлення `violates ... Content Security Policy directive: connect-src` означає, що браузер блокує запит за політикою сторінки до звернення до API. Зміни CORS у backend не можуть змінити CSP іншого сервера.

У фронтенді знайдіть CSP у `index.html` (`<meta http-equiv="Content-Security-Policy">`), заголовках Vite, reverse proxy або вебсервера. У директиві `connect-src` дозвольте адресу фактично запущеного API. Для наведеного локального налаштування приклад директиви:

```text
connect-src 'self' http://localhost:8080 https://nominatim.openstreetmap.org ws://localhost:5173;
```

Збережіть інші потрібні директиви та origins вашої політики. `ws://localhost:5173` потрібен для Vite HMR у Development. Якщо CSP задана одночасно в HTML і HTTP-заголовку, кожна політика має дозволяти API; додавання другої, менш суворої політики не скасовує першу. Після зміни перезапустіть Vite та перезавантажте сторінку.

## Перевірка

```bash
curl -i -X OPTIONS http://localhost:8080/api/v1/auth/login \
  -H 'Origin: http://localhost:5173' \
  -H 'Access-Control-Request-Method: POST' \
  -H 'Access-Control-Request-Headers: content-type,authorization'
```

Очікується `204`, `Access-Control-Allow-Origin: http://localhost:5173` і `Access-Control-Allow-Credentials: true`. Це перевіряє backend; CSP перевіряйте в браузері на сторінці фронтенду.

Автоматична перевірка CORS, без підключення до БД:

```bash
dotnet build src/UhilTaxi.Api/UhilTaxi.Api.csproj
python3 tests/cors_integration.py
```
