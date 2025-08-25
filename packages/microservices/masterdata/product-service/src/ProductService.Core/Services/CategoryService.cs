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

    public async Task<IEnumerable<CategoryReadDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<CategoryReadDto>>(categories);
    }

    public async Task<CategoryReadDto?> GetByIdAsync(string id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category == null ? null : _mapper.Map<CategoryReadDto>(category);
    }

    public async Task<CategoryReadDto> CreateAsync(CreateCategoryDto dto)
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
        return _mapper.Map<CategoryReadDto>(createdCategory);
    }

    public async Task<CategoryReadDto?> UpdateAsync(string id, UpdateCategoryDto dto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory == null)
        {
            return null;
        }

        _mapper.Map(dto, existingCategory);
        existingCategory.UpdatedAt = DateTime.UtcNow;
        
        var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
        return updatedCategory == null ? null : _mapper.Map<CategoryReadDto>(updatedCategory);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<CategoryReadDto>> GetRootCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var rootCategories = categories.Where(c => c.IsRootCategory).ToList();
        return _mapper.Map<IEnumerable<CategoryReadDto>>(rootCategories);
    }

    public async Task<IEnumerable<CategoryReadDto>> GetSubCategoriesAsync(string parentId)
    {
        var subCategories = await _categoryRepository.GetSubCategoriesAsync(parentId);
        return _mapper.Map<IEnumerable<CategoryReadDto>>(subCategories);
    }

    public async Task<bool> IsNameAvailableAsync(string name)
    {
        var categories = await _categoryRepository.GetAllAsync();
        return !categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<CategoryReadDto>> GetCategoryHierarchyAsync(string categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null) return new List<CategoryReadDto>();

        var hierarchy = new List<Category>();
        var current = category;
        
        while (current != null)
        {
            hierarchy.Insert(0, current);
            if (!string.IsNullOrEmpty(current.ParentCategoryId))
            {
                current = await _categoryRepository.GetByIdAsync(current.ParentCategoryId);
            }
            else
            {
                current = null;
            }
        }

        return _mapper.Map<IEnumerable<CategoryReadDto>>(hierarchy);
    }

    public async Task<bool> CanDeleteCategoryAsync(string categoryId)
    {
        var subCategories = await _categoryRepository.GetSubCategoriesAsync(categoryId);
        return !subCategories.Any();
    }
}