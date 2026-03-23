using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.DbInitializer;
using BulkyBook.DataAccess.Repository;
using BulkyBook.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BulkyBook.Utility;
using BulkyBookWeb.Services;
using Stripe;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;

internal class Program
{
	private static void Main(string[] args)
	{
        var builder = WebApplication.CreateBuilder(args);
        
        // Set default culture to en-US
        var cultureInfo = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        var configuration = builder.Configuration; // Use builder.Configuration directly
 
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not provided.");
        }
 
        // Add services to the container.
        builder.Services.AddControllersWithViews();
		builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        
        builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));
     
            
        builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = $"/Identity/Account/Login";
            options.LogoutPath = $"/Identity/Account/Logout";
            options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
        });

        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
        });

        var facebookAppId = configuration["Authentication:Facebook:AppId"] ?? Environment.GetEnvironmentVariable("AUTH_FACEBOOK_APP_ID");
        var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"] ?? Environment.GetEnvironmentVariable("AUTH_FACEBOOK_APP_SECRET");
        if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
        {
            authBuilder.AddFacebook(fb =>
            {
                fb.AppId = facebookAppId;
                fb.AppSecret = facebookAppSecret;
            });
        }

        var microsoftClientId = configuration["Authentication:Microsoft:ClientId"] ?? Environment.GetEnvironmentVariable("AUTH_MICROSOFT_CLIENT_ID");
        var microsoftClientSecret = configuration["Authentication:Microsoft:ClientSecret"] ?? Environment.GetEnvironmentVariable("AUTH_MICROSOFT_CLIENT_SECRET");
        if (!string.IsNullOrWhiteSpace(microsoftClientId) && !string.IsNullOrWhiteSpace(microsoftClientSecret))
        {
            authBuilder.AddMicrosoftAccount(ms =>
            {
                ms.ClientId = microsoftClientId;
                ms.ClientSecret = microsoftClientSecret;
            });
        }

        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(100);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;

        });

        builder.Services.AddScoped<IDbInitializer, DbInitializer>();
        builder.Services.AddRazorPages();

        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
		builder.Services.AddScoped<ICategoryService, CategoryService>();
		builder.Services.AddScoped<ICompanyService, CompanyService>();
		builder.Services.AddScoped<IOrderService, OrderService>();
		builder.Services.AddScoped<IProductService, BulkyBookWeb.Services.ProductService>();
		builder.Services.AddScoped<IUserService, UserService>();
		builder.Services.AddScoped<ICartService, CartService>();
		builder.Services.AddScoped<IHomeService, HomeService>();
		builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddHealthChecks();
		var app = builder.Build();

        var supportedCultures = new[] { "en-US" };
        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture(supportedCultures[0])
            .AddSupportedCultures(supportedCultures)
            .AddSupportedUICultures(supportedCultures);

        app.UseRequestLocalization(localizationOptions);

        app.MapHealthChecks("/health");
		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Home/Error");
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			app.UseHsts();
		}

		//app.UseHttpsRedirection();
		app.UseStaticFiles();
        StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
        app.UseRouting();
		app.UseAuthentication();
		app.UseAuthorization();
        app.UseSession();
        SeedDatabase();
		app.MapRazorPages();
		app.MapControllerRoute(
			name: "default",
			pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

		app.Run();


        void SeedDatabase()
        {
            const int maxRetries = 12;
            const int delaySeconds = 5;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                    dbInitializer.Initialize();
                    return;
                }
                catch when (attempt < maxRetries)
                {
                    Console.WriteLine($"Database initialization attempt {attempt} failed. Retrying in {delaySeconds} seconds...");
                    System.Threading.Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            throw new InvalidOperationException("Database initialization failed after multiple attempts.");
        }
    }
}