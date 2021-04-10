using DAX.ObjectVersioning.Core;
using FluentResults;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenFTTH.CQRS;
using OpenFTTH.Events.Core;
using OpenFTTH.Events.RouteNetwork;
using OpenFTTH.EventSourcing;
using OpenFTTH.RouteNetwork.API.Commands;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.API.Queries;
using OpenFTTH.RouteNetwork.Business.Interest.Projections;
using OpenFTTH.RouteNetwork.Business.RouteElements.Model;
using OpenFTTH.RouteNetwork.Business.RouteElements.StateHandling;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenFTTH.RouteNetwork.Business.RouteElements.EventHandling
{
    public class RouteNetworkEventHandler : IObserver<RouteNetworkEditOperationOccuredEvent>
    {
        private readonly ILogger<RouteNetworkEventHandler> _logger;
        private readonly IRouteNetworkState _networkState;
        private readonly IEventStore _eventStore;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;

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

        public RouteNetworkEventHandler(ILoggerFactory loggerFactory, IRouteNetworkState networkState, IEventStore eventStore, ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
        {
            if (null == loggerFactory)
            {
                throw new ArgumentNullException("loggerFactory is null");
            }

            _logger = loggerFactory.CreateLogger<RouteNetworkEventHandler>();

            _networkState = networkState;
            _eventStore = eventStore;
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
        }

        public void HandleEvent(RouteNetworkEditOperationOccuredEvent request)
        {
            _logger.LogDebug("Got route network edit opreation occured message:");

            if (request.RouteNetworkCommands != null)
            {
                foreach (var command in request.RouteNetworkCommands)
                {
                    if (command.RouteNetworkEvents != null)
                    {
                        var trans = _networkState.GetTransaction();

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

                        _networkState.FinishWithTransaction();

                        // Process eventually splits that might requires updates to route network interests
                        if (command.CmdType == "ExistingRouteSegmentSplitted")
                        {
                            HandleSplitCommand(command, trans);
                        }
                    }
                }
            }
        }

        private void HandleSplitCommand(Events.RouteNetworkCommand command, ITransaction trans)
        {
            if (!_networkState.IsLoadMode)
            {
                RouteNodeAdded? routeNodeAddedEvent = null;
                RouteSegmentAdded? fromAddedSegmentEvent = null;
                RouteSegmentAdded? toAddedSegmentEvent = null;
                RouteSegmentRemoved? removedSegmentEvent = null;

                foreach (var routeNetworkEvent in command.RouteNetworkEvents)
                {
                    if (routeNetworkEvent is RouteNodeAdded)
                        routeNodeAddedEvent = routeNetworkEvent as RouteNodeAdded;
                    else if (routeNetworkEvent is RouteSegmentAdded && routeNodeAddedEvent != null && ((RouteSegmentAdded)routeNetworkEvent).FromNodeId == routeNodeAddedEvent.NodeId)
                        fromAddedSegmentEvent = routeNetworkEvent as RouteSegmentAdded;
                    else if (routeNetworkEvent is RouteSegmentAdded && routeNodeAddedEvent != null && ((RouteSegmentAdded)routeNetworkEvent).ToNodeId == routeNodeAddedEvent.NodeId)
                        toAddedSegmentEvent = routeNetworkEvent as RouteSegmentAdded;
                    else if (routeNetworkEvent is RouteSegmentRemoved)
                        removedSegmentEvent = routeNetworkEvent as RouteSegmentRemoved;
                }

                // Only proceed if we manage to get all the needed information from the split command
                if (routeNodeAddedEvent != null && fromAddedSegmentEvent != null && toAddedSegmentEvent != null && removedSegmentEvent != null)
                {
                    // Find all interests of the deleted route segment
                    var interestsProjection = _eventStore.Projections.Get<InterestsProjection>();

                    var interestRelationsResult = interestsProjection.GetInterestsByRouteNetworkElementId(removedSegmentEvent.SegmentId);

                    if (interestRelationsResult.IsFailed)
                    { 
                        _logger.LogError($"Split handler: Failed with error: {interestRelationsResult.Errors.First().Message} trying to get interest related by removed segment with id: {removedSegmentEvent.SegmentId} processing split command: " + JsonConvert.SerializeObject(command));
                        return;
                    }

                    foreach (var interest in interestRelationsResult.Value)
                    {
                        var newRouteNetworkElementIdList = CreateNewRouteNetworkElementIdListFromSplit(interest.Item1.RouteNetworkElementRefs, removedSegmentEvent.SegmentId, routeNodeAddedEvent.NodeId, fromAddedSegmentEvent, toAddedSegmentEvent);

                        var updateWalkOfInterestCommand = new UpdateWalkOfInterest(interest.Item1.Id, newRouteNetworkElementIdList);
                        var updateWalkOfInterestCommandResult = _commandDispatcher.HandleAsync<UpdateWalkOfInterest, Result<RouteNetworkInterest>>(updateWalkOfInterestCommand).Result;

                        if (updateWalkOfInterestCommandResult.IsFailed)
                        {
                            _logger.LogError($"Split handler: Failed error: {updateWalkOfInterestCommandResult.Errors.First().Message} trying to update interest with id: {interest.Item1.Id} processing split command: " + JsonConvert.SerializeObject(command));
                        }
                    }
                }
                else
                {
                    _logger.LogError("Split handler: can't find needed information in event: " + JsonConvert.SerializeObject(command));
                }

            }
        }

        private RouteNetworkElementIdList CreateNewRouteNetworkElementIdListFromSplit(RouteNetworkElementIdList existingRouteNetworkElementIds, Guid removedSegmentId, Guid newNodeId, RouteSegmentAdded newFromSegmentEvent, RouteSegmentAdded newToSegmentEvent)
        {
            RouteNetworkElementIdList result = new RouteNetworkElementIdList();

            for (int i = 0; i < existingRouteNetworkElementIds.Count; i++)
            {
                var existingId = existingRouteNetworkElementIds[i]; 

                if (existingId == removedSegmentId)
                {
                    // Check if the from segment is the one connected to the from node of the removed segment in the walk
                    if (newFromSegmentEvent.FromNodeId == existingRouteNetworkElementIds[i - 1] || newFromSegmentEvent.ToNodeId == existingRouteNetworkElementIds[i - 1])
                    {
                        // The from segment is comming first
                        result.Add(newFromSegmentEvent.SegmentId);
                        result.Add(newNodeId);
                        result.Add(newToSegmentEvent.SegmentId);
                    }
                    else
                    {
                        // The to segment is comming first
                        result.Add(newToSegmentEvent.SegmentId);
                        result.Add(newNodeId);
                        result.Add(newFromSegmentEvent.SegmentId);
                    }
                }
                else
                    result.Add(existingId);
            }

            var walk = new ValidatedRouteNetworkWalk(result);
            RouteNetworkElementIdList segmentsOnly = new();
            segmentsOnly.AddRange(walk.SegmentIds);
            return segmentsOnly;
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
