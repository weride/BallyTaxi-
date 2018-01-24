using System;
using System.Collections.Generic;


namespace Quickode.BallyTaxi.Models
{

    public class LocationModel
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public double? heading { get; set; }
        /*public double Lan { get; set; } //TODO: for competability mode. should be removed*/
    }

    public class LocationModelForAvailabletaxis
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
        public long orderId { get; set; }
    }

    public class DriverToPrint
    {
        public string companyNumber { get; set; }
        public double distance { get; set; }
        public double startTime { get; set; }
        public double endTime { get; set; }
    }

    public class DriverProfileModels
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string LicensePlate { get; set; }
        public string TaxiLicense { get; set; }
        public int CarTypeId { get; set; }
        public Guid ImageID { get; set; }
        public int TaxiStationId { get; set; }
        public bool AcceptCreditCard { get; set; }
        public int ChargeCCId { get; set; }
        public int BankNumber { get; set; }
        public int BankBranch { get; set; }
        public string BankAccount { get; set; }
        public string BankHolderName { get; set; }
        public string IdentityCardNumber { get; set; }
        public string CCProviderNumber { get; set; }
        public int? paymentMethod { get; set; }
        public string driverCode { get; set; }
        public int payment { get; set; }
        public int seatsNumber { get; set; }
        public int? courier { get; set; }
        public bool? isHandicapped { get; set; }

        public string companyNumber { get; set; }
        public int productionYear { get; set; }

        public bool? isReadTermsOfUse { get; set; }
        public bool? isPrivate { get; set; }

        public string tz { get; set; }
        public string studentCard { get; set; }
        public string authorizedDealer { get; set; }


    }

    public class DriverProfileToDisplay
    {
        public long DriverID { get; set; }
        public Guid? ImageID { get; set; }
        public CarObject carObject { get; set; }
        public string TaxiLicense { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string TaxiStationName { get; set; }
        public int TaxiStationID { get; set; }
        public int DriverStatus { get; set; }
        public bool ValidNotificationToken { get; set; }
        public string driverCode { get; set; }
        public string paymentForMonth { get; set; }
        public string paymentForRide { get; set; }
        public string paymentForMonthNew { get; set; }
        public string lastNumbersCC { get; set; }
        public int payment { get; set; }

        public string Token { get; set; }
        public int? PaymentStatus { get; set; }
        public bool WantFutureRide { get; set; }
        public int? paymentMethod { get; set; }
        public bool isSucceedPayPal { get; set; }
        public int seatsNumber { get; set; }
        public int courier { get; set; }
        public bool isHandicapped { get; set; }
        public bool isBankDetails { get; set; }
        public string companyNumber { get; set; }
        public bool isReadTermsOfUse { get; set; }
        public bool isPrivate { get; set; }
        public string tz { get; set; }
        public string studentCard { get; set; }
        public string authorizedDealer { get; set; }
        public int rating { get; set; }


        public DriverProfileToDisplay(Driver driver, User user, TaxiStation station, CarType car = null, string lastNumbersCC = null)
        {
            this.DriverID = driver.UserId;
            this.ImageID = user.ImageId;
            this.PhoneNumber = user.Phone;
            this.Name = user.Name;
            this.Email = user.Email;
            this.DriverStatus = driver.Status.Value;
            this.driverCode = driver.driverCode;
            this.lastNumbersCC = lastNumbersCC;
            this.seatsNumber = driver.seats == null ? 4 : driver.seats.Value;
           
            payment = driver.payment.HasValue ? driver.payment.Value : 0;
            courier = driver.courier.HasValue ? driver.courier.Value : 0;
            isHandicapped = driver.isHandicapped.HasValue ? driver.isHandicapped.Value : false;

            companyNumber = driver.companyNumber;
            tz = driver.tz;
            studentCard = driver.studentCard;
            authorizedDealer = driver.authorizedDealer;

            Token = user.AuthenticationToken;
            PaymentStatus = driver.PaymentStatus;
            WantFutureRide = true;
            paymentMethod = driver.paymentMethod;
            isSucceedPayPal = user.PayPalId != null && user.PayPalId != "" ? true : false;

            this.isBankDetails = driver.BankAccount != null ? true : false;

            this.carObject = new CarObject()
            {
                CarTypeId = driver.CarType.HasValue ? driver.CarType.Value : 0,
                LicensePlate = driver.LicensePlate,
                CarTypeName = car != null ? user.LanguageId == (int)UserLanguages.en ? car.EnglishName : car.HebrewName : "",
                productionYear = driver.productionYear.HasValue ? driver.productionYear.Value : 0
            };

            isReadTermsOfUse = user.isReadTermsOfUse.HasValue ? user.isReadTermsOfUse.Value : false;
            isPrivate = driver.isPrivate.HasValue ? driver.isPrivate.Value : false;

            this.TaxiLicense = driver.TaxiLicense;
            if (station != null)
            {//!!
                TaxiStationID = station.StationId;
                TaxiStationName = user.LanguageId == (int)UserLanguages.en ? station.EnglishName : station.HebrewName;
            }
            else
                TaxiStationName = "";

            ValidNotificationToken = user.DriverValidNotificationToken;
        }
        public DriverProfileToDisplay()
        {

        }
    }

    public class DriverDTOModels
    {
        public DriverProfileToDisplay DriverObject { get; set; }
        public string Token { get; set; }
        public int? PaymentStatus { get; set; }
        public int? DriverStatus { get; set; }
        public bool WantFutureRide { get; set; }
        public int? paymentMethod { get; set; }
        public bool isSucceedPayPal { get; set; }
        public bool isBankDetails { get; set; }
        public Dictionary<string, string> systemdData { get; set; }


        public OrderDetailsModel nextRideObject { get; set; }
        public DriverDTOModels(Driver driver, User user, TaxiStation station)
        {
            this.Token = user.AuthenticationToken;
            this.DriverObject = new DriverProfileToDisplay(driver, user, station);
            this.PaymentStatus = driver.PaymentStatus;
            this.paymentMethod = driver.paymentMethod;
            this.DriverStatus = driver.Status;
            this.WantFutureRide = driver.WantFutureRide;
            this.isBankDetails = driver.BankAccount != null ? true : false;

        }

        public DriverDTOModels(Driver driver, User user, TaxiStation station, CarType car, string lastNumbersCC = null)
        {
            this.Token = user.AuthenticationToken;
            this.DriverObject = new DriverProfileToDisplay(driver, user, station, car, lastNumbersCC);
            this.PaymentStatus = driver.PaymentStatus;
            this.paymentMethod = driver.paymentMethod;
            this.DriverStatus = driver.Status;
            this.WantFutureRide = driver.WantFutureRide;
            this.isSucceedPayPal = user.PayPalId != null && user.PayPalId != "" ? true : false;
            this.isBankDetails = driver.BankAccount != null ? true : false;
        }

        public DriverDTOModels(Driver driver, Order order, User user, TaxiStation station)
        {
            this.Token = user.AuthenticationToken;
            this.DriverObject = new DriverProfileToDisplay(driver, user, station);
            this.PaymentStatus = driver.PaymentStatus;
            this.paymentMethod = driver.paymentMethod;
            this.DriverStatus = driver.Status;
            this.WantFutureRide = driver.WantFutureRide;
            this.isBankDetails = driver.BankAccount != null ? true : false;
            if (order != null)
            {
                this.nextRideObject = new OrderDetailsModel(order, driver, user, null, station);
            }
        }

        public DriverDTOModels(Driver driver, Order order, User user, TaxiStation station, CarType car, string lastNumbersCC)
        {
            this.Token = user.AuthenticationToken;
            this.DriverObject = new DriverProfileToDisplay(driver, user, station, car, lastNumbersCC);
            this.PaymentStatus = driver.PaymentStatus;
            this.paymentMethod = driver.paymentMethod;
            this.isSucceedPayPal = user.PayPalId != null && user.PayPalId != "" ? true : false;
            this.DriverStatus = driver.Status;
            this.WantFutureRide = driver.WantFutureRide;
            this.isBankDetails = driver.BankAccount != null ? true : false;

            if (order != null)
            {
                this.nextRideObject = new OrderDetailsModel(order, driver, user, null, station);
            }
        }
        public DriverDTOModels()
        {

        }
    }

    public class CarObject
    {
        public string LicensePlate { get; set; }
        public int CarTypeId { get; set; }
        public string CarTypeName { get; set; }
        public int productionYear { get; set; }
        public CarObject()
        {

        }
    }

    public class BaseDriverData
    {
        public long UserID { get; set; }
        public double? Lat { get; set; }
        public double? Lon { get; set; }
        public DateTime? LastUpdateLocation { get; set; }
        public int? Status { get; set; }
        public BaseDriverData(Driver driver)
        {
            this.UserID = driver.UserId;
            this.Lat = driver.Location.Latitude;
            this.Lon = driver.Location.Longitude;
            this.LastUpdateLocation = driver.LastUpdateLocation;
            this.Status = driver.Status;
        }
    }
}