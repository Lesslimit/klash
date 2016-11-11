using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
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
            services.AddAuthentication(opts =>
            {
                opts.SignInScheme = "Cookies";
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

            app.UseMvc();

            app.UseSimpleInjectorAspNetRequestScoping(container);
            InitializeContainer(app);
            container.Verify();

            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseSignalR();

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/login"),
                LogoutPath = new PathString("/logout")
            });

            app.UseOAuthAuthentication(new OAuthOptions
            {
                AuthenticationScheme = "Slack",
                ClientId = slackOptions.Value.ClientId,
                ClientSecret = slackOptions.Value.ClientSecret,
                CallbackPath = new PathString("/slack-auth"),
                AuthorizationEndpoint = "https://slack.com/oauth/authorize",
                TokenEndpoint = "https://slack.com/api/oauth.access",
                UserInformationEndpoint = "https://slack.com/api/users.identity?token=",
                Scope = { "identity.basic" },
                Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint + context.AccessToken);
                        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);

                        response.EnsureSuccessStatusCode();

                        var userObject = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var user = userObject.SelectToken("user");
                        var userId = user.Value<string>("id");

                        if (!string.IsNullOrEmpty(userId))
                        {
                            context.Identity.AddClaim(
                                new Claim(ClaimTypes.NameIdentifier, userId, ClaimValueTypes.String, context.Options.ClaimsIssuer)
                                );
                        }

                        var fullName = user.Value<string>("name");

                        if (!string.IsNullOrEmpty(fullName))
                        {
                            context.Identity.AddClaim(new Claim(ClaimTypes.Name, fullName, ClaimValueTypes.String, context.Options.ClaimsIssuer));
                        }
                    }
                }
            });

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello Mazafaka!");
            });
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
