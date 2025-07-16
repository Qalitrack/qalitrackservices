using ProductService.Core.DTOs;

namespace ProductService.Core.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryReadDto>> GetAllAsync();
    Task<CategoryReadDto?> GetByIdAsync(string id);
    Task<CategoryReadDto> CreateAsync(CreateCategoryDto dto);
    Task<CategoryReadDto?> UpdateAsync(string id, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(string id);
    Task<bool> IsNameAvailableAsync(string name);
    Task<IEnumerable<CategoryReadDto>> GetRootCategoriesAsync();
    Task<IEnumerable<CategoryReadDto>> GetSubCategoriesAsync(string parentId);
    Task<IEnumerable<CategoryReadDto>> GetCategoryHierarchyAsync(string categoryId);
    Task<bool> CanDeleteCategoryAsync(string categoryId);
}