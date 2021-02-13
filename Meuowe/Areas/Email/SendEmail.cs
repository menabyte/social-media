using System;
using System.Net.Mail;

namespace Meuowe.Areas.Email
{
    public class SendEmail
    {
        public void Send(string emailSubject, string emailMessage, string emailAddress)
        {
            // Credentials
            MailMessage m = new MailMessage();

            m.From = new MailAddress("meuowe@outlook.com", "Meuowe");
            m.To.Add(new MailAddress(emailAddress));
            m.Subject = emailSubject;
            m.IsBodyHtml = true;
            m.Body = String.Format(emailMessage);

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.live.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Credentials = new System.Net.NetworkCredential()
            {
                UserName = "meuowe@outlook.com",
                Password = "BiGOL42$*("
            };
            smtp.Send(m);
        }
    }
}
