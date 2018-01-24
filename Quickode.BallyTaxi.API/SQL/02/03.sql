alter table TaxiStations
add IsoCountry varchar(2)
go

alter table TaxiStations
add Region varchar(100)
go

alter table Users
add PreferedStationId int
go

alter table Users
add PayPalId nvarchar(100)
go

alter table Drivers
add PayPalId nvarchar(100)
go

ALTER TABLE Users ADD CONSTRAINT DF_ValidNotificationToken DEFAULT 1 FOR ValidNotificationToken;
GO

drop table [BusinessApprovedPhones]
go



CREATE TABLE [dbo].[BusinessApprovedPhones](
	[BusinessApprovedPhoneId] [int] IDENTITY(1,1) NOT NULL,
	[BusinessId] [int] NOT NULL,
	[Phone] [nvarchar](50) NOT NULL,
	[ApprovedDate] [datetime] NOT NULL,
	[CancelledDate] [date] NULL,
 CONSTRAINT [PK_BusinessApprovedPhones] PRIMARY KEY CLUSTERED 
(
	[BusinessApprovedPhoneId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


ALTER TABLE dbo.BusinessApprovedPhones ADD CONSTRAINT
	FK_BusinessApprovedPhones_Businesses FOREIGN KEY
	(
	BusinessId
	) REFERENCES dbo.Businesses
	(
	BusinessId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO


update Versions set Version = '02.03'
go