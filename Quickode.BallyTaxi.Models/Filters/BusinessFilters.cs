using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class BusinessFilters
    {
        public static IQueryable<Business> ByCountry(this IQueryable<Business> businesses, string isoCountry)
        {
            if (businesses == null)
                return null;

            return businesses.Where(x => x.IsoCountry == isoCountry);
        }

        public static IQueryable<Business> ByName(this IQueryable<Business> businesses, string name)
        {
            if (businesses == null)
                return null;

            return businesses.Where(x => x.BusinessName == name);
        }
    }
}