using System;
using System.Configuration;
using Quickode.BallyTaxi.Core;
using System.Collections.Generic;
using Quickode.BallyTaxi.Models;
using System.IO;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class ConfigurationHelper
    {
        static Dictionary<string, object> _configValues = new Dictionary<string, object>();
        static ConfigurationHelper()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var config = db.SystemSettings;
                foreach (SystemSetting x in config)
                    _configValues.Add(x.ParamKey, x.ParamValue);
            }

        }

        public static void ConfigurationHelperReload()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var config = db.SystemSettings;
                foreach (SystemSetting x in config)
                {
                    if (_configValues.ContainsKey(x.ParamKey))
                    {
                        _configValues.Remove(x.ParamKey);
                    }
                    _configValues.Add(x.ParamKey, x.ParamValue);
                }
            }
        }

        public static string getValue(string key, string value = "")
        {
            string Value = value;
            if (_configValues.ContainsKey(key))
                Value = _configValues[key].ToString();
            return Value;
        }
        public static int getIntValue(string key, int value = 0)
        {
            int Value = value;
            if (_configValues.ContainsKey(key))
                Value = Convert.ToInt32(_configValues[key]);                
            return Value;
        }

        public static int CODE_EXPIRATION_TIME 
        {
            get
            {
                //return ConfigurationManager.AppSettings["code_expiration_time"].ToInt(30);
                return getIntValue("code_expiration_time");
            }
        }

        public static int FREE_TRIAL_DAYS
        {
            get
            {
                //return ConfigurationManager.AppSettings["free_trial_days"].ToInt();
                return getIntValue("free_trial_days");
            }
        }

        public static string LINK_FOR_ADVERTISING
        {
            get
            {
                //return ConfigurationManager.AppSettings["free_trial_days"].ToInt();
                return getValue("linkForAdvertising");
            }
        }

        public static string LINK_FOR_ADVERTISING_PASSENGER
        {
            get
            {
                //return ConfigurationManager.AppSettings["free_trial_days"].ToInt();
                return getValue("linkForAdvertisingPassenger");
            }
        }
        //

        public static int sendnotifToUpdateForDriverAndroid
        {
            get
            {
                //return ConfigurationManager.AppSettings["free_trial_days"].ToInt();
                return getIntValue("sendnotifToUpdateForDriverAndroid");
            }
        }

        public static int sendnotifToUpdateForDriverIphone
        {
            get
            {
                //return ConfigurationManager.AppSettings["free_trial_days"].ToInt();
                return getIntValue("sendnotifToUpdateForDriverIphone");
            }
        }

        public static int sendnotifToUpdateForPassengerAndroid
        {
            get
            {
                //return ConfigurationManager.AppSettings["free_trial_days"].ToInt();
                return getIntValue("sendnotifToUpdateForPassengerAndroid");
            }
        }
        public static int sendnotifToUpdateForPassengerIphone
        {
            get
            {
                //return ConfigurationManager.AppSettings["free_trial_days"].ToInt();
                return getIntValue("sendnotifToUpdateForPassengerIphone");
            }
        }


        public static int AVAILABLE_DISTANCE
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_distance"].ToInt();
                return getIntValue("available_distance");
            }
        }
         
        public static int REMINDER_SECONDS
        {
            get
            {
                //return ConfigurationManager.AppSettings["reminder_seconds"].ToInt();
                return getIntValue("reminder_seconds");
            }
        }

        public static int AVAILABLE_RADIUS
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("available_radius");
            }
        }
        public static int AVAILABLE_RADIUS_500
        {
            get
            {
                return getIntValue("available_radius_500");
            }
        }
        public static int AVAILABLE_RADIUS_MAX
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("available_radius_Max");
            }
        }
        
        public static int AVAILABLE_RADIUS_JERUSALEM
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForJerusalem");
            }
        }

        public static int AVAILABLE_RADIUS_BNEI_BRAK
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForBneiBrak");
            }
        }

        public static int AVAILABLE_RADIUS_RAMAT_GAN
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForRamatGan");
            }
        }

        public static int AVAILABLE_RADIUS_TelAviv
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForTelAviv");
            }
        }

        public static int AVAILABLE_RADIUS_Haifa
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForHaifa");
            }
        }

        public static int AVAILABLE_RADIUS_Ashdod
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForAshdod");
            }
        }

        public static int AVAILABLE_RADIUS_FORIntercityRide
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForIntercityRide");
            }
        }

        public static int AVAILABLE_RADIUS_FORCourier
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForCourier");
            }
        }

        public static int AVAILABLE_RADIUS_FORHandicapped
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("availableRadiusForHandicapped ");
            }
        }

        public static int UPDATE_MINUTES
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_radius"].ToInt();
                return getIntValue("update_minutes");
            }
        }

        public static int MaxDriversOfferedSingleDrive
        {
            get
            {
                //return ConfigurationManager.AppSettings["max_drivers_offered_single_driver"].ToInt();
                return getIntValue("max_drivers_offered_single_driver");
            }
        }
        public static int MaxSecondsFirstDriverOfferedOrder
        {
            get
            {
                //return ConfigurationManager.AppSettings["max_seconds_first_driver_offered_order"].ToInt();
                return getIntValue("max_seconds_first_driver_offered_order");
            }
        }

        public static int MAX_ORDER_WAIT_SECONDS
        {
            get
            {
                //return ConfigurationManager.AppSettings["max_order_wait_seconds"].ToInt();
                return getIntValue("max_order_wait_seconds");
            }
        }

        public static int MAX_ORDER_WAIT_SECONDS_IVR
        {
            get
            {
                //return ConfigurationManager.AppSettings["max_order_wait_seconds"].ToInt();
                return getIntValue("max_order_wait_seconds_IVR");
            }
        }

        //

        public static string AVAILABLE_HOURS_RANGE
        {
            get
            {
                //return ConfigurationManager.AppSettings["available_hours_range"];
                return getValue("available_hours_range");
            }
        }

        public static string NOTIFICATIONS_ANDROID_SENDERID_PASSENGER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.Android.SenderID.Passenger"];
                return getValue("Notifications.Android.SenderID.Passenger");
            }
        }

        public static string NOTIFICATIONS_ANDROID_APIKEY_PASSENGER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.Android.ApiKey.Passenger"];
                return getValue("Notifications.Android.ApiKey.Passenger");
            }
        }

        public static string NOTIFICATIONS_ANDROID_PACKAGE_PASSENGER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.Android.Package.Passenger"];
                return getValue("Notifications.Android.Package.Passenger");
            }
        }

        public static string NOTIFICATIONS_IOS_P12_PASSENGER
        {
            get
            {
                return Path.Combine(Notifications_IOS_Root_Path, getValue("Notifications.IOS.P12.Passenger"));
            }
        }

        public static string NOTIFICATIONS_IOS_P12SANDBOX_PASSENGER
        {
            get
            {
                return Path.Combine(Notifications_IOS_Root_Path, getValue("Notifications.IOS.P12sandbox.Passenger"));
            }
        }

        public static string NOTIFICATIONS_IOS_P12SANDBOX_PASS_PASSENGER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.IOS.P12sandboxPass.Passenger"];
                return getValue("Notifications.IOS.P12sandboxPass.Passenger");
            }
        }

        public static string NOTIFICATIONS_IOS_P12_PASS_PASSENGER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.IOS.P12Pass.Passenger"];
                return getValue("Notifications.IOS.P12Pass.Passenger");
            }
        }

        public static string NOTIFICATIONS_ANDROID_SENDERID_DRIVER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.Android.SenderID.Driver"];
                return getValue("Notifications.Android.SenderID.Driver");
            }
        }

        public static string NOTIFICATIONS_ANDROID_APIKEY_DRIVER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.Android.ApiKey.Driver"];
                return getValue("Notifications.Android.ApiKey.Driver");
            }
        }

        public static string NOTIFICATIONS_ANDROID_PACKAGE_DRIVER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.Android.Package.Driver"];
                return getValue("Notifications.Android.Package.Driver");
            }
        }

        public static string NOTIFICATIONS_IOS_P12SANDBOX_DRIVER
        {
            get
            {
                return Path.Combine(Notifications_IOS_Root_Path, getValue("Notifications.IOS.P12sandbox.Driver"));
            }
        }

        public static string NOTIFICATIONS_IOS_P12SANDBOX_PASS_DRIVER
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.IOS.P12sandboxPass.Driver"];
                return getValue("Notifications.IOS.P12sandboxPass.Driver");
            }
        }

        public static string NOTIFICATIONS_IOS_P12_DRIVER
        {
            get
            {
                return Path.Combine(Notifications_IOS_Root_Path, getValue("Notifications.IOS.P12.Driver"));
            }
        }

        public static string NOTIFICATIONS_IOS_P12_PASS_DRIVER
        {
            get
            {
                return getValue("Notifications.IOS.P12Pass.Driver");
            }
        }

        private static string Notifications_IOS_Root_Path
        {
            get
            {
                //return ConfigurationManager.AppSettings["Notifications.IOS.P12Pass.Driver"];
                return getValue("Notifications.IOS.Root.Path");
            }
        }

        public static bool NOTIFICATIONS_IOS_SANDBOX
        {
            get
            {
                bool result = false;
                Boolean.TryParse(getValue("Notifications.IOS.Sandbox"), out result);
                return result;
            }
        }

        public static string SMSUser
        {
            //return ConfigurationManager.AppSettings["Twillo.AccountSID"] ?? "";
            get
            {
                return getValue("Twillo.AccountSID");
            }
        }

        public static string SMSToken
        {
            //return ConfigurationManager.AppSettings["Twillo.AuthToken"] ?? "";
            get
            {
                return getValue("Twillo.AuthToken");
            }
        }

        public static string SMSFromNumber
        {
            get
            {
                return getValue("Twillo.FromNumber");
            }
        }

        public static string MediaFolderPath
        {
            //return ConfigurationManager.AppSettings["Twillo.FromNumber"] ?? "";
            get
            {
                return getValue("imageURLPath"); //"https://s3-eu-west-1.amazonaws.com/riderimages/";*******fix!!
            }
        }

        public static string WerideIcomSource
        {
            get
            {
                return getValue("WerideIcomSource");
            }
        }

        public static string MediaFromFolderPath
        {
            //return ConfigurationManager.AppSettings["Twillo.FromNumber"] ?? "";
            get
            {
                return getValue("Media.FolderPath"); //"https://s3-eu-west-1.amazonaws.com/riderimages/";//for upload this is a folder - from here to upload
            }
        }

        public static string BucketName
        {
            //return ConfigurationManager.AppSettings["Twillo.FromNumber"] ?? "";
            get
            {
                return getValue("BucketName"); //"https://s3-eu-west-1.amazonaws.com/riderimages/";//folder in s3
            }
        }
        //

        public static string MediaToFolderPath
        {
            //return ConfigurationManager.AppSettings["Twillo.FromNumber"] ?? "";
            get
            {
                return getValue("Media.ToFolderPath"); //"https://s3-eu-west-1.amazonaws.com/riderimages/";//for upload this is a folder - to here to upload
            }
        }

        public static string StorageAccount
        {
            //return ConfigurationManager.AppSettings["Twillo.FromNumber"] ?? "";
            get
            {
                return getValue("Storage.Account");
            }
        }

        public static string StorageKey
        {
            //return ConfigurationManager.AppSettings["Twillo.FromNumber"] ?? "";
            get
            {
                return getValue("Storage.Key");
            }
        }

        public static string StorageUrl
        {
            //return ConfigurationManager.AppSettings["Twillo.FromNumber"] ?? "";
            get
            {
                return getValue("Storage.Url");
            }
        }

        public static string MapsAPIKey
        {
            get
            {
                return getValue("Maps_API_Key");
            }
        }
        public static string NewMapsAPIKey
        {
            get
            {
                return getValue("NewMaps_API_Key");
            }
        }
        public static string GoogleAPIKey
        {
            get
            {
                return getValue("Google_API_Key");
            }
        }


        public static string TranzilaPW
        {
            get
            {
                return getValue("TranzilaPW");
            }
        }

        public static string TranzilaSupplier
        {
            get
            {
                return getValue("TranzilaSupplier");
            }
        }
    }
}
