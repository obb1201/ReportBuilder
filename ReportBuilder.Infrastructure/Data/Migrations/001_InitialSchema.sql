-- =============================================
-- Report Builder Metadata Database Schema
-- Version: 1.0.0
-- Description: Initial schema for metadata storage
-- =============================================

-- Create MetadataObjects table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetadataObjects')
BEGIN
    CREATE TABLE [dbo].[MetadataObjects] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ApiName] NVARCHAR(255) NOT NULL,
        [Label] NVARCHAR(255) NOT NULL,
        [Namespace] NVARCHAR(100) NULL,
        [IsCustom] BIT NOT NULL DEFAULT 0,
        [IsStandard] BIT NOT NULL DEFAULT 1,
        [BaseType] NVARCHAR(50) NOT NULL DEFAULT 'sObject',
        [CapabilitiesJson] NVARCHAR(MAX) NOT NULL,
        [AuditJson] NVARCHAR(MAX) NOT NULL,
        [LastSyncedAt] DATETIME2 NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [UQ_MetadataObjects_ApiName] UNIQUE ([ApiName])
    );

    -- Indexes for MetadataObjects
    CREATE INDEX [IX_MetadataObjects_Label] ON [dbo].[MetadataObjects]([Label]);
    CREATE INDEX [IX_MetadataObjects_IsCustom] ON [dbo].[MetadataObjects]([IsCustom]);
    CREATE INDEX [IX_MetadataObjects_LastSyncedAt] ON [dbo].[MetadataObjects]([LastSyncedAt]);
    
    PRINT 'Table MetadataObjects created successfully';
END
GO

-- Create MetadataFields table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetadataFields')
BEGIN
    CREATE TABLE [dbo].[MetadataFields] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ObjectId] INT NOT NULL,
        [ApiName] NVARCHAR(255) NOT NULL,
        [Label] NVARCHAR(255) NOT NULL,
        [DataType] NVARCHAR(50) NOT NULL,
        [Length] INT NULL,
        [Precision] INT NULL,
        [Scale] INT NULL,
        [IsNillable] BIT NOT NULL DEFAULT 1,
        [DefaultValue] NVARCHAR(255) NULL,
        [IsCalculated] BIT NOT NULL DEFAULT 0,
        [IsCustom] BIT NOT NULL DEFAULT 0,
        [IsRequired] BIT NOT NULL DEFAULT 0,
        [IsUnique] BIT NOT NULL DEFAULT 0,
        [IsExternalId] BIT NOT NULL DEFAULT 0,
        [ReferenceTo] NVARCHAR(255) NULL,
        [RelationshipName] NVARCHAR(255) NULL,
        [HelpText] NVARCHAR(MAX) NULL,
        [CapabilitiesJson] NVARCHAR(MAX) NOT NULL,
        [PicklistValuesJson] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_MetadataFields_MetadataObjects] 
            FOREIGN KEY ([ObjectId]) REFERENCES [dbo].[MetadataObjects]([Id]) 
            ON DELETE CASCADE,
        CONSTRAINT [UQ_MetadataFields_ObjectId_ApiName] 
            UNIQUE ([ObjectId], [ApiName])
    );

    -- Indexes for MetadataFields
    CREATE INDEX [IX_MetadataFields_DataType] ON [dbo].[MetadataFields]([DataType]);
    CREATE INDEX [IX_MetadataFields_IsCustom] ON [dbo].[MetadataFields]([IsCustom]);
    CREATE INDEX [IX_MetadataFields_IsCalculated] ON [dbo].[MetadataFields]([IsCalculated]);
    CREATE INDEX [IX_MetadataFields_ReferenceTo] ON [dbo].[MetadataFields]([ReferenceTo]);
    
    PRINT 'Table MetadataFields created successfully';
END
GO

-- Create MetadataRelationships table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetadataRelationships')
BEGIN
    CREATE TABLE [dbo].[MetadataRelationships] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [FromObjectId] INT NOT NULL,
        [FromField] NVARCHAR(255) NOT NULL,
        [ToObject] NVARCHAR(255) NOT NULL,
        [RelationshipName] NVARCHAR(255) NOT NULL,
        [Cardinality] NVARCHAR(50) NOT NULL DEFAULT 'ManyToOne',
        [RelationshipType] NVARCHAR(50) NOT NULL DEFAULT 'Lookup',
        [IsRequired] BIT NOT NULL DEFAULT 0,
        [IsPolymorphic] BIT NOT NULL DEFAULT 0,
        [PolymorphicTargetsJson] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_MetadataRelationships_MetadataObjects] 
            FOREIGN KEY ([FromObjectId]) REFERENCES [dbo].[MetadataObjects]([Id]) 
            ON DELETE CASCADE
    );

    -- Indexes for MetadataRelationships
    CREATE INDEX [IX_MetadataRelationships_FromObjectId_FromField] 
        ON [dbo].[MetadataRelationships]([FromObjectId], [FromField]);
    CREATE INDEX [IX_MetadataRelationships_ToObject] 
        ON [dbo].[MetadataRelationships]([ToObject]);
    CREATE INDEX [IX_MetadataRelationships_RelationshipName] 
        ON [dbo].[MetadataRelationships]([RelationshipName]);
    
    PRINT 'Table MetadataRelationships created successfully';
END
GO

-- Create ChildRelationships table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ChildRelationships')
BEGIN
    CREATE TABLE [dbo].[ChildRelationships] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ParentObjectId] INT NOT NULL,
        [ChildObject] NVARCHAR(255) NOT NULL,
        [ViaForeignKey] NVARCHAR(255) NOT NULL,
        [RelationshipName] NVARCHAR(255) NOT NULL,
        [Cardinality] NVARCHAR(50) NOT NULL DEFAULT 'OneToMany',
        [IsCascadeDelete] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [FK_ChildRelationships_MetadataObjects] 
            FOREIGN KEY ([ParentObjectId]) REFERENCES [dbo].[MetadataObjects]([Id]) 
            ON DELETE CASCADE
    );

    -- Indexes for ChildRelationships
    CREATE INDEX [IX_ChildRelationships_ParentObjectId_ChildObject] 
        ON [dbo].[ChildRelationships]([ParentObjectId], [ChildObject]);
    CREATE INDEX [IX_ChildRelationships_RelationshipName] 
        ON [dbo].[ChildRelationships]([RelationshipName]);
    
    PRINT 'Table ChildRelationships created successfully';
END
GO

-- Create MetadataSyncStatus table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MetadataSyncStatus')
BEGIN
    CREATE TABLE [dbo].[MetadataSyncStatus] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [LastSyncStarted] DATETIME2 NULL,
        [LastSyncCompleted] DATETIME2 NULL,
        [IsSuccess] BIT NOT NULL DEFAULT 0,
        [ErrorMessage] NVARCHAR(MAX) NULL,
        [ObjectsProcessed] INT NOT NULL DEFAULT 0,
        [FieldsProcessed] INT NOT NULL DEFAULT 0,
        [RelationshipsProcessed] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Index for MetadataSyncStatus
    CREATE INDEX [IX_MetadataSyncStatus_LastSyncCompleted] 
        ON [dbo].[MetadataSyncStatus]([LastSyncCompleted]);
    
    PRINT 'Table MetadataSyncStatus created successfully';
END
GO

-- Create useful views

-- View: All objects with field counts
CREATE OR ALTER VIEW [dbo].[vw_MetadataObjectSummary]
AS
SELECT 
    o.Id,
    o.ApiName,
    o.Label,
    o.IsCustom,
    o.IsStandard,
    o.LastSyncedAt,
    COUNT(DISTINCT f.Id) as FieldCount,
    COUNT(DISTINCT r.Id) as RelationshipCount,
    COUNT(DISTINCT cr.Id) as ChildRelationshipCount
FROM [dbo].[MetadataObjects] o
LEFT JOIN [dbo].[MetadataFields] f ON f.ObjectId = o.Id
LEFT JOIN [dbo].[MetadataRelationships] r ON r.FromObjectId = o.Id
LEFT JOIN [dbo].[ChildRelationships] cr ON cr.ParentObjectId = o.Id
GROUP BY 
    o.Id, o.ApiName, o.Label, o.IsCustom, o.IsStandard, o.LastSyncedAt;
GO

-- View: All reference fields (lookups)
CREATE OR ALTER VIEW [dbo].[vw_ReferenceFields]
AS
SELECT 
    o.ApiName as ObjectApiName,
    o.Label as ObjectLabel,
    f.ApiName as FieldApiName,
    f.Label as FieldLabel,
    f.ReferenceTo,
    f.RelationshipName,
    f.IsRequired
FROM [dbo].[MetadataObjects] o
INNER JOIN [dbo].[MetadataFields] f ON f.ObjectId = o.Id
WHERE f.ReferenceTo IS NOT NULL;
GO

-- View: Latest sync status
CREATE OR ALTER VIEW [dbo].[vw_LatestSyncStatus]
AS
SELECT TOP 1
    LastSyncStarted,
    LastSyncCompleted,
    IsSuccess,
    ErrorMessage,
    ObjectsProcessed,
    FieldsProcessed,
    RelationshipsProcessed,
    DATEDIFF(SECOND, LastSyncStarted, LastSyncCompleted) as DurationSeconds
FROM [dbo].[MetadataSyncStatus]
ORDER BY CreatedAt DESC;
GO

PRINT 'Database schema created successfully';
PRINT 'Run this script in your SQL Server database to create all tables, indexes, and views';
