using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class VehicleVariantsODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public VehicleVariantsODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var vehicleVariants = _services.VehicleVariantService.GetQueryableForOData();
            return Ok(vehicleVariants);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var vehicleVariant = _services.VehicleVariantService.GetQueryableForOData()
                .FirstOrDefault(vv => vv.Id == key);

            if (vehicleVariant == null)
            {
                return NotFound();
            }

            return Ok(vehicleVariant);
        }
    }
}