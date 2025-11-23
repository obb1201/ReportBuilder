@echo off
REM Build script for Report Builder Metadata Service
REM Run this after copying files to your machine

echo ================================================================
echo   Report Builder - Build Script (Windows)
echo ================================================================
echo.

REM Check if .NET 9 is installed
echo Checking .NET version...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET SDK not found. Please install .NET 9 SDK first.
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo Found .NET version: %DOTNET_VERSION%

REM Extract major version
for /f "tokens=1 delims=." %%a in ("%DOTNET_VERSION%") do set MAJOR_VERSION=%%a
if not "%MAJOR_VERSION%"=="9" (
    echo WARNING: .NET 9 is required. You have version %DOTNET_VERSION%
    echo          The project may not build correctly.
)
echo.

REM Restore packages
echo Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore packages
    pause
    exit /b 1
)
echo Packages restored successfully
echo.

REM Build solution
echo Building solution...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo Build succeeded
echo.

REM Run tests (if test project exists)
if exist "ReportBuilder.Tests" (
    echo Running tests...
    dotnet test --configuration Release --no-build
)

echo.
echo ================================================================
echo   Build completed successfully!
echo ================================================================
echo.
echo Next steps:
echo   1. Set up SQL Server database (see QUICKSTART.md)
echo   2. Run WSDL extractor:
echo      cd ReportBuilder.WsdlTester
echo      dotnet run -- "C:\path\to\your-wsdl.wsdl"
echo   3. Start API:
echo      cd ReportBuilder.Api
echo      dotnet run
echo.
pause
