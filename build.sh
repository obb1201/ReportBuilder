#!/bin/bash
# Build script for Report Builder Metadata Service
# Run this after copying files to your machine

echo "╔════════════════════════════════════════════════════════════╗"
echo "║  Report Builder - Build Script                            ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

# Check if .NET 9 is installed
echo "🔍 Checking .NET version..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK not found. Please install .NET 9 SDK first."
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✓ Found .NET version: $DOTNET_VERSION"

# Check if major version is 9
MAJOR_VERSION=$(echo $DOTNET_VERSION | cut -d. -f1)
if [ "$MAJOR_VERSION" != "9" ]; then
    echo "⚠️  Warning: .NET 9 is required. You have version $DOTNET_VERSION"
    echo "   The project may not build correctly."
fi
echo ""

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "❌ Failed to restore packages"
    exit 1
fi
echo "✓ Packages restored"
echo ""

# Build solution
echo "🔨 Building solution..."
dotnet build --configuration Release
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi
echo "✓ Build succeeded"
echo ""

# Run tests (if test project exists)
if [ -d "ReportBuilder.Tests" ]; then
    echo "🧪 Running tests..."
    dotnet test --configuration Release --no-build
fi

echo ""
echo "✅ Build completed successfully!"
echo ""
echo "Next steps:"
echo "  1. Set up SQL Server database (see QUICKSTART.md)"
echo "  2. Run WSDL extractor: cd ReportBuilder.WsdlTester && dotnet run -- your-wsdl.wsdl"
echo "  3. Start API: cd ReportBuilder.Api && dotnet run"
echo ""
