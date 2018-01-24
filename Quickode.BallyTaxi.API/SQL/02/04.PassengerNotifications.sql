sp_rename 'Users.NotificationToken', 'DriverNotificationToken', 'Column'
go

alter table Users 
add PassengerNotificationToken nvarchar(200) null
go

sp_rename 'Users.ValidNotificationToken', 'DriverValidNotificationToken', 'Column'
go

alter table Users
add PassengerValidNotificationToken bit null
go

update Users set PassengerValidNotificationToken = 0
go

alter table Users
alter Column PassengerValidNotificationToken bit not null
go

update Versions set Version = '02.04'
go