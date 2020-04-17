namespace JN.RabbitMQClient.Entities
{
    public class Result<T>
    {
        public int ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public T ReturnedObject { get; set; }
        public bool Success { get; set; }
    }

    public class Result
    {
        public int ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public bool Success { get; set; }
    }
}
