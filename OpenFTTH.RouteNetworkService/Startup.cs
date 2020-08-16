using System;
using System.Reflection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenFTTH.RouteNetworkService.Repositories;
using OpenFTTH.RouteNetworkService.Repositories.InMemoryImpl;

namespace OpenFTTH.RouteNetworkService
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

            // MediatR
            var routeNetworkService = AppDomain.CurrentDomain.Load("OpenFTTH.RouteNetworkService");
            services.AddMediatR(new Assembly[] { routeNetworkService });

            services.AddSingleton<IRouteNodeRepository, InMemRouteNodeRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
