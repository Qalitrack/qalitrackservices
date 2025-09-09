using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QaliTrack.MasterData.Core.Common;
using QaliTrack.MasterData.Core.Modules.ProductCatalog.DTOs;
using QaliTrack.MasterData.Core.Modules.ProductCatalog.Entities;
using QaliTrack.MasterData.Infrastructure.Data;
using AutoMapper;

namespace QaliTrack.MasterData.Api.Controllers;

[ApiController]
[Route("productcatalog")]
[Tags("Product Catalog Module")]
public class ProductCatalogController : ControllerBase
{
    private readonly MasterDataDbContext _context;
    private readonly IMapper _mapper;

    public ProductCatalogController(MasterDataDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #region Product Categories

    /// <summary>
    /// Get product categories with hierarchy support
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductCategoryDto>>>> GetProductCategories(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.ProductCategories
            .Include(pc => pc.ParentCategory)
            .Include(pc => pc.SubCategories)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(pc => pc.Name.Contains(queryParams.Search) || pc.Code.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var categories = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var categoryDtos = _mapper.Map<IEnumerable<ProductCategoryDto>>(categories);

        return Ok(new ApiResponse<IEnumerable<ProductCategoryDto>>
        {
            Data = categoryDtos,
            Success = true,
            Message = "Product categories retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get product category by ID
    /// </summary>
    [HttpGet("categories/{id}")]
    public async Task<ActionResult<ApiResponse<ProductCategoryDto>>> GetProductCategory(Guid id)
    {
        var category = await _context.ProductCategories
            .Include(pc => pc.ParentCategory)
            .Include(pc => pc.SubCategories)
            .FirstOrDefaultAsync(pc => pc.Id == id);

        if (category == null)
        {
            return NotFound(new ApiResponse<ProductCategoryDto> { Success = false, Message = "Product category not found" });
        }

        var categoryDto = _mapper.Map<ProductCategoryDto>(category);
        return Ok(new ApiResponse<ProductCategoryDto> { Data = categoryDto, Success = true, Message = "Product category retrieved successfully" });
    }

    /// <summary>
    /// Create new product category
    /// </summary>
    [HttpPost("categories")]
    public async Task<ActionResult<ApiResponse<ProductCategoryDto>>> CreateProductCategory(CreateProductCategoryDto createCategoryDto)
    {
        var category = _mapper.Map<ProductCategory>(createCategoryDto);
        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        var categoryDto = _mapper.Map<ProductCategoryDto>(category);
        return CreatedAtAction(nameof(GetProductCategory), new { id = category.Id },
            new ApiResponse<ProductCategoryDto> { Data = categoryDto, Success = true, Message = "Product category created successfully" });
    }

    /// <summary>
    /// Update product category
    /// </summary>
    [HttpPut("categories/{id}")]
    public async Task<ActionResult<ApiResponse<ProductCategoryDto>>> UpdateProductCategory(Guid id, UpdateProductCategoryDto updateCategoryDto)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new ApiResponse<ProductCategoryDto> { Success = false, Message = "Product category not found" });
        }

        _mapper.Map(updateCategoryDto, category);
        await _context.SaveChangesAsync();

        var categoryDto = _mapper.Map<ProductCategoryDto>(category);
        return Ok(new ApiResponse<ProductCategoryDto> { Data = categoryDto, Success = true, Message = "Product category updated successfully" });
    }

    /// <summary>
    /// Delete product category
    /// </summary>
    [HttpDelete("categories/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProductCategory(Guid id)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Product category not found" });
        }

        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Product category deleted successfully" });
    }

    #endregion

    #region Packaging Types

    /// <summary>
    /// Get packaging types
    /// </summary>
    [HttpGet("packaging-types")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PackagingTypeDto>>>> GetPackagingTypes(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.PackagingTypes.AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(pt => pt.Name.Contains(queryParams.Search) || pt.Code.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var packagingTypes = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var packagingTypeDtos = _mapper.Map<IEnumerable<PackagingTypeDto>>(packagingTypes);

        return Ok(new ApiResponse<IEnumerable<PackagingTypeDto>>
        {
            Data = packagingTypeDtos,
            Success = true,
            Message = "Packaging types retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get packaging type by ID
    /// </summary>
    [HttpGet("packaging-types/{id}")]
    public async Task<ActionResult<ApiResponse<PackagingTypeDto>>> GetPackagingType(Guid id)
    {
        var packagingType = await _context.PackagingTypes.FindAsync(id);
        if (packagingType == null)
        {
            return NotFound(new ApiResponse<PackagingTypeDto> { Success = false, Message = "Packaging type not found" });
        }

        var packagingTypeDto = _mapper.Map<PackagingTypeDto>(packagingType);
        return Ok(new ApiResponse<PackagingTypeDto> { Data = packagingTypeDto, Success = true, Message = "Packaging type retrieved successfully" });
    }

    /// <summary>
    /// Create new packaging type
    /// </summary>
    [HttpPost("packaging-types")]
    public async Task<ActionResult<ApiResponse<PackagingTypeDto>>> CreatePackagingType(CreatePackagingTypeDto createPackagingTypeDto)
    {
        var packagingType = _mapper.Map<PackagingType>(createPackagingTypeDto);
        _context.PackagingTypes.Add(packagingType);
        await _context.SaveChangesAsync();

        var packagingTypeDto = _mapper.Map<PackagingTypeDto>(packagingType);
        return CreatedAtAction(nameof(GetPackagingType), new { id = packagingType.Id },
            new ApiResponse<PackagingTypeDto> { Data = packagingTypeDto, Success = true, Message = "Packaging type created successfully" });
    }

    /// <summary>
    /// Update packaging type
    /// </summary>
    [HttpPut("packaging-types/{id}")]
    public async Task<ActionResult<ApiResponse<PackagingTypeDto>>> UpdatePackagingType(Guid id, UpdatePackagingTypeDto updatePackagingTypeDto)
    {
        var packagingType = await _context.PackagingTypes.FindAsync(id);
        if (packagingType == null)
        {
            return NotFound(new ApiResponse<PackagingTypeDto> { Success = false, Message = "Packaging type not found" });
        }

        _mapper.Map(updatePackagingTypeDto, packagingType);
        await _context.SaveChangesAsync();

        var packagingTypeDto = _mapper.Map<PackagingTypeDto>(packagingType);
        return Ok(new ApiResponse<PackagingTypeDto> { Data = packagingTypeDto, Success = true, Message = "Packaging type updated successfully" });
    }

    /// <summary>
    /// Delete packaging type
    /// </summary>
    [HttpDelete("packaging-types/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeletePackagingType(Guid id)
    {
        var packagingType = await _context.PackagingTypes.FindAsync(id);
        if (packagingType == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Packaging type not found" });
        }

        _context.PackagingTypes.Remove(packagingType);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Packaging type deleted successfully" });
    }

    #endregion

    #region Product Base

    /// <summary>
    /// Get base products
    /// </summary>
    [HttpGet("products")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductBaseDto>>>> GetProductBases(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.ProductBases
            .Include(pb => pb.Category)
            .Include(pb => pb.Specifications)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(pb => pb.Name.Contains(queryParams.Search) || pb.Code.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var productBases = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var productBaseDtos = _mapper.Map<IEnumerable<ProductBaseDto>>(productBases);

        return Ok(new ApiResponse<IEnumerable<ProductBaseDto>>
        {
            Data = productBaseDtos,
            Success = true,
            Message = "Base products retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get base product by ID
    /// </summary>
    [HttpGet("products/{id}")]
    public async Task<ActionResult<ApiResponse<ProductBaseDto>>> GetProductBase(Guid id)
    {
        var productBase = await _context.ProductBases
            .Include(pb => pb.Category)
            .Include(pb => pb.Specifications)
            .Include(pb => pb.Variants)
            .FirstOrDefaultAsync(pb => pb.Id == id);

        if (productBase == null)
        {
            return NotFound(new ApiResponse<ProductBaseDto> { Success = false, Message = "Base product not found" });
        }

        var productBaseDto = _mapper.Map<ProductBaseDto>(productBase);
        return Ok(new ApiResponse<ProductBaseDto> { Data = productBaseDto, Success = true, Message = "Base product retrieved successfully" });
    }

    /// <summary>
    /// Get product specifications by product ID
    /// </summary>
    [HttpGet("products/{id}/specifications")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductSpecificationDto>>>> GetProductSpecifications(Guid id)
    {
        var specifications = await _context.ProductSpecifications
            .Where(ps => ps.ProductBaseId == id)
            .ToListAsync();

        var specificationDtos = _mapper.Map<IEnumerable<ProductSpecificationDto>>(specifications);
        return Ok(new ApiResponse<IEnumerable<ProductSpecificationDto>> 
        { 
            Data = specificationDtos, 
            Success = true, 
            Message = "Product specifications retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new base product
    /// </summary>
    [HttpPost("products")]
    public async Task<ActionResult<ApiResponse<ProductBaseDto>>> CreateProductBase(CreateProductBaseDto createProductBaseDto)
    {
        var productBase = _mapper.Map<ProductBase>(createProductBaseDto);
        _context.ProductBases.Add(productBase);
        await _context.SaveChangesAsync();

        var productBaseDto = _mapper.Map<ProductBaseDto>(productBase);
        return CreatedAtAction(nameof(GetProductBase), new { id = productBase.Id },
            new ApiResponse<ProductBaseDto> { Data = productBaseDto, Success = true, Message = "Base product created successfully" });
    }

    /// <summary>
    /// Update base product
    /// </summary>
    [HttpPut("products/{id}")]
    public async Task<ActionResult<ApiResponse<ProductBaseDto>>> UpdateProductBase(Guid id, UpdateProductBaseDto updateProductBaseDto)
    {
        var productBase = await _context.ProductBases.FindAsync(id);
        if (productBase == null)
        {
            return NotFound(new ApiResponse<ProductBaseDto> { Success = false, Message = "Base product not found" });
        }

        _mapper.Map(updateProductBaseDto, productBase);
        await _context.SaveChangesAsync();

        var productBaseDto = _mapper.Map<ProductBaseDto>(productBase);
        return Ok(new ApiResponse<ProductBaseDto> { Data = productBaseDto, Success = true, Message = "Base product updated successfully" });
    }

    /// <summary>
    /// Delete base product
    /// </summary>
    [HttpDelete("products/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProductBase(Guid id)
    {
        var productBase = await _context.ProductBases.FindAsync(id);
        if (productBase == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Base product not found" });
        }

        _context.ProductBases.Remove(productBase);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Base product deleted successfully" });
    }

    #endregion

    #region Product Variants

    /// <summary>
    /// Get product variants
    /// </summary>
    [HttpGet("variants")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductVariantDto>>>> GetProductVariants(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.ProductVariants
            .Include(pv => pv.ProductBase)
            .Include(pv => pv.PackagingType)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(pv => pv.VariantName.Contains(queryParams.Search) || pv.VariantCode.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var productVariants = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var productVariantDtos = _mapper.Map<IEnumerable<ProductVariantDto>>(productVariants);

        return Ok(new ApiResponse<IEnumerable<ProductVariantDto>>
        {
            Data = productVariantDtos,
            Success = true,
            Message = "Product variants retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get product variant by ID
    /// </summary>
    [HttpGet("variants/{id}")]
    public async Task<ActionResult<ApiResponse<ProductVariantDto>>> GetProductVariant(Guid id)
    {
        var productVariant = await _context.ProductVariants
            .Include(pv => pv.ProductBase)
            .Include(pv => pv.PackagingType)
            .FirstOrDefaultAsync(pv => pv.Id == id);

        if (productVariant == null)
        {
            return NotFound(new ApiResponse<ProductVariantDto> { Success = false, Message = "Product variant not found" });
        }

        var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);
        return Ok(new ApiResponse<ProductVariantDto> { Data = productVariantDto, Success = true, Message = "Product variant retrieved successfully" });
    }

    /// <summary>
    /// Get product variants by product ID
    /// </summary>
    [HttpGet("variants/by-product/{productId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductVariantDto>>>> GetProductVariantsByProduct(Guid productId)
    {
        var productVariants = await _context.ProductVariants
            .Include(pv => pv.ProductBase)
            .Include(pv => pv.PackagingType)
            .Where(pv => pv.ProductBaseId == productId)
            .ToListAsync();

        var productVariantDtos = _mapper.Map<IEnumerable<ProductVariantDto>>(productVariants);
        return Ok(new ApiResponse<IEnumerable<ProductVariantDto>> 
        { 
            Data = productVariantDtos, 
            Success = true, 
            Message = "Product variants retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new product variant
    /// </summary>
    [HttpPost("variants")]
    public async Task<ActionResult<ApiResponse<ProductVariantDto>>> CreateProductVariant(CreateProductVariantDto createProductVariantDto)
    {
        var productVariant = _mapper.Map<ProductVariant>(createProductVariantDto);
        _context.ProductVariants.Add(productVariant);
        await _context.SaveChangesAsync();

        var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);
        return CreatedAtAction(nameof(GetProductVariant), new { id = productVariant.Id },
            new ApiResponse<ProductVariantDto> { Data = productVariantDto, Success = true, Message = "Product variant created successfully" });
    }

    /// <summary>
    /// Update product variant
    /// </summary>
    [HttpPut("variants/{id}")]
    public async Task<ActionResult<ApiResponse<ProductVariantDto>>> UpdateProductVariant(Guid id, UpdateProductVariantDto updateProductVariantDto)
    {
        var productVariant = await _context.ProductVariants.FindAsync(id);
        if (productVariant == null)
        {
            return NotFound(new ApiResponse<ProductVariantDto> { Success = false, Message = "Product variant not found" });
        }

        _mapper.Map(updateProductVariantDto, productVariant);
        await _context.SaveChangesAsync();

        var productVariantDto = _mapper.Map<ProductVariantDto>(productVariant);
        return Ok(new ApiResponse<ProductVariantDto> { Data = productVariantDto, Success = true, Message = "Product variant updated successfully" });
    }

    /// <summary>
    /// Delete product variant
    /// </summary>
    [HttpDelete("variants/{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteProductVariant(Guid id)
    {
        var productVariant = await _context.ProductVariants.FindAsync(id);
        if (productVariant == null)
        {
            return NotFound(new ApiResponse<object> { Success = false, Message = "Product variant not found" });
        }

        _context.ProductVariants.Remove(productVariant);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object> { Success = true, Message = "Product variant deleted successfully" });
    }

    #endregion

    #region Product Usage Permissions

    /// <summary>
    /// Get product usage permissions
    /// </summary>
    [HttpGet("permissions")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductUsagePermissionDto>>>> GetProductUsagePermissions(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.ProductUsagePermissions
            .Include(pup => pup.ProductVariant)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(pup => pup.UsageType.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var permissions = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var permissionDtos = _mapper.Map<IEnumerable<ProductUsagePermissionDto>>(permissions);

        return Ok(new ApiResponse<IEnumerable<ProductUsagePermissionDto>>
        {
            Data = permissionDtos,
            Success = true,
            Message = "Product usage permissions retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get product usage permissions by site ID
    /// </summary>
    [HttpGet("permissions/by-site/{siteId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductUsagePermissionDto>>>> GetProductUsagePermissionsBySite(Guid siteId)
    {
        var permissions = await _context.ProductUsagePermissions
            .Include(pup => pup.ProductVariant)
            .Where(pup => pup.SiteId == siteId)
            .ToListAsync();

        var permissionDtos = _mapper.Map<IEnumerable<ProductUsagePermissionDto>>(permissions);
        return Ok(new ApiResponse<IEnumerable<ProductUsagePermissionDto>> 
        { 
            Data = permissionDtos, 
            Success = true, 
            Message = "Product usage permissions retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new product usage permission
    /// </summary>
    [HttpPost("permissions")]
    public async Task<ActionResult<ApiResponse<ProductUsagePermissionDto>>> CreateProductUsagePermission(CreateProductUsagePermissionDto createPermissionDto)
    {
        var permission = _mapper.Map<ProductUsagePermission>(createPermissionDto);
        _context.ProductUsagePermissions.Add(permission);
        await _context.SaveChangesAsync();

        var permissionDto = _mapper.Map<ProductUsagePermissionDto>(permission);
        return CreatedAtAction(nameof(GetProductUsagePermissions), new { id = permission.Id },
            new ApiResponse<ProductUsagePermissionDto> { Data = permissionDto, Success = true, Message = "Product usage permission created successfully" });
    }

    #endregion

    #region Site Capabilities

    /// <summary>
    /// Get site capabilities
    /// </summary>
    [HttpGet("capabilities")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SiteCapabilityDto>>>> GetSiteCapabilities(
        [FromQuery] QueryParameters queryParams)
    {
        var query = _context.SiteCapabilities
            .Include(sc => sc.ProductCategory)
            .AsQueryable();

        if (!string.IsNullOrEmpty(queryParams.Search))
        {
            query = query.Where(sc => sc.CapabilityType.Contains(queryParams.Search));
        }

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        var capabilities = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        var capabilityDtos = _mapper.Map<IEnumerable<SiteCapabilityDto>>(capabilities);

        return Ok(new ApiResponse<IEnumerable<SiteCapabilityDto>>
        {
            Data = capabilityDtos,
            Success = true,
            Message = "Site capabilities retrieved successfully",
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        });
    }

    /// <summary>
    /// Get site capabilities by site ID
    /// </summary>
    [HttpGet("capabilities/by-site/{siteId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<SiteCapabilityDto>>>> GetSiteCapabilitiesBySite(Guid siteId)
    {
        var capabilities = await _context.SiteCapabilities
            .Include(sc => sc.ProductCategory)
            .Where(sc => sc.SiteId == siteId)
            .ToListAsync();

        var capabilityDtos = _mapper.Map<IEnumerable<SiteCapabilityDto>>(capabilities);
        return Ok(new ApiResponse<IEnumerable<SiteCapabilityDto>> 
        { 
            Data = capabilityDtos, 
            Success = true, 
            Message = "Site capabilities retrieved successfully" 
        });
    }

    /// <summary>
    /// Create new site capability
    /// </summary>
    [HttpPost("capabilities")]
    public async Task<ActionResult<ApiResponse<SiteCapabilityDto>>> CreateSiteCapability(CreateSiteCapabilityDto createCapabilityDto)
    {
        var capability = _mapper.Map<SiteCapability>(createCapabilityDto);
        _context.SiteCapabilities.Add(capability);
        await _context.SaveChangesAsync();

        var capabilityDto = _mapper.Map<SiteCapabilityDto>(capability);
        return CreatedAtAction(nameof(GetSiteCapabilities), new { id = capability.Id },
            new ApiResponse<SiteCapabilityDto> { Data = capabilityDto, Success = true, Message = "Site capability created successfully" });
    }

    #endregion
}