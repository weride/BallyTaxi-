using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Models
{
    public class BankAccountModel
    {
        public int bankNumber { get; set; }
        public int bankBranch { get; set; }
        public string bankAccountNumber { get; set; }
        public string bankHolderName { get; set; }
    }

    public class BankToDisplay
    {
        public int bankId { get; set; }
        public string bankName { get; set; }
    }
}