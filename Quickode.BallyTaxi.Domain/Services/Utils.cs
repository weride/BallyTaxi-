using Quickode.BallyTaxi.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Spatial;
using System.Globalization;
using System.Net.Mail;
using System.Reflection;
using System.Resources;
using System.Web;
using System.Xml;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class Utils
    {
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static DbGeography LatLongToLocation(double latitude, double longitude)
        {
            return DbGeography.FromText(string.Format("POINT ({0} {1})", longitude.ToString().Replace(",", "."), latitude.ToString().Replace(",", ".")));
        }

        public static string TranslateMessage(string languageCulture, string messageCode)
        {
            //var rm = new ResourceManager("Quickode.BallyTaxi.Domain.Resource.BallyTaxiText", Assembly.GetExecutingAssembly());
            var culture = CultureInfo.CreateSpecificCulture(languageCulture);

            try
            {
                var translatedPhrase = Resources.Resources.ResourceManager.GetString(messageCode, culture);
                return
                translatedPhrase;
            }
            catch (Exception)
            {
                return string.Format("Not found translation for message code:{0} to lang:{1}", messageCode, languageCulture);
            }
        }

        public static bool SendMail(List<string> sDestination, string sSubject, string sMessage, List<Attachment> lAttach, int languageId=1)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();
                for (int i = 0; i < sDestination.Count; i++)
                {
                    mail.To.Add(new MailAddress(sDestination[i]));
                    Logger.DebugFormat("SendMail to" , sDestination[i]);
                }
                //String strPathAndQuery = HttpContext.Current.Request.Url.PathAndQuery;
                //String strUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                //LinkedResource inline = new LinkedResource("http://www.we-rider.com/pics/logo.png"); //HttpRuntime.AppDomainAppPath + "riderLogo‪.jpg"); //, MediaTypeNames.Image.Jpeg
                //inline.ContentId = Guid.NewGuid().ToString();

                mail.Subject = sSubject;
                mail.IsBodyHtml = true;
                var dir = languageId == (int)UserLanguages.he ? "rtl" : "ltr";
                // sMessage = "<div dir=" + dir + ">" + sMessage + "</div><div><img src=\"" + "..\img\weride-logo.png"+ "\" /></div>";//+string.Format( @"<img src=""cid:{0}"" />", inline.ContentId);
                //sMessage = "<div dir=" + dir + ">" + sMessage + "</div><div><img src='http://www.we-rider.com/pics/logo.png' /></div>";//+string.Format( @"<img src=""cid:{0}"" />", inline.ContentId);
                sMessage = "<div dir=" + dir + ">" + sMessage + "</div><div><img src='http://dev.we-rider.com/Contents/weride-logo.png' height='40px'/></div>";//+string.Format( @"<img src=""cid:{0}"" />", inline.ContentId);                
                mail.Body = sMessage;
                // Log.LogInfo("SendMail", "mail" + mail);
                if (lAttach != null)//כאן מצרפים
                {
                    foreach (var img in lAttach)
                        mail.Attachments.Add(img);
                    //SmtpServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                }

                SmtpServer.Send(mail);
                Logger.Debug("SendMail succeeded");

                return true;

            }

            catch (Exception e)
            {
                //LogWriter.WriteLog(e, "MessageSend");
                //lo.LogInfo("SendMail", "Exception : " + e);
                Logger.Debug("SendMail failed");
                return false;
            }


        }

        public static Dictionary<int, string> PrepareToSendEmail(DriverEmail emailType, int languageId, double amount=0)
        {
            var userCulture = new NotificationsServices().GetLanguageCulture(languageId);
            Dictionary<int, string> dStrings = new Dictionary<int, string>();
            dStrings[0] = TranslateMessage(userCulture, emailType.ToString());
            if (emailType == DriverEmail.RegisterTitle)
            {
                int i = 1;
                while (i <= 3)
                    dStrings[i] = TranslateMessage(userCulture, "Register" + i++);
            }
            else if (emailType == DriverEmail.CanceledTripTitle)
            {
                int i = 1;
                while (i <= 2)
                {
                    dStrings[i] = TranslateMessage(userCulture, "CanceledTrip" + i++);
                }
            }
            else if (emailType == DriverEmail.CompleteTripTitle)
            {
                int i = 1;
                while (i <= 2)
                {
                    if (i == 1)
                        dStrings[i] = string.Format(TranslateMessage(userCulture, "CompleteTrip" + i++), amount);
                    else
                        dStrings[i] = TranslateMessage(userCulture, "CompleteTrip" + i++);
                }
            }
            dStrings[dStrings.Count] = TranslateMessage(userCulture, DriverEmail.endMail.ToString());
            return dStrings;
        }

        internal static Dictionary<int, string> PrepareToSendEmailForPassenger(PassengerEmail emailType, int languageId)
        {
            var userCulture = new NotificationsServices().GetLanguageCulture(languageId);
            Dictionary<int, string> dStrings = new Dictionary<int, string>();
            dStrings[0] = TranslateMessage(userCulture, emailType.ToString());
            if (emailType==PassengerEmail.CanceledTripTitle)
            {
                int i = 1;
                while (i <= 2)
                {
                    dStrings[i] = TranslateMessage(userCulture, "CanceledTripForPass" + i++);
                }
            }
            if (emailType == PassengerEmail.CompleteTripTitle)
            {
                int i = 1;
                while (i <= 2)
                {
                    dStrings[i] = TranslateMessage(userCulture, "CompletedTripPass" + i++);
                }
            }
            dStrings[dStrings.Count] = TranslateMessage(userCulture, DriverEmail.endMail.ToString());
            return dStrings;
        }

        public static Dictionary<int, object> AddressToLocation(string address)
        {
            var lObject = new Dictionary<int, object>();
            string url = "http://maps.google.com/maps/api/geocode/xml?address=" + address + "&sensor=false";

            XmlDocument doc = new XmlDocument();
            doc.Load(url);
            XmlNode element = doc.SelectSingleNode("//GeocodeResponse/status");
            if (element.InnerText == "ZERO_RESULTS")
            {
                return null;
            }
            else
            {
                XmlNodeList xnList = doc.SelectNodes("//GeocodeResponse/result/geometry/location");
                XmlNode xn = xnList[0];
                var lat = Convert.ToDouble(xn["lat"].InnerText);
                var lng = Convert.ToDouble(xn["lng"].InnerText);

                var location = LatLongToLocation(lat, lng);
                lObject[0] = location;

                string longname = null;
                string shortname = null;
                string typename = null;
                string city = null;

                XmlNodeList xnListCity = doc.SelectNodes("//GeocodeResponse/result/address_component");
                foreach (XmlNode xnCity in xnListCity)
                {
                    longname = xnCity["long_name"].InnerText;
                    shortname = xnCity["short_name"].InnerText;
                    typename = xnCity["type"].InnerText;

                    switch (typename)
                    {
                        //Add whatever you are looking for below
                        case "political":
                            {
                                // var Address_country = longname;
                                city = longname;//Address_LongName != "" ? longname : Address_LongName;//
                                break;
                            }
                        case "locality":
                            {
                                // var Address_country = longname;
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
                    if (city != "" && city != null)
                        break;
                }

                lObject[1] = city;
                return lObject;
            }
        }
    }
}