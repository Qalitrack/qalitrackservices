using AutoMapper;
using ProductService.Core.DTOs;
using ProductService.Core.Entities;
using ProductService.Core.Interfaces;

namespace ProductService.Core.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryDto>>(categories);
    }

    public async Task<CategoryDto?> GetByIdAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category == null ? null : _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;
        
        // Set hierarchical properties
        if (!string.IsNullOrEmpty(category.ParentCategoryId))
        {
            var parentCategory = await _categoryRepository.GetByIdAsync(category.ParentCategoryId);
            if (parentCategory != null)
            {
                category.Level = parentCategory.Level + 1;
                category.Path = $"{parentCategory.Path}/{category.Name}";
            }
        }
        else
        {
            category.Level = 0;
            category.Path = category.Name;
        }
        
        var createdCategory = await _categoryRepository.CreateAsync(category);
        return _mapper.Map<CategoryDto>(createdCategory);
    }

    public async Task<CategoryDto?> UpdateAsync(string id, CategoryDto dto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return null;
        }

        _mapper.Map(dto, existingCategory);
        existingCategory.UpdatedAt = DateTime.UtcNow;
        
        var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
        return updatedCategory == null ? null : _mapper.Map<CategoryDto>(updatedCategory);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CategoryDto>> GetHierarchyAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var rootCategories = categories.Where(c => c.IsRootCategory).ToList();
        return _mapper.Map<IEnumerable<CategoryDto>>(rootCategories);
    }

    public async Task<IEnumerable<CategoryDto>> GetSubCategoriesAsync(string parentId)
    {
        var subCategories = await _categoryRepository.GetSubCategoriesAsync(parentId);
        return _mapper.Map<IEnumerable<CategoryDto>>(subCategories);
    }
}