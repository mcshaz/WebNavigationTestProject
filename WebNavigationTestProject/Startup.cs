﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using cloudscribe.Web.Navigation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using WebNavigationTestProject.AuthorizationHandlers;
using Microsoft.AspNetCore.Authorization;
using cloudscribe.Web.SiteMap;

namespace WebNavigationTestProject
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<ISiteMapNodeService, NavigationTreeSiteMapNodeService>();

            var customAppModelProvider = new CustomApplicationModelProvider();
            services.AddSingleton<IApplicationModelProvider>(customAppModelProvider);
            services.AddSingleton<IActionFilterMap>(customAppModelProvider);
            //our autopermission resolver must be added before call to AddCloudscribeNavigation
            services.AddScoped<INavigationNodePermissionResolver, NavigationNodeAutoPermissionResolver>();
            services.AddCloudscribeNavigation(Configuration.GetSection("NavigationOptions"));

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.Configure<MvcOptions>(options =>
            {
                // options.InputFormatters.Add(new Xm)
                options.CacheProfiles.Add("SiteMapCacheProfile",
                     new CacheProfile
                     {
                         Duration = 100
                     });

            });

            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization()
                .AddRazorOptions(options =>
                {
                    // if you download the cloudscribe.Web.Navigation Views and put them in your views folder
                    // then you don't need this line and can customize the views (recommended)
                    // you can find them here:
                    // https://github.com/joeaudette/cloudscribe.Web.Navigation/tree/master/src/cloudscribe.Web.Navigation/Views
                    options.AddCloudscribeNavigationBootstrap3Views();
                });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "application";
                options.DefaultChallengeScheme = "application";
            })
                .AddCookie("application", options =>
                {
                    options.LoginPath = new PathString("/FakeAccount/Index");

                });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                     new CultureInfo("en-US"),
                     new CultureInfo("es-ES"),
                     new CultureInfo("fr-FR"),
                     new CultureInfo("de-DE"),
                     new CultureInfo("hi-IN"),  //Hindi
                     new CultureInfo("ru-RU"),
                     new CultureInfo("zh-Hans"), //Chinese Simplified
                     new CultureInfo("zh-Hant"), //Chinese Traditional
                };

                // State what the default culture for your application is. This will be used if no specific culture
                // can be determined for a given request.
                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;

                // You can change which providers are configured to determine the culture for requests, or even add a custom
                // provider with your own logic. The providers will be asked in order to provide a culture for each request,
                // and the first to provide a non-null result that is in the configured supported cultures list will be used.
                // By default, the following built-in providers are configured:
                // - QueryStringRequestCultureProvider, sets culture via "culture" and "ui-culture" query string values, useful for testing
                // - CookieRequestCultureProvider, sets culture via "ASPNET_CULTURE" cookie
                // - AcceptLanguageHeaderRequestCultureProvider, sets culture via the "Accept-Language" request header
                //options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                //{
                //  // My custom request culture logic
                //  return new ProviderCultureResult("en");
                //}));
            });

            services.AddAuthorization(options =>
            {

                options.AddPolicy(
                    "AdminsPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Admins");
                    });

                options.AddPolicy(
                    "MembersPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Admins", "Members");
                    });

                options.AddPolicy("Over21",
                    policy => policy.Requirements.Add(new MinimumAgeRequirement(21)));

                options.AddPolicy("EmployeesOnly", policy => policy.RequireClaim("EmployeeId"));

            });

            services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();


        }




        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/localization
            //https://msdn.microsoft.com/en-us/library/ee825488(v=cs.20).aspx

            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute("areaRoute", "{area:exists}/{controller=Roswell}/{action=Index}/{id?}");


                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}