using LivestockLink.Core.Entities;
using LivestockLink.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivestockLink.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetSuppliers()
    {
        var suppliers = await _context.SupplierProfiles
            .Where(s => s.IsApproved)
            .Select(s => new
            {
                s.Id,
                s.BusinessName,
                s.FarmLocation,
                s.State,
                s.DeliveryCoverage,
                s.Description,
                ProductCount = s.Products.Count(p => p.IsActive)
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetSupplierById(Guid id)
    {
        var supplier = await _context.SupplierProfiles
            .Include(s => s.Products.Where(p => p.IsActive))
            .FirstOrDefaultAsync(s => s.Id == id && s.IsApproved);

        if (supplier == null) return NotFound("Supplier not found.");

        return Ok(new
        {
            supplier.Id,
            supplier.BusinessName,
            supplier.FarmLocation,
            supplier.State,
            supplier.DeliveryCoverage,
            supplier.Description,
            Products = supplier.Products.Select(p => new
            {
                p.Id,
                p.Title,
                p.Category,
                CategoryName = p.Category.ToString(),
                p.ProductForm,
                ProductFormName = p.ProductForm.ToString(),
                p.Unit,
                p.PricePerUnit,
                p.AvailableQuantity,
                p.MinimumOrderQuantity,
                p.DeliveryCoverage,
                p.Description,
                p.ImageUrl
            })
        });
    }
}
