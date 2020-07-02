using AWS.SES.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.SES.Services
{
    public interface ISESService : IDisposable
    {
        Task SendEmailAsync(EmailRequest emailRequest, int maxRetries = 10, CancellationToken cancellationToken = default);
    }
}
