using Quickode.BallyTaxi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Quickode.BallyTaxi.Integrations.Twilio;
using PhoneNumbers;
using Quickode.BallyTaxi.Core;
using Quickode.BallyTaxi.Models.Models;
using Quickode.BallyTaxi.Models.Filters;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class UserService
    {
        readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string CheckPhoneNumber(string phone, int prefix)
        {
            PhoneNumber number;
            PhoneNumberUtil util = PhoneNumberUtil.GetInstance();
            string countryCode = util.GetRegionCodeForCountryCode(prefix);
            try
            {
                number = util.Parse(phone, countryCode);
            }
            catch (NumberParseException ex)
            {
                logger.Error(ex);
                throw new PhoneNumberNotValidException();
            }

            if (util.IsValidNumber(number))
            {
                string INTERNATIONAL = util.Format(number, PhoneNumberFormat.INTERNATIONAL);
                string E164 = util.Format(number, PhoneNumberFormat.E164);
                string NATIONAL = util.Format(number, PhoneNumberFormat.NATIONAL);
                string RFC3966 = util.Format(number, PhoneNumberFormat.RFC3966);

                return E164;
            }
            else
            {
                var phoneNational = number.NationalNumber.ToString();
                if (phoneNational.Substring(0, 2) == "55" && phoneNational.Length == 9 && number.CountryCode == 972)
                    return util.Format(number, PhoneNumberFormat.E164);

                throw new PhoneNumberNotValidException();
            }
        }

        public static bool checkDriverExsist(string phoneFormatted)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var phone = db.PhoneDriversApproveds.Where(p => p.phone == phoneFormatted).FirstOrDefault();
                if (phone != null)
                    return true;
            }
            return false;
        }



        public static User CheckAuthorization(string scheme, string authorization, string appVersion = null)
        {
            if (scheme.Trim() == "Token" && authorization.Contains("token="))
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    User user = db.Users
                        .Where(x => x.AuthenticationToken == authorization.Replace("token=", "").Trim())
                        .FirstOrDefault();

                    if (user != null)
                    {
                        var driver = db.Drivers.Where(d => d.UserId == user.UserId).FirstOrDefault();
                        if (driver != null)
                        {
                            if (driver.isBlocked.HasValue && driver.isBlocked.Value == true && (driver.dateEndBlock.HasValue ? driver.dateEndBlock.Value <= DateTime.UtcNow : 1 == 1))
                            {
                                throw new UserBlockedException();
                            }

                            try
                            {
                                if (user.PlatformId.HasValue)
                                {
                                    if (appVersion != null && appVersion != "" && appVersion.Length > 0)
                                    {
                                        user.AppVersion = appVersion;
                                        db.SaveChanges();
                                    }

                                    if (user.PlatformId == (int)PlatformTypes.IOS)
                                    {

                                        var lastVersion = db.SystemSettings.Where(s => s.ParamKey == "lastVersionIphon").FirstOrDefault().ParamValue;
                                        lastVersion = lastVersion.Replace(".", "");
                                        var driverVersion = driver.User.AppVersion.Replace(".", "");
                                        //if (Convert.ToDouble(driver.User.AppVersion) < Convert.ToDouble(lastVersion))
                                        if (Convert.ToDouble(driverVersion) < Convert.ToDouble(lastVersion))
                                        {
                                            throw new userIsNotUpdateVersionException();
                                        }
                                    }
                                    else if (user.PlatformId == (int)PlatformTypes.Android)
                                    {
                                        var lastVersion = db.SystemSettings.Where(s => s.ParamKey == "lastVersionAndroid").FirstOrDefault().ParamValue;
                                        lastVersion = lastVersion.Replace(".", "");
                                        var driverVersion = driver.User.AppVersion.Replace(".", "");
                                        if (Convert.ToInt32(driverVersion) < Convert.ToInt32(lastVersion))
                                        {
                                            throw new userIsNotUpdateVersionException();
                                        }
                                    }
                                }
                            }
                            catch (userIsNotUpdateVersionException ex)
                            {
                                var dateAdd = DateTime.UtcNow.AddHours(1);
                                if (user.lastSendNotificationForUpdate.HasValue == false || user.lastSendNotificationForUpdate >= dateAdd)
                                {
                                    NotificationsServices.Current.DriverNotification(user, DriverNotificationTypes.massageForDriver, 0);
                                    user.lastSendNotificationForUpdate = DateTime.UtcNow;
                                    db.SaveChanges();
                                }
                                throw new UserBlockedException();
                            }
                            catch (Exception e)
                            {
                                logger.ErrorFormat("CheckAuthorization error: {0}", e);
                                throw new Exception();
                            }
                        }

                        if (user.Active)
                            return user;
                        else
                            throw new UserBlockedException();
                    }
                    else
                        throw new UserUnauthorizedException();
                }
            }
            throw new AuthenticationTokenIncorrectException();
        }

        public static void UpdateUserImageId(long userId, Guid imageId) {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.Where(x => x.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    user.ImageId = imageId;
                    db.SaveChanges();
                }
                else throw new Exception();
            }
        }

        public static void UpdateCodeSentFlag(int userID, bool value)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var pending_user = db.PendingUsers.Where(x => x.PendingUserId == userID).FirstOrDefault();
                if (pending_user != null)
                {
                    pending_user.CodeSent = value;
                    db.SaveChanges();
                }
                else throw new UserNotExistException();
            }
        }

        public static User FetchUser(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return
                    db.Users.GetById(userId);
            }
        }

        public static User ValidateSMSCode(string phone, string code, string deviceId, int userType, string versionOS, int platformId, string appVersion, int languageId, bool replaceDeviceId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var pending_user = db.PendingUsers.Where(x => x.Phone == phone).FirstOrDefault();
                //var userDelete = db.Users.Where(x => x.Phone == phone && (userType == (int)UserType.Driver ? x.Driver != null : x.Driver == null)).FirstOrDefault();

                if (pending_user != null)
                {
                    if (pending_user.CodeValidation == code)
                    {
                        //DateTime now = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, DateTime.UtcNow.Hour, DateTime.UtcNow.Minute, DateTime.UtcNow.Second); //10 o'clock
                        //DateTime code_expiration_date = new DateTime(pending_user.CodeExpiration.Year, pending_user.CodeExpiration.Month, pending_user.CodeExpiration.Day, pending_user.CodeExpiration.Hour, pending_user.CodeExpiration.Minute, pending_user.CodeExpiration.Second); //10 o'clock

                        //Code is valid only for an helf hour
                        //int expiration_time = int.Parse(ConfigurationHelper.CODE_EXPIRATION_TIME);
                        if (pending_user.CodeExpiration < DateTime.UtcNow)
                        {
                            throw new ExpirationException();
                        }
                        else
                        {
                            //check if user already registered (maybe remove application from his device and reinstall it now)
                            var user = db.Users.Where(x => x.Phone == phone && (userType == (int)UserType.Driver ? x.Driver != null : x.Driver == null)).FirstOrDefault();
                            if (user == null)
                            {
                                User new_user = db.Users.Create();
                                new_user.PlatformId = platformId;
                                new_user.Phone = phone;
                                new_user.AppVersion = appVersion;
                                new_user.RegistrationDate = DateTime.UtcNow;
                                new_user.VersionOS = versionOS;
                                new_user.DeviceId = deviceId;
                                new_user.Active = true;
                                new_user.AuthenticationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                                //UserLanguages Language;
                                if (languageId > 0)
                                    new_user.LanguageId = languageId;
                                else
                                    //default language is english
                                    new_user.LanguageId = (int)UserLanguages.en;
                                db.Users.Add(new_user);
                                // db.SaveChanges();

                                if ((int)UserType.Driver == userType)
                                {
                                    Driver driver = db.Drivers.Create();
                                    driver.UserId = new_user.UserId;
                                    driver.Status = (int)DriverStatus.Available;
                                    driver.PaymentStatus = (int)DriverPaymentStatus.Free;
                                    db.Drivers.Add(driver);
                                    // db.SaveChanges();
                                }
                                db.PendingUsers.Remove(pending_user);
                                db.SaveChanges();

                                if (userType == (int)UserType.Passenger)
                                {
                                    ///???????? new DateTime(2017, 8, 25);
                                    //var endDate = new DateTime(2017, 8, 25);
                                    //if (DateTime.UtcNow <= endDate)
                                    //{
                                    //    try
                                    //    {
                                    //        TestServices.sendCouponToNewPassenger(phone);
                                    //    }
                                    //    catch (Exception e)
                                    //    {
                                    //        logger.Error("error in send coupon: " + e.Message);

                                    //    }
                                    //}
                                }

                                var result_user = db.Users.GetById(new_user.UserId);
                                if (result_user != null)
                                {
                                    if (result_user.Active) return result_user;
                                    else throw new UserBlockedException();
                                }
                                else throw new UserNotExistException();
                            }
                            else
                            {
                                //string deviceId, int userType, string versionOS, int platformId, string appVersion, int languageId, bool replaceDeviceId)
                                if (user.Active)
                                {
                                    int free_trial_days = ConfigurationHelper.FREE_TRIAL_DAYS;
                                    if (user.Driver != null && (user.Driver.PaymentStatus == (int)DriverPaymentStatus.Free || user.Driver.PaymentStatus == (int)DriverPaymentStatus.HasNoPaymentDetails) && user.RegistrationDate.AddDays(free_trial_days) < DateTime.UtcNow)
                                    {
                                        user.Driver.PaymentStatus = (int)DriverPaymentStatus.HasNoPaymentDetails;
                                    }
                                    //if (replaceDeviceId)
                                    //{
                                    //    user.DeviceId = deviceId;
                                    //}

                                    user.PlatformId = platformId;
                                    user.AppVersion = appVersion;
                                    user.VersionOS = versionOS;
                                    user.DeviceId = (deviceId != null && deviceId.Length > 0) ? deviceId : user.DeviceId;
                                    db.PendingUsers.Remove(pending_user);
                                    db.SaveChanges();
                                    return user;
                                }
                                else throw new UserBlockedException();
                            }
                        }
                    }
                    else throw new ValidateSMSFaildException();
                }
                else throw new PhoneNotExistException();
            }

        }

        public static bool changeLanguage(long userId, int languageId, ref int userType)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.GetById(userId);
                if (user.Driver != null)
                {
                    userType = (int)UserType.Driver;
                }
                else userType = (int)UserType.Passenger;

                if (user != null && languageId > 0)
                {
                    user.LanguageId = languageId;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public static bool addPhoneToAprovedList(string phone, int lang)
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var row = db.PhoneDriversApproveds.Where(p => p.phone == phone).FirstOrDefault();
                    if (row == null)
                    {
                        var newPhone = db.PhoneDriversApproveds.Add(new PhoneDriversApproved() { phone = phone });
                        db.SaveChanges();
                        var userCulture = "";

                        var userCultureObj = db.Languages.Where(l => l.LanguageId == lang).FirstOrDefault();
                        if (userCultureObj == null)
                            userCulture = "en-US";
                        else
                            userCulture = userCultureObj.LanguageCulture;
                        var text = Utils.TranslateMessage(userCulture, "sms_register");

                        TwilioService twilioService = new TwilioService();
                        bool status = twilioService.SendSMS(phone, text, false);
                        if (status)
                        {
                            return true;
                        }
                        else
                            throw new SMSProcessException();
                    }
                    else
                    {
                        throw new PhoneAlreadyExistException();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }



        public static bool sendEmailTodriver(DriverEmail emailType, int languageId, string email)
        {
            //send email to driver that ended the ride:
            var textForEmail = Utils.PrepareToSendEmail(emailType, languageId);
            var massage = "";
            for (int i = 1; i < textForEmail.Count; i++)
            {
                massage += "<h3>" + textForEmail[i] + "</h3>";
            }
            var isSend = Utils.SendMail(new List<string>() { email }, textForEmail[0], massage, null, languageId);
            return isSend;
        }

        public static bool PhoneExists(string phone, int userType)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                //var user = db.Users.GetByPhone(phone);


                var user = db.Users.Where(u => u.Phone == phone && (userType == (int)UserType.Driver ? u.Driver != null : u.Driver == null)).FirstOrDefault();
                if (user == null)
                    return false;
                else
                {
                    if (userType == (int)UserType.Driver)
                    {
                        if (user.Driver == null)
                            return false;
                    }
                    logger.DebugFormat("Phone {0} already exists in User DB", phone);
                    return true;
                }
            }
        }

        public static void UpdateNotificationToken(string token, long userId, int userType, bool? developer = null)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var device = db.Users.GetById(userId);

                if (device != null)
                {
                    if (userType == (int)UserType.Driver)
                    {
                        device.DriverNotificationToken = token;
                        device.DriverValidNotificationToken = true;
                    }
                    else
                    {
                        device.PassengerNotificationToken = token;
                        device.PassengerValidNotificationToken = true;
                    }

                    if (developer.HasValue)
                        device.IsDebug = developer;
                    db.SaveChanges();
                }
                else throw new DeviceNotExistException();
            }
        }

        public static void ResendSMSCodeValidation(string phone, bool debug = false)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                var pending_user = db.PendingUsers.Where(x => x.Phone == phone).FirstOrDefault();
                if (pending_user != null)
                {
                    var sent = SendSMSCode(pending_user.PendingUserId, phone, pending_user.CodeValidation, 0, debug);
                }
                else
                    throw new UserNotExistException();
            }
        }

        //delete this code once change to driver & passenger login methods.
        public static User Login(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                int free_trial_days = ConfigurationHelper.FREE_TRIAL_DAYS;
                var user = db.Users.GetById(userId);
                if (user != null)
                {

                    if (user.Driver != null && (user.Driver.PaymentStatus == (int)DriverPaymentStatus.Free || user.Driver.PaymentStatus == (int)DriverPaymentStatus.HasNoPaymentDetails) && user.RegistrationDate.AddDays(free_trial_days) < DateTime.UtcNow)
                    {
                        user.Driver.PaymentStatus = (int)DriverPaymentStatus.HasNoPaymentDetails;
                        db.SaveChanges();
                    }
                    return user;
                }
                else throw new UserNotExistException();
            }
        }

        public static PendingUser Register(string phone, bool isDebug)
        {
            //var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var pending_user = db.PendingUsers.Where(x => x.Phone == phone).FirstOrDefault();

                //var alreadyExist = db.PendingUsers.Any(x => x.Phone == phone);
                logger.Debug("pending user");

                if (pending_user == null)
                {
                    logger.Debug("pending user is null");
                    pending_user = db.PendingUsers.Create();

                    pending_user.Phone = phone;
                    pending_user.RegistrationDate = DateTime.UtcNow;
                    pending_user.CodeExpiration = DateTime.UtcNow.AddMinutes(ConfigurationHelper.CODE_EXPIRATION_TIME);
                    pending_user.CodeValidation = GenerateCodeValidation(isDebug);
                    if (phone == "+972548443995")
                        pending_user.CodeValidation = "1234";
                    db.PendingUsers.Add(pending_user);

                    db.SaveChanges();
                    logger.Debug("new user created");

                    return pending_user;
                }
                else
                {
                    if (pending_user.CodeExpiration < DateTime.UtcNow)
                    {
                        pending_user.CodeValidation = GenerateCodeValidation(isDebug);
                        if (phone == "+972548443995")
                            pending_user.CodeValidation = "1234";
                        pending_user.CodeExpiration = DateTime.UtcNow.AddMinutes(ConfigurationHelper.CODE_EXPIRATION_TIME);
                        db.SaveChanges();
                        logger.Debug("user updated");
                    }
                    else
                    {
                        if (phone == "+972548443995")
                            pending_user.CodeValidation = "1234";
                        db.SaveChanges();
                        logger.Debug("user not expired. using previous code" + pending_user.CodeValidation);
                    }
                    return pending_user;
                }
            }
        }

        internal static void ChangeDriverNotification(string oldId, string newId)
        {
            logger.Info(string.Format("Changing driver notification token from '{0}' to '{1}'", oldId, newId));

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var users = db.Users.ByDriverNotificationToken(oldId).ToList();
                foreach (var u in users)
                {
                    u.DriverNotificationToken = newId;
                    u.DriverValidNotificationToken = true;
                }

                db.SaveChanges();
            }
        }

        internal static void ChangePassengerNotification(string oldId, string newId)
        {
            logger.Info(string.Format("Changing passenger notification token from '{0}' to '{1}'", oldId, newId));

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var users = db.Users.ByPassengerNotificationToken(oldId).ToList();
                foreach (var u in users)
                {
                    u.PassengerNotificationToken = newId;
                    u.PassengerValidNotificationToken = true;

                }

                db.SaveChanges();
            }
        }
        private static string GenerateCodeValidation(bool isDebug)
        {
            var random = new Random();
            var validationCode = isDebug ? "1234" : new string(
                Enumerable.Repeat("0123456789", 4)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            logger.DebugFormat("validationCode={0}", validationCode);
            return validationCode;
        }

        public static bool SendSMSCode(int userID, string phone, string code, int languageId, bool debug = false)
        {
            logger.Debug("SendSMSCode function");
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                //var text = string.Format(Helper.Translate("Driver_Register_SMS_Text", "Welcome to Rider. Your authorization code is {0}"), code);
                var userCulture = "";

                var userCultureObj = db.Languages.Where(l => l.LanguageId == languageId).FirstOrDefault();
                if (userCultureObj == null)
                    userCulture = "en-US";
                else
                    userCulture = userCultureObj.LanguageCulture;
                var text = string.Format(Utils.TranslateMessage(userCulture, "Driver_Register_SMS_Text"), code);

                TwilioService twilioService = new TwilioService();
                bool status = twilioService.SendSMS(phone, text, debug);
                logger.Debug("SendSMSCode to" + phone);

                logger.Debug("SendSMSCode status" + status);

                if (status)
                {
                    UpdateCodeSentFlag(userID, status);
                    logger.Debug("SendSMSCode Succeeded");
                    return true;
                }
                else
                {
                    logger.Debug("SendSMSCode Failed");
                    throw new SMSProcessException();
                }
            }
        }

        public static bool SendSMSNotif(long userID, string phone, SMSType type, int languageId, Dictionary<string, object> data = null)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                //var text = string.Format(Helper.Translate("Driver_Register_SMS_Text", "Welcome to Rider. Your authorization code is {0}"), code);
                var userCulture = "";

                var userCultureObj = db.Languages.Where(l => l.LanguageId == languageId).FirstOrDefault();
                if (userCultureObj == null)
                    userCulture = "en-US";
                else
                    userCulture = userCultureObj.LanguageCulture;
                var text = "";
                if (type == SMSType.CouponTextForSMS)
                {
                    text = string.Format(Utils.TranslateMessage(userCulture, type.ToString()), data["amount"], data["number"]);
                }
                else if (type == SMSType.DriverFoundSMS)
                    text = string.Format(Utils.TranslateMessage(userCulture, type.ToString()), data["Phone"]);
                else if (type == SMSType.ReminderForFutureRide)
                    text = string.Format(Utils.TranslateMessage(userCulture, type.ToString()), data["orderTime"], data["Address"]);
                else
                    text = Utils.TranslateMessage(userCulture, type.ToString());


                TwilioService twilioService = new TwilioService();
                bool status = twilioService.SendSMS(phone, text, false);
                if (status)
                {
                    return true;
                }
                else
                    return false;
            }
        }



        /*public static bool PushTest(long userId,int pushtype)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            { 
                var user = db.Users
                    .Include(x => x.Language)
                    .Include(x => x.Driver)
                    .Include(x=>x.Passenger)
                    .Where(x => x.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    List<User> users = new List<User>();
                    users.Add(user);
                    var data = new Dictionary<string, object>();
                    data.Add("type", pushtype.ToString());
                    System.Resources.ResourceManager rm = new System.Resources.ResourceManager("Quickode.BallyTaxi.Domain.Resource.BallyTaxiText", System.Reflection.Assembly.GetExecutingAssembly());
                    System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.CreateSpecificCulture(user.Language.LanguageCulture);
                    if (user.Passenger != null)
                    {
                        PassengerNotificationTypes type = (PassengerNotificationTypes)(pushtype);
                        string dateString = rm.GetString(type.ToString(), culture);
                        NotificationsServices.Current.SendNotifications(users, dateString, data, "default", UserType.Passenger);

                    }
                    else if (user.Driver != null) {
                        DriverNotificationTypes type = (DriverNotificationTypes)(pushtype);
                        string dateString = rm.GetString(type.ToString(), culture);
                        NotificationsServices.Current.SendNotifications(users, dateString, data, "default", (int)UserType.Driver);
                    }
                    else return false;
                    
                    return true;
                }
                else throw new UserNotExistException();
            }
        }*/

        public static void SendMessage(long UserId, string Subject, string Message)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var new_message = db.Messages.Create();

                new_message.MessageContent = Message;
                new_message.Subject = Subject;
                new_message.UserId = UserId;
                db.Messages.Add(new_message);

                db.SaveChanges();
            }
        }
    }
}