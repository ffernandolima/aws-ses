using Amazon.Runtime;
using Amazon.SimpleEmail.Model;
using AWS.SES.Exceptions;
using AWS.SES.Factories;
using AWS.SES.Helpers;
using AWS.SES.RateLimiters;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.SES.Senders
{
    internal class SESSender
    {
        private static readonly Lazy<SESSender> Factory = new Lazy<SESSender>(() => new SESSender(), isThreadSafe: true);

        private SESSender()
        { }

        public static SESSender Instance => Factory.Value;

        public Task SendEmailAsync(SendEmailRequest sendEmailRequest, int maxRetries, CancellationToken cancellationToken = default)
        {
            return Task.Factory.StartNew(() =>
            {
                SESRetryHelper.Do(() =>
                {
                    using (var client = SESClientFactory.CreateClient())
                    {
                        var response = AsyncHelper.RunSync(() => client.SendEmailAsync(sendEmailRequest));

                        if (response.HttpStatusCode != HttpStatusCode.OK)
                        {
                            var message = $"An error occurred while sending an email through SES. HttpStatusCode returned is '{response.HttpStatusCode.ToString()}'.";

                            throw new AmazonServiceException(message, null, response.HttpStatusCode);
                        }
                    }
                },

                (exception, attempts) =>
                {
                    if (attempts > maxRetries)
                    {
                        return false;
                    }

                    var sleepTime = TimeSpan.Zero;

                    switch (exception.ExceptionType)
                    {
                        case SESExceptionType.MessageRejected:
                        case SESExceptionType.NoCredentialsFound:
                        case SESExceptionType.Configuration:
                            {
                                return false;
                            }
                        case SESExceptionType.DailyQuota:
                            {
                                SESBaseLimiter.QuotaLimiter.WaitToProceed();
                                sleepTime = SESBaseLimiter.BackoffDelay.Max();
                            }
                            break;
                        case SESExceptionType.SendRate:
                            {
                                SESBaseLimiter.RateLimiter.WaitToProceed();
                                sleepTime = SESBaseLimiter.BackoffDelay.Calculate(attempts);
                            }
                            break;
                        default:
                            {
                                return true;
                            }
                    }

                    Debug.WriteLine("[{0:D3}]\t{1}\t{2}\t{3}\t[{4}]", Thread.CurrentThread.ManagedThreadId, attempts, exception.ExceptionType.ToString(), sleepTime.TotalMilliseconds, sendEmailRequest.Message.Subject.Data);

                    Thread.Sleep(sleepTime);

                    return true;
                });

            }, cancellationToken);
        }
    }
}
