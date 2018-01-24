using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quickode.BallyTaxi.Models;
using Quickode.BallyTaxi.Models.Filters;
using Quickode.BallyTaxi.Models.Models;
using System.Collections.Generic;
using System.Net.Mail;
using System;
using System.Web;
using System.Web.Security;
using System.Web.Script.Serialization;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class ReportsService
    {

        public static BusinessModel IsValid(string _username, string _password)
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var account = db.Accounts.Where(a => a.Email == _username && a.Password == _password && a.Status == true).FirstOrDefault();
                if (account != null)
                {
                    var business = db.Businesses.Where(b => b.BusinessId == account.BusinessId).FirstOrDefault();
                    return new BusinessModel() { BusinessId = business.BusinessId, BusinessName = business.BusinessName, isNeedFile = business.isNeedFile==null?false:business.isNeedFile.Value, PayPalAccount = business.PayPalAccountId, Phone = business.Phone };
                }
                else return null;
            }
        }



        public static bool SendMail(List<string> sDestination, string sSubject, string sMessage, List<Attachment> lAttach)
        {
            try
            {

                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient();
                for (int i = 0; i < sDestination.Count(); i++)
                {
                    mail.To.Add(new MailAddress(sDestination[i]));
                }
                mail.Subject = sSubject;
                mail.IsBodyHtml = true;
                sMessage = "<div dir='rtl' >" + sMessage + "</div>";
                mail.Body = sMessage;
                // Log.LogInfo("SendMail", "mail" + mail);
                if (lAttach != null)//כאן מצרפים
                {
                    foreach (var img in lAttach)
                        mail.Attachments.Add(img);
                    //SmtpServer.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                }

                SmtpServer.Send(mail);
                return true;

            }

            catch (Exception e)
            {
                //LogWriter.WriteLog(e, "MessageSend");
                //lo.LogInfo("SendMail", "Exception : " + e);
                return false;
            }


        }

        public static BusinessModel GetBusinessDetails(HttpRequestBase request)
        {
            try
            {
                if (request.IsAuthenticated)
                {
                    HttpCookie authCookie = request.Cookies[FormsAuthentication.FormsCookieName];
                    var business = new BusinessModel();
                    if (authCookie != null)
                    {
                        FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                        var serializer = new JavaScriptSerializer();
                        business = (BusinessModel)serializer.Deserialize(authTicket.UserData, typeof(BusinessModel));
                        return business;
                    }
                }
                return null;
            }
            catch (Exception e)
            {

                return null;
            }
        }
    }

}
