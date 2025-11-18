namespace HeimdallWeb.Services;

public static class ImageService
{
    private static readonly long _maxSizeInBytes = 2097152; // 2 MB

    public static bool IsImageFile(this IFormFile file)
    {
        // List of allowed image MIME types
        var allowedMimeTypes = new List<string>
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        return allowedMimeTypes.Contains(file.ContentType);
    }

    public static bool IsFileSizeInvalid(this IFormFile file)
    {
        return file.Length >= _maxSizeInBytes;
    }

    // Existing simple save method kept intact
    public async static Task<string> SaveProfileImageAsync(this IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new IOException("Houve um erro ao fazer upload do arquivo");

        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "users");

        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        string uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(uploadsFolder, uniqueName);

        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/users/{uniqueName}";
    }


    public static bool DeleteOldProfileImage(string imagePath)
    {
        try
        {
            // If the provided path is null/empty do nothing
            if (string.IsNullOrWhiteSpace(imagePath))
                return false;

            // Normalize the relative path and build the absolute path under wwwroot
            string relativePath = imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            // Delete the file if it exists
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

        }
        catch(ArgumentNullException)
        {
            throw new ArgumentNullException("Ocorreu um erro ao deletar a imagem de perfil antiga.");
        }
        catch (IOException)
        {
            throw new IOException("Ocorreu um erro ao deletar a imagem de perfil antiga.");
        }

        return true;
    }

}
