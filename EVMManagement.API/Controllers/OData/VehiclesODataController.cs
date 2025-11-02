using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class VehiclesODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public VehiclesODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var vehicles = _services.VehicleService.GetQueryableForOData();
            return Ok(vehicles);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var vehicle = _services.VehicleService.GetQueryableForOData()
                .FirstOrDefault(v => v.Id == key);

            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }
    }
}