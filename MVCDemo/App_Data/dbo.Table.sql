CREATE TABLE [dbo].[MovieDBs]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[Title] nvarchar(100) not null,
	[Director]	nvarchar(100) null,
	[Date]	datetime null
)
