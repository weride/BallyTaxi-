CREATE TABLE [dbo].[CreditCards](
	[CardId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [bigint] NOT NULL,
	[LastFourDigits] [varchar](4) NOT NULL,
	[ExpMonth] [int] NOT NULL,
	[ExpYear] [int] NOT NULL,
	[CVV] [varchar](4) NOT NULL,
	[CardHolderName] [nvarchar](100) NOT NULL,
	[CardHolderId] [varchar](50) NULL,	
	[IsDefaultCard] [bit] NOT NULL,
	[Token] [nvarchar](200) NOT NULL	
 CONSTRAINT [PK_CreditCards] PRIMARY KEY CLUSTERED 
(
	[CardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE dbo.CreditCards ADD CONSTRAINT
	FK_CreditCards_Users FOREIGN KEY
	(
	UserId
	) REFERENCES dbo.Users
	(
	UserId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

