using System;

namespace AWS.SES.RateLimiters
{
    /// <summary>
    /// Limiter per second
    /// </summary>
    internal class SESRateLimiter : SESBaseLimiter
    {
        public SESRateLimiter()
            : base(InfoSource.MaxSendRate, TimeSpan.FromSeconds(1))
        { }
    }
}
