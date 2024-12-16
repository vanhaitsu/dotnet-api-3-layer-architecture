using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Services.Interfaces;
using Services.Utils;

namespace Services.Helpers;

public class CloudinaryHelper : ICloudinaryHelper
{
    private readonly ICloudinary _cloudinary;

    public CloudinaryHelper(ICloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task<string> UploadImageAsync(IFormFile file, string? name = null, string? publicId = null,
        bool? overwrite = true)
    {
        var parameters = new ImageUploadParams
        {
            File = new FileDescription(name ?? file.FileName, file.OpenReadStream()),
            PublicId = publicId ?? AuthenticationTools.GenerateUniqueToken(),
            Overwrite = overwrite
        };

        var result = await _cloudinary.UploadAsync(parameters);
        return result.SecureUrl.ToString();
    }
}