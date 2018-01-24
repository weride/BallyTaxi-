using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class OrderDriverFilters
    {
        public static IQueryable<Orders_Drivers> ByDriver(this IQueryable<Orders_Drivers> orderDrivers, long userId)
        {
            if (orderDrivers == null)
                return null;

            return
                from od in orderDrivers
                where od.DriverId == userId
                select od;
        }

        public static IQueryable<Orders_Drivers> ByOrder(this IQueryable<Orders_Drivers> orderDrivers, long orderId)
        {
            if (orderDrivers == null)
                return null;

            return
                from od in orderDrivers
                where od.OrderId == orderId
                select od;
        }

        public static IQueryable<Orders_Drivers> ByPriority(this IQueryable<Orders_Drivers> orderDrivers, long Priority)
        {
            if (orderDrivers == null)
                return null;

            return
                from od in orderDrivers
                where od.Priority == Priority
                select od;
        }

        public static IQueryable<Orders_Drivers> Accepted(this IQueryable<Orders_Drivers> orderDrivers, long orderId)
        {
            if (orderDrivers == null)
                return null;

            return
                from od in orderDrivers
                where od.OrderId == orderId
                where od.StatusId == (int)Order_DriverStatus.Accepted
                select od;
        }
    }
}