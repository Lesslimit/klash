using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNet.Security.OAuth.Slack;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore;
using SimpleInjector.Integration.AspNetCore.Mvc;

namespace Klash.WebApp
{
    public class Startup
    {
        Container container = new Container();
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IControllerActivator>(new SimpleInjectorControllerActivator(container));
            services.AddSignalR(opts =>
            {
                opts.Hubs.EnableDetailedErrors = true;
            });

            services.AddOptions();
            services.AddAuthentication(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            services.Configure<SlackOptions>(Program.Configuration.GetSection("Slack"));
        }

        public void Configure(IApplicationBuilder app,
                              IHostingEnvironment env,
                              ILoggerFactory loggerFactory,
                              IOptions<SlackOptions> slackOptions)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSimpleInjectorAspNetRequestScoping(container);
            InitializeContainer(app);
            container.Verify();

            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseSignalR();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/signin"),
                LogoutPath = new PathString("/signout")
            });

            app.UseSlackAuthentication(new SlackAuthenticationOptions
            {
                ClientId = slackOptions.Value.ClientId,
                ClientSecret = slackOptions.Value.ClientSecret
            });

            app.UseMvc();
        }

        private void InitializeContainer(IApplicationBuilder app)
        {
            container.Options.DefaultScopedLifestyle = new AspNetRequestLifestyle();

            container.RegisterMvcControllers(app);
            container.Register<ILogger, CustomLogger>(Lifestyle.Singleton);
            container.RegisterSingleton(app.ApplicationServices.GetService<ILoggerFactory>());
        }
    }

    public class SlackOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
