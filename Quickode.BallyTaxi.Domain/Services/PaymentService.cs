using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Models.Filters;
using System.Net;
using System.Collections.Specialized;
using System.Net.Http;
using System.Web.Script.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using Quickode.BallyTaxi.Models.Models;
using System.Security.Cryptography;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class PaymentService
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string SetExpressCheckout(int LanguageId)
        {
            try
            {
                //var url = "https://api-3t.sandbox.paypal.com/nvp";
                var url = "https://api-3t.paypal.com/nvp";
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                using (WebClient client = new WebClient())
                {
                    using (var db = new BallyTaxiEntities().AutoLocal())
                    {
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
                        byte[] response =
                        client.UploadValues(url, new NameValueCollection()
                        {
                            { "USER", userName },
                            { "PWD", password },
                            {"SIGNATURE", signature },
                            {"VERSION", "124.0"},
                            {"METHOD", "SetExpressCheckout"},
                            {"RETURNURL", System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + (LanguageId==(int)UserLanguages.he? "/HtmlPage_HE.html": "/HtmlPage_EN.html") }, //"https://example/playgrounds/api/ec/?call=GetExpressCheckoutDetails" },
                            { "CANCELURL", "https://example/playgrounds/api/ec/" },
                            { "localecode", LanguageId==(int)UserLanguages.he? "he_IL" : "US"},
                            { "HDRIMG", "https://www.paypal.com/example/i/logo/logo_150x65.gif" },
                            { "LOGOIMG", "https://www.paypal.com/example/i/logo/logo_150x65.gif" },
                            //this i add:
                            //{"CURRENCY", "ILS" },
                            //end
                            {"BRANDNAME", "New Werider App"},
                            {"CUSTOMERSERVICENUMBER", "613613"},
                            {"paymentrequest_0_currencycode","ILS" },
                            {"PAYMENTREQUEST_0_DESC", "No Charge - Preauthorized Payment only"},
                            {"PAYMENTREQUEST_0_CUSTOM", "Paypal Preauth for Paypal" },
                            {"paymentrequest_0_paymentaction", "Sale" },
                            {"l_billingtype0", "MerchantInitiatedBilling" },
                            {"L_BILLINGAGREEMENTDESCRIPTION0", "Preauthorized Payment for Werider"},
                            {"l_paymenttype0", "InstantOnly"},
                            {"L_BILLINGAGREEMENTCUSTOM0","Paypal Preauth for Paypal" }
                        });

                        string result = Encoding.UTF8.GetString(response);
                        if (result.Contains("TOKEN="))
                        {
                            string token = result.Replace("TOKEN=", "");
                            token = token.Split('&')[0];
                            token = token.Replace("%2d", "-");
                            return token;
                        }
                        else //error
                        {
                            Logger.ErrorFormat("error in SetExpressCheckout: {0}", result);
                            return null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("error in SetExpressCheckout: {0}", e.Message);
                return null;
            }
        }

        public static bool addBankAccountDetails(BankAccountModel data, long userId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var driver = db.Drivers.Where(d => d.UserId == userId).FirstOrDefault();
                if (driver != null)
                {
                    if (data.bankAccountNumber.Length >= 4)
                    {
                        var stringToEncrypt = data.bankAccountNumber.Substring(0, data.bankAccountNumber.Length - 4);
                        var accountEncript = EncryptString(stringToEncrypt); //
                        var last4Digits = data.bankAccountNumber.Substring(data.bankAccountNumber.Length - 4);
                        // var decriptString = DecryptString(accountEncript);
                        driver.BankNumber = data.bankNumber;
                        driver.BankBranch = data.bankBranch;
                        driver.BankHolderName = data.bankHolderName;
                        driver.BankAccount = accountEncript + last4Digits;
                        db.SaveChanges();
                        return true;
                    }
                }
                return false;
            }
        }

        public static string EncryptString(string inputString)
        {
            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "Torah613"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = Encoding.UTF8.GetBytes(inputString);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                memStream = new MemoryStream();
                ICryptoTransform transform = provider.CreateEncryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();

            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            return Convert.ToBase64String(memStream.ToArray());
        }

        public static string DecryptString(string inputString)
        {
            MemoryStream memStream = null;
            try
            {
                byte[] key = { };
                byte[] IV = { 12, 21, 43, 17, 57, 35, 67, 27 };
                string encryptKey = "Torah613"; // MUST be 8 characters
                key = Encoding.UTF8.GetBytes(encryptKey);
                byte[] byteInput = new byte[inputString.Length];
                byteInput = Convert.FromBase64String(inputString);
                DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
                memStream = new MemoryStream();
                ICryptoTransform transform = provider.CreateDecryptor(key, IV);
                CryptoStream cryptoStream = new CryptoStream(memStream, transform, CryptoStreamMode.Write);
                cryptoStream.Write(byteInput, 0, byteInput.Length);
                cryptoStream.FlushFinalBlock();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }

            Encoding encoding1 = Encoding.UTF8;
            return encoding1.GetString(memStream.ToArray());
        }

        public static List<BankToDisplay> getBankList()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var banks = db.Banks.Select(b => new BankToDisplay() { bankId = b.bankId, bankName = b.bankName }).ToList();
                return banks;
            }
        }

        public static object DoPaymentForCC(long userId, double amount, int currency)
        {

            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                var token = "";
                var cvv = "";
                var expDate = "";
                var row = db.CreditCardUsers.Where(c => c.userId == userId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(o => o.isDefault).FirstOrDefault();
                if (row != null)
                {
                    token = row.tokenId;
                    cvv = row.cvv;
                    expDate = row.expdate;
                }
                Logger.DebugFormat("DoPaymentForCC with tokenid: {0}, for userId: {1}", token, userId);
                var url = "https://secure5.tranzila.com/cgi-bin/tranzila71pme.cgi";
                string result = "";
                string tranzilaPW = ConfigurationHelper.TranzilaPW;
                string supplier = ConfigurationHelper.TranzilaSupplier;
                //  string strPost = "supplier=ttxriderapp&sum=" + amount + "&currency=" + currency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=UcIQoP&TranzilaTK=" + token + "&index=156695" + "&tranmode=F";
                //string strPost = "supplier=ttxwerider&sum=" + amount + "&currency=" + currency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=aCOfTA&TranzilaTK=" + token + "&index=156695" + "&tranmode=F";
                string strPost = "supplier=" + supplier + "&sum=" + amount + "&currency=" + currency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=" + tranzilaPW + "&TranzilaTK=" + token + "&index=156695" + "&tranmode=F";
                StreamWriter myWriter = null;

                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Method = "POST";
                objRequest.ContentLength = strPost.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";

                try
                {
                    myWriter = new StreamWriter(objRequest.GetRequestStream());
                    myWriter.Write(strPost);
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                finally
                {
                    myWriter.Close();
                }

                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (StreamReader sr =
                   new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();


                    // Close and clean up the StreamReader
                    sr.Close();
                }
                return result;
            }
        }

        public static bool DoPaymentForCCTranmodeA(long userId, double amount, int currency, long orderId)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {

                var token = "";
                var cvv = "";
                var expDate = "";
                // var row = db.CreditCardUsers.Where(c => c.userId == userId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(o => o.isDefault).FirstOrDefault();
                var row = db.CreditCardUsers.Where(c => c.userId == userId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(o => o.isDefault).FirstOrDefault();
                if (row != null)
                {
                    token = row.tokenId;
                    cvv = row.cvv;
                    expDate = row.expdate;
                }
                var url = "https://secure5.tranzila.com/cgi-bin/tranzila71pme.cgi";
                string result = "";
                string tranzilaPW = ConfigurationHelper.TranzilaPW;
                string supplier = ConfigurationHelper.TranzilaSupplier;
                string strPost = "supplier=" + supplier + "&sum=" + amount + "&currency=" + currency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=" + tranzilaPW + "&TranzilaTK=" + token + "&tranmode=A&OrderNumber=" + orderId.ToString();
                //  string strPost = "supplier=ttxriderapp&sum=" + amount + "&currency=" + currency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=UcIQoP&TranzilaTK=" + token + "&tranmode=A&OrderNumber=" + orderId.ToString();
                StreamWriter myWriter = null;

                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Method = "POST";
                objRequest.ContentLength = strPost.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";

                try
                {
                    myWriter = new StreamWriter(objRequest.GetRequestStream());
                    myWriter.Write(strPost);
                }
                catch (Exception e)
                {
                    return false;
                }
                finally
                {
                    myWriter.Close();
                }

                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (StreamReader sr =
                   new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    Logger.DebugFormat("the result that return from tranzila about payment: {0}", result);
                    if (result.Contains("Response=000"))
                    {
                        return true;
                    }

                    // Close and clean up the StreamReader
                    sr.Close();
                }
                return false;
            }
        }

        public static bool checkCardCorrect(string cvv, string expDate, int courency, string token)
        {
            try
            {
                Logger.DebugFormat("checkCardCorrect with tokenid: {0}, ", token);
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var url = "https://secure5.tranzila.com/cgi-bin/tranzila71pme.cgi";
                    string result = "";
                    string tranzilaPW = ConfigurationHelper.TranzilaPW;
                    string supplier = ConfigurationHelper.TranzilaSupplier;
                    // string strPost = "supplier=ttxriderapp&sum=5&currency=" + courency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=UcIQoP&TranzilaTK=" + token + "&tranmode=V";
                    // string strPost = "supplier=ttxwerider&sum=5&currency=" + courency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=aCOfTA&TranzilaTK=" + token + "&tranmode=V";
                    //tranmode=V בדיקת אימות ללא חיוב
                    //tranmode=A מבצע אימות וחיוב כספי!
                    string strPost = "supplier=" + supplier + "&sum=5&currency=" + courency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=" + tranzilaPW + "&TranzilaTK=" + token + "&tranmode=V";
                    StreamWriter myWriter = null;

                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                    objRequest.Method = "POST";
                    objRequest.ContentLength = strPost.Length;
                    objRequest.ContentType = "application/x-www-form-urlencoded";

                    try
                    {
                        myWriter = new StreamWriter(objRequest.GetRequestStream());
                        myWriter.Write(strPost);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorFormat("checkCardCorrect error", e.Message);
                        return false;
                    }
                    finally
                    {
                        myWriter.Close();
                    }

                    HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                    using (StreamReader sr =
                       new StreamReader(objResponse.GetResponseStream()))
                    {
                        result = sr.ReadToEnd();
                        // Close and clean up the StreamReader
                        sr.Close();
                        if (result.Contains("Response=000"))
                        {
                            return true;
                        }
                        else
                            Logger.ErrorFormat("check card is not correct: {0}", result);
                    }
                    return false;
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("checkCardCorrect error", e.Message);
                return false;
            }
        }

        public static object PreAuthForCC(long userId, int Courency)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var token = "";
                var cvv = "";
                var expDate = "";
                var row = db.CreditCardUsers.Where(c => c.userId == userId).OrderByDescending(o => o.creditCardUser1).OrderByDescending(o => o.isDefault).FirstOrDefault();
                if (row != null)
                {
                    token = row.tokenId;
                    cvv = row.cvv;
                    expDate = row.expdate;
                }
                Logger.DebugFormat("PreAuthForCC with tokenid: {0}, for userId: {1}", token, userId);

                var url = "https://secure5.tranzila.com/cgi-bin/tranzila71pme.cgi";
                string result = "";
                string tranzilaPW = ConfigurationHelper.TranzilaPW;
                string supplier = ConfigurationHelper.TranzilaSupplier;

                string strPost = "supplier=" + supplier + "&sum=5&currency=" + Courency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=" + tranzilaPW + "&TranzilaTK=" + token + "&tranmode=V";
                //  string strPost = "supplier=ttxriderapp&sum=5&currency=" + Courency + "&expdate=" + expDate + "&mycvv=" + cvv + "&TranzilaPW=UcIQoP&TranzilaTK=" + token + "&tranmode=V";
                StreamWriter myWriter = null;
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Method = "POST";
                objRequest.ContentLength = strPost.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";
                try
                {
                    myWriter = new StreamWriter(objRequest.GetRequestStream());
                    myWriter.Write(strPost);
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                finally
                {
                    myWriter.Close();
                }

                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (StreamReader sr =
                   new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    // Close and clean up the StreamReader
                    sr.Close();
                }
                return result;
            }
        }

        public static string GetTokenForCC(long userId, string creditCardNumber, string cVV, int courency, string expMonth, string expYear)
        {
            try
            {
                if (expMonth.Length == 1)
                    expMonth = '0' + expMonth;
                if (expYear.Length == 1)
                    expYear = '0' + expYear;
                if (expYear.Length > 2)
                {
                    expYear = expYear.Substring(expYear.Length - 2);
                }
                var expDate = expMonth + expYear;
                var url = "https://secure5.tranzila.com/cgi-bin/tranzila71pme.cgi";

                string result = "";
                string tranzilaPW = ConfigurationHelper.TranzilaPW;
                string supplier = ConfigurationHelper.TranzilaSupplier;

                string strPost = "supplier=" + supplier + "&sum=5&currency=" + courency + "&ccno=" + creditCardNumber + "&mycvv=" + cVV + "&expdate=" + expDate + "&TranzilaPW=" + tranzilaPW + "&TranzilaTK=1&OrderNumber=0";
                //string strPost = "supplier=ttxriderapp&sum=5&currency=" + courency + "&ccno=" + creditCardNumber + "&mycvv=" + cVV + "&expdate=" + expDate + "&TranzilaPW=UcIQoP&TranzilaTK=1&OrderNumber=0";
                StreamWriter myWriter = null;

                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(url);
                objRequest.Method = "POST";
                objRequest.ContentLength = strPost.Length;
                objRequest.ContentType = "application/x-www-form-urlencoded";
                try
                {
                    myWriter = new StreamWriter(objRequest.GetRequestStream());
                    myWriter.Write(strPost);
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                finally
                {
                    myWriter.Close();
                }

                HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse();
                using (StreamReader sr =
                   new StreamReader(objResponse.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                    if (result.Contains("TranzilaTK="))
                    {
                        string token = result.Replace("TranzilaTK=", "");
                        token = token.Replace("\n", "");
                        result = token;

                        //check if the Credit Card Number is Correct:
                        var resultCheck = checkCardCorrect(cVV, expDate, courency, token);
                        if (resultCheck == true)
                        {
                            using (var db = new BallyTaxiEntities().AutoLocal())
                            {
                                var CardRow = db.CreditCardUsers.Where(c => c.tokenId.Substring(c.tokenId.Length - 4) == token.Substring(token.Length - 4) && c.cvv == cVV && c.userId == userId).FirstOrDefault();

                                var isExsistIsDefault = db.CreditCardUsers.Where(c => c.userId == userId && c.isDefault == true).FirstOrDefault();
                                if (isExsistIsDefault != null)
                                    isExsistIsDefault.isDefault = false;

                                db.CreditCardUsers.Add(new CreditCardUser() { tokenId = token, userId = userId, cvv = cVV, expdate = expDate, isDefault = true });
                                if (CardRow != null)
                                {
                                    db.CreditCardUsers.Remove(CardRow);
                                }
                                db.SaveChanges();
                            }
                            return result;
                        }
                    }
                    else
                        Logger.DebugFormat("error in get token for cc: {0}", result);
                    // Close and clean up the StreamReader
                    sr.Close();
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("GetTokenForCC", ex.Message);
                return null;
            }
        }


        //public static void SendErrorInPayPalToDriversAndPassenger(long orderId)
        //{
        //    var data = new Dictionary<string, object>();
        //    using (var db = new BallyTaxiEntities().AutoLocal())
        //    {
        //        var passenger = db.Users.Where(u => u.UserId == db.Orders.Where(o => o.OrderId == orderId).FirstOrDefault().PassengerId).FirstOrDefault();

        //        var driversId = db.Orders_Drivers.Where(o => o.OrderId == orderId).Select(o => o.DriverId).ToList();
        //        var drivers = db.Users.Where(u => driversId.Contains(u.UserId)).ToList();
        //            //u => u. db.Orders_Drivers.Where(od => od.OrderId == orderId).Select(od => od.DriverId).ToList().Contains(u.UserId)).ToList();
        //        //db.Orders_Drivers.Where(od => od.OrderId == orderId).ToList().Select(orderId).Contains(u.UserId)).ToList();
        //        NotificationsServices.Current.PassengerNotification(passenger, PassengerNotificationTypes.paypalPassengerError, orderId);
        //        foreach (var driver in drivers)
        //        {
        //            NotificationsServices.Current.DriverNotification(driver, DriverNotificationTypes.paypalDriverError, orderId, data);
        //        }

        //    }
        //}

        public static string CreateBillingAgreement(string token, long userId)
        {
            try
            {
                //https://api-3t.sandbox.paypal.com/nvp&USER = rider - driver_api1.gmail.com & PWD = 4KHRP5CQ5JHTKBJQ & SIGNATURE = AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW & VERSION = 124.0 & METHOD = SetExpressCheckout & RETURNURL = https://example/playgrounds/api/ec/?call=GetExpressCheckoutDetails&CANCELURL=https://example/playgrounds/api/ec/&localecode=US&HDRIMG=https://www.paypal.com/example/i/logo/logo_150x65.gif&LOGOIMG=https://www.paypal.com/example/i/logo/logo_150x65.gif&BRANDNAME=PayPal Test Site&CUSTOMERSERVICENUMBER=0123456789&paymentrequest_0_currencycode=USD&PAYMENTREQUEST_0_DESC=Free Text Description&PAYMENTREQUEST_0_CUSTOM=Open for merchant use&paymentrequest_0_paymentaction=Sale&l_billingtype0=MerchantInitiatedBilling&L_BILLINGAGREEMENTDESCRIPTION0=Free text description&l_paymenttype0=InstantOnly&L_BILLINGAGREEMENTCUSTOM0=Free for merchant use

                //var url = "https://api-3t.sandbox.paypal.com/nvp";
                var url = "https://api-3t.paypal.com/nvp";
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                using (WebClient client = new WebClient())
                {
                    using (var db = new BallyTaxiEntities().AutoLocal())
                    {
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

                        //method 3:
                        //https://api-3t.sandbox.paypal.com/nvp 
                        //&USER=rider-driver_api1.gmail.com&PWD=4KHRP5CQ5JHTKBJQ&SIGNATURE=AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW&VERSION=124.0&METHOD=GetExpressCheckoutDetails&TOKEN=EC-77N21475UF2703144

                        var response =
                        client.UploadValues(url, new NameValueCollection()
                        {
                            { "USER", userName },
                            { "PWD",password },
                            {"SIGNATURE", signature },
                            {"VERSION", "124.0"},
                            {"METHOD", "GetExpressCheckoutDetails"},
                            {"TOKEN", token }
                        });
                        string result3 = Encoding.UTF8.GetString(response);

                        //method 4:
                        //https://api-3t.sandbox.paypal.com/nvp
                        //&USER=rider-driver_api1.gmail.com&PWD=4KHRP5CQ5JHTKBJQ&SIGNATURE=AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW&VERSION=124.0&METHOD=CreateBillingAgreement&TOKEN=EC-77N21475UF2703144&NOTIFYURL=http://example/IPN/database/store.php

                        response =
                        client.UploadValues(url, new NameValueCollection()
                        {
                            { "USER", userName },
                            { "PWD", password },
                            {"SIGNATURE", signature },
                            {"VERSION", "124.0"},
                            {"METHOD", "CreateBillingAgreement"},
                            {"TOKEN", token },
                            {"NOTIFYURL", "http://example/IPN/database/store.php" }
                        });
                        string result4 = Encoding.UTF8.GetString(response);
                        if (result4.Contains("BILLINGAGREEMENTID="))
                        {
                            var billingAgreementId = result4.Replace("BILLINGAGREEMENTID=", "");
                            billingAgreementId = billingAgreementId.Split('&')[0];
                            billingAgreementId = billingAgreementId.Replace("%2d", "-");
                            var user = db.Users.Where(u => u.UserId == userId).FirstOrDefault();
                            user.PayPalId = billingAgreementId;
                            db.SaveChanges();
                            return billingAgreementId;
                        }
                        else
                        {
                            Logger.ErrorFormat("error in CreateBillingAgreement");
                            return null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("error in CreateBillingAgreement", e.Message);
                return null;
            }
        }

        public static string DoReferenceTransaction(long orderID, int amount)
        {
            try
            {
                var transactionId = "";
                //todo: take the correct billing and another data from database:
                using (var db = new BallyTaxiEntities().AutoLocal())
                {
                    var order = db.Orders.GetById(orderID);
                    var user = db.Users.GetById(order.PassengerId);
                    var billingAgreementId = user.PayPalId; //"B-750190528S304935D";
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
                    //method 5:
                    //var url = "https://api-3t.sandbox.paypal.com/nvp";
                    var url = "https://api-3t.paypal.com/nvp";
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                    using (WebClient client = new WebClient())
                    {
                        //&USER=rider-driver_api1.gmail.com&PWD=4KHRP5CQ5JHTKBJQ&SIGNATURE=AFcWxV21C7fd0v3bYYYRCpSSRl31AYbTQGkCJNHzMWnhTx4kkTtvPANW&VERSION=124.0&METHOD=DoReferenceTransaction&REFERENCEID=B-7V2186058N105235U&paymentaction =Authorization&paymenttype=InstantOnly&IPADDRESS=127.0.0.1&RISKSESSIONCORRELATIONID =90FA1B3E-03F8-4B5B-82EC-7A7519294B9B&MERCHANTSESSIONID =rbjhmki6n18u3metl0mmh0rf36&AMT=10¤cycode=USD&ITEMAMT=10&DESC =Your Description&CUSTOM=Open for merchant use&INVNUM=Merchant internal order identifier&NOTIFYURL =http://example/IPN/database/store.php
                        var response =
                     client.UploadValues(url, new NameValueCollection()
                     {
                        { "USER", userName },
                        { "PWD", password },
                        {"SIGNATURE", signature },
                        {"VERSION", "124.0"},
                        {"METHOD", "DoReferenceTransaction"},
                        //{"TOKEN", token },
                        { "REFERENCEID", billingAgreementId },
                        { "paymentaction", "Authorization" },
                        {"paymenttype",  "InstantOnly"},
                        {"IPADDRESS", "127.0.0.1" },
                        {"RISKSESSIONCORRELATIONID", "90FA1B3E-03F8-4B5B-82EC-7A7519294B9B" },
                        { "MERCHANTSESSIONID", "rbjhmki6n18u3metl0mmh0rf36"},
                        {"AMT", Convert.ToString(amount) },
                        {"cycode", "ILS" },//USD
                        {"ITEMAMT", Convert.ToString(amount)  },
                        { "DESC", "Werider App travel charge"},
                        { "CUSTOM", "Paypal for Werider App" },
                        { "INVNUM", Convert.ToString(order.CreationDate.Ticks) }, //
                        { "NOTIFYURL", "http://example/IPN/database/store.php"}
                     });
                        string result5 = Encoding.UTF8.GetString(response);
                        if (result5.Contains("Failure"))//the function faild:
                        {
                            Logger.ErrorFormat("Problem in DoReferenceTransaction: {0}", result5);
                            return null;
                        }
                        else//in sucsess
                        {
                            var s = System.Text.RegularExpressions.Regex.Split(result5, "TRANSACTIONID=");
                            transactionId = s[1];
                            transactionId = transactionId.Split('&')[0];
                            var transId = db.Orders
                                .Where(r => r.OrderId == orderID).FirstOrDefault();
                            transId.transactionId = transactionId;
                            db.SaveChanges();
                        }
                    }
                    return transactionId;
                }
            }
            catch (Exception e)
            {
                Logger.ErrorFormat("Problem in DoReferenceTransaction: {0}", e.Message);
                return null;
            }
        }
    }
}
