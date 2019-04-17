using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RabbitMQ.Client;

[assembly: InternalsVisibleTo("JN.RabbitMQClient.Tests")]
namespace JN.RabbitMQClient
{
    public static class Utils
    {
        //public static long UnixTimeNow()
        //{
        //    var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
        //    return (long) timeSpan.TotalSeconds;
        //}


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

        internal static long GetFirstErrorTimeStampFromMessageArgs(IBasicProperties properties)
        {
            long res = 0;

            if (properties == null) 
                return res;

            if (properties.Headers == null)
                return res;

            if (properties.Headers.ContainsKey(Constants.FirstErrorTimeStampHeaderName))
                res = (long) (properties.Headers[Constants.FirstErrorTimeStampHeaderName]);

            return res;
        }

        public static List<string> GetHostsList(string hosts)
        {
            if (string.IsNullOrEmpty(hosts))
                return new List<string>();

            var hostArr = hosts.Split(';');

            var res = new List<string>(hostArr.Where(x=>!string.IsNullOrWhiteSpace(x)));

            return res;
        }

    }
}

