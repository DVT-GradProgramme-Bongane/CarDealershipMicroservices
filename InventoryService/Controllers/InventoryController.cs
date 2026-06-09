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
    public async Task<IActionResult> Create(CreateCarRequest createCarRequest)
    {
        if (!string.IsNullOrWhiteSpace(createCarRequest.Vin))
        {
            return BadRequest(new { message = "Vin is required." });
        }
        
        if(createCarRequest.Vin.Length > 17)
        {
            return BadRequest(new { message = "Vin number is longer than 17 characters." });
        };
        
        if (!string.IsNullOrWhiteSpace(createCarRequest.Make))
        {
            return BadRequest(new { message = "Make is required." });
        }
        
        if (!string.IsNullOrWhiteSpace(createCarRequest.Model))
        {
            return BadRequest(new { message = "Model is required." });
        }

        if (!string.IsNullOrWhiteSpace(createCarRequest.Color))
        {
            return BadRequest(new { message = "Color is required." });
        }
        
        var newCar = new Car
        {
            Vin = createCarRequest.Vin,
            Make = createCarRequest.Make,
            Model = createCarRequest.Model,
            Year = createCarRequest.Year,
            Color = createCarRequest.Color,
            Price = createCarRequest.Price,
            Mileage = createCarRequest.Mileage,
        };
        
        _db.Cars.Add(newCar);
        await _db.SaveChangesAsync();
        
        return Created($"/accessories/{newCar.Id}", createCarRequest);;
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
public sealed record CreateCarRequest(string Vin,string Make, string Model, int Year, string Color, decimal Price, int Mileage);