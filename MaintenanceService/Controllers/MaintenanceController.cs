using MaintenanceService.Data;
using MaintenanceService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceService.Controllers;

[ApiController]
[Route("maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly MaintenanceDbContext _context;

    public MaintenanceController(MaintenanceDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MaintenanceJob>>> GetJobs(CancellationToken cancellationToken)
    {
        var jobs = await _context.Jobs
            .AsNoTracking()
            .OrderByDescending(job => job.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(jobs);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MaintenanceJob>> GetJob(Guid id, CancellationToken cancellationToken)
    {
        var job = await _context.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(job => job.Id == id, cancellationToken);

        if (job == null)
        {
            return NotFound(new { message = $"Maintenance job with ID {id} not found." });
        }

        return Ok(job);
    }

    [HttpPost]
    public async Task<ActionResult<MaintenanceJob>> CreateJob(
        [FromBody] CreateMaintenanceJobRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CarId == Guid.Empty)
        {
            return BadRequest(new { message = "CarId is required." });
        }

        if (request.ClientId == Guid.Empty)
        {
            return BadRequest(new { message = "ClientId is required." });
        }

        if (request.StaffId == Guid.Empty)
        {
            return BadRequest(new { message = "StaffId is required." });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { message = "Description is required." });
        }

        var job = new MaintenanceJob
        {
            Id = Guid.NewGuid(),
            CarId = request.CarId,
            ClientId = request.ClientId,
            StaffId = request.StaffId,
            Description = request.Description.Trim(),
            Status = MaintenanceJobStatuses.Scheduled,
            Scheduled = request.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<MaintenanceJob>> UpdateJobStatus(
        Guid id,
        [FromBody] UpdateMaintenanceJobStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Status))
        {
            return BadRequest(new { message = "Status is required." });
        }

        if (!MaintenanceJobStatuses.IsValid(request.Status))
        {
            return BadRequest(new { message = "Status must be scheduled, in-progress, or completed." });
        }

        var job = await _context.Jobs.FindAsync([id], cancellationToken);
        if (job == null)
        {
            return NotFound(new { message = $"Maintenance job with ID {id} not found." });
        }

        job.Status = NormalizeStatus(request.Status);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(job);
    }

    private static string NormalizeStatus(string status)
    {
        return status.Trim().ToLowerInvariant();
    }
}
