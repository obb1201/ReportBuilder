# 🚀 Quick Start Guide - Report Builder Metadata Service

## Goal
Get the WSDL parser running and extract your Salesforce metadata in **under 30 minutes**.

## Prerequisites Checklist
- ✅ .NET 9 SDK installed (`dotnet --version` should show 9.x)
- ✅ SQL Server installed and running
- ✅ Your Salesforce Enterprise WSDL file ready

---

## Step 1: Copy Files to Your Machine (5 minutes)

1. Copy all the generated files to your local machine
2. Maintain the folder structure exactly as shown

Your folder structure should look like:
```
C:\Dev\ReportBuilder\
├── ReportBuilder.Core\
│   ├── Models\Metadata\
│   ├── Interfaces\
│   └── ReportBuilder.Core.csproj
├── ReportBuilder.Metadata\
│   ├── Services\
│   └── ReportBuilder.Metadata.csproj
├── ReportBuilder.Infrastructure\
│   ├── Data\
│   ├── Repositories\
│   └── ReportBuilder.Infrastructure.csproj
└── ReportBuilder.WsdlTester\
    ├── Program.cs
    └── ReportBuilder.WsdlTester.csproj
```

---

## Step 2: Test WSDL Extraction (10 minutes)

Open a terminal in the `ReportBuilder.WsdlTester` directory:

```bash
# Build the project
dotnet build

# Run the extractor (replace with your WSDL path)
dotnet run -- "C:\Salesforce\enterprise.wsdl" "metadata_output.json"
```

**Expected Output:**
```
╔════════════════════════════════════════════════════════════╗
║  Salesforce WSDL Metadata Extractor                       ║
║  Report Builder - Metadata Service Layer                  ║
╚════════════════════════════════════════════════════════════╝

📄 Validating WSDL: C:\Salesforce\enterprise.wsdl
✓ WSDL validation passed
  Objects found: 946

🔄 Extracting metadata from WSDL...
✓ Extraction completed in 2.34 seconds

📊 Summary Statistics
════════════════════════════════════════════════════════════
  Total Objects:              946
  Standard Objects:           856
  Custom Objects:              90
  Total Fields:            15,234
  Reference Relationships:  3,456
  Child Relationships:      2,123

💾 Exporting metadata to: metadata_output.json
✓ Metadata exported successfully
  File size: 4,523.12 KB

✅ All operations completed successfully!
```

**🎉 Success!** You now have a complete metadata catalog in JSON format.

---

## Step 3: Set Up Database (10 minutes)

### Option A: Using SQL Server Management Studio (SSMS)
1. Open SSMS
2. Connect to your SQL Server instance
3. Create a new database: `CREATE DATABASE ReportBuilderMetadata`
4. Open and execute: `ReportBuilder.Infrastructure\Data\Migrations\001_InitialSchema.sql`

### Option B: Using sqlcmd
```bash
# Create database
sqlcmd -S localhost -Q "CREATE DATABASE ReportBuilderMetadata"

# Run migration script
sqlcmd -S localhost -d ReportBuilderMetadata -i "ReportBuilder.Infrastructure\Data\Migrations\001_InitialSchema.sql"
```

**Verify:**
```sql
-- Run this in SSMS
SELECT * FROM sys.tables WHERE name LIKE 'Metadata%'

-- Should show:
-- MetadataObjects
-- MetadataFields
-- MetadataRelationships
-- ChildRelationships
-- MetadataSyncStatus
```

---

## Step 4: Review Your Metadata (5 minutes)

Open `metadata_output.json` in VS Code or any text editor.

You should see:
```json
[
  {
    "apiName": "Account",
    "label": "Account",
    "isCustom": false,
    "isStandard": true,
    "fields": [
      {
        "apiName": "Name",
        "label": "Account Name",
        "dataType": "String",
        "length": 255,
        "isNillable": false,
        "capabilities": {
          "isFilterable": true,
          "isSortable": true,
          "filterOperations": ["Equals", "NotEquals", "Like", ...]
        }
      },
      ...
    ],
    "relationships": [
      {
        "fromField": "OwnerId",
        "toObject": "User",
        "relationshipName": "Owner",
        "cardinality": "ManyToOne"
      },
      ...
    ]
  },
  ...
]
```

---

## 🎯 What You've Accomplished

✅ **WSDL Parser Working** - Extracts all metadata from your Salesforce org
✅ **Metadata Catalog Created** - Complete JSON file with all objects, fields, and relationships
✅ **Database Schema Ready** - Tables created and ready for metadata import
✅ **Foundation Complete** - Ready to build the API and UI layers

---

## Next Immediate Steps

### Today (Remaining Time)
1. **Review the metadata JSON** - Familiarize yourself with the structure
2. **Spot check 5-10 objects** - Verify fields and relationships are correct
3. **Test with custom objects** - Make sure your custom objects (`__c` suffix) are included

### Tomorrow
1. **Build the API layer** - We already have the controllers ready
2. **Test API endpoints** - Import metadata to database via API
3. **Add caching** - Redis or memory cache for fast lookups

### This Week
1. **Start Blazor UI** - Object selector component
2. **Field panel** - Display fields with metadata
3. **Basic filtering** - Simple filter builder

---

## 🆘 Common Issues & Solutions

### Issue: "WSDL file not found"
**Solution:** Use absolute path or verify file location
```bash
# Use full path
dotnet run -- "C:\Users\YourName\Documents\enterprise.wsdl"
```

### Issue: "No schema element found in WSDL"
**Solution:** Ensure you're using **Enterprise WSDL**, not Partner WSDL
- Enterprise WSDL: Specific to your org, includes custom objects
- Partner WSDL: Generic, won't work for this project

### Issue: "Database connection failed"
**Solution:** Check connection string and SQL Server status
```bash
# Test SQL Server connection
sqlcmd -S localhost -Q "SELECT @@VERSION"
```

### Issue: "Permission denied on database"
**Solution:** Grant permissions or use integrated security
```sql
-- In SSMS
CREATE LOGIN [YourDomain\YourUser] FROM WINDOWS;
USE ReportBuilderMetadata;
CREATE USER [YourDomain\YourUser] FOR LOGIN [YourDomain\YourUser];
ALTER ROLE db_owner ADD MEMBER [YourDomain\YourUser];
```

---

## 📞 Need Help?

If you encounter issues:
1. Check the error message carefully
2. Verify prerequisites are met
3. Review the troubleshooting section in README.md
4. Check file paths and permissions

---

## 🎉 Congratulations!

You've successfully set up the foundation for your Salesforce-style report builder. The hardest part (metadata extraction) is done!

**Your metadata is now:**
- ✅ Extracted from WSDL
- ✅ Structured and validated
- ✅ Ready for database import
- ✅ Available for API consumption

Next up: Build the API and start creating the report builder UI! 🚀
