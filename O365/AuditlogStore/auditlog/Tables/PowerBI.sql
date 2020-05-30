CREATE TABLE [auditlog].[PowerBI]
(
	[Id] INT NOT NULL IDENTITY(1,1)
	,[ActivityId] UNIQUEIDENTIFIER NULL
	,ClientIP VARCHAR(40) NULL
	,CreationTime DATETIME NULL
	,DatasetName NVARCHAR(200)
	,AuditLogId UNIQUEIDENTIFIER NOT NULL
	,IsSuccess BIT NULL
	,ItemName VARCHAR(200)
	,ObjectId VARCHAR(200) NULL
	,Operation VARCHAR(200)
	,OrganizationId UNIQUEIDENTIFIER NULL
	,RecordType INT NOT NULL
	,RequestId UNIQUEIDENTIFIER NULL
	,UserAgent NVARCHAR(500) NULL
	,UserId VARCHAR(200) NULL
	,UserKey VARCHAR(200) NULL
	,UserType INT NULL
	,[Workload] VARCHAR(200) NULL
	,DatasourceId UNIQUEIDENTIFIER NULL
	,DatasourceName VARCHAR(200) NULL
	,GatewayId UNIQUEIDENTIFIER NULL
	,GatewayName VARCHAR(200) NULL
	,CapacityId UNIQUEIDENTIFIER NULL
	,CapacityName VARCHAR(200) NULL
	,DashboardId UNIQUEIDENTIFIER NULL
	,DashboardName VARCHAR(200) NULL
	,WorkspaceId UNIQUEIDENTIFIER NULL
	,WorkSpaceName VARCHAR(200) NULL
	,DataflowId UNIQUEIDENTIFIER NULL
	,DataflowName VARCHAR(200) NULL
	,DataflowType VARCHAR(200) NULL
	,DataConnectivityMode VARCHAR(200) NULL
	,DatasetId UNIQUEIDENTIFIER NULL
	,DistributionMethod VARCHAR(200) NULL
	,ReportId UNIQUEIDENTIFIER NULL
	,ReportName VARCHAR(200) NULL
	,ReportType VARCHAR(200) NULL
	,ImportDisplayName VARCHAR(200) NULL
	,ImportId UNIQUEIDENTIFIER NULL
	,ImportSource VARCHAR(200) NULL
	,ImportType VARCHAR(200) NULL
	,ArtifactId UNIQUEIDENTIFIER NULL
	,ConsumptionMethod VARCHAR(200) NULL
	,AppName VARCHAR(200) NULL
	,AppReportId UNIQUEIDENTIFIER NULL
	,[InsertedDate] DATETIME NOT NULL CONSTRAINT DF_PBIInsertedDate DEFAULT GETDATE(), 
    [IsObserved] BIT NULL, 
    [_raw] NVARCHAR(MAX) NULL,
	IsMigrated BIT NULL DEFAULT 0	
	--,SubscribeeInformation
	--,FolderDisplayName VARCHAR(200) NULL
	--,FolderObjectId UNIQUEIDENTIFIER NULL
	--,AuditedArtifactInformation
	--,UserInformation
	--,Datasets	
	--,ExportedArtifactInfo
	--,CustomVisualAccessTokenResourceId VARCHAR(1000) NULL
	--,CustomVisualAccessTokenSiteUri NVARCHAR(250) NULL
	--,DataflowAccessTokenRequestParameters
	--,EmbedTokenId UNIQUEIDENTIFIER NULL
	--,RLSIdentities
	--,ArtifactName VARCHAR(200) NULL
	--,ShareWithCurrentFilter BIT NULL
	--,SharingInformation
	--,OrgAppPermission NVARCHAR(1000) NULL
	--,FolderAccessRequests 
	--,RefreshType VARCHAR(200) NULL
	--,Schedules
	--,TileText VARCHAR(300) NULL
	--,StorageAccountName VARCHAR(200) NULL
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Is this activity migrated from Splunk process?',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerBI',
    @level2type = N'COLUMN',
    @level2name = N'IsMigrated'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Raw activity json from O365',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerBI',
    @level2type = N'COLUMN',
    @level2name = N'_raw'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'If this operation been analyzed or not',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'PowerBI',
    @level2type = N'COLUMN',
    @level2name = N'IsObserved'