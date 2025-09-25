'use client'

import { useState, useEffect } from 'react'
import { apiClient, ApiError } from '@/lib/api/client'
import { DriverProfile, DriverProfileStatus, CreateDriverProfileRequest, UpdateDriverProfileRequest, LicenseClass } from '@/lib/types/api'
import { useAuth } from '@/lib/auth/AuthContext'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input } from '@/components/ui/FormField'
import { PhotoUpload } from '@/components/ui/PhotoUpload'

// Enhanced Multi-select License Class Component with Search
function LicenseClassSelect({ 
  value, 
  onChange, 
  licenseClasses
}: { 
  value: string[]
  onChange: (value: string[]) => void
  licenseClasses: LicenseClass[]
}) {
  const [searchTerm, setSearchTerm] = useState('')
  const [isOpen, setIsOpen] = useState(false)

  // Filter available license classes (exclude already selected)
  const availableClasses = licenseClasses.filter(lc => 
    !value.includes(lc.id) && 
    lc.name.toLowerCase().includes(searchTerm.toLowerCase())
  )

  // Get selected license class details
  const selectedClasses = licenseClasses.filter(lc => value.includes(lc.id))

  const handleSelect = (licenseClassId: string) => {
    onChange([...value, licenseClassId])
    setSearchTerm('')
    setIsOpen(false)
  }

  const handleRemove = (licenseClassId: string) => {
    onChange(value.filter(id => id !== licenseClassId))
  }

  return (
    <div className="space-y-2">
      {/* Selected License Classes Tags */}
      {selectedClasses.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {selectedClasses.map((licenseClass) => (
            <div 
              key={licenseClass.id}
              className="inline-flex items-center bg-amber-100 text-amber-800 text-sm font-medium px-3 py-1 rounded-full"
            >
              <span>{licenseClass.name}</span>
              <button
                type="button"
                onClick={() => handleRemove(licenseClass.id)}
                className="ml-2 inline-flex items-center justify-center w-4 h-4 text-amber-600 hover:text-amber-800 hover:bg-amber-200 rounded-full transition-colors"
              >
                ×
              </button>
            </div>
          ))}
        </div>
      )}

      {/* Search Input */}
      <div className="relative">
        <input
          type="text"
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value)
            setIsOpen(true)
          }}
          onFocus={() => setIsOpen(true)}
          placeholder="Search and select license classes..."
          className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-amber-500 focus:border-transparent"
        />
        
        {/* Search Results Dropdown */}
        {isOpen && (
          <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-48 overflow-y-auto">
            {availableClasses.length > 0 ? (
              <div className="py-1">
                {availableClasses.map((licenseClass) => (
                  <button
                    key={licenseClass.id}
                    type="button"
                    onClick={() => handleSelect(licenseClass.id)}
                    className="w-full text-left px-4 py-2 hover:bg-gray-100 transition-colors"
                  >
                    <div className="font-medium text-sm text-gray-900">{licenseClass.name}</div>
                    {licenseClass.description && (
                      <div className="text-xs text-gray-600 mt-1">{licenseClass.description}</div>
                    )}
                  </button>
                ))}
              </div>
            ) : searchTerm ? (
              <div className="px-4 py-3 text-sm text-gray-500">
                No license classes found matching "{searchTerm}"
              </div>
            ) : (
              <div className="px-4 py-3 text-sm text-gray-500">
                {value.length === licenseClasses.length 
                  ? "All license classes selected" 
                  : "Type to search license classes"
                }
              </div>
            )}
          </div>
        )}
      </div>

      {/* Click outside to close dropdown */}
      {isOpen && (
        <div 
          className="fixed inset-0 z-0" 
          onClick={() => setIsOpen(false)}
        />
      )}


      {/* Helper Text */}
      {value.length > 0 && (
        <p className="text-sm text-gray-500">
          {value.length} license class{value.length !== 1 ? 'es' : ''} selected
        </p>
      )}
    </div>
  )
}

export default function ProfilePage() {
  const { user: currentUser } = useAuth()
  const { showSuccess, showError } = useToast()

  // State management
  const [profile, setProfile] = useState<DriverProfile | null>(null)
  const [pendingProfile, setPendingProfile] = useState<DriverProfile | null>(null)
  const [isEditingPending, setIsEditingPending] = useState(false)
  const [licenseClasses, setLicenseClasses] = useState<LicenseClass[]>([])
  const [loading, setLoading] = useState(true)
  const [isEditing, setIsEditing] = useState(false)
  const [activeTab, setActiveTab] = useState<'profile' | 'activity' | 'history'>('profile')

  // Form state
  const [formData, setFormData] = useState({
    full_name: '',
    phone_number: '',
    id_number: '',
    license_number: '',
    license_expiry_date: '',
    license_classes: [] as string[],
    profile_photo: null as File | null,
    license_front_image: null as File | null,
    license_back_image: null as File | null,
    id_front_image: null as File | null,
    id_back_image: null as File | null
  })
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    fetchProfile()
    fetchLicenseClasses()
  }, [])

  const fetchProfile = async () => {
    try {
      setLoading(true)
      // Get the current user's driver profile using /me endpoint
      const profileData = await apiClient.getMyDriverProfile()
      setProfile(profileData)
      
      // Handle pending profile logic based on backend metadata
      let pendingData = null
      if (profileData.has_pending_version && profileData.pending_version_id) {
        // If showing approved profile but there's a pending, the pending is what user should edit
        // We'll need to fetch the pending profile specifically for editing
        try {
          pendingData = await apiClient.getDriverProfile(profileData.pending_version_id)
          setPendingProfile(pendingData)
        } catch (pendingError) {
          console.error('Failed to load pending profile:', pendingError)
          setPendingProfile(null)
        }
      } else {
        setPendingProfile(null)
      }

      // Initialize form data - use pending profile for editing if it exists, otherwise main profile
      const profileForForm = pendingData || profileData
      setFormData({
        full_name: profileForForm.full_name,
        phone_number: profileForForm.phone_number,
        id_number: profileForForm.id_number,
        license_number: profileForForm.license_number,
        license_expiry_date: profileForForm.license_expiry_date,
        license_classes: profileForForm.license_classes 
          ? profileForForm.license_classes.map((lc: any) => typeof lc === 'string' ? lc : lc.id)
          : [],
        profile_photo: null,
        license_front_image: null,
        license_back_image: null,
        id_front_image: null,
        id_back_image: null
      })
    } catch (error) {
      console.error('Failed to fetch profile:', error)
      if (error instanceof ApiError) {
        showError('Failed to load profile', error.detail || 'Please try again.')
      }
    } finally {
      setLoading(false)
    }
  }

  const fetchLicenseClasses = async () => {
    try {
      const response = await apiClient.getLicenseClasses()
      setLicenseClasses(response.results || [])
    } catch (error) {
      console.error('Failed to fetch license classes:', error)
      showError('Failed to load license classes', 'Please refresh the page to try again.')
      setLicenseClasses([])
    }
  }

  const handleInputChange = (field: string, value: string | string[] | File | null) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    if (formErrors[field]) {
      setFormErrors(prev => ({ ...prev, [field]: '' }))
    }
  }

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {}
    
    if (!formData.full_name?.trim()) errors.full_name = 'Full name is required'
    if (!formData.phone_number?.trim()) errors.phone_number = 'Phone number is required'
    if (!formData.id_number?.trim()) errors.id_number = 'ID number is required'
    if (!formData.license_number?.trim()) errors.license_number = 'License number is required'
    if (!formData.license_expiry_date) errors.license_expiry_date = 'License expiry date is required'
    if (!formData.license_classes || formData.license_classes.length === 0) {
      errors.license_classes = 'At least one license class is required'
    }
    
    setFormErrors(errors)
    return Object.keys(errors).length === 0
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    if (!validateForm()) {
      return
    }
    
    setIsSubmitting(true)
    setFormErrors({})
    
    try {
      const submitData = new FormData()
      submitData.append('full_name', formData.full_name)
      submitData.append('phone_number', formData.phone_number)
      submitData.append('id_number', formData.id_number)
      submitData.append('license_number', formData.license_number)
      submitData.append('license_expiry_date', formData.license_expiry_date)
      
      // Append license classes
      formData.license_classes.forEach(licenseClassId => {
        submitData.append('license_classes', licenseClassId)
      })
      
      // Append images if selected (but not if marked as REMOVED)
      if (formData.profile_photo && formData.profile_photo !== 'REMOVED') {
        submitData.append('profile_photo', formData.profile_photo)
      } else if (formData.profile_photo === 'REMOVED') {
        submitData.append('profile_photo', '') // Send empty to remove
      }
      
      if (formData.license_front_image && formData.license_front_image !== 'REMOVED') {
        submitData.append('license_front_image', formData.license_front_image)
      } else if (formData.license_front_image === 'REMOVED') {
        submitData.append('license_front_image', '') // Send empty to remove
      }
      
      if (formData.license_back_image && formData.license_back_image !== 'REMOVED') {
        submitData.append('license_back_image', formData.license_back_image)
      } else if (formData.license_back_image === 'REMOVED') {
        submitData.append('license_back_image', '') // Send empty to remove
      }
      
      if (formData.id_front_image && formData.id_front_image !== 'REMOVED') {
        submitData.append('id_front_image', formData.id_front_image)
      } else if (formData.id_front_image === 'REMOVED') {
        submitData.append('id_front_image', '') // Send empty to remove
      }
      
      if (formData.id_back_image && formData.id_back_image !== 'REMOVED') {
        submitData.append('id_back_image', formData.id_back_image)
      } else if (formData.id_back_image === 'REMOVED') {
        submitData.append('id_back_image', '') // Send empty to remove
      }

      if (profile) {
        // Update existing profile
        const updatedProfile = await apiClient.updateDriverProfile(profile.id, submitData)
        setProfile(updatedProfile)
        showSuccess('Profile updated', 'Your profile has been updated and submitted for approval.')
      } else {
        // Create new profile
        const newProfile = await apiClient.createDriverProfile(submitData)
        setProfile(newProfile)
        showSuccess('Profile created', 'Your profile has been created and submitted for approval.')
      }
      
      setIsEditing(false)
      setIsEditingPending(false)
      fetchProfile() // Refresh profile data
    } catch (error) {
      console.error('Failed to save profile:', error)
      if (error instanceof ApiError) {
        const fieldErrors: Record<string, string> = {}
        error.getAllErrors().forEach(err => {
          fieldErrors[err.field] = err.message
        })
        setFormErrors(fieldErrors)
        
        if (!Object.keys(fieldErrors).length) {
          showError('Failed to save profile', error.detail || 'Please try again.')
        }
      } else {
        showError('Failed to save profile', 'An unexpected error occurred.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleSubmitProfile = async () => {
    // Use pending profile ID if there's a pending version, otherwise main profile ID
    const profileIdToSubmit = profile?.pending_version_id || profile?.id
    if (!profileIdToSubmit) return
    
    try {
      await apiClient.submitDriverProfile(profileIdToSubmit)
      showSuccess('Profile submitted!', 'Your profile has been submitted for admin approval.')
      fetchProfile() // Refresh to show new status
    } catch (error) {
      showError('Failed to submit profile', error instanceof Error ? error.message : 'Please try again.')
    }
  }

  const getStatusBadge = (status: string) => {
    const statusConfig = {
      draft: { color: 'bg-gray-100 text-gray-800', text: 'Draft' },
      pending: { color: 'bg-yellow-100 text-yellow-800', text: 'Pending Approval' },
      approved: { color: 'bg-green-100 text-green-800', text: 'Approved' },
      rejected: { color: 'bg-red-100 text-red-800', text: 'Rejected' },
      changes_requested: { color: 'bg-orange-100 text-orange-800', text: 'Changes Requested' }
    }
    
    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.draft
    
    return (
      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.color}`}>
        {config.text}
      </span>
    )
  }

  const isExpiringSoon = (profile: DriverProfile) => {
    // Use the backend's license_status determination
    return profile.license_status === 'expiring_soon'
  }

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="card animate-pulse">
          <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
          <div className="h-3 bg-gray-200 rounded w-1/2"></div>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Driver Profile</h1>
          <p className="text-gray-600">Manage your driver information and documents</p>
        </div>
        {profile && !isEditing && !isEditingPending && !profile.has_pending_version && (
          <div className="flex space-x-3">
            <button 
              onClick={() => {
                if (profile.has_pending_version) {
                  setIsEditingPending(true)
                } else {
                  setIsEditing(true)
                }
              }}
              className="bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white px-4 py-2 rounded-md flex items-center space-x-2 transition-all duration-200"
            >
              <span className="text-lg mr-2">✏️</span>
{profile.has_pending_version ? 'Edit Draft' : 'Edit Profile'}
            </button>
            
            {(profile.has_pending_version && profile.pending_status === 'draft') && (
              <button 
                onClick={handleSubmitProfile}
                className="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-md flex items-center space-x-2"
              >
                <span className="text-lg mr-2">📤</span>
                Submit for Approval
              </button>
            )}
          </div>
        )}
      </div>

      {/* Profile Status Alert */}
      {profile && (
        <div className="space-y-4">
          {/* Approved Profile Status */}
          {profile.status === 'approved' && !profile.has_pending_version && (
            <div className="bg-green-50 border border-green-200 rounded-lg p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-3">
                  <div className="text-green-500">
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-green-900">Profile Approved</p>
                    <p className="text-sm text-green-700">Your profile is approved and active.</p>
                  </div>
                </div>
                <div className="flex items-center space-x-3">
                  {getStatusBadge(profile.status)}
                </div>
              </div>
            </div>
          )}

          {/* Pending Changes Alert */}
          {profile.has_pending_version && (
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-3">
                  <div className="text-amber-500">
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-amber-900">Pending Changes</p>
                    <p className="text-sm text-amber-700">
                      You have {profile.pending_status === 'draft' ? 'draft' : 'pending'} changes waiting for approval.
                    </p>
                  </div>
                </div>
                <div className="flex items-center space-x-3">
                  <span className="inline-flex px-2 py-1 text-xs font-medium bg-amber-100 text-amber-800 rounded-full">
                    {profile.pending_status === 'draft' ? 'Draft' : 'Pending'}
                  </span>
                  <button 
                    onClick={() => setIsEditingPending(true)}
                    className="bg-amber-600 hover:bg-amber-700 text-white px-3 py-1 rounded text-sm"
                  >
                    Edit Pending Changes
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* First Time / No Profile */}
          {profile.is_first_profile && (
            <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
              <div className="flex items-center justify-between">
                <div className="flex items-center space-x-3">
                  <div className="text-amber-500">
                    <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div>
                    <p className="text-sm font-medium text-amber-900">Complete Your Profile</p>
                    <p className="text-sm text-amber-700">
                      {(profile.has_pending_version && profile.pending_status === 'pending') && 'Your changes are pending approval by an administrator.'}
                      {(profile.has_pending_version && profile.pending_status === 'draft') && 'Complete your changes and submit for approval.'}
                      {(profile.has_pending_version && profile.pending_status === 'changes_requested') && 'Changes have been requested. Please review the feedback and resubmit.'}
                      {(profile.has_pending_version && profile.pending_status === 'rejected') && 'Your changes were rejected. Please review the feedback and resubmit.'}
                      {!profile.has_pending_version && profile.status === 'draft' && 'Complete your profile and submit for approval.'}
                    </p>
                    
                    {/* Admin Feedback Display */}
                    {(profile.has_pending_version && (profile.pending_status === 'changes_requested' || profile.pending_status === 'rejected') && profile.approval_notes) && (
                      <div className="mt-3 p-3 bg-amber-50 border border-amber-200 rounded-md">
                        <div className="flex items-start">
                          <div className="flex-shrink-0">
                            <svg className="w-5 h-5 text-amber-400" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
                            </svg>
                          </div>
                          <div className="ml-3 flex-1">
                            <h4 className="text-sm font-medium text-amber-800">
                              {profile.pending_status === 'changes_requested' ? 'Changes Requested' : 'Feedback from Admin'}
                            </h4>
                            <p className="mt-1 text-sm text-amber-700 whitespace-pre-wrap">
                              {profile.approval_notes}
                            </p>
                            {profile.reviewed_at && (
                              <p className="mt-2 text-xs text-amber-600">
                                — {new Date(profile.reviewed_at).toLocaleDateString()}
                              </p>
                            )}
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                </div>
                <div className="flex items-center space-x-3">
                  {getStatusBadge(profile.has_pending_version ? profile.pending_status : profile.status)}
                  {(profile.has_pending_version && (profile.pending_status === 'draft' || profile.pending_status === 'changes_requested')) && (
                    <button 
                      onClick={handleSubmitProfile}
                      className="bg-amber-600 hover:bg-amber-700 text-white px-3 py-1 rounded text-sm"
                    >
                      Submit for Approval
                    </button>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* License Expiry Warning */}
      {profile && isExpiringSoon(profile) && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
          <div className="flex items-center space-x-3">
            <div className="text-amber-500">
              <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
              </svg>
            </div>
            <div>
              <p className="text-sm font-medium text-amber-900">License Expiring Soon</p>
              <p className="text-sm text-amber-700">
                Your license expires on {new Date(profile.license_expiry_date).toLocaleDateString()}. Please renew it soon.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Tab Navigation */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          {['profile', 'activity', 'history'].map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab as any)}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === tab
                  ? 'border-amber-500 text-amber-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab === 'profile' && '👤 Profile'}
              {tab === 'activity' && '📊 Activity'}
              {tab === 'history' && '📋 History'}
            </button>
          ))}
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'profile' && (
        <>
          {profile ? (
            isEditing || isEditingPending ? (
              <EditProfileForm 
                profile={isEditingPending && pendingProfile ? pendingProfile : profile}
                licenseClasses={licenseClasses}
                formData={formData}
                setFormData={setFormData}
                formErrors={formErrors}
                isSubmitting={isSubmitting}
                onSubmit={handleSubmit}
                onCancel={() => {
                  setIsEditing(false)
                  setIsEditingPending(false)
                }}
                isEditingPending={isEditingPending}
                onInputChange={handleInputChange}
              />
            ) : (
              <ViewProfile profile={profile} licenseClasses={licenseClasses} />
            )
          ) : (
            <CreateProfileForm 
              licenseClasses={licenseClasses}
              formData={formData}
              setFormData={setFormData}
              formErrors={formErrors}
              isSubmitting={isSubmitting}
              onSubmit={handleSubmit}
              onInputChange={handleInputChange}
            />
          )}
        </>
      )}

      {activeTab === 'activity' && (
        <ActivityTabComponent profile={profile} />
      )}

      {activeTab === 'history' && (
        <HistoryTab profile={profile} />
      )}
    </div>
  )
}

// View Profile Component
function ViewProfile({ profile, licenseClasses }: { profile: DriverProfile; licenseClasses: LicenseClass[] }) {
  const getStatusBadge = (status: string) => {
    const statusConfig = {
      draft: { color: 'bg-gray-100 text-gray-800', text: 'Draft' },
      pending: { color: 'bg-yellow-100 text-yellow-800', text: 'Pending Approval' },
      approved: { color: 'bg-green-100 text-green-800', text: 'Approved' },
      rejected: { color: 'bg-red-100 text-red-800', text: 'Rejected' },
      changes_requested: { color: 'bg-orange-100 text-orange-800', text: 'Changes Requested' }
    }
    
    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.draft
    
    return (
      <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${config.color}`}>
        {config.text}
      </span>
    )
  }

  return (
    <div className="space-y-6">
      {/* Driver's License Card */}
      <div className="max-w-4xl mx-auto">
        <div className="bg-gradient-to-r from-amber-500 to-orange-600 rounded-xl shadow-2xl p-8 text-white relative overflow-hidden">
          {/* Background Pattern */}
          <div className="absolute inset-0 opacity-10">
            <div className="absolute top-0 right-0 w-96 h-96 bg-white rounded-full transform translate-x-32 -translate-y-32"></div>
            <div className="absolute bottom-0 left-0 w-64 h-64 bg-white rounded-full transform -translate-x-16 translate-y-16"></div>
          </div>
          
          {/* Header */}
          <div className="relative z-10 flex items-center justify-between mb-6">
            <div>
              <h1 className="text-2xl font-bold">DRIVER LICENSE</h1>
              <p className="text-amber-100">Commercial Driver Profile</p>
            </div>
            <div className="flex items-center space-x-2">
              {getStatusBadge(profile.status)}
            </div>
          </div>

          {/* Main Content */}
          <div className="relative z-10 grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Photo Section */}
            <div className="lg:col-span-1 flex flex-col items-center">
              {profile.profile_photo ? (
                <img 
                  src={profile.profile_photo} 
                  alt="Profile" 
                  className="w-40 h-40 rounded-lg object-cover border-4 border-white shadow-lg"
                />
              ) : (
                <div className="w-40 h-40 bg-gradient-to-r from-gray-400 to-gray-600 rounded-lg flex items-center justify-center text-white text-6xl border-4 border-white shadow-lg">
                  👤
                </div>
              )}
              <div className="mt-4 text-center">
                <p className="text-xs text-amber-200">PHOTO</p>
              </div>
            </div>

            {/* Information Section */}
            <div className="lg:col-span-2 space-y-6">
              {/* Personal Information */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-3">
                  <div>
                    <label className="text-xs font-medium text-amber-100 uppercase tracking-wider">Full Name</label>
                    <p className="text-xl font-bold">{profile.full_name || 'Not provided'}</p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-amber-100 uppercase tracking-wider">ID Number</label>
                    <p className="text-lg font-mono">{profile.id_number || 'Not provided'}</p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-amber-100 uppercase tracking-wider">Phone</label>
                    <p className="text-lg font-mono">{profile.phone_number || 'Not provided'}</p>
                  </div>
                </div>

                <div className="space-y-3">
                  <div>
                    <label className="text-xs font-medium text-amber-100 uppercase tracking-wider">License Number</label>
                    <p className="text-lg font-mono font-bold">{profile.license_number || 'Not provided'}</p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-amber-100 uppercase tracking-wider">Expires</label>
                    <p className="text-lg font-bold">
                      {profile.license_expiry_date 
                        ? new Date(profile.license_expiry_date).toLocaleDateString('en-US', {
                            month: '2-digit',
                            day: '2-digit', 
                            year: 'numeric'
                          })
                        : 'Not provided'
                      }
                    </p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-amber-100 uppercase tracking-wider">Class</label>
                    <div className="flex flex-wrap gap-1">
                      {profile.license_classes && profile.license_classes.length > 0 ? (
                        profile.license_classes.map((licenseClassItem: any, index: number) => {
                          // Handle both object format and string ID format
                          const licenseClass = typeof licenseClassItem === 'object' 
                            ? licenseClassItem 
                            : licenseClasses.find(lc => lc.id === licenseClassItem)
                          const key = typeof licenseClassItem === 'object' ? licenseClassItem.id : licenseClassItem
                          
                          return (
                            <span key={key || index} className="bg-white text-amber-800 px-2 py-1 rounded text-sm font-bold">
                              {licenseClass?.name || (typeof licenseClassItem === 'string' ? licenseClassItem : 'Unknown')}
                            </span>
                          )
                        })
                      ) : (
                        <span className="text-amber-100">Not provided</span>
                      )}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* License Number Bar */}
          <div className="relative z-10 mt-8 pt-4 border-t border-amber-300">
            <div className="flex justify-between items-center text-sm">
              <span className="font-mono">{profile.license_number || 'LICENSE-NUMBER'}</span>
              <span>{profile.status === 'approved' ? '✓ VERIFIED' : '⏳ PENDING'}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Documents Section */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {/* License Front Document */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
          <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            📄 License (Front)
          </h3>
          {profile.license_front_image ? (
            <img 
              src={profile.license_front_image} 
              alt="License Front" 
              className="w-full h-48 object-cover rounded-lg border shadow-md"
            />
          ) : (
            <div className="w-full h-48 bg-gray-100 rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center">
              <div className="text-center">
                <div className="text-4xl text-gray-400 mb-2">📄</div>
                <p className="text-gray-500">No front license image</p>
              </div>
            </div>
          )}
        </div>

        {/* License Back Document */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
          <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            📄 License (Back)
          </h3>
          {profile.license_back_image ? (
            <img 
              src={profile.license_back_image} 
              alt="License Back" 
              className="w-full h-48 object-cover rounded-lg border shadow-md"
            />
          ) : (
            <div className="w-full h-48 bg-gray-100 rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center">
              <div className="text-center">
                <div className="text-4xl text-gray-400 mb-2">📄</div>
                <p className="text-gray-500">No back license image</p>
              </div>
            </div>
          )}
        </div>

        {/* ID Documents */}
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-4">
          <h3 className="text-lg font-semibold text-gray-900 mb-4 flex items-center">
            🆔 ID Documents
          </h3>
          <div className="space-y-4">
            {profile.id_front_image ? (
              <img 
                src={profile.id_front_image} 
                alt="ID Front" 
                className="w-full h-32 object-cover rounded-lg border shadow-md"
              />
            ) : (
              <div className="w-full h-32 bg-gray-100 rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center">
                <div className="text-center">
                  <div className="text-2xl text-gray-400 mb-1">🆔</div>
                  <p className="text-gray-500 text-sm">No front ID</p>
                </div>
              </div>
            )}
            
            {profile.id_back_image ? (
              <img 
                src={profile.id_back_image} 
                alt="ID Back" 
                className="w-full h-32 object-cover rounded-lg border shadow-md"
              />
            ) : (
              <div className="w-full h-32 bg-gray-100 rounded-lg border-2 border-dashed border-gray-300 flex items-center justify-center">
                <div className="text-center">
                  <div className="text-2xl text-gray-400 mb-1">🆔</div>
                  <p className="text-gray-500 text-sm">No back ID</p>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Activity Stats */}
      {profile.stats && (
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
          <h3 className="text-lg font-semibold text-gray-900 mb-4">Activity Overview</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-amber-50 rounded-lg p-4 text-center">
              <div className="text-2xl font-bold text-amber-600">{profile.stats.total_activities}</div>
              <div className="text-sm text-amber-600">Total Activities</div>
            </div>
            <div className="bg-green-50 rounded-lg p-4 text-center">
              <div className="text-2xl font-bold text-green-600">{profile.stats.profile_updates}</div>
              <div className="text-sm text-green-600">Profile Updates</div>
            </div>
            <div className="bg-purple-50 rounded-lg p-4 text-center">
              <div className="text-2xl font-bold text-purple-600">{profile.stats.recent_activity_count}</div>
              <div className="text-sm text-purple-600">Recent Activities</div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

// Create Profile Form Component  
function CreateProfileForm({ 
  licenseClasses, 
  formData, 
  setFormData, 
  formErrors, 
  isSubmitting, 
  onSubmit, 
  onInputChange 
}: any) {
  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <div className="text-center mb-6">
        <div className="w-20 h-20 bg-gradient-to-r from-amber-400 to-orange-500 rounded-full flex items-center justify-center text-white text-3xl mx-auto mb-4">
          👤
        </div>
        <h2 className="text-xl font-bold text-gray-900">Create Your Driver Profile</h2>
        <p className="text-gray-600">Complete your profile to get started</p>
      </div>
      
      <form onSubmit={onSubmit} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField label="Full Name" required errors={formErrors.full_name ? [formErrors.full_name] : []}>
            <Input
              type="text"
              value={formData.full_name}
              onChange={(e) => onInputChange('full_name', e.target.value)}
              placeholder="John Doe"
              errors={formErrors.full_name ? [formErrors.full_name] : []}
            />
          </FormField>

          <FormField label="Phone Number" required errors={formErrors.phone_number ? [formErrors.phone_number] : []}>
            <Input
              type="tel"
              value={formData.phone_number}
              onChange={(e) => onInputChange('phone_number', e.target.value)}
              placeholder="+1 (555) 123-4567"
              errors={formErrors.phone_number ? [formErrors.phone_number] : []}
            />
          </FormField>
          
          <FormField label="ID Number" required errors={formErrors.id_number ? [formErrors.id_number] : []}>
            <Input
              type="text"
              value={formData.id_number}
              onChange={(e) => onInputChange('id_number', e.target.value)}
              placeholder="12345678"
              errors={formErrors.id_number ? [formErrors.id_number] : []}
            />
          </FormField>

          <FormField label="License Number" required errors={formErrors.license_number ? [formErrors.license_number] : []}>
            <Input
              type="text"
              value={formData.license_number}
              onChange={(e) => onInputChange('license_number', e.target.value)}
              placeholder="DL123456789"
              errors={formErrors.license_number ? [formErrors.license_number] : []}
            />
          </FormField>
          
          <FormField label="License Expiry Date" required errors={formErrors.license_expiry_date ? [formErrors.license_expiry_date] : []}>
            <Input
              type="date"
              value={formData.license_expiry_date}
              onChange={(e) => onInputChange('license_expiry_date', e.target.value)}
              errors={formErrors.license_expiry_date ? [formErrors.license_expiry_date] : []}
            />
          </FormField>
          
          <FormField label="License Classes" required errors={formErrors.license_classes ? [formErrors.license_classes] : []}>
            <LicenseClassSelect
              value={formData.license_classes}
              onChange={(value) => onInputChange('license_classes', value)}
              licenseClasses={licenseClasses}
            />
          </FormField>
        </div>

        {/* Photo Upload Section */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
          <PhotoUpload
            label="Profile Photo"
            onFileSelect={(file) => onInputChange('profile_photo', file)}
            currentImage={formData.profile_photo ? URL.createObjectURL(formData.profile_photo) : undefined}
            onRemoveImage={() => onInputChange('profile_photo', null)}
          />
          
          <PhotoUpload
            label="License (Front)"
            onFileSelect={(file) => onInputChange('license_front_image', file)}
            currentImage={formData.license_front_image ? URL.createObjectURL(formData.license_front_image) : undefined}
            onRemoveImage={() => onInputChange('license_front_image', null)}
          />

          <PhotoUpload
            label="License (Back)"
            onFileSelect={(file) => onInputChange('license_back_image', file)}
            currentImage={formData.license_back_image ? URL.createObjectURL(formData.license_back_image) : undefined}
            onRemoveImage={() => onInputChange('license_back_image', null)}
          />
          
          <PhotoUpload
            label="ID (Front)"
            onFileSelect={(file) => onInputChange('id_front_image', file)}
            currentImage={formData.id_front_image ? URL.createObjectURL(formData.id_front_image) : undefined}
            onRemoveImage={() => onInputChange('id_front_image', null)}
          />

          <PhotoUpload
            label="ID (Back)"
            onFileSelect={(file) => onInputChange('id_back_image', file)}
            currentImage={formData.id_back_image ? URL.createObjectURL(formData.id_back_image) : undefined}
            onRemoveImage={() => onInputChange('id_back_image', null)}
          />
        </div>
        
        <div className="flex justify-end pt-4">
          <button 
            type="submit" 
            disabled={isSubmitting} 
            className="bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 disabled:opacity-50 text-white px-6 py-2 rounded-md flex items-center space-x-2 transition-all duration-200"
          >
            {isSubmitting && (
              <svg className="animate-spin -ml-1 mr-3 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            )}
            <span>{isSubmitting ? 'Creating Profile...' : 'Create Profile'}</span>
          </button>
        </div>
      </form>
    </div>
  )
}

// Edit Profile Form Component
function EditProfileForm({ 
  profile, 
  licenseClasses, 
  formData, 
  setFormData, 
  formErrors, 
  isSubmitting, 
  onSubmit, 
  onCancel, 
  isEditingPending, 
  onInputChange 
}: any) {
  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <h2 className="text-xl font-bold text-gray-900 mb-6">
        {isEditingPending ? 'Edit Pending Changes' : 'Edit Profile'}
      </h2>
      <p className="text-gray-600 mb-4">
        {isEditingPending 
          ? 'You are editing your pending changes. These changes are not yet approved and will need admin approval.'
          : 'Changes will be submitted for approval. Your current approved profile will remain active until changes are approved.'
        }
      </p>
      
      {/* Version Information */}
      {profile.version_number && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 mb-4">
          <div className="flex items-center space-x-2 text-sm text-amber-700">
            <span className="font-medium">Version:</span>
            <span>v{profile.version_number}</span>
            <span>•</span>
            <span className="capitalize">{profile.status}</span>
            {isEditingPending && (
              <>
                <span>•</span>
                <span className="font-medium text-amber-700">Editing Pending Changes</span>
              </>
            )}
          </div>
        </div>
      )}
      
      <form onSubmit={onSubmit} className="space-y-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <FormField label="Full Name" required errors={formErrors.full_name ? [formErrors.full_name] : []}>
            <Input
              type="text"
              value={formData.full_name}
              onChange={(e) => onInputChange('full_name', e.target.value)}
              placeholder="John Doe"
              errors={formErrors.full_name ? [formErrors.full_name] : []}
            />
          </FormField>

          <FormField label="Phone Number" required errors={formErrors.phone_number ? [formErrors.phone_number] : []}>
            <Input
              type="tel"
              value={formData.phone_number}
              onChange={(e) => onInputChange('phone_number', e.target.value)}
              placeholder="+1 (555) 123-4567"
              errors={formErrors.phone_number ? [formErrors.phone_number] : []}
            />
          </FormField>
          
          <FormField label="ID Number" required errors={formErrors.id_number ? [formErrors.id_number] : []}>
            <Input
              type="text"
              value={formData.id_number}
              onChange={(e) => onInputChange('id_number', e.target.value)}
              placeholder="12345678"
              errors={formErrors.id_number ? [formErrors.id_number] : []}
            />
          </FormField>

          <FormField label="License Number" required errors={formErrors.license_number ? [formErrors.license_number] : []}>
            <Input
              type="text"
              value={formData.license_number}
              onChange={(e) => onInputChange('license_number', e.target.value)}
              placeholder="DL123456789"
              errors={formErrors.license_number ? [formErrors.license_number] : []}
            />
          </FormField>
          
          <FormField label="License Expiry Date" required errors={formErrors.license_expiry_date ? [formErrors.license_expiry_date] : []}>
            <Input
              type="date"
              value={formData.license_expiry_date}
              onChange={(e) => onInputChange('license_expiry_date', e.target.value)}
              errors={formErrors.license_expiry_date ? [formErrors.license_expiry_date] : []}
            />
          </FormField>
          
          <FormField label="License Classes" required errors={formErrors.license_classes ? [formErrors.license_classes] : []}>
            <LicenseClassSelect
              value={formData.license_classes}
              onChange={(value) => onInputChange('license_classes', value)}
              licenseClasses={licenseClasses}
            />
          </FormField>
        </div>

        {/* Photo Upload Section */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-6">
          <PhotoUpload
            label="Profile Photo"
            currentImage={formData.profile_photo === 'REMOVED' ? undefined : (formData.profile_photo ? URL.createObjectURL(formData.profile_photo) : profile.profile_photo)}
            onFileSelect={(file) => onInputChange('profile_photo', file)}
            onRemoveImage={() => onInputChange('profile_photo', 'REMOVED')}
          />
          
          <PhotoUpload
            label="License (Front)"
            currentImage={formData.license_front_image ? (formData.license_front_image === 'REMOVED' ? undefined : URL.createObjectURL(formData.license_front_image)) : profile.license_front_image}
            onFileSelect={(file) => onInputChange('license_front_image', file)}
            onRemoveImage={() => onInputChange('license_front_image', 'REMOVED')}
          />

          <PhotoUpload
            label="License (Back)"
            currentImage={formData.license_back_image ? (formData.license_back_image === 'REMOVED' ? undefined : URL.createObjectURL(formData.license_back_image)) : profile.license_back_image}
            onFileSelect={(file) => onInputChange('license_back_image', file)}
            onRemoveImage={() => onInputChange('license_back_image', 'REMOVED')}
          />
          
          <PhotoUpload
            label="ID (Front)"
            currentImage={formData.id_front_image ? (formData.id_front_image === 'REMOVED' ? undefined : URL.createObjectURL(formData.id_front_image)) : profile.id_front_image}
            onFileSelect={(file) => onInputChange('id_front_image', file)}
            onRemoveImage={() => onInputChange('id_front_image', 'REMOVED')}
          />

          <PhotoUpload
            label="ID (Back)"
            currentImage={formData.id_back_image ? (formData.id_back_image === 'REMOVED' ? undefined : URL.createObjectURL(formData.id_back_image)) : profile.id_back_image}
            onFileSelect={(file) => onInputChange('id_back_image', file)}
            onRemoveImage={() => onInputChange('id_back_image', 'REMOVED')}
          />
        </div>
        
        <div className="flex justify-end space-x-3 pt-4">
          <button type="button" onClick={onCancel} className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50">
            Cancel
          </button>
          <button 
            type="submit" 
            disabled={isSubmitting} 
            className="bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 disabled:opacity-50 text-white px-6 py-2 rounded-md flex items-center space-x-2 transition-all duration-200"
          >
            {isSubmitting && (
              <svg className="animate-spin -ml-1 mr-3 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            )}
            <span>{isSubmitting ? 'Submitting Changes...' : 'Submit Changes'}</span>
          </button>
        </div>
      </form>
    </div>
  )
}

// Activity Tab Component
function ActivityTabComponent({ profile }: { profile: DriverProfile | null }) {
  if (!profile?.stats) {
    return (
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <div className="text-center py-8">
          <div className="text-gray-400 text-4xl mb-4">📊</div>
          <p className="text-gray-500">No activity data available</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-6">Driver Activity</h2>
        
        {/* Activity Stats */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
          <div className="bg-amber-50 rounded-lg p-4">
            <div className="text-2xl font-bold text-amber-600">{profile.stats.total_activities || 0}</div>
            <div className="text-sm text-amber-600">Total Activities</div>
          </div>
          <div className="bg-green-50 rounded-lg p-4">
            <div className="text-2xl font-bold text-green-600">{profile.stats.profile_updates || 0}</div>
            <div className="text-sm text-green-600">Profile Updates</div>
          </div>
          <div className="bg-purple-50 rounded-lg p-4">
            <div className="text-2xl font-bold text-purple-600">{profile.stats.recent_activity_count || 0}</div>
            <div className="text-sm text-purple-600">Recent Activities</div>
          </div>
        </div>
        
        {/* Recent Activity */}
        {profile.recent_activity?.activities && (
          <div>
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Activity</h3>
            <div className="space-y-3">
              {profile.recent_activity.activities.slice(0, 5).map((activity: any, index: number) => (
                <div key={index} className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
                  <div className="text-2xl">📝</div>
                  <div className="flex-1">
                    <p className="text-sm font-medium text-gray-900">{activity.activity_type}</p>
                    <p className="text-xs text-gray-500">{new Date(activity.created_at).toLocaleString()}</p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

// History Tab Component
function HistoryTab({ profile }: { profile: DriverProfile | null }) {
  return (
    <div className="space-y-6">
      <div className="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
        <h2 className="text-xl font-bold text-gray-900 mb-6">Profile History</h2>
        
        <div className="text-center py-8">
          <div className="text-gray-400 text-4xl mb-4">📋</div>
          <p className="text-gray-500">Profile history will be displayed here</p>
          <p className="text-gray-400 text-sm">Version: {profile?.version_number || 'N/A'}</p>
        </div>
      </div>
    </div>
  )
}