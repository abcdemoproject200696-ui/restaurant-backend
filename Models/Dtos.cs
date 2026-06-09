namespace RestaurantBackend.Models;

// ===== Client se aane wala order request =====
public class CreateOrderRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = "Cash on Delivery";
    public int? DiscountId { get; set; }   // optional: order pe laga discount
    public List<CreateOrderItem> Items { get; set; } = new();
}

// Discount add/update ke liye
public class DiscountRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Percentage { get; set; }
}

// ===== Razorpay =====
public class RazorpayOrderRequest
{
    public decimal Amount { get; set; }   // rupees me (backend paise me convert karega)
}

public class RazorpayVerifyRequest
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
}

public class CreateOrderItem
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
}

// ===== Status update request =====
public class UpdateStatusRequest
{
    public OrderStatus Status { get; set; }
}

// ===== Dashboard ke counts =====
public class OrderStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Delivered { get; set; }
    public int Cancelled { get; set; }
    public decimal TotalRevenue { get; set; } // sirf delivered orders ka
    public int TotalCustomers { get; set; }   // kitne signed-up customers
}

// ===== Auth =====
public class SignupRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Pincode { get; set; } = string.Empty;
}

// Login: phone + password (match hone par OTP banta hai)
public class LoginRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Admin: customer ko active/inactive
public class SetActiveRequest
{
    public bool IsActive { get; set; }
}

// Password change (admin kisi ka bhi, user apna)
public class ChangePasswordRequest
{
    public string Password { get; set; } = string.Empty;
}

public class RequestOtpRequest
{
    public string Phone { get; set; } = string.Empty;
}

// Backend OTP wapas bhejta hai taaki frontend popup me dikha sake (demo)
public class OtpResponse
{
    public string Otp { get; set; } = string.Empty;
}

public class VerifyOtpRequest
{
    public string Phone { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}
