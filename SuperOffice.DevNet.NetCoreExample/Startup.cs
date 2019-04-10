using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperOffice.DevNet.NetCoreExample.Data;
using SuperOffice.DevNet.NetCoreExample.Models;
using SuperOffice.DevNet.NetCoreExample.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SuperOffice.DevNet.NetCoreExample
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowCredentials();
                    //.WithMethods("login", "tokens");
                });
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect("SuperOffice", "SuperOffice Online", options =>
               {
                   options.ClientId = Configuration["Authentication:SuperOffice:ClientId"];
                   options.ClientSecret = Configuration["Authentication:SuperOffice:ClientSecret"];
                   options.Authority = "https://sod.superoffice.com/login";
                   options.SaveTokens = true;
                   options.Scope.Add("openid");
                   options.CallbackPath = new Microsoft.AspNetCore.Http.PathString("/callback");
                   options.GetClaimsFromUserInfoEndpoint = false;
                   options.ResponseMode = "form_post";
                   options.ResponseType = "code id_token";
               });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // place in: public void Configure(IApplicationBuilder app, IHostingEnvironment env)
            app.UseCors("CorsPolicy");


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseCors(builder =>
            {
                builder
                .WithOrigins("https://sod.superoffice.com", "https://qastage.superoffice.com", "https://online.superoffice.com")
                .AllowAnyMethod();
            });
        }
    }
}
