CREATE PROCEDURE [auditlog].[uspGetFailedUrls]
AS
SET NOCOUNT ON;

BEGIN 
	 SELECT Id, [Url] FROM [auditlog].[FailedBlobUrls]
END;
