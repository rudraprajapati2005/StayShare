using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StayShare.Data;
using StayShare.Repositories;
using System;
namespace StayShare
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
            // Add MVC support
            services.AddControllersWithViews();

            // Add EF Core DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Register repositories
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOccupancyRepository, OccupancyRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";       // where to redirect if not logged in
                    options.LogoutPath = "/Auth/Logout";     // logout URL
                    options.AccessDeniedPath = "/Auth/AccessDenied"; // optional
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                });

            services.AddAuthorization();
            // Optional: add session or caching if required later
            // services.AddDistributedMemoryCache();
            // services.AddSession();
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Optional: if you add authentication/authorization later
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "room_residents",
                    pattern: "Room/Residents/{id}",
                    defaults: new { controller = "Room", action = "Residents" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
