using ArgentinaAccesible.Services;
using ArgentinaAccesible.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


var cultura = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddErrorDescriber<SpanishIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews(options =>
{
    var p = options.ModelBindingMessageProvider;
    p.SetValueMustNotBeNullAccessor(_ => "Este campo es obligatorio.");
    p.SetMissingBindRequiredValueAccessor(nombre => $"Falta el valor de {nombre}.");
    p.SetMissingKeyOrValueAccessor(() => "Falta un valor.");
    p.SetValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    p.SetAttemptedValueIsInvalidAccessor((valor, nombre) => $"El valor '{valor}' no es válido para {nombre}.");
    p.SetUnknownValueIsInvalidAccessor(nombre => $"El valor ingresado no es válido para {nombre}.");
    p.SetNonPropertyAttemptedValueIsInvalidAccessor(valor => $"El valor '{valor}' no es válido.");
    p.SetNonPropertyUnknownValueIsInvalidAccessor(() => "El valor ingresado no es válido.");
    p.SetValueMustBeANumberAccessor(nombre => $"El campo {nombre} debe ser un número.");
    p.SetNonPropertyValueMustBeANumberAccessor(() => "El valor debe ser un número.");
});
builder.Services.AddHttpClient<GeminiService>(c => c.Timeout = TimeSpan.FromSeconds(30));


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Crea la BD (migraciones) y carga roles/usuarios/datos iniciales
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();