'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input, Select, Textarea } from '@/components/ui/FormField'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { 
  ValidationError, 
  parseApiErrors, 
  getErrorsForField, 
  getGeneralErrors,
  validateRequired
} from '@/lib/validation'

export default function ExpensesPage() {
  const [expenses, setExpenses] = useState<any[]>([])
  const [trips, setTrips] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [showAddDialog, setShowAddDialog] = useState(false)
  const { showError } = useToast()

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    try {
      setLoading(true)
      const [expensesData, tripsData] = await Promise.all([
        apiService.getMyExpenses(),
        apiService.getMyTrips()
      ])
      setExpenses(expensesData || [])
      setTrips(tripsData || [])
    } catch (error) {
      showError('Failed to load expenses', error instanceof Error ? error.message : 'An unexpected error occurred')
    } finally {
      setLoading(false)
    }
  }

  const totalExpenses = expenses.reduce((sum, expense) => sum + parseFloat(expense.amount || 0), 0)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">My Expenses</h1>
          <p className="text-gray-600">Track and manage your trip expenses</p>
        </div>
        <div className="flex items-center space-x-4">
          <RefreshButton onRefresh={fetchData} loading={loading} theme="driver" />
          <button 
            onClick={() => setShowAddDialog(true)}
            className="btn btn-primary"
          >
            <span className="text-lg mr-2">💰</span>
            Add Expense
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">💰</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">${totalExpenses.toFixed(2)}</p>
              <p className="text-sm text-gray-600">Total Expenses</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📄</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">{expenses.length}</p>
              <p className="text-sm text-gray-600">Total Records</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📊</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                ${expenses.length > 0 ? (totalExpenses / expenses.length).toFixed(2) : '0.00'}
              </p>
              <p className="text-sm text-gray-600">Average Expense</p>
            </div>
          </div>
        </div>
      </div>

      {/* Expenses List */}
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
      ) : expenses.length === 0 ? (
        <div className="card text-center py-12">
          <div className="text-6xl mb-4">💰</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No expenses recorded</h3>
          <p className="text-gray-600 mb-4">Start tracking your trip expenses.</p>
          <button 
            onClick={() => setShowAddDialog(true)}
            className="btn btn-primary"
          >
            Add First Expense
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {expenses.map((expense) => (
            <div key={expense.id} className="card card-hover">
              <div className="flex items-center justify-between">
                <div className="flex-1">
                  <div className="flex items-center space-x-4">
                    <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                      <span className="text-xl">💰</span>
                    </div>
                    <div>
                      <h3 className="text-lg font-semibold text-gray-900">{expense.description}</h3>
                      <p className="text-gray-600">Trip ID: {expense.trip_id}</p>
                      <div className="flex items-center space-x-4 mt-1">
                        <span className="text-sm text-gray-500">
                          Created: {expense.created_at ? new Date(expense.created_at).toLocaleDateString() : 'No date'}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div className="text-right">
                  <div className="text-2xl font-bold text-gray-900">${expense.amount}</div>
                  <div className="text-sm text-gray-500">Amount</div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Add Expense Dialog */}
      {showAddDialog && (
        <AddExpenseDialog 
          trips={trips}
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

// Add Expense Dialog Component
function AddExpenseDialog({ trips, onClose, onSuccess }: { trips: any[]; onClose: () => void; onSuccess: () => void }) {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    trip_id: '',
    description: '',
    amount: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const tripError = validateRequired(formData.trip_id, 'Trip')
    if (tripError) newErrors.trip_id = tripError
    
    const descriptionError = validateRequired(formData.description, 'Description')
    if (descriptionError) newErrors.description = descriptionError
    
    const amountError = validateRequired(formData.amount, 'Amount')
    if (amountError) newErrors.amount = amountError
    else if (isNaN(parseFloat(formData.amount)) || parseFloat(formData.amount) <= 0) {
      newErrors.amount = 'Amount must be a positive number'
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
      await apiService.createExpense({
        ...formData,
        amount: parseFloat(formData.amount).toFixed(2)
      })
      showSuccess('Expense added successfully!', 'Your expense has been recorded.')
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
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 w-full max-w-md">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold">Add New Expense</h3>
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
            label="Trip" 
            required
            errors={[...getErrorsForField(errors, 'trip_id'), ...(clientErrors.trip_id ? [clientErrors.trip_id] : [])]}
          >
            <Select
              value={formData.trip_id}
              onChange={(e) => handleInputChange('trip_id', e.target.value)}
              errors={[...getErrorsForField(errors, 'trip_id'), ...(clientErrors.trip_id ? [clientErrors.trip_id] : [])]}
            >
              <option value="">Select a trip</option>
              {trips.map((trip) => (
                <option key={trip.id} value={trip.id}>
                  Trip {trip.id}: {trip.start_location} → {trip.end_location}
                </option>
              ))}
            </Select>
          </FormField>
          
          <FormField 
            label="Description" 
            required
            errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
          >
            <Input
              type="text"
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              placeholder="Fuel, Toll, Parking, etc."
              errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
            />
          </FormField>
          
          <FormField 
            label="Amount" 
            required
            errors={[...getErrorsForField(errors, 'amount'), ...(clientErrors.amount ? [clientErrors.amount] : [])]}
          >
            <Input
              type="number"
              step="0.01"
              value={formData.amount}
              onChange={(e) => handleInputChange('amount', e.target.value)}
              placeholder="25.50"
              errors={[...getErrorsForField(errors, 'amount'), ...(clientErrors.amount ? [clientErrors.amount] : [])]}
            />
          </FormField>
          
          <div className="flex justify-end space-x-3 pt-4">
            <button type="button" onClick={onClose} className="btn btn-outline">
              Cancel
            </button>
            <button type="submit" disabled={isLoading} className="btn btn-primary">
              {isLoading ? 'Adding...' : 'Add Expense'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}