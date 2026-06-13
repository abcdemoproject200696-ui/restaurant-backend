namespace RestaurantBackend.Models;

// Ek customer order
public class Order
{
    public int Id { get; set; }

    // Customer ki details
    public string CustomerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Payment: "Cash on Delivery" (offline) ya "Online - Google Pay/UPI" etc.
    public string PaymentMethod { get; set; } = "Cash on Delivery";

    // Bill ka hisaab
    public decimal Subtotal { get; set; }      // items ka total (tax se pehle)
    public decimal TaxAmount { get; set; }     // GST
    public string DiscountName { get; set; } = string.Empty; // applied discount ka naam (ya khaali)
    public decimal DiscountAmount { get; set; } // discount me kitna kam hua
    public decimal TotalAmount { get; set; }   // Subtotal + Tax - Discount

    // Delivery location (optional) — customer ne "use my location" diya to exact GPS
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Is order me kaun kaun se items hain
    public List<OrderItem> Items { get; set; } = new();
}
