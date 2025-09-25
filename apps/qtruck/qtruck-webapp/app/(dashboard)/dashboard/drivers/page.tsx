'use client'

import { useState, useEffect, useCallback } from 'react'
import { apiClient, ApiError } from '@/lib/api/client'
import { DriverProfile, DriverProfileStatus, CreateDriverProfileRequest, DriverProfileListParams, LicenseClass } from '@/lib/types/api'
import { useAuth } from '@/lib/auth/AuthContext'
import { useToast } from '@/components/ui/Toast'
import { ConfirmDialog } from '@/components/ui/Dialog'

interface DriverProfileStats {
  pending: number
  approved: number
  rejected: number
  draft: number
  expiring_soon: number
  total: number
}

type TabType = 'pending' | 'approved' | 'rejected' | 'draft' | 'expiring' | 'all'

export default function DriverProfileManagementPage() {
  const { user: currentUser } = useAuth()
  const { showSuccess, showError } = useToast()

  // State management
  const [profiles, setProfiles] = useState<DriverProfile[]>([])
  const [licenseClasses, setLicenseClasses] = useState<LicenseClass[]>([])
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<TabType>('all')
  const [searchTerm, setSearchTerm] = useState('')
  const [includeStats, setIncludeStats] = useState(true)
  
  // Dialog states
  const [showCreateDialog, setShowCreateDialog] = useState(false)
  const [confirmAction, setConfirmAction] = useState<{
    open: boolean
    title: string
    message: string
    onConfirm: () => void
    variant: 'danger' | 'warning' | 'info'
  }>({
    open: false,
    title: '',
    message: '',
    onConfirm: () => {},
    variant: 'info'
  })
  const [feedbackDialog, setFeedbackDialog] = useState<{
    open: boolean
    profile: DriverProfile | null
    action: 'request_changes' | 'reject' | null
    feedback: string
  }>({
    open: false,
    profile: null,
    action: null,
    feedback: ''
  })
  const [licenseViewModal, setLicenseViewModal] = useState<{
    open: boolean
    profile: DriverProfile | null
  }>({
    open: false,
    profile: null
  })

  // Create profile form state
  const [createForm, setCreateForm] = useState<CreateDriverProfileRequest>({
    full_name: '',
    phone_number: '',
    id_number: '',
    license_number: '',
    license_expiry_date: '',
    license_classes: []
  })
  const [createLoading, setCreateLoading] = useState(false)
  const [createErrors, setCreateErrors] = useState<Record<string, string>>({})

  // Load driver profiles data
  const fetchDriverProfiles = useCallback(async () => {
    try {
      setLoading(true)
      const params: DriverProfileListParams = { 
        page_size: 1000,
        include: includeStats ? 'stats,activity' : undefined
      }
      
      if (searchTerm.trim()) {
        params.search = searchTerm.trim()
      }

      // Add license expiring filter for expiring tab
      if (activeTab === 'expiring') {
        params.license_expiring = 30 // Next 30 days
      }

      const response = await apiClient.getDriverProfiles(params)
      setProfiles(response.results || [])
    } catch (error) {
      console.error('Failed to fetch driver profiles:', error)
      if (error instanceof ApiError) {
        showError('Failed to load driver profiles', error.detail || 'Please try again.')
      } else {
        showError('Failed to load driver profiles', 'An unexpected error occurred.')
      }
    } finally {
      setLoading(false)
    }
  }, [searchTerm, activeTab, includeStats])

  // Load license classes
  const fetchLicenseClasses = useCallback(async () => {
    try {
      const response = await apiClient.getLicenseClasses()
      setLicenseClasses(response.results || [])
    } catch (error) {
      console.error('Failed to fetch license classes:', error)
    }
  }, [])

  // Calculate stats
  const stats: DriverProfileStats = profiles.reduce(
    (acc, profile) => {
      if (profile.status === 'pending') acc.pending++
      else if (profile.status === 'approved') acc.approved++
      else if (profile.status === 'rejected') acc.rejected++
      else if (profile.status === 'draft') acc.draft++

      // Check if license is expiring soon (within 30 days)
      const expiryDate = new Date(profile.license_expiry_date)
      const thirtyDaysFromNow = new Date()
      thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30)
      if (expiryDate <= thirtyDaysFromNow && expiryDate >= new Date()) {
        acc.expiring_soon++
      }

      acc.total++
      return acc
    },
    { pending: 0, approved: 0, rejected: 0, draft: 0, expiring_soon: 0, total: 0 }
  )

  // Filter profiles by active tab
  const getFilteredProfiles = (): DriverProfile[] => {
    let filtered = profiles

    switch (activeTab) {
      case 'pending':
        filtered = profiles.filter(p => p.status === 'pending')
        break
      case 'approved':
        filtered = profiles.filter(p => p.status === 'approved')
        break
      case 'rejected':
        filtered = profiles.filter(p => p.status === 'rejected')
        break
      case 'draft':
        filtered = profiles.filter(p => p.status === 'draft')
        break
      case 'expiring':
        const thirtyDaysFromNow = new Date()
        thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30)
        filtered = profiles.filter(p => {
          const expiryDate = new Date(p.license_expiry_date)
          return expiryDate <= thirtyDaysFromNow && expiryDate >= new Date()
        })
        break
      case 'all':
        filtered = profiles
        break
    }

    return filtered
  }

  // Profile actions
  const handleApproveProfile = async (profile: DriverProfile) => {
    try {
      await apiClient.approveDriverProfile(profile.id, { action: 'approve' })
      showSuccess('Profile approved', `${profile.full_name}'s profile has been approved.`)
      await fetchDriverProfiles()
    } catch (error) {
      console.error('Failed to approve profile:', error)
      if (error instanceof ApiError) {
        showError('Failed to approve profile', error.detail || 'Please try again.')
      }
    }
  }

  const handleRejectProfile = (profile: DriverProfile) => {
    setFeedbackDialog({
      open: true,
      profile: profile,
      action: 'reject',
      feedback: ''
    })
  }

  const handleRequestChanges = (profile: DriverProfile) => {
    setFeedbackDialog({
      open: true,
      profile: profile,
      action: 'request_changes',
      feedback: ''
    })
  }

  const handleSubmitFeedback = async () => {
    if (!feedbackDialog.profile || !feedbackDialog.action) return
    
    try {
      await apiClient.approveDriverProfile(feedbackDialog.profile.id, { 
        action: feedbackDialog.action,
        notes: feedbackDialog.feedback || 'No specific feedback provided.'
      })
      
      const actionText = feedbackDialog.action === 'request_changes' ? 'Changes requested' : 'Profile rejected'
      const successMessage = feedbackDialog.action === 'request_changes' 
        ? `${feedbackDialog.profile.full_name} has been notified to update their profile.`
        : `${feedbackDialog.profile.full_name}'s profile has been rejected.`
        
      showSuccess(actionText, successMessage)
      await fetchDriverProfiles()
      
      // Close dialog
      setFeedbackDialog({
        open: false,
        profile: null,
        action: null,
        feedback: ''
      })
    } catch (error) {
      console.error(`Failed to ${feedbackDialog.action}:`, error)
      if (error instanceof ApiError) {
        const errorText = feedbackDialog.action === 'request_changes' ? 'request changes' : 'reject profile'
        showError(`Failed to ${errorText}`, error.detail || 'Please try again.')
      }
    }
  }

  const handleDeleteProfile = (profile: DriverProfile) => {
    setConfirmAction({
      open: true,
      title: 'Delete Profile',
      message: `Are you sure you want to permanently delete ${profile.full_name}'s profile? This action cannot be undone.`,
      variant: 'danger',
      onConfirm: async () => {
        try {
          await apiClient.deleteDriverProfile(profile.id)
          showSuccess('Profile deleted', `${profile.full_name}'s profile has been permanently deleted.`)
          await fetchDriverProfiles()
        } catch (error) {
          console.error('Failed to delete profile:', error)
          if (error instanceof ApiError) {
            showError('Failed to delete profile', error.detail || 'Please try again.')
          }
        }
      }
    })
  }

  const handleCreateProfile = async (e: React.FormEvent) => {
    e.preventDefault()
    setCreateErrors({})
    setCreateLoading(true)

    try {
      await apiClient.createDriverProfile(createForm)
      showSuccess('Profile created', 'The driver profile has been created successfully.')
      setShowCreateDialog(false)
      setCreateForm({
        full_name: '',
        phone_number: '',
        id_number: '',
        license_number: '',
        license_expiry_date: '',
        license_classes: []
      })
      await fetchDriverProfiles()
    } catch (error) {
      console.error('Failed to create profile:', error)
      if (error instanceof ApiError) {
        const fieldErrors: Record<string, string> = {}
        error.getAllErrors().forEach(err => {
          fieldErrors[err.field] = err.message
        })
        setCreateErrors(fieldErrors)
        
        if (!Object.keys(fieldErrors).length) {
          showError('Failed to create profile', error.detail || 'Please try again.')
        }
      }
    } finally {
      setCreateLoading(false)
    }
  }

  // Effects
  useEffect(() => {
    fetchLicenseClasses()
  }, [fetchLicenseClasses])

  useEffect(() => {
    if (searchTerm) {
      const timer = setTimeout(() => {
        fetchDriverProfiles()
      }, 500)
      return () => clearTimeout(timer)
    } else {
      fetchDriverProfiles()
    }
  }, [fetchDriverProfiles, searchTerm])

  const filteredProfiles = getFilteredProfiles()

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Driver Profile Management</h1>
          <p className="text-gray-600">Manage driver profiles, approvals, and license verification</p>
        </div>
        <div className="flex items-center space-x-4">
          <button
            onClick={() => setShowCreateDialog(true)}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white px-4 py-2 rounded-md flex items-center space-x-2 transition-all duration-200"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            <span>Create Profile</span>
          </button>
          <button
            onClick={fetchDriverProfiles}
            disabled={loading}
            className="bg-gray-100 hover:bg-gray-200 text-gray-700 px-4 py-2 rounded-md flex items-center space-x-2"
          >
            <svg className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <span>Refresh</span>
          </button>
        </div>
      </div>

      {/* Stats Dashboard */}
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        <StatsCard title="Pending" value={stats.pending} color="yellow" icon="⏳" />
        <StatsCard title="Approved" value={stats.approved} color="green" icon="✅" />
        <StatsCard title="Rejected" value={stats.rejected} color="red" icon="❌" />
        <StatsCard title="Expiring Soon" value={stats.expiring_soon} color="slate" icon="⚠️" />
        <StatsCard title="Total" value={stats.total} color="blue" icon="👥" />
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          {[
            { key: 'all' as TabType, label: 'All Profiles', count: stats.total },
            { key: 'pending' as TabType, label: 'Pending Review', count: stats.pending },
            { key: 'approved' as TabType, label: 'Approved', count: stats.approved },
            { key: 'rejected' as TabType, label: 'Rejected', count: stats.rejected },
            { key: 'expiring' as TabType, label: 'Expiring Soon', count: stats.expiring_soon }
          ].map(tab => (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={`py-2 px-1 border-b-2 font-medium text-sm whitespace-nowrap ${
                activeTab === tab.key
                  ? 'border-slate-500 text-slate-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab.label}
              {tab.count > 0 && (
                <span className={`ml-2 py-0.5 px-2 rounded-full text-xs ${
                  activeTab === tab.key ? 'bg-slate-100 text-slate-600' : 'bg-gray-100 text-gray-600'
                }`}>
                  {tab.count}
                </span>
              )}
            </button>
          ))}
        </nav>
      </div>

      {/* Filters */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1">
          <input
            type="text"
            placeholder="Search profiles by name, license number, or phone..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
          />
        </div>
        <div className="flex items-center space-x-2">
          <input
            type="checkbox"
            id="includeStats"
            checked={includeStats}
            onChange={(e) => setIncludeStats(e.target.checked)}
            className="h-4 w-4 text-slate-600 focus:ring-slate-500 border-gray-300 rounded accent-slate-600"
          />
          <label htmlFor="includeStats" className="text-sm text-gray-700">
            Include activity stats
          </label>
        </div>
      </div>

      {/* Profiles List */}
      {loading ? (
        <div className="space-y-4">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="bg-white rounded-lg shadow p-6 animate-pulse">
              <div className="flex items-center space-x-4">
                <div className="w-16 h-16 bg-gray-200 rounded-full"></div>
                <div className="flex-1">
                  <div className="h-4 bg-gray-200 rounded w-1/3 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/4 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : filteredProfiles.length === 0 ? (
        <div className="bg-white rounded-lg shadow p-12 text-center">
          <div className="text-6xl mb-4">👤</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No profiles found</h3>
          <p className="text-gray-600">
            {searchTerm
              ? 'Try adjusting your search criteria.'
              : 'No profiles match the current tab filter.'
            }
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredProfiles.map(profile => (
            <DriverProfileCard
              key={profile.id}
              profile={profile}
              licenseClasses={licenseClasses}
              onApprove={() => handleApproveProfile(profile)}
              onReject={() => handleRejectProfile(profile)}
              onRequestChanges={() => handleRequestChanges(profile)}
              onDelete={() => handleDeleteProfile(profile)}
              onViewLicense={() => setLicenseViewModal({ open: true, profile })}
            />
          ))}
        </div>
      )}

      {/* Create Profile Dialog */}
      {showCreateDialog && (
        <CreateDriverProfileDialog
          form={createForm}
          onChange={setCreateForm}
          onSubmit={handleCreateProfile}
          onClose={() => setShowCreateDialog(false)}
          loading={createLoading}
          errors={createErrors}
          licenseClasses={licenseClasses}
        />
      )}

      {/* Confirmation Dialog */}
      <ConfirmDialog
        isOpen={confirmAction.open}
        onClose={() => setConfirmAction(prev => ({ ...prev, open: false }))}
        onConfirm={() => {
          confirmAction.onConfirm()
          setConfirmAction(prev => ({ ...prev, open: false }))
        }}
        title={confirmAction.title}
        message={confirmAction.message}
        type={confirmAction.variant}
      />

      {/* Feedback Dialog */}
      {feedbackDialog.open && feedbackDialog.profile && (
        <div className="fixed inset-0 bg-gray-500 bg-opacity-75 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-md w-full mx-4">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-medium text-gray-900">
                {feedbackDialog.action === 'request_changes' ? 'Request Changes' : 'Reject Profile'}
              </h3>
              <button
                onClick={() => setFeedbackDialog({ open: false, profile: null, action: null, feedback: '' })}
                className="text-gray-400 hover:text-gray-600"
              >
                <span className="sr-only">Close</span>
                <svg className="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
            
            <div className="mb-4">
              <p className="text-sm text-gray-600 mb-3">
                {feedbackDialog.action === 'request_changes' 
                  ? `Provide feedback for ${feedbackDialog.profile.full_name} on what needs to be updated in their profile:`
                  : `Provide feedback for ${feedbackDialog.profile.full_name} on why their profile is being rejected:`
                }
              </p>
              <textarea
                value={feedbackDialog.feedback}
                onChange={(e) => setFeedbackDialog(prev => ({ ...prev, feedback: e.target.value }))}
                placeholder={feedbackDialog.action === 'request_changes' 
                  ? "Please update your phone number format and verify your license information..."
                  : "The submitted documents are not clear enough for verification..."
                }
                rows={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-md resize-none focus:outline-none focus:ring-2 focus:ring-slate-500 focus:border-transparent"
              />
            </div>
            
            <div className="flex justify-end space-x-3">
              <button
                onClick={() => setFeedbackDialog({ open: false, profile: null, action: null, feedback: '' })}
                className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500"
              >
                Cancel
              </button>
              <button
                onClick={handleSubmitFeedback}
                disabled={!feedbackDialog.feedback.trim()}
                className={`px-4 py-2 text-sm font-medium text-white rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 ${
                  feedbackDialog.action === 'request_changes'
                    ? 'bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 focus:ring-slate-500'
                    : 'bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 focus:ring-red-500'
                } ${
                  !feedbackDialog.feedback.trim() ? 'opacity-50 cursor-not-allowed' : ''
                }`}
              >
                {feedbackDialog.action === 'request_changes' ? 'Request Changes' : 'Reject Profile'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* License View Modal */}
      {licenseViewModal.open && licenseViewModal.profile && (
        <LicenseViewModal
          profile={licenseViewModal.profile}
          licenseClasses={licenseClasses}
          onClose={() => setLicenseViewModal({ open: false, profile: null })}
        />
      )}
    </div>
  )
}

// Helper Components
function StatsCard({ title, value, color, icon }: {
  title: string
  value: number
  color: string
  icon: string
}) {
  const colorClasses = {
    blue: 'from-slate-500 to-slate-600',
    green: 'from-green-500 to-green-600',
    red: 'from-red-500 to-red-600',
    orange: 'from-slate-500 to-slate-600',
    yellow: 'from-yellow-500 to-yellow-600',
    gray: 'from-gray-500 to-gray-600',
    slate: 'from-slate-500 to-slate-600'
  }

  return (
    <div className={`bg-gradient-to-r ${colorClasses[color as keyof typeof colorClasses]} text-white rounded-lg p-4`}>
      <div className="flex items-center space-x-3">
        <div className="text-2xl">{icon}</div>
        <div>
          <p className="text-2xl font-bold">{value}</p>
          <p className="text-white/80 text-sm">{title}</p>
        </div>
      </div>
    </div>
  )
}

function DriverProfileCard({ 
  profile, 
  licenseClasses,
  onApprove,
  onReject,
  onRequestChanges,
  onDelete,
  onViewLicense
}: {
  profile: DriverProfile
  licenseClasses: LicenseClass[]
  onApprove: () => void
  onReject: () => void
  onRequestChanges: () => void
  onDelete: () => void
  onViewLicense: () => void
}) {
  const getStatusColor = (status: DriverProfileStatus) => {
    switch (status) {
      case 'pending': return 'bg-yellow-100 text-yellow-800'
      case 'approved': return 'bg-green-100 text-green-800'
      case 'rejected': return 'bg-red-100 text-red-800'
      case 'draft': return 'bg-gray-100 text-gray-800'
      default: return 'bg-gray-100 text-gray-800'
    }
  }

  const isExpiringSoon = () => {
    const expiryDate = new Date(profile.license_expiry_date)
    const thirtyDaysFromNow = new Date()
    thirtyDaysFromNow.setDate(thirtyDaysFromNow.getDate() + 30)
    return expiryDate <= thirtyDaysFromNow && expiryDate >= new Date()
  }

  const getLicenseClassNames = () => {
    return profile.license_classes
      .map(classId => licenseClasses.find(lc => lc.id === classId)?.name)
      .filter(Boolean)
      .join(', ')
  }

  return (
    <div className="bg-white rounded-lg shadow hover:shadow-md transition-shadow p-6">
      <div className="flex items-start justify-between">
        <div className="flex items-start space-x-4">
          <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center">
            {profile.profile_photo ? (
              <img 
                src={profile.profile_photo} 
                alt={profile.full_name}
                className="w-16 h-16 rounded-full object-cover"
              />
            ) : (
              <div className="text-2xl">👤</div>
            )}
          </div>
          <div className="flex-1">
            <div className="flex items-center space-x-2 mb-1">
              <h3 className="text-lg font-semibold text-gray-900">{profile.full_name}</h3>
              {profile.has_pending_version && (
                <span className="bg-slate-100 text-slate-800 text-xs px-2 py-1 rounded-full font-medium">
                  UPDATE PENDING
                </span>
              )}
            </div>
            <div className="space-y-1 text-sm text-gray-600">
              <p>📱 {profile.phone_number}</p>
              <p>🆔 {profile.id_number}</p>
              <p>🪪 {profile.license_number}</p>
              <p>📅 Expires: {new Date(profile.license_expiry_date).toLocaleDateString()}</p>
              {getLicenseClassNames() && (
                <p>📋 Classes: {getLicenseClassNames()}</p>
              )}
              {profile.stats && (
                <p>📊 Activities: {profile.stats.total_activities}</p>
              )}
            </div>
            <div className="flex items-center space-x-2 mt-2">
              <span className={`text-xs px-2 py-1 rounded-full font-medium ${getStatusColor(profile.status)}`}>
                {profile.status.toUpperCase()}
              </span>
              {isExpiringSoon() && (
                <span className="bg-orange-100 text-orange-800 text-xs px-2 py-1 rounded-full font-medium">
                  EXPIRES SOON
                </span>
              )}
              <span className="text-xs text-gray-500">
                v{profile.version_number} • {new Date(profile.updated_at).toLocaleDateString()}
              </span>
            </div>
          </div>
        </div>
        
        {/* Previous Review Context for Resubmitted Profiles */}
        {profile.status === 'pending' && profile.approval_notes && profile.reviewed_at && (
          <div className="mt-4 p-3 bg-slate-50 border border-slate-200 rounded-md">
            <div className="flex items-start">
              <div className="flex-shrink-0">
                <svg className="w-4 h-4 text-slate-400 mt-0.5" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                </svg>
              </div>
              <div className="ml-3 flex-1">
                <h4 className="text-sm font-medium text-slate-800">
                  Previous Review (Resubmitted)
                </h4>
                <p className="mt-1 text-sm text-slate-700 whitespace-pre-wrap">
                  {profile.approval_notes}
                </p>
                <p className="mt-1 text-xs text-slate-600">
                  Reviewed on {new Date(profile.reviewed_at).toLocaleDateString()}
                </p>
              </div>
            </div>
          </div>
        )}

        <div className="flex items-center space-x-2">
          {profile.status === 'pending' && (
            <>
              <button
                onClick={onApprove}
                className="bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white px-3 py-1 rounded text-sm transition-all duration-200"
              >
                Approve
              </button>
              <button
                onClick={onRequestChanges}
                className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white px-3 py-1 rounded text-sm transition-all duration-200"
              >
                Request Changes
              </button>
              <button
                onClick={onReject}
                className="bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white px-3 py-1 rounded text-sm transition-all duration-200"
              >
                Reject
              </button>
            </>
          )}

          {(profile.status === 'rejected' || profile.status === 'draft') && (
            <button
              onClick={onDelete}
              className="bg-red-600 hover:bg-red-700 text-white px-3 py-1 rounded text-sm"
            >
              Delete
            </button>
          )}

          <button
            onClick={onViewLicense}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white px-3 py-1 rounded text-sm transition-all duration-200"
          >
            View License
          </button>
        </div>
      </div>
    </div>
  )
}

function LicenseViewModal({ profile, onClose, licenseClasses }: {
  profile: DriverProfile
  onClose: () => void
  licenseClasses: LicenseClass[]
}) {
  const [currentImageIndex, setCurrentImageIndex] = useState(0)
  const [isMaximized, setIsMaximized] = useState(false)
  
  const images = [
    { type: 'License Front', url: profile.license_front_image },
    { type: 'License Back', url: profile.license_back_image },
    { type: 'ID Front', url: profile.id_front_image },
    { type: 'ID Back', url: profile.id_back_image },
    { type: 'Profile Photo', url: profile.profile_photo }
  ].filter(img => img.url)

  const nextImage = () => {
    setCurrentImageIndex((prev) => (prev + 1) % images.length)
  }

  const prevImage = () => {
    setCurrentImageIndex((prev) => (prev - 1 + images.length) % images.length)
  }

  const goToImage = (index: number) => {
    setCurrentImageIndex(index)
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg max-w-4xl w-full max-h-[90vh] overflow-hidden">
        {/* Modal Header */}
        <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
          <div>
            <h3 className="text-lg font-semibold text-gray-900">{profile.full_name} - License & ID Details</h3>
            <p className="text-sm text-gray-600">License: {profile.license_number} | ID: {profile.id_number}</p>
          </div>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 p-2"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Modal Content */}
        <div className="p-6">
          {/* Profile Details */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-6">
            <div className="space-y-3">
              <h4 className="font-semibold text-gray-900">Personal Information</h4>
              <div className="space-y-2 text-sm">
                <p><span className="font-medium text-gray-700">Full Name:</span> {profile.full_name}</p>
                <p><span className="font-medium text-gray-700">Phone:</span> {profile.phone_number}</p>
                <p><span className="font-medium text-gray-700">ID Number:</span> {profile.id_number}</p>
              </div>
            </div>
            <div className="space-y-3">
              <h4 className="font-semibold text-gray-900">License Information</h4>
              <div className="space-y-2 text-sm">
                <p><span className="font-medium text-gray-700">License Number:</span> {profile.license_number}</p>
                <p><span className="font-medium text-gray-700">Expiry Date:</span> {new Date(profile.license_expiry_date).toLocaleDateString()}</p>
                <p><span className="font-medium text-gray-700">License Classes:</span> {
                  profile.license_classes
                    ?.map(classId => licenseClasses.find(lc => lc.id === classId)?.name)
                    .filter(Boolean)
                    .join(', ') || 'None'
                }</p>
                <p><span className="font-medium text-gray-700">Status:</span> 
                  <span className={`ml-1 text-xs px-2 py-1 rounded-full ${
                    profile.status === 'approved' ? 'bg-green-100 text-green-800' :
                    profile.status === 'pending' ? 'bg-yellow-100 text-yellow-800' :
                    profile.status === 'rejected' ? 'bg-red-100 text-red-800' :
                    'bg-gray-100 text-gray-800'
                  }`}>
                    {profile.status.toUpperCase()}
                  </span>
                </p>
              </div>
            </div>
          </div>

          {/* Image Carousel */}
          {images.length > 0 && (
            <div className="space-y-4">
              <h4 className="font-semibold text-gray-900">Documents & Photos</h4>
              
              {/* Main Image Display */}
              <div className="relative bg-gray-100 rounded-lg overflow-hidden" style={{ height: '400px' }}>
                <img
                  src={images[currentImageIndex].url}
                  alt={images[currentImageIndex].type}
                  className="w-full h-full object-contain cursor-pointer"
                  onClick={() => setIsMaximized(true)}
                />
                
                {/* Image Type Label */}
                <div className="absolute top-4 left-4 bg-black bg-opacity-75 text-white px-3 py-1 rounded-md text-sm">
                  {images[currentImageIndex].type}
                </div>
                
                {/* Maximize Button */}
                <button
                  onClick={() => setIsMaximized(true)}
                  className="absolute top-4 right-4 bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-2 rounded-md transition-all"
                  title="Click to maximize"
                >
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
                  </svg>
                </button>

                {/* Navigation Arrows */}
                {images.length > 1 && (
                  <>
                    <button
                      onClick={prevImage}
                      className="absolute left-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-2 rounded-full transition-all"
                    >
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                      </svg>
                    </button>
                    <button
                      onClick={nextImage}
                      className="absolute right-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-2 rounded-full transition-all"
                    >
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                      </svg>
                    </button>
                  </>
                )}
              </div>

              {/* Image Thumbnails */}
              {images.length > 1 && (
                <div className="flex space-x-2 overflow-x-auto pb-2">
                  {images.map((image, index) => (
                    <button
                      key={index}
                      onClick={() => goToImage(index)}
                      className={`flex-shrink-0 relative overflow-hidden rounded-lg border-2 transition-all ${
                        index === currentImageIndex ? 'border-slate-500' : 'border-gray-300 hover:border-gray-400'
                      }`}
                      style={{ width: '80px', height: '60px' }}
                    >
                      <img
                        src={image.url}
                        alt={image.type}
                        className="w-full h-full object-cover"
                      />
                      <div className="absolute bottom-0 left-0 right-0 bg-black bg-opacity-75 text-white text-xs px-1 py-0.5 truncate">
                        {image.type}
                      </div>
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Close Button */}
          <div className="flex justify-end mt-6">
            <button
              onClick={onClose}
              className="px-6 py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-md transition-colors"
            >
              Close
            </button>
          </div>
        </div>
      </div>

      {/* Maximized Image Modal */}
      {isMaximized && (
        <div className="fixed inset-0 bg-black bg-opacity-95 flex items-center justify-center z-[60]" onClick={() => setIsMaximized(false)}>
          <div className="relative max-w-[95vw] max-h-[95vh] flex items-center justify-center">
            <img
              src={images[currentImageIndex].url}
              alt={images[currentImageIndex].type}
              className="max-w-full max-h-full object-contain"
              onClick={(e) => e.stopPropagation()}
            />
            
            {/* Close Button */}
            <button
              onClick={() => setIsMaximized(false)}
              className="absolute top-4 right-4 bg-black bg-opacity-75 hover:bg-opacity-100 text-white p-3 rounded-full transition-all"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
            
            {/* Image Type Label */}
            <div className="absolute bottom-4 left-4 bg-black bg-opacity-75 text-white px-4 py-2 rounded-md">
              {images[currentImageIndex].type}
            </div>
            
            {/* Navigation in Maximized View */}
            {images.length > 1 && (
              <>
                <button
                  onClick={(e) => { e.stopPropagation(); prevImage(); }}
                  className="absolute left-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-3 rounded-full transition-all"
                >
                  <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
                  </svg>
                </button>
                <button
                  onClick={(e) => { e.stopPropagation(); nextImage(); }}
                  className="absolute right-4 top-1/2 transform -translate-y-1/2 bg-black bg-opacity-50 hover:bg-opacity-75 text-white p-3 rounded-full transition-all"
                >
                  <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                  </svg>
                </button>
              </>
            )}
            
            {/* Instructions */}
            <div className="absolute bottom-4 right-4 bg-black bg-opacity-75 text-white px-4 py-2 rounded-md text-sm">
              Click anywhere to close
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

function CreateDriverProfileDialog({
  form,
  onChange,
  onSubmit,
  onClose,
  loading,
  errors,
  licenseClasses
}: {
  form: CreateDriverProfileRequest
  onChange: (form: CreateDriverProfileRequest) => void
  onSubmit: (e: React.FormEvent) => void
  onClose: () => void
  loading: boolean
  errors: Record<string, string>
  licenseClasses: LicenseClass[]
}) {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="px-6 py-4 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold text-gray-900">Create Driver Profile</h3>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
            >
              <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        </div>
        
        <form onSubmit={onSubmit} className="p-6 space-y-4">
          {errors.general && (
            <div className="bg-red-50 border border-red-200 rounded-md p-3 text-red-700 text-sm">
              {errors.general}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Full Name
            </label>
            <input
              type="text"
              value={form.full_name}
              onChange={(e) => onChange({ ...form, full_name: e.target.value })}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                errors.full_name ? 'border-red-300' : 'border-gray-300'
              }`}
              required
            />
            {errors.full_name && (
              <p className="text-red-600 text-xs mt-1">{errors.full_name}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Phone Number
              </label>
              <input
                type="tel"
                value={form.phone_number}
                onChange={(e) => onChange({ ...form, phone_number: e.target.value })}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                  errors.phone_number ? 'border-red-300' : 'border-gray-300'
                }`}
                required
              />
              {errors.phone_number && (
                <p className="text-red-600 text-xs mt-1">{errors.phone_number}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                ID Number
              </label>
              <input
                type="text"
                value={form.id_number}
                onChange={(e) => onChange({ ...form, id_number: e.target.value })}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                  errors.id_number ? 'border-red-300' : 'border-gray-300'
                }`}
                required
              />
              {errors.id_number && (
                <p className="text-red-600 text-xs mt-1">{errors.id_number}</p>
              )}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                License Number
              </label>
              <input
                type="text"
                value={form.license_number}
                onChange={(e) => onChange({ ...form, license_number: e.target.value })}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                  errors.license_number ? 'border-red-300' : 'border-gray-300'
                }`}
                required
              />
              {errors.license_number && (
                <p className="text-red-600 text-xs mt-1">{errors.license_number}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                License Expiry Date
              </label>
              <input
                type="date"
                value={form.license_expiry_date}
                onChange={(e) => onChange({ ...form, license_expiry_date: e.target.value })}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                  errors.license_expiry_date ? 'border-red-300' : 'border-gray-300'
                }`}
                required
              />
              {errors.license_expiry_date && (
                <p className="text-red-600 text-xs mt-1">{errors.license_expiry_date}</p>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              License Classes
            </label>
            <div className="grid grid-cols-2 gap-2 max-h-32 overflow-y-auto border border-gray-300 rounded-md p-2">
              {licenseClasses.map(licenseClass => (
                <label key={licenseClass.id} className="flex items-center space-x-2">
                  <input
                    type="checkbox"
                    checked={form.license_classes?.includes(licenseClass.id) || false}
                    onChange={(e) => {
                      const classes = form.license_classes || []
                      if (e.target.checked) {
                        onChange({ ...form, license_classes: [...classes, licenseClass.id] })
                      } else {
                        onChange({ ...form, license_classes: classes.filter(c => c !== licenseClass.id) })
                      }
                    }}
                    className="h-4 w-4 text-slate-600 focus:ring-slate-500 border-gray-300 rounded accent-slate-600"
                  />
                  <span className="text-sm text-gray-700">{licenseClass.name}</span>
                </label>
              ))}
            </div>
          </div>

          <div className="flex justify-end space-x-3 pt-4">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50"
              disabled={loading}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={loading}
              className="px-4 py-2 bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white rounded-md disabled:opacity-50 flex items-center space-x-2 transition-all duration-200"
            >
              {loading && (
                <svg className="animate-spin -ml-1 mr-3 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
              )}
              <span>{loading ? 'Creating...' : 'Create Profile'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}