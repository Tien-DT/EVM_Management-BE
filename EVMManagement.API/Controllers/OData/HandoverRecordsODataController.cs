using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using EVMManagement.API.Services;

namespace EVMManagement.API.Controllers.OData
{
    [Authorize]
    public class HandoverRecordsODataController : ODataController
    {
        private readonly IServiceFacade _services;

        public HandoverRecordsODataController(IServiceFacade services)
        {
            _services = services;
        }

        [EnableQuery(MaxExpansionDepth = 3, PageSize = 50)]
        public IActionResult Get()
        {
            var handoverRecords = _services.HandoverRecordService.GetQueryableForOData();
            return Ok(handoverRecords);
        }

        [EnableQuery(MaxExpansionDepth = 3)]
        public IActionResult Get([FromRoute] Guid key)
        {
            var handoverRecord = _services.HandoverRecordService.GetQueryableForOData()
                .FirstOrDefault(h => h.Id == key);

            if (handoverRecord == null)
            {
                return NotFound();
            }

            return Ok(handoverRecord);
        }
    }
}