using System;

namespace JN.RabbitMQClient.Limiter
{
    public class WindowLimiter : IWindowLimiter
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public int MaxAllowed { get; }
        public int WindowCount { get; private set; }
        public int WindowSeconds { get; }

        private readonly object _lockObj = new object();

        public WindowLimiter(int maxAllowed, int windowSeconds, 
            Constants.MessageProcessInstruction allowedProcessInstruction, 
            Constants.MessageProcessInstruction deniedProcessInstruction)
        {
            if (maxAllowed <= 0 || windowSeconds <= 0)
                throw new ArgumentException("Invalid parameters; cannot be 0");
 
            MaxAllowed = maxAllowed;
            WindowSeconds = windowSeconds;
            AllowedProcessInstruction = allowedProcessInstruction;
            DeniedProcessInstruction = deniedProcessInstruction;

            Start = DateTime.Now;
            End = Start.AddSeconds(windowSeconds);
        }

        public bool IsAllowed()
        {
            return IsAllowed(DateTime.Now);
        }

        public Constants.MessageProcessInstruction AllowedProcessInstruction { get; }
        public Constants.MessageProcessInstruction DeniedProcessInstruction { get; }

        protected bool IsAllowed(DateTime processingTime)
        {
            lock (_lockObj)
            {
                if (processingTime < Start)
                    return false;

                if (processingTime > End)
                {
                    Start = processingTime;
                    End = Start.AddSeconds(WindowSeconds);
                    WindowCount = 0;
                }

                var result = WindowCount < MaxAllowed;

                if (result)
                    WindowCount++;

                return result;
            }

        }


    }
}
