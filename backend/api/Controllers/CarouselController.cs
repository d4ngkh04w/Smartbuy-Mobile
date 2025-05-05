// using api.DTOs.Carousel;
// using api.Interfaces.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;

// namespace api.Controllers
// {
//     [Route("api/v1/carousel")]
//     [ApiController]
//     [Authorize]
//     public class CarouselController : ControllerBase
//     {
//         private readonly ICarouselService _carouselService;

//         public CarouselController(ICarouselService carouselService)
//         {
//             _carouselService = carouselService;
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<IActionResult> GetCarousels()
//         {
//             var result = await _carouselService.GetAllAsync();

//             if (!result.Success && result.ErrorMessage != null)
//             {
//                 return result.ErrorMessage switch
//                 {
//                     string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = result.ErrorMessage }),
//                     string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
//                     _ => BadRequest(new { Message = result.ErrorMessage })
//                 };
//             }

//             return Ok(new
//             {
//                 Message = "Carousels retrieved successfully",
//                 result.Carousels
//             });
//         }

//         [HttpGet("{id:int}")]
//         [AllowAnonymous]
//         public async Task<IActionResult> GetCarouselById([FromRoute] int id)
//         {
//             var result = await _carouselService.GetByIdAsync(id);
//             if (!result.Success && result.ErrorMessage != null)
//             {
//                 return result.ErrorMessage switch
//                 {
//                     string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Carousel not found" }),
//                     string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
//                     _ => BadRequest(new { Message = result.ErrorMessage })
//                 };
//             }
//             return Ok(new
//             {
//                 Message = "Carousel retrieved successfully",
//                 result.Carousel
//             });
//         }

//         [HttpPost]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> CreateCarousel([FromForm] IFormFile imageFile, [FromForm] CarouselDTO dto)
//         {
//             var result = await _carouselService.CreateAsync(imageFile, dto);

//             if (!result.Success && result.ErrorMessage != null)
//             {
//                 return result.ErrorMessage switch
//                 {
//                     string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase)
//                         => StatusCode(500, new { Message = result.ErrorMessage }),
//                     _ => BadRequest(new { Message = result.ErrorMessage })
//                 };
//             }

//             return CreatedAtAction(nameof(GetCarouselById),
//                                 new { id = result.Carousel!.Id },
//                                 new
//                                 {
//                                     Message = "Carousel created successfully",
//                                     result.Carousel
//                                 });
//         }


//         [HttpPut("{id:int}")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> UpdateCarousel([FromRoute] int id, [FromForm] IFormFile? imageFile, [FromForm] CarouselDTO dto)
//         {
//             var result = await _carouselService.UpdateAsync(id, imageFile, dto);

//             if (!result.Success && result.ErrorMessage != null)
//             {
//                 return result.ErrorMessage switch
//                 {
//                     string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase)
//                         => NotFound(new { Message = "Carousel not found" }),
//                     string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase)
//                         => StatusCode(500, new { Message = result.ErrorMessage }),
//                     _ => BadRequest(new { Message = result.ErrorMessage })
//                 };
//             }

//             return Ok(new
//             {
//                 Message = "Carousel updated successfully",
//                 result.Carousel
//             });
//         }

//         [HttpDelete("{id:int}")]
//         [Authorize(Roles = "admin")]
//         public async Task<IActionResult> DeleteCarousel(int id)
//         {
//             var result = await _carouselService.DeleteAsync(id);
//             if (!result.Success && result.ErrorMessage != null)
//             {
//                 return result.ErrorMessage switch
//                 {
//                     string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Carousel not found" }),
//                     string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
//                     _ => BadRequest(new { Message = result.ErrorMessage })
//                 };
//             }
//             return NoContent();
//         }

//         [HttpGet("active")]
//         [AllowAnonymous]
//         public async Task<IActionResult> GetActiveCarousels()
//         {
//             var result = await _carouselService.GetActiveAsync();
//             if (!result.Success && result.ErrorMessage != null)
//             {
//                 return result.ErrorMessage switch
//                 {
//                     string msg when msg.Contains("Not found", StringComparison.OrdinalIgnoreCase) => NotFound(new { Message = "Active carousels not found" }),
//                     string msg when msg.Contains("Error", StringComparison.OrdinalIgnoreCase) => StatusCode(500, new { Message = result.ErrorMessage }),
//                     _ => BadRequest(new { Message = result.ErrorMessage })
//                 };
//             }
//             return Ok(new
//             {
//                 Message = "Active carousels retrieved successfully",
//                 result.Carousels
//             });
//         }
//     }
// }
