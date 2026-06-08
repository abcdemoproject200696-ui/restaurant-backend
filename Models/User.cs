namespace RestaurantBackend.Models;

// App ka registered user (signup se banta hai)
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;   // unique-ish identifier (login isi se)
    public string Address { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
