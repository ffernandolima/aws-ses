
namespace AWS.SES.Exceptions
{
    public enum SESExceptionType
    {
        Unknown = 0,
        NoCredentialsFound = 1,
        MessageRejected = 2,
        DailyQuota = 3,
        SendRate = 4,
        Configuration = 5,
        AccessDenied = 6
    }
}
