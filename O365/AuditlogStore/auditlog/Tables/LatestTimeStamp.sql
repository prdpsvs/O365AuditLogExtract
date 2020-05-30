CREATE TABLE [auditlog].[LatestTimeStamp]
(
	[TimeStamp] DATETIME NOT NULL
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'On successful execution, this timestamp will be used for next run',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'LatestTimeStamp',
    @level2type = N'COLUMN',
    @level2name = N'TimeStamp'