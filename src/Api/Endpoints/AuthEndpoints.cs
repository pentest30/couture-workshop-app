using Couture.Identity.Contracts;
using Couture.Identity.Domain;
using Couture.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Couture.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", Login).AllowAnonymous();
        group.MapGet("/me", GetCurrentUser).RequireAuthorization();
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        UserManager<CoutureUser> userManager,
        TokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null || !user.IsActive)
            return Results.Unauthorized();

        var validPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            return Results.Unauthorized();

        user.LastLoginAt = DateTimeOffset.UtcNow;
        await userManager.UpdateAsync(user);

        var (accessToken, refreshToken, expiry) = await tokenService.GenerateTokenAsync(user);

        return Results.Ok(new LoginResponse(
            accessToken,
            refreshToken,
            expiry,
            user.Id,
            user.UserName!,
            user.FullName,
            (await userManager.GetRolesAsync(user)).ToList()));
    }

    private static async Task<IResult> GetCurrentUser(
        ICurrentUser currentUser,
        UserManager<CoutureUser> userManager)
    {
        var user = await userManager.FindByIdAsync(currentUser.UserId.ToString());
        if (user is null) return Results.NotFound();

        var roles = await userManager.GetRolesAsync(user);

        return Results.Ok(new UserProfile(
            user.Id,
            user.UserName!,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            roles.ToList(),
            user.TwoFactorSmsEnabled,
            user.SessionDurationHours,
            user.LastLoginAt));
    }
}

public record LoginRequest(string Email, string Password);
public record LoginResponse(string AccessToken, string RefreshToken, DateTime Expiry, Guid UserId, string UserName, string FullName, List<string> Roles);
public record UserProfile(Guid Id, string UserName, string FirstName, string LastName, string? Phone, List<string> Roles, bool TwoFactorEnabled, int SessionDurationHours, DateTimeOffset? LastLoginAt);
