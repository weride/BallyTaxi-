using Quickode.BallyTaxi.Core;
using Quickode.BallyTaxi.Domain.Services;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("passenger")]
    public class PassengerController : BaseController
    {
        [Route("updateProfile")]
        [HttpPost]
        public HttpResponseMessage UpdateProfile([FromBody]PassengerProfileModels data)
        {
            try
            {
                Logger.InfoFormat("UpdateProfile: {0}", data.ToJson());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var passenger = PassengerService.UpdatePassengerProfile(user.UserId, data.Name, data.Email, data.ImageID.HasValue ? data.ImageID.Value : Guid.Empty, data.PreferredPaymentMethod, data.latHome, data.longHome, data.homeAddress, data.homeCity, data.latBusiness, data.longBusiness, data.businessAddress, data.businessCity, data.preferredTaxiStationId, data.businessId, data.isHandicapped);//(user.UserId, data.Email, data.Name, data.Language, data.ImageID, data.PreferredPaymentMethod);
                PassengerDTOModels passenger_result = new PassengerDTOModels(passenger);
                return Request.CreateResponse(HttpStatusCode.OK, passenger_result);
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


        [Route("getEstimateTimeLatLon")]
        [HttpGet]
        public HttpResponseMessage getEstimateTimeLatLon(EstimationTimeModel data)
        {
            try
            {
                Logger.Info("getEstimateTimeLatLon:" + data.ToJson());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                //var response_object = OrderService.GetEstimationTime(orderID, user.UserId);
                var response_object = PassengerService.getEstimateTimeLatLon(data.pickupLatitude, data.pickupLongitude, data.destinationLatitude, data.destinationLongitude, data.time, user.LanguageId, user.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, response_object);
            }
            catch (UserBlockedException ex)
            {
                Logger.Error("UserBlockedException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                Logger.Error("UserUnauthorizedException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                Logger.Error("AuthenticationTokenIncorrectException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (OrderNotFoundException ex)
            {
                Logger.Error("OrderNotFoundException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.Error("UserNotExistException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (GoogleAPIException ex)
            {
                Logger.Error("GoogleAPIException");
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError(ex.Message));
            }
        }

        [Route("getMyRides")]
        [HttpGet]
        public HttpResponseMessage GetMyRides()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var orders = OrderService.GetMyRides(user.UserId, UserType.Passenger);
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
                var orders = OrderService.GetMyPostRides(user.UserId, UserType.Passenger, date);
                //List<SummaryRideObject> currentRides = new List<SummaryRideObject>();
                List<SummaryRideObject> postRides = new List<SummaryRideObject>();

                foreach (var item in orders.OrderByDescending(x => x.OrderTime))
                {
                    //if (item.StatusId == (int)OrderStatus.Completed)
                    postRides.Add(new SummaryRideObject(item));
                    //else currentRides.Add(new SummaryRideObject(item));
                }
                var sum = postRides.Select(p => p.amount).Sum();
                return Request.CreateResponse(HttpStatusCode.OK, new { postRides = postRides, sumAmount = sum });
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

        [Route("getCreditCardList")]
        [HttpGet]
        public HttpResponseMessage getCreditCardListByUser()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                List<CreditCardUser> lCreditCardUser = PassengerService.getCreditCardListByUser(user.UserId);
                var LCards = new List<CreditCardDetails>();
                foreach (var item in lCreditCardUser)
                {
                    var creditCardDetails = new CreditCardDetails()
                    {
                        creditCardId = item.creditCardUser1,
                        lastNumbersCC = item.tokenId.Substring(item.tokenId.Length - 4)
                    };
                    LCards.Add(creditCardDetails);
                }
                return Request.CreateResponse(HttpStatusCode.OK, LCards);
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

        [Route("updateCreditCard")]
        [HttpGet]
        public HttpResponseMessage updateCreditCard(int creditCardId, int type)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                if (type == (int)updateCreditCardType.delete)
                {
                    var res = PassengerService.DeleteCreditCard(creditCardId, user.UserId);
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else if (type == (int)updateCreditCardType.setDefault)
                {
                    var res = PassengerService.setCreditCardDefault(creditCardId, user.UserId);
                    return Request.CreateResponse(HttpStatusCode.OK, res);
                }
                throw new OrderNotRelevantException();
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
            catch (OrderNotRelevantException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError(ex.Message));
            }
        }

        [Route("getCuponAmount")]
        [HttpGet]
        public HttpResponseMessage getCuponAmount(string cuponCode)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                double amount = PassengerService.getCuponAmount(user.UserId, cuponCode);
                double? sumAmount = PassengerService.getCouponAmountByUserId(user.UserId);
                sumAmount = sumAmount == null ? 0 : sumAmount;
                return Request.CreateResponse(HttpStatusCode.OK, new { amount = amount, sumAmount = sumAmount });
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


        [Route("getLastAddresses")]
        [HttpGet]
        public HttpResponseMessage GetLastAddresses()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var orders = OrderService.GetLastAddresses(user.UserId);
                List<LocationObject> addresses = new List<LocationObject>();
                foreach (var item in orders)
                {
                    LocationObject tmp = new LocationObject()
                    {
                        lat = item.PickUpLocation.Latitude,
                        lon = item.PickUpLocation.Longitude,
                        address = item.PickUpAddress,
                        CityName = item.pickUpCityName
                    };
                    if (!addresses.Any(x => x.address == tmp.address))
                        addresses.Add(tmp);

                    //dynamic tmp = new System.Dynamic.ExpandoObject();
                    //tmp.lat = item.PickUpLocation.Latitude;
                    //tmp.lon = item.PickUpLocation.Longitude;
                    //tmp.address = item.PickUpAddress;

                    //if (!addresses.Contains(tmp)) addresses.Add(tmp);

                    if (item.DestinationAddress != null && item.DestinationAddress != "" && item.DestinationLocation != null)
                    //  if (item.DestinationAddress != null && item.DestinationLocation != null)
                    {
                        LocationObject tmp_des = new LocationObject()
                        {
                            lat = item.DestinationLocation.Latitude,
                            lon = item.DestinationLocation.Longitude,
                            address = item.DestinationAddress,
                            CityName = item.destinationCityName
                        };
                        if (!addresses.Any(x => x.address == tmp_des.address))
                            addresses.Add(tmp_des);

                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, addresses);
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

        [Route("GetListOfLastStations")]
        [HttpGet]
        public HttpResponseMessage GetListOfLastStations(int languageId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var stations = PassengerService.GetListOfLastStations(languageId, user.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, stations);
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


        [Route("getFavoriteDrivers")]
        [HttpGet]
        public HttpResponseMessage GetFavoriteDrivers()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var favorites = PassengerService.GetFavoriteDrivers(user.UserId);
                List<FavoriteDriverModel> favorites_list = new List<FavoriteDriverModel>();
                foreach (var item in favorites.OrderByDescending(x => x.CreationDate))
                {
                    favorites_list.Add(new FavoriteDriverModel(item));
                }
                return Request.CreateResponse(HttpStatusCode.OK, favorites_list);
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

        [Route("UpdateFavoriteStation")]
        [HttpPost]
        public HttpResponseMessage UpdateFavoriteStation(FavoriteStationObject data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                PassengerService.AddFavoriteStation(user.UserId, data.StationID);
                return Request.CreateResponse(HttpStatusCode.NoContent);
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

        [Route("GetFavoriteStation")]
        [HttpPost]
        public HttpResponseMessage GetFavoriteStation(FavoriteStationObject data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var favoriteStation = PassengerService.GetFavoriteStation(user.UserId, user.LanguageId);
                return Request.CreateResponse(HttpStatusCode.OK, favoriteStation);
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

        [Route("addFavoriteDriver")]
        [HttpPost]
        public HttpResponseMessage AddFavoriteDriver(FavoriteDriverObject data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                PassengerService.AddFavoriteDriver(user.UserId, data.DriverID, data.Notes);
                return Request.CreateResponse(HttpStatusCode.NoContent);
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

        [Route("removeFavoriteDriver")]
        [HttpGet]
        public HttpResponseMessage RemoveFavoriteDriver(long driverID)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                PassengerService.RemoveFavoriteDriver(user.UserId, driverID);
                return Request.CreateResponse(HttpStatusCode.NoContent);
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
            catch (FavoriteNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("getFavoriteAddresses")]
        [HttpGet]
        public HttpResponseMessage GetFavoriteAddresses()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var orders = OrderService.GetFavoriteAddress(user.UserId);
                List<object> addresses = new List<object>();
                foreach (var item in orders.OrderByDescending(x => x.CreationDate))
                {
                    dynamic tmp = new System.Dynamic.ExpandoObject();
                    tmp.favoriteIndex = item.FavoriteIndex;
                    tmp.pickup_lat = item.PickUpLocation.Latitude;
                    tmp.pickup_lon = item.PickUpLocation.Longitude;
                    tmp.pickup_address = item.PickUpAddress;


                    if (item.DestinationAddress != null && item.DestinationLocation != null)
                    {
                        tmp.destination_lat = item.DestinationLocation.Latitude;
                        tmp.destination_lon = item.DestinationLocation.Longitude;
                        tmp.destination_address = item.DestinationAddress;
                    }

                    addresses.Add(tmp);

                }
                return Request.CreateResponse(HttpStatusCode.OK, addresses);
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

        [Route("addFavoriteAddress")]
        [HttpPost]
        public HttpResponseMessage AddFavoriteAddress(FavoriteAddressObject data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                PassengerService.AddFavoriteAddress(user.UserId, data.pickupLatitude, data.pickupLongitude, data.pickupAddress, data.destinationLatitude, data.destinationLongitude, data.destinationAddress);
                return Request.CreateResponse(HttpStatusCode.NoContent);
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


        [Route("removeFavoriteAddress")]
        [HttpGet]
        public HttpResponseMessage RemoveFavoriteAddress(long favoriteIndex)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                PassengerService.RemoveFavoriteAddress(user.UserId, favoriteIndex);
                return Request.CreateResponse(HttpStatusCode.NoContent);
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
            catch (FavoriteNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
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
                byte[] content = ContentService.GetPassengerEULAHtmlPage(selectedLang);

                if (content != null)
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(content);
                    // response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
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
                byte[] content = ContentService.GetPassengerPrivacyHtmlPage(lang.Replace("\"", "").ToLowerInvariant());

                if (content != null)
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    response.Content = new ByteArrayContent(content);
                    // response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
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

        [Route("passengerCancelAutoAccept")]
        [HttpGet]
        public HttpResponseMessage PassengerCancelAutoAccept()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                PassengerService.PassengerCancelAutoAccept(user.UserId);
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (UserBlockedException ex)
            {
                Logger.Error("UserBlockedException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserUnauthorizedException ex)
            {
                Logger.Error("UserUnauthorizedException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (AuthenticationTokenIncorrectException ex)
            {
                Logger.Error("AuthenticationTokenIncorrectException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.Error("UserNotExistException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("login")]
        [HttpPost]
        public HttpResponseMessage PassengerLogin(languageData data)
        {
            try
            {
                Logger.DebugFormat("PassengerLogin: {0}", Request.Headers.Authorization.Parameter.ToJson());
                var passenger = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                bool languageId = PassengerService.updateLanguage(passenger.UserId, data.appVersion, data != null ? data.languageId : 0);
                if (passenger != null)
                {
                    var systemData = PassengerService.getSystemData();
                    //!!
                    var result = PassengerService.getCouponAmountByUserId(passenger.UserId);
                    double couponAmount = result == null ? 0 : result.Value;
                    var business = (passenger.PreferredPaymentMethod.HasValue && passenger.BusinessId.HasValue) ? BusinessServices.GetBusinessById(passenger.BusinessId.Value) : null;
                    var station = passenger.PreferedStationId.HasValue ? DriverService.GetTaxiStationById(passenger.PreferedStationId.Value) : null;
                    var lastNumbersCC = PassengerService.getLastNumbersCCForPassenger(passenger.UserId);
                    PassengerDTOModels user_result = new PassengerDTOModels(passenger, lastNumbersCC, station, business, couponAmount, systemData);
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

        ////
        //[Route("PaymentByPayPal")]
        //[HttpPost]
        //public HttpResponseMessage PaymentByPayPal(int price, int orderId)
        //{

        //    try
        //    {
        //        var passenger = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
        //        if (passenger != null)
        //        {
        //            //TODO: the payment with paypal!!
        //            PassengerService.PaymentByPayPal(passenger.UserId, passenger.PayPalId, price, orderId);
        //            return Request.CreateResponse(HttpStatusCode.OK, passenger);
        //        }
        //        else
        //            return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError("invalid user type"));

        //    }
        //    catch (UserBlockedException ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
        //    }
        //    catch (UserUnauthorizedException ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
        //    }
        //    catch (AuthenticationTokenIncorrectException ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
        //    }
        //    catch (UserNotExistException ex)
        //    {
        //        return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
        //    }

        //}
    }


}
