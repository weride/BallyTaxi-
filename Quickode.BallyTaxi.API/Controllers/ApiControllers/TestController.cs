using Quickode.BallyTaxi.Domain.Services;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Core;
using Quickode.BallyTaxi.Models.Models;
using Quickode.BallyTaxi.Models;
using System;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using Quickode.BallyTaxi.Models.Filters;
using System.Linq;
using System.Net.Mail;
using System.Collections;
using System.Text;
using System.IO;
using System.Speech.Synthesis;
using System.Globalization;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("test")]
    public class TestController : BaseController
    {

        [Route("getAllImagesFromServer")]
        [HttpGet]
        public HttpResponseMessage getAllImagesFromServer()
        {

            List<Image> res = MediaService.getAllImagesFromServer();
            List<object> lContent = new List<object>();
            foreach (var image in res)
            {
                byte[] content = MediaService.GetImageContent(image);

                if (content != null)
                {
                    //HttpResponseMessage response = new HttpResponseMessage();
                    //response.Content = new ByteArrayContent(content);
                    //response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/" + image.Extension);
                    //response.StatusCode = HttpStatusCode.OK;
                    //return response;
                    //lContent.Add(response);
                }
                else
                    lContent.Add(new { image.Filename });
                //throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Request.CreateResponse(HttpStatusCode.OK, lContent);
        }

        // Start test of Health Check
        [Route("healthCheck")]
        [HttpGet]
        public HttpResponseMessage HealthCheck([FromUri] string key)
        {
            if (key != Constants.TestData.HealthCheckKey)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid key");

            //var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }

        [Route("sendSMSToAprovedDrivers")]
        [HttpPost]
        public HttpResponseMessage sendSMSToAprovedDrivers()
        {
            bool res = TestServices.sendSMSToAprovedDrivers();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }

        [Route("onWeekDay")]
        [HttpPost]
        public HttpResponseMessage onWeekDay(long orderId)
        {
            var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
            bool isOnWeekDay = OrderService.onWeekDay(orderId, 31.0461, 34.8516);
            return Request.CreateResponse(HttpStatusCode.OK, new { isOnWeekDay = isOnWeekDay });
        }

        [Route("sendSMSWithType")]
        [HttpGet]
        public HttpResponseMessage sendSMSWithType(int smsType, int minutes = 0)
        {

            bool res = TestServices.sendSMSWithType(smsType, minutes);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }

        [Route("sendAdvertising")]
        [HttpGet]
        public HttpResponseMessage sendAdvertising(int userId)
        {
            bool res = TestServices.sendAdvertising(userId);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }

        [Route("sendSMSPassenger")]
        [HttpGet]
        public HttpResponseMessage sendSMSPassenger()
        {
            //var text = "הורד את אפליקציית ווי-ריידר החדשה לנוסע וקבל קופון לנסיעות ע\"ס 50 ש\"ח http://www.we-rider.com/";
            bool res = TestServices.sendSMSPassenger();
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }

        [Route("sendNotificationTrial")]
        [HttpGet]
        public HttpResponseMessage sendNotificationTrial()
        {

            var request = WebRequest.Create("https://onesignal.com/api/v1/notifications") as HttpWebRequest;

            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";

            request.Headers.Add("authorization", "Basic NjZmYmVkOWUtMzlmZC00OWI1LWIzMDUtZmU3YTg2M2IwODIy");

            byte[] byteArray = Encoding.UTF8.GetBytes("{"
                                                    + "\"app_id\": \"263616f2-3ef8-4347-ada3-3ea15a4f699e\","
                                                    + "\"contents\": {\"en\": \"English Message\"},"
                                                    + "\"include_player_ids\": [\"0444db69-86ef-4653-9e89-ea2fb48c136e\"]}");

            string responseContent = null;

            try
            {
                using (var writer = request.GetRequestStream())
                {
                    writer.Write(byteArray, 0, byteArray.Length);
                }

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        responseContent = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
            }

            System.Diagnostics.Debug.WriteLine(responseContent);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }


        [Route("sendMailTrail")]
        [HttpPost]
        public HttpResponseMessage sendMailTrail()
        {
            bool res = Utils.SendMail(new List<string>() { "shoshana.bash@gmail.com" }, "ניסיון", "ניסיון ללוגו", null);
            return Request.CreateResponse(HttpStatusCode.OK, new { success = res });
        }
        public void sendMail()
        {
            bool res = Utils.SendMail(new List<string>() { "shoshana.bash@gmail.com" }, "ניסיון", "ניסיון ללוגו", null);
        }

        [Route("deleteDriver")]
        [HttpPatch]
        public HttpResponseMessage DeleteDriver([FromUri] string key)
        {
            if (key != Constants.TestData.TestKey)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid key");

            var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

            TestServices.DeleteDriver(user.UserId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }

        [Route("deletePassenger")]
        [HttpPatch]
        public HttpResponseMessage DeletePassenger([FromUri] string key)
        {
            if (key != Constants.TestData.TestKey)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid key");

            var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

            TestServices.DeletePassenger(user.UserId);

            return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
        }

        [Route("driverOrder")]
        [HttpPatch]
        public HttpResponseMessage IsDriverInOrder([FromUri] string key, [FromUri] long orderId)
        {
            if (key != Constants.TestData.TestKey)
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid key");

            var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);


            var orderDriver = TestServices.DriverInOrder(user.UserId, orderId);
            if (orderDriver != null)
                return Request.CreateResponse(HttpStatusCode.OK,
                    new OrderDriverModel(orderDriver));

            return Request.CreateErrorResponse(HttpStatusCode.NoContent, "Driver not in Order");
        }

        [Route("cleanOrders")]
        [HttpGet]
        public HttpResponseMessage CleanOrders(string key)
        {
            if (key != "dl")
                return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Invalid key");

            var msgs = TestServices.CleanPendingOrders();

            return Request.CreateResponse(HttpStatusCode.OK, msgs);
        }

        [Route("TestPush")]
        [HttpGet]
        public HttpResponseMessage TestPush()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                TestServices.TestPush(user.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, "success");
            }
            catch (UserNotExistException ex)
            {
                Logger.Error("UserNotExistException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("push")]
        [HttpGet]
        public HttpResponseMessage Push(long userId)
        {
            try
            {
                TestServices.TestPush(userId);
                return Request.CreateResponse(HttpStatusCode.OK, "success");
            }
            catch (UserNotExistException ex)
            {
                Logger.Error("UserNotExistException:" + ex.Message + ". token=" + Request.Headers.Authorization.Parameter);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }


        [Route("reload")]
        [HttpGet]
        public HttpResponseMessage ConfigurationHelperReload()
        {
            try
            {
                ConfigurationHelper.ConfigurationHelperReload();
                return Request.CreateResponse(HttpStatusCode.OK, "success");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }


        [Route("heatmap")]
        [HttpGet]
        public HttpResponseMessage heatmap(int? NumberofRecords = null, double? LastDateFrom = null, double? LastDateTo = null, double? LastTimeFrom = null, double? LastTimeTo = null, string phone = null)
        {
            try
            {
                var result = TestServices.heatmap(NumberofRecords, LastDateFrom, LastDateTo, LastTimeFrom, LastTimeTo, phone);
                var resultOrder = TestServices.heatmapOredr(NumberofRecords, LastDateFrom, LastDateTo, LastTimeFrom, LastTimeTo);
                return Request.CreateResponse(HttpStatusCode.OK, new { coordinates = result, orders = resultOrder });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }



        [Route("orderT1")]
        [HttpGet]
        public HttpResponseMessage orderT1(long orderId)
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var order = db.Orders.GetById(orderId);
                    string address1 = "";
                    string address2 = "";
                    string cityEN = "";
                    var addressEN = OrderService.getAddressFromLatLong(order.PickUpLocation.Latitude.Value, order.PickUpLocation.Longitude.Value, 2);
                    if (addressEN != null)
                    {
                        address2 = addressEN[0];
                        if (addressEN.Count() > 1 && addressEN[1] != null)
                            cityEN = addressEN[1];
                    }

                    // var availableRadius = cityEN == "Jerusalem" ? ConfigurationHelper.AVAILABLE_RADIUS_JERUSALEM : cityEN == "Tel Aviv-Yafo" ? ConfigurationHelper.AVAILABLE_RADIUS_TelAviv : cityEN == "Ashdod" ? ConfigurationHelper.AVAILABLE_RADIUS_Ashdod : cityEN == "Haifa" ? ConfigurationHelper.AVAILABLE_RADIUS_Haifa : ConfigurationHelper.AVAILABLE_RADIUS;
                    var availableRadius = ConfigurationHelper.AVAILABLE_RADIUS_500;
                    //.WantFutureRide()
                    //   .LocationWithinTime(ConfigurationHelper.UPDATE_MINUTES)
                    //   .Near(order.PickUpLocation, ConfigurationHelper.AVAILABLE_RADIUS)

                    var dateForFutureRide = DateTime.UtcNow.AddMinutes(20);
                    //var dateForFutureRide = DateTime.UtcNow.AddMinutes(30);
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
                                   .NearNew(order.PickUpLocation, (order.courier.HasValue && order.courier.Value > 0) ? ConfigurationHelper.AVAILABLE_RADIUS_FORCourier : (order.isHandicapped.HasValue && order.isHandicapped.Value == true) ? ConfigurationHelper.AVAILABLE_RADIUS_FORHandicapped : (order.isInterCity.HasValue && order.isInterCity.Value == true) ? ConfigurationHelper.AVAILABLE_RADIUS_FORIntercityRide : /*ConfigurationHelper.AVAILABLE_RADIUS*/ availableRadius, order.OrderTime > dateForFutureRide)
                                   .Where(d => d.LastUpdateLocation >= dateFilter)
                                   //.BYPaymentMethod(order.PaymentMethod.Value)
                                   .OrderBy(x => x.Location.Distance(order.PickUpLocation))
                                   // .Take(ConfigurationHelper.MaxDriversOfferedSingleDrive)
                                   .Select(u => new { userId = u.UserId, name = u.User.Name, updateLocation = u.LastUpdateLocation, locationLat = u.Location.Latitude, locationLong = u.Location.Longitude })
                                   .ToList();

                    return Request.CreateResponse(HttpStatusCode.OK, availableRadius.ToJson()); ;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("orderT")]
        [HttpGet]
        public HttpResponseMessage orderT(double lat, double lon, int minute, string password)
        {
            try
            {
                if (password == "Torah613!")
                {
                    using (var db = new BallyTaxiEntities().AutoLocal())
                    {
                        var locationT = Utils.LatLongToLocation(lat, lon);
                        var dateFilter = DateTime.UtcNow.AddMinutes(-minute);
                        var availDrivers = db.Drivers
                                  //.Where(d=>(d.TaxiStation.StationId == 129 /*מוניות הדקה ה99*/ && order.isInterCity == true) || (order.isInterCity == false))
                                  //.isIn99minuteCompAndInInterCity(order.isInterCity)
                                  .AvailableToDrive()
                                  .ThatAreActive()
                                  .WantFutureRide()
                                  .Where(d => d.LastUpdateLocation >= dateFilter)
                                  .Near(locationT, ConfigurationHelper.AVAILABLE_RADIUS_500)
                                  //.Near(locationT, ConfigurationHelper.AVAILABLE_RADIUS)
                                  .OrderBy(x => x.Location.Distance(locationT))
                                  .Take(ConfigurationHelper.MaxDriversOfferedSingleDrive)
                                  .ToList();

                        if (availDrivers != null)
                        {
                            var listDrivers = new List<Dictionary<string, string>>();
                            foreach (var item in availDrivers)
                            {
                                var dic = new Dictionary<string, string>();
                                dic["phone"] = item.User.Phone;
                                dic["lat"] = item.Location.Latitude.Value.ToString();
                                dic["long"] = item.Location.Longitude.Value.ToString();
                                dic["distance"] = item.Location.Distance(locationT).ToString();
                                listDrivers.Add(dic);
                            }
                            return Request.CreateResponse(HttpStatusCode.OK, listDrivers);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, 0);
                    }
                }
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError("password is not valid"));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("convertTextToVoice")]
        [HttpGet]
        public HttpResponseMessage convertTextToVoice(string text)
        {
            //using (var speaker = new SpeechSynthesizer())
            //{
            //    speaker.Voice = (SpeechSynthesizer.AllVoices.First(x => x.Gender == VoiceGender.Female && x.Language.Contains("ES")));
            //    ttssynthesizer.Voice = speaker.Voice;
            //}

            //SpeechSynthesisStream ttsStream = await ttssynthesizer.SynthesizeTextToStreamAsync(TTS);
            try
            {
                SpeechSynthesizer reader = new SpeechSynthesizer();
                // reader.SelectVoiceByHints(VoiceGender.Neutral, VoiceAge.NotSet, 0, CultureInfo.GetCultureInfo("he-IL"));
                reader.SelectVoiceByHints(VoiceGender.Neutral, VoiceAge.NotSet, 0, CultureInfo.GetCultureInfo("he-il"));

                reader.Speak(text);

                // reader.Voice.Culture = NotificationsServices.Current.GetLanguageCulture((int)UserLanguages.he);
                reader.Speak(text);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });

            }
            catch (Exception e)
            {

                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(e.Message));

            }
        }

        [Route("sendCouponToNewPassenger")]
        [HttpGet]
        public HttpResponseMessage sendCouponToNewPassenger(string phone)
        {
            try
            {
                var phoneFormatted = UserService.CheckPhoneNumber(phone, 972);
                var result = TestServices.sendCouponToNewPassenger(phoneFormatted);
                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });

            }
            catch (UserNotExistInMonitexTableException ex)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
            catch (UserNotExistException e)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(e.Message));
            }
            catch (CantSentMultipleCoupon e)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(e.Message));
            }
            catch (Exception e)
            {

                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(e.Message));
            }
        }


        [Route("sendCoupon")]
        [HttpGet]
        public HttpResponseMessage sendCoupon()
        {
            try
            {
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    //var userId = 16409;
                    //var passenger = db.Users.GetById(userId);
                    //Random r = new Random();
                    //int randNum = r.Next(1000000);
                    //string sixDigitNumber = randNum.ToString("D6");
                    ////TODO: must check if number already exist!!!!!

                    //var coupon = new Coupon()
                    //{
                    //    number = sixDigitNumber,
                    //    amount = 50,
                    //    currency = "IL",
                    //    dtStart = DateTime.UtcNow,
                    //    dtEnd = DateTime.UtcNow.AddMonths(1),
                    //    passengerIdSMS = userId
                    //};
                    //db.Coupons.Add(coupon);

                    //db.SaveChanges();

                    //var data = new Dictionary<string, object>();
                    //data.Add("amount", 50);
                    //data.Add("number", sixDigitNumber);
                    //bool status = UserService.SendSMSNotif(passenger.UserId, passenger.Phone, SMSType.CouponTextForSMS, passenger.LanguageId, data);


                    var numberOfSend = 0;
                    var allPassengers = db.Users.Where(u => u.Driver == null && u.Name != null).ToList();
                    foreach (var passenger in allPassengers)
                    {
                        if (numberOfSend < 500)
                        {
                            if (passenger != null)
                            {
                                Random r = new Random();
                                int randNum = r.Next(1000000);
                                string sixDigitNumber = randNum.ToString("D6");

                                var row = db.Coupons.Where(p => p.passengerIdSMS == passenger.UserId).FirstOrDefault();
                                if (row == null)
                                {
                                    numberOfSend++;
                                    var coupon = new Coupon()
                                    {
                                        number = sixDigitNumber,
                                        amount = 50,
                                        currency = "IL",
                                        dtStart = DateTime.UtcNow,
                                        dtEnd = DateTime.UtcNow.AddMonths(1),
                                        passengerIdSMS = passenger.UserId
                                    };
                                    db.Coupons.Add(coupon);
                                    //var couponPassenger = new couponPassenger()
                                    //{
                                    //    couponId = coupon.couponId,
                                    //    passengerId = passenger.UserId
                                    //};
                                    //db.couponPassengers.Add(couponPassenger);
                                    db.SaveChanges();

                                    //var textEn = "You have received a coupon valued at " + amount + " NIS! Enter the code " + number + " before the next ride to redeem it. *The coupon is valid for one month only. Rider App.";
                                    //var textHe = "קיבלת קופון בסך " + amount + " ש''ח. הזן את הקוד " + number + "  לפני הנסיעה הבאה שלך בכדי לממש את ההטבה. *הקופון תקף לחודש ימים. צוות ריידר. ";
                                    var data = new Dictionary<string, object>();
                                    data.Add("amount", 50);
                                    data.Add("number", sixDigitNumber);
                                    bool status = UserService.SendSMSNotif(passenger.UserId, passenger.Phone, SMSType.CouponTextForSMS, passenger.LanguageId, data);
                                    if (status == true)
                                    {
                                        Logger.DebugFormat("SMS was sent to phone: {0} about coupon id: {1} ", passenger.Phone, coupon.couponId);
                                    }
                                    else
                                    {
                                        Logger.DebugFormat("SMS failed sent to phone: {0} about coupon id: {1} ", passenger.Phone, coupon.couponId);
                                    }
                                }
                                else
                                {
                                    Logger.DebugFormat("this User with phone: {0} already get coupon", passenger.Phone);
                                }
                            }
                        }
                        else
                            break;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, 0);
                }

            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(e.Message));
            }
        }


        [Route("testApp")]
        [HttpGet]
        public async System.Threading.Tasks.Task<HttpResponseMessage> testApp(int numTest)
        {
            try
            {
                int numLoop = 0;
                while (numLoop <= numTest)
                {
                    Logger.DebugFormat("start loop number {0}", numLoop);
                    long orderId = 0;
                    try
                    {
                        Logger.Debug("start to check method - HealthCheck");
                        var response = HealthCheck("1234");
                        if (response.StatusCode == HttpStatusCode.OK)
                            Logger.Debug("HealthCheck is ok");
                    }
                    catch (Exception e)
                    {

                        Logger.ErrorFormat("HealthCheck error:", e.Message);
                    }

                    var url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/driver/stations?languageId=1";
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                        client.DefaultRequestHeaders.Add("Authorization", "Token " + "token=VoybMayiZU66ipawGqkquw==");
                        try//1
                        {
                            Logger.Debug("start to check method - driver/stations");
                            var response = await client.GetStringAsync(url1);
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            Dictionary<string, string> stations = jss.Deserialize<Dictionary<string, string>>(response);
                            Logger.Debug("driver/stations is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("driver/stations error:", e.Message);
                        }
                        try//2
                        {
                            Logger.Debug("start to check method - driver/carTypes");
                            url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/driver/carTypes?languageId=1";
                            var response = await client.GetStringAsync(url1);
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            Dictionary<string, string> cars = jss.Deserialize<Dictionary<string, string>>(response);
                            Logger.Debug("driver/carTypes is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("driver/carTypes error:", e.Message);
                        }
                        try//3
                        {
                            Logger.Debug("start to check method - driver/login");
                            url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/driver/login";
                            var content = new FormUrlEncodedContent(new[]
                            {
                            new KeyValuePair<string, string>("", "")
                        });
                            var response = client.PostAsync(url1, content).Result;
                            string resultContent = response.Content.ReadAsStringAsync().Result;
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            DriverDTOModels driver = jss.Deserialize<DriverDTOModels>(resultContent);
                            Logger.Debug("driver/login is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("driver/login error:", e.Message);
                        }
                        try//4
                        {
                            Logger.Debug("start to check method - driver/GetTravelHistory");
                            url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/driver/GetTravelHistory?date=" + DateTime.Now.ConvertToUnixTimestamp();
                            var content = new FormUrlEncodedContent(new[]
                            {
                            new KeyValuePair<string, string>("", "")
                        });
                            var response = client.PostAsync(url1, content).Result;
                            string resultContent = response.Content.ReadAsStringAsync().Result;
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            Dictionary<string, object> driver = jss.Deserialize<Dictionary<string, object>>(resultContent);
                            Logger.Debug("driver/GetTravelHistory is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("driver/GetTravelHistory error:", e.Message);
                        }
                        try//5
                        {
                            Logger.Debug("start to check method - driver/CreateOrder");
                            url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/order/createOrder";
                            //string content = "paymentMethod=2&pickupAddress=כנפי נשרים 21&pickUpCityName=ירושלים&pickupLatitude=31.787249206971229&pickupLongitude=35.1837158203125";

                            OrderModel orderModel = new OrderModel()
                            {
                                paymentMethod = 2,
                                pickUpCityName = "ירושלים",
                                pickupLatitude = 31.787249206971229,
                                pickupLongitude = 35.1837158203125,
                                pickupAddress = "כנפי נשרים 21",
                                time = DateTime.Now.ConvertToUnixTimestamp()
                            };
                            var response = client.PostAsJsonAsync(url1, orderModel).Result;//.PostAsync(url1, content).Result;
                            string resultContent = response.Content.ReadAsStringAsync().Result;
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            var result = jss.Deserialize<Dictionary<string, long>>(resultContent);
                            orderId = result["orderID"];
                            Logger.Debug("driver/CreateOrder is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("driver/CreateOrder error:", e.Message);
                        }
                        try//6
                        {
                            Logger.Debug("start to check method - order/GetEstimationTime");
                            url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/order/getEstimationTime?orderID=" + orderId.ToString();

                            var response = client.GetStringAsync(url1).Result;
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            var result = jss.Deserialize<object>(response);
                            Logger.Debug("order/GetEstimationTime is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("order/GetEstimationTime error:", e.Message);
                        }
                        try//7
                        {
                            Logger.Debug("start to check method - order/getOrderDetailsForDriver");
                            url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/order/getOrderDetailsForDriver?orderID=" + orderId.ToString();

                            var response = client.GetStringAsync(url1).Result;
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            var result = jss.Deserialize<OrderDetailsModel>(response);
                            Logger.Debug("order/getOrderDetailsForDriver is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("order/getOrderDetailsForDriver error:", e.Message);
                        }
                        try//8
                        {
                            Logger.Debug("start to check method - order/getOrderDetailsForPassenger");
                            url1 = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/order/getOrderDetailsForPassenger?orderID=" + orderId.ToString();

                            var response = client.GetStringAsync(url1).Result;
                            JavaScriptSerializer jss = new JavaScriptSerializer();
                            var result = jss.Deserialize<OrderDetailsModel>(response);
                            Logger.Debug("order/getOrderDetailsForPassenger is ok");
                        }
                        catch (Exception e)
                        {

                            Logger.ErrorFormat("order/getOrderDetailsForPassenger error:", e.Message);
                        }
                    }
                    numLoop++;
                }
                Logger.Debug("the test completed");
                return Request.CreateResponse(HttpStatusCode.OK, 200);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)507, new HttpError(ex.Message));
            }
        }

        [Route("saveDetails")]
        [HttpGet]
        public HttpResponseMessage saveDetails(string name, string email, string phone, string type)//NewUser data
        {
            try
            {
                var result = TestServices.saveDetails(name, email, phone, type);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (UserForbiddenException e)
            {
                Logger.Error(e);
                return Request.CreateErrorResponse((HttpStatusCode)405, new HttpError(e.Message));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)403, new HttpError(ex.Message));
            }


        }

        [Route("daleteFromPhoneApproved")]
        [HttpGet]
        public HttpResponseMessage daleteFromPhoneApproved(string phone)
        {
            try
            {
                phone = "+" + phone;
                phone = phone.Replace(" ", "");
                var result = TestServices.daleteFromPhoneApproved(phone);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }

            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse((HttpStatusCode)403, new HttpError(ex.Message));
            }


        }
    }
}