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

    // POST /api/auth/signup  -> naya user banao (ya phone pehle se ho to update)
    [HttpPost("signup")]
    public async Task<ActionResult<User>> Signup([FromBody] SignupRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(req.Phone ?? "", @"^\d{10}$"))
            return BadRequest("Valid 10-digit phone is required.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == req.Phone);
        if (user is null)
        {
            user = new User { Phone = req.Phone!.Trim(), CreatedAt = DateTime.UtcNow };
            _db.Users.Add(user);
        }
        user.Name = req.Name.Trim();
        user.Address = (req.Address ?? "").Trim();
        user.Pincode = (req.Pincode ?? "").Trim();
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // POST /api/auth/request-otp  -> OTP banao aur wapas bhejo (demo: popup me dikhega)
    [HttpPost("request-otp")]
    public ActionResult<OtpResponse> RequestOtp([FromBody] RequestOtpRequest req)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(req.Phone ?? "", @"^\d{10}$"))
            return BadRequest("Valid 10-digit phone is required.");

        var otp = _otp.Generate(req.Phone!);
        return Ok(new OtpResponse { Otp = otp });
    }

    // POST /api/auth/verify-otp  -> OTP check karo, sahi ho to user wapas
    [HttpPost("verify-otp")]
    public async Task<ActionResult<User>> VerifyOtp([FromBody] VerifyOtpRequest req)
    {
        if (!_otp.Verify(req.Phone ?? "", req.Otp ?? ""))
            return BadRequest("Invalid OTP.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == req.Phone);
        if (user is null) return NotFound("User not registered. Please sign up.");
        return Ok(user);
    }

    // GET /api/auth/user?phone=...  -> phone se user dhoondo
    [HttpGet("user")]
    public async Task<ActionResult<User>> GetUser([FromQuery] string phone)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == phone);
        return user is null ? NotFound() : Ok(user);
    }

    // GET /api/auth/users?search=...  -> sabhi customers (naam ya phone se search)
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

    // PUT /api/auth/users/5  -> customer update (admin): name/phone/address
    [HttpPut("users/{id:int}")]
    public async Task<ActionResult<User>> UpdateUser(int id, [FromBody] SignupRequest req)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Name is required.");
        if (!System.Text.RegularExpressions.Regex.IsMatch(req.Phone ?? "", @"^\d{10}$"))
            return BadRequest("Valid 10-digit phone is required.");

        // Naya phone kisi aur customer ka to nahi?
        var clash = await _db.Users.AnyAsync(u => u.Phone == req.Phone && u.Id != id);
        if (clash) return BadRequest("This phone number is already used by another customer.");

        user.Name = req.Name.Trim();
        user.Phone = req.Phone!.Trim();
        user.Address = (req.Address ?? "").Trim();
        user.Pincode = (req.Pincode ?? "").Trim();
        await _db.SaveChangesAsync();
        return Ok(user);
    }

    // DELETE /api/auth/users/5  -> customer delete (admin)
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = true, id });
    }
}
