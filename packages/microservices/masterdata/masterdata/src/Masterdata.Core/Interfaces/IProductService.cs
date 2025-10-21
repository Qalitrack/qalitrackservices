using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Masterdata.Core.DTOs;
using Masterdata.Core.DTOs.Product;
using Masterdata.Core.Models;

namespace Masterdata.Core.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedProductsAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<ProductDto> CreateAsync(CreateProductDto dto);
    Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IsCodeAvailableAsync(string code, Guid? excludeId = null);
}
