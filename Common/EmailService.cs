using Azure.Communication.Email;
using Azure.Communication.Email.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading;

namespace IM_Core.Common
{
    public interface IEmailService
    {
        void SendEmail();
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        public EmailService(IConfiguration _configuration)
        {
            configuration = _configuration;
        }

        public void SendEmail()
        {
            EmailClient emailClient = new EmailClient(configuration.GetConnectionString("CommunicationService"));
            EmailContent emailContent = new EmailContent("Welcome to Azure Communication Service Email APIs.");
            emailContent.PlainText = "This email message is sent from Azure Communication Service Email using .NET SDK.";
            List<EmailAddress> emailAddresses = new List<EmailAddress>
                        { new EmailAddress("omarsharif91@gmail.com") { DisplayName = "Friendly Display Name" }
            };
            EmailRecipients emailRecipients = new EmailRecipients(emailAddresses);
            EmailMessage emailMessage = new EmailMessage("DoNotReply@14026211-a689-4495-97a4-a8ff1ed12a23.azurecomm.net", emailContent, emailRecipients);
            SendEmailResult emailResult = emailClient.Send(emailMessage, CancellationToken.None);

        }
    }
}
