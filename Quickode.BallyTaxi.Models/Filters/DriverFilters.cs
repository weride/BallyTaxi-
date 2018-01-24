using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity.Spatial;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class DriverFilters
    {
        public static Driver GetById(this IQueryable<Driver> drivers, long driverId)
        {
            if (drivers == null)
                return null;

            return
                (from d in drivers
                 where d.UserId == driverId
                 select d)
                .SingleOrDefault();
        }

        public static IQueryable<Driver> LocationWithinTime(this IQueryable<Driver> drivers, int minutes)
        {
            if (drivers == null)
                return null;

            var dt = DateTime.UtcNow.AddMinutes(-minutes);

            return
                from d in drivers
                where d.LastUpdateLocation.HasValue && d.LastUpdateLocation.Value > dt
                select d;
        }

        public static IQueryable<Driver> AvailableToDrive(this IQueryable<Driver> drivers, bool isFutureRide = false)
        {
            if (drivers == null)
                return null;

            return
                from d in drivers
                where d.Status == (int)DriverStatus.Available || d.Status == (int)DriverStatus.HasRequest || d.Status == (int)DriverStatus.HasRequestAsFirst || (isFutureRide == true && d.Status == (int)DriverStatus.NotAvailable)
                select d;
        }

        //public static IQueryable<Driver> isIn99minuteCompAndInInterCity(this IQueryable<Driver> drivers, bool? isInterCity)
        //{
        //    if (drivers == null)
        //        return null;

        //    return
        //        from d in drivers
        //        where (d.TaxiStation.StationId == 129 /*מוניות הדקה ה99*/ && isInterCity == true) || (isInterCity == false)
        //        select d;
        //}

        public static IQueryable<Driver> NotAvailableToDrive(this IQueryable<Driver> drivers)
        {
            if (drivers == null)
                return null;

            return
                from d in drivers
                where d.Status != (int)DriverStatus.Available && d.Status != (int)DriverStatus.HasRequest
                select d;
        }

        public static IQueryable<Driver> ThatAreActive(this IQueryable<Driver> drivers)
        {
            if (drivers == null)
                return null;

            return
                from d in drivers
                where d.User.Active
                select d;
        }

        public static IQueryable<Driver> Near(this IQueryable<Driver> drivers, DbGeography location, int distance)
        {
            if (drivers == null)
                return null;

            return
                from d in drivers
                where d.Location.Distance(location) <= (distance + 10)
                orderby d.Location.Distance(location)
                select d;

        }

        public static IQueryable<Driver> NearNew(this IQueryable<Driver> drivers, DbGeography location, int distance, bool? isFutureRide)
        {
            if (drivers == null)
                return null;

            if (isFutureRide.HasValue && isFutureRide.Value == true)
                return drivers;

            return
                from d in drivers
                where d.Location.Distance(location) <= (distance + 10)
                orderby d.Location.Distance(location)
                select d;

        }
        public static IQueryable<Driver> byPrefferedStationId(this IQueryable<Driver> drivers, int? taxiStationId, bool? isFromStation)
        {
            if (drivers == null)
                return null;

            if (taxiStationId.HasValue && isFromStation == true)
                return
                    from d in drivers
                    where d.TaxiStationId == taxiStationId
                    select d;

            return drivers;
        }

        public static IQueryable<Driver> courier(this IQueryable<Driver> drivers, int? courier)
        {
            if (drivers == null)
                return null;

            if (courier.HasValue && courier.Value > 0)
                return from d in drivers
                       where d.courier >= courier
                       select d;
            return
                drivers;
        }

        public static IQueryable<Driver> isHandicapped(this IQueryable<Driver> drivers, bool? isHandicapped)
        {
            if (drivers == null)
                return null;

            if (isHandicapped == true)
                return from d in drivers
                       where d.isHandicapped == true
                       select d;
            return
                drivers;
        }

        //
        public static IQueryable<Driver> bySeats(this IQueryable<Driver> drivers, Order order)
        {
            if (drivers == null)
                return null;

            if (order.seats > 4)
                return from d in drivers
                       where d.seats >= order.seats
                       select d;
            return
                drivers;
        }

        public static IQueryable<User> filterDriverNotUpdate(this IQueryable<User> users, int numvberOfSending)
        {
            if (users == null)
                return null;
            //??
            if (numvberOfSending < 3)
                return users;
            else
                return from d in users
                       where (d.Driver.UpdateLocationStatus == null || 
                             (d.Driver.UpdateLocationStatus.HasValue &&
                             (d.Driver.UpdateLocationStatus.Value == (int)updateLocationStatus.sent ||
                             d.Driver.UpdateLocationStatus.Value == (int)updateLocationStatus.updateLocation)))
                       select d;
        }

        public static IQueryable<Driver> WantFutureRide(this IQueryable<Driver> drivers)
        {
            if (drivers == null)
                return null;

            return drivers;
            //from d in drivers
            //where d.WantFutureRide == true
            //select d;
        }
        public static IQueryable<Driver> notAlreadyDeclined(this IQueryable<Driver> drivers, long orderId)
        {
            if (drivers == null)
                return null;
            var driversDeclined = new List<long>();
            using (var db = new BallyTaxiEntities().AutoLocal())
            {
                driversDeclined = db.Orders_Drivers.Where(o => o.OrderId == orderId && o.StatusId == (int)Order_DriverStatus.Declined).Select(u => u.DriverId).ToList();
            }
            return
                from d in drivers
                where !driversDeclined.Contains(d.UserId)
                select d;
        }

        public static IQueryable<Driver> inRegion(this IQueryable<Driver> drivers, int regionId)
        {
            if (drivers == null)
                return null;

            return
                from d in drivers
                where d.DriverRegions.FirstOrDefault() != null && d.DriverRegions.Select(s => s.regionId).ToList().Contains(regionId)
                select d;
        }



        public static IQueryable<Driver> BYPaymentMethod(this IQueryable<Driver> drivers, int paymentMethod)
        {
            //if the payment method is CreditCard - only return drivers that accept CC. 
            //Otherwise - return all drivers, since paying by cash or by app doesn't restrict the type of driver needed.
            if (drivers == null)
                return null;

            //if (paymentMethod == (int)CustomerPaymentMethod.CreditCard)
            //    return
            //    from d in drivers
            //    where d.AcceptsCC == true
            //    select d;
            //else
            return drivers;
        }
    }
}