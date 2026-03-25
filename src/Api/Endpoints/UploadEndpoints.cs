namespace Couture.Api.Endpoints;

public static class UploadEndpoints
{
    public static void MapUploadEndpoints(this WebApplication app)
    {
        app.MapPost("/api/uploads", async (HttpRequest request, IWebHostEnvironment env) =>
        {
            var form = await request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();
            if (file is null || file.Length == 0)
                return Results.BadRequest(new { error = "No file provided." });

            // Validate file type
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return Results.BadRequest(new { error = "File type not allowed. Use JPG, PNG, WebP or GIF." });

            // Max 5MB
            if (file.Length > 5 * 1024 * 1024)
                return Results.BadRequest(new { error = "File too large. Max 5MB." });

            // Save to wwwroot/uploads/{year}/{guid}{ext}
            var year = DateTime.UtcNow.Year;
            var fileName = $"{Guid.NewGuid()}{ext}";
            var relativePath = $"uploads/{year}/{fileName}";
            var fullPath = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads", year.ToString());

            Directory.CreateDirectory(fullPath);
            var filePath = Path.Combine(fullPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Results.Ok(new
            {
                fileName = file.FileName,
                storagePath = $"/{relativePath}",
                size = file.Length,
            });
        })
        .DisableAntiforgery()
        .RequireAuthorization();
    }
}
