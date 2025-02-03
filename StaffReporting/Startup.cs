using Management.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

namespace WorkManagementSystem
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Authentication configuration
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(options =>
                    {
                        options.LoginPath = "/Account/Login";
                        options.AccessDeniedPath = "/Account/Login";
                        options.ExpireTimeSpan = TimeSpan.FromHours(10); // 10-hour timeout
                        options.SlidingExpiration = true; // Renew the cookie on each request if it is near expiration
                    });

            // Database connection configuration
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Controller and view configuration
            services.AddControllersWithViews();

            // Configure FormOptions for large file uploads
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 2147483648; // 2 GB
            });

            // Configure Kestrel limits (optional if also configured in appsettings.json)
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 2147483648; // 2 GB
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Enable HTTPS redirection
            app.UseHttpsRedirection();

            // Serve static files
            app.UseStaticFiles();

            // Enable routing
            app.UseRouting();

            // Middleware to set max request size dynamically
            app.Use(async (context, next) =>
            {
                var maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (maxRequestBodySizeFeature != null)
                {
                    maxRequestBodySizeFeature.MaxRequestBodySize = 2147483648; // 2 GB
                }
                await next();
            });

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Configure endpoint routing
            app.UseEndpoints(endpoints =>
            {
                // Area-specific routing
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Work}/{action=Index}/{id?}");

                // Default routing
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
        }
    }
}