﻿using AutoMapper;
using EP.Balda.Logic.Commands;
using EP.Balda.Logic.Profiles;
using EP.Balda.Logic.Services;
using EP.Balda.Web.Constants;
using EP.Balda.Web.Filters;
using EP.Balda.Web.Validators;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using System.Collections.Generic;
using System.Text;

namespace EP.Balda.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthenticationCore();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .AddIdentityServerAuthentication(JwtBearerDefaults.AuthenticationScheme, opt =>
                {
                    opt.Authority = "http://localhost:5000";
                    opt.RequireHttpsMetadata = false;
                })
                .AddGoogle("Google", opt =>
                 {
                     var googleAuthNSection =
                         Configuration.GetSection("Authentication:Google");

                     opt.ClientId = googleAuthNSection["ClientId"];
                     opt.ClientSecret = googleAuthNSection["ClientSecret"];
                     opt.CallbackPath = new PathString("/api/google");
                 })
                .AddFacebook("Facebook", opt =>
                {
                    var facebookAuthNSection =
                        Configuration.GetSection("Authentication:Facebook");

                    opt.ClientId = facebookAuthNSection["ClientId"];
                    opt.ClientSecret = facebookAuthNSection["ClientSecret"];
                    opt.CallbackPath = new PathString("/api/facebook");
                });

            services.AddAuthorization(opt =>
            {
                opt.AddPolicy("google",
                      cfg => cfg.AddAuthenticationSchemes("Google")
                          .RequireAuthenticatedUser());
                opt.AddPolicy("facebook",
                    cfg => cfg.AddAuthenticationSchemes("Facebook")
                    .RequireAuthenticatedUser());
            });

            services.AddSession();

            services.AddSwaggerDocument(cfg =>
            {
                cfg.SchemaType = SchemaType.OpenApi3;
                cfg.AddSecurity("oauth", new[] { "baldagame_api" }, new OpenApiSecurityScheme()
                {
                    Flow = OpenApiOAuth2Flow.Implicit,
                    Type = NSwag.OpenApiSecuritySchemeType.OAuth2,
                    AuthorizationUrl = "http://localhost:5000/connect/authorize",
                    Scopes = new Dictionary<string, string>()
                    {
                        { "baldagame_api", "Access to Balda Game api" }
                    }
                });
            });

            services.AddAutoMapper(typeof(PlayerProfile).Assembly);
            services.AddMediatR(typeof(CreateNewGameCommand).Assembly);
            
            services.AddBaldaGameServices();

            services.AddCors();
            services.AddMvc(opt => opt.Filters.Add(typeof(ModelValidationFilter)))
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddFluentValidation(cfg =>
            {
                cfg.RegisterValidatorsFromAssemblyContaining<UserLoginValidator>();
                cfg.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
            }); ;
        }

        // This method gets called by the runtime. Use this method to
        // configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
                              IMediator mediator)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                // The default HSTS value is 30 days. You may want to change this
                // for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

            app.UseCors(opt =>
                opt.AllowAnyHeader()
                .AllowAnyMethod()
                .WithOrigins("http://localhost:4200")
                .AllowCredentials());

            app.UseAuthentication();

            mediator.Send(new CreateDatabaseCommand()).Wait();

            app.UseOpenApi().UseSwaggerUi3(opt => opt.OAuth2Client = new OAuth2ClientSettings()
            {
                AppName = "Balda Game",
                ClientId = "swagger"
            });

            app.UseMvc();

        }
    }
}