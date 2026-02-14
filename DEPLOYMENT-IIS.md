# DOSHI - دليل النشر على IIS (Windows)

> نشر نظام DOSHI لإدارة فواتير الإنترنت على خادم IIS في Windows مع إتاحته عبر الإنترنت.

---

## المتطلبات الأساسية

| المكون | الإصدار | رابط التحميل |
|--------|---------|-------------|
| Windows 10/11 Pro أو Server 2019+ | 64-bit Intel | مثبت مسبقاً |
| .NET 9 SDK | 9.0+ | https://dotnet.microsoft.com/download/dotnet/9.0 |
| .NET 9 Hosting Bundle | 9.0+ | https://dotnet.microsoft.com/download/dotnet/9.0 (Windows Hosting Bundle) |
| Node.js | 18+ | https://nodejs.org |
| SQL Server | 2019+ أو Express | https://www.microsoft.com/sql-server/sql-server-downloads |
| Git | أي إصدار | https://git-scm.com/download/win |

---

## الخطوة 1: تثبيت المتطلبات

### 1.1 تثبيت .NET 9 Hosting Bundle

```powershell
# حمّل وثبّت من الرابط:
# https://dotnet.microsoft.com/download/dotnet/9.0
# اختر: ASP.NET Core Runtime → Windows → Hosting Bundle
# هذا يثبت Runtime + IIS Module معاً
```

> **مهم جداً**: يجب تثبيت **Hosting Bundle** وليس فقط Runtime. بدونه لن يعمل التطبيق على IIS.

### 1.2 تثبيت Node.js

```powershell
# حمّل وثبّت من https://nodejs.org (LTS)
# تحقق من التثبيت:
node --version
npm --version
```

### 1.3 تثبيت SQL Server

```powershell
# حمّل SQL Server Express من:
# https://www.microsoft.com/sql-server/sql-server-downloads
# اختر Express (مجاني)
# أثناء التثبيت اختر Mixed Mode Authentication
# عيّن كلمة مرور sa
```

### 1.4 تفعيل IIS

```powershell
# افتح PowerShell كمسؤول (Run as Administrator):

# تفعيل IIS مع جميع الميزات المطلوبة
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServer
Enable-WindowsOptionalFeature -Online -FeatureName IIS-CommonHttpFeatures
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpErrors
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpRedirect
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ApplicationDevelopment
Enable-WindowsOptionalFeature -Online -FeatureName IIS-NetFxExtensibility45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HealthAndDiagnostics
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpLogging
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Security
Enable-WindowsOptionalFeature -Online -FeatureName IIS-RequestFiltering
Enable-WindowsOptionalFeature -Online -FeatureName IIS-Performance
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerManagementTools
Enable-WindowsOptionalFeature -Online -FeatureName IIS-StaticContent
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DefaultDocument
Enable-WindowsOptionalFeature -Online -FeatureName IIS-DirectoryBrowsing
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ASPNET45
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIExtensions
Enable-WindowsOptionalFeature -Online -FeatureName IIS-ISAPIFilter
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpCompressionStatic
Enable-WindowsOptionalFeature -Online -FeatureName IIS-HttpCompressionDynamic

# أو بأمر واحد مختصر:
dism /online /enable-feature /featurename:IIS-WebServerRole /featurename:IIS-WebServer /featurename:IIS-CommonHttpFeatures /featurename:IIS-StaticContent /featurename:IIS-DefaultDocument /featurename:IIS-HttpErrors /featurename:IIS-ApplicationDevelopment /featurename:IIS-ASPNET45 /featurename:IIS-NetFxExtensibility45 /featurename:IIS-ISAPIExtensions /featurename:IIS-ISAPIFilter /featurename:IIS-HttpCompressionStatic /featurename:IIS-HttpCompressionDynamic /featurename:IIS-HealthAndDiagnostics /featurename:IIS-HttpLogging /featurename:IIS-Security /featurename:IIS-RequestFiltering /featurename:IIS-Performance /featurename:IIS-WebServerManagementTools /all
```

> أعد تشغيل الكمبيوتر بعد تفعيل IIS.

### 1.5 تحقق من تثبيت ASP.NET Core Module

```powershell
# بعد تثبيت Hosting Bundle وإعادة التشغيل:
# افتح IIS Manager → Modules → تحقق من وجود AspNetCoreModuleV2
# أو:
Get-WebGlobalModule | Where-Object { $_.Name -like "*AspNetCore*" }
```

---

## الخطوة 2: إعداد قاعدة البيانات

### 2.1 إنشاء قاعدة البيانات

```powershell
# افتح SQL Server Management Studio (SSMS) أو sqlcmd:
sqlcmd -S localhost -U sa -P "BBS@Strong2024!" -Q "CREATE DATABASE BroadbandBillingDb"
```

### 2.2 تطبيق الـ Migrations

```powershell
# من مجلد المشروع:
cd C:\path\to\BBS-Mikrotik

dotnet ef database update --project src/BroadbandBilling.Infrastructure/BroadbandBilling.Infrastructure.csproj --startup-project src/BroadbandBilling.API/BroadbandBilling.API.csproj
```

> إذا لم يكن `dotnet-ef` مثبتاً:
> ```powershell
> dotnet tool install --global dotnet-ef
> ```

---

## الخطوة 3: بناء التطبيق (Backend)

### 3.1 نشر Backend

```powershell
cd C:\path\to\BBS-Mikrotik

# بناء ونشر API
dotnet publish src/BroadbandBilling.API/BroadbandBilling.API.csproj -c Release -o C:\inetpub\doshi\api --runtime win-x64 --self-contained false
```

### 3.2 إعداد appsettings.Production.json

أنشئ ملف `C:\inetpub\doshi\api\appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=YOUR_SA_PASSWORD;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=YOUR_SA_PASSWORD;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "GENERATE_A_STRONG_64_CHAR_SECRET_KEY_HERE_MINIMUM_32_BYTES_LONG!!",
    "Issuer": "BroadbandBillingAPI",
    "Audience": "BroadbandBillingClients",
    "ExpiryInMinutes": 15,
    "RefreshTokenExpiryInDays": 60
  },
  "MikroTik": {
    "Host": "192.168.88.1",
    "Port": 8728,
    "Username": "admin",
    "Password": "YOUR_MIKROTIK_PASSWORD",
    "Timeout": 30,
    "RetryAttempts": 3
  },
  "CorsOrigins": [
    "http://YOUR_DOMAIN_OR_IP",
    "https://YOUR_DOMAIN_OR_IP"
  ],
  "Serilog": {
    "Using": ["Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "C:\\inetpub\\doshi\\logs\\api-log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

> **مهم**: غيّر `YOUR_SA_PASSWORD` و `YOUR_DOMAIN_OR_IP` و `GENERATE_A_STRONG_64_CHAR_SECRET_KEY_HERE` بالقيم الحقيقية.

### 3.3 إنشاء web.config للـ API

أنشئ ملف `C:\inetpub\doshi\api\web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\BroadbandBilling.API.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

### 3.4 إنشاء مجلد السجلات

```powershell
mkdir C:\inetpub\doshi\api\logs
mkdir C:\inetpub\doshi\logs
```

---

## الخطوة 4: بناء التطبيق (Frontend)

### 4.1 إعداد متغيرات البيئة

أنشئ ملف `frontend-vue/.env.production`:

```env
VITE_API_BASE_URL=/api
VITE_APP_TITLE=DOSHI
```

### 4.2 بناء Frontend

```powershell
cd C:\path\to\BBS-Mikrotik\frontend-vue

npm install
npm run build
```

### 4.3 نسخ ملفات Frontend

```powershell
# نسخ مجلد dist إلى مجلد IIS
xcopy /E /I /Y dist C:\inetpub\doshi\www
```

### 4.4 إنشاء web.config للـ Frontend

أنشئ ملف `C:\inetpub\doshi\www\web.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <!-- Proxy API requests to backend -->
        <rule name="API Proxy" stopProcessing="true">
          <match url="^api/(.*)" />
          <action type="Rewrite" url="http://localhost:5000/api/{R:1}" />
        </rule>
        <!-- Vue Router - SPA fallback -->
        <rule name="SPA Routes" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/index.html" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <remove fileExtension=".json" />
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <remove fileExtension=".woff" />
      <mimeMap fileExtension=".woff" mimeType="font/woff" />
      <remove fileExtension=".woff2" />
      <mimeMap fileExtension=".woff2" mimeType="font/woff2" />
    </staticContent>
  </system.webServer>
</configuration>
```

---

## الخطوة 5: تثبيت URL Rewrite Module

مطلوب لتوجيه طلبات API من Frontend إلى Backend.

```powershell
# حمّل وثبّت من:
# https://www.iis.net/downloads/microsoft/url-rewrite
# أو عبر winget:
winget install Microsoft.IIS.UrlRewrite
```

> أعد تشغيل IIS بعد التثبيت:
> ```powershell
> iisreset
> ```

---

## الخطوة 6: تثبيت ARR (Application Request Routing)

مطلوب لعمل Reverse Proxy من Frontend إلى Backend API.

```powershell
# حمّل وثبّت من:
# https://www.iis.net/downloads/microsoft/application-request-routing
# أو:
winget install Microsoft.IIS.ApplicationRequestRouting
```

### تفعيل Proxy في ARR

```powershell
# افتح IIS Manager
# اضغط على اسم السيرفر (أعلى القائمة اليسرى)
# افتح Application Request Routing Cache
# اضغط Server Proxy Settings (يمين)
# ✅ فعّل Enable proxy
# اضغط Apply
```

---

## الخطوة 7: إعداد مواقع IIS

### 7.1 إنشاء Application Pool للـ API

```powershell
# PowerShell كمسؤول:
Import-Module WebAdministration

# إنشاء Application Pool للـ API
New-WebAppPool -Name "DoshiAPI"
Set-ItemProperty "IIS:\AppPools\DoshiAPI" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\DoshiAPI" -Name "startMode" -Value "AlwaysRunning"

# إنشاء Application Pool للـ Frontend
New-WebAppPool -Name "DoshiFrontend"
Set-ItemProperty "IIS:\AppPools\DoshiFrontend" -Name "managedRuntimeVersion" -Value ""
```

### 7.2 إنشاء موقع API (Backend)

```powershell
# إنشاء موقع API على بورت 5000 (داخلي فقط)
New-Website -Name "DoshiAPI" -PhysicalPath "C:\inetpub\doshi\api" -Port 5000 -ApplicationPool "DoshiAPI"
```

### 7.3 إنشاء موقع Frontend (الرئيسي)

```powershell
# إنشاء الموقع الرئيسي على بورت 80
# أولاً أوقف Default Web Site:
Stop-Website -Name "Default Web Site"

New-Website -Name "DoshiFrontend" -PhysicalPath "C:\inetpub\doshi\www" -Port 80 -ApplicationPool "DoshiFrontend"
```

### 7.4 تحقق من عمل المواقع

```powershell
# تشغيل المواقع
Start-Website -Name "DoshiAPI"
Start-Website -Name "DoshiFrontend"

# تحقق
Get-Website | Format-Table Name, State, PhysicalPath
```

---

## الخطوة 8: إعداد الصلاحيات

```powershell
# منح صلاحيات لـ IIS App Pool على مجلدات التطبيق
$acl = Get-Acl "C:\inetpub\doshi"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\doshi" $acl

# صلاحيات لمجلد السجلات
$acl2 = Get-Acl "C:\inetpub\doshi\logs"
$rule2 = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl2.SetAccessRule($rule2)
Set-Acl "C:\inetpub\doshi\logs" $acl2
```

---

## الخطوة 9: إعداد CORS للإنتاج

عدّل `CorsOrigins` في `appsettings.Production.json`:

```json
{
  "CorsOrigins": [
    "http://YOUR_PUBLIC_IP",
    "https://YOUR_DOMAIN.com",
    "http://localhost"
  ]
}
```

---

## الخطوة 10: إتاحة الموقع عبر الإنترنت

### 10.1 إعداد جدار الحماية (Firewall)

```powershell
# PowerShell كمسؤول - فتح بورت 80 و 443
New-NetFirewallRule -DisplayName "DOSHI HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "DOSHI HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow
```

### 10.2 إعداد Port Forwarding في الراوتر

1. ادخل إلى لوحة تحكم الراوتر (عادة `192.168.1.1` أو `192.168.88.1`)
2. ابحث عن **Port Forwarding** أو **NAT** أو **Virtual Server**
3. أضف قاعدة:
   - **External Port**: `80`
   - **Internal IP**: عنوان IP الداخلي لجهازك (مثل `192.168.1.100`)
   - **Internal Port**: `80`
   - **Protocol**: TCP
4. كرر نفس الشيء لبورت `443` إذا كنت ستستخدم HTTPS

### 10.3 معرفة IP العام

```powershell
# معرفة عنوان IP العام:
Invoke-RestMethod -Uri "https://api.ipify.org"
```

### 10.4 إعداد DNS (اختياري لكن مُوصى به)

إذا لديك دومين:
1. اذهب إلى لوحة تحكم الدومين (Namecheap, GoDaddy, Cloudflare...)
2. أضف سجل **A Record**:
   - **Name**: `@` أو `doshi`
   - **Value**: عنوان IP العام الخاص بك
   - **TTL**: 3600

### 10.5 إعداد Dynamic DNS (إذا IP متغير)

إذا عنوان IP العام يتغير (معظم اتصالات المنزل):

1. سجّل في خدمة DDNS مجانية مثل [No-IP](https://www.noip.com) أو [DuckDNS](https://www.duckdns.org)
2. حمّل عميل DDNS وثبّته على جهازك
3. سيحدّث عنوان IP تلقائياً

---

## الخطوة 11: إعداد HTTPS (SSL) - مُوصى به بشدة

### الخيار أ: شهادة Let's Encrypt مجانية (مع دومين)

```powershell
# ثبّت win-acme:
# حمّل من https://www.win-acme.com/
# أو:
winget install win-acme

# شغّل win-acme:
wacs.exe

# اتبع الخطوات:
# 1. اختر N (Create new certificate)
# 2. اختر 1 (Single binding of an IIS site)
# 3. اختر موقع DoshiFrontend
# 4. اتبع التعليمات لإثبات ملكية الدومين
```

### الخيار ب: شهادة Self-Signed (بدون دومين - للاختبار)

```powershell
# إنشاء شهادة ذاتية:
New-SelfSignedCertificate -DnsName "YOUR_IP_OR_HOSTNAME" -CertStoreLocation "cert:\LocalMachine\My" -NotAfter (Get-Date).AddYears(5)

# ربط الشهادة بالموقع في IIS Manager:
# 1. افتح IIS Manager
# 2. اختر DoshiFrontend
# 3. Bindings → Add
# 4. Type: https, Port: 443
# 5. اختر الشهادة من القائمة
```

---

## الخطوة 12: اختبار النشر

### 12.1 اختبار محلي

```powershell
# اختبار API
Invoke-RestMethod -Uri "http://localhost:5000/api/auth/admin/login" -Method Post -ContentType "application/json" -Body '{"username":"admin","password":"gburi@admin"}'

# اختبار Frontend
Start-Process "http://localhost"
```

### 12.2 اختبار من الخارج

```powershell
# من جهاز آخر أو هاتف:
# http://YOUR_PUBLIC_IP
# أو
# http://your-domain.com
```

---

## الخطوة 13: التحديث (عند وجود تحديثات)

### تحديث Backend

```powershell
cd C:\path\to\BBS-Mikrotik

git pull origin main

# إيقاف الموقع
Stop-Website -Name "DoshiAPI"

# إعادة النشر
dotnet publish src/BroadbandBilling.API/BroadbandBilling.API.csproj -c Release -o C:\inetpub\doshi\api --runtime win-x64 --self-contained false

# تطبيق migrations جديدة
dotnet ef database update --project src/BroadbandBilling.Infrastructure/BroadbandBilling.Infrastructure.csproj --startup-project src/BroadbandBilling.API/BroadbandBilling.API.csproj

# تشغيل الموقع
Start-Website -Name "DoshiAPI"
```

### تحديث Frontend

```powershell
cd C:\path\to\BBS-Mikrotik\frontend-vue

git pull origin main
npm install
npm run build

# نسخ الملفات الجديدة
xcopy /E /I /Y dist C:\inetpub\doshi\www
```

---

## هيكل المجلدات النهائي

```
C:\inetpub\doshi\
├── api\                          ← Backend (.NET API)
│   ├── BroadbandBilling.API.dll
│   ├── appsettings.json
│   ├── appsettings.Production.json
│   ├── web.config
│   └── logs\
├── www\                          ← Frontend (Vue.js)
│   ├── index.html
│   ├── assets\
│   ├── web.config
│   └── favicon.svg
└── logs\                         ← Application logs
```

---

## استكشاف الأخطاء

### الموقع لا يعمل (500 Error)

```powershell
# تحقق من سجلات stdout:
Get-Content C:\inetpub\doshi\api\logs\stdout* -Tail 50

# تحقق من سجلات التطبيق:
Get-Content C:\inetpub\doshi\logs\api-log-*.txt -Tail 50

# تحقق من Event Viewer:
Get-EventLog -LogName Application -Newest 20 | Where-Object { $_.Source -like "*IIS*" -or $_.Source -like "*ASP.NET*" }
```

### خطأ 502.5 - Process Failure

```powershell
# تأكد من تثبيت Hosting Bundle:
dotnet --list-runtimes

# يجب أن يظهر:
# Microsoft.AspNetCore.App 9.0.x
# Microsoft.NETCore.App 9.0.x
```

### خطأ CORS

تأكد من أن `CorsOrigins` في `appsettings.Production.json` يحتوي على عنوان الموقع الصحيح.

### قاعدة البيانات لا تتصل

```powershell
# تحقق من أن SQL Server يعمل:
Get-Service -Name "MSSQLSERVER"

# تحقق من الاتصال:
sqlcmd -S localhost -U sa -P "YOUR_PASSWORD" -Q "SELECT 1"
```

### الموقع لا يظهر من الخارج

1. تحقق من Port Forwarding في الراوتر
2. تحقق من جدار الحماية: `Get-NetFirewallRule | Where-Object { $_.DisplayName -like "*DOSHI*" }`
3. اختبر من https://www.yougetsignal.com/tools/open-ports/ (بورت 80)

---

## ملخص الأوامر السريعة

```powershell
# === تثبيت لمرة واحدة ===
# 1. ثبّت .NET 9 Hosting Bundle
# 2. ثبّت Node.js 18+
# 3. ثبّت SQL Server Express
# 4. فعّل IIS (أوامر أعلاه)
# 5. ثبّت URL Rewrite + ARR
# 6. أعد تشغيل الكمبيوتر

# === بناء ونشر ===
cd C:\path\to\BBS-Mikrotik

# Backend
dotnet publish src/BroadbandBilling.API/BroadbandBilling.API.csproj -c Release -o C:\inetpub\doshi\api --runtime win-x64 --self-contained false

# Frontend
cd frontend-vue
npm install
npm run build
xcopy /E /I /Y dist C:\inetpub\doshi\www

# Database
dotnet ef database update --project src/BroadbandBilling.Infrastructure/BroadbandBilling.Infrastructure.csproj --startup-project src/BroadbandBilling.API/BroadbandBilling.API.csproj

# === إدارة IIS ===
iisreset                          # إعادة تشغيل IIS
Start-Website -Name "DoshiAPI"    # تشغيل API
Stop-Website -Name "DoshiAPI"     # إيقاف API
Start-Website -Name "DoshiFrontend"  # تشغيل Frontend
Get-Website                       # عرض حالة المواقع
```

---

## بيانات الدخول الافتراضية

| الدور | اسم المستخدم | كلمة المرور |
|-------|-------------|-------------|
| مدير النظام | `admin` | `gburi@admin` |

> **تنبيه أمني**: غيّر كلمة مرور المدير وسر JWT فوراً بعد النشر!
