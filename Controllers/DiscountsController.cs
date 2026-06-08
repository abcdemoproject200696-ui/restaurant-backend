using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models;

namespace RestaurantBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscountsController : ControllerBase
{
    private readonly AppDbContext _db;
    public DiscountsController(AppDbContext db) => _db = db;

    // GET /api/discounts  -> saare discounts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Discount>>> GetAll()
        => Ok(await _db.Discounts.OrderBy(d => d.Name).ToListAsync());

    // POST /api/discounts  -> naya discount (owner)
    [HttpPost]
    public async Task<ActionResult<Discount>> Create([FromBody] DiscountRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Discount name is required.");
        if (req.Percentage <= 0 || req.Percentage > 100) return BadRequest("Percentage must be between 1 and 100.");

        var d = new Discount { Name = req.Name.Trim(), Percentage = req.Percentage };
        _db.Discounts.Add(d);
        await _db.SaveChangesAsync();
        return Ok(d);
    }

    // PUT /api/discounts/5  -> update
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Discount>> Update(int id, [FromBody] DiscountRequest req)
    {
        var d = await _db.Discounts.FindAsync(id);
        if (d is null) return NotFound();
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Discount name is required.");
        if (req.Percentage <= 0 || req.Percentage > 100) return BadRequest("Percentage must be between 1 and 100.");

        d.Name = req.Name.Trim();
        d.Percentage = req.Percentage;
        await _db.SaveChangesAsync();
        return Ok(d);
    }

    // DELETE /api/discounts/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var d = await _db.Discounts.FindAsync(id);
        if (d is null) return NotFound();
        _db.Discounts.Remove(d);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true, id });
    }
}
