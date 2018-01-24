using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Spatial;
using Quickode.BallyTaxi.Models.Models;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class CreditCardFilters
    {

        public static IQueryable<CreditCard> ByUserId(this IQueryable<CreditCard> CreditCards, long userId)
        {
            if (CreditCards == null)
                return null;

            return
                from cc in CreditCards
                where cc.UserId == userId
                select cc;
        }

        public static CreditCard ByCardId(this IQueryable<CreditCard> CreditCards, long cardId)
        {
            if (CreditCards == null)
                return null;

            return
                (from cc in CreditCards
                where cc.CardId == cardId
                select cc).SingleOrDefault();
        }

        public static CreditCard GetDefault(this IQueryable<CreditCard> cards, long userId)
        {
            if (cards == null)
                return null;

            return
                (from cc in cards
                 where (cc.UserId == userId && cc.IsDefaultCard == true)
                 select cc).SingleOrDefault();
        }

        public static List<CreditCard> GetAreValid(this IQueryable<CreditCard> cards)
        {
            if (cards == null)
                return null;

            var ValidCards = new List<CreditCard>();
            foreach(CreditCard cc in cards)
            {
                DateTime expDate = new DateTime(2000 + cc.ExpYear, cc.ExpMonth,1).AddMonths(1).AddDays(-1);
                if (expDate > DateTime.UtcNow)
                    ValidCards.Add(cc);
            }
            return ValidCards;
        }

        public static CreditCard GetAreValid(this CreditCard card)
        {
            if (card == null)
                return null;
            
            DateTime expDate = new DateTime(2000 + card.ExpYear, card.ExpMonth, 1).AddMonths(1).AddDays(-1);
            if (expDate > DateTime.UtcNow)
                return card;
            else return null;                        
        }


    }
}