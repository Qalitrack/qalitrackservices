'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input, Textarea } from '@/components/ui/FormField'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { 
  ValidationError, 
  parseApiErrors, 
  getErrorsForField, 
  getGeneralErrors,
  validateRequired
} from '@/lib/validation'
import { Material, MaterialVariant } from '@/lib/types/api'

export default function MaterialsPage() {
  const [materials, setMaterials] = useState<Material[]>([])
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const [showAddVariantDialog, setShowAddVariantDialog] = useState(false)
  const [selectedMaterial, setSelectedMaterial] = useState<Material | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [sortBy, setSortBy] = useState('name')
  const [filterBy, setFilterBy] = useState('all')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [hasNextPage, setHasNextPage] = useState(false)
  const { showSuccess, showError } = useToast()

  useEffect(() => {
    fetchMaterials(true) // Reset on mount
  }, [])

  useEffect(() => {
    // Debounced search - trigger for both search and clear
    const timer = setTimeout(() => {
      fetchMaterials(true) // Reset when searching or clearing
    }, 500)

    return () => clearTimeout(timer)
  }, [searchTerm])

  useEffect(() => {
    // Reset when sort/filter changes
    fetchMaterials(true)
  }, [sortBy, filterBy])

  const fetchMaterials = async (reset = false, page = 1) => {
    try {
      if (reset) {
        setLoading(true)
        setCurrentPage(1)
        setMaterials([])
      } else {
        setLoadingMore(true)
      }

      const params: any = {
        page,
        page_size: 20
      }

      if (searchTerm.trim()) {
        params.search = searchTerm.trim()
      }

      // Add sorting
      if (sortBy) {
        params.ordering = sortBy
      }

      const response = await apiService.getMaterials(params)
      
      if (reset) {
        setMaterials(response.results || [])
      } else {
        setMaterials(prev => [...prev, ...(response.results || [])])
      }
      
      setTotalCount(response.count || 0)
      setHasNextPage(!!response.next)
      setCurrentPage(page)
      
    } catch (error) {
      console.error('Failed to load materials:', error)
      showError('Failed to load materials', error instanceof Error ? error.message : 'An unexpected error occurred')
    } finally {
      setLoading(false)
      setLoadingMore(false)
    }
  }

  const loadMoreMaterials = () => {
    if (hasNextPage && !loadingMore) {
      fetchMaterials(false, currentPage + 1)
    }
  }

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    // The useEffect will handle the debounced search
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Materials Management</h1>
          <p className="text-gray-600">
            Manage materials with photos and descriptions 
            {totalCount > 0 && (
              <span className="text-slate-600 ml-2">
                ({totalCount} total, {materials.length} loaded)
              </span>
            )}
          </p>
        </div>
        <div className="flex items-center space-x-4">
          <RefreshButton onRefresh={() => fetchMaterials(true)} loading={loading} theme="admin" />
          <button 
            onClick={() => setShowAddDialog(true)}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
          >
            <span className="text-lg mr-2">📦</span>
            Add Material
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📦</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {totalCount > 0 ? totalCount : materials.length}
              </p>
              <p className="text-sm text-gray-600">Total Materials</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">🏷️</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {materials.reduce((total, m) => total + (m.variants?.length || 0), 0)}
              </p>
              <p className="text-sm text-gray-600">Total Variants</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📷</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {materials.filter(m => m.photos && m.photos.length > 0).length}
              </p>
              <p className="text-sm text-gray-600">With Photos</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📝</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {materials.filter(m => m.description).length}
              </p>
              <p className="text-sm text-gray-600">With Descriptions</p>
            </div>
          </div>
        </div>
      </div>

      {/* Search and Filter Bar */}
      <div className="card p-4">
        <div className="flex flex-col lg:flex-row gap-4 items-start lg:items-center">
          {/* Search Section */}
          <div className="flex-1 w-full lg:w-auto">
            <label className="block text-sm font-medium text-gray-700 mb-2">Search Materials</label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <span className="text-gray-400 text-lg">🔍</span>
              </div>
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="input pl-10 pr-10 w-full"
                placeholder="Search by name or description..."
              />
              {searchTerm && (
                <button
                  onClick={() => handleSearch('')}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-red-500 transition-colors"
                  title="Clear search"
                >
                  <span className="text-xl">×</span>
                </button>
              )}
            </div>
          </div>
          
          {/* Sort & Filter Section */}
          <div className="flex flex-col sm:flex-row gap-4 w-full lg:w-auto">
            {/* Sort */}
            <div className="min-w-[160px]">
              <label className="block text-sm font-medium text-gray-700 mb-2">Sort By</label>
              <div className="relative">
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value)}
                  className="input pr-8 appearance-none cursor-pointer w-full"
                >
                  <option value="name">📝 Name (A-Z)</option>
                  <option value="-name">📝 Name (Z-A)</option>
                  <option value="created_at">📅 Date (Oldest)</option>
                  <option value="-created_at">📅 Date (Newest)</option>
                </select>
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <span className="text-gray-400">▼</span>
                </div>
              </div>
            </div>
            
            {/* Filter */}
            <div className="min-w-[160px]">
              <label className="block text-sm font-medium text-gray-700 mb-2">Filter By</label>
              <div className="relative">
                <select
                  value={filterBy}
                  onChange={(e) => setFilterBy(e.target.value)}
                  className="input pr-8 appearance-none cursor-pointer w-full"
                >
                  <option value="all">📦 All Materials</option>
                  <option value="with_photos">📷 With Photos</option>
                  <option value="with_variants">🏷️ With Variants</option>
                  <option value="with_description">📝 With Description</option>
                </select>
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <span className="text-gray-400">▼</span>
                </div>
              </div>
            </div>
            
            {/* Clear Filters Button */}
            {(searchTerm || sortBy !== 'name' || filterBy !== 'all') && (
              <div className="flex items-end">
                <button
                  onClick={() => {
                    setSearchTerm('')
                    setSortBy('name')
                    setFilterBy('all')
                    handleSearch('')
                  }}
                  className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 h-[42px]"
                  title="Clear all filters"
                >
                  <span className="text-lg mr-1">🗑️</span>
                  Clear
                </button>
              </div>
            )}
          </div>
        </div>
        
        {/* Active Filters Display */}
        {(searchTerm || sortBy !== 'name' || filterBy !== 'all') && (
          <div className="mt-4 pt-3 border-t border-gray-200">
            <div className="flex flex-wrap gap-2 items-center">
              <span className="text-sm text-gray-600 font-medium">Active filters:</span>
              
              {searchTerm && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-slate-100 text-slate-800">
                  🔍 "{searchTerm}"
                  <button
                    onClick={() => handleSearch('')}
                    className="ml-1 text-slate-600 hover:text-slate-800"
                  >
                    ×
                  </button>
                </span>
              )}
              
              {sortBy !== 'name' && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                  📊 {sortBy === '-name' ? 'Name (Z-A)' : 
                       sortBy === 'created_at' ? 'Date (Oldest)' : 
                       sortBy === '-created_at' ? 'Date (Newest)' : 'Custom Sort'}
                  <button
                    onClick={() => setSortBy('name')}
                    className="ml-1 text-green-600 hover:text-green-800"
                  >
                    ×
                  </button>
                </span>
              )}
              
              {filterBy !== 'all' && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-slate-100 text-slate-800">
                  🔍 {filterBy === 'with_photos' ? 'With Photos' :
                       filterBy === 'with_variants' ? 'With Variants' :
                       filterBy === 'with_description' ? 'With Description' : filterBy}
                  <button
                    onClick={() => setFilterBy('all')}
                    className="ml-1 text-purple-600 hover:text-purple-800"
                  >
                    ×
                  </button>
                </span>
              )}
            </div>
          </div>
        )}
      </div>

      {/* Materials List */}
      {loading ? (
        <div className="space-y-4">
          {[1, 2, 3, 4, 5].map(i => (
            <div key={i} className="card animate-pulse">
              <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div className="h-3 bg-gray-200 rounded w-1/2 mb-1"></div>
              <div className="h-3 bg-gray-200 rounded w-1/3"></div>
            </div>
          ))}
        </div>
      ) : materials.length === 0 ? (
        <div className="card text-center py-12">
          <div className="text-6xl mb-4">📦</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No materials configured</h3>
          <p className="text-gray-600 mb-4">Add materials to start planning deliveries.</p>
          <button 
            onClick={() => setShowAddDialog(true)}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
          >
            Add First Material
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {materials.map((material) => (
            <MaterialCard 
              key={material.id} 
              material={material} 
              onEdit={() => {/* TODO: Edit functionality */}}
              onDelete={() => {/* TODO: Delete functionality */}}
              onAddVariant={() => {
                setSelectedMaterial(material)
                setShowAddVariantDialog(true)
              }}
              onRefresh={() => fetchMaterials(true)}
            />
          ))}
          
          {/* Load More Button */}
          {hasNextPage && (
            <div className="flex justify-center py-6">
              <button
                onClick={loadMoreMaterials}
                disabled={loadingMore}
                className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 disabled:opacity-50"
              >
                {loadingMore ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-slate-600 border-t-transparent mr-2"></div>
                    Loading more...
                  </>
                ) : (
                  <>Load More Materials ({totalCount - materials.length} remaining)</>
                )}
              </button>
            </div>
          )}
          
          {/* Loading More Indicator */}
          {loadingMore && (
            <div className="space-y-4">
              {[1, 2, 3].map(i => (
                <div key={i} className="card animate-pulse">
                  <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2 mb-1"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/3"></div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Add Material Dialog */}
      {showAddDialog && (
        <AddMaterialDialog 
          onClose={() => setShowAddDialog(false)} 
          onSuccess={() => {
            fetchMaterials()
            setShowAddDialog(false)
          }} 
        />
      )}

      {/* Add Variant Dialog */}
      {showAddVariantDialog && selectedMaterial && (
        <AddVariantDialog 
          material={selectedMaterial}
          onClose={() => {
            setShowAddVariantDialog(false)
            setSelectedMaterial(null)
          }} 
          onSuccess={() => {
            fetchMaterials()
            setShowAddVariantDialog(false)
            setSelectedMaterial(null)
          }} 
        />
      )}
    </div>
  )
}

// Add Material Dialog Component
function AddMaterialDialog({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    photos: [] as File[]
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const nameError = validateRequired(formData.name, 'Material name')
    if (nameError) newErrors.name = nameError
    
    setClientErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    if (clientErrors[field]) {
      setClientErrors(prev => ({ ...prev, [field]: '' }))
    }
    if (getErrorsForField(errors, field).length > 0) {
      setErrors(prev => prev.filter(error => error.field !== field))
    }
  }

  const handleFileChange = (files: FileList | null) => {
    if (files) {
      setFormData(prev => ({ ...prev, photos: Array.from(files) }))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    // Clear previous errors
    setErrors([])
    setClientErrors({})
    
    // Client-side validation
    if (!validateForm()) {
      return
    }
    
    setIsLoading(true)
    
    try {
      // Create FormData for file uploads
      const data = new FormData()
      data.append('name', formData.name)
      if (formData.description) {
        data.append('description', formData.description)
      }
      
      // Add each photo to FormData
      formData.photos.forEach((photo, index) => {
        data.append(`photos`, photo)
      })
      
      await apiService.createMaterial(data)
      showSuccess('Material created successfully!', 'The material has been added to your inventory.')
      onSuccess()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Failed to create material', generalErrors[0])
      } else {
        showError('Failed to create material', 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Add New Material</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <span className="text-xl">×</span>
          </button>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Display general/non-field errors */}
          {getGeneralErrors(errors).map((error, index) => (
            <div key={index} className="bg-red-50 border border-red-200 rounded-lg p-4">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-sm text-red-700">{error}</p>
                </div>
              </div>
            </div>
          ))}

          <FormField 
            label="Material Name" 
            required
            errors={[...getErrorsForField(errors, 'name'), ...(clientErrors.name ? [clientErrors.name] : [])]}
          >
            <Input
              type="text"
              value={formData.name}
              onChange={(e) => handleInputChange('name', e.target.value)}
              placeholder="Sand, Gravel, Cement, etc."
              errors={[...getErrorsForField(errors, 'name'), ...(clientErrors.name ? [clientErrors.name] : [])]}
            />
          </FormField>
          
          <FormField label="Photos (Optional)">
            <Input
              type="file"
              accept="image/*"
              multiple
              onChange={(e) => handleFileChange(e.target.files)}
            />
            {formData.photos.length > 0 && (
              <div className="mt-2 text-sm text-gray-600">
                {formData.photos.length} photo(s) selected
              </div>
            )}
          </FormField>
          
          <FormField 
            label="Description (Optional)"
            errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
          >
            <Textarea
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              placeholder="Additional details about the material..."
              rows={3}
              errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
            />
          </FormField>
          
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">
              Cancel
            </button>
            <button type="submit" disabled={isLoading} className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 disabled:opacity-50">
              {isLoading ? 'Creating...' : 'Create Material'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// Material Card Component with Photo Viewer
function MaterialCard({ 
  material, 
  onEdit, 
  onDelete, 
  onAddVariant, 
  onRefresh 
}: { 
  material: Material; 
  onEdit: () => void; 
  onDelete: () => void;
  onAddVariant: () => void;
  onRefresh: () => void;
}) {
  const [showPhotoViewer, setShowPhotoViewer] = useState(false)
  const [selectedPhotoIndex, setSelectedPhotoIndex] = useState(0)

  const handlePhotoClick = (index: number) => {
    setSelectedPhotoIndex(index)
    setShowPhotoViewer(true)
  }

  return (
    <>
      <div className="card card-hover">
        <div className="space-y-4">
          {/* Header */}
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="w-12 h-12 bg-slate-100 rounded-lg flex items-center justify-center">
                <span className="text-xl">📦</span>
              </div>
              <div>
                <h3 className="text-lg font-semibold text-gray-900">{material.name}</h3>
                {material.description && (
                  <p className="text-gray-600 mt-1">{material.description}</p>
                )}
                <div className="flex items-center space-x-4 mt-2">
                  {material.variants && material.variants.length > 0 && (
                    <span className="text-sm text-slate-600">🏷️ {material.variants.length} variant(s)</span>
                  )}
                  {material.photos && material.photos.length > 0 && (
                    <span className="text-sm text-green-600">📷 {material.photos.length} photo(s)</span>
                  )}
                </div>
              </div>
            </div>
            
            <div className="flex items-center space-x-3">
              <div className="text-right text-sm text-gray-500">
                <p>Created: {new Date(material.created_at || Date.now()).toLocaleDateString()}</p>
              </div>
              <div className="flex space-x-2">
                <button onClick={onAddVariant} className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">
                  Add Variant
                </button>
                <button onClick={onEdit} className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">
                  Edit
                </button>
                <button onClick={onDelete} className="bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">
                  Delete
                </button>
              </div>
            </div>
          </div>

          {/* Material Variants */}
          {material.variants && material.variants.length > 0 && (
            <div className="border-t pt-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Variants ({material.variants.length})</h4>
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {material.variants.map((variant) => (
                  <VariantCard 
                    key={variant.id} 
                    variant={variant} 
                    onRefresh={onRefresh}
                  />
                ))}
              </div>
            </div>
          )}

          {/* Photos Grid */}
          {material.photos && material.photos.length > 0 && (
            <div className="border-t pt-4">
              <h4 className="text-sm font-medium text-gray-700 mb-3">Photos</h4>
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-3">
                {material.photos.map((photo, index: number) => (
                  <div
                    key={photo.id || index}
                    className="aspect-square bg-gray-100 rounded-lg overflow-hidden cursor-pointer hover:ring-2 hover:ring-amber-500 transition-all"
                    onClick={() => handlePhotoClick(index)}
                  >
                    <img
                      src={photo.photo}
                      alt={photo.caption || `Photo ${index + 1}`}
                      className="w-full h-full object-cover"
                      onError={(e) => {
                        // Fallback for broken images
                        const target = e.target as HTMLImageElement
                        target.src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100" viewBox="0 0 100 100"><rect width="100" height="100" fill="%23f3f4f6"/><text x="50%" y="50%" text-anchor="middle" dy=".3em" fill="%236b7280">📷</text></svg>'
                      }}
                    />
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Photo Viewer Modal */}
      {showPhotoViewer && material.photos && material.photos.length > 0 && (
        <PhotoViewer
          photos={material.photos}
          initialIndex={selectedPhotoIndex}
          onClose={() => setShowPhotoViewer(false)}
          materialName={material.name}
        />
      )}
    </>
  )
}

// Variant Card Component
function VariantCard({ variant, onRefresh }: { 
  variant: MaterialVariant; 
  onRefresh: () => void;
}) {
  const { showSuccess, showError } = useToast()
  const [showPhotoViewer, setShowPhotoViewer] = useState(false)
  const [selectedPhotoIndex, setSelectedPhotoIndex] = useState(0)

  const handlePhotoClick = (index: number) => {
    setSelectedPhotoIndex(index)
    setShowPhotoViewer(true)
  }

  const handleDeleteVariant = async () => {
    if (!confirm(`Are you sure you want to delete variant "${variant.name}"?`)) return

    try {
      await apiService.deleteMaterialVariant(variant.id)
      showSuccess('Variant deleted', `${variant.name} has been removed.`)
      onRefresh()
    } catch (error) {
      console.error('Failed to delete variant:', error)
      showError('Failed to delete variant', error instanceof Error ? error.message : 'An unexpected error occurred')
    }
  }

  return (
    <>
      <div className="bg-gray-50 rounded-lg p-4 hover:bg-gray-100 transition-colors">
        <div className="flex items-center justify-between mb-2">
          <h5 className="font-medium text-gray-900">{variant.name}</h5>
          <button
            onClick={handleDeleteVariant}
            className="text-red-500 hover:text-red-700 text-sm"
          >
            ×
          </button>
        </div>
        
        {variant.description && (
          <p className="text-sm text-gray-600 mb-3">{variant.description}</p>
        )}
        
        <div className="flex items-center justify-between">
          <div className="text-xs text-gray-500">
            {variant.photos && variant.photos.length > 0 && (
              <span>📷 {variant.photos.length} photo(s)</span>
            )}
          </div>
          <div className="text-xs text-gray-500">
            {variant.material_name}
          </div>
        </div>

        {/* Variant Photos */}
        {variant.photos && variant.photos.length > 0 && (
          <div className="mt-3 grid grid-cols-3 gap-2">
            {variant.photos.slice(0, 3).map((photo, index) => (
              <div
                key={photo.id}
                className="aspect-square bg-gray-200 rounded overflow-hidden cursor-pointer hover:ring-2 hover:ring-slate-500"
                onClick={() => handlePhotoClick(index)}
              >
                <img
                  src={photo.photo}
                  alt={photo.caption || `Variant photo ${index + 1}`}
                  className="w-full h-full object-cover"
                  onError={(e) => {
                    const target = e.target as HTMLImageElement
                    target.src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="60" height="60" viewBox="0 0 60 60"><rect width="60" height="60" fill="%23e5e7eb"/><text x="50%" y="50%" text-anchor="middle" dy=".3em" fill="%236b7280" font-size="20">📷</text></svg>'
                  }}
                />
              </div>
            ))}
            {variant.photos.length > 3 && (
              <div className="aspect-square bg-gray-200 rounded flex items-center justify-center text-gray-500 text-xs">
                +{variant.photos.length - 3} more
              </div>
            )}
          </div>
        )}
      </div>

      {/* Photo Viewer for Variant Photos */}
      {showPhotoViewer && variant.photos && variant.photos.length > 0 && (
        <PhotoViewer
          photos={variant.photos}
          initialIndex={selectedPhotoIndex}
          onClose={() => setShowPhotoViewer(false)}
          materialName={`${variant.material_name} - ${variant.name}`}
        />
      )}
    </>
  )
}

// Add Variant Dialog Component
function AddVariantDialog({ 
  material, 
  onClose, 
  onSuccess 
}: { 
  material: Material;
  onClose: () => void; 
  onSuccess: () => void; 
}) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    photos: [] as File[]
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const nameError = validateRequired(formData.name, 'Variant name')
    if (nameError) newErrors.name = nameError
    
    setClientErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    if (clientErrors[field]) {
      setClientErrors(prev => ({ ...prev, [field]: '' }))
    }
    if (getErrorsForField(errors, field).length > 0) {
      setErrors(prev => prev.filter(error => error.field !== field))
    }
  }

  const handleFileChange = (files: FileList | null) => {
    if (files) {
      setFormData(prev => ({ ...prev, photos: Array.from(files) }))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    // Clear previous errors
    setErrors([])
    setClientErrors({})
    
    // Client-side validation
    if (!validateForm()) {
      return
    }
    
    setIsLoading(true)
    
    try {
      // Create FormData for multipart upload
      const data = new FormData()
      data.append('material', material.id)
      data.append('name', formData.name)
      if (formData.description) {
        data.append('description', formData.description)
      }
      
      // Add each photo to FormData
      formData.photos.forEach((photo) => {
        data.append('photos', photo)
      })
      
      await apiService.createMaterialVariant(data)
      showSuccess('Variant created successfully!', `${formData.name} has been added to ${material.name}.`)
      onSuccess()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Failed to create variant', generalErrors[0])
      } else {
        showError('Failed to create variant', 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Add Variant to {material.name}</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <span className="text-xl">×</span>
          </button>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Display general/non-field errors */}
          {getGeneralErrors(errors).map((error, index) => (
            <div key={index} className="bg-red-50 border border-red-200 rounded-lg p-4">
              <div className="flex">
                <div className="flex-shrink-0">
                  <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                  </svg>
                </div>
                <div className="ml-3">
                  <p className="text-sm text-red-700">{error}</p>
                </div>
              </div>
            </div>
          ))}

          <FormField 
            label="Variant Name" 
            required
            errors={[...getErrorsForField(errors, 'name'), ...(clientErrors.name ? [clientErrors.name] : [])]}
          >
            <Input
              type="text"
              value={formData.name}
              onChange={(e) => handleInputChange('name', e.target.value)}
              placeholder="Darugo, Kajido, River Sand, etc."
              errors={[...getErrorsForField(errors, 'name'), ...(clientErrors.name ? [clientErrors.name] : [])]}
            />
          </FormField>
          
          <FormField 
            label="Description (Optional)"
            errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
          >
            <Textarea
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              placeholder="Details about this variant..."
              rows={3}
              errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
            />
          </FormField>
          
          <FormField label="Photos (Optional)">
            <Input
              type="file"
              accept="image/*"
              multiple
              onChange={(e) => handleFileChange(e.target.files)}
            />
            {formData.photos.length > 0 && (
              <div className="mt-2 text-sm text-gray-600">
                {formData.photos.length} photo(s) selected
              </div>
            )}
          </FormField>
          
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">
              Cancel
            </button>
            <button type="submit" disabled={isLoading} className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 disabled:opacity-50">
              {isLoading ? 'Creating...' : 'Create Variant'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// Photo Viewer Component
function PhotoViewer({ 
  photos, 
  initialIndex, 
  onClose, 
  materialName 
}: { 
  photos: any[]; 
  initialIndex: number; 
  onClose: () => void;
  materialName: string;
}) {
  const [currentIndex, setCurrentIndex] = useState(initialIndex)

  const goToPrevious = () => {
    setCurrentIndex((prev) => (prev === 0 ? photos.length - 1 : prev - 1))
  }

  const goToNext = () => {
    setCurrentIndex((prev) => (prev === photos.length - 1 ? 0 : prev + 1))
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'ArrowLeft') goToPrevious()
    if (e.key === 'ArrowRight') goToNext()
    if (e.key === 'Escape') onClose()
  }

  return (
    <div 
      className="fixed inset-0 bg-black bg-opacity-90 flex items-center justify-center z-50"
      onClick={onClose}
      onKeyDown={handleKeyDown}
      tabIndex={0}
    >
      <div className="relative max-w-4xl max-h-full p-4" onClick={(e) => e.stopPropagation()}>
        {/* Header */}
        <div className="absolute top-4 left-4 right-4 flex items-center justify-between text-white z-10">
          <div>
            <h3 className="text-lg font-semibold">{materialName}</h3>
            <p className="text-sm opacity-75">
              {currentIndex + 1} of {photos.length}
              {photos[currentIndex]?.caption && ` • ${photos[currentIndex].caption}`}
            </p>
          </div>
          <button 
            onClick={onClose}
            className="bg-black bg-opacity-50 hover:bg-opacity-75 rounded-full p-2 transition-all"
          >
            <span className="text-2xl">×</span>
          </button>
        </div>

        {/* Navigation Buttons */}
        {photos.length > 1 && (
          <>
            <button
              onClick={goToPrevious}
              className="absolute left-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 hover:bg-opacity-75 text-white rounded-full p-3 transition-all z-10"
            >
              <span className="text-xl">‹</span>
            </button>
            <button
              onClick={goToNext}
              className="absolute right-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 hover:bg-opacity-75 text-white rounded-full p-3 transition-all z-10"
            >
              <span className="text-xl">›</span>
            </button>
          </>
        )}

        {/* Main Image */}
        <div className="flex items-center justify-center">
          <img
            src={photos[currentIndex]?.photo || photos[currentIndex]?.url}
            alt={photos[currentIndex]?.caption || `Photo ${currentIndex + 1}`}
            className="max-w-full max-h-[80vh] object-contain rounded-lg"
            onError={(e) => {
              const target = e.target as HTMLImageElement
              target.src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="400" height="300" viewBox="0 0 400 300"><rect width="400" height="300" fill="%23374151"/><text x="50%" y="50%" text-anchor="middle" dy=".3em" fill="%23f3f4f6" font-size="48">📷</text></svg>'
            }}
          />
        </div>

        {/* Thumbnail Strip */}
        {photos.length > 1 && (
          <div className="absolute bottom-4 left-1/2 transform -translate-x-1/2">
            <div className="flex space-x-2 bg-black bg-opacity-50 rounded-lg p-2">
              {photos.map((photo, index) => (
                <button
                  key={photo.id || index}
                  onClick={() => setCurrentIndex(index)}
                  className={`w-16 h-12 rounded overflow-hidden transition-all ${
                    index === currentIndex 
                      ? 'ring-2 ring-white' 
                      : 'opacity-60 hover:opacity-100'
                  }`}
                >
                  <img
                    src={photo.photo || photo.url}
                    alt={`Thumbnail ${index + 1}`}
                    className="w-full h-full object-cover"
                    onError={(e) => {
                      const target = e.target as HTMLImageElement
                      target.src = 'data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" width="64" height="48" viewBox="0 0 64 48"><rect width="64" height="48" fill="%236b7280"/><text x="50%" y="50%" text-anchor="middle" dy=".3em" fill="%23f3f4f6" font-size="16">📷</text></svg>'
                    }}
                  />
                </button>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}