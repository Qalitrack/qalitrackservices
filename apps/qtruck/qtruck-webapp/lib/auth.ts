// Re-export the new auth system
export { useAuth } from './auth/AuthContext'
export * from './types/api'

// Legacy auth service for backward compatibility
export const authService = {
  async login(email: string, password: string, role: string) {
    // This is deprecated - components should use useAuth() hook instead
    console.warn('authService.login() is deprecated. Use useAuth() hook instead.')
    return { success: false, error: 'Use useAuth() hook instead' }
  },
  
  async register(userData: any, role: string) {
    // This is deprecated - components should use useAuth() hook instead  
    console.warn('authService.register() is deprecated. Use useAuth() hook instead.')
    return { success: false, error: 'Use useAuth() hook instead' }
  }
}