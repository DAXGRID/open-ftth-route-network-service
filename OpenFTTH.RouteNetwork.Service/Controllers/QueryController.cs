using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenFTTH.CQRS;

namespace OpenFTTH.RouteNetworkService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly ILogger<QueryController> _logger;
        private readonly IMediator _mediator;
        private Dictionary<string, Type> _requestTypes = new Dictionary<string, Type>();

        public QueryController(ILogger<QueryController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult Get()
        {
            string bodyString = Request.GetRawBodyStringAsync().Result;

            if (String.IsNullOrEmpty(bodyString))
            {
                _logger.LogWarning("Invalid query request. Request body is empty.");
                return BadRequest("Request body is empty. It must contain a query request object.");
            }

            try
            {
                var bodyObject = JObject.Parse(bodyString);

                if (bodyObject == null || !bodyObject.ContainsKey("RequestName"))
                {
                    _logger.LogWarning("Invalid query request. Request body must contain an attribut: 'RequestName'");
                    return BadRequest("Invalid query request. Request body must contain an attribut: 'RequestName'");
                }

                JToken? requestToken = "notset";

                if (bodyObject.TryGetValue("RequestName", out requestToken))
                {
                    var requestName = requestToken.ToString();

                    _logger.LogDebug($"Query controller recieved request: {requestName}");

                    if (GetRequestTypes().ContainsKey(requestName.ToLower()))
                    {
                        var eventType = GetRequestTypes()[requestName.ToLower()];

                        var eventObject = JsonConvert.DeserializeObject(bodyString, eventType);

                        if (eventObject != null)
                        {
                            var result = _mediator.Send(eventObject).Result;
                            return Content(JsonConvert.SerializeObject(result), "application/json");
                        }
                        else
                        {
                            var errTxt = $"Null result from request: {requestName}";
                            _logger.LogWarning(errTxt);
                            return BadRequest(errTxt);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Invalid query request. No class for: {requestName} found.");
                        return BadRequest($"Invalid query request. No class for: {requestName} found.");
                    }
                }
                else
                {
                    var errTxt = $"Cannot find RequestName in body: {bodyString}";
                    _logger.LogWarning(errTxt);
                    return BadRequest(errTxt);
                }

            }
            catch (JsonReaderException ex)
            {
                _logger.LogWarning($"Error parsing query body: {bodyString}", ex);
                return BadRequest($"Error parsing query body: {ex.Message}");
            }
        }


        private Dictionary<string, Type> GetRequestTypes()
        {
            if (_requestTypes.Count != 0)
                return _requestTypes;

            _requestTypes = new Dictionary<string, Type>();

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GetName().Name.ToLower().StartsWith("microsoft") && !a.GetName().Name.ToLower().StartsWith("system"));

            var requestTypes =
                assemblies
                .SelectMany(x => x.GetTypes().Where(a => a.Name.Contains("Query")))
                .ToList();

            foreach (var type in requestTypes)
            {
                _requestTypes.Add(type.Name.ToLower(), type);
            }

            return _requestTypes;
        }
    }
}
