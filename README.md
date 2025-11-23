# Salesforce-Style Report Builder - Metadata Service Layer

A complete ASP.NET Core 9 + Blazor solution for building a Salesforce-style report builder with dynamic metadata extraction from Enterprise WSDL.

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Blazor Web UI (Report Builder)               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Object       │  │ Field Panel  │  │ Filter       │          │
│  │ Selector     │  │              │  │ Builder      │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ASP.NET Core Web API                         │
│  ┌──────────────────────────────────────────────────────┐      │
│  │  GET /api/metadata/objects                           │      │
│  │  GET /api/metadata/objects/{name}                    │      │
│  │  GET /api/metadata/objects/{name}/fields             │      │
│  │  POST /api/metadata/sync/from-wsdl                   │      │
│  └──────────────────────────────────────────────────────┘      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                  Core Business Logic                            │
│  ┌──────────────────┐         ┌─────────────────────┐          │
│  │ WSDL Metadata    │         │ Metadata Repository │          │
│  │ Extractor        │────────▶│ (EF Core)          │          │
│  └──────────────────┘         └─────────────────────┘          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    SQL Server Database                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │ Metadata     │  │ Metadata     │  │ Metadata     │          │
│  │ Objects      │  │ Fields       │  │ Relationships│          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└─────────────────────────────────────────────────────────────────┘
```

## 📁 Solution Structure

```
ReportBuilder/
├── ReportBuilder.Core/              # Domain models & interfaces
│   ├── Models/
│   │   └── Metadata/
│   │       ├── MetadataObject.cs
│   │       ├── MetadataField.cs
│   │       └── MetadataRelationship.cs
│   └── Interfaces/
│       ├── IMetadataRepository.cs
│       └── IWsdlMetadataExtractor.cs
│
├── ReportBuilder.Metadata/          # WSDL parsing & extraction
│   └── Services/
│       └── WsdlMetadataExtractor.cs
│
├── ReportBuilder.Infrastructure/    # Data access & persistence
│   ├── Data/
│   │   ├── MetadataDbContext.cs
│   │   ├── Entities/
│   │   └── Migrations/
│   └── Repositories/
│       └── MetadataRepository.cs
│
├── ReportBuilder.Api/               # REST API endpoints
│   └── Controllers/
│       └── MetadataController.cs
│
├── ReportBuilder.Web/               # Blazor Server UI
│   ├── Components/
│   ├── Pages/
│   └── Services/
│
└── ReportBuilder.WsdlTester/        # Console app for testing
    └── Program.cs
```

## 🚀 Getting Started

### Prerequisites

- .NET 9 SDK
- SQL Server 2019+ (or SQL Server Express)
- Visual Studio 2022 or VS Code
- Your Salesforce Enterprise WSDL file

### Step 1: Create the Solution

```bash
# Create solution directory
mkdir ReportBuilder
cd ReportBuilder

# Create solution file
dotnet new sln -n ReportBuilder

# Create projects
dotnet new classlib -n ReportBuilder.Core
dotnet new classlib -n ReportBuilder.Metadata
dotnet new classlib -n ReportBuilder.Infrastructure
dotnet new webapi -n ReportBuilder.Api
dotnet new blazorserver -n ReportBuilder.Web
dotnet new console -n ReportBuilder.WsdlTester

# Add projects to solution
dotnet sln add ReportBuilder.Core
dotnet sln add ReportBuilder.Metadata
dotnet sln add ReportBuilder.Infrastructure
dotnet sln add ReportBuilder.Api
dotnet sln add ReportBuilder.Web
dotnet sln add ReportBuilder.WsdlTester
```

### Step 2: Add Project References

```bash
# Metadata depends on Core
cd ReportBuilder.Metadata
dotnet add reference ../ReportBuilder.Core
cd ..

# Infrastructure depends on Core
cd ReportBuilder.Infrastructure
dotnet add reference ../ReportBuilder.Core
cd ..

# API depends on all layers
cd ReportBuilder.Api
dotnet add reference ../ReportBuilder.Core
dotnet add reference ../ReportBuilder.Metadata
dotnet add reference ../ReportBuilder.Infrastructure
cd ..

# Web depends on Core and calls API
cd ReportBuilder.Web
dotnet add reference ../ReportBuilder.Core
cd ..

# WsdlTester depends on Metadata
cd ReportBuilder.WsdlTester
dotnet add reference ../ReportBuilder.Core
dotnet add reference ../ReportBuilder.Metadata
cd ..
```

### Step 3: Install NuGet Packages

```bash
# Core (no external dependencies needed)

# Metadata
cd ReportBuilder.Metadata
dotnet add package Microsoft.Extensions.Logging.Abstractions
cd ..

# Infrastructure
cd ReportBuilder.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
cd ..

# API
cd ReportBuilder.Api
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
cd ..

# Web
cd ReportBuilder.Web
dotnet add package MudBlazor
cd ..
```

### Step 4: Set Up Database

1. Update connection string in `ReportBuilder.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MetadataDb": "Server=localhost;Database=ReportBuilderMetadata;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

2. Run the SQL migration script:

```bash
sqlcmd -S localhost -d master -i ReportBuilder.Infrastructure/Data/Migrations/001_InitialSchema.sql
```

Or use SQL Server Management Studio to run the script.

### Step 5: Test WSDL Extraction

```bash
cd ReportBuilder.WsdlTester

# Run the extractor
dotnet run -- path/to/your/enterprise.wsdl metadata.json

# This will:
# - Validate your WSDL
# - Extract all objects, fields, and relationships
# - Export to JSON for review
```

### Step 6: Configure and Run API

1. Update `ReportBuilder.Api/Program.cs` to register services:

```csharp
using Microsoft.EntityFrameworkCore;
using ReportBuilder.Core.Interfaces;
using ReportBuilder.Infrastructure.Data;
using ReportBuilder.Infrastructure.Repositories;
using ReportBuilder.Metadata.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<MetadataDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MetadataDb")));

// Add repositories and services
builder.Services.AddScoped<IMetadataRepository, MetadataRepository>();
builder.Services.AddScoped<IWsdlMetadataExtractor, WsdlMetadataExtractor>();

// Add CORS for Blazor
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazor");
app.UseAuthorization();
app.MapControllers();

app.Run();
```

2. Run the API:

```bash
cd ReportBuilder.Api
dotnet run
```

3. Test the API at: `https://localhost:5000/swagger`

### Step 7: Sync Metadata to Database

Using the API:

```bash
curl -X POST "https://localhost:5000/api/metadata/sync/from-wsdl" \
  -H "Content-Type: application/json" \
  -d '{
    "wsdlPath": "C:/path/to/enterprise.wsdl",
    "clearExisting": true
  }'
```

Or use Swagger UI to test the endpoint.

## 📊 Key Features Implemented

### ✅ Metadata Extraction
- Complete WSDL parsing for all sObjects
- Field metadata with data types and constraints
- Reference relationships (lookup/master-detail)
- Child relationships (one-to-many)
- Field capabilities (filterable, sortable, aggregatable)

### ✅ API Endpoints
- `GET /api/metadata/objects` - List all objects
- `GET /api/metadata/objects/{name}` - Get object details
- `GET /api/metadata/objects/{name}/fields` - Get object fields
- `GET /api/metadata/objects/{name}/relationships` - Get relationships
- `POST /api/metadata/sync/from-wsdl` - Sync from WSDL
- `GET /api/metadata/sync/status` - Get sync status

### ✅ Database Schema
- Optimized tables with proper indexes
- Foreign key relationships
- JSON storage for complex structures
- Audit fields on all tables
- Helpful views for common queries

## 🎯 Next Steps

### Phase 1: Complete Blazor UI (Week 4-6)
1. Create object selector component
2. Build field panel with drag-and-drop
3. Implement filter builder
4. Add column designer
5. Create query preview

### Phase 2: Query Generation (Week 7-8)
1. SOQL query builder
2. Filter clause generation
3. Relationship JOIN handling
4. Aggregate function support
5. Sort/group clause generation

### Phase 3: Report Execution (Week 9-10)
1. Salesforce API integration
2. Result set handling
3. Data transformation
4. Export capabilities (CSV, Excel, PDF)

### Phase 4: Advanced Features (Week 11-12)
1. Report templates
2. Scheduled reports
3. Dashboard integration
4. Custom calculations
5. Role-based access control

## 📖 Usage Examples

### Get All Objects
```csharp
GET /api/metadata/objects
```

### Get Specific Object
```csharp
GET /api/metadata/objects/Account
```

Response:
```json
{
  "apiName": "Account",
  "label": "Account",
  "isCustom": false,
  "fields": [
    {
      "apiName": "Name",
      "label": "Account Name",
      "dataType": "String",
      "length": 255,
      "isNillable": false
    }
  ],
  "relationships": [
    {
      "fromField": "OwnerId",
      "toObject": "User",
      "relationshipName": "Owner"
    }
  ]
}
```

### Search Objects
```csharp
GET /api/metadata/search?query=contact
```

## 🔧 Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "MetadataDb": "Server=localhost;Database=ReportBuilderMetadata;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## 🐛 Troubleshooting

### WSDL Parsing Issues
- Ensure WSDL is Enterprise (not Partner) WSDL
- Check file path is absolute or relative to execution directory
- Verify WSDL is not corrupted

### Database Connection Issues
- Verify SQL Server is running
- Check connection string
- Ensure database exists or create it first
- Run migrations script

### API Issues
- Check port conflicts (default: 5000/5001)
- Verify CORS settings for Blazor
- Check Swagger UI at /swagger endpoint

## 📝 License

This project is for internal use. All rights reserved.

## 👥 Support

For questions or issues, contact the development team.

---

**Built with ❤️ using ASP.NET Core 9 and Blazor**
