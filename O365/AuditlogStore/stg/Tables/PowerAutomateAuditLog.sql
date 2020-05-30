CREATE TABLE [stg].[PowerAutomateAuditLog]
(
	[ActivityId] UNIQUEIDENTIFIER NOT NULL,
	Activity NVARCHAR(MAX) NOT NULL,
	Operation VARCHAR(50) NOT NULL
)
