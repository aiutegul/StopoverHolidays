﻿using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using StopoverAdminPanel.Auth;
using System;
using System.Web.Http;

[assembly: OwinStartup(typeof(StopoverAdminPanel.Startup))]

namespace StopoverAdminPanel
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // app.UseCors(CorsOptions.AllowAll);

            HttpConfiguration config = new HttpConfiguration();
            ConfigureOAuth(app);
            WebApiConfig.Register(config);
            app.UseWebApi(config);
            
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/Account/login"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(60),
                Provider = new SimpleAuthorizationServerProvider()
            };
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }
}