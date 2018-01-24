using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using Quickode.BallyTaxi.Models;
using System.Data.Entity.Spatial;
using System.Net;
using System.IO;
using Quickode.BallyTaxi.Models.Models;
using Newtonsoft.Json;
using System.Resources;
using System.Globalization;
using System.Reflection;
using Quickode.BallyTaxi.Models.Filters;
using System.Data.Entity.Validation;
using Quickode.BallyTaxi.Core;
using System.Net.Http;
using System.Web.Script.Serialization;
using Quickode.BallyTaxi.Integrations.Twilio;
using System.Threading.Tasks;
using System.Xml;
using System.Timers;
using Newtonsoft.Json.Linq;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class OrderService
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static double getInterCityPrice(LocationForIntercityTravel locationObj)
        {
            //try to get the price from db:

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                //add it to be in Hebrew:
                //&language=he
                //Dictionary<string, double> result = new Dictionary<string, double>();
                var pickUpCity = getCityfromLocation(locationObj.lat, locationObj.lon);
                var destinationCity = getCityfromLocation(locationObj.destinationLatitude, locationObj.lat);

                double priceInterCity = 0;
                var row = db.PriceBetweenCities
                    .Where(c => c.Cities.Contains(locationObj.destinationCityName) && c.Cities.Contains(locationObj.pickUpCityName))
                    .FirstOrDefault();
                if (row == null)
                {
                    row = db.PriceBetweenCities
                    .Where(c => c.Cities.Contains(pickUpCity) && c.Cities.Contains(destinationCity))
                    .FirstOrDefault();
                }
                if (row != null)
                {
                    var price = DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay < TimeSpan.Parse("05:00") ? row.PriceOnNight : row.PriceOnDay;
                    //if (price > 0)
                    //{
                    priceInterCity = price;
                    //}

                    //var basicDiscount = db.SystemSettings
                    //   .Where(x => x.ParamKey == "BasicDiscountForIntercityTravel")
                    //   .FirstOrDefault().ParamValue;
                    ////price after discount:
                    //result["priceAfterDiscount"] = result["priceBeforeDiscount"] * (1 - Convert.ToDouble(basicDiscount));
                    priceInterCity = Math.Ceiling(priceInterCity);
                    priceInterCity = Math.Ceiling(priceInterCity);
                }
                return priceInterCity;

            }
        }

        public static long CreateOrder(double pickupLatitude, double pickupLongitude, string pickupAddress, double destinationLatitude, double destinationLongitude, string destinationAddress, long userId, string notes, DateTime ordertime, bool? isInterCity, string pickUpCityName, string destinationCityName, string fileNumber, bool isFromWeb, int businessId, bool isWithDiscount, int seatsNumber, int? courier, bool? isHandicapped, long accountId, bool isFromStations, int roleId, int paymentMethod, bool isIVROrder = false)
        {
            var pickup_location = Utils.LatLongToLocation(pickupLatitude, pickupLongitude);
            var destination_location = Utils.LatLongToLocation(destinationLatitude, destinationLongitude);
            var station = getStationByRole(roleId);
            //DbGeography pickup_location = DbGeography.FromText(string.Format("POINT ({0} {1})", pickupLongitude.ToString().Replace(",", "."), pickupLatitude.ToString().Replace(",", ".")));
            //DbGeography destination_location = DbGeography.FromText(string.Format("POINT ({0} {1})", destinationLongitude.ToString().Replace(",", "."), destinationLatitude.ToString().Replace(",", ".")));
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    // check if exist any driver in 20 km around the pickup location
                    //var available_distance = ConfigurationHelper.AVAILABLE_DISTANCE;
                    //var passenger = db.Users.Where(u => u.UserId == userId).FirstOrDefault();
                    //passenger.PreferredPaymentMethod = paymentMethod;
                    // bool has_drivers = db.Drivers.Any(x => x.Location.Distance(pickup_location) <= available_distance);
                    //if (has_drivers)
                    //{
                    var order = db.Orders.Create();
                    order.CreationDate = DateTime.UtcNow;
                    order.FlowStep = (int)FlowSteps.Step1;
                    order.LastUpdateFlowStep = DateTime.UtcNow;
                    order.PickUpAddress = pickupAddress;
                    order.PickUpLocation = pickup_location;
                    order.DestinationAddress = destinationAddress;
                    order.DestinationLocation = destination_location;
                    order.StatusId = (int)OrderStatus.Pending;
                    order.OrderTime = ordertime;
                    order.Notes = notes;
                    order.PassengerId = userId;
                    //delete the next line?
                    order.PaymentMethod = paymentMethod;
                    order.isInterCity = isInterCity;
                    order.pickUpCityName = pickUpCityName;
                    order.destinationCityName = destinationCityName;
                    order.FileNumber = fileNumber;
                    order.isFromWeb = isFromWeb;
                    order.isWithDiscount = isWithDiscount;
                    order.seats = seatsNumber;
                    order.courier = courier;
                    order.isHandicapped = isHandicapped;
                    order.orderCount = 1;
                    order.isFromStations = isFromStations;
                    order.PreferedStationId = station;
                    order.isIVROrder = isIVROrder;
                    //  if (businessId > 0 && paymentMethod == (int)CustomerPaymentMethod.Business)
                    //edited by Shoshana on 16-01-2018
                    if (businessId > 0)
                    {
                        order.businessId = businessId;
                        if (accountId > 0)
                            order.AccountId = accountId;
                    }
                    if (isWithDiscount == true)
                    {
                        var model = new LocationForIntercityTravel();

                        model.destinationCityName = order.destinationCityName;
                        model.destinationLatitude = order.DestinationLocation.Latitude.Value;
                        model.destinationLongitude = order.DestinationLocation.Longitude.Value;
                        model.lat = order.PickUpLocation.Latitude.Value;
                        model.lon = order.PickUpLocation.Longitude.Value;
                        model.pickUpCityName = order.pickUpCityName;
                        var result = CalcPriceIntercityTravel(model);
                        if (result != null && result.Count > 0)
                            order.priceForInterCity = result["priceBeforeDiscount"];
                    }
                    db.Orders.Add(order);
                    db.SaveChanges();

                    if (order.DestinationLocation.Latitude != 0 && order.DestinationLocation.Longitude != 0 && order.PickUpLocation.Latitude != 0 && order.PickUpLocation.Longitude != 0)
                    {
                        new Task(() => { updateIsInterCityForPayment(order.OrderId, order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, order.DestinationLocation.Latitude.Value, order.DestinationLocation.Longitude.Value); }).Start();
                    }

                    if (order.isFromStations == true)
                    {
                        new Task(() => { searchDriverByRegion(order, roleId); }).Start();
                    }
                    else
                    {
                        new Task(() => { HandleSearchDrivers(order, roleId); }).Start();
                    }
                    //TODO run background service

                    return order.OrderId;
                    // }
                    //db.SaveChanges();
                }
            }
            catch (DbEntityValidationException e)
            {
                LogValidationErrors(e);
            }
            catch (Exception exc)
            {
                Logger.ErrorFormat("Problems generating AD scripts - {0}", exc.Message);
            }
            throw new NoRelevantDataException();
        }

        public static void updateIsInterCityForPayment(long orderId, double pickupLat, double pickupLon, double destLat, double destLon)
        {
            var pickupCity = getCityFromLatLong(pickupLat, pickupLon);
            var destCity = getCityFromLatLong(destLat, destLon);
            if (pickupCity != null && destCity != null && pickupCity != destCity)
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var order = db.Orders.GetById(orderId);
                    if (order != null)
                    {
                        order.isInterCityForPayment = true;
                        db.SaveChanges();
                    }
                }
            }

        }

        public static string getCityFromLatLong(double lat, double lon)
        {
            string lang = "en";

            XmlDocument doc = new XmlDocument();
            try
            {
                var Address = "";
                //var strings = new string[2];
                string Address_LongName = "";
                doc.Load("http://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lon + "&sensor=false&language=" + lang);
                XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                if (element.InnerText == "ZERO_RESULTS")
                {
                    return null;
                }
                else
                {
                    //element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");
                    //Address = element.InnerText;
                    //if (Address.Contains(","))
                    //{
                    //    String[] separated = Address.Split(',');
                    //    Address = separated[0];
                    //    Address += separated[1];
                    //}
                    //strings[0] = Address;
                    string city = null;

                    string longname = "";
                    string shortname = "";
                    string typename = "";

                    XmlNodeList xnList = doc.SelectNodes("//GeocodeResponse/result/address_component");
                    foreach (XmlNode xn in xnList)
                    {
                        longname = xn["long_name"].InnerText;
                        shortname = xn["short_name"].InnerText;
                        typename = xn["type"].InnerText;

                        switch (typename)
                        {
                            //Add whatever you are looking for below
                            case "political":
                                {
                                    // var Address_country = longname;
                                    //Address_LongName != "" ? longname : Address_LongName;//
                                    city = longname;
                                    break;
                                }
                            case "locality":
                                {
                                    // var Address_country = longname;
                                    //Address_LongName  != "" ?  longname: Address_LongName;//
                                    city = longname;
                                    break;
                                }
                            //case "country":
                            //    {
                            //        strings[2] = shortname;
                            //        break;
                            //    }
                            default:
                                break;
                        }
                        if (city != null)
                            break;
                    }
                    return city;
                }


            }
            catch (Exception)
            {

                return null;
            }
        }

        public static int? getStationByRole(int roleId)
        {
            switch (roleId)
            {
                case (int)RoleAccountForAdmin.admin:
                    return null;
                case (int)RoleAccountForAdmin.kastle:
                    return ConfigurationHelperForStations.KasstleTaxi;
                case (int)RoleAccountForAdmin.shekem:
                    return ConfigurationHelperForStations.ShekemTaxi;
                default:
                    return null;
            }
        }

        private static void searchDriverByRegion(Order order, int roleId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var station = getStationByRole(roleId);
                bool flag = false;
                foreach (var region in db.Regions.Where(t => station > 0 ? t.taxiStationId == station : 1 == 1).OrderByDescending(e => e.regionId).ToList())
                {
                    var list = region.LocationForRegions.OrderByDescending(e => e.locationId).Select(s => s.location).ToList();
                    list.Add(region.LocationForRegions.OrderByDescending(e => e.locationId).Select(s => s.location).FirstOrDefault());
                    var resultPolygon = DriverService.ConvertGeoCoordinatesToPolygon(list.AsEnumerable());
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
                    var result = order.PickUpLocation.Intersects(resultPolygon);
                    if (result == true)
                    {
                        flag = true;
                        reCreateOrderToSpecificRegion(region.regionId, order.OrderId, roleId);
                        break;
                    }
                }
                //no region found:
                if (flag == false)
                    HandleSearchDrivers(order, roleId);
            }


        }

        public static long reCreateOrder(long userId, long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                Order order = null;
                if (orderId > 0)
                {
                    order = db.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();

                }
                else
                {
                    var dateBEfore12Hours = DateTime.UtcNow.AddHours(-12);
                    order = db.Orders.Where(o => o.PassengerId == userId && o.StatusId == (int)OrderStatus.Dissatisfied && o.OrderTime > dateBEfore12Hours)
                        .OrderByDescending(o => o.CreationDate).FirstOrDefault();
                }
                if (order != null)
                {
                    if (order.orderCount.HasValue && order.orderCount == 3)
                    {
                        Logger.ErrorFormat("you cannot recreate order: {0} because it created already 3 times for passenger: {1}", orderId, userId);
                        throw new orderCannotReCreatedException();
                    }
                    if (order.PassengerId != userId)
                    {
                        Logger.ErrorFormat("this user is not the user that create this orderId: {0} userId: {1}, passengerId: {2}", order.OrderId, userId, order.PassengerId);
                        throw new UserForbiddenException();
                    }
                    var dateNow = DateTime.UtcNow;

                    order.CreationDate = DateTime.UtcNow;
                    order.FlowStep = (int)FlowSteps.Step1;
                    order.LastUpdateFlowStep = DateTime.UtcNow;
                    order.StatusId = (int)OrderStatus.Pending;
                    order.orderCount = order.orderCount.HasValue ? order.orderCount + 1 : 2;
                    var orderDriver = order.Orders_Drivers.ToList();
                    orderDriver.ForEach(t => t.isReadTheOrderForDriver = t.isReadTheOrderForDriver == true ? t.isReadTheOrderForDriver = false : t.isReadTheOrderForDriver = null);
                    db.SaveChanges();
                    new Task(() => { HandleSearchDrivers(order); }).Start();
                    return order.OrderId;
                }
                throw new OrderNotFoundException();
            }
        }

        public static Order CreateVirtualOrder(double pickupLatitude, double pickupLongitude, string pickupAddress, long DriverId, DateTime orderDateTime, int paymentMethod, string pickUpCityName, long PassengerId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                var pickup_location = Utils.LatLongToLocation(pickupLatitude, pickupLongitude);
                var destLocation = Utils.LatLongToLocation(0, 0);

                var order = db.Orders.Create();
                order.CreationDate = DateTime.UtcNow;
                order.PickUpAddress = pickupAddress;//pickupAddress;
                order.PickUpLocation = pickup_location;
                order.DestinationLocation = destLocation;
                order.StatusId = (int)OrderStatus.Confirmed;
                order.OrderTime = orderDateTime;
                order.DriverId = DriverId;
                order.FlowStep = (int)FlowSteps.Step3;
                order.LastUpdateFlowStep = DateTime.UtcNow;
                order.PassengerId = PassengerId;
                order.PaymentMethod = paymentMethod;
                order.pickUpCityName = pickUpCityName;//pickUpCityName;
                order.isVirtual = true;

                db.Orders.Add(order);
                db.SaveChanges();

                var driver = db.Drivers.GetById(DriverId);
                driver.Status = (int)DriverStatus.InPickupLocation;
                db.SaveChanges();

                var orderDriver = db.Orders_Drivers.Create();
                orderDriver.DriverId = DriverId;
                orderDriver.OrderId = order.OrderId;
                orderDriver.StatusId = (int)Order_DriverStatus.Accepted;
                db.Orders_Drivers.Add(orderDriver);
                db.SaveChanges();
                return order;
            }
        }

        public static bool updateOrderIfRead(long userId, int userType, long orderID)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                if (orderID > 0 && userType == (int)UserType.Driver)
                {

                    var orderD = db.Orders.GetById(orderID);

                    if ((orderD.DriverId.HasValue && orderD.DriverId.Value != userId) || orderD.StatusId == (int)OrderStatus.Pending)
                    {
                        var orderDriver = db.Orders_Drivers.Where(d => d.DriverId == userId && d.OrderId == orderD.OrderId).FirstOrDefault();
                        if (orderDriver != null)
                        {
                            orderDriver.isReadTheOrderForDriver = true;
                            db.SaveChanges();
                            return true;
                        }
                    }
                    //else

                    //orderD.isReadTheOrderForDriver = true;
                    //db.SaveChanges();
                    return false;
                }

                // var driverUser = db.Drivers.GetById(userId);
                var datenow = DateTime.UtcNow.AddMinutes(1);
                var dateBEfore12Hours = DateTime.UtcNow.AddHours(-12);

                var order = new Order();
                if (userType == (int)UserType.Driver)
                {
                    order = db.Orders.Where(o => (o.DriverId == userId) && order.StatusId != (int)OrderStatus.DriverDeclined && (o.OrderTime.Value > datenow) && o.isReadTheOrderForDriver != true)
                    .OrderByDescending(o => o.OrderTime).FirstOrDefault();
                    if (order == null)
                    {
                        order = db.Orders.Where(o => (o.DriverId == userId) && (o.OrderTime.Value <= datenow) && o.OrderTime > dateBEfore12Hours && o.isReadTheOrderForDriver != true)
                                        .OrderByDescending(o => o.CreationDate).FirstOrDefault();
                    }
                }

                else if (userType == (int)UserType.Passenger)
                {
                    order = db.Orders.Where(o => (o.PassengerId == userId) && order.StatusId != (int)OrderStatus.Canceled && (o.OrderTime.Value > datenow) && o.isReadTheOrder != true)
                    .OrderByDescending(o => o.OrderTime).FirstOrDefault();
                    if (order == null)
                    {
                        order = db.Orders.Where(o => (o.PassengerId == userId) && (o.OrderTime.Value <= datenow) && o.OrderTime > dateBEfore12Hours && o.isReadTheOrder != true)
                                        .OrderByDescending(o => o.CreationDate).FirstOrDefault();
                    }
                }
                //var futureRide=db.Orders.Where(o=> (userType == (int)UserType.Passenger ? o.PassengerId == userId : o.DriverId == userId) && o.OrderTime>datenow)
                //var order = db.Orders.Where(o => (userType == (int)UserType.Passenger ? o.PassengerId == userId : o.DriverId == userId) && (o.OrderTime.Value <= datenow) && o.OrderTime > dateBEfore12Hours)
                //    .OrderByDescending(o => o.OrderId).FirstOrDefault();

                //var orderPending = db.Orders.Where(o => o.StatusId == (int)OrderStatus.Pending && o.Orders_Drivers.Count > 0 && o.Orders_Drivers.Select(d => d.DriverId).Contains(userId) && o.isReadTheOrderForDriver != true).OrderBy(o => driverUser.Location.Distance(o.PickUpLocation)).FirstOrDefault();
                //if (orderPending != null && (order != null ? orderPending.OrderId > order.OrderId : 1 == 1))
                //{
                //    order = orderPending;
                //}

                if (order != null)
                {
                    if (userType == (int)UserType.Passenger)
                        order.isReadTheOrder = true;
                    else if (userType == (int)UserType.Driver)
                        order.isReadTheOrderForDriver = true;
                    db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        public static Dictionary<string, double> CalcPriceIntercityTravel(LocationForIntercityTravel locationObj)
        {
            try
            {
                var pickUpCityArr = getAddressFromLatLong(locationObj.lat, locationObj.lon, (int)UserLanguages.en);
                // System.Threading.Thread.Sleep(2500);

                var destinationCityArr = getAddressFromLatLong(locationObj.destinationLatitude, locationObj.destinationLongitude, (int)UserLanguages.en);
                //try to get the price from db:
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    Dictionary<string, double> result = new Dictionary<string, double>();
                    PriceForInterCityNewUpdate row = null;
                    if (pickUpCityArr != null && destinationCityArr != null)
                    {
                        var pickUpCity = pickUpCityArr[1];
                        var destCity = destinationCityArr[1];

                        row = db.PriceForInterCityNewUpdates
                         .Where(c => ((c.pickUpCity.Trim() == pickUpCity.Trim() || c.pickUpCity.Trim() == pickUpCity.Trim().Replace("-", " "))
                                     && (c.destinationCity.Trim() == destCity.Trim() || c.destinationCity.Trim() == destCity.Trim().Replace("-", " ")))
                              || ((c.pickUpCity.Trim() == destCity.Trim() || c.pickUpCity.Trim() == destCity.Trim().Replace("-", " "))
                                     && (c.destinationCity.Trim() == pickUpCity.Trim() || c.destinationCity.Trim() == pickUpCity.Trim().Replace("-", " "))))
                         .FirstOrDefault();
                    }
                    if (row == null)
                    {
                        row = db.PriceForInterCityNewUpdates
                       .Where(c => c.pickUpCity.Trim() == locationObj.pickUpCityName.Trim() && c.destinationCity.Trim() == locationObj.destinationCityName.Trim()
                         || c.pickUpCity.Trim() == locationObj.destinationCityName.Trim() && c.destinationCity.Trim() == locationObj.pickUpCityName.Trim())
                       .FirstOrDefault();
                    }

                    if (row != null)
                    {
                        var date = DateTime.UtcNow;
                        if (locationObj.time > 0)
                        {
                            date = locationObj.time.ConvertFromUnixTimestamp();
                        }
                        else
                        {
                            locationObj.time = DateTime.UtcNow.ConvertToUnixTimestamp();
                        }
                        var location = Utils.LatLongToLocation(locationObj.lat, locationObj.lon);
                        var dateFormatted = convertToLocalTime(locationObj.time, location);
                        if (dateFormatted == null)
                        {
                            dateFormatted = date.AddHours(3); // for israel
                        }
                        var price = (dateFormatted.Value.TimeOfDay > TimeSpan.Parse("21:00") && dateFormatted.Value.TimeOfDay <= TimeSpan.Parse("24:00")) || (dateFormatted.Value.TimeOfDay < TimeSpan.Parse("05:00") && dateFormatted.Value.TimeOfDay >= TimeSpan.Parse("00:00")) ? row.price2.Value : row.price1.Value;
                        //if (price > 0)
                        //{
                        result["priceBeforeDiscount"] = price;
                        //}
                        //var basicDiscount = db.SystemSettings
                        //   .Where(x => x.ParamKey == "BasicDiscountForIntercityTravel")
                        //   .FirstOrDefault().ParamValue;
                        //price after discount:
                        result["priceAfterDiscount"] = result["priceBeforeDiscount"];// * (1 - Convert.ToDouble(basicDiscount));
                                                                                     //add 10 % if the time after 21:00 and before 05:00
                                                                                     //if (DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay > TimeSpan.Parse("21:00") && DateTime.Now.TimeOfDay < TimeSpan.Parse("05:00"))
                                                                                     //{
                                                                                     //    result["priceBeforeDiscount"] = result["priceBeforeDiscount"] * 1.1;
                                                                                     //    result["priceAfterDiscount"] = result["priceAfterDiscount"] * 1.1;
                                                                                     //}
                        result["priceBeforeDiscount"] = Math.Ceiling(result["priceBeforeDiscount"]);
                        result["priceAfterDiscount"] = Math.Ceiling(result["priceAfterDiscount"]);
                        return result;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("CalcPriceIntercityTravel: - {0}", ex.Message);
                return null;
            }
        }

        public static long createOrderIfDriverCancel(long orderID, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var orderDeclined = db.Orders.Where(o => o.OrderId == orderID && o.PassengerId == userId).FirstOrDefault();
                if (orderDeclined != null)
                {
                    orderDeclined.isReadTheOrder = false;
                    orderDeclined.StatusId = (int)OrderStatus.Pending;
                    orderDeclined.FlowStep = (int)FlowSteps.Step1;
                    orderDeclined.LastUpdateFlowStep = DateTime.UtcNow;
                    orderDeclined.CreationDate = DateTime.UtcNow;
                    db.SaveChanges();
                    new Task(() => { HandleSearchDrivers(orderDeclined); }).Start();
                    return orderDeclined.OrderId;
                }
                throw new OrderNotFoundException();
            }
        }

        public static List<long> getPendingOrdersForDriver(long userId)
        {
            var pendingOrders = new List<long>();
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                pendingOrders = db.Orders_Drivers.Include("Order").Where(od => od.DriverId == userId && od.StatusId == (int)Order_DriverStatus.SentPush && od.Order.StatusId == (int)OrderStatus.Pending).Select(o => o.OrderId).ToList();
            }
            return pendingOrders;
        }

        public static Order getOrderLastDetailsForDriver(long driverId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driverUser = db.Drivers.GetById(driverId);
                var datenow = DateTime.UtcNow.AddMinutes(1);

                var futureRide = db.Orders.Where(o => o.DriverId == driverId && o.OrderTime > datenow && o.isReadTheOrderForDriver != true)
                    .OrderByDescending(o => o.OrderTime).FirstOrDefault();

                var dateBEfore12Hours = DateTime.UtcNow.AddHours(-12);

                var order = db.Orders.Where(o => o.DriverId == driverId && (o.OrderTime.Value <= datenow) && (o.OrderTime.Value > dateBEfore12Hours))
                    .OrderByDescending(o => o.CreationDate).FirstOrDefault();

                var orderPending = db.Orders.Join(db.Orders_Drivers, o => o.OrderId, od => od.OrderId, (o, od) => new { o, od })
                    .Where(d => d.o.StatusId == (int)OrderStatus.Pending && d.od.DriverId == driverId && d.od.isReadTheOrderForDriver != true)
                    .Select(d => d.o as Order)
                    .OrderBy(d => driverUser.Location.Distance(d.PickUpLocation)).FirstOrDefault();
                // var orderPending = db.Orders.Where(o => o.StatusId == (int)OrderStatus.Pending && o.Orders_Drivers.Count > 0 && o.Orders_Drivers.Select(d => d.DriverId).Contains(driverId) && o.Orders_Drivers.Where(d => d.isReadTheOrderForDriver != true))
                // .OrderBy(o => driverUser.Location.Distance(o.PickUpLocation)).FirstOrDefault();
                if (orderPending != null && (order != null ? orderPending.OrderId > order.OrderId : 1 == 1))
                {
                    return orderPending;
                }
                if (order != null && order.isReadTheOrderForDriver != true)
                {
                    if (driverUser.Status == (int)DriverStatus.InPickupLocation || driverUser.Status == (int)DriverStatus.OnTheWayToDestination || driverUser.Status == (int)DriverStatus.OnTheWayToPickupLocation)
                        return order;
                }

                if (futureRide != null)
                {
                    return futureRide;
                }
                else
                    throw new OrderNotFoundException();
            }
        }

        private static void LogValidationErrors(DbEntityValidationException e)
        {
            foreach (var eve in e.EntityValidationErrors)
            {
                Logger.ErrorFormat("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                foreach (var ve in eve.ValidationErrors)
                {
                    Logger.ErrorFormat("- Property: \"{0}\", Error: \"{1}\"",
                        ve.PropertyName, ve.ErrorMessage);
                }
            }
        }

        public static Order updateDestinationLocation(long userId, long orderId, double destinationLatitude, double destinationLongitude, string destinationAddress, string destinationCity)
        {
            //System.Data.Entity.Spatial.DbGeography destination_location = DbGeography.FromText(string.Format("POINT ({0} {1})", destinationLongitude.ToString().Replace(",", "."), destinationLatitude.ToString().Replace(",", ".")));
            var destination_location = Utils.LatLongToLocation(destinationLatitude, destinationLongitude);

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                Order order = db.Orders
                    //.Include(x => x.User.Driver)
                    //.Include(x => x.User1)
                    .Where(x => x.OrderId == orderId)
                    .FirstOrDefault();

                if (order == null)
                    throw new OrderNotFoundException();

                if ((order.DriverId.HasValue && order.DriverId.Value == userId) || (order.PassengerId == userId))
                {
                    order.DestinationAddress = destinationAddress;
                    order.DestinationLocation = destination_location;
                    order.destinationCityName = destinationCity;
                    db.SaveChanges();
                    return order;
                }

                throw new UserPermissionException();

            }
        }

        public static Order getDetailsForEndRide(long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order != null)
                    return order;
                return null;
            }
        }

        public static Element MapsAPICall(double originLat, double originLng, double destinationLat, double destinationLng, string userCulture, DateTime departureTime)
        {
            //origin: 
            //String url = String.Format("https://maps.googleapis.com/maps/api/distancematrix/json?origins={0},{1}&destinations={2},{3}&mode=Car&language=us-en&sensor=false", originLat, originLng, destinationLat, destinationLng);

            //taking into account language
            //String url = String.Format("https://maps.googleapis.com/maps/api/distancematrix/json?origins={0},{1}&destinations={2},{3}&mode=Car&language={4}&sensor=false", originLat, originLng, destinationLat, destinationLng, userCulture);//best_guess

            //using an api key & departure time for distance in traffic:
            //String url = String.Format("https://maps.googleapis.com/maps/api/distancematrix/json?origins={0},{1}&destinations={2},{3}&mode=driving&language={4}&sensor=false&departure_time={5}&traffic_model=optimistic&key={6}", originLat, originLng, destinationLat, destinationLng, userCulture, departureTime.ConvertToUnixTimestamp(), ConfigurationHelper.MapsAPIKey);
            //traffic_model: best_guess, pessimistic, optimistic  
            //departure_time
            String url = String.Format("https://maps.googleapis.com/maps/api/distancematrix/json?origins={0},{1}&destinations={2},{3}&mode=driving&language={4}&departure_time={5}&traffic_model=optimistic&key={6}", originLat, originLng, destinationLat, destinationLng, userCulture, departureTime.ConvertToUnixTimestamp(), ConfigurationHelper.GoogleAPIKey);

            //Pass request to google api with orgin and destination details
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(result))
                {
                    Parent p = result.FromJson<Parent>(); //JsonConvert.DeserializeObject<Parent>(result);
                    Logger.DebugFormat("MapsAPICall: url: {0}", url);
                    Logger.DebugFormat("MapsAPICall: google response status: {0}", p.status);
                    if (p.status == "OK")
                    {
                        for (var i = 0; i < p.rows.Length; i++)
                        {
                            var results = p.rows[i].elements;
                            for (var j = 0; j < results.Length; j++)
                            {
                                Element element = results[j];
                                if (element != null)
                                    if (element.duration_in_traffic == null)
                                    {
                                        element.duration_in_traffic = new DurationInTraffic();
                                        element.duration_in_traffic.text = null;
                                        element.duration_in_traffic.value = 0;
                                    }
                                return element;
                            }
                        }
                    }
                    else return null;
                }
            }
            return null;
        }

        public static Order getOrderLastDetailsForPassenger(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var datenow = DateTime.UtcNow.AddMinutes(1);
                var dateBEfore12Hours = DateTime.UtcNow.AddHours(-12);

                var futureRide = db.Orders.Where(o => o.PassengerId == userId && o.OrderTime > datenow && o.StatusId != (int)OrderStatus.Canceled && o.isReadTheOrder != true).OrderByDescending(o => o.OrderTime).FirstOrDefault();

                var order = db.Orders.Where(o => o.PassengerId == userId && (o.OrderTime.Value <= datenow) && o.OrderTime > dateBEfore12Hours)
                    .OrderByDescending(o => o.CreationDate).FirstOrDefault();

                if (futureRide != null)
                {

                    return futureRide;
                }
                if (order != null && order.isReadTheOrder != true)
                {
                    return order;
                }
                else
                    throw new OrderNotFoundException();
            }
        }

        public static bool DoPayment(long orderId, double? amount)
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var order = db.Orders.GetById(orderId);
                    var userName = db.SystemSettings
                        .Where(u => u.ParamKey == "userNameForPayPal")
                        .FirstOrDefault().ParamValue;
                    //"rider-driver_api1.gmail.com";
                    var password = db.SystemSettings
                        .Where(u => u.ParamKey == "passwordForPayPal")
                        .FirstOrDefault().ParamValue; //"4KHRP5CQ5JHTKBJQ";
                    var signature = db.SystemSettings
                        .Where(u => u.ParamKey == "signatureForPayPal")
                        .FirstOrDefault().ParamValue; //"AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW";
                    var transactionId = db.Orders
                        .Where(t => t.OrderId == orderId)
                        .FirstOrDefault().transactionId;
                    //method 6:
                    //var url = "https://api-3t.sandbox.paypal.com/nvp";
                    var url = "https://api-3t.paypal.com/nvp";
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                    using (WebClient client = new WebClient())
                    {
                        Logger.DebugFormat("orderId in DoPayment : {0} ", Convert.ToString(orderId));
                        var response =
                     client.UploadValues(url, new System.Collections.Specialized.NameValueCollection()
                     {
                        { "USER", userName },
                        { "PWD", password },
                        {"SIGNATURE", signature },
                        {"VERSION", "124.0"},
                        {"METHOD", "DoCapture"},
                        { "AUTHORIZATIONID", transactionId },
                        {"AMT", Convert.ToString(amount) },
                        {"cycode", "ILS" },//USD
                        {"completetype", "Complete" },
                        {"INVNUM",Convert.ToString(order.CreationDate.Ticks) },
                        {"NOTE", "trip charge" }
                     });
                        string result6 = System.Text.Encoding.UTF8.GetString(response);
                        if (result6.Contains("Failure"))//the function faild:
                        {
                            Logger.ErrorFormat("error in DoPayment 222: {0} ", result6.ToString());
                            return true;//must have false !!!!
                        }
                        else//in sucsess
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("error in DoPayment 111: {0}", e.Message);
                return false;
            }
        }

        public static void updateErrorForOrder(long orderId, int errorId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order != null)
                {
                    order.errorId = errorId;
                    db.SaveChanges();
                }
            }
        }

        public static object GetEstimationTimeForPassenger(long orderId, int languageId, long userId)
        {

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);

                if (order == null)
                    throw new OrderNotFoundException();

                if (!order.PickUpLocation.Latitude.HasValue || !order.PickUpLocation.Longitude.HasValue)
                    throw new OrderNotFoundException();

                if (!order.DriverId.HasValue)
                    throw new UserNotExistException();

                Driver driver = null;
                if (userId == order.PassengerId)
                {
                    driver = db.Drivers.GetById(order.DriverId.Value);
                }
                var userCulture = NotificationsServices.Current.GetLanguageCulture(languageId);

                if (driver != null && driver.Location.Latitude.HasValue && driver.Location.Longitude.HasValue)
                {
                    try
                    {
                        // DateTime departureTime = order.OrderTime ?? order.CreationDate;
                        //DateTime departureTime = DateTime.UtcNow;
                        DateTime departureTime = DateTime.Now;

                        var google_response = MapsAPICall(driver.Location.Latitude.Value, driver.Location.Longitude.Value, order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, userCulture, departureTime);
                        if (google_response != null)
                        {
                            return new { lat = driver.Location.Latitude.Value, lon = driver.Location.Longitude.Value, estimateTime = google_response.duration.text, estimateDistance = google_response.distance.text, estimateTimeInTraffic = google_response.duration_in_traffic.text };
                        }
                        else
                            throw new GoogleAPIException();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        return new { lat = driver.Location.Latitude.Value, lon = driver.Location.Longitude.Value };
                    }
                }
                else throw new UserNotExistException();
            }
        }

        public static object passengerRefuseRidePrice(long orderId, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();
                if (order.StatusId == (int)OrderStatus.Canceled || order.StatusId == (int)OrderStatus.Completed || order.StatusId == (int)OrderStatus.Dissatisfied || order.StatusId == (int)OrderStatus.Pending)
                    throw new OrderNotRelevantException();

                var passenger = db.Users.GetById(order.PassengerId);
                if (passenger == null)
                    throw new UserNotExistException();
                if (passenger.UserId != userId)
                    throw new UserNotExistException();

                if (!order.DriverId.HasValue)
                    throw new UserNotExistException();

                var driver = db.Drivers.GetById(order.DriverId.Value);
                if (driver == null)
                    throw new UserNotExistException();
                var Pdata = new Dictionary<string, object>();
                // Pdata.Add("orderId", order.OrderId);
                NotificationsServices.Current.DriverNotification(driver.User, DriverNotificationTypes.PassengerRefuseRidePrice, orderId);
                return new { orderStatus = order.StatusId };
            }
        }

        public static object PassengerEndsRide(long orderId, int rating, int tip, long userId, int paymentMethod)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();
                if (order.StatusId == (int)OrderStatus.Canceled || order.StatusId == (int)OrderStatus.Completed || order.StatusId == (int)OrderStatus.Dissatisfied || order.StatusId == (int)OrderStatus.Pending)
                    throw new OrderNotRelevantException();

                var passenger = db.Users.GetById(order.PassengerId);
                if (passenger == null)
                    throw new UserNotExistException();
                if (passenger.UserId != userId)
                    throw new UserNotExistException();

                if (!order.DriverId.HasValue)
                    throw new UserNotExistException();

                var driver = db.Drivers.GetById(order.DriverId.Value);
                if (driver == null)
                    throw new UserNotExistException();
                //!!!!???
                //XcreateOrder שנקבע ב order.PaymentMethod כל צורת התשלום באפליקציה מתבצע לפי
                //Xולא לפי מה שהנוסע ממלא בסיום נסיעה וזה מהווה בעיה
                //Xלפי בחירת הנוסעorder.PaymentMethod התיקון כרגע הוא :בסיום נסיעה לשנות את ה


                //OrderStatus.PaymentError כאשר הכרטיס אשראי לא נמצא \ לא תקין\ החיוב שהנהג ביצע נדחה
                if (order.StatusId == (int)OrderStatus.PaymentError)
                {
                    //Xchanged by Shoshana on 02/01/18
                    // כרגע רק כשחיוב הנסיעה לא התבצע ניתן לנוסע אפשרות לבחור אמצעי תשלום אחר
                    order.PaymentMethod = paymentMethod;
                    var Pdata = new Dictionary<string, object>();
                    Pdata.Add("orderId", order.OrderId);
                    Pdata.Add("paymentMethod", paymentMethod);
                    NotificationsServices.Current.DriverNotification(driver.User, DriverNotificationTypes.creaditCardDriverError, orderId, extraInfo: Pdata);
                    //*******must do the payment******
                }
                //changed by Shoshana on 02/01/18
                order.PaymentMethod = paymentMethod;
                order.Rating = rating;
                order.tip = tip;
                order.StatusId = (int)OrderStatus.Completed;

                db.SaveChanges();

                #region Coupons
                if (tip > 0)
                {
                    //if he has coupon:
                    double sumAmount = 0;
                    var coupons = db.Coupons.Where(c => c.passengerId == order.PassengerId && c.orderId == null && c.dtStart < DateTime.Now && c.dtEnd > DateTime.Now).ToList();

                    if (coupons.Count > 0)
                    {
                        //?? order.tip;
                        var sumAmountAndCoupon = order.tip;

                        foreach (var couponItem in coupons)
                        {
                            if (sumAmount < sumAmountAndCoupon/*order.Amount*/)
                            {
                                couponItem.orderId = orderId;

                                var flag = false;
                                if (order.PaymentMethod == (int)CustomerPaymentMethod.CreditCard)
                                {
                                    var checkSum = sumAmount + couponItem.amount;
                                    if (sumAmountAndCoupon/*order.Amount*/ - checkSum < 5 && sumAmountAndCoupon/*order.Amount*/ - checkSum > 0)
                                    {
                                        flag = true;
                                    }
                                }

                                sumAmount += couponItem.amount;
                                if (flag == true)
                                    sumAmount -= 5;

                                //רק קופון אחד יכול להיווצר
                                if (sumAmount > sumAmountAndCoupon/*order.Amount*/)
                                {
                                    var newCoupon = new Coupon()
                                    {
                                        amount = sumAmount - sumAmountAndCoupon.Value/*order.Amount.Value*/,
                                        currency = couponItem.currency,
                                        dtEnd = couponItem.dtEnd,
                                        dtStart = couponItem.dtStart,
                                        number = couponItem.number,
                                        passengerId = order.PassengerId,
                                        passengerIdSMS = couponItem.passengerIdSMS,
                                        orderId = null
                                    };
                                    db.Coupons.Add(newCoupon);
                                    couponItem.amount = couponItem.amount - newCoupon.amount;
                                }
                            }
                        }
                    }
                    #endregion

                    var amountOfPaymentAfterUsingCoupon = order.tip - sumAmount;

                    if (amountOfPaymentAfterUsingCoupon > 0)
                    {
                        //!!!!
                        if (order.PaymentMethod == (int)CustomerPaymentMethod.CreditCard)
                        {
                            new Task(() =>
                            {
                                var result = PaymentService.DoPaymentForCCTranmodeA(passenger.UserId, amountOfPaymentAfterUsingCoupon.Value, 1, order.OrderId);
                            }).Start();

                        }
                        //!!!!
                        else if (order.PaymentMethod == (int)CustomerPaymentMethod.Paypal)
                        {
                            new Task(() =>
                            {
                                var doPayment = DoPayment(orderId, amountOfPaymentAfterUsingCoupon.Value);
                            }).Start();
                        }
                    }

                }


                var coupon = db.Coupons.Where(c => c.orderId == orderId && c.passengerId == order.PassengerId).ToList();

                if (coupon.Count > 0)
                {
                    var sumAmount = coupon.Sum(c => c.amount);
                    // PaymentSuccessfulAndCoupon
                    var Pdata1 = new Dictionary<string, object>();
                    Pdata1.Add("paymentMethod", order.PaymentMethod);
                    Pdata1.Add("amount", order.Amount);
                    Pdata1.Add("currency", order.Currency);
                    Pdata1.Add("couponAmount", sumAmount);
                    NotificationsServices.Current.DriverNotification(db.Users.GetById(driver.UserId), DriverNotificationTypes.PaymentSuccessfulAndCoupon, orderId, Pdata1);
                }
                else if (order.PaymentMethod != (int)CustomerPaymentMethod.Cash)
                {
                    var Pdata1 = new Dictionary<string, object>();
                    Pdata1.Add("paymentMethod", order.PaymentMethod);
                    Pdata1.Add("amount", order.Amount);
                    Pdata1.Add("currency", order.Currency);
                    NotificationsServices.Current.DriverNotification(db.Users.GetById(driver.UserId), DriverNotificationTypes.PaymentSuccessful, orderId, Pdata1);
                }
                #region sendMail
                //new Task(() =>
                //{
                //    //send email for the passenger that canceled the ride:
                //    var thisPassenger = db.Users.Where(p => p.UserId == order.PassengerId).FirstOrDefault();
                //    Dictionary<int, string> lTexts = Utils.PrepareToSendEmailForPassenger(PassengerEmail.CompleteTripTitle, thisPassenger.LanguageId);
                //    var massage1 = "";
                //    for (int i = 1; i < lTexts.Count; i++)
                //    {
                //        massage1 += "<h3>" + lTexts[i] + "</h3>";
                //    }
                //    var isSend1 = Utils.SendMail(new List<string>() { thisPassenger.Email }, lTexts[0], massage1, null, thisPassenger.LanguageId);
                //    if (isSend1 == false)
                //        Logger.ErrorFormat("error when sending email for passenger: {0} about CancelTrip. email:{1}", thisPassenger.UserId, thisPassenger.Email);
                //    else
                //        Logger.DebugFormat("email for passenger: {0} about CancelTrip success. email:{1}", order.PassengerId, thisPassenger.Email);
                //}).Start();
                #endregion
                return new { orderStatus = order.StatusId };
            }
        }

        public static object DriverEndRide(long orderId, double? amount = null, string currency = "")
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();

                var passenger = db.Users.GetById(order.PassengerId);
                if (passenger == null)
                    throw new UserNotExistException();

                if (!order.DriverId.HasValue)
                    throw new UserNotExistException();

                var driver = db.Drivers.Include("User").GetById(order.DriverId.Value);
                if (driver == null)
                    throw new UserNotExistException();

                if ((order.StatusId != (int)OrderStatus.Confirmed) && (order.StatusId != (int)OrderStatus.Payment) && (order.StatusId != (int)OrderStatus.DisputeAmount))
                    throw new ExpirationException();

                if (driver.Status == (int)DriverStatus.Available)
                    throw new ExpirationException();
                //מעדכן את פרטי ההזמנה
                order.EndTime = DateTime.UtcNow;
                order.Amount = amount;
                order.Currency = currency;

                order.StatusId = (int)OrderStatus.Payment;
                driver.Status = (int)DriverStatus.Available;
                db.SaveChanges();

                var Pdata = new Dictionary<string, object>();
                Pdata.Add("amount", amount);
                //  Pdata.Add("currency", currency);
                NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.RidePrice, orderId, extraInfo: Pdata);
                return new { orderStatus = order.StatusId };
            }
        }

        public static object DriverEndsRide(long orderId, int? paymentMethod, double? amount = null, string currency = "")
        {

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();

                //XcreateOrder שנקבע ב order.PaymentMethod כל צורת התשלום באפליקציה מתבצע לפי
                //Xולא לפי מה שהנוסע ממלא בסיום נסיעה וזה מהווה בעיה
                //Xלפי קביעת הנוסעorder.PaymentMethod התיקון כרגע הוא :בסיום נסיעה לשנות את ה

                //X!!!! changed by Shoshana on 02/01/18
                //paymentMethod = order.PaymentMethod;
                //Xorder.PaymentMethod = paymentMethod;
                Logger.DebugFormat("paymentMethod : {0}", paymentMethod);

                var passenger = db.Users.GetById(order.PassengerId);
                if (passenger == null)
                    throw new UserNotExistException();

                if (!order.DriverId.HasValue)
                    throw new UserNotExistException();

                var driver = db.Drivers.Include("User").GetById(order.DriverId.Value);
                if (driver == null)
                    throw new UserNotExistException();

                if ((order.StatusId != (int)OrderStatus.Confirmed) && (order.StatusId != (int)OrderStatus.Payment) && (order.StatusId != (int)OrderStatus.DisputeAmount))
                    throw new ExpirationException();

                if (driver.Status == (int)DriverStatus.Available)
                    throw new ExpirationException();

                if (paymentMethod == (int)CustomerPaymentMethod.InApp && passenger.AlwaysApproveSum == true && (!amount.HasValue || string.IsNullOrEmpty(currency)))
                    throw new NoRelevantDataException(); //if we use the app automatically - must have amounts...

                //!!!! changed by Shoshana on 02/01/18
                if (paymentMethod > 0)
                    order.PaymentMethod = (int)paymentMethod;

                //מעדכן את פרטי ההזמנה
                order.EndTime = DateTime.UtcNow;
                order.Amount = amount;
                order.Currency = currency;

                //isr:
                //if (order.PaymentMethod == (int)CustomerPaymentMethod.Business)
                //    notifyIsrTaxiState(order.DriverId.Value, (int)DriverStatus.Available);

                #region Coupons
                //use the coupon if the passenger has:
                double sumAmount = 0;
                //מחפש  את כל הקופונים 1.שנשלחו לנוסע 2.ועדיין לא מומשו 3.קופונים שכרגע תקפים 
                var coupons = db.Coupons.Where(c => c.passengerId == order.PassengerId && c.orderId == null && c.dtStart < DateTime.Now && c.dtEnd > DateTime.Now).ToList();
                if (coupons.Count > 0)
                {
                    // double sumAmount = 0; //coupons.Select(c => c.amount).Sum();

                    //  if (order.PaymentMethod == (int)CustomerPaymentMethod.CreditCard)
                    //{
                    //      var sum = coupons.Select(c => c.amount).Sum();
                    //     if (amount - sum < 20 && amount - sum > 0)
                    //     {
                    //      }
                    //   }

                    var sumAmountAndCoupon = order.Amount + order.tip;
                    Logger.DebugFormat("DriverEndsRide order.tip {0}", order.tip);

                    foreach (var coupon in coupons)
                    {
                        if (sumAmount < sumAmountAndCoupon/*order.Amount*/)
                        {
                            coupon.orderId = orderId;

                            var flag = false;
                            if (order.PaymentMethod == (int)CustomerPaymentMethod.CreditCard)
                            {
                                var checkSum = sumAmount + coupon.amount;
                                if (sumAmountAndCoupon/*order.Amount*/ - checkSum < 5 && sumAmountAndCoupon/*order.Amount*/ - checkSum > 0)
                                {
                                    flag = true;
                                }
                            }

                            sumAmount += coupon.amount;
                            if (flag == true)
                                sumAmount -= 5;

                            // אם ההנחה גדולה מהמחיר לתשלום
                            //נוצר קופון חדש לנוסע
                            if (sumAmount > sumAmountAndCoupon/*order.Amount*/)
                            {
                                var newCoupon = new Coupon()
                                {
                                    amount = sumAmount - sumAmountAndCoupon.Value/*order.Amount.Value*/,
                                    currency = coupon.currency,
                                    dtEnd = coupon.dtEnd,
                                    dtStart = coupon.dtStart,
                                    number = coupon.number,
                                    passengerId = order.PassengerId,
                                    passengerIdSMS = coupon.passengerIdSMS,
                                    orderId = null
                                };
                                db.Coupons.Add(newCoupon);
                                coupon.amount = coupon.amount - newCoupon.amount;
                            }

                        }
                    }
                }
                db.SaveChanges();

                var amountOfPaymentAfterUsingCoupon = order.Amount - sumAmount;
                Logger.DebugFormat("The amount left over after using the coupon: {0} ", amountOfPaymentAfterUsingCoupon);
                #endregion
                //////////////////end using the coupon/////////////////////

                #region //Payment in Taxi

                if (paymentMethod == (int)CustomerPaymentMethod.Cash)
                {
                    //send push message to passenger 
                    var Pdata = new Dictionary<string, object>();
                    Pdata.Add("paymentMethod", paymentMethod);
                    //האם שולח נוטיפיקציה עם התשלום המלא?
                    Pdata.Add("amount", amount);
                    Pdata.Add("currency", currency);
                    NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayInTaxi, orderId, extraInfo: Pdata);

                    //update driver and order status
                    order.StatusId = (int)OrderStatus.Payment;
                    driver.Status = (int)DriverStatus.Available;
                }
                #endregion
                #region payment with paypal:
                else if (paymentMethod == (int)CustomerPaymentMethod.Paypal)
                {
                    bool? doPayment = null;
                    if (amountOfPaymentAfterUsingCoupon > 0)
                        doPayment = DoPayment(orderId, amountOfPaymentAfterUsingCoupon /*amount*/);
                    if (doPayment == true)//sucsess:
                    {
                        order.StatusId = (int)OrderStatus.Payment;
                        driver.Status = (int)DriverStatus.Available;
                        var Pdata = new Dictionary<string, object>();
                        Pdata.Add("paymentMethod", paymentMethod);
                        Pdata.Add("amount", amount);
                        Pdata.Add("currency", currency);
                        //NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayWithPayPal, orderId, extraInfo: Pdata);
                        NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayWithPayPal, orderId, extraInfo: Pdata);
                    }
                }
                #endregion
                #region pay with business:
                else if (paymentMethod == (int)CustomerPaymentMethod.Business)
                {
                    order.StatusId = (int)OrderStatus.Payment;
                    driver.Status = (int)DriverStatus.Available;
                    var Pdata = new Dictionary<string, object>();
                    Pdata.Add("paymentMethod", paymentMethod);
                    Pdata.Add("amount", amount);
                    Pdata.Add("currency", currency);

                    //NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayWithPayPal, orderId, extraInfo: Pdata);
                    if (order.isFromWeb == true && order.Passenger.DeviceId == null)
                    {
                        bool status1 = UserService.SendSMSNotif(order.PassengerId, order.Passenger.Phone, SMSType.PayWithBuissness, order.Passenger.LanguageId);
                    }
                    NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayWithBuissness, orderId, extraInfo: Pdata);
                }
                #endregion

                #region Payment using Credit card:
                //Payment using Credit card:
                else if (paymentMethod == (int)CustomerPaymentMethod.CreditCard)
                {
                    order.StatusId = (int)OrderStatus.Payment;
                    driver.Status = (int)DriverStatus.Available;
                    //db.SaveChanges();

                    var creditCard = db.CreditCardUsers.Where(c => c.userId == passenger.UserId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(c => c.isDefault).FirstOrDefault();
                    //אם כרטיס האשראי לא תקין או לא נמצא שולח נטיפיקציה עם הערה לנוסע
                    if (creditCard == null)
                    {

                        order.StatusId = (int)OrderStatus.PaymentError;
                        db.SaveChanges();
                        var Pdata = new Dictionary<string, object>();
                        Pdata.Add("paymentMethod", paymentMethod);
                        Pdata.Add("amount", amount);
                        Pdata.Add("currency", currency);
                        Pdata.Add("orderId", order.OrderId);
                        NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.NoCreditCard, orderId, extraInfo: Pdata);
                    }
                    else
                    {
                        new Task(() =>
                        {
                            bool? result = null;
                            //todo: remove isProduction condition
                            if (amountOfPaymentAfterUsingCoupon > 0)
                                result = PaymentService.DoPaymentForCCTranmodeA(passenger.UserId, amountOfPaymentAfterUsingCoupon.Value, 1, order.OrderId);
                            else
                                result = true;
                            Logger.DebugFormat("result for do payment: {0}", result);
                            if (result == true)
                            {
                                //push to passenger that payment was done
                                var Pdata = new Dictionary<string, object>();
                                Pdata.Add("paymentMethod", paymentMethod);
                                Pdata.Add("amount", amount);
                                Pdata.Add("currency", currency);
                                Pdata.Add("orderId", order.OrderId);
                                Pdata.Add("LastFourDigits", creditCard.tokenId.Substring(creditCard.tokenId.Length));
                                NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PaymentSuccessful, orderId, extraInfo: Pdata);
                            }
                            else//payment error
                            {
                                order.StatusId = (int)OrderStatus.PaymentError;
                                // db.SaveChanges();
                                var Pdata = new Dictionary<string, object>();
                                Pdata.Add("paymentMethod", paymentMethod);
                                Pdata.Add("amount", amount);
                                Pdata.Add("currency", currency);
                                Pdata.Add("orderId", order.OrderId);
                                Pdata.Add("LastFourDigits", creditCard.tokenId.Substring(creditCard.tokenId.Length));
                                NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PaymentError, orderId, extraInfo: Pdata);

                            }
                        }).Start();
                    }
                }
                #endregion
                db.SaveChanges();
                #region SendMail
                //new Task(() =>
                //{
                //    //send email to driver that ended the ride:
                //    var textForEmail = Utils.PrepareToSendEmail(DriverEmail.CompleteTripTitle, driver.User.LanguageId, order.Amount.Value);
                //    var massage = "";
                //    for (int i = 1; i < textForEmail.Count; i++)
                //    {
                //        massage += "<h3>" + textForEmail[i] + "</h3>";
                //    }
                //    var isSend = Utils.SendMail(new List<string>() { driver.User.Email }, textForEmail[0], massage, null, driver.User.LanguageId);
                //    if (isSend == false)
                //        Logger.ErrorFormat("error when sending email for driver: {0} about CompleteTrip. email:{1}", driver.UserId, driver.User.Email);
                //    else
                //        Logger.DebugFormat("email for driver: {0} about CompleteTrip success. email:{1}", driver.UserId, driver.User.Email);

                //    //send email also to passenger that ended the ride in the end of the passenger function
                //}).Start();
                #endregion
                return new { orderStatus = order.StatusId };
            }
        }

        public static object GeneralEndsRide(long orderId, int paymentMethod, int rating, int tip, long userId, double? amount = null, string currency = "", string FileNumber = "")
        {
            bool isSuccess = true;
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                #region Exeptions Handling

                if (order == null)
                    throw new OrderNotFoundException();
                if (order.StatusId == (int)OrderStatus.Canceled || order.StatusId == (int)OrderStatus.Completed || order.StatusId == (int)OrderStatus.Dissatisfied || order.StatusId == (int)OrderStatus.Pending)
                    throw new OrderNotRelevantException();

                var passenger = db.Users.GetById(order.PassengerId);
                if (passenger == null)
                    throw new UserNotExistException();
                if (passenger.UserId != userId)
                    throw new UserNotExistException();

                if (!order.DriverId.HasValue)
                    throw new UserNotExistException();

                //=============

                var driver = db.Drivers.Include("User").GetById(order.DriverId.Value);
                if (driver == null)
                    throw new UserNotExistException();

                if ((order.StatusId != (int)OrderStatus.Confirmed) && (order.StatusId != (int)OrderStatus.Payment) && (order.StatusId != (int)OrderStatus.DisputeAmount))
                    throw new ExpirationException();

                if (paymentMethod == (int)CustomerPaymentMethod.InApp && passenger.AlwaysApproveSum == true && (!amount.HasValue || string.IsNullOrEmpty(currency)))
                    throw new NoRelevantDataException(); //if we use the app automatically - must have amounts...
                #endregion

                #region order settings
                //מעדכן את פרטי ההזמנה
                //changed by Shoshana on 02/01/18
                order.PaymentMethod = paymentMethod;
                order.Rating = rating;
                order.tip = tip;
                order.EndTime = DateTime.UtcNow;
                order.Amount = amount;
                order.Currency = currency;
                order.FileNumber = FileNumber;
                // driver.Status = (int)DriverStatus.Available;

                db.SaveChanges();
                #endregion

                #region Payment
                //PassengerNotificationTypes notificationType = PassengerNotificationTypes.PaymentSuccessful;
                //???
                double price = (double)amount + tip;
                var amountOfPaymentAfterUsingCoupon = priceAfterCoupons(order, price);

                if (amountOfPaymentAfterUsingCoupon > 0)
                {
                    var Pdata = new Dictionary<string, object>();
                    Pdata.Add("paymentMethod", paymentMethod);
                    Pdata.Add("amount", amountOfPaymentAfterUsingCoupon);
                    //  Pdata.Add("amount", amount);
                    Pdata.Add("currency", currency);

                    #region Cash Payment in Driver Taxi 
                    if (paymentMethod == (int)CustomerPaymentMethod.Cash)
                    {
                        //NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayInTaxi, orderId, extraInfo: Pdata);
                    }
                    #endregion

                    #region Payment using Credit Card
                    else if (paymentMethod == (int)CustomerPaymentMethod.CreditCard)
                    {
                        var creditCard = db.CreditCardUsers.Where(c => c.userId == passenger.UserId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(c => c.isDefault).FirstOrDefault();
                        if (creditCard == null)
                        {
                            isSuccess = false;
                            order.StatusId = (int)OrderStatus.PaymentError;
                            NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.NoCreditCard, orderId, extraInfo: Pdata);
                            SendPaymentErrorNotificationToDriver(driver.User, order.OrderId, (int)CustomerPaymentMethod.Cash);
                        }
                        else
                        {
                            //new Task(() =>
                            //{
                            bool? result = null;
                            result = PaymentService.DoPaymentForCCTranmodeA(passenger.UserId, amountOfPaymentAfterUsingCoupon, 1, order.OrderId);
                            Logger.DebugFormat("result for do payment: {0}", result);
                            if (result == true)
                            {
                                //  NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PaymentSuccessful, orderId, extraInfo: Pdata);
                            }
                            else//payment error
                            {
                                order.StatusId = (int)OrderStatus.PaymentError;
                                isSuccess = false;
                                NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PaymentError, orderId, extraInfo: Pdata);
                                SendPaymentErrorNotificationToDriver(driver.User, order.OrderId, (int)CustomerPaymentMethod.Cash);
                            }
                            Pdata.Add("orderId", order.OrderId);
                            if (creditCard == null)
                                Pdata.Add("LastFourDigits", creditCard.tokenId.Substring(creditCard.tokenId.Length));
                            //}).Start();
                        }
                    }
                    #endregion

                    #region Payment using Paypal
                    else if (paymentMethod == (int)CustomerPaymentMethod.Paypal)
                    {
                        bool? doPayment = null;
                        if (amountOfPaymentAfterUsingCoupon > 0)
                            doPayment = DoPayment(orderId, amountOfPaymentAfterUsingCoupon /*amount*/);
                        if (doPayment == true)//sucsess:
                        {
                            // NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayWithPayPal, orderId, extraInfo: Pdata);
                        }
                        else
                        {
                            isSuccess = false;
                        }
                    }
                    #endregion

                    #region pay with business:
                    else if (paymentMethod == (int)CustomerPaymentMethod.Business)
                    {
                        if (order.isFromWeb == true && order.Passenger.DeviceId == null)
                        {
                            bool status1 = UserService.SendSMSNotif(order.PassengerId, order.Passenger.Phone, SMSType.PayWithBuissness, order.Passenger.LanguageId);
                        }
                        // NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.PayWithBuissness, orderId, extraInfo: Pdata);
                    }
                    #endregion
                    #endregion
                }
                //send notifications to driver
                if (isSuccess)
                    DriverNotificationsOnOrderCompleted(order, driver.User);


                #region update order sttings
                order.StatusId = (int)OrderStatus.Completed;
                //driver.Status = (int)DriverStatus.Available;
                db.SaveChanges();
                #endregion

                #region Send Mail
                //new Task(() =>
                //{
                //    //send email to driver that ended the ride:
                //    var textForEmail = Utils.PrepareToSendEmail(DriverEmail.CompleteTripTitle, driver.User.LanguageId, order.Amount.Value);
                //    OrderCompleteMail(textForEmail, driver.User, "driver");

                //    //send email to driver that completed the ride:
                //    textForEmail = Utils.PrepareToSendEmailForPassenger(PassengerEmail.CompleteTripTitle, passenger.LanguageId);
                //    OrderCompleteMail(textForEmail, passenger, "passenger");
                //}).Start();

                #endregion

                return isSuccess;
            }
        }

        private static void SendPaymentErrorNotificationToDriver(User driver, long orderId, int paymentMethod)
        {
            //is relevant anymore?
            //send notification to driver or passenger????
            NotificationsServices.Current.DriverNotification(driver, DriverNotificationTypes.creaditCardDriverError, orderId,
                extraInfo: new Dictionary<string, object>() { { "orderId", orderId }, { "paymentMethod", paymentMethod } });
        }

        private static void DriverNotificationsOnOrderCompleted(Order order, User driver)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var Pdata1 = new Dictionary<string, object>();
                Pdata1.Add("paymentMethod", order.PaymentMethod);
                Pdata1.Add("amount", order.Amount);
                Pdata1.Add("currency", order.Currency);
                var coupon = db.Coupons.Where(c => c.orderId == order.OrderId && c.passengerId == order.PassengerId).ToList();

                if (coupon.Count > 0)
                {
                    var sumAmount = coupon.Sum(c => c.amount);
                    // PaymentSuccessfulAndCoupon
                    Pdata1.Add("couponAmount", sumAmount);
                    NotificationsServices.Current.DriverNotification(driver, DriverNotificationTypes.PaymentSuccessfulAndCoupon, order.OrderId, Pdata1);
                }
                //else if (order.PaymentMethod == (int)CustomerPaymentMethod.Cash)
                //    NotificationsServices.Current.DriverNotification(driver, DriverNotificationTypes.RideEndedSuccessfull, order.OrderId, Pdata1);
                else
                {
                    DriverNotificationTypes notificationType;
                    if (order.tip > 0)
                    {
                        notificationType = DriverNotificationTypes.RideEndedSuccessfullAndTip;
                        Pdata1.Add("tip", order.tip);
                    }
                    else
                        notificationType = DriverNotificationTypes.PaymentSuccessful;
                    NotificationsServices.Current.DriverNotification(driver, notificationType, order.OrderId, Pdata1);
                }
            }
        }

        private static void OrderCompleteMail(Dictionary<int, string> lTexts, User user, string userType)
        {
            var massage1 = "";
            for (int i = 1; i < lTexts.Count; i++)
            {
                massage1 += "<h3>" + lTexts[i] + "</h3>";
            }
            var isSend1 = Utils.SendMail(new List<string>() { user.Email }, lTexts[0], massage1, null, user.LanguageId);
            if (isSend1 == false)
                Logger.ErrorFormat("error when sending email for " + userType + ": {0} about CompleteTrip. email:{1}", user.UserId, user.Email);
            else
                Logger.DebugFormat("email for " + userType + ": {0} about CompleteTrip success. email:{1}", user.UserId, user.Email);
        }

        private static double priceAfterCoupons(Order order, double amount)
        {
            double amountOfPaymentAfterUsingCoupon = 0;
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                double sumAmount = 0;
                //מחפש  את כל הקופונים 1.שנשלחו לנוסע 2.ועדיין לא מומשו 3.קופונים שכרגע תקפים 
                var coupons = db.Coupons.Where(c => c.passengerId == order.PassengerId && c.orderId == null &&( c.dtStart < DateTime.Now|| c.dtStart == DateTime.Now) &&( c.dtEnd > DateTime.Now || c.dtEnd == DateTime.Now)).ToList();
                if (coupons.Count > 0)
                {
                    var sumAmountAndCoupon = amount;
                    foreach (var coupon in coupons)
                    {
                        if (sumAmount < sumAmountAndCoupon/*order.Amount*/)
                        {
                            //coupon.orderId = orderId;
                            coupon.orderId = order.OrderId;

                            var flag = false;
                            if (order.PaymentMethod == (int)CustomerPaymentMethod.CreditCard)
                            {
                                var checkSum = sumAmount + coupon.amount;
                                if (sumAmountAndCoupon - checkSum < 5 && sumAmountAndCoupon - checkSum > 0)
                                {
                                    flag = true;
                                }
                            }

                            sumAmount += coupon.amount;
                            if (flag == true)
                                sumAmount -= 5;

                            // אם ההנחה גדולה מהמחיר לתשלום
                            //נוצר קופון חדש לנוסע
                            if (sumAmount > sumAmountAndCoupon)
                            {
                                var newCoupon = new Coupon()
                                {
                                    amount = sumAmount - sumAmountAndCoupon,
                                    currency = coupon.currency,
                                    dtEnd = coupon.dtEnd,
                                    dtStart = coupon.dtStart,
                                    number = coupon.number,
                                    passengerId = order.PassengerId,
                                    passengerIdSMS = coupon.passengerIdSMS,
                                    orderId = null
                                };
                                db.Coupons.Add(newCoupon);
                                coupon.amount = coupon.amount - newCoupon.amount;
                            }
                        }
                    }
                }
                amountOfPaymentAfterUsingCoupon = amount - sumAmount;
                db.SaveChanges();
            }

            return amountOfPaymentAfterUsingCoupon;
        }

        public static object GetEstimationTime(long orderId, int languageId, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);

                if (order == null)
                    throw new OrderNotFoundException();

                if (!order.PickUpLocation.Latitude.HasValue || !order.PickUpLocation.Longitude.HasValue)
                    throw new OrderNotFoundException();

                if ((userId == order.PassengerId) && (!order.DriverId.HasValue)) // if a passenger runs the method - there must be a driver assigned.
                    throw new UserNotExistException();

                Driver driver = null;
                if (userId != order.PassengerId) // a driver runs the method
                {
                    if (!DriverInOrder(userId, orderId))
                        throw new UserForbiddenException();
                    else driver = db.Drivers.GetById(userId);
                }
                else
                    driver = db.Drivers.GetById(order.DriverId.Value);

                var userCulture = NotificationsServices.Current.GetLanguageCulture(languageId);

                if (driver != null && driver.Location.Latitude.HasValue && driver.Location.Longitude.HasValue)
                {
                    try
                    {


                        DateTime departureTime = order.OrderTime ?? order.CreationDate;

                        var google_response = MapsAPICall(driver.Location.Latitude.Value, driver.Location.Longitude.Value, order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, userCulture, departureTime);
                        if (google_response != null)
                        {
                            //return new { lat = driver.Location.Latitude.Value, lon = driver.Location.Longitude.Value, estimateTime = google_response.value };
                            return new { lat = driver.Location.Latitude.Value, lon = driver.Location.Longitude.Value, estimateTime = google_response.duration.text, estimateDistance = google_response.distance.text, estimateTimeInTraffic = google_response.duration_in_traffic.text };
                        }
                        else
                            throw new GoogleAPIException();
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e);
                        return new { lat = driver.Location.Latitude.Value, lon = driver.Location.Longitude.Value };

                    }
                }
                else throw new UserNotExistException();



            }
        }

        public static Order GetOrderDetails(long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders
                    .Include(x => x.Passenger)
                    .Include(x => x.Driver)
                    .GetById(orderId);

                if (order != null)
                {
                    return order;
                }
                else
                    throw new OrderNotFoundException();
            }

        }

        public static List<Order> GetMyRides(long userId, UserType userType)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var addMinutesDate = DateTime.UtcNow.AddMinutes(1);
                var orders = db.Orders.Include("Passenger")
                    .NotCancelled()
                    .Where(o => o.OrderTime > addMinutesDate)
                    .Include(o => o.Passenger)
                    .Include(o => o.Driver)
                    .Include(o => o.Driver.User);


                if (userType == UserType.Driver)
                    orders = orders.ByDriver(userId);

                if (userType == UserType.Passenger)
                    orders = orders.Include(o => o.Driver).Include(o => o.Driver.User).ByPassenger(userId);

                return orders.ToList();
            }
        }

        public static List<Order> GetMyPostRides(long userId, UserType userType, double date)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var dateAddMinute = DateTime.UtcNow.AddMinutes(1);

                var newDate = date.ConvertFromUnixTimestamp();
                var orders = db.Orders
                    .NotCancelled()
                    //.Where(o => (o.StatusId == (int)OrderStatus.Completed || (o.StatusId == (int)OrderStatus.Payment && o.Amount != null)) && o.OrderTime.Value.Month == newDate.Month && o.OrderTime.Value.Year == newDate.Year)
                    .Where(o => (o.StatusId == (int)OrderStatus.Completed ||
                    (o.StatusId == (int)OrderStatus.Payment && o.Amount != null)) &&
                    o.OrderTime.Value.Month == newDate.Month && o.OrderTime.Value.Year == newDate.Year)
                    //??
                    //  && o.FlowStep != (int)FlowSteps.Step5 && o.OrderTime < dateAddMinute)
                    .Include("Passenger")
                    .Include(o => o.Driver)
                    .Include(o => o.Driver.User);

                if (userType == UserType.Driver)
                    orders = orders.ByDriver(userId);

                if (userType == UserType.Passenger)
                    orders = orders.Include(o => o.Driver).Include(o => o.Driver.User).ByPassenger(userId);

                return orders.ToList();
            }
        }

        //public static List<Order> GetMyRides1(long userId, int userType)
        //{
        //    using (var db = new BallyTaxiEntities())
        //    {
        //        if (userType == (int)UserType.Passenger)
        //        {
        //            var order = db.Orders
        //                .Include(x => x.User.Driver)
        //                .Where(x => x.PassengerId == userId && x.StatusId != (int)OrderStatus.Canceled)
        //                    .ToList();
        //            if (order != null)
        //            {
        //                return order;
        //            }
        //            else return new List<Order>();
        //        }
        //        else if (userType == (int)UserType.Driver) { 
        //            var order = db.Orders
        //                .Include(x => x.User.Passenger)
        //                .Where(x => x.DriverId == userId && x.StatusId != (int)OrderStatus.Canceled )
        //                .ToList();
        //            if (order != null)
        //            {
        //                return order;
        //            }
        //            else return new List<Order>(); 
        //        }
        //        else return new List<Order>(); 
        //    }

        //}

        public static List<Order> GetLastAddresses(long passengerId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.Orders.Where(x => x.PassengerId == passengerId /*&& x.StatusId != (int)OrderStatus.Canceled*/)
                    .OrderByDescending(x => x.OrderTime).ToList();
            }
        }

        public static List<FavoriteAddress> GetFavoriteAddress(long passengerId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.FavoriteAddresses.Where(x => x.PassengerId == passengerId)
                     .ToList();
            }
        }

        public static long CancelOrderByPassenger(long orderId, long passengerUserId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var passengerUser = db.Users.GetById(passengerUserId);
                if (passengerUser != null)
                {
                    var order = db.Orders.Include(c => c.Passenger).GetById(orderId);
                    //.Include(x=> x.User)
                    //.Include(x => x.Drivers.Select(c => c.Driver.User))
                    //.Where(x => x.OrderId == orderId).FirstOrDefault();
                    if (order.StatusId == (int)OrderStatus.Confirmed || order.StatusId == (int)OrderStatus.Pending)
                    {
                        if (order != null)
                        {
                            var dateMore30Minutes = DateTime.UtcNow.AddMinutes(30);
                            if (order.OrderTime < dateMore30Minutes && (order.FlowStep == (int)FlowSteps.Step4 || order.FlowStep == (int)FlowSteps.Step5))
                            {
                                return 0;
                                //throw new CannotCancelFutureRideException();
                            }

                            order.StatusId = (int)OrderStatus.Canceled;
                            order.isReadTheOrderForDriver = false;

                            //this order already has driver - the driver accept on this ride
                            if (order.DriverId.HasValue)
                            {
                                //change status of driver to Available
                                var driver = db.Drivers.GetById(order.DriverId.Value);
                                var futureDate = DateTime.UtcNow.AddMinutes(15);
                                if (driver.Status == (int)DriverStatus.OnTheWayToPickupLocation || driver.Status == (int)DriverStatus.InPickupLocation || driver.Status == (int)DriverStatus.OnTheWayToDestination)
                                {
                                    if (order.OrderTime < futureDate)
                                        driver.Status = (int)DriverStatus.Available;
                                }
                                var pData = new Dictionary<string, object>();
                                if (order.OrderTime > futureDate)
                                {
                                    DateTime? resultDate = OrderService.convertToLocalTime(order.OrderTime.Value.ConvertToUnixTimestamp(), order.PickUpLocation);
                                    if (resultDate == null)
                                        resultDate = order.OrderTime.Value.AddHours(3);//for jerusalem

                                    //DateTime convertedDate = order.OrderTime.Value.AddHours(3);
                                    pData["isFutureRide"] = true;
                                    pData["orderTime"] = resultDate.Value.ToShortDateString() + " " + resultDate.Value.ToShortTimeString();
                                    pData["address"] = order.PickUpAddress;
                                    //NotificationsServices.Current.DriverNotification(driver.User, DriverNotificationTypes.UserCancelRideRequest, orderId, pData);
                                }
                                else
                                {
                                    pData["isFutureRide"] = false;
                                }
                                db.SaveChanges();
                                NotificationsServices.Current.DriverNotification(driver.User, DriverNotificationTypes.UserCancelRideRequest, orderId, pData);

                                //Utils.SendMail(new List<string>() { driver.User.Email}, )
                            }

                            //send push message to All drivers that registered for this order except driver that accepted (we dealt with him above code block)
                            // & change status
                            foreach (Orders_Drivers order_driver in order.Orders_Drivers)
                            {
                                //if ((order.DriverId.HasValue && order_driver.DriverId != order.DriverId.Value) || !order.DriverId.HasValue)
                                //{
                                //    NotificationsServices.Current.DriverNotification(order_driver.Driver.User, DriverNotificationTypes.UserCancelRideRequest, orderId);
                                //}

                                if (order_driver.StatusId == (int)Order_DriverStatus.Accepted && order_driver.Driver.Status == (int)DriverStatus.PendingAcceptRequest)
                                    order_driver.Driver.Status = (int)DriverStatus.Available;
                                if (order_driver.StatusId != (int)Order_DriverStatus.Declined)
                                    order_driver.StatusId = (int)Order_DriverStatus.Cancelled;
                            }

                            //clear status of driver 0 if he didn't accept yet. Make sure there is a driver in order_driver...
                            if (!order.DriverId.HasValue)
                            {
                                var driver0 = db.Orders_Drivers.ByOrder(orderId).ByPriority(0).Select(u => u.DriverId).SingleOrDefault();
                                if (driver0 != 0)
                                {
                                    var driver = db.Drivers.GetById(driver0);
                                    if (driver.Status == (int)DriverStatus.HasRequestAsFirst)
                                        driver.Status = (int)DriverStatus.Available;
                                }
                            }
                            if (order.isFromWeb == true && order.Passenger.DeviceId == null)
                            {
                                UserService.SendSMSNotif(order.PassengerId, order.Passenger.Phone, SMSType.riderCancelled, order.Passenger.LanguageId);
                            }
                            //send email for the passenger that canceled the ride:
                            //Dictionary<int, string> lTexts = Utils.PrepareToSendEmailForPassenger(PassengerEmail.CanceledTripTitle, order.Passenger.LanguageId);
                            //var massage = "";
                            //for (int i = 1; i < lTexts.Count; i++)
                            //{
                            //    massage += "<h3>" + lTexts[i] + "</h3>";
                            //}
                            //var isSend = Utils.SendMail(new List<string>() { order.Passenger.Email }, lTexts[0], massage, null, order.Passenger.LanguageId);
                            //if (isSend == false)
                            //    Logger.ErrorFormat("error when sending email for passenger: {0} about CancelTrip. email:{1}", order.PassengerId, order.Passenger.Email);
                            //else
                            //    Logger.DebugFormat("email for passenger: {0} about CancelTrip success. email:{1}", order.PassengerId, order.Passenger.Email);


                            if (order.DriverId.HasValue)
                            {
                                ////send email for the driver that wanted the ride:
                                //var driver = db.Drivers.Include("User").GetById(order.DriverId.Value);
                                //Dictionary<int, string> lTextsDriver = Utils.PrepareToSendEmail(DriverEmail.CanceledTripTitle, order.Driver.User.LanguageId);
                                ////var isSend=Utils.SendMail(new List<string>() { driver.User.Email}, lTexts[0], )

                                //massage = "";
                                //for (int i = 1; i < lTextsDriver.Count; i++)
                                //{
                                //    massage += "<h3>" + lTextsDriver[i] + "</h3>";
                                //}
                                //var isSend1 = Utils.SendMail(new List<string>() { driver.User.Email }, lTextsDriver[0], massage, null, order.Driver.User.LanguageId);
                                //if (isSend1 == false)
                                //    Logger.ErrorFormat("error when sending email for driver: {0} about CancelTrip. email:{1}", driver.UserId, driver.User.Email);
                                //else
                                //    Logger.DebugFormat("email for driver: {0} about CancelTrip success. email:{1}", driver.UserId, driver.User.Email);
                            }
                        }
                        else
                            throw new OrderNotFoundException();
                    }
                    else
                        throw new orderCannotCanceledException();
                }
                else
                    throw new UserNotExistException();

                db.SaveChanges();
                return orderId;
            }
        }

        public static Order CheckFutureRideForDriver(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var dateAddMinute = DateTime.UtcNow.AddMinutes(1);
                var dateNow = DateTime.UtcNow.AddMinutes(-15);
                Logger.Debug("CheckFutureRideForDriver dateAddMinute " + dateAddMinute + " dateNow" + dateNow);

                var order = db.Orders
                            .Where(o => o.DriverId == userId && o.StatusId == (int)OrderStatus.Confirmed && o.FlowStep == (int)FlowSteps.Step5 && o.OrderTime <= dateAddMinute && o.OrderTime > dateNow).FirstOrDefault();
                return order;
            }
        }

        public static Order CheckFutureRide(long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var dateNow = DateTime.UtcNow.AddMinutes(-15);
                var dateAddMinute = DateTime.UtcNow.AddMinutes(1);
                var order = db.Orders.Where(o => o.PassengerId == userId && o.StatusId == (int)OrderStatus.Confirmed && o.DriverId != null && o.FlowStep == (int)FlowSteps.Step5 && o.OrderTime <= dateAddMinute && o.OrderTime > dateNow).FirstOrDefault();
                return order;
            }
        }

        public static void CancelOrderByDriver(long orderId, long driverUserId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Drivers.GetById(driverUserId);
                var order = db.Orders.GetById(orderId);
                var passenger = db.Users.GetById(order.PassengerId);

                if (driver != null)
                {
                    if (order != null)
                    {
                        //check it is cancelled by correct driver
                        if (order.DriverId.HasValue && (order.DriverId != driverUserId))
                            throw new UserUnauthorizedException();

                        //this order already has driver - the driver accept on this ride
                        if (order.DriverId.HasValue)
                        {
                            order.StatusId = (int)OrderStatus.Canceled;
                            driver.Status = (int)DriverStatus.Available;
                            if (passenger != null)
                                NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.DriverCancelledRide, orderId);
                        }
                        else
                            throw new OrderNotFoundException();

                        //change Order_Driver records to Cancelled
                        foreach (var orderDriver in order.Orders_Drivers)
                        {
                            orderDriver.StatusId = (int)Order_DriverStatus.Cancelled;
                        }
                    }
                    else
                        throw new OrderNotFoundException();
                }
                else
                    throw new UserNotExistException();

                db.SaveChanges();
            }
        }

        public static DriverQueue AcceptOrder(long orderId, long driverId, int? reminderSeconds)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);

                if (order == null)
                    throw new OrderNotFoundException();

                //this order already has driver
                if (order.DriverId.HasValue)
                {
                    Logger.ErrorFormat("AcceptOrder: OrderId:{0}, can't be accepted by driver id:{1}, after confirmed to driver id:{2}", orderId, driverId, order.DriverId.Value);
                    throw new ExpirationException();
                }

                if ((order.StatusId == (int)OrderStatus.Canceled) || (order.StatusId == (int)OrderStatus.Dissatisfied))
                {
                    Logger.ErrorFormat("AcceptOrder: OrderId:{0}, can't be accepted by driver id:{1}, because it is cancelled", orderId, driverId);
                    var driverForCancel = db.Drivers.GetById(driverId);
                    //changed by Shoshana on 17/01/18
                    //if (order.StatusId == (int)OrderStatus.Canceled)
                    //    NotificationsServices.Current.DriverNotification(driverForCancel.User, DriverNotificationTypes.UserCancelRideRequest, orderId);
                    throw new ExpirationException();
                }

                var order_driver = order.Orders_Drivers.Where(x => x.DriverId == driverId).SingleOrDefault();
                var driver = db.Drivers.GetById(driverId);
                if (order_driver != null && driver != null)
                {
                    order_driver.StatusId = (int)Order_DriverStatus.Accepted;
                    //var dateAddMinutes = DateTime.UtcNow.AddMinutes(15);
                    //if (order.OrderTime >= dateAddMinutes)
                    //    driver.Status = (int)DriverStatus.Available;
                    //else
                    driver.Status = (int)DriverStatus.PendingAcceptRequest;
                    db.SaveChanges();
                    AllocateRide(orderId);
                }
                else
                    throw new UserForbiddenException();

            }
            //If Driver did accept within 12 seconds, and he's not driver0 - he just needs to know he is in queue.
            using (var db1 = new BallyTaxiEntities().AutoLocal())
            {
                var order = db1.Orders.GetById(orderId); //get updated order details
                if (order == null)
                    throw new OrderNotFoundException();
                Logger.DebugFormat("order.DriverId {0}", order.DriverId);
                if (order.DriverId == driverId) // this driver was allocated the ride
                    return DriverQueue.GotRide;
                else
                    return DriverQueue.InQueue;
            }

            //return the closest ride of this driver TODO
            //return DriverService.GetNextOrder(driverId, null);               
        }

        public static object IVROrder(DbGeography location, string pickupCity, string phoneFormatted, string address, double orderTime)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                User Passenger = db.Users.Where(u => u.Phone == phoneFormatted && u.Driver == null).FirstOrDefault();

                if (Passenger == null)
                {

                    User new_user = db.Users.Create();
                    new_user.Name = null;
                    new_user.Email = null;
                    new_user.Active = true;
                    new_user.RegistrationDate = DateTime.UtcNow;
                    new_user.Phone = phoneFormatted;
                    new_user.AuthenticationToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                    new_user.LanguageId = (int)UserLanguages.he;
                    new_user.DriverValidNotificationToken = false;
                    new_user.PassengerValidNotificationToken = false;
                    new_user.isFromIVR = true;

                    db.Users.Add(new_user);
                    db.SaveChanges();
                    Passenger = new_user;
                }

                var dateFormatted = orderTime.ConvertFromUnixTimestamp();

                var orderResult = CreateOrder(location.Latitude.Value, location.Longitude.Value, address, 0, 0, null, Passenger.UserId, null, dateFormatted, null, pickupCity, null, null, false, 0, false, 4, null, null, 0, false, 1, (int)CustomerPaymentMethod.Cash, true);

                //var orderResult = CreateOrder(location.Latitude.Value, location.Longitude.Value, address, 0, 0, null, Passenger.UserId, null, dateFormatted, (int)CustomerPaymentMethod.Cash, null, pickupCity, null, null, false, 0, false, 4, null, null, 0, false, 1, true);
                if (orderResult > 0)
                {
                    var order = db.Orders.GetById(orderResult);
                    do
                    {
                        using (var db1 = new BallyTaxiEntities().AutoLocal())
                        {
                            order = db1.Orders.Include(d => d.Driver).Include(d => d.Driver.User).Where(o => o.OrderId == orderResult).FirstOrDefault();
                        }

                        System.Threading.Thread.Sleep(2000);
                    } while (order.StatusId == (int)OrderStatus.Pending);

                    //after time out or when found driver:

                    if (order.StatusId == (int)OrderStatus.Dissatisfied)//driver not found
                    {
                        return new { hasDriver = false };
                    }
                    else if (order.StatusId == (int)OrderStatus.Confirmed)
                    {
                        dynamic time = null;
                        if (order.OrderTime <= DateTime.UtcNow.AddMinutes(15))//if not future ride:
                        {
                            var objectTime = GetEstimationTimeForPassenger(order.OrderId, (int)UserLanguages.he, order.PassengerId);

                            var TimeResult = objectTime as dynamic;
                            time = TimeResult.GetType().GetProperty("estimateTime") != null ? TimeResult.estimateTime : null;
                        }

                        return new { hasDriver = true, driverPhone = order.Driver.User.Phone, driverName = order.Driver.User.Name, time = time };
                    }
                    else
                    {
                        return new { hasDriver = false };
                    }
                }

                return null;
            }

        }

        //private static void PassengerNotification1(long orderId, Order order)
        //{

        //    //List<User> users = new List<User>();
        //    //users.Add(order.Passenger.User);
        //    //var data = new Dictionary<string, object>();
        //    //data.Add("type", (int)PassengerNotificationTypes.DriverFound);
        //    //data.Add("rideID", orderId);
        //    //ResourceManager rm = new ResourceManager("Quickode.BallyTaxi.Domain.Resource.BallyTaxiText", Assembly.GetExecutingAssembly());
        //    //CultureInfo culture = CultureInfo.CreateSpecificCulture(order.Passenger.User.Language.LanguageCulture);
        //    //string dateString = rm.GetString(PassengerNotificationTypes.DriverFound.ToString(), culture);
        //    //NotificationsServices.Current.SendNotifications(users, dateString, data, "default", UserType.Passenger);
        //}

        public static long DeclineOrder(long orderId, long driverId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders
                    //.Include(x => x.Drivers)
                    //.Include(x => x.User.Language)
                    //.Include(x => x.User1.Language)
                    .Where(x => x.OrderId == orderId).SingleOrDefault();
                if (order != null)
                {
                    if (order.StatusId == (int)OrderStatus.Canceled)
                    {
                        Logger.ErrorFormat("DeclineOrder: OrderId:{0}, can't be declined by driver id:{1}, because it is cancelled", orderId, driverId);
                        throw new ExpirationException();
                    }
                    if (order.DriverId.HasValue && (order.DriverId != driverId))
                    {
                        Logger.ErrorFormat("DeclineOrder: OrderId:{0}, can't be declined by driver id:{1}, because driver already got message that it's not his ride", orderId, driverId);
                        throw new ExpirationException();
                    }
                    if (order.StatusId == (int)OrderStatus.DriverDeclined)
                    {
                        Logger.ErrorFormat("DeclineOrder: OrderId:{0}, can't be declined by driver id:{1}, because it is already declined", orderId, driverId);
                        throw new ExpirationException();
                    }
                    var dateMore30Minutes = DateTime.UtcNow.AddMinutes(30);
                    if (order.OrderTime < dateMore30Minutes && (order.FlowStep == (int)FlowSteps.Step4 || order.FlowStep == (int)FlowSteps.Step5))
                    {
                        return 0;
                    }

                    var Pdata = new Dictionary<string, object>();
                    Pdata.Add("driverName", order.Driver.User.Name);
                    Pdata.Add("driverPhone", order.Driver.User.Phone);

                    var futureDate = DateTime.UtcNow.AddMinutes(15);
                    if (order.OrderTime > futureDate)
                    {
                        DateTime? resultDate = OrderService.convertToLocalTime(order.OrderTime.Value.ConvertToUnixTimestamp(), order.PickUpLocation);
                        if (resultDate == null)
                            resultDate = order.OrderTime.Value.AddHours(3);//for jerusalem

                        //DateTime convertedDate = order.OrderTime.Value.AddHours(3);
                        Pdata["isFutureRide"] = true;
                        Pdata["orderTime"] = resultDate.Value.ToShortDateString() + " " + resultDate.Value.ToShortTimeString();
                        Pdata["address"] = order.PickUpAddress;
                    }
                    else
                    {
                        Pdata["isFutureRide"] = false;
                    }

                    var passengerUser = db.Users.GetById(order.PassengerId);

                    NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.DriverCancelledRide, orderId, extraInfo: Pdata);
                    var order_driver = order.Orders_Drivers.Where(x => x.DriverId == driverId).SingleOrDefault();
                    var thisdriver = db.Drivers.Include("User").GetById(driverId);
                    if (order_driver != null && thisdriver != null)
                    {
                        order_driver.StatusId = (int)Order_DriverStatus.Declined;
                        if (order.OrderTime < futureDate)
                            thisdriver.Status = (int)DriverStatus.Available;
                        //order.FlowStep = (int)FlowSteps.Step1;
                        order.StatusId = (int)OrderStatus.DriverDeclined;
                        order.isReadTheOrder = false;
                        order.isReadTheOrderForDriver = false;
                        //order.CreationDate = DateTime.Now;
                        order.DriverId = null;
                        //order.LastUpdateFlowStep = DateTime.UtcNow;
                        db.SaveChanges();

                        #region send email to driver that decline the ride:

                        //send the email in new task:

                        //new Task(() =>
                        //{
                        //    using (var db1 = new BallyTaxiEntities().AutoLocal())
                        //    {

                        //        var textForEmail = Utils.PrepareToSendEmail(DriverEmail.CanceledTripTitle, thisdriver.User.LanguageId);
                        //        var massage = "";
                        //        for (int i = 1; i < textForEmail.Count; i++)
                        //        {
                        //            massage += "<h3>" + textForEmail[i] + "</h3>";
                        //        }
                        //        var isSend = Utils.SendMail(new List<string>() { thisdriver.User.Email }, textForEmail[0], massage, null, thisdriver.User.LanguageId);
                        //        if (isSend == false)
                        //            Logger.ErrorFormat("error when sending email for driver: {0} about CancelTrip. email:{1}", thisdriver.UserId, thisdriver.User.Email);
                        //        else
                        //            Logger.DebugFormat("email for driver: {0} about CancelTrip success. email:{1}", thisdriver.UserId, thisdriver.User.Email);

                        //        //send email for the passenger that canceled the ride:
                        //        var thisPassenger = db1.Users.Where(p => p.UserId == order.PassengerId).FirstOrDefault();
                        //        if (thisPassenger != null && thisPassenger.Email != null)
                        //        {
                        //            Dictionary<int, string> lTexts = Utils.PrepareToSendEmailForPassenger(PassengerEmail.CanceledTripTitle, thisPassenger.LanguageId);
                        //            var massage1 = "";
                        //            for (int i = 1; i < lTexts.Count; i++)
                        //            {
                        //                massage1 += "<h3>" + lTexts[i] + "</h3>";
                        //            }
                        //            var isSend1 = Utils.SendMail(new List<string>() { thisPassenger.Email }, lTexts[0], massage1, null, thisPassenger.LanguageId);
                        //            if (isSend1 == false)
                        //                Logger.ErrorFormat("error when sending email for passenger: {0} about CancelTrip. email:{1}", thisPassenger.UserId, thisPassenger.Email);
                        //            else
                        //                Logger.DebugFormat("email for passenger: {0} about CancelTrip success. email:{1}", order.PassengerId, thisPassenger.Email);
                        //        }
                        //    }
                        //    Logger.DebugFormat("the email in decline order ended in order: " + order.OrderId.ToString());
                        //}).Start();
                        #endregion

                        return order.OrderId;
                        //AllocateRide(orderId);
                    }
                    else
                    {
                        Logger.ErrorFormat("DeclineOrder: OrderId:{0},  driver id:{1}, throw UserForbiddenException", orderId, driverId);
                        throw new UserForbiddenException();
                    }

                }
                else
                    throw new OrderNotFoundException();

            }
        }

        public static bool ConnectOrderToSpecificDriver(long userId, long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var user = db.Users.GetById(userId);
                var order = db.Orders.GetById(orderId);
                if (order != null)
                {
                    order.StatusId = (int)OrderStatus.Pending;
                    order.CreationDate = DateTime.UtcNow;
                    order.FlowStep = (int)FlowSteps.Step1;
                    order.LastUpdateFlowStep = DateTime.UtcNow;
                    order.isReadTheOrder = false;
                    order.isReadTheOrderForDriver = false;
                    db.SaveChanges();

                    string address1 = "";
                    string address2 = "";
                    string cityEN = "";
                    if (user.LanguageId == (int)UserLanguages.he)
                    {
                        var addressHE = getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 1);
                        if (addressHE != null)
                        {
                            address1 = addressHE[0];
                        }
                    }
                    else
                    {
                        var addressEN = getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 2);
                        if (addressEN != null)
                        {
                            address2 = addressEN[0];
                            if (addressEN.Count() > 1 && addressEN[1] != null)
                                cityEN = addressEN[1];
                        }
                    }


                    var dateForFutureRide = DateTime.UtcNow.AddMinutes(30);
                    var dateFilter = DateTime.UtcNow.AddMinutes(-(ConfigurationHelper.UPDATE_MINUTES / 2) + 1);
                    var availDrivers = db.Drivers
                                   .Where(d => d.UserId == user.UserId)
                                   .ToList();

                    if (availDrivers != null && availDrivers.Count > 0)
                    {
                        Logger.Info("Found drivers");
                        //anyOperation = true;

                        var firstDriver = availDrivers.First();
                        //added to check because the FK:
                        if (db.Orders_Drivers.Where(o => o.DriverId == firstDriver.UserId && o.OrderId == order.OrderId).FirstOrDefault() == null)
                            db.Orders_Drivers.Add(new Orders_Drivers()
                            {
                                DriverId = firstDriver.UserId,
                                OrderId = order.OrderId,
                                StatusId = (int)Order_DriverStatus.SentPush,
                                Priority = 0
                            });

                        firstDriver.Status = (int)DriverStatus.HasRequestAsFirst;
                        order.FlowStep = (int)FlowSteps.Step2;
                        order.LastUpdateFlowStep = DateTime.UtcNow;

                        db.SaveChanges();



                        //Notify driver on drive, as first driver
                        var location = new LocationObject()
                        {
                            lat = order.PickUpLocation.Latitude,
                            lon = order.PickUpLocation.Longitude,
                            address = order.PickUpAddress
                        };
                        var data = new Dictionary<string, object>();
                        data.Add(Constants.NotificationKeys.DriverPriorty, 0);
                        data.Add("location", location);
                        data.Add("TimeFrame", ConfigurationHelper.MaxSecondsFirstDriverOfferedOrder);

                        var firstDriverUser = db.Users.GetById(firstDriver.UserId);
                        data.Add("address", firstDriverUser.LanguageId == 1 ? address1 : address2);
                        NotificationsServices.Current.DriverNotification(firstDriverUser, DriverNotificationTypes.NewRideRequest, order.OrderId, data);
                        Logger.DebugFormat("Driver #0 ({1}), got push notification about order{0}", order.OrderId, firstDriverUser.DeviceId);
                        //var otherDrivers = new List<long>();
                        return true;
                    }
                }
                return false;
            }
        }

        public static int reCreateOrderToSpecificRegion(int regionId, long orderId, int roleId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order != null)
                {
                    order.StatusId = (int)OrderStatus.Pending;
                    order.CreationDate = DateTime.UtcNow;
                    order.FlowStep = (int)FlowSteps.Step1;
                    order.LastUpdateFlowStep = DateTime.UtcNow;
                    order.isReadTheOrder = false;
                    order.isReadTheOrderForDriver = false;
                    db.SaveChanges();

                    string address1 = "";
                    string address2 = "";
                    string cityEN = "";
                    var addressHE = getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 1);
                    if (addressHE != null)
                    {
                        address1 = addressHE[0];
                    }
                    var addressEN = getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 2);
                    if (addressEN != null)
                    {
                        address2 = addressEN[0];
                        if (addressEN.Count() > 1 && addressEN[1] != null)
                            cityEN = addressEN[1];
                    }

                    //var availableRadius = cityEN == "Jerusalem" ? ConfigurationHelper.AVAILABLE_RADIUS_JERUSALEM : cityEN == "Tel Aviv-Yafo" ? ConfigurationHelper.AVAILABLE_RADIUS_TelAviv : cityEN == "Ashdod" ? ConfigurationHelper.AVAILABLE_RADIUS_Ashdod : cityEN == "Haifa" ? ConfigurationHelper.AVAILABLE_RADIUS_Haifa : ConfigurationHelper.AVAILABLE_RADIUS;



                    var dateForFutureRide = DateTime.UtcNow.AddMinutes(30);
                    var dateFilter = DateTime.UtcNow.AddMinutes(-(ConfigurationHelper.UPDATE_MINUTES / 2) + 1);
                    var availDrivers = db.Drivers
                                   //for inter city travel:
                                   //.Where(d => ((d.TaxiStation.StationId == 297 || d.TaxiStation.StationId == 338 /*מוניות הדקה ה99*/ ) && order.isWithDiscount == true) || (order.isWithDiscount == false))
                                   //isHandicapped:
                                   .isHandicapped(order.isHandicapped)
                                   //courier:
                                   .courier(order.courier)
                                   //.isIn99minuteCompAndInInterCity(order.isInterCity)
                                   .AvailableToDrive()
                                   .ThatAreActive()
                                   .WantFutureRide()
                                   .bySeats(order)
                                   .notAlreadyDeclined(order.OrderId)
                                   //TODO:must check for same country!!!
                                   //.NearNew(order.PickUpLocation, (order.courier.HasValue && order.courier.Value > 0) ? ConfigurationHelper.AVAILABLE_RADIUS_FORCourier : (order.isHandicapped.HasValue && order.isHandicapped.Value == true) ? ConfigurationHelper.AVAILABLE_RADIUS_FORHandicapped : (order.isInterCity.HasValue && order.isInterCity.Value == true) ? ConfigurationHelper.AVAILABLE_RADIUS_FORIntercityRide : /*ConfigurationHelper.AVAILABLE_RADIUS*/ availableRadius, order.OrderTime > dateForFutureRide)
                                   .inRegion(regionId)
                                    .Where(d => d.LastUpdateLocation >= dateFilter)
                                   //.BYPaymentMethod(order.PaymentMethod.Value)
                                   .OrderBy(x => x.DriverRegions.Where(d => d.regionId == regionId).Select(d => d.driverRegionId).FirstOrDefault())
                                    .Take(1)
                                   .ToList();
                    if (order.isWithDiscount == true)
                    {
                        availDrivers = availDrivers.OrderByDescending(x => (x.TaxiStationId == 297 || x.TaxiStationId == 338)).ToList();
                    }
                    availDrivers = filterDriversByRole(availDrivers, roleId);

                    if (availDrivers != null && availDrivers.Count > 0)
                    {
                        Logger.Info("Found drivers");
                        //anyOperation = true;

                        var firstDriver = availDrivers.First();
                        //added to check because the FK:
                        if (db.Orders_Drivers.Where(o => o.DriverId == firstDriver.UserId && o.OrderId == order.OrderId).FirstOrDefault() == null)
                            db.Orders_Drivers.Add(new Orders_Drivers()
                            {
                                DriverId = firstDriver.UserId,
                                OrderId = order.OrderId,
                                StatusId = (int)Order_DriverStatus.SentPush,
                                Priority = 0
                            });
                        else
                        {
                            var orderDriver = order.Orders_Drivers.Where(d => d.DriverId == firstDriver.UserId).FirstOrDefault();
                            orderDriver.isReadTheOrderForDriver = false;
                        }

                        firstDriver.Status = (int)DriverStatus.HasRequestAsFirst;
                        order.FlowStep = (int)FlowSteps.Step2;
                        order.LastUpdateFlowStep = DateTime.UtcNow;

                        db.SaveChanges();



                        //Notify driver on drive, as first driver
                        var location = new LocationObject()
                        {
                            lat = order.PickUpLocation.Latitude,
                            lon = order.PickUpLocation.Longitude,
                            address = order.PickUpAddress
                        };
                        var data = new Dictionary<string, object>();
                        data.Add(Constants.NotificationKeys.DriverPriorty, 0);
                        data.Add("location", location);
                        data.Add("TimeFrame", ConfigurationHelper.MaxSecondsFirstDriverOfferedOrder);

                        var firstDriverUser = db.Users.GetById(firstDriver.UserId);
                        data.Add("address", firstDriverUser.LanguageId == 1 ? address1 : address2);
                        NotificationsServices.Current.DriverNotification(firstDriverUser, DriverNotificationTypes.NewRideRequest, order.OrderId, data);
                        Logger.DebugFormat("Driver #0 ({1}), got push notification about order{0}", order.OrderId, firstDriverUser.DeviceId);
                        //var otherDrivers = new List<long>();
                    }
                    else
                    {
                        Logger.DebugFormat("no driver found for order {0}", order.OrderId);
                        HandleSearchDrivers(order, 1); //roleId //not send the roleId send the admin
                        //bacause no taxi exist in this region 
                    }
                    return 1;
                }
                else
                {
                    throw new OrderNotFoundException();
                }
            }
        }

        private static List<Driver> filterDriversByRole(List<Driver> availDrivers, int roleId)
        {
            switch (roleId)
            {
                case (int)RoleAccountForAdmin.admin:
                    {
                        return availDrivers;
                    }
                case (int)RoleAccountForAdmin.shekem:
                    {
                        return availDrivers.Where(o => o.TaxiStationId == ConfigurationHelperForStations.ShekemTaxi).ToList();
                    }
                case (int)RoleAccountForAdmin.kastle:
                    {
                        return availDrivers.Where(o => o.TaxiStationId == ConfigurationHelperForStations.KasstleTaxi).ToList();
                    }
                default:
                    break;
            }
            return null;
        }

        public static void AllocateRide(long orderId)
        {
            //allocate ride!
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.Include(c => c.Driver).Include(u => u.Driver.User).Include(c => c.Passenger).GetById(orderId);
                // var driver0id = db.Orders_Drivers.ByOrder(order.OrderId).ByPriority(0).Select(x => x.DriverId).SingleOrDefault();
                // var order_driver0Status = db.Orders_Drivers.ByOrder(order.OrderId).ByPriority(0).Select(u => u.StatusId).SingleOrDefault();
                long driverId = 0;
                // bool withinInitSeconds = false;
                bool orderTimout = false;

                //if (order.CreationDate.AddSeconds(ConfigurationHelper.MaxSecondsFirstDriverOfferedOrder) > DateTime.UtcNow)
                //    withinInitSeconds = true;
                if ((order.CreationDate.AddSeconds(order.isIVROrder != true ? ConfigurationHelper.MAX_ORDER_WAIT_SECONDS : ConfigurationHelper.MAX_ORDER_WAIT_SECONDS_IVR) < DateTime.UtcNow))
                {
                    //withinInitSeconds = false;
                    orderTimout = true;
                }
                // within 12 seconds and driver 0 accepted ==> goes to driver0
                //if (withinInitSeconds && order.StatusId == (int)OrderStatus.Pending && (order_driver0Status == (int)Order_DriverStatus.Accepted))
                //    driverId = driver0id;

                // within 12 seconds and driver 0 hasn't accepted or decline => wait
                //else if (withinInitSeconds && order.StatusId == (int)OrderStatus.Pending && (order_driver0Status == (int)Order_DriverStatus.SentPush))
                //    return;

                // between 12 seconds and timeout: it doesn't matter what driver0 did (need to clear driver0 status to available)
                //OR driver0 declined - and then the ride is open to everyone
                //=> goes to first in line that accepted AND IS FREE !!!!!!!!!!!!!!
                if (!orderTimout && order.StatusId == (int)OrderStatus.Pending)
                    driverId = db.Orders_Drivers.Accepted(orderId).OrderBy(x => x.Priority).Select(u => u.DriverId).Take(1).SingleOrDefault(); //get first driver that accepted               

                if (driverId != 0)
                {
                    //send notification to driver
                    order.DriverId = driverId;
                    order.StatusId = (int)OrderStatus.Confirmed;
                    order.FlowStep = (int)FlowSteps.Step3;
                    var driver = db.Drivers.Where(x => x.UserId == order.DriverId.Value).SingleOrDefault();
                    var dateFuture = DateTime.UtcNow.AddMinutes(15);
                    if (order.OrderTime > dateFuture)
                    {
                        driver.Status = (int)DriverStatus.Available;
                    }
                    else
                    {
                        driver.Status = (int)DriverStatus.OnTheWayToPickupLocation;
                    }
                    var DriverAllocated = new List<User>();
                    DriverAllocated.Add(db.Users.GetById(driverId));

                    var data = new Dictionary<string, object>();
                    if (order.Passenger.ImageId != null)
                        data.Add("ImageId", order.Passenger.ImageId);
                    if (!string.IsNullOrEmpty(order.Passenger.Name))
                        data.Add("Name", order.Passenger.Name);
                    data.Add("Phone", order.Passenger.Phone);
                    db.SaveChanges();

                    NotificationsServices.Current.DriversNotification(DriverAllocated, DriverNotificationTypes.RideAllocated, orderId, data);

                    /* TODO - support future rides

                    //if this current ride (less hour from now) change driver status to OnTheWayToPickupLocation
                    if (order.OrderTime.HasValue && 0 < (order.OrderTime.Value - DateTime.UtcNow).TotalHours && (order.OrderTime.Value - DateTime.UtcNow).TotalHours < 1)
                    {
                        var driver = db.Drivers.Where(x => x.UserId == order.DriverId.Value).FirstOrDefault();
                        driver.Status = (int)DriverStatus.OnTheWayToPickupLocation;
                        var DriverAllocated = new List<User>();
                        DriverAllocated.Add(db.Users.GetById(driverId));
                        DriversNotification(DriverAllocated, DriverNotificationTypes.RideAllocated, orderId);
                    }
                    //in future ride
                    else if (order.OrderTime.HasValue && (order.OrderTime.Value - DateTime.UtcNow).TotalHours >= 1)
                    {
                        //TODO make service to send reminderForFutureRide push on reminderSeconds from order time
                        var seconds = reminderSeconds.HasValue ? reminderSeconds.Value : ConfigurationHelper.REMINDER_SECONDS;
                        //int seconds = 0; 
                        //if (reminderSeconds.HasValue)
                        //{
                        //    seconds = reminderSeconds.Value; 
                        //}
                        //else {
                        //    seconds = int.Parse(ConfigurationHelper.REMINDER_SECONDS); 
                        //}
                    }*/


                    //send push message to passenger
                    var passengerUser = db.Users.GetById(order.PassengerId);
                    if (order.isFromWeb == true && order.Passenger.DeviceId == null)
                    {
                        var dataPass = new Dictionary<string, object>();

                        dataPass.Add("Phone", order.Driver.User.Phone);
                        bool status = UserService.SendSMSNotif(order.PassengerId, order.Passenger.Phone, SMSType.DriverFoundSMS, order.Passenger.LanguageId, dataPass);
                    }
                    else
                        NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.DriverFound, orderId);

                    //////////////////////////////////////////////
                    //check if  the payment will be with paypal and the passenger have permission to pay:
                    //כרגע צורת התשלום היא לפי בחירת הנוסע בסיום!! הנסיעה, האם זה רלוונטי 
                    if (order.PaymentMethod == (int)CustomerPaymentMethod.Paypal)//
                    {
                        if (order.transactionId == null)
                        {
                            Logger.DebugFormat("if transactionId, passengerUser: {0}", passengerUser.ToString());
                            NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.paypalPassengerError, orderId);
                            var driverForPush = db.Users.Where(u => u.UserId == order.DriverId).FirstOrDefault();
                            NotificationsServices.Current.DriverNotification(driverForPush, DriverNotificationTypes.paypalDriverError, orderId, data);
                            order.PaymentMethod = (int)CustomerPaymentMethod.Cash;

                            Logger.DebugFormat("allocateRide: order.PaymentMethod = (int)CustomerPaymentMethod.Cash;");
                        }
                    }

                    //////////////////////////////////////////////

                    //////////////////////////////////////////--/

                    if (order.PaymentMethod == (int)CustomerPaymentMethod.CreditCard)
                    {
                        Logger.DebugFormat("start to check if credit Card is correct in orderId: {0}", order.OrderId);
                        new Task(() =>
                        {
                            try
                            {
                                using (var db1 = new BallyTaxiEntities().AutoLocal())
                                {
                                    Logger.DebugFormat("start to check if credit Card is correct in thread in orderId in thread: {0}", order.OrderId);
                                    var creditCard = db1.CreditCardUsers.Where(c => c.userId == order.PassengerId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(c => c.isDefault).FirstOrDefault();
                                    //Logger.DebugFormat("start to check if credit Card is correct in thread in orderId in thread: {0} the card is: {1}", order.OrderId, creditCard.ToJson());

                                    if (creditCard != null)
                                    {
                                        //new Task(() =>
                                        //{
                                        var resultCheck = PaymentService.checkCardCorrect(creditCard.cvv, creditCard.expdate, 1, creditCard.tokenId);
                                        if (resultCheck == false)
                                        {
                                            Logger.DebugFormat("if transactionId, passengerId: {0}", order.PassengerId.ToString());
                                            var orderdb1 = db1.Orders.Where(o => o.OrderId == orderId).FirstOrDefault();
                                            orderdb1.PaymentMethod = (int)CustomerPaymentMethod.Cash;
                                            Logger.DebugFormat("allocateRide: orderdb1.PaymentMethod = (int)CustomerPaymentMethod.Cash;");

                                            db1.SaveChanges();
                                            var dataCard = new Dictionary<string, object>();
                                            dataCard.Add("ImageId", order.Passenger.ImageId);
                                            dataCard.Add("Name", order.Passenger.Name);
                                            dataCard.Add("Phone", order.Passenger.Phone);
                                            dataCard.Add("paymentMethod", orderdb1.PaymentMethod);
                                            NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.CreditCardPassengerError, orderId);
                                            var driverForPush = db1.Users.Where(u => u.UserId == order.DriverId).FirstOrDefault();
                                            NotificationsServices.Current.DriverNotification(driverForPush, DriverNotificationTypes.creaditCardDriverError, orderId, dataCard);
                                            Logger.DebugFormat("creditCard card for userId: {0} for orderId: {1} is not correct, payment method: {2}", orderdb1.PassengerId, orderdb1.OrderId, orderdb1.PaymentMethod);

                                            // order.PaymentMethod = (int)CustomerPaymentMethod.Cash;
                                        }
                                        //}).Start();
                                    }
                                }
                            }
                            catch (Exception e)
                            {

                                Logger.ErrorFormat("there is error in checkCardCorrect {0}:", e.Message);
                            }
                        }).Start();
                    }
                    ////////////////////////////////////////////--/

                    //if (order.PaymentMethod == (int)CustomerPaymentMethod.Business)
                    //    notifyIsrTaxiState(driverId, DriverStatus.OnTheWayToPickupLocation);

                    //send push message for All drivers that registered for this order & clear their status
                    var notifyToDrivers = new List<User>();
                    var orderDrivers = db.Orders_Drivers.ByOrder(orderId).ToList();
                    Logger.DebugFormat("driver that get the ride: {0}", driverId);

                    foreach (var od in orderDrivers)
                    {
                        if (od.DriverId == driverId) // the driver that got the ride.. dont need to change him!
                            continue;

                        // handle drives that did NOT get the ride: 
                        if ((od.StatusId == (int)Order_DriverStatus.SentPush) || (od.StatusId == (int)Order_DriverStatus.Accepted)) //don't notifiy whoever declines
                        {
                            var driverUser = db.Users.GetById(od.DriverId);
                            if (driverUser != null)
                            {
                                //need to clear status if driver0 or he accepted - otherwise he will stay in status PendingAcceptRequest / HasRequestAsFirst
                                if (od.StatusId == (int)Order_DriverStatus.Accepted || od.Priority == 0)
                                    driverUser.Driver.Status = (int)DriverStatus.Available;

                                //need to notify him!
                                notifyToDrivers.Add(driverUser);
                                //change status to skipped
                                od.StatusId = (int)Order_DriverStatus.Skipped;
                            }
                        }
                    }

                    if (notifyToDrivers.Count > 0)
                        NotificationsServices.Current.DriversNotification(notifyToDrivers, DriverNotificationTypes.RideNotRelevantAnymore, orderId);
                }

                // No Driver Allocated or timeout
                if (driverId == 0) //NOTE: can reach here within the 12 second time slot where driver0 hasnt responded yet...
                {
                    //case 1 - timeout has been reached..
                    if (orderTimout && (order.StatusId == (int)OrderStatus.Pending))
                    {
                        Logger.DebugFormat("No drivers accepted Old order: {0} in Step2. Change status to Dissatisfied.", orderId);
                        OrderTimedOut(db, order.OrderId);
                    }
                    //case 2 - All drivers did decline!! 
                    else if (order.StatusId == (int)OrderStatus.Pending)
                    {
                        List<Orders_Drivers> declined = (from od in order.Orders_Drivers
                                                         where od.OrderId == orderId
                                                         where od.StatusId == (int)Order_DriverStatus.Declined
                                                         select od).ToList();
                        if (declined.Count() == order.Orders_Drivers.Count()) //all the drivers did decline
                        {
                            Logger.DebugFormat("All drivers declined order: {0}. Change status to Dissatisfied.", orderId);
                            OrderTimedOut(db, order.OrderId);
                        }
                    }
                }
                db.SaveChanges();
            }
        }

        private static async void notifyIsrTaxiState(long driverId, DriverStatus driverStatus)
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var driver = db.Drivers.Where(d => d.UserId == driverId).FirstOrDefault();
                    //var isrtaxiStation=db.
                    var url = "http://apps.isrcorp.co.il:50594/RiderWS/public/notifyIsrTaxiState";
                    var isrStation = db.ISR_TaxiStations.Where(t => t.StationId == driver.TaxiStationId).FirstOrDefault();
                    var state = driverStatus == DriverStatus.OnTheWayToPickupLocation ? "busy" : "Free";
                    if (isrStation != null)
                    {
                        Logger.DebugFormat("start notifyIsrTaxiState driverId: {0}, orderId: {1}", driverId);
                        using (var client = new HttpClient())
                        {
                            var content = new FormUrlEncodedContent(new[]
                                   {
                            new KeyValuePair<string, string>("orgName", isrStation.StationName),
                            new KeyValuePair<string, string>("vid", driver.driverCode),
                            new KeyValuePair<string, string>("state", state)
                        });
                            var response = await client.PostAsync(url, content);
                            string resultContent = response.Content.ReadAsStringAsync().Result;
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            object result = jss.Deserialize<object>(resultContent);

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("error in notifyIsrTaxiState {0}", e.Message);
            }

        }

        public static void HandlePendingOrders()
        {
            Logger.Info("Start to handle Pending Orders");
            var numberOfErrors = 0;
            var numberOfLoops = 0;
            int numberOfMinutesToSend = 30 * 60000;

            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                numberOfMinutesToSend = Convert.ToInt32(db.SystemSettings.Where(s => s.ParamKey == "numberOfMinutesToSend21Notif").FirstOrDefault().ParamValue);
                Logger.InfoFormat("numberOfMinutesToSend", numberOfMinutesToSend);
            }

            Logger.InfoFormat("numberOfMinutesToSend", numberOfMinutesToSend);
            //send notifivations to android users to update their apps

            //changed by Shoshana on 17/01/18:
            //change this operation to Task Schduler
            var timer = new System.Threading.Timer((e) =>
                {
                    //שולח הודעה -נוטיפיקציה מוסתרת למכשירי אנדרואיד (הם באמת לא רואים את הכתוב) ואז כשהיא מתקבלת זה שולח לשרת משהו 
                    everyHalfAnHour();
                }, null, 0, numberOfMinutesToSend);
            //}).Start();

            while (true)//
            {
                var anyOperation = false;
                numberOfLoops++;
                try
                {
                    using (var db = new BallyTaxiEntities().AutoLocal())
                    {
                        // Since loop is performed every second, notify only 1:300 times (5 minutes) 
                        if (numberOfLoops % 300 == 0)
                            Logger.Info("Fetching pending orders");
                        #region // Handle step 1 orders

                        var o1 = new User();

                        //get orders where flowStep=1
                        var pendingOrders = db.Orders
                            .InStep(FlowSteps.Step1)
                            .Pending()
                            .ToList();

                        foreach (var order in pendingOrders)
                        {
                            //Logger.DebugFormat("{0}", pendingOrders.ToJson());
                            numberOfLoops = 0; // cause the next loop to print the "Fetching pending orders"
                            // and - at the and, if the order still didn't handled, check if it should be expired
                            if (order.StatusId == (int)OrderStatus.Pending && order.CreationDate.AddSeconds(order.isIVROrder != true ? ConfigurationHelper.MAX_ORDER_WAIT_SECONDS : ConfigurationHelper.MAX_ORDER_WAIT_SECONDS_IVR) < DateTime.UtcNow)
                            {
                                Logger.DebugFormat("No drivers found for Old order: {0} in Step 1. Change status to Dissatisfied.", order.OrderId);
                                OrderTimedOut(db, order.OrderId);
                            }
                            db.SaveChanges();
                        }
                        #endregion

                        #region // Handle step 2 orders: after sending notifications to 20 drivers
                        //TODO: 
                        pendingOrders = db.Orders
                            .InStep(FlowSteps.Step2)
                            .Pending()
                            .ToList();
                        foreach (var order in pendingOrders)
                        {
                            Logger.DebugFormat("Start to handle order:{0} in STEP2", order.OrderId);
                            AllocateRide(order.OrderId); //dealing with after time-out is within the method.    
                            numberOfLoops = 0;
                        }
                        #endregion

                        //futureRide: //
                        //==================!!!!!========================


                        //this is for the time to convert it to the correcy offset:
                        //https://developers.google.com/maps/documentation/timezone/intro
                        //https://maps.googleapis.com/maps/api/timezone/json?location=31.816635,35.1881003&timestamp=1489485881

                        // var dateAdd = DateTime.UtcNow.AddHours(1);
                        var nextHour = DateTime.UtcNow.AddHours(1);
                        // var date1 = DateTime.UtcNow.AddMinutes(10);
                        var in10Minutes = DateTime.UtcNow.AddMinutes(10);
                        //נסיעות עתידיות שאמורות להתבצע בין 10 דקות לשעה
                        var futeureRides = db.Orders.Where(o => o.OrderTime.Value <= nextHour && o.OrderTime.Value > in10Minutes && o.FlowStep == (int)FlowSteps.Step3 && o.StatusId != (int)OrderStatus.Canceled).ToList();

                        if (futeureRides != null && futeureRides.Count > 0)
                        {
                            //Logger.DebugFormat("the date add: {0} the orderId: {1} the orderTime: {2}", nextHour, futeureRides.FirstOrDefault().OrderId, futeureRides.FirstOrDefault().OrderTime);
                            Logger.DebugFormat("the nextHour: {0} the orderId: {1} the orderTime: {2}", nextHour, futeureRides.FirstOrDefault().OrderId, futeureRides.FirstOrDefault().OrderTime);
                            foreach (var futureOrder in futeureRides)
                            {
                                //ConfigurationHelper.REMINDER_SECONDS
                                if ((futureOrder.OrderTime.Value - DateTime.UtcNow).TotalHours < 1)//#######
                                {
                                    Logger.DebugFormat("futureOrder.OrderTime.Value - DateTime.UtcNow).TotalHours {0}", (futureOrder.OrderTime.Value - DateTime.UtcNow).TotalHours);
                                    futureOrder.FlowStep = (int)FlowSteps.Step4;
                                    db.SaveChanges();
                                    var data = new Dictionary<string, object>();
                                    //if (futureOrder.Passenger.ImageId != null)

                                    DateTime? resultDate = convertToLocalTime(futureOrder.OrderTime.Value.ConvertToUnixTimestamp(), futureOrder.PickUpLocation);
                                    if (resultDate == null)
                                        resultDate = futureOrder.OrderTime.Value.AddHours(3);//for jerusalem

                                    //CultureInfo ci = new CultureInfo("he-IL");
                                    //string date = futureOrder.OrderTime.Value.ToUniversalTime().ToString("R", ci);


                                    data.Add("orderTime", resultDate.Value.ToShortTimeString());
                                    //if (!string.IsNullOrEmpty(futureOrder.Passenger.Name))
                                    data.Add("Address", futureOrder.PickUpAddress);
                                    //data.Add("Phone", futureOrder.Passenger.Phone);

                                    //===================!!!!!!!!!!!==================
                                    NotificationsServices.Current.DriversNotification(new List<User>() { futureOrder.Driver.User }, DriverNotificationTypes.ReminderForFutureRide, futureOrder.OrderId, data);
                                    if (futureOrder.isFromWeb == true && futureOrder.Passenger.DeviceId == null)
                                        UserService.SendSMSNotif(futureOrder.PassengerId, futureOrder.Passenger.Phone, SMSType.ReminderForFutureRide, futureOrder.Passenger.LanguageId, data);
                                    else
                                        NotificationsServices.Current.PassengerNotification(futureOrder.Passenger, PassengerNotificationTypes.ReminderForFutureRide, futureOrder.OrderId, null, data);

                                    Logger.Debug("after sending remind 4 for future ride");
                                }
                            }
                        }
                        //?נסיעות עתידיות שאמורות להתרחש בדקה הקרובה
                        var dateAddMinute = DateTime.UtcNow.AddMinutes(1);
                        var currentFutureRides = db.Orders.Where(o => o.OrderTime <= dateAddMinute && o.OrderTime > DateTime.UtcNow && o.FlowStep == (int)FlowSteps.Step4 && o.StatusId != (int)OrderStatus.Canceled).ToList();
                        foreach (var currentFutureRide in currentFutureRides)
                        {
                            Logger.DebugFormat("the date add minutes: {0} the orderId: {1} the orderTime: {2}", dateAddMinute, currentFutureRides.FirstOrDefault().OrderId, currentFutureRides.FirstOrDefault().OrderTime);

                            currentFutureRide.FlowStep = (int)FlowSteps.Step5;
                            currentFutureRide.isReadTheOrder = false;
                            currentFutureRide.isReadTheOrderForDriver = false;

                            //האם זה נכון לעדכן אוטומטית את הסטטוס של הנהג ללא אישורו?
                            DriverService.UpdateDriverToOnTheWayToPickUp(currentFutureRide.DriverId.Value);
                            db.SaveChanges();

                            var data = new Dictionary<string, object>();
                            NotificationsServices.Current.DriversNotification(new List<User>() { currentFutureRide.Driver.User }, DriverNotificationTypes.recivedFutureRide, currentFutureRide.OrderId, data);
                            if (currentFutureRide.isFromWeb == true && currentFutureRide.Passenger.DeviceId == null)
                                UserService.SendSMSNotif(currentFutureRide.PassengerId, currentFutureRide.Passenger.Phone, SMSType.ReminderForFutureRide, currentFutureRide.Passenger.LanguageId, null);
                            else
                                NotificationsServices.Current.PassengerNotification(currentFutureRide.Passenger, PassengerNotificationTypes.DriverArriveInFutureRide, currentFutureRide.OrderId);
                            Logger.Debug("after sending remind 5 for future ride");
                        }


                        //====================================================
                        //handle massage for drivers and passengers:
                        if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverAndroid").FirstOrDefault().ParamValue == "0")
                        {
                            var drivers = db.Users.Where(u => u.Driver != null && u.PlatformId == (int)PlatformTypes.Android).ToList();
                            NotificationsServices.Current.DriversNotification(drivers, DriverNotificationTypes.massageForDriver, 0);

                            var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverAndroid").FirstOrDefault();
                            row.ParamValue = "1";
                            db.SaveChanges();
                        }
                        if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverIphone").FirstOrDefault().ParamValue == "0")
                        {
                            var drivers = db.Users.Where(u => u.Driver != null && u.PlatformId == (int)PlatformTypes.IOS).ToList();
                            NotificationsServices.Current.DriversNotification(drivers, DriverNotificationTypes.massageForDriver, 0);

                            var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverIphone").FirstOrDefault();
                            row.ParamValue = "1";
                            db.SaveChanges();
                        }
                        if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerAndroid").FirstOrDefault().ParamValue == "0")
                        {
                            var passengers = db.Users.Where(u => u.Driver == null && u.PlatformId == (int)PlatformTypes.Android).ToList();
                            foreach (var user in passengers)
                            {
                                NotificationsServices.Current.PassengerNotification(user, PassengerNotificationTypes.massageForPassenger, 0);
                            }
                            var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerAndroid").FirstOrDefault();
                            row.ParamValue = "1";
                            db.SaveChanges();
                        }
                        if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerIphone").FirstOrDefault().ParamValue == "0")
                        {
                            var passengers = db.Users.Where(u => u.Driver == null && u.PlatformId == (int)PlatformTypes.IOS).ToList();
                            foreach (var user in passengers)
                            {
                                NotificationsServices.Current.PassengerNotification(user, PassengerNotificationTypes.massageForPassenger, 0);
                            }
                            var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerIphone").FirstOrDefault();
                            row.ParamValue = "1";
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    numberOfErrors++;

                    //if (numberOfErrors > 100)
                    //    break;
                }

                if (!anyOperation)
                {
                    //TODO: Need to define for prod
                    System.Threading.Thread.Sleep(900);
                }

                anyOperation = false;

                //handle the location of drivers:
                //every half of hour send notification to android to check where are they:
                //if they not update there location not send to them.
            }

            //Logger.Info("Quit handling Pending Orders");
        }

        public static void updateAppsVersion()
        {//daily operation?
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                //handle massage for drivers and passengers:
                if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverAndroid").FirstOrDefault().ParamValue == "0")
                {
                    var drivers = db.Users.Where(u => u.Driver != null && u.PlatformId == (int)PlatformTypes.Android).ToList();
                    NotificationsServices.Current.DriversNotification(drivers, DriverNotificationTypes.massageForDriver, 0);

                    var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverAndroid").FirstOrDefault();
                    row.ParamValue = "1";
                    db.SaveChanges();
                }
                if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverIphone").FirstOrDefault().ParamValue == "0")
                {
                    var drivers = db.Users.Where(u => u.Driver != null && u.PlatformId == (int)PlatformTypes.IOS).ToList();
                    NotificationsServices.Current.DriversNotification(drivers, DriverNotificationTypes.massageForDriver, 0);

                    var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForDriverIphone").FirstOrDefault();
                    row.ParamValue = "1";
                    db.SaveChanges();
                }
                if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerAndroid").FirstOrDefault().ParamValue == "0")
                {
                    var passengers = db.Users.Where(u => u.Driver == null && u.PlatformId == (int)PlatformTypes.Android).ToList();
                    foreach (var user in passengers)
                    {
                        NotificationsServices.Current.PassengerNotification(user, PassengerNotificationTypes.massageForPassenger, 0);
                    }
                    var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerAndroid").FirstOrDefault();
                    row.ParamValue = "1";
                    db.SaveChanges();
                }
                if (db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerIphone").FirstOrDefault().ParamValue == "0")
                {
                    var passengers = db.Users.Where(u => u.Driver == null && u.PlatformId == (int)PlatformTypes.IOS).ToList();
                    foreach (var user in passengers)
                    {
                        NotificationsServices.Current.PassengerNotification(user, PassengerNotificationTypes.massageForPassenger, 0);
                    }
                    var row = db.SystemSettings.Where(s => s.ParamKey == "sendnotifToUpdateForPassengerIphone").FirstOrDefault();
                    row.ParamValue = "1";
                    db.SaveChanges();
                }
            }
        }

        public static DateTime? convertToLocalTime(double date, DbGeography location)//**********
        {
            if (location.Latitude == 0 && location.Longitude == 0)
            {
                //by default i put the location of israel
                location = Utils.LatLongToLocation(31.0461, 34.8516);
            }
            String url = String.Format("https://maps.googleapis.com/maps/api/timezone/json?location={0},{1}&timestamp={2}", location.Latitude, location.Longitude, date);
            //Pass request to google api with orgin and destination details
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(result))
                {
                    timeZoneFromLocation timezoneResult = result.FromJson<timeZoneFromLocation>(); //JsonConvert.DeserializeObject<Parent>(result);
                    if (timezoneResult.status == "OK")
                    {
                        var dateTime = date.ConvertFromUnixTimestamp();
                        dateTime = dateTime.AddSeconds(timezoneResult.rawOffset + timezoneResult.dstOffset);
                        return dateTime;
                    }
                }
            }
            return null;
        }

        public static void everyHalfAnHour()
        {

            //שולח הודעה -נוטיפיקציה מוסתרת למכשירי אנדרואיד (הם באמת לא רואים את הכתוב) ואז כשהיא מתקבלת זה שולח לשרת משהו 

            Logger.Debug("start to check the drivers that get will get the notification ");
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var numvberOfSending = Convert.ToInt32(db.SystemSettings.Where(s => s.ParamKey == "numvberOfSendingNotif21").FirstOrDefault().ParamValue);
                if (numvberOfSending == 3)
                {
                    var driverNotUpdate = db.Drivers.Where(d => d.UpdateLocationStatus == (int)updateLocationStatus.sent).ToList();
                    driverNotUpdate.ForEach(e => e.UpdateLocationStatus = (int)updateLocationStatus.notUpdateAfter3Times);

                    var row = db.SystemSettings.Where(s => s.ParamKey == "numvberOfSendingNotif21").FirstOrDefault();
                    row.ParamValue = (numvberOfSending + 1).ToString();

                    db.SaveChanges();
                }

                var dateMoreHalfAnHour = DateTime.UtcNow.AddMinutes(-3);
                //notUpdateAfter3Times נהגי אנדרואיד עם סטטוס שונה מ 
                var drivers = db.Users.Include("Driver")
                                        .Where(d => d.Driver != null &&
                                                          d.Driver.LastUpdateLocation <= dateMoreHalfAnHour &&
                                                          d.PlatformId == (int)PlatformTypes.Android
                                                        // && (d.Driver.UpdateLocationStatus == null || (d.Driver.UpdateLocationStatus.HasValue && (d.Driver.UpdateLocationStatus.Value == (int)updateLocationStatus.sent || d.Driver.UpdateLocationStatus.Value == (int)updateLocationStatus.updateLocation)))
                                                        //check what to do after 3 time he not update
                                                        )
                                                        .filterDriverNotUpdate(numvberOfSending)
                                                        .ToList();
                foreach (var user in drivers)
                {
                    user.Driver.UpdateLocationStatus = (int)updateLocationStatus.sent;
                    db.SaveChanges();
                    var data = new Dictionary<string, object>();
                    data["token"] = user.AuthenticationToken;
                    NotificationsServices.Current.DriversNotification(new List<User>() { user }, DriverNotificationTypes.sendToGetLocation, 0, data);
                }

                numvberOfSending++;
                if (numvberOfSending <= 3)
                {
                    var row = db.SystemSettings.Where(s => s.ParamKey == "numvberOfSendingNotif21").FirstOrDefault();
                    row.ParamValue = (numvberOfSending).ToString();
                    db.SaveChanges();
                }
            }
        }

        public static void OrderTimedOut(BallyTaxiEntities db, long orderId)
        {

            var order = db.Orders.GetById(orderId);

            if (order != null && order.DriverId.HasValue == false)//הזמנה שלא נמצא לה נהג
            {

                order.StatusId = (int)OrderStatus.Dissatisfied;
                //find nearest driver even if not available-

                var closestDriver = db.Drivers
                    //.ThatAreActive()
                    .WantFutureRide()
                    .LocationWithinTime(ConfigurationHelper.UPDATE_MINUTES)//נהג שהמיקום שלו עודכן בעשר דקות האחרונות
                    .Near(order.PickUpLocation, ConfigurationHelper.AVAILABLE_RADIUS_500)
                    // .Near(order.PickUpLocation, ConfigurationHelper.AVAILABLE_RADIUS)
                    .OrderBy(x => x.Location.Distance(order.PickUpLocation))
                    .FirstOrDefault();
                bool driverInDB = false;

                Orders_Drivers orderDriver0 = null;
                //change status in order_driver and send push message to All drivers that registered for this order (but didn't cancel).
                foreach (var orderDriver in order.Orders_Drivers)
                {
                    //if (orderDriver.StatusId != (int)Order_DriverStatus.Declined && orderDriver.StatusId != (int)Order_DriverStatus.Accepted)
                    //    NotificationsServices.Current.DriverNotification(orderDriver.Driver.User, DriverNotificationTypes.UserCancelRideRequest, orderId);

                    if (orderDriver.StatusId == (int)Order_DriverStatus.Accepted && orderDriver.Driver.Status == (int)DriverStatus.PendingAcceptRequest)
                    //long shot: accepted but by the time allocateRide started - it went to timeout.
                    //??
                    {
                        orderDriver.StatusId = (int)Order_DriverStatus.Skipped;
                        orderDriver.Driver.Status = (int)DriverStatus.Available;
                    }
                    //??
                    if (orderDriver.Priority == 0)
                        orderDriver0 = orderDriver;

                    if (closestDriver != null && orderDriver.DriverId == closestDriver.UserId)
                    {
                        orderDriver.StatusId = (int)Order_DriverStatus.Standby;
                        driverInDB = true;
                    }
                    else if (orderDriver.StatusId != (int)Order_DriverStatus.Declined)
                        orderDriver.StatusId = (int)Order_DriverStatus.Skipped;
                }

                //clear status of driver 0                    
                if (orderDriver0 != null)
                {
                    var driver = db.Drivers.GetById(orderDriver0.DriverId);
                    if (driver != null)
                        if (driver.Status == (int)DriverStatus.HasRequestAsFirst)
                            driver.Status = (int)DriverStatus.Available;
                }
                //db.SaveChanges();

                //send push message to passenger with closer drivers even if not available (so passenger can call him)
                var passengerUser = db.Users.GetById(order.PassengerId);
                var Pdata = new Dictionary<string, object>();
                if (closestDriver != null) // add info of nearest driver to the message, and to db (if he's not in it)
                {
                    Pdata.Add("nearDriver", closestDriver.UserId);

                    if (string.IsNullOrEmpty(closestDriver.User.Name))
                        Pdata.Add("driverName", "");
                    else
                        Pdata.Add("driverName", closestDriver.User.Name);

                    Pdata.Add("driverPhone", closestDriver.User.Phone);
                }
                if (order.isFromWeb == true && order.Passenger.DeviceId == null && order.StatusId != (int)OrderStatus.Canceled)
                {
                    bool status = UserService.SendSMSNotif(order.PassengerId, order.Passenger.Phone, SMSType.DriverNotFound, order.Passenger.LanguageId);
                }
                else
                {
                    //if (order.isWithDiscount == true)
                    //    NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.notFoundTaxiWithDiscount, orderId, extraInfo: Pdata);

                    //else 
                    if (order.isHandicapped == true)
                        NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.notFoundTaxiWithHandicapped, orderId, extraInfo: Pdata);
                    else if (order.courier.HasValue && order.courier > 0)
                        NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.notFoundTaxiWithCourier, orderId, extraInfo: Pdata);
                    else if (order.orderCount < 3)
                        NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.driverNotFoundFirst, orderId, extraInfo: Pdata);
                    else
                        NotificationsServices.Current.PassengerNotification(passengerUser, PassengerNotificationTypes.DriverNotFound, orderId, extraInfo: Pdata);
                }
            }
            else
            {
                if (order != null && order.DriverId.HasValue)
                {
                    Logger.Debug("this order: " + order.OrderId.ToString() + " come to time out but one driver want it so the creation date will be less ");
                    order.CreationDate = order.CreationDate.AddSeconds(ConfigurationHelper.MAX_ORDER_WAIT_SECONDS / 2);
                    //??
                    order.FlowStep = (int)FlowSteps.Step2;
                    db.SaveChanges();
                }
            }
            //throw new OrderNotFoundException();
        }

        public static bool DriverInOrder(long userId, long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var orderDriver = db.Orders_Drivers
                    .ByDriver(userId)
                    .ByOrder(orderId)
                    .SingleOrDefault();
                if (orderDriver != null)
                    return true;
            }
            return false;
        }

        public static bool PassengerRejectsAmount(long orderId, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();
                var driver = db.Users.GetById(order.DriverId.Value);
                if (driver == null)
                    throw new UserNotExistException();
                if (order.PassengerId != userId)
                    throw new UserNotExistException();
                if (order.StatusId != (int)OrderStatus.Payment)
                    throw new ExpirationException();
                order.StatusId = (int)OrderStatus.DisputeAmount;
                db.SaveChanges();

                //notify Driver 
                var Pdata = new Dictionary<string, object>();
                Pdata.Add("paymentMethod", order.PaymentMethod);
                Pdata.Add("amount", order.Amount);
                Pdata.Add("currency", order.Currency);
                NotificationsServices.Current.DriverNotification(driver, DriverNotificationTypes.AmountDispute, orderId, Pdata);
            }
            return true;
        }

        // need to write this!!!
        public static bool ChargeCard(string RavSapakNumber, long creditCardId, double amount, Currency currency)
        {
            //check there is such a card or throw exception
            //add another method for adding a new card.
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var card = db.CreditCards.ByCardId(creditCardId);
                if (card == null)
                    return false;
                else
                {
                    //write method to charge card using api
                }

            }

            return true;
        }

        public static object PassengerAcceptAmount(long orderId, long cardId, long userId, double amount, string currency, bool makeDefaultCard = false, bool always = false)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();
                var driver = db.Drivers.GetById(order.DriverId.Value);
                if (driver == null)
                    throw new UserNotExistException();
                var passenger = db.Users.GetById(userId);
                if (passenger == null)
                    throw new UserNotExistException();
                if (order.PassengerId != userId)
                    throw new UserNotExistException();
                if (order.StatusId != (int)OrderStatus.Payment)
                    throw new ExpirationException();
                CreditCard card = db.CreditCards.ByCardId(cardId).GetAreValid();

                var return_code = false;
                var Pdata = new Dictionary<string, object>();
                if (card != null)
                    return_code = true; // FIX FIX FIX - should be ChargeCard
                if (return_code) //payment successful
                {
                    order.StatusId = (int)OrderStatus.Completed;
                    driver.Status = (int)DriverStatus.Available;
                    if (always)
                        passenger.AlwaysApproveSum = true;
                    if (makeDefaultCard)
                    {
                        //make only this card the default card
                        var CCList = db.CreditCards.ByUserId(userId);
                        foreach (CreditCard cc in CCList)
                        {
                            if (cc.CardId == cardId)
                                cc.IsDefaultCard = true;
                            else
                                cc.IsDefaultCard = false;
                        }
                    }
                    //notify driver that payment was done                    
                    Pdata.Add("paymentMethod", (int)CustomerPaymentMethod.InApp);
                    Pdata.Add("amount", amount);
                    Pdata.Add("currency", currency);
                    NotificationsServices.Current.DriverNotification(db.Users.GetById(driver.UserId), DriverNotificationTypes.PaymentSuccessful, orderId, Pdata);

                    db.SaveChanges();
                    return new { paymentStatus = "ok" };
                }
                else //payment error or card expiration
                {
                    passenger.AlwaysApproveSum = false; //we can't automatically use cc...
                                                        //notify driver that payment has error                   
                    Pdata.Add("paymentMethod", (int)CustomerPaymentMethod.InApp);
                    Pdata.Add("amount", amount);
                    Pdata.Add("currency", currency);
                    NotificationsServices.Current.DriverNotification(db.Users.GetById(driver.UserId), DriverNotificationTypes.PaymentError, orderId, Pdata);
                    db.SaveChanges();
                    if (card == null) // no valid card was found...
                        return new { paymentStatus = "card not valid" };
                    else //return_code is false
                        return new { paymentStatus = "error" };
                }
            }
        }

        /*public static object PassengerAcceptAmountAlways(long orderId, long cardId, long userId, double amount, string currency, bool makeDefaultCard = false)
        {
            return PassengerAcceptAmount(orderId, cardId, userId, amount, currency, makeDefaultCard, true);
        }*/

        public static List<BasicCreditCardModels> ToBasicInfo(this List<CreditCard> cards)
        {
            if (cards == null)
                return null;
            var BasicInfoList = new List<BasicCreditCardModels>();

            foreach (CreditCard cc in cards)
            {
                BasicInfoList.Add(new BasicCreditCardModels(cc));
            }
            return BasicInfoList;
        }

        public static void RateOrderByPassenger(long orderId, long userId, int rating)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();
                if (order.PassengerId != userId)
                    throw new UserUnauthorizedException();
                if (rating > 5 || rating < 1)
                    return;
                order.Rating = rating;
                db.SaveChanges();
            }
            return;
        }

        public static bool IsValidCard(long userId, long cardId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var card = db.CreditCards.ByUserId(userId).ByCardId(cardId).GetAreValid();
                if (card == null)
                    return false;
                else return true;
            }
        }

        public static void HandleSearchDrivers(Order order, int roleId = 1)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                // var passenger = db.Users.GetById(order.PassengerId);
                string address1 = "";
                string address2 = "";
                string cityEN = "";
                var addressHE = getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 1);
                if (addressHE != null)
                {
                    address1 = addressHE[0];
                }
                var addressEN = getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 2);
                if (addressEN != null)
                {
                    address2 = addressEN[0];
                    if (addressEN.Count() > 1 && addressEN[1] != null)
                        cityEN = addressEN[1];
                }

                var availableRadius = ConfigurationHelper.AVAILABLE_RADIUS_500;
                //var availableRadius = cityEN == "Jerusalem" ?
                //ConfigurationHelper.AVAILABLE_RADIUS_JERUSALEM : cityEN == "Tel Aviv-Yafo" ?
                //    ConfigurationHelper.AVAILABLE_RADIUS_TelAviv : cityEN == "Ashdod" ?
                //    ConfigurationHelper.AVAILABLE_RADIUS_Ashdod : cityEN == "Haifa" ?
                //    ConfigurationHelper.AVAILABLE_RADIUS_Haifa : cityEN == "Bnei Brak" ?
                //    ConfigurationHelper.AVAILABLE_RADIUS_BNEI_BRAK : cityEN == "Ramat Gan" ?
                //    ConfigurationHelper.AVAILABLE_RADIUS_RAMAT_GAN : ConfigurationHelper.AVAILABLE_RADIUS;

                //.WantFutureRide()
                //   .LocationWithinTime(ConfigurationHelper.UPDATE_MINUTES)
                //   .Near(order.PickUpLocation, ConfigurationHelper.AVAILABLE_RADIUS)

                var dateForFutureRide = DateTime.UtcNow.AddMinutes(30);
                var dateFilter = DateTime.UtcNow.AddMinutes(-(ConfigurationHelper.UPDATE_MINUTES / 2) + 1);
                var availDrivers = db.Drivers
                               //for inter city travel:
                               //.Where(d => ((d.TaxiStation.StationId == 297 || d.TaxiStation.StationId == 338 /*מוניות הדקה ה99*/ ) && order.isWithDiscount == true) || (order.isWithDiscount == false))
                               //isHandicapped:
                               .isHandicapped(order.isHandicapped)
                               //courier:
                               .courier(order.courier)
                               //.isIn99minuteCompAndInInterCity(order.isInterCity)
                               .AvailableToDrive()
                               .ThatAreActive()
                               .WantFutureRide()
                               .bySeats(order)
                               .notAlreadyDeclined(order.OrderId)
                               //TODO:must check for same country!!!
                               .NearNew(order.PickUpLocation, (order.courier.HasValue && order.courier.Value > 0) ? ConfigurationHelper.AVAILABLE_RADIUS_FORCourier : (order.isHandicapped.HasValue && order.isHandicapped.Value == true) ? ConfigurationHelper.AVAILABLE_RADIUS_FORHandicapped : (order.

                               isInterCity.HasValue && order.isInterCity.Value == true) ? ConfigurationHelper.AVAILABLE_RADIUS_FORIntercityRide : /*ConfigurationHelper.AVAILABLE_RADIUS*/ availableRadius, order.OrderTime > dateForFutureRide)
                               .Where(d => d.LastUpdateLocation >= dateFilter)
                               //.byPrefferedStationId(order.PreferedStationId,order.isFromStations)//must add it for the ניהול תחנות
                               //.BYPaymentMethod(order.PaymentMethod.Value)
                               .OrderBy(x => x.Location.Distance(order.PickUpLocation))
                               // .Take(ConfigurationHelper.MaxDriversOfferedSingleDrive)
                               .ToList();
                if (order.isWithDiscount == true)
                {
                    availDrivers = availDrivers.OrderByDescending(x => (x.TaxiStationId == 297 || x.TaxiStationId == 338)).ToList();
                }
                availDrivers = filterDriversByRole(availDrivers, roleId);

                if (availDrivers != null && availDrivers.Count > 0)
                {
                    Logger.Info("Found drivers");
                    //anyOperation = true;

                    var firstDriver = availDrivers.First();
                    //added to check because the FK:
                    if (db.Orders_Drivers.Where(o => o.DriverId == firstDriver.UserId && o.OrderId == order.OrderId).FirstOrDefault() == null)
                        db.Orders_Drivers.Add(new Orders_Drivers()
                        {
                            DriverId = firstDriver.UserId,
                            OrderId = order.OrderId,
                            StatusId = (int)Order_DriverStatus.SentPush,
                            Priority = 0
                        });

                    firstDriver.Status = (int)DriverStatus.HasRequestAsFirst;
                    order.FlowStep = (int)FlowSteps.Step2;
                    order.LastUpdateFlowStep = DateTime.UtcNow;
                    db.SaveChanges();

                    //Notify driver on drive, as first driver
                    var location = new LocationObject()
                    {
                        lat = order.PickUpLocation.Latitude,
                        lon = order.PickUpLocation.Longitude,
                        address = order.PickUpAddress
                    };
                    var data = new Dictionary<string, object>();
                    data.Add(Constants.NotificationKeys.DriverPriorty, 0);
                    data.Add("location", location);
                    data.Add("TimeFrame", ConfigurationHelper.MaxSecondsFirstDriverOfferedOrder);

                    var firstDriverUser = db.Users.GetById(firstDriver.UserId);
                    data.Add("address", firstDriverUser.LanguageId == 1 ? address1 : address2);
                    NotificationsServices.Current.DriverNotification(firstDriverUser, DriverNotificationTypes.NewRideRequest, order.OrderId, data);
                    Logger.DebugFormat("Driver #0 ({1}), got push notification about order{0}", order.OrderId, firstDriverUser.DeviceId);
                    //var otherDrivers = new List<long>();

                    if (availDrivers.Count > 1)
                    {
                        var numberOfFirstDrivers = 10;
                        var numberForStop = new int[2];
                        if (availDrivers.Count > numberOfFirstDrivers)
                        {
                            if (availDrivers.Count - numberOfFirstDrivers > 5)
                            {
                                numberForStop[0] = (availDrivers.Count - numberOfFirstDrivers) / 3 + numberOfFirstDrivers;
                                numberForStop[1] = (availDrivers.Count - numberOfFirstDrivers) / 3 * 2 + numberOfFirstDrivers;
                            }
                        }
                        Logger.DebugFormat("numberForStop[0]: {0}, numberForStop[1]: {1}", numberForStop[0], numberForStop[1]);

                        var driverPriority = 0;
                        // Notify all other drivers
                        foreach (var additionalDriver in availDrivers.Where(x => x.UserId != firstDriver.UserId))
                        {
                            if (driverPriority > 0 && (driverPriority == numberOfFirstDrivers || numberForStop.Contains(driverPriority)))
                            {
                                Logger.DebugFormat("HandleSearchDrivers delay to search drivers driverPriority:", driverPriority);
                                System.Threading.Thread.Sleep(5000);
                            }

                            driverPriority++;
                            additionalDriver.Status = (int)DriverStatus.HasRequest;
                            //otherDrivers.Add(additionalDriver.UserId);
                            var driverCheck = db.Orders_Drivers.Where(o => o.DriverId == additionalDriver.UserId && o.OrderId == order.OrderId).FirstOrDefault();
                            if (driverCheck == null)
                                db.Orders_Drivers.Add(new Orders_Drivers()
                                {
                                    DriverId = additionalDriver.UserId,
                                    OrderId = order.OrderId,
                                    StatusId = (int)Order_DriverStatus.SentPush,
                                    Priority = driverPriority
                                });
                            db.SaveChanges();
                            // sending every driver different notification, because of the priority

                            data[Constants.NotificationKeys.DriverPriorty] = driverPriority;
                            data.Remove("TimeFrame");
                            var user = db.Users.GetById(additionalDriver.UserId);
                            data["address"] = user.LanguageId == 1 ? address1 : address2;
                            NotificationsServices.Current.DriverNotification(user, DriverNotificationTypes.NewRideRequest, order.OrderId, data);
                            Logger.DebugFormat("Driver #{1} ({2}), got push nitification about order{0}", order.OrderId, driverPriority, user.DeviceId);
                        }
                    }
                }
                else
                {
                    Logger.DebugFormat("no driver found for order {0}", order.OrderId);
                }
            }
        }

        //public static void HandleSearchDrivers(Order order)
        //{
        //    using (var db = new BallyTaxiEntities().AutoLocal())
        //    {
        //        var dateFilter = DateTime.UtcNow.AddMinutes(-ConfigurationHelper.UPDATE_MINUTES);
        //        var availDrivers = db.Drivers
        //                       //for inter city travel:
        //                       .Where(d => ((d.TaxiStation.StationId == 297 || d.TaxiStation.StationId == 338 /*מוניות הדקה ה99*/ ) && order.isWithDiscount == true) || (order.isWithDiscount == false))
        //                       //isHandicapped:
        //                       .isHandicapped(order.isHandicapped)
        //                       //courier:
        //                       .courier(order.courier)

        //                       //.isIn99minuteCompAndInInterCity(order.isInterCity)
        //                       .AvailableToDrive()
        //                       .ThatAreActive()
        //                       .WantFutureRide()
        //                       .bySeats(order)

        //                       .notAlreadyDeclined(order.OrderId)
        //                       .Near(order.PickUpLocation, ConfigurationHelper.AVAILABLE_RADIUS)
        //                       .Where(d => d.LastUpdateLocation >= dateFilter)
        //                       //.BYPaymentMethod(order.PaymentMethod.Value)
        //                       .OrderBy(x => x.Location.Distance(order.PickUpLocation))
        //                       // .Take(ConfigurationHelper.MaxDriversOfferedSingleDrive)
        //                       .ToList();

        //        string address1 = "";
        //        string address2 = "";
        //        var addressHE = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 1);
        //        if (addressHE != null)
        //        {
        //            address1 = addressHE[0];
        //        }
        //        var addressEN = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 2);
        //        if (addressEN != null)
        //        {
        //            address2 = addressEN[0];
        //        }

        //        if (availDrivers != null && availDrivers.Count > 0)
        //        {
        //            Logger.Info("Found drivers");
        //            //anyOperation = true;

        //            var firstDriver = availDrivers.First();
        //            //added to check because the FK:
        //            if (db.Orders_Drivers.Where(o => o.DriverId == firstDriver.UserId && o.OrderId == order.OrderId).FirstOrDefault() == null)
        //                db.Orders_Drivers.Add(new Orders_Drivers()
        //                {
        //                    DriverId = firstDriver.UserId,

        //                    OrderId = order.OrderId,
        //                    StatusId = (int)Order_DriverStatus.SentPush,
        //                    Priority = 0
        //                });

        //            firstDriver.Status = (int)DriverStatus.HasRequestAsFirst;
        //            order.FlowStep = (int)FlowSteps.Step2;
        //            order.LastUpdateFlowStep = DateTime.UtcNow;

        //            db.SaveChanges();



        //            //Notify driver on drive, as first driver
        //            var location = new LocationObject()
        //            {
        //                lat = order.PickUpLocation.Latitude,
        //                lon = order.PickUpLocation.Longitude,
        //                address = order.PickUpAddress
        //            };
        //            var data = new Dictionary<string, object>();
        //            data.Add(Constants.NotificationKeys.DriverPriorty, 0);
        //            data.Add("location", location);
        //            data.Add("TimeFrame", ConfigurationHelper.MaxSecondsFirstDriverOfferedOrder);

        //            var firstDriverUser = db.Users.GetById(firstDriver.UserId);
        //            data.Add("address", firstDriverUser.LanguageId == 1 ? address1 : address2);
        //            NotificationsServices.Current.DriverNotification(firstDriverUser, DriverNotificationTypes.NewRideRequest, order.OrderId, data);
        //            Logger.DebugFormat("Driver #0 ({1}), got push notification about order{0}", order.OrderId, firstDriverUser.DeviceId);
        //            //var otherDrivers = new List<long>();

        //            if (availDrivers.Count > 1)
        //            {
        //                var driverPriority = 0;
        //                // Notify all other drivers
        //                foreach (var additionalDriver in availDrivers.Where(x => x.UserId != firstDriver.UserId))
        //                {
        //                    driverPriority++;
        //                    additionalDriver.Status = (int)DriverStatus.HasRequest;
        //                    //otherDrivers.Add(additionalDriver.UserId);
        //                    var driverCheck = db.Orders_Drivers.Where(o => o.DriverId == additionalDriver.UserId && o.OrderId == order.OrderId).FirstOrDefault();
        //                    if (driverCheck == null)
        //                        db.Orders_Drivers.Add(new Orders_Drivers()
        //                        {
        //                            DriverId = additionalDriver.UserId,
        //                            OrderId = order.OrderId,
        //                            StatusId = (int)Order_DriverStatus.SentPush,
        //                            Priority = driverPriority
        //                        });
        //                    db.SaveChanges();
        //                    // sending every driver different notification, because of the priority

        //                    data[Constants.NotificationKeys.DriverPriorty] = driverPriority;
        //                    data.Remove("TimeFrame");
        //                    var user = db.Users.GetById(additionalDriver.UserId);
        //                    data["address"] = user.LanguageId == 1 ? address1 : address2;
        //                    NotificationsServices.Current.DriverNotification(user, DriverNotificationTypes.NewRideRequest, order.OrderId, data);
        //                    Logger.DebugFormat("Driver #{1} ({2}), got push nitification about order{0}", order.OrderId, driverPriority, user.DeviceId);
        //                }
        //            }
        //        }

        //    }
        //}

        public static string[] getAddressFromLatLong(double lat, double lon, int LanguageId)
        {
            string lang = "en";
            try
            {
                lang = ((UserLanguages)LanguageId).ToString();
            }
            catch (Exception)
            {
                lang = "en";
            }

            XmlDocument doc = new XmlDocument();
            try
            {

                var Address = "";
                var strings = new string[2];
                string Address_LongName = "";
                //doc.Load("http://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lon + "&sensor=false&language=" + lang);
                // XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                XmlNode element = getGeocodingData(doc, lat, lon, lang);
                //}
                if (element.InnerText == "ZERO_RESULTS")
                {
                    return null;
                }
                else
                {
                    element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");
                    Address = element.InnerText;
                    if (Address.Contains(","))
                    {
                        String[] separated = Address.Split(',');
                        Address = separated[0];
                        Address += separated[1];
                    }
                    strings[0] = Address;

                    string longname = "";
                    string shortname = "";
                    string typename = "";
                    string typeLast = "";
                    bool fHit = false;


                    XmlNodeList xnList = doc.SelectNodes("//GeocodeResponse/result/address_component");
                    foreach (XmlNode xn in xnList)
                    {

                        longname = xn["long_name"].InnerText;
                        shortname = xn["short_name"].InnerText;
                        typename = xn["type"].InnerText;
                        typeLast = xn.LastChild.InnerText;

                        fHit = true;
                        switch (typename)
                        {
                            //Add whatever you are looking for below
                            case "political":
                                {
                                    // var Address_country = longname;
                                    Address_LongName = longname;//Address_LongName != "" ? longname : Address_LongName;//
                                    strings[1] = Address_LongName;
                                    break;
                                }
                            case "locality":
                                {
                                    // var Address_country = longname;
                                    Address_LongName = longname;//Address_LongName  != "" ?  longname: Address_LongName;//
                                    strings[1] = Address_LongName;
                                    break;
                                }
                            //case "country":
                            //    {
                            //        strings[2] = shortname;
                            //        break;
                            //    }
                            default:
                                fHit = false;
                                break;
                        }
                        if (typeLast == "political" || typeLast == "locality")
                        {
                            Address_LongName = longname;//Address_LongName  != "" ?  longname: Address_LongName;//
                            strings[1] = Address_LongName;
                        }
                        if (Address_LongName != "")
                            break;
                    }
                }
                return strings;
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("getAddressFromLatLong error", e.Message);
                return null;
            }
        }

        private static XmlNode getGeocodingData(XmlDocument doc, double lat, double lon, string lang, int numtring = 0)
        {
            numtring++;
            doc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lon + "&language=" + lang + "&key=" + ConfigurationHelper.GoogleAPIKey);
            XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
            if (element.InnerText == "OVER_QUERY_LIMIT" && numtring < 3)
            {
                System.Threading.Thread.Sleep(2500);
                getGeocodingData(doc, lat, lon, lang, numtring);
            }
            return element;
        }

        public static string getCityfromLocation(double lat, double lon)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                var Address_ShortName = "";
                doc.Load("http://maps.googleapis.com/maps/api/geocode/xml?latlng=" + lat + "," + lon + "&sensor=false&language=en");
                XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
                if (element.InnerText == "ZERO_RESULTS")
                {
                    return ("No data available for the specified location");
                }
                else
                {
                    element = doc.SelectSingleNode("//GeocodeResponse/result/formatted_address");
                    string longname = "";
                    string shortname = "";
                    string typename = "";
                    bool fHit = false;


                    XmlNodeList xnList = doc.SelectNodes("//GeocodeResponse/result/address_component");
                    foreach (XmlNode xn in xnList)
                    {
                        longname = xn["long_name"].InnerText;
                        shortname = xn["short_name"].InnerText;
                        typename = xn["type"].InnerText;

                        fHit = true;
                        switch (typename)
                        {
                            //Add whatever you are looking for below
                            case "country":
                                {
                                    // var Address_country = longname;
                                    Address_ShortName = shortname;
                                    break;
                                }
                            default:
                                fHit = false;
                                break;
                        }
                    }
                }
                return Address_ShortName;
            }
            catch (Exception)
            {
                return "";
            }
        }

        //=====================================================================================
        public static bool onWeekDay(long orderId, double latitude, double longitude)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var order = db.Orders.GetById(orderId);
                if (order == null)
                    throw new OrderNotFoundException();
                if (order.StatusId == (int)OrderStatus.Canceled || order.StatusId == (int)OrderStatus.Completed || order.StatusId == (int)OrderStatus.Dissatisfied || order.StatusId == (int)OrderStatus.Pending)
                    throw new OrderNotRelevantException();

                ShabbatObj shabbatObjResult = getShabbatTimes(order.OrderTime, latitude, longitude);
                ShabbatEvent ShabbatEntrance = new ShabbatEvent();
                ShabbatEvent ShabbatEnd = new ShabbatEvent();

                if (shabbatObjResult != null)
                {
                    ShabbatEntrance = shabbatObjResult.items.Where(item => item.category == "candles").FirstOrDefault();
                    ShabbatEnd = shabbatObjResult.items.Where(item => item.category == "havdalah").FirstOrDefault();
                }
                //if(ShabbatEntrance!=null && ShabbatEnd != null) { }
                //todo check day in week and hour
                //DateTime dOrderTime = orderTime.ConvertFromUnixTimestamp();
                if (order.OrderTime > ShabbatEntrance.date && order.OrderTime < ShabbatEnd.date)
                    return false;
                return true;
            }
        }


        public static ShabbatObj getShabbatTimes(DateTime? orderTime, double latitude, double longitude)
        {
            string timeZoneId = getTimeZoneId(orderTime, latitude, longitude);

            if (timeZoneId != "" && timeZoneId != null)
            {
                string url = String.Format("http://www.hebcal.com/shabbat/?cfg=json&m=60&b=60&latitude={0}&longitude={1}&tzid={2}", latitude, longitude, timeZoneId);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(result))
                    {
                        ShabbatObj shabbatObjResult = result.FromJson<ShabbatObj>();
                        return shabbatObjResult;
                    }
                }
            }
            return null;
        }

        // public static timeZoneObj getTimeZoneId(double date, double latitude, double longitude)//**********
        public static string getTimeZoneId(DateTime? orderTime, double latitude, double longitude)//**********
        {

            //by default the location of israel
            latitude = latitude != 0 ? latitude : 31.0461;
            longitude = longitude != 0 ? longitude : 34.8516;
            //??
            String url = String.Format("https://maps.googleapis.com/maps/api/timezone/json?location={0},{1}&timestamp={2}", latitude, longitude, orderTime);
            //String url = "https://maps.googleapis.com/maps/api/timezone/json?location=" + lat + "," + lon + "&timestamp="++"1331766000" + "&key=YOUR_API_KEY");

            //Pass request to google api with orgin and destination details
            HttpWebRequest request =
                (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                if (!string.IsNullOrEmpty(result))
                {
                    timeZoneFromLocation timezoneResult = result.FromJson<timeZoneFromLocation>(); //JsonConvert.DeserializeObject<Parent>(result);
                    if (timezoneResult.status == "OK")
                    {
                        return timezoneResult.timeZoneId;
                    }
                }
            }
            return null;
        }

    }
}