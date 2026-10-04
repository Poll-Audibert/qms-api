namespace api.DTOs;

public class LoginDto
{
    public string email { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string? device_code { get; set; }
}