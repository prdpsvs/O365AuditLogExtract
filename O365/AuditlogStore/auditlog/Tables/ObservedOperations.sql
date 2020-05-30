CREATE TABLE [auditlog].[ObservedOperations]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[OperationName] VARCHAR(100) NOT NULL,
	[Workload] VARCHAR(100) NOT NULL,

	CONSTRAINT UQ_OperationName_Workload UNIQUE (OperationName, Workload)
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Operations which are analyzed by development team',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'ObservedOperations',
    @level2type = N'COLUMN',
    @level2name = N'OperationName'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Power BI, Power Apps, Microsoft Flow are supported workloads',
    @level0type = N'SCHEMA',
    @level0name = N'auditlog',
    @level1type = N'TABLE',
    @level1name = N'ObservedOperations',
    @level2type = N'COLUMN',
    @level2name = N'Workload'