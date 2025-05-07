using APBD25_CW9.Models;
using APBD25_CW9.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace APBD25_CW9.Controlers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<IActionResult> GetWarehouse([FromBody] WarehouseDto warehouseDto, CancellationToken cancellationToken)
        {
        
            var code = await _warehouseService.sqlOperation(warehouseDto,cancellationToken);
            
            if (code == -400)
                return BadRequest("Ammount too small");
            if (code == -404)
                return NotFound();
            return Ok(code);
        }
    }
}
