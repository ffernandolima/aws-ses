using AWS.SES.Exceptions;
using System;
using System.Threading;

namespace AWS.SES.Helpers
{
    /// <remarks>
    /// https://msdn.microsoft.com/en-us/library/dn589788.aspx
    /// http://blogs.msdn.com/b/dgartner/archive/2010/03/09/trying-and-retrying-in-c.aspx
    /// </remarks>
    internal static class SESRetryHelper
    {
        public static TResult Do<TResult>(this Func<TResult> retryAction, Func<SESException, int, bool> shouldRetry) => Do(retryAction, shouldRetry, TimeSpan.Zero);

        public static TResult Do<TResult>(this Func<TResult> retryAction, Func<SESException, int, bool> shouldRetry, TimeSpan delay)
        {
            var result = default(TResult);

            void retryActionInternal()
            {
                result = retryAction();
            }

            Do(retryActionInternal, shouldRetry, delay);

            return result;
        }

        public static void Do(this Action retryAction, Func<SESException, int, bool> shouldRetry) => Do(retryAction, shouldRetry, TimeSpan.Zero);

        public static void Do(this Action retryAction, Func<SESException, int, bool> shouldRetry, TimeSpan delay)
        {
            var counter = 0;

            SESException sesException;
            bool shouldRetryResult;

            do
            {
                try
                {
                    retryAction.Invoke();
                    return;
                }
                catch (Exception ex)
                {
                    sesException = new SESException(ex, counter);
                    counter++;
                }

                shouldRetryResult = shouldRetry(sesException, counter);

                if (shouldRetryResult && sesException != null)
                {
                    Thread.Sleep(delay);
                }

            } while (shouldRetryResult);

            throw sesException;
        }
    }
}
