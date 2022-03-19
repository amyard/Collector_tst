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

            foreach (var emailAddress in toAddress)
            {
                mailMessage.To.Add(emailAddress);
            }

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

            var mailMessage = CreateMailMessage(
                subject,
                sendErrorMessage,
                emailFromAddress,
                CreateEmailAddressList(notifyEmailAddress).ToList()
            );

            try
            {
                SendMailMessage(mailMessage);
                Log.Information($"Email: {subject} {mailMessage}");
            }
            catch (SmtpException ex)
            {
                Log.Error($"SmtpException has occurred: {ex.Message}");
            }
            finally
            {
                mailMessage.Dispose();
            }
        }

        private static IEnumerable<string> CreateEmailAddressList(string emailAddressList)
        {
            var listEmailAddress = new List<string>();
            listEmailAddress.AddRange(emailAddressList
                                                .ToLower()
                                                .Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)
                                                .Distinct());
            return listEmailAddress;
        }
    }
}