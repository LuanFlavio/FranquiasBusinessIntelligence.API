using Domain.Data;
using Domain.Domain;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace FranquiasBusinessIntelligence.API.Services.General
{
    public class EnvioEmail
    {

        private SmtpClient client;
        private MailMessage mailMessage;
        private IEmail email;
        public EnvioEmail(IEmail email)
        {
            this.email = email;
            configurarEmail();
        }
        public async Task<string> enviarEmail(ICompany empresas)
        {
            try
            {
                mailMessage.Body = repleceWords(empresas);
                mailMessage.Subject = "Título exemplo"; // this.email.tituloEmail;
                string[] subs = empresas.Email!.Split(';');

                foreach (var sub in subs)
                {
                    mailMessage.To.Add(sub);
                }

                try
                {
                    await client.SendMailAsync(mailMessage);
                    return "E-mail enviado com sucesso!";
                }
                catch (Exception ex)
                {

                    return ex.Message;
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }
        private void configurarEmail()
        {
            client = new SmtpClient(email.smtp);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(email.email, email.senha);
            client.Port = email.porta;
            client.EnableSsl = email.SSL;

            mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(email.email);
            mailMessage.IsBodyHtml = true;
        }


        private string repleceWords(ICompany empresa)
        {
            var texto = "Texto Exemplo"; //this.email.corpoEmail;
            var words = texto.Split(" ");
            var alterar = words.Where(p => p.Contains("#")).ToList();
            PropertyInfo[] properties = typeof(ICompany).GetProperties();

            foreach (var s in alterar)
            {
                foreach (PropertyInfo property in properties)
                {
                    if (s.ToUpper().Replace("#", "").Equals(property.Name.ToUpper()))
                    {
                        texto = texto.Replace(s, property.GetValue(empresa)!.ToString());
                    }
                }
            }
            return texto;
        }


    }
}
