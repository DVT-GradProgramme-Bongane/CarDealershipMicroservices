using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory.Api.Data;
using Inventory.Api.Models;
using Inventory.Api.Services;

[ApiController, Route("inventory")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly RabbitMqPublisher _rabbit;

    public InventoryController(AppDbContext db, RabbitMqPublisher rabbit)
    {
        _db = db;
        _rabbit = rabbit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _db.Cars.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var car = await _db.Cars.FindAsync(id);
        return car is null ? NotFound() : Ok(car);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Car car)
    {
        car.Id = Guid.NewGuid();
        car.CreatedAt = DateTime.UtcNow;
        _db.Cars.Add(car);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById),
            new { id = car.Id }, car);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Car updated)
    {
        var car = await _db.Cars.FindAsync(id);
        if (car is null) return NotFound();
        car.Make = updated.Make;  car.Model = updated.Model;
        car.Year = updated.Year;  car.Color = updated.Color;
        car.Price = updated.Price; car.Mileage = updated.Mileage;
        car.Type = updated.Type;
        await _db.SaveChangesAsync();
        return Ok(car);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id,
        [FromBody] StatusUpdateDto dto)
    {
        var car = await _db.Cars.FindAsync(id);
        if (car is null) return NotFound();
        car.Status = dto.Status;
        await _db.SaveChangesAsync();
        await _rabbit.PublishAsync("car.status.updated", new {
            car_id = car.Id, status = car.Status
        });
        return Ok(car);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var car = await _db.Cars.FindAsync(id);
        if (car is null) return NotFound();
        _db.Cars.Remove(car);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record StatusUpdateDto(CarStatus Status);