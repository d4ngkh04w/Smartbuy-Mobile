namespace api.Helpers
{
    public static class ImageHelper
    {
        private const string DEFAULT_ROOT_PATH = "wwwroot";
        private const string DEFAULT_IMAGES_FOLDER = "uploads/images";

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
            string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), DEFAULT_ROOT_PATH, DEFAULT_IMAGES_FOLDER, folder);

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

                return (true, null, $"/{DEFAULT_IMAGES_FOLDER}/{folder}/{fileName}");
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
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}