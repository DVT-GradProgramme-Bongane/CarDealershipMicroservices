using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UsedCarSalesService.Contracts;
using UsedCarSalesService.Services;

namespace UsedCarSalesService.Controller;

[ApiController]
[Route("used-sales")]
public class UsedSalesController : ControllerBase
{
    private readonly UsedSalesService _service;

    public UsedSalesController(UsedSalesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAll());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var sale = await _service.GetById(id);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest req)
    {
        try
        {
            var id = await _service.CreateSale(req);
            return CreatedAtAction(nameof(Get), new { id }, id);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSaleStatusRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Status))
        {
            return BadRequest("Status is required.");
        }
        try
        {
            var updated = await _service.UpdateStatus(id, req.Status);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }
}
