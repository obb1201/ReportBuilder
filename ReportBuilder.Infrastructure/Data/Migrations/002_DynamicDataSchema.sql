-- Dynamic Salesforce Data Tables Schema
-- This creates tables to store actual Salesforce data based on metadata

-- Table to track which objects have been populated with data
CREATE TABLE SalesforceObjectData (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ObjectApiName NVARCHAR(100) NOT NULL UNIQUE,
    RecordCount INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    LastUpdatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsPopulated BIT NOT NULL DEFAULT 0,
    CONSTRAINT UQ_ObjectData_ApiName UNIQUE (ObjectApiName)
);

-- Index for quick lookup
CREATE INDEX IX_SalesforceObjectData_ApiName ON SalesforceObjectData(ObjectApiName);

-- Track generated data records
CREATE TABLE GeneratedDataLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ObjectApiName NVARCHAR(100) NOT NULL,
    RecordsGenerated INT NOT NULL,
    GeneratedDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL, -- 'Success', 'Failed', 'InProgress'
    ErrorMessage NVARCHAR(MAX) NULL
);

-- Query execution log
CREATE TABLE QueryExecutionLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SoqlQuery NVARCHAR(MAX) NOT NULL,
    SqlQuery NVARCHAR(MAX) NOT NULL,
    ExecutionTimeMs INT NOT NULL,
    RecordCount INT NOT NULL,
    ExecutedDate DATETIME NOT NULL DEFAULT GETDATE(),
    UserId NVARCHAR(100) NULL,
    Success BIT NOT NULL DEFAULT 1,
    ErrorMessage NVARCHAR(MAX) NULL
);

CREATE INDEX IX_QueryLog_ExecutedDate ON QueryExecutionLog(ExecutedDate DESC);

GO
