using Quickode.BallyTaxi.Domain.Services;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Core;
using System;
using System.Collections.Generic;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("user")]
    public class UserController : BaseController
    {
        readonly static log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [Route("sendPhoneNumber")]
        [HttpPost]
        public HttpResponseMessage SendPhoneNumber([FromBody]RegisterUserModels data, [FromUri]int userType, [FromUri]int languageId, [FromUri] bool? debug = null)
        {
            Logger.Debug(data.phoneNumber);
            try
            {
                Logger.Debug("before check");
                string phoneFormatted = UserService.CheckPhoneNumber(data.phoneNumber, int.Parse(data.phonePrefix));
                Logger.DebugFormat("after check:{0}", phoneFormatted);
                if (userType == (int)UserType.Driver && ConfigurationHelper.getValue("DriverCheck") == "true")
                {
                    var result = UserService.checkDriverExsist(phoneFormatted);
                    if (result == false)
                        throw new PhoneNotExistInDataBase();
                }
                var pending_user = UserService.Register(phoneFormatted, debug.HasValue && debug.Value);
                Logger.Debug("after register");
                var phoneExists = UserService.PhoneExists(pending_user.Phone, userType);

                UserService.SendSMSCode(pending_user.PendingUserId, pending_user.Phone, pending_user.CodeValidation, languageId, debug.HasValue && debug.Value);
                Logger.Debug("after sms");
                return Request.CreateResponse(HttpStatusCode.OK, new { phoneFormatted, pending_user.CodeValidation, phoneExists });
            }
            catch (PhoneNumberNotValidException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (SMSProcessException ex)
            {
                Logger.Error("SMSProcessException", ex);
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (PhoneNotExistInDataBase ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)508, new HttpError(ex.Message));
            }
        }

        [Route("login")]
        [HttpPost]
        public HttpResponseMessage Login()
        {
            //return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError("Not Implemented"));

            //delete this code once change to driver & passenger login methods.
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var user_db = UserService.Login(user.UserId);
                var driver = DriverService.FetchDriver(user.UserId);

                if (driver != null)
                {
                    var order = DriverService.GetNextOrder(user.UserId, null);
                    var station = driver.TaxiStationId.HasValue ? DriverService.GetTaxiStationById(driver.TaxiStationId.Value) : null;
                    DriverDTOModels user_result = new DriverDTOModels(driver, order, user, station);

                   

                    return Request.CreateResponse(HttpStatusCode.OK, user_result);
                }
                var passenger = PassengerService.FetchPassenger(user.UserId);

                if (passenger != null)
                {
                    var result = PassengerService.getCouponAmountByUserId(passenger.UserId);
                    double couponAmount = result == null ? 0 : result.Value;
                    var business = (passenger.PreferredPaymentMethod.HasValue && passenger.PreferredPaymentMethod == (int)CustomerPaymentMethod.Business && passenger.BusinessId.HasValue) ? BusinessServices.GetBusinessById(passenger.BusinessId.Value) : null;
                    var station = passenger.PreferedStationId.HasValue ? DriverService.GetTaxiStationById(passenger.PreferedStationId.Value) : null;
                    PassengerDTOModels user_result = new PassengerDTOModels(user, station, business, couponAmount);
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

        [Route("validateCode")]
        [HttpPost]
        public HttpResponseMessage ValidateCode(ValidateSMSCode data)
        {
            try
            {
                Logger.Info("ValidateCode: data=" + data.ToJson());
                var user = UserService.ValidateSMSCode(data.Phone, data.Code, data.DeviceID, data.UserType, data.OSVersion, data.PlatformID, data.AppVersion, data.LanguageId, data.replaceDeviceId);
                if (data.UserType == (int)UserType.Driver)
                {
                    //var driver = user.Driver;
                    var driver = DriverService.GetDriverByUserIdIncludeUser(user.UserId);

                    var station = driver.TaxiStationId.HasValue ? DriverService.GetTaxiStationById((int)driver.TaxiStationId) : null;
                    DriverDTOModels user_result = new DriverDTOModels(user.Driver, user, station);
                    Logger.Info("return as driver. " + user_result.ToJson());
                    //var isSend = UserService.sendEmailTodriver(DriverEmail.RegisterTitle, driver.User.LanguageId, driver.User.Email);
                    //if (isSend == false)
                    //    Logger.ErrorFormat("error when sending email for driver: {0} about Register. email:{1}", driver.UserId, driver.User.Email);
                    //else
                    //    Logger.DebugFormat("email for driver: {0} about Register success. email:{1}", driver.UserId, driver.User.Email);


                    return Request.CreateResponse(HttpStatusCode.OK, user_result);
                }
                else if (data.UserType == (int)UserType.Passenger)
                {
                    var systemData = PassengerService.getSystemData();
                    PassengerDTOModels user_result = new PassengerDTOModels(user, systemData);
                    Logger.Info("return as passenger. " + user_result.ToJson());
                    return Request.CreateResponse(HttpStatusCode.OK, user_result);
                }
                else return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError("invalid user type"));
            }
            catch (UserBlockedException ex)
            {
                Logger.Info("UserBlockedException");
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (ValidateSMSFaildException ex)
            {
                Logger.Info("ValidateSMSFaildException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                Logger.Info("UserNotExistException");
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (ExpirationException ex)
            {
                Logger.Info("ExpirationException");
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));
            }
            catch (PhoneNotExistException ex)
            {
                Logger.Info("PhoneNotExistException");
                return Request.CreateErrorResponse((HttpStatusCode)500, new HttpError(ex.Message));
            }
        }

        [Route("resendSMSValidation")]
        [HttpPost]
        public HttpResponseMessage ResendSMSValidation(ResendValidateSMSCode data, [FromUri] bool? debug = null)
        {
            try
            {
                UserService.ResendSMSCodeValidation(data.phoneNumber);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
            }
            catch (SMSProcessException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }

        }


        [Route("changeLanguage")]
        [HttpGet]
        public HttpResponseMessage changeLanguage(int languageId)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                //cange language
                int userType = 0;
                var result = UserService.changeLanguage(user.UserId, languageId, ref userType);
                if (result == true)
                {
                    if (userType == (int)UserType.Driver)//only driver
                    {
                        string[] str = DriverService.getStringsForPayment(languageId);
                        var dicResult = new Dictionary<string, string>();
                        dicResult["paymentForMonth"] = str[0];
                        dicResult["paymentForRide"] = str[1];
                        dicResult["paymentForMonthNew"] = str[2];
                        return Request.CreateResponse(HttpStatusCode.OK, new { result = dicResult });
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true });

                }
                throw new ApplicationException();
            }
            catch (SMSProcessException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError(ex.Message));
            }
            catch (UserNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (UserBlockedException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(ex.Message));
            }
            catch (ApplicationException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)511, new HttpError(ex.Message));
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)510, new HttpError(ex.Message));

            }

        }

        [Route("addPhoneToAprovedList")]
        [HttpGet]
        public HttpResponseMessage addPhoneToAprovedList(string phone, string phonePrefix = "+972", int lang = 1)
        {
            try
            {
                string phoneFormatted = UserService.CheckPhoneNumber(phone, int.Parse(phonePrefix));
                bool result = UserService.addPhoneToAprovedList(phoneFormatted, lang);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
            }
            catch (SMSProcessException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError(ex.Message));
            }
            catch (PhoneAlreadyExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (PhoneNumberNotValidException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }

        }


        [Route("updateNotificationToken")]
        [HttpPost]
        public HttpResponseMessage UpdateNotificationToken(UpdateNotification data)
        {
            try
            {
                Logger.Debug("UpdateNotificationToken: " + data.ToJson());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                UserService.UpdateNotificationToken(data.Token, user.UserId, data.UserType, data.Developer);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
            }
            catch (DeviceNotExistException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
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

        /*[Route("pushtest")]
        [HttpPost]
        public HttpResponseMessage PushTest(int type)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var success = UserService.PushTest(user.UserId, type);
                if (success)
                {
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError("error in test push"));
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
        }*/


        [Route("sendMessage")]
        [HttpPost]
        public HttpResponseMessage SendMessage(SendMessage data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                UserService.SendMessage(user.UserId, data.Subject, data.Message);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
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

        [Route("check")]
        [HttpGet]
        public HttpResponseMessage HealthCheck()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }


    }
}
