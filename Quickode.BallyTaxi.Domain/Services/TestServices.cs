using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quickode.BallyTaxi.Core;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Models.Filters;
using Quickode.BallyTaxi.Integrations.Twilio;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class TestServices
    {
        readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool DeleteDriver(long userId)
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    logger.WarnFormat("DeleteDriver({0})", userId);

                    var driver = db.Drivers.GetById(userId);
                    if (driver == null)
                        return false;

                    var ordersDrivers = db.Orders_Drivers.ByDriver(userId).ToList();
                    foreach (var od in ordersDrivers)
                        db.Orders_Drivers.Remove(od);

                    var favoriteDrivers = db.FavoriteDrivers.ByDriver(userId).ToList();
                    foreach (var fd in favoriteDrivers)
                        db.FavoriteDrivers.Remove(fd);

                    foreach (var order in db.Orders.ByDriver(userId).ToList())
                        order.DriverId = null;

                    db.Drivers.Remove(driver);

                    var user = db.Users.GetById(userId);
                    db.Users.Remove(user);

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        public static bool sendSMSToAprovedDrivers()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                // var userPhone = db.Users.Where(u => u.Driver != null).Select(u => u.Phone).ToList();
                // var phoneApproved = db.PhoneDriversApproveds.Where(u => !userPhone.Contains(u.phone)).ToList();

                //   foreach (var row in phoneApproved)
                // {
                var phone = "+972545205096"; // "+972584590555";//"+972548400967";
                var userCulture = "";

                var userCultureObj = db.Languages.Where(l => l.LanguageId == 1).FirstOrDefault();
                if (userCultureObj == null)
                    userCulture = "he-IL";
                else
                    userCulture = userCultureObj.LanguageCulture;
                //var text = Utils.TranslateMessage(userCulture, "SMSToPhones");
                var text = "הורידו את אפליקציית ווי-ריידר החדשה לנהג! נסיעות כבר התחילו!";

                // var text1 = " הורידו את אפליקציית ריידר לנהג, והתחילו לעבוד 100 הנהגים הראשונים שיקחו נסיעה בריידר החדשה יזכו ב 1000 ש''ח! מחר זה מתחיל! \n נהג מונית קח 1000 ש''ח!\n";
                //var text1 = "";
                // text1 += "אנדרואיד" + Environment.NewLine;
                // text1 += "https://play.google.com/store/apps/details?id=com.quickode.ballytaxi.ballytaxidriver" + Environment.NewLine;
                //var text2 = "אייפון" + Environment.NewLine;
                // text2 += "https://itunes.apple.com/us/app/rider-d/id1131603056?ls=1&mt=8";

                //text1 = string.Format(text1);

                TwilioService twilioService = new TwilioService();
                bool status = twilioService.SendSMS(phone, text, false);
                //status= twilioService.SendSMS(phone, text1, false);
                //status=twilioService.SendSMS(phone, text2, false);
                if (status)
                {
                    //return true;
                }
                else
                    logger.ErrorFormat("error when sending sms to {0}", phone);
                //throw new SMSProcessException();
                // }
            }
            return false;
        }

        public static bool sendSMSPassenger()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var passengers = db.rider_users_.ToList();

                var text = "הורד את אפליקציית ווי-ריידר החדשה לנוסע וקבל קופון לנסיעות ע\"ס 50 ש\"ח http://www.we-rider.com/";

                // var phone = "+972585608148";
                // TwilioService twilioService = new TwilioService();
                //bool status = twilioService.SendSMS(phone, text, false);//phone

                TwilioService twilioService = new TwilioService();
                bool status;
                foreach (var passenger in passengers)
                {
                    status = twilioService.SendSMS(passenger.phone, text, false);//phone
                }
            }
            return true;

        }

        public static bool sendAdvertising(int userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.GetById(userId);
                var dicInfo = new Dictionary<string, object>();
                dicInfo["url"] = ConfigurationHelper.LINK_FOR_ADVERTISING;

                if (user.Driver != null)
                    NotificationsServices.Current.DriverNotification(user, DriverNotificationTypes.openAdvertising, 0, dicInfo);
                else
                    NotificationsServices.Current.PassengerNotification(user, PassengerNotificationTypes.openAdvertising, 0, extraInfo: dicInfo);
                return true;
            }
        }

        public static bool sendSMSWithType(int smsType, int minutes)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var usersToSend = new List<User>();
                if (smsType == (int)SMSTypeToSend.AllDrivers)
                {
                    //DateTime dateMinutes = DateTime.UtcNow.AddMinutes(-minutes);
                    var usersToSendsms = db.Users.Where(u => u.Driver != null && u.Name != null /*&& (minutes > 0 ? u.Driver.LastUpdateLocation > dateMinutes : u.UserId == u.UserId)*/).ToList();
                    // var text = " נהג יקר, סיים את הרישום בכדי שתוכל לקבל נסיעות: http://we-rider.com/linkToDownload.html";
                    var text = "עוברים לווי-ריידר, נהג יקר אלפי נסיעות מחכות לך בריידר הורד והתחל להרויח: http://we-rider.com/linkToDownload.html";
                    // var text = "הורידו את אפליקציית ריידר החדשה לנהג! נסיעות כבר התחילו!";
                    foreach (var user in usersToSendsms)
                    {
                        TwilioService twilioService = new TwilioService();
                        bool status = twilioService.SendSMS(user.Phone, text, false);
                        //status= twilioService.SendSMS(phone, text1, false);
                        //status=twilioService.SendSMS(phone, text2, false);
                        if (status)
                        {
                            //return true;
                        }
                        //else
                        //    logger.ErrorFormat("error when sending sms to {0}", user.phone);
                    }
                }
                if (smsType == (int)SMSTypeToSend.AllPassengers)
                {
                    usersToSend = db.Users.Where(u => u.Driver == null).ToList();
                }
                if (smsType == (int)SMSTypeToSend.DriversNotEndRegistration)
                {
                    var userPhone = db.Users.Where(u => u.Driver != null && u.isReadTermsOfUse != true).Select(u => u.Phone).ToList();
                    var usersToSendApproved = db.PhoneDriversApproveds.Where(u => !userPhone.Contains(u.phone)).ToList();
                    var userCulture = "";
                    //var userCultureObj = db.Languages.Where(l => l.LanguageId == item.LanguageId).FirstOrDefault();
                    //if (userCultureObj == null)
                    userCulture = "he-IL";
                    //var phone = "+972585608148";
                    // else
                    //    userCulture = userCultureObj.LanguageCulture;
                    //var text = Utils.TranslateMessage(userCulture, ((SMSTypeToSend)1).ToString());

                    var text = "הורד את אפליקציית ווי-ריידר לנהג, אלפי נסיעות מחכות לך בווי-ריידר http://we-rider.com/linkToDownload.html";
                    // var text = "ריידר במבצע חדש! כדי להמשיך להרויח עם ריידר נא לעדכן את האפליקציה";

                    // var text = "הורידו את אפליקציית ריידר החדשה לנהג! נסיעות כבר התחילו!";
                    foreach (var user in usersToSendApproved)
                    {
                        TwilioService twilioService = new TwilioService();
                        bool status = twilioService.SendSMS(user.phone, text, false);
                        //status= twilioService.SendSMS(phone, text1, false);
                        //status=twilioService.SendSMS(phone, text2, false);
                        if (status)
                        {
                            //return true;
                        }
                        //else
                        //    logger.ErrorFormat("error when sending sms to {0}", user.phone);
                    }

                }
                else
                {
                    if (usersToSend.Count > 0)
                    {
                        foreach (var item in usersToSend)
                        {
                            // var phone = "+972584590555";
                            var userCulture = "";
                            var userCultureObj = db.Languages.Where(l => l.LanguageId == item.LanguageId).FirstOrDefault();
                            if (userCultureObj == null)
                                userCulture = "he-IL";
                            else
                                userCulture = userCultureObj.LanguageCulture;
                            var text = Utils.TranslateMessage(userCulture, ((SMSTypeToSend)smsType).ToString());

                            TwilioService twilioService = new TwilioService();
                            bool status = twilioService.SendSMS(item.Phone, text, false);//phone
                            //status= twilioService.SendSMS(phone, text1, false);
                            //status=twilioService.SendSMS(phone, text2, false);
                            if (status)
                            {
                                //return true;
                            }
                            else
                                logger.ErrorFormat("error when sending sms to {0}", item.Phone);
                        }

                    }
                }
                if (smsType == (int)SMSTypeToSend.ToAllDriverToUpdateTheApp)
                {

                    //string[] str = new string[2];
                    //str[0] = "https://tinyurl.com/mqvfmj8";
                    //str[1] = "https://tinyurl.com/mf3j6fv";
                    //var phone = "+972545205096";
                    //var text = "נהג ריידר יקר! נא לעדכן גירסה, קישור לחנויות ";
                    //TwilioService twilioService = new TwilioService();
                    //bool status = twilioService.SendMassage(phone, text, str ,false);//phone

                    //var usersToSendSms = new List<User>();
                    //usersToSendSms = db.Users.Where(d => d.Driver != null && d.PlatformId == (int)PlatformTypes.Android).ToList();
                    //var text = "נהג ריידר יקר! נא לעדכן גירסה, קישור לחנות https://tinyurl.com/mqvfmj8";
                    //foreach (var user in usersToSendSms)
                    //{
                    //    TwilioService twilioService = new TwilioService();
                    //    bool status = twilioService.SendSMS(user.Phone, text, false);//phone
                    //}


                    //var users = db.Users.Where(d => d.Driver != null && d.Name == null).ToList();

                    //var textAndroid = "נהג ריידר יקר! נא לעדכן גירסה, קישור לחנות https://tinyurl.com/mqvfmj8";
                    //var textIphone = "נהג ריידר יקר! נא לעדכן גירסה, קישור לחנות https://tinyurl.com/mf3j6fv";
                    //foreach (var user in users)
                    //{
                    //    TwilioService twilioService = new TwilioService();
                    //    bool status = twilioService.SendSMS(user.Phone, user.PlatformId == (int)PlatformTypes.Android ? textAndroid : textIphone, false);//phone
                    //}

                    //var users = db.Users.Where(d => d.Driver != null && d.Name != null && (d.Driver.payment == null || d.Driver.payment == 0));
                    //var text = "נהג ריידר יקר! נא לעדכן גירסה ולמלא פרטי חשבון בנק / אשראי / פיפאל";
                    //foreach (var user in users)
                    //{
                    //    TwilioService twilioService = new TwilioService();
                    //    bool status = twilioService.SendSMS(user.Phone, text, false);//phone
                    //}

                    var AllDriversToSend = db.Users.Where(u => u.Driver != null && u.isReadTermsOfUse != true /*&& (minutes > 0 ? u.Driver.LastUpdateLocation > dateMinutes : u.UserId == u.UserId)*/).ToList();
                    // var textend = "ריידר יוצאת בקמפיין ענק לנוסעים, מומלץ להשאיר את האפליקציה פתוחה";
                    //var textNotEnd = "הנכם מתבקשים לסיים את הרישום ולהתחיל לקבל נסיעות";
                    //var text = "נהג יקר, עכשיו כולם עם רידר, משאירים את האפליקציה פתוחה ומקבלים הזמנות";
                    // var text = "עוברים לריידר, נהג יקר אלפי נסיעות מחכות לך בריידר הורד והתחל להרויח: http://we-rider.com/linkToDownload.html";
                    // var text = "נהג יקר, צרף נהג לריידר, וקבל 50 ש\"ח מיידית לחשבונך";
                    //text += " שלח את פרטי הנהג שצרפת לוואצאפ 0585608148 או למייל office@rider-app.com";
                    //bool status = false;
                    ////var phone = "+972585608148";
                    //var phone = "+972585608148";
                    //TwilioService twilioService = new TwilioService();
                    //status = twilioService.SendSMS(phone, text, false);//phone

                    //var text = "הורד/עדכן את אפליקציית ריידר לנהג, אלפי נסיעות מחכות לך בריידר http://we-rider.com/linkToDownload.html";

                    var text = "ריידר במבצע חדש! כדי להמשיך להרויח עם ווי-ריידר נא לעדכן את האפליקציה";

                    //var phone = "+972585608148";
                    //TwilioService twilioService = new TwilioService();
                    //var status = twilioService.SendSMS(phone, text, false);//phone

                    foreach (var user in AllDriversToSend)
                    {
                        TwilioService twilioService = new TwilioService();
                        var status = twilioService.SendSMS(user.Phone, text, false);//phone
                        // if (user.Name == null)
                        //    status = twilioService.SendSMS(user.Phone, textNotEnd, false);//phone
                        //status = twilioService.SendSMS(user.Phone, textend, false);//phone
                    }

                    //var phone = "+972584590555";
                    //var textend = "ריידר יוצאת בקמפיין ענק לנוסעים, מומלץ להשאיר את האפליקציה פתוחה";
                    //var textNotEnd = "הנכם מתבקשים לסיים את הרישום ולהתחיל לקבל נסיעות";
                    //bool status = false;

                    //    TwilioService twilioService = new TwilioService();
                    //    if (user.Name == null)
                    //        status = twilioService.SendSMS(phone, textNotEnd, false);//phone
                    //    status = twilioService.SendSMS(phone, textend, false);//phone
                }
            }
            return true;
        }

        public static object heatmapOredr(int? numberofRecords, double? lastDateFrom, double? lastDateTo, double? lastTimeFrom, double? lastTimeTo)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                DateTime dateLastFrom = lastDateFrom.HasValue ? lastDateFrom.Value.ConvertFromUnixTimestampForMilliSeconds().Date : new DateTime(2017, 1, 1);
                DateTime dateLastTo = lastDateTo.HasValue ? lastDateTo.Value.ConvertFromUnixTimestampForMilliSeconds().Date : DateTime.UtcNow;
                if (lastTimeFrom.HasValue)
                    dateLastFrom = dateLastFrom.AddMilliseconds(lastTimeFrom.Value);
                if (lastTimeTo.HasValue)
                    dateLastTo = dateLastTo.AddMilliseconds(lastTimeTo.Value);
                var list = db.Orders.Where(o => o.CreationDate >= dateLastFrom && o.CreationDate <= dateLastTo).Take(numberofRecords.HasValue ? numberofRecords.Value : 100000).Select(o => new { lat = o.PickUpLocation.Latitude, lon = o.PickUpLocation.Longitude, status = o.StatusId }).ToList();
                return list;
            }
        }

        public static object heatmap(int? numberofRecords, double? lastDateFrom, double? lastDateTo, double? lastTimeFrom, double? lastTimeTo, string phone)
        {
            try
            {
                using (var db = new Entities())
                {
                    if (phone != null && phone != "")
                    {
                        phone = phone.Replace(" ", "");
                        phone = "+" + phone;
                    }
                    DateTime dateLastFrom = lastDateFrom.HasValue ? lastDateFrom.Value.ConvertFromUnixTimestampForMilliSeconds().Date : new DateTime(2017, 1, 1);
                    DateTime dateLastTo = lastDateTo.HasValue ? lastDateTo.Value.ConvertFromUnixTimestampForMilliSeconds().Date : DateTime.UtcNow;
                    if (lastTimeFrom.HasValue)
                        dateLastFrom = dateLastFrom.AddMilliseconds(lastTimeFrom.Value);
                    if (lastTimeTo.HasValue)
                        dateLastTo = dateLastTo.AddMilliseconds(lastTimeTo.Value);
                    var list = db.DataLocationDrivers.Where(o => o.LastUpdateLocation >= dateLastFrom && o.LastUpdateLocation <= dateLastTo && (phone != null ? o.Phone == phone : 1 == 1)).Take(numberofRecords.HasValue ? numberofRecords.Value : 100000).Select(o => new { lat = o.Location.Latitude, lon = o.Location.Longitude }).ToList();
                    return list;
                    //var result = new List<object>();
                    //foreach (var item in list)
                    //{
                    //    result.Add(new { lat = item.Latitude, lon = item.Longitude });
                    //}

                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool sendCouponToNewPassenger(string phone)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var riderPhonePassenger = db.rider_users_.Where(u => u.phone == phone).FirstOrDefault();
                if (riderPhonePassenger == null)
                    throw new UserNotExistInMonitexTableException();

                var passenger = db.Users.Where(u => u.Driver == null && u.Phone == phone).FirstOrDefault();
                if (passenger == null)
                    throw new UserNotExistException();

                Coupon numberInDB = null;
                string sixDigitNumber = "";
                do
                {
                    Random r = new Random();
                    int randNum = r.Next(1000000);
                    sixDigitNumber = randNum.ToString("D6");
                    numberInDB = db.Coupons.Where(c => c.number == sixDigitNumber).FirstOrDefault();
                }
                while (numberInDB != null);

                if (passenger.Coupons1.Where(c => c.dtStart <= DateTime.UtcNow && c.dtEnd >= DateTime.UtcNow).FirstOrDefault() != null)
                    throw new CantSentMultipleCoupon();


                var coupon = new Coupon()
                {
                    number = sixDigitNumber,
                    amount = 50,
                    currency = "IL",
                    dtStart = DateTime.UtcNow,
                    dtEnd = DateTime.UtcNow.AddMonths(1),
                    passengerIdSMS = passenger.UserId
                };
                db.Coupons.Add(coupon);
                db.SaveChanges();

                //var textEn = "You have received a coupon valued at " + amount + " NIS! Enter the code " + number + " before the next ride to redeem it. *The coupon is valid for one month only. Rider App.";
                //var textHe = "קיבלת קופון בסך " + amount + " ש''ח. הזן את הקוד " + number + "  לפני הנסיעה הבאה שלך בכדי לממש את ההטבה. *הקופון תקף לחודש ימים. צוות ריידר. ";
                var data = new Dictionary<string, object>();
                data.Add("amount", 50);
                data.Add("number", sixDigitNumber);
                bool status = UserService.SendSMSNotif(passenger.UserId, passenger.Phone, SMSType.CouponTextForSMS, passenger.LanguageId, data);

                var text = status ? "SMS was sent to phone: {0} about coupon id: {1} " : "SMS failed sent to phone: {0} about coupon id: {1} ";
                logger.DebugFormat(text, passenger.Phone, coupon.couponId);

                return true;
            }
        }

        public static bool DeletePassenger(long userId)
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    logger.WarnFormat("DeletePassenger({0})", userId);

                    var passenger = db.Users.GetById(userId);
                    if (passenger == null)
                        return false;

                    var orders = db.Orders.ByPassenger(userId).ToList();
                    foreach (var o in orders)
                    {
                        var orderDrivers = db.Orders_Drivers.ByOrder(o.OrderId).ToList();
                        foreach (var od in orderDrivers)
                            db.Orders_Drivers.Remove(od);

                        db.Orders.Remove(o);
                    }

                    var favoriteDrivers = db.FavoriteDrivers.ByPassenger(userId).ToList();
                    foreach (var fd in favoriteDrivers)
                        db.FavoriteDrivers.Remove(fd);

                    db.Users.Remove(passenger);

                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e);
                throw;
            }
        }

        public static string CleanPendingOrders()
        {
            logger.Info("CleanPendingOrders");
            var sb = new StringBuilder();

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var pendingOrders = db.Orders.Pending().ToList();

                foreach (var po in pendingOrders)
                {
                    sb.AppendLine(string.Format("Clean order (id:{0})", po.OrderId));
                    po.StatusId = (int)OrderStatus.Canceled;

                    if (po.DriverId.HasValue)
                    {
                        var driver = db.Drivers.GetById(po.DriverId.Value);
                        if (driver != null && driver.Status.HasValue &&
                            driver.Status.Value.IsOneOf((int)DriverStatus.HasRequest, (int)DriverStatus.HasRequestAsFirst, (int)DriverStatus.PendingAcceptRequest))
                        {
                            driver.Status = (int)DriverStatus.Available;
                            sb.AppendLine(string.Format("Clean driver (id:{0}) with phone: {1}", driver.UserId, driver.User.Phone));
                        }
                    }

                    var orderDrivers = db.Orders_Drivers.ByOrder(po.OrderId);
                    foreach (var od in orderDrivers)
                    {
                        var driver = db.Drivers.GetById(od.DriverId);
                        if (driver != null && driver.Status.HasValue &&
                            driver.Status.Value.IsOneOf((int)DriverStatus.HasRequest, (int)DriverStatus.HasRequestAsFirst, (int)DriverStatus.PendingAcceptRequest))
                        {
                            driver.Status = (int)DriverStatus.Available;
                            sb.AppendLine(string.Format("Clean driver (id:{0}) with phone: {1}", driver.UserId, driver.User.Phone));
                        }
                    }
                }

                var workingDrivers = db.Drivers.NotAvailableToDrive().ThatAreActive().ToList();
                foreach (var driver in workingDrivers)
                {
                    sb.AppendLine(string.Format("Clean driver (id:{0}) with phone: {1} that found in status:{2}", driver.UserId, driver.User.Phone, driver.Status));
                    driver.Status = (int)DriverStatus.Available;
                }

                db.SaveChanges();

                logger.Info(sb.ToString());
                return sb.ToString();
            }
        }

        public static Orders_Drivers DriverInOrder(long userId, long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var orderDriver = db.Orders_Drivers
                    .ByDriver(userId)
                    .ByOrder(orderId)
                    .SingleOrDefault();


                return orderDriver;
            }
        }

        public static void TestPush(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                User passenger = db.Users.GetById(userId);
                if (passenger == null)
                    throw new UserNotExistException();
                var Pdata = new Dictionary<string, object>();
                Pdata.Add("userId", userId);
                //TestPassengerNotification(passenger, PassengerNotificationTypes., Pdata);
            }
        }

        private static void TestPassengerNotification(User passengerUser, PassengerNotificationTypes notificationType, Dictionary<string, object> extraInfo = null)
        {
            var users = new List<User>();
            users.Add(passengerUser);

            if (extraInfo == null)
                extraInfo = new Dictionary<string, object>();

            if (!extraInfo.ContainsKey("type"))
                extraInfo.Add("type", (int)notificationType);

            //NotificationsServices.Current.SendNotification(passengerUser, "testing", extraInfo, "default", UserType.Passenger);
        }

        public static void TestPush2()
        {
            var driverUser = new User()
            {
                DriverNotificationToken = "APA91bH6q5Gc6G7aYvACJMZCD2NTP5wg0ThXNJp0MWzOdjlugpxvwbjkzQsEUo4UUZ5Fu6XmjdYn6NqTztw62BG3dq7Q8RBsNQQMkk4ti-d68mIBUe1ZY2qBtINW_uQEtl_VsVx35G__eXg1NWOic9dyG8w7VyNEfw",
                DeviceId = "7c458245d1456ba2",
                Phone = "+972548343366",
                Active = true,
                PlatformId = (int)PlatformTypes.Android,
                VersionOS = "23",
                AppVersion = "1.0(8)"
            };

            //Notify driver on drive, as first driver
            var location = new LocationObject()
            {
                lat = 31.91601731,
                lon = 34.8047042,
                address = "Einstein St 13 Ness Ziona"
            };
            var data = new Dictionary<string, object>();
            data.Add(Constants.NotificationKeys.DriverPriorty, 0);
            data.Add("location", location);
            data.Add("TimeFrame", ConfigurationHelper.MaxSecondsFirstDriverOfferedOrder);
            data.Add(Constants.NotificationKeys.NotificationType, (int)DriverNotificationTypes.NewRideRequest);
            data.Add(Constants.NotificationKeys.OrderId, 529);

            var messageText = "New ride for you testing-529";
            var users = new List<User>();
            users.Add(driverUser);

            //NotificationsServices.Current.SendNotifications(users, messageText, data, "default", UserType.Driver);
        }

        public static void TestPush3()
        {
            var passengerUser = new User()
            {
                PassengerNotificationToken = "32da4d76bc351488825b67370ca0740b3845a1ec82abe6ee185936cc5ee0c10f",
                DeviceId = "35E7F946-207C-4074-AE7F-A10F02D7D1FF",
                Phone = "+972544850318",
                Active = true,
                PlatformId = (int)PlatformTypes.IOS,
                VersionOS = "9.2",
                AppVersion = "1.0"
            };

            var extraInfo = new Dictionary<string, object>();


            extraInfo.Add("type", (int)PassengerNotificationTypes.ChooseCreditCard);

            //NotificationsServices.Current.SendNotification(passengerUser, "testing", extraInfo, "default", UserType.Passenger);

        }

        public static bool saveDetails(string name, string email, string phone, string type)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                //add culture to phone number:
                string phoneFormatted = UserService.CheckPhoneNumber(phone, 972);

                TwilioService twilioService = new TwilioService();
                //בדיקה אם המספר קיים
                var passenger = db.Users.Where(u => u.Phone == phoneFormatted && u.Driver == null).FirstOrDefault();

                //אם המספר לא קיים-יצירת נוסע חדש
                if (passenger == null)
                {
                    User new_user = db.Users.Create();
                    new_user.Name = passenger != null ? passenger.Name : null;
                    new_user.Email = passenger != null ? passenger.Email : null;
                    new_user.Active = true;
                    new_user.RegistrationDate = DateTime.UtcNow;
                    new_user.Phone = phone;
                    new_user.AuthenticationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    new_user.LanguageId = (int)UserLanguages.he;
                    new_user.DriverValidNotificationToken = false;
                    new_user.PassengerValidNotificationToken = false;
                    new_user.isFromWeb = true;

                    db.Users.Add(new_user);
                    db.SaveChanges();
                    passenger = new_user;

                    //שליחת אס.אם.אס עם קישור להורדת האפליקציה-לנוסעים החדשים
                    //android--https://tinyurl.com/kmxo9nd
                    //iphone---https://tinyurl.com/l3p4utv

                    var text = type == "Android" ? "נוסע יקר! היכנס לקישור הבא- https://tinyurl.com/kmxo9nd בכדי להוריד את האפליקציה שלנו. Werider." : "נוסע יקר! היכנס לקישור הבא- https://tinyurl.com/l3p4utv בכדי להוריד את האפליקציה שלנו. Rider.";

                    bool status1 = twilioService.SendSMS(phone, text, false);
                }

                //יצירת ושליחת קופון לכל אחד מהנוסעים שנרשם

                //יצירת קוד קופון רנדומלי
                Random r = new Random();
                int randNum = r.Next(1000000);
                string sixDigitNumber = randNum.ToString("D6");

                var couponEzer = db.Coupons.Where(c => c.number == sixDigitNumber).FirstOrDefault();
                while (couponEzer != null)
                {
                    r = new Random();
                    randNum = r.Next(1000000);
                    sixDigitNumber = randNum.ToString("D6");
                    couponEzer = db.Coupons.Where(c => c.number == sixDigitNumber).FirstOrDefault();
                }
                var userCoupon = db.Coupons.Where(c => c.passengerIdSMS == passenger.UserId && c.orderId == null && c.dtEnd >= DateTime.UtcNow).FirstOrDefault();
                if (userCoupon != null)
                {
                    throw new UserForbiddenException();
                }
                else
                {

                    //יצירת קופון
                    var coupon = new Coupon()
                    {
                        number = sixDigitNumber,
                        amount = 100,
                        currency = "IL",
                        dtStart = DateTime.UtcNow,
                        dtEnd = DateTime.UtcNow.AddMonths(1),
                        passengerIdSMS = passenger.UserId
                    };
                    db.Coupons.Add(coupon);
                    db.SaveChanges();

                    //שליחת קופון באס.אם.אס.
                    var textEn = "You have received a coupon valued at 100 NIS! Enter the code " + sixDigitNumber + " before the next ride to redeem it. *The coupon is valid for one month only. Werider App.";
                    var textHe = "קיבלת קופון בסך 100 ש''ח. הזן את הקוד " + sixDigitNumber + " לפני הנסיעה הבאה שלך.";
                    //var textHe = "קיבלת קופון בסך 100 ש''ח. הזן את הקוד " + sixDigitNumber + "  לפני הנסיעה הבאה שלך בכדי לממש את ההטבה. *הקופון תקף לחודש ימים. צוות ריידר. ";
                    bool status = twilioService.SendSMS(phone,/*passenger.LanguageId==1?textHe:*/textHe, false);
                    if (status == true)
                        return true;
                }
            }
            return false;
        }

        public static string daleteFromPhoneApproved(string phone)
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var row = db.PhoneDriversApproveds.Where(p => p.phone == phone).FirstOrDefault();
                    if (row != null)
                    {
                        var userDriver = db.Users.Where(d => d.Driver != null && d.Phone == phone).FirstOrDefault();
                        if (userDriver == null)
                        {
                            db.PhoneDriversApproveds.Remove(row);
                            db.SaveChanges();
                            return "success";
                        }
                        else
                            return "the driver is registered to the app";
                    }
                    return "the phone not exist in db";
                }
            }
            catch (Exception e)
            {

                return "error";
            }
        }
    }
}
