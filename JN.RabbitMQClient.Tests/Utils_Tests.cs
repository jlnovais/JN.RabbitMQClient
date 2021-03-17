using System;
using JN.RabbitMQClient.Other;
using NUnit.Framework;

namespace JN.RabbitMQClient.Tests
{
    [TestFixture]
    public class Utils_Tests
    {
        [Test]
        public void ToUnixTimestamp_validDate_returnsCorrectTimestamp()
        {

            var date = new DateTime(2014, 05, 01, 12, 0, 0);

            const long expectedTimestamp = 1398945600;

            var actualTimestamp = date.ToUnixTimestamp();

            Assert.AreEqual(expectedTimestamp, actualTimestamp);
        }

        [Test]
        public void ToDateTime_givenTimestamp_returnsCorrectDate()
        {
            var expectedDate = new DateTime(2014, 05, 01, 10, 50, 0);

            const long timestamp = 1398941400;

            var actualDate = expectedDate.ToDateTime(timestamp);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [TestCase("hostA;hostB", 2)]
        [TestCase("hostA", 1)]
        public void GetHostsList_withValues_returnsExpectedHosts(string hosts, short totalExpectedHosts)
        {
            var actual = Utils.GetHostsList(hosts);

            Assert.AreEqual(totalExpectedHosts, actual.Count);
        }

        [TestCase("", 0)]
        [TestCase(";", 0)]
        [TestCase("  ", 0)]
        [TestCase(null, 0)]
        public void GetHostsList_withoutValues_returnsNoHosts(string hosts, short totalExpectedHosts)
        {
            var actual = Utils.GetHostsList(hosts);

            Assert.AreEqual(totalExpectedHosts, actual.Count);
        }
    }
}
