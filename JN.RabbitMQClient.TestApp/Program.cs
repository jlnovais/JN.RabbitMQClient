using System;

namespace JN.RabbitMQClient.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {

            string queueName = "testeQueue";
            string virtualHost = "Testes";//"Testes";

            Consumer c = new Consumer
            {
                Host = "localhost",
                Password = "123",
                Username = "teste",
                QueueName = queueName,
                VirtualHost = virtualHost,
                Description = "test client",
                PrefetchCount = 1
            };


            //c.Host = "localhost";
            //c.Password = "123";
            //c.Username = "teste";
            //c.QueueName = "teste";
            //c.VirtualHost = "Testes";
            //c.Description = "test client";

            Sender sender = new Sender()
            {
                QueueName = queueName,
                Host = "localhost",
                Username = "teste",
                Password = "123",
                VirtualHost = virtualHost
            };

            sender.Send("test message... " + DateTime.Now, true);


            c.onStopReceive += StopReceive;
            c.onMessageReceived += MessageReceived;
            c.Start("teste consumer....");

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
