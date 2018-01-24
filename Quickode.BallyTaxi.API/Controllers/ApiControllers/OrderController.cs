using Quickode.BallyTaxi.Domain.Services;
using Quickode.BallyTaxi.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Core;
using Quickode.BallyTaxi.Models.Models;
using System.Collections.Generic;
using System.Data.Entity.Spatial;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("order")]
    public class OrderController : BaseController
    {

        [Route("getInterCityPrice")]
        [HttpPost]
        public HttpResponseMessage getInterCityPrice(LocationForIntercityTravel locationObj)
        {
            try
            {
                Logger.Debug("Calc Price Intercity Travel: " + locationObj.ToJson());
                //calc the distance:
                double priceModel = OrderService.getInterCityPrice(locationObj);
                Logger.DebugFormat("Calc Price Intercity Travel success: {0}", priceModel);
                return Request.CreateResponse(HttpStatusCode.OK, new { priceModel = priceModel });
            }
            catch (Exception e)
            {
                Logger.Error("error in getInterCityPrice");
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }

        [Route("calcPriceIntercityTravel")]
        [HttpPost]
        public HttpResponseMessage CalcPriceIntercityTravel(LocationForIntercityTravel locationObj)
        {
            var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
            Logger.Debug("Calc Price Intercity Travel: " + locationObj.ToJson());
            //calc the distance:

            var priceModel = OrderService.CalcPriceIntercityTravel(locationObj);
            if (priceModel != null && priceModel.Count > 0)
            {
                var response_object = PassengerService.getEstimateTimeLatLon(locationObj.lat, locationObj.lon, locationObj.destinationLatitude, locationObj.destinationLongitude, locationObj.time, user.LanguageId, user.UserId);
                Logger.DebugFormat("Calc Price Intercity Travel success: {0}", priceModel.ToJson());

                return Request.CreateResponse(HttpStatusCode.OK, new { priceModel, response_object });
            }
            else
            {
                Logger.Error("priceModel is null");
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("priceModel is null"));
            }
        }

        [Route("interCityPrice")]
        [HttpPost]
        public HttpResponseMessage interCityPrice(LocationForIntercityTravel data)
        {
            Logger.Debug("Calc Price Intercity Travel: " + data.ToJson());
            //calc the distance:
            var priceModel = OrderService.CalcPriceIntercityTravel(data);
            if (priceModel != null)
            {
                Logger.DebugFormat("Calc Price Intercity Travel success: {0}", priceModel);
                return Request.CreateResponse(HttpStatusCode.OK, priceModel);
            }
            else
            {
                Logger.Error("priceModel is null");
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError("priceModel is null"));

            }
        }

        [Route("reCreateOrder")]
        [HttpGet]
        public HttpResponseMessage reCreateOrder(long orderId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                long orderIdResult = OrderService.reCreateOrder(user.UserId, orderId);

                var result = PassengerService.getCouponAmountByUserId(user.UserId);
                double couponAmount = result == null ? 0 : result.Value;

                Logger.Info("Order reCreated with id:" + orderId.ToString());
                return Request.CreateResponse(HttpStatusCode.OK, new { OrderID = orderIdResult, couponAmount = couponAmount });
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error(ex);
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
            catch (UserForbiddenException e)
            {
                Logger.Error(e);
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError(e.Message));
            }
            catch (orderCannotReCreatedException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)585, new HttpError(ex.Message));
            }
            catch (OrderNotFoundException ex)
            {
                Logger.ErrorFormat("the order: " + orderId.ToString() + " is not found");
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {

                Logger.ErrorFormat("error in create order:", ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }



        [Route("createOrder")]
        [HttpPost]
        public HttpResponseMessage CreateOrder(OrderModel data)
        {
            try
            {
                Logger.Debug("Create order:" + data.ToJson());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var orderDateTime = data.time.ConvertFromUnixTimestamp();
                // var orderID = OrderService.CreateOrder(data.pickupLatitude, data.pickupLongitude, data.pickupAddress == null ? "" : data.pickupAddress, data.destinationLatitude, data.destinationLongitude, data.destinationAddress, user.UserId, data.notes, orderDateTime, data.paymentMethod, data.isInterCity, data.pickUpCityName, data.destinationCityName, data.fileNumber, data.isFromWeb, data.businessId, data.isWithDiscount, data.seatsNumber, data.courier, data.isHandicapped, data.accountId, data.isFromStations, data.roleId == 0 ? 1 : data.roleId);
                var orderID = OrderService.CreateOrder(data.pickupLatitude, data.pickupLongitude, data.pickupAddress == null ? "" : data.pickupAddress, data.destinationLatitude, data.destinationLongitude, data.destinationAddress, user.UserId, data.notes, orderDateTime, data.isInterCity, data.pickUpCityName, data.destinationCityName, data.fileNumber, data.isFromWeb, data.businessId, data.isWithDiscount, data.seatsNumber, data.courier, data.isHandicapped, data.accountId, data.isFromStations, data.roleId == 0 ? 1 : data.roleId, data.paymentMethod);

                var result = PassengerService.getCouponAmountByUserId(user.UserId);
                double couponAmount = result == null ? 0 : result.Value;
                //if (data.paymentMethod == (int)CustomerPaymentMethod.Paypal)
                //{
                //    var transactionId = OrderService.DoReferenceTransaction(orderID, data.isInterCity == true ? 500 : 100);
                //    Logger.Info("transactionId: " + transactionId);
                //    if (transactionId == null)
                //        Logger.ErrorFormat("error in DoReferenceTransaction: ", transactionId);
                //}
                Logger.Info("Order created with id:" + orderID.ToString());
                return Request.CreateResponse(HttpStatusCode.OK, new { OrderID = orderID, couponAmount = couponAmount });
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error(ex);
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
            catch (Exception ex)
            {
                Logger.ErrorFormat("error in create order:", ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }


        [Route("createVirtualOrder")]
        [HttpPost]
        public HttpResponseMessage createVirtualOrder(VirtualOrderModel data)
        {
            try
            {
                Logger.Debug("Create virtual order:" + data.ToJson());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var orderDateTime = data.time.ConvertFromUnixTimestampForMilliSeconds();

                var virtualUser = PassengerService.createOrGetVirtualPassengerForDriver(user.UserId);
                if (virtualUser == null)
                    throw new Exception();
                var PickUpAddress = "";
                var pickUpCityName = "";
                var address = OrderService.getAddressFromLatLong(data.pickupLatitude, data.pickupLongitude, user.LanguageId);
                if (address != null)
                {
                    PickUpAddress = address[0];
                    pickUpCityName = address[1];
                }

                var order = OrderService.CreateVirtualOrder(data.pickupLatitude, data.pickupLongitude, PickUpAddress, user.UserId, orderDateTime, data.paymentMethod, pickUpCityName, virtualUser.UserId);
                Logger.Info("Order created with id:" + order.OrderId.ToString());
                var orderResult = FetchFullOrderDetails(order);
                Logger.Info("GetOrderDetailsForDriver: (" + order.OrderId.ToString() + ") result=" + orderResult.ToJson());

                return Request.CreateResponse(HttpStatusCode.OK, orderResult);
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error(ex);
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
            catch (Exception ex)
            {

                Logger.ErrorFormat("error in create virtual order:", ex.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
        }

        [Route("updateDestinationLocation")]
        [HttpPost]
        public HttpResponseMessage updateDestinationLocation(DestinationLocationModel order)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var updated_order = OrderService.updateDestinationLocation(user.UserId, order.rideID, order.destinationLatitude, order.destinationLongitude, order.destinationAddress, order.destinationCity);
                var orderResult = FetchFullOrderDetails(updated_order);
                return Request.CreateResponse(HttpStatusCode.OK, orderResult);
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
            catch (UserPermissionException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("updateOrderIfRead")]
        [HttpGet]
        public HttpResponseMessage updateOrderIfRead(int userType, long orderID = 0)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                Logger.DebugFormat("updateOrderIfRead with userId: {0}", user.UserId);
                var result = OrderService.updateOrderIfRead(user.UserId, userType, orderID);


                Logger.DebugFormat("updateOrderIfRead: {0}", result);
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
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
        }

        [Route("getOrderDetailsForPassenger")]
        [HttpGet]
        public HttpResponseMessage getOrderDetailsForUser(long orderID, bool isFromWeb = false)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var order = OrderService.GetOrderDetails(orderID);

                if (order.isReadTheOrder.HasValue && order.isReadTheOrder.Value == true && isFromWeb == false)
                    throw new OrderNotRelevantException();
                if (order.PassengerId != user.UserId)
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("User is forbidden"));
                if ((order.StatusId == (int)OrderStatus.Canceled || order.StatusId == (int)OrderStatus.Completed) && isFromWeb == false)
                    throw new OrderNotRelevantException();
                if (order.StatusId == (int)OrderStatus.DriverDeclined && isFromWeb != true)
                {
                    var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                    var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.DriverCancelledRide.ToString());

                    var futureDate = DateTime.UtcNow.AddMinutes(15);
                    // var futureDate = DateTime.UtcNow.AddMinutes(3);
                    if (order.OrderTime > futureDate)
                    {
                        DateTime? resultDate = OrderService.convertToLocalTime(order.OrderTime.Value.ConvertToUnixTimestamp(), order.PickUpLocation);
                        if (resultDate == null)
                            resultDate = order.OrderTime.Value.AddHours(3);//for jerusalem

                        var pData = new Dictionary<string, object>();
                        pData["orderTime"] = resultDate.Value.ToShortDateString() + " " + resultDate.Value.ToShortTimeString();
                        pData["address"] = order.PickUpAddress;
                        messageText = string.Format(Utils.TranslateMessage(culture, PassengerNotificationTypes.DriverCancelledFutureRide.ToString()), pData["orderTime"], pData["address"]);
                    }

                    return Request.CreateErrorResponse((HttpStatusCode)584, new HttpError(messageText));
                }
                if ((order.StatusId == (int)OrderStatus.Dissatisfied) && isFromWeb == false)
                {
                    //if (order.isWithDiscount == true)
                    //{
                    //    var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                    //    var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.notFoundTaxiWithDiscount.ToString());
                    //    return Request.CreateErrorResponse((HttpStatusCode)581, new HttpError(messageText));
                    //}
                    //else 
                    if (order.isHandicapped == true)
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.notFoundTaxiWithHandicapped.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)582, new HttpError(messageText));
                    }
                    else if (order.courier.HasValue && order.courier > 0)
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.notFoundTaxiWithCourier.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)583, new HttpError(messageText));
                    }
                    else if (order.orderCount < 3)
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.driverNotFoundFirst.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)585, new HttpError(messageText));
                    }
                    else
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.DriverNotFound.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)580, new HttpError(messageText));
                    }
                }
                var orderResult = FetchFullOrderDetails(order);
                //added by Shoshana on 03/01/18
                if (orderResult.driverObject != null)
                    orderResult.driverObject.rating = order.DriverId.HasValue ? DriverService.getRatingForDriver(orderResult.driverObject.DriverID) : 0;
                Logger.DebugFormat("orderResult: {0}", orderResult.ToJson());
                return Request.CreateResponse(HttpStatusCode.OK, orderResult);
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
            catch (OrderNotRelevantException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
        }

        [Route("createOrderIfDriverCancel")]
        [HttpGet]
        public HttpResponseMessage createOrderIfDriverCancel(long orderID)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                long orderId = OrderService.createOrderIfDriverCancel(orderID, user.UserId);
                var result = PassengerService.getCouponAmountByUserId(user.UserId);
                double couponAmount = result == null ? 0 : result.Value;
                return Request.CreateResponse(HttpStatusCode.OK, new { OrderID = orderID, couponAmount = couponAmount });
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
            catch (OrderNotRelevantException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
        }

        [Route("getPendingOrdersForDriver")]
        [HttpGet]
        public HttpResponseMessage getPendingOrdersForDriver()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                List<long> PendingOrders = OrderService.getPendingOrdersForDriver(user.UserId);

                return Request.CreateResponse(HttpStatusCode.OK, PendingOrders);
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

        [Route("getOrderLastDetailsForDriver")]
        [HttpGet]
        public HttpResponseMessage getOrderLastDetailsForDriver()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                Logger.Debug("getOrderLastDetailsForDriver user=" + user.UserId.ToString());

                var order = OrderService.getOrderLastDetailsForDriver(user.UserId);
                var address = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, user.LanguageId);
                if (address != null)
                {
                    order.PickUpAddress = address[0];
                    order.pickUpCityName = address[1];
                }
                Logger.Debug("order=" + order.OrderId.ToString());

                switch (order.StatusId)
                {
                    case (int)OrderStatus.Canceled:
                        {
                            var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                            var messageText = Utils.TranslateMessage(culture, DriverNotificationTypes.UserCancelRideRequest.ToString());

                            var futureDate = DateTime.UtcNow.AddMinutes(15);
                            // var futureDate = DateTime.UtcNow.AddMinutes(3);
                            if (order.OrderTime > futureDate)
                            {
                                DateTime? resultDate = OrderService.convertToLocalTime(order.OrderTime.Value.ConvertToUnixTimestamp(), order.PickUpLocation);
                                if (resultDate == null)
                                    resultDate = order.OrderTime.Value.AddHours(3);//for jerusalem

                                var pData = new Dictionary<string, object>();
                                //DateTime convertedDate = order.OrderTime.Value.AddHours(3);
                                pData["orderTime"] = resultDate.Value.ToShortDateString() + " " + resultDate.Value.ToShortTimeString();
                                pData["address"] = order.PickUpAddress;
                                messageText = string.Format(Utils.TranslateMessage(culture, DriverNotificationTypes.UserCancelFutureRideRequest.ToString()), pData["orderTime"], pData["address"]);
                            }

                            return Request.CreateErrorResponse((HttpStatusCode)590, new HttpError(messageText));
                        }
                    case (int)OrderStatus.Completed:
                    case (int)OrderStatus.DriverDeclined:
                    case (int)OrderStatus.Dissatisfied:
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("Order Not Rellevant"));
                        }
                    case (int)OrderStatus.Pending:
                    //if (!OrderService.DriverInOrder(user.UserId, orderID))
                    //    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("User is forbidden"));
                    //else
                    //break;
                    case (int)OrderStatus.Confirmed:
                    case (int)OrderStatus.DisputeAmount:
                        break;
                    case (int)OrderStatus.Payment:
                        {
                            var driver = DriverService.GetDriverByUserId(user.UserId);
                            if (order.Amount > 0 && (driver.Status == (int)DriverStatus.Available || driver.Status == (int)DriverStatus.HasRequest || driver.Status == (int)DriverStatus.NotAvailable))
                                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("Order Not Rellevant"));
                        }
                        break;
                        //if (order.DriverId != user.UserId)
                        //return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("User is forbidden"));
                        //else break;
                }

                // if we get to here - user can see order details. he is either the driver, or the order is still pending and he is in order_driver                
                var orderResult = FetchFullOrderDetails(order);
                Logger.Info("getOrderLastDetailsForDriver: (" + user.UserId.ToString() + ") result=" + orderResult.ToJson());
                return Request.CreateResponse(HttpStatusCode.OK, orderResult);
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
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError(ex.Message));
            }
        }

        [Route("getOrderLastDetailsForPassenger")]
        [HttpGet]
        public HttpResponseMessage getOrderLastDetailsForPassenger()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                Logger.Debug("getOrderLastDetailsForPassenger user=" + user.UserId.ToString());

                var order = OrderService.getOrderLastDetailsForPassenger(user.UserId);
                //var address = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, user.LanguageId);
                //if (address != null)
                //{
                //    order.PickUpAddress = address[0];
                //    order.pickUpCityName = address[1];
                //}
                Logger.Debug("order=" + order.OrderId.ToString());

                if ((order.StatusId == (int)OrderStatus.Canceled || order.StatusId == (int)OrderStatus.Completed))
                    throw new OrderNotRelevantException();
                if (order.StatusId == (int)OrderStatus.DriverDeclined)
                {
                    var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                    var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.DriverCancelledRide.ToString());

                    // var futureDate = DateTime.UtcNow.AddMinutes(3);
                    var futureDate = DateTime.UtcNow.AddMinutes(15);
                    if (order.OrderTime > futureDate)
                    {
                        DateTime? resultDate = OrderService.convertToLocalTime(order.OrderTime.Value.ConvertToUnixTimestamp(), order.PickUpLocation);
                        if (resultDate == null)
                            resultDate = order.OrderTime.Value.AddHours(3);//for jerusalem

                        var pData = new Dictionary<string, object>();
                        //DateTime convertedDate = order.OrderTime.Value.AddHours(3);
                        pData["orderTime"] = resultDate.Value.ToShortDateString() + " " + resultDate.Value.ToShortTimeString();
                        pData["address"] = order.PickUpAddress;
                        messageText = string.Format(Utils.TranslateMessage(culture, PassengerNotificationTypes.DriverCancelledFutureRide.ToString()), pData["orderTime"], pData["address"]);
                    }

                    return Request.CreateErrorResponse((HttpStatusCode)584, new HttpError(messageText));
                }
                if ((order.StatusId == (int)OrderStatus.Dissatisfied))
                {
                    //if (order.isWithDiscount == true)
                    //{
                    //    var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                    //    var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.notFoundTaxiWithDiscount.ToString());
                    //    return Request.CreateErrorResponse((HttpStatusCode)581, new HttpError(messageText));
                    //}
                    //else
                    if (order.isHandicapped == true)
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.notFoundTaxiWithHandicapped.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)582, new HttpError(messageText));
                    }
                    else if (order.courier.HasValue && order.courier > 0)
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.notFoundTaxiWithCourier.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)583, new HttpError(messageText));
                    }
                    else if (order.orderCount < 3)
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.driverNotFoundFirst.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)585, new HttpError(messageText));
                    }
                    else
                    {
                        var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                        var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.DriverNotFound.ToString());
                        return Request.CreateErrorResponse((HttpStatusCode)580, new HttpError(messageText));
                    }
                }
                var dateNow = DateTime.UtcNow.AddMinutes(1);
                if (order.StatusId == (int)OrderStatus.Confirmed && order.DriverId.HasValue)
                {
                    var driver = DriverService.GetDriverByUserId(order.DriverId.Value);
                    if (driver != null && (driver.Status == (int)DriverStatus.Available || driver.Status == (int)DriverStatus.HasRequest || driver.Status == (int)DriverStatus.HasRequestAsFirst) && order.OrderTime <= dateNow)
                    {
                        OrderService.updateErrorForOrder(order.OrderId, (int)ErrorId.driverBecomeAvailableInTheMiddleOfTheTravel);


                        throw new OrderNotFoundException();
                    }
                }

                // if we get to here - user can see order details. he is either the driver, or the order is still pending and he is in order_driver                
                try
                {
                    var orderResult = FetchFullOrderDetails(order);
                    if (orderResult.driverObject != null)
                        orderResult.driverObject.rating = order.DriverId.HasValue ? DriverService.getRatingForDriver(orderResult.driverObject.DriverID) : 0;
                    Logger.Info("getOrderLastDetailsForDriver: (" + user.UserId.ToString() + ") result=" + orderResult.ToJson());
                    return Request.CreateResponse(HttpStatusCode.OK, orderResult);
                }
                catch (Exception e)
                {
                    Logger.ErrorFormat("error in getOrderLastDetailsForPassenger: {0}", e.Message);
                    throw new ExpirationException();
                }
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
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
            catch (ExpirationException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
        }


        [Route("getOrderDetailsForDriver")]
        [HttpGet]
        public HttpResponseMessage GetOrderDetailsForDriver(long orderID)
        {
            Logger.Debug("GetOrderDetailsForDriver. orderID=" + orderID.ToString());
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                Logger.Debug("user=" + user.UserId.ToString());

                var order = OrderService.GetOrderDetails(orderID);
                if (order.DriverId.HasValue && order.DriverId != user.UserId)
                {
                    Logger.Debug("the order taken by another driver " + order.OrderId.ToString());
                    var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                    var messageText = Utils.TranslateMessage(culture, DriverNotificationTypes.RideNotRelevantAnymore.ToString());
                    return Request.CreateErrorResponse((HttpStatusCode)591, new HttpError(messageText));
                }
                //return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("Order Not Rellevant"));
                var address = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, user.LanguageId);
                if (address != null)
                {
                    order.PickUpAddress = address[0];
                    order.pickUpCityName = address[1];
                }
                Logger.Debug("order=" + order.OrderId.ToString());

                switch (order.StatusId)
                {
                    case (int)OrderStatus.Canceled:
                        {

                            var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                            // var futureDate = DateTime.UtcNow.AddMinutes(3);
                            var futureDate = DateTime.UtcNow.AddMinutes(15);

                            var messageText = Utils.TranslateMessage(culture, DriverNotificationTypes.UserCancelRideRequest.ToString());
                            if (order.OrderTime > futureDate)
                            {
                                DateTime? resultDate = OrderService.convertToLocalTime(order.OrderTime.Value.ConvertToUnixTimestamp(), order.PickUpLocation);
                                if (resultDate == null)
                                    resultDate = order.OrderTime.Value.AddHours(3);//for jerusalem

                                var pData = new Dictionary<string, object>();
                                //DateTime convertedDate = order.OrderTime.Value.AddHours(3);
                                pData["orderTime"] = resultDate.Value.ToShortDateString() + " " + resultDate.Value.ToShortTimeString();
                                pData["address"] = order.PickUpAddress;
                                messageText = string.Format(Utils.TranslateMessage(culture, DriverNotificationTypes.UserCancelFutureRideRequest.ToString()), pData["orderTime"], pData["address"]);
                            }

                            return Request.CreateErrorResponse((HttpStatusCode)590, new HttpError(messageText));
                        }
                    case (int)OrderStatus.Completed:
                    case (int)OrderStatus.Dissatisfied:
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("Order Not Rellevant"));
                        }
                    case (int)OrderStatus.Pending:
                    //if (!OrderService.DriverInOrder(user.UserId, orderID))
                    //    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("User is forbidden"));
                    //else
                    //break;
                    case (int)OrderStatus.Confirmed:
                    case (int)OrderStatus.DisputeAmount:
                    case (int)OrderStatus.Payment:
                        break;
                        //if (order.DriverId != user.UserId)
                        //return Request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError("User is forbidden"));
                        //else break;
                }

                // if we get to here - user can see order details. he is either the driver, or the order is still pending and he is in order_driver                
                Logger.Info("this order is : " + order.OrderId.ToString());
                var orderResult = FetchFullOrderDetails(order);
                Logger.Info("GetOrderDetailsForDriver: (" + orderID.ToString() + ") result=" + orderResult.ToJson());
                return Request.CreateResponse(HttpStatusCode.OK, orderResult);
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
        }


        [Route("getDetailsForEndRide")]
        [HttpGet]
        public HttpResponseMessage getDetailsForEndRide(long orderId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var order = OrderService.getDetailsForEndRide(orderId);
                if (order.Amount != null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { amount = order.Amount, paymentMethod = order.PaymentMethod });
                else
                    return Request.CreateErrorResponse((HttpStatusCode)550, new HttpError("the order was not finished"));
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
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("cancelOrder")]
        [HttpGet]
        public HttpResponseMessage CancelOrder(long orderID)
        {
            Logger.DebugFormat("CancelOrder number : {0} ", orderID);
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                long result = OrderService.CancelOrderByPassenger(orderID, user.UserId);
                if (result == 0)
                {
                    var culture = NotificationsServices.Current.GetLanguageCulture(user.LanguageId);
                    var messageText = Utils.TranslateMessage(culture, PassengerNotificationTypes.FutureRideCannotCancelled.ToString());
                    Logger.ErrorFormat("you cannot cancel future ride : {0}", orderID);
                    return Request.CreateErrorResponse((HttpStatusCode)530, new HttpError(messageText));
                }

                return Request.CreateResponse(HttpStatusCode.OK);
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
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (orderCannotCanceledException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (CannotCancelFutureRideException ex)
            {
                Logger.ErrorFormat("you cannot cancel future ride : {0}", orderID);
                return Request.CreateErrorResponse((HttpStatusCode)530, new HttpError(ex.Message));
            }

        }

        [Route("driverCancelOrder")]
        [HttpPost]
        public HttpResponseMessage CancelOrderByDriver(long orderID)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                OrderService.CancelOrderByDriver(orderID, user.UserId);
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
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }


        }

        [Route("getEstimationTimeForPassenger")]
        [HttpGet]
        public HttpResponseMessage GetEstimationTimeForPassenger(long orderID)
        {
            try
            {
                Logger.Info("GetEstimationTimeForPassenger:" + orderID.ToString());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                //var response_object = OrderService.GetEstimationTime(orderID, user.UserId);
                var response_object = OrderService.GetEstimationTimeForPassenger(orderID, user.LanguageId, user.UserId);
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

        [Route("getEstimationTime")]
        [HttpGet]
        public HttpResponseMessage GetEstimationTime(long orderID)
        {
            try
            {
                Logger.Info("GetEstimationTime:" + orderID.ToString());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                //var response_object = OrderService.GetEstimationTime(orderID, user.UserId);
                var response_object = OrderService.GetEstimationTime(orderID, user.LanguageId, user.UserId);
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

        [Route("passengerEndsRide")]
        [HttpPost]
        public HttpResponseMessage PassengerEndsRide(EndRidePassengerModel data)
        {
            Logger.InfoFormat("PassengerEndsRide. data = {0} ", data.ToJson());
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var response_object = OrderService.PassengerEndsRide(data.orderId, data.rating, data.tip, user.UserId, data.paymentMethod);
                var couponAmount = PassengerService.getCouponAmountByUserId(user.UserId);
                if (couponAmount == null)
                    couponAmount = 0;
                return Request.CreateResponse(HttpStatusCode.OK, new { CouponAmount = couponAmount.Value });
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error("NoRelevantDataException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message)); //ok code?
            }
            catch (couponNotRelevantException ex)
            {
                Logger.Error("couponNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                Logger.Error("OrderNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
        }

        [Route("passengerRefuseRidePrice")]
        [HttpPost]
        public HttpResponseMessage passengerRefuseRidePrice(long orderID)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var response_object = OrderService.passengerRefuseRidePrice(orderID, user.UserId);
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error("NoRelevantDataException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message)); //ok code?
            }
            catch (couponNotRelevantException ex)
            {
                Logger.Error("couponNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                Logger.Error("OrderNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }

        }

        [Route("checkFutureRideForDriver")]
        [HttpPost]
        public HttpResponseMessage checkFutureRideForDriver()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var order = OrderService.CheckFutureRideForDriver(user.UserId);
                if (order != null)
                {
                    var address = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, user.LanguageId);
                    if (address != null)
                    {
                        order.PickUpAddress = address[0];
                        order.pickUpCityName = address[1];
                    }
                    var orderResult = FetchFullOrderDetails(order);
                    return Request.CreateResponse(HttpStatusCode.OK, orderResult);
                }
                return Request.CreateErrorResponse((HttpStatusCode)566, new HttpError("not found future ride"));
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error("NoRelevantDataException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message)); //ok code?
            }
            catch (couponNotRelevantException ex)
            {
                Logger.Error("couponNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                Logger.Error("OrderNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(e.Message));
            }
        }

        [Route("CheckFutureRide")]
        [HttpPost]
        public HttpResponseMessage CheckFutureRide()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var order = OrderService.CheckFutureRide(user.UserId);
                if (order != null)
                {
                    var address = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, user.LanguageId);
                    if (address != null)
                    {
                        order.PickUpAddress = address[0];
                        order.pickUpCityName = address[1];
                    }
                    var orderResult = FetchFullOrderDetails(order);
                    return Request.CreateResponse(HttpStatusCode.OK, orderResult);
                }
                return Request.CreateErrorResponse((HttpStatusCode)566, new HttpError("not found future ride"));
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error("NoRelevantDataException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message)); //ok code?
            }
            catch (couponNotRelevantException ex)
            {
                Logger.Error("couponNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                Logger.Error("OrderNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(e.Message));
            }
        }

        [Route("generalEndsRide")]
        [HttpPost]
        public HttpResponseMessage GeneralEndsRide(EndsRideModel data)
        {
            Logger.InfoFormat("GeneralEndsRide. data = {0} ", data.ToJson());
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var paymentSucceeded = OrderService.GeneralEndsRide(data.orderId, data.paymentMethod, data.rating, data.tip, user.UserId, data.amount, data.currency, data.FileNumber);

                var couponAmount = PassengerService.getCouponAmountByUserId(user.UserId);
                if (couponAmount == null)
                    couponAmount = 0;
                return Request.CreateResponse(HttpStatusCode.OK, new { couponAmount = couponAmount.Value, paymentSucceeded });
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error("NoRelevantDataException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message)); //ok code?
            }
            catch (couponNotRelevantException ex)
            {
                Logger.Error("couponNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (OrderNotRelevantException ex)
            {
                Logger.Error("OrderNotRelevantException");
                return Request.CreateErrorResponse((HttpStatusCode.Forbidden), new HttpError(ex.Message));
            }
        }


        [Route("driverEndsRide")]
        [HttpPost]
        public HttpResponseMessage DriverEndsRide(EndRideModel data)
        {
            Logger.InfoFormat("DriverEndsRide. data = {0} ", data.ToJson());
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var response_object = OrderService.DriverEndsRide(data.orderId, data.paymentMethod, data.amount, data.currency);

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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error("NoRelevantDataException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message)); //ok code?
            }
        }

        [Route("driverEndRide")]
        [HttpPost]
        public HttpResponseMessage DriverEndRide(EndRideModel data)
        {
            Logger.InfoFormat("DriverEndRide. data = {0} ", data.ToJson());
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                //var response_object = OrderService.DriverEndsRide(data.orderId, data.paymentMethod, data.amount, data.currency);
                var response_object = OrderService.DriverEndRide(data.orderId, data.amount, data.currency);
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (NoRelevantDataException ex)
            {
                Logger.Error("NoRelevantDataException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message)); //ok code?
            }
        }

        [Route("rejectAmount")]
        [HttpGet]
        public HttpResponseMessage PassengerRejectsAmount(long orderId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                OrderService.PassengerRejectsAmount(orderId, user.UserId);
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
        }

        [Route("passengerAcceptAmount")]
        [HttpPost]
        public HttpResponseMessage PassengerAcceptAmount(EndRideModel data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var response_object = OrderService.PassengerAcceptAmount(data.orderId, data.cardId, user.UserId, data.amount, data.currency, data.setDefaultCard, data.alwaysApprovePayment);
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
            catch (ExpirationException ex)
            {
                Logger.Error("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }

        }

        /*        [Route("passengerAcceptAmountAlways")]
                [HttpPost]
                public HttpResponseMessage PassengerAcceptAmountAlways(EndRideModel data)
                {
                    try
                    {
                        var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                        var response_object = OrderService.PassengerAcceptAmountAlways(data.orderId, data.cardId, user.UserId, data.amount, data.currency, data.setDefaultCard);
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
                    catch (ExpirationException ex)
                    {
                        Logger.Error("ExpirationException");
                        return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
                    }
                }*/

        [Route("rateOrder")]
        [HttpPost]
        public HttpResponseMessage RateOrderByPassenger(long orderID, int rating)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                OrderService.RateOrderByPassenger(orderID, user.UserId, rating);
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
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }


        }


        [Route("ConnectOrderToSpecificDriver")]
        [HttpGet]
        public HttpResponseMessage ConnectOrderToSpecificDriver(long orderId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var result = OrderService.ConnectOrderToSpecificDriver(user.UserId, orderId);
                if (result == true)
                    return Request.CreateResponse(HttpStatusCode.OK);
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
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }


        }

        [Route("reCreateOrderToSpecificRegion")]
        [HttpGet]
        public HttpResponseMessage reCreateOrderToSpecificRegion(long orderId, int regionId, int roleId)
        {
            try
            {
                // var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var result = OrderService.reCreateOrderToSpecificRegion(regionId, orderId, roleId);
                if (result == 1)
                    return Request.CreateResponse(HttpStatusCode.OK);
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
            catch (OrderNotFoundException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("IVROrder")]
        [HttpPost]
        public HttpResponseMessage IVROrder(IVROrderModel data)
        {
            try
            {
                string phoneFormatted = data.phone;
                try
                {
                    phoneFormatted = UserService.CheckPhoneNumber(data.phone, 972);
                }
                catch (Exception e)
                {

                    //phoneFormatted = data.phone;
                }

                var dicResult = Utils.AddressToLocation(data.address);
                if (dicResult.Count > 1)
                {

                    var location = dicResult[0] as DbGeography;
                    var pickupCity = dicResult[1] as string;

                    var result = OrderService.IVROrder(location, pickupCity, phoneFormatted, data.address, data.orderTime);
                    if (result != null)
                    {

                        return Request.CreateResponse(HttpStatusCode.OK, result);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, true);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse((HttpStatusCode)500, new HttpError(e.Message));
            }
        }

        [Route("OrdersHandling")]
        [HttpPost]
        public HttpResponseMessage OrdersHandling()
        {
            OrderService.HandlePendingOrders();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }
    }
}
