using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class FavoriteDriverFilters
    {
        public static IQueryable<FavoriteDriver> ByDriver(this IQueryable<FavoriteDriver> favoriteDriver, long driverId)
        {
            if (favoriteDriver == null)
                return null;

            return
                from fd in favoriteDriver
                where fd.DriverId == driverId
                select fd;
        }

        public static IQueryable<FavoriteDriver> ByPassenger(this IQueryable<FavoriteDriver> favoriteDriver, long passengerId)
        {
            if (favoriteDriver == null)
                return null;

            return
                from fd in favoriteDriver
                where fd.PassengerId == passengerId
                select fd;
        }
    }
}