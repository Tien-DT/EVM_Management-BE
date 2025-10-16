using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using MimeKit;
using EVMManagement.BLL.Options;
using EVMManagement.BLL.Services.Interface;

namespace EVMManagement.BLL.Services.Class
{
    public class GmailApiService : IEmailService
    {
        private readonly GmailApiSettings _settings;

        public GmailApiService(IOptions<GmailApiSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            var service = await CreateGmailServiceAsync();
            var message = CreateMimeMessage(toEmail, subject, body, isHtml);
            await SendMessageAsync(service, message);
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath)
        {
            var service = await CreateGmailServiceAsync();
            var message = CreateMimeMessageWithAttachment(toEmail, subject, body, attachmentPath);
            await SendMessageAsync(service, message);
        }

        private async Task<GmailService> CreateGmailServiceAsync()
        {
            var clientSecrets = new ClientSecrets
            {
                ClientId = _settings.ClientId,
                ClientSecret = _settings.ClientSecret
            };

            var tokenResponse = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
            {
                RefreshToken = _settings.RefreshToken
            };

            var credential = new UserCredential(
                new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow(
                    new Google.Apis.Auth.OAuth2.Flows.GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = clientSecrets,
                        Scopes = new[] { GmailService.Scope.GmailSend }
                    }),
                "user",
                tokenResponse);

            await credential.RefreshTokenAsync(System.Threading.CancellationToken.None);

            return new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "EVM Management"
            });
        }

        private MimeMessage CreateMimeMessage(string toEmail, string subject, string body, bool isHtml)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml)
            {
                builder.HtmlBody = body;
            }
            else
            {
                builder.TextBody = body;
            }

            message.Body = builder.ToMessageBody();
            return message;
        }

        private MimeMessage CreateMimeMessageWithAttachment(string toEmail, string subject, string body, string attachmentPath)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };

            if (File.Exists(attachmentPath))
            {
                builder.Attachments.Add(attachmentPath);
            }

            message.Body = builder.ToMessageBody();
            return message;
        }

        private async Task SendMessageAsync(GmailService service, MimeMessage mimeMessage)
        {
            using var memoryStream = new MemoryStream();
            await mimeMessage.WriteToAsync(memoryStream);
            var rawMessage = Convert.ToBase64String(memoryStream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", string.Empty);

            var gmailMessage = new Message
            {
                Raw = rawMessage
            };

            await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();
        }
    }
}
