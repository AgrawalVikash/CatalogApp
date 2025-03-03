using Catalog.Service.Interface;
using Microsoft.AspNetCore.Mvc;

namespace CatalogWebApp.Controllers;

[ApiController]
[Route("api/products")]
public class CatalogController : ControllerBase
{
    private readonly ILogger<CatalogController> _logger;
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService, ILogger<CatalogController> logger)
    {
        _catalogService = catalogService;
        _logger = logger;
    }

    [HttpGet("GetProducts")]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string productCode = "")
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and pageSize must be greater than 0.");
        }

        try
        {
            var (products, totalPages, totalItems) = await _catalogService.GetProductsAsync(page, pageSize, productCode);

            var response = new
            {
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                Products = products
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to retrieve products: {ex.Message}");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
}
