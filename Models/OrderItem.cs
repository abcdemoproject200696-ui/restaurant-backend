namespace RestaurantBackend.Models;

// Order ke andar ka ek line item (item + quantity)
public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int MenuItemId { get; set; }

    // Naam aur price order ke time copy kar lete hain (taaki baad me menu badle to purana order na badle)
    public string MenuItemName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }

    public int Quantity { get; set; }
}
