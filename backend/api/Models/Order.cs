using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalAmount { get; set; }
        public Guid UserId { get; set; }
        [Column(TypeName = "enum('Chờ xác nhận', 'Đã xác nhận', 'Đang giao hàng', 'Đã giao hàng', 'Đã huỷ', 'Đã hoàn tiền', 'Đã trả hàng', 'Hoàn thành')")]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "enum('COD', 'Banking')")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12, 2)")]
        public decimal ShippingFee { get; set; } = 0;

        [Column(TypeName = "timestamp")]
        public DateTime OrderDate { get; set; }

        [Column(TypeName = "timestamp")]
        public DateTime? DeliveryDate { get; set; } = null;

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}