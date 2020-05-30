CREATE PROCEDURE [auditlog].[uspDeleteExecutedUrls]
@ExecutedUrls [auditlog].[FailureUrlType] READONLY
AS
BEGIN 
 BEGIN TRY
 BEGIN TRAN	
	DELETE FROM auditlog.FailedBlobUrls WHERE ID IN (SELECT Id FROM @ExecutedUrls)
COMMIT TRAN;
 END TRY
 BEGIN CATCH
        -- Rollback any active or uncommittable transactions before
        -- inserting information in the ErrorLog
        IF @@TRANCOUNT > 0
        BEGIN
            ROLLBACK TRAN;
        END

        EXECUTE [error].[uspLogError];
        THROW;
 END CATCH;
END;
GO