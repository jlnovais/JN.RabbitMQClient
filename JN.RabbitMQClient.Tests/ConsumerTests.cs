using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Moq;
using NUnit.Framework;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RabbitMQClient;
using Constants = JN.RabbitMQClient.Constants;


[assembly:  IgnoresAccessChecksTo("JN.RabbitMQClient.Consumer")]
namespace JN.RabbitMQClient.Tests
{


    [TestFixture]
    class ConsumerTests
    {

        private static int _totalMessagesReceived;
        private static int _totalStopProcessing;

        private readonly MessageReceiveDelegate _receivedMethod = (message, sourceQueueName, errorTimestamp, description) =>
        {
            _totalMessagesReceived++;

            switch (message)
            {
                case "error":
                    return Constants.MessageProcessInstruction.RequeueMessageWithDelay;
                case "ignore":
                    return Constants.MessageProcessInstruction.IgnoreMessage;
                default:
                    return Constants.MessageProcessInstruction.OK;
            }
        };

        private readonly StopReceiveDelegate _stopReceiveMethod = (name, description, consumerDescription) =>
        {
            _totalStopProcessing++;
        };

        private static Consumer _consumer;


        private Consumer GetConsumer(IModel model)
        {
            var consumer = new Consumer { _model = model };

            consumer.onMessageReceived += _receivedMethod;
            consumer.onStopReceive += _stopReceiveMethod;

            return consumer;
        }

        private static Mock<IModel> GetModel(string expectedMessage)
        {
            var modelMock = new Mock<IModel>();

            modelMock
                .Setup(
                    x => x.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(),
                        It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>())
                )
                .Callback<string, bool, string, bool, bool, IDictionary<string, object>, IBasicConsumer>(
                    (queue, noAck, consumerTag, noLocal, exclusive, arguments, consumer) =>
                        consumer.HandleBasicDeliver("", 1, false, "exchange", "route", null,
                            Encoding.UTF8.GetBytes(expectedMessage))

                );


            modelMock
                .Setup(
                    x => x.Abort()
                )
                .Callback(
                    () =>
                    {
                        _consumer?.OnStopReceiveHandler("", "", "");
                    }
                );


            IBasicProperties properties = new BasicProperties();
            
            Tools.SetPropertiesConsumer(properties, 1000);

            modelMock
                .Setup(x => x. CreateBasicProperties())
                .Returns(properties);

            return modelMock;
        }



        [SetUp]
        public void Setup()
        {
            _consumer = null;

            _totalMessagesReceived = 0;
            _totalStopProcessing = 0;
        }

        [Test]
        public void Consumer_receivesMessage_executesCallBackStopProcessingOneTime()
        {
            const int expectedTotalCallsToStopProcessing = 1;

            const string expectedMessage = "message";

            var modelFake = GetModel(expectedMessage);
            _consumer = GetConsumer(modelFake.Object);

            _consumer.Start("my Connection description");

            Thread.CurrentThread.Join(1000);
            _consumer.Stop();

            Assert.AreEqual(expectedTotalCallsToStopProcessing, _totalStopProcessing);
        }

        [Test]
        public void Consumer_receivesMessage_executesCallBackMessageReceived()
        {
            const string expectedMessage = "message";

            var modelFake = GetModel(expectedMessage);
            var consumer = GetConsumer(modelFake.Object);

            consumer.Start();

            Thread.CurrentThread.Join(1000);

            consumer.Stop();

            Assert.AreEqual(_totalMessagesReceived, 1);
        }


        [Test]
        public void Consumer_receivesMessageAndErrorOccursWhileProcessing_messageIsRequeuedIfRetryQueueIsDefined()
        {
            const string expectedMessage = "error";

            var modelMock = GetModel(expectedMessage);
            var consumer = GetConsumer(modelMock.Object);

            consumer.RetryQueue = "retryQueue";
            consumer.RetentionPeriodInRetryQueueMilliseconds = 5000;

            consumer.Start();

            Thread.CurrentThread.Join(1000);

            consumer.Stop();

            modelMock.Verify(
                x =>
                    x.BasicPublish( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(),
                        It.IsAny<byte[]>()),
                Times.AtLeast(1));


        }

        [Test]
        public void Consumer_receivesMessageAndErrorOccursWhileProcessing_messageIsIgnored()
        {
            const string expectedMessage = "ignore";

            var modelMock = GetModel(expectedMessage);
            var consumer = GetConsumer(modelMock.Object);

            consumer.Start();

            Thread.CurrentThread.Join(1000);

            consumer.Stop();

            modelMock.Verify(
                x =>
                    x.BasicReject(It.IsAny<ulong>(), false),
                Times.AtLeast(1));
        }

        [Test]
        public void Consumer_receivesMessageAndProcessOK_messageIsAcknowledge()
        {
            const string expectedMessage = "ok";

            var modelMock = GetModel(expectedMessage);
            var consumer = GetConsumer(modelMock.Object);

            consumer.Start();

            Thread.CurrentThread.Join(1000);

            consumer.Stop();

            modelMock.Verify(
                x =>
                    x.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>() ),
                Times.AtLeast(1));
        }


        [Test]
        public void Consumer_StartAgain_ReturnsError()
        {
            const string expectedMessage = "message";

            var modelFake = GetModel(expectedMessage);
            var consumer = GetConsumer(modelFake.Object);

            consumer.Start();

            Assert.Throws<Exception>(() => consumer.Start());
        }


    }
}


