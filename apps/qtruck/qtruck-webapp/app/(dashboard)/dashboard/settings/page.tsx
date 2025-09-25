'use client'

import { useState, useEffect } from 'react'
import { apiClient, ApiError } from '@/lib/api/client'
import { useAuth } from '@/lib/auth/AuthContext'
import { useToast } from '@/components/ui/Toast'
import { ConfirmDialog } from '@/components/ui/Dialog'
import type { 
  SystemSettings, 
  UpdateSystemSettingsRequest, 
  LicenseClass, 
  CreateLicenseClassRequest,
  UpdateLicenseClassRequest,
  UserType 
} from '@/lib/types/api'

export default function SettingsPage() {
  const { user } = useAuth()
  const { showSuccess, showError } = useToast()
  
  // Tab management
  const [activeTab, setActiveTab] = useState<'system' | 'licenses'>('system')
  
  // System Settings State
  const [systemSettings, setSystemSettings] = useState<SystemSettings | null>(null)
  const [systemSettingsLoading, setSystemSettingsLoading] = useState(true)
  const [systemSettingsSaving, setSystemSettingsSaving] = useState(false)
  
  // License Classes State
  const [licenseClasses, setLicenseClasses] = useState<LicenseClass[]>([])
  const [licenseClassesLoading, setLicenseClassesLoading] = useState(true)
  const [licenseFormData, setLicenseFormData] = useState<CreateLicenseClassRequest>({
    name: '',
    description: ''
  })
  const [editingLicense, setEditingLicense] = useState<LicenseClass | null>(null)
  const [showAddLicenseDialog, setShowAddLicenseDialog] = useState(false)
  const [licenseOperationLoading, setLicenseOperationLoading] = useState(false)
  
  // Confirm Dialog State
  const [confirmDialog, setConfirmDialog] = useState<{
    isOpen: boolean
    title: string
    message: string
    onConfirm: () => void | Promise<void>
    type: 'danger' | 'warning' | 'info'
  }>({
    isOpen: false,
    title: '',
    message: '',
    onConfirm: () => {},
    type: 'info'
  })

  useEffect(() => {
    if (activeTab === 'system') {
      fetchSystemSettings()
    } else if (activeTab === 'licenses') {
      fetchLicenseClasses()
    }
  }, [activeTab])

  // System Settings Functions
  const fetchSystemSettings = async () => {
    try {
      setSystemSettingsLoading(true)
      const response = await apiClient.getSystemSettings()
      
      // Get the first (and likely only) system settings record
      if (response.results && response.results.length > 0) {
        setSystemSettings(response.results[0])
      } else {
        showError('No system settings found', 'Please contact administrator.')
      }
    } catch (error) {
      console.error('Failed to load system settings:', error)
      if (error instanceof ApiError) {
        showError('Failed to load system settings', error.detail || 'Please try again.')
      } else {
        showError('Failed to load system settings', 'An unexpected error occurred.')
      }
    } finally {
      setSystemSettingsLoading(false)
    }
  }

  const updateSystemSettings = async (updatedData: UpdateSystemSettingsRequest) => {
    if (!systemSettings) return
    
    try {
      setSystemSettingsSaving(true)
      const updated = await apiClient.updateSystemSettings(systemSettings.id, updatedData)
      setSystemSettings(updated)
      showSuccess('Settings saved', 'System settings have been updated successfully.')
    } catch (error) {
      console.error('Failed to update system settings:', error)
      if (error instanceof ApiError) {
        showError('Failed to save settings', error.detail || 'Please try again.')
      } else {
        showError('Failed to save settings', 'An unexpected error occurred.')
      }
    } finally {
      setSystemSettingsSaving(false)
    }
  }

  // License Classes Functions
  const fetchLicenseClasses = async () => {
    try {
      setLicenseClassesLoading(true)
      const response = await apiClient.getLicenseClasses()
      setLicenseClasses(response.results || [])
    } catch (error) {
      console.error('Failed to load license classes:', error)
      if (error instanceof ApiError) {
        showError('Failed to load license classes', error.detail || 'Please try again.')
      } else {
        showError('Failed to load license classes', 'An unexpected error occurred.')
      }
    } finally {
      setLicenseClassesLoading(false)
    }
  }

  const handleAddLicense = () => {
    setLicenseFormData({ name: '', description: '' })
    setEditingLicense(null)
    setShowAddLicenseDialog(true)
  }

  const handleEditLicense = (license: LicenseClass) => {
    setLicenseFormData({ name: license.name, description: license.description })
    setEditingLicense(license)
    setShowAddLicenseDialog(true)
  }

  const handleSaveLicense = async () => {
    if (!licenseFormData.name.trim()) {
      showError('Validation Error', 'License name is required.')
      return
    }

    try {
      setLicenseOperationLoading(true)
      
      if (editingLicense) {
        // Update existing license
        const updated = await apiClient.updateLicenseClass(editingLicense.id, licenseFormData)
        setLicenseClasses(prev => prev.map(lc => lc.id === updated.id ? updated : lc))
        showSuccess('License updated', `License "${updated.name}" has been updated successfully.`)
      } else {
        // Create new license
        const created = await apiClient.createLicenseClass(licenseFormData)
        setLicenseClasses(prev => [...prev, created])
        showSuccess('License created', `License "${created.name}" has been created successfully.`)
      }
      
      setShowAddLicenseDialog(false)
      setEditingLicense(null)
    } catch (error) {
      console.error('Failed to save license class:', error)
      if (error instanceof ApiError) {
        showError(editingLicense ? 'Failed to update license' : 'Failed to create license', 
                  error.detail || 'Please try again.')
      } else {
        showError(editingLicense ? 'Failed to update license' : 'Failed to create license', 
                  'An unexpected error occurred.')
      }
    } finally {
      setLicenseOperationLoading(false)
    }
  }

  const handleDeleteLicense = (license: LicenseClass) => {
    setConfirmDialog({
      isOpen: true,
      title: 'Delete License Class',
      message: `Are you sure you want to delete the license class "${license.name}"? This action cannot be undone.`,
      type: 'danger',
      onConfirm: async () => {
        try {
          await apiClient.deleteLicenseClass(license.id)
          setLicenseClasses(prev => prev.filter(lc => lc.id !== license.id))
          showSuccess('License deleted', `License "${license.name}" has been deleted successfully.`)
        } catch (error) {
          console.error('Failed to delete license class:', error)
          if (error instanceof ApiError) {
            showError('Failed to delete license', error.detail || 'Please try again.')
          } else {
            showError('Failed to delete license', 'An unexpected error occurred.')
          }
        }
      }
    })
  }

  const handleSystemSettingChange = (key: keyof UpdateSystemSettingsRequest, value: any) => {
    if (!systemSettings) return
    
    const updatedSettings = { ...systemSettings, [key]: value }
    setSystemSettings(updatedSettings)
  }

  const handleSaveSystemSettings = () => {
    if (!systemSettings) return
    
    const updateData: UpdateSystemSettingsRequest = {
      tester_registration_enabled: systemSettings.tester_registration_enabled,
      license_expiry_warning_days: systemSettings.license_expiry_warning_days,
      auto_approve_user_types: systemSettings.auto_approve_user_types
    }
    
    updateSystemSettings(updateData)
  }

  if (!user || user.user_type !== 'admin') {
    return (
      <div className="flex items-center justify-center h-96">
        <div className="text-center">
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Access Denied</h2>
          <p className="text-gray-600">You need administrator privileges to access this page.</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-bold text-gray-900">System Settings</h1>
        <p className="text-gray-600">Manage system configuration and license classes</p>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('system')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'system'
                ? 'border-slate-500 text-slate-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            System Settings
          </button>
          <button
            onClick={() => setActiveTab('licenses')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'licenses'
                ? 'border-slate-500 text-slate-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            License Classes
          </button>
        </nav>
      </div>

      {/* System Settings Tab */}
      {activeTab === 'system' && (
        <div className="bg-white shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <h3 className="text-lg leading-6 font-medium text-gray-900 mb-4">
              System Configuration
            </h3>
            
            {systemSettingsLoading ? (
              <div className="flex items-center justify-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-500"></div>
              </div>
            ) : systemSettings ? (
              <div className="space-y-6">
                {/* Tester Registration */}
                <div className="flex items-center justify-between">
                  <div>
                    <label className="text-sm font-medium text-gray-700">
                      Tester Registration
                    </label>
                    <p className="text-sm text-gray-500">
                      Allow new users to register with tester role
                    </p>
                  </div>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input
                      type="checkbox"
                      className="sr-only peer"
                      checked={systemSettings.tester_registration_enabled}
                      onChange={(e) => handleSystemSettingChange('tester_registration_enabled', e.target.checked)}
                    />
                    <div className="w-11 h-6 bg-gray-200 peer-focus:outline-none peer-focus:ring-4 peer-focus:ring-slate-300 rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-slate-600"></div>
                  </label>
                </div>

                {/* License Expiry Warning Days */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    License Expiry Warning (Days)
                  </label>
                  <div className="max-w-xs">
                    <input
                      type="number"
                      min="1"
                      max="365"
                      value={systemSettings.license_expiry_warning_days}
                      onChange={(e) => handleSystemSettingChange('license_expiry_warning_days', parseInt(e.target.value))}
                      className="block w-full border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 sm:text-sm"
                    />
                    <p className="mt-1 text-sm text-gray-500">
                      Days before license expiry to show warnings
                    </p>
                  </div>
                </div>

                {/* Auto-approve User Types */}
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-2">
                    Auto-approve User Types
                  </label>
                  <div className="space-y-2">
                    {(['admin', 'driver', 'tester'] as UserType[]).map((userType) => (
                      <label key={userType} className="flex items-center">
                        <input
                          type="checkbox"
                          checked={systemSettings.auto_approve_user_types.includes(userType)}
                          onChange={(e) => {
                            const currentTypes = systemSettings.auto_approve_user_types
                            const newTypes = e.target.checked
                              ? [...currentTypes, userType]
                              : currentTypes.filter(t => t !== userType)
                            handleSystemSettingChange('auto_approve_user_types', newTypes)
                          }}
                          className="h-4 w-4 text-slate-600 focus:ring-slate-500 border-gray-300 rounded accent-slate-600"
                        />
                        <span className="ml-2 text-sm text-gray-700 capitalize">{userType}</span>
                      </label>
                    ))}
                  </div>
                  <p className="mt-1 text-sm text-gray-500">
                    User types that are automatically approved upon registration
                  </p>
                </div>

                {/* Save Button */}
                <div className="pt-4 border-t border-gray-200">
                  <button
                    onClick={handleSaveSystemSettings}
                    disabled={systemSettingsSaving}
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-slate-600 hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    {systemSettingsSaving ? (
                      <>
                        <div className="animate-spin -ml-1 mr-3 h-5 w-5 text-white">
                          <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
                            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                          </svg>
                        </div>
                        Saving...
                      </>
                    ) : (
                      'Save Settings'
                    )}
                  </button>
                </div>
              </div>
            ) : (
              <div className="text-center py-8">
                <p className="text-gray-500">Failed to load system settings</p>
                <button
                  onClick={fetchSystemSettings}
                  className="mt-2 text-slate-600 hover:text-slate-500"
                >
                  Retry
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* License Classes Tab */}
      {activeTab === 'licenses' && (
        <div className="bg-white shadow rounded-lg">
          <div className="px-4 py-5 sm:p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg leading-6 font-medium text-gray-900">
                License Classes
              </h3>
              <button
                onClick={handleAddLicense}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-slate-600 hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500"
              >
                Add License Class
              </button>
            </div>

            {licenseClassesLoading ? (
              <div className="flex items-center justify-center py-8">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-slate-500"></div>
              </div>
            ) : licenseClasses.length > 0 ? (
              <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
                <table className="min-w-full divide-y divide-gray-300">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Name
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Description
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Created
                      </th>
                      <th className="relative px-6 py-3">
                        <span className="sr-only">Actions</span>
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {licenseClasses.map((license) => (
                      <tr key={license.id}>
                        <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                          {license.name}
                        </td>
                        <td className="px-6 py-4 text-sm text-gray-500">
                          {license.description}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                          {new Date(license.created_at).toLocaleDateString()}
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                          <button
                            onClick={() => handleEditLicense(license)}
                            className="text-slate-600 hover:text-slate-900 mr-4"
                          >
                            Edit
                          </button>
                          <button
                            onClick={() => handleDeleteLicense(license)}
                            className="text-red-600 hover:text-red-900"
                          >
                            Delete
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="text-center py-8">
                <p className="text-gray-500">No license classes found</p>
                <button
                  onClick={handleAddLicense}
                  className="mt-2 text-slate-600 hover:text-slate-500"
                >
                  Add your first license class
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Add/Edit License Dialog */}
      {showAddLicenseDialog && (
        <div className="fixed inset-0 bg-gray-600 bg-opacity-50 overflow-y-auto h-full w-full flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg shadow-lg max-w-md w-full mx-4">
            <h3 className="text-lg font-medium text-gray-900 mb-4">
              {editingLicense ? 'Edit License Class' : 'Add License Class'}
            </h3>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Name
                </label>
                <input
                  type="text"
                  value={licenseFormData.name}
                  onChange={(e) => setLicenseFormData(prev => ({ ...prev, name: e.target.value }))}
                  className="block w-full border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 sm:text-sm"
                  placeholder="e.g., CDL Class A"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Description
                </label>
                <textarea
                  value={licenseFormData.description}
                  onChange={(e) => setLicenseFormData(prev => ({ ...prev, description: e.target.value }))}
                  rows={3}
                  className="block w-full border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 sm:text-sm"
                  placeholder="Describe this license class..."
                />
              </div>
            </div>
            <div className="mt-6 flex justify-end space-x-3">
              <button
                onClick={() => {
                  setShowAddLicenseDialog(false)
                  setEditingLicense(null)
                }}
                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500"
              >
                Cancel
              </button>
              <button
                onClick={handleSaveLicense}
                disabled={licenseOperationLoading}
                className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-slate-600 hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-slate-500 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {licenseOperationLoading ? (
                  <>
                    <div className="animate-spin -ml-1 mr-3 h-5 w-5 text-white">
                      <svg className="animate-spin h-5 w-5" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                      </svg>
                    </div>
                    Saving...
                  </>
                ) : (
                  editingLicense ? 'Update' : 'Create'
                )}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Confirm Dialog */}
      <ConfirmDialog
        isOpen={confirmDialog.isOpen}
        title={confirmDialog.title}
        message={confirmDialog.message}
        type={confirmDialog.type}
        onConfirm={() => {
          confirmDialog.onConfirm()
          setConfirmDialog(prev => ({ ...prev, isOpen: false }))
        }}
        onCancel={() => setConfirmDialog(prev => ({ ...prev, isOpen: false }))}
      />
    </div>
  )
}