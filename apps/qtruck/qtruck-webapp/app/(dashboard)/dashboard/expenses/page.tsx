'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { RefreshButton } from '@/components/ui/RefreshButton'

export default function ExpensesPage() {
  const [expenses, setExpenses] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchExpenses()
  }, [])

  const fetchExpenses = async () => {
    try {
      setLoading(true)
      // Note: This would need to be an admin endpoint to get all expenses
      // For now, we'll use a placeholder since the API service doesn't have getAllExpenses
      setExpenses([])
    } catch (error) {
      console.error('Failed to load expenses:', error)
    } finally {
      setLoading(false)
    }
  }

  const getTotalExpenses = () => {
    return expenses.reduce((sum, expense) => sum + parseFloat(expense.amount || 0), 0)
  }

  const getExpensesByCategory = () => {
    const categories = expenses.reduce((acc, expense) => {
      const category = expense.category || expense.description || 'Other'
      acc[category] = (acc[category] || 0) + parseFloat(expense.amount || 0)
      return acc
    }, {} as Record<string, number>)
    return categories
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Expense Management</h1>
          <p className="text-gray-600">Monitor all fleet expenses and costs</p>
        </div>
        <RefreshButton onRefresh={fetchExpenses} loading={loading} theme="admin" />
      </div>

      {/* Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">💰</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">{expenses.length}</p>
              <p className="text-sm text-gray-600">Total Expenses</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">💵</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">KSh {getTotalExpenses().toFixed(2)}</p>
              <p className="text-sm text-gray-600">Total Amount</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📅</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                KSh {(getTotalExpenses() / Math.max(1, new Date().getDate())).toFixed(2)}
              </p>
              <p className="text-sm text-gray-600">Daily Average</p>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">📊</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">
                {Object.keys(getExpensesByCategory()).length}
              </p>
              <p className="text-sm text-gray-600">Categories</p>
            </div>
          </div>
        </div>
      </div>

      {/* Expense Categories */}
      {Object.keys(getExpensesByCategory()).length > 0 && (
        <div className="card">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Expenses by Category</h2>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            {Object.entries(getExpensesByCategory()).map(([category, amount]) => (
              <div key={category} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                <span className="font-medium text-gray-900">{category}</span>
                <span className="text-gray-600">KSh {amount.toFixed(2)}</span>
              </div>
            ))}
          </div>
        </div>
      )}

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
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No expenses recorded yet</h3>
          <p className="text-gray-600">Expenses will appear here as drivers record trip costs.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {expenses.map((expense) => (
            <div key={expense.id} className="card card-hover">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
                    <span className="text-xl">💰</span>
                  </div>
                  <div>
                    <h3 className="text-lg font-semibold text-gray-900">
                      {expense.description}
                    </h3>
                    <p className="text-gray-600">
                      Trip #{expense.trip_id} • {expense.driver_name || 'Unknown Driver'}
                    </p>
                    <p className="text-sm text-gray-500">
                      {expense.date ? new Date(expense.date).toLocaleDateString() : 'No date'}
                    </p>
                  </div>
                </div>
                
                <div className="flex items-center space-x-4">
                  <div className="text-right">
                    <p className="text-lg font-bold text-gray-900">KSh {expense.amount}</p>
                    {expense.receipt_url && (
                      <p className="text-sm text-green-600">Receipt attached</p>
                    )}
                  </div>
                  <div className="flex space-x-2">
                    <button className="btn btn-sm btn-outline">
                      View Details
                    </button>
                    {expense.receipt_url && (
                      <button className="btn btn-sm btn-outline">
                        View Receipt
                      </button>
                    )}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}