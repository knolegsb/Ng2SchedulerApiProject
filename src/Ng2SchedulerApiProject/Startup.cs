using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ng2Scheduler.Data;
using Newtonsoft.Json.Serialization;
using Ng2Scheduler.Data.Abstract;
using Ng2Scheduler.Data.Repositories;
using Ng2SchedulerApiProject.ViewModels.Mappings;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Ng2SchedulerApiProject.Extensions;

namespace Ng2SchedulerApiProject
{
    public class Startup
    {
        private static string _applicationPath = string.Empty;
        string sqlConnectionString = string.Empty;
        bool useInMemoryProvider = false;
        //public IConfigurationRoot configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            _applicationPath = env.WebRootPath;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            //if (env.IsDevelopment())
            //{
            //    builder.AddUserSecrets<Startup>();
            //}
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string sqlConnectionString = Configuration.GetConnectionString("DefaultConnection");
            try
            {
                useInMemoryProvider = bool.Parse(Configuration["AppSettings:InMemoryProvider"]);
            }
            catch { }

            services.AddDbContext<SchedulerContext>(options =>
            {
                switch (useInMemoryProvider)
                {
                    case true:
                        options.UseInMemoryDatabase();
                        break;
                    default:
                        options.UseSqlServer(sqlConnectionString, b => b.MigrationsAssembly("Ng2SchedulerApiProject"));
                        break;
                }
            });

            services.AddScoped<IScheduleRepository, ScheduleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAttendeeRepository, AttendeeRepository>();

            AutoMapperConfiguration.Configure();

            // Enable Cors
            services.AddCors();

            // Add framework services.
            services.AddMvc()
                .AddJsonOptions(opts => {
                    // Force Camel Case to JSON
                    opts.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseStaticFiles();

            app.UseCors(builder => builder.AllowAnyOrigin()
                                            .AllowAnyHeader()
                                            .AllowAnyMethod());

            app.UseExceptionHandler(
                builder =>
            {
                builder.Run(
                    async context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");

                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            context.Response.AddApplicationError(error.Error.Message);
                            await context.Response.WriteAsync(error.Error.Message).ConfigureAwait(false);
                        }
                    });
            });

            app.UseMvc(routes => {
            routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");                
            });

            SchedulerDbInitializer.Initialize(app.ApplicationServices);
        }
    }
}
