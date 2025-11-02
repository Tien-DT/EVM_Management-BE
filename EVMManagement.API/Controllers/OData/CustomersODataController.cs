using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class CustomersODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public CustomersODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var customers = _services.CustomerService.GetQueryableForOData();
            return Ok(customers);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var customer = _services.CustomerService.GetQueryableForOData()
                .FirstOrDefault(c => c.Id == key);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer);
        }
    }
}