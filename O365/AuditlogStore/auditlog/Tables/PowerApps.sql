CREATE TABLE [auditlog].[PowerApps]
(
	[Id] INT NOT NULL IDENTITY(1,1)
	,EnvironmentName NVARCHAR(200) NULL
	,TargetObjectId UNIQUEIDENTIFIER NULL	
	,AppName NVARCHAR(1000) NULL
	,PermissionType VARCHAR(50) NULL
	,CreationTime DATETIME NULL
	,AuditLogId UNIQUEIDENTIFIER NOT NULL
	,ObjectId UNIQUEIDENTIFIER NULL
	,Operation VARCHAR(200) NULL
	,OrganizationId UNIQUEIDENTIFIER NULL
	,RecordType INT NULL
	,ResultStatus VARCHAR(200) NULL
	,UserId VARCHAR(200) NULL
	,UserKey VARCHAR(200) NULL
	,UserType INT NULL
	,Version INT NULL
	,[Workload] VARCHAR(200)
	,[InsertedDate] DATETIME NOT NULL CONSTRAINT DF_PASInsertedDate DEFAULT GETDATE(), 
    [IsObserved] BIT NULL, 
    [_raw] NVARCHAR(MAX) NULL,
	IsMigrated BIT NULL DEFAULT 0
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'If this operation been analyzed or not',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerApps',
    @level2type = N'COLUMN',
    @level2name = N'IsObserved'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Raw activity json from O365',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerApps',
    @level2type = N'COLUMN',
    @level2name = N'_raw'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Is this activity migrated from Splunk process?',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerApps',
    @level2type = N'COLUMN',
    @level2name = N'IsMigrated'