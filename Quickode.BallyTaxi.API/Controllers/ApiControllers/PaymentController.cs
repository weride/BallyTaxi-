using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Collections.Specialized;
using System.Net.Http;
using System.Web.Http;
using Quickode.BallyTaxi.Domain.Services;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Models.Models;
using Quickode.BallyTaxi.Core;

namespace Quickode.BallyTaxi.API.Controllers
{
    [RoutePrefix("Payment")]
    public class PaymentController : BaseController
    {
        [Route("getBankList")]
        [HttpGet]
        public HttpResponseMessage getBankList()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);

                var banksList = PaymentService.getBankList();
                return Request.CreateResponse(HttpStatusCode.OK, banksList);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in getBankList : {0}", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }

        [Route("addBankAccountDetails")]
        [HttpPost]
        public HttpResponseMessage addBankAccountDetails(BankAccountModel data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var result = PaymentService.addBankAccountDetails(data, user.UserId);
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in getBankList : {0}", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }


        [Route("GetTokenForCC")]
        [HttpPost]
        public HttpResponseMessage GetTokenForCC(CreditCardModel data)
        {
            try
            {
                Logger.DebugFormat("GetTokenForCC data: {0}", data.ToJson());
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var result = PaymentService.GetTokenForCC(user.UserId, data.CreditCardNumber, data.CVV, 1, data.expMonth, data.expYear);
                if (result != null)
                {
                    //Logger.Debug("token: " + token);
                    result = result.Substring(result.Length - 4);
                    return Request.CreateResponse(HttpStatusCode.OK, new { lastNumbersCC = result });
                }
                else
                {
                    Logger.Debug("error in GetTokenForCC: ");
                    return Request.CreateErrorResponse((HttpStatusCode)550, new HttpError("error in GetTokenForCC"));
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in GetTokenForCC : {0}", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }

        [Route("PreAuthForCC")]
        [HttpPost]
        public HttpResponseMessage PreAuthForCC(AmountForCCModel data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var result = PaymentService.PreAuthForCC(user.UserId, data.currency);
                if (result != null)
                {
                    //Logger.Debug("token: " + token);
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    Logger.Debug("error in GetTokenForCC: ");
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError("error in GetTokenForCC"));
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in GetTokenForCC : ", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }

        [Route("DoPaymentForCC")]
        [HttpPost]
        public HttpResponseMessage DoPaymentForCC(AmountForCCModel data)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var result = PaymentService.DoPaymentForCCTranmodeA(user.UserId, data.amount, data.currency, 0);
                if (result == true)
                {
                    //Logger.Debug("token: " + token);
                    return Request.CreateResponse(HttpStatusCode.OK, result);
                }
                else
                {
                    Logger.Debug("error in DoPaymentForCC: ");
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError("error in DoPaymentForCC"));
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in DoPaymentForCC : ", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }

        [Route("SetExpressCheckout")]
        [HttpPost]
        public HttpResponseMessage SetExpressCheckout()
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var token = PaymentService.SetExpressCheckout(user.LanguageId);
                if (token != null)
                {
                    Logger.Debug("token: " + token);
                    return Request.CreateResponse(HttpStatusCode.OK, token);
                }
                else
                {
                    Logger.Debug("error in SetExpressCheckout: ");
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError("error in SetExpressCheckout"));
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in SetExpressCheckout : ", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }

        //
        [Route("CreateBillingAgreement")]
        [HttpPost]
        public HttpResponseMessage CreateBillingAgreement([FromUri]string token)
        {
            try
            {
                var user = UserService.CheckAuthorization(Request.Headers.Authorization.Scheme, Request.Headers.Authorization.Parameter);
                var BillingId = PaymentService.CreateBillingAgreement(token, user.UserId);
                if (BillingId != null)
                {
                    Logger.Debug("BillingId: " + BillingId);
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError("error in CreateBillingAgreement"));
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in CreateBillingAgreement : {0}", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }


        [Route("DoReferenceTransaction")]
        [HttpPost]
        public HttpResponseMessage DoReferenceTransaction([FromUri]bool isInterCity, [FromUri]long orderId)
        {
            try
            {
                var amount = isInterCity == true ? 500 : 100;
                var result = PaymentService.DoReferenceTransaction(orderId, amount);
                if (result == null)//the function faild:
                {
                    Logger.ErrorFormat("Error in DoReferenceTransaction  ");
                    //PaymentService.SendErrorInPayPalToDriversAndPassenger(orderId);
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, new HttpError("error in DoReferenceTransaction"));
                }
                else//in sucsess
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Error in DoReferenceTransaction : ", e.Message);
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError(e.Message));
            }
        }

        [Route("PayPalPayment")]
        [HttpPost]
        public HttpResponseMessage PayPalPayment(PayPalPaymentModel model)
        {
            try
            {
                ////method 1:
                //https://api-3t.sandbox.paypal.com/nvp&USER = rider - driver_api1.gmail.com & PWD = 4KHRP5CQ5JHTKBJQ & SIGNATURE = AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW & VERSION = 124.0 & METHOD = SetExpressCheckout & RETURNURL = https://example/playgrounds/api/ec/?call=GetExpressCheckoutDetails&CANCELURL=https://example/playgrounds/api/ec/&localecode=US&HDRIMG=https://www.paypal.com/example/i/logo/logo_150x65.gif&LOGOIMG=https://www.paypal.com/example/i/logo/logo_150x65.gif&BRANDNAME=PayPal Test Site&CUSTOMERSERVICENUMBER=0123456789&paymentrequest_0_currencycode=USD&PAYMENTREQUEST_0_DESC=Free Text Description&PAYMENTREQUEST_0_CUSTOM=Open for merchant use&paymentrequest_0_paymentaction=Sale&l_billingtype0=MerchantInitiatedBilling&L_BILLINGAGREEMENTDESCRIPTION0=Free text description&l_paymenttype0=InstantOnly&L_BILLINGAGREEMENTCUSTOM0=Free for merchant use

                var url = "https://api-3t.sandbox.paypal.com/nvp";
                //var url = "https://api-3t.paypal.com/nvp";
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                using (WebClient client = new WebClient())
                {
                    byte[] response =
                    client.UploadValues(url, new NameValueCollection()
                    {
                            {"USER", model.userName },
                            {"PWD", model.password },
                            {"SIGNATURE", model.signature },
                            {"VERSION", "124.0"},
                            {"METHOD", "SetExpressCheckout"},
                            {"RETURNURL", "http://localhost:51029/Views/HtmlPage1.html" },
                            {"CANCELURL", "https://example/playgrounds/api/ec/" },
                           // {"localecode", "US" },
                            {"localecode", "IL" },
                            {"HDRIMG", "https://www.paypal.com/example/i/logo/logo_150x65.gif" },
                            {"LOGOIMG", "https://www.paypal.com/example/i/logo/logo_150x65.gif" },
                            {"BRANDNAME", "PayPal Test Site"},
                            {"CUSTOMERSERVICENUMBER", "0123456789"},
                            //{"paymentrequest_0_currencycode","USD" },
                            {"paymentrequest_0_currencycode","ILS" },
                            {"PAYMENTREQUEST_0_DESC", "Free Text Description"},
                            {"PAYMENTREQUEST_0_CUSTOM", "Open for merchant use" },
                            {"paymentrequest_0_paymentaction", "Sale" },
                            {"l_billingtype0", "MerchantInitiatedBilling" },
                            {"L_BILLINGAGREEMENTDESCRIPTION0", "Free text description"},
                            {"l_paymenttype0", "InstantOnly"},
                            {"L_BILLINGAGREEMENTCUSTOM0","Free for merchant use" }
                    });

                    string result = System.Text.Encoding.UTF8.GetString(response);

                    //var bytesAsString = System.Text.Encoding.ASCII.GetString(response);
                    //var person = Newtonsoft.Json.JsonConvert.DeserializeObject<Array>(bytesAsString);

                    //using (var stream = new System.IO.MemoryStream(response))
                    //using (var reader = new System.IO.StreamReader(stream))
                    //    var json = Newtonsoft.Json.JsonSerializer.Create().Deserialize(reader, typeof( Array));

                    //System.IO.Stream stream = result.to;
                    //System.IO.StreamReader sr = new StreamReader(stream);

                    // var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    // var jsonObject = serializer.DeserializeObject(response.ReadToEnd());

                    string token = result.Replace("TOKEN=", "");
                    token = token.Split('&')[0];
                    token = token.Replace("%2d", "-");

                    //&TIMESTAMP
                    //method 2:
                    //https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token=EC-1RS85233MU2973231&useraction=commit
                    url = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=express-checkout";
                    //url = "https://www.paypal.com/cgi-bin/webscr?cmd=express-checkout";

                    response =
                    client.UploadValues(url, new NameValueCollection()
                    {
                          //{ "cmd", "express-checkout" },
                          {"token", token},
                          {"useraction", "commit"}
                    });
                    string result2 = System.Text.Encoding.UTF8.GetString(response);

                    //method 3:
                    //https://api-3t.sandbox.paypal.com/nvp 
                    //&USER=rider-driver_api1.gmail.com&PWD=4KHRP5CQ5JHTKBJQ&SIGNATURE=AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW&VERSION=124.0&METHOD=GetExpressCheckoutDetails&TOKEN=EC-77N21475UF2703144
                    url = "https://api-3t.sandbox.paypal.com/nvp";
                    //url = "https://api-3t.paypal.com/nvp";
                    response =
                    client.UploadValues(url, new NameValueCollection()
                    {
                            {"USER", model.userName },
                            {"PWD", model.password },
                            {"SIGNATURE", model.signature },
                            {"VERSION", "124.0"},
                            {"METHOD", "GetExpressCheckoutDetails"},
                            {"TOKEN", token }
                    });
                    string result3 = System.Text.Encoding.UTF8.GetString(response);

                    //method 4:
                    //https://api-3t.sandbox.paypal.com/nvp
                    //&USER=rider-driver_api1.gmail.com&PWD=4KHRP5CQ5JHTKBJQ&SIGNATURE=AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW&VERSION=124.0&METHOD=CreateBillingAgreement&TOKEN=EC-77N21475UF2703144&NOTIFYURL=http://example/IPN/database/store.php

                    response =
                    client.UploadValues(url, new NameValueCollection()
                    {
                            { "USER", model.userName },
                            { "PWD", model.password },
                            {"SIGNATURE", model.signature },
                            {"VERSION", "124.0"},
                            {"METHOD", "CreateBillingAgreement"},
                            {"TOKEN", token },
                            { "NOTIFYURL", "http://example/IPN/database/store.php" }
                    });
                    string result4 = System.Text.Encoding.UTF8.GetString(response);

                    var billingAgreementId = result4.Replace("BILLINGAGREEMENTID=", "");
                    billingAgreementId = billingAgreementId.Split('&')[0];
                    billingAgreementId = billingAgreementId.Replace("%2d", "-");

                    //var billingAgreementId = "B-750190528S304935D";
                    // method 5:
                    // https://api-3t.sandbox.paypal.com/nvp;
                    //&USER=rider-driver_api1.gmail.com&PWD=4KHRP5CQ5JHTKBJQ&SIGNATURE=AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW&VERSION=124.0&METHOD=DoReferenceTransaction&REFERENCEID=B-7V2186058N105235U&paymentaction =Authorization&paymenttype=InstantOnly&IPADDRESS=127.0.0.1&RISKSESSIONCORRELATIONID =90FA1B3E-03F8-4B5B-82EC-7A7519294B9B&MERCHANTSESSIONID =rbjhmki6n18u3metl0mmh0rf36&AMT=10¤cycode=USD&ITEMAMT=10&DESC =Your Description&CUSTOM=Open for merchant use&INVNUM=Merchant internal order identifier&NOTIFYURL =http://example/IPN/database/store.php

                    response =
                    client.UploadValues(url, new NameValueCollection()
                    {
                        { "USER", model.userName },
                        { "PWD", model.password },
                        {"SIGNATURE", model.signature },
                        {"VERSION", "124.0"},
                        {"METHOD", "DoReferenceTransaction"},
                        //{"TOKEN", token },
                        { "REFERENCEID", billingAgreementId },
                        { "paymentaction", "Authorization" },
                        {"paymenttype",  "InstantOnly"},
                        {"IPADDRESS", "127.0.0.1" },
                        {"RISKSESSIONCORRELATIONID", "90FA1B3E-03F8-4B5B-82EC-7A7519294B9B" },
                        { "MERCHANTSESSIONID", "rbjhmki6n18u3metl0mmh0rf36"},
                        {"AMT", "10" },
                        {"cycode", "USD" },
                        {"ITEMAMT", "10" },
                        { "DESC", "Your Description"},
                        { "CUSTOM", "Open for merchant use" },
                        { "INVNUM", "70" },
                        { "NOTIFYURL", "http://example/IPN/database/store.php"}
                    });
                    string result5 = System.Text.Encoding.UTF8.GetString(response);
                    var s = System.Text.RegularExpressions.Regex.Split(result5, "TRANSACTIONID=");
                    var transactionId = s[1];
                    transactionId = transactionId.Split('&')[0];

                    //method 6:
                    //&USER=rider-driver_api1.gmail.com&PWD=4KHRP5CQ5JHTKBJQ&SIGNATURE=AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW&VERSION=124.0&METHOD=DoCapture&AUTHORIZATIONID=1C451923Y0354211P&AMT=10¤cycode=USD&
                    //completetype =Complete&INVNUM=Merchant internal order identifier&NOTE=Free for merchant use

                    response =
                   client.UploadValues(url, new NameValueCollection()
                   {
                        { "USER", model.userName },
                        { "PWD", model.password },
                        {"SIGNATURE", model.signature },
                        {"VERSION", "124.0"},
                        {"METHOD", "DoCapture"},
                        { "AUTHORIZATIONID", transactionId },
                        {"AMT", "10" },
                        {"cycode", "USD" },
                        {"completetype", "Complete" },
                        {"INVNUM", "70" },
                        {"NOTE", "Free for merchant use" }
                   });
                    string result6 = System.Text.Encoding.UTF8.GetString(response);
                    return Request.CreateResponse(HttpStatusCode.OK, result6);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, new HttpError(ex.Message));
            }
        }
    }
}