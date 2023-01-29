using MVC_Store.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MVC_Store.CustomCookieAuth;

namespace MVC_Store
{
    public class Program
    {
        public static void Main(string[] args)
        {
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo("en-US");

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContextPool<Db>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Db"));
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/Account";
                options.EventsType = typeof(CustomCookieAuthenticationEvents);
            });

            builder.Services.AddScoped<CustomCookieAuthenticationEvents>();

            builder.Services.AddAuthorization();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDistributedMemoryCache();

            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
               name: "Admin",
               pattern: "{area:exists}/{controller}/{action}/{id?}");

            app.MapControllerRoute(
                name: "Pages",
                pattern: "{page}/{controller=Pages}/{action=Index}");

            app.MapControllerRoute(
                name: "Shop",
                pattern: "/{controller}/{action}/{name?}");

            app.MapControllerRoute(
                name: "Cart",
                pattern: "/{controller=Cart}/{action}/{id?}");

            app.MapControllerRoute(
                name: "Account",
                pattern: "/{controller=Account}/{action}/{id?}");

            app.MapControllerRoute(
                name: "Default",
                pattern: "{controller=Pages}/{action=Index}");

            app.Run();
        }
    }
}