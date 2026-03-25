using Microsoft.AspNetCore.Identity;

namespace Couture.Identity.Domain;

public sealed class 
    CoutureUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public bool TwoFactorSmsEnabled { get; set; }
    public int SessionDurationHours { get; set; } = 8;
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public string FullName => $"{FirstName} {LastName}";
}
