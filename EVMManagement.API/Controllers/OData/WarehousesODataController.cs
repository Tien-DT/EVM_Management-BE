using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class WarehousesODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public WarehousesODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var warehouses = _services.WarehouseService.GetQueryableForOData();
            return Ok(warehouses);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var warehouse = _services.WarehouseService.GetQueryableForOData()
                .FirstOrDefault(w => w.Id == key);

            if (warehouse == null)
            {
                return NotFound();
            }

            return Ok(warehouse);
        }
    }
}