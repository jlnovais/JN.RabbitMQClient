# JN.RabbitMQClient
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

In `MessageReceived` the returned `Constants.MessageProcessInstruction` will define how the message will be processed (accept, requeue, ignore).

The `Sender` can be used as in the following example.

```csharp
public class ResponseProcessor
{
	private readonly BrokerConfig _config;

	public ResponseProcessor(BrokerConfig config)
	{
		_config = config;
	}

	public Result ProcessResponse(NotificationResponse response)
	{
		var res = new Result();

		try
		{
			var sender = GetSender(_config);

			var objStr = GetJSONFromObject(response);

			sender.Send(objStr);
			res.Success = true;
		}
		catch (Exception ex)
		{
			res.Success = false;
			res.ErrorCode = -100;
			res.ErrorDescription = ex.Message;
		}

		return res;
	}

	private ISender GetSender(BrokerConfig config)
	{
		var sender = new Sender
		{
			Host = config.Host,
			Username = config.Username,
			Password = config.Password,
			VirtualHost = config.VirtualHost,
			Port = config.Port,
			QueueName = config.RoutingKeyOrQueueName,
			ExchangeName = config.Exchange,
			ShuffleHostList = config.ShuffleHostList
		};

		return sender;
	}

	private string GetJSONFromObject<T>(T objectToSerialize)
	{
		string json = JsonConvert.SerializeObject(objectToSerialize);

		return json;
	}
}

```


