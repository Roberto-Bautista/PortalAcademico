using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=app.db";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Para acceder a HttpContext desde las vistas
builder.Services.AddHttpContextAccessor();

// ============== CONFIGURAR REDIS ==============
var redisConnection = builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrEmpty(redisConnection))
{
    try
    {
        // Configurar Cache con Redis
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = builder.Configuration["Redis:InstanceName"];
        });
        
        Console.WriteLine($"✅ Redis configurado: {redisConnection}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Redis no disponible, usando cache en memoria: {ex.Message}");
        builder.Services.AddDistributedMemoryCache();
    }
}
else
{
    Console.WriteLine("⚠️ Redis no configurado, usando cache en memoria");
    builder.Services.AddDistributedMemoryCache();
}

// Configurar Sesiones (con Redis si está disponible)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".PortalAcademico.Session";
});

var app = builder.Build();

// Seed roles y usuario coordinador
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Crear rol Coordinador
    if (!await roleManager.RoleExistsAsync("Coordinador"))
    {
        await roleManager.CreateAsync(new IdentityRole("Coordinador"));
        Console.WriteLine("✅ Rol Coordinador creado");
    }
    
    // Crear usuario coordinador
    var coordinador = await userManager.FindByEmailAsync("coordinador@universidad.edu");
    if (coordinador == null)
    {
        coordinador = new IdentityUser
        {
            UserName = "coordinador@universidad.edu",
            Email = "coordinador@universidad.edu",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(coordinador, "Coord123!");
        await userManager.AddToRoleAsync(coordinador, "Coordinador");
        Console.WriteLine("✅ Usuario Coordinador creado");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ============== HABILITAR SESIONES ==============
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

Console.WriteLine("🚀 Aplicación iniciada");

app.Run();