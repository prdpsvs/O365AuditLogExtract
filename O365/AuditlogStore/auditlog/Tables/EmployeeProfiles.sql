﻿CREATE TABLE [auditlog].[EmployeeProfiles]
(
	[Id] INT NOT NULL IDENTITY(1,1),
	Ain VARCHAR(20) NOT NULL,
	CompanyName VARCHAR(100)  NULL,
	DepartmentName NVARCHAR(200)  NULL,
	Email NVARCHAR(100)  NULL,
	FirstName NVARCHAR(100)  NULL,
	FullName NVARCHAR(100)  NULL,
	JobFamily VARCHAR(100)  NULL,
	JobFamilyGroup NVARCHAR(300)  NULL,
	LastName NVARCHAR(100)  NULL,
	[Location] VARCHAR(100)  NULL,
	Lvl1LeaderFullName NVARCHAR(200)  NULL,
	Lvl2LeaderFullName NVARCHAR(200)  NULL,
	UserName NVARCHAR(100)  NULL,
	WorkCity VARCHAR(50)  NULL,
	WorkState VARCHAR(50)  NULL,
	WorkZip VARCHAR(50)  NULL,
	CONSTRAINT EP_ID PRIMARY KEY(Id) 
)
