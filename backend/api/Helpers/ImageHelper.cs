namespace api.Helpers
{
    public static class ImageHelper
    {
        public static async Task<(bool Success, string? ErrorMessage, string? FilePath)> SaveImageAsync(IFormFile file, string webRootPath, string folder, long maxSize)
        {
            if (file == null || file.Length == 0)
            {
                return (false, "File is empty", null);
            }

            if (!file.ContentType.StartsWith("image/"))
            {
                return (false, "File must be an image", null);
            }

            if (file.Length > maxSize)
            {
                return (false, $"File size must be less than {maxSize / 1024 / 1024} MB", null);
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string uploadFolder = Path.Combine(webRootPath, "uploads", "images", folder);

            try
            {
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath = Path.Combine(uploadFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, null, Path.Combine("uploads", "images", folder, fileName));
            }
            catch (Exception)
            {
                return (false, "Error saving file", null);
            }
        }

        public static bool DeleteImage(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("[INF] File deleted successfully");
                    return true;
                }
                Console.WriteLine("[ERR] File not found");
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}