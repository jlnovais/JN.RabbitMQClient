using System;
using NUnit.Framework;

namespace JN.RabbitMQClient.Tests
{
    public class Test : RabbitMqToolsBase
    {
        public void SetupConnection()
        {
            base.SetupConnection();
        }
    }

    [TestFixture]
    class RabbitMQToolsBaseTests
    {
        [TestCase("")]
        [TestCase(null)]
        [TestCase(" ")]
        [TestCase(";")]
        [TestCase(";;")]
        [TestCase(" ; ; ")]
        public void Setupconnection_invalidHosts_throwsException(string host)
        {
            var test = new Test {Host = host};

            Assert.Throws<Exception>(() => test.SetupConnection());
        }

    }
}
