using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Models
{
    public class PayPalPaymentModel
    {
        public string userName { get; set; }
        public string password { get; set; }
        public string signature { get; set; }
    }
}