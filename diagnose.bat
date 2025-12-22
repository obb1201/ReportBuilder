@echo off
REM Compile Error Troubleshooting Script
echo ============================================
echo Report Builder - Compile Error Diagnostics
echo ============================================
echo.

echo Step 1: Checking .NET Version...
dotnet --version
if errorlevel 1 (
    echo ERROR: .NET SDK not found!
    echo Please install .NET 9 SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo.

echo Step 2: Cleaning Solution...
dotnet clean
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
for /d /r . %%d in (bin,obj) do @if exist "%%d" rmdir /s /q "%%d"
echo Clean complete.
echo.

echo Step 3: Restoring NuGet Packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Package restore failed!
    echo Try running: dotnet nuget locals all --clear
    pause
    exit /b 1
)
echo.

echo Step 4: Building Solution...
dotnet build --no-incremental > build-output.txt 2>&1
if errorlevel 1 (
    echo.
    echo BUILD FAILED - Errors detected:
    echo ============================================
    type build-output.txt | findstr /i "error"
    echo ============================================
    echo.
    echo Full build output saved to: build-output.txt
    echo Please share the errors above for help.
    pause
    exit /b 1
) else (
    echo.
    echo BUILD SUCCESSFUL!
    echo ============================================
    type build-output.txt | findstr /i "succeeded"
    echo ============================================
)
echo.

echo Step 5: Verifying Projects...
echo Checking Core...
cd ReportBuilder.Core
dotnet build > nul 2>&1
if errorlevel 1 (echo [FAIL] ReportBuilder.Core) else (echo [OK] ReportBuilder.Core)
cd ..

echo Checking Metadata...
cd ReportBuilder.Metadata
dotnet build > nul 2>&1
if errorlevel 1 (echo [FAIL] ReportBuilder.Metadata) else (echo [OK] ReportBuilder.Metadata)
cd ..

echo Checking Infrastructure...
cd ReportBuilder.Infrastructure
dotnet build > nul 2>&1
if errorlevel 1 (echo [FAIL] ReportBuilder.Infrastructure) else (echo [OK] ReportBuilder.Infrastructure)
cd ..

echo Checking API...
cd ReportBuilder.Api
dotnet build > nul 2>&1
if errorlevel 1 (echo [FAIL] ReportBuilder.Api) else (echo [OK] ReportBuilder.Api)
cd ..

echo Checking MVC...
cd ReportBuilder.Web.Mvc
dotnet build > nul 2>&1
if errorlevel 1 (echo [FAIL] ReportBuilder.Web.Mvc) else (echo [OK] ReportBuilder.Web.Mvc)
cd ..

echo.
echo ============================================
echo Diagnostics Complete!
echo ============================================
echo.
pause
