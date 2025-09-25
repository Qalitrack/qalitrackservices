'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { apiClient } from '@/lib/api/client'
import { useToast } from '@/components/ui/Toast'
import { FormField, Input } from '@/components/ui/FormField'
import { 
  ValidationError, 
  getErrorsForField, 
  getGeneralErrors,
  validateEmail,
  validatePassword,
  validatePasswordConfirm,
  validateRequired
} from '@/lib/validation'
import { ApiError } from '@/lib/api/client'

export default function DriverRegisterPage() {
  const router = useRouter()
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    password_confirm: '',
    first_name: '',
    last_name: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const firstNameError = validateRequired(formData.first_name, 'First name')
    if (firstNameError) newErrors.first_name = firstNameError
    
    const lastNameError = validateRequired(formData.last_name, 'Last name')
    if (lastNameError) newErrors.last_name = lastNameError
    
    const emailError = validateEmail(formData.email)
    if (emailError) newErrors.email = emailError
    
    
    const passwordError = validatePassword(formData.password)
    if (passwordError) newErrors.password = passwordError
    
    const confirmError = validatePasswordConfirm(formData.password, formData.password_confirm)
    if (confirmError) newErrors.password_confirm = confirmError
    
    setClientErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleInputChange = (field: string, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }))
    // Clear errors for this field when user types
    if (clientErrors[field]) {
      setClientErrors(prev => ({ ...prev, [field]: '' }))
    }
    if (getErrorsForField(errors, field).length > 0) {
      setErrors(prev => prev.filter(error => error.field !== field))
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    
    // Clear previous errors
    setErrors([])
    setClientErrors({})
    
    // Client-side validation
    if (!validateForm()) {
      return
    }
    
    setIsLoading(true)

    try {
      await apiClient.register({
        email: formData.email,
        password: formData.password,
        password_confirm: formData.password_confirm,
        first_name: formData.first_name,
        last_name: formData.last_name,
        user_type: 'driver'
      })
      showSuccess(
        'Account created successfully!', 
        'Your account is pending approval. An administrator will review and approve your account before you can log in.'
      )
      router.push('/auth/login/driver')
    } catch (error) {
      console.log('Registration error:', error)
      
      let apiErrors: ValidationError[] = []
      if (error instanceof ApiError) {
        apiErrors = error.getAllErrors()
      }
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Registration failed', generalErrors[0])
      } else {
        showError('Registration failed', 'Please check the form for errors.')
      }
    } finally {
      setIsLoading(false)
    }
  }


  return (
    <main className="min-h-screen flex">
      {/* Left Side - Registration Form */}
      <div className="w-full lg:flex-1 flex items-center justify-center p-8 bg-white overflow-y-auto">
        <div className="max-w-md w-full space-y-8">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900">
              Create Driver Account
            </h2>
            <p className="mt-2 text-gray-600">
              Set up your driver profile to get started
            </p>
          </div>

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

            <div className="grid grid-cols-2 gap-4">
              <FormField 
                label="First Name" 
                required
                errors={[...getErrorsForField(errors, 'first_name'), ...(clientErrors.first_name ? [clientErrors.first_name] : [])]}
              >
                <Input
                  type="text"
                  value={formData.first_name}
                  onChange={(e) => handleInputChange('first_name', e.target.value)}
                  placeholder="John"
                  errors={[...getErrorsForField(errors, 'first_name'), ...(clientErrors.first_name ? [clientErrors.first_name] : [])]}
                />
              </FormField>
              
              <FormField 
                label="Last Name" 
                required
                errors={[...getErrorsForField(errors, 'last_name'), ...(clientErrors.last_name ? [clientErrors.last_name] : [])]}
              >
                <Input
                  type="text"
                  value={formData.last_name}
                  onChange={(e) => handleInputChange('last_name', e.target.value)}
                  placeholder="Smith"
                  errors={[...getErrorsForField(errors, 'last_name'), ...(clientErrors.last_name ? [clientErrors.last_name] : [])]}
                />
              </FormField>
            </div>

            <FormField 
              label="Email" 
              required
              errors={[...getErrorsForField(errors, 'email'), ...(clientErrors.email ? [clientErrors.email] : [])]}
            >
              <Input
                type="email"
                value={formData.email}
                onChange={(e) => handleInputChange('email', e.target.value)}
                placeholder="your.email@company.com"
                errors={[...getErrorsForField(errors, 'email'), ...(clientErrors.email ? [clientErrors.email] : [])]}
              />
            </FormField>


            <FormField 
              label="Password" 
              required
              hint="Password must be at least 8 characters long"
              errors={[...getErrorsForField(errors, 'password'), ...(clientErrors.password ? [clientErrors.password] : [])]}
            >
              <Input
                type="password"
                value={formData.password}
                onChange={(e) => handleInputChange('password', e.target.value)}
                placeholder="Create a secure password"
                errors={[...getErrorsForField(errors, 'password'), ...(clientErrors.password ? [clientErrors.password] : [])]}
              />
            </FormField>

            <FormField 
              label="Confirm Password" 
              required
              errors={[...getErrorsForField(errors, 'password_confirm'), ...(clientErrors.password_confirm ? [clientErrors.password_confirm] : [])]}
            >
              <Input
                type="password"
                value={formData.password_confirm}
                onChange={(e) => handleInputChange('password_confirm', e.target.value)}
                placeholder="Confirm your password"
                errors={[...getErrorsForField(errors, 'password_confirm'), ...(clientErrors.password_confirm ? [clientErrors.password_confirm] : [])]}
              />
            </FormField>


            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-gradient-to-r from-amber-500 to-amber-600 hover:from-amber-600 hover:to-amber-700 text-white font-medium py-3 px-6 rounded-lg transition-all duration-300"
            >
              {isLoading ? 'Creating Account...' : 'Create Driver Account'}
            </button>
          </form>

          <div className="text-center space-y-2">
            <Link href="/auth/register" className="text-sm text-gray-600 hover:text-gray-900 transition-colors block">
              ← Back to role selection
            </Link>
            <div className="text-sm text-gray-600">
              Already have a driver account? <Link href="/auth/login/driver" className="text-amber-600 hover:text-amber-700">Log in</Link>
            </div>
          </div>
        </div>
      </div>

      {/* Right Side - Features */}
      <div className="hidden lg:flex lg:flex-1 bg-gradient-to-br from-amber-500 to-teal-600 relative">
        <div className="absolute inset-0 flex items-center justify-center p-12">
          <div className="text-white max-w-md">
            <div className="flex items-center justify-center space-x-3 mb-8">
              <span className="text-4xl">🚛</span>
              <span className="text-3xl font-bold text-white">QTruck</span>
            </div>
            
            <div className="text-6xl mb-6 text-center">📱</div>
            
            <h2 className="text-3xl font-bold mb-4 text-center">Join as a Driver</h2>
            <p className="text-xl mb-8 opacity-90 text-center">
              Create your driver account and start managing trips efficiently with our comprehensive platform.
            </p>
            <div className="space-y-4">
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Easy trip management & tracking</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Expense recording with receipts</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Real-time communication with dispatch</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Performance bonuses & incentives</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Mobile-optimized interface</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}