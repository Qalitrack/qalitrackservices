'use client'

import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { useToast } from '@/components/ui/Toast'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { 
  FormField, 
  Input, 
  Select, 
  Textarea, 
  Button
} from '@/components/ui/FormField'
import {
  parseApiErrors,
  getErrorsForField,
  getGeneralErrors,
  validateRequired
} from '@/lib/validation'
import { Trip, TripListParams, CreateTripRequest, Truck, Driver, Material, MaterialVariant } from '@/lib/types/api'

export default function TripsPage() {
  const [trips, setTrips] = useState<Trip[]>([])
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const [selectedTrip, setSelectedTrip] = useState<Trip | null>(null)
  const [showTripDetails, setShowTripDetails] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState<'all' | 'pending' | 'in_progress' | 'completed' | 'cancelled'>('all')
  const [sortBy, setSortBy] = useState('date')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [hasNextPage, setHasNextPage] = useState(false)
  
  // Form data for creating trips
  const [trucks, setTrucks] = useState<Truck[]>([])
  const [drivers, setDrivers] = useState<Driver[]>([])
  const [materials, setMaterials] = useState<Material[]>([])
  const [materialVariants, setMaterialVariants] = useState<MaterialVariant[]>([])
  
  const { showSuccess, showError } = useToast()

  useEffect(() => {
    fetchTrips(true) // Reset on mount
    fetchFormData() // Load trucks, drivers, materials
  }, [])

  useEffect(() => {
    // Debounced search - trigger for both search and clear
    const timer = setTimeout(() => {
      fetchTrips(true) // Reset when searching or clearing
    }, 500)

    return () => clearTimeout(timer)
  }, [searchTerm])

  useEffect(() => {
    // Reset when filter/sort changes
    fetchTrips(true)
  }, [statusFilter, sortBy])

  const fetchFormData = async () => {
    try {
      const [trucksResponse, driversResponse, materialsResponse] = await Promise.all([
        apiClient.getTrucks(),
        apiClient.getDriverProfiles(),
        apiClient.getMaterials()
      ])
      
      setTrucks(trucksResponse.results || [])
      setDrivers(driversResponse.results || [])
      setMaterials(materialsResponse.results || [])
    } catch (error) {
      console.error('Failed to load form data:', error)
    }
  }

  const fetchTrips = async (reset = false, page = 1) => {
    try {
      if (reset) {
        setLoading(true)
        setCurrentPage(1)
        setTrips([])
      } else {
        setLoadingMore(true)
      }

      const params: TripListParams = {
        page,
        page_size: 20
      }

      if (searchTerm.trim()) {
        params.search = searchTerm.trim()
      }

      if (statusFilter !== 'all') {
        params.status = statusFilter
      }

      // Add sorting
      if (sortBy) {
        params.ordering = sortBy
      }

      const response = await apiClient.getTrips(params)
      
      if (reset) {
        setTrips(response.results || [])
      } else {
        setTrips(prev => [...prev, ...(response.results || [])])
      }
      
      setTotalCount(response.count || 0)
      setHasNextPage(!!response.next)
      setCurrentPage(page)
      
    } catch (error) {
      console.error('Failed to load trips:', error)
      showError('Failed to load trips', error instanceof Error ? error.message : 'An unexpected error occurred')
    } finally {
      setLoading(false)
      setLoadingMore(false)
    }
  }

  const loadMoreTrips = () => {
    if (hasNextPage && !loadingMore) {
      fetchTrips(false, currentPage + 1)
    }
  }

  const handleSearch = (value: string) => {
    setSearchTerm(value)
    // The useEffect will handle the debounced search
  }

  const getStatusColor = (status: Trip['status']) => {
    switch (status) {
      case 'completed': return 'bg-green-100 text-green-800'
      case 'in_progress': return 'bg-slate-100 text-slate-800'
      case 'pending': return 'bg-yellow-100 text-yellow-800'
      case 'cancelled': return 'bg-red-100 text-red-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusIcon = (status: Trip['status']) => {
    switch (status) {
      case 'completed': return '✅'
      case 'in_progress': return '🚛'
      case 'pending': return '📅'
      case 'cancelled': return '❌'
      default: return '❓'
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Trips Management</h1>
          <p className="text-gray-600">
            Monitor and manage all fleet trips
            {totalCount > 0 && (
              <span className="text-slate-600 ml-2">
                ({totalCount} total, {trips.length} loaded)
              </span>
            )}
          </p>
        </div>
        <div className="flex items-center space-x-4">
          <RefreshButton onRefresh={() => fetchTrips(true)} loading={loading} theme="admin" />
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        {(['pending', 'in_progress', 'completed', 'cancelled'] as const).map(status => {
          const count = trips.filter(t => t.status === status).length
          return (
            <div key={status} className="card">
              <div className="flex items-center space-x-3">
                <span className="text-2xl">{getStatusIcon(status)}</span>
                <div>
                  <p className="text-2xl font-bold text-gray-900">{count}</p>
                  <p className="text-sm text-gray-600 capitalize">{status.replace('_', ' ')}</p>
                </div>
              </div>
            </div>
          )
        })}
      </div>

      {/* Search and Filter Bar */}
      <div className="card p-4">
        <div className="flex flex-col lg:flex-row gap-4 items-start lg:items-center">
          {/* Search Section */}
          <div className="flex-1 w-full lg:w-auto">
            <label className="block text-sm font-medium text-gray-700 mb-2">Search Trips</label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <span className="text-gray-400 text-lg">🔍</span>
              </div>
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => handleSearch(e.target.value)}
                className="input pl-10 pr-10 w-full"
                placeholder="Search by locations..."
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
          
          {/* Status Filter & Sort Section */}
          <div className="flex flex-col sm:flex-row gap-4 w-full lg:w-auto">
            {/* Status Filter */}
            <div className="min-w-[160px]">
              <label className="block text-sm font-medium text-gray-700 mb-2">Filter by Status</label>
              <div className="relative">
                <select
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value as any)}
                  className="input pr-8 appearance-none cursor-pointer w-full"
                >
                  <option value="all">📋 All Status</option>
                  <option value="pending">📅 Pending</option>
                  <option value="in_progress">🚛 In Progress</option>
                  <option value="completed">✅ Completed</option>
                  <option value="cancelled">❌ Cancelled</option>
                </select>
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <span className="text-gray-400">▼</span>
                </div>
              </div>
            </div>
            
            {/* Sort */}
            <div className="min-w-[160px]">
              <label className="block text-sm font-medium text-gray-700 mb-2">Sort By</label>
              <div className="relative">
                <select
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value)}
                  className="input pr-8 appearance-none cursor-pointer w-full"
                >
                  <option value="date">📅 Date (Newest)</option>
                  <option value="-date">📅 Date (Oldest)</option>
                  <option value="total_cost">💰 Cost (Low to High)</option>
                  <option value="-total_cost">💰 Cost (High to Low)</option>
                  <option value="status">📋 Status (A-Z)</option>
                  <option value="-status">📋 Status (Z-A)</option>
                </select>
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <span className="text-gray-400">▼</span>
                </div>
              </div>
            </div>
            
            {/* Clear Filters Button */}
            {(searchTerm || statusFilter !== 'all' || sortBy !== 'date') && (
              <div className="flex items-end">
                <button
                  onClick={() => {
                    setSearchTerm('')
                    setStatusFilter('all')
                    setSortBy('date')
                    handleSearch('')
                  }}
                  className="btn btn-outline text-gray-600 hover:text-red-600 hover:border-red-300 transition-colors h-[42px]"
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
        {(searchTerm || statusFilter !== 'all' || sortBy !== 'date') && (
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
              
              {statusFilter !== 'all' && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                  {getStatusIcon(statusFilter)} {statusFilter.replace('_', ' ')}
                  <button
                    onClick={() => setStatusFilter('all')}
                    className="ml-1 text-green-600 hover:text-green-800"
                  >
                    ×
                  </button>
                </span>
              )}
              
              {sortBy !== 'date' && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-slate-100 text-slate-800">
                  📋 {sortBy.replace('-', '').replace('_', ' ')}
                  <button
                    onClick={() => setSortBy('date')}
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

      {/* Trips List */}
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
      ) : trips.length === 0 ? (
        <div className="card text-center py-12">
          <div className="text-6xl mb-4">🗺️</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">
            {statusFilter === 'all' ? 'No trips yet' : `No ${statusFilter} trips`}
          </h3>
          <p className="text-gray-600">
            {statusFilter === 'all' 
              ? 'Trips will appear here once drivers start creating them.'
              : `No trips with status "${statusFilter}" found.`
            }
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {trips.map((trip) => (
            <div key={trip.id} className="card card-hover">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <div className="w-12 h-12 bg-slate-100 rounded-lg flex items-center justify-center">
                    <span className="text-xl">{getStatusIcon(trip.status)}</span>
                  </div>
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">
                      Trip #{trip.id}
                    </h3>
                    <p className="text-gray-600">
                      {trip.start_location || trip.origin || 'Unknown'} → {trip.end_location || trip.destination || 'Unknown'}
                    </p>
                    <div className="flex items-center space-x-4 mt-1">
                      <span className="text-sm text-gray-500">
                        Driver: {trip.driver_name || trip.driver?.name || 'Unassigned'}
                      </span>
                      <span className="text-sm text-gray-500">
                        Date: {trip.date ? new Date(trip.date).toLocaleDateString() : 'Not set'}
                      </span>
                      {trip.total_cost && (
                        <span className="text-sm text-gray-500">
                          Cost: KSh {trip.total_cost}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
                
                {/* Financial Summary */}
                {trip.material_cost && parseFloat(trip.material_cost) > 0 && (
                  <div className="text-right">
                    <div className="space-y-1">
                      <div className="text-sm text-gray-500">Revenue</div>
                      <div className="text-lg font-bold text-green-600">KSh {parseFloat(trip.material_cost).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
                      
                      {trip.expenses && trip.expenses.length > 0 && (() => {
                        const totalExpenses = trip.expenses.reduce((sum, expense) => sum + parseFloat(expense.amount || '0'), 0)
                        const revenue = parseFloat(trip.material_cost || '0')
                        const netProfit = revenue - totalExpenses
                        
                        return (
                          <>
                            <div className="text-sm text-gray-500">Expenses</div>
                            <div className="text-sm font-semibold text-red-600">KSh {totalExpenses.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
                            <div className="text-xs text-gray-400 border-t pt-1">Net Profit</div>
                            <div className={`text-sm font-bold ${netProfit >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                              KSh {netProfit.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                            </div>
                          </>
                        )
                      })()}
                    </div>
                  </div>
                )}
                
                <div className="flex items-center space-x-4">
                  <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(trip.status)}`}>
                    {trip.status}
                  </span>
                  <div className="flex space-x-2">
                    <button 
                      onClick={() => {
                        setSelectedTrip(trip)
                        setShowTripDetails(true)
                      }}
                      className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm"
                    >
                      View Details
                    </button>
                    <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">
                      Edit
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Trip Details Modal */}
      {showTripDetails && selectedTrip && (
        <TripDetailsModal 
          trip={selectedTrip}
          onClose={() => {
            setShowTripDetails(false)
            setSelectedTrip(null)
          }}
        />
      )}
    </div>
  )
}

// Trip Card Component
function TripCard({ 
  trip, 
  onEdit, 
  onDelete, 
  onRefresh 
}: { 
  trip: Trip; 
  onEdit: () => void; 
  onDelete: () => void;
  onRefresh: () => void;
}) {
  const { showSuccess, showError } = useToast()
  
  const getStatusColor = (status: Trip['status']) => {
    switch (status) {
      case 'completed': return 'bg-green-100 text-green-800'
      case 'in_progress': return 'bg-slate-100 text-slate-800'
      case 'pending': return 'bg-yellow-100 text-yellow-800'
      case 'cancelled': return 'bg-red-100 text-red-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusIcon = (status: Trip['status']) => {
    switch (status) {
      case 'completed': return '✅'
      case 'in_progress': return '🚛'
      case 'pending': return '📅'
      case 'cancelled': return '❌'
      default: return '❓'
    }
  }

  const calculateTripCost = async () => {
    try {
      const result = await apiClient.calculateTripCost(trip.id)
      showSuccess('Trip cost calculated', `Total cost: KSh ${result.total_cost}`)
      onRefresh()
    } catch (error) {
      console.error('Failed to calculate cost:', error)
      showError('Failed to calculate cost', error instanceof Error ? error.message : 'An unexpected error occurred')
    }
  }

  return (
    <div className="card card-hover">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <div className="w-12 h-12 bg-slate-100 rounded-lg flex items-center justify-center">
            <span className="text-xl">{getStatusIcon(trip.status)}</span>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">
              Trip #{trip.id.slice(0, 8)}...
            </h3>
            <p className="text-gray-600">
              {trip.start_location || 'Unknown'} → {trip.end_location || 'Unknown'}
            </p>
            <div className="flex items-center space-x-4 mt-1">
              <span className="text-sm text-gray-500">
                Driver: {trip.driver?.name || 'Unassigned'}
              </span>
              <span className="text-sm text-gray-500">
                Truck: {trip.truck?.license_plate || 'Unassigned'}
              </span>
              <span className="text-sm text-gray-500">
                Date: {trip.date ? new Date(trip.date).toLocaleDateString() : 'Not set'}
              </span>
            </div>
            {trip.material && (
              <div className="flex items-center space-x-2 mt-1">
                <span className="text-sm text-slate-600">
                  🏗️ {trip.material.name}
                </span>
                {trip.material_variant && (
                  <span className="text-sm text-slate-500">
                    • {trip.material_variant.name}
                  </span>
                )}
                {trip.material_cost && (
                  <span className="text-sm text-green-600">
                    • Material: KSh {trip.material_cost}
                  </span>
                )}
              </div>
            )}
          </div>
        </div>
        
        <div className="flex items-center space-x-4">
          <div className="text-right">
            <div className="flex items-center space-x-2 mb-1">
              <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(trip.status)}`}>
                {trip.status.replace('_', ' ')}
              </span>
            </div>
            {trip.total_cost && (
              <p className="text-sm font-semibold text-green-600">
                Total: KSh {trip.total_cost}
              </p>
            )}
            {trip.total_mileage && (
              <p className="text-xs text-gray-500">
                {trip.total_mileage} miles
              </p>
            )}
          </div>
          <div className="flex space-x-2">
            <button 
              onClick={calculateTripCost}
              className="bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm"
              title="Calculate Total Cost"
            >
              💰
            </button>
            <button 
              onClick={onEdit}
              className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm"
            >
              Edit
            </button>
            <button 
              onClick={onDelete}
              className="btn btn-sm btn-outline text-red-600"
            >
              Delete
            </button>
          </div>
        </div>
      </div>
      
      {/* Trip Expenses Preview */}
      {trip.expenses && trip.expenses.length > 0 && (
        <div className="border-t pt-4 mt-4">
          <h4 className="text-sm font-medium text-gray-700 mb-2">
            Expenses ({trip.expenses.length})
          </h4>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-2">
            {trip.expenses.slice(0, 3).map((expense) => (
              <div key={expense.id} className="bg-gray-50 rounded p-2 text-sm">
                <div className="font-medium">{expense.description}</div>
                <div className="text-green-600">KSh {expense.amount}</div>
              </div>
            ))}
            {trip.expenses.length > 3 && (
              <div className="bg-gray-50 rounded p-2 text-sm flex items-center justify-center text-gray-500">
                +{trip.expenses.length - 3} more
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

// Add Trip Dialog Component  
function AddTripDialog({ 
  trucks,
  drivers,
  materials,
  materialVariants,
  onClose, 
  onSuccess 
}: { 
  trucks: Truck[];
  drivers: Driver[];
  materials: Material[];
  materialVariants: MaterialVariant[];
  onClose: () => void; 
  onSuccess: () => void; 
}) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    truck_id: '',
    driver_id: '',
    start_location: '',
    end_location: '',
    start_mileage: '',
    end_mileage: '',
    material_id: '',
    material_variant_id: '',
    material_cost: '',
    status: 'pending' as Trip['status']
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})
  const [filteredVariants, setFilteredVariants] = useState<MaterialVariant[]>([])

  useEffect(() => {
    if (formData.material_id) {
      const filtered = materialVariants.filter(v => v.material === formData.material_id)
      setFilteredVariants(filtered)
    } else {
      setFilteredVariants([])
    }
  }, [formData.material_id, materialVariants])

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    if (!formData.truck_id) newErrors.truck_id = 'Truck is required'
    if (!formData.driver_id) newErrors.driver_id = 'Driver is required'
    if (!formData.date) newErrors.date = 'Date is required'
    setClientErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    if (clientErrors[field]) {
      setClientErrors(prev => ({ ...prev, [field]: '' }))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setErrors([])
    setClientErrors({})
    
    if (!validateForm()) return
    
    setIsLoading(true)
    
    try {
      const data: CreateTripRequest = {
        truck_id: formData.truck_id,
        driver_id: formData.driver_id,
        status: formData.status
      }
      
      if (formData.start_location) data.start_location = formData.start_location
      if (formData.end_location) data.end_location = formData.end_location
      if (formData.start_mileage) data.start_mileage = parseFloat(formData.start_mileage)
      if (formData.end_mileage) data.end_mileage = parseFloat(formData.end_mileage)
      if (formData.material_id) data.material_id = formData.material_id
      if (formData.material_variant_id) data.material_variant_id = formData.material_variant_id
      if (formData.material_cost) data.material_cost = formData.material_cost
      
      await apiClient.createTrip(data)
      showSuccess('Trip created successfully!', 'The trip has been added to the system.')
      onSuccess()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      showError('Failed to create trip', 'Please check the form for errors.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Create New Trip</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">×</button>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="Truck" required errors={clientErrors.truck_id ? [clientErrors.truck_id] : []}>
              <Select value={formData.truck_id} onChange={(e) => handleInputChange('truck_id', e.target.value)}>
                <option value="">Select a truck...</option>
                {trucks.map((truck) => (
                  <option key={truck.id} value={truck.id}>{truck.license_plate} - {truck.make} {truck.model}</option>
                ))}
              </Select>
            </FormField>
            
            <FormField label="Driver" required errors={clientErrors.driver_id ? [clientErrors.driver_id] : []}>
              <Select value={formData.driver_id} onChange={(e) => handleInputChange('driver_id', e.target.value)}>
                <option value="">Select a driver...</option>
                {drivers.map((driver) => (
                  <option key={driver.id} value={driver.id}>{driver.name}</option>
                ))}
              </Select>
            </FormField>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            
            <FormField label="Status">
              <Select value={formData.status} onChange={(e) => handleInputChange('status', e.target.value)}>
                <option value="pending">Pending</option>
                <option value="in_progress">In Progress</option>
                <option value="completed">Completed</option>
                <option value="cancelled">Cancelled</option>
              </Select>
            </FormField>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="Start Location">
              <Input type="text" value={formData.start_location} onChange={(e) => handleInputChange('start_location', e.target.value)} placeholder="e.g., Nairobi Depot" />
            </FormField>
            
            <FormField label="End Location">
              <Input type="text" value={formData.end_location} onChange={(e) => handleInputChange('end_location', e.target.value)} placeholder="e.g., Mombasa Port" />
            </FormField>
          </div>
          
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="Material (Optional)">
              <Select value={formData.material_id} onChange={(e) => handleInputChange('material_id', e.target.value)}>
                <option value="">No material...</option>
                {materials.map((material) => (
                  <option key={material.id} value={material.id}>{material.name}</option>
                ))}
              </Select>
            </FormField>
            
            <FormField label="Material Variant (Optional)">
              <Select value={formData.material_variant_id} onChange={(e) => handleInputChange('material_variant_id', e.target.value)} disabled={!formData.material_id}>
                <option value="">No variant...</option>
                {filteredVariants.map((variant) => (
                  <option key={variant.id} value={variant.id}>{variant.name}</option>
                ))}
              </Select>
            </FormField>
          </div>
          
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">Cancel</button>
            <button type="submit" disabled={isLoading} className="btn btn-warning">
              {isLoading ? 'Creating...' : 'Create Trip'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

// Trip Details Modal Component
function TripDetailsModal({
  trip,
  onClose
}: {
  trip: Trip | null;
  onClose: () => void;
}) {
  const [activeTab, setActiveTab] = useState<'expenses' | 'mileage' | 'material'>('expenses')
  const [expandedImage, setExpandedImage] = useState<{ url: string; title: string } | null>(null)
  
  if (!trip) return null

  // Calculate financial summary
  const totalExpenses = trip.expenses?.reduce((sum, expense) => sum + parseFloat(expense.amount || '0'), 0) || 0
  const revenue = parseFloat(trip.material_cost || '0')
  const netProfit = revenue - totalExpenses
  const profitMargin = revenue > 0 ? (netProfit / revenue) * 100 : 0

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full max-h-[90vh] flex flex-col">
        
        {/* Header */}
        <div className="flex items-center justify-between p-6 border-b border-gray-200 flex-shrink-0">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">Trip Details</h2>
            <p className="text-gray-600">
              {trip.start_location || 'Start'} → {trip.end_location || 'Destination'}
            </p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 text-2xl font-bold"
          >
            ×
          </button>
        </div>

        {/* Content */}
        <div className="flex-1 overflow-y-auto p-6">
          
          {/* Trip Information */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Trip Information</h3>
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-gray-600">Trip ID:</span>
                  <span className="font-mono text-sm">#{trip.id.slice(0, 8)}...</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Status:</span>
                  <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                    trip.status === 'completed' ? 'bg-green-100 text-green-800' :
                    trip.status === 'in_progress' ? 'bg-slate-100 text-slate-800' :
                    trip.status === 'pending' ? 'bg-yellow-100 text-yellow-800' :
                    'bg-gray-100 text-gray-800'
                  }`}>
                    {trip.status.replace('_', ' ')}
                  </span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Date:</span>
                  <span>{trip.date ? new Date(trip.date).toLocaleDateString() : 'Not set'}</span>
                </div>
                {trip.material?.name && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Material:</span>
                    <span>{trip.material.name}</span>
                  </div>
                )}
                <div className="flex justify-between">
                  <span className="text-gray-600">Truck:</span>
                  <span>{trip.truck?.license_plate || 'Not assigned'}</span>
                </div>
                {trip.driver?.user.first_name && (
                  <div className="flex justify-between">
                    <span className="text-gray-600">Driver:</span>
                    <span>{trip.driver.user.first_name} {trip.driver.user.last_name}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Financial Summary */}
            <div className="space-y-4">
              <h3 className="text-lg font-semibold text-gray-900">Financial Summary</h3>
              <div className="space-y-2">
                <div className="flex justify-between">
                  <span className="text-gray-600">Revenue:</span>
                  <span className="font-semibold text-green-600">KSh {revenue.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-600">Total Expenses:</span>
                  <span className="font-semibold text-red-600">KSh {totalExpenses.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span>
                </div>
                <hr className="my-2" />
                <div className="flex justify-between text-lg">
                  <span className="font-semibold text-gray-900">Net Profit:</span>
                  <span className={`font-bold ${netProfit >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                    KSh {netProfit.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                  </span>
                </div>
                {revenue > 0 && (
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600">Profit Margin:</span>
                    <span className={`font-medium ${profitMargin >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                      {profitMargin.toFixed(1)}%
                    </span>
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Tabs */}
          <div className="space-y-4">
            {/* Tab Navigation */}
            <div className="border-b border-gray-200">
              <nav className="flex space-x-8">
                <button
                  onClick={() => setActiveTab('expenses')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'expenses'
                      ? 'border-slate-500 text-slate-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  💰 Expenses ({trip.expenses?.length || 0})
                </button>
                <button
                  onClick={() => setActiveTab('mileage')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'mileage'
                      ? 'border-slate-500 text-slate-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  📏 Mileage Photos
                </button>
                <button
                  onClick={() => setActiveTab('material')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'material'
                      ? 'border-slate-500 text-slate-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  📦 Material Photos
                </button>
              </nav>
            </div>

            {/* Tab Content */}
            <div className="py-4">
              {/* Expenses Tab */}
              {activeTab === 'expenses' && (
                <div>
                  {!trip.expenses || trip.expenses.length === 0 ? (
                    <div className="text-center py-8 text-gray-500">
                      <div className="text-4xl mb-2">💰</div>
                      <p>No expenses recorded for this trip</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      {trip.expenses.map((expense) => (
                        <div key={expense.id} className="border border-gray-200 rounded-lg p-4">
                          <div className="flex items-center justify-between">
                            <div className="flex-1">
                              <h4 className="font-semibold text-gray-900">{expense.description}</h4>
                              <p className="text-sm text-gray-600">
                                {expense.created_at ? new Date(expense.created_at).toLocaleDateString() : 'Date not available'}
                              </p>
                            </div>
                            <div className="text-right">
                              <p className="text-lg font-bold text-gray-900">KSh {expense.amount}</p>
                              {expense.receipt_photo_url && (
                                <p className="text-xs text-green-600">📷 Receipt attached</p>
                              )}
                            </div>
                          </div>
                          {expense.receipt_photo_url && (
                            <div className="mt-3">
                              <img 
                                src={expense.receipt_photo_url} 
                                alt="Receipt" 
                                className="w-24 h-24 object-cover rounded-md border cursor-pointer hover:opacity-80 transition-opacity"
                                onClick={() => setExpandedImage({ url: expense.receipt_photo_url!, title: `Receipt - ${expense.description}` })}
                              />
                            </div>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {/* Mileage Photos Tab */}
              {activeTab === 'mileage' && (
                <div>
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {/* Start Mileage Photo */}
                    <div className="space-y-3">
                      <h4 className="font-semibold text-gray-900">Start Mileage Photo</h4>
                      {trip.proof_image_url ? (
                        <div className="space-y-2">
                          <img 
                            src={trip.proof_image_url} 
                            alt="Start Mileage" 
                            className="w-full max-w-sm h-48 object-cover rounded-lg border cursor-pointer hover:opacity-80 transition-opacity"
                            onClick={() => setExpandedImage({ url: trip.proof_image_url!, title: 'Start Mileage Photo' })}
                          />
                          <p className="text-sm text-gray-600">Mileage: {trip.start_mileage || 'Not recorded'}</p>
                        </div>
                      ) : (
                        <div className="text-center py-8 text-gray-500 border-2 border-dashed border-gray-300 rounded-lg">
                          <div className="text-2xl mb-2">📷</div>
                          <p>No start mileage photo</p>
                        </div>
                      )}
                    </div>

                    {/* End Mileage Photo */}
                    <div className="space-y-3">
                      <h4 className="font-semibold text-gray-900">End Mileage Photo</h4>
                      {trip.proof_end_image_url ? (
                        <div className="space-y-2">
                          <img 
                            src={trip.proof_end_image_url} 
                            alt="End Mileage" 
                            className="w-full max-w-sm h-48 object-cover rounded-lg border cursor-pointer hover:opacity-80 transition-opacity"
                            onClick={() => setExpandedImage({ url: trip.proof_end_image_url!, title: 'End Mileage Photo' })}
                          />
                          <p className="text-sm text-gray-600">Mileage: {trip.end_mileage || 'Not recorded'}</p>
                        </div>
                      ) : (
                        <div className="text-center py-8 text-gray-500 border-2 border-dashed border-gray-300 rounded-lg">
                          <div className="text-2xl mb-2">📷</div>
                          <p>No end mileage photo</p>
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {/* Material Photos Tab */}
              {activeTab === 'material' && (
                <div>
                  <h4 className="font-semibold text-gray-900 mb-4">Material Loading/Unloading Photos</h4>
                  {trip.material_loading_photos && Array.isArray(trip.material_loading_photos) && trip.material_loading_photos.length > 0 ? (
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                      {trip.material_loading_photos.map((photoUrl, index) => (
                        <div key={index} className="space-y-2">
                          <img 
                            src={photoUrl} 
                            alt={`Material Photo ${index + 1}`} 
                            className="w-full h-32 object-cover rounded-lg border cursor-pointer hover:opacity-80 transition-opacity"
                            onClick={() => setExpandedImage({ url: photoUrl, title: `Material Photo ${index + 1}` })}
                          />
                          <p className="text-xs text-gray-600 text-center">Photo {index + 1}</p>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <div className="text-center py-8 text-gray-500 border-2 border-dashed border-gray-300 rounded-lg">
                      <div className="text-2xl mb-2">📦</div>
                      <p>No material photos</p>
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Footer */}
        <div className="flex justify-end space-x-3 p-6 pt-4 border-t border-gray-200 flex-shrink-0">
          <button onClick={onClose} className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">
            Close
          </button>
        </div>

      </div>

      {/* Expandable Image Modal */}
      {expandedImage && (
        <div 
          className="fixed inset-0 bg-black bg-opacity-90 flex items-center justify-center z-[60]" 
          onClick={() => setExpandedImage(null)}
        >
          <div className="relative max-w-[90vw] max-h-[90vh] flex flex-col">
            {/* Image Header */}
            <div className="bg-black bg-opacity-50 text-white p-3 flex items-center justify-between">
              <h3 className="font-semibold text-lg">{expandedImage.title}</h3>
              <button 
                onClick={() => setExpandedImage(null)}
                className="text-white hover:text-gray-300 text-xl font-bold"
              >
                ×
              </button>
            </div>
            
            {/* Image Display */}
            <div className="flex-1 flex items-center justify-center bg-black">
              <img 
                src={expandedImage.url} 
                alt={expandedImage.title}
                className="max-w-full max-h-full object-contain"
                onClick={(e) => e.stopPropagation()}
              />
            </div>
            
            {/* Image Footer */}
            <div className="bg-black bg-opacity-50 text-white p-3 text-center">
              <p className="text-sm">Click outside the image or press × to close</p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}