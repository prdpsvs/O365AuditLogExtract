/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

-- Adding Primary key and clustered indexes to tables
ALTER TABLE auditlog.PowerApps
ADD CONSTRAINT PK_PowerApps_AuditLogId PRIMARY KEY NONCLUSTERED(AuditLogId);
GO

CREATE CLUSTERED INDEX ixcPowerAppsId ON auditlog.PowerApps (Id);
GO

ALTER TABLE auditlog.PowerBI
ADD CONSTRAINT PK_PowerBI_AuditLogId PRIMARY KEY NONCLUSTERED(AuditLogId);
GO

CREATE CLUSTERED INDEX ixcPowerBIId ON auditlog.PowerBI (Id);
GO

ALTER TABLE auditlog.PowerAutomate
ADD CONSTRAINT PK_PowerAutomate_AuditLogId PRIMARY KEY NONCLUSTERED(AuditLogId);
GO

CREATE CLUSTERED INDEX ixcPowerAutomateId ON auditlog.PowerAutomate (Id);
GO

TRUNCATE TABLE auditlog.ObservedOperations
GO

--Observed Properties
DECLARE @PowerAutomateWorkloadName VARCHAR(20) = 'PowerAutomate'

-- INSERT Observed Properties for Power Automate
INSERT INTO auditlog.ObservedOperations(OperationName, Workload)
VALUES('CreateFlow',@PowerAutomateWorkloadName),
('EditFlow',@PowerAutomateWorkloadName),
('RenewTrial',@PowerAutomateWorkloadName),
('StartAPaidTrial',@PowerAutomateWorkloadName),
('DeleteFlow',@PowerAutomateWorkloadName),
('PutPermissions',@PowerAutomateWorkloadName),
('DeletePermissions', @PowerAutomateWorkloadName)

GO

DECLARE @PowerAppsWorkloadName VARCHAR(20) = 'PowerApps'
-- INSERT Observed Properties for Power Apps
INSERT INTO auditlog.ObservedOperations(OperationName, Workload)
VALUES('CreatePowerApp',@PowerAppsWorkloadName),
('DeletePowerApp',@PowerAppsWorkloadName),
('LaunchPowerApp',@PowerAppsWorkloadName),
('PowerAppPermissionEdited',@PowerAppsWorkloadName),
('PublishPowerApp',@PowerAppsWorkloadName),
('UpdatePowerApp',@PowerAppsWorkloadName),
('PromotePowerAppVersion',@PowerAppsWorkloadName)
GO

DECLARE @PowerBIWorkloadName VARCHAR(20) = 'PowerBI'
INSERT INTO auditlog.ObservedOperations(OperationName, Workload)
VALUES('AnalyzedByExternalApplication',@PowerBIWorkloadName)
,('ChangeGatewayDatasourceUsers',@PowerBIWorkloadName)
,('CreateDashboard',@PowerBIWorkloadName)
,('CreateDataflow',@PowerBIWorkloadName)
,('CreateDataset',@PowerBIWorkloadName)
,('CreateEmailSubscription',@PowerBIWorkloadName)
,('CreateFolder',@PowerBIWorkloadName)
,('CreateReport',@PowerBIWorkloadName)
,('DeleteComment',@PowerBIWorkloadName)
,('DeleteDashboard',@PowerBIWorkloadName)
,('DeleteDataflow',@PowerBIWorkloadName)
,('DeleteDataset',@PowerBIWorkloadName)
,('DeleteEmailSubscription',@PowerBIWorkloadName)
,('DeleteReport',@PowerBIWorkloadName)
,('DownloadReport',@PowerBIWorkloadName)
,('EditDataset',@PowerBIWorkloadName)
,('EditReport',@PowerBIWorkloadName)
,('ExportArtifact',@PowerBIWorkloadName)
,('ExportReport',@PowerBIWorkloadName)
,('ExportTile',@PowerBIWorkloadName)
,('GenerateCustomVisualAADAccessToken',@PowerBIWorkloadName)
,('GenerateDataflowSasToken',@PowerBIWorkloadName)
,('GenerateEmbedToken',@PowerBIWorkloadName)
,('GetGroupsAsAdmin',@PowerBIWorkloadName)
,('Import',@PowerBIWorkloadName)
,('OptInForProTrial',@PowerBIWorkloadName)
,('PostComment',@PowerBIWorkloadName)
,('RefreshDataset',@PowerBIWorkloadName)
,('RenameDashboard',@PowerBIWorkloadName)
,('SetScheduledRefresh',@PowerBIWorkloadName)
,('ShareReport',@PowerBIWorkloadName)
,('UpdateApp',@PowerBIWorkloadName)
,('UpdateEmailSubscription',@PowerBIWorkloadName)
,('UpdateFolderAccess',@PowerBIWorkloadName)
,('ViewDashboard',@PowerBIWorkloadName)
,('ViewReport',@PowerBIWorkloadName)
,('ViewTile',@PowerBIWorkloadName)
,('ViewUsageMetrics',@PowerBIWorkloadName)
,('SetDataflowStorageLocationForWorkspace',@PowerBIWorkloadName)
,('UpdateFolder',@PowerBIWorkloadName)
,('ViewDataflow',@PowerBIWorkloadName)
GO
