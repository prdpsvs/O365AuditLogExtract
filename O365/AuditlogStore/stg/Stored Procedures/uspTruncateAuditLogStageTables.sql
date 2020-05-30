CREATE PROCEDURE [stg].[uspTruncateAuditLogStageTables]
	
AS
	TRUNCATE TABLE stg.PowerAppsAuditLog
	TRUNCATE TABLE stg.PowerAutomateAuditLog
	TRUNCATE TABLE stg.PowerBIAuditLog

