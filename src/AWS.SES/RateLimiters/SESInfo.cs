using Amazon.Runtime;
using Amazon.SimpleEmail;
using AWS.SES.Factories;
using AWS.SES.Helpers;
using Extensions.Configuration.Factory;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;

namespace AWS.SES.RateLimiters
{
    /// <summary>
    /// Search the quota information of amazon SES
    /// </summary>
    internal class SESInfo : IDisposable
    {
        private readonly object _syncLock = new object();

        private readonly IConfiguration _configuration;

        /// <summary>
        /// Period of data validity
        /// </summary>
        protected TimeSpan UpdateInterval => _configuration.GetValue<TimeSpan>("AWS:SES:RateLimit:QuotasUpdateInterval");

        protected bool ExternalClient { get; set; }
        protected DateTime LastUpdated { get; set; }
        protected IAmazonSimpleEmailService Client { get; set; }

        private double _max24HourSend;
        /// <summary>
        /// Maximum messages quantity sent per hour in 24 hours
        /// </summary>
        public double Max24HourSend
        {
            get
            {
                Update();
                return _max24HourSend;
            }
            protected set { _max24HourSend = value; }
        }

        private double _maxSendRate;
        /// <summary>
        /// Maximum messages quantity sent per second
        /// </summary>
        public double MaxSendRate
        {
            get
            {
                Update();
                return _maxSendRate;
            }
            protected set { _maxSendRate = value; }
        }

        private double _sent;
        /// <summary>
        /// Messages quantity sent in 24 hours
        /// </summary>
        public double Sent
        {
            get
            {
                Update();
                return _sent;
            }
            protected set { _sent = value; }
        }


        /// <summary>
        /// Remaining messages in 24 hours
        /// </summary>
        public double RemainingTotal
        {
            get
            {
                Update();
                return Math.Max(0, Max24HourSend - Sent);
            }
        }

        public SESInfo()
            : this(SESClientFactory.CreateClient(), ConfigurationFactory.Instance.GetConfiguration())
        {
            ExternalClient = false;
        }

        public SESInfo(IAmazonSimpleEmailService client, IConfiguration configuration)
        {
            ExternalClient = true;
            Client = client;

            _configuration = configuration;

            Update();
        }

        /// <summary>
        /// Updates the quota values
        /// </summary>
        /// <returns></returns>
        public SESInfo Update()
        {
            if ((DateTime.Now - LastUpdated) < UpdateInterval)
            {
                return this;
            }

            lock (_syncLock)
            {
                var response = AsyncHelper.RunSync(() => Client.GetSendQuotaAsync());

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    var message = $"An error occurred while trying to get SES quota information. HttpStatusCode returned is '{response.HttpStatusCode.ToString()}'.";

                    throw new AmazonServiceException(message, null, response.HttpStatusCode);
                }

                Max24HourSend = response.Max24HourSend;
                MaxSendRate = response.MaxSendRate;
                Sent = response.SentLast24Hours;

                LastUpdated = DateTime.Now;
            }

            return this;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);              // Calling from Dispose, it's safe
            GC.SuppressFinalize(this);  // GC: don't bother calling finalize later
        }

        public void Dispose(bool notFromFinalizer)
        {
            // If there are not unmanaged resources bailout
            if (notFromFinalizer)
            {
                return;
            }

            if (Client != null && !ExternalClient)
            {
                Client.Dispose();
                Client = null;
            }
        }

        #endregion IDisposable Members
    }
}
