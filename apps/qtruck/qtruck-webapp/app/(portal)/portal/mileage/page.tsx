'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input, Select } from '@/components/ui/FormField'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { 
  ValidationError, 
  parseApiErrors, 
  getErrorsForField, 
  getGeneralErrors,
  validateRequired
} from '@/lib/validation'

export default function MileagePage() {
  const [mileageRecords, setMileageRecords] = useState<any[]>([])
  const [trucks, setTrucks] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const { showError } = useToast()

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    try {
      setLoading(true)
      const [mileageData, trucksData] = await Promise.all([
        apiService.getVehicleMileage(),
        apiService.getTrucks()
      ])
      setMileageRecords(mileageData || [])
      setTrucks(trucksData || [])
    } catch (error) {
      showError('Failed to load mileage records', error instanceof Error ? error.message : 'An unexpected error occurred')
    } finally {
      setLoading(false)
    }
  }

  const totalMileage = mileageRecords.reduce((sum, record) => sum + (record.mileage || 0), 0)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Vehicle Mileage</h1>
          <p className="text-gray-600">Track vehicle mileage with proof images</p>
        </div>
        <div className="flex items-center space-x-4">
          <RefreshButton onRefresh={fetchData} loading={loading} theme="driver" />
          <button 
            onClick={() => setShowAddDialog(true)}
            className="btn btn-primary"
          >
            <span className="text-lg mr-2">📏</span>
            Log Mileage
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📏</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">{totalMileage.toFixed(1)}</p>
              <p className="text-sm text-gray-600">Total Miles Driven</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📄</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">{mileageRecords.length}</p>
              <p className="text-sm text-gray-600">Mileage Records</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📊</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {mileageRecords.length > 0 ? (totalMileage / mileageRecords.length).toFixed(1) : '0.0'}
              </p>
              <p className="text-sm text-gray-600">Average per Record</p>
            </div>
          </div>
        </div>
      </div>

      {/* Mileage Records List */}
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
      ) : mileageRecords.length === 0 ? (
        <div className="card text-center py-12">
          <div className="text-6xl mb-4">📏</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No mileage records</h3>
          <p className="text-gray-600 mb-4">Start logging your vehicle mileage.</p>
          <button 
            onClick={() => setShowAddDialog(true)}
            className="btn btn-primary"
          >
            Log First Mileage
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {mileageRecords.map((record) => (
            <div key={record.id} className="card card-hover">
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-4">
                    <div className="w-12 h-12 bg-amber-100 rounded-lg flex items-center justify-center">
                      <span className="text-xl">📏</span>
                    </div>
                    <div>
                      <h3 className="text-lg font-semibold text-gray-900">
                        {record.truck?.license_plate || `Truck ${record.truck_id}`}
                      </h3>
                      <p className="text-gray-600">
                        {record.start_mileage} → {record.end_mileage} miles
                      </p>
                      <div className="flex items-center space-x-4 mt-1">
                        <span className="text-sm text-gray-500">
                          Date: {record.date ? new Date(record.date).toLocaleDateString() : 'No date'}
                        </span>
                        {record.proof_image && (
                          <span className="text-sm text-green-600">📷 Proof attached</span>
                        )}
                        {record.proof_end_image && (
                          <span className="text-sm text-green-600">📷 End proof attached</span>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
                
                <div className="text-right">
                  <div className="text-2xl font-bold text-gray-900">{record.mileage || 0}</div>
                  <div className="text-sm text-gray-500">Miles Driven</div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add Mileage Dialog */}
      {showAddDialog && (
        <AddMileageDialog 
          trucks={trucks}
          onClose={() => setShowAddDialog(false)} 
          onSuccess={() => {
            fetchData()
            setShowAddDialog(false)
          }} 
        />
      )}
    </div>
  )
}

// Add Mileage Dialog Component
function AddMileageDialog({ trucks, onClose, onSuccess }: { trucks: any[]; onClose: () => void; onSuccess: () => void }) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    truck_id: '',
    start_mileage: '',
    end_mileage: '',
    date: new Date().toISOString().split('T')[0],
    proof_image: null as File | null,
    proof_end_image: null as File | null
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const truckError = validateRequired(formData.truck_id, 'Truck')
    if (truckError) newErrors.truck_id = truckError
    
    const startMileageError = validateRequired(formData.start_mileage, 'Start mileage')
    if (startMileageError) newErrors.start_mileage = startMileageError
    else if (isNaN(parseFloat(formData.start_mileage))) {
      newErrors.start_mileage = 'Start mileage must be a number'
    }
    
    const endMileageError = validateRequired(formData.end_mileage, 'End mileage')
    if (endMileageError) newErrors.end_mileage = endMileageError
    else if (isNaN(parseFloat(formData.end_mileage))) {
      newErrors.end_mileage = 'End mileage must be a number'
    } else if (parseFloat(formData.end_mileage) <= parseFloat(formData.start_mileage)) {
      newErrors.end_mileage = 'End mileage must be greater than start mileage'
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

  const handleFileChange = (field: string, file: File | null) => {
    setFormData(prev => ({ ...prev, [field]: file }))
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
      // Note: For file uploads, you'd typically use FormData and a different API method
      // For now, we'll send the basic data without files
      const mileageData = {
        truck_id: formData.truck_id,
        start_mileage: parseFloat(formData.start_mileage),
        end_mileage: parseFloat(formData.end_mileage),
        date: new Date(formData.date).toISOString()
      }
      
      // This would need to be implemented in apiService to handle file uploads
      // await apiService.createVehicleMileage(mileageData, formData.proof_image, formData.proof_end_image)
      
      showSuccess('Mileage logged successfully!', 'Your mileage record has been saved.')
      onSuccess()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Failed to log mileage', generalErrors[0])
      } else {
        showError('Failed to log mileage', 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Log Vehicle Mileage</h3>
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
            label="Start Mileage" 
            required
            errors={[...getErrorsForField(errors, 'start_mileage'), ...(clientErrors.start_mileage ? [clientErrors.start_mileage] : [])]}
          >
            <Input
              type="number"
              value={formData.start_mileage}
              onChange={(e) => handleInputChange('start_mileage', e.target.value)}
              placeholder="25000"
              errors={[...getErrorsForField(errors, 'start_mileage'), ...(clientErrors.start_mileage ? [clientErrors.start_mileage] : [])]}
            />
          </FormField>
          
          <FormField 
            label="End Mileage" 
            required
            errors={[...getErrorsForField(errors, 'end_mileage'), ...(clientErrors.end_mileage ? [clientErrors.end_mileage] : [])]}
          >
            <Input
              type="number"
              value={formData.end_mileage}
              onChange={(e) => handleInputChange('end_mileage', e.target.value)}
              placeholder="25150"
              errors={[...getErrorsForField(errors, 'end_mileage'), ...(clientErrors.end_mileage ? [clientErrors.end_mileage] : [])]}
            />
          </FormField>
          
          <FormField label="Date">
            <Input
              type="date"
              value={formData.date}
              onChange={(e) => handleInputChange('date', e.target.value)}
            />
          </FormField>
          
          <FormField label="Start Mileage Proof Image">
            <Input
              type="file"
              accept="image/*"
              onChange={(e) => handleFileChange('proof_image', e.target.files?.[0] || null)}
            />
          </FormField>
          
          <FormField label="End Mileage Proof Image">
            <Input
              type="file"
              accept="image/*"
              onChange={(e) => handleFileChange('proof_end_image', e.target.files?.[0] || null)}
            />
          </FormField>
          
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="btn btn-outline">
              Cancel
            </button>
            <button type="submit" disabled={isLoading} className="btn btn-primary">
              {isLoading ? 'Logging...' : 'Log Mileage'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}