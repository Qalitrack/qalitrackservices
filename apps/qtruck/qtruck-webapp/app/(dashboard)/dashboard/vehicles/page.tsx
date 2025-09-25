'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { useToast } from '@/components/ui/Toast'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { Truck } from '@/lib/types/api'

export default function VehiclesPage() {
  const [trucks, setTrucks] = useState<Truck[]>([])
  const [loading, setLoading] = useState(true)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const { showError } = useToast()

  useEffect(() => {
    fetchVehicles()
  }, [])

  const fetchVehicles = async () => {
    try {
      setLoading(true)
      const response = await apiService.getTrucks()
      setTrucks(response.results || [])
    } catch (error) {
      console.error('Failed to load vehicles:', error)
      showError('Failed to load vehicles', error instanceof Error ? error.message : 'An unexpected error occurred')
    } finally {
      setLoading(false)
    }
  }


  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Fleet Trucks</h1>
          <p className="text-gray-600">Manage your fleet of trucks</p>
        </div>
        <div className="flex items-center space-x-4">
          <RefreshButton onRefresh={fetchVehicles} loading={loading} theme="admin" />
          <button 
            onClick={() => setShowAddDialog(true)}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
          >
            <span className="text-lg mr-2">🚛</span>
            Add Truck
          </button>
        </div>
      </div>

      {/* Stats Card */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">🚛</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">{trucks.length}</p>
              <p className="text-sm text-gray-600">Total Trucks</p>
            </div>
          </div>
        </div>
      </div>

      {/* Vehicles List */}
      {loading ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {[1, 2, 3, 4, 5, 6].map(i => (
            <div key={i} className="card animate-pulse">
              <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div className="h-3 bg-gray-200 rounded w-1/2 mb-1"></div>
              <div className="h-3 bg-gray-200 rounded w-1/3"></div>
            </div>
          ))}
        </div>
      ) : trucks.length === 0 ? (
        <div className="card text-center py-12">
          <div className="text-6xl mb-4">🚛</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No trucks yet</h3>
          <p className="text-gray-600 mb-4">Add your first truck to get started.</p>
          <button 
            onClick={() => setShowAddDialog(true)}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
          >
            Add First Truck
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {trucks.map((truck) => (
            <div key={truck.id} className="card card-hover">
              <div className="flex items-start justify-between mb-4">
                <div className="flex items-center space-x-3">
                  <span className="text-3xl">🚛</span>
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">
                      {truck.license_plate}
                    </h3>
                    <p className="text-gray-600">{truck.model}</p>
                  </div>
                </div>
              </div>
              
              <div className="flex justify-end space-x-2">
                <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">
                  Edit
                </button>
                <button className="bg-gradient-to-r from-red-500 to-red-600 hover:from-red-600 hover:to-red-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">
                  Delete
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add Truck Dialog */}
      {showAddDialog && (
        <AddTruckDialog 
          onClose={() => setShowAddDialog(false)} 
          onSuccess={() => {
            fetchVehicles()
            setShowAddDialog(false)
          }} 
        />
      )}
    </div>
  )
}

// Add Vehicle Dialog Component (same as in dashboard)
function AddTruckDialog({ onClose, onSuccess }: { onClose: () => void; onSuccess: () => void }) {
  const [formData, setFormData] = useState({
    license_plate: '',
    model: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const { showError, showSuccess } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setIsLoading(true)
    
    try {
      await apiService.createTruck(formData)
      showSuccess('Truck created', `${formData.license_plate} has been added to the fleet.`)
      onSuccess()
    } catch (error) {
      console.error('Failed to create truck:', error)
      showError('Failed to create truck', error instanceof Error ? error.message : 'An unexpected error occurred')
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
            <label className="label">License Plate</label>
            <input
              type="text"
              value={formData.license_plate}
              onChange={(e) => setFormData(prev => ({ ...prev, license_plate: e.target.value }))}
              className="input"
              placeholder="TRK-001"
              required
            />
          </div>
          
          <div>
            <label className="label">Model</label>
            <input
              type="text"
              value={formData.model}
              onChange={(e) => setFormData(prev => ({ ...prev, model: e.target.value }))}
              className="input"
              placeholder="Ford F-150"
              required
            />
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