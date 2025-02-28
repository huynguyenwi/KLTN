using System.Net.Mail;
using System.Net;

namespace Shopping.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("nhhuy.dhti15a3hn@sv.uneti.edu.vn", "ztvfrlonmpasigtn")
            };

            return client.SendMailAsync(
                new MailMessage(from: "nhhuy.dhti15a3hn@sv.uneti.edu.vn",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
