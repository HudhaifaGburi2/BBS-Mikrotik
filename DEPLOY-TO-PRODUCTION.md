# DOSHI Production Deployment - Quick Guide

**Current Setup:**
- Domain: `https://my.dushi.sa/`
- Frontend: `C:\inetpub\wwwroot\` (root)
- Backend API: `C:\inetpub\wwwroot\api\`

---

## Step 1: Build on Development Machine

```bash
# Navigate to project
cd /home/hg/Desktop/ms/BBS-Mikrotik

# Build Frontend
cd frontend-vue
npm install
npm run build

# Build Backend
cd ..
dotnet publish src/BroadbandBilling.API/BroadbandBilling.API.csproj \
  -c Release \
  -o ./publish/api \
  --runtime win-x64 \
  --self-contained false
```

---

## Step 2: Transfer Files to Windows Server

Transfer these folders to your Windows server:
- `frontend-vue/dist/*` → Copy to server
- `publish/api/*` → Copy to server
- `src/BroadbandBilling.API/appsettings.Production.json` → Copy to server

---

## Step 3: Deploy on Windows IIS Server

```powershell
# Stop sites
Stop-Website -Name "DoshiAPI"
Stop-Website -Name "Doshi"

# Deploy Frontend (to root)
Remove-Item C:\inetpub\wwwroot\* -Recurse -Force -Exclude api,logs
Copy-Item -Path "C:\path\to\transferred\dist\*" -Destination "C:\inetpub\wwwroot\" -Recurse -Force

# Deploy Backend
Remove-Item C:\inetpub\wwwroot\api\* -Recurse -Force -Exclude appsettings.Production.json,logs
Copy-Item -Path "C:\path\to\transferred\api\*" -Destination "C:\inetpub\wwwroot\api\" -Recurse -Force

# Copy production config (if not exists)
if (!(Test-Path "C:\inetpub\wwwroot\api\appsettings.Production.json")) {
    Copy-Item "C:\path\to\transferred\appsettings.Production.json" "C:\inetpub\wwwroot\api\"
}

# Create logs folder if not exists
if (!(Test-Path "C:\inetpub\wwwroot\logs")) {
    New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\logs"
}
if (!(Test-Path "C:\inetpub\wwwroot\api\logs")) {
    New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\api\logs"
}

# Set permissions
$acl = Get-Acl "C:\inetpub\wwwroot"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\wwwroot" $acl

# Restart IIS
iisreset

# Start sites
Start-Website -Name "DoshiAPI"
Start-Website -Name "Doshi"
```

---

## Step 4: Verify Deployment

```powershell
# Check sites are running
Get-Website | Format-Table Name, State, Bindings

# Test API
Invoke-RestMethod -Uri "http://localhost:5000/api/health" -Method Get

# Check logs
Get-Content C:\inetpub\wwwroot\api\logs\stdout* -Tail 20
Get-Content C:\inetpub\wwwroot\logs\api-log-*.txt -Tail 20
```

---

## Folder Structure

```
C:\inetpub\wwwroot\
├── index.html              ← Frontend (Vue)
├── assets\                 ← Frontend assets
├── favicon.svg
├── web.config              ← Frontend + API proxy
├── api\                    ← Backend (.NET)
│   ├── BroadbandBilling.API.dll
│   ├── appsettings.json
│   ├── appsettings.Production.json  ← Production config
│   ├── web.config
│   └── logs\
└── logs\                   ← Application logs
```

---

## Configuration Files

### `C:\inetpub\wwwroot\web.config` (Frontend)
Already included in build output from `frontend-vue/public/web.config`

### `C:\inetpub\wwwroot\api\web.config` (Backend)
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

### `C:\inetpub\wwwroot\api\appsettings.Production.json`
Already created in your project - ensure it's copied to the server.

---

## Troubleshooting

### Site shows 500 error
```powershell
# Check API logs
Get-Content C:\inetpub\wwwroot\api\logs\stdout* -Tail 50

# Check application logs
Get-Content C:\inetpub\wwwroot\logs\api-log-*.txt -Tail 50

# Verify .NET runtime
dotnet --list-runtimes
```

### API not responding
```powershell
# Check if backend site is running
Get-Website -Name "DoshiAPI"

# Test backend directly
Invoke-RestMethod -Uri "http://localhost:5000/api/health"
```

### Database connection error
```powershell
# Test SQL connection
sqlcmd -S localhost -U sa -P "MaJd@1405" -Q "SELECT 1"

# Verify database exists
sqlcmd -S localhost -U sa -P "MaJd@1405" -Q "SELECT name FROM sys.databases WHERE name='BroadbandBillingDb'"
```

---

## Access Your Site

- **Production URL**: https://my.dushi.sa/
- **Admin Login**: `admin` / `gburi@admin`
- **Hangfire Dashboard**: https://my.dushi.sa/hangfire (Admin only)

---

## Quick Update Commands

```powershell
# Quick restart
iisreset

# View logs
Get-Content C:\inetpub\wwwroot\api\logs\stdout* -Tail 50

# Check site status
Get-Website | Format-Table Name, State
```
