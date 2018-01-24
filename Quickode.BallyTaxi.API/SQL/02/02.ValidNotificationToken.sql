ALTER PROCEDURE mysp_deleteUser
	-- Add the parameters for the stored procedure here
	@userId int
AS
BEGIN
	-- 
	Print @userId
	delete from [Orders_Drivers] where DriverId = @userId

	delete from [Orders_Drivers] where orderid in (select orderid from orders where driverId = @userId)
 
	delete from [Orders_Drivers] where orderid in (select orderid from orders where passengerId = @userId)

	delete from Orders where passengerId = @userId

	delete from Orders where driverid = @userId

	delete from drivers where userid= @userId

	delete from Users where userid = @userId

END
GO


update Versions set Version = '02.02'
go
