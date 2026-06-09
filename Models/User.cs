namespace RestaurantBackend.Models;

// App ka registered user (signup se banta hai)
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;   // login isi se (unique)
    public string Password { get; set; } = string.Empty; // login ke liye
    public string Address { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    // Admin hi active/inactive karta hai. Naya signup by-default inactive.
    public bool IsActive { get; set; } = false;

    // "Admin" ya "Customer"
    public string Role { get; set; } = "Customer";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
