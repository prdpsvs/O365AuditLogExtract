﻿CREATE TABLE [stg].[PowerBIAuditLog]
(
	[ActivityId] UNIQUEIDENTIFIER NOT NULL,
	Activity NVARCHAR(MAX) NOT NULL,
	Operation VARCHAR(50) NOT NULL
)
