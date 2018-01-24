using Quickode.BallyTaxi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
namespace Quickode.BallyTaxi.Domain.Services
{
    public static class ContentService
    {
        public static List<CountryCode> GetCountryData()
        { 
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.CountryCodes.OrderBy(x=>x.name).ToList();
            } 
        }

        public static byte[] GetDriverEULAHtmlPage(string lang )
        {
            string filename = String.Format("driver_{0}.html", lang);
            //return File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.RelativeSearchPath, filename));
            return File.ReadAllBytes(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/EULA"), filename));
        }
          
        public static byte[] GetPassengerEULAHtmlPage(string lang )
        {
            string filename = String.Format("passenger_{0}.html", lang);
            return File.ReadAllBytes(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/EULA"), filename));
        }

        public static byte[] GetDriverPrivacyHtmlPage(string lang)
        {
            string filename = String.Format("driver_privacy_{0}.html", lang);
            return File.ReadAllBytes(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Privacy"), filename));
        }

        public static byte[] GetPassengerPrivacyHtmlPage(string lang)
        {
            string filename = String.Format("passenger_privacy_{0}.html", lang);
            return File.ReadAllBytes(Path.Combine(System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Privacy"), filename));
        }

        

        public static bool IsSupportedCountry(string isocode) 
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                return db.SupportedCountries.Any(x=>x.ISOcode == isocode);
            }

        }

    }
}