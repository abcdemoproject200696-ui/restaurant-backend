namespace RestaurantBackend.Models;

// Owner ke banaye discounts (Anniversary, Birthday, Diwali etc)
public class Discount
{
    public int Id { get; set; }                       // auto-generated (discount_Id)
    public string Name { get; set; } = string.Empty;  // DiscountName
    public decimal Percentage { get; set; }           // discount_percentage
}
