'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { useAuth } from '@/lib/auth/AuthContext'
import { apiClient, ApiError } from '@/lib/api/client'
import { useToast } from '@/components/ui/Toast'
import { ButtonLoading } from '@/components/ui/Loading'
import { FormField, Input } from '@/components/ui/FormField'
import { 
  ValidationError, 
  getErrorsForField, 
  getGeneralErrors,
  validateEmail,
  validateRequired
} from '@/lib/validation'

export default function AdminLoginPage() {
  const router = useRouter()
  const { setAuthenticatedUser } = useAuth()
  const { showSuccess, showError } = useToast()
  const [formData, setFormData] = useState({
    email: '',
    password: ''
  })
  const [isLoading, setIsLoading] = useState(false)
  const [errors, setErrors] = useState<ValidationError[]>([])
  const [clientErrors, setClientErrors] = useState<Record<string, string>>({})

  const validateForm = () => {
    const newErrors: Record<string, string> = {}
    
    const emailError = validateEmail(formData.email)
    if (emailError) newErrors.email = emailError
    
    const passwordError = validateRequired(formData.password, 'Password')
    if (passwordError) newErrors.password = passwordError
    
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

  const handleSignIn = async (e: React.FormEvent) => {
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
      // Automatically append +admin to the email for admin login
      const emailWithAlias = formData.email.includes('+') 
        ? formData.email 
        : formData.email.replace('@', '+admin@')
      
      // Call API directly for detailed error handling
      const response = await apiClient.login({ 
        email: emailWithAlias, 
        password: formData.password 
      })
      
      // Set authenticated user in context
      await setAuthenticatedUser(response, 'admin')
      
      showSuccess('Login successful!', 'Welcome back to your admin dashboard.')
      
      // Small delay to ensure auth state is updated before redirect
      setTimeout(() => {
        router.push('/dashboard')
      }, 100)
    } catch (error) {
      console.log('Login error:', error)
      
      let apiErrors: ValidationError[] = []
      if (error instanceof ApiError) {
        apiErrors = error.getAllErrors()
      }
      setErrors(apiErrors)
      
      const generalErrors = getGeneralErrors(apiErrors)
      if (generalErrors.length > 0) {
        showError('Login failed', generalErrors[0])
      } else {
        showError('Login failed', 'Please check your credentials and try again.')
      }
    } finally {
      setIsLoading(false)
    }
  }


  return (
    <main className="min-h-screen flex">
      {/* Left Side - Login Form */}
      <div className="w-full lg:flex-1 flex items-center justify-center p-8 bg-white overflow-y-auto">
        <div className="max-w-md w-full space-y-8">
          <div className="text-center">
            <h2 className="text-2xl font-bold text-gray-900">
              Admin Sign In
            </h2>
            <p className="mt-2 text-gray-600">
              Welcome back! Please enter your admin credentials.
            </p>
          </div>

          <form onSubmit={handleSignIn} className="space-y-6">
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
              label="Admin Email" 
              required
              errors={[...getErrorsForField(errors, 'email'), ...(clientErrors.email ? [clientErrors.email] : [])]}
            >
              <Input
                type="email"
                value={formData.email}
                onChange={(e) => handleInputChange('email', e.target.value)}
                placeholder="admin@company.com"
                errors={[...getErrorsForField(errors, 'email'), ...(clientErrors.email ? [clientErrors.email] : [])]}
              />
            </FormField>

            <FormField 
              label="Password" 
              required
              errors={[...getErrorsForField(errors, 'password'), ...(clientErrors.password ? [clientErrors.password] : [])]}
            >
              <Input
                type="password"
                value={formData.password}
                onChange={(e) => handleInputChange('password', e.target.value)}
                placeholder="Enter your password"
                errors={[...getErrorsForField(errors, 'password'), ...(clientErrors.password ? [clientErrors.password] : [])]}
              />
            </FormField>


            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-3 px-6 rounded-lg transition-all duration-300 flex items-center justify-center"
            >
              {isLoading ? (
                <>
                  <ButtonLoading />
                  <span className="ml-2">Signing in...</span>
                </>
              ) : (
                'Sign in as Admin'
              )}
            </button>
          </form>


          <div className="text-center space-y-2">
            <Link href="/auth/login" className="text-sm text-gray-600 hover:text-gray-900 transition-colors block">
              ← Back to role selection
            </Link>
            <div className="text-sm text-gray-600">
              Don't have an admin account? <Link href="/auth/register/admin" className="text-slate-600 hover:text-slate-700">Register here</Link>
            </div>
          </div>
        </div>
      </div>

      {/* Right Side - Features */}
      <div className="hidden lg:flex lg:flex-1 bg-gradient-to-br from-slate-700 to-slate-900 relative">
        <div className="absolute inset-0 flex items-center justify-center p-12">
          <div className="text-white max-w-md">
            <div className="flex items-center justify-center space-x-3 mb-8">
              <span className="text-4xl">🚛</span>
              <span className="text-3xl font-bold text-white">QTruck</span>
            </div>
            
            <div className="text-6xl mb-6 text-center">🖥️</div>
            
            <h2 className="text-3xl font-bold mb-4 text-center">Admin Dashboard</h2>
            <p className="text-xl mb-8 opacity-90 text-center">
              Manage your fleet, monitor operations, and analyze performance with comprehensive administrative tools.
            </p>
            <div className="space-y-4">
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Complete fleet overview & control</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Driver performance monitoring</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Trip assignment & route optimization</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>Financial reporting & analytics</p>
              </div>
              <div className="flex items-start">
                <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                  <span className="text-sm">→</span>
                </div>
                <p>System administration tools</p>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  )
}