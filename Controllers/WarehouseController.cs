using apbd_cw9.Models;
using apbd_cw9.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw9.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController(IWarehouseService _warehouseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse(AddProductDto product)
    {
        if (product == null)
        {
            return BadRequest("Product data cannot be null");
        }
        
        try
        {
            var id = await _warehouseService.AddProductToWarehouse(product);
            return Ok(id);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
    
    [HttpPost("Procedure")]
    public async Task<IActionResult> AddProductToWarehouseProcedure(AddProductDto product)
    {
        if (product == null)
        {
            return BadRequest("Product data cannot be null");
        }

        try
        {
            var result = await _warehouseService.AddProductToWarehouseProcedure(product);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}