using Quickode.BallyTaxi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quickode.BallyTaxi.Domain.Services
{
    public static class ConfigurationHelperForStations
    {
        static Dictionary<int, string> _configValues = new Dictionary<int, string>();
        static ConfigurationHelperForStations()
        {
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                var config = db.TaxiStations;
                foreach (TaxiStation x in config)
                    _configValues.Add(x.StationId, x.HebrewName);
            }

        }

        public static int getStationId(string value)
        {
            int id = 0;
            if (_configValues.ContainsValue(value))
                id = _configValues.Where(o => o.Value == value).FirstOrDefault().Key;
            return id;
        }

        public static AccountsForAdmin AccountsForAdmin { get; set; }

        //public static int KasstleTaxiEnglish
        //{
        //    get
        //    {
        //        //return ConfigurationManager.AppSettings["code_expiration_time"].ToInt(30);
        //        return getStationId("Hakastel");
        //    }
        //}

        public static int KasstleTaxi
        {
            get
            {
                //return ConfigurationManager.AppSettings["code_expiration_time"].ToInt(30);
                return getStationId("קסטל");
            }
        }
        //public static int ShekemTaxiEnglish
        //{
        //    get
        //    {
        //        //return ConfigurationManager.AppSettings["code_expiration_time"].ToInt(30);
        //        return getStationId("Hashekem");
        //    }
        //}

        public static int ShekemTaxi
        {
            get
            {
                //return ConfigurationManager.AppSettings["code_expiration_time"].ToInt(30);
                return getStationId("שקם");
            }
        }
    }
}

