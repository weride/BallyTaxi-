using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quickode.BallyTaxi.Core
{
    public static class Extensions
    {
        public static int ToInt(this string number, int value = 0)
        {
            if (string.IsNullOrEmpty(number))
                return value;

            int numberValue;
            if (int.TryParse(number, out numberValue))
                return numberValue;

            return value;
        }

        public static string ToJson(this object obj)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            return json;
        }

        public static T FromJson<T>(this string jsonString)
        {
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
                return obj;
            }
            catch
            {
                return default(T);
            }
        }

        public static DateTime ConvertFromUnixTimestamp(this double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static DateTime ConvertFromUnixTimestampForMilliSeconds(this double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddMilliseconds(timestamp);
        }
        

        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static bool IsOneOf<T>(this T item, IEnumerable<T> isOneOf)
        {
            if (item == null)
                return false;

            if (isOneOf == null)
                return false;

            foreach (var i in isOneOf)
                if (item.Equals(i))
                    return true;

            return false;

        }

        public static bool IsOneOf<T>(this T item, params T[] isOneOf)
        {
            if (item == null)
                return false;

            if (isOneOf == null)
                return false;

            foreach (var i in isOneOf)
                if (item.Equals(i))
                    return true;

            return false;

        }

        public static bool VeryCloseTo(this DateTime x, DateTime y)
        {
            var offset = (x - y).TotalSeconds;

            return Math.Abs(offset) < 5;
        }
    }
}
