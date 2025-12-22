# 🔧 Common Compile Errors & Fixes

## ✅ Changes Made

1. ✅ **Removed ReportBuilder.Web** (Blazor project)
2. ✅ **Updated solution file** - Removed Blazor references
3. ✅ **Fixed DataController** - Used correct repository methods

---

## 🔨 If You Still Have Compile Errors

### Error: "IMetadataRepository does not contain..."

**File:** `ReportBuilder.Api/Controllers/DataController.cs`

**Fix Already Applied:**
- Changed `GetObjectWithFieldsAsync` → `GetObjectByNameAsync` + `GetFieldsForObjectAsync`
- Changed `GetObjectAsync` → `GetObjectByNameAsync`

---

### Error: Missing using statements

**Add to top of files as needed:**

```csharp
// DataController.cs
using System.Data;
using Microsoft.Data.SqlClient;
using ReportBuilder.Core.Interfaces;
using ReportBuilder.Infrastructure.Services;

// Services classes
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
```

---

### Error: Package not found (Bogus, Dapper, etc.)

**Solution:**
```bash
cd ReportBuilder.Infrastructure
dotnet restore
```

If still failing:
```bash
dotnet add package Bogus --version 35.6.1
dotnet add package Dapper --version 2.1.28
dotnet add package Microsoft.Data.SqlClient --version 5.2.0
```

---

### Error: Cannot find MetadataDbContext

**Fix:** Already exists in `ReportBuilder.Infrastructure/Data/MetadataDbContext.cs`

If missing, check:
```bash
cd ReportBuilder.Infrastructure
ls -la Data/MetadataDbContext.cs
```

---

### Error: Namespace 'Services' not found

**Fix:** Check that Services folder exists:
```bash
cd ReportBuilder.Infrastructure
ls -la Services/
```

Should contain:
- DataGeneratorService.cs
- DynamicTableService.cs
- QueryExecutionService.cs
- SoqlToSqlConverter.cs

---

### Error: Build failed in ReportBuilder.Web

**Already Fixed:** Project removed from solution

If you still see this:
```bash
# Make sure ReportBuilder.Web is deleted
rm -rf ReportBuilder.Web
```

---

## 🚀 Clean Build Steps

### 1. Clean Everything
```bash
dotnet clean
rm -rf */bin */obj
```

### 2. Restore Packages
```bash
dotnet restore
```

### 3. Build Solution
```bash
dotnet build
```

### 4. Build Individual Projects (if needed)
```bash
cd ReportBuilder.Core && dotnet build
cd ../ReportBuilder.Metadata && dotnet build
cd ../ReportBuilder.Infrastructure && dotnet build
cd ../ReportBuilder.Api && dotnet build
cd ../ReportBuilder.Web.Mvc && dotnet build
```

---

## 📋 Projects in Solution (After Cleanup)

1. ✅ **ReportBuilder.Core** - Domain models
2. ✅ **ReportBuilder.Metadata** - WSDL parser
3. ✅ **ReportBuilder.Infrastructure** - Database + Services
4. ✅ **ReportBuilder.Api** - REST API backend
5. ✅ **ReportBuilder.WsdlTester** - Testing tool
6. ✅ **ReportBuilder.Web.Mvc** - MVC frontend (the one we use)

**Removed:**
- ❌ ReportBuilder.Web (Blazor) - Not needed

---

## 🎯 Expected Build Output

```
Microsoft (R) Build Engine version X.X.X
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  All projects are up-to-date for restore.
  ReportBuilder.Core -> C:\Work\...\bin\Debug\net9.0\ReportBuilder.Core.dll
  ReportBuilder.Metadata -> C:\Work\...\bin\Debug\net9.0\ReportBuilder.Metadata.dll
  ReportBuilder.Infrastructure -> C:\Work\...\bin\Debug\net9.0\ReportBuilder.Infrastructure.dll
  ReportBuilder.Api -> C:\Work\...\bin\Debug\net9.0\ReportBuilder.Api.dll
  ReportBuilder.WsdlTester -> C:\Work\...\bin\Debug\net9.0\ReportBuilder.WsdlTester.dll
  ReportBuilder.Web.Mvc -> C:\Work\...\bin\Debug\net9.0\ReportBuilder.Web.Mvc.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 🔍 Verify All Files Exist

### Check Infrastructure Services:
```bash
ls -la ReportBuilder.Infrastructure/Services/
```

**Should show:**
- DataGeneratorService.cs
- DynamicTableService.cs  
- QueryExecutionService.cs
- SoqlToSqlConverter.cs

### Check API Controllers:
```bash
ls -la ReportBuilder.Api/Controllers/
```

**Should show:**
- DataController.cs
- HealthController.cs
- MetadataController.cs

### Check MVC Controllers:
```bash
ls -la ReportBuilder.Web.Mvc/Controllers/
```

**Should show:**
- DataApiController.cs
- HomeController.cs
- MetadataApiController.cs
- ReportBuilderController.cs

---

## 🐛 Still Having Issues?

### Step 1: Check .NET Version
```bash
dotnet --version
```

**Required:** .NET 9.0 or higher

### Step 2: Check SQL Server
```bash
sqlcmd -S (localdb)\MSSQLLocalDB -Q "SELECT @@VERSION"
```

Should connect successfully.

### Step 3: Verify Connection String
**File:** `ReportBuilder.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "MetadataDb": "Server=(localdb)\\MSSQLLocalDB;Database=ReportBuilderDb;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Step 4: Run Migrations
```bash
cd ReportBuilder.Infrastructure/Data/Migrations
sqlcmd -S (localdb)\MSSQLLocalDB -d ReportBuilderDb -i 001_InitialSchema.sql
sqlcmd -S (localdb)\MSSQLLocalDB -d ReportBuilderDb -i 002_DynamicDataSchema.sql
```

---

## ✅ Success Checklist

- [ ] Solution builds without errors
- [ ] All 6 projects compile
- [ ] No "missing reference" errors
- [ ] No "namespace not found" errors
- [ ] Database migrations ran successfully
- [ ] API starts on port 5000
- [ ] MVC starts on port 5200

---

## 🎉 Once It Builds Successfully

```bash
# Terminal 1 - API
cd ReportBuilder.Api
dotnet run

# Terminal 2 - MVC
cd ReportBuilder.Web.Mvc
dotnet run

# Open browser
http://localhost:5200/ReportBuilder
```

**You're ready to go!** 🚀

---

## 📞 Common Error Messages & Solutions

**"The type or namespace name 'Bogus' could not be found"**
→ Run: `dotnet restore` in ReportBuilder.Infrastructure

**"Cannot find type 'MetadataDbContext'"**
→ Check file exists: ReportBuilder.Infrastructure/Data/MetadataDbContext.cs

**"CS0246: The type or namespace name 'ILogger' could not be found"**
→ Add: `using Microsoft.Extensions.Logging;`

**"CS1061: 'IMetadataRepository' does not contain a definition for..."**
→ Already fixed in DataController.cs (check file is updated)

**"Build FAILED"**
→ Run `dotnet clean` then `dotnet build --no-incremental`

---

## 💡 Pro Tips

1. **Always clean before building** after major changes
2. **Restore packages** if you see "package not found"
3. **Build projects individually** to isolate errors
4. **Check file paths** are correct (case-sensitive on Linux)
5. **Verify all using statements** are present

---

**If you get specific error messages, share them and I can help fix!** 🔧
