# DOSHI - IIS Deployment Guide (Quick)
**Frontend + Backend on Same Site**

---

## 1. Prerequisites & Installation

### Required Software
```powershell
# Install in this order:
# 1. .NET 9 Hosting Bundle: https://dotnet.microsoft.com/download/dotnet/9.0
# 2. Node.js 18+: https://nodejs.org
# 3. SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads
# 4. IIS URL Rewrite: https://www.iis.net/downloads/microsoft/url-rewrite
# 5. IIS ARR: https://www.iis.net/downloads/microsoft/application-request-routing

# Enable IIS (PowerShell as Admin):
dism /online /enable-feature /featurename:IIS-WebServerRole /featurename:IIS-WebServer /featurename:IIS-ASPNET45 /featurename:IIS-NetFxExtensibility45 /all

# Restart after IIS installation
Restart-Computer
```

### Enable ARR Proxy
```powershell
# In IIS Manager:
# Server Name → Application Request Routing Cache → Server Proxy Settings → Enable proxy ✅
```

---

## 2. Database Setup

```powershell
# Create database
sqlcmd -S localhost -U sa -P "YourPassword" -Q "CREATE DATABASE BroadbandBillingDb"

# Apply migrations
cd C:\path\to\BBS-Mikrotik
dotnet tool install --global dotnet-ef
dotnet ef database update --project src/BroadbandBilling.Infrastructure --startup-project src/BroadbandBilling.API
```

---

## 3. Build & Deploy Backend

```powershell
cd C:\path\to\BBS-Mikrotik

# Publish API
dotnet publish src/BroadbandBilling.API/BroadbandBilling.API.csproj -c Release -o C:\inetpub\wwwroot\doshi\api --runtime win-x64 --self-contained false
```

### Create `C:\inetpub\wwwroot\doshi\api\appsettings.Production.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;",
    "HangfireConnection": "Server=localhost;Database=BroadbandBillingDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Secret": "YOUR_64_CHAR_SECRET_KEY_MINIMUM_32_BYTES_CHANGE_THIS_NOW",
    "Issuer": "BroadbandBillingAPI",
    "Audience": "BroadbandBillingClients",
    "ExpiryInMinutes": 15,
    "RefreshTokenExpiryInDays": 60
  },
  "MikroTik": {
    "Host": "192.168.88.1",
    "Port": 8728,
    "Username": "admin",
    "Password": "YOUR_MIKROTIK_PASSWORD"
  },
  "CorsOrigins": ["http://localhost", "http://YOUR_IP", "https://YOUR_DOMAIN"]
}
```

### Create `C:\inetpub\wwwroot\doshi\api\web.config`
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" arguments=".\BroadbandBilling.API.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

```powershell
# Create logs folder
mkdir C:\inetpub\wwwroot\doshi\api\logs
```

---

## 4. Build & Deploy Frontend

### Create `frontend-vue/.env.production`
```env
# Base URL for Vue Router
VITE_APP_BASE_URL=/doshi/

# Base URL for API calls
VITE_API_BASE_URL=/doshi/api
VITE_APP_TITLE=DOSHI
```

```powershell
cd C:\path\to\BBS-Mikrotik\frontend-vue

# Build
npm install
npm run build

# Deploy
xcopy /E /I /Y dist C:\inetpub\wwwroot\doshi
```

### Create `C:\inetpub\wwwroot\doshi\web.config`
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <!-- Proxy /doshi/api requests to backend -->
        <rule name="API Proxy" stopProcessing="true">
          <match url="^doshi/api/(.*)" />
          <action type="Rewrite" url="http://localhost:5000/api/{R:1}" />
        </rule>
        <!-- Vue Router SPA fallback for /doshi/ -->
        <rule name="SPA Routes" stopProcessing="true">
          <match url="^doshi/.*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
            <add input="{REQUEST_URI}" pattern="^/doshi/api" negate="true" />
          </conditions>
          <action type="Rewrite" url="/doshi/index.html" />
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

## 5. Configure IIS Sites

```powershell
Import-Module WebAdministration

# Create Application Pools
New-WebAppPool -Name "DoshiAPI"
Set-ItemProperty "IIS:\AppPools\DoshiAPI" -Name "managedRuntimeVersion" -Value ""
Set-ItemProperty "IIS:\AppPools\DoshiAPI" -Name "startMode" -Value "AlwaysRunning"

New-WebAppPool -Name "DoshiMain"
Set-ItemProperty "IIS:\AppPools\DoshiMain" -Name "managedRuntimeVersion" -Value ""

# Stop Default Site
Stop-Website -Name "Default Web Site"

# Create Backend Site (Internal - Port 5000)
New-Website -Name "DoshiAPI" -PhysicalPath "C:\inetpub\wwwroot\doshi\api" -Port 5000 -ApplicationPool "DoshiAPI"

# Create Main Site (Public - Port 80)
New-Website -Name "Doshi" -PhysicalPath "C:\inetpub\wwwroot\doshi" -Port 80 -ApplicationPool "DoshiMain"

# Set Permissions
$acl = Get-Acl "C:\inetpub\wwwroot\doshi"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\wwwroot\doshi" $acl

# Start Sites
Start-Website -Name "DoshiAPI"
Start-Website -Name "Doshi"
```

---

## 6. Firewall & Internet Access

```powershell
# Open Firewall Ports
New-NetFirewallRule -DisplayName "DOSHI HTTP" -Direction Inbound -Protocol TCP -LocalPort 80 -Action Allow
New-NetFirewallRule -DisplayName "DOSHI HTTPS" -Direction Inbound -Protocol TCP -LocalPort 443 -Action Allow

# Get Public IP
Invoke-RestMethod -Uri "https://api.ipify.org"
```

### Router Port Forwarding
1. Access router admin panel (usually `192.168.1.1` or `192.168.88.1`)
2. Find **Port Forwarding** / **NAT** / **Virtual Server**
3. Add rule:
   - **External Port**: 80
   - **Internal IP**: Your PC IP (e.g., `192.168.1.100`)
   - **Internal Port**: 80
   - **Protocol**: TCP

---

## 7. SSL Certificate (Optional but Recommended)

### Option A: Let's Encrypt (with domain)
```powershell
# Install win-acme
winget install win-acme

# Run and follow prompts
wacs.exe
```

### Option B: Self-Signed (testing only)
```powershell
New-SelfSignedCertificate -DnsName "YOUR_IP" -CertStoreLocation "cert:\LocalMachine\My" -NotAfter (Get-Date).AddYears(5)

# Bind in IIS Manager:
# Doshi site → Bindings → Add → Type: https, Port: 443, Select certificate
```

---

## 8. Testing

```powershell
# Local Test
Start-Process "http://localhost"

# API Test
Invoke-RestMethod -Uri "http://localhost/api/health" -Method Get

# External Test (from another device)
# http://YOUR_PUBLIC_IP
```

**Default Login:**
- Username: `admin`
- Password: `gburi@admin`

⚠️ **Change default password immediately after first login!**

---

## 9. Folder Structure

```
C:\inetpub\wwwroot\doshi\
├── index.html              ← Frontend entry
├── assets\                 ← Frontend assets
├── web.config              ← Frontend + API proxy config
└── api\                    ← Backend
    ├── BroadbandBilling.API.dll
    ├── appsettings.Production.json
    ├── web.config
    └── logs\
```

---

## 10. Troubleshooting

### 500 Error
```powershell
# Check logs
Get-Content C:\inetpub\wwwroot\doshi\api\logs\stdout* -Tail 50
```

### 502.5 Process Failure
```powershell
# Verify .NET runtime
dotnet --list-runtimes
# Should show: Microsoft.AspNetCore.App 9.0.x
```

### API Not Accessible
```powershell
# Verify backend is running
Get-Website | Where-Object { $_.Name -eq "DoshiAPI" }

# Test backend directly
Invoke-RestMethod -Uri "http://localhost:5000/api/health"
```

### CORS Errors
Update `CorsOrigins` in `appsettings.Production.json` with your actual domain/IP.

### Site Not Accessible from Internet
1. Check firewall: `Get-NetFirewallRule | Where-Object { $_.DisplayName -like "*DOSHI*" }`
2. Verify port forwarding in router
3. Test port: https://www.yougetsignal.com/tools/open-ports/

---

## 11. Updates

```powershell
cd C:\path\to\BBS-Mikrotik
git pull

# Update Backend
Stop-Website -Name "DoshiAPI"
dotnet publish src/BroadbandBilling.API/BroadbandBilling.API.csproj -c Release -o C:\inetpub\wwwroot\doshi\api --runtime win-x64 --self-contained false
dotnet ef database update --project src/BroadbandBilling.Infrastructure --startup-project src/BroadbandBilling.API
Start-Website -Name "DoshiAPI"

# Update Frontend
cd frontend-vue
npm install
npm run build
xcopy /E /I /Y dist C:\inetpub\wwwroot\doshi

# Restart IIS
iisreset
```

---

## Quick Commands Reference

```powershell
# Restart IIS
iisreset

# Check site status
Get-Website

# Start/Stop sites
Start-Website -Name "Doshi"
Stop-Website -Name "Doshi"

# View logs
Get-Content C:\inetpub\wwwroot\doshi\api\logs\stdout* -Tail 50

# Check .NET runtime
dotnet --list-runtimes

# Test API
Invoke-RestMethod -Uri "http://localhost/api/health"
```

---

**Support:** For issues, check logs in `C:\inetpub\wwwroot\doshi\api\logs\`
