﻿using DAX.ObjectVersioning.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.RouteNetwork.Business.EventHandling;
using OpenFTTH.RouteNetwork.Service.Business.DomainModel.RouteNetwork;
using System;
using System.Collections.Generic;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.InMemory
{
    /// <summary>
    /// Object to hold on to all the versioned object state
    /// </summary>
    public class InMemRouteNetworkState : IRouteNetworkState
    {
        private ILoggerFactory _loggerFactory;
        private readonly ILogger<InMemRouteNetworkState> _logger;

        private InMemoryObjectManager _objectManager = new InMemoryObjectManager();
        
        private bool _loadMode = true;
        private ITransaction? _loadModeTransaction;
        private ITransaction? _cmdTransaction;
        private DateTime __lastEventRecievedTimestamp = DateTime.UtcNow;
        private long _numberOfObjectsLoaded = 0;

        public InMemoryObjectManager ObjectManager => _objectManager;
        public DateTime LastEventRecievedTimestamp => __lastEventRecievedTimestamp;
        public long NumberOfObjectsLoaded => _numberOfObjectsLoaded;

        public InMemRouteNetworkState(ILoggerFactory loggerFactory)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory cannot be null");
            }

            _loggerFactory = loggerFactory;

            _logger = loggerFactory.CreateLogger<InMemRouteNetworkState>();
        }

        /// <summary>
        /// Use this method to seed the in memory state with route network json data
        /// </summary>
        public void SeedRouteNetworkEvents(string json)
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                settings.Converters.Add(new StringEnumConverter());
                settings.TypeNameHandling = TypeNameHandling.Auto;
                return settings;
            });

            var editOperationEvents = JsonConvert.DeserializeObject<List<RouteNetworkEditOperationOccuredEvent>>(json);

            var routeNetworkEventHandler = new RouteNetworkEventHandler(_loggerFactory, this);

            foreach (var editOperationEvent in editOperationEvents)
                routeNetworkEventHandler.HandleEvent(editOperationEvent);
        }

        public ITransaction GetTransaction()
        {
            if (_loadMode)
                return GetLoadModeTransaction();
            else
                return GetCommandTransaction();
        }

        public void FinishWithTransaction()
        {
            __lastEventRecievedTimestamp = DateTime.UtcNow;
            _numberOfObjectsLoaded++;

            // We're our of load mode, and dealing with last event
            if (!_loadMode && _loadModeTransaction == null)
            {
                // Commit the command transaction
                if (_cmdTransaction != null)
                {
                    _cmdTransaction.Commit();
                    _cmdTransaction = null;
                }
            }
        }

        public IRouteNetworkElement? GetRouteNetworkElement(Guid id)
        {
            if (_loadMode && _loadModeTransaction != null)
                return _loadModeTransaction.GetObject(id) as IRouteNetworkElement;
            else if (_cmdTransaction != null)
            {
                var transObj = _cmdTransaction.GetObject(id);

                if (transObj != null)
                    return transObj as IRouteNetworkElement;
                else
                    return _objectManager.GetObject(id) as IRouteNetworkElement;
            }
            else
                return null;
        }

        private ITransaction GetLoadModeTransaction()
        {
            if (_loadModeTransaction == null)
                _loadModeTransaction = _objectManager.CreateTransaction();

            return _loadModeTransaction;
        }

        private ITransaction GetCommandTransaction()
        {
            if (_cmdTransaction == null)
                _cmdTransaction = _objectManager.CreateTransaction();

            return _cmdTransaction;
        }
    }
}
