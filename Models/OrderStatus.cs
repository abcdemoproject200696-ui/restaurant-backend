namespace RestaurantBackend.Models;

// Order ke alag-alag status
public enum OrderStatus
{
    Pending = 0,      // Naya order, abhi accept nahi hua
    InProgress = 1,   // Ban raha hai / kitchen me hai
    Delivered = 2,    // Customer ko mil gaya
    Cancelled = 3     // Cancel ho gaya
}
