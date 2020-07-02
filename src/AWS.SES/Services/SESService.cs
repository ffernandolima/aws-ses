using Amazon.SimpleEmail.Model;
using AWS.SES.Exceptions;
using AWS.SES.Models;
using AWS.SES.Senders;
using Extensions.Configuration.Factory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AWS.SES.Services
{
    public class SESService : ISESService
    {
        private readonly IConfiguration _configuration;

        public SESService() => _configuration = ConfigurationFactory.Instance.GetConfiguration();

        public Task SendEmailAsync(EmailRequest emailRequest, int maxRetries = 10, CancellationToken cancellationToken = default) => SendEmailInternalAsync(emailRequest, maxRetries, cancellationToken);

        private Task SendEmailInternalAsync(EmailRequest emailRequest, int maxRetries, CancellationToken cancellationToken = default)
        {
            var disabled = _configuration.GetValue<bool>("AWS:SES:Disabled");

            if (disabled)
            {
                return Task.CompletedTask;
            }

            try
            {
                var sendEmailRequest = ToSendEmailRequest(emailRequest);

                return SESSender.Instance.SendEmailAsync(sendEmailRequest, maxRetries, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new SESManagerException(ex);
            }
        }

        private SendEmailRequest ToSendEmailRequest(EmailRequest emailRequest)
        {
            if (string.IsNullOrWhiteSpace(emailRequest.From))
            {
                emailRequest.From = _configuration.GetValue<string>("AWS:SES:EmailRequest:From");
            }

            if (!(emailRequest.To?.Any() ?? false))
            {
                throw new SESException("A message must have recipents.");
            }

            emailRequest.To = emailRequest.To.Where(t => !string.IsNullOrWhiteSpace(t)).ToList();

            if (string.IsNullOrWhiteSpace(emailRequest.Subject))
            {
                throw new SESException("A message must have a subject.");
            }

            if (string.IsNullOrWhiteSpace(emailRequest.HtmlBody))
            {
                throw new SESException("A message must have content (body).");
            }

            if (string.IsNullOrWhiteSpace(emailRequest.From))
            {
                throw new SESException("A message must have a sender.");
            }

            var sendEmailRequest = new SendEmailRequest
            {
                Source = emailRequest.From,

                ReplyToAddresses = new List<string>
                {
                    emailRequest.ReplyTo
                },

                Destination = new Destination
                {
                    ToAddresses = emailRequest.To
                },

                Message = new Message
                {
                    Subject = new Content
                    {
                        Data = emailRequest.Subject
                    },

                    Body = new Body
                    {
                        Html = new Content
                        {
                            Data = emailRequest.HtmlBody
                        }
                    }
                }
            };

            return sendEmailRequest;
        }

        #region IDisposable Members

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}
