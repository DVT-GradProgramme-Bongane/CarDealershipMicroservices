using ClientServices.Data;
using ClientServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientServices.Controller;

[ApiController]
[Route("clients")] 
public class CustomerController : ControllerBase
{
    private readonly ClientDbContext _context;

    public CustomerController(ClientDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
    {
        var customers = await _context.Customers.OrderByDescending(c => c.CreatedAt).ToListAsync();
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Customer>> GetCustomerById(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
    {
        var emailExists = await _context.Customers.AnyAsync(c => c.Email.ToLower() == customer.Email.ToLower());
        if (emailExists)
        {
            return BadRequest(new { message = "A customer with this email address already exists." });
        }

        customer.Id = Guid.NewGuid();
        customer.CreatedAt = DateTime.UtcNow;

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomers), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] Customer updatedCustomer)
    {
        var existingCustomer = await _context.Customers.FindAsync(id);
        if (existingCustomer == null)
        {
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }

        if (existingCustomer.Email.ToLower() != updatedCustomer.Email.ToLower())
        {
            var emailExists = await _context.Customers.AnyAsync(c => c.Email.ToLower() == updatedCustomer.Email.ToLower());
            if (emailExists)
            {
                return BadRequest(new { message = "This email is already taken by another customer." });
            }
        }

        existingCustomer.FirstName = updatedCustomer.FirstName;
        existingCustomer.LastName = updatedCustomer.LastName;
        existingCustomer.Email = updatedCustomer.Email;
        existingCustomer.Phone = updatedCustomer.Phone;
        existingCustomer.IdNumber = updatedCustomer.IdNumber;

        await _context.SaveChangesAsync();
        return Ok(existingCustomer);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(new { message = $"Customer with ID {id} not found." });
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Customer deleted successfully." });
    }
}