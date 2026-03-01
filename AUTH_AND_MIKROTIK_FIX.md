# 🔧 Authentication & MikroTik Connection Fix

## Issues Fixed

### Issue 1: Admin Pages Causing Immediate Logout
**Symptom:** Navigating to `/admin/subscribers`, `/admin/invoices`, `/admin/payments`, `/admin/users/new` redirects to `/?redirect=...`

**Root Cause:** 
- Router guard called `checkSession()` which made API call to `/auth/me`
- If JWT cookie expired, 401 returned, user redirected to login
- Pinia persistence wasn't being trusted - always making API calls

**Fix Applied:**
1. **`frontend-vue/src/stores/auth.ts`**
   - `checkSession()` now trusts persisted user data first
   - Only makes API call if no persisted state exists
   - Added `sessionChecked` flag to track state

2. **`frontend-vue/src/router/index.ts`**
   - Router guard now checks `auth.isAuthenticated` first (from persisted state)
   - Only calls `checkSession()` if not authenticated
   - Preserves redirect path in query params

---

### Issue 2: MikroTik Connection Failing
**Symptom:** "فشل الاتصال بجهاز MikroTik - تحقق من إعدادات الخادم" on all MikroTik pages

**Root Cause:**
- Frontend was sending empty `{}` objects to MikroTik endpoints
- `MikroTikConnectionRequest` had `required` properties that couldn't be null
- Service wasn't falling back to `appsettings.json` defaults

**Fix Applied:**
1. **`src/BroadbandBilling.Application/Common/Interfaces/IMikroTikService.cs`**
   - Made `Host`, `Username`, `Password` nullable in `MikroTikConnectionRequest`

2. **`src/BroadbandBilling.Infrastructure/Services/MikroTikService.cs`**
   - `CreateConnectionAsync()` now uses defaults from `appsettings.json` when request values are empty
   - Added `Host`, `Username`, `Password` to `MikroTikSettings` class

3. **`src/BroadbandBilling.Infrastructure/DependencyInjection.cs`**
   - Added `services.Configure<MikroTikSettings>(configuration.GetSection("MikroTik"))`

4. **`src/BroadbandBilling.API/appsettings.json`**
   - Fixed JSON syntax error (removed invalid comment)
   - MikroTik settings already configured:
     ```json
     "MikroTik": {
       "Host": "192.168.11.50",
       "Port": 8728,
       "Username": "admin",
       "Password": "0557764770"
     }
     ```

---

## Files Modified

### Frontend
- `frontend-vue/src/stores/auth.ts` - Improved session handling
- `frontend-vue/src/router/index.ts` - Fixed route guard logic

### Backend
- `src/BroadbandBilling.Application/Common/Interfaces/IMikroTikService.cs` - Nullable connection properties
- `src/BroadbandBilling.Infrastructure/Services/MikroTikService.cs` - Default settings fallback
- `src/BroadbandBilling.Infrastructure/DependencyInjection.cs` - MikroTikSettings registration
- `src/BroadbandBilling.API/appsettings.json` - Fixed JSON syntax

---

## Testing Instructions

### Step 1: Restart Backend
```bash
cd /home/hg/Desktop/ms/BBS-Mikrotik
dotnet build
dotnet run --project src/BroadbandBilling.API/BroadbandBilling.API.csproj
```

### Step 2: Restart Frontend
```bash
cd /home/hg/Desktop/ms/BBS-Mikrotik/frontend-vue
npm run dev
```

### Step 3: Test Authentication
1. Open http://localhost:8080
2. Login as admin: `admin` / `gburi@admin`
3. Navigate to these pages (should NOT redirect to login):
   - http://localhost:8080/admin/subscribers
   - http://localhost:8080/admin/invoices
   - http://localhost:8080/admin/payments
   - http://localhost:8080/admin/users/new
4. Refresh the page (F5) - should stay logged in
5. Open a new tab and go to http://localhost:8080/admin/dashboard - should work

### Step 4: Test MikroTik Connection
1. Navigate to http://localhost:8080/admin/online-users
2. Click "اتصال" button - should load active sessions
3. Navigate to http://localhost:8080/admin/mikrotik/users
4. Click "اتصال" button - should load PPP users
5. Navigate to http://localhost:8080/admin/mikrotik/profiles
6. Click "اتصال" button - should load PPP profiles

### Step 5: Verify MikroTik Router Connectivity
Make sure your MikroTik router at `192.168.11.50` is:
- Accessible from your machine
- API service enabled (port 8728)
- Credentials correct: `admin` / `0557764770`

To test connectivity manually:
```bash
# Test if port is open
nc -zv 192.168.11.50 8728

# Or using telnet
telnet 192.168.11.50 8728
```

---

## Troubleshooting

### If still getting logged out:
1. Clear browser sessionStorage: `sessionStorage.clear()`
2. Clear cookies for localhost
3. Login again

### If MikroTik still fails:
1. Check backend logs: `src/BroadbandBilling.API/Logs/log-*.txt`
2. Verify MikroTik router is reachable
3. Verify API service is enabled on MikroTik:
   - WinBox → IP → Services → api (port 8728) should be enabled
4. Check firewall rules on MikroTik

### Common MikroTik Errors:
- "Connection refused" - API service not enabled or firewall blocking
- "Authentication failed" - Wrong username/password
- "Connection timeout" - Router not reachable or wrong IP

---

## Summary of Changes

| File | Change |
|------|--------|
| `auth.ts` | Trust persisted state, improved `checkSession()` |
| `router/index.ts` | Fixed route guard to check persisted auth first |
| `IMikroTikService.cs` | Made connection properties nullable |
| `MikroTikService.cs` | Use defaults from appsettings.json |
| `DependencyInjection.cs` | Register MikroTikSettings configuration |
| `appsettings.json` | Fixed JSON syntax error |

**Status:** ✅ All fixes applied
**Last Updated:** 2026-03-01
