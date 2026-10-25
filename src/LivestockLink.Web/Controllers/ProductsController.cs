using System.Security.Claims;
using LivestockLink.Core.DTOs;
using LivestockLink.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestockLink.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] ProductFilterRequest? filter)
    {
        var products = await _productService.GetProductsAsync(filter);
        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound("Product listing not found.");
        }
        return Ok(product);
    }

    [HttpGet("my-listings")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetMyListings()
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var products = await _productService.GetSupplierProductsAsync(profileId);
        return Ok(products);
    }

    [HttpPost]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var created = await _productService.CreateProductAsync(profileId, request);
        return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var updated = await _productService.UpdateProductAsync(profileId, id, request);
        return Ok(updated);
    }

    [HttpPatch("{id:guid}/toggle-active")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult<bool>> ToggleActive(Guid id)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var isActive = await _productService.ToggleProductActiveAsync(profileId, id);
        return Ok(new { id, isActive });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Supplier")]
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var profileId = GetProfileId();
        if (profileId == Guid.Empty)
        {
            return Unauthorized("Supplier profile not associated with account.");
        }

        var result = await _productService.DeleteProductAsync(profileId, id);
        if (!result)
        {
            return NotFound("Product listing not found or could not be removed.");
        }

        return NoContent();
    }

    private Guid GetProfileId()
    {
        var profileIdClaim = User.FindFirstValue("profileId");
        return Guid.TryParse(profileIdClaim, out var pid) ? pid : Guid.Empty;
    }
}
