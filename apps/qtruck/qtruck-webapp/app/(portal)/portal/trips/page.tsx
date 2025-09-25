'use client'

import { useState, useEffect } from 'react'
import { apiClient } from '@/lib/api/client'
import { useToast } from '@/components/ui/Toast'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { PhotoUpload } from '@/components/ui/PhotoUpload'
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
  validateRequired,
  ValidationError
} from '@/lib/validation'
import { Trip, TripListParams, CreateTripRequest, Truck, Material, MaterialVariant, Expense, CreateExpenseRequest } from '@/lib/types/api'

export default function TripsPage() {
  const [trips, setTrips] = useState<Trip[]>([])
  const [trucks, setTrucks] = useState<Truck[]>([])
  const [materials, setMaterials] = useState<Material[]>([])
  const [materialVariants, setMaterialVariants] = useState<MaterialVariant[]>([])
  const [loading, setLoading] = useState(true)
  const [loadingMore, setLoadingMore] = useState(false)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const [selectedTrip, setSelectedTrip] = useState<Trip | null>(null)
  const [showActionDialog, setShowActionDialog] = useState(false)
  const [actionType, setActionType] = useState<'start' | 'complete'>('start')
  const [currentPage, setCurrentPage] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [hasNextPage, setHasNextPage] = useState(false)
  const [showExpenseForm, setShowExpenseForm] = useState(false)
  const [showTripDetails, setShowTripDetails] = useState(false)
  const { showSuccess, showError } = useToast()

  useEffect(() => {
    fetchData(true) // Always reset on initial load
  }, [])

  const fetchData = async (reset = false, page = 1) => {
    try {
      if (reset) {
        setLoading(true)
        setCurrentPage(1)
        setTrips([])
      } else {
        setLoadingMore(true)
      }

      // Get trips for current driver (API will filter by driver automatically)
      const params: TripListParams = {
        page,
        page_size: 20,
        ordering: '-date' // Most recent first
      }

      const [tripsResponse, trucksResponse, materialsResponse] = await Promise.all([
        apiClient.getTrips(params),
        apiClient.getTrucks(),
        apiClient.getMaterials()
      ])
      
      if (reset) {
        setTrips(tripsResponse.results || [])
      } else {
        setTrips(prev => [...prev, ...(tripsResponse.results || [])])
      }
      
      setTotalCount(tripsResponse.count || 0)
      setHasNextPage(!!tripsResponse.next)
      setCurrentPage(page)
      
      setTrucks(trucksResponse.results || [])
      setMaterials(materialsResponse.results || [])
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
      fetchData(false, currentPage + 1)
    }
  }

  const handleTripAction = (trip: Trip, action: 'start' | 'complete') => {
    setSelectedTrip(trip)
    setActionType(action)
    setShowActionDialog(true)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">My Trips</h1>
          <p className="text-gray-600">
            Manage your transport trips and material deliveries
            {totalCount > 0 && (
              <span className="text-amber-600 ml-2">
                ({totalCount} total, {trips.length} loaded)
              </span>
            )}
          </p>
        </div>
        <div className="flex items-center space-x-4">
          <RefreshButton onRefresh={() => fetchData(true)} loading={loading} theme="driver" />
          <button 
            onClick={() => setShowAddDialog(true)}
            className="btn btn-primary"
          >
            <span className="text-lg mr-2">🚛</span>
            Start New Trip
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">🗺️</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">{trips.length}</p>
              <p className="text-sm text-gray-600">Total Trips</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">⏳</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {trips.filter(t => t.status === 'pending' || t.status === 'in_progress').length}
              </p>
              <p className="text-sm text-gray-600">Active Trips</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">✅</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {trips.filter(t => t.status === 'completed').length}
              </p>
              <p className="text-sm text-gray-600">Completed</p>
            </div>
          </div>
        </div>
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
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No trips yet</h3>
          <p className="text-gray-600 mb-4">Create your first trip to get started.</p>
          <button 
            onClick={() => setShowAddDialog(true)}
            className="btn btn-primary"
          >
            Create First Trip
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {trips.map((trip) => (
            <DriverTripCard 
              key={trip.id} 
              trip={trip} 
              onAction={handleTripAction}
              onRefresh={() => fetchData(true)}
              theme="driver"
              onOpenExpenseForm={(trip) => {
                setSelectedTrip(trip)
                setShowExpenseForm(true)
              }}
              onViewDetails={(trip) => {
                setSelectedTrip(trip)
                setShowTripDetails(true)
              }}
            />
          ))}
          
          {/* Load More Button */}
          {hasNextPage && (
            <div className="flex justify-center py-6">
              <button
                onClick={loadMoreTrips}
                disabled={loadingMore}
                className="btn btn-outline"
              >
                {loadingMore ? (
                  <>
                    <div className="animate-spin rounded-full h-4 w-4 border-2 border-amber-600 border-t-transparent mr-2"></div>
                    Loading more...
                  </>
                ) : (
                  <>Load More Trips ({totalCount - trips.length} remaining)</>
                )}
              </button>
            </div>
          )}
        </div>
      )}

      {/* Add Trip Dialog */}
      {showAddDialog && (
        <AddTripDialog 
          trucks={trucks}
          materials={materials}
          materialVariants={materialVariants}
          onClose={() => setShowAddDialog(false)} 
          onSuccess={() => {
            fetchData(true)
            setShowAddDialog(false)
          }} 
        />
      )}

      {/* Trip Action Dialog (Start/Complete) */}
      {showActionDialog && selectedTrip && (
        <TripActionDialog 
          trip={selectedTrip}
          actionType={actionType}
          onClose={() => {
            setShowActionDialog(false)
            setSelectedTrip(null)
          }}
          onSuccess={() => {
            fetchData(true)
            setShowActionDialog(false)
            setSelectedTrip(null)
          }} 
        />
      )}

      {/* Add Expense Dialog */}
      {showExpenseForm && selectedTrip && (
        <AddExpenseDialog 
          trip={selectedTrip}
          onClose={() => {
            setShowExpenseForm(false)
            setSelectedTrip(null)
          }}
          onSuccess={() => {
            fetchData(true)
            setShowExpenseForm(false)
            setSelectedTrip(null)
          }} 
        />
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

// Driver Trip Card Component
function DriverTripCard({ 
  trip, 
  onAction, 
  onRefresh,
  onOpenExpenseForm,
  onViewDetails
}: { 
  trip: Trip; 
  onAction: (trip: Trip, action: 'start' | 'complete') => void;
  onRefresh: () => void;
  onOpenExpenseForm: (trip: Trip) => void;
  onViewDetails?: (trip: Trip) => void;
}) {
  const { showSuccess, showError } = useToast()
  
  const getStatusColor = (status: Trip['status']) => {
    switch (status) {
      case 'completed': return 'bg-green-100 text-green-800'
      case 'in_progress': return 'bg-amber-100 text-amber-800'
      case 'pending': return 'bg-yellow-100 text-yellow-800'
      case 'cancelled': return 'bg-red-100 text-red-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const getStatusIcon = (status: Trip['status']) => {
    switch (status) {
      case 'completed': return '✅'
      case 'in_progress': return '🚛'
      case 'pending': return '⏳'
      case 'cancelled': return '❌'
      default: return '❓'
    }
  }

  const canStart = trip.status === 'pending'
  const canComplete = trip.status === 'in_progress'

  return (
    <div className="card card-hover">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <div className="w-12 h-12 bg-amber-100 rounded-lg flex items-center justify-center">
            <span className="text-xl">{getStatusIcon(trip.status)}</span>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-gray-900">
              {trip.start_location || 'Start'} → {trip.end_location || 'Destination'}
            </h3>
            <p className="text-gray-600">Trip #{trip.id.slice(0, 8)}...</p>
            <div className="flex items-center space-x-4 mt-1">
              <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(trip.status)}`}>
                {trip.status.replace('_', ' ')}
              </span>
              <span className="text-sm text-gray-500">
                Truck: {trip.truck?.license_plate || 'Not assigned'}
              </span>
              <span className="text-sm text-gray-500">
                {trip.date ? new Date(trip.date).toLocaleDateString() : 'No date'}
              </span>
            </div>
            {trip.material && (
              <div className="flex items-center space-x-2 mt-1">
                <span className="text-sm text-amber-600">
                  🏗️ {trip.material.name}
                </span>
                {trip.material_variant && (
                  <span className="text-sm text-amber-500">
                    • {trip.material_variant.name}
                  </span>
                )}
              </div>
            )}
          </div>
        </div>
        
        <div className="flex items-center space-x-6">
          <div className="text-right text-sm text-gray-500">
            {trip.start_mileage && <p>Start: {trip.start_mileage} mi</p>}
            {trip.end_mileage && <p>End: {trip.end_mileage} mi</p>}
            {trip.total_mileage && <p>Total: {trip.total_mileage} mi</p>}
          </div>
          <div className="text-right">
            {trip.material_cost && parseFloat(trip.material_cost) > 0 && (
              <>
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
              </>
            )}
          </div>
          <div className="flex flex-col space-y-2">
            {canStart && (
              <button
                onClick={(e) => {
                  e.stopPropagation()
                  onAction(trip, 'start')
                }}
                className="btn btn-sm btn-primary"
              >
                🚀 Start Trip
              </button>
            )}
            {canComplete && (
              <button
                onClick={(e) => {
                  e.stopPropagation()
                  onAction(trip, 'complete')
                }}
                className="bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white font-medium py-2 px-3 rounded-lg transition-all duration-200 text-sm"
              >
                ✅ Complete Trip
              </button>
            )}
            {(trip.status === 'completed' || trip.status === 'cancelled') && (
              <span className="text-sm text-gray-500 px-3 py-1 text-center">
                Trip {trip.status}
              </span>
            )}
            
            {/* View Details Button - Always show */}
            <button
              onClick={(e) => {
                e.stopPropagation()
                onViewDetails?.(trip)
              }}
              className="bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white font-medium py-2 px-3 rounded-lg transition-all duration-200 text-sm"
            >
              👁️ View Details
            </button>
          </div>
        </div>
      </div>
      
      {/* Trip Expenses Preview */}
      {trip.expenses && trip.expenses.length > 0 && (
        <div className="border-t pt-3 mt-3">
          <h4 className="text-sm font-medium text-gray-700 mb-2">
            Expenses ({trip.expenses.length})
          </h4>
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
            {trip.expenses.slice(0, 3).map((expense) => (
              <div key={expense.id} className="bg-gray-50 rounded p-2 text-sm">
                <div className="font-medium truncate">{expense.description}</div>
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
      
      {/* Add Expense Button - Show for in_progress trips */}
      {trip.status === 'in_progress' && (
        <div className="mt-4 pt-4 border-t border-gray-200">
          <button
            onClick={(e) => {
              e.stopPropagation()
              onOpenExpenseForm(trip)
            }}
            className="w-full sm:w-auto bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white px-4 py-2 rounded-lg font-medium text-sm transition-all duration-200"
          >
            + Add Expense
          </button>
        </div>
      )}
    </div>
  )
}

// Add Trip Dialog Component for Drivers
function AddTripDialog({ 
  trucks, 
  materials, 
  materialVariants, 
  onClose, 
  onSuccess 
}: { 
  trucks: Truck[]; 
  materials: Material[]; 
  materialVariants: MaterialVariant[]; 
  onClose: () => void; 
  onSuccess: () => void 
}) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    truck_id: '',
    start_location: '',
    end_location: '',
    // start_mileage removed - set when starting trip
    material_id: '',
    material_variant_id: '',
    material_cost: '',
    status: 'pending' as Trip['status']
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})
  const [filteredVariants, setFilteredVariants] = useState<MaterialVariant[]>([])
  const [materialSearch, setMaterialSearch] = useState('')
  const [filteredMaterials, setFilteredMaterials] = useState<Material[]>([])
  const [showMaterialDropdown, setShowMaterialDropdown] = useState(false)
  const [loadingMaterials, setLoadingMaterials] = useState(false)
  const [currentLocation, setCurrentLocation] = useState<{lat: number, lng: number} | null>(null)
  const [gettingLocation, setGettingLocation] = useState(false)

  // Filter variants when material changes
  useEffect(() => {
    if (formData.material_id) {
      const filtered = materialVariants.filter(v => v.material === formData.material_id)
      setFilteredVariants(filtered)
      if (formData.material_variant_id && !filtered.some(v => v.id === formData.material_variant_id)) {
        setFormData(prev => ({ ...prev, material_variant_id: '' }))
      }
    } else {
      setFilteredVariants([])
      setFormData(prev => ({ ...prev, material_variant_id: '' }))
    }
  }, [formData.material_id, materialVariants])

  // Debounced search materials
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      searchMaterials(materialSearch)
    }, 300) // 300ms debounce

    return () => clearTimeout(timeoutId)
  }, [materialSearch])

  // Search materials dynamically
  const searchMaterials = async (searchTerm: string) => {
    if (!searchTerm.trim()) {
      setFilteredMaterials([])
      setShowMaterialDropdown(false)
      return
    }
    
    setLoadingMaterials(true)
    try {
      const response = await apiClient.getMaterials({ search: searchTerm, page_size: 10 })
      setFilteredMaterials(response.results || [])
      setShowMaterialDropdown(true)
    } catch (error) {
      console.error('Failed to search materials:', error)
      setFilteredMaterials([])
    } finally {
      setLoadingMaterials(false)
    }
  }

  // Handle material search input
  const handleMaterialSearch = (value: string) => {
    setMaterialSearch(value)
    if (!value.trim()) {
      setFormData(prev => ({ ...prev, material_id: '' }))
      setFilteredVariants([])
    }
  }

  // Clear material selection
  const clearMaterialSelection = () => {
    setMaterialSearch('')
    setFormData(prev => ({ ...prev, material_id: '', material_variant_id: '' }))
    setFilteredMaterials([])
    setFilteredVariants([])
    setShowMaterialDropdown(false)
  }


  // Get current location
  const getCurrentLocation = () => {
    if (!navigator.geolocation) {
      showError('Location Error', 'Geolocation is not supported by this browser')
      return
    }
    
    // Check if we're in a secure context for geolocation
    const isSecure = window.isSecureContext || window.location.protocol === 'https:'
    if (!isSecure) {
      console.log('Geolocation requires HTTPS, skipping location capture')
      // Don't show error for non-HTTPS, just skip location capture silently
      return
    }
    
    setGettingLocation(true)
    navigator.geolocation.getCurrentPosition(
      (position) => {
        setCurrentLocation({
          lat: position.coords.latitude,
          lng: position.coords.longitude
        })
        setGettingLocation(false)
        showSuccess('Location captured', 'Current location has been recorded for trip verification')
      },
      (error) => {
        setGettingLocation(false)
        console.error('Error getting location:', error)
        let errorMessage = 'Unable to get your current location'
        
        switch(error.code) {
          case error.PERMISSION_DENIED:
            if (error.message && error.message.includes('secure origins')) {
              // This is the HTTPS error, handle it gracefully
              console.log('Geolocation blocked due to insecure origin, continuing without location')
              return // Don't show error to user
            }
            errorMessage = 'Location access denied. Please allow location permissions and try again.'
            break
          case error.POSITION_UNAVAILABLE:
            errorMessage = 'Location information is unavailable.'
            break
          case error.TIMEOUT:
            errorMessage = 'Location request timed out.'
            break
        }
        
        showError('Location Error', errorMessage)
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 60000
      }
    )
  }

  // Auto-get location when dialog opens
  useEffect(() => {
    getCurrentLocation()
  }, [])

  // Handle material selection
  const handleMaterialSelect = (material: Material) => {
    setFormData(prev => ({ ...prev, material_id: material.id }))
    setMaterialSearch(material.name)
    setShowMaterialDropdown(false)
    // Load variants for selected material
    if (material.variants) {
      setFilteredVariants(material.variants)
    }
  }

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    if (!formData.truck_id) newErrors.truck_id = 'Truck is required'
    if (!formData.start_location) newErrors.start_location = 'Start location is required'
    if (!formData.end_location) newErrors.end_location = 'End location is required'
    
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    setErrors([])
    setClientErrors({})
    
    if (!validateForm()) {
      return
    }
    
    setIsLoading(true)
    
    try {
      const data: CreateTripRequest = {
        truck_id: formData.truck_id,
        driver_id: '', // Will be set automatically by backend for current driver
        start_location: formData.start_location,
        end_location: formData.end_location,
        status: formData.status
      }
      
      if (formData.material_id) data.material_id = formData.material_id
      if (formData.material_variant_id) data.material_variant_id = formData.material_variant_id
      if (formData.material_cost) data.material_cost = formData.material_cost
      
      // Add location coordinates if captured
      if (currentLocation) {
        data.current_location_coords = [currentLocation.lng, currentLocation.lat] // GeoJSON format [lng, lat]
      }
      
      // Create trip first
      await apiClient.createTrip(data)
      
      showSuccess('Trip created successfully!', 'Your trip has been created and is ready to start.')
      
      onSuccess()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Failed to create trip', generalErrors[0])
      } else {
        showError('Failed to create trip', 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg w-full max-w-lg max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between p-6 pb-4 border-b border-gray-200 flex-shrink-0">
          <h3 className="text-lg font-semibold">Create New Trip</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <span className="text-xl">×</span>
          </button>
        </div>
        
        <div className="flex-1 overflow-y-auto px-6 py-4">
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
            label="Truck" 
            required
            errors={[...getErrorsForField(errors, 'truck_id'), ...(clientErrors.truck_id ? [clientErrors.truck_id] : [])]}
          >
            <Select
              value={formData.truck_id}
              onChange={(e) => handleInputChange('truck_id', e.target.value)}
              errors={[...getErrorsForField(errors, 'truck_id'), ...(clientErrors.truck_id ? [clientErrors.truck_id] : [])]}
            >
              <option value="">Select a truck</option>
              {trucks.map((truck) => (
                <option key={truck.id} value={truck.id}>
                  {truck.license_plate} - {truck.model}
                </option>
              ))}
            </Select>
          </FormField>
          
          <FormField 
            label="Start Location" 
            required
            errors={[...getErrorsForField(errors, 'start_location'), ...(clientErrors.start_location ? [clientErrors.start_location] : [])]}
          >
            <Input
              type="text"
              value={formData.start_location}
              onChange={(e) => handleInputChange('start_location', e.target.value)}
              placeholder="Warehouse A, City"
              errors={[...getErrorsForField(errors, 'start_location'), ...(clientErrors.start_location ? [clientErrors.start_location] : [])]}
            />
          </FormField>
          
          <FormField 
            label="End Location" 
            required
            errors={[...getErrorsForField(errors, 'end_location'), ...(clientErrors.end_location ? [clientErrors.end_location] : [])]}
          >
            <Input
              type="text"
              value={formData.end_location}
              onChange={(e) => handleInputChange('end_location', e.target.value)}
              placeholder="Construction Site B, City"
              errors={[...getErrorsForField(errors, 'end_location'), ...(clientErrors.end_location ? [clientErrors.end_location] : [])]}
            />
          </FormField>
          
          
          <FormField 
            label="Material" 
            errors={getErrorsForField(errors, 'material_id')}
          >
            <div className="relative">
              <Input
                type="text"
                value={materialSearch}
                onChange={(e) => handleMaterialSearch(e.target.value)}
                placeholder="Type to search materials..."
                errors={getErrorsForField(errors, 'material_id')}
              />
              {/* Clear button */}
              {materialSearch && (
                <button
                  type="button"
                  onClick={clearMaterialSelection}
                  className="absolute right-8 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              )}
              {/* Loading spinner */}
              {loadingMaterials && (
                <div className="absolute right-3 top-1/2 transform -translate-y-1/2">
                  <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-amber-600"></div>
                </div>
              )}
              {/* Search results dropdown */}
              {showMaterialDropdown && filteredMaterials.length > 0 && (
                <div className="absolute z-10 w-full bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-auto mt-1">
                  {filteredMaterials.map((material) => (
                    <button
                      key={material.id}
                      type="button"
                      onClick={() => handleMaterialSelect(material)}
                      className="w-full text-left px-3 py-2 hover:bg-amber-50 focus:bg-amber-50 focus:outline-none border-b last:border-b-0"
                    >
                      <div className="font-medium">{material.name}</div>
                      {material.description && (
                        <div className="text-sm text-gray-500 truncate">{material.description}</div>
                      )}
                      <div className="text-xs text-amber-600 mt-1">
                        {material.variants?.length || 0} variants available
                      </div>
                    </button>
                  ))}
                </div>
              )}
              {/* No results message */}
              {showMaterialDropdown && filteredMaterials.length === 0 && materialSearch.trim() && !loadingMaterials && (
                <div className="absolute z-10 w-full bg-white border border-gray-300 rounded-md shadow-lg mt-1 p-3 text-center text-gray-500">
                  No materials found for "{materialSearch}"
                </div>
              )}
            </div>
          </FormField>
          
          {formData.material_id && (
            <FormField 
              label="Material Variant" 
              errors={getErrorsForField(errors, 'material_variant_id')}
            >
              <Select
                value={formData.material_variant_id}
                onChange={(e) => handleInputChange('material_variant_id', e.target.value)}
                errors={getErrorsForField(errors, 'material_variant_id')}
              >
                <option value="">Select variant (optional)</option>
                {filteredVariants.map((variant) => (
                  <option key={variant.id} value={variant.id}>
                    {variant.name}
                  </option>
                ))}
              </Select>
            </FormField>
          )}
          
          {formData.material_id && (
            <FormField 
              label="Trip Revenue (KSh)" 
              errors={getErrorsForField(errors, 'material_cost')}
            >
              <Input
                type="number"
                step="0.01"
                value={formData.material_cost}
                onChange={(e) => handleInputChange('material_cost', e.target.value)}
                placeholder="15000.00"
                errors={getErrorsForField(errors, 'material_cost')}
              />
            </FormField>
          )}

          {/* Location Status */}
          <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-md">
            <svg className={`w-5 h-5 ${currentLocation ? 'text-amber-500' : 'text-gray-400'}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" />
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" />
            </svg>
            {gettingLocation ? (
              <span className="text-sm text-gray-600">Getting current location...</span>
            ) : currentLocation ? (
              <span className="text-sm text-amber-600">Location captured for trip verification</span>
            ) : (window.isSecureContext || window.location.protocol === 'https:') ? (
              <button
                type="button"
                onClick={getCurrentLocation}
                className="text-sm text-amber-600 hover:text-amber-800 underline"
              >
                Click to get current location
              </button>
            ) : (
              <span className="text-sm text-yellow-600">Location capture requires HTTPS (optional)</span>
            )}
          </div>

          
          </form>
        </div>
        
        {/* Fixed footer with buttons */}
        <div className="flex justify-end space-x-3 p-6 pt-4 border-t border-gray-200 flex-shrink-0">
          <button type="button" onClick={onClose} className="btn btn-outline">
            Cancel
          </button>
          <button 
            type="submit" 
            disabled={isLoading}
            className="btn btn-primary"
            onClick={(e) => {
              e.preventDefault()
              const form = e.currentTarget.closest('.bg-white')?.querySelector('form') as HTMLFormElement
              if (form) {
                form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }))
              }
            }}
          >
            {isLoading ? 'Creating...' : 'Create Trip'}
          </button>
        </div>

      </div>
    </div>
  )
}

// Trip Action Dialog Component (Start/Complete Trip)
function TripActionDialog({ 
  trip, 
  actionType, 
  onClose, 
  onSuccess 
}: { 
  trip: Trip;
  actionType: 'start' | 'complete';
  onClose: () => void; 
  onSuccess: () => void 
}) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    mileage: '',
    notes: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})
  const [mileagePhoto, setMileagePhoto] = useState<File | null>(null)
  const [mileagePhotoUrl, setMileagePhotoUrl] = useState<string | null>(null)
  const [materialPhotos, setMaterialPhotos] = useState<File[]>([])
  const [materialPhotoUrls, setMaterialPhotoUrls] = useState<string[]>([])

  const isStartAction = actionType === 'start'
  const title = isStartAction ? 'Start Trip' : 'Complete Trip'
  const mileageLabel = isStartAction ? 'Start Mileage' : 'End Mileage'
  const submitText = isStartAction ? 'Start Trip' : 'Complete Trip'


  const removeMileagePhoto = () => {
    if (mileagePhotoUrl) {
      URL.revokeObjectURL(mileagePhotoUrl)
    }
    setMileagePhoto(null)
    setMileagePhotoUrl(null)
  }

  const removeMaterialPhoto = (index: number) => {
    if (materialPhotoUrls[index]) {
      URL.revokeObjectURL(materialPhotoUrls[index])
    }
    setMaterialPhotos(prev => prev.filter((_, i) => i !== index))
    setMaterialPhotoUrls(prev => prev.filter((_, i) => i !== index))
  }


  const handleMileagePhotoSelect = (file: File) => {
    // Clean up previous photo
    if (mileagePhotoUrl) {
      URL.revokeObjectURL(mileagePhotoUrl)
    }
    const previewUrl = URL.createObjectURL(file)
    setMileagePhoto(file)
    setMileagePhotoUrl(previewUrl)
  }

  const handleMaterialPhotoSelect = (file: File) => {
    const previewUrl = URL.createObjectURL(file)
    setMaterialPhotos(prev => [...prev, file])
    setMaterialPhotoUrls(prev => [...prev, previewUrl])
  }


  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    if (!formData.mileage) {
      newErrors.mileage = `${mileageLabel} is required`
    } else if (parseFloat(formData.mileage) <= 0) {
      newErrors.mileage = 'Mileage must be greater than 0'
    }

    // Require mileage photo
    if (!mileagePhoto) {
      newErrors.mileage_photo = `${mileageLabel} photo is required`
    }

    // For material trips, require material photos
    if (trip.material && materialPhotos.length === 0) {
      const photoLabel = isStartAction ? 'Material loading photos' : 'Material unloading photos'
      newErrors.material_photos = `${photoLabel} are required for material transport`
    }
    
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

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    setErrors([])
    setClientErrors({})
    
    if (!validateForm()) {
      return
    }
    
    setIsLoading(true)
    
    try {
      const updateData: any = {
        status: isStartAction ? 'in_progress' : 'completed'
      }
      
      if (isStartAction) {
        updateData.start_mileage = parseFloat(formData.mileage)
      } else {
        updateData.end_mileage = parseFloat(formData.mileage)
        // Calculate total mileage if we have both start and end
        if (trip.start_mileage) {
          updateData.total_mileage = parseFloat(formData.mileage) - trip.start_mileage
        }
      }
      
      if (formData.notes.trim()) {
        updateData.notes = formData.notes.trim()
      }
      
      // Update trip first
      await apiClient.updateTrip(trip.id, updateData)
      
      // Upload photos
      const photoUploadPromises = []
      
      // Upload mileage photo
      if (mileagePhoto) {
        const photoType = isStartAction ? 'start_mileage' : 'end_mileage'
        photoUploadPromises.push(
          apiClient.uploadTripPhoto(trip.id, photoType, mileagePhoto)
        )
      }
      
      // Upload material photos (loading/unloading)
      if (materialPhotos.length > 0) {
        const materialPhotoType = isStartAction ? 'material_loading' : 'material_unloading'
        for (const photo of materialPhotos) {
          photoUploadPromises.push(
            apiClient.uploadTripPhoto(trip.id, materialPhotoType, photo)
          )
        }
      }
      
      // Wait for all photo uploads
      if (photoUploadPromises.length > 0) {
        try {
          await Promise.all(photoUploadPromises)
        } catch (uploadError) {
          console.error('Failed to upload some photos:', uploadError)
          // Continue with success message even if photos failed
        }
      }
      
      const successMessage = isStartAction 
        ? 'Trip started successfully! Safe travels.'
        : 'Trip completed successfully! Great job.'
      
      showSuccess(`Trip ${actionType}ed!`, successMessage)
      onSuccess()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError(`Failed to ${actionType} trip`, generalErrors[0])
      } else {
        showError(`Failed to ${actionType} trip`, 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg w-full max-w-md max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between p-6 pb-4 border-b border-gray-200 flex-shrink-0">
          <h3 className="text-lg font-semibold">{title}</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <span className="text-xl">×</span>
          </button>
        </div>
        
        <div className="flex-1 overflow-y-auto px-6 py-4">
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4 mb-4">
          <div className="flex items-center">
            <span className="text-2xl mr-3">{isStartAction ? '🚀' : '🏁'}</span>
            <div>
              <h4 className="font-medium text-amber-900">
                {trip.start_location} → {trip.end_location}
              </h4>
              <p className="text-sm text-amber-700">
                Trip #{trip.id.slice(0, 8)}... • Truck: {trip.truck?.license_plate}
              </p>
              {trip.material && (
                <p className="text-sm text-amber-600">
                  Material: {trip.material.name}
                  {trip.material_variant && ` • ${trip.material_variant.name}`}
                </p>
              )}
            </div>
          </div>
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
            label={mileageLabel}
            required
            errors={[...getErrorsForField(errors, 'mileage'), ...(clientErrors.mileage ? [clientErrors.mileage] : [])]}
          >
            <Input
              type="number"
              step="0.1"
              value={formData.mileage}
              onChange={(e) => handleInputChange('mileage', e.target.value)}
              placeholder={isStartAction ? "15000" : "15250"}
              errors={[...getErrorsForField(errors, 'mileage'), ...(clientErrors.mileage ? [clientErrors.mileage] : [])]}
            />
            {trip.start_mileage && !isStartAction && (
              <p className="text-sm text-gray-500 mt-1">
                Start mileage: {trip.start_mileage} miles
              </p>
            )}
          </FormField>

          {/* Mileage Photo */}
          <PhotoUpload
            label={`${mileageLabel} Photo`}
            onFileSelect={handleMileagePhotoSelect}
            onRemoveImage={() => removeMileagePhoto()}
            currentImage={mileagePhotoUrl || undefined}
            accept="image/*"
            required={true}
          />

          {/* Material Photos - Only show if trip has material */}
          {trip.material && (
            <PhotoUpload
              label={isStartAction ? 'Material Loading Photos' : 'Material Unloading Photos'}
              onFileSelect={handleMaterialPhotoSelect}
              onRemoveImage={removeMaterialPhoto}
              currentImages={materialPhotoUrls}
              accept="image/*"
              required={true}
              mode="multiple"
              maxImages={5}
            />
          )}
          
          <FormField label="Notes (Optional)">
            <Textarea
              value={formData.notes}
              onChange={(e) => handleInputChange('notes', e.target.value)}
              placeholder={`Add any notes about ${isStartAction ? 'starting' : 'completing'} this trip...`}
              rows={3}
            />
          </FormField>
          
        </form>
        </div>
        
        {/* Fixed footer with buttons */}
        <div className="flex justify-end space-x-3 p-6 pt-4 border-t border-gray-200 flex-shrink-0">
          <button type="button" onClick={onClose} className="btn btn-outline">
            Cancel
          </button>
          <button 
            type="submit" 
            disabled={isLoading} 
            className={`btn ${isStartAction ? 'btn-primary' : 'btn-success'}`}
            onClick={(e) => {
              e.preventDefault()
              const form = e.currentTarget.closest('.bg-white')?.querySelector('form') as HTMLFormElement
              if (form) {
                form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }))
              }
            }}
          >
            {isLoading ? `${isStartAction ? 'Starting' : 'Completing'}...` : submitText}
          </button>
        </div>

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
                    trip.status === 'in_progress' ? 'bg-amber-100 text-amber-800' :
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
                      ? 'border-amber-500 text-amber-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  💰 Expenses ({trip.expenses?.length || 0})
                </button>
                <button
                  onClick={() => setActiveTab('mileage')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'mileage'
                      ? 'border-amber-500 text-amber-600'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  📏 Mileage Photos
                </button>
                <button
                  onClick={() => setActiveTab('material')}
                  className={`py-2 px-1 border-b-2 font-medium text-sm ${
                    activeTab === 'material'
                      ? 'border-amber-500 text-amber-600'
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
                                <p className="text-xs text-amber-600">📷 Receipt attached</p>
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
          <button onClick={onClose} className="btn btn-outline">
            Close
          </button>
        </div>

      </div>

      {/* Image Viewer Modal */}
      {expandedImage && (
        <div className="fixed inset-0 bg-black bg-opacity-90 flex items-center justify-center z-[60]" onClick={() => setExpandedImage(null)}>
          <div className="relative max-w-[90vw] max-h-[90vh] flex flex-col">
            {/* Image Header */}
            <div className="flex items-center justify-between p-4 text-white">
              <h3 className="text-lg font-semibold">{expandedImage.title}</h3>
              <button
                onClick={() => setExpandedImage(null)}
                className="text-white hover:text-gray-300 text-2xl font-bold"
              >
                ×
              </button>
            </div>
            
            {/* Image */}
            <div className="flex-1 flex items-center justify-center p-4">
              <img 
                src={expandedImage.url}
                alt={expandedImage.title}
                className="max-w-full max-h-full object-contain rounded-lg"
                onClick={(e) => e.stopPropagation()}
              />
            </div>
            
            {/* Image Footer */}
            <div className="p-4 text-center">
              <p className="text-white text-sm">Click outside the image or press × to close</p>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// Add Expense Dialog Component
function AddExpenseDialog({ 
  trip, 
  onClose, 
  onSuccess 
}: { 
  trip: Trip;
  onClose: () => void; 
  onSuccess: () => void 
}) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    description: '',
    amount: '',
    receipt_photo: null as File | null
  })
  const [receiptPhotoPreview, setReceiptPhotoPreview] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    if (!formData.description.trim()) {
      newErrors.description = 'Description is required'
    }
    
    if (!formData.amount.trim()) {
      newErrors.amount = 'Amount is required'
    } else if (parseFloat(formData.amount) <= 0) {
      newErrors.amount = 'Amount must be greater than 0'
    }
    
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

  const handleReceiptPhotoSelect = (file: File) => {
    if (receiptPhotoPreview) {
      URL.revokeObjectURL(receiptPhotoPreview)
    }
    const previewUrl = URL.createObjectURL(file)
    setReceiptPhotoPreview(previewUrl)
    setFormData(prev => ({ ...prev, receipt_photo: file }))
  }

  const removePhoto = () => {
    if (receiptPhotoPreview) {
      URL.revokeObjectURL(receiptPhotoPreview)
    }
    setReceiptPhotoPreview(null)
    setFormData(prev => ({ ...prev, receipt_photo: null }))
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    setErrors([])
    setClientErrors({})

    if (!validateForm()) {
      showError('Validation Error', 'Please correct the errors in the form.')
      return
    }

    setIsLoading(true)
    try {
      const expenseData = new FormData()
      expenseData.append('trip_id', trip.id)
      expenseData.append('description', formData.description)
      expenseData.append('amount', formData.amount)
      
      if (formData.receipt_photo) {
        expenseData.append('receipt_photo_file', formData.receipt_photo)
      }

      await apiClient.createExpense(expenseData as any)
      
      showSuccess('Expense Added', 'The expense has been successfully added to the trip.')
      
      // Clean up photo URL
      if (receiptPhotoPreview) {
        URL.revokeObjectURL(receiptPhotoPreview)
      }
      
      onSuccess()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Failed to add expense', generalErrors[0])
      } else {
        showError('Failed to add expense', 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg w-full max-w-md max-h-[90vh] flex flex-col">
        <div className="flex items-center justify-between p-6 pb-4 border-b border-gray-200 flex-shrink-0">
          <div>
            <h3 className="text-lg font-semibold">Add Expense</h3>
            <p className="text-sm text-gray-600">Trip: {trip.start_location} → {trip.end_location}</p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <span className="text-xl">×</span>
          </button>
        </div>
        
        <div className="flex-1 overflow-y-auto px-6 py-4">
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
              label="Description" 
              required
              errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
            >
              <Input
                type="text"
                value={formData.description}
                onChange={(e) => handleInputChange('description', e.target.value)}
                placeholder="Fuel, tolls, meals, repairs, etc."
                errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
              />
            </FormField>

            <FormField 
              label="Amount (KSh)" 
              required
              errors={[...getErrorsForField(errors, 'amount'), ...(clientErrors.amount ? [clientErrors.amount] : [])]}
            >
              <Input
                type="number"
                step="0.01"
                value={formData.amount}
                onChange={(e) => handleInputChange('amount', e.target.value)}
                placeholder="150.00"
                errors={[...getErrorsForField(errors, 'amount'), ...(clientErrors.amount ? [clientErrors.amount] : [])]}
              />
            </FormField>

            <PhotoUpload
              label="Receipt Photo (Optional)"
              onFileSelect={handleReceiptPhotoSelect}
              currentImage={receiptPhotoPreview || undefined}
              accept="image/*"
              required={false}
            />
          </form>
        </div>
        
        {/* Fixed footer with buttons */}
        <div className="flex justify-end space-x-3 p-6 pt-4 border-t border-gray-200 flex-shrink-0">
          <button type="button" onClick={onClose} className="btn btn-outline">
            Cancel
          </button>
          <button 
            type="submit" 
            disabled={isLoading}
            className="btn btn-primary"
            onClick={(e) => {
              e.preventDefault()
              const form = e.currentTarget.closest('.bg-white')?.querySelector('form') as HTMLFormElement
              if (form) {
                form.dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }))
              }
            }}
          >
            {isLoading ? 'Adding...' : 'Add Expense'}
          </button>
        </div>

      </div>
    </div>
  )
}