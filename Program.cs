using PGSA_Licence3.Data;
using Microsoft.EntityFrameworkCore;
using PGSA_Licence3.Services.Groupes_Management;
using PGSA_Licence3.Services.Students;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Ajouter le DbContext avec MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 🔹 Enregistrer les services de gestion des groupes
builder.Services.AddScoped<CreateGroupeService>();
builder.Services.AddScoped<UpdateGroupeService>();
builder.Services.AddScoped<GetGroupesService>();
builder.Services.AddScoped<GetGroupeDetailsService>();
builder.Services.AddScoped<DeleteGroupeService>();
builder.Services.AddScoped<AddStudentToGroupeService>();
builder.Services.AddScoped<RemoveStudentFromGroupeService>();
builder.Services.AddScoped<ImportStudentsToGroupeService>();
builder.Services.AddScoped<SaveImportedStudentsService>();
builder.Services.AddScoped<StudentImportService>();

// 🔹 Enregistrer les services de gestion des rôles
builder.Services.AddScoped<PGSA_Licence3.Services.Role_Managment.RoleService>();
builder.Services.AddScoped<PGSA_Licence3.Services.Role_Managment.PermissionService>();
builder.Services.AddScoped<PGSA_Licence3.Services.Role_Managment.UserRoleService>();
builder.Services.AddScoped<PGSA_Licence3.Services.Role_Managment.PermissionSeedService>();

// 🔹 Enregistrer les services de statistiques et rapports
builder.Services.AddScoped<PGSA_Licence3.Services.Statistics.StatisticsService>();

// 🔹 Ajouter les controllers avec Razor Runtime Compilation
builder.Services.AddControllersWithViews()
       .AddRazorRuntimeCompilation();

var app = builder.Build();

// 🔹 Initialiser les permissions de base au démarrage
using (var scope = app.Services.CreateScope())
{
    var permissionSeedService = scope.ServiceProvider.GetRequiredService<PGSA_Licence3.Services.Role_Managment.PermissionSeedService>();
    try
    {
        permissionSeedService.SeedPermissionsAsync().GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Une erreur s'est produite lors de l'initialisation des permissions.");
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
