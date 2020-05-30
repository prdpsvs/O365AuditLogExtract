CREATE TABLE [auditlog].[FailedBlobUrls]
(
	[Id] INT NOT NULL IDENTITY(1,1),
	[Url] NVARCHAR(2000) NOT NULL,
	InsertedDate DATETIME NOT NULL DEFAULT GETDATE(),
	CONSTRAINT PK_FID PRIMARY KEY (Id),
	CONSTRAINT U_Url UNIQUE([Url]))

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'This column will contain URL''s for which activities couldn''t be retrieved',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'FailedBlobUrls',
    @level2type = N'COLUMN',
    @level2name = N'Url'