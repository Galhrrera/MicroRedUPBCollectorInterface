using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataStructure
{
    public class Mail
    {
        /// <summary>
        /// Correo desde el cual se va a enviar el email
        /// </summary>
        private readonly MailboxAddress From = new MailboxAddress("Micro-Red CollectorInterface", "microredupb@gmail.com");

        /// <summary>
        /// Lista de destinatarios del email
        /// </summary>
        private List<MailboxAddress> To { get; set; }

        /// <summary>
        /// Asunto del correo
        /// </summary>
        private string Subject { get; set; }

        /// <summary>
        /// Cuerpo del documento
        /// </summary>
        private List<string> Body { get; set; }

        /// <summary>
        /// Mensaje del correo
        /// </summary>
        private MimeMessage Message { get; set; }

        public Mail(List<MailboxAddress> to, string subject, List<string> body)
        {
            Message = new MimeMessage();
            To = to;
            Subject = subject;
            Body = body;

            Message.From.Add(From);
            foreach (var item in To)
            {
                Message.To.Add(item);
            }

            Message.Subject = Subject;

            string stringBody = "";

            foreach (var item in Body)
            {
                stringBody = stringBody + "<p>" + item + "</p>";
            }

            stringBody = stringBody +
                "<br>Saludos del <b>CollectorInterface</b>, cualquier inquietud comunicarse con:<br>" +
                "<b>Andrés Eusse</b><br>" +
                "<b>andresfelipe.eusse@upb.edu.co</b></p>";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = stringBody
            };

            Message.Body = bodyBuilder.ToMessageBody();
        }

        public async Task<bool> SendMail()
        {
            var client = new SmtpClient
            {
                ServerCertificateValidationCallback = (p1, p2, p3, p4) => true
            };
            await client.ConnectAsync("smtp.gmail.com", 465, true);

            await client.AuthenticateAsync("microredupb@gmail.com", "MicroRedUPB0.");

            await client.SendAsync(Message);

            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            await client.DisconnectAsync(true);
            return true;
        }

    }
}
