using System;

namespace AWS.SES.Exceptions
{
    public class SESManagerException : ApplicationException
    {
        public SESManagerException(Exception innerException)
            : base("An error occurred while processing your request.", innerException)
        { }
    }
}
