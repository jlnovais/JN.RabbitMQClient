using System;

namespace JN.RabbitMQClient.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Consumer c = new Consumer
            {
                Host = "localhost",
                Password = "123",
                Username = "teste",
                QueueName = "teste",
                VirtualHost = "Testes",
                Description = "test client",
                PrefetchCount = 1
            };


            c.onStopReceive += StopReceive;
            c.onMessageReceived += MessageReceived;
            c.Start("test consumer....");

            Console.WriteLine("Started...");
            Console.ReadLine();

            c.Stop();

            Console.WriteLine("Stopped...");

            Console.ReadLine();
        }

        private static Constants.MessageProcessInstruction MessageReceived(string message, string sourcequeuename, long firsterrortimestamp, string consumerdescription)
        {
            Console.WriteLine("Message received: " + message);
            return Constants.MessageProcessInstruction.OK;
        }

        private static void StopReceive(string queuename, string lasterrordescription, string consumerdescription)
        {
            Console.WriteLine("Stop receiving on queue " + queuename + "; last error: " + lasterrordescription);
        }
    }
}
