using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class OrderFilters
    {
        public static IQueryable<Order> Pending(this IQueryable<Order> orders)
        {
            if (orders == null)
                return null;

            return
                from o in orders
                where o.StatusId == (int)OrderStatus.Pending
                orderby o.OrderId
                select o; 
        }


        public static IQueryable<Order> InStep(this IQueryable<Order> orders, FlowSteps step)
        {
            if (orders == null)
                return null;

            return
                from o in orders
                where o.FlowStep == (int)step
                select o;
        }

        public static IQueryable<Order> NotCancelled(this IQueryable<Order> orders)
        {
            if (orders == null)
                return null;

            return
                from o in orders
                where o.StatusId != (int)OrderStatus.Canceled && o.StatusId != (int)OrderStatus.Dissatisfied && o.StatusId != (int)OrderStatus.DriverDeclined
                orderby o.OrderId
                select o;
        }

        public static IQueryable<Order> ByPassenger(this IQueryable<Order> orders, long userId)
        {
            if (orders == null)
                return null;

            return
                from o in orders
                where o.PassengerId == userId
                select o;
        }

        public static IQueryable<Order> ByDriver(this IQueryable<Order> orders, long userId)
        {
            if (orders == null)
                return null;

            return
                from o in orders
                where o.DriverId.HasValue && o.DriverId.Value == userId
                select o;
        }

        public static IQueryable<Order> GetByStatusId(this IQueryable<Order> orders, long statusId)
        {
            if (orders == null)
                return null;

            return
                (from o in orders
                 where o.StatusId == statusId
                 select o);
        }

        public static Order GetById(this IQueryable<Order> orders, long orderId)
        {
            if (orders == null)
                return null;

            return
                (from o in orders
                 where o.OrderId == orderId
                 select o)
                .SingleOrDefault();
        }
    }
}