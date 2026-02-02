# Fix "Failed to fetch" Error - CORS Issue

## Problem
Getting "Failed to fetch" when trying to login from frontend, but API works fine when tested directly.

## Root Cause
**CORS (Cross-Origin Resource Sharing)** - The frontend origin is not in the allowed CORS origins list.

## Current CORS Configuration
**Backend (appsettings.json):**
```json
"CorsOrigins": [
  "http://localhost:8000",
  "http://localhost:3000",
  "http://localhost:4200",
  "http://127.0.0.1:8000"
]
```

## How to Check Your Frontend Port

### Option 1: Check Browser Address Bar
Look at your browser URL when you opened the frontend:
- `file:///home/hg/...` ❌ **This will NOT work** (no HTTP server)
- `http://localhost:8000` ✅ Works (already in CORS list)
- `http://localhost:5500` ❌ Not in CORS list (need to add)
- `http://127.0.0.1:5500` ❌ Not in CORS list (need to add)

### Option 2: Check Your Terminal
If you started a server, check what port it shows:
```bash
# Python HTTP server
python3 -m http.server 8000  # Uses port 8000

# VS Code Live Server
# Usually uses port 5500 or 5501

# Node http-server
http-server -p 8000  # You specify the port
```

## Solutions

### Solution 1: Serve Frontend on Port 8000 ✅ RECOMMENDED

**Using Python (easiest):**
```bash
cd /home/hg/Desktop/Freelance/isp/BroadbandBillingSystem/frontend
python3 -m http.server 8000
```

Then open: `http://localhost:8000`

**Using Node http-server:**
```bash
# Install if needed
npm install -g http-server

# Serve on port 8000
cd /home/hg/Desktop/Freelance/isp/BroadbandBillingSystem/frontend
http-server -p 8000
```

Then open: `http://localhost:8000`

### Solution 2: Add Your Port to CORS Origins

If you're using a different port (e.g., 5500), update `appsettings.json`:

```json
"CorsOrigins": [
  "http://localhost:8000",
  "http://localhost:3000",
  "http://localhost:4200",
  "http://localhost:5500",
  "http://127.0.0.1:8000",
  "http://127.0.0.1:5500"
]
```

Then restart the API:
```bash
cd /home/hg/Desktop/Freelance/isp/BroadbandBillingSystem/src/BroadbandBilling.API
dotnet run
```

## ⚠️ NEVER Open HTML Files Directly

**DON'T DO THIS:**
- Double-clicking `index.html` (opens as `file://`)
- Right-click → Open with Browser (opens as `file://`)

**DO THIS:**
- Always serve via HTTP server
- Access via `http://localhost:PORT`

## Verify It's Working

1. **Start Backend API:**
```bash
cd /home/hg/Desktop/Freelance/isp/BroadbandBillingSystem/src/BroadbandBilling.API
dotnet run
```
Should see: `Now listening on: http://localhost:5286`

2. **Start Frontend Server:**
```bash
cd /home/hg/Desktop/Freelance/isp/BroadbandBillingSystem/frontend
python3 -m http.server 8000
```
Should see: `Serving HTTP on 0.0.0.0 port 8000`

3. **Open Browser:**
- Go to: `http://localhost:8000`
- Try login with: `admin` / `Admin@123`
- Should work without "Failed to fetch" error

## Test in Browser Console

Open browser DevTools (F12), go to Console, and run:
```javascript
// Check current origin
console.log('Current Origin:', window.location.origin);

// Test API connection
fetch('http://localhost:5286/api/Auth/admin/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ username: 'admin', password: 'Admin@123' })
})
.then(r => r.json())
.then(d => console.log('Success:', d))
.catch(e => console.error('Error:', e));
```

Expected Output:
- If CORS works: You'll see the login response with `accessToken`
- If CORS fails: `Failed to fetch` or CORS error in console
