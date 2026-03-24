namespace Couture.Identity.Services;

public sealed class JwtOptions
{
    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = "CoutureWorkshop";
    public string Audience { get; set; } = "CoutureWorkshop";
    public int TokenExpirationInMinutes { get; set; } = 480;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
}
