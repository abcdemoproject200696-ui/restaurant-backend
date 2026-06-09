using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models;

namespace RestaurantBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _db;
    public MenuController(AppDbContext db) => _db = db;

    // GET /api/menu?category=Pizza&search=paneer
    // category aur search dono optional hain
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenu(
        [FromQuery] string? category, [FromQuery] string? search)
    {
        var query = _db.MenuItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category) && category != "All")
            query = query.Where(m => m.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search));

        var items = await query.OrderBy(m => m.Category).ThenBy(m => m.Name).ToListAsync();
        return Ok(items);
    }

    // GET /api/menu/categories  -> ["All","Pizza","Burger",...]
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var cats = await _db.MenuItems.Select(m => m.Category).Distinct().OrderBy(c => c).ToListAsync();
        cats.Insert(0, "All");
        return Ok(cats);
    }

    // GET /api/menu/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MenuItem>> GetItem(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    // POST /api/menu -> naya item (admin). ImageUrl me base64 data URI ho sakta hai.
    [HttpPost]
    public async Task<ActionResult<MenuItem>> Create([FromBody] MenuItemRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(req.Category)) return BadRequest("Category is required.");
        if (req.Price <= 0) return BadRequest("Price must be greater than 0.");

        var item = new MenuItem
        {
            Name = req.Name.Trim(),
            Category = req.Category.Trim(),
            Description = (req.Description ?? "").Trim(),
            Price = req.Price,
            ImageUrl = req.ImageUrl ?? "",
            IsAvailable = true,
        };
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    // PUT /api/menu/5 -> item update (admin) — image bhi update
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MenuItem>> Update(int id, [FromBody] MenuItemRequest req)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(req.Category)) return BadRequest("Category is required.");
        if (req.Price <= 0) return BadRequest("Price must be greater than 0.");

        item.Name = req.Name.Trim();
        item.Category = req.Category.Trim();
        item.Description = (req.Description ?? "").Trim();
        item.Price = req.Price;
        if (!string.IsNullOrWhiteSpace(req.ImageUrl)) item.ImageUrl = req.ImageUrl; // nayi image di to update
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    // DELETE /api/menu/5 -> item delete (admin). Row delete = image bhi DB se gayi.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item is null) return NotFound();
        _db.MenuItems.Remove(item);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true, id });
    }
}
