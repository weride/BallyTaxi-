using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using Quickode.BallyTaxi.Models.Filters;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class PassengerService
    {
        public static User UpdatePassengerProfile(long userId, string name, string email, Guid? imageId, int PreferredPaymentMethod, double latHome, double longHome, string homeAddress, string homeCity, double latBusiness, double longBusiness, string businessAddress, string businessCity, int preferredTaxiStationId, int businessId, bool? isHandicapped)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var passenger = db.Users.GetById(userId);
                if (passenger != null)
                {
                    passenger.Email = email;
                    passenger.Name = name;
                    //if (imageId != Guid.Empty)
                    //    passenger.ImageId = imageId;
                    passenger.PreferredPaymentMethod = PreferredPaymentMethod;
                    passenger.locationHome = Utils.LatLongToLocation(latHome, longHome);
                    passenger.homeAddress = homeAddress;
                    passenger.homeCity = homeCity;
                    if (latBusiness != 0 && longBusiness != 0)
                        passenger.locationBusiness = Utils.LatLongToLocation(latBusiness, longBusiness);
                    if (businessAddress != "" && businessAddress != null)
                    {
                        passenger.businessAddress = businessAddress;
                        passenger.businessCity = businessCity;
                    }
                    if (preferredTaxiStationId != 0)
                        passenger.PreferedStationId = preferredTaxiStationId;
                    if (PreferredPaymentMethod == (int)CustomerPaymentMethod.Business && businessId > 0)
                        passenger.BusinessId = businessId;
                    if (isHandicapped.HasValue)
                        passenger.isHandicapped = isHandicapped;
                    db.SaveChanges();
                    return passenger;
                }
                else throw new UserNotExistException();
            }
        }

        public static object getEstimateTimeLatLon(double pickupLatitude, double pickupLongitude, double destinationLatitude, double destinationLongitude, double time, int languageId, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                var userCulture = NotificationsServices.Current.GetLanguageCulture(languageId);
                try
                {
                    DateTime departureTime = time.ConvertFromUnixTimestamp();

                    var google_response = OrderService.MapsAPICall(pickupLatitude, pickupLongitude, destinationLatitude, destinationLongitude, userCulture, departureTime);
                    if (google_response != null)
                    {
                        //return new { lat = driver.Location.Latitude.Value, lon = driver.Location.Longitude.Value, estimateTime = google_response.value };
                        return new { lat = pickupLatitude, lon = pickupLongitude, estimateTime = google_response.duration.text, estimateDistance = google_response.distance.text, estimateTimeInTraffic = google_response.duration_in_traffic.text };
                    }
                    else
                        throw new GoogleAPIException();
                }
                catch (Exception e)
                {
                    return new { lat = pickupLatitude, lon = pickupLongitude };
                }
            }
        }

        //public static Passenger GetPassengerProfile(long userId)
        //{
        //    using (BallyTaxiEntities db = new BallyTaxiEntities())
        //    {
        //        var passenger = db.Passengers.Include(x => x.User).Where(x => x.UserId == userId).FirstOrDefault();
        //        if (passenger != null)
        //        {
        //            return passenger;
        //        }
        //        else throw new UserNotExistException();
        //    }
        //}

        public static List<FavoriteDriver> GetFavoriteDrivers(long passengerId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.FavoriteDrivers
                    //.Include(x=>x.Passenger.Passenger)
                    //.Include(x=>x.Driver.Driver)
                    .Where(x => x.PassengerId == passengerId).ToList();
            }
        }


        public static void AddFavoriteDriver(long passengerId, long driverId, string notes)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Drivers.Where(x => x.UserId == driverId).FirstOrDefault();
                if (driver != null)
                {
                    var favorite = db.FavoriteDrivers.Create();
                    favorite.DriverId = driverId;
                    favorite.PassengerId = passengerId;
                    favorite.Notes = notes;
                    favorite.CreationDate = DateTime.UtcNow;
                    db.FavoriteDrivers.Add(favorite);
                    db.SaveChanges();
                }
                else throw new UserNotExistException();
            }
        }

        public static double? getCouponAmountByUserId(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var list = db.Coupons.Where(c => c.passengerId == userId && DateTime.Now > c.dtStart && DateTime.Now < c.dtEnd && c.orderId == null).Select(c => c.amount).ToList();
                return
                   list.Sum();
            }
        }

        public static double getCuponAmount(long userId, string cuponCode)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var row = db.Coupons.Where(c => c.passengerId == null && c.number == cuponCode && c.orderId == null && c.dtStart < DateTime.Now && c.dtEnd > DateTime.Now).FirstOrDefault();
                if (row != null)
                {
                    row.passengerId = userId;
                    db.SaveChanges();
                    return row.amount;
                }
                return 0;
            }
        }

        public static User FetchPassenger(long passengerId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return
                    db.Users.GetById(passengerId);
            }
        }

        public static void RemoveFavoriteDriver(long passengerId, long driverId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Drivers.Where(x => x.UserId == driverId).FirstOrDefault();
                if (driver != null)
                {
                    var favorite = db.FavoriteDrivers.Where(x => x.PassengerId == passengerId && x.DriverId == driverId).FirstOrDefault();
                    if (favorite != null)
                    {
                        db.FavoriteDrivers.Remove(favorite);
                        db.SaveChanges();
                    }
                    else throw new FavoriteNotExistException();
                }
                else throw new UserNotExistException();
            }
        }

        public static List<CreditCardUser> getCreditCardListByUser(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var cards = db.CreditCardUsers.Where(c => c.userId == userId).OrderByDescending(c => c.creditCardUser1).OrderByDescending(o => o.isDefault).ToList();
                return cards.ToList();
            }
        }

        public static User createOrGetVirtualPassengerForDriver(long driverId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Users.GetById(driverId);
                if (driver != null)
                {
                    var user = db.Users.Where(u => u.Phone == "M" + driver.Phone && u.Driver == null).FirstOrDefault();
                    if (user == null)
                    {
                        User new_user = db.Users.Create();
                        new_user.Name = "meter - מונה";
                        new_user.Email = driver.Email;
                        new_user.Active = true;
                        new_user.RegistrationDate = DateTime.UtcNow;
                        new_user.Phone = "M" + driver.Phone;
                        new_user.AuthenticationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                        new_user.LanguageId = driver.LanguageId;
                        new_user.DriverValidNotificationToken = false;
                        new_user.PassengerValidNotificationToken = false;
                        new_user.isVirtual = true;

                        db.Users.Add(new_user);
                        db.SaveChanges();
                        return new_user;
                    }
                    else
                    {
                        return user;
                    }

                }
                return null;
            }
        }

        public static bool DeleteCreditCard(long creditCardUser, long UserId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var card = db.CreditCardUsers.Where(c => c.creditCardUser1 == creditCardUser && c.userId == UserId).FirstOrDefault();
                if (card != null)
                {
                    if (card.isDefault == true)
                    {
                        var cardForDefault = db.CreditCardUsers.Where(c => c.userId == UserId && c.creditCardUser1 != creditCardUser).OrderByDescending(c => c.creditCardUser1).FirstOrDefault();
                        if (cardForDefault != null)
                            cardForDefault.isDefault = true;
                    }
                    db.CreditCardUsers.Remove(card);
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public static void AddFavoriteAddress(long passengerId, double pickupLatitude, double pickupLongitude, string pickupAddress, double? destinationLatitude, double? destinationLongitude, string destinationAddress)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                System.Data.Entity.Spatial.DbGeography pickup_location = DbGeography.FromText(string.Format("POINT ({0} {1})", pickupLongitude.ToString().Replace(",", "."), pickupLatitude.ToString().Replace(",", ".")));
                System.Data.Entity.Spatial.DbGeography pickup_destination = null;
                if (destinationLatitude.HasValue && destinationLongitude.HasValue)
                {
                    pickup_destination = DbGeography.FromText(string.Format("POINT ({0} {1})", destinationLongitude.Value.ToString().Replace(",", "."), destinationLatitude.Value.ToString().Replace(",", ".")));
                }

                var fav = db.FavoriteAddresses.Create();
                fav.CreationDate = DateTime.UtcNow;
                fav.PickUpAddress = pickupAddress;
                fav.PassengerId = passengerId;
                fav.PickUpLocation = pickup_location;
                if (pickup_destination != null)
                {
                    fav.DestinationAddress = destinationAddress;
                    fav.DestinationLocation = pickup_destination;
                }
                db.FavoriteAddresses.Add(fav);
                db.SaveChanges();


            }
        }

        public static bool setCreditCardDefault(int creditCardId, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var card = db.CreditCardUsers.Where(c => c.creditCardUser1 == creditCardId && c.userId == userId).FirstOrDefault();
                if (card != null)
                {
                    var card2 = db.CreditCardUsers.Where(c => c.userId == userId && c.isDefault == true).FirstOrDefault();
                    if (card2 != null)
                        card2.isDefault = false;
                    card.isDefault = true;
                    db.SaveChanges();
                }
            }
            return true;
        }

        public static void RemoveFavoriteAddress(long passengerId, long favoriteIndex)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var favorite = db.FavoriteAddresses.Where(x => x.PassengerId == passengerId && x.FavoriteIndex == favoriteIndex).FirstOrDefault();
                if (favorite != null)
                {
                    db.FavoriteAddresses.Remove(favorite);
                    db.SaveChanges();
                }
                else throw new FavoriteNotExistException();
            }
        }

        public static void PassengerCancelAutoAccept(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var passenger = db.Users.GetById(userId);
                if (passenger == null)
                    throw new UserNotExistException();
                passenger.AlwaysApproveSum = false;
                db.SaveChanges();
            }
        }

        public static Dictionary<int, string> GetListOfLastStations(int languageId, long userId)
        {
            Dictionary<int, string> LastStations = new Dictionary<int, string>();
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var stationsId = db.Orders
                    .Where(o => o.PassengerId == userId).Select(t => t.Driver.TaxiStationId).ToList();
                var stations = db.TaxiStations
                    .Where(s => stationsId.Contains(s.StationId)).ToList();

                if (stations != null)
                {
                    foreach (TaxiStation Station in stations)
                    {
                        if (languageId == (int)UserLanguages.he)
                            LastStations.Add(Station.StationId, Station.HebrewName);
                        else if (languageId == (int)UserLanguages.en)
                            LastStations.Add(Station.StationId, Station.EnglishName);
                        else //default
                            LastStations.Add(Station.StationId, Station.EnglishName);
                    }
                    return LastStations;
                }
                else
                    throw new NoRelevantDataException();
            }
        }

        public static Dictionary<string, string> getSystemData()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var phone = db.SystemSettings
                    .Where(s => s.ParamKey == "RiderPhone").FirstOrDefault().ParamValue;
                var email = db.SystemSettings
                    .Where(s => s.ParamKey == "RiderEmail").FirstOrDefault().ParamValue;

                Dictionary<string, string> systemData = new Dictionary<string, string>();
                systemData["phone"] = phone;
                systemData["email"] = email;
                return systemData;
            }
        }

        public static void AddFavoriteStation(long userId, int stationID)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.Where(u => u.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    if (stationID == 0)
                        user.PreferedStationId = null;
                    else
                        user.PreferedStationId = stationID;
                    db.SaveChanges();
                }
            }
        }

        public static FavoriteStationObject GetFavoriteStation(long userId, int LanguageId)
        {
            var favoriteStationObject = new FavoriteStationObject();
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.Where(u => u.UserId == userId).FirstOrDefault();
                if (user != null)
                {
                    var favoriteStationId = user.PreferedStationId;
                    if (favoriteStationId != null)
                    {

                        favoriteStationObject.StationID = favoriteStationId.Value;
                        var taxiStatioObj = db.TaxiStations.Where(t => t.StationId == favoriteStationId).FirstOrDefault();
                        favoriteStationObject.StationName = LanguageId == (int)UserLanguages.he ? taxiStatioObj.HebrewName : taxiStatioObj.EnglishName;
                        return favoriteStationObject;
                    }

                }
            }
            return null;
        }

        public static string getLastNumbersCCForPassenger(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var row = db.CreditCardUsers.Where(c => c.userId == userId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(o => o.isDefault).FirstOrDefault();
                if (row != null)
                    return row.tokenId.Substring(row.tokenId.Length - 4);
                return null;
            }
        }

        public static bool updateLanguage(long userId, string appVersion, int languageId = 0)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.GetById(userId);

                if (user != null && appVersion != null && appVersion != "" && appVersion.Length > 0)
                {
                    user.AppVersion = appVersion;
                    db.SaveChanges();
                }
                if (user != null && languageId > 0)
                {
                    user.LanguageId = languageId;
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }
    }
}
