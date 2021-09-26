using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocelot.Cache.CacheManager;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Ocelot.Authorisation;

namespace BookShareOnline.APIGateway
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
            var secret = Configuration["JWT:Secret"];
            //var secret = "Thisismytestprivatekey";
            var key = Encoding.ASCII.GetBytes(secret);
            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            services.AddOcelot().AddCacheManager(settings => settings.WithDictionaryHandle());
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:4200");
                    builder.AllowAnyMethod();
                    builder.AllowAnyHeader();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });

            app.UseOcelot().Wait();
            #region commented
            //var configuration = new OcelotPipelineConfiguration
            //{
            //    AuthorisationMiddleware = async (ctx, next) =>
            //    {
            //        if (this.Authorize(ctx))
            //        {
            //            await next.Invoke();

            //        }
            //        else
            //        {
            //            ctx.Errors.Add(new UnauthorisedError($"Fail to authorize"));
            //        }

            //    }
            //};

            //app.UseOcelot(configuration).Wait();
            #endregion
        }
        //private bool Authorize(DownstreamContext ctx)
        //{
        //    if (ctx.DownstreamReRoute.AuthenticationOptions.AuthenticationProviderKey == null) return true;
        //    else
        //    {
        //        //flag for authorization
        //        bool auth = false;

        //        //where are stored the claims of the jwt token
        //        Claim[] claims = ctx.HttpContext.User.Claims.ToArray<Claim>();

        //        //where are stored the required claims for the route
        //        Dictionary<string, string> required = ctx.DownstreamReRoute.RouteClaimsRequirement;

        //        //((AUTHORIZATION LOGIC))
        //        Regex reor = new Regex(@"[^,\s+$ ][^\,]*[^,\s+$ ]");
        //        MatchCollection matches;

        //        Regex reand = new Regex(@"[^&\s+$ ][^\&]*[^&\s+$ ]");
        //        MatchCollection matchesand;
        //        int cont = 0;
        //        foreach (KeyValuePair<string, string> claim in required)
        //        {
        //            matches = reor.Matches(claim.Value);
        //            foreach (Match match in matches)
        //            {
        //                matchesand = reand.Matches(match.Value);
        //                cont = 0;
        //                foreach (Match m in matchesand)
        //                {
        //                    foreach (Claim cl in claims)
        //                    {
        //                        if (cl.Type == claim.Key)
        //                        {
        //                            if (cl.Value == m.Value)
        //                            {
        //                                cont++;
        //                            }
        //                        }
        //                    }
        //                }
        //                if (cont == matchesand.Count)
        //                {
        //                    auth = true;
        //                    break;
        //                }
        //            }
        //        }
        //        return auth;
        //    }
        //}
    }
}