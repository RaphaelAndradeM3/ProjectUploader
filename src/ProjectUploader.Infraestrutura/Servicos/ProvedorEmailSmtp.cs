using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ProjectUploader.Aplicacao.Interfaces;

namespace ProjectUploader.Infraestrutura.Servicos;

public class ProvedorEmailSmtp : IServicoEmail
{
    public async Task EnviarEmailRecuperacaoAsync(string emailDestino, string linkOuCodigoRecuperacao)
    {
        // TODO: Mover credenciais para appsettings.json via IOptions futuramente.
        var smtpClient = new SmtpClient("smtp.exemplo.com")
        {
            Port = 587,
            Credentials = new NetworkCredential("usuario", "senha"),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress("no-reply@projectuploader.com"),
            Subject = "Recuperação de Senha - Project Uploader",
            Body = $"Você solicitou a recuperação de senha.\n\nAcesse ou use o código: {linkOuCodigoRecuperacao}\n\nSe não foi você, ignore este email.",
            IsBodyHtml = false,
        };
        mailMessage.To.Add(emailDestino);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
