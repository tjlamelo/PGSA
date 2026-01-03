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

// 🔹 Ajouter les controllers avec Razor Runtime Compilation
builder.Services.AddControllersWithViews()
       .AddRazorRuntimeCompilation();

var app = builder.Build();

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
