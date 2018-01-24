ALTER TABLE dbo.Orders ADD
	EndTime datetime NULL,
	PaymentMethod int NULL,
	Amount float NULL,
	Currency nvarchar(250) NULL
GO

ALTER TABLE dbo.Users ADD
	AlwaysApproveSum bit NULL
GO

ALTER TABLE dbo.Drivers ADD
	AcceptsCC bit NULL
GO