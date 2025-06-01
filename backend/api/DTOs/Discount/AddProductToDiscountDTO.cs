using System.ComponentModel.DataAnnotations;

namespace api.DTOs.Discount
{
    public class AddProductToDiscountDTO
    {
        [Required]
        public List<int> ProductIds { get; set; } = new List<int>();
    }
}