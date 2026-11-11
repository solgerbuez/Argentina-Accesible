using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace ArgentinaAccesible.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ResetPasswordModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
            [Display(Name = "Correo electrónico")]
            public string Email { get; set; } = "";

            [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos {2} caracteres.")]
            [DataType(DataType.Password)]
            [Display(Name = "Nueva contraseña")]
            public string Password { get; set; } = "";

            [DataType(DataType.Password)]
            [Display(Name = "Repetir contraseña")]
            [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
            public string ConfirmPassword { get; set; } = "";

            public string Code { get; set; } = "";
        }

        public IActionResult OnGet(string? code = null, string? email = null)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Falta el código para restablecer la contraseña.");

            try
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                    Email = email ?? ""
                };
            }
            catch (FormatException)
            {
                return BadRequest("El enlace no es válido.");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var usuario = await _userManager.FindByEmailAsync(Input.Email);
            if (usuario == null)
                return RedirectToPage("./ResetPasswordConfirmation");   // no se revela si el correo existe

            var resultado = await _userManager.ResetPasswordAsync(usuario, Input.Code, Input.Password);
            if (resultado.Succeeded)
                return RedirectToPage("./ResetPasswordConfirmation");

            foreach (var error in resultado.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }
    }
}