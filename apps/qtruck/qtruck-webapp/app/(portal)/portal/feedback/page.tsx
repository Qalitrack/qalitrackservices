'use client'

import { useState, useEffect } from 'react'
import { apiClient, Feedback, FeedbackType, CreateFeedbackRequest } from '@/lib/api'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input, Select, Textarea } from '@/components/ui/FormField'
import { 
  ValidationError, 
  parseApiErrors, 
  getErrorsForField, 
  getGeneralErrors,
  validateRequired
} from '@/lib/validation'

export default function FeedbackPage() {
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState<CreateFeedbackRequest>({
    feedback_type: '' as FeedbackType,
    subject: '',
    description: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})
  const [activeTab, setActiveTab] = useState<'create' | 'history'>('create')
  const [myFeedback, setMyFeedback] = useState<Feedback[]>([])
  const [feedbackLoading, setFeedbackLoading] = useState(false)

  const feedbackTypes = [
    { value: 'feature_request' as FeedbackType, label: 'Feature Request', icon: '💡', description: 'Suggest new features or improvements' },
    { value: 'bug_report' as FeedbackType, label: 'Bug Report', icon: '🐛', description: 'Report issues or problems' },
    { value: 'general' as FeedbackType, label: 'General Feedback', icon: '💬', description: 'General comments or suggestions' },
    { value: 'complaint' as FeedbackType, label: 'Complaint', icon: '⚠️', description: 'Report issues with service or experience' },
    { value: 'suggestion' as FeedbackType, label: 'Suggestion', icon: '💭', description: 'Share ideas for improvements' }
  ]

  // Load user's feedback history
  useEffect(() => {
    if (activeTab === 'history') {
      loadMyFeedback()
    }
  }, [activeTab])

  const loadMyFeedback = async () => {
    try {
      setFeedbackLoading(true)
      const response = await apiClient.getMyFeedback({
        include: 'response_details',
        ordering: '-created_at'
      })
      const results = response.results || []
      setMyFeedback(results)
    } catch (error) {
      console.error('Error loading my feedback:', error)
      showError('Failed to load feedback history', 'Please try again.')
    } finally {
      setFeedbackLoading(false)
    }
  }

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const typeError = validateRequired(formData.feedback_type, 'Feedback type')
    if (typeError) newErrors.feedback_type = typeError
    
    const subjectError = validateRequired(formData.subject, 'Subject')
    if (subjectError) newErrors.subject = subjectError
    
    const descriptionError = validateRequired(formData.description, 'Description')
    if (descriptionError) newErrors.description = descriptionError
    
    setClientErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleInputChange = (field: keyof CreateFeedbackRequest, value: string) => {
    if (field === 'feedback_type') {
      setFormData(prev => ({ ...prev, [field]: value as FeedbackType }))
    } else {
      setFormData(prev => ({ ...prev, [field]: value }))
    }
    
    if (clientErrors[field]) {
      setClientErrors(prev => ({ ...prev, [field]: '' }))
    }
    if (getErrorsForField(errors, field).length > 0) {
      setErrors(prev => prev.filter(error => error.field !== field))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    setErrors([])
    setClientErrors({})
    
    if (!validateForm()) {
      return
    }
    
    setIsLoading(true)
    
    try {
      await apiClient.createFeedback(formData)
      showSuccess('Feedback submitted successfully!', 'Thank you for your feedback. We\'ll review it soon.')
      setFormData({ feedback_type: '' as FeedbackType, subject: '', description: '' })
      setErrors([])
      setClientErrors({})
      // Switch to history tab to show the newly submitted feedback
      setActiveTab('history')
      // Refresh feedback history to show the new submission
      loadMyFeedback()
    } catch (error) {
      const apiErrors = parseApiErrors(error)
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Failed to submit feedback', generalErrors[0])
      } else {
        showError('Failed to submit feedback', 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }


  const getStatusBadge = (status: string) => {
    const statusConfig = {
      pending: { color: 'bg-yellow-100 text-yellow-800', text: 'Pending Review' },
      reviewed: { color: 'bg-amber-100 text-amber-800', text: 'Reviewed' },
      in_progress: { color: 'bg-amber-100 text-amber-800', text: 'In Progress' },
      resolved: { color: 'bg-green-100 text-green-800', text: 'Resolved' },
      rejected: { color: 'bg-red-100 text-red-800', text: 'Rejected' }
    }
    const config = statusConfig[status as keyof typeof statusConfig] || statusConfig.pending
    return (
      <span className={`inline-flex px-2 py-1 text-xs font-medium rounded-full ${config.color}`}>
        {config.text}
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


  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Feedback</h1>
        <p className="text-gray-600">Send us your feedback or view your previous submissions</p>
      </div>

      {/* Tab Navigation */}
      <div className="border-b border-gray-200">
        <nav className="-mb-px flex space-x-8">
          <button
            onClick={() => setActiveTab('create')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'create'
                ? 'border-amber-500 text-amber-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            ✏️ Send Feedback
          </button>
          <button
            onClick={() => setActiveTab('history')}
            className={`py-2 px-1 border-b-2 font-medium text-sm ${
              activeTab === 'history'
                ? 'border-amber-500 text-amber-600'
                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
          >
            📋 My Feedback ({myFeedback.length})
          </button>
        </nav>
      </div>

      {/* Tab Content */}
      {activeTab === 'create' && (
        <div className="space-y-6">

      {/* Feedback Type Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {feedbackTypes.map((type) => (
          <div 
            key={type.value}
            className={`card cursor-pointer transition-all hover:shadow-lg ${
              formData.feedback_type === type.value 
                ? 'ring-2 ring-amber-500 bg-amber-50' 
                : 'hover:bg-gray-50'
            }`}
            onClick={() => handleInputChange('feedback_type', type.value)}
          >
            <div className="text-center">
              <div className="text-3xl mb-2">{type.icon}</div>
              <h3 className="font-semibold text-gray-900 mb-1">{type.label}</h3>
              <p className="text-sm text-gray-600">{type.description}</p>
            </div>
          </div>
        ))}
      </div>

      {/* Feedback Form */}
      <div className="card">
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Display general/non-field errors */}
          {getGeneralErrors(errors).map((error, index) => (
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

          <FormField 
            label="Feedback Type" 
            required
            errors={[...getErrorsForField(errors, 'feedback_type'), ...(clientErrors.feedback_type ? [clientErrors.feedback_type] : [])]}
          >
            <Select
              value={formData.feedback_type}
              onChange={(e) => handleInputChange('feedback_type', e.target.value)}
              errors={[...getErrorsForField(errors, 'feedback_type'), ...(clientErrors.feedback_type ? [clientErrors.feedback_type] : [])]}
            >
              <option value="">Select feedback type</option>
              {feedbackTypes.map((type) => (
                <option key={type.value} value={type.value}>
                  {type.label}
                </option>
              ))}
            </Select>
          </FormField>
          
          <FormField 
            label="Subject" 
            required
            errors={[...getErrorsForField(errors, 'subject'), ...(clientErrors.subject ? [clientErrors.subject] : [])]}
          >
            <Input
              type="text"
              value={formData.subject}
              onChange={(e) => handleInputChange('subject', e.target.value)}
              placeholder={
                formData.feedback_type === 'feature_request' ? 'GPS Integration' :
                formData.feedback_type === 'bug_report' ? 'App Crashes on Photo Upload' :
                'Brief summary of your feedback'
              }
              errors={[...getErrorsForField(errors, 'subject'), ...(clientErrors.subject ? [clientErrors.subject] : [])]}
            />
          </FormField>
          
          <FormField 
            label="Description" 
            required
            errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
          >
            <Textarea
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              placeholder={
                formData.feedback_type === 'feature_request' ? 
                  'Please describe the feature you would like to see added. How would it help you in your daily work?' :
                formData.feedback_type === 'bug_report' ? 
                  'Please describe the issue in detail. What were you doing when it happened? What did you expect to happen?' :
                  'Please provide detailed feedback, suggestions, or comments about your experience with QTruck.'
              }
              rows={6}
              errors={[...getErrorsForField(errors, 'description'), ...(clientErrors.description ? [clientErrors.description] : [])]}
            />
          </FormField>
          
          <div className="bg-gray-50 rounded-lg p-4">
            <h4 className="font-medium text-gray-900 mb-2">💡 Tips for good feedback:</h4>
            <ul className="text-sm text-gray-600 space-y-1">
              <li>• Be specific about what you experienced</li>
              <li>• Include steps to reproduce any issues</li>
              <li>• Mention your device/browser if reporting bugs</li>
              <li>• Explain how suggested features would help your work</li>
            </ul>
          </div>
          
          <div className="flex justify-end pt-4">
            <button type="submit" disabled={isLoading} className="btn btn-primary">
              {isLoading ? 'Submitting...' : 'Submit Feedback'}
            </button>
          </div>
        </form>
      </div>
        </div>
      )}

      {/* History Tab */}
      {activeTab === 'history' && (
        <div className="space-y-6">
          {feedbackLoading ? (
            <div className="text-center py-8">
              <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-amber-600"></div>
              <p className="mt-2 text-gray-600">Loading your feedback...</p>
            </div>
          ) : myFeedback.length === 0 ? (
            <div className="text-center py-12">
              <div className="text-6xl mb-4">📝</div>
              <h3 className="text-lg font-semibold text-gray-900 mb-2">No feedback submitted yet</h3>
              <p className="text-gray-600 mb-4">Your submitted feedback will appear here.</p>
              <button
                onClick={() => setActiveTab('create')}
                className="btn btn-primary"
              >
                Send Your First Feedback
              </button>
            </div>
          ) : (
            <div className="space-y-4">
              {myFeedback.map((feedback) => (
                <div key={feedback.id} className="card">
                  <div className="flex justify-between items-start mb-4">
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <span className="text-2xl">
                          {feedbackTypes.find(t => t.value === feedback.feedback_type)?.icon || '💬'}
                        </span>
                        <div>
                          <h3 className="font-semibold text-gray-900">{feedback.subject}</h3>
                          <p className="text-sm text-gray-500">
                            {feedbackTypes.find(t => t.value === feedback.feedback_type)?.label} • 
                            {formatDate(feedback.created_at)}
                          </p>
                        </div>
                      </div>
                    </div>
                    {getStatusBadge(feedback.status)}
                  </div>
                  
                  <div className="text-gray-700 mb-4">
                    {feedback.description}
                  </div>
                  
                  {feedback.admin_response && (
                    <div className="bg-amber-50 border-l-4 border-amber-400 p-4 mt-4">
                      <div className="flex items-start">
                        <div className="text-amber-400 mr-3">👨‍💼</div>
                        <div className="flex-1">
                          <h4 className="font-medium text-amber-900 mb-1">
                            Response from Admin
                            {feedback.response_date && (
                              <span className="font-normal text-sm text-amber-700 ml-2">
                                • {formatDate(feedback.response_date)}
                              </span>
                            )}
                          </h4>
                          <p className="text-amber-800">{feedback.admin_response}</p>
                          {feedback.response_metadata && (
                            <p className="text-xs text-amber-600 mt-2">
                              Response time: {feedback.response_metadata.days_to_respond} days
                            </p>
                          )}
                        </div>
                      </div>
                    </div>
                  )}
                  
                  {feedback.status === 'pending' && (
                    <div className="bg-yellow-50 border border-yellow-200 rounded p-3 mt-4">
                      <p className="text-sm text-yellow-800">
                        <span className="font-medium">⏱️ Pending review:</span> We'll review your feedback and respond soon.
                      </p>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  )
}