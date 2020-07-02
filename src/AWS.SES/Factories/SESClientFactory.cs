using Amazon.SimpleEmail;
using Extensions.Configuration.Factory;
using Microsoft.Extensions.Configuration;

namespace AWS.SES.Factories
{
    internal static class SESClientFactory
    {
        public static IAmazonSimpleEmailService CreateClient()
        {
            var configuration = ConfigurationFactory.Instance.GetConfiguration();

            var options = configuration.GetAWSOptions("AWS:SES");

            var client = options.CreateServiceClient<IAmazonSimpleEmailService>();

            return client;
        }
    }
}
