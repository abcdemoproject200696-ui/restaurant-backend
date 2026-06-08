using System.Collections.Concurrent;

namespace RestaurantBackend.Services;

// OTP yaad rakhne wala simple in-memory store (phone -> otp).
// Server restart par reset ho jaata hai (demo ke liye theek hai).
public class OtpService
{
    private readonly ConcurrentDictionary<string, string> _otps = new();
    private readonly Random _rng = new();

    // 6-digit OTP banao aur phone ke against save karo
    public string Generate(string phone)
    {
        var otp = _rng.Next(100000, 1000000).ToString();
        _otps[phone] = otp;
        return otp;
    }

    // Jo OTP aaya wo save kiye gaye se match karta hai?
    public bool Verify(string phone, string otp)
        => _otps.TryGetValue(phone, out var saved) && saved == otp;
}
