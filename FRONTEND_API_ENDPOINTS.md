# Frontend API Endpoints Documentation

## Important: Updated Authentication Endpoints

The authentication endpoints have been updated. **DO NOT** use the old `/api/auth/login` endpoint.

### ✅ Correct Endpoints

**Admin Login:**
```
POST /api/auth/admin/login
```

**Subscriber Login:**
```
POST /api/auth/subscriber/login
```

**Logout:**
```
POST /api/auth/logout
```

**Refresh Token:**
```
POST /api/auth/refresh-token
```

### ❌ Old Endpoint (DO NOT USE)
```
POST /api/auth/login  ← This will return 404
```

## Request Format

**Admin/Subscriber Login:**
```json
{
  "username": "admin",
  "password": "Admin@123",
  "rememberMe": true,
  "deviceName": "Mozilla/5.0...",
  "operatingSystem": "Linux"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1...",
  "refreshToken": "abc123...",
  "expiresIn": 3600,
  "userType": "Admin",
  "fullName": "مدير النظام",
  "role": "SuperAdmin",
  "hasActiveSubscription": false
}
```

## Frontend Configuration

The frontend `auth.js` is already configured correctly:
- Uses `/api/auth/admin/login` for admin users
- Uses `/api/auth/subscriber/login` for subscribers
- Automatically detects user type

## Testing

1. **Clear browser cache** to remove old API calls
2. **Hard refresh** the frontend page (Ctrl+Shift+R)
3. Try logging in - should work without 404 errors

## Common Issues

**404 Error on Login:**
- ✅ Check browser console - old `/api/auth/login` calls are cached
- ✅ Clear cache and hard refresh
- ✅ Verify frontend is using updated `auth.js`

**CORS Error:**
- ✅ Ensure API is running on `http://localhost:5286`
- ✅ Verify CORS origins in `appsettings.json` include your frontend URL
