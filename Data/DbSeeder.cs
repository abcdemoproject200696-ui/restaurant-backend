using RestaurantBackend.Models;

namespace RestaurantBackend.Data;

// Pehli baar app chalne par menu data daal deta hai
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.MenuItems.Any()) return; // pehle se data hai to kuch mat karo

        // Live food image URLs (verified working). Frontend me fallback bhi hai.
        // Unsplash params se chhoti optimized image aati hai.
        string U(string id) => $"https://images.unsplash.com/photo-{id}?w=400&q=70&auto=format&fit=crop";

        var items = new List<MenuItem>
        {
            // ===== Pizza =====
            new() { Name = "Margherita Pizza", Category = "Pizza", Description = "Classic cheese & tomato", Price = 199, ImageUrl = U("1513104890138-7c749659a591") },
            new() { Name = "Onion Pizza", Category = "Pizza", Description = "Loaded with fresh onions", Price = 229, ImageUrl = U("1565299624946-b28f40a0ae38") },
            new() { Name = "Paneer Pizza", Category = "Pizza", Description = "Spicy paneer toppings", Price = 279, ImageUrl = U("1574071318508-1cdbab80d002") },
            new() { Name = "Cheese Burst Pizza", Category = "Pizza", Description = "Extra cheese in crust", Price = 329, ImageUrl = U("1604382354936-07c5d9983bd3") },

            // ===== Burger =====
            new() { Name = "Veg Burger", Category = "Burger", Description = "Crispy veg patty", Price = 99, ImageUrl = U("1568901346375-23c9450c58cd") },
            new() { Name = "Cheese Burger", Category = "Burger", Description = "Double cheese slice", Price = 139, ImageUrl = U("1571091718767-18b5b1457add") },
            new() { Name = "Aloo Tikki Burger", Category = "Burger", Description = "Desi aloo tikki", Price = 89, ImageUrl = U("1550547660-d9450f859349") },

            // ===== Idly =====
            new() { Name = "Plain Idly", Category = "Idly", Description = "Steamed soft idly (2 pcs)", Price = 59, ImageUrl = "https://foodish-api.com/images/idly/idly1.jpg" },
            new() { Name = "Rava Idly", Category = "Idly", Description = "Semolina idly with sambar", Price = 79, ImageUrl = "https://foodish-api.com/images/idly/idly2.jpg" },
            new() { Name = "Sambar Idly", Category = "Idly", Description = "Idly dipped in sambar", Price = 89, ImageUrl = "https://foodish-api.com/images/idly/idly3.jpg" },

            // ===== Noodles =====
            new() { Name = "Veg Noodles", Category = "Noodles", Description = "Stir-fried veggies", Price = 119, ImageUrl = U("1612929633738-8fe44f7ec841") },
            new() { Name = "Hakka Noodles", Category = "Noodles", Description = "Indo-chinese style", Price = 139, ImageUrl = U("1569718212165-3a8278d5f624") },
            new() { Name = "Schezwan Noodles", Category = "Noodles", Description = "Spicy schezwan sauce", Price = 149, ImageUrl = U("1585032226651-759b368d7246") },

            // ===== Sandwich =====
            new() { Name = "Veg Sandwich", Category = "Sandwich", Description = "Fresh veggies & sauce", Price = 79, ImageUrl = U("1528735602780-2552fd46c7af") },
            new() { Name = "Cheese Sandwich", Category = "Sandwich", Description = "Melted cheese", Price = 99, ImageUrl = U("1539252554453-80ab65ce3586") },
            new() { Name = "Grilled Sandwich", Category = "Sandwich", Description = "Grilled & crispy", Price = 119, ImageUrl = U("1553909489-cd47e0907980") },
        };

        db.MenuItems.AddRange(items);
        db.SaveChanges();
    }

    // 5 fake customers daal do (jo pehle se nahi hain unhe, phone se check karke)
    public static void SeedUsers(AppDbContext db)
    {
        var fakes = new List<User>
        {
            new() { Name = "Amit Sharma",  Phone = "9811122233", Address = "45 Park Street, Delhi", Pincode = "110001" },
            new() { Name = "Priya Singh",  Phone = "9822233344", Address = "12 Lake View, Bangalore", Pincode = "560001" },
            new() { Name = "Rohit Verma",  Phone = "9833344455", Address = "78 Hill Road, Mumbai", Pincode = "400001" },
            new() { Name = "Sneha Patel",  Phone = "9844455566", Address = "9 Green Avenue, Ahmedabad", Pincode = "380001" },
            new() { Name = "Vikram Rao",   Phone = "9855566677", Address = "23 MG Road, Pune", Pincode = "411001" },
        };

        var added = false;
        foreach (var f in fakes)
        {
            if (!db.Users.Any(u => u.Phone == f.Phone))
            {
                db.Users.Add(f);
                added = true;
            }
        }
        if (added) db.SaveChanges();
    }

    // 4 default discounts
    public static void SeedDiscounts(AppDbContext db)
    {
        if (db.Discounts.Any()) return;
        db.Discounts.AddRange(
            new Discount { Name = "Anniversary", Percentage = 10 },
            new Discount { Name = "Birthday", Percentage = 15 },
            new Discount { Name = "Diwali", Percentage = 20 },
            new Discount { Name = "New Year", Percentage = 25 }
        );
        db.SaveChanges();
    }
}
