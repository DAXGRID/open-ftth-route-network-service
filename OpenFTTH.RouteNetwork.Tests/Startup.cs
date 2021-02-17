using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OpenFTTH.CQRS;
using OpenFTTH.EventSourcing;
using OpenFTTH.EventSourcing.InMem;
using OpenFTTH.RouteNetwork.Business.StateHandling;
using OpenFTTH.RouteNetwork.Business.StateHandling.Interest;
using OpenFTTH.RouteNetwork.Business.StateHandling.Network;
using OpenFTTH.RouteNetwork.Tests.Fixtures;
using System;
using System.Reflection;

namespace OpenFTTH.RouteNetworkService.Tests
{
    public class Startup
    {
        private ServiceProvider _serviceProvider;
        public void ConfigureServices(IServiceCollection services)
        {
            // Route Network State and Repository
            services.AddSingleton<IRouteNetworkState, InMemRouteNetworkState>();
            services.AddSingleton<IRouteNetworkRepository, InMemRouteNetworkRepository>();
            services.AddSingleton<IInterestRepository, MemoryMappedInterestRepository>();
            

            // ES and CQRS
            services.AddSingleton<IEventStore, InMemEventStore>();

            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

            var businessAssemblies = new Assembly[] { AppDomain.CurrentDomain.Load("OpenFTTH.RouteNetwork.Business") };

            services.AddCQRS(businessAssemblies);

            services.AddProjections(businessAssemblies);


            // Test Route Network Data
            services.AddSingleton<ITestRouteNetworkData, TestRouteNetwork>();
            
        }
    }
}
