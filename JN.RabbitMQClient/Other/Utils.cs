using System;
using System.Collections.Generic;
using System.Linq;

namespace JN.RabbitMQClient
{
    public static class Utils
    {
        /// <summary>
        /// convert to unixTimeStamp; usage: var timestamp = currentDate.ToUnixTimestamp();
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(this DateTime target)
        {
            var date = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            var unixTimestamp = Convert.ToInt64((target - date).TotalSeconds);

            return unixTimestamp;
        }

        /// <summary>
        /// convert to datetime; usage: var dateTime = DateTime.UtcNow.ToDateTime(timestamp);
        /// </summary>
        /// <param name="target"></param>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this DateTime target, long timestamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);

            return dateTime.AddSeconds(timestamp);
        }

        public static List<string> GetHostsList(string hosts)
        {
            if (string.IsNullOrEmpty(hosts))
                return new List<string>();

            var hostArr = hosts.Split(';');

            var res = hostArr.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

            return res;
        }
    }
}
