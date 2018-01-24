using Quickode.BallyTaxi.Domain.Services;
using Quickode.BallyTaxi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Core;
using System.Web.Script.Serialization;
using System.Web;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("driver")]
    public class DriverController : BaseController
    {
        [Route("updateProfile")]
        [HttpPost]
        public HttpResponseMessage UpdateProfile([FromBody]DriverProfileModels data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var driver = DriverService.UpdateDriverProfile(user.UserId, data.Email, data.Name, data.LicensePlate, data.TaxiLicense, data.CarTypeId, data.AcceptCreditCard, data.ImageID, data.TaxiStationId, data.ChargeCCId, data.BankNumber, data.BankBranch, data.BankAccount, data.BankHolderName, data.IdentityCardNumber, data.CCProviderNumber, data.paymentMethod, data.driverCode, data.payment, data.seatsNumber, data.isHandicapped, data.courier, data.companyNumber, data.productionYear, data.isReadTermsOfUse, data.isPrivate, data.tz, data.studentCard, data.authorizedDealer);

                var station = driver != null && driver.TaxiStationId.HasValue ? DriverService.GetTaxiStationById(driver.TaxiStationId.Value) : null;
                var car = driver != null && driver.CarType.HasValue ? DriverService.getCarTypeById(driver.CarType.Value) : null;
                var lastNumbersCC = PassengerService.getLastNumbersCCForPassenger(user.UserId);

                var driver_result = new DriverDTOModels(driver, user, station, car, lastNumbersCC);
                Logger.DebugFormat("UpdateProfile for Driver result: {0}", driver_result);
                return Request.CreateResponse(HttpStatusCode.OK, driver_result);
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("getDataStatistics")]
        [HttpGet]
        public HttpResponseMessage getDataStatistics(double? date = null)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                DateTime? dateTime = null;
                if (date.HasValue)
                    dateTime = date.Value.ConvertFromUnixTimestamp();
                var result = DriverService.getDataStatistics(user.UserId, dateTime);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("there is an error in getDataStatistics: {0}", ex.Message);
                return Request.CreateErrorResponse((HttpStatusCode)420, new HttpError(ex.Message));
            }
        }

        [Route("getMyRides")]
        [HttpGet]
        public HttpResponseMessage GetMyRides()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var orders = OrderService.GetMyRides(user.UserId, UserType.Driver);
                List<SummaryRideObject> currentRides = new List<SummaryRideObject>();
                //List<SummaryRideObject> postRides = new List<SummaryRideObject>();

                foreach (var item in orders.OrderBy(x => x.OrderTime))
                {
                    if (item.StatusId != (int)OrderStatus.Completed)
                        //postRides.Add(new SummaryRideObject(item));
                        //else
                        currentRides.Add(new SummaryRideObject(item));
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { currentRides = currentRides });
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }

        [Route("getMyPostRides")]
        [HttpGet]
        public HttpResponseMessage GetMyPostRides(double date)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var orders = OrderService.GetMyPostRides(user.UserId, UserType.Driver, date);
                //List<SummaryRideObject> currentRides = new List<SummaryRideObject>();
                List<SummaryRideObject> postRides = new List<SummaryRideObject>();

                foreach (var item in orders.OrderByDescending(x => x.OrderTime))
                {
                    item.Amount = Convert.ToDouble(Math.Round((Convert.ToDecimal(item.Amount)), 2));
                    if (item.PickUpAddress.Contains(","))
                    {
                        String[] separated = item.PickUpAddress.Split(',');
                        item.PickUpAddress = separated[0];
                        item.PickUpAddress += separated[1];
                    }
                    if (item.DestinationAddress != null && item.DestinationAddress != "")
                    {
                        if (item.DestinationAddress.Contains(","))
                        {
                            String[] separated = item.DestinationAddress.Split(',');
                            item.DestinationAddress = separated[0];
                            item.DestinationAddress += separated[1];
                        }
                    }

                    //var result = OrderService.getAddressFromLatLong(item.PickUpLocation.Latitude.Value, item.PickUpLocation.Longitude.Value, user.LanguageId);
                    //if (result != null)
                    //{
                    //    item.PickUpAddress = result[0];
                    //    if (result.Count() > 1 && result[1] != null)
                    //        item.pickUpCityName = result[1];
                    //}
                    //if (item.DestinationLocation != null)
                    //{
                    //    var resultD = OrderService.getAddressFromLatLong(item.PickUpLocation.Latitude.Value, item.PickUpLocation.Longitude.Value, user.LanguageId);
                    //    if (resultD != null)
                    //    {
                    //        item.DestinationAddress = resultD[0];
                    //        if (resultD.Count() > 1 && resultD[1] != null)
                    //            item.destinationCityName = resultD[1];
                    //    }
                    //}

                    //if (item.StatusId == (int)OrderStatus.Completed)
                    postRides.Add(new SummaryRideObject(item));
                    //else currentRides.Add(new SummaryRideObject(item));
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { postRides = postRides });
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }

        [Route("acceptOrder")]
        [HttpGet]
        public HttpResponseMessage AcceptOrder(long orderID, int? reminderSeconds = null)
        {
            if (Request == null || Request.Headers == null || Request.Headers.Authorization == null)
            {
                Logger.Error("No Request.Headers.Authorization found");
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError("No Request.Headers.Authorization found"));
            }

            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var driverQueue = OrderService.AcceptOrder(orderID, user.UserId, reminderSeconds);

                //var order_result = FetchFullOrderDetails(order);
                return Request.CreateResponse(HttpStatusCode.OK, new { DriverQueue = driverQueue });
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (ExpirationException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (UserForbiddenException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError(ex.Message));
            }

        }


        [Route("declineOrder")]
        [HttpGet]
        public HttpResponseMessage DeclineOrder(long orderID)
        {
            if (Request == null || Request.Headers == null || Request.Headers.Authorization == null)
            {
                Logger.Error("No Request.Headers.Authorization found");
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError("No Request.Headers.Authorization found"));
            }

            try
            {

                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                long result = OrderService.DeclineOrder(orderID, user.UserId);
                if (result == 0)
                {
                    var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                    var messageText = Utils.TranslateMessage(culture, DriverNotificationTypes.FutureRideCannotCancelled.ToString());
                    Logger.ErrorFormat("you cannot cancel future ride : {0}", orderID);
                    return Request.CreateErrorResponse((HttpStatusCode)530, new HttpError(messageText));
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (UserBlockedException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (OrderNotFoundException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (ExpirationException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (UserForbiddenException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError(ex.Message));
            }
            catch (CannotCancelFutureRideException ex)
            {
                Logger.ErrorFormat("you cannot cancel future ride : {0}", orderID);
                return Request.CreateErrorResponse((HttpStatusCode)530, new HttpError(ex.Message));
            }
        }

        [Route("changeDriverStatus")]
        [HttpGet]
        public HttpResponseMessage ChangeDriverStatus(int status, long? orderID = null)
        {
            if (Request == null || Request.Headers == null || Request.Headers.Authorization == null)
            {
                Logger.Error("No Request.Headers.Authorization found");
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError("No Request.Headers.Authorization found"));
            }

            try
            {
                Logger.Debug("changeDriverStatus: status: " + status + " token: " + Request.Headers.Authorization.Parameter);
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var order = DriverService.ChangeDriverStatus(status, user.UserId, orderID);

                var orderResult = FetchFullOrderDetails(order);

                return Request.CreateResponse(HttpStatusCode.OK, orderResult);

            }
            catch (UserBlockedException ex)
            {
                Logger.ErrorFormat("changeDriverStatus for status: " + status.ToString() + " error: " + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                Logger.ErrorFormat("changeDriverStatus for status: " + status.ToString() + " error: " + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                Logger.ErrorFormat("changeDriverStatus for status: " + status.ToString() + " error: " + ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (ExpirationException ex)
            {
                Logger.ErrorFormat("changeDriverStatus for status: " + status.ToString() + " error: " + ex.Message);
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (OrderNotFoundException ex)
            {
                Logger.ErrorFormat("changeDriverStatus for status: " + status.ToString() + " error: " + ex.Message);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                Logger.ErrorFormat("changeDriverStatus for status: " + status.ToString() + " error: " + ex.Message);
                return Request.CreateErrorResponse((HttpStatusCode)508, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.ErrorFormat("changeDriverStatus for status: " + status.ToString() + " error: " + ex.Message);
                return Request.CreateErrorResponse((HttpStatusCode)509, new HttpError(ex.Message));
            }

        }

        [Route("driverWantFutureRide")]
        [HttpGet]
        public HttpResponseMessage DriverWantFutureRide(int status)
        {
            if (Request == null || Request.Headers == null || Request.Headers.Authorization == null)
            {
                Logger.Error("No Request.Headers.Authorization found");
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError("No Request.Headers.Authorization found"));
            }
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                DriverService.DriverWantFutureRide(status == 1, user.UserId);
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (UserBlockedException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }

        [Route("getAvailableTaxis")]
        [HttpPost]
        public HttpResponseMessage getAvailableTaxis(LocationModelForAvailabletaxis location)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var availableTaxis = DriverService.GetAvailableTaxis(location.Lat, location.Lon, location.orderId);
                return Request.CreateResponse(HttpStatusCode.OK, availableTaxis);
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }

        [Route("updateLocation")]
        [HttpPost]
        public HttpResponseMessage UpdateLocation(LocationModel location)
        {
            try
            {
                //Logger.Debug(location.ToJson());

                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var driver = DriverService.UpdateLocation(location.Lat, location.Lon, user.UserId, location.heading);
                BaseDriverData driver_result = new BaseDriverData(driver);
                try
                {
                    DriverService.changeRegionForDriver(user.UserId);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("error in change region " + e.Message);
                }
                string textForRegion = DriverService.getRegionForDriver(user.UserId, user.LanguageId);
                return Request.CreateResponse(HttpStatusCode.OK, new { driver_result, textForRegion = textForRegion });
            }
            catch (UserBlockedException ex)
            {
                Logger.Error(string.Format("update location for location:{0}. Error - UserBlockedException", location.ToJson()));
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                Logger.Error(string.Format("update location for location:{0}. Error - UserUnauthorizedException", location.ToJson()));
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                Logger.Error(string.Format("update location for location:{0}. Error - AuthenticationTokenIncorrectException", location.ToJson()));
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.Error(string.Format("update location for location:{0}. Error - UserNotExistException", location.ToJson()));
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                Logger.Error(string.Format("update location for location:{0}. Error - Exception", location.ToJson()));
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)500, new HttpError(ex.Message));
            }
        }

        [Route("eula")]
        [HttpGet]
        public HttpResponseMessage GetEULA(int languageId)
        {
            var lang = Convert.ToString((UserLanguages)languageId);
            var selectedLang = Core.CultureHelper.GetImplementedCulture(lang);
            try
            {
                byte[] content = ContentService.GetDriverEULAHtmlPage(selectedLang);

                if (content != null)
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(content);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("ELUA file not found"));
            }
        }

        [Route("Privacy")]
        [HttpGet]
        public HttpResponseMessage GetPrivacy(string lang)
        {
            try
            {
                byte[] content = ContentService.GetDriverPrivacyHtmlPage(lang.Replace("\"", "").ToLowerInvariant());

                if (content != null)
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(content);
                    response.StatusCode = HttpStatusCode.OK;
                    return response;
                }
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("Privacy file not found"));
            }
        }

        [Route("stations")]
        [HttpGet]
        public HttpResponseMessage GetStationsList(int languageId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var list = DriverService.GetListOfStations(languageId);
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (NoRelevantDataException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NoContent, new HttpError(ex.Message));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }

        [Route("carTypes")]
        [HttpGet]
        public HttpResponseMessage GetCarTypeList(int languageId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var list = DriverService.GetCarTypeList(languageId);
                return Request.CreateResponse(HttpStatusCode.OK, list);
            }
            catch (NoRelevantDataException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NoContent, new HttpError(ex.Message));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }

        [Route("login")]
        [HttpPost]
        public HttpResponseMessage DriverLogin(languageData data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter, data.appVersion);
                var user_db = DriverService.Login(user.UserId, data.appVersion, data != null ? data.languageId : 0);
                var driver = DriverService.FetchDriver(user.UserId);

                if (driver != null)
                {
                    ////TODO: delete It after it will fix in iphone!!!!!!:
                    ////-----
                    //isAvailable = DriverService.setDriverToAvailable(user.UserId);
                    ////-----
                    var order = DriverService.GetNextOrder(user.UserId, null);
                    var station = driver.TaxiStationId.HasValue ? DriverService.GetTaxiStationById(driver.TaxiStationId.Value) : null;
                    var car = driver != null && driver.CarType.HasValue ? DriverService.getCarTypeById(driver.CarType.Value) : null;
                    var lastNumbersCC = PassengerService.getLastNumbersCCForPassenger(user.UserId);
                    DriverDTOModels user_result = new DriverDTOModels(driver, order, user, station, car, lastNumbersCC);
                    string[] str = DriverService.getStringsForPayment(user.LanguageId);
                    user_result.DriverObject.paymentForMonth = str[0];
                    user_result.DriverObject.paymentForRide = str[1];
                    user_result.DriverObject.paymentForMonthNew = str[2];

                    var systemData = PassengerService.getSystemData();
                    user_result.systemdData = systemData;

                    //var dicInfo = new Dictionary<string, object>();
                    //dicInfo["url"] = ConfigurationHelper.LINK_FOR_ADVERTISING;
                    //  NotificationsServices.Current.DriverNotification(user, DriverNotificationTypes.openAdvertising, 0, dicInfo);

                    return Request.CreateResponse(HttpStatusCode.OK, user_result);
                }
                else
                    return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError("invalid user type"));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }

        }

        [Route("getRatingForDriver")]
        [HttpPost]
        public HttpResponseMessage getRatingForDriver()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                int rating = DriverService.getRatingForDriver(user.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, new { rating = rating });
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }

        }

        [Route("GetTravelHistory")]
        [HttpPost]
        public HttpResponseMessage GetTravelHistory([FromUri]double date)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var dateTime = date.ConvertFromUnixTimestamp();
                var result = DriverService.GetTravelHistory(user.UserId, dateTime);
                if (result != null)
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                else
                {
                    Logger.Error("error in GetTravelHistory");
                    return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError("error in GetTravelHistory"));
                }
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, new HttpError("error in GetTravelHistory"));
            }
        }

        [Route("getParamsToPrint")]
        [HttpGet]
        public HttpResponseMessage getParamsToPrint(long orderId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                DriverToPrint data = DriverService.getParamsToPrint(user.UserId, orderId);
                return Request.CreateResponse(HttpStatusCode.OK, data);

            }
            catch (Exception e)
            {
                Logger.Error(e);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError("error in getParamsToPrint"));
            }
        }


    }
}
