using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using WristMatch.Domain;
using WristMatch.Infrastructure;

namespace WristMatch.Api.Controllers;

[Authorize(Roles = "SuperAdmin,CatalogManager")]
[ApiController]
[Route("api/v1/admin/products/{productId:guid}/assets")]
public sealed class AdminProductAssetsController(WristMatchDbContext db, UserManager<AdminUser> users) : ControllerBase
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<IActionResult> Upload(Guid productId, [FromForm] IFormFile? file, [FromForm] string? source, [FromForm] string? licenceNote, [FromForm] string? attribution, [FromForm] bool primary = true, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
        if (product is null)
            return NotFound();
        if (file is null || file.Length is <= 0 or > 8 * 1024 * 1024 || string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(licenceNote))
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid asset", detail: "Provide a JPEG or PNG image up to 8 MiB with source and licence details.");

        await using var input = file.OpenReadStream();
        try
        {
            var format = await Image.DetectFormatAsync(input, cancellationToken);
            if (format.Name is not "JPEG" and not "PNG")
                return Problem(statusCode: StatusCodes.Status415UnsupportedMediaType, title: "Unsupported image", detail: "Product images must be JPEG or PNG.");
            input.Position = 0;
            using var image = await Image.LoadAsync(input, cancellationToken);
            if (image.Width > 4096 || image.Height > 4096 || (long)image.Width * image.Height > 16_000_000)
                return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid dimensions", detail: "The image dimensions are not supported.");

            var userId = Guid.Parse(users.GetUserId(User)!);
            var now = DateTimeOffset.UtcNow;
            var storageKey = Path.Combine(now.ToString("yyyyMMdd"), $"{Guid.NewGuid():N}.jpg");
            var root = Path.Combine(AppContext.BaseDirectory, "catalogue-media");
            var path = Path.Combine(root, storageKey);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            image.Metadata.ExifProfile = null; image.Metadata.IptcProfile = null; image.Metadata.XmpProfile = null;
            await image.SaveAsJpegAsync(path, new JpegEncoder { Quality = 92 }, cancellationToken);

            if (primary)
                foreach (var current in await db.ProductAssets.Where(asset => asset.ProductId == productId && asset.IsPrimary).ToListAsync(cancellationToken))
                    current.RemovePrimary();
            var asset = new ProductAsset(productId, storageKey, "image/jpeg", image.Width, image.Height, new FileInfo(path).Length, userId, now);
            asset.Approve(primary);
            db.ProductAssets.Add(asset);
            db.ProductAssetLicences.Add(new ProductAssetLicence(asset.Id, source.Trim(), licenceNote.Trim(), string.IsNullOrWhiteSpace(attribution) ? null : attribution.Trim(), userId, now));
            if (primary)
                product.SetPrimaryImage($"/api/v1/product-assets/{asset.Id}/image", false);
            await db.SaveChangesAsync(cancellationToken);
            return Created($"/api/v1/product-assets/{asset.Id}/image", new { asset.Id, asset.Width, asset.Height, asset.ByteSize, asset.IsApproved, asset.IsPrimary, imageUrl = $"/api/v1/product-assets/{asset.Id}/image" });
        }
        catch (UnknownImageFormatException)
        {
            return Problem(statusCode: StatusCodes.Status415UnsupportedMediaType, title: "Unsupported image", detail: "Product images must be valid JPEG or PNG files.");
        }
        catch (InvalidImageContentException)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "Corrupt image", detail: "The product image could not be decoded.");
        }
    }

    [HttpPost("{assetId:guid}/tryon-ready")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableTryOn(Guid productId, Guid assetId, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(item => item.Id == productId, cancellationToken);
        var asset = await db.ProductAssets.SingleOrDefaultAsync(item => item.Id == assetId && item.ProductId == productId && item.IsApproved && item.IsPrimary, cancellationToken);
        if (product is null || asset is null)
            return NotFound();
        product.SetTryOnReady(true);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}

[ApiController]
[Route("api/v1/product-assets")]
public sealed class ProductAssetsController(WristMatchDbContext db) : ControllerBase
{
    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken cancellationToken)
    {
        var asset = await db.ProductAssets.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && item.IsApproved && item.DeletedAt == null, cancellationToken);
        if (asset is null)
            return NotFound();
        var root = Path.Combine(AppContext.BaseDirectory, "catalogue-media");
        var path = Path.GetFullPath(Path.Combine(root, asset.StorageKey));
        if (!path.StartsWith(Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(path))
            return NotFound();
        return PhysicalFile(path, asset.MimeType, enableRangeProcessing: true);
    }
}
