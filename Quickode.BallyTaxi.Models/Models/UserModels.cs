namespace Quickode.BallyTaxi.Models
{
    public class RegisterUserModels
    {
        public string phoneNumber { get; set; }
        public string phonePrefix { get; set; }
    }

    public class NewUser
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string type { get; set; }
    }

    public class UserImageModel
    {
        public string Base64 { get; set; }
        public string Extension { get; set; }

    }

    public class ValidateSMSCode
    {
        public string Phone { get; set; }
        public string Code { get; set; }
        public string DeviceID { get; set; }
        public int PlatformID { get; set; }
        public string OSVersion { get; set; }
        public string AppVersion { get; set; }
        public int UserType { get; set; }
        public int LanguageId { get; set; }
        public bool replaceDeviceId { get; set; }
    }

    public class ResendValidateSMSCode
    {
        public string phoneNumber { get; set; }
    }

    public class languageData
    {
        public int languageId { get; set; }
        public string appVersion { get; set; }
    }

    public class UpdateNotification
    {
        public int UserType { get; set; }
        public string Token { get; set; }
        public bool? Developer { get; set; }
    }

    public class SendMessage
    {
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    //public class UserDTOModels
    //{
    //    public long UserId { get; set; }
    //    public long RegistrationDate { get; set; }
    //    public string Phone { get; set; }
    //    public string Email { get; set; }
    //    public string DeviceId { get; set; }
    //    public string NotificationToken { get; set; }
    //    public int PlatformId { get; set; }
    //    public string VersionOS { get; set; }
    //    public string AuthenticationToken { get; set; }
    //    public bool Active { get; set; }
    //    public UserDTOModels(User user)
    //    {
    //        this.Active = user.Active;
    //        this.AuthenticationToken = user.AuthenticationToken;
    //        this.DeviceId = user.DeviceId;
    //        this.Email = user.Email;
    //        this.NotificationToken = user.NotificationToken;
    //        this.Phone = user.Phone;
    //        this.PlatformId = user.PlatformId.Value;
    //        this.UserId = user.UserId;
    //        this.VersionOS = user.VersionOS;
    //        DateTime UnixEpoch = new DateTime(1 9 7 0, 1, 1, 0, 0, 0, DateTimeKind.Utc); 
    //        this.RegistrationDate = (long)(user.RegistrationDate - UnixEpoch).TotalSeconds; ;
    //    }

    //}

    /*public class UserObject  
    {  
        public string token { get; set; }
        public DriverProfileToDisplay DriverObject { get; set; }
        public PassengerProfileToDisplay PassengerObject { get; set; }

        public int? paymentStatus { get; set; }
        public UserObject(User user) 
        {
            this.token = user.AuthenticationToken;
            if (user.Driver != null) { 
                this.DriverObject = new DriverProfileToDisplay(user.Driver, user, null); 
                this.paymentStatus = user.Driver.PaymentStatus;
            }
            if (user.Passenger != null)
            {
                this.PassengerObject = new PassengerProfileToDisplay(user); 
            }
        }
    }*/



}