-- =================================================================================
-- Author:      Microsoft
-- Create date: 12/18/2019
-- Description: This Stored Procedure will insert power platform audit logs from
-- staging tables to aduitlog schema tables
-- to be deployed in Power BI Service
-- =================================================================================

CREATE PROCEDURE [stg].[uspLoadAuditLogDataFromStagingToMainTables]
(
    @Status NVARCHAR(1000) = N'' OUTPUT
)
AS
SET NOCOUNT ON;

BEGIN 
 BEGIN TRY
    BEGIN TRAN
    
    -- Power Apps
    -- If duplicate ActivityId is in staged data, take first row and delete the rest
    DELETE stgDeleteDuplicatePowerAppActivities FROM
      (
      SELECT
        ActivityId
       ,Activity
       ,rn=row_number() OVER (partition BY ActivityId ORDER BY ActivityId)
      FROM
        stg.PowerAppsAuditLog
    ) stgDeleteDuplicatePowerAppActivities
    WHERE rn > 1;
    
    DELETE stgpaa 
    FROM stg.PowerAppsAuditLog stgpaa 
    INNER JOIN auditlog.PowerApps pa
    ON pa.AuditLogId = stgpaa.ActivityId

    SET @Status = 'DELETED Duplicate Activities for Power Apps'

     -- PowerBI
    -- If duplicate ActivityId is in staged data, take first row and delete the rest
    DELETE stgDeleteDuplicatePowerBIActivities FROM
      (
      SELECT
        ActivityId
       ,Activity
       ,rn=row_number() OVER (partition BY ActivityId ORDER BY ActivityId)
      FROM
        stg.PowerBIAuditLog
    ) stgDeleteDuplicatePowerBIActivities
    WHERE rn > 1;
    
    DELETE stgpaa 
    FROM stg.PowerBIAuditLog stgpaa 
    INNER JOIN auditlog.PowerBI pa
    ON pa.AuditLogId = stgpaa.ActivityId

    SET @Status = @Status + 'DELETED Duplicate Activities for Power BI'

    -- Power Automate
    -- If duplicate ActivityId is in staged data, take first row and delete the rest
    DELETE stgDeleteDuplicatePowerAutomateActivities FROM
      (
      SELECT
        ActivityId
       ,Activity
       ,rn=row_number() OVER (partition BY ActivityId ORDER BY ActivityId)
      FROM
        stg.PowerAutomateAuditLog
    ) stgDeleteDuplicatePowerAutomateActivities
    WHERE rn > 1;

    DELETE stgpaa 
    FROM stg.PowerAutomateAuditLog stgpaa 
    INNER JOIN auditlog.PowerAutomate pa
    ON pa.AuditLogId = stgpaa.ActivityId

    SET @Status = @Status + 'DELETED Duplicate Activities for Power Automate. '

    --Load data to main tables
    
    INSERT INTO auditlog.PowerApps 
    (EnvironmentName, TargetObjectId, AppName, PermissionType,
    CreationTime, AuditLogId, ObjectId, Operation, OrganizationId, RecordType, ResultStatus,
    UserId, UserKey, UserType, Version, Workload, InsertedDate, IsObserved, _raw, IsMigrated)
    SELECT 	    
    ISNULL(JSON_VALUE(Activity,'$.AdditionalInfo.environmentName'), NULL) AS EnvironmentName  
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.AdditionalInfo.targetObjectId') AS UNIQUEIDENTIFIER), NULL) AS TargetObjectId
    ,ISNULL(JSON_VALUE(Activity,'$.AppName'), NULL) AS AppName 
	,ISNULL(JSON_VALUE(Activity,'$.AdditionalInfo.permissionType'), NULL) AS PermissionType
	,ISNULL(CAST(JSON_VALUE(Activity,'$.CreationTime') AS DATETIME), NULL) AS CreationTime
	,ISNULL(CAST(JSON_VALUE(Activity,'$.Id') AS UNIQUEIDENTIFIER), NULL) AS AuditLogId
	,ISNULL(CAST(JSON_VALUE(Activity,'$.ObjectId') AS UNIQUEIDENTIFIER), NULL) AS ObjectId
	,ISNULL(JSON_VALUE(Activity,'$.Operation'), NULL) AS Operation
	,ISNULL(CAST(JSON_VALUE(Activity,'$.OrganizationId') AS UNIQUEIDENTIFIER), NULL) AS OrganizationId
	,ISNULL(JSON_VALUE(Activity,'$.RecordType') , NULL) AS RecordType
	,ISNULL(JSON_VALUE(Activity,'$.ResultStatus'), NULL) AS ResultStatus
	,ISNULL(JSON_VALUE(Activity,'$.UserId'), NULL) AS UserId
	,ISNULL(JSON_VALUE(Activity,'$.UserKey'), NULL) AS UserKey
	,ISNULL(JSON_VALUE(Activity,'$.UserType'), NULL) AS UserType
    ,ISNULL(JSON_VALUE(Activity,'$.Version'), NULL) AS Version
	,ISNULL(JSON_VALUE(Activity,'$.Workload'), NULL) AS [Workload]
    ,GETDATE() AS InsertedDate
	,IsObserved
    ,Activity AS _raw
    ,0 AS IsMigrated
    FROM 
    (
        SELECT ActivityId, Activity, Operation, 0 AS IsObserved 
        FROM stg.PowerAppsAuditLog 
        WHERE Operation NOT IN (SELECT OperationName FROM auditlog.ObservedOperations WHERE Workload = 'PowerApps')
        
        UNION
        
        SELECT ActivityId, Activity, Operation, 1 AS IsObserved 
        FROM stg.PowerAppsAuditLog 
        WHERE Operation IN (SELECT OperationName FROM auditlog.ObservedOperations WHERE Workload = 'PowerApps')
    ) PowerAppsAuditLog;

    SET @Status = @Status + 'Inserted Audit logs for Power Apps. '


   INSERT INTO auditlog.PowerAutomate
 (ClientIP, CreationTime, FlowConnectorNames, FlowDetailsUrl, AuditLogId, ObjectId, Operation,
 OrganizationId, RecordType, ResultStatus, UserId, UserKey, UserType, UserUpn, Version, Workload, 
 InsertedDate, IsObserved, _raw, IsMigrated)
     SELECT 
	 ISNULL(JSON_VALUE(Activity,'$.ClientIP'), NULL) AS ClientIP
	,ISNULL(CAST(JSON_VALUE(Activity,'$.CreationTime') AS DATETIME), NULL) AS CreationTime
	,ISNULL(JSON_VALUE(Activity,'$.FlowConnectorNames'), NULL) AS FlowConnectorNames
	,ISNULL(JSON_VALUE(Activity,'$.FlowDetailsUrl'), NULL) AS FlowDetailsUrl
	,ISNULL(CAST(JSON_VALUE(Activity,'$.Id') AS UNIQUEIDENTIFIER), NULL) AS AuditLogId
	,ISNULL(CAST(JSON_VALUE(Activity,'$.ObjectId') AS UNIQUEIDENTIFIER), NULL) AS ObjectId
	,ISNULL(JSON_VALUE(Activity,'$.Operation'), NULL) AS Operation
	,ISNULL(CAST(JSON_VALUE(Activity,'$.OrganizationId') AS UNIQUEIDENTIFIER), NULL) AS OrganizationId
	,ISNULL(JSON_VALUE(Activity,'$.RecordType'), NULL) AS RecordType
	,ISNULL(JSON_VALUE(Activity,'$.ResultStatus'), NULL) AS ResultStatus
    ,ISNULL(JSON_VALUE(Activity,'$.UserId'), NULL) AS UserId
	,ISNULL(JSON_VALUE(Activity,'$.UserKey'), NULL) AS UserKey
	,ISNULL(JSON_VALUE(Activity,'$.UserType'), NULL) AS UserType
    ,ISNULL(JSON_VALUE(Activity,'$.UserUPN'), NULL) AS UserUPN
    ,ISNULL(JSON_VALUE(Activity,'$.Version'), NULL) AS Version
    ,ISNULL(JSON_VALUE(Activity,'$.Workload'), NULL) AS Workload
	,GETDATE() AS InsertedDate
    ,IsObserved
    ,Activity AS _raw
    ,0 AS IsMigrated
    --,ISNULL(JSON_VALUE(Activity,'$.RecipientUPN'), NULL) AS RecipientUPN
	--,ISNULL(JSON_VALUE(Activity,'$.LicenseDisplayName'), NULL) AS LicenseDisplayName
	--,ISNULL(JSON_VALUE(Activity,'$.UserTypeInitiated'), NULL) AS UserTypeInitiated
	--,ISNULL(JSON_VALUE(Activity,'$.SharingPermission'), NULL) AS SharingPermission
  
    FROM 
    (
        SELECT ActivityId, Activity, Operation, 0 AS IsObserved 
        FROM stg.PowerAutomateAuditLog 
        WHERE Operation NOT IN (SELECT OperationName FROM auditlog.ObservedOperations WHERE Workload = 'PowerAutomate')
        
        UNION
        
        SELECT ActivityId, Activity, Operation, 1 AS IsObserved 
        FROM stg.PowerAutomateAuditLog 
        WHERE Operation IN (SELECT OperationName FROM auditlog.ObservedOperations WHERE Workload = 'PowerAutomate')
    ) PowerAutomateAuditLog;

    SET @Status = @Status + 'Inserted Audit logs for Power Automate. '

    
INSERT INTO auditlog.PowerBI
(ActivityId, ClientIP, CreationTime, DatasetName, AuditLogId, IsSuccess, ItemName, ObjectId,
Operation, OrganizationId, RecordType, RequestId, UserAgent, UserId, UserKey, UserType,
Workload, DatasourceId, DatasourceName, GatewayId, GatewayName, CapacityId, CapacityName,
DashboardId, DashboardName, WorkspaceId, WorkSpaceName, DataflowId, DataflowName, DataflowType,
DataConnectivityMode, DatasetId, DistributionMethod, ReportId, ReportName, ReportType, ImportDisplayName,
ImportId, ImportSource, ImportType, ArtifactId, ConsumptionMethod, AppName, AppReportId, InsertedDate, 
IsObserved, _raw, IsMigrated)

    SELECT 
	CASE 
		WHEN JSON_VALUE(Activity,'$.ActivityId') = '00000000-0000-0000-0000-000000000000' THEN NULL
		WHEN JSON_VALUE(Activity,'$.ActivityId') IS NULL THEN NULL
		ELSE CAST(JSON_VALUE(Activity,'$.ActivityId') AS UNIQUEIDENTIFIER)
	END AS ActivityId
	,ISNULL(JSON_VALUE(Activity,'$.ClientIP'), NULL) as ClientIP
	,ISNULL(CAST(JSON_VALUE(Activity,'$.CreationTime') AS DATETIME), NULL) as CreationTime
	,ISNULL(JSON_VALUE(Activity,'$.DatasetName'), NULL) as DatasetName
	,ISNULL(CAST(JSON_VALUE(Activity,'$.Id') AS UNIQUEIDENTIFIER), NULL) as AuditLogId
	,CASE WHEN LOWER(JSON_VALUE(Activity,'$.IsSuccess')) = 'true' THEN 1
          WHEN LOWER(JSON_VALUE(Activity,'$.IsSuccess')) = 'false' THEN 0
          ELSE NULL 
     END AS IsSuccess
	,ISNULL(JSON_VALUE(Activity,'$.ItemName'), NULL) as ItemName
	,ISNULL(JSON_VALUE(Activity,'$.ObjectId'), NULL) as ObjectId    
	,ISNULL(JSON_VALUE(Activity,'$.Operation'), NULL) as Operation
	,ISNULL(CAST(JSON_VALUE(Activity,'$.OrganizationId') AS UNIQUEIDENTIFIER), NULL) as OrganizationId
	,ISNULL(JSON_VALUE(Activity,'$.RecordType'), NULL) as RecordType
	,ISNULL(CAST(JSON_VALUE(Activity,'$.RequestId') AS UNIQUEIDENTIFIER), NULL) as RequestId
	,ISNULL(JSON_VALUE(Activity,'$.UserAgent'), NULL) as UserAgent
	,ISNULL(JSON_VALUE(Activity,'$.UserId'), NULL) as UserId
	,ISNULL(JSON_VALUE(Activity,'$.UserKey'), NULL) as UserKey
	,ISNULL(JSON_VALUE(Activity,'$.UserType'), NULL) as UserType
	,ISNULL(JSON_VALUE(Activity,'$.Workload'), NULL) as Workload
	,ISNULL(CAST(JSON_VALUE(Activity,'$.DatasourceId') AS UNIQUEIDENTIFIER), NULL) as DatasourceId
	,ISNULL(JSON_VALUE(Activity,'$.DatasourceName'), NULL) as DatasourceName
	,ISNULL(CAST(JSON_VALUE(Activity,'$.GatewayId') AS UNIQUEIDENTIFIER), NULL) as GatewayId
	,ISNULL(JSON_VALUE(Activity,'$.GatewayName'), NULL) as GatewayName
	,ISNULL(CAST(JSON_VALUE(Activity,'$.CapacityId') AS UNIQUEIDENTIFIER), NULL) as CapacityId
	,ISNULL(JSON_VALUE(Activity,'$.CapacityName'), NULL) as CapacityName
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.DashboardId') AS UNIQUEIDENTIFIER), NULL) as DashboardId
    ,ISNULL(JSON_VALUE(Activity,'$.DashboardName'), NULL) as DashboardName
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.WorkspaceId') AS UNIQUEIDENTIFIER), NULL) as WorkspaceId
    ,ISNULL(JSON_VALUE(Activity,'$.WorkSpaceName'), NULL) as WorkSpaceName
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.DataflowId') AS UNIQUEIDENTIFIER), NULL) as DataflowId
    ,ISNULL(JSON_VALUE(Activity,'$.DataflowName'), NULL) as DataflowName  
    ,ISNULL(JSON_VALUE(Activity,'$.DataflowType'), NULL) as DataflowType
    ,ISNULL(JSON_VALUE(Activity,'$.DataConnectivityMode'), NULL) as DataConnectivityMode
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.DatasetId') AS UNIQUEIDENTIFIER), NULL) as DatasetId
    ,ISNULL(JSON_VALUE(Activity,'$.DistributionMethod'), NULL) as DistributionMethod
    ,ISNULL(CAST( JSON_VALUE(Activity,'$.ReportId') AS UNIQUEIDENTIFIER), NULL) as ReportId
    ,ISNULL(JSON_VALUE(Activity,'$.ReportName'), NULL) as ReportName
    ,ISNULL(JSON_VALUE(Activity,'$.ReportType'), NULL) as ReportType      
    ,ISNULL(JSON_VALUE(Activity,'$.ImportDisplayName'), NULL) as ImportDisplayName
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.ImportId') AS UNIQUEIDENTIFIER), NULL) as ImportId
    ,ISNULL(JSON_VALUE(Activity,'$.ImportSource'), NULL) as ImportSource
    ,ISNULL(JSON_VALUE(Activity,'$.ImportType'), NULL) as ImportType
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.ArtifactId') AS UNIQUEIDENTIFIER), NULL) as ArtifactId
    ,ISNULL(JSON_VALUE(Activity,'$.ConsumptionMethod'), NULL) as ConsumptionMethod
    ,ISNULL(JSON_VALUE(Activity,'$.AppName'), NULL) as AppName
    ,ISNULL(CAST(JSON_VALUE(Activity,'$.AppReportId') AS UNIQUEIDENTIFIER), NULL) as AppReportId
    ,GETDATE() AS InsertedDate
    ,IsObserved
    ,Activity AS _raw
    ,0 AS IsMigrated

	--,ISNULL(JSON_VALUE(Activity,'$.RefreshType'), NULL) as RefreshType
    --,ISNULL(JSON_VALUE(Activity,'$.TileText'), NULL) as TileText
    --,ISNULL(JSON_VALUE(Activity,'$.StorageAccountName'), NULL) as StorageAccountName
    --,ISNULL(JSON_VALUE(Activity,'$.ArtifactName'), NULL) as ArtifactName
    --,ISNULL(JSON_VALUE(Activity,'$.ShareWithCurrentFilter'), NULL) as ShareWithCurrentFilter
    --,ISNULL(JSON_VALUE(Activity,'$.OrgAppPermission'), NULL) as OrgAppPermission
	--,ISNULL(JSON_VALUE(Activity,'$.CustomVisualAccessTokenResourceId'), NULL) as CustomVisualAccessTokenResourceId
    --,ISNULL(JSON_VALUE(Activity,'$.CustomVisualAccessTokenSiteUri'), NULL) as CustomVisualAccessTokenSiteUri
    --,ISNULL(CAST(JSON_VALUE(Activity,'$.EmbedTokenId') AS UNIQUEIDENTIFIER), NULL) as EnbedTokenId
    --,ISNULL(JSON_VALUE(Activity,'$.FolderDisplayName'), NULL) as FolderDisplayName
    --,ISNULL(CAST(JSON_VALUE(Activity,'$.FolderObjectId') AS UNIQUEIDENTIFIER), NULL) as FolderObjectId
	
    FROM 
    (
        SELECT ActivityId, Activity, Operation, 0 AS IsObserved 
        FROM stg.PowerBIAuditLog 
        WHERE Operation NOT IN (SELECT OperationName FROM auditlog.ObservedOperations WHERE Workload = 'PowerBI')
        
        UNION
        
        SELECT ActivityId, Activity, Operation, 1 AS IsObserved 
        FROM stg.PowerBIAuditLog 
        WHERE Operation IN (SELECT OperationName FROM auditlog.ObservedOperations WHERE Workload = 'PowerBI')
    ) PowerBIAuditLog;
    
     SET @Status = @Status + 'Inserted Audit logs for Power BI. '
    COMMIT TRAN
END TRY
 BEGIN CATCH
        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRAN;
        END

        SET @Status = 'Error ' + CONVERT(varchar(50), ERROR_NUMBER(), NULL) +
          ', Severity ' + CONVERT(varchar(5), ERROR_SEVERITY()) +
          ', State ' + CONVERT(varchar(5), ERROR_STATE()) + 
          ', Procedure ' + ISNULL(ERROR_PROCEDURE(), '-') + 
          ', Line ' + CONVERT(varchar(5), ERROR_LINE());

        EXECUTE [error].[uspLogError];

        THROW;
 END CATCH;
END;
GO
