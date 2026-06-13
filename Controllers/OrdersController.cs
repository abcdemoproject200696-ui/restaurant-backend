using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models;

namespace RestaurantBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    public OrdersController(AppDbContext db) => _db = db;

    // GET /api/orders?status=Pending&phone=...&recentDays=7  (sab optional)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders(
        [FromQuery] string? status, [FromQuery] string? phone, [FromQuery] int? recentDays)
    {
        var query = _db.Orders.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "All"
            && Enum.TryParse<OrderStatus>(status, true, out var st))
        {
            query = query.Where(o => o.Status == st);
        }

        // Kisi ek customer ke orders (phone se)
        if (!string.IsNullOrWhiteSpace(phone))
        {
            query = query.Where(o => o.Phone == phone);
        }

        // Sirf pichle N din ke (jaise 1 week = 7)
        if (recentDays.HasValue && recentDays.Value > 0)
        {
            var from = DateTime.UtcNow.AddDays(-recentDays.Value);
            query = query.Where(o => o.CreatedAt >= from);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders);
    }

    // GET /api/orders/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? NotFound() : Ok(order);
    }

    // POST /api/orders  -> naya order place karo
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderRequest req)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(req.CustomerName)) return BadRequest("Customer name is required.");
        if (string.IsNullOrWhiteSpace(req.Phone)) return BadRequest("Phone is required.");
        if (string.IsNullOrWhiteSpace(req.Address)) return BadRequest("Address is required.");
        if (string.IsNullOrWhiteSpace(req.Pincode)) return BadRequest("Pincode is required.");
        if (req.Items is null || req.Items.Count == 0) return BadRequest("Cart is empty.");

        var order = new Order
        {
            CustomerName = req.CustomerName.Trim(),
            Address = req.Address.Trim(),
            Phone = req.Phone.Trim(),
            Pincode = req.Pincode.Trim(),
            PaymentMethod = string.IsNullOrWhiteSpace(req.PaymentMethod) ? "Cash on Delivery" : req.PaymentMethod.Trim(),
            Status = OrderStatus.Pending,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            CreatedAt = DateTime.UtcNow,
        };

        decimal subtotal = 0;
        foreach (var line in req.Items)
        {
            if (line.Quantity <= 0) continue;
            var menuItem = await _db.MenuItems.FindAsync(line.MenuItemId);
            if (menuItem is null) return BadRequest($"Menu item {line.MenuItemId} not found.");

            order.Items.Add(new OrderItem
            {
                MenuItemId = menuItem.Id,
                MenuItemName = menuItem.Name,
                ImageUrl = menuItem.ImageUrl,
                Price = menuItem.Price,
                Quantity = line.Quantity,
            });
            subtotal += menuItem.Price * line.Quantity;
        }

        if (order.Items.Count == 0) return BadRequest("No valid items in cart.");

        // GST 5% — tax aur discount EXACT (2 decimals), ceil nahi
        order.Subtotal = subtotal;
        order.TaxAmount = Math.Round(subtotal * 0.05m, 2);

        // Discount (agar select kiya hai) — name ke aage % bhi
        if (req.DiscountId.HasValue)
        {
            var discount = await _db.Discounts.FindAsync(req.DiscountId.Value);
            if (discount is not null)
            {
                order.DiscountName = $"{discount.Name} {discount.Percentage}%";
                order.DiscountAmount = Math.Round(subtotal * discount.Percentage / 100m, 2);
            }
        }

        // Sirf FINAL amount pe Math.Ceiling (jaise 93.45 -> 94)
        order.TotalAmount = Math.Ceiling(order.Subtotal + order.TaxAmount - order.DiscountAmount);
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    // PUT /api/orders/5/cancel  -> order cancel karo
    [HttpPut("{id:int}/cancel")]
    public async Task<ActionResult<Order>> CancelOrder(int id)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        if (order.Status == OrderStatus.Delivered)
            return BadRequest("Delivered order cannot be cancelled.");
        if (order.Status == OrderStatus.Cancelled)
            return BadRequest("Order is already cancelled.");

        order.Status = OrderStatus.Cancelled;
        await _db.SaveChangesAsync();
        return Ok(order);
    }

    // PUT /api/orders/5/status  -> status badlo (Pending/InProgress/Delivered/Cancelled)
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<Order>> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();

        order.Status = req.Status;
        await _db.SaveChangesAsync();
        return Ok(order);
    }

    // GET /api/orders/stats  -> dashboard ke counts
    [HttpGet("stats")]
    public async Task<ActionResult<OrderStatsDto>> GetStats()
    {
        var orders = await _db.Orders.ToListAsync();
        var customerCount = await _db.Users.CountAsync();
        var stats = new OrderStatsDto
        {
            Total = orders.Count,
            Pending = orders.Count(o => o.Status == OrderStatus.Pending),
            InProgress = orders.Count(o => o.Status == OrderStatus.InProgress),
            Delivered = orders.Count(o => o.Status == OrderStatus.Delivered),
            Cancelled = orders.Count(o => o.Status == OrderStatus.Cancelled),
            TotalRevenue = orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalAmount),
            TotalCustomers = customerCount,
        };
        return Ok(stats);
    }
}
