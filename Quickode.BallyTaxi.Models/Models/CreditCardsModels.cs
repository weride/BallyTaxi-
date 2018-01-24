using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Models
{
    public class BasicCreditCardModels
    {
        public int CardId { get; set; }
        public long UserId { get; set; }
        public string LastFourDigits { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public bool IsDefaultCard { get; set; }
        public BasicCreditCardModels (CreditCard creditCard)
        {
            CardId = creditCard.CardId;
            UserId = creditCard.UserId;
            LastFourDigits = creditCard.LastFourDigits;
            ExpMonth = creditCard.ExpMonth;
            ExpYear = creditCard.ExpYear;
            IsDefaultCard = creditCard.IsDefaultCard;
        }

    }

    public class CreditCardModel
    {
        public string CreditCardNumber { get; set; }
        public string CVV { get; set; }
        public string expMonth { get; set; }
        public string expYear { get; set; }
    }

    public class AmountForCCModel
    {
        public double amount { get; set; }
        public int currency { get; set; }
    }

    public class CreditCardDetails
    {
        public int creditCardId { get; set; }
        public string lastNumbersCC { get; set; }
    }
}