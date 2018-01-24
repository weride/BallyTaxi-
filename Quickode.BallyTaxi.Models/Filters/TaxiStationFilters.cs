using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class TaxiStationFilters
    {
        public static TaxiStation GetById(this IQueryable<TaxiStation> stations, int id)
        {
            if (stations == null)
                return null;

            return stations.Where(x => x.StationId == id).SingleOrDefault();
        }

       
    }
}