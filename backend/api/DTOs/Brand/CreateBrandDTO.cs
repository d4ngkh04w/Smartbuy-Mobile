namespace api.DTOs.Brand
{
    public class CreateBrandDTO
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? Logo { get; set; } = null;
    }
}