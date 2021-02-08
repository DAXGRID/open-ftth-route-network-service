using CSharpFunctionalExtensions;
using Marten;
using Microsoft.Extensions.Options;
using OpenFTTH.RouteNetwork.API.Model;
using OpenFTTH.RouteNetwork.Business.DomainModel.Interest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenFTTH.RouteNetwork.Business.StateHandling.Interest
{
    /// <summary>
    /// This interest repository keeps everything cached in memory for maximum performance, while persisting changes 
    /// to a document store. Notice that if no document store is passed into the constructor, then the repository 
    /// will work entirely in-memory and not persist any changes.
    /// </summary>
    public class MemoryMappedInterestRepository : IInterestRepository
    {
        private readonly IDocumentStore? _documentStore;
        private readonly ConcurrentDictionary<Guid, IInterest> _interestById = new ConcurrentDictionary<Guid, IInterest>();
        private readonly InMemoryInterestRelationIndex _interestIndex = new InMemoryInterestRelationIndex();

        public MemoryMappedInterestRepository(IDocumentStore? documentStore = null)
        {
            this._documentStore = documentStore;

            InitialReadFromStore();
        }

        public IInterest RegisterWalkOfInterest(Guid interestId, RouteNetworkElementIdList walkIds)
        {
            WalkOfInterest walkOfInterest = new WalkOfInterest(interestId, walkIds);

            if (!_interestById.TryAdd(interestId, walkOfInterest))
                throw new ApplicationException($"Interest with id: {interestId} already exist in repository");

            _interestIndex.AddOrUpdate(walkOfInterest);

            InsertIntoStore(walkOfInterest);

            return walkOfInterest;
        }

        public void RemoteInterest(Guid interestId)
        {
            _interestIndex.Remove(interestId);

            DeleteFromStore(interestId);
        }

        public Result<IInterest> GetInterest(Guid interestId)
        {
            if (_interestById.TryGetValue(interestId, out IInterest? interest))
            {
                return Result.Success<IInterest>(interest);
            }
            else
            {
                return Result.Failure<IInterest>($"No interest with id: {interestId} found");
            }
        }

        public Result<List<(IInterest, RouteNetworkInterestRelationKindEnum)>> GetInterestsByRouteNetworkElementId(Guid routeNetworkElementId)
        {
            var interestRelations = _interestIndex.GetRouteNetworkElementInterestRelations(routeNetworkElementId);

            List<(IInterest, RouteNetworkInterestRelationKindEnum)> result = new List<(IInterest, RouteNetworkInterestRelationKindEnum)>();

            foreach (var interestRelation in interestRelations)
            {
                result.Add((_interestById[interestRelation.Item1], interestRelation.Item2));
            }

            return Result.Success<List<(IInterest, RouteNetworkInterestRelationKindEnum)>> (result);
        }

        private void InsertIntoStore(IInterest interest)
        {
            if (_documentStore != null)
            {
                using var session = _documentStore.LightweightSession();

                session.Store(interest);
                session.SaveChanges();
            }
        }
         
        private void DeleteFromStore(Guid interestId)
        {
            if (_documentStore != null)
            {
                using var session = _documentStore.LightweightSession();

                session.Delete<IInterest>(interestId);
                session.SaveChanges();
            }
        }

        private void InitialReadFromStore()
        {
            if (_documentStore != null)
            {
                using var session = _documentStore.OpenSession();

                var interests = session.Query<IInterest>().ToArray();

                foreach (var interest in interests)
                {
                    _interestIndex.Add(interest);
                    _interestById[interest.Id] = interest;
                }
            }
        }
    }
}
