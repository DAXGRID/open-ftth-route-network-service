﻿using Microsoft.Extensions.DependencyInjection;
using OpenFTTH.CQRS;
using OpenFTTH.EventSourcing;
using OpenFTTH.EventSourcing.InMem;
using System;
using System.Reflection;

namespace OpenFTTH.RouteNetworkService.Tests
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventStore, InMemEventStore>();

            services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
            services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

            var businessAssemblies = new Assembly[] { AppDomain.CurrentDomain.Load("OpenFTTH.RouteNetwork.Business") };

            services.AddCQRS(businessAssemblies);

            services.AddProjections(businessAssemblies);
        }
    }
}
