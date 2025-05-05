using api.DTOs.Order;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/order")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userIdClaim == null)
                return Guid.Empty;

            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Orders retrieved successfully",
                result.Orders
            });
        }

        [HttpGet("me")]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            var result = await _orderService.GetOrdersByUserIdAsync(userId);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "User orders retrieved successfully",
                result.Orders
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid id)
        {
            // Kiểm tra xem người dùng có phải là admin hay không hoặc nếu người dùng đang truy cập đơn hàng của chính họ
            bool isAdmin = User.IsInRole("admin");
            Guid userId = GetCurrentUserId();

            var result = await _orderService.GetOrderByIdAsync(id);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Order not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            // Xác minh người dùng có quyền truy cập vào đơn hàng này hay không
            if (!isAdmin && result.Order?.UserId != userId)
            {
                return Forbid();
            }

            return Ok(new
            {
                Message = "Order retrieved successfully",
                result.Order
            });
        }

        [HttpPost]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO createOrderDTO)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            var result = await _orderService.CreateOrderAsync(createOrderDTO, userId);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return CreatedAtAction(nameof(GetOrderById),
                                new { id = result.Order!.Id },
                                new
                                {
                                    Message = "Order created successfully",
                                    result.Order
                                });
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] Guid id, [FromBody] UpdateOrderStatusDTO updateOrderStatusDTO)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, updateOrderStatusDTO);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Order not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Order status updated successfully",
                result.Order
            });
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOrder([FromRoute] Guid id)
        {
            var result = await _orderService.DeleteOrderAsync(id);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Order not found" }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return NoContent();
        }
    }
}