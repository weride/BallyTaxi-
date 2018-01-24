ALTER TABLE dbo.Drivers ADD
	ChargeCCId int NULL,
	BankNumber int NULL,
	BankBranch int NULL,
	BankAccount nvarchar(50) NULL,
	BankHolderName nvarchar(50) NULL,
	IdentityCardNumber  nvarchar(50) NULL,
	CCProviderNumber nvarchar(50) NULL
GO


ALTER TABLE dbo.Drivers ADD CONSTRAINT
	FK_Drivers_CreditCards FOREIGN KEY
	(
	ChargeCCId
	) REFERENCES dbo.CreditCards
	(
	CardId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO



