using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ───── Identity Setup (this gives you AddDefaultIdentity + Roles) ─────
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
})
    .AddRoles<IdentityRole>()                           // enables RoleManager<IdentityRole>
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Optional: only add the developer exception page filter in Development
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireLecturerRole",
        policy => policy.RequireRole("Lecturer"));

    options.AddPolicy("RequireCoordinatorOrManagerRole",
        policy => policy.RequireRole("Coordinator", "Manager"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Your custom services
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IClaimService, ClaimService>();

var app = builder.Build();

// ───── Pipeline ─────
if (app.Environment.IsDevelopment())
{
    // Remove UseMigrationsEndPoint() unless you have Identity UI scaffolded
    // app.UseMigrationsEndPoint();   <-- comment or delete this line
}
else
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

// ───── Seed Data ─────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync(); // applies migrations automatically

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedData.Initialize(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();