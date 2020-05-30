CREATE TABLE [auditlog].[PowerAutomate]
(
	[Id] INT NOT NULL IDENTITY(1,1)	
	,ClientIP VARCHAR(20) NULL
	,CreationTime DATETIME NULL
	,FlowConnectorNames VARCHAR(1000) NULL
	,FlowDetailsUrl NVARCHAR(1000) NULL
	,AuditLogId UNIQUEIDENTIFIER NOT NULL
	--,LicenseDisplayName VARCHAR(200) NULL
	,ObjectId UNIQUEIDENTIFIER NULL
	,Operation VARCHAR(200) NULL
	,OrganizationId UNIQUEIDENTIFIER NOT NULL
	,RecordType INT NOT NULL
	,ResultStatus VARCHAR(200)
	--,SharingPermission INT NULL
	,UserId VARCHAR(50) NULL
	,UserKey VARCHAR(50) NULL
	,UserType INT NULL
	--,UserTypeInitiated INT NULL
	,UserUPN VARCHAR(200) NULL
	,Version INT NULL
	,Workload VARCHAR(200)
	--,RecipientUPN VARCHAR(300)
	,[InsertedDate] DATETIME NOT NULL CONSTRAINT DF_PAInsertedDate DEFAULT GETDATE(), 
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
    @level1name = N'PowerAutomate',
    @level2type = N'COLUMN',
    @level2name = N'IsObserved'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Raw activity json from O365',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerAutomate',
    @level2type = N'COLUMN',
    @level2name = N'_raw'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Is this activity migrated from Splunk process?',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerAutomate',
    @level2type = N'COLUMN',
    @level2name = N'IsMigrated'