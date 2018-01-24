using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Models
{
    public class LocationForIntercityTravel
    {
        public double destinationLatitude { get; set; }
        public double destinationLongitude { get; set; }
        public string destinationCityName { get; set; }
        public double lat { get; set; }
        public double lon { get; set; }
        public string pickUpCityName { get; set; }
        public double time { get; set; }
    }
}