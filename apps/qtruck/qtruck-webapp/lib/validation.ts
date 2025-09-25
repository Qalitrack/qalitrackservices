export interface ValidationError {
  field: string
  message: string
}

export interface ApiError {
  message?: string
  errors?: Record<string, string[]>
}

export function parseApiErrors(error: any): ValidationError[] {
  const errors: ValidationError[] = []
  
  if (error?.response?.data?.errors) {
    // Django REST framework error format
    const apiErrors = error.response.data.errors
    for (const [field, messages] of Object.entries(apiErrors)) {
      if (Array.isArray(messages)) {
        messages.forEach((message: string) => {
          errors.push({ field, message })
        })
      } else if (typeof messages === 'string') {
        errors.push({ field, message: messages })
      }
    }
  } else if (error?.response?.data) {
    // Check for field-specific errors in response data
    const data = error.response.data
    for (const [field, messages] of Object.entries(data)) {
      if (Array.isArray(messages)) {
        messages.forEach((message: string) => {
          errors.push({ field, message })
        })
      } else if (typeof messages === 'string' && field !== 'message') {
        errors.push({ field, message: messages })
      }
    }
  }
  
  return errors
}

export function getErrorsForField(errors: ValidationError[], fieldName: string): string[] {
  return errors
    .filter(error => error.field === fieldName)
    .map(error => error.message)
}

export function hasErrorsForField(errors: ValidationError[], fieldName: string): boolean {
  return errors.some(error => error.field === fieldName)
}

export function getGeneralErrors(errors: ValidationError[]): string[] {
  return errors
    .filter(error => ['non_field_errors', 'general', '__all__', 'message'].includes(error.field))
    .map(error => error.message)
}

// Client-side validation helpers
export function validateEmail(email: string): string | null {
  if (!email) return 'Email is required'
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return 'Please enter a valid email address'
  return null
}

export function validatePassword(password: string): string | null {
  if (!password) return 'Password is required'
  if (password.length < 8) return 'Password must be at least 8 characters long'
  return null
}

export function validatePasswordConfirm(password: string, confirmPassword: string): string | null {
  if (!confirmPassword) return 'Please confirm your password'
  if (password !== confirmPassword) return 'Passwords do not match'
  return null
}

export function validateRequired(value: string, fieldName: string): string | null {
  if (!value || value.trim() === '') return `${fieldName} is required`
  return null
}