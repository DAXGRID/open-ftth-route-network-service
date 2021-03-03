﻿using FluentResults;
using OpenFTTH.CQRS;
using OpenFTTH.RouteNetwork.API.Model;
using System;

namespace OpenFTTH.RouteNetwork.API.Commands
{

    /// <summary>
    /// Used to register interest in a node in the route network.
    /// An typical usage is to register some equipment put into a route node - such as a well, cabinet etc.
    /// </summary>
    public class RegisterNodeOfInterest : ICommand<Result<RouteNetworkInterest>>
    {
        public static string RequestName => typeof(RegisterNodeOfInterest).Name;
        public Guid InterestId { get; }
        public Guid RouteNetworkElementId { get; }
        public string? CustomData { get; }

        public RegisterNodeOfInterest(Guid interestId, Guid routeNetworkElementId, string? customData = null)
        {
            this.InterestId = interestId;
            this.RouteNetworkElementId = routeNetworkElementId;
            this.CustomData = customData;
        }
    }
}