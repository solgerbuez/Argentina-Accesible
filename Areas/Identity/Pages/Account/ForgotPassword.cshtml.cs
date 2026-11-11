using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ArgentinaAccesible.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; } = "";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var usuario = await _userManager.FindByEmailAsync(Input.Email);
            if (usuario != null)
            {
                // Token de un solo uso, codificado para poder viajar dentro de un enlace
                var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
                var codigo = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                var enlace = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code = codigo, email = Input.Email },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Restablecer tu contraseña - Argentina Accesible",
                    $"<p>Hola,</p>" +
                    $"<p>Recibimos un pedido para restablecer la contraseña de tu cuenta en <strong>Argentina Accesible</strong>.</p>" +
                    $"<p><a href=\"{HtmlEncoder.Default.Encode(enlace!)}\">Crear una nueva contraseña</a></p>" +
                    $"<p>Si no fuiste vos, ignorá este mensaje: tu contraseña no cambia.</p>");
            }

            // Siempre la misma respuesta, exista o no el correo: así nadie puede averiguar qué correos están registrados
            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}