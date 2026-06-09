using Microsoft.AspNetCore.Mvc;
using FinanceService.Services;
using FinanceService.DTOs;

namespace FinanceService.Controllers;

[ApiController]
[Route("finance")]
public class FinanceController : ControllerBase
{
    private readonly IFinancingApplicationService _service;

    public FinanceController(IFinancingApplicationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFinancingApplicationDto dto, CancellationToken ct)
    {
        var result = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateApplicationStatusDto dto, CancellationToken ct)
    {
        var result = await _service.UpdateStatusAsync(id, dto.Status, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
