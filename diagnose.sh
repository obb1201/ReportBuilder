#!/bin/bash
# Compile Error Troubleshooting Script

echo "============================================"
echo "Report Builder - Compile Error Diagnostics"
echo "============================================"
echo ""

echo "Step 1: Checking .NET Version..."
if ! command -v dotnet &> /dev/null; then
    echo "ERROR: .NET SDK not found!"
    echo "Please install .NET 9 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi
dotnet --version
echo ""

echo "Step 2: Cleaning Solution..."
dotnet clean
find . -type d -name "bin" -exec rm -rf {} + 2>/dev/null
find . -type d -name "obj" -exec rm -rf {} + 2>/dev/null
echo "Clean complete."
echo ""

echo "Step 3: Restoring NuGet Packages..."
if ! dotnet restore; then
    echo "ERROR: Package restore failed!"
    echo "Try running: dotnet nuget locals all --clear"
    exit 1
fi
echo ""

echo "Step 4: Building Solution..."
if ! dotnet build --no-incremental > build-output.txt 2>&1; then
    echo ""
    echo "BUILD FAILED - Errors detected:"
    echo "============================================"
    grep -i "error" build-output.txt
    echo "============================================"
    echo ""
    echo "Full build output saved to: build-output.txt"
    echo "Please share the errors above for help."
    exit 1
else
    echo ""
    echo "BUILD SUCCESSFUL!"
    echo "============================================"
    grep -i "succeeded" build-output.txt
    echo "============================================"
fi
echo ""

echo "Step 5: Verifying Projects..."
for project in Core Metadata Infrastructure Api Web.Mvc; do
    cd "ReportBuilder.$project" 2>/dev/null || continue
    if dotnet build > /dev/null 2>&1; then
        echo "[OK] ReportBuilder.$project"
    else
        echo "[FAIL] ReportBuilder.$project"
    fi
    cd ..
done

echo ""
echo "============================================"
echo "Diagnostics Complete!"
echo "============================================"
echo ""
