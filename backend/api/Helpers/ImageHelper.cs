namespace api.Helpers
{
    public static class ImageHelper
    {
        public static async Task<(bool Success, string? ErrorMessage, string? FilePath)> SaveImageAsync(IFormFile file, string folder, long maxSize)
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
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "images", folder);

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            string filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return (true, null, $"/uploads/images/{folder}/{fileName}");
        }
        
        public static bool DeleteImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                return false;
            }

            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}