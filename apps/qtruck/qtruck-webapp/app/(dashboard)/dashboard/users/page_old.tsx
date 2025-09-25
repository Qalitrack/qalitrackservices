'use client'

import { useState, useEffect } from 'react'
import { apiService } from '@/lib/api'
import { getBaseEmail } from '@/lib/utils'
import { useToast } from '@/components/ui/Toast'
import { ConfirmDialog } from '@/components/ui/Dialog'
import { RefreshButton } from '@/components/ui/RefreshButton'
import { useAuth } from '@/lib/auth/AuthContext'
import { FormField, Input } from '@/components/ui/FormField'
import DriverProfilesTab from '@/components/admin/DriverProfilesTab'
import { 
  ValidationError, 
  parseApiErrors, 
  getErrorsForField, 
  getGeneralErrors,
  validateEmail,
  validatePassword,
  validatePasswordConfirm,
  validateRequired
} from '@/lib/validation'

export default function UserManagementPage() {
  const { user: currentUser } = useAuth()
  const [activeTab, setActiveTab] = useState<'pending' | 'active' | 'rejected' | 'deactivated' | 'drivers'>('active')
  const [pendingUsers, setPendingUsers] = useState<any[]>([])
  const [activeUsers, setActiveUsers] = useState<any[]>([])
  const [rejectedUsers, setRejectedUsers] = useState<any[]>([])
  const [deactivatedUsers, setDeactivatedUsers] = useState<any[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterRole, setFilterRole] = useState('all')
  const { showSuccess, showError } = useToast()
  
  // Admin creation dialog state
  const [showAddAdminDialog, setShowAddAdminDialog] = useState(false)
  const [adminFormData, setAdminFormData] = useState({
    first_name: '',
    last_name: '',
    email: '',
    password: '',
    password_confirm: ''
  })
  const [adminFormLoading, setAdminFormLoading] = useState(false)
  const [adminFormErrors, setAdminFormErrors] = useState<ValidationError[]>([])
  const [adminClientErrors, setAdminClientErrors] = useState<Record<string, string>>({})
  const [confirmDialog, setConfirmDialog] = useState<{
    isOpen: boolean
    title: string
    message: string
    onConfirm: () => void
    type: 'danger' | 'warning' | 'info'
  }>({
    isOpen: false,
    title: '',
    message: '',
    onConfirm: () => {},
    type: 'info'
  })

  useEffect(() => {
    fetchData()
  }, [])

  const fetchData = async () => {
    try {
      setLoading(true)
      await Promise.all([fetchAllUsers()])
    } catch (error) {
      console.error('Failed to load users:', error)
    } finally {
      setLoading(false)
    }
  }

  const fetchAllUsers = async () => {
    try {
      const allUsers = await apiService.getAllUsers()
      
      // Categorize users based on their status
      const pending = allUsers.filter((user: any) => !user.is_approved && user.is_active)
      const active = allUsers.filter((user: any) => user.is_approved && user.is_active)
      const rejected = allUsers.filter((user: any) => !user.is_approved && !user.is_active)
      const deactivated = allUsers.filter((user: any) => user.is_approved && !user.is_active)
      
      setPendingUsers(pending)
      setActiveUsers(active)
      setRejectedUsers(rejected)
      setDeactivatedUsers(deactivated)
    } catch (error) {
      console.error('Failed to load users:', error)
    }
  }

  // Keep these for backward compatibility
  const fetchPendingUsers = () => fetchAllUsers()
  const fetchActiveUsers = () => fetchAllUsers()

  // Helper functions
  const isLastAdmin = (user: any) => {
    return user.user_type === 'admin' && activeUsers.filter(u => u.user_type === 'admin' && u.is_active !== false).length === 1
  }

  const isOwnAccount = (user: any) => {
    return currentUser && user.id === currentUser.id
  }

  const canDeactivateUser = (user: any) => {
    return !isOwnAccount(user) && !isLastAdmin(user)
  }

  const handleApproveUser = async (userId: string) => {
    try {
      await apiService.approveUser(userId)
      showSuccess('User approved', 'The user has been successfully approved and can now log in.')
      await fetchData()
    } catch (error) {
      showError('Failed to approve user', error instanceof Error ? error.message : 'An unexpected error occurred')
    }
  }

  const handleRejectUser = (userId: string, userName: string) => {
    setConfirmDialog({
      isOpen: true,
      title: 'Reject User',
      message: `Are you sure you want to reject ${userName}? They will be moved to the rejected tab and can be moved back to pending approval later if needed.`,
      type: 'danger',
      onConfirm: async () => {
        try {
          await apiService.rejectUser(userId)
          showSuccess('User rejected', 'The user has been rejected and moved to the rejected tab.')
          await fetchData()
        } catch (error) {
          showError('Failed to reject user', error instanceof Error ? error.message : 'An unexpected error occurred')
        }
      }
    })
  }

  // User management functions
  const handleDeactivateUser = (user: any) => {
    const userName = `${user.first_name} ${user.last_name}`.trim() || getBaseEmail(user.email)
    
    setConfirmDialog({
      isOpen: true,
      title: 'Deactivate User',
      message: `Are you sure you want to deactivate ${userName}? They will not be able to log in but their data will be preserved.`,
      type: 'warning',
      onConfirm: async () => {
        try {
          await apiService.deactivateUser(user.id)
          showSuccess('User deactivated', 'The user account has been deactivated.')
          await fetchActiveUsers()
        } catch (error) {
          showError('Failed to deactivate user', error instanceof Error ? error.message : 'An unexpected error occurred')
        }
      }
    })
  }

  const handleReactivateUser = async (userId: string) => {
    try {
      await apiService.reactivateUser(userId)
      showSuccess('User reactivated', 'The user account has been reactivated.')
      await fetchActiveUsers()
    } catch (error) {
      showError('Failed to reactivate user', error instanceof Error ? error.message : 'An unexpected error occurred')
    }
  }

  const handleDeleteUser = (user: any) => {
    const userName = `${user.first_name} ${user.last_name}`.trim() || getBaseEmail(user.email)
    
    setConfirmDialog({
      isOpen: true,
      title: 'Delete User Account',
      message: `Are you sure you want to permanently delete ${userName}? This action cannot be undone and will remove all their data from the system.`,
      type: 'danger',
      onConfirm: async () => {
        try {
          await apiService.deleteUser(user.id)
          showSuccess('User deleted', 'The user account has been permanently deleted.')
          await fetchData()
        } catch (error) {
          showError('Failed to delete user', error instanceof Error ? error.message : 'An unexpected error occurred')
        }
      }
    })
  }

  const handleMoveToReapproval = async (userId: string) => {
    try {
      // Reactivate the rejected user so they appear in pending approvals
      await apiService.reactivateUser(userId)
      showSuccess('User moved to pending', 'The user has been moved back to pending approvals for review.')
      await fetchData()
    } catch (error) {
      showError('Failed to move user to pending', error instanceof Error ? error.message : 'An unexpected error occurred')
    }
  }

  // Search and filter functions
  const filteredUsers = (users: any[]) => {
    return users.filter(user => {
      const matchesSearch = !searchTerm || 
        user.first_name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        user.last_name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        getBaseEmail(user.email).toLowerCase().includes(searchTerm.toLowerCase())
      
      const matchesRole = filterRole === 'all' || user.user_type === filterRole
      
      return matchesSearch && matchesRole
    })
  }

  // Admin form validation
  const validateAdminForm = () => {
    const newErrors: Record<string, string> = {}
    
    const firstNameError = validateRequired(adminFormData.first_name, 'First name')
    if (firstNameError) newErrors.first_name = firstNameError
    
    const lastNameError = validateRequired(adminFormData.last_name, 'Last name')
    if (lastNameError) newErrors.last_name = lastNameError
    
    const emailError = validateEmail(adminFormData.email)
    if (emailError) newErrors.email = emailError
    
    const passwordError = validatePassword(adminFormData.password)
    if (passwordError) newErrors.password = passwordError
    
    const confirmError = validatePasswordConfirm(adminFormData.password, adminFormData.password_confirm)
    if (confirmError) newErrors.password_confirm = confirmError
    
    setAdminClientErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleAdminInputChange = (field: string, value: string) => {
    setAdminFormData(prev => ({ ...prev, [field]: value }))
    // Clear errors for this field when user types
    if (adminClientErrors[field]) {
      setAdminClientErrors(prev => ({ ...prev, [field]: '' }))
    }
    if (getErrorsForField(adminFormErrors, field).length > 0) {
      setAdminFormErrors(prev => prev.filter(error => error.field !== field))
    }
  }

  const handleCreateAdmin = async (e: React.FormEvent) => {
    e.preventDefault()
    
    // Clear previous errors
    setAdminFormErrors([])
    setAdminClientErrors({})
    
    // Client-side validation
    if (!validateAdminForm()) {
      return
    }
    
    setAdminFormLoading(true)

    try {
      await apiService.createAdmin(adminFormData)
      showSuccess('Admin created successfully!', 'The new admin can now log in to the system.')
      setShowAddAdminDialog(false)
      setAdminFormData({
        first_name: '',
        last_name: '',
        email: '',
        password: '',
        password_confirm: ''
      })
      await fetchData() // Refresh the user lists
    } catch (error) {
      console.log('Admin creation error:', error)
      const apiErrors = parseApiErrors(error)
      setAdminFormErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Failed to create admin', generalErrors[0])
      } else {
        showError('Failed to create admin', 'Please check the form for errors.')
      }
    } finally {
      setAdminFormLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">User Management</h1>
          <p className="text-gray-600">Manage user accounts, approvals, and access control</p>
        </div>
        <div className="flex items-center space-x-4">
          <button
            onClick={() => setShowAddAdminDialog(true)}
            className="btn btn-primary flex items-center space-x-2"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            <span>Add Admin</span>
          </button>
          <RefreshButton onRefresh={fetchData} loading={loading} theme="admin" />
        </div>
      </div>

      {/* Stats Dashboard */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-4">
        <div className="card bg-gradient-to-r from-slate-500 to-slate-600 text-white">
          <div className="flex items-center space-x-3">
            <div className="p-3 bg-white/20 rounded-full">
              <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div>
              <p className="text-2xl font-bold">{pendingUsers.length}</p>
              <p className="text-slate-100">Pending</p>
            </div>
          </div>
        </div>
        
        <div className="card bg-gradient-to-r from-green-500 to-green-600 text-white">
          <div className="flex items-center space-x-3">
            <div className="p-3 bg-white/20 rounded-full">
              <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                <path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div>
              <p className="text-2xl font-bold">{activeUsers.length}</p>
              <p className="text-green-100">Active</p>
            </div>
          </div>
        </div>

        <div className="card bg-gradient-to-r from-red-500 to-red-600 text-white">
          <div className="flex items-center space-x-3">
            <div className="p-3 bg-white/20 rounded-full">
              <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
            </div>
            <div>
              <p className="text-2xl font-bold">{rejectedUsers.length}</p>
              <p className="text-red-100">Rejected</p>
            </div>
          </div>
        </div>

        <div className="card bg-gradient-to-r from-orange-500 to-orange-600 text-white">
          <div className="flex items-center space-x-3">
            <div className="p-3 bg-white/20 rounded-full">
              <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M3 6a3 3 0 013-3h10a1 1 0 01.8 1.6L14.25 8l2.55 3.4A1 1 0 0116 13H6a1 1 0 00-1 1v3a1 1 0 11-2 0V6z" clipRule="evenodd" />
              </svg>
            </div>
            <div>
              <p className="text-2xl font-bold">{deactivatedUsers.length}</p>
              <p className="text-orange-100">Deactivated</p>
            </div>
          </div>
        </div>

        <div className="card bg-gradient-to-r from-purple-500 to-purple-600 text-white">
          <div className="flex items-center space-x-3">
            <div className="p-3 bg-white/20 rounded-full">
              <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                <path d="M9 6a3 3 0 11-6 0 3 3 0 016 0zM17 6a3 3 0 11-6 0 3 3 0 016 0zM12.93 17c.046-.327.07-.66.07-1a6.97 6.97 0 00-1.5-4.33A5 5 0 0119 16v1h-6.07zM6 11a5 5 0 015 5v1H1v-1a5 5 0 015-5z" />
              </svg>
            </div>
            <div>
              <p className="text-2xl font-bold">{activeUsers.filter(u => u.user_type === 'admin').length}</p>
              <p className="text-purple-100">Admins</p>
            </div>
          </div>
        </div>

        <div className="card bg-gradient-to-r from-teal-500 to-teal-600 text-white">
          <div className="flex items-center space-x-3">
            <div className="p-3 bg-white/20 rounded-full">
              <svg className="w-6 h-6" fill="currentColor" viewBox="0 0 20 20">
                <path d="M8 9a3 3 0 100-6 3 3 0 000 6zM8 11a6 6 0 016 6H2a6 6 0 016-6zM16 7a1 1 0 10-2 0v1h-1a1 1 0 100 2h1v1a1 1 0 102 0v-1h1a1 1 0 100-2h-1V7z" />
              </svg>
            </div>
            <div>
              <p className="text-2xl font-bold">{activeUsers.filter(u => u.user_type === 'driver').length}</p>
              <p className="text-teal-100">Drivers</p>
            </div>
          </div>
        </div>
      </div>

      {/* Tab Navigation */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('active')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'active'
                ? 'border-slate-500 text-slate-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Active Users
            <span className="ml-2 bg-gray-100 text-gray-600 py-0.5 px-2 rounded-full text-xs">
              {activeUsers.length}
            </span>
          </button>
          
          <button
            onClick={() => setActiveTab('pending')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'pending'
                ? 'border-slate-500 text-slate-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Pending Approvals
            {pendingUsers.length > 0 && (
              <span className="ml-2 bg-red-100 text-red-600 py-0.5 px-2 rounded-full text-xs">
                {pendingUsers.length}
              </span>
            )}
          </button>
          
          <button
            onClick={() => setActiveTab('rejected')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'rejected'
                ? 'border-slate-500 text-slate-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Rejected
            {rejectedUsers.length > 0 && (
              <span className="ml-2 bg-red-100 text-red-600 py-0.5 px-2 rounded-full text-xs">
                {rejectedUsers.length}
              </span>
            )}
          </button>
          
          <button
            onClick={() => setActiveTab('deactivated')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'deactivated'
                ? 'border-slate-500 text-slate-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Deactivated
            {deactivatedUsers.length > 0 && (
              <span className="ml-2 bg-orange-100 text-orange-600 py-0.5 px-2 rounded-full text-xs">
                {deactivatedUsers.length}
              </span>
            )}
          </button>

          <button
            onClick={() => setActiveTab('drivers')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'drivers'
                ? 'border-slate-500 text-slate-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            Driver Profiles
            <span className="ml-2 bg-purple-100 text-purple-600 py-0.5 px-2 rounded-full text-xs">
              🚛
            </span>
          </button>
        </nav>
      </div>

      {/* Search and Filter Controls */}
      {activeTab === 'active' && (
        <div className="flex flex-col sm:flex-row gap-4">
          <div className="flex-1">
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <svg className="h-5 w-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>
              <input
                type="text"
                placeholder="Search users by name or email..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:placeholder-gray-400 focus:ring-1 focus:ring-slate-500 focus:border-slate-500"
              />
            </div>
          </div>
          
          <select
            value={filterRole}
            onChange={(e) => setFilterRole(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-1 focus:ring-slate-500 focus:border-slate-500"
          >
            <option value="all">All Roles</option>
            <option value="admin">Admins</option>
            <option value="driver">Drivers</option>
            <option value="tester">Testers</option>
          </select>
        </div>
      )}

      {/* Tab Content */}
      {loading ? (
        <div className="space-y-4">
          {[1, 2, 3].map(i => (
            <div key={i} className="card animate-pulse">
              <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
              <div className="h-3 bg-gray-200 rounded w-1/2 mb-1"></div>
              <div className="h-3 bg-gray-200 rounded w-1/3"></div>
            </div>
          ))}
        </div>
      ) : (
        <>
          {/* Pending Users Tab */}
          {activeTab === 'pending' && (
            <>
              {pendingUsers.length === 0 ? (
                <div className="card text-center py-12">
                  <div className="text-6xl mb-4">✅</div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">All caught up!</h3>
                  <p className="text-gray-600">No pending user approvals at this time.</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {pendingUsers.map((user) => (
                    <div key={user.id} className="card hover:shadow-lg transition-shadow">
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <div className="flex items-center space-x-4">
                            <div className="w-12 h-12 bg-gradient-to-br from-gray-100 to-gray-200 rounded-full flex items-center justify-center shadow-sm">
                              <span className="text-xl">
                                {user.user_type === 'admin' ? '👑' : 
                                 user.user_type === 'driver' ? '🚗' : '🧪'}
                              </span>
                            </div>
                            <div>
                              <h3 className="text-lg font-semibold text-gray-900">
                                {user.first_name} {user.last_name}
                              </h3>
                              <p className="text-gray-600">{getBaseEmail(user.email)}</p>
                              <div className="flex items-center space-x-4 mt-1">
                                <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                                  user.user_type === 'admin' ? 'bg-slate-100 text-slate-800' :
                                  user.user_type === 'driver' ? 'bg-green-100 text-green-800' :
                                  'bg-purple-100 text-purple-800'
                                }`}>
                                  {user.user_type.toUpperCase()}
                                </span>
                                <span className="text-sm text-gray-500">
                                  Registered: {new Date(user.created_at || Date.now()).toLocaleDateString()}
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div className="flex space-x-3">
                          <button 
                            onClick={() => handleApproveUser(user.id)}
                            className="btn btn-success flex items-center space-x-2"
                          >
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                            </svg>
                            <span>Approve</span>
                          </button>
                          <button 
                            onClick={() => handleRejectUser(user.id, `${user.first_name} ${user.last_name}`)}
                            className="btn btn-outline text-red-600 flex items-center space-x-2"
                          >
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L10 10.586l1.293-1.293a1 1 0 001.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                            </svg>
                            <span>Reject</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}

          {/* Active Users Tab */}
          {activeTab === 'active' && (
            <>
              {filteredUsers(activeUsers).length === 0 ? (
                <div className="card text-center py-12">
                  <div className="text-6xl mb-4">👥</div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">No users found</h3>
                  <p className="text-gray-600">
                    {searchTerm || filterRole !== 'all' 
                      ? 'Try adjusting your search or filter criteria.' 
                      : 'No active users in the system.'}
                  </p>
                </div>
              ) : (
                <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                  {filteredUsers(activeUsers).map((user) => (
                    <div key={user.id} className="card hover:shadow-lg transition-all duration-200 border-l-4 border-l-slate-500">
                      <div className="flex items-start justify-between">
                        <div className="flex items-start space-x-4 flex-1">
                          <div className="relative">
                            <div className="w-14 h-14 bg-gradient-to-br from-slate-100 to-slate-200 rounded-full flex items-center justify-center shadow-sm">
                              <span className="text-2xl">
                                {user.user_type === 'admin' ? '👑' : 
                                 user.user_type === 'driver' ? '🚗' : '🧪'}
                              </span>
                            </div>
                            {user.is_active === false && (
                              <div className="absolute -top-1 -right-1 w-4 h-4 bg-red-500 rounded-full border-2 border-white" title="Deactivated"></div>
                            )}
                          </div>
                          
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center space-x-2 mb-1">
                              <h3 className="text-lg font-semibold text-gray-900 truncate">
                                {user.first_name} {user.last_name}
                              </h3>
                              {isOwnAccount(user) && (
                                <span className="inline-flex px-2 py-1 text-xs font-medium rounded-full bg-yellow-100 text-yellow-800">
                                  You
                                </span>
                              )}
                            </div>
                            
                            <p className="text-gray-600 text-sm mb-2">{getBaseEmail(user.email)}</p>
                            
                            <div className="flex flex-wrap items-center gap-2">
                              <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                                user.user_type === 'admin' ? 'bg-purple-100 text-purple-800' :
                                user.user_type === 'driver' ? 'bg-green-100 text-green-800' :
                                'bg-slate-100 text-slate-800'
                              }`}>
                                {user.user_type.toUpperCase()}
                              </span>
                              
                              <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                                user.is_active === false ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'
                              }`}>
                                {user.is_active === false ? 'Deactivated' : 'Active'}
                              </span>
                              
                              {user.user_type === 'admin' && isLastAdmin(user) && (
                                <span className="inline-flex px-2 py-1 text-xs font-medium rounded-full bg-orange-100 text-orange-800">
                                  Last Admin
                                </span>
                              )}
                            </div>
                            
                            <div className="mt-2 text-xs text-gray-500">
                              <p>Joined: {new Date(user.created_at || Date.now()).toLocaleDateString()}</p>
                              <p>Last login: {user.last_login ? new Date(user.last_login).toLocaleDateString() : 'Never'}</p>
                            </div>
                          </div>
                        </div>
                        
                        <div className="flex flex-col space-y-2 ml-4">
                          {user.is_active === false ? (
                            <button 
                              onClick={() => handleReactivateUser(user.id)}
                              className="btn btn-sm btn-success flex items-center space-x-1"
                            >
                              <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                              </svg>
                              <span>Reactivate</span>
                            </button>
                          ) : (
                            <>
                              {canDeactivateUser(user) && (
                                <button 
                                  onClick={() => handleDeactivateUser(user)}
                                  className="btn btn-sm btn-outline text-red-600 flex items-center space-x-1"
                                >
                                  <svg className="w-3 h-3" fill="currentColor" viewBox="0 0 20 20">
                                    <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L10 10.586l1.293-1.293a1 1 0 001.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
                                  </svg>
                                  <span>Deactivate</span>
                                </button>
                              )}
                              
                              {!canDeactivateUser(user) && (
                                <div className="text-xs text-gray-500 text-center px-2">
                                  {isOwnAccount(user) ? "Can't deactivate own account" : "Last admin account"}
                                </div>
                              )}
                            </>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}

          {/* Rejected Users Tab */}
          {activeTab === 'rejected' && (
            <>
              {rejectedUsers.length === 0 ? (
                <div className="card text-center py-12">
                  <div className="text-6xl mb-4">✅</div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">No rejected users</h3>
                  <p className="text-gray-600">No users have been rejected at this time.</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {rejectedUsers.map((user) => (
                    <div key={user.id} className="card hover:shadow-lg transition-shadow border-l-4 border-l-red-500">
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <div className="flex items-center space-x-4">
                            <div className="w-12 h-12 bg-gradient-to-br from-red-100 to-red-200 rounded-full flex items-center justify-center shadow-sm">
                              <span className="text-xl">
                                {user.user_type === 'admin' ? '👑' : 
                                 user.user_type === 'driver' ? '🚗' : '🧪'}
                              </span>
                            </div>
                            <div>
                              <h3 className="text-lg font-semibold text-gray-900">
                                {user.first_name} {user.last_name}
                              </h3>
                              <p className="text-gray-600">{getBaseEmail(user.email)}</p>
                              <div className="flex items-center space-x-4 mt-1">
                                <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                                  user.user_type === 'admin' ? 'bg-slate-100 text-slate-800' :
                                  user.user_type === 'driver' ? 'bg-green-100 text-green-800' :
                                  'bg-purple-100 text-purple-800'
                                }`}>
                                  {user.user_type.toUpperCase()}
                                </span>
                                <span className="inline-flex px-2 py-1 text-xs font-medium rounded-full bg-red-100 text-red-800">
                                  REJECTED
                                </span>
                                <span className="text-sm text-gray-500">
                                  Registered: {new Date(user.created_at || Date.now()).toLocaleDateString()}
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div className="flex space-x-3">
                          <button 
                            onClick={() => handleMoveToReapproval(user.id)}
                            className="btn btn-success flex items-center space-x-2"
                          >
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M4 2a1 1 0 011 1v2.101a7.002 7.002 0 0111.601 2.566 1 1 0 11-1.885.666A5.002 5.002 0 005.999 7H9a1 1 0 010 2H4a1 1 0 01-1-1V3a1 1 0 011-1zm.008 9.057a1 1 0 011.276.61A5.002 5.002 0 0014.001 13H11a1 1 0 110-2h5a1 1 0 011 1v5a1 1 0 11-2 0v-2.101a7.002 7.002 0 01-11.601-2.566 1 1 0 01.61-1.276z" clipRule="evenodd" />
                            </svg>
                            <span>Move to Re-approval</span>
                          </button>
                          <button 
                            onClick={() => handleDeleteUser(user)}
                            className="btn btn-outline text-red-600 flex items-center space-x-2"
                          >
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M9 2a1 1 0 000 2h2a1 1 0 100-2H9z" clipRule="evenodd" />
                              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293z" clipRule="evenodd" />
                            </svg>
                            <span>Delete</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}

          {/* Deactivated Users Tab */}
          {activeTab === 'deactivated' && (
            <>
              {deactivatedUsers.length === 0 ? (
                <div className="card text-center py-12">
                  <div className="text-6xl mb-4">✅</div>
                  <h3 className="text-lg font-semibold text-gray-900 mb-2">No deactivated users</h3>
                  <p className="text-gray-600">No users have been deactivated at this time.</p>
                </div>
              ) : (
                <div className="space-y-4">
                  {deactivatedUsers.map((user) => (
                    <div key={user.id} className="card hover:shadow-lg transition-shadow border-l-4 border-l-orange-500">
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <div className="flex items-center space-x-4">
                            <div className="w-12 h-12 bg-gradient-to-br from-orange-100 to-orange-200 rounded-full flex items-center justify-center shadow-sm">
                              <span className="text-xl">
                                {user.user_type === 'admin' ? '👑' : 
                                 user.user_type === 'driver' ? '🚗' : '🧪'}
                              </span>
                            </div>
                            <div>
                              <h3 className="text-lg font-semibold text-gray-900">
                                {user.first_name} {user.last_name}
                              </h3>
                              <p className="text-gray-600">{getBaseEmail(user.email)}</p>
                              <div className="flex items-center space-x-4 mt-1">
                                <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${
                                  user.user_type === 'admin' ? 'bg-slate-100 text-slate-800' :
                                  user.user_type === 'driver' ? 'bg-green-100 text-green-800' :
                                  'bg-purple-100 text-purple-800'
                                }`}>
                                  {user.user_type.toUpperCase()}
                                </span>
                                <span className="inline-flex px-2 py-1 text-xs font-medium rounded-full bg-orange-100 text-orange-800">
                                  DEACTIVATED
                                </span>
                                <span className="text-sm text-gray-500">
                                  Joined: {new Date(user.created_at || Date.now()).toLocaleDateString()}
                                </span>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div className="flex space-x-3">
                          <button 
                            onClick={() => handleReactivateUser(user.id)}
                            className="btn btn-success flex items-center space-x-2"
                          >
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z" clipRule="evenodd" />
                            </svg>
                            <span>Reactivate</span>
                          </button>
                          <button 
                            onClick={() => handleDeleteUser(user)}
                            className="btn btn-outline text-red-600 flex items-center space-x-2"
                          >
                            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                              <path fillRule="evenodd" d="M9 2a1 1 0 000 2h2a1 1 0 100-2H9z" clipRule="evenodd" />
                              <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293z" clipRule="evenodd" />
                            </svg>
                            <span>Delete</span>
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </>
          )}

          {/* Driver Profiles Tab */}
          {activeTab === 'drivers' && (
            <DriverProfilesTab />
          )}
        </>
      )}

      {/* Add Admin Dialog */}
      {showAddAdminDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
          <div className="bg-white rounded-lg shadow-xl w-full max-w-md max-h-[90vh] overflow-y-auto">
            <div className="px-6 py-4 border-b border-gray-200">
              <div className="flex items-center justify-between">
                <h3 className="text-lg font-semibold text-gray-900">Create New Admin</h3>
                <button
                  onClick={() => setShowAddAdminDialog(false)}
                  className="text-gray-400 hover:text-gray-600"
                >
                  <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>
            
            <div className="px-6 py-4">
              <form onSubmit={handleCreateAdmin} className="space-y-4">
                {/* Display general/non-field errors */}
                {getGeneralErrors(adminFormErrors).map((error, index) => (
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

                <div className="grid grid-cols-2 gap-4">
                  <FormField 
                    label="First Name" 
                    required
                    errors={[...getErrorsForField(adminFormErrors, 'first_name'), ...(adminClientErrors.first_name ? [adminClientErrors.first_name] : [])]}
                  >
                    <Input
                      type="text"
                      value={adminFormData.first_name}
                      onChange={(e) => handleAdminInputChange('first_name', e.target.value)}
                      placeholder="John"
                      errors={[...getErrorsForField(adminFormErrors, 'first_name'), ...(adminClientErrors.first_name ? [adminClientErrors.first_name] : [])]}
                    />
                  </FormField>
                  
                  <FormField 
                    label="Last Name" 
                    required
                    errors={[...getErrorsForField(adminFormErrors, 'last_name'), ...(adminClientErrors.last_name ? [adminClientErrors.last_name] : [])]}
                  >
                    <Input
                      type="text"
                      value={adminFormData.last_name}
                      onChange={(e) => handleAdminInputChange('last_name', e.target.value)}
                      placeholder="Smith"
                      errors={[...getErrorsForField(adminFormErrors, 'last_name'), ...(adminClientErrors.last_name ? [adminClientErrors.last_name] : [])]}
                    />
                  </FormField>
                </div>

                <FormField 
                  label="Admin Email" 
                  required
                  hint="Use the admin's base email without any alias"
                  errors={[...getErrorsForField(adminFormErrors, 'email'), ...(adminClientErrors.email ? [adminClientErrors.email] : [])]}
                >
                  <Input
                    type="email"
                    value={adminFormData.email}
                    onChange={(e) => handleAdminInputChange('email', e.target.value)}
                    placeholder="admin@company.com"
                    errors={[...getErrorsForField(adminFormErrors, 'email'), ...(adminClientErrors.email ? [adminClientErrors.email] : [])]}
                  />
                </FormField>

                <FormField 
                  label="Password" 
                  required
                  hint="Password must be at least 8 characters long"
                  errors={[...getErrorsForField(adminFormErrors, 'password'), ...(adminClientErrors.password ? [adminClientErrors.password] : [])]}
                >
                  <Input
                    type="password"
                    value={adminFormData.password}
                    onChange={(e) => handleAdminInputChange('password', e.target.value)}
                    placeholder="Create a secure password"
                    errors={[...getErrorsForField(adminFormErrors, 'password'), ...(adminClientErrors.password ? [adminClientErrors.password] : [])]}
                  />
                </FormField>

                <FormField 
                  label="Confirm Password" 
                  required
                  errors={[...getErrorsForField(adminFormErrors, 'password_confirm'), ...(adminClientErrors.password_confirm ? [adminClientErrors.password_confirm] : [])]}
                >
                  <Input
                    type="password"
                    value={adminFormData.password_confirm}
                    onChange={(e) => handleAdminInputChange('password_confirm', e.target.value)}
                    placeholder="Confirm your password"
                    errors={[...getErrorsForField(adminFormErrors, 'password_confirm'), ...(adminClientErrors.password_confirm ? [adminClientErrors.password_confirm] : [])]}
                  />
                </FormField>

                <div className="flex justify-end space-x-3 pt-4">
                  <button
                    type="button"
                    onClick={() => setShowAddAdminDialog(false)}
                    className="btn btn-outline"
                    disabled={adminFormLoading}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    disabled={adminFormLoading}
                    className="btn btn-primary flex items-center space-x-2"
                  >
                    {adminFormLoading ? (
                      <>
                        <svg className="animate-spin -ml-1 mr-3 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                        </svg>
                        <span>Creating...</span>
                      </>
                    ) : (
                      <>
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                        <span>Create Admin</span>
                      </>
                    )}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Confirmation Dialog */}
      <ConfirmDialog
        isOpen={confirmDialog.isOpen}
        onClose={() => setConfirmDialog(prev => ({ ...prev, isOpen: false }))}
        onConfirm={confirmDialog.onConfirm}
        title={confirmDialog.title}
        message={confirmDialog.message}
        type={confirmDialog.type}
        confirmText={confirmDialog.type === 'danger' ? 'Reject User' : 'Confirm'}
        cancelText="Cancel"
      />
    </div>
  )
}