using Extensions.Configuration.Factory;
using Microsoft.Extensions.Configuration;
using System;

namespace AWS.SES.RateLimiters
{
    /// <summary>
    /// Calculates the delay for new requests using the ExponentialBackoff algorithm
    /// </summary>
    internal class ExponentialBackoff
    {
        public static readonly ExponentialBackoff Default = FromConfiguration();

        /// <summary>
        /// Minimum backoff time
        /// </summary>
        public int MinSleepMillis { get; set; }

        /// <summary>
        /// Maximum backoff time
        /// </summary>
        public int MaxSleepMillis { get; set; }

        /// <summary>
        /// Algorithm's base calculation
        /// </summary>
        /// <remarks>
        /// Used as multiplier of attempts quantity, for example:
        /// 
        /// BASE   |    2  |    3  |    4  |
        /// 100ms  |  200ms|  400ms|  800ms|
        /// 1000ms | 2000ms| 4000ms| 8000ms|
        /// </remarks>
        public int BaseSleepMillis { get; set; }

        public ExponentialBackoff(TimeSpan baseSleep, TimeSpan maxSleep)
            : this((int)baseSleep.TotalMilliseconds, (int)maxSleep.TotalMilliseconds)
        { }

        public ExponentialBackoff(int baseSleepMillis, int maxSleepMillis)
        {
            if (baseSleepMillis <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baseSleepMillis));
            }

            if (maxSleepMillis <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSleepMillis));
            }

            if (maxSleepMillis < baseSleepMillis)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSleepMillis), $"{nameof(maxSleepMillis)} cannot be smaller than {nameof(baseSleepMillis)} interval.");
            }

            MinSleepMillis = int.MinValue;

            MaxSleepMillis = maxSleepMillis;
            BaseSleepMillis = baseSleepMillis;
        }

        /// <summary>
        /// Calculates the backoff period based on the number of attempts
        /// </summary>
        /// <param name="attempts"></param>
        /// <returns></returns>
        public TimeSpan Calculate(int attempts)
        {
            var k = 1 << (attempts - 1);
            var newSleep = BaseSleepMillis * k;

            var millis = Math.Min(MaxSleepMillis, Math.Max(MinSleepMillis, newSleep));
            var result = TimeSpan.FromMilliseconds(millis);

            return result;
        }

        /// <summary>
        /// Maximum period of backoff as TimeSpan
        /// </summary>
        internal TimeSpan Max() => TimeSpan.FromMilliseconds(MaxSleepMillis);

        private static ExponentialBackoff FromConfiguration()
        {
            var configuration = ConfigurationFactory.Instance.GetConfiguration();

            var minimumInterval = configuration.GetValue<TimeSpan>("AWS:SES:ExponentialBackoff:MinimumInterval");
            var maximumInterval = configuration.GetValue<TimeSpan>("AWS:SES:ExponentialBackoff:MaximumInterval");
            var baseInterval = configuration.GetValue<TimeSpan>("AWS:SES:ExponentialBackoff:BaseInterval");

            var exponentialBackoff = new ExponentialBackoff(baseInterval, maximumInterval)
            {
                MinSleepMillis = (int)minimumInterval.TotalMilliseconds
            };

            return exponentialBackoff;
        }
    }
}
