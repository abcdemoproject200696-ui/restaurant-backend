using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using RestaurantBackend.Models;

namespace RestaurantBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpFactory;

    public PaymentsController(IConfiguration config, IHttpClientFactory httpFactory)
    {
        _config = config;
        _httpFactory = httpFactory;
    }

    private string KeyId => _config["Razorpay:KeyId"] ?? "";
    private string KeySecret => _config["Razorpay:KeySecret"] ?? "";

    // POST /api/payments/create-order  -> Razorpay pe order banao
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] RazorpayOrderRequest req)
    {
        if (req.Amount <= 0) return BadRequest("Invalid amount.");

        var http = _httpFactory.CreateClient();
        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{KeyId}:{KeySecret}"));
        http.DefaultRequestHeaders.Authorization = new("Basic", auth);

        var body = new
        {
            amount = (int)(req.Amount * 100), // paise me
            currency = "INR",
            receipt = "rcpt_" + DateTime.UtcNow.Ticks,
        };

        var res = await http.PostAsJsonAsync("https://api.razorpay.com/v1/orders", body);
        var json = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            return StatusCode((int)res.StatusCode, json);

        return Content(json, "application/json"); // isme "id": "order_xxx"
    }

    // POST /api/payments/verify  -> payment signature verify (fraud se bachao)
    [HttpPost("verify")]
    public IActionResult Verify([FromBody] RazorpayVerifyRequest req)
    {
        var payload = $"{req.RazorpayOrderId}|{req.RazorpayPaymentId}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(KeySecret));
        var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();

        var ok = hash == req.RazorpaySignature?.ToLowerInvariant();
        return ok ? Ok(new { success = true }) : BadRequest(new { success = false });
    }
}
