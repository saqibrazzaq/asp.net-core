using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;

namespace Rocky.Utility
{
    public class EmailSender : IEmailSender
    {
        private IConfiguration _configuration;
        public MailJetSettings _mailJetSettings { get; set; }
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string htmlMessage)
        {
            _mailJetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();
            MailjetClient client = new MailjetClient(_mailJetSettings.ApiKey, _mailJetSettings.SecretKey);
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            };
             // construct your email with builder
         var emailBuilder = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("saqib.razzaq@saqibtechnologies.com"))
                .WithSubject(subject)
                .WithHtmlPart(htmlMessage)
                .WithTo(new SendContact(email))
                .Build();

            // invoke API to send email
            var response = await client.SendTransactionalEmailAsync(emailBuilder);
            //MailjetResponse response = await client.PostAsync(request);
        }
    }
}
