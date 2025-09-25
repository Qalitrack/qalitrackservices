'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { apiClient, ApiError } from '@/lib/api/client'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { getBaseEmail } from '@/lib/utils'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input } from '@/components/ui/FormField'
import { 
  ValidationError, 
  getErrorsForField, 
  getGeneralErrors,
  validateEmail,
  validateRequired
} from '@/lib/validation'

interface DashboardData {
  drivers: any[]
  trips: any[]
  trucks: any[]
  vehicleMileage: any[]
  tripsByStatus: any
}

interface LoadingState {
  drivers: boolean
  trips: boolean
  trucks: boolean
  vehicleMileage: boolean
  tripsByStatus: boolean
}

export default function AdminDashboard() {
  const [data, setData] = useState<DashboardData>({
    drivers: [],
    trips: [],
    trucks: [],
    vehicleMileage: [],
    tripsByStatus: null
  })

  const [materials, setMaterials] = useState<any[]>([])
  const [pendingUsers, setPendingUsers] = useState<any[]>([])
  const [loadingMaterials, setLoadingMaterials] = useState(true)
  const [loadingPendingUsers, setLoadingPendingUsers] = useState(true)

  const [showAddVehicleDialog, setShowAddVehicleDialog] = useState(false)
  const [showAddMaterialDialog, setShowAddMaterialDialog] = useState(false)
  const [showAddAdminDialog, setShowAddAdminDialog] = useState(false)
  
  const [loading, setLoading] = useState<LoadingState>({
    drivers: true,
    trips: true,
    trucks: true,
    vehicleMileage: true,
    tripsByStatus: true
  })

  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    fetchDashboardData()
  }, [])

  const fetchDashboardData = async () => {
    // Set loading state for all components
    setLoading({
      drivers: true,
      trips: true,
      trucks: true,
      vehicleMileage: true,
      tripsByStatus: true
    })
    setLoadingMaterials(true)
    setLoadingPendingUsers(true)
    
    // Fetch all data in parallel
    const fetchPromises = [
      fetchDrivers(),
      fetchTrips(),
      fetchTrucks(),
      fetchVehicleMileage(),
      fetchTripsByStatus(),
      fetchMaterials(),
      fetchPendingUsers()
    ]

    await Promise.allSettled(fetchPromises)
  }

  const fetchDrivers = async () => {
    try {
      // Use getDriverProfiles since getDrivers doesn't exist
      const response = await apiService.getDriverProfiles()
      setData(prev => ({ ...prev, drivers: response.results || [] }))
    } catch (error) {
      setErrors(prev => ({ ...prev, drivers: error instanceof Error ? error.message : 'Failed to load drivers' }))
    } finally {
      setLoading(prev => ({ ...prev, drivers: false }))
    }
  }

  const fetchTrips = async () => {
    try {
      // getTrips method doesn't exist yet, return empty array for now
      setData(prev => ({ ...prev, trips: [] }))
    } catch (error) {
      setErrors(prev => ({ ...prev, trips: error instanceof Error ? error.message : 'Failed to load trips' }))
    } finally {
      setLoading(prev => ({ ...prev, trips: false }))
    }
  }

  const fetchTrucks = async () => {
    try {
      const response = await apiService.getTrucks()
      setData(prev => ({ ...prev, trucks: response.results || [] }))
    } catch (error) {
      setErrors(prev => ({ ...prev, trucks: error instanceof Error ? error.message : 'Failed to load trucks' }))
    } finally {
      setLoading(prev => ({ ...prev, trucks: false }))
    }
  }

  const fetchVehicleMileage = async () => {
    try {
      // getVehicleMileage method doesn't exist yet, return empty array for now
      setData(prev => ({ ...prev, vehicleMileage: [] }))
    } catch (error) {
      setErrors(prev => ({ ...prev, vehicleMileage: error instanceof Error ? error.message : 'Failed to load vehicle mileage' }))
    } finally {
      setLoading(prev => ({ ...prev, vehicleMileage: false }))
    }
  }

  const fetchTripsByStatus = async () => {
    try {
      // getTripsByStatus method doesn't exist yet, return null for now
      setData(prev => ({ ...prev, tripsByStatus: null }))
    } catch (error) {
      setErrors(prev => ({ ...prev, tripsByStatus: error instanceof Error ? error.message : 'Failed to load trip status' }))
    } finally {
      setLoading(prev => ({ ...prev, tripsByStatus: false }))
    }
  }

  const fetchMaterials = async () => {
    try {
      const response = await apiService.getMaterials()
      setMaterials(response.results || [])
    } catch (error) {
      console.error('Failed to load materials:', error)
    } finally {
      setLoadingMaterials(false)
    }
  }

  const fetchPendingUsers = async () => {
    try {
      // getPendingUsers method doesn't exist yet, return empty array for now
      setPendingUsers([])
    } catch (error) {
      console.error('Failed to load pending users:', error)
    } finally {
      setLoadingPendingUsers(false)
    }
  }

  const LoadingCard = ({ title }: { title: string }) => (
    <div className="card">
      <div className="animate-pulse">
        <div className="flex items-center justify-between">
          <div>
            <div className="h-4 bg-gray-200 rounded w-24 mb-2"></div>
            <div className="h-8 bg-gray-200 rounded w-16 mb-2"></div>
            <div className="h-3 bg-gray-200 rounded w-20"></div>
          </div>
          <div className="h-12 w-12 bg-gray-200 rounded"></div>
        </div>
      </div>
    </div>
  )

  const ErrorCard = ({ title, error }: { title: string; error: string }) => (
    <div className="card border-red-200 bg-red-50">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm font-medium text-red-600">{title}</p>
          <p className="text-xs text-red-500 mt-1">{error}</p>
        </div>
        <div className="text-2xl text-red-400">⚠️</div>
      </div>
    </div>
  )

  const getRecentTrips = () => {
    return data.trips.slice(0, 5).map(trip => ({
      id: trip.id || 'N/A',
      route: `${trip.origin || 'Unknown'} → ${trip.destination || 'Unknown'}`,
      driver: trip.driver_name || trip.driver || 'Unassigned',
      status: trip.status || 'unknown',
      time: trip.created_at ? new Date(trip.created_at).toLocaleString() : 'Unknown'
    }))
  }

  const getVehicleStatusCounts = () => {
    // Since the new Truck type doesn't have status, we'll just show total count
    return {
      total: data.trucks.length
    }
  }

  const isAnyLoading = Object.values(loading).some(Boolean)

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Dashboard</h1>
          <p className="text-gray-600">Overview of your fleet management system</p>
          {isAnyLoading && (
            <div className="mt-2 flex items-center text-slate-600">
              <div className="animate-spin rounded-full h-4 w-4 border-2 border-slate-600 border-t-transparent mr-2"></div>
              Loading dashboard data...
            </div>
          )}
        </div>
        <RefreshButton onRefresh={fetchDashboardData} loading={isAnyLoading} theme="admin" />
      </div>

      {/* Stats Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        {/* Total Vehicles */}
        {loading.trucks ? (
          <LoadingCard title="Total Vehicles" />
        ) : errors.trucks ? (
          <ErrorCard title="Total Vehicles" error={errors.trucks} />
        ) : (
          <div className="card card-hover">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">Total Trucks</p>
                <p className="text-2xl font-bold text-gray-900">{data.trucks.length}</p>
                <p className="text-sm text-slate-600">Trucks registered</p>
              </div>
              <div className="text-3xl text-slate-500">🚛</div>
            </div>
          </div>
        )}

        {/* Active Drivers */}
        {loading.drivers ? (
          <LoadingCard title="Active Drivers" />
        ) : errors.drivers ? (
          <ErrorCard title="Active Drivers" error={errors.drivers} />
        ) : (
          <div className="card card-hover">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">Total Drivers</p>
                <p className="text-2xl font-bold text-gray-900">{data.drivers.length}</p>
                <p className="text-sm text-green-600">Drivers registered</p>
              </div>
              <div className="text-3xl text-green-500">👥</div>
            </div>
          </div>
        )}

        {/* Total Trips */}
        {loading.trips ? (
          <LoadingCard title="Total Trips" />
        ) : errors.trips ? (
          <ErrorCard title="Total Trips" error={errors.trips} />
        ) : (
          <div className="card card-hover">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">Total Trips</p>
                <p className="text-2xl font-bold text-gray-900">{data.trips.length}</p>
                <p className="text-sm text-amber-600">All time</p>
              </div>
              <div className="text-3xl text-amber-500">🗺️</div>
            </div>
          </div>
        )}

        {/* Vehicle Mileage */}
        {loading.vehicleMileage ? (
          <LoadingCard title="Vehicle Mileage" />
        ) : errors.vehicleMileage ? (
          <ErrorCard title="Vehicle Mileage" error={errors.vehicleMileage} />
        ) : (
          <div className="card card-hover">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600">Mileage Records</p>
                <p className="text-2xl font-bold text-gray-900">{data.vehicleMileage.length}</p>
                <p className="text-sm text-purple-600">Tracked</p>
              </div>
              <div className="text-3xl text-purple-500">📊</div>
            </div>
          </div>
        )}
      </div>

      {/* Charts and Tables Row */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Recent Trips */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Recent Trips</h2>
            <button className="text-slate-600 hover:text-slate-700 text-sm font-medium">
              View all
            </button>
          </div>
          
          {loading.trips ? (
            <div className="space-y-3">
              {[1, 2, 3].map(i => (
                <div key={i} className="animate-pulse p-3 bg-gray-50 rounded-lg">
                  <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2 mb-1"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/3"></div>
                </div>
              ))}
            </div>
          ) : errors.trips ? (
            <div className="text-center py-8 text-gray-500">
              <p>Failed to load trips</p>
              <p className="text-sm">{errors.trips}</p>
            </div>
          ) : data.trips.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <p>No trips found</p>
            </div>
          ) : (
            <div className="space-y-3">
              {getRecentTrips().map((trip, index) => (
                <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <div className="font-medium text-gray-900">{trip.id}</div>
                    <div className="text-sm text-gray-600">{trip.route}</div>
                    <div className="text-sm text-gray-500">{trip.driver?.name || 'Unassigned'}</div>
                  </div>
                  <div className="text-right">
                    <div className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                      trip.status === 'completed' ? 'bg-green-100 text-green-800' :
                      trip.status === 'in-progress' ? 'bg-slate-100 text-slate-800' :
                      trip.status === 'scheduled' ? 'bg-yellow-100 text-yellow-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      {trip.status}
                    </div>
                    <div className="text-sm text-gray-500 mt-1">{trip.time}</div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Vehicle Status */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Fleet Overview</h2>
            <button className="text-slate-600 hover:text-slate-700 text-sm font-medium">
              Manage fleet
            </button>
          </div>
          
          {loading.trucks ? (
            <div className="space-y-4">
              <div className="animate-pulse flex items-center justify-between">
                <div className="flex items-center space-x-3">
                  <div className="w-3 h-3 bg-gray-200 rounded-full"></div>
                  <div className="h-4 bg-gray-200 rounded w-20"></div>
                </div>
                <div className="h-4 bg-gray-200 rounded w-16"></div>
              </div>
            </div>
          ) : errors.trucks ? (
            <div className="text-center py-8 text-gray-500">
              <p>Failed to load fleet data</p>
              <p className="text-sm">{errors.trucks}</p>
            </div>
          ) : (
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-3">
                <div className="w-3 h-3 rounded-full bg-slate-500"></div>
                <span className="text-gray-700">Total Trucks</span>
              </div>
              <span className="text-gray-900 font-medium">{getVehicleStatusCounts().total} trucks</span>
            </div>
          )}
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <button 
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
            onClick={() => setShowAddAdminDialog(true)}
          >
            <span className="text-lg mr-2">👑</span>
            Add Admin
          </button>
          <button 
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
            onClick={() => setShowAddVehicleDialog(true)}
          >
            <span className="text-lg mr-2">🚛</span>
            Add Truck
          </button>
          <button 
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
            onClick={() => setShowAddMaterialDialog(true)}
          >
            <span className="text-lg mr-2">📦</span>
            Add Material
          </button>
          <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200" onClick={() => window.location.reload()}>
            <span className="text-lg mr-2">🔄</span>
            Refresh Data
          </button>
        </div>
      </div>

      {/* Admin Management Section */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Pending User Approvals */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Pending User Approvals</h2>
            <span className="bg-red-100 text-red-800 text-xs font-medium px-2.5 py-0.5 rounded-full">
              {pendingUsers.length}
            </span>
          </div>
          
          {loadingPendingUsers ? (
            <div className="space-y-3">
              {[1, 2, 3].map(i => (
                <div key={i} className="animate-pulse p-3 bg-gray-50 rounded-lg">
                  <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </div>
              ))}
            </div>
          ) : pendingUsers.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <p>No pending approvals</p>
            </div>
          ) : (
            <div className="space-y-3">
              {pendingUsers.slice(0, 5).map((user, index) => (
                <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <div className="font-medium text-gray-900">
                      {user.first_name} {user.last_name}
                    </div>
                    <div className="text-sm text-gray-600">{getBaseEmail(user.email)}</div>
                    <div className="text-sm text-gray-500">
                      Role: <span className="capitalize">{user.user_type}</span>
                    </div>
                  </div>
                  <div className="flex space-x-2">
                    <button className="bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">Approve</button>
                    <button 
                      onClick={() => {
                        if (window.confirm('Are you sure you want to reject this user?')) {
                          // This would call the reject API
                          console.log('Rejecting user...')
                        }
                      }}
                      className="bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm"
                    >
                      Reject
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Materials Management */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Materials</h2>
            <button className="text-slate-600 hover:text-slate-700 text-sm font-medium">
              Manage all
            </button>
          </div>
          
          {loadingMaterials ? (
            <div className="space-y-3">
              {[1, 2, 3].map(i => (
                <div key={i} className="animate-pulse p-3 bg-gray-50 rounded-lg">
                  <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </div>
              ))}
            </div>
          ) : materials.length === 0 ? (
            <div className="text-center py-8 text-gray-500">
              <p>No materials configured</p>
              <button className="mt-2 text-slate-600 hover:text-slate-700 text-sm">
                Add first material
              </button>
            </div>
          ) : (
            <div className="space-y-3">
              {materials.slice(0, 5).map((material, index) => (
                <div key={index} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                  <div>
                    <div className="font-medium text-gray-900">{material.name}</div>
                    <div className="text-sm text-gray-600">
                      {material.description || 'No description'}
                    </div>
                  </div>
                  <div className="flex space-x-2">
                    <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">Edit</button>
                    <button className="bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">Delete</button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Add Vehicle Dialog */}
      {showAddVehicleDialog && <AddVehicleDialog onClose={() => setShowAddVehicleDialog(false)} onSuccess={fetchDashboardData} />}
      
      {/* Add Material Dialog */}
      {showAddMaterialDialog && <AddMaterialDialog onClose={() => setShowAddMaterialDialog(false)} onSuccess={fetchDashboardData} />}
      
      {/* Add Admin Dialog */}
      {showAddAdminDialog && <AddAdminDialog onClose={() => setShowAddAdminDialog(false)} onSuccess={fetchDashboardData} />}
    </div>
  )
}

// Dialog Components
function AddVehicleDialog({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const [formData, setFormData] = useState({
    license_plate: '',
    model: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    if (!formData.license_plate.trim()) {
      newErrors.license_plate = 'License plate is required'
    }
    
    if (!formData.model.trim()) {
      newErrors.model = 'Vehicle model is required'
    }
    
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) {
      return
    }
    
    setIsLoading(true)
    
    try {
      await apiService.createTruck(formData)
      onSuccess()
      onClose()
    } catch (error) {
      if (error instanceof Error && (error as any).response?.data) {
        const apiErrors = (error as any).response.data
        const newErrors: Record<string, string> = {}
        
        for (const [field, messages] of Object.entries(apiErrors)) {
          if (Array.isArray(messages)) {
            newErrors[field] = messages[0]
          } else if (typeof messages === 'string') {
            newErrors[field] = messages
          }
        }
        
        setErrors(newErrors)
      } else {
        alert(error instanceof Error ? error.message : 'Failed to create vehicle')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Add New Truck</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <span className="text-xl">×</span>
          </button>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className={`label ${errors.license_plate ? 'text-red-700' : ''}`}>
              License Plate <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={formData.license_plate}
              onChange={(e) => {
                setFormData(prev => ({ ...prev, license_plate: e.target.value }))
                if (errors.license_plate) setErrors(prev => ({ ...prev, license_plate: '' }))
              }}
              className={`input ${errors.license_plate ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : ''}`}
              placeholder="TRK-001"
            />
            {errors.license_plate && (
              <p className="text-sm text-red-600 flex items-center mt-1">
                <span className="mr-1">⚠️</span>
                {errors.license_plate}
              </p>
            )}
          </div>
          
          <div>
            <label className={`label ${errors.model ? 'text-red-700' : ''}`}>
              Model <span className="text-red-500">*</span>
            </label>
            <input
              type="text"
              value={formData.model}
              onChange={(e) => {
                setFormData(prev => ({ ...prev, model: e.target.value }))
                if (errors.model) setErrors(prev => ({ ...prev, model: '' }))
              }}
              className={`input ${errors.model ? 'border-red-300 focus:ring-red-500 focus:border-red-500' : ''}`}
              placeholder="Ford F-150"
            />
            {errors.model && (
              <p className="text-sm text-red-600 flex items-center mt-1">
                <span className="mr-1">⚠️</span>
                {errors.model}
              </p>
            )}
          </div>
          
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">
              Cancel
            </button>
            <button type="submit" disabled={isLoading} className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 disabled:opacity-50">
              {isLoading ? 'Creating...' : 'Create Truck'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

function AddMaterialDialog({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const [formData, setFormData] = useState({
    name: '',
    description: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const { showError, showSuccess } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    
    try {
      await apiService.createMaterial(formData)
      showSuccess('Material created', `${formData.name} has been added successfully.`)
      onSuccess()
      onClose()
    } catch (error) {
      console.error('Failed to create material:', error)
      showError('Failed to create material', error instanceof Error ? error.message : 'An unexpected error occurred')
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
          <div>
            <label className="label">Material Name</label>
            <input
              type="text"
              value={formData.name}
              onChange={(e) => setFormData(prev => ({ ...prev, name: e.target.value }))}
              className="input"
              placeholder="Sand, Gravel, Cement, etc."
              required
            />
          </div>
          
          
          <div>
            <label className="label">Description (Optional)</label>
            <textarea
              value={formData.description}
              onChange={(e) => setFormData(prev => ({ ...prev, description: e.target.value }))}
              className="input"
              placeholder="Additional details about the material..."
              rows={3}
            />
          </div>
          
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

function AddAdminDialog({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    password_confirm: '',
    first_name: '',
    last_name: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})
  const { showSuccess, showError } = useToast()

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const emailError = validateEmail(formData.email)
    if (emailError) newErrors.email = emailError
    
    const firstNameError = validateRequired(formData.first_name, 'First name')
    if (firstNameError) newErrors.first_name = firstNameError
    
    const lastNameError = validateRequired(formData.last_name, 'Last name')
    if (lastNameError) newErrors.last_name = lastNameError
    
    const passwordError = validateRequired(formData.password, 'Password')
    if (passwordError) newErrors.password = passwordError
    else if (formData.password.length < 8) newErrors.password = 'Password must be at least 8 characters'
    
    if (formData.password !== formData.password_confirm) {
      newErrors.password_confirm = 'Passwords do not match'
    }
    
    setClientErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    // Clear errors for this field when user types
    if (clientErrors[field]) {
      setClientErrors(prev => ({ ...prev, [field]: '' }))
    }
    if (getErrorsForField(errors, field).length > 0) {
      setErrors(prev => prev.filter(error => error.field !== field))
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
      await apiClient.createUser({
        email: formData.email,
        password: formData.password,
        first_name: formData.first_name,
        last_name: formData.last_name,
        user_type: 'admin'
      })
      
      showSuccess('Admin created', 'New admin user has been created successfully.')
      onSuccess()
      onClose()
    } catch (error) {
      if (error instanceof ApiError) {
        console.log('[CreateAdmin] ApiError details:', error.errors)
        const apiErrors = error.getAllErrors()
        console.log('[CreateAdmin] Parsed errors:', apiErrors)
        setErrors(apiErrors)
        
        const generalErrors = getGeneralErrors(apiErrors)
        if (generalErrors.length > 0) {
          showError('Failed to create admin', generalErrors[0])
        } else {
          showError('Failed to create admin', 'Please check the form for errors and try again.')
        }
      } else {
        console.log('[CreateAdmin] Non-ApiError:', error)
        showError('Failed to create admin', 'An unexpected error occurred. Please try again.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Add New Admin</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-600">
            <span className="text-xl">×</span>
          </button>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="label">First Name</label>
              <input
                type="text"
                value={formData.first_name}
                onChange={(e) => handleInputChange('first_name', e.target.value)}
                className={`input ${
                  [...getErrorsForField(errors, 'first_name'), ...(clientErrors.first_name ? [clientErrors.first_name] : [])].length > 0 
                    ? 'border-red-500 focus:border-red-500' 
                    : ''
                }`}
                placeholder="John"
                required
              />
              {[...getErrorsForField(errors, 'first_name'), ...(clientErrors.first_name ? [clientErrors.first_name] : [])].map((error, i) => (
                <p key={i} className="mt-1 text-sm text-red-600">{error}</p>
              ))}
            </div>
            <div>
              <label className="label">Last Name</label>
              <input
                type="text"
                value={formData.last_name}
                onChange={(e) => handleInputChange('last_name', e.target.value)}
                className={`input ${
                  [...getErrorsForField(errors, 'last_name'), ...(clientErrors.last_name ? [clientErrors.last_name] : [])].length > 0 
                    ? 'border-red-500 focus:border-red-500' 
                    : ''
                }`}
                placeholder="Smith"
                required
              />
              {[...getErrorsForField(errors, 'last_name'), ...(clientErrors.last_name ? [clientErrors.last_name] : [])].map((error, i) => (
                <p key={i} className="mt-1 text-sm text-red-600">{error}</p>
              ))}
            </div>
          </div>
          
          <div>
            <label className="label">Admin Email</label>
            <input
              type="email"
              value={formData.email}
              onChange={(e) => handleInputChange('email', e.target.value)}
              className={`input ${
                [...getErrorsForField(errors, 'email'), ...(clientErrors.email ? [clientErrors.email] : [])].length > 0 
                  ? 'border-red-500 focus:border-red-500' 
                  : ''
              }`}
              placeholder="admin@company.com"
              required
            />
            {[...getErrorsForField(errors, 'email'), ...(clientErrors.email ? [clientErrors.email] : [])].map((error, i) => (
              <p key={i} className="mt-1 text-sm text-red-600">{error}</p>
            ))}
          </div>
          
          <div>
            <label className="label">Password</label>
            <input
              type="password"
              value={formData.password}
              onChange={(e) => handleInputChange('password', e.target.value)}
              className={`input ${
                [...getErrorsForField(errors, 'password'), ...(clientErrors.password ? [clientErrors.password] : [])].length > 0 
                  ? 'border-red-500 focus:border-red-500' 
                  : ''
              }`}
              placeholder="Create a secure password"
              required
            />
            {[...getErrorsForField(errors, 'password'), ...(clientErrors.password ? [clientErrors.password] : [])].map((error, i) => (
              <p key={i} className="mt-1 text-sm text-red-600">{error}</p>
            ))}
          </div>
          
          <div>
            <label className="label">Confirm Password</label>
            <input
              type="password"
              value={formData.password_confirm}
              onChange={(e) => handleInputChange('password_confirm', e.target.value)}
              className={`input ${
                [...getErrorsForField(errors, 'password_confirm'), ...(clientErrors.password_confirm ? [clientErrors.password_confirm] : [])].length > 0 
                  ? 'border-red-500 focus:border-red-500' 
                  : ''
              }`}
              placeholder="Confirm password"
              required
            />
            {[...getErrorsForField(errors, 'password_confirm'), ...(clientErrors.password_confirm ? [clientErrors.password_confirm] : [])].map((error, i) => (
              <p key={i} className="mt-1 text-sm text-red-600">{error}</p>
            ))}
          </div>
          
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200">
              Cancel
            </button>
            <button type="submit" disabled={isLoading} className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 disabled:opacity-50">
              {isLoading ? 'Creating...' : 'Create Admin'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}