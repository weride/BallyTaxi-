using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Models
{
    public class BusinessModel
    {
        public int BusinessId { get; set; }
        public string BusinessName { get; set; }
        public string PayPalAccount { get; set; }
        public string Phone { get; set; }
        public bool isNeedFile { get; set; }

    }

    public class BusinessPersonModel
    {
        public string IsoCountry { get; set; }
        public string BusinessName { get; set; }
        public string Phone { get; set; }
    }
}