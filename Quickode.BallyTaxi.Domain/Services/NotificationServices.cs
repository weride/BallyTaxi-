using Newtonsoft.Json;
using PushSharp;
using PushSharp.Apple;
using Quickode.BallyTaxi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using PushSharp.Core;
using PushSharp.Google;
using Quickode.BallyTaxi.Core;
using Newtonsoft.Json.Linq;
using Quickode.BallyTaxi.Models.Filters;
using System.Threading.Tasks;

namespace Quickode.BallyTaxi.Domain.Services
{
    public class NotificationsServices : IDisposable
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ApnsServiceBroker DevAppleDriverBrokder;
        private ApnsServiceBroker DevApplePassengerBrokder;
        private ApnsServiceBroker ProdAppleDriverBrokder;
        private ApnsServiceBroker ProdApplePassengerBrokder;

        private GcmServiceBroker AndriodDriverBroker;
        private GcmServiceBroker AndriodPassengerBroker;


        //private bool isIOSSandBox = ConfigurationHelper.NOTIFICATIONS_IOS_SANDBOX;

        //singleton pattern
        private static NotificationsServices _current;
        public static NotificationsServices Current
        {
            get
            {
                if (_current == null)
                    _current = new NotificationsServices();
                return _current;
            }
        }
        public NotificationsServices()
        {
            //new Task(() => { OrderService.HandlePendingOrders(); });

            //https://github.com/Redth/PushSharp/wiki/How-to-Configure-&-Send-Apple-Push-Notifications-using-PushSharp            

            #region // Configure apple push (APNS)

            // Configuration (NOTE: .pfx can also be used here)

            //ApnsConfiguration apnsPassengerConfig;
            try
            {

                var devApnsDriverConfig = new ApnsConfiguration(
                    ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_DRIVER,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_PASS_DRIVER);
                Logger.DebugFormat("devApnsDriverConfig ApnsConfiguration.ApnsServerEnvironment.Sandbox ", ApnsConfiguration.ApnsServerEnvironment.Sandbox);
                Logger.DebugFormat("devApnsDriverConfig NOTIFICATIONS_IOS_P12SANDBOX_DRIVER", ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_DRIVER);
                Logger.DebugFormat("devApnsDriverConfig NOTIFICATIONS_IOS_P12SANDBOX_PASS_DRIVER", ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_PASS_DRIVER);



                var devApnsPassengerConfig = new ApnsConfiguration(
                    ApnsConfiguration.ApnsServerEnvironment.Sandbox,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_PASSENGER,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_PASS_PASSENGER);
                Logger.DebugFormat("devApnsPassengerConfig ApnsConfiguration.ApnsServerEnvironment.Sandbox ", ApnsConfiguration.ApnsServerEnvironment.Sandbox);
                Logger.DebugFormat("devApnsPassengerConfig NOTIFICATIONS_IOS_P12SANDBOX_PASSENGER ", ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_PASSENGER);
                Logger.DebugFormat("devApnsPassengerConfig NOTIFICATIONS_IOS_P12SANDBOX_PASS_PASSENGER ", ConfigurationHelper.NOTIFICATIONS_IOS_P12SANDBOX_PASS_PASSENGER);


                var prodApnsPassengerConfig = new ApnsConfiguration(
                    ApnsConfiguration.ApnsServerEnvironment.Production,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12_PASSENGER,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12_PASS_PASSENGER);
                Logger.DebugFormat("prodApnsPassengerConfig ApnsConfiguration.ApnsServerEnvironment.Production ", ApnsConfiguration.ApnsServerEnvironment.Production);
                Logger.DebugFormat("prodApnsPassengerConfig NOTIFICATIONS_IOS_P12_PASSENGER ", ConfigurationHelper.NOTIFICATIONS_IOS_P12_PASSENGER);
                Logger.DebugFormat("prodApnsPassengerConfig NOTIFICATIONS_IOS_P12_PASS_PASSENGER ", ConfigurationHelper.NOTIFICATIONS_IOS_P12_PASS_PASSENGER);


                var prodApnsDriverConfig = new ApnsConfiguration(
                    ApnsConfiguration.ApnsServerEnvironment.Production,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12_DRIVER,
                    ConfigurationHelper.NOTIFICATIONS_IOS_P12_PASS_DRIVER);
                Logger.DebugFormat("prodApnsDriverConfig ApnsConfiguration.ApnsServerEnvironment.Production ", ApnsConfiguration.ApnsServerEnvironment.Production);
                Logger.DebugFormat("prodApnsDriverConfig NOTIFICATIONS_IOS_P12_DRIVER", ConfigurationHelper.NOTIFICATIONS_IOS_P12_DRIVER);
                Logger.DebugFormat("prodApnsDriverConfig NOTIFICATIONS_IOS_P12_PASS_DRIVER ", ConfigurationHelper.NOTIFICATIONS_IOS_P12_PASS_DRIVER);



                // Create a new broker
                DevAppleDriverBrokder = new ApnsServiceBroker(devApnsDriverConfig);
                DevApplePassengerBrokder = new ApnsServiceBroker(devApnsPassengerConfig);
                ProdAppleDriverBrokder = new ApnsServiceBroker(prodApnsDriverConfig);
                ProdApplePassengerBrokder = new ApnsServiceBroker(prodApnsPassengerConfig);

                // Wire up events
                DevAppleDriverBrokder.OnNotificationFailed += (notification, aggregateEx) =>
                {

                    aggregateEx.Handle(ex =>
                    {

                        // See what kind of exception it was to further diagnose
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;
                            // Deal with the failed notification
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;

                            Logger.Info($"Apple Dev Driver Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                        }
                        else
                        {
                            // Inner exception might hold more useful information like an ApnsConnectionException           
                            Logger.Info($"Apple Dev Driver Notification Failed for some unknown reason : {ex.InnerException}");
                        }

                        // Mark it as handled
                        return true;
                    });
                };

                ProdAppleDriverBrokder.OnNotificationFailed += (notification, aggregateEx) =>
                {

                    aggregateEx.Handle(ex =>
                    {

                        // See what kind of exception it was to further diagnose
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;

                            // Deal with the failed notification
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;

                            Logger.Info($"Apple Prod Driver Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                        }
                        else
                        {
                            // Inner exception might hold more useful information like an ApnsConnectionException           
                            Logger.Info($"Apple Prod Driver Notification Failed for some unknown reason : {ex.InnerException}");
                        }

                        // Mark it as handled
                        return true;
                    });
                };

                DevApplePassengerBrokder.OnNotificationFailed += (notification, aggregateEx) =>
                {

                    aggregateEx.Handle(ex =>
                    {

                        // See what kind of exception it was to further diagnose
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;

                            // Deal with the failed notification
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;

                            Logger.Info($"Apple Dev Passenger Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");

                        }
                        else
                        {
                            // Inner exception might hold more useful information like an ApnsConnectionException           
                            Logger.Info($"Apple Dev Passenger Notification Failed for some unknown reason : {ex.InnerException}");
                        }

                        // Mark it as handled
                        return true;
                    });
                };

                ProdApplePassengerBrokder.OnNotificationFailed += (notification, aggregateEx) =>
                {

                    aggregateEx.Handle(ex =>
                    {

                        // See what kind of exception it was to further diagnose
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;

                            // Deal with the failed notification
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;

                            Logger.Info($"Apple Prod Passenger Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
                        }
                        else
                        {
                            // Inner exception might hold more useful information like an ApnsConnectionException           
                            Logger.Info($"Apple Prod Passenger Notification Failed for some unknown reason : {ex.InnerException}");
                        }

                        // Mark it as handled
                        return true;
                    });
                };

                DevAppleDriverBrokder.OnNotificationSucceeded += (notification) =>
                {
                    Logger.Info("Apple Dev Driver Notification Sent!");
                };

                ProdAppleDriverBrokder.OnNotificationSucceeded += (notification) =>
                {
                    Logger.Info("Apple Prod Driver Notification Sent!");
                };

                DevApplePassengerBrokder.OnNotificationSucceeded += (notification) =>
                {
                    Logger.Info("Apple Dev Psseenger Notification Sent!");
                };

                ProdApplePassengerBrokder.OnNotificationSucceeded += (notification) =>
                {
                    Logger.Info("Apple Prod Psseenger Notification Sent!");
                };

                // Start the brokers
                DevAppleDriverBrokder.Start();
                DevApplePassengerBrokder.Start();
                ProdAppleDriverBrokder.Start();
                ProdApplePassengerBrokder.Start();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            #endregion

            #region // Configure Google notifications (GCM)

            try
            {
                // Configuration

                var gcmDriverConfig = new GcmConfiguration(
                    ConfigurationHelper.NOTIFICATIONS_ANDROID_SENDERID_DRIVER,
                    ConfigurationHelper.NOTIFICATIONS_ANDROID_APIKEY_DRIVER,
                    null);

                var gcmPassengerConfig = new GcmConfiguration(
                    ConfigurationHelper.NOTIFICATIONS_ANDROID_SENDERID_PASSENGER,
                    ConfigurationHelper.NOTIFICATIONS_ANDROID_APIKEY_PASSENGER,
                    null);

                // Create a new broker
                AndriodDriverBroker = new GcmServiceBroker(gcmDriverConfig);
                AndriodPassengerBroker = new GcmServiceBroker(gcmPassengerConfig);

                // Wire up events
                AndriodDriverBroker.OnNotificationFailed += (notification, aggregateEx) =>
                {

                    aggregateEx.Handle(ex =>
                    {

                        // See what kind of exception it was to further diagnose
                        if (ex is GcmNotificationException)
                        {
                            var notificationException = (GcmNotificationException)ex;

                            // Deal with the failed notification
                            var gcmNotification = notificationException.Notification;

                            var description = notificationException.Description;

                            Logger.Error($"GCM Driver (1) Notification Failed: ID={gcmNotification.MessageId}, Desc={description}, Notificaion={gcmNotification.Notification}");
                        }
                        else if (ex is GcmMulticastResultException)
                        {
                            var multicastException = (GcmMulticastResultException)ex;

                            foreach (var succeededNotification in multicastException.Succeeded)
                            {
                                Logger.Error($"GCM Driver (2) Notification Failed: ID={succeededNotification.MessageId}");
                            }

                            foreach (var failedKvp in multicastException.Failed)
                            {
                                var n = failedKvp.Key;
                                var e = failedKvp.Value;

                                Logger.Error($"GCM Driver (3) Notification Failed: ID={n.MessageId}, Desc={e.Message}");
                            }

                        }
                        else if (ex is DeviceSubscriptionExpiredException)
                        {
                            var expiredException = (DeviceSubscriptionExpiredException)ex;

                            var oldId = expiredException.OldSubscriptionId;
                            var newId = expiredException.NewSubscriptionId;

                            Logger.Info($"Device RegistrationId Expired: {oldId}");

                            if (ex.Data != null && ex.Data.Keys != null)

                            {
                                foreach (var k in ex.Data.Keys)
                                {
                                    Logger.Info(string.Format("Expired : {0}:{1})", k, ex.Data[k]));
                                }
                            }


                            if (!string.IsNullOrWhiteSpace(newId))
                            {
                                // If this value isn't null, our subscription changed and we should update our database
                                Logger.Info($"Device RegistrationId Changed To: {newId}");

                                UserService.ChangeDriverNotification(oldId, newId);
                            }
                        }
                        else if (ex is RetryAfterException)
                        {
                            var retryException = (RetryAfterException)ex;
                            // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                            Logger.Error($"GCM Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
                        }
                        else
                        {
                            Logger.ErrorFormat("GCM Driver Notification Failed for some unknown reason {0}", ex);
                        }

                        // Mark it as handled
                        return true;
                    });
                };

                AndriodPassengerBroker.OnNotificationFailed += (notification, aggregateEx) =>
                {

                    aggregateEx.Handle(ex =>
                    {

                        // See what kind of exception it was to further diagnose
                        if (ex is GcmNotificationException)
                        {
                            var notificationException = (GcmNotificationException)ex;

                            // Deal with the failed notification
                            var gcmNotification = notificationException.Notification;
                            var description = notificationException.Description;

                            Logger.Error($"GCM (1) Passenger Notification Failed: ID={gcmNotification.MessageId}, Desc={description}");
                        }
                        else if (ex is GcmMulticastResultException)
                        {
                            var multicastException = (GcmMulticastResultException)ex;

                            foreach (var succeededNotification in multicastException.Succeeded)
                            {
                                Logger.Error($"GCM (2) Passenger Notification Failed: ID={succeededNotification.MessageId}");
                            }

                            foreach (var failedKvp in multicastException.Failed)
                            {
                                var n = failedKvp.Key;
                                var e = failedKvp.Value;

                                Logger.Error($"GCM (3) Passenger Notification Failed: ID={n.MessageId}, Desc={e.Message}");
                            }

                        }
                        else if (ex is DeviceSubscriptionExpiredException)
                        {
                            var expiredException = (DeviceSubscriptionExpiredException)ex;

                            var oldId = expiredException.OldSubscriptionId;
                            var newId = expiredException.NewSubscriptionId;

                            Logger.Info($"Device RegistrationId Expired: {oldId}");

                            if (!string.IsNullOrWhiteSpace(newId))
                            {
                                // If this value isn't null, our subscription changed and we should update our database
                                UserService.ChangePassengerNotification(oldId, newId);
                                Logger.Info($"Device RegistrationId Changed To: {newId}");
                            }
                        }
                        else if (ex is RetryAfterException)
                        {
                            var retryException = (RetryAfterException)ex;
                            // If you get rate limited, you should stop sending messages until after the RetryAfterUtc date
                            Logger.Error($"GCM Rate Limited, don't send more until after {retryException.RetryAfterUtc}");
                        }
                        else
                        {
                            Logger.ErrorFormat("GCM Passenger Notification Failed for some unknown reason {0}", ex);
                        }

                        // Mark it as handled
                        return true;
                    });
                };

                AndriodDriverBroker.OnNotificationSucceeded += (notification) =>
                {

                    Logger.Info($"GCM Driver Notification Sent! {notification.Notification}");
                };

                AndriodPassengerBroker.OnNotificationSucceeded += (notification) =>
                {
                    Logger.Info($"GCM Passenger Notification Sent! {notification.Notification}");
                };

                // Start the brokers
                AndriodDriverBroker.Start();
                AndriodPassengerBroker.Start();
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
            #endregion
        }

        //public void PushRegister(long UserId, string DeviceId, string NotificationToken)
        //{
        //    using (var db = new BallyTaxiEntities().AutoLocal())
        //    {
        //        User user = db.Users.Where(x => x.UserId == UserId && x.DeviceId == DeviceId).FirstOrDefault();
        //        if (user != null)
        //        {
        //            user.NotificationToken = NotificationToken;
        //            db.SaveChanges();
        //        }
        //        else
        //             throw new UserNotExistException();

        //    }
        //}

        //private void SendNotification(User user, string message, Dictionary<string, object> data, string sound, UserType userType)
        //{
        //    SendNotifications(new List<User> { user }, message, data, sound, userType);
        //}

        private void SendPassengerdNotifications(List<User> users, string message, Dictionary<string, object> data, string sound, bool isHidden = false)
        {
            if (users == null || users.Count == 0 || string.IsNullOrWhiteSpace(message))
                return;
            foreach (User usr in users)
            {
                string msg = string.Format("Tel: \"{0}\" will be sent message \"{1}\"", usr.Phone, message);
                if (Convert.ToInt32((data["type"])) != (int)DriverNotificationTypes.sendToGetLocation)
                    Logger.Info(msg);
            }
            //ios 
            string[] RegistrationIds = users
                .Where(d => d.PlatformId.HasValue && d.PlatformId == (int)PlatformTypes.IOS && !string.IsNullOrWhiteSpace(d.PassengerNotificationToken) && d.PassengerValidNotificationToken)
                .Select(d => d.PassengerNotificationToken)
                .Distinct()
                .ToArray();

            if (RegistrationIds != null && RegistrationIds.Length > 0)
            {
                string iosSound = sound != "default" ? sound + ".aif" : sound;
                foreach (string token in RegistrationIds)
                {
                    var payload = new JObject();

                    if (isHidden == false)
                    {
                        payload = new JObject(
                           new JProperty("aps", new JObject(
                            new JProperty("alert", message),
                             //edited by S.Shraiber to check
                             new JProperty("content-available", 1),
                            new JProperty("sound", sound)))

                           );
                    }
                    else
                    {
                        payload = new JObject(
                             new JProperty("aps", new JObject(
                              // new JProperty("alert", message),
                              new JProperty("content-available", 1),
                              new JProperty("sound", "")))
                            );
                    }


                    foreach (var item in data)
                        payload.Add(item.Key, JToken.FromObject(item.Value));

                    var appleNotification = new ApnsNotification(token, payload);
                    appleNotification.LowPriority = false;

                    var UserDebug = users.First().IsDebug.HasValue && users.First().IsDebug.Value;

                    if (UserDebug && DevApplePassengerBrokder != null)
                        // Queue a notification to send to passenger
                        DevApplePassengerBrokder.QueueNotification(appleNotification);
                    if (!UserDebug && ProdApplePassengerBrokder != null)
                        // Queue a notification to send to passenger
                        ProdApplePassengerBrokder.QueueNotification(appleNotification);
                }
            }

            //android
            RegistrationIds = users.Where(d => d.PlatformId == (int)PlatformTypes.Android && !string.IsNullOrWhiteSpace(d.PassengerNotificationToken) && d.PassengerValidNotificationToken)
                .Select(d => d.PassengerNotificationToken).Distinct().ToArray();
            if (RegistrationIds != null && RegistrationIds.Length > 0)
            {
                var andriodNotification = new GcmNotification()
                {
                    RegistrationIds = RegistrationIds.ToList(),
                    Data = new JObject(),
                    Priority = GcmNotificationPriority.High,
                    DelayWhileIdle = false
                };


                string androidSound = sound != "default" ? sound + ".mp3" : sound;
                andriodNotification.Data.Add("message", message);
                andriodNotification.Data.Add("sound", androidSound);

                foreach (var item in data)
                    if (item.Value != null)
                        andriodNotification.Data.Add(item.Key, JToken.FromObject(item.Value));

                if (AndriodPassengerBroker != null)
                    AndriodPassengerBroker.QueueNotification(andriodNotification);

            }


        }

        private void SendDriverNotifications(List<User> users, string message, Dictionary<string, object> data, string sound, bool isHidden = false)
        {
            if (users == null || users.Count == 0 || string.IsNullOrWhiteSpace(message))
                return;
            foreach (User usr in users)
            {
                string msg = string.Format("Tel: \"{0}\" will be sent message \"{1}\"", usr.Phone, message);
                Logger.Info(msg);
            }
            //ios 
            string[] RegistrationIds = users
                .Where(d => d.PlatformId.HasValue && d.PlatformId == (int)PlatformTypes.IOS && !string.IsNullOrWhiteSpace(d.DriverNotificationToken) && d.DriverValidNotificationToken)
                .Select(d => d.DriverNotificationToken)
                .Distinct()
                .ToArray();




            if (RegistrationIds != null && RegistrationIds.Length > 0)
            {
                string iosSound = sound != "default" ? sound + ".aif" : sound;
                foreach (string token in RegistrationIds)
                {
                    var payload = new JObject();
                    if (isHidden == false)
                    {
                        payload = new JObject(
                           new JProperty("aps", new JObject(
                               new JProperty("alert", message),
                               new JProperty("content-available", 1),
                               new JProperty("sound", sound)))
                           );
                    }
                    else
                    {
                        payload = new JObject(
                             new JProperty("aps", new JObject(
                              // new JProperty("alert", message),
                              new JProperty("content-available", 1),
                              new JProperty("sound", "")))
                            );
                    }
                    foreach (var item in data)
                        payload.Add(item.Key, JToken.FromObject(item.Value));

                    var appleNotification = new ApnsNotification(token, payload);
                    appleNotification.LowPriority = false;

                    /*if (userType == UserType.Driver && AppleDriverBrokder != null)
                        // Queue a notification to send to driver
                        AppleDriverBrokder.QueueNotification(appleNotification); //if user is debug use DevAppleDriverBroker

                    if (userType == UserType.Passenger && ApplePassengerBrokder != null)
                        // Queue a notification to send to passenger
                        ApplePassengerBrokder.QueueNotification(appleNotification); //if user is debug use DevAppleDriverBroker*/
                    var UserDebug = users.First().IsDebug.HasValue && users.First().IsDebug.Value;
                    Logger.DebugFormat("UserDebug", UserDebug);
                    if (UserDebug && DevAppleDriverBrokder != null)
                        // Queue a notification to send to driver
                        DevAppleDriverBrokder.QueueNotification(appleNotification);
                    if (!UserDebug && ProdAppleDriverBrokder != null)
                        // Queue a notification to send to driver
                        ProdAppleDriverBrokder.QueueNotification(appleNotification);
                }
            }

            //android
            RegistrationIds = users.Where(d => d.PlatformId == (int)PlatformTypes.Android && !string.IsNullOrWhiteSpace(d.DriverNotificationToken) && d.DriverValidNotificationToken)
                .Select(d => d.DriverNotificationToken).Distinct().ToArray();
            if (RegistrationIds != null && RegistrationIds.Length > 0)
            {
                var andriodNotification = new GcmNotification()
                {
                    RegistrationIds = RegistrationIds.ToList(),
                    Data = new JObject(),
                    Priority = GcmNotificationPriority.High,
                    DelayWhileIdle = false
                };


                string androidSound = sound != "default" ? sound + ".mp3" : sound;
                andriodNotification.Data.Add("message", message);
                andriodNotification.Data.Add("sound", androidSound);

                foreach (var item in data)
                    if (item.Value != null)
                        andriodNotification.Data.Add(item.Key, JToken.FromObject(item.Value));

                if (AndriodDriverBroker != null)
                    AndriodDriverBroker.QueueNotification(andriodNotification);
            }
        }

        public void DriversNotification(List<User> driversUser, DriverNotificationTypes notificationType, long orderId, Dictionary<string, object> extraInfo = null)
        {
            if (extraInfo == null)
                extraInfo = new Dictionary<string, object>();
            if (extraInfo.ContainsKey(Constants.NotificationKeys.NotificationType))
                extraInfo.Remove(Constants.NotificationKeys.NotificationType);
            if (!extraInfo.ContainsKey(Constants.NotificationKeys.NotificationType))
                extraInfo.Add(Constants.NotificationKeys.NotificationType, (int)notificationType);
            extraInfo[Constants.NotificationKeys.NotificationType] = (int)notificationType;
            if (extraInfo.ContainsKey(Constants.NotificationKeys.OrderId))
                extraInfo.Remove(Constants.NotificationKeys.OrderId);
            if (!extraInfo.ContainsKey(Constants.NotificationKeys.OrderId))
                extraInfo.Add(Constants.NotificationKeys.OrderId, orderId);

            var usersByLang = new Dictionary<int, List<User>>();
            foreach (var driverUser in driversUser)
            {
                if (!usersByLang.ContainsKey(driverUser.LanguageId))
                    usersByLang.Add(driverUser.LanguageId, new List<User>());

                usersByLang[driverUser.LanguageId].Add(driverUser);
            }

            foreach (var pushByLang in usersByLang)
            {
                var userCulture = GetLanguageCulture(pushByLang.Key);
                var messageText = Utils.TranslateMessage(userCulture, notificationType.ToString());
                if (notificationType == DriverNotificationTypes.UserCancelRideRequest && extraInfo.ContainsKey("isFutureRide") && ((bool)extraInfo["isFutureRide"]) == true)
                {
                    messageText = string.Format(Utils.TranslateMessage(userCulture, DriverNotificationTypes.UserCancelFutureRideRequest.ToString()), extraInfo["orderTime"], extraInfo["address"]);
                }
                if (notificationType == DriverNotificationTypes.ReminderForFutureRide)
                {
                    messageText = string.Format(Utils.TranslateMessage(userCulture, notificationType.ToString()), extraInfo["orderTime"], extraInfo["Address"]);
                }
                if (notificationType == DriverNotificationTypes.creaditCardDriverError)
                {
                    var paymentM = Utils.TranslateMessage(userCulture, ((CustomerPaymentMethod)extraInfo["paymentMethod"]).ToString());
                    messageText = string.Format(Utils.TranslateMessage(userCulture, notificationType.ToString()), paymentM);
                }
                if (notificationType == DriverNotificationTypes.PaymentSuccessfulAndCoupon)
                    messageText = string.Format(Utils.TranslateMessage(userCulture, notificationType.ToString()), extraInfo["couponAmount"]);
                if (notificationType == DriverNotificationTypes.NewRideRequest)
                    messageText = string.Format(Utils.TranslateMessage(userCulture, notificationType.ToString()), extraInfo["address"]);
                if (notificationType == DriverNotificationTypes.RideEndedSuccessfullAndTip)
                    messageText = string.Format(Utils.TranslateMessage(userCulture, notificationType.ToString()), extraInfo["tip"]);

                if (notificationType == DriverNotificationTypes.openAdvertising)
                    messageText = "advertising";

                Current.SendDriverNotifications(pushByLang.Value, messageText, extraInfo, "default", notificationType == DriverNotificationTypes.openAdvertising ? true : false);
            }
        }


        public void DriverNotification(User driverUser, DriverNotificationTypes notificationType, long orderId, Dictionary<string, object> extraInfo = null)
        {
            List<User> users = new List<User>();
            users.Add(driverUser);

            DriversNotification(users, notificationType, orderId, extraInfo);
        }

        public void PassengerNotification(User passengerUser, PassengerNotificationTypes notificationType, long orderId, string sound = "default", Dictionary<string, object> extraInfo = null)
        {
            var users = new List<User>();
            users.Add(passengerUser);

            if (extraInfo == null)
                extraInfo = new Dictionary<string, object>();

            if (!extraInfo.ContainsKey(Constants.NotificationKeys.OrderId))
                extraInfo.Add(Constants.NotificationKeys.OrderId, orderId);

            if (!extraInfo.ContainsKey(Constants.NotificationKeys.NotificationType))
                extraInfo.Add(Constants.NotificationKeys.NotificationType, (int)notificationType);
            extraInfo[Constants.NotificationKeys.NotificationType] = (int)notificationType;

            var userCulture = GetLanguageCulture(passengerUser.LanguageId);
            Logger.DebugFormat("language for passenger notfication: {0} for type: {1}", userCulture, notificationType.ToString());
            var messageText = Utils.TranslateMessage(userCulture, notificationType.ToString());
            if (notificationType == PassengerNotificationTypes.ReminderForFutureRide)
            {
                messageText = string.Format(Utils.TranslateMessage(userCulture, notificationType.ToString()), extraInfo["orderTime"], extraInfo["Address"]);
            }
            if (notificationType == PassengerNotificationTypes.DriverCancelledRide && extraInfo != null && (bool)extraInfo["isFutureRide"] == true)
            {
                messageText = string.Format(Utils.TranslateMessage(userCulture, PassengerNotificationTypes.DriverCancelledFutureRide.ToString()), extraInfo["orderTime"], extraInfo["address"]);
            }
            if (notificationType == PassengerNotificationTypes.RidePrice && extraInfo != null)
            {
                messageText = string.Format(Utils.TranslateMessage(userCulture, PassengerNotificationTypes.RidePrice.ToString()), extraInfo["amount"]);
            }

            if (notificationType == PassengerNotificationTypes.openAdvertising)
                messageText = "advertising";

            SendPassengerdNotifications(new List<User> { passengerUser }, messageText, extraInfo, sound, notificationType == PassengerNotificationTypes.openAdvertising ? true : false);
        }

        public string GetLanguageCulture(int langugaueId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var lang = db.Languages.GetById(langugaueId);

                if (lang == null)
                    lang = db.Languages.GetById((int)UserLanguages.en);

                if (lang == null)
                    throw new NullReferenceException("Missing languagues object in DB");

                return lang.LanguageCulture;
            }
        }

        public void Dispose()
        {

            // Stop the broker, wait for it to finish   
            // This isn't done after every message, but after you're
            // done with the broker
            DevAppleDriverBrokder.Stop();
            DevApplePassengerBrokder.Stop();
            ProdAppleDriverBrokder.Stop();
            ProdApplePassengerBrokder.Stop();

            DevAppleDriverBrokder = null;
            DevApplePassengerBrokder = null;
            ProdAppleDriverBrokder = null;
            ProdApplePassengerBrokder = null;

            AndriodDriverBroker.Stop();
            AndriodPassengerBroker.Stop();

            AndriodDriverBroker = null;
            AndriodPassengerBroker = null;
        }
    }

}
