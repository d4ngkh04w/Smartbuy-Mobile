namespace api.DTOs.Brand
{
    public class UpdateBrandDTO
    {
        public string? Name { get; set; } = null;
        public IFormFile? Logo { get; set; } = null;
    }
}