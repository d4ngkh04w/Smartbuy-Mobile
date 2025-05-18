using api.DTOs.Discount;
using api.Models;
using api.Utils;

namespace api.Mappers
{
    public static class DiscountMapper
    {
        public static DiscountDTO ToDTO(this Discount discount)
        {
            return new DiscountDTO
            {
                Id = discount.Id,
                DiscountPercentage = discount.DiscountPercentage,
                DiscountAmount = discount.DiscountAmount,
                StartDate = DateTimeUtils.FormatDateTime(discount.StartDate),
                EndDate = DateTimeUtils.FormatDateTime(discount.EndDate),
                Status = DateTime.Now < discount.StartDate ? "Upcoming" :
                         DateTime.Now > discount.EndDate ? "Expired" : "Active"
            };
        }

        public static Discount ToModel(this CreateDiscountDTO discountDTO)
        {
            return new Discount
            {
                DiscountPercentage = discountDTO.DiscountPercentage,
                DiscountAmount = discountDTO.DiscountAmount,
                StartDate = discountDTO.StartDate,
                EndDate = discountDTO.EndDate
            };
        }
    }
}
