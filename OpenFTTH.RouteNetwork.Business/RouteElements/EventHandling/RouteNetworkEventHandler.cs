using DAX.ObjectVersioning.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenFTTH.Events.Core;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.RouteNetwork.Business.RouteElements.Model;
using OpenFTTH.RouteNetwork.Business.RouteElements.StateHandling;
using System;
using System.Collections.Generic;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.EventHandling
{
    public class RouteNetworkEventHandler : IObserver<RouteNetworkEditOperationOccuredEvent>
    {
        private readonly ILogger<RouteNetworkEventHandler> _logger;

        private IRouteNetworkState _networkState;

        private HashSet<Guid> _alreadyProcessed = new HashSet<Guid>();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(RouteNetworkEditOperationOccuredEvent @event)
        {
            HandleEvent(@event);
        }

        public RouteNetworkEventHandler(ILoggerFactory loggerFactory, IRouteNetworkState networkState)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<RouteNetworkEventHandler>();

            _networkState = networkState;
        }

        public void HandleEvent(RouteNetworkEditOperationOccuredEvent request)
        {
            _logger.LogDebug("Got route network edit opreation occured message:");

            var trans = _networkState.GetTransaction();

            if (request.RouteNetworkCommands != null)
            {
                foreach (var command in request.RouteNetworkCommands)
                {
                    if (command.RouteNetworkEvents != null)
                    {
                        foreach (var routeNetworkEvent in command.RouteNetworkEvents)
                        {
                            switch (routeNetworkEvent)
                            {
                                case RouteNodeAdded domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case RouteNodeMarkedForDeletion domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case RouteSegmentAdded domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case RouteSegmentMarkedForDeletion domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case RouteSegmentRemoved domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case NamingInfoModified domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case LifecycleInfoModified domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case MappingInfoModified domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case SafetyInfoModified domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case RouteNodeInfoModified domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;

                                case RouteSegmentInfoModified domainEvent:
                                    HandleEvent(domainEvent, trans);
                                    break;
                            }
                        }
                    }
                }
            }

            _networkState.FinishWithTransaction();
        }


        private void HandleEvent(RouteNodeAdded request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            var routeNode = new RouteNode(request.NodeId, request.Geometry)
            {
                RouteNodeInfo = request.RouteNodeInfo,
                NamingInfo = request.NamingInfo,
                MappingInfo = request.MappingInfo,
                LifecycleInfo = request.LifecyleInfo,
                SafetyInfo = request.SafetyInfo
            };

            transaction.Add(routeNode, ignoreDublicates: true);
        }

        private void HandleEvent(RouteSegmentAdded request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;


            if (!(_networkState.GetRouteNetworkElement(request.FromNodeId) is RouteNode fromNode))
            {
                _logger.LogError($"Route network event stream seems to be broken! RouteSegmentAdded event with id: {request.EventId} and segment id: {request.SegmentId} has a FromNodeId: {request.FromNodeId} that don't exists in the current state.");
                return;
            }


            if (!(_networkState.GetRouteNetworkElement(request.ToNodeId) is RouteNode toNode))
            {
                _logger.LogError($"Route network event stream seems to be broken! RouteSegmentAdded event with id: {request.EventId} and segment id: {request.SegmentId} has a ToNodeId: {request.ToNodeId} that don't exists in the current state.");
                return;
            }

            var routeSegment = new RouteSegment(request.SegmentId, request.Geometry, fromNode, toNode)
            {
                RouteSegmentInfo = request.RouteSegmentInfo,
                NamingInfo = request.NamingInfo,
                MappingInfo = request.MappingInfo,
                LifecycleInfo = request.LifecyleInfo,
                SafetyInfo = request.SafetyInfo
            };

            transaction.Add(routeSegment, ignoreDublicates: true);
        }

        private void HandleEvent(RouteSegmentMarkedForDeletion request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            transaction.Delete(request.SegmentId, ignoreDublicates: true);
        }

        private void HandleEvent(RouteSegmentRemoved request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            transaction.Delete(request.SegmentId, ignoreDublicates: true);
        }


        private void HandleEvent(RouteNodeMarkedForDeletion request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            transaction.Delete(request.NodeId, ignoreDublicates: true);
        }

        private void HandleEvent(NamingInfoModified request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            if (_networkState.GetRouteNetworkElement(request.AggregateId) is IRouteNetworkElement existingRouteNetworkElement)
            {
                // We don't care about versioning af naming info, so we just modify the reference
                existingRouteNetworkElement.NamingInfo = request.NamingInfo;
            }
            else
            {
                _logger.LogWarning($"Could not lookup existing route network element by id: {request.AggregateId} processing event: {JsonConvert.SerializeObject(request)}");
            }
        }

        private void HandleEvent(LifecycleInfoModified request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            if (_networkState.GetRouteNetworkElement(request.AggregateId) is IRouteNetworkElement existingRouteNetworkElement)
            {
                // We don't care about versioning
                existingRouteNetworkElement.LifecycleInfo = request.LifecycleInfo;
            }
            else
            {
                _logger.LogWarning($"Could not lookup existing route network element by id: {request.AggregateId} processing event: {JsonConvert.SerializeObject(request)}");
            }
        }

        private void HandleEvent(MappingInfoModified request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            if (_networkState.GetRouteNetworkElement(request.AggregateId) is IRouteNetworkElement existingRouteNetworkElement)
            {
                // We don't care about versioning
                existingRouteNetworkElement.MappingInfo = request.MappingInfo;
            }
            else
            {
                _logger.LogWarning($"Could not lookup existing route network element by id: {request.AggregateId} processing event: {JsonConvert.SerializeObject(request)}");
            }
        }

        private void HandleEvent(SafetyInfoModified request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            if (_networkState.GetRouteNetworkElement(request.AggregateId) is IRouteNetworkElement existingRouteNetworkElement)
            {
                // We don't care about versioning
                existingRouteNetworkElement.SafetyInfo = request.SafetyInfo;
            }
            else
            {
                _logger.LogWarning($"Could not lookup existing route network element by id: {request.AggregateId} processing event: {JsonConvert.SerializeObject(request)}");
            }
        }

        private void HandleEvent(RouteNodeInfoModified request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            if (_networkState.GetRouteNetworkElement(request.NodeId) is IRouteNode existingRouteNode)
            {
                // We don't care about versioning here
                existingRouteNode.RouteNodeInfo = request.RouteNodeInfo;
            }
            else
            {
                _logger.LogWarning($"Could not lookup existing route node by id: {request.NodeId} processing event: {JsonConvert.SerializeObject(request)}");
            }
        }

        private void HandleEvent(RouteSegmentInfoModified request, ITransaction transaction)
        {
            _logger.LogDebug($"Handler got {request.GetType().Name} event seq no: {request.EventSequenceNumber}");

            if (AlreadyProcessed(request.EventId))
                return;

            if (_networkState.GetRouteNetworkElement(request.SegmentId) is IRouteSegment existingRouteSegment)
            {
                // We don't care about versioning here
                existingRouteSegment.RouteSegmentInfo = request.RouteSegmentInfo;
            }
            else
            {
                _logger.LogWarning($"Could not lookup existing route segment by id: {request.SegmentId} processing event: {JsonConvert.SerializeObject(request)}");
            }
        }

        private bool AlreadyProcessed(Guid id)
        {
            if (_alreadyProcessed.Contains(id))
                return true;
            else
            {
                _alreadyProcessed.Add(id);
                return false;
            }
        }
    }
}
