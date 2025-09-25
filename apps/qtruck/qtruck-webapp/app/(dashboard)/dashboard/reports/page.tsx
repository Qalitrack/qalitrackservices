'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { RefreshButton } from '@/components/ui/RefreshButton'

export default function ReportsPage() {
  const [loading, setLoading] = useState(true)
  const [reportData, setReportData] = useState({
    totalTrips: 0,
    totalExpenses: 0,
    activeVehicles: 0,
    totalDrivers: 0,
    monthlyStats: []
  })

  useEffect(() => {
    fetchReportData()
  }, [])

  const fetchReportData = async () => {
    try {
      setLoading(true)
      // In a real implementation, this would be specific report endpoints
      const [trips, vehicles, drivers] = await Promise.all([
        apiService.getTrips(),
        apiService.getTrucks(),
        apiService.getDrivers()
      ])

      setReportData({
        totalTrips: trips.length,
        totalExpenses: 0, // Would be calculated from expenses API
        activeVehicles: vehicles.filter((v: any) => v.status === 'active').length,
        totalDrivers: drivers.length,
        monthlyStats: [] // Would be monthly aggregated data
      })
    } catch (error) {
      console.error('Failed to load report data:', error)
    } finally {
      setLoading(false)
    }
  }

  const exportReport = (format: string) => {
    alert(`Exporting report as ${format}... (Feature coming soon)`)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Reports & Analytics</h1>
          <p className="text-gray-600">Fleet performance insights and analytics</p>
        </div>
        <div className="flex items-center space-x-3">
          <RefreshButton onRefresh={fetchReportData} loading={loading} theme="admin" />
          <button 
            onClick={() => exportReport('PDF')}
            className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
          >
            📄 Export PDF
          </button>
          <button 
            onClick={() => exportReport('Excel')}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
          >
            📊 Export Excel
          </button>
        </div>
      </div>

      {/* Key Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-slate-100 rounded-lg flex items-center justify-center">
              <span className="text-2xl">🗺️</span>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{reportData.totalTrips}</p>
              <p className="text-sm text-gray-600">Total Trips</p>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <span className="text-2xl">🚛</span>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{reportData.activeVehicles}</p>
              <p className="text-sm text-gray-600">Active Vehicles</p>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-slate-100 rounded-lg flex items-center justify-center">
              <span className="text-2xl">👥</span>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">{reportData.totalDrivers}</p>
              <p className="text-sm text-gray-600">Total Drivers</p>
            </div>
          </div>
        </div>

        <div className="card">
          <div className="flex items-center space-x-3">
            <div className="w-12 h-12 bg-slate-100 rounded-lg flex items-center justify-center">
              <span className="text-2xl">💰</span>
            </div>
            <div>
              <p className="text-2xl font-bold text-gray-900">KSh {reportData.totalExpenses.toFixed(2)}</p>
              <p className="text-sm text-gray-600">Total Expenses</p>
            </div>
          </div>
        </div>
      </div>

      {/* Report Categories */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <div className="card card-hover">
          <div className="text-center">
            <div className="w-16 h-16 bg-slate-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">🚛</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Fleet Utilization</h3>
            <p className="text-gray-600 text-sm mb-4">Vehicle usage, efficiency, and performance metrics</p>
            <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full">
              Generate Report
            </button>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="w-16 h-16 bg-slate-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">👨‍💼</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Driver Performance</h3>
            <p className="text-gray-600 text-sm mb-4">Driver efficiency, trip completion, and safety</p>
            <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full">
              Generate Report
            </button>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="w-16 h-16 bg-slate-200 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">💰</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Financial Summary</h3>
            <p className="text-gray-600 text-sm mb-4">Costs, expenses, and profitability analysis</p>
            <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full">
              Generate Report
            </button>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="w-16 h-16 bg-slate-300 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">🗺️</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Trip Analytics</h3>
            <p className="text-gray-600 text-sm mb-4">Routes, delivery times, and trip efficiency</p>
            <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full">
              Generate Report
            </button>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="w-16 h-16 bg-slate-400 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">🔧</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Maintenance</h3>
            <p className="text-gray-600 text-sm mb-4">Vehicle maintenance, costs, and scheduling</p>
            <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full">
              Generate Report
            </button>
          </div>
        </div>

        <div className="card card-hover">
          <div className="text-center">
            <div className="w-16 h-16 bg-slate-500 rounded-full flex items-center justify-center mx-auto mb-4">
              <span className="text-2xl">📊</span>
            </div>
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Custom Reports</h3>
            <p className="text-gray-600 text-sm mb-4">Build custom reports with specific criteria</p>
            <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full">
              Create Custom
            </button>
          </div>
        </div>
      </div>

      {/* Recent Reports */}
      <div className="card">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold text-gray-900">Recent Reports</h2>
          <button className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-1.5 px-3 rounded-lg transition-all duration-200 text-sm">
            View All
          </button>
        </div>
        
        <div className="text-center py-8 text-gray-500">
          <div className="text-4xl mb-4">📊</div>
          <p>No reports generated yet</p>
          <p className="text-sm">Generate your first report to see it here</p>
        </div>
      </div>
    </div>
  )
}