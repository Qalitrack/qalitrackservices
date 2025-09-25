'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { useToast } from '@/components/ui/Toast'
import { RefreshButton } from '@/components/ui/RefreshButton'
import Link from 'next/link'

export default function DriverPortal() {
  const [trips, setTrips] = useState<any[]>([])
  const [expenses, setExpenses] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const { showError } = useToast()

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    try {
      setLoading(true)
      const [tripsData, expensesData] = await Promise.all([
        apiService.getMyTrips(),
        apiService.getMyExpenses()
      ])
      setTrips(tripsData || [])
      setExpenses(expensesData || [])
    } catch (error) {
      showError('Failed to load dashboard data', error instanceof Error ? error.message : 'An unexpected error occurred')
    } finally {
      setLoading(false)
    }
  }

  const activeTrips = trips.filter(trip => trip.status === 'pending' || trip.status === 'in_progress')
  const completedTrips = trips.filter(trip => trip.status === 'completed')
  const totalExpenses = expenses.reduce((sum, expense) => sum + parseFloat(expense.amount || 0), 0)
  if (loading) {
    return (
      <div className="space-y-6">
        <div className="card animate-pulse">
          <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
          <div className="h-3 bg-gray-200 rounded w-1/2"></div>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {[1, 2, 3, 4].map(i => (
            <div key={i} className="card animate-pulse">
              <div className="h-8 bg-gray-200 rounded w-1/2 mx-auto mb-2"></div>
              <div className="h-4 bg-gray-200 rounded w-3/4 mx-auto"></div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Welcome Header */}
      <div className="card">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Driver Dashboard 👋</h1>
            <p className="text-gray-600">Here's your trip and expense overview</p>
          </div>
          <div className="flex items-center space-x-4">
            <RefreshButton onRefresh={fetchData} loading={loading} theme="driver" />
            <div className="w-16 h-16 bg-gradient-to-r from-amber-400 to-orange-500 rounded-full flex items-center justify-center text-white text-2xl">
              🚗
            </div>
          </div>
        </div>
      </div>

      {/* Quick Stats */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="card card-hover">
          <div className="text-center">
            <div className="text-3xl mb-2">🗺️</div>
            <div className="text-2xl font-bold text-gray-900">{activeTrips.length}</div>
            <div className="text-sm text-gray-600">Active Trips</div>
            <div className="text-xs text-amber-600 mt-1">Currently in progress</div>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="text-3xl mb-2">✅</div>
            <div className="text-2xl font-bold text-gray-900">{completedTrips.length}</div>
            <div className="text-sm text-gray-600">Completed Trips</div>
            <div className="text-xs text-amber-600 mt-1">Total completed</div>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="text-3xl mb-2">💰</div>
            <div className="text-2xl font-bold text-gray-900">${totalExpenses.toFixed(2)}</div>
            <div className="text-sm text-gray-600">Total Expenses</div>
            <div className="text-xs text-amber-600 mt-1">All recorded expenses</div>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="text-3xl mb-2">📄</div>
            <div className="text-2xl font-bold text-gray-900">{expenses.length}</div>
            <div className="text-sm text-gray-600">Expense Records</div>
            <div className="text-xs text-amber-600 mt-1">Total entries</div>
          </div>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="card">
        <h2 className="text-lg font-semibold text-gray-900 mb-4">Quick Actions</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          <Link href="/portal/trips" className="bg-gradient-to-r from-orange-400 to-orange-500 text-white p-6 rounded-xl hover:from-orange-500 hover:to-orange-600 transition-all transform hover:-translate-y-1 shadow-lg block text-center">
            <div className="text-3xl mb-2">🗺️</div>
            <div className="font-semibold">View Trips</div>
            <div className="text-sm opacity-90">Manage your trips</div>
          </Link>

          <Link href="/portal/expenses" className="bg-gradient-to-r from-amber-600 to-orange-600 text-white p-6 rounded-xl hover:from-amber-700 hover:to-orange-700 transition-all transform hover:-translate-y-1 shadow-lg block text-center">
            <div className="text-3xl mb-2">💰</div>
            <div className="font-semibold">View Expenses</div>
            <div className="text-sm opacity-90">Record trip expenses</div>
          </Link>

          <Link href="/portal/mileage" className="bg-gradient-to-r from-amber-500 to-amber-600 text-white p-6 rounded-xl hover:from-amber-600 hover:to-amber-700 transition-all transform hover:-translate-y-1 shadow-lg block text-center">
            <div className="text-3xl mb-2">📏</div>
            <div className="font-semibold">Vehicle Mileage</div>
            <div className="text-sm opacity-90">Update vehicle mileage</div>
          </Link>

          <Link href="/portal/feedback" className="bg-gradient-to-r from-orange-500 to-amber-600 text-white p-6 rounded-xl hover:from-orange-600 hover:to-amber-700 transition-all transform hover:-translate-y-1 shadow-lg block text-center">
            <div className="text-3xl mb-2">💬</div>
            <div className="font-semibold">Send Feedback</div>
            <div className="text-sm opacity-90">Report issues or suggestions</div>
          </Link>
        </div>
      </div>

      {/* Recent Trips */}
      {activeTrips.length > 0 && (
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Active Trips</h2>
            <Link href="/portal/trips" className="btn btn-outline btn-sm">View All</Link>
          </div>
          
          <div className="space-y-4">
            {activeTrips.slice(0, 3).map((trip) => (
              <div key={trip.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div className="flex-1">
                  <div className="flex items-center space-x-3">
                    <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                      trip.status === 'pending' ? 'bg-yellow-100 text-yellow-800' : 
                      'bg-amber-100 text-amber-800'
                    }`}>
                      {trip.status}
                    </span>
                    <span className="text-sm text-gray-500">
                      {trip.date ? new Date(trip.date).toLocaleDateString() : 'No date'}
                    </span>
                  </div>
                  <h4 className="font-semibold text-gray-900 mt-1">
                    {trip.start_location} → {trip.end_location}
                  </h4>
                  <p className="text-gray-600 text-sm">
                    Trip ID: {trip.id} | Start Mileage: {trip.start_mileage}
                  </p>
                </div>
                <div className="text-right">
                  <div className="text-lg font-bold text-gray-900">${trip.total_cost || '0.00'}</div>
                  <div className="text-sm text-gray-500">Total Cost</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recent Expenses */}
      {expenses.length > 0 && (
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Recent Expenses</h2>
            <Link href="/portal/expenses" className="btn btn-outline btn-sm">View All</Link>
          </div>
          
          <div className="space-y-4">
            {expenses.slice(0, 5).map((expense) => (
              <div key={expense.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                <div className="flex-1">
                  <h4 className="font-semibold text-gray-900">{expense.description}</h4>
                  <p className="text-gray-600 text-sm">
                    Trip ID: {expense.trip_id} | {expense.created_at ? new Date(expense.created_at).toLocaleDateString() : 'No date'}
                  </p>
                </div>
                <div className="text-right">
                  <div className="text-lg font-bold text-gray-900">${expense.amount}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Empty States */}
      {trips.length === 0 && expenses.length === 0 && (
        <div className="card text-center py-12">
          <div className="text-6xl mb-4">🚛</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">Welcome to QTruck!</h3>
          <p className="text-gray-600 mb-6">You don't have any trips or expenses yet. Get started by exploring the navigation menu.</p>
          <div className="flex flex-wrap gap-3 justify-center">
            <Link href="/portal/trips" className="btn btn-primary">View Trips</Link>
            <Link href="/portal/profile" className="btn btn-outline">Setup Profile</Link>
          </div>
        </div>
      )}
    </div>
  )
}