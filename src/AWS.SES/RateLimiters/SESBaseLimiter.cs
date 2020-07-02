using System;

namespace AWS.SES.RateLimiters
{
    /// <summary>
    /// Provides facilities for ratelimit and delay of sending messages
    /// </summary>
    internal abstract class SESBaseLimiter : RateGate
    {
        protected static SESInfo InfoSource { get; }
        public static SESBaseLimiter RateLimiter { get; }
        public static SESBaseLimiter QuotaLimiter { get; }
        public static ExponentialBackoff BackoffDelay { get; }

        static SESBaseLimiter()
        {
            InfoSource = new SESInfo();
            RateLimiter = new SESRateLimiter();
            QuotaLimiter = new SESQuotaLimiter();

            BackoffDelay = ExponentialBackoff.Default;
        }

        public SESBaseLimiter(double quantity, TimeSpan time)
            : base(Convert.ToInt32(Math.Round(quantity, 0, MidpointRounding.AwayFromZero)), time)
        { }
    }
}
