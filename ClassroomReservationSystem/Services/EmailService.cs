using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
namespace ClassroomReservationSystem.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpSection = _configuration.GetSection("SmtpSettings");

                using var smtpClient = new SmtpClient(smtpSection["Host"])
                {
                    Port = int.Parse(smtpSection["Port"]),
                    Credentials = new NetworkCredential(
                        smtpSection["UserName"],
                        smtpSection["Password"]
                    ),
                    EnableSsl = bool.Parse(smtpSection["EnableSsl"])
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSection["UserName"], "Classroom Reservation System"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);

                return "Email sent successfully!";
            }
            catch (Exception ex)
            {
                return $"Failed to send email: {ex.Message}";
            }
        }
    }
}
