using System;
using System.Collections.Generic;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RabbitMQClient;
//using RabbitMQ.Client.Framing.v0_9_1;

//using Constants = RabbitMQ.Client.Framing.Constants;

namespace JN.RabbitMQClient.Tests
{
    [TestFixture]
    public class UtilsTests
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
        public void ToDateTime_givenTimestamp_retursCorrectDate()
        {
            var expectedDate = new DateTime(2014, 05, 01, 10, 50, 0);

            const long timestamp = 1398941400;

            var actualDate = expectedDate.ToDateTime(timestamp);

            Assert.AreEqual(expectedDate, actualDate);
        }

        [Test]
        public void GetTimestampFromMessageArgs_nullArgs_returnsZero()
        {
            //IBasicProperties properties = null;

            const long expectedTimestamp = 0;

            var actualTimestamp = Utils.GetFirstErrorTimeStampFromMessageArgs(null);

            Assert.AreEqual(expectedTimestamp, actualTimestamp);
        }

        [Test]
        public void GetTimestampFromMessageArgs_notNullArgsWithoutHeaderDefined_returnsZero()
        {
            IBasicProperties properties = new BasicProperties();

            const long expectedTimestamp = 0;

            var actualTimestamp = Utils.GetFirstErrorTimeStampFromMessageArgs(properties);

            Assert.AreEqual(expectedTimestamp, actualTimestamp);
        }

        [Test]
        public void GetTimestampFromMessageArgs_notNullArgsWithHeaderDefined_returnsExpectedValue()
        {
            const long expectedTimestamp = 123456789;

            IBasicProperties properties = new BasicProperties();
            properties.Headers = new Dictionary<string, object>();
            properties.Headers.Add(Constants.FirstErrorTimeStampHeaderName, expectedTimestamp);

            var actualTimestamp = Utils.GetFirstErrorTimeStampFromMessageArgs(properties);

            Assert.AreEqual(expectedTimestamp, actualTimestamp);
        }

        [Test]
        public void GetHostsList_validHostString_returnsListOfHosts()
        {
            const string hostStr = "192.168.1.1;192.168.1.2;192.168.1.3";

            var actual = Utils.GetHostsList(hostStr);

            Assert.IsNotNull(actual);
            Assert.That(actual.Count == 3);
        }

        [Test]
        public void GetHostsList_nullHostString_returnsEmptyList()
        {
            const string hostStr = null;

            var actual = Utils.GetHostsList(hostStr);

            Assert.IsNotNull(actual);
            Assert.That(actual.Count == 0);
        }

        [Test]
        public void GetHostsList_EmptyHostString_returnsEmptyList()
        {
            const string hostStr = "";

            var actual = Utils.GetHostsList(hostStr);

            Assert.IsNotNull(actual);
            Assert.That(actual.Count == 0);
        }

        [Test]
        public void GetHostsList_HostStringWithOneHost_returnsEmptyList()
        {
            const string hostStr = "192.168.1.1";

            var actual = Utils.GetHostsList(hostStr);

            Assert.IsNotNull(actual);
            Assert.That(actual.Count == 1);
        }

        [TestCase(";;")]
        [TestCase(";")]
        [TestCase(" ; ; ")]
        public void GetHostsList_NonEmptyHostStringWithEmptyHosts_returnsEmptyList(string hostStr)
        {
            var actual = Utils.GetHostsList(hostStr);

            Assert.IsNotNull(actual);
            Assert.That(actual.Count == 0);
        }
    }
}
