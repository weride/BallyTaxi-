using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class UsersFilters
    {
        public static User GetById(this IQueryable<User> users, long userId)
        {
            if (users == null)
                return null;

            return
                (from u in users
                 where u.UserId == userId
                 select u)
                 .SingleOrDefault();
        }

        public static User GetByPhone(this IQueryable<User> users, string phone)
        {
            if (users == null)
                return null;

            return
                (from u in users
                 where u.Phone == phone
                 select u)
                 .SingleOrDefault();
        }

        public static IQueryable<User> ByDriverNotificationToken(this IQueryable<User> users, string notificationToken)
        {
            if (users == null)
                return null;

            return
                from u in users
                where u.DriverNotificationToken == notificationToken
                select u;
        }

        public static IQueryable<User> ByPassengerNotificationToken(this IQueryable<User> users, string notificationToken)
        {
            if (users == null)
                return null;

            return
                from u in users
                where u.PassengerNotificationToken == notificationToken
                select u;
        }
    }
}