CREATE TABLE [error].[ErrorLog]
(
	[ErrorLogID] [int] IDENTITY(1,1) NOT NULL,
    [ErrorDateTime] DATETIME2(0) NOT NULL, 
    [UserName] [sys].[sysname] NOT NULL, 
    [ErrorNumber] INT NOT NULL, 
    [ErrorSeverity] INT NULL, 
	[ErrorState] INT NULL,
    [ErrorProcedure] NVARCHAR(126) NULL, 
    [ErrorLine] INT NULL, 
    [ErrorMessage] NVARCHAR(MAX) NOT NULL,
	[UpdateDate] DATETIME2(0) NOT NULL CONSTRAINT DF_ErrorLog_UpdateDate DEFAULT SYSDATETIME(),
	CONSTRAINT [PK_ErrorLog_ErrorLogID] PRIMARY KEY CLUSTERED 
(
	[ErrorLogID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY], 
   
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [error].[ErrorLog] ADD  CONSTRAINT [DF_ErrorLog_ErrorDateTime]  DEFAULT (sysdatetime()) FOR [ErrorDateTime]
GO
