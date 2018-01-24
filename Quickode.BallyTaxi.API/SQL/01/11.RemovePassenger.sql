
ALTER TABLE dbo.Orders
	DROP CONSTRAINT FK_Orders_Passenger
GO
ALTER TABLE dbo.Orders ADD CONSTRAINT
	FK_Orders_Passenger FOREIGN KEY
	(
	PassengerId
	) REFERENCES dbo.Users
	(
	UserId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

ALTER TABLE dbo.FavoriteAddresses
	DROP CONSTRAINT FK_FavoriteAddresses_User
GO

ALTER TABLE dbo.FavoriteAddresses ADD CONSTRAINT
	FK_FavoriteAddresses_Passenger FOREIGN KEY
	(
	PassengerId
	) REFERENCES dbo.Users
	(
	UserId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO

ALTER TABLE dbo.FavoriteDrivers
	DROP CONSTRAINT FK_FavoriteDrivers_Passenger
GO
ALTER TABLE dbo.FavoriteDrivers ADD CONSTRAINT
	FK_FavoriteDrivers_Passenger FOREIGN KEY
	(
	PassengerId
	) REFERENCES dbo.Users
	(
	UserId
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
