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

// 🔹 Ajouter les controllers avec Razor Runtime Compilation
builder.Services.AddControllersWithViews()
       .AddRazorRuntimeCompilation();

// Services for user and permission management
// builder.Services.AddScoped<SaveUserService>();
// builder.Services.AddScoped<PermissionService>();
// Cahier de texte service
builder.Services.AddScoped<CahierDeTexteService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//     await DatabaseSeeder.SeedAsync(context);
// }

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

// Active les routes via les attributs [Route]
app.MapControllers();

// Route par défaut
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
