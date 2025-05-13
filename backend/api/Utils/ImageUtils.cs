using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using api.Exceptions;
using Microsoft.AspNetCore.Http;

namespace api.Utils
{
    public static class ImageUtils
    {
        public static bool IsImage(this IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            string[] validImageTypes = {
                "image/jpeg", "image/jpg", "image/pjpeg",
                "image/png",
                "image/svg+xml"
            };

            if (!validImageTypes.Contains(file.ContentType))
            {
                return false;
            }

            return true;
        }

        public static async Task<string> SaveImageAsync(IFormFile file, string webRootPath, string folder, long maxSize)
        {
            if (!file.IsImage())
            {
                throw new BadRequestException("Invalid image format");
            }

            if (file.Length > maxSize)
            {
                throw new BadRequestException($"File size must be less than {maxSize / 1024 / 1024} MB");
            }

            string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string uploadFolder = $"{webRootPath}/uploads/images/{folder}";

            try
            {
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                string filePath = $"{uploadFolder}/{fileName}";

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                return $"/uploads/images/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new ServerException("Error saving image: " + ex.Message);
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