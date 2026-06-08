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
}
