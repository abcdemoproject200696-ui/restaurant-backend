using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantBackend.Data;
using RestaurantBackend.Models;
using RestaurantBackend.Services;

namespace RestaurantBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly OtpService _otp;

    public AuthController(AppDbContext db, OtpService otp)
    {
        _db = db;
        _otp = otp;
    }

    // 10 digit, 6-9 se shuru (asli Indian mobile — 12345 jaisा reject)
    private static bool ValidPhone(string? p) =>
        System.Text.RegularExpressions.Regex.IsMatch(p ?? "", @"^[6-9]\d{9}$");

    // POST /api/auth/signup -> naya user (by-default INACTIVE, role Customer)
    [HttpPost("signup")]
    public async Task<ActionResult<User>> Signup([FromBody] SignupRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (!ValidPhone(req.Phone)) return BadRequest("Valid 10-digit phone is required.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 4)
            return BadRequest("Password must be at least 4 characters.");

        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Phone == req.Phone);
        if (existing is not null)
            return Conflict("This mobile number is already registered. Please login.");

        // Role: sirf Customer / DeliveryBoy / Kitchen (Admin signup se nahi ban sakta)
        var role = req.Role is "DeliveryBoy" or "Kitchen" ? req.Role : "Customer";

        var user = new User
        {
            Name = req.Name.Trim(),
            Phone = req.Phone!.Trim(),
            Password = req.Password,
            Address = (req.Address ?? "").Trim(),
            Pincode = (req.Pincode ?? "").Trim(),
            // Customer turant active; Delivery Boy + Kitchen inactive (admin activate karega)
            IsActive = role == "Customer",
            Role = role,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // POST /api/auth/login -> phone + password check. Sahi+active hone par OTP banao.
    //   404 = user nahi mila, 401 = galat password, 403 = account inactive
    [HttpPost("login")]
    public async Task<ActionResult<OtpResponse>> Login([FromBody] LoginRequest req)
    {
        if (!ValidPhone(req.Phone)) return BadRequest("Valid 10-digit phone is required.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == req.Phone);
        if (user is null) return NotFound("This mobile number is not registered.");
        if (user.Password != (req.Password ?? "")) return Unauthorized("Incorrect password.");
        if (!user.IsActive)
            return StatusCode(403, "Your account is not active yet. Please contact the admin.");

        var otp = _otp.Generate(req.Phone!);
        return Ok(new OtpResponse { Otp = otp });
    }

    // POST /api/auth/verify-otp -> OTP sahi to user wapas (active hi hoga, login ne check kiya)
    [HttpPost("verify-otp")]
    public async Task<ActionResult<User>> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        if (!_otp.Verify(req.Phone ?? "", req.Otp ?? ""))
            return BadRequest("Invalid OTP.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == req.Phone);
        if (user is null) return NotFound("User not registered. Please sign up.");
        if (!user.IsActive)
            return StatusCode(403, "Your account is not active. Please contact the admin.");
        return Ok(user);
    }

    // POST /api/auth/forgot-otp { phone } -> user registered ho to OTP, warna 404
    [HttpPost("forgot-otp")]
    public async Task<ActionResult<OtpResponse>> ForgotOtp([FromBody] RequestOtpRequest req)
    {
        if (!ValidPhone(req.Phone)) return BadRequest("Valid 10-digit phone is required.");
        var exists = await _db.Users.AnyAsync(u => u.Phone == req.Phone);
        if (!exists) return NotFound("This mobile number is not registered.");
        var otp = _otp.Generate(req.Phone!);
        return Ok(new OtpResponse { Otp = otp });
    }

    // POST /api/auth/reset-password { phone, otp, password } -> OTP verify + naya password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (!_otp.Verify(req.Phone ?? "", req.Otp ?? "")) return BadRequest("Invalid OTP.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 4)
            return BadRequest("Password must be at least 4 characters.");
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == req.Phone);
        if (user is null) return NotFound("User not found.");
        user.Password = req.Password;
        await _db.SaveChangesAsync();
        return Ok(new { success = true });
    }

    // GET /api/auth/user?phone=... -> phone se user
    [HttpGet("user")]
    public async Task<ActionResult<User>> GetUser([FromQuery] string phone)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        return user is null ? NotFound() : Ok(user);
    }

    // GET /api/auth/users?search=... -> saare customers (admin ko password bhi dikhta hai)
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers([FromQuery] string? search)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(u => u.Name.Contains(s) || u.Phone.Contains(s));
        }
        var users = await query.OrderByDescending(u => u.Id).ToListAsync();
        return Ok(users);
    }

    // PUT /api/auth/users/5 -> customer update (admin): name/phone/address/pincode
    [HttpPut("users/{id:int}")]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] SignupRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (!ValidPhone(req.Phone)) return BadRequest("Valid 10-digit phone is required.");

        var clash = await _db.Users.AnyAsync(u => u.Phone == req.Phone && u.Id != id);
        if (clash) return BadRequest("This phone number is already used by another customer.");

        user.Name = req.Name.Trim();
        user.Phone = req.Phone!.Trim();
        user.Address = (req.Address ?? "").Trim();
        user.Pincode = (req.Pincode ?? "").Trim();
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // PUT /api/auth/users/5/active -> admin active/inactive
    [HttpPut("users/{id:int}/active")]
    public async Task<ActionResult<User>> SetActive(int id, [FromBody] SetActiveRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // PUT /api/auth/users/5/role -> admin role badle (Customer / DeliveryBoy / Kitchen)
    [HttpPut("users/{id:int}/role")]
    public async Task<ActionResult<User>> SetRole(int id, [FromBody] SetRoleRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Role == "Admin") return BadRequest("Admin role cannot be changed.");
        user.Role = req.Role is "DeliveryBoy" or "Kitchen" or "Customer" ? req.Role : "Customer";
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // PUT /api/auth/users/5/password -> password change (admin kisi ka bhi, user apna)
    [HttpPut("users/{id:int}/password")]
    public async Task<ActionResult<User>> ChangePassword(int id, [FromBody] ChangePasswordRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 4)
            return BadRequest("Password must be at least 4 characters.");
        user.Password = req.Password;
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // DELETE /api/auth/users/5 -> customer delete (admin)
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        if (user.Role == "Admin") return BadRequest("Admin account cannot be deleted.");
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true, id });
    }
}
