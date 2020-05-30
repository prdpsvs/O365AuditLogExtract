
-- =================================================================================
-- Author:      Microsoft
-- Create date: 12/18/2019
-- Description: This Stored Procedure will update latest timestamp for a given ID
-- to be deployed in Power BI Service
-- =================================================================================
CREATE PROCEDURE [auditlog].[uspUpdateLatestTimestamp]
(
	@Timestamp VARCHAR(50)
    , @RowsUpdated int = 0 OUTPUT
)
AS
SET NOCOUNT ON;

BEGIN 
 BEGIN TRY
 BEGIN TRAN

    SET @Timestamp = CONVERT (DATETIME, @Timestamp, 126)
    
    IF EXISTS(SELECT * FROM auditlog.LatestTimeStamp)
    BEGIN
        UPDATE auditlog.LatestTimeStamp
	    SET [TimeStamp]=@Timestamp

        SET @RowsUpdated = @@ROWCOUNT
    END
    ELSE
       BEGIN
        INSERT INTO auditlog.LatestTimeStamp(TimeStamp) VALUES (@Timestamp)
        SET @RowsUpdated = @@ROWCOUNT
       END
    
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