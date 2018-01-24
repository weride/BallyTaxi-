using Quickode.BallyTaxi.Core;
using Quickode.BallyTaxi.Models.Interfaces;
using Twilio;
using System.Threading.Tasks;
using System;
using Quickode.BallyTaxi.Domain.Services;

namespace Quickode.BallyTaxi.Integrations.Twilio
{
    public class TwilioService : ITextMessageService 
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public bool SendSMS(string phone, string text, bool debug = false)
        {
            if (debug) return true;


            // Find your Account Sid and Auth Token at twilio.com/user/account
            //string AccountSid = "AC14c1cad1c61e8a6d7ba1cc6892d8747c";
            //string AuthToken = "bf66f8914a858024ecb5b238a93f0927";
            try
            {
                //var twilio = new TwilioRestClient(AppSettings.SMSUser(), AppSettings.SMSToken());
                var twilio = new TwilioRestClient(ConfigurationHelper.SMSUser, ConfigurationHelper.SMSToken);
                var SMSFromNumber = ConfigurationHelper.SMSFromNumber;
                var sms =  twilio.SendSmsMessage(SMSFromNumber, phone, text);//"Rider"//"Rider App""Rider App IL""RiderApp"

                //see https://www.twilio.com/help/faq/sms/what-do-the-sms-statuses-mean

                if (sms.Status == "sent" || sms.Status == "queued")
                    return true;
                else
                    return false;

            }
            catch(Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }

        public bool SendMassage(string phone, string text, string[] massageURL, bool debug = false)
        {
            if (debug) return true;


            // Find your Account Sid and Auth Token at twilio.com/user/account
            //string AccountSid = "AC14c1cad1c61e8a6d7ba1cc6892d8747c";
            //string AuthToken = "bf66f8914a858024ecb5b238a93f0927";
            try
            {
                //var twilio = new TwilioRestClient(AppSettings.SMSUser(), AppSettings.SMSToken());
                var twilio = new TwilioRestClient(ConfigurationHelper.SMSUser, ConfigurationHelper.SMSToken);

                var sms = twilio.SendMessage(ConfigurationHelper.SMSFromNumber, phone, text, massageURL);//"Rider"//"Rider App""Rider App IL""RiderApp"

                //see https://www.twilio.com/help/faq/sms/what-do-the-sms-statuses-mean

                if (sms.Status == "sent" || sms.Status == "queued")
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
        }
    }
}