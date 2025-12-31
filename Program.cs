using PGSA_Licence3.Data;
using PGSA_Licence3.Services.UserManagement;
using PGSA_Licence3.Services.CahierDeTexte;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Models.Seeders;
using PGSA_Licence3.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Ajouter le DbContext avec MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 🔹 Ajouter les controllers avec Razor Runtime Compilation et configuration JSON
builder.Services.AddControllersWithViews()
       .AddRazorRuntimeCompilation()
       .AddJsonOptions(options =>
       {
           options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
           options.JsonSerializerOptions.MaxDepth = 64;
       });
       
// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     var context = services.GetRequiredService<ApplicationDbContext>();
//     await DatabaseSeeder.SeedAsync(context); // plus de passwordHasher
// }
// Dans Program.cs, ajoutez ce code pour configurer les convertisseurs JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 🔹 Services personnalisés
builder.Services.AddScoped<CahierDeTexteService>();
// builder.Services.AddScoped<SaveUserService>();
// builder.Services.AddScoped<PermissionService>();
 

// 🔹 Authentification cookie avec schéma par défaut
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";          // Authentification par défaut
    options.DefaultChallengeScheme = "Cookies"; // Redirection pour [Authorize]
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/Login";               // Redirection si non connecté
    options.AccessDeniedPath = "/Login";        // Redirection si accès refusé
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.ReturnUrlParameter = "returnUrl";   // Pour redirection après login
});

// 🔹 Autorisation
builder.Services.AddAuthorization();

var app = builder.Build();

// 🔹 Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 🔹 Authentification et autorisation (ordre IMPORTANT)
app.UseAuthentication();
app.UseAuthorization();

// 🔹 Active les routes via attributs [Route]
app.MapControllers();

// 🔹 Route par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");
app.MapGet("/", context =>
{
    context.Response.Redirect("/Login");
    return Task.CompletedTask;
});

app.Run();