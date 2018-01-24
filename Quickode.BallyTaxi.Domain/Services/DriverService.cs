using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Quickode.BallyTaxi.Models;
using System.Data.Entity.Spatial;
using System.Resources;
using System.Globalization;
using System.Reflection;
using Quickode.BallyTaxi.Models.Filters;
using System.Data.Entity.SqlServer;
using Quickode.BallyTaxi.Core;
using System.Threading.Tasks;
using System.Text;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class DriverService
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Driver UpdateDriverProfile(long userId, string email, string name, string licensePlate, string taxiLicense, int cartype, bool acctepCreditCard, Guid imageId, int stationID, int chargeCCId, int bankNumber, int bankBranch, string bankAccount, string bankHolderName, string identityCardNumber, string ccProviderNumber, int? paymentMethod, string driverCode, int payment, int seatsNumber, bool? isHandicapped, int? courier, string companyNumber, int productionYear, bool? isReadTermsOfUse,bool? isPrivate,string tz,string studentCard,string authorizedDealer)
        {
            //all fields are mandatory in structure. if the field calue is empty - field will be nullified.
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Drivers
                    .Include(x => x.User)
                    .Where(x => x.UserId == userId).FirstOrDefault();
                if (driver != null)
                {
                    if (isPrivate.HasValue)
                        driver.isPrivate = isPrivate;

                    if (email == "")
                        driver.User.Email = null;
                    else driver.User.Email = email;

                    if (name == "")
                        driver.User.Name = null;
                    else driver.User.Name = name;

                    if (licensePlate == "")
                        driver.LicensePlate = null;
                    else driver.LicensePlate = licensePlate;

                    if (taxiLicense == "")
                        driver.TaxiLicense = null;
                    else driver.TaxiLicense = taxiLicense;

                    if (cartype == 0)
                        driver.CarType = null;
                    else driver.CarType = cartype;

                    driver.AcceptsCC = acctepCreditCard;//no need to check if its empty - its a bool field. can be either 0 or 1

                    //if (imageId == Guid.Empty)
                    //    driver.User.ImageId = null;
                    //else driver.User.ImageId = imageId;

                    if (stationID == 0)
                        driver.TaxiStationId = null;
                    else driver.TaxiStationId = stationID;

                    if (chargeCCId == 0)
                        driver.ChargeCCId = null;
                    else if (OrderService.IsValidCard(userId, chargeCCId))
                        driver.ChargeCCId = chargeCCId;
                    else driver.ChargeCCId = null;

                    if (bankNumber == 0)
                        driver.BankNumber = null;
                    else driver.BankNumber = bankNumber;

                    if (bankBranch == 0)
                        driver.BankBranch = null;
                    else driver.BankBranch = bankBranch;

                    if (bankAccount == "")
                        driver.BankAccount = null;
                    else driver.BankAccount = bankAccount;

                    if (bankHolderName == "")
                        driver.BankHolderName = null;
                    else driver.BankHolderName = bankHolderName;

                    if (identityCardNumber == "")
                        driver.IdentityCardNumber = null;
                    else driver.IdentityCardNumber = identityCardNumber;

                    if (ccProviderNumber == "")
                        driver.CCProviderNumber = null;
                    else driver.CCProviderNumber = ccProviderNumber;

                    if (driverCode == "")
                        driver.driverCode = null;
                    else driver.driverCode = driverCode;

                    if (payment == 0)
                        driver.payment = null;
                    else driver.payment = payment;

                    if (seatsNumber == 0)
                        driver.seats = null;
                    else driver.seats = seatsNumber;

                    if (isHandicapped == null)
                        driver.isHandicapped = null;
                    else driver.isHandicapped = isHandicapped;

                    if (courier == null)
                        driver.courier = null;
                    else driver.courier = courier;

                    if (companyNumber != null)
                        driver.companyNumber = companyNumber;

                    if (paymentMethod > 0)
                        driver.paymentMethod = paymentMethod;

                    if (productionYear > 0)
                        driver.productionYear = productionYear;

                    if (isReadTermsOfUse.HasValue)
                        driver.User.isReadTermsOfUse = isReadTermsOfUse;

                    //   if (isPrivate)
                    //   driver.isPrivate = isPrivate;
                    if (tz != null)
                        driver.tz = tz;
                    if (studentCard != null)
                        driver.studentCard = studentCard;
                    if (authorizedDealer != null)
                        driver.authorizedDealer = authorizedDealer;



                    db.SaveChanges();

                    return driver;
                }
                else throw new UserNotExistException();
            }
        }

        public static Dictionary<string, int> getDataStatistics(long userId, DateTime? dateTime)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                if(!dateTime.HasValue)
                {
                    dateTime = DateTime.UtcNow;
                }
                var orders = db.Orders.Join(db.Orders_Drivers, o => o.OrderId, od => od.OrderId, (o, od) => new { od, o })
                                    .Where(o => o.od.DriverId == userId &&  o.o.OrderTime.Value.Month == dateTime.Value.Month && o.o.OrderTime.Value.Year == dateTime.Value.Year)
                                    .ToList();

                var dicResult = new Dictionary<string, int>();
                dicResult["Completed"] = Convert.ToInt32(((orders.Where(o => o.o.DriverId == userId && (o.o.StatusId == (int)OrderStatus.Completed || (o.o.StatusId == (int)OrderStatus.Payment && o.o.Amount > 0))).Count()/(double)orders.Count))*100);
                dicResult["Cancelled"] = Convert.ToInt32(((orders.Where(o => o.o.DriverId == userId && (o.o.StatusId == (int)OrderStatus.Canceled || (o.o.StatusId == (int)OrderStatus.DriverDeclined && o.od.StatusId==(int)Order_DriverStatus.Declined))).Count() / (double)orders.Count)) * 100);
                dicResult["Missed"]= Convert.ToInt32(((orders.Where(o => o.o.DriverId != userId || o.o.StatusId==(int)OrderStatus.Dissatisfied).Count() / (double)orders.Count)) * 100);
                return dicResult;
            }
        }

        public static CarType getCarTypeById(int carTypeId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.CarTypes.Where(c => c.CarTypeId == carTypeId).FirstOrDefault();
            }
        }

        public static Driver UpdateLocation(double latitude, double longitude, long userId, double? heading)
        {
            var location = Utils.LatLongToLocation(latitude, longitude);
            //DbGeography.FromText(string.Format("POINT ({0} {1})", Longitude.ToString().Replace(",", "."), Latitude.ToString().Replace(",", ".")));

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                //TODO: Tooo oooo complex query. Check
                var driver = db.Drivers.Include(x => x.Orders_Drivers.Select(z => z.Order.Passenger.Language)).Include(x => x.User).Where(x => x.UserId == userId).FirstOrDefault();
                if (driver != null)
                {
                    driver.Location = location;
                    driver.LastUpdateLocation = DateTime.UtcNow;
                    driver.heading = heading;
                    driver.UpdateLocationStatus = (int)updateLocationStatus.updateLocation;

                    if (driver.Status == (int)DriverStatus.OnTheWayToPickupLocation)
                    {
                        Logger.Debug("Driver OnTheWayToPickupLocation");
                        //driver.Location

                        var current_order = db.Orders_Drivers
                            .ByDriver(driver.UserId)

                            .Where(x => x.StatusId == (int)Order_DriverStatus.Accepted)
                            .OrderByDescending(x => x.OrderId)
                            .Take(1)
                            .SingleOrDefault();

                        if (current_order != null)
                        {
                            var o = current_order.Order;

                            Logger.DebugFormat("Driver in order {0}. DriverLocation:{1},{2}. OrderLocation:{3},{4}",
                                current_order.OrderId,
                                o.PickUpLocation.Latitude, o.PickUpLocation.Longitude,
                                location.Latitude, location.Longitude);

                            var distance = o.PickUpLocation.Distance(driver.Location);
                            if (distance <= 200)
                            {
                                Logger.Debug("Driver is close to passenger");
                                //set status driver to inPickupLocation
                                //driver.Status = (int)DriverStatus.InPickupLocation;

                                //send push message for passenger that driver arrived to the location
                                //NotificationsServices.Current.PassengerNotification(current_order.Order.Passenger, PassengerNotificationTypes.DriverArrived, current_order.OrderId, "whistle");

                            }
                            else
                                Logger.DebugFormat("Driver is not close to passenger: {0}", distance);
                        }
                    }
                    db.SaveChanges();


                    //var dbLog = new Entities();
                    //dbLog.DataLocationDrivers.Add(new DataLocationDrivers()
                    //{
                    //    LastUpdateLocation = DateTime.UtcNow,
                    //    Location = location,
                    //    Phone = driver.User.Phone,
                    //    UserId = driver.UserId
                    //});
                    //dbLog.SaveChanges();

                    return driver;
                }

                else throw new UserNotExistException();
            }
        }

        public static void UpdateDriverToOnTheWayToPickUp(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                var driver = db.Drivers.GetById(userId);
                if (driver != null)
                {
                    driver.Status = (int)DriverStatus.OnTheWayToPickupLocation;
                    db.SaveChanges();
                }
            }
        }
        public static Driver GetDriverByUserId(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return
                db.Drivers.GetById(userId);
            }
        }

        public static Driver GetDriverByUserIdIncludeUser(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return
                    db.Drivers.Include("User").GetById(userId);
            }
        }

        public static Driver FetchDriver(long driverId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return
                    db.Drivers.GetById(driverId);
            }
        }

        public static Order GetNextOrder(long userId, long? orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                if (!orderId.HasValue)
                {
                    return db.Orders
                        .Include(x => x.Driver)
                        .Include(x => x.Passenger)
                        .Where(x => x.DriverId == userId && x.OrderTime.HasValue)
                        .OrderByDescending(x => x.OrderTime.Value)
                        .FirstOrDefault();
                }
                else
                {
                    return db.Orders
                        .Include(x => x.Driver)
                        .Include(x => x.Passenger)
                        .Where(x => x.DriverId == userId && x.OrderId == orderId.Value)
                        .FirstOrDefault();
                }
            }
        }

        public static Order ChangeDriverStatus(int status, long userId, long? orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Drivers.Include(x => x.User).Where(x => x.UserId == userId).FirstOrDefault();
                if (driver != null)
                {
                    //check valid new status according current status
                    if (driver.Status == (int)DriverStatus.Available && status == (int)DriverStatus.InPickupLocation) throw new ExpirationException();
                    if (driver.Status == (int)DriverStatus.Available && status == (int)DriverStatus.OnTheWayToDestination) throw new ExpirationException();

                    //if (driver.Status == (int)DriverStatus.OnTheWayToPickupLocation && status == (int)DriverStatus.Available) throw new ExpirationException();
                    if (driver.Status == (int)DriverStatus.OnTheWayToPickupLocation && status == (int)DriverStatus.OnTheWayToDestination) throw new ExpirationException();
                    // if (driver.Status == (int)DriverStatus.OnTheWayToPickupLocation && status == (int)DriverStatus.NotAvailable) throw new ExpirationException();

                    if (driver.Status == (int)DriverStatus.InPickupLocation && status == (int)DriverStatus.OnTheWayToPickupLocation) throw new ExpirationException();
                    // if (driver.Status == (int)DriverStatus.InPickupLocation && status == (int)DriverStatus.NotAvailable) throw new ExpirationException();

                    if (driver.Status == (int)DriverStatus.OnTheWayToDestination && status == (int)DriverStatus.InPickupLocation) throw new ExpirationException();
                    if (driver.Status == (int)DriverStatus.OnTheWayToDestination && status == (int)DriverStatus.OnTheWayToPickupLocation) throw new ExpirationException();

                    if (orderId.HasValue && orderId != 0)
                    {
                        var order = db.Orders.Where(x => x.OrderId == orderId.Value).FirstOrDefault();
                        if (order != null)
                        {
                            if (order.StatusId == (int)OrderStatus.DriverDeclined && order.DriverId == null)
                                throw new OrderNotRelevantException();
                        }
                    }

                    //this ride Completed
                    if (orderId.HasValue && orderId != 0)
                    {
                        if (status == (int)DriverStatus.OnTheWayToDestination) //|| (driver.Status == (int)DriverStatus.InPickupLocation && status == (int)DriverStatus.Available))
                        {
                            var order = db.Orders.Where(x => x.OrderId == orderId.Value).FirstOrDefault();
                            if (order != null)
                            {
                                if (status == (int)DriverStatus.OnTheWayToDestination)
                                    order.StatusId = (int)OrderStatus.Payment;
                                //else
                                //    order.StatusId = (int)OrderStatus.Completed;
                            }
                            else throw new OrderNotFoundException();
                        }
                    }

                    driver.Status = status;
                    db.SaveChanges();

                    //Driver at location
                    if (driver.Status == (int)DriverStatus.InPickupLocation)
                    {
                        Order order;
                        if (orderId.HasValue)
                            order = db.Orders.Where(x => x.OrderId == orderId.Value).FirstOrDefault();
                        else
                            order = db.Orders.ByDriver(userId).GetByStatusId((int)OrderStatus.Confirmed).OrderByDescending(x => x.OrderId).Take(1).SingleOrDefault();
                        if (order != null)
                        {
                            //send push message for passenger that driver arrived to the location
                            if (order.isFromWeb == true && order.Passenger.DeviceId == null)
                            {
                                bool status1 = UserService.SendSMSNotif(order.PassengerId, order.Passenger.Phone, SMSType.DriverArrived, order.Passenger.LanguageId);
                            }
                            NotificationsServices.Current.PassengerNotification(order.Passenger, PassengerNotificationTypes.DriverArrived, order.OrderId, "whistle");
                            // NotificationsServices.Current.PassengerNotification(order.Passenger, PassengerNotificationTypes.DriverArrived, order.OrderId, "whistle");
                        }
                    }

                    new Task(() => { changeRegionForDriver(userId); }).Start();

                    //return the closest ride of this driver
                    if (status == (int)DriverStatus.Available)
                    {
                        return null; // GetNextOrder(userId,null);
                    }
                    //return the current ride of this driver
                    else
                    {
                        return GetNextOrder(userId, orderId);
                    }
                }
                else throw new UserNotExistException();
            }
        }

        public static void DriverWantFutureRide(bool status, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Drivers.Include(x => x.User).Where(x => x.UserId == userId).FirstOrDefault();
                if (driver != null)
                {
                    driver.WantFutureRide = status;
                    db.SaveChanges();
                }
                else throw new UserNotExistException();
            }
        }

        public static List<object> GetAvailableTaxis(double Latitude, double Longitude, long orderId)
        {
            var location = Utils.LatLongToLocation(Latitude, Longitude);
            //Logger.DebugFormat("location to show: lat: {0}, long: {1}", Latitude, Longitude);
            //System.Data.Entity.Spatial.DbGeography location = DbGeography.FromText(string.Format("POINT ({0} {1})", Longitude.ToString().Replace(",", "."), Latitude.ToString().Replace(",", ".")));
            var UtcNow = DateTime.UtcNow;
            var available_hours_range = int.Parse(ConfigurationHelper.AVAILABLE_HOURS_RANGE);//3 hours
           // var beforeSomeHours = UtcNow.AddHours(-available_hours_range);
            var beforeFewMinutes =  DateTime.UtcNow.AddMinutes(-(ConfigurationHelper.UPDATE_MINUTES / 2) + 1);


            List<object> availableTaxis = new List<object>();
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var drivers = new List<Driver>();
                if (orderId > 0)
                {
                    var order = db.Orders.GetById(orderId);
                    if (order != null && order.DriverId.HasValue)
                    {
                        drivers = db.Drivers.Include(x => x.User).Where(d => d.UserId == order.DriverId).ToList();
                    }
                }
                if (drivers == null || drivers.Count == 0)
                {
                    //give all driver with status "available" and location in radius of 5000 meter from current location and their location is updated on helf hour range from now
                    //int available_radius = ConfigurationHelper.AVAILABLE_RADIUS_MAX;
                    int available_radius = ConfigurationHelper.AVAILABLE_RADIUS_500;
                    //change by Shoshana on 17/01/18
                    //drivers = db.Drivers.Include(x => x.User).Where(x => x.Location.Distance(location) <= available_radius && (x.LastUpdateLocation >= beforeSomeHours && x.LastUpdateLocation <= UtcNow)).ToList();
                    drivers = db.Drivers.Include(x => x.User).Where(x => x.Location.Distance(location) <= available_radius && (x.LastUpdateLocation >= beforeFewMinutes && x.LastUpdateLocation <= UtcNow)).ToList();
                }
                if (drivers != null)
                {
                    foreach (var item in drivers)
                    {
                        availableTaxis.Add(new { lat = item.Location.Latitude, lon = item.Location.Longitude, imageID = item.User.ImageId, userId = item.UserId, heading=(item.heading.HasValue? item.heading.Value: 0) });
                    }
                }
                return availableTaxis;
            }
        }

        public static object UpdateLocation(object lat, object lon, long userId, object heading)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<int, string> GetListOfStations(int languageId)
        {
            Dictionary<int, string> AllStations = new Dictionary<int, string>();
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var stations = db.TaxiStations.ToList();
                if (stations != null)
                {
                    foreach (TaxiStation Station in stations)
                    {
                        if (languageId == (int)UserLanguages.he)
                            AllStations.Add(Station.StationId, Station.HebrewName);
                        else if (languageId == (int)UserLanguages.en)
                            AllStations.Add(Station.StationId, Station.EnglishName);
                        else //default
                            AllStations.Add(Station.StationId, Station.EnglishName);
                    }
                    return AllStations;
                }
                else
                    throw new NoRelevantDataException();
            }
        }

        public static string getRegionForDriver(long userId, int languageId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var regionDriver = db.DriverRegions.Where(d => d.driverId == userId).FirstOrDefault();
                if (regionDriver != null)
                {
                    var userCulture = NotificationsServices.Current.GetLanguageCulture(languageId);

                    var regionForDrivers = db.DriverRegions.Where(d => d.regionId == regionDriver.regionId).OrderBy(r => r.driverRegionId).ToArray();
                    var number = Array.IndexOf(regionForDrivers, regionDriver);
                    number++;
                    var stringRegion = string.Format(Utils.TranslateMessage(userCulture, "RegionStringForDriver"), regionDriver.Region.name, number + "/" + regionForDrivers.Count());
                    return stringRegion;
                }
            }
            return "";
        }

        public static TaxiStation GetTaxiStationById(int taxiStationID)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.TaxiStations.GetById(taxiStationID);
            }
        }

        public static User Login(long userId, string appVersion, int languageId = 0)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                int free_trial_days = ConfigurationHelper.FREE_TRIAL_DAYS;
                var user = db.Users.GetById(userId);
                if (user != null)
                {
                    if (languageId > 0)
                    {
                        user.LanguageId = languageId;
                        db.SaveChanges();
                    }
                    if (appVersion != null && appVersion != "" && appVersion.Length > 0)
                    {
                        user.AppVersion = appVersion;
                        db.SaveChanges();
                    }
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

        public static void changeRegionForDriver(long userId)
        {
            changeRegionForDriverFunction(userId);
        }

        public static void changeRegionForDriverFunction(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                //Logger.DebugFormat("start changeRegionForDriverFunction for driver: {0}", userId);
                var driver = db.Users.Include("Driver").GetById(userId);
                if (driver != null && driver.Driver != null)
                {
                    if (driver.Driver.Status > (int)DriverStatus.HasRequest)//he is not available and dont want orders!!
                    {
                        var regionOfDriverA = db.Regions.Where(d => d.DriverRegions.Select(r => r.driverId).Contains(driver.UserId)).FirstOrDefault();
                        if (regionOfDriverA != null)
                        {
                            db.DriverRegions.Remove(regionOfDriverA.DriverRegions.Where(t => t.driverId == driver.UserId).FirstOrDefault());
                            db.SaveChanges();
                        }
                        return;
                    }

                    //check if the driver in the same region he already there:
                    var regionOfDriver = db.Regions.Where(d => d.DriverRegions.Select(r => r.driverId).Contains(driver.UserId)).FirstOrDefault();
                    if (regionOfDriver != null)
                    {
                        if (driver.Driver.TaxiStationId != regionOfDriver.taxiStationId)
                        {
                            db.DriverRegions.Remove(regionOfDriver.DriverRegions.Where(t => t.driverId == driver.UserId).FirstOrDefault());
                            db.SaveChanges();
                        }
                        Logger.DebugFormat("the region for driver: {0} is: {1}", userId, regionOfDriver.regionId);
                        var list = regionOfDriver.LocationForRegions.OrderByDescending(e => e.locationId).Select(s => s.location).ToList();
                        list.Add(regionOfDriver.LocationForRegions.OrderByDescending(e => e.locationId).Select(s => s.location).FirstOrDefault());
                        var resultPolygon = ConvertGeoCoordinatesToPolygon(list.AsEnumerable());
                        var result = driver.Driver.Location.Intersects(resultPolygon);
                        if (result == true)
                        {
                            Logger.DebugFormat("the driver: {0} is in the same region : {1}", userId, regionOfDriver.regionId);
                            return;
                        }
                        else
                        {
                            db.DriverRegions.Remove(regionOfDriver.DriverRegions.Where(t => t.driverId == driver.UserId).FirstOrDefault());
                            db.SaveChanges();
                        }
                    }
                    var listRegion = db.Regions.Where(t => t.taxiStationId == driver.Driver.TaxiStationId).OrderByDescending(e => e.regionId).ToList();
                    if (regionOfDriver != null)
                    {
                        listRegion = listRegion.Where(t => t.regionId != regionOfDriver.regionId).ToList();
                    }
                    //Logger.DebugFormat("search the current region for driver: {0}", userId);
                    foreach (var region in listRegion)
                    {
                        var list = region.LocationForRegions.OrderByDescending(e => e.locationId).Select(s => s.location).ToList();
                        list.Add(region.LocationForRegions.OrderByDescending(e => e.locationId).Select(s => s.location).FirstOrDefault());
                        var resultPolygon = ConvertGeoCoordinatesToPolygon(list.AsEnumerable());
                        //var PolygonFromMultiplePoints = DbGeography.FromBinary(region.LocationForRegions.OrderByDescending(t => t.locationId).FirstOrDefault().location.AsBinary());
                        //var first = region.LocationForRegions.OrderByDescending(t=>t.locationId).FirstOrDefault();
                        // foreach (var item in region.LocationForRegions/*.Where(r => r.locationId != first.locationId).OrderByDescending(t => t.locationId)*/.ToList())
                        // {

                        //PolygonFromMultiplePoints.Union(item.location);
                        //var polygon = DbGeography.PointFrom(region.LocationForRegions.Toa().Select(s => s.location), 4326)
                        //}
                        //PolygonFromMultiplePoints.Union(first.location);
                        //var temp_multipointgeometry = DbGeometry.MultiPointFromBinary(PolygonFromMultiplePoints.AsBinary(), DbGeometry.DefaultCoordinateSystemId);
                        //PolygonFromMultiplePoints = DbGeography.PolygonFromBinary(temp_multipointgeometry.ConvexHull.AsBinary(), DbGeography.DefaultCoordinateSystemId);
                        var result = driver.Driver.Location.Intersects(resultPolygon);
                        if (result == true)
                        {

                            var driverInLocation = db.DriverRegions.Where(d => d.driverId == driver.UserId).FirstOrDefault();
                            if (driverInLocation != null)
                            {

                                if (driverInLocation.regionId != region.regionId)
                                {
                                    db.DriverRegions.Remove(driverInLocation);

                                }
                                else
                                {
                                    return;
                                }

                            }
                            db.DriverRegions.Create();
                            var newDriverLocation = new DriverRegion() { driverId = driver.UserId, regionId = region.regionId };
                            db.DriverRegions.Add(newDriverLocation);
                            db.SaveChanges();
                            //Logger.DebugFormat("the functio success for driver: {0} ", userId);
                            return;
                        }
                    }

                }
            }
        }

        public static DbGeography ConvertGeoCoordinatesToPolygon(IEnumerable<DbGeography> coordinates)
        {
            var coordinateList = coordinates.ToList();
            if (coordinateList.First() != coordinateList.Last())
            {
                throw new Exception("First and last point do not match. This is not a valid polygon");
            }

            var count = 0;
            var sb = new StringBuilder();
            sb.Append(@"POLYGON((");
            foreach (var coordinate in coordinateList)
            {
                if (count == 0)
                {
                    sb.Append(coordinate.Longitude + " " + coordinate.Latitude);
                }
                else
                {
                    sb.Append("," + coordinate.Longitude + " " + coordinate.Latitude);
                }

                count++;
            }

            sb.Append(@"))");

            return DbGeography.PolygonFromText(sb.ToString(), 4326);
        }


        public static Dictionary<string, object> GetTravelHistory(long userId, DateTime date)
        {
            var result = new Dictionary<string, object>();
            var month = date.Month;
            var year = date.Year;
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var travelNumber = db.Orders.Where(o => o.DriverId == userId && o.OrderTime.HasValue && o.OrderTime.Value.Month == month && o.OrderTime.Value.Year == year && (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount.HasValue))).Count();
                var paymentAmount = db.Orders.Where(o => o.DriverId == userId && o.OrderTime.HasValue && o.OrderTime.Value.Month == month && o.OrderTime.Value.Year == year && (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount.HasValue))).Sum(o => o.Amount);
                double monthlyDrive = 0;
                var orders = db.Orders.Where(o => o.DriverId == userId && o.OrderTime.HasValue && o.OrderTime.Value.Month == month && o.OrderTime.Value.Year == year && (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount.HasValue))).ToList();
                foreach (var order in orders)
                {
                    if (order.DestinationLocation.Latitude != 0 && order.DestinationLocation.Longitude != 0)
                    {
                        var locA = new System.Device.Location.GeoCoordinate((double)order.PickUpLocation.Latitude, (double)order.PickUpLocation.Longitude);
                        var locB = new System.Device.Location.GeoCoordinate((double)order.DestinationLocation.Latitude, (double)order.DestinationLocation.Longitude);
                        double distance = locA.GetDistanceTo(locB); // metres

                        distance = distance / 1000;// km
                        monthlyDrive += distance;
                    }
                }
                var hoursDriveInMinutes = db.Orders.Where(o => o.DriverId == userId && o.OrderTime.HasValue && o.OrderTime.Value.Month == month && o.OrderTime.Value.Year == year && (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount.HasValue)) && o.EndTime.HasValue).Select(r => SqlFunctions.DateDiff("minute", r.OrderTime.Value, r.EndTime.Value)).Sum(); //DbFunctions.DiffHours(o.ClientDateTimeStamp, clientDateTime)
                double? hoursDrive = 0;
                if (hoursDriveInMinutes.HasValue)
                    hoursDrive = Convert.ToDouble(hoursDriveInMinutes) / 60;
                //!!!!
                var travelCancellation = db.Orders.Where(o => o.DriverId == userId && o.CreationDate.Month == month && o.CreationDate.Year == year && o.StatusId == (int)OrderStatus.Canceled).Count(); ;
                var paymentByPayPal = db.Orders.Where(o => o.DriverId == userId && o.CreationDate.Month == month && o.CreationDate.Year == year && (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount.HasValue)) && (o.PaymentMethod == (int)CustomerPaymentMethod.Paypal || o.PaymentMethod == (int)CustomerPaymentMethod.CreditCard)).Select(o => o.Amount).Sum();
                var paymentByBusiness = db.Orders.Where(o => o.DriverId == userId && o.CreationDate.Month == month && o.CreationDate.Year == year && (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount.HasValue)) && o.PaymentMethod == (int)CustomerPaymentMethod.Business).Select(o => o.Amount).Sum();
                var paymentByCash = db.Orders.Where(o => o.DriverId == userId && o.CreationDate.Month == month && o.CreationDate.Year == year && (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount.HasValue)) && o.PaymentMethod == (int)CustomerPaymentMethod.Cash).Select(o => o.Amount).Sum();

                result["travelNumber"] = travelNumber;
                result["paymentAmount"] = paymentAmount == null ? 0 : Math.Round((Convert.ToDecimal(paymentAmount)), 2);
                result["monthlyDrive"] = Math.Round((Convert.ToDecimal(monthlyDrive)), 2);
                result["travelCancellation"] = travelCancellation;
                result["paymentByPayPal"] = paymentByPayPal == null ? 0 : Math.Round((Convert.ToDecimal(paymentByPayPal)), 2);
                result["paymentByBusiness"] = paymentByBusiness == null ? 0 : Math.Round((Convert.ToDecimal(paymentByBusiness)), 2);
                result["paymentByCash"] = paymentByCash == null ? 0 : Math.Round((Convert.ToDecimal(paymentByCash)), 2);
                result["hoursDrive"] = hoursDrive == null ? 0 : Math.Round((Convert.ToDecimal(hoursDrive)), 2);

                return result;
            }


        }

        public static int? setDriverToAvailable(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.Include("Driver").GetById(userId);
                if (user != null)
                {
                    if (user.Driver.Status == (int)DriverStatus.NotAvailable)
                    {
                        user.Driver.Status = (int)DriverStatus.Available;
                        db.SaveChanges();
                        return (int)DriverStatus.Available;
                    }
                }
                return null;
            }
        }

        public static string[] getStringsForPayment(int LanguageId)
        {
            string[] str = new string[3];
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                if (LanguageId == (int)UserLanguages.he)
                {
                    str[0] = db.SystemSettings.Where(s => s.ParamKey == "paymentForMonth_Hebrew").FirstOrDefault().ParamValue;
                    str[1] = db.SystemSettings.Where(s => s.ParamKey == "paymentForRide_Hebrew").FirstOrDefault().ParamValue;
                    str[2] = db.SystemSettings.Where(s => s.ParamKey == "paymentForMonthNew_Hebrew").FirstOrDefault().ParamValue;
                }
                else
                {
                    str[0] = db.SystemSettings.Where(s => s.ParamKey == "paymentForMonth_English").FirstOrDefault().ParamValue;
                    str[1] = db.SystemSettings.Where(s => s.ParamKey == "paymentForRide_English").FirstOrDefault().ParamValue;
                    str[2] = db.SystemSettings.Where(s => s.ParamKey == "paymentForMonthNew_English").FirstOrDefault().ParamValue;
                }
            }
            return str;
        }

        public static Dictionary<int, string> GetCarTypeList(int languageId)
        {
            Dictionary<int, string> AllCarType = new Dictionary<int, string>();
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var CarTypes = db.CarTypes.ToList();
                if (CarTypes != null)
                {
                    foreach (CarType carType in CarTypes)
                    {
                        if (languageId == (int)UserLanguages.he)
                            AllCarType.Add(carType.CarTypeId, carType.HebrewName);
                        else if (languageId == (int)UserLanguages.en)
                            AllCarType.Add(carType.CarTypeId, carType.EnglishName);
                        else //default
                            AllCarType.Add(carType.CarTypeId, carType.EnglishName);
                    }
                    return AllCarType;
                }
                else
                    throw new NoRelevantDataException();
            }
        }

        public static DriverToPrint getParamsToPrint(long userId, long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var drivertoPrint = new DriverToPrint();
                var order = db.Orders.Where(o => o.OrderId == orderId && o.DriverId == userId).FirstOrDefault();
                var driver = db.Drivers.GetById(userId);
                if (order != null && driver != null)
                {
                    double distance = 0;
                    if (order.DestinationLocation.Latitude != 0 && order.DestinationLocation.Longitude != 0)
                    {
                        var locA = new System.Device.Location.GeoCoordinate((double)order.PickUpLocation.Latitude, (double)order.PickUpLocation.Longitude);
                        var locB = new System.Device.Location.GeoCoordinate((double)order.DestinationLocation.Latitude, (double)order.DestinationLocation.Longitude);
                        distance = locA.GetDistanceTo(locB); // metres

                    }
                    distance = distance / 1000;


                    drivertoPrint.companyNumber = driver.companyNumber;
                    drivertoPrint.startTime = order.OrderTime.Value.ConvertToUnixTimestamp();
                    drivertoPrint.endTime = order.EndTime.HasValue ? order.EndTime.Value.ConvertToUnixTimestamp() : 0;
                    drivertoPrint.distance = Convert.ToDouble(Math.Round(Convert.ToDecimal(distance), 2));
                }
                return drivertoPrint;
            }
        }

        public static int getRatingForDriver(long userId)
        {
            double? rating = 0;
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                rating = db.Orders.Where(r => r.Rating > 0 && r.DriverId == userId).ToList().Average(o => o.Rating);
                if (rating == null)
                    rating = 0;
            }
            return Convert.ToInt32(Math.Round(Convert.ToDecimal(rating)));
        }
    }
}