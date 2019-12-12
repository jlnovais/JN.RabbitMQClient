namespace JN.RabbitMQClient.Entities
{
    public class Result<T>
    {
        public int ErrorCode;
        public string ErrorDescription;
        public T ReturnedObject;
        public bool Success;
    }

    public class Result
    {
        public int ErrorCode;
        public string ErrorDescription;
        public bool Success;
    }
}
