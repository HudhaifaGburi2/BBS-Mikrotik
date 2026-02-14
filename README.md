# DOSHI - نظام إدارة فواتير الإنترنت مع MikroTik

نظام متكامل لإدارة اشتراكات الإنترنت والفوترة مع تكامل MikroTik PPPoE.

---

## المتطلبات الأساسية

| المكون | الإصدار المطلوب | ملاحظات |
|--------|----------------|---------|
| .NET SDK | 9.0+ | [تحميل](https://dotnet.microsoft.com/download/dotnet/9.0) |
| Node.js | 20+ | [تحميل](https://nodejs.org/) |
| SQL Server | 2019+ | أو Azure SQL Edge للأجهزة ARM64 |
| Docker | 24+ | مطلوب فقط إذا تستخدم SQL Server عبر Docker |
| MikroTik Router | RouterOS 6/7 | اختياري - للتكامل مع PPPoE |

---

## هيكل المشروع

```
BBS-Mikrotik/
├── BroadbandBillingSystem.sln          # ملف الحل .NET
├── src/                                 # الخادم الخلفي (Backend)
│   ├── BroadbandBilling.API/           # طبقة API - Controllers, Middleware
│   ├── BroadbandBilling.Application/   # طبقة التطبيق - Commands, Queries, DTOs
│   ├── BroadbandBilling.Domain/        # طبقة النطاق - Entities, Enums, ValueObjects
│   └── BroadbandBilling.Infrastructure/# طبقة البنية التحتية - EF Core, Services
├── frontend-vue/                        # الواجهة الأمامية (Frontend)
│   ├── src/
│   │   ├── views/                      # صفحات التطبيق
│   │   ├── stores/                     # Pinia stores
│   │   ├── services/                   # HTTP client
│   │   ├── layouts/                    # Admin + Client layouts
│   │   ├── components/                 # مكونات مشتركة
│   │   ├── composables/                # Vue composables
│   │   ├── router/                     # Vue Router
│   │   └── types/                      # TypeScript types
│   ├── public/                         # ملفات ثابتة (favicon, صور)
│   ├── package.json
│   └── vite.config.ts
└── .gitignore
```

---

## 1. إعداد قاعدة البيانات

### الخيار أ: SQL Server مباشر (Windows/Linux x64)

1. ثبّت SQL Server 2019 أو أحدث
2. أنشئ قاعدة بيانات:
```sql
CREATE DATABASE BroadbandBillingDb;
```

### الخيار ب: Docker (جميع الأنظمة بما فيها ARM64)

```bash
# لأجهزة x64 (Intel/AMD)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=BBS@Strong2024!" \
  -p 1433:1433 --name bbs-sql -d mcr.microsoft.com/mssql/server:2022-latest

# لأجهزة ARM64 (Apple M1/M2, Raspberry Pi)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=BBS@Strong2024!" \
  -p 1433:1433 --name bbs-sql -d mcr.microsoft.com/azure-sql-edge:latest
```

### التحقق من الاتصال

```bash
# باستخدام sqlcmd
sqlcmd -S localhost -U sa -P "BBS@Strong2024!" -Q "SELECT @@VERSION"

# أو باستخدام Docker
docker exec -it bbs-sql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "BBS@Strong2024!" -Q "SELECT 1"
```

---

## 2. إعداد الخادم الخلفي (Backend)

### 2.1 استعادة الحزم

```bash
dotnet restore BroadbandBillingSystem.sln
```

### 2.2 تعديل إعدادات الاتصال

عدّل الملف `src/BroadbandBilling.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "اكتب_مفتاح_سري_طويل_64_حرف_على_الأقل_هنا_لتشفير_JWT",
    "Issuer": "BroadbandBillingAPI",
    "Audience": "BroadbandBillingClients",
    "ExpiryInMinutes": 15,
    "RefreshTokenExpiryInDays": 60
  },
  "MikroTik": {
    "Host": "192.168.88.1",
    "Port": 8728,
    "Username": "admin",
    "Password": "كلمة_مرور_MikroTik"
  }
}
```

> **مهم:** غيّر `Jwt:Secret` لكل بيئة إنتاج. يمكنك توليد مفتاح عشوائي:
> ```bash
> openssl rand -base64 64
> ```

### 2.3 تطبيق الهجرات (Database Migrations)

```bash
# تثبيت أداة EF Core (مرة واحدة)
dotnet tool install --global dotnet-ef

# تطبيق الهجرات
dotnet ef database update \
  --project src/BroadbandBilling.Infrastructure/BroadbandBilling.Infrastructure.csproj \
  --startup-project src/BroadbandBilling.API/BroadbandBilling.API.csproj
```

هذا سينشئ جميع الجداول ويضيف مستخدم المسؤول الافتراضي.

### 2.4 تعيين كلمة مرور المسؤول

كلمة المرور الافتراضية للمسؤول تحتاج تحديث. استخدم هذا الأمر SQL:

```sql
-- أولاً: ولّد BCrypt hash لكلمة المرور المطلوبة
-- يمكنك استخدام أي أداة BCrypt online أو كتابة برنامج C# صغير

-- ثم حدّث كلمة المرور:
UPDATE Users SET PasswordHash = 'BCRYPT_HASH_HERE' WHERE Username = 'admin';
```

أو أنشئ ملف C# مؤقت:
```bash
mkdir -p /tmp/hashpw && cd /tmp/hashpw
dotnet new console
dotnet add package BCrypt.Net-Next
```

عدّل `Program.cs`:
```csharp
var hash = BCrypt.Net.BCrypt.HashPassword("YOUR_ADMIN_PASSWORD", 12);
Console.WriteLine(hash);
```

```bash
dotnet run
# انسخ الناتج واستخدمه في SQL أعلاه
```

### 2.5 تشغيل الخادم

```bash
# وضع التطوير
dotnet run --project src/BroadbandBilling.API/BroadbandBilling.API.csproj

# أو البناء والتشغيل
dotnet build -c Release
dotnet src/BroadbandBilling.API/bin/Release/net9.0/BroadbandBilling.API.dll
```

الخادم يعمل على:
- **HTTP:** `http://localhost:5286`
- **Swagger:** `http://localhost:5286/swagger`

---

## 3. إعداد الواجهة الأمامية (Frontend)

### 3.1 تثبيت الحزم

```bash
cd frontend-vue
npm install
```

### 3.2 وضع التطوير

```bash
npm run dev
```

الواجهة تعمل على `http://localhost:8080` مع proxy تلقائي للـ API.

### 3.3 بناء للإنتاج

```bash
npm run build
```

الملفات المبنية تكون في `frontend-vue/dist/`.

---

## 4. النشر للإنتاج (Production Deployment)

### 4.1 بناء Backend

```bash
dotnet publish src/BroadbandBilling.API/BroadbandBilling.API.csproj \
  -c Release -o ./publish/api
```

### 4.2 بناء Frontend

```bash
cd frontend-vue
npm run build
```

### 4.3 إعداد Nginx (موصى به)

```nginx
server {
    listen 80;
    server_name your-domain.com;

    # Frontend - ملفات Vue المبنية
    location / {
        root /var/www/doshi/frontend;
        try_files $uri $uri/ /index.html;
    }

    # Backend API - proxy إلى .NET
    location /api/ {
        proxy_pass http://localhost:5286;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 4.4 إعداد خدمة systemd للـ Backend

```bash
sudo nano /etc/systemd/system/doshi-api.service
```

```ini
[Unit]
Description=DOSHI API
After=network.target

[Service]
WorkingDirectory=/var/www/doshi/api
ExecStart=/usr/bin/dotnet BroadbandBilling.API.dll
Restart=always
RestartSec=10
SyslogIdentifier=doshi-api
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5286

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable doshi-api
sudo systemctl start doshi-api
sudo systemctl status doshi-api
```

### 4.5 نسخ الملفات

```bash
# Backend
sudo mkdir -p /var/www/doshi/api
sudo cp -r ./publish/api/* /var/www/doshi/api/

# Frontend
sudo mkdir -p /var/www/doshi/frontend
sudo cp -r ./frontend-vue/dist/* /var/www/doshi/frontend/

# صلاحيات
sudo chown -R www-data:www-data /var/www/doshi
```

---

## 5. إعدادات الإنتاج المهمة

### 5.1 ملف `appsettings.Production.json`

أنشئ هذا الملف بجانب `appsettings.json` في مجلد الـ API:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=PRODUCTION_PASSWORD;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "مفتاح_سري_مختلف_للإنتاج_64_حرف_على_الأقل"
  },
  "CorsOrigins": [
    "http://your-domain.com",
    "https://your-domain.com"
  ]
}
```

### 5.2 أمان JWT

- التوكنات محفوظة في **HttpOnly Cookies** (غير قابلة للوصول من JavaScript)
- في الإنتاج: `Secure=true` (HTTPS فقط) + `SameSite=Strict` (حماية CSRF)
- لا يتم إرسال التوكنات في headers أو response body

### 5.3 HTTPS (مطلوب للإنتاج)

```bash
# باستخدام Certbot + Let's Encrypt
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com
```

---

## 6. إعداد MikroTik

### 6.1 تفعيل API على الراوتر

```
/ip service set api disabled=no port=8728
/ip service set api-ssl disabled=no port=8729
```

### 6.2 إنشاء مستخدم API

```
/user add name=api-user password=api-password group=full
```

### 6.3 تحديث الإعدادات

في `appsettings.json`:
```json
{
  "MikroTik": {
    "Host": "عنوان_IP_للراوتر",
    "Port": 8728,
    "Username": "api-user",
    "Password": "api-password"
  }
}
```

---

## 7. بيانات الدخول الافتراضية

| الحقل | القيمة |
|-------|--------|
| اسم المستخدم | `admin` |
| كلمة المرور | يجب تعيينها (راجع القسم 2.4) |
| رابط الدخول | `http://localhost:8080` (تطوير) |

---

## 8. المنافذ المستخدمة

| الخدمة | المنفذ | الوصف |
|--------|--------|-------|
| Backend API | 5286 | .NET Kestrel HTTP |
| Frontend Dev | 8080 | Vite dev server |
| SQL Server | 1433 | قاعدة البيانات |
| MikroTik API | 8728 | RouterOS API |
| Hangfire | 5286/hangfire | لوحة المهام المجدولة |

---

## 9. استكشاف الأخطاء

### الخادم لا يعمل
```bash
# تحقق من السجلات
cat src/BroadbandBilling.API/Logs/log-*.txt

# تحقق من اتصال قاعدة البيانات
dotnet ef database update --project src/BroadbandBilling.Infrastructure/BroadbandBilling.Infrastructure.csproj --startup-project src/BroadbandBilling.API/BroadbandBilling.API.csproj
```

### خطأ CORS
تأكد أن عنوان الواجهة الأمامية موجود في `CorsOrigins` في `appsettings.json`.

### خطأ 401 Unauthorized
- تأكد أن الكوكيز مفعلة في المتصفح
- تأكد أن `withCredentials: true` في إعدادات Axios
- تحقق أن `Jwt:Secret` متطابق بين التطوير والإنتاج

### MikroTik لا يتصل
- تأكد أن خدمة API مفعلة على الراوتر (`/ip service print`)
- تأكد أن المنفذ 8728 مفتوح في الفايروول
- تأكد من صحة بيانات الاتصال

---

## 10. التقنيات المستخدمة

### Backend
- **.NET 9** - إطار العمل
- **Entity Framework Core** - ORM
- **MediatR** - CQRS Pattern
- **BCrypt** - تشفير كلمات المرور
- **JWT + HttpOnly Cookies** - المصادقة
- **Hangfire** - المهام المجدولة
- **Serilog** - التسجيل
- **AutoMapper** - تحويل الكائنات

### Frontend
- **Vue 3** + **TypeScript** - إطار العمل
- **Pinia** - إدارة الحالة
- **Vue Router** - التوجيه
- **Tailwind CSS 4** - التنسيق
- **Axios** - طلبات HTTP
- **Vite** - أداة البناء

---

## 11. خطوات سريعة (Quick Start)

```bash
# 1. قاعدة البيانات (Docker)
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=BBS@Strong2024!" \
  -p 1433:1433 --name bbs-sql -d mcr.microsoft.com/mssql/server:2022-latest

# 2. الهجرات
dotnet ef database update \
  --project src/BroadbandBilling.Infrastructure/BroadbandBilling.Infrastructure.csproj \
  --startup-project src/BroadbandBilling.API/BroadbandBilling.API.csproj

# 3. تشغيل Backend
dotnet run --project src/BroadbandBilling.API/BroadbandBilling.API.csproj &

# 4. تشغيل Frontend
cd frontend-vue && npm install && npm run dev
```

افتح المتصفح على `http://localhost:8080` وسجّل الدخول.
