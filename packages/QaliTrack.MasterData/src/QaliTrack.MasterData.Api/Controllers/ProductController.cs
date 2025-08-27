using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.Product.Entities;
using QaliTrack.MasterData.Core.Modules.Product.DTOs;
using QaliTrack.MasterData.Infrastructure.Data;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Tags("Product Module")]
public class ProductController : ControllerBase
{
    private readonly MasterDataDbContext _context;

    public ProductController(MasterDataDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get products with Django-style filtering, searching, and pagination
    /// </summary>
    /// <param name="queryParams">Query parameters for filtering, search, and pagination</param>
    /// <returns>Paginated list of products</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductSummaryDto>>>> GetProducts(
        [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.Products
                .Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Category = p.Category,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    Status = p.Status,
                    Brand = p.Brand,
                    MinStockLevel = p.MinStockLevel,
                    CreatedAt = p.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ProductSummaryDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ProductSummaryDto>>.ErrorResponse("Error retrieving products", ex.Message));
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProduct(Guid id)
    {
        try
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Category = p.Category,
                    SubCategory = p.SubCategory,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    Status = p.Status,
                    Description = p.Description,
                    Brand = p.Brand,
                    Model = p.Model,
                    Weight = p.Weight,
                    Currency = p.Currency,
                    Dimensions = p.Dimensions,
                    Color = p.Color,
                    Material = p.Material,
                    IsHazardous = p.IsHazardous,
                    HazardClass = p.HazardClass,
                    StorageRequirements = p.StorageRequirements,
                    MinStockLevel = p.MinStockLevel,
                    MaxStockLevel = p.MaxStockLevel,
                    ReorderLevel = p.ReorderLevel,
                    QualityStandards = p.QualityStandards,
                    Certifications = p.Certifications,
                    OrganizationId = p.OrganizationId,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    HasSpecifications = p.Specifications.Any(),
                    HasDocuments = p.Documents.Any(d => d.IsActive),
                    IsLowStock = p.MinStockLevel > 0 && p.MaxStockLevel > 0 && p.MaxStockLevel < p.MinStockLevel
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(ApiResponse<ProductDetailDto>.ErrorResponse("Product not found"));
            }

            return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(product));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDetailDto>.ErrorResponse("Error retrieving product", ex.Message));
        }
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> CreateProduct(CreateProductDto dto)
    {
        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Code = dto.Code,
                Category = dto.Category,
                SubCategory = dto.SubCategory ?? string.Empty,
                Unit = dto.Unit,
                UnitPrice = dto.UnitPrice,
                OrganizationId = dto.OrganizationId,
                Description = dto.Description ?? string.Empty,
                Brand = dto.Brand ?? string.Empty,
                Model = dto.Model ?? string.Empty,
                Weight = dto.Weight,
                Currency = dto.Currency,
                Dimensions = dto.Dimensions,
                Color = dto.Color,
                Material = dto.Material,
                IsHazardous = dto.IsHazardous,
                HazardClass = dto.HazardClass,
                StorageRequirements = dto.StorageRequirements,
                MinStockLevel = dto.MinStockLevel,
                MaxStockLevel = dto.MaxStockLevel,
                ReorderLevel = dto.ReorderLevel,
                QualityStandards = dto.QualityStandards,
                Certifications = dto.Certifications,
                Notes = dto.Notes,
                Status = "Active"
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var responseDto = new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Code = product.Code,
                Category = product.Category,
                SubCategory = product.SubCategory,
                Unit = product.Unit,
                UnitPrice = product.UnitPrice,
                Status = product.Status,
                Description = product.Description,
                Brand = product.Brand,
                Model = product.Model,
                Weight = product.Weight,
                Currency = product.Currency,
                Dimensions = product.Dimensions,
                Color = product.Color,
                Material = product.Material,
                IsHazardous = product.IsHazardous,
                HazardClass = product.HazardClass,
                StorageRequirements = product.StorageRequirements,
                MinStockLevel = product.MinStockLevel,
                MaxStockLevel = product.MaxStockLevel,
                ReorderLevel = product.ReorderLevel,
                QualityStandards = product.QualityStandards,
                Certifications = product.Certifications,
                OrganizationId = product.OrganizationId,
                Notes = product.Notes,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                HasSpecifications = false,
                HasDocuments = false,
                IsLowStock = false
            };

            return CreatedAtAction(nameof(GetProduct), 
                new { id = product.Id }, 
                ApiResponse<ProductDetailDto>.SuccessResponse(responseDto, "Product created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDetailDto>.ErrorResponse("Error creating product", ex.Message));
        }
    }

    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> UpdateProduct(Guid id, UpdateProductDto dto)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductDetailDto>.ErrorResponse("Product not found"));
            }

            product.Name = dto.Name;
            product.Category = dto.Category;
            product.SubCategory = dto.SubCategory ?? product.SubCategory;
            product.Unit = dto.Unit;
            product.UnitPrice = dto.UnitPrice;
            product.Status = dto.Status;
            product.Description = dto.Description ?? product.Description;
            product.Brand = dto.Brand ?? product.Brand;
            product.Model = dto.Model ?? product.Model;
            product.Weight = dto.Weight;
            product.Currency = dto.Currency;
            product.Dimensions = dto.Dimensions;
            product.Color = dto.Color;
            product.Material = dto.Material;
            product.IsHazardous = dto.IsHazardous;
            product.HazardClass = dto.HazardClass;
            product.StorageRequirements = dto.StorageRequirements;
            product.MinStockLevel = dto.MinStockLevel;
            product.MaxStockLevel = dto.MaxStockLevel;
            product.ReorderLevel = dto.ReorderLevel;
            product.QualityStandards = dto.QualityStandards;
            product.Certifications = dto.Certifications;
            product.Notes = dto.Notes;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductDetailDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Category = p.Category,
                    SubCategory = p.SubCategory,
                    Unit = p.Unit,
                    UnitPrice = p.UnitPrice,
                    Status = p.Status,
                    Description = p.Description,
                    Brand = p.Brand,
                    Model = p.Model,
                    Weight = p.Weight,
                    Currency = p.Currency,
                    Dimensions = p.Dimensions,
                    Color = p.Color,
                    Material = p.Material,
                    IsHazardous = p.IsHazardous,
                    HazardClass = p.HazardClass,
                    StorageRequirements = p.StorageRequirements,
                    MinStockLevel = p.MinStockLevel,
                    MaxStockLevel = p.MaxStockLevel,
                    ReorderLevel = p.ReorderLevel,
                    QualityStandards = p.QualityStandards,
                    Certifications = p.Certifications,
                    OrganizationId = p.OrganizationId,
                    Notes = p.Notes,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    HasSpecifications = p.Specifications.Any(),
                    HasDocuments = p.Documents.Any(d => d.IsActive),
                    IsLowStock = p.MinStockLevel > 0 && p.MaxStockLevel > 0 && p.MaxStockLevel < p.MinStockLevel
                })
                .FirstOrDefaultAsync();

            return Ok(ApiResponse<ProductDetailDto>.SuccessResponse(responseDto!, "Product updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDetailDto>.ErrorResponse("Error updating product", ex.Message));
        }
    }

    /// <summary>
    /// Delete product (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProduct(Guid id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.CreateError("Product not found"));
            }

            product.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Product deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting product", ex.Message));
        }
    }

    /// <summary>
    /// Get product specifications
    /// </summary>
    [HttpGet("{id}/specifications")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductSpecificationDto>>>> GetProductSpecifications(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ProductSpecifications
                .Where(s => s.ProductId == id)
                .Select(s => new ProductSpecificationDto
                {
                    Id = s.Id,
                    ProductId = s.ProductId,
                    SpecificationName = s.SpecificationName,
                    SpecificationValue = s.SpecificationValue,
                    Unit = s.Unit,
                    SpecificationType = s.SpecificationType,
                    IsCritical = s.IsCritical,
                    ToleranceRange = s.ToleranceRange,
                    TestMethod = s.TestMethod,
                    Notes = s.Notes,
                    CreatedAt = s.CreatedAt
                })
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ProductSpecificationDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ProductSpecificationDto>>.ErrorResponse("Error retrieving product specifications", ex.Message));
        }
    }

    /// <summary>
    /// Update product specification
    /// </summary>
    [HttpPut("{id}/specifications")]
    public async Task<ActionResult<ApiResponse<ProductSpecificationDto>>> UpdateProductSpecification(Guid id, UpdateProductSpecificationDto dto)
    {
        try
        {
            var specification = await _context.ProductSpecifications
                .FirstOrDefaultAsync(s => s.ProductId == id);

            if (specification == null)
            {
                return NotFound(ApiResponse<ProductSpecificationDto>.ErrorResponse("Product specification not found"));
            }

            specification.SpecificationName = dto.SpecificationName;
            specification.SpecificationValue = dto.SpecificationValue;
            specification.Unit = dto.Unit;
            specification.SpecificationType = dto.SpecificationType;
            specification.IsCritical = dto.IsCritical;
            specification.ToleranceRange = dto.ToleranceRange;
            specification.TestMethod = dto.TestMethod;
            specification.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new ProductSpecificationDto
            {
                Id = specification.Id,
                ProductId = specification.ProductId,
                SpecificationName = specification.SpecificationName,
                SpecificationValue = specification.SpecificationValue,
                Unit = specification.Unit,
                SpecificationType = specification.SpecificationType,
                IsCritical = specification.IsCritical,
                ToleranceRange = specification.ToleranceRange,
                TestMethod = specification.TestMethod,
                Notes = specification.Notes,
                CreatedAt = specification.CreatedAt
            };

            return Ok(ApiResponse<ProductSpecificationDto>.SuccessResponse(responseDto, "Product specification updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductSpecificationDto>.ErrorResponse("Error updating product specification", ex.Message));
        }
    }

    /// <summary>
    /// Partially update product specification
    /// </summary>
    [HttpPatch("{id}/specifications")]
    public async Task<ActionResult<ApiResponse<ProductSpecificationDto>>> PatchProductSpecification(Guid id, PatchProductSpecificationDto dto)
    {
        try
        {
            var specification = await _context.ProductSpecifications
                .FirstOrDefaultAsync(s => s.ProductId == id);

            if (specification == null)
            {
                return NotFound(ApiResponse<ProductSpecificationDto>.ErrorResponse("Product specification not found"));
            }

            if (dto.SpecificationName != null)
                specification.SpecificationName = dto.SpecificationName;
            if (dto.SpecificationValue != null)
                specification.SpecificationValue = dto.SpecificationValue;
            if (dto.Unit != null)
                specification.Unit = dto.Unit;
            if (dto.SpecificationType != null)
                specification.SpecificationType = dto.SpecificationType;
            if (dto.IsCritical.HasValue)
                specification.IsCritical = dto.IsCritical.Value;
            if (dto.ToleranceRange != null)
                specification.ToleranceRange = dto.ToleranceRange;
            if (dto.TestMethod != null)
                specification.TestMethod = dto.TestMethod;
            if (dto.Notes != null)
                specification.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            var responseDto = new ProductSpecificationDto
            {
                Id = specification.Id,
                ProductId = specification.ProductId,
                SpecificationName = specification.SpecificationName,
                SpecificationValue = specification.SpecificationValue,
                Unit = specification.Unit,
                SpecificationType = specification.SpecificationType,
                IsCritical = specification.IsCritical,
                ToleranceRange = specification.ToleranceRange,
                TestMethod = specification.TestMethod,
                Notes = specification.Notes,
                CreatedAt = specification.CreatedAt
            };

            return Ok(ApiResponse<ProductSpecificationDto>.SuccessResponse(responseDto, "Product specification updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductSpecificationDto>.ErrorResponse("Error updating product specification", ex.Message));
        }
    }

    /// <summary>
    /// Delete product specification
    /// </summary>
    [HttpDelete("{id}/specifications")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProductSpecification(Guid id)
    {
        try
        {
            var specification = await _context.ProductSpecifications
                .FirstOrDefaultAsync(s => s.ProductId == id);

            if (specification == null)
            {
                return NotFound(ApiResponse.CreateError("Product specification not found"));
            }

            _context.ProductSpecifications.Remove(specification);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Product specification deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting product specification", ex.Message));
        }
    }

    /// <summary>
    /// Add specification to product
    /// </summary>
    [HttpPost("{id}/specifications")]
    public async Task<ActionResult<ApiResponse<ProductSpecificationDto>>> CreateProductSpecification(Guid id, CreateProductSpecificationDto dto)
    {
        try
        {
            if (!await ProductExists(id))
            {
                return NotFound(ApiResponse<ProductSpecificationDto>.ErrorResponse("Product not found"));
            }

            var specification = new ProductSpecification
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                SpecificationName = dto.SpecificationName,
                SpecificationValue = dto.SpecificationValue,
                Unit = dto.Unit,
                SpecificationType = dto.SpecificationType,
                IsCritical = dto.IsCritical,
                ToleranceRange = dto.ToleranceRange,
                TestMethod = dto.TestMethod,
                Notes = dto.Notes
            };

            _context.ProductSpecifications.Add(specification);
            await _context.SaveChangesAsync();

            var responseDto = new ProductSpecificationDto
            {
                Id = specification.Id,
                ProductId = specification.ProductId,
                SpecificationName = specification.SpecificationName,
                SpecificationValue = specification.SpecificationValue,
                Unit = specification.Unit,
                SpecificationType = specification.SpecificationType,
                IsCritical = specification.IsCritical,
                ToleranceRange = specification.ToleranceRange,
                TestMethod = specification.TestMethod,
                Notes = specification.Notes,
                CreatedAt = specification.CreatedAt
            };

            return Ok(ApiResponse<ProductSpecificationDto>.SuccessResponse(responseDto, "Product specification created successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductSpecificationDto>.ErrorResponse("Error creating product specification", ex.Message));
        }
    }

    /// <summary>
    /// Get product documents
    /// </summary>
    [HttpGet("{id}/documents")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDocumentDto>>>> GetProductDocuments(
        Guid id, [FromQuery] QueryParameters queryParams)
    {
        try
        {
            var query = _context.ProductDocuments
                .Where(d => d.ProductId == id)
                .Select(d => new ProductDocumentDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    FileName = d.FileName,
                    OriginalFileName = d.OriginalFileName,
                    ContentType = d.ContentType,
                    FilePath = d.FilePath,
                    FileUrl = d.FileUrl,
                    FileSize = d.FileSize,
                    Category = d.Category,
                    Description = d.Description,
                    Version = d.Version,
                    ExpiryDate = d.ExpiryDate,
                    IsActive = d.IsActive,
                    UploadedBy = d.UploadedBy,
                    UploadedAt = d.UploadedAt,
                    IsExpired = d.ExpiryDate.HasValue && d.ExpiryDate < DateTime.Today
                })
                .OrderByDescending(d => d.UploadedAt)
                .AsQueryable();

            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            var pagedResult = await query.ToPagedResultAsync(queryParams, baseUrl);

            return Ok(ApiResponse<PagedResult<ProductDocumentDto>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<PagedResult<ProductDocumentDto>>.ErrorResponse("Error retrieving product documents", ex.Message));
        }
    }

    /// <summary>
    /// Update product document
    /// </summary>
    [HttpPut("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<ProductDocumentDto>>> UpdateProductDocument(Guid id, Guid documentId, UpdateProductDocumentDto dto)
    {
        try
        {
            var document = await _context.ProductDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.ProductId == id);

            if (document == null)
            {
                return NotFound(ApiResponse<ProductDocumentDto>.ErrorResponse("Product document not found"));
            }

            document.FileName = dto.FileName;
            document.OriginalFileName = dto.OriginalFileName;
            document.ContentType = dto.ContentType;
            document.FilePath = dto.FilePath;
            document.FileUrl = dto.FileUrl;
            document.FileSize = dto.FileSize;
            document.Category = dto.Category;
            document.Description = dto.Description;
            document.Version = dto.Version;
            document.ExpiryDate = dto.ExpiryDate;
            document.IsActive = dto.IsActive;
            document.UploadedBy = dto.UploadedBy;

            await _context.SaveChangesAsync();

            var responseDto = new ProductDocumentDto
            {
                Id = document.Id,
                ProductId = document.ProductId,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                Category = document.Category,
                Description = document.Description,
                Version = document.Version,
                ExpiryDate = document.ExpiryDate,
                IsActive = document.IsActive,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt,
                IsExpired = document.ExpiryDate.HasValue && document.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<ProductDocumentDto>.SuccessResponse(responseDto, "Product document updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDocumentDto>.ErrorResponse("Error updating product document", ex.Message));
        }
    }

    /// <summary>
    /// Partially update product document
    /// </summary>
    [HttpPatch("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<ProductDocumentDto>>> PatchProductDocument(Guid id, Guid documentId, PatchProductDocumentDto dto)
    {
        try
        {
            var document = await _context.ProductDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.ProductId == id);

            if (document == null)
            {
                return NotFound(ApiResponse<ProductDocumentDto>.ErrorResponse("Product document not found"));
            }

            if (dto.FileName != null)
                document.FileName = dto.FileName;
            if (dto.OriginalFileName != null)
                document.OriginalFileName = dto.OriginalFileName;
            if (dto.ContentType != null)
                document.ContentType = dto.ContentType;
            if (dto.FilePath != null)
                document.FilePath = dto.FilePath;
            if (dto.FileSize.HasValue)
                document.FileSize = dto.FileSize.Value;
            if (dto.UploadedBy != null)
                document.UploadedBy = dto.UploadedBy;
            if (dto.Category != null)
                document.Category = dto.Category;
            if (dto.Description != null)
                document.Description = dto.Description;
            if (dto.Version != null)
                document.Version = dto.Version;
            if (dto.ExpiryDate.HasValue)
                document.ExpiryDate = dto.ExpiryDate.Value;
            if (dto.FileUrl != null)
                document.FileUrl = dto.FileUrl;
            if (dto.IsActive.HasValue)
                document.IsActive = dto.IsActive.Value;

            await _context.SaveChangesAsync();

            var responseDto = new ProductDocumentDto
            {
                Id = document.Id,
                ProductId = document.ProductId,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                Category = document.Category,
                Description = document.Description,
                Version = document.Version,
                ExpiryDate = document.ExpiryDate,
                IsActive = document.IsActive,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt,
                IsExpired = document.ExpiryDate.HasValue && document.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<ProductDocumentDto>.SuccessResponse(responseDto, "Product document updated successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDocumentDto>.ErrorResponse("Error updating product document", ex.Message));
        }
    }

    /// <summary>
    /// Delete product document
    /// </summary>
    [HttpDelete("{id}/documents/{documentId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProductDocument(Guid id, Guid documentId)
    {
        try
        {
            var document = await _context.ProductDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.ProductId == id);

            if (document == null)
            {
                return NotFound(ApiResponse.CreateError("Product document not found"));
            }

            _context.ProductDocuments.Remove(document);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.CreateSuccess("Product document deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.CreateError("Error deleting product document", ex.Message));
        }
    }

    /// <summary>
    /// Add document to product
    /// </summary>
    [HttpPost("{id}/documents")]
    public async Task<ActionResult<ApiResponse<ProductDocumentDto>>> CreateProductDocument(Guid id, CreateProductDocumentDto dto)
    {
        try
        {
            if (!await ProductExists(id))
            {
                return NotFound(ApiResponse<ProductDocumentDto>.ErrorResponse("Product not found"));
            }

            var document = new ProductDocument
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                FileName = dto.FileName,
                OriginalFileName = dto.OriginalFileName,
                ContentType = dto.ContentType,
                FilePath = dto.FilePath,
                FileUrl = dto.FileUrl,
                FileSize = dto.FileSize,
                Category = dto.Category,
                Description = dto.Description,
                Version = dto.Version,
                ExpiryDate = dto.ExpiryDate,
                UploadedBy = dto.UploadedBy,
                UploadedAt = DateTime.UtcNow
            };

            _context.ProductDocuments.Add(document);
            await _context.SaveChangesAsync();

            var responseDto = new ProductDocumentDto
            {
                Id = document.Id,
                ProductId = document.ProductId,
                FileName = document.FileName,
                OriginalFileName = document.OriginalFileName,
                ContentType = document.ContentType,
                FilePath = document.FilePath,
                FileUrl = document.FileUrl,
                FileSize = document.FileSize,
                Category = document.Category,
                Description = document.Description,
                Version = document.Version,
                ExpiryDate = document.ExpiryDate,
                IsActive = document.IsActive,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt,
                IsExpired = document.ExpiryDate.HasValue && document.ExpiryDate < DateTime.Today
            };

            return Ok(ApiResponse<ProductDocumentDto>.SuccessResponse(responseDto, "Product document uploaded successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<ProductDocumentDto>.ErrorResponse("Error uploading product document", ex.Message));
        }
    }

    private async Task<bool> ProductExists(Guid id)
    {
        return await _context.Products.AnyAsync(e => e.Id == id);
    }
}