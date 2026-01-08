using PGSA_Licence3.Data;
using PGSA_Licence3.Services.UserManagement;
 
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Models.Seeders;
using PGSA_Licence3.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
// 1. CONFIGURATION DES SERVICES (builder.Services)
// ==========================================================

// 🔹 Base de données MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 🔹 Controllers, Razor et Configuration JSON (Regroupés pour éviter les erreurs)
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddJsonOptions(options =>
    {
        // Gère les cycles de référence dans les relations Entity Framework
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
        // Convertit les Enums en chaînes de caractères (ex: "Actif" au lieu de 1)
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// 🔹 Services personnalisés (Business Logic)
// builder.Services.AddScoped<CahierDeTexteService>();
// builder.Services.AddScoped<SaveUserService>(); // Décommentez si nécessaire
// builder.Services.AddScoped<PermissionService>(); // Décommentez si nécessaire

// 🔹 Authentification par Cookies
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.ReturnUrlParameter = "returnUrl";
    });

// 🔹 Autorisation
builder.Services.AddAuthorization();

// ==========================================================
// 2. CONSTRUCTION DE L'APPLICATION
// ==========================================================

var app = builder.Build();

// ==========================================================
// 3. INITIALISATION DES DONNÉES (SEEDING)
// ==========================================================

// On crée un scope pour récupérer le DbContext car l'app est maintenant construite
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DatabaseSeeder.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur est survenue lors du peuplement de la base de données.");
    }
}

// ==========================================================
// 4. CONFIGURATION DU PIPELINE HTTP (Middlewares)
// ==========================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Sécurité (L'ordre est crucial : Authentication AVANT Authorization)
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Mapping des routes
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

// Redirection de la racine vers la page de login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Login");
    return Task.CompletedTask;
});

// Lancement
app.Run();