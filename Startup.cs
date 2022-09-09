using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IM.Common;
using IM.Hubs;
using IM.SQL;
using IM_Core.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using WebApi.Options;

namespace IM_Core
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
            services.AddControllers();

            services.AddOptions<ConnectionStringOptions>()
               .Bind(Configuration.GetSection(ConnectionStringOptions.ConnectionStringName))
               .ValidateDataAnnotations();


            services.AddSignalR();

            services.AddScoped(typeof(DataAccessMethods));
            services.AddScoped(typeof(UsersMethods));
            services.AddScoped(typeof(IncidentsMethods));
            services.AddScoped(typeof(MessagesMethods));
            services.AddScoped<IEmailService, EmailService>();

            services.AddMvc(setupAction => {
                setupAction.EnableEndpointRouting = false;
            }).AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
            })
             .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

       

            services.AddCors(options =>
            {
                options.AddPolicy("ClientPermission", policy =>
                {
                    policy.AllowAnyHeader()
                        .AllowAnyMethod()
                        //.AllowAnyOrigin()
                        .WithOrigins(
                            "https://localhost:44338", 
                            "http://localhost:3000",
                            "http://localhost:4200",
                            "https://lively-bush-0d9b77d10.1.azurestaticapps.net",
                            "http://localhost/ImAngular",
                            "https://calm-mud-02aada210.1.azurestaticapps.net",
                            "https://localhost:7135", 
                            "https://salmon-bay-0ee5f3310.1.azurestaticapps.net",
                            "https://incidentbyid.azurewebsites.net"
                            )
                        .AllowCredentials();
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

           // app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseAuthorization();

            // global cors policy
            //app.UseCors(x => x
            //    .AllowAnyOrigin()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader());
            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseCors("ClientPermission");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Users}/{action=Index}/{id?}");
                endpoints.MapHub<NotificationHub>("/hubs/notifications");              
            });
        }
    }
}
