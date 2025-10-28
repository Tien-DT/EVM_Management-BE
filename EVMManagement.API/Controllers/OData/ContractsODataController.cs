using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class ContractsODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public ContractsODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var contracts = _services.ContractService.GetQueryableForOData();
            return Ok(contracts);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var contract = _services.ContractService.GetQueryableForOData()
                .FirstOrDefault(c => c.Id == key);

            if (contract == null)
            {
                return NotFound();
            }

            return Ok(contract);
        }
    }
}