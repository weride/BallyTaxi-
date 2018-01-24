

CREATE TABLE [dbo].[Versions](
	[Version] [varchar](10) NOT NULL,
 CONSTRAINT [PK_Versions] PRIMARY KEY CLUSTERED 
(
	[Version] ASC
))

GO

Insert into Versions values('01.14')
GO
