using Microsoft.AspNetCore.Identity;

namespace Couture.Identity.Domain;

public sealed class CoutureRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public CoutureRole() { }
    public CoutureRole(string name, string? description = null) : base(name)
    {
        Description = description;
    }
}
