using api.DTOs.Cart;
using api.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/v1/cart")]
    [ApiController]
    [Authorize(Roles = "user")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userIdClaim == null)
                return Guid.Empty;

            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetCurrentUserId();

            var result = await _cartService.GetCartAsync(userId);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Cart retrieved successfully",
                result.Cart
            });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            var result = await _cartService.AddToCartAsync(userId, dto);

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
                Message = "Product added to cart successfully",
                result.Cart
            });
        }

        [HttpPut("items/{itemId:int}")]
        public async Task<IActionResult> UpdateCartItem([FromRoute] int itemId, [FromBody] UpdateCartItemDTO dto)
        {
            var userId = GetCurrentUserId();

            var result = await _cartService.UpdateCartItemAsync(userId, itemId, dto);

            if (!result.Success && result.ErrorMessage != null)
            {
                // Trường hợp này có thể xảy ra khi số lượng sản phẩm trong kho không đủ
                if (result.Cart != null)
                {
                    return Ok(new
                    {
                        Message = result.ErrorMessage,
                        result.Cart
                    });
                }

                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new
            {
                Message = "Cart item updated successfully",
                result.Cart
            });
        }

        [HttpDelete("items/{itemId:int}")]
        public async Task<IActionResult> RemoveCartItem([FromRoute] int itemId)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            var result = await _cartService.RemoveCartItemAsync(userId, itemId);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new { Message = "Cart item removed successfully" });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
                return Unauthorized(new { Message = "User not authenticated" });

            var result = await _cartService.ClearCartAsync(userId);

            if (!result.Success && result.ErrorMessage != null)
            {
                return result.ErrorMessage switch
                {
                    string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
                    _ => BadRequest(new { Message = result.ErrorMessage })
                };
            }

            return Ok(new { Message = "Cart cleared successfully" });
        }
    }
}