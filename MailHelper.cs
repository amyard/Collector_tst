using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Serilog;

namespace Collector
{
    public static class MailHelper
    {
        public static MailMessage CreateMailMessage(string subject, string body, string fromAddress,
            List<string> toAddress)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            
            toAddress.ForEach(emailAddress => mailMessage.To.Add(emailAddress));

            return mailMessage;
        }

        public static void SendMailMessage(MailMessage mailMessage)
        {
            using (var mailClient = new SmtpClient())
            {
                mailClient.Send(mailMessage);
            }
        }

        public static void SendEmailForGlobalException(string sendErrorMessage, string emailFromAddress,
            string notifyEmailAddress)
        {
            string subject = "Unsuccessful transmission: Check the Log File.";

            using (var mailMessage = CreateMailMessage(subject, sendErrorMessage, emailFromAddress,
                CreateEmailAddressList(notifyEmailAddress)))
            {
                try
                {
                    SendMailMessage(mailMessage);
                    Log.Information($"Email: {subject} {mailMessage}");
                }
                catch (SmtpException ex)
                {
                    Log.Error($"SmtpException has occurred: {ex.Message}");
                }
            }
        }

        private static List<string> CreateEmailAddressList(string emailAddressList)
        {
            return emailAddressList.ToLower()
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
        }
    }
}