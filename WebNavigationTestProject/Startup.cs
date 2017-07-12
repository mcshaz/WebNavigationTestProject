using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using cloudscribe.Web.Navigation;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Authorization;
using WebNavigationTestProject.AuthorizationHandlers;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization;

namespace WebNavigationTestProject
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //create a single instance and have the same instance injected into both interfaces it implements
            var customAppModelProvider = new CustomApplicationModelProvider();
            services.AddSingleton<IApplicationModelProvider>(customAppModelProvider);
            services.AddSingleton<IActionFilterMap>(customAppModelProvider);
            //our autopermission resolver must be added before call to AddCloudscribeNavigation
            services.AddScoped<INavigationNodePermissionResolver, NavigationNodeAutoPermissionResolver>();
            services.AddCloudscribeNavigation(Configuration.GetSection("NavigationOptions"));

            // Add framework services.
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
            
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Over21",
                                    policy => policy.Requirements.Add(new MinimumAgeRequirement(21)));

                options.AddPolicy("EmployeesOnly", policy => policy.RequireClaim("EmployeeId"));
            });

            services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                     new CultureInfo("en-US"),
                     new CultureInfo("en-GB"),
                     new CultureInfo("en-AU"),
                     new CultureInfo("en-NZ"),
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            app.UseStaticFiles();

            var ApplicationCookie = new CookieAuthenticationOptions
            {
                AuthenticationScheme = "application",
                CookieName = "application",
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/FakeAccount/Index"),
                Events = new CookieAuthenticationEvents
                {
                    //OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                }
            };

            app.UseCookieAuthentication(ApplicationCookie);

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
