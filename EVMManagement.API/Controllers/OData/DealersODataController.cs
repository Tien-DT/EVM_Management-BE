using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class DealersODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public DealersODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var dealers = _services.DealerService.GetQueryableForOData();
            return Ok(dealers);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var dealer = _services.DealerService.GetQueryableForOData()
                .FirstOrDefault(d => d.Id == key);

            if (dealer == null)
            {
                return NotFound();
            }

            return Ok(dealer);
        }
    }
}