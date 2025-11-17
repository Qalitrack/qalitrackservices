'use client'

import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input, Textarea, Select } from '@/components/ui/FormField'
import { RefreshButton } from '@/components/ui/RefreshButton'
import {
  ValidationError,
  parseApiErrors,
  getErrorsForField,
  getGeneralErrors,
  validateRequired
} from '@/lib/validation'
import { TripType, CreateTripTypeRequest } from '@/lib/types/api'

interface TripTypesTabProps {
  isActive: boolean
}

export default function TripTypesTab({ isActive }: TripTypesTabProps) {
  const [tripTypes, setTripTypes] = useState<TripType[]>([])
  const [loading, setLoading] = useState(false)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const [editingTripType, setEditingTripType] = useState<TripType | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [sortBy, setSortBy] = useState('category')
  const [filterBy, setFilterBy] = useState('all')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [hasNextPage, setHasNextPage] = useState(false)
  const { showSuccess, showError } = useToast()

  // Form state
  const [formData, setFormData] = useState<CreateTripTypeRequest>({
    name: '',
    description: '',
    category: 'other',
    empty_trip_option: '',
    material_requirement: 'none',
    is_active: true
  })
  const [formErrors, setFormErrors] = useState<ValidationError[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)

  // Only fetch data when tab is active
  useEffect(() => {
    if (isActive) {
      fetchTripTypes(true)
    }
  }, [isActive])

  useEffect(() => {
    if (isActive) {
      const timer = setTimeout(() => {
        fetchTripTypes(true)
      }, 500)
      return () => clearTimeout(timer)
    }
  }, [searchTerm, isActive])

  useEffect(() => {
    if (isActive) {
      fetchTripTypes(true)
    }
  }, [sortBy, filterBy, isActive])

  const fetchTripTypes = async (reset = false, page = 1) => {
    try {
      if (reset) {
        setLoading(true)
        setCurrentPage(1)
      } else {
        setLoading(true)
      }

      const response = await apiClient.getTripTypes({
        page,
        ordering: sortBy === 'name' ? 'name' : sortBy === 'created_at' ? '-created_at' : 'category,name',
        search: searchTerm || undefined,
        category: filterBy !== 'all' ? filterBy : undefined,
        page_size: 20
      })

      if (reset) {
        setTripTypes(response.results || [])
      } else {
        setTripTypes(prev => [...prev, ...(response.results || [])])
      }

      setTotalCount(response.count || 0)
      setHasNextPage(!!response.next)
    } catch (error) {
      console.error('Failed to fetch trip types:', error)
      showError('Failed to load trip types')
    } finally {
      setLoading(false)
    }
  }

  const handleAdd = () => {
    setFormData({
      name: '',
      description: '',
      category: 'other',
      empty_trip_option: '',
      material_requirement: 'none',
      is_active: true
    })
    setFormErrors([])
    setEditingTripType(null)
    setShowAddDialog(true)
  }

  const handleEdit = (tripType: TripType) => {
    setFormData({
      name: tripType.name,
      description: tripType.description,
      category: tripType.category,
      empty_trip_option: tripType.empty_trip_option,
      material_requirement: tripType.material_requirement,
      is_active: tripType.is_active
    })
    setFormErrors([])
    setEditingTripType(tripType)
    setShowAddDialog(true)
  }

  const validateForm = (): ValidationError[] => {
    const errors: ValidationError[] = []

    if (!validateRequired(formData.name)) {
      errors.push({ field: 'name', message: 'Trip type name is required' })
    }

    if (formData.category === 'empty' && !validateRequired(formData.empty_trip_option)) {
      errors.push({ field: 'empty_trip_option', message: 'Empty trip option is required for empty trip category' })
    }

    return errors
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()

    const errors = validateForm()
    if (errors.length > 0) {
      setFormErrors(errors)
      return
    }

    setIsSubmitting(true)
    try {
      const data: CreateTripTypeRequest = {
        name: formData.name?.trim() || '',
        description: formData.description?.trim(),
        category: formData.category,
        empty_trip_option: formData.empty_trip_option?.trim(),
        material_requirement: formData.material_requirement,
        is_active: formData.is_active
      }

      if (editingTripType) {
        await apiClient.updateTripType(editingTripType.id, data)
        showSuccess('Trip type updated successfully')
      } else {
        await apiClient.createTripType(data)
        showSuccess('Trip type created successfully')
      }

      setShowAddDialog(false)
      fetchTripTypes(true)
    } catch (error: any) {
      const parsedErrors = parseApiErrors(error)
      setFormErrors(parsedErrors)
      const generalErrors = getGeneralErrors(parsedErrors)
      if (generalErrors.length > 0) {
        showError(generalErrors[0])
      } else {
        showError(editingTripType ? 'Failed to update trip type' : 'Failed to create trip type')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleDelete = async (tripType: TripType) => {
    if (!confirm(`Are you sure you want to delete "${tripType.name}"? This action cannot be undone.`)) {
      return
    }

    try {
      await apiClient.deleteTripType(tripType.id)
      showSuccess('Trip type deleted successfully')
      fetchTripTypes(true)
    } catch (error) {
      console.error('Failed to delete trip type:', error)
      showError('Failed to delete trip type')
    }
  }

  const handleToggleActive = async (tripType: TripType) => {
    try {
      await apiClient.updateTripType(tripType.id, {
        is_active: !tripType.is_active
      })
      showSuccess(`Trip type ${tripType.is_active ? 'deactivated' : 'activated'} successfully`)
      fetchTripTypes(true)
    } catch (error) {
      console.error('Failed to toggle trip type status:', error)
      showError('Failed to update trip type status')
    }
  }

  const getCategoryBadgeColor = (category: string) => {
    switch (category) {
      case 'loaded': return 'bg-blue-100 text-blue-800'
      case 'empty': return 'bg-yellow-100 text-yellow-800'
      case 'maintenance': return 'bg-red-100 text-red-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  if (!isActive) {
    return null // Don't render content when tab is not active
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-xl font-semibold text-gray-900">Trip Types</h2>
          <p className="text-gray-600 text-sm">Manage trip types that drivers can select when creating trips</p>
        </div>
        <div className="flex gap-3">
          <RefreshButton onRefresh={() => fetchTripTypes(true)} loading={loading} />
          <button
            onClick={handleAdd}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm"
          >
            Add Trip Type
          </button>
        </div>
      </div>

      {/* Filters and Search */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <input
              type="text"
              placeholder="Search trip types..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
            />
          </div>
          <div>
            <select
              value={filterBy}
              onChange={(e) => setFilterBy(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
            >
              <option value="all">All Categories</option>
              <option value="loaded">Loaded Trips</option>
              <option value="empty">Empty Trips</option>
              <option value="maintenance">Maintenance</option>
              <option value="other">Other</option>
            </select>
          </div>
          <div>
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
            >
              <option value="category">Sort by Category</option>
              <option value="name">Sort by Name</option>
              <option value="created_at">Sort by Created Date</option>
            </select>
          </div>
        </div>
      </div>

      {/* Trip Types List */}
      {loading && tripTypes.length === 0 ? (
        <div className="flex justify-center items-center h-64">
          <div className="text-gray-500">Loading trip types...</div>
        </div>
      ) : tripTypes.length === 0 ? (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
          <div className="text-gray-500 mb-4">No trip types found</div>
          <button
            onClick={handleAdd}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Create First Trip Type
          </button>
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Name
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Category
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Empty Trip Option
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Materials
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Created By
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {tripTypes.map((tripType) => (
                  <tr key={tripType.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <div>
                        <div className="font-medium text-gray-900 text-sm">{tripType.name}</div>
                        {tripType.description && (
                          <div className="text-xs text-gray-500 mt-1">{tripType.description}</div>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-1 text-xs font-medium rounded-full ${getCategoryBadgeColor(tripType.category)}`}>
                        {tripType.category.charAt(0).toUpperCase() + tripType.category.slice(1)}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {tripType.empty_trip_option || '-'}
                    </td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-1 text-xs font-medium rounded-full ${
                        tripType.material_requirement === 'mandatory'
                          ? 'bg-red-100 text-red-800'
                          : tripType.material_requirement === 'optional'
                            ? 'bg-yellow-100 text-yellow-800'
                            : 'bg-gray-100 text-gray-800'
                      }`}>
                        {tripType.material_requirement.charAt(0).toUpperCase() + tripType.material_requirement.slice(1)}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => handleToggleActive(tripType)}
                        className={`px-2 py-1 text-xs font-medium rounded-full ${
                          tripType.is_active
                            ? 'bg-green-100 text-green-800 hover:bg-green-200'
                            : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                        } transition-colors`}
                      >
                        {tripType.is_active ? 'Active' : 'Inactive'}
                      </button>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {tripType.created_by_name}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="flex justify-end gap-2">
                        <button
                          onClick={() => handleEdit(tripType)}
                          className="text-blue-600 hover:text-blue-800 text-sm font-medium"
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(tripType)}
                          className="text-red-600 hover:text-red-800 text-sm font-medium"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Load More */}
          {hasNextPage && (
            <div className="p-4 border-t border-gray-200">
              <button
                onClick={() => {
                  const nextPage = currentPage + 1
                  setCurrentPage(nextPage)
                  fetchTripTypes(false, nextPage)
                }}
                disabled={loading}
                className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors disabled:opacity-50"
              >
                {loading ? 'Loading...' : 'Load More'}
              </button>
            </div>
          )}
        </div>
      )}

      {/* Add/Edit Modal */}
      {showAddDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div className="p-6">
              <h3 className="text-lg font-bold text-gray-900 mb-4">
                {editingTripType ? 'Edit Trip Type' : 'Add Trip Type'}
              </h3>

              <form onSubmit={handleSubmit}>
                <div className="space-y-4">
                  <FormField
                    label="Trip Type Name"
                    required
                    error={getErrorsForField(formErrors, 'name')}
                  >
                    <Input
                      value={formData.name}
                      onChange={(value) => setFormData(prev => ({ ...prev, name: value }))}
                      placeholder="e.g., Material Delivery, Car Wash"
                    />
                  </FormField>

                  <FormField
                    label="Category"
                    error={getErrorsForField(formErrors, 'category')}
                  >
                    <Select
                      value={formData.category}
                      onChange={(value) => setFormData(prev => ({ ...prev, category: value as any }))}
                    >
                      <option value="loaded">Loaded Trip</option>
                      <option value="empty">Empty Trip</option>
                      <option value="maintenance">Maintenance</option>
                      <option value="other">Other</option>
                    </Select>
                  </FormField>

                  {formData.category === 'empty' && (
                    <FormField
                      label="Empty Trip Option"
                      required
                      error={getErrorsForField(formErrors, 'empty_trip_option')}
                    >
                      <Input
                        value={formData.empty_trip_option}
                        onChange={(value) => setFormData(prev => ({ ...prev, empty_trip_option: value }))}
                        placeholder="e.g., Car Wash, Starting Day, Ending Day"
                      />
                    </FormField>
                  )}

                  <FormField
                    label="Material Requirement"
                    error={getErrorsForField(formErrors, 'material_requirement')}
                  >
                    <Select
                      value={formData.material_requirement}
                      onChange={(value) => setFormData(prev => ({ ...prev, material_requirement: value as any }))}
                    >
                      <option value="none">No Materials</option>
                      <option value="optional">Optional Materials</option>
                      <option value="mandatory">Mandatory Materials</option>
                    </Select>
                  </FormField>

                  <FormField
                    label="Description"
                    error={getErrorsForField(formErrors, 'description')}
                  >
                    <Textarea
                      value={formData.description}
                      onChange={(value) => setFormData(prev => ({ ...prev, description: value }))}
                      placeholder="Describe what this trip type entails..."
                      rows={3}
                    />
                  </FormField>

                  <div className="flex items-center">
                    <input
                      type="checkbox"
                      id="is_active"
                      checked={formData.is_active}
                      onChange={(e) => setFormData(prev => ({ ...prev, is_active: e.target.checked }))}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <label htmlFor="is_active" className="ml-2 text-sm text-gray-700">
                      Active (available for drivers to select)
                    </label>
                  </div>

                  {getGeneralErrors(formErrors).length > 0 && (
                    <div className="rounded-lg bg-red-50 p-3">
                      {getGeneralErrors(formErrors).map((error, index) => (
                        <div key={index} className="text-sm text-red-800">{error}</div>
                      ))}
                    </div>
                  )}
                </div>

                <div className="flex justify-end gap-3 mt-6">
                  <button
                    type="button"
                    onClick={() => setShowAddDialog(false)}
                    className="px-4 py-2 text-gray-700 bg-gray-100 rounded-lg hover:bg-gray-200 transition-colors"
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={isSubmitting}
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50"
                  >
                    {isSubmitting ? 'Saving...' : (editingTripType ? 'Update' : 'Create')}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}