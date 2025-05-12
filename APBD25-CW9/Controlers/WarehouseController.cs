using APBD25_CW9.Exceptions;
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
            try
            {
                var code = await _warehouseService.sqlOperation(warehouseDto, cancellationToken);

                return Ok(code);
            }
            catch (NotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (BadRequestException e)
            {
                return BadRequest(e.Message);
            }
            catch (ConflictException e)
            {
                return Conflict(e.Message);
            }
        }

        [HttpPost("/proceudra")]
        public async Task<IActionResult> GetWarehouseProcedure([FromBody] WarehouseDto warehouseDto,
            CancellationToken cancellationToken)
        {
            var code = await _warehouseService.ProcedureAsync(warehouseDto,cancellationToken);
            return Ok(code);
        }
    }
}
