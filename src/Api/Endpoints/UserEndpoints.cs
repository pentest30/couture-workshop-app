using Couture.Identity.Contracts;
using Couture.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Couture.Api.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/", ListUsers); // Any authenticated user can list (needed for artisan pickers)
        group.MapGet("/{id:guid}", GetUser);
        group.MapPost("/", CreateUser).RequireAuthorization(CouturePermissions.UsersManage);
        group.MapPut("/{id:guid}", UpdateUser).RequireAuthorization(CouturePermissions.UsersManage);
        group.MapDelete("/{id:guid}", DeactivateUser).RequireAuthorization(CouturePermissions.UsersManage);
    }

    private static async Task<IResult> ListUsers(
        UserManager<CoutureUser> userManager,
        [FromQuery] string? search,
        [FromQuery] bool? activeOnly,
        [FromQuery] string? role)
    {
        var query = userManager.Users.AsNoTracking();

        if (activeOnly == true)
            query = query.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(s) ||
                u.LastName.ToLower().Contains(s) ||
                u.Email!.ToLower().Contains(s));
        }

        var users = await query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName).ToListAsync();

        var items = new List<UserListItem>();
        foreach (var u in users)
        {
            var roles = await userManager.GetRolesAsync(u);
            // Filter by role after fetching roles (Identity stores roles separately)
            if (!string.IsNullOrWhiteSpace(role) && !roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                continue;
            items.Add(new UserListItem(u.Id, u.UserName!, u.FirstName, u.LastName, u.Email!,
                u.PhoneNumber, roles.ToList(), u.IsActive, u.LastLoginAt, u.CreatedAt));
        }

        return Results.Ok(new { items, totalCount = items.Count });
    }

    private static async Task<IResult> GetUser(Guid id, UserManager<CoutureUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return Results.NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Results.Ok(new UserListItem(user.Id, user.UserName!, user.FirstName, user.LastName,
            user.Email!, user.PhoneNumber, roles.ToList(), user.IsActive, user.LastLoginAt, user.CreatedAt));
    }

    private static async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest req,
        UserManager<CoutureUser> userManager)
    {
        var user = new CoutureUser
        {
            UserName = req.Email,
            Email = req.Email,
            FirstName = req.FirstName,
            LastName = req.LastName,
            PhoneNumber = req.Phone,
            IsActive = true,
        };

        var result = await userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            return Results.BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });

        if (req.Roles.Count > 0)
            await userManager.AddToRolesAsync(user, req.Roles);

        return Results.Created($"/api/users/{user.Id}", new { id = user.Id, userName = user.UserName, fullName = user.FullName });
    }

    private static async Task<IResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserRequest req,
        UserManager<CoutureUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return Results.NotFound();

        if (req.FirstName is not null) user.FirstName = req.FirstName;
        if (req.LastName is not null) user.LastName = req.LastName;
        if (req.Phone is not null) user.PhoneNumber = req.Phone;
        if (req.IsActive.HasValue) user.IsActive = req.IsActive.Value;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Results.BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });

        // Update roles if provided
        if (req.Roles is not null)
        {
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            if (req.Roles.Count > 0)
                await userManager.AddToRolesAsync(user, req.Roles);
        }

        // Reset password if provided
        if (!string.IsNullOrWhiteSpace(req.NewPassword))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var passResult = await userManager.ResetPasswordAsync(user, token, req.NewPassword);
            if (!passResult.Succeeded)
                return Results.BadRequest(new { error = string.Join("; ", passResult.Errors.Select(e => e.Description)) });
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateUser(Guid id, UserManager<CoutureUser> userManager)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null) return Results.NotFound();

        user.IsActive = false;
        await userManager.UpdateAsync(user);
        return Results.NoContent();
    }
}

public record CreateUserRequest(string FirstName, string LastName, string Email, string Password, string? Phone, List<string> Roles);
public record UpdateUserRequest(string? FirstName, string? LastName, string? Phone, bool? IsActive, List<string>? Roles, string? NewPassword);
public record UserListItem(Guid Id, string UserName, string FirstName, string LastName, string Email,
    string? Phone, List<string> Roles, bool IsActive, DateTimeOffset? LastLoginAt, DateTimeOffset CreatedAt);
