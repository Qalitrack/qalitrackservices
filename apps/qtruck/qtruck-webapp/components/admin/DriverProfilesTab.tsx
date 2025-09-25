'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { getBaseEmail } from '@/lib/utils'
import { useToast } from '@/components/ui/Toast'
import { RefreshButton } from '@/components/ui/RefreshButton'
import ActivityHeatmap from '@/components/ui/ActivityHeatmap'

interface DriverProfile {
  id: string
  driver: string
  driver_name: string
  full_name: string
  phone_number: string
  id_number: string
  license_number: string
  license_expiry_date: string
  license_class: string
  profile_photo?: string
  license_front_image?: string
  license_back_image?: string
  id_front_image?: string
  id_back_image?: string
  status: 'draft' | 'pending' | 'approved' | 'rejected' | 'changes_requested'
  status_display: string
  is_current: boolean
  version_number: number
  submitted_at?: string
  reviewed_at?: string
  reviewed_by?: string
  reviewed_by_name?: string
  approval_notes: string
  rejection_reason: string
  created_at: string
  updated_at: string
  is_license_expired: boolean
  days_until_license_expiry: number
}

interface EnhancedDriver {
  id: string
  name: string
  phone: string
  license_number: string
  user_email: string
  user_base_email: string
  user_is_active: boolean
  current_profile?: DriverProfile
  pending_profile?: DriverProfile
  latest_activity?: any
  activity_stats: any
  license_status: {
    status: 'no_profile' | 'expired' | 'expiring_soon' | 'valid'
    message: string
    days_until_expiry?: number
    days_overdue?: number
  }
  created_at: string
  updated_at: string
}

export default function DriverProfilesTab() {
  const [drivers, setDrivers] = useState<EnhancedDriver[]>([])
  const [pendingProfiles, setPendingProfiles] = useState<DriverProfile[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterStatus, setFilterStatus] = useState('all')
  const [selectedDriver, setSelectedDriver] = useState<EnhancedDriver | null>(null)
  const [showProfileDetail, setShowProfileDetail] = useState(false)
  const [subTab, setSubTab] = useState<'drivers' | 'pending' | 'expiring'>('drivers')
  const { showSuccess, showError } = useToast()

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    try {
      setLoading(true)
      await Promise.all([fetchDrivers(), fetchPendingProfiles()])
    } catch (error) {
      console.error('Failed to load driver data:', error)
    } finally {
      setLoading(false)
    }
  }

  const fetchDrivers = async () => {
    try {
      const driversData = await apiService.getEnhancedDrivers()
      setDrivers(driversData)
    } catch (error) {
      console.error('Failed to load drivers:', error)
    }
  }

  const fetchPendingProfiles = async () => {
    try {
      const profiles = await apiService.getPendingDriverProfiles()
      setPendingProfiles(profiles)
    } catch (error) {
      console.error('Failed to load pending profiles:', error)
    }
  }

  const handleApproveProfile = async (profileId: string, action: 'approve' | 'reject' | 'request_changes', notes = '') => {
    try {
      await apiService.approveDriverProfile(profileId, action, notes)
      showSuccess(`Profile ${action}d successfully`, 'The driver profile has been updated.')
      await fetchData()
    } catch (error) {
      showError('Failed to update profile', error instanceof Error ? error.message : 'An unexpected error occurred')
    }
  }

  const filteredDrivers = drivers.filter(driver => {
    const matchesSearch = driver.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         driver.user_base_email.toLowerCase().includes(searchTerm.toLowerCase())
    
    if (filterStatus === 'all') return matchesSearch
    if (filterStatus === 'no_profile') return matchesSearch && !driver.current_profile
    if (filterStatus === 'expired') return matchesSearch && driver.license_status.status === 'expired'
    if (filterStatus === 'expiring') return matchesSearch && driver.license_status.status === 'expiring_soon'
    if (filterStatus === 'valid') return matchesSearch && driver.license_status.status === 'valid'
    
    return matchesSearch
  })

  const expiringDrivers = drivers.filter(driver => 
    driver.license_status.status === 'expired' || driver.license_status.status === 'expiring_soon'
  )

  const getLicenseStatusBadge = (status: string, message: string) => {
    const baseClasses = "inline-flex px-2 py-1 text-xs font-medium rounded-full"
    
    switch (status) {
      case 'expired':
        return `${baseClasses} bg-red-100 text-red-800`
      case 'expiring_soon':
        return `${baseClasses} bg-yellow-100 text-yellow-800`
      case 'valid':
        return `${baseClasses} bg-green-100 text-green-800`
      default:
        return `${baseClasses} bg-gray-100 text-gray-800`
    }
  }

  const getProfileStatusBadge = (status: string) => {
    const baseClasses = "inline-flex px-2 py-1 text-xs font-medium rounded-full"
    
    switch (status) {
      case 'approved':
        return `${baseClasses} bg-green-100 text-green-800`
      case 'pending':
        return `${baseClasses} bg-yellow-100 text-yellow-800`
      case 'rejected':
        return `${baseClasses} bg-red-100 text-red-800`
      case 'changes_requested':
        return `${baseClasses} bg-orange-100 text-orange-800`
      default:
        return `${baseClasses} bg-gray-100 text-gray-800`
    }
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Driver Profiles</h2>
          <p className="text-gray-600">Manage driver profiles, licenses, and approvals</p>
        </div>
        <RefreshButton onRefresh={fetchData} loading={loading} />
      </div>

      {/* Statistics */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">👥</span>
            <div>
              <p className="text-2xl font-bold text-gray-900">{drivers.length}</p>
              <p className="text-sm text-gray-600">Total Drivers</p>
            </div>
          </div>
        </div>
        
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">⏳</span>
            <div>
              <p className="text-2xl font-bold text-yellow-600">{pendingProfiles.length}</p>
              <p className="text-sm text-gray-600">Pending Approval</p>
            </div>
          </div>
        </div>
        
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">⚠️</span>
            <div>
              <p className="text-2xl font-bold text-red-600">{expiringDrivers.length}</p>
              <p className="text-sm text-gray-600">License Issues</p>
            </div>
          </div>
        </div>
        
        <div className="card">
          <div className="flex items-center space-x-3">
            <span className="text-2xl">✅</span>
            <div>
              <p className="text-2xl font-bold text-green-600">
                {drivers.filter(d => d.current_profile && d.license_status.status === 'valid').length}
              </p>
              <p className="text-sm text-gray-600">Valid Profiles</p>
            </div>
          </div>
        </div>
      </div>

      {/* Sub-tab Navigation */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setSubTab('drivers')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              subTab === 'drivers'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            All Drivers
            <span className="ml-2 bg-gray-100 text-gray-600 py-0.5 px-2 rounded-full text-xs">
              {drivers.length}
            </span>
          </button>
          
          <button
            onClick={() => setSubTab('pending')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              subTab === 'pending'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Pending Approvals
            {pendingProfiles.length > 0 && (
              <span className="ml-2 bg-red-100 text-red-600 py-0.5 px-2 rounded-full text-xs">
                {pendingProfiles.length}
              </span>
            )}
          </button>
          
          <button
            onClick={() => setSubTab('expiring')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              subTab === 'expiring'
                ? 'border-blue-500 text-blue-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            License Issues
            {expiringDrivers.length > 0 && (
              <span className="ml-2 bg-yellow-100 text-yellow-600 py-0.5 px-2 rounded-full text-xs">
                {expiringDrivers.length}
              </span>
            )}
          </button>
        </nav>
      </div>

      {/* Filters and Search */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <input
            type="text"
            placeholder="Search drivers..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          />
        </div>
        
        {subTab === 'drivers' && (
          <div className="sm:w-48">
            <select
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
            >
              <option value="all">All Status</option>
              <option value="no_profile">No Profile</option>
              <option value="valid">Valid License</option>
              <option value="expiring">Expiring Soon</option>
              <option value="expired">Expired</option>
            </select>
          </div>
        )}
      </div>

      {/* Content */}
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
      ) : (
        <>
          {subTab === 'drivers' && (
            <DriversGrid 
              drivers={filteredDrivers}
              onViewProfile={(driver) => {
                setSelectedDriver(driver)
                setShowProfileDetail(true)
              }}
              getLicenseStatusBadge={getLicenseStatusBadge}
            />
          )}
          
          {subTab === 'pending' && (
            <PendingProfilesGrid 
              profiles={pendingProfiles}
              onApprove={handleApproveProfile}
              getProfileStatusBadge={getProfileStatusBadge}
            />
          )}
          
          {subTab === 'expiring' && (
            <ExpiringLicensesGrid 
              drivers={expiringDrivers}
              onViewProfile={(driver) => {
                setSelectedDriver(driver)
                setShowProfileDetail(true)
              }}
              getLicenseStatusBadge={getLicenseStatusBadge}
            />
          )}
        </>
      )}

      {/* Driver Detail Modal */}
      {selectedDriver && showProfileDetail && (
        <DriverDetailModal
          driver={selectedDriver}
          onClose={() => {
            setSelectedDriver(null)
            setShowProfileDetail(false)
          }}
        />
      )}
    </div>
  )
}

// Component for displaying drivers grid
function DriversGrid({ drivers, onViewProfile, getLicenseStatusBadge }: {
  drivers: EnhancedDriver[]
  onViewProfile: (driver: EnhancedDriver) => void
  getLicenseStatusBadge: (status: string, message: string) => string
}) {
  if (drivers.length === 0) {
    return (
      <div className="card text-center py-12">
        <div className="text-6xl mb-4">👥</div>
        <h3 className="text-lg font-semibold text-gray-900 mb-2">No drivers found</h3>
        <p className="text-gray-600">No drivers match your current filters.</p>
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {drivers.map((driver) => (
        <div key={driver.id} className="card card-hover">
          <div className="flex items-start space-x-4">
            <div className="w-12 h-12 bg-blue-100 rounded-full flex items-center justify-center">
              <span className="text-xl">👤</span>
            </div>
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-gray-900">{driver.name}</h3>
              <p className="text-gray-600">{getBaseEmail(driver.user_email)}</p>
              {driver.phone && (
                <p className="text-sm text-gray-500">{driver.phone}</p>
              )}
              
              <div className="mt-2 space-y-2">
                <div className={getLicenseStatusBadge(driver.license_status.status, driver.license_status.message)}>
                  {driver.license_status.message}
                </div>
                
                {driver.current_profile ? (
                  <div className="text-xs text-gray-500">
                    Profile v{driver.current_profile.version_number} • {driver.current_profile.license_number}
                  </div>
                ) : (
                  <div className="text-xs text-orange-600">No profile created</div>
                )}
              </div>
            </div>
          </div>
          
          <div className="mt-4 flex justify-end space-x-2">
            <button 
              onClick={() => onViewProfile(driver)}
              className="btn btn-sm btn-outline"
            >
              View Details
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}

// Component for pending profiles
function PendingProfilesGrid({ profiles, onApprove, getProfileStatusBadge }: {
  profiles: DriverProfile[]
  onApprove: (profileId: string, action: 'approve' | 'reject' | 'request_changes', notes?: string) => void
  getProfileStatusBadge: (status: string) => string
}) {
  if (profiles.length === 0) {
    return (
      <div className="card text-center py-12">
        <div className="text-6xl mb-4">✅</div>
        <h3 className="text-lg font-semibold text-gray-900 mb-2">No pending profiles</h3>
        <p className="text-gray-600">All driver profiles have been reviewed.</p>
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
      {profiles.map((profile) => (
        <div key={profile.id} className="card">
          <div className="flex items-start justify-between mb-4">
            <div>
              <h3 className="text-lg font-semibold text-gray-900">{profile.full_name}</h3>
              <p className="text-gray-600">{profile.driver_name}</p>
            </div>
            <div className={getProfileStatusBadge(profile.status)}>
              {profile.status_display}
            </div>
          </div>
          
          <div className="space-y-2 text-sm">
            <div><span className="font-medium">License:</span> {profile.license_number}</div>
            <div><span className="font-medium">Expiry:</span> {profile.license_expiry_date}</div>
            <div><span className="font-medium">Phone:</span> {profile.phone_number}</div>
            <div><span className="font-medium">ID:</span> {profile.id_number}</div>
            <div><span className="font-medium">Submitted:</span> {profile.submitted_at ? new Date(profile.submitted_at).toLocaleDateString() : 'N/A'}</div>
          </div>
          
          <div className="mt-4 flex space-x-2">
            <button
              onClick={() => onApprove(profile.id, 'approve')}
              className="btn btn-sm bg-green-600 text-white hover:bg-green-700"
            >
              Approve
            </button>
            <button
              onClick={() => onApprove(profile.id, 'reject', 'Please provide additional documentation')}
              className="btn btn-sm bg-red-600 text-white hover:bg-red-700"
            >
              Reject
            </button>
            <button
              onClick={() => onApprove(profile.id, 'request_changes', 'Please update your license information')}
              className="btn btn-sm btn-outline"
            >
              Request Changes
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}

// Component for expiring licenses
function ExpiringLicensesGrid({ drivers, onViewProfile, getLicenseStatusBadge }: {
  drivers: EnhancedDriver[]
  onViewProfile: (driver: EnhancedDriver) => void
  getLicenseStatusBadge: (status: string, message: string) => string
}) {
  if (drivers.length === 0) {
    return (
      <div className="card text-center py-12">
        <div className="text-6xl mb-4">✅</div>
        <h3 className="text-lg font-semibold text-gray-900 mb-2">All licenses are valid</h3>
        <p className="text-gray-600">No drivers have expiring or expired licenses.</p>
      </div>
    )
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {drivers.map((driver) => (
        <div key={driver.id} className="card border-l-4 border-yellow-400">
          <div className="flex items-start space-x-4">
            <div className="w-12 h-12 bg-yellow-100 rounded-full flex items-center justify-center">
              <span className="text-xl">⚠️</span>
            </div>
            <div className="flex-1">
              <h3 className="text-lg font-semibold text-gray-900">{driver.name}</h3>
              <p className="text-gray-600">{getBaseEmail(driver.user_email)}</p>
              
              <div className="mt-2">
                <div className={getLicenseStatusBadge(driver.license_status.status, driver.license_status.message)}>
                  {driver.license_status.message}
                </div>
              </div>
              
              {driver.current_profile && (
                <div className="mt-2 text-sm text-gray-500">
                  License: {driver.current_profile.license_number}<br />
                  Expires: {driver.current_profile.license_expiry_date}
                </div>
              )}
            </div>
          </div>
          
          <div className="mt-4 flex justify-end">
            <button 
              onClick={() => onViewProfile(driver)}
              className="btn btn-sm btn-outline"
            >
              View Profile
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}

// Driver Detail Modal Component
function DriverDetailModal({ driver, onClose }: {
  driver: EnhancedDriver
  onClose: () => void
}) {
  const [heatmapData, setHeatmapData] = useState<any>({})
  const [activityData, setActivityData] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<'profile' | 'activity' | 'history'>('profile')

  useEffect(() => {
    fetchDriverData()
  }, [driver.id])

  const fetchDriverData = async () => {
    try {
      setLoading(true)
      const [heatmap, activity] = await Promise.all([
        apiService.getDriverHeatmap(driver.id),
        apiService.getEnhancedDriverActivity(driver.id, 30)
      ])
      setHeatmapData(heatmap)
      setActivityData(activity)
    } catch (error) {
      console.error('Failed to load driver data:', error)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-4xl max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-200 bg-gradient-to-r from-blue-500 to-blue-600 text-white">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <div className="w-12 h-12 bg-white/20 rounded-full flex items-center justify-center">
                <span className="text-2xl">👤</span>
              </div>
              <div>
                <h3 className="text-xl font-semibold">{driver.name}</h3>
                <p className="text-blue-100">{getBaseEmail(driver.user_email)}</p>
              </div>
            </div>
            <button
              onClick={onClose}
              className="text-white/80 hover:text-white p-2 hover:bg-white/10 rounded-full transition-colors"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>

        {/* Tab Navigation */}
        <div className="border-b border-gray-200">
          <nav className="-mb-px flex px-6">
            <button
              onClick={() => setActiveTab('profile')}
              className={`py-3 px-4 border-b-2 font-medium text-sm ${
                activeTab === 'profile'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Profile Details
            </button>
            <button
              onClick={() => setActiveTab('activity')}
              className={`py-3 px-4 border-b-2 font-medium text-sm ${
                activeTab === 'activity'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Activity
            </button>
            <button
              onClick={() => setActiveTab('history')}
              className={`py-3 px-4 border-b-2 font-medium text-sm ${
                activeTab === 'history'
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              Profile History
            </button>
          </nav>
        </div>

        {/* Content */}
        <div className="p-6">
          {activeTab === 'profile' && (
            <ProfileTab driver={driver} />
          )}
          
          {activeTab === 'activity' && (
            <ActivityTab 
              driver={driver} 
              heatmapData={heatmapData} 
              activityData={activityData} 
              loading={loading} 
            />
          )}
          
          {activeTab === 'history' && (
            <HistoryTab driver={driver} />
          )}
        </div>
      </div>
    </div>
  )
}

// Profile Tab
function ProfileTab({ driver }: { driver: EnhancedDriver }) {
  return (
    <div className="space-y-6">
      {/* Current Profile */}
      {driver.current_profile ? (
        <div className="card">
          <h4 className="text-lg font-semibold text-gray-900 mb-4">Current Profile (v{driver.current_profile.version_number})</h4>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div className="space-y-3">
              <div>
                <label className="text-sm font-medium text-gray-500">Full Name</label>
                <p className="text-gray-900">{driver.current_profile.full_name}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">Phone Number</label>
                <p className="text-gray-900">{driver.current_profile.phone_number}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">ID Number</label>
                <p className="text-gray-900">{driver.current_profile.id_number}</p>
              </div>
            </div>
            <div className="space-y-3">
              <div>
                <label className="text-sm font-medium text-gray-500">License Number</label>
                <p className="text-gray-900">{driver.current_profile.license_number}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">License Class</label>
                <p className="text-gray-900">{driver.current_profile.license_class}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-500">License Expiry</label>
                <div className="flex items-center space-x-2">
                  <p className="text-gray-900">{driver.current_profile.license_expiry_date}</p>
                  {driver.current_profile.is_license_expired ? (
                    <span className="text-xs bg-red-100 text-red-800 px-2 py-1 rounded-full">Expired</span>
                  ) : driver.current_profile.days_until_license_expiry <= 30 ? (
                    <span className="text-xs bg-yellow-100 text-yellow-800 px-2 py-1 rounded-full">
                      {driver.current_profile.days_until_license_expiry} days left
                    </span>
                  ) : (
                    <span className="text-xs bg-green-100 text-green-800 px-2 py-1 rounded-full">Valid</span>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      ) : (
        <div className="card text-center py-8">
          <div className="text-4xl mb-2">📝</div>
          <h4 className="text-lg font-semibold text-gray-900 mb-2">No Profile Created</h4>
          <p className="text-gray-600">This driver hasn't created their profile yet.</p>
        </div>
      )}

      {/* Pending Profile */}
      {driver.pending_profile && (
        <div className="card border-l-4 border-l-yellow-400">
          <h4 className="text-lg font-semibold text-gray-900 mb-4">
            Pending Profile (v{driver.pending_profile.version_number})
            <span className="ml-2 text-sm bg-yellow-100 text-yellow-800 px-2 py-1 rounded-full">
              {driver.pending_profile.status_display}
            </span>
          </h4>
          <p className="text-gray-600 mb-4">A new profile version is waiting for approval.</p>
          <div className="text-sm text-gray-500">
            Submitted: {driver.pending_profile.submitted_at ? 
              new Date(driver.pending_profile.submitted_at).toLocaleDateString() : 'N/A'}
          </div>
        </div>
      )}

      {/* License Status */}
      <div className="card">
        <h4 className="text-lg font-semibold text-gray-900 mb-4">License Status</h4>
        <div className="flex items-center space-x-3">
          <div className={`px-3 py-2 rounded-full text-sm font-medium ${
            driver.license_status.status === 'valid' ? 'bg-green-100 text-green-800' :
            driver.license_status.status === 'expiring_soon' ? 'bg-yellow-100 text-yellow-800' :
            'bg-red-100 text-red-800'
          }`}>
            {driver.license_status.message}
          </div>
        </div>
      </div>
    </div>
  )
}

// Activity Tab
function ActivityTab({ driver, heatmapData, activityData, loading }: {
  driver: EnhancedDriver
  heatmapData: any
  activityData: any
  loading: boolean
}) {
  if (loading) {
    return (
      <div className="space-y-6">
        <div className="card animate-pulse">
          <div className="h-48 bg-gray-200 rounded"></div>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Activity Statistics */}
      {activityData && (
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div className="card text-center">
            <div className="text-2xl font-bold text-blue-600">{activityData.stats.total_activities}</div>
            <div className="text-sm text-gray-600">Total Activities (30 days)</div>
          </div>
          <div className="card text-center">
            <div className="text-2xl font-bold text-green-600">{activityData.stats.daily_average}</div>
            <div className="text-sm text-gray-600">Daily Average</div>
          </div>
          <div className="card text-center">
            <div className="text-2xl font-bold text-purple-600">
              {activityData.stats.last_activity ? 
                new Date(activityData.stats.last_activity).toLocaleDateString() : 'N/A'}
            </div>
            <div className="text-sm text-gray-600">Last Activity</div>
          </div>
        </div>
      )}

      {/* Activity Heatmap */}
      <div className="card">
        <ActivityHeatmap data={heatmapData} />
      </div>

      {/* Recent Activities */}
      {activityData && activityData.recent_activities && (
        <div className="card">
          <h4 className="text-lg font-semibold text-gray-900 mb-4">Recent Activities</h4>
          <div className="space-y-3 max-h-64 overflow-y-auto">
            {activityData.recent_activities.map((activity: any) => (
              <div key={activity.id} className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
                <div>
                  <div className="font-medium text-gray-900">{activity.activity_type_display}</div>
                  <div className="text-sm text-gray-500">
                    {new Date(activity.created_at).toLocaleString()}
                  </div>
                </div>
                <div className="text-xs bg-gray-100 text-gray-600 px-2 py-1 rounded">
                  {activity.activity_type}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}

// History Tab
function HistoryTab({ driver }: { driver: EnhancedDriver }) {
  const [profileHistory, setProfileHistory] = useState<any[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    fetchProfileHistory()
  }, [driver.id])

  const fetchProfileHistory = async () => {
    try {
      setLoading(true)
      if (driver.current_profile) {
        const history = await apiService.getDriverProfileHistory(driver.current_profile.id)
        setProfileHistory(history)
      }
    } catch (error) {
      console.error('Failed to load profile history:', error)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="card animate-pulse">
            <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
            <div className="h-3 bg-gray-200 rounded w-1/2"></div>
          </div>
        ))}
      </div>
    )
  }

  if (profileHistory.length === 0) {
    return (
      <div className="card text-center py-8">
        <div className="text-4xl mb-2">📋</div>
        <h4 className="text-lg font-semibold text-gray-900 mb-2">No Changes Yet</h4>
        <p className="text-gray-600">No profile changes have been tracked for this driver.</p>
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {profileHistory.map((change: any) => (
        <div key={change.id} className="card">
          <div className="flex items-start justify-between">
            <div>
              <div className="font-medium text-gray-900">{change.field_name}</div>
              <div className="text-sm text-gray-600 mt-1">
                <span className="text-red-600">- {change.old_value || 'Empty'}</span>
                <br />
                <span className="text-green-600">+ {change.new_value}</span>
              </div>
            </div>
            <div className="text-right">
              <div className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">
                {change.change_type_display}
              </div>
              <div className="text-xs text-gray-500 mt-1">
                {new Date(change.created_at).toLocaleDateString()}
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  )
}