using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class OrdersODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public OrdersODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var orders = _services.OrderService.GetQueryableForOData();
            return Ok(orders);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var order = _services.OrderService.GetQueryableForOData()
                .FirstOrDefault(o => o.Id == key);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
    }
}