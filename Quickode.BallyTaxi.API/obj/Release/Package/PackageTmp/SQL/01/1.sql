

/****** Object:  Table [dbo].[TaxiStations]    Script Date: 31/05/2016 10:11:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TaxiStations](
	[StationId] [int] IDENTITY(1,1) NOT NULL,
	[HebrewName] [nvarchar](100) NOT NULL,
	[EnglishName] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_TaxiStations] PRIMARY KEY CLUSTERED 
(
	[StationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE TABLE [dbo].[SystemSettings](
	[ParamKey] [varchar](50) NOT NULL,
	[ParamValue] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_SystemSettings] PRIMARY KEY CLUSTERED 
(
	[ParamKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO

ALTER TABLE dbo.Drivers ADD
	TaxiStationId int NULL
GO

ALTER TABLE dbo.Drivers ADD CONSTRAINT
	FK_Drivers_TaxiStations FOREIGN KEY
	(
	TaxiStationId
	) REFERENCES dbo.TaxiStations
	(
	StationId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
