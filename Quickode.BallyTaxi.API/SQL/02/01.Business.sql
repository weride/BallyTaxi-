

CREATE TABLE [dbo].[Businesses](
	[BusinessId] [int] IDENTITY(1,1) NOT NULL,
	[IsoCountry] [varchar](2) NOT NULL,
	[BusinessName] [nvarchar](100) NOT NULL,
	[PayPalAccountId] [nvarchar](200) NULL,
 CONSTRAINT [PK_Businesses] PRIMARY KEY CLUSTERED 
(
	[BusinessId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


CREATE TABLE [dbo].[BusinessApprovedPhones](
	[BusinessApprovedPhoneId] [int] IDENTITY(1,1) NOT NULL,
	[BusinessId] [int] NOT NULL,
	[Phone] [int] NOT NULL,
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
---
alter table Users
add ValidNotificationToken bit
go

update Users
set ValidNotificationToken = 1 where NotificationToken is not null
go

update Users
set ValidNotificationToken = 0 where NotificationToken is null
go

alter table Users
alter column ValidNotificationToken bit  null
go

--

alter table Businesses
add Phone varchar(50)
go

update Versions set Version = '02.01'
go
