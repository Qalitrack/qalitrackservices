'use client'

import { useState, useEffect, useCallback } from 'react'
import { apiClient, ApiError } from '@/lib/api/client'
import { User, UserStatus, UserType, CreateUserRequest, UserListParams } from '@/lib/types/api'
import { useAuth } from '@/lib/auth/AuthContext'
import { useToast } from '@/components/ui/Toast'
import { ConfirmDialog } from '@/components/ui/Dialog'

interface UserStats {
  pending: number
  approved: number
  rejected: number
  deactivated: number
  admins: number
  drivers: number
  testers: number
}

type TabType = 'pending' | 'active' | 'rejected' | 'deactivated' | 'all'

export default function UserManagementPage() {
  const { user: currentUser } = useAuth()
  const { showSuccess, showError } = useToast()

  // State management
  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<TabType>('active')
  const [searchTerm, setSearchTerm] = useState('')
  const [filterRole, setFilterRole] = useState<UserType | 'all'>('all')
  
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

  // Create user form state
  const [createForm, setCreateForm] = useState<CreateUserRequest>({
    email: '',
    password: '',
    first_name: '',
    last_name: '',
    user_type: 'driver'
  })
  const [createLoading, setCreateLoading] = useState(false)
  const [createErrors, setCreateErrors] = useState<Record<string, string>>({})

  // Load users data
  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true)
      const params: UserListParams = { page_size: 1000 } // Get all users for now
      
      if (filterRole !== 'all') {
        params.user_type = filterRole
      }
      
      if (searchTerm.trim()) {
        params.search = searchTerm.trim()
      }

      const response = await apiClient.getUsers(params)
      setUsers(response.results || [])
    } catch (error) {
      console.error('Failed to fetch users:', error)
      if (error instanceof ApiError) {
        showError('Failed to load users', error.detail || 'Please try again.')
      } else {
        showError('Failed to load users', 'An unexpected error occurred.')
      }
    } finally {
      setLoading(false)
    }
  }, [filterRole, searchTerm, showError])

  // Calculate stats
  const stats: UserStats = users.reduce(
    (acc, user) => {
      if (user.status === 'preapproval') acc.pending++
      else if (user.status === 'approved' && user.is_active) acc.approved++
      else if (user.status === 'rejected') acc.rejected++
      else if (user.status === 'approved' && !user.is_active) acc.deactivated++

      if (user.user_type === 'admin') acc.admins++
      else if (user.user_type === 'driver') acc.drivers++
      else if (user.user_type === 'tester') acc.testers++

      return acc
    },
    { pending: 0, approved: 0, rejected: 0, deactivated: 0, admins: 0, drivers: 0, testers: 0 }
  )

  // Filter users by active tab
  const getFilteredUsers = (): User[] => {
    let filtered = users

    switch (activeTab) {
      case 'pending':
        filtered = users.filter(u => u.status === 'preapproval')
        break
      case 'active':
        filtered = users.filter(u => u.status === 'approved' && u.is_active)
        break
      case 'rejected':
        filtered = users.filter(u => u.status === 'rejected')
        break
      case 'deactivated':
        filtered = users.filter(u => u.status === 'approved' && !u.is_active)
        break
      case 'all':
        filtered = users
        break
    }

    return filtered
  }

  // User actions
  const handleApproveUser = async (user: User) => {
    try {
      await apiClient.approveUser(user.id)
      showSuccess('User approved', `${user.full_name} has been approved and can now log in.`)
      await fetchUsers()
    } catch (error) {
      console.error('Failed to approve user:', error)
      if (error instanceof ApiError) {
        showError('Failed to approve user', error.detail || 'Please try again.')
      }
    }
  }

  const handleRejectUser = (user: User) => {
    setConfirmAction({
      open: true,
      title: 'Reject User',
      message: `Are you sure you want to reject ${user.full_name}? They will be moved to rejected status and cannot log in.`,
      variant: 'danger',
      onConfirm: async () => {
        try {
          await apiClient.rejectUser(user.id)
          showSuccess('User rejected', `${user.full_name} has been rejected.`)
          await fetchUsers()
        } catch (error) {
          console.error('Failed to reject user:', error)
          if (error instanceof ApiError) {
            showError('Failed to reject user', error.detail || 'Please try again.')
          }
        }
      }
    })
  }

  const handleDeactivateUser = (user: User) => {
    setConfirmAction({
      open: true,
      title: 'Deactivate User',
      message: `Are you sure you want to deactivate ${user.full_name}? They will not be able to log in but their data will be preserved.`,
      variant: 'warning',
      onConfirm: async () => {
        try {
          await apiClient.deactivateUser(user.id)
          showSuccess('User deactivated', `${user.full_name} has been deactivated.`)
          await fetchUsers()
        } catch (error) {
          console.error('Failed to deactivate user:', error)
          if (error instanceof ApiError) {
            showError('Failed to deactivate user', error.detail || 'Please try again.')
          }
        }
      }
    })
  }

  const handleReactivateUser = async (user: User) => {
    try {
      await apiClient.reactivateUser(user.id)
      showSuccess('User reactivated', `${user.full_name} has been reactivated.`)
      await fetchUsers()
    } catch (error) {
      console.error('Failed to reactivate user:', error)
      if (error instanceof ApiError) {
        showError('Failed to reactivate user', error.detail || 'Please try again.')
      }
    }
  }

  const handleDeleteUser = (user: User) => {
    setConfirmAction({
      open: true,
      title: 'Delete User',
      message: `Are you sure you want to permanently delete ${user.full_name}? This action cannot be undone and will remove all their data.`,
      variant: 'danger',
      onConfirm: async () => {
        try {
          await apiClient.deleteUser(user.id)
          showSuccess('User deleted', `${user.full_name} has been permanently deleted.`)
          await fetchUsers()
        } catch (error) {
          console.error('Failed to delete user:', error)
          if (error instanceof ApiError) {
            showError('Failed to delete user', error.detail || 'Please try again.')
          }
        }
      }
    })
  }

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault()
    setCreateErrors({})
    setCreateLoading(true)

    try {
      await apiClient.createUser(createForm)
      showSuccess('User created', 'The new user has been created successfully.')
      setShowCreateDialog(false)
      setCreateForm({
        email: '',
        password: '',
        first_name: '',
        last_name: '',
        user_type: 'driver'
      })
      await fetchUsers()
    } catch (error) {
      console.error('Failed to create user:', error)
      if (error instanceof ApiError) {
        const fieldErrors: Record<string, string> = {}
        error.getAllErrors().forEach(err => {
          fieldErrors[err.field] = err.message
        })
        setCreateErrors(fieldErrors)
        
        if (!Object.keys(fieldErrors).length) {
          showError('Failed to create user', error.detail || 'Please try again.')
        }
      }
    } finally {
      setCreateLoading(false)
    }
  }

  // Helper functions
  const isOwnAccount = (user: User) => currentUser?.id === user.id
  const isLastAdmin = (user: User) => {
    if (user.user_type !== 'admin') return false
    const activeAdmins = users.filter(u => u.user_type === 'admin' && u.status === 'approved' && u.is_active)
    return activeAdmins.length === 1
  }
  const canDeactivate = (user: User) => !isOwnAccount(user) && !isLastAdmin(user)

  // Effects
  useEffect(() => {
    fetchUsers()
  }, [fetchUsers])

  // Debounce search
  useEffect(() => {
    const timer = setTimeout(() => {
      fetchUsers()
    }, 500)
    return () => clearTimeout(timer)
  }, [searchTerm])

  const filteredUsers = getFilteredUsers()

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">User Management</h1>
          <p className="text-gray-600">Manage user accounts, approvals, and access control</p>
        </div>
        <div className="flex items-center space-x-4">
          <button
            onClick={() => setShowCreateDialog(true)}
            className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white px-4 py-2 rounded-md flex items-center space-x-2 transition-all duration-200"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
            </svg>
            <span>Create User</span>
          </button>
          <button
            onClick={fetchUsers}
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
      <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-4">
        <StatsCard title="Pending" value={stats.pending} color="blue" icon="⏳" />
        <StatsCard title="Active" value={stats.approved} color="green" icon="✅" />
        <StatsCard title="Rejected" value={stats.rejected} color="red" icon="❌" />
        <StatsCard title="Deactivated" value={stats.deactivated} color="orange" icon="⏸️" />
        <StatsCard title="Admins" value={stats.admins} color="purple" icon="👑" />
        <StatsCard title="Drivers" value={stats.drivers} color="teal" icon="🚗" />
        <StatsCard title="Testers" value={stats.testers} color="indigo" icon="🧪" />
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          {[
            { key: 'active' as TabType, label: 'Active Users', count: stats.approved },
            { key: 'pending' as TabType, label: 'Pending', count: stats.pending },
            { key: 'rejected' as TabType, label: 'Rejected', count: stats.rejected },
            { key: 'deactivated' as TabType, label: 'Deactivated', count: stats.deactivated },
            { key: 'all' as TabType, label: 'All Users', count: users.length }
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
            placeholder="Search users by name or email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
          />
        </div>
        <select
          value={filterRole}
          onChange={(e) => setFilterRole(e.target.value as UserType | 'all')}
          className="px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
        >
          <option value="all">All Roles</option>
          <option value="admin">Admins</option>
          <option value="driver">Drivers</option>
          <option value="tester">Testers</option>
        </select>
      </div>

      {/* Users List */}
      {loading ? (
        <div className="space-y-4">
          {[...Array(5)].map((_, i) => (
            <div key={i} className="bg-white rounded-lg shadow p-6 animate-pulse">
              <div className="flex items-center space-x-4">
                <div className="w-12 h-12 bg-gray-200 rounded-full"></div>
                <div className="flex-1">
                  <div className="h-4 bg-gray-200 rounded w-1/3 mb-2"></div>
                  <div className="h-3 bg-gray-200 rounded w-1/4"></div>
                </div>
              </div>
            </div>
          ))}
        </div>
      ) : filteredUsers.length === 0 ? (
        <div className="bg-white rounded-lg shadow p-12 text-center">
          <div className="text-6xl mb-4">👥</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No users found</h3>
          <p className="text-gray-600">
            {searchTerm || filterRole !== 'all'
              ? 'Try adjusting your search or filter criteria.'
              : 'No users match the current tab filter.'
            }
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredUsers.map(user => (
            <UserCard
              key={user.id}
              user={user}
              currentUser={currentUser}
              isOwn={isOwnAccount(user)}
              isLastAdmin={isLastAdmin(user)}
              canDeactivate={canDeactivate(user)}
              onApprove={() => handleApproveUser(user)}
              onReject={() => handleRejectUser(user)}
              onDeactivate={() => handleDeactivateUser(user)}
              onReactivate={() => handleReactivateUser(user)}
              onDelete={() => handleDeleteUser(user)}
            />
          ))}
        </div>
      )}

      {/* Create User Dialog */}
      {showCreateDialog && (
        <CreateUserDialog
          form={createForm}
          onChange={setCreateForm}
          onSubmit={handleCreateUser}
          onClose={() => setShowCreateDialog(false)}
          loading={createLoading}
          errors={createErrors}
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
    orange: 'from-orange-500 to-orange-600',
    purple: 'from-purple-500 to-purple-600',
    teal: 'from-teal-500 to-teal-600',
    indigo: 'from-indigo-500 to-indigo-600'
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

function UserCard({ 
  user, 
  currentUser, 
  isOwn, 
  isLastAdmin, 
  canDeactivate,
  onApprove,
  onReject,
  onDeactivate,
  onReactivate,
  onDelete
}: {
  user: User
  currentUser: User | null
  isOwn: boolean
  isLastAdmin: boolean
  canDeactivate: boolean
  onApprove: () => void
  onReject: () => void
  onDeactivate: () => void
  onReactivate: () => void
  onDelete: () => void
}) {
  const getStatusColor = (status: UserStatus, isActive: boolean) => {
    if (status === 'preapproval') return 'bg-yellow-100 text-yellow-800'
    if (status === 'approved' && isActive) return 'bg-green-100 text-green-800'
    if (status === 'approved' && !isActive) return 'bg-orange-100 text-orange-800'
    if (status === 'rejected') return 'bg-red-100 text-red-800'
    return 'bg-gray-100 text-gray-800'
  }

  const getUserTypeIcon = (type: UserType) => {
    switch (type) {
      case 'admin': return '👑'
      case 'driver': return '🚗'
      case 'tester': return '🧪'
      default: return '👤'
    }
  }

  return (
    <div className="bg-white rounded-lg shadow hover:shadow-md transition-shadow p-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <div className="w-12 h-12 bg-gray-100 rounded-full flex items-center justify-center text-xl">
            {getUserTypeIcon(user.user_type)}
          </div>
          <div>
            <div className="flex items-center space-x-2">
              <h3 className="text-lg font-semibold text-gray-900">{user.full_name}</h3>
              {isOwn && (
                <span className="bg-yellow-100 text-yellow-800 text-xs px-2 py-1 rounded-full font-medium">
                  You
                </span>
              )}
            </div>
            <p className="text-gray-600">{user.base_email}</p>
            <div className="flex items-center space-x-2 mt-1">
              <span className={`text-xs px-2 py-1 rounded-full font-medium ${
                user.user_type === 'admin' ? 'bg-purple-100 text-purple-800' :
                user.user_type === 'driver' ? 'bg-green-100 text-green-800' :
                'bg-slate-100 text-slate-800'
              }`}>
                {user.user_type.toUpperCase()}
              </span>
              <span className={`text-xs px-2 py-1 rounded-full font-medium ${getStatusColor(user.status, user.is_active)}`}>
                {user.status === 'preapproval' ? 'PENDING' : 
                 user.status === 'approved' && !user.is_active ? 'DEACTIVATED' :
                 user.status.toUpperCase()}
              </span>
              {isLastAdmin && (
                <span className="bg-red-100 text-red-800 text-xs px-2 py-1 rounded-full font-medium">
                  LAST ADMIN
                </span>
              )}
            </div>
            <p className="text-xs text-gray-500 mt-1">
              Joined: {new Date(user.created_at).toLocaleDateString()}
            </p>
          </div>
        </div>

        <div className="flex items-center space-x-2">
          {user.status === 'preapproval' && (
            <>
              <button
                onClick={onApprove}
                className="bg-green-600 hover:bg-green-700 text-white px-3 py-1 rounded text-sm"
              >
                Approve
              </button>
              <button
                onClick={onReject}
                className="bg-red-600 hover:bg-red-700 text-white px-3 py-1 rounded text-sm"
              >
                Reject
              </button>
            </>
          )}

          {user.status === 'approved' && user.is_active && canDeactivate && (
            <button
              onClick={onDeactivate}
              className="bg-orange-600 hover:bg-orange-700 text-white px-3 py-1 rounded text-sm"
            >
              Deactivate
            </button>
          )}

          {user.status === 'approved' && !user.is_active && (
            <button
              onClick={onReactivate}
              className="bg-green-600 hover:bg-green-700 text-white px-3 py-1 rounded text-sm"
            >
              Reactivate
            </button>
          )}

          {(user.status === 'rejected' || !user.is_active) && !isOwn && !isLastAdmin && (
            <button
              onClick={onDelete}
              className="bg-red-600 hover:bg-red-700 text-white px-3 py-1 rounded text-sm"
            >
              Delete
            </button>
          )}
        </div>
      </div>
    </div>
  )
}

function CreateUserDialog({
  form,
  onChange,
  onSubmit,
  onClose,
  loading,
  errors
}: {
  form: CreateUserRequest
  onChange: (form: CreateUserRequest) => void
  onSubmit: (e: React.FormEvent) => void
  onClose: () => void
  loading: boolean
  errors: Record<string, string>
}) {
  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center p-4 z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md">
        <div className="px-6 py-4 border-b border-gray-200">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold text-gray-900">Create New User</h3>
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

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                First Name
              </label>
              <input
                type="text"
                value={form.first_name}
                onChange={(e) => onChange({ ...form, first_name: e.target.value })}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                  errors.first_name ? 'border-red-300' : 'border-gray-300'
                }`}
                required
              />
              {errors.first_name && (
                <p className="text-red-600 text-xs mt-1">{errors.first_name}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Last Name
              </label>
              <input
                type="text"
                value={form.last_name}
                onChange={(e) => onChange({ ...form, last_name: e.target.value })}
                className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                  errors.last_name ? 'border-red-300' : 'border-gray-300'
                }`}
                required
              />
              {errors.last_name && (
                <p className="text-red-600 text-xs mt-1">{errors.last_name}</p>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <input
              type="email"
              value={form.email}
              onChange={(e) => onChange({ ...form, email: e.target.value })}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                errors.email ? 'border-red-300' : 'border-gray-300'
              }`}
              required
            />
            {errors.email && (
              <p className="text-red-600 text-xs mt-1">{errors.email}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Password
            </label>
            <input
              type="password"
              value={form.password}
              onChange={(e) => onChange({ ...form, password: e.target.value })}
              className={`w-full px-3 py-2 border rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500 ${
                errors.password ? 'border-red-300' : 'border-gray-300'
              }`}
              required
              minLength={8}
            />
            {errors.password && (
              <p className="text-red-600 text-xs mt-1">{errors.password}</p>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              User Type
            </label>
            <select
              value={form.user_type}
              onChange={(e) => onChange({ ...form, user_type: e.target.value as UserType })}
              className="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-slate-500 focus:border-slate-500"
            >
              <option value="driver">Driver</option>
              <option value="tester">Tester</option>
              <option value="admin">Admin</option>
            </select>
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
              <span>{loading ? 'Creating...' : 'Create User'}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}