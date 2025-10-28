using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class TransportsODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public TransportsODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var transports = _services.TransportService.GetQueryableForOData();
            return Ok(transports);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var transport = _services.TransportService.GetQueryableForOData()
                .FirstOrDefault(t => t.Id == key);

            if (transport == null)
            {
                return NotFound();
            }

            return Ok(transport);
        }
    }
}