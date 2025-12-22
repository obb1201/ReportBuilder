# 🔧 COMPILE ERRORS - Complete Fix Guide

## ✅ Already Fixed

1. ✅ **Removed ReportBuilder.Web** (Blazor project deleted)
2. ✅ **Updated solution file** (removed Blazor references)
3. ✅ **Fixed DataController** (correct repository methods)

---

## 🚀 Quick Fix Process

### Windows:
```batch
diagnose.bat
```

### Linux/Mac:
```bash
chmod +x diagnose.sh
./diagnose.sh
```

This will:
1. Check .NET version
2. Clean all projects
3. Restore packages
4. Build and show errors
5. Test each project individually

---

## 🔍 Common Errors & Solutions

### ERROR 1: "Project file does not exist: ReportBuilder.Web"

**Cause:** Solution still references deleted Blazor project

**Fix:** Already done, but verify:
```bash
# Check solution file
grep "ReportBuilder.Web" ReportBuilder.sln

# Should only show: ReportBuilder.Web.Mvc
# Should NOT show: ReportBuilder.Web (without .Mvc)
```

If still shows ReportBuilder.Web:
```bash
# Manually edit ReportBuilder.sln and remove lines containing:
# ReportBuilder.Web\ReportBuilder.Web.csproj
# But keep: ReportBuilder.Web.Mvc
```

---

### ERROR 2: "CS1061: IMetadataRepository does not contain definition for..."

**Files to check:**
- `ReportBuilder.Api/Controllers/DataController.cs`

**Correct code should be:**
```csharp
// Line ~43 - CORRECT:
var metadataObject = await _metadataRepository.GetObjectByNameAsync(objectName);
var fields = await _metadataRepository.GetFieldsForObjectAsync(objectName);
metadataObject.Fields = fields;

// Line ~149 - CORRECT:
var metadata = await _metadataRepository.GetObjectByNameAsync(table);
```

**WRONG code (if you see this, fix it):**
```csharp
// WRONG:
var metadataObject = await _metadataRepository.GetObjectWithFieldsAsync(objectName);

// WRONG:
var metadata = await _metadataRepository.GetObjectAsync(table);
```

---

### ERROR 3: "The type or namespace name 'Bogus' could not be found"

**Cause:** NuGet packages not restored

**Fix:**
```bash
cd ReportBuilder.Infrastructure
dotnet restore
dotnet add package Bogus --version 35.6.1
dotnet add package Dapper --version 2.1.28  
dotnet add package Microsoft.Data.SqlClient --version 5.2.0
```

---

### ERROR 4: "CS0246: The type or namespace name 'ILogger' could not be found"

**Missing using statements in DataController.cs**

Add to top of file:
```csharp
using Microsoft.AspNetCore.Mvc;
using ReportBuilder.Core.Interfaces;
using ReportBuilder.Infrastructure.Services;
using System.Data;
```

---

### ERROR 5: "Cannot find MetadataDbContext"

**Check file exists:**
```bash
ls -la ReportBuilder.Infrastructure/Data/MetadataDbContext.cs
```

If missing, the file should be in the package. Extract again.

---

### ERROR 6: Missing Services

**Check all services exist:**
```bash
ls -la ReportBuilder.Infrastructure/Services/
```

Should show:
- DataGeneratorService.cs
- DynamicTableService.cs
- QueryExecutionService.cs
- SoqlToSqlConverter.cs

If any missing, extract package again.

---

### ERROR 7: "CS0234: The type or namespace name 'Services' does not exist"

**In Program.cs, verify using statement:**
```csharp
using ReportBuilder.Infrastructure.Services;
```

**In DataController.cs, verify:**
```csharp
using ReportBuilder.Infrastructure.Services;

// Constructor should have:
private readonly DynamicTableService _tableService;
private readonly DataGeneratorService _dataGenerator;
private readonly QueryExecutionService _queryExecutor;
```

---

### ERROR 8: Build fails but no clear error

**Force clean rebuild:**
```bash
# Windows
rmdir /s /q bin obj
for /d /r . %d in (bin,obj) do @if exist "%d" rmdir /s /q "%d"
dotnet clean
dotnet restore
dotnet build --no-incremental

# Linux/Mac
find . -name "bin" -type d -exec rm -rf {} +
find . -name "obj" -type d -exec rm -rf {} +
dotnet clean
dotnet restore
dotnet build --no-incremental
```

---

## 📋 Manual Verification Checklist

### 1. Solution File Check
```bash
cat ReportBuilder.sln | grep "ReportBuilder.Web"
```

**Expected output:** Only `ReportBuilder.Web.Mvc`  
**Bad output:** Both `ReportBuilder.Web` AND `ReportBuilder.Web.Mvc`

### 2. Projects Count
```bash
ls -d ReportBuilder.* | wc -l
```

**Expected:** 6 directories
- ReportBuilder.Api
- ReportBuilder.Core
- ReportBuilder.Infrastructure
- ReportBuilder.Metadata
- ReportBuilder.Web.Mvc
- ReportBuilder.WsdlTester

### 3. Services Files
```bash
ls ReportBuilder.Infrastructure/Services/
```

**Expected:** 4 files
- DataGeneratorService.cs
- DynamicTableService.cs
- QueryExecutionService.cs
- SoqlToSqlConverter.cs

### 4. API Controllers
```bash
ls ReportBuilder.Api/Controllers/
```

**Expected:** 3 files
- DataController.cs
- HealthController.cs
- MetadataController.cs

---

## 🔨 Nuclear Option - Complete Reset

If nothing works, try complete reset:

```bash
# 1. Extract fresh copy from ZIP
unzip ReportBuilder-Complete.zip -d ReportBuilder-Fresh

# 2. Navigate to fresh copy
cd ReportBuilder-Fresh

# 3. Clean everything
dotnet clean
find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null
find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null

# 4. Restore packages
dotnet restore

# 5. Build
dotnet build
```

---

## 📸 Share Your Errors

If still having issues, please share:

1. **Full error output:**
   ```bash
   dotnet build > errors.txt 2>&1
   # Share errors.txt
   ```

2. **List of projects:**
   ```bash
   ls -la | grep "ReportBuilder"
   ```

3. **.NET Version:**
   ```bash
   dotnet --version
   ```

4. **Specific error messages** - copy/paste the exact errors

---

## ✅ Expected Successful Build Output

```
Microsoft (R) Build Engine version 17.9.0+xxx
Copyright (C) Microsoft Corporation. All rights reserved.

  Determining projects to restore...
  All projects are up-to-date for restore.
  
  ReportBuilder.Core -> ...\bin\Debug\net9.0\ReportBuilder.Core.dll
  ReportBuilder.Metadata -> ...\bin\Debug\net9.0\ReportBuilder.Metadata.dll
  ReportBuilder.Infrastructure -> ...\bin\Debug\net9.0\ReportBuilder.Infrastructure.dll
  ReportBuilder.Api -> ...\bin\Debug\net9.0\ReportBuilder.Api.dll
  ReportBuilder.WsdlTester -> ...\bin\Debug\net9.0\ReportBuilder.WsdlTester.dll
  ReportBuilder.Web.Mvc -> ...\bin\Debug\net9.0\ReportBuilder.Web.Mvc.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:XX.XX
```

---

## 🎯 If You Still Have Errors

**Please provide:**

1. Run `diagnose.bat` (Windows) or `./diagnose.sh` (Linux/Mac)
2. Share the output
3. Or paste the specific error messages here

I'll help fix them immediately! 🔧

---

## 💡 Common Gotchas

1. **Case sensitivity** - Linux/Mac are case-sensitive for filenames
2. **Line endings** - Windows (CRLF) vs Unix (LF) can cause issues
3. **NuGet cache** - Sometimes needs clearing: `dotnet nuget locals all --clear`
4. **Project references** - Make sure all projects reference correct versions
5. **SDK version** - Requires .NET 9.0 SDK minimum

---

## 🚀 Once Build Succeeds

```bash
# Terminal 1
cd ReportBuilder.Api
dotnet run

# Terminal 2  
cd ReportBuilder.Web.Mvc
dotnet run

# Browser
http://localhost:5200/ReportBuilder
```

**You're ready!** 🎉
