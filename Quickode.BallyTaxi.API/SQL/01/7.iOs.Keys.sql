Update SystemSettings set ParamValue = 'riderDriverPushProd_pKey.p12' where ParamKey = 'Notifications.IOS.P12.Driver'
GO
Update SystemSettings set ParamValue = 'riderPassengerPushProd_pKey.p12' where ParamKey = 'Notifications.IOS.P12.Passenger'
GO
Update SystemSettings set ParamValue = 'riderDriverPushDev_pKey.p12' where ParamKey = 'Notifications.IOS.P12sandbox.Driver'
GO
Update SystemSettings set ParamValue = 'riderPassengerPushDev_pKey.p12' where ParamKey = 'Notifications.IOS.P12sandbox.Passenger'
GO

Update SystemSettings set ParamValue = 'true' where ParamKey = 'Notifications.IOS.Sandbox'
GO
