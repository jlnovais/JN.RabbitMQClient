# JN.Authentication
Simple implementation of RabbitMQ consumer and sender.

## Install
Download the package from NuGet:

`Install-Package JN.RabbitMQClient`

## Usage
First, you must create the consumer and then define delegates for `onStopReceive` and `onMessageReceived`. The consumer will start running when `Start` is called and until `Stop` is called.

```csharp
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


```





