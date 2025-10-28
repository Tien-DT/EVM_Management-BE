using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class VehicleModelsODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public VehicleModelsODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var vehicleModels = _services.VehicleModelService.GetQueryableForOData();
            return Ok(vehicleModels);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var vehicleModel = _services.VehicleModelService.GetQueryableForOData()
                .FirstOrDefault(vm => vm.Id == key);

            if (vehicleModel == null)
            {
                return NotFound();
            }

            return Ok(vehicleModel);
        }
    }
}