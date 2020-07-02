using System;

namespace AWS.SES.RateLimiters
{
    /// <summary>
    /// Limiter per 24 hours
    /// </summary>
    internal class SESQuotaLimiter : SESBaseLimiter
    {
        public SESQuotaLimiter()
            : base(InfoSource.Max24HourSend, TimeSpan.FromHours(24))
        { }
    }
}
