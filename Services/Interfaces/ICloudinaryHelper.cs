using Microsoft.AspNetCore.Http;

namespace Services.Interfaces;

public interface ICloudinaryHelper
{
    Task<string> UploadImageAsync(IFormFile file, string? name = null, string? publicId = null, bool? overwrite = true);
}