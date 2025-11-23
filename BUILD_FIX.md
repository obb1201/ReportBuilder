# 🔧 Build Issues - Quick Fix Guide

## Issue: Missing ReportBuilder.Api.csproj

**Error Message:**
```
error MSB3202: The project file "...\ReportBuilder.Api\ReportBuilder.Api.csproj" was not found.
```

### ✅ Solution: Download the NEW ZIP Package

I've just created a **complete** ZIP package with all missing files included.

**What was missing in the first ZIP:**
- ❌ ReportBuilder.Api/ReportBuilder.Api.csproj
- ❌ ReportBuilder.Api/Program.cs
- ❌ ReportBuilder.Api/appsettings.json
- ❌ ReportBuilder.Api/Properties/launchSettings.json

**What's included in the NEW ZIP:**
- ✅ Complete API project with all files
- ✅ Program.cs with full configuration
- ✅ appsettings.json with connection string
- ✅ Swagger configuration
- ✅ CORS setup
- ✅ All 5 projects ready to build

---

## 📦 Download the Updated Package

**New File:** [ReportBuilder-Complete.zip](computer:///mnt/user-data/outputs/ReportBuilder-Complete.zip)

**File Size:** 44 KB (was 41 KB before)

---

## 🚀 Steps to Fix

### Option 1: Download New ZIP (Recommended)
1. Delete your current `ReportBuilder-Complete` folder
2. Download the NEW `ReportBuilder-Complete.zip`
3. Extract to `C:\Users\obb12\Downloads\ReportBuilder-Complete\`
4. Run `build.bat` again

### Option 2: Add Missing Files Manually
If you want to keep your current folder, add these files:

**1. Create `ReportBuilder.Api/ReportBuilder.Api.csproj`:**
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\ReportBuilder.Core\ReportBuilder.Core.csproj" />
    <ProjectReference Include="..\ReportBuilder.Metadata\ReportBuilder.Metadata.csproj" />
    <ProjectReference Include="..\ReportBuilder.Infrastructure\ReportBuilder.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.9.0" />
  </ItemGroup>
</Project>
```

**2. Create `ReportBuilder.Api/Program.cs`:**
(See the full file in the outputs folder)

**3. Create `ReportBuilder.Api/appsettings.json`:**
```json
{
  "ConnectionStrings": {
    "MetadataDb": "Server=localhost;Database=ReportBuilderMetadata;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## ✅ Verify Your Build

After fixing, run:
```bash
cd C:\Users\obb12\Downloads\ReportBuilder-Complete
build.bat
```

**Expected Output:**
```
Restoring NuGet packages...
Packages restored successfully

Building solution...
Build succeeded
    0 Warning(s)
    0 Error(s)
```

---

## 🎯 Quick Test After Build

Once build succeeds, test the WSDL extractor:

```bash
cd ReportBuilder.WsdlTester
dotnet run -- "C:\Path\To\Your\enterprise.wsdl" test.json
```

**You should see:**
```
╔════════════════════════════════════════╗
║  Salesforce WSDL Metadata Extractor   ║
╚════════════════════════════════════════╝

✓ WSDL validation passed
  Objects found: 946
✓ Extraction completed
```

---

## 💡 Why This Happened

The initial ZIP was missing the API project files because:
1. The API folder existed but was empty (only had Controllers subfolder)
2. The essential project files weren't copied to outputs
3. This has now been fixed in the NEW ZIP

---

## 📞 Still Having Issues?

### Error: "SDK version not found"
**Solution:** Install .NET 9 SDK from https://dot.net

### Error: "Connection string error"
**Solution:** Update `appsettings.json` with your SQL Server details

### Error: "Package restore failed"
**Solution:** Run `dotnet restore` manually first

### Project doesn't appear in Visual Studio
**Solution:** Close VS, delete `.vs` folder, reopen solution

---

## ✨ You're Ready!

After downloading the NEW ZIP and running `build.bat`, you should have:
- ✅ All 5 projects building successfully
- ✅ 0 errors, 0 warnings
- ✅ Ready to test WSDL extraction
- ✅ Ready to set up database and run API

**Download the updated ZIP now and try again!** 🚀
