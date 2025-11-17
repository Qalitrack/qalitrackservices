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
import TripTypesTab from './TripTypesTab'

// Trip List Component
function TripsTab({ isActive }: { isActive: boolean }) {
  const [trips, setTrips] = useState<Trip[]>([])
  const [loading, setLoading] = useState(false)
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

  // Only fetch data when tab is active
  useEffect(() => {
    if (isActive) {
      fetchTrips(true) // Reset on mount
      fetchFormData() // Load trucks, drivers, materials
    }
  }, [isActive])

  useEffect(() => {
    if (isActive) {
      const timer = setTimeout(() => {
        fetchTrips(true) // Reset when searching or clearing
      }, 500)
      return () => clearTimeout(timer)
    }
  }, [searchTerm, isActive])

  useEffect(() => {
    if (isActive) {
      fetchTrips(true)
    }
  }, [statusFilter, sortBy, isActive])

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

      // Set ordering
      if (sortBy === 'date') {
        params.ordering = '-date'
      } else if (sortBy === 'start_location') {
        params.ordering = 'start_location'
      } else if (sortBy === 'truck') {
        params.ordering = 'truck__license_plate'
      }

      const response = await apiClient.getTrips(params)

      if (reset) {
        setTrips(response.results || [])
      } else {
        setTrips(prev => [...prev, ...(response.results || [])])
      }

      setTotalCount(response.count || 0)
      setHasNextPage(!!response.next)
    } catch (error) {
      console.error('Failed to fetch trips:', error)
      showError('Failed to load trips')
    } finally {
      setLoading(false)
      setLoadingMore(false)
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'pending':
        return 'bg-yellow-100 text-yellow-800'
      case 'in_progress':
        return 'bg-blue-100 text-blue-800'
      case 'completed':
        return 'bg-green-100 text-green-800'
      case 'cancelled':
        return 'bg-red-100 text-red-800'
      default:
        return 'bg-gray-100 text-gray-800'
    }
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString()
  }

  if (!isActive) {
    return null
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <div>
          <h2 className="text-xl font-semibold text-gray-900">Trips</h2>
          <p className="text-gray-600 text-sm">View and manage all trips in the system</p>
        </div>
        <div className="flex gap-3">
          <RefreshButton onRefresh={() => fetchTrips(true)} loading={loading} />
          <button
            onClick={() => setShowAddDialog(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors text-sm"
          >
            Add Trip
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4 mb-6">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
          <div>
            <input
              type="text"
              placeholder="Search trips..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
            />
          </div>
          <div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as any)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
            >
              <option value="all">All Status</option>
              <option value="pending">Pending</option>
              <option value="in_progress">In Progress</option>
              <option value="completed">Completed</option>
              <option value="cancelled">Cancelled</option>
            </select>
          </div>
          <div>
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-sm"
            >
              <option value="date">Sort by Date</option>
              <option value="start_location">Sort by Location</option>
              <option value="truck">Sort by Truck</option>
            </select>
          </div>
          <div className="text-sm text-gray-600 self-center">
            {totalCount} trips found
          </div>
        </div>
      </div>

      {/* Trips List */}
      {loading && trips.length === 0 ? (
        <div className="flex justify-center items-center h-64">
          <div className="text-gray-500">Loading trips...</div>
        </div>
      ) : trips.length === 0 ? (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-8 text-center">
          <div className="text-gray-500 mb-4">No trips found</div>
          <button
            onClick={() => setShowAddDialog(true)}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
          >
            Create First Trip
          </button>
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50 border-b border-gray-200">
                <tr>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Trip
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Truck
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Driver
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Status
                  </th>
                  <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Date
                  </th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {trips.map((trip) => (
                  <tr key={trip.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <div>
                        <div className="font-medium text-gray-900 text-sm">
                          {trip.start_location || 'Start'} → {trip.end_location || 'Destination'}
                        </div>
                        <div className="text-xs text-gray-500">
                          #{trip.id.slice(0, 8)}...
                          {trip.material && ` • ${trip.material.name}`}
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {trip.truck?.license_plate}
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">
                      {trip.driver?.user?.full_name || trip.driver?.user?.email}
                    </td>
                    <td className="px-4 py-3">
                      <span className={`px-2 py-1 text-xs font-medium rounded-full ${getStatusColor(trip.status)}`}>
                        {trip.status.replace('_', ' ')}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-500">
                      {formatDate(trip.date)}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => {
                          setSelectedTrip(trip)
                          setShowTripDetails(true)
                        }}
                        className="text-blue-600 hover:text-blue-800 text-sm font-medium"
                      >
                        View Details
                      </button>
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
                  fetchTrips(false, nextPage)
                }}
                disabled={loadingMore}
                className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors disabled:opacity-50"
              >
                {loadingMore ? 'Loading...' : 'Load More'}
              </button>
            </div>
          )}
        </div>
      )}

      {/* Add Trip Dialog would go here - simplified for now */}
      {showAddDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-md w-full p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Add Trip</h3>
            <p className="text-gray-600 mb-4">Trip creation functionality would be implemented here.</p>
            <button
              onClick={() => setShowAddDialog(false)}
              className="w-full px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
            >
              Close
            </button>
          </div>
        </div>
      )}

      {/* Trip Details Modal would go here - simplified for now */}
      {showTripDetails && selectedTrip && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg max-w-2xl w-full p-6">
            <h3 className="text-lg font-bold text-gray-900 mb-4">Trip Details</h3>
            <div className="space-y-3">
              <div><strong>Trip ID:</strong> {selectedTrip.id}</div>
              <div><strong>Route:</strong> {selectedTrip.start_location || 'Start'} → {selectedTrip.end_location || 'Destination'}</div>
              <div><strong>Truck:</strong> {selectedTrip.truck?.license_plate}</div>
              <div><strong>Driver:</strong> {selectedTrip.driver?.user?.full_name || selectedTrip.driver?.user?.email}</div>
              <div><strong>Status:</strong> {selectedTrip.status}</div>
              <div><strong>Date:</strong> {formatDate(selectedTrip.date)}</div>
            </div>
            <button
              onClick={() => setShowTripDetails(false)}
              className="w-full mt-6 px-4 py-2 bg-gray-100 text-gray-700 rounded-lg hover:bg-gray-200 transition-colors"
            >
              Close
            </button>
          </div>
        </div>
      )}
    </div>
  )
}

export default function TripsDashboard() {
  const [activeTab, setActiveTab] = useState<'trips' | 'trip-types'>('trips')

  // Store tab preference in localStorage
  useEffect(() => {
    const savedTab = localStorage.getItem('trips-dashboard-active-tab') as 'trips' | 'trip-types'
    if (savedTab && (savedTab === 'trips' || savedTab === 'trip-types')) {
      setActiveTab(savedTab)
    }
  }, [])

  const handleTabChange = (tab: 'trips' | 'trip-types') => {
    setActiveTab(tab)
    localStorage.setItem('trips-dashboard-active-tab', tab)
  }

  return (
    <div>
      <div className="bg-white shadow-sm border-b border-gray-200">
        <div className="px-6">
          <nav className="-mb-px flex space-x-8" aria-label="Tabs">
            <button
              onClick={() => handleTabChange('trips')}
              className={`${
                activeTab === 'trips'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
            >
              Trips
            </button>
            <button
              onClick={() => handleTabChange('trip-types')}
              className={`${
                activeTab === 'trip-types'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              } whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm transition-colors`}
            >
              Trip Types
            </button>
          </nav>
        </div>
      </div>

      <div>
        {activeTab === 'trips' && <TripsTab isActive={true} />}
        {activeTab === 'trip-types' && <TripTypesTab isActive={true} />}
      </div>
    </div>
  )
}