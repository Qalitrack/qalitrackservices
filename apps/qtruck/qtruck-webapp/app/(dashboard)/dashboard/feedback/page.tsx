'use client'

import { useState, useEffect, useCallback } from 'react'
import { apiClient, ApiError, Feedback, FeedbackStatus, FeedbackType, RespondToFeedbackRequest } from '@/lib/api'
import { useAuth } from '@/lib/auth/AuthContext'
import { useToast } from '@/components/ui/Toast'
import { ConfirmDialog } from '@/components/ui/Dialog'
import { FormField, Textarea, Select } from '@/components/ui/FormField'

interface FeedbackStats {
  pending: number
  reviewed: number
  in_progress: number
  resolved: number
  rejected: number
  total: number
}

type TabType = 'pending' | 'reviewed' | 'in_progress' | 'resolved' | 'rejected' | 'all'

export default function FeedbackManagementPage() {
  const { user: currentUser } = useAuth()
  const { showSuccess, showError } = useToast()

  // State management
  const [feedback, setFeedback] = useState<Feedback[]>([])
  const [loading, setLoading] = useState(true)
  const [activeTab, setActiveTab] = useState<TabType>('all')
  const [searchTerm, setSearchTerm] = useState('')
  const [stats, setStats] = useState<FeedbackStats>({
    pending: 0,
    reviewed: 0,
    in_progress: 0,
    resolved: 0,
    rejected: 0,
    total: 0
  })

  // Dialog states
  const [respondDialog, setRespondDialog] = useState<{
    open: boolean
    feedback: Feedback | null
    loading: boolean
    response: string
    status: FeedbackStatus
  }>({
    open: false,
    feedback: null,
    loading: false,
    response: '',
    status: 'reviewed'
  })

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

  const feedbackTypes: { value: FeedbackType; label: string; icon: string; color: string }[] = [
    { value: 'feature_request', label: 'Feature Request', icon: '💡', color: 'bg-slate-100 text-slate-800' },
    { value: 'bug_report', label: 'Bug Report', icon: '🐛', color: 'bg-red-100 text-red-800' },
    { value: 'general', label: 'General Feedback', icon: '💬', color: 'bg-gray-100 text-gray-800' },
    { value: 'complaint', label: 'Complaint', icon: '⚠️', color: 'bg-slate-100 text-slate-800' },
    { value: 'suggestion', label: 'Suggestion', icon: '💭', color: 'bg-slate-100 text-slate-800' }
  ]

  // Load feedback data
  const loadFeedback = useCallback(async () => {
    try {
      setLoading(true)
      
      // First, get all feedback for stats calculation (no filters)
      const allFeedbackResponse = await apiClient.getFeedback({
        include: 'response_details,user_profile',
        ordering: '-created_at'
      })
      const allResults = allFeedbackResponse.results || []
      
      // Calculate stats from all feedback
      const newStats = {
        pending: allResults.filter(f => f.status === 'pending').length,
        reviewed: allResults.filter(f => f.status === 'reviewed').length,
        in_progress: allResults.filter(f => f.status === 'in_progress').length,
        resolved: allResults.filter(f => f.status === 'resolved').length,
        rejected: allResults.filter(f => f.status === 'rejected').length,
        total: allResults.length
      }
      setStats(newStats)

      // Then apply filters for display
      let filteredResults = allResults

      // Filter by active tab
      if (activeTab !== 'all') {
        filteredResults = filteredResults.filter(f => f.status === activeTab)
      }

      // Filter by search term
      if (searchTerm.trim()) {
        const searchLower = searchTerm.trim().toLowerCase()
        filteredResults = filteredResults.filter(f => 
          f.subject?.toLowerCase().includes(searchLower) ||
          f.description?.toLowerCase().includes(searchLower) ||
          f.user_profile?.first_name?.toLowerCase().includes(searchLower) ||
          f.user_profile?.last_name?.toLowerCase().includes(searchLower) ||
          f.user_profile?.email?.toLowerCase().includes(searchLower)
        )
      }

      setFeedback(filteredResults)

    } catch (error) {
      showError('Failed to load feedback', error instanceof ApiError ? error.message : 'Unknown error')
    } finally {
      setLoading(false)
    }
  }, [activeTab, searchTerm, showError])

  useEffect(() => {
    loadFeedback()
  }, [activeTab])

  // Debounced search effect
  useEffect(() => {
    const timer = setTimeout(() => {
      loadFeedback()
    }, 500)

    return () => clearTimeout(timer)
  }, [searchTerm])

  const getStatusBadge = (status: FeedbackStatus) => {
    const statusConfig = {
      pending: { color: 'bg-yellow-100 text-yellow-800', text: 'Pending Review' },
      reviewed: { color: 'bg-slate-100 text-slate-800', text: 'Reviewed' },
      in_progress: { color: 'bg-purple-100 text-purple-800', text: 'In Progress' },
      resolved: { color: 'bg-green-100 text-green-800', text: 'Resolved' },
      rejected: { color: 'bg-red-100 text-red-800', text: 'Rejected' }
    }
    const config = statusConfig[status] || statusConfig.pending
    return (
      <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${config.color}`}>
        {config.text}
      </span>
    )
  }

  const getTypeBadge = (type: FeedbackType) => {
    const typeConfig = feedbackTypes.find(t => t.value === type)
    if (!typeConfig) return null
    
    return (
      <span className={`inline-flex items-center px-2 py-1 text-xs font-medium rounded-full ${typeConfig.color}`}>
        <span className="mr-1">{typeConfig.icon}</span>
        {typeConfig.label}
      </span>
    )
  }

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  const handleRespondToFeedback = (feedback: Feedback) => {
    setRespondDialog({
      open: true,
      feedback,
      loading: false,
      response: '',
      status: 'reviewed'
    })
  }

  const submitResponse = async () => {
    if (!respondDialog.feedback) return

    try {
      setRespondDialog(prev => ({ ...prev, loading: true }))
      
      const data: RespondToFeedbackRequest = {
        admin_response: respondDialog.response,
        status: respondDialog.status
      }

      await apiClient.respondToFeedback(respondDialog.feedback.id, data)
      
      showSuccess('Response sent successfully', 'The feedback has been updated with your response.')
      
      setRespondDialog({
        open: false,
        feedback: null,
        loading: false,
        response: '',
        status: 'reviewed'
      })
      
      // Reload feedback
      loadFeedback()
      
    } catch (error) {
      showError('Failed to send response', error instanceof ApiError ? error.message : 'Unknown error')
    } finally {
      setRespondDialog(prev => ({ ...prev, loading: false }))
    }
  }

  const handleQuickAction = (feedback: Feedback, action: 'resolve' | 'reject') => {
    console.log('handleQuickAction called:', action, feedback.id)
    const actionConfig = {
      resolve: {
        title: 'Resolve Feedback',
        message: `Are you sure you want to mark this feedback as resolved? This indicates the issue has been addressed.`,
        variant: 'info' as const
      },
      reject: {
        title: 'Reject Feedback',
        message: `Are you sure you want to reject this feedback? This will mark it as not actionable.`,
        variant: 'danger' as const
      }
    }

    const config = actionConfig[action]
    
    setConfirmAction({
      open: true,
      title: config.title,
      message: config.message,
      variant: config.variant,
      onConfirm: async () => {
        try {
          console.log(`${action.charAt(0).toUpperCase() + action.slice(1)}ing feedback:`, feedback.id)
          if (action === 'resolve') {
            const result = await apiClient.resolveFeedback(feedback.id)
            console.log('Resolve result:', result)
            showSuccess('Feedback resolved', 'The feedback has been marked as resolved.')
          } else {
            const result = await apiClient.rejectFeedback(feedback.id)
            console.log('Reject result:', result)
            showSuccess('Feedback rejected', 'The feedback has been marked as rejected.')
          }
          loadFeedback()
        } catch (error) {
          console.error(`Error ${action}ing feedback:`, error)
          showError(`Failed to ${action} feedback`, error instanceof ApiError ? error.message : 'Unknown error')
        }
        setConfirmAction(prev => ({ ...prev, open: false }))
      }
    })
  }

  const tabs: { key: TabType; label: string; count: number; color: string }[] = [
    { key: 'all', label: 'All Feedback', count: stats.total, color: 'text-gray-600' },
    { key: 'pending', label: 'Pending', count: stats.pending, color: 'text-yellow-600' },
    { key: 'in_progress', label: 'In Progress', count: stats.in_progress, color: 'text-purple-600' },
    { key: 'reviewed', label: 'Reviewed', count: stats.reviewed, color: 'text-slate-600' },
    { key: 'resolved', label: 'Resolved', count: stats.resolved, color: 'text-green-600' },
    { key: 'rejected', label: 'Rejected', count: stats.rejected, color: 'text-red-600' },
  ]

  if (loading && feedback.length === 0) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-slate-600"></div>
          <p className="mt-2 text-gray-600">Loading feedback...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Feedback Management</h1>
        <p className="text-gray-600">Review and respond to user feedback</p>
      </div>

      {/* Search */}
      <div className="flex justify-between items-center">
        <div className="flex-1 max-w-lg">
          <input
            type="text"
            placeholder="Search feedback..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-slate-500 focus:border-slate-500"
          />
        </div>
      </div>

      {/* Tabs */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          {tabs.map((tab) => (
            <button
              key={tab.key}
              onClick={() => setActiveTab(tab.key)}
              className={`py-2 px-1 border-b-2 font-medium text-sm ${
                activeTab === tab.key
                  ? 'border-slate-500 text-slate-600'
                  : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
              }`}
            >
              {tab.label} ({tab.count})
            </button>
          ))}
        </nav>
      </div>

      {/* Feedback List */}
      {feedback.length === 0 ? (
        <div className="text-center py-12">
          <div className="text-6xl mb-4">📝</div>
          <h3 className="text-lg font-semibold text-gray-900 mb-2">No feedback found</h3>
          <p className="text-gray-600">
            {searchTerm ? 'Try adjusting your search terms.' : 'No feedback has been submitted yet.'}
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          {feedback.map((item) => (
            <div key={item.id} className="card">
              <div className="flex justify-between items-start mb-4">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <div>
                      <h3 className="font-semibold text-gray-900">{item.subject}</h3>
                      <div className="flex items-center gap-2 text-sm text-gray-500">
                        <span>by {item.user.full_name}</span>
                        <span>•</span>
                        <span>{formatDate(item.created_at)}</span>
                      </div>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  {getTypeBadge(item.feedback_type)}
                  {getStatusBadge(item.status)}
                </div>
              </div>

              <div className="text-gray-700 mb-4">
                {item.description}
              </div>

              {/* Admin Response */}
              {item.admin_response && (
                <div className="bg-slate-50 border-l-4 border-slate-400 p-4 mb-4">
                  <div className="flex items-start">
                    <div className="text-slate-400 mr-3">👨‍💼</div>
                    <div className="flex-1">
                      <h4 className="font-medium text-slate-900 mb-1">
                        Your Response
                        {item.response_date && (
                          <span className="font-normal text-sm text-slate-700 ml-2">
                            • {formatDate(item.response_date)}
                          </span>
                        )}
                      </h4>
                      <p className="text-slate-800">{item.admin_response}</p>
                      {item.response_metadata && (
                        <p className="text-xs text-slate-600 mt-2">
                          Response time: {item.response_metadata.days_to_respond} days
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              )}

              {/* Actions */}
              <div className="flex justify-end gap-2 pt-4 border-t">
                {/* Hide edit buttons for resolved/rejected feedback */}
                {item.status !== 'resolved' && item.status !== 'rejected' && (
                  <button
                    onClick={() => handleRespondToFeedback(item)}
                    className="bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
                  >
                    {item.admin_response ? 'Update Response' : 'Respond'}
                  </button>
                )}
                
                {item.status !== 'resolved' && item.status !== 'rejected' && (
                  <button
                    onClick={() => handleQuickAction(item, 'resolve')}
                    className="bg-gradient-to-r from-green-600 to-green-700 hover:from-green-700 hover:to-green-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
                  >
                    ✅ Resolve
                  </button>
                )}
                
                {item.status !== 'rejected' && item.status !== 'resolved' && (
                  <button
                    onClick={() => handleQuickAction(item, 'reject')}
                    className="bg-gradient-to-r from-red-600 to-red-700 hover:from-red-700 hover:to-red-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200"
                  >
                    ❌ Reject
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Response Dialog */}
      {respondDialog.open && respondDialog.feedback && (
        <div className="fixed inset-0 z-50 overflow-y-auto">
          <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
            <div className="fixed inset-0 bg-gray-500 bg-opacity-75 transition-opacity" 
                 onClick={() => setRespondDialog(prev => ({ ...prev, open: false }))}></div>

            <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
              <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                <div className="mb-4">
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    Respond to Feedback
                  </h3>
                  <div className="bg-gray-50 p-3 rounded">
                    <p className="font-medium text-gray-900">{respondDialog.feedback.subject}</p>
                    <p className="text-sm text-gray-600 mt-1">{respondDialog.feedback.description}</p>
                  </div>
                </div>

                <FormField label="Status" required>
                  <Select
                    value={respondDialog.status}
                    onChange={(e) => setRespondDialog(prev => ({ 
                      ...prev, 
                      status: e.target.value as FeedbackStatus 
                    }))}
                  >
                    <option value="reviewed">Reviewed</option>
                    <option value="in_progress">In Progress</option>
                    <option value="resolved">Resolved</option>
                    <option value="rejected">Rejected</option>
                  </Select>
                </FormField>

                <FormField label="Response" required>
                  <Textarea
                    value={respondDialog.response}
                    onChange={(e) => setRespondDialog(prev => ({ 
                      ...prev, 
                      response: e.target.value 
                    }))}
                    placeholder="Write your response to the user..."
                    rows={4}
                  />
                </FormField>
              </div>

              <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                <button
                  onClick={submitResponse}
                  disabled={respondDialog.loading || !respondDialog.response.trim()}
                  className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 w-full sm:w-auto sm:ml-3 disabled:opacity-50"
                >
                  {respondDialog.loading ? 'Sending...' : 'Send Response'}
                </button>
                <button
                  onClick={() => setRespondDialog(prev => ({ ...prev, open: false }))}
                  className="bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 mt-3 w-full sm:mt-0 sm:w-auto"
                >
                  Cancel
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* Confirm Dialog */}
      <ConfirmDialog
        isOpen={confirmAction.open}
        onClose={() => setConfirmAction(prev => ({ ...prev, open: false }))}
        onConfirm={confirmAction.onConfirm}
        title={confirmAction.title}
        message={confirmAction.message}
        type={confirmAction.variant}
      />
    </div>
  )
}