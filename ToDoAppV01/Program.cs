using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ToDoAppV01.Data;
using ToDoAppV01.Models;

namespace ToDoAppV01
{
    public class Program
    {
        public static Dictionary<string, RoleConfig> roles;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>() // Hinzufuegen der Rollenverwaltung
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();

            var app = builder.Build();
            app.UseStaticFiles();



            CreateRoles(app).Wait();
            CreateAdminUser(app).Wait();

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

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }

        /// <summary>
        /// Anlegen der Rollen "Admin" und "User", falls diese noch nicht existieren
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static async Task CreateRoles(WebApplication app)
        {

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var db = services.GetRequiredService<ApplicationDbContext>();
                    // Wendet alle noch nicht ausgeführten Migrationen an
                    db.Database.Migrate();

                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var config = services.GetRequiredService<IConfiguration>();

                    #region Roles aus der "appsettings.json" auslesen
                    roles = app.Configuration
                    .GetSection("Roles")
                    .Get<Dictionary<string, RoleConfig>>();

                    foreach (var role in roles)
                    {
                        Trace.WriteLine($"Role from config: {role.Key}, Role: {role.Value.Role} , Email: {role.Value.Email}, PW: {role.Value.Password} ");
                    }

                    #endregion Roles aus der "appsettings.json" auslesen
                    // ---------------------------------------------------------------------
                    #region Rollen anlegen
                    foreach (var role in roles.Keys)
                    {
                        Trace.WriteLine($"Seeding role: {role}");
                        if (!await roleManager.RoleExistsAsync(role))
                        {
                            var result = await roleManager.CreateAsync(new IdentityRole(role));
                            if (!result.Succeeded)
                            {
                                var errors = string.Join("; ", result.Errors);
                                logger?.LogError("Failed to create role {Role}: {Errors}", role, errors);
                                Trace.WriteLine($"Failed to create role {role}: {errors}");
                                throw new Exception($"Failed to create role {role}: {errors}");
                            }
                            logger?.LogInformation("Created role {Role}", role);
                            Trace.WriteLine($"Created role {role}");
                        }
                        else
                        {
                            logger?.LogDebug("Role {Role} already exists", role);
                            Trace.WriteLine($"Role {role} already exists");
                        }
                    }
                    #endregion Rollen anlegen

                    // ---------------------------------------------------------------------

                    #region Anlegen eines Admin-Benutzers, falls dieser noch nicht existiert
                    string adminEmail = roles["Admin"].Email;
                    string adminPassword = roles["Admin"].Password;

                    if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
                    {
                        logger?.LogInformation("Admin account configuration not provided; skipping admin user creation.");
                        Trace.WriteLine("Admin account configuration not provided; skipping admin user creation.");
                        return;
                    }

                    var adminUser = await userManager.FindByEmailAsync(adminEmail);
                    if (adminUser == null)
                    {
                        adminUser = new IdentityUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true
                        };

                        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                        if (!createResult.Succeeded)
                        {
                            var errors = string.Join("; ", createResult.Errors);
                            logger?.LogError("Failed to create admin user {Email}: {Errors}", adminEmail, errors);
                            Trace.WriteLine($"Failed to create admin user {adminEmail}: {errors}");
                            throw new Exception($"Failed to create admin user {adminEmail}: {errors}");
                        }
                        logger?.LogInformation("Created admin user {Email}", adminEmail);
                        Trace.WriteLine($"Created admin user {adminEmail}");
                    }

                    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                    {
                        var addRoleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
                        if (!addRoleResult.Succeeded)
                        {
                            var errors = string.Join("; ", addRoleResult.Errors);
                            logger?.LogError("Failed to assign Administrator role to {Email}: {Errors}", adminEmail, errors);
                            Trace.WriteLine($"Failed to assign Administrator role to {adminEmail}: {errors}");
                            throw new Exception($"Failed to assign Administrator role to {adminEmail}: {errors}");
                        }
                        logger?.LogInformation("Assigned Administrator role to {Email}", adminEmail);
                        Trace.WriteLine($"Assigned Administrator role to {adminEmail}");
                    }
                    #endregion Anlegen eines Admin-Benutzers, falls dieser noch nicht existiert

                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
                    // In vielen Produktionen möchte man hier throw; damit Deployment fehlschlägt und sichtbar wird.
                    throw;
                }
            }
        }

        /// <summary>
        /// Anlegen eines Admin-Benutzers, falls dieser noch nicht existiert
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task CreateAdminUser(WebApplication app)
        {
            // Initialisierung eines Standard-Benutzers
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                string defaultUserEmail = roles["Admin"].Email;
                var adminUser = await userManager.FindByEmailAsync(defaultUserEmail);

                if (adminUser == null)
                {
                    var defaultUser = new IdentityUser
                    {
                        UserName = defaultUserEmail,
                        Email = defaultUserEmail,
                        EmailConfirmed = true
                    };

                    string defaultUserPassword = roles["Admin"].Password;

                    if (string.IsNullOrEmpty(defaultUserPassword))
                    {
                        Trace.WriteLine("Kein Passwort fuer den Standardbenutzer in der Konfiguration gefunden.");
                        logger.LogError("Kein Passwort fuer den Standardbenutzer in der Konfiguration gefunden.");
                        throw new InvalidOperationException("Kein Passwort fuer den Standardbenutzer in der Konfiguration gefunden.");
                    }
                    var createUserResult = await userManager.CreateAsync(defaultUser, defaultUserPassword);
                    if (createUserResult.Succeeded)
                    {
                        adminUser = await userManager.FindByEmailAsync(defaultUserEmail);
                    }

                    if (!createUserResult.Succeeded || adminUser == null)
                    {
                        // Fehlerbehandlung bei der Benutzererstellung
                        Trace.WriteLine("Fehler bei der Erstellung des Standardbenutzers.");
                        logger.LogError("Fehler bei der Erstellung des Standardbenutzers.");
                        throw new InvalidOperationException("Fehler bei der Erstellung des Standardbenutzers.");
                    }
                }
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
