namespace RestaurantBackend.Models;

// Ek menu item jaise Pizza, Burger, Idly etc.
public class MenuItem
{
    public int Id { get; set; }

    // Item ka naam, jaise "Paneer Pizza"
    public string Name { get; set; } = string.Empty;

    // Category jaise "Pizza", "Burger" — isi se filter hota hai
    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    // Item ki image ka URL
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsAvailable { get; set; } = true;
}
