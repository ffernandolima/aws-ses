using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using System;

namespace AWS.SES.Exceptions
{
    public class SESException : Exception
    {
        private static string ExceptionIdentifier => typeof(SESException).Name;

        public SESException(string message)
            : base($"{ExceptionIdentifier} - {message}")
        { }

        public SESException(Exception innerException)
            : base(ExceptionIdentifier, innerException) => ExceptionType = ToExceptionType(innerException);

        public SESException(Exception innerException, int retries)
            : base(ExceptionIdentifier, innerException)
        {
            RetryCount = retries;
            ExceptionType = ToExceptionType(innerException);
        }

        public int RetryCount { get; }
        public SESExceptionType ExceptionType { get; }
        public override string Message => RetryCount > 0 ? $"{base.Message} [{RetryCount} Retries]." : base.Message;

        private SESExceptionType ToExceptionType(Exception exception)
        {
            if (exception is AmazonSimpleEmailServiceException sesException)
            {
                if (sesException.ErrorCode == "Throttling")
                {
                    if (sesException.Message == "Daily message quota exceeded.")
                    {
                        return SESExceptionType.DailyQuota;
                    }

                    if (sesException.Message == "Maximum sending rate exceeded.")
                    {
                        return SESExceptionType.SendRate;
                    }
                }
                else if (sesException.ErrorCode == "AccessDenied")
                {
                    return SESExceptionType.AccessDenied;
                }
            }

            if (exception is MessageRejectedException)
            {
                return SESExceptionType.MessageRejected;
            }

            if (exception is AmazonServiceException && exception.Message.StartsWith("Unable to find credentials"))
            {
                return SESExceptionType.NoCredentialsFound;
            }

            return SESExceptionType.Unknown;
        }
    }
}
