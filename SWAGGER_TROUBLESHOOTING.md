# 🔧 Swagger Not Working - Troubleshooting Guide

## 🎯 Quick Fixes

### Fix #1: Try the Correct URL

**Instead of:** `https://localhost:5001`  
**Try:** `https://localhost:5001/swagger`

The Swagger UI is at the `/swagger` path, not the root.

---

### Fix #2: Try HTTP Instead of HTTPS

If you get SSL errors:

**Try:** `http://localhost:5000/swagger`

---

### Fix #3: Check What Port the API is Using

Look at the console output when you run `dotnet run`. You should see:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

Use the port numbers shown in YOUR output.

---

## 🔍 Step-by-Step Troubleshooting

### Step 1: Stop and Restart the API

```bash
# Press Ctrl+C to stop the API if it's running

# Navigate to API folder
cd C:\Users\obb12\Downloads\ReportBuilder-Complete\ReportBuilder.Api

# Start it again
dotnet run
```

**Look for these messages:**
```
info: Program[0]
      Report Builder Metadata API started
info: Program[0]
      Swagger UI available at: https://localhost:...
```

### Step 2: Test the Health Endpoint First

Before trying Swagger, test if the API is running at all:

**Open browser to:**
```
http://localhost:5000/api/health
```

**You should see:**
```json
{
  "status": "healthy",
  "message": "Report Builder API is running!",
  "timestamp": "2024-11-23T...",
  "version": "1.0.0"
}
```

✅ If you see this, the API is working!

### Step 3: Now Try Swagger

**Go to:**
```
http://localhost:5000/swagger
```

or

```
https://localhost:5001/swagger
```

You should see the Swagger UI with all endpoints listed.

---

## 🐛 Common Issues & Solutions

### Issue: "This site can't be reached" / "Connection refused"

**Cause:** API is not running

**Solution:**
```bash
cd ReportBuilder.Api
dotnet run
```

Make sure you see "Application started" message.

---

### Issue: "Your connection is not private" (SSL Warning)

**Cause:** Development HTTPS certificate not trusted

**Solution Option A - Accept the risk:**
1. Click "Advanced"
2. Click "Proceed to localhost (unsafe)"

**Solution Option B - Use HTTP:**
```
http://localhost:5000/swagger
```

**Solution Option C - Trust the certificate:**
```bash
dotnet dev-certs https --trust
```

Then restart the API.

---

### Issue: "404 Not Found"

**Cause:** Wrong URL path

**Solution:** Make sure you're using `/swagger` at the end:
```
http://localhost:5000/swagger   ✅ Correct
http://localhost:5000            ❌ Wrong
```

---

### Issue: Port 5000/5001 already in use

**Error message:**
```
System.IO.IOException: Failed to bind to address https://127.0.0.1:5001: address already in use.
```

**Solution:**

Find what's using the port:
```bash
# Windows
netstat -ano | findstr :5001

# Kill the process
taskkill /PID <process_id> /F
```

Or change the port in `appsettings.json`:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5050"
      },
      "Https": {
        "Url": "https://localhost:5051"
      }
    }
  }
}
```

---

### Issue: "Cannot connect to database" errors in Swagger

**Cause:** Database connection string is wrong or SQL Server is not running

**Solution:**

1. Test SQL Server connection:
```bash
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

2. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "MetadataDb": "Server=localhost;Database=ReportBuilderMetadata;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

3. Make sure database exists:
```sql
USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'ReportBuilderMetadata')
BEGIN
    CREATE DATABASE ReportBuilderMetadata;
END
GO
```

**Note:** The API will start even if database is not reachable. Database errors only happen when you call endpoints that need it.

---

## ✅ Verification Checklist

Run through this checklist:

- [ ] API is running (see "Application started" message)
- [ ] You can access `http://localhost:5000/api/health`
- [ ] You can access `http://localhost:5000/swagger`
- [ ] You see Swagger UI with API endpoints
- [ ] SQL Server is running
- [ ] Database `ReportBuilderMetadata` exists

---

## 🎯 Quick Test Commands

### Test 1: Health Check
```bash
curl http://localhost:5000/api/health
```

### Test 2: Get Objects (requires database)
```bash
curl http://localhost:5000/api/metadata/objects
```

### Test 3: PowerShell Test
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/health"
```

---

## 📋 What to Check in Console Output

When you run `dotnet run`, you should see:

```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Program[0]
      Report Builder Metadata API started
```

**If you see errors instead:**
- Post the error message
- Check the solution above
- Make sure all packages are restored: `dotnet restore`

---

## 🆘 Still Not Working?

### Share These Details:

1. **What URL did you try?**
   - Example: `https://localhost:5001/swagger`

2. **What do you see?**
   - Blank page?
   - Error message? (copy the exact message)
   - Browser can't connect?

3. **Console output when starting API**
   - Copy the full output from `dotnet run`

4. **Test the health endpoint:**
   ```
   http://localhost:5000/api/health
   ```
   What response do you get?

---

## 💡 Pro Tips

1. **Always use `/swagger` path** - not just `localhost:5000`
2. **Try HTTP first** - easier than dealing with SSL in development
3. **Check console** - look for the actual ports in the output
4. **Test health endpoint** - confirms API is responding
5. **Restart API** - sometimes ports get stuck

---

## ✨ Expected Working Setup

Once working, you should:

1. Open: `http://localhost:5000/swagger`
2. See: Swagger UI with green "Schemas" section
3. See: Two controllers:
   - **Health** - Test endpoints
   - **Metadata** - Your metadata API endpoints
4. Click any endpoint → "Try it out" → "Execute" → Get a response

---

**Try these fixes and let me know what happens!**
