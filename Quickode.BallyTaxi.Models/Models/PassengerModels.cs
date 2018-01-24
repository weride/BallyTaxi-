using System;
using System.Collections.Generic;

namespace Quickode.BallyTaxi.Models
{
    public class PassengerProfileModels
    {    
        public string Email { get; set; }
        public string Name { get; set; }
        // public int Language { get; set; }
        public Guid? ImageID { get; set; }
        public int PreferredPaymentMethod { get; set; }
        public int businessId { get; set; }
        public int preferredTaxiStationId { get; set; }
        public double latHome { get; set; }
        public double longHome { get; set; }
        public string homeAddress { get; set; }
        public string homeCity { get; set; }
        public double latBusiness { get; set; }
        public double longBusiness { get; set; }
        public string businessAddress { get; set; }
        public string businessCity { get; set; }
        public bool? isHandicapped { get; set; }

    }

    public class PassengerProfileToDisplay
    {
        public long PassengerID { get; set; }
        public Guid? ImageID { get; set; }
        public string Phone { get; set; }
        public string FullName { get; set; }
        public int PreferredPaymentMethod { get; set; }
        //public int businessId { get; set; }
        //public string businessName { get; set; }
        public string Email { get; set; }
        public int preferredTaxiStationId { get; set; }
        public string preferredTaxiStationName { get; set; }
        public double latHome { get; set; }
        public double longHome { get; set; }
        public string homeAddress { get; set; }
        public string homeCity { get; set; }
        public double latBusiness { get; set; }
        public double longBusiness { get; set; }
        public string businessAddress { get; set; }
        public string businessCity { get; set; }
        public double couponAmount { get; set; }
        public string lastNumbersCC { get; set; }
        public double pickupLatitude { get; set; }
        public double pickupLongitude { get; set; }
        public double destinationLatitude { get; set; }
        public double destinationLongitude { get; set; }
        public bool HasPayPalAccount { get; set; }
        public bool isHandicapped { get; set; }

        public Models.BusinessModel businessModel { get; set; }

        public PassengerProfileToDisplay(User passengerUser,  Business business=null, double couponAmount=0, TaxiStation station=null, string lastNumbersCC = null, Order order=null)
        {
            if (passengerUser != null)
            {
                isHandicapped = passengerUser.isHandicapped.HasValue?passengerUser.isHandicapped.Value:false;
                this.lastNumbersCC = lastNumbersCC;
                PassengerID = passengerUser.UserId;
                ImageID = passengerUser.ImageId;
                Phone = passengerUser.Phone;
                FullName = passengerUser.Name;
                if (passengerUser.PayPalId != null)
                    HasPayPalAccount = true;
                else
                    HasPayPalAccount = false;
                if (passengerUser.PreferredPaymentMethod.HasValue)
                {
                    PreferredPaymentMethod = passengerUser.PreferredPaymentMethod.Value;
                    if (passengerUser.PreferredPaymentMethod.HasValue)
                    {
                        if (business != null) {
                            businessModel = new Models.BusinessModel()
                            {
                                BusinessId = passengerUser.BusinessId.HasValue ? passengerUser.BusinessId.Value : 0,
                                BusinessName = business.BusinessName,
                                isNeedFile = business.isNeedFile.HasValue ? business.isNeedFile.Value : false,
                                Phone = business.Phone
                            };
                        }
                    }
                }
                Email = passengerUser.Email;
                if(passengerUser.PreferedStationId.HasValue)
                {
                    preferredTaxiStationId = passengerUser.PreferedStationId.Value;
                    if(station!=null)
                    {
                        preferredTaxiStationName = passengerUser.LanguageId == (int)UserLanguages.he ? station.HebrewName : station.EnglishName;
                    }

                }
                if (passengerUser.locationHome != null)
                {
                    latHome = passengerUser.locationHome.Latitude.Value;
                    longHome = passengerUser.locationHome.Longitude.Value;
                }
                homeAddress = passengerUser.homeAddress;
                homeCity = passengerUser.homeCity;
                if (passengerUser.locationBusiness != null)
                {
                    latBusiness = passengerUser.locationBusiness.Latitude.Value;
                    longBusiness = passengerUser.locationBusiness.Longitude.Value;
                }

                if(order!=null)
                {
                    if(order.PickUpLocation!=null)
                    {
                        pickupLatitude = order.PickUpLocation.Latitude.Value;
                        pickupLongitude = order.PickUpLocation.Longitude.Value;
                    }
                    if(order.DestinationLocation!=null)
                    {
                        destinationLatitude = order.DestinationLocation.Latitude.Value;
                        destinationLongitude = order.DestinationLocation.Longitude.Value;
                    }
                }

                businessAddress = passengerUser.businessAddress;
                businessCity = passengerUser.businessCity;
               this.couponAmount = couponAmount;
            }
        }
        public PassengerProfileToDisplay()
        {

        }
    }

    public class PassengerDTOModels
    {

        public PassengerProfileToDisplay PassengerObject { get; set; }
        public string Token { get; set; }
        public Dictionary<string, string> systemdData { get; set; }
        public PassengerDTOModels(User passengerUser)
        {

            this.Token = passengerUser.AuthenticationToken;
            this.PassengerObject = new PassengerProfileToDisplay(passengerUser);
        }
        public PassengerDTOModels(User passengerUser, Dictionary<string, string> systemData)
        {

            this.Token = passengerUser.AuthenticationToken;
            this.PassengerObject = new PassengerProfileToDisplay(passengerUser);
            this.systemdData = systemData;
        }
        public PassengerDTOModels(User passengerUser, TaxiStation station, Business business, double couponAmount)
        {

            this.Token = passengerUser.AuthenticationToken;
            this.PassengerObject = new PassengerProfileToDisplay(passengerUser, business, couponAmount, station);
        }

        public PassengerDTOModels(User passengerUser,string lastNumbersCC, TaxiStation station, Business business, double couponAmount, Dictionary<string, string> systemData)
        {

            this.Token = passengerUser.AuthenticationToken;
            this.PassengerObject = new PassengerProfileToDisplay(passengerUser , business, couponAmount, station, lastNumbersCC);
            this.systemdData = systemData;
        }
    }

    public class FavoriteDriverModel
    {
        public long PassengerID { get; set; }
        public DriverProfileToDisplay Driver { get; set; }
        public string Notes { get; set; }

        public FavoriteDriverModel(FavoriteDriver favorite)
        {
            this.PassengerID = favorite.PassengerId;
            this.Driver = new DriverProfileToDisplay(favorite.Driver, favorite.Driver.User, favorite.Driver.TaxiStationId.HasValue ? favorite.Driver.TaxiStation : null);
            this.Notes = favorite.Notes;
        }
    }

    public class FavoriteStationObject
    {
        public int StationID { get; set; }
        public string StationName { get; set; }
    }

    public class FavoriteDriverObject
    {
        public long DriverID { get; set; }
        public string Notes { get; set; }
    }

    public class FavoriteOrderObject
    {
        public long OrderID { get; set; }
        public string Notes { get; set; }
    }

    public class FavoriteAddressObject
    {
        public double pickupLatitude { get; set; }
        public double pickupLongitude { get; set; }
        public string pickupAddress { get; set; }
        public double destinationLatitude { get; set; }
        public double destinationLongitude { get; set; }
        public string destinationAddress { get; set; }
    }

}