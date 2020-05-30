CREATE PROCEDURE [auditlog].[uspGetLatestTimestamp]

-- ============================================================================
-- Author:      Microsoft
-- Create date: 12/18/2019
-- Description: This Stored Procedure will return BusinessGroupName for given a specific ID
-- to be deployed in Power BI Service
-- ============================================================================

(
	-- 120 minutes
	@IngestionFrequencyInMinutes INT = 120,
	@LatestTimestamp varchar(100) OUTPUT,
	@EndTimestamp varchar(100) OUTPUT
)
AS
SET NOCOUNT ON;

BEGIN 
	
	IF @IngestionFrequencyInMinutes IS  NULL
		THROW 70001, '@IngestionFrequencyInMinutes parameter is null.', 1;
	
	SELECT @LatestTimestamp = ISNULL(CONVERT(varchar, [TimeStamp], 126), ''), 
	@EndTimestamp = ISNULL(CONVERT(varchar, DATEADD(mi, @IngestionFrequencyInMinutes, [TimeStamp]) , 126), '')
	FROM auditlog.LatestTimeStamp     
	
END