using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace ArgentinaAccesible.Services;

// Envía los correos de Identity (restablecer contraseña) usando MailKit y SMTP
public class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IConfiguration config, ILogger<MailKitEmailSender> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var host = _config["Email:Host"] ?? "smtp.gmail.com";
        var port = _config.GetValue("Email:Port", 587);
        var usuario = _config["Email:User"];
        var clave = _config["Email:Password"];
        var nombre = _config["Email:FromName"] ?? "Argentina Accesible";

        // Las credenciales vienen de User Secrets (local) o de variables de entorno (hosting)
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
        {
            _logger.LogWarning("No se envió el correo: faltan Email:User o Email:Password.");
            return;
        }

        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(nombre, usuario));
        mensaje.To.Add(MailboxAddress.Parse(email));
        mensaje.Subject = subject;
        mensaje.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

        using var smtp = new MailKit.Net.Smtp.SmtpClient { Timeout = 20000 };
        try
        {
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(usuario, clave);
            await smtp.SendAsync(mensaje);
        }
        catch (Exception ex)
        {
            // Se registra el error pero no se muestra al visitante
            _logger.LogError(ex, "No se pudo enviar el correo a {Destino}", email);
        }
        finally
        {
            if (smtp.IsConnected) await smtp.DisconnectAsync(true);
        }
    }
}