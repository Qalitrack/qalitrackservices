'use client'

import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react'
import { useRouter } from 'next/navigation'
import { apiClient, ApiError } from '../api/client'
import { User, LoginRequest, RegisterRequest, UserType } from '../types/api'

interface AuthState {
  user: User | null
  loading: boolean
  isAuthenticated: boolean
}

interface AuthContextType extends AuthState {
  login: (credentials: LoginRequest) => Promise<{ success: boolean; error?: string }>
  setAuthenticatedUser: (response: any, userType: UserType) => Promise<void>
  register: (data: RegisterRequest) => Promise<{ success: boolean; error?: string }>
  logout: (userType?: UserType) => Promise<void>
  refreshUser: (userType?: UserType) => Promise<void>
  getCurrentUserType: () => UserType | null
  isUserTypeLoggedIn: (userType: UserType) => boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [state, setState] = useState<AuthState>({
    user: null,
    loading: true,
    isAuthenticated: false
  })
  const router = useRouter()

  console.log('[AuthProvider] Current state:', state)

  // Track state changes
  useEffect(() => {
    console.log('[AuthProvider] State changed:', state)
  }, [state])

  // Helper function to get user type from email
  const getUserTypeFromEmail = (email: string): UserType => {
    if (email.includes('+admin@')) return 'admin'
    if (email.includes('+driver@')) return 'driver'
    if (email.includes('+tester@')) return 'tester'
    return 'driver' // default fallback
  }

  // Helper functions for user-type specific token storage
  const getTokenKey = (userType: UserType, tokenType: 'access' | 'refresh') => {
    return `${tokenType}_token_${userType}`
  }

  const getCurrentUserType = (): UserType | null => {
    return state.user?.user_type || null
  }

  const isUserTypeLoggedIn = (userType: UserType): boolean => {
    if (typeof window === 'undefined') return false
    const token = localStorage.getItem(getTokenKey(userType, 'access'))
    return !!token
  }

  // Initialize auth state on mount
  useEffect(() => {
    initializeAuth()
  }, [])

  const initializeAuth = async () => {
    try {
      console.log('[AuthContext] Initializing auth...')
      
      // Check current path to determine expected user type
      let targetUserType: UserType | null = null
      if (typeof window !== 'undefined') {
        const path = window.location.pathname
        console.log('[AuthContext] Current path:', path)
        
        if (path.includes('/dashboard')) {
          targetUserType = 'admin'
        } else if (path.includes('/portal')) {
          targetUserType = 'driver' // Could also be tester
        }
        console.log('[AuthContext] Target user type from path:', targetUserType)
      }

      // If no specific user type expected, try to find any logged in user
      if (!targetUserType) {
        console.log('[AuthContext] No target user type from path, checking all types...')
        for (const type of ['admin', 'driver', 'tester'] as UserType[]) {
          const isLoggedIn = isUserTypeLoggedIn(type)
          console.log(`[AuthContext] ${type} logged in:`, isLoggedIn)
          if (isLoggedIn) {
            targetUserType = type
            break
          }
        }
        console.log('[AuthContext] Selected target user type:', targetUserType)
      }

      if (targetUserType && isUserTypeLoggedIn(targetUserType)) {
        console.log('[AuthContext] Found valid target user type:', targetUserType)
        
        // Set up API client with the specific user type token
        const token = localStorage.getItem(getTokenKey(targetUserType, 'access'))
        const refreshToken = localStorage.getItem(getTokenKey(targetUserType, 'refresh'))
        console.log('[AuthContext] Retrieved token for', targetUserType, ':', token ? 'exists' : 'null')
        
        if (token) {
          // Set token in API client to fetch user info
          apiClient['tokenStorage'].access_token = token
          apiClient['tokenStorage'].refresh_token = refreshToken
          apiClient['tokenStorage'].user_type = targetUserType
          
          
          console.log('[AuthContext] Set API client token for user type:', targetUserType)
          
          try {
            const user = await apiClient.getCurrentUser()
            console.log('[AuthContext] Fetched current user:', user)
            
            setState({
              user,
              loading: false,
              isAuthenticated: true
            })
            console.log('[AuthContext] Auth initialized successfully - isAuthenticated: true')
            return
          } catch (error) {
            console.error('[AuthContext] Token validation failed:', error)
            // Clear invalid tokens
            localStorage.removeItem(getTokenKey(targetUserType, 'access'))
            localStorage.removeItem(getTokenKey(targetUserType, 'refresh'))
            apiClient['tokenStorage'].access_token = null
            apiClient['tokenStorage'].refresh_token = null
            apiClient['tokenStorage'].user_type = null
            console.log('[AuthContext] Cleared invalid tokens for user type:', targetUserType)
          }
        }
      }

      console.log('[AuthContext] No valid authentication found, setting unauthenticated state')
      setState({
        user: null,
        loading: false,
        isAuthenticated: false
      })
    } catch (error) {
      console.error('[AuthContext] Failed to initialize auth:', error)
      setState({
        user: null,
        loading: false,
        isAuthenticated: false
      })
    }
  }

  const login = async (credentials: LoginRequest): Promise<{ success: boolean; error?: string }> => {
    try {
      console.log('[AuthContext] Starting login for:', credentials.email)
      setState(prev => ({ ...prev, loading: true }))
      
      const userType = getUserTypeFromEmail(credentials.email)
      console.log('[AuthContext] Detected user type:', userType)
      
      const response = await apiClient.login(credentials)
      console.log('[AuthContext] API login successful, received tokens')
      
      // Store tokens with user type prefix
      if (typeof window !== 'undefined') {
        localStorage.setItem(getTokenKey(userType, 'access'), response.access)
        localStorage.setItem(getTokenKey(userType, 'refresh'), response.refresh)
        console.log('[AuthContext] Stored tokens in localStorage with keys:', 
          getTokenKey(userType, 'access'), getTokenKey(userType, 'refresh'))
      }
      
      // Set up API client for this user type
      apiClient['tokenStorage'].access_token = response.access
      apiClient['tokenStorage'].refresh_token = response.refresh
      apiClient['tokenStorage'].user_type = userType
      console.log('[AuthContext] Set up API client with user type:', userType)
      
      try {
        const user = await apiClient.getCurrentUser()
        console.log('[AuthContext] Fetched user data:', user)

        setState({
          user,
          loading: false,
          isAuthenticated: true
        })
        console.log('[AuthContext] Login successful, state updated')

        return { success: true }
      } catch (error) {
        console.error('[AuthContext] Failed to fetch user after login:', error)
        // Clear invalid tokens
        if (typeof window !== 'undefined') {
          localStorage.removeItem(getTokenKey(userType, 'access'))
          localStorage.removeItem(getTokenKey(userType, 'refresh'))
        }
        apiClient['tokenStorage'].access_token = null
        apiClient['tokenStorage'].refresh_token = null
        apiClient['tokenStorage'].user_type = null
        
        setState(prev => ({ ...prev, loading: false }))
        return { success: false, error: 'Authentication failed. Please try again.' }
      }
    } catch (error) {
      setState(prev => ({ ...prev, loading: false }))
      
      if (error instanceof ApiError) {
        // Handle specific error cases
        if (error.status === 401) {
          if (error.detail?.includes('pending approval')) {
            return { success: false, error: 'Your account is pending approval. Please contact an administrator.' }
          }
          return { success: false, error: 'Invalid email or password.' }
        }
        
        return { success: false, error: error.detail || 'Login failed.' }
      }

      return { success: false, error: 'An unexpected error occurred. Please try again.' }
    }
  }

  const setAuthenticatedUser = async (response: any, userType: UserType) => {
    try {
      // Store tokens with user type prefix
      if (typeof window !== 'undefined') {
        localStorage.setItem(getTokenKey(userType, 'access'), response.access)
        localStorage.setItem(getTokenKey(userType, 'refresh'), response.refresh)
      }
      
      // Set up API client for this user type
      apiClient['tokenStorage'].access_token = response.access
      apiClient['tokenStorage'].refresh_token = response.refresh
      apiClient['tokenStorage'].user_type = userType
      
      try {
        const user = await apiClient.getCurrentUser()
        
        setState({
          user,
          loading: false,
          isAuthenticated: true
        })
      } catch (error) {
        console.error('[AuthContext] Failed to fetch user in setAuthenticatedUser:', error)
        // Clear invalid tokens
        if (typeof window !== 'undefined') {
          localStorage.removeItem(getTokenKey(userType, 'access'))
          localStorage.removeItem(getTokenKey(userType, 'refresh'))
        }
        apiClient['tokenStorage'].access_token = null
        apiClient['tokenStorage'].refresh_token = null
        apiClient['tokenStorage'].user_type = null
        
        setState({
          user: null,
          loading: false,
          isAuthenticated: false
        })
      }
    } catch (error) {
      console.error('[AuthContext] Failed to set authenticated user:', error)
      setState({
        user: null,
        loading: false,
        isAuthenticated: false
      })
    }
  }

  const register = async (data: RegisterRequest): Promise<{ success: boolean; error?: string }> => {
    try {
      setState(prev => ({ ...prev, loading: true }))
      
      const response = await apiClient.register(data)

      setState(prev => ({ ...prev, loading: false }))

      // After successful registration, redirect to login with success message
      const statusMessage = response.status === 'approved' 
        ? 'Registration successful! You can now log in.'
        : 'Registration successful! Your account is pending approval.'
      
      router.push(`/auth/login?message=${encodeURIComponent(statusMessage)}`)

      return { success: true }
    } catch (error) {
      setState(prev => ({ ...prev, loading: false }))
      
      if (error instanceof ApiError) {
        // Handle validation errors
        const allErrors = error.getAllErrors()
        
        if (allErrors.length > 0) {
          // Return first error for simplicity, or you could return all errors
          return { success: false, error: allErrors[0].message }
        }
        
        return { success: false, error: error.detail || 'Registration failed.' }
      }

      return { success: false, error: 'An unexpected error occurred. Please try again.' }
    }
  }

  const logout = async (userType?: UserType) => {
    try {
      if (userType) {
        // Logout specific user type
        if (typeof window !== 'undefined') {
          localStorage.removeItem(getTokenKey(userType, 'access'))
          localStorage.removeItem(getTokenKey(userType, 'refresh'))
        }
        
        // If logging out current user, update state
        if (state.user?.user_type === userType) {
          setState({
            user: null,
            loading: false,
            isAuthenticated: false
          })
          router.push('/auth/login')
        }
      } else {
        // Logout current user
        await apiClient.logout()
        const currentUserType = state.user?.user_type
        
        if (currentUserType && typeof window !== 'undefined') {
          localStorage.removeItem(getTokenKey(currentUserType, 'access'))
          localStorage.removeItem(getTokenKey(currentUserType, 'refresh'))
        }
        
        setState({
          user: null,
          loading: false,
          isAuthenticated: false
        })
        router.push('/auth/login')
      }
    } catch (error) {
      console.error('Logout error:', error)
      // Always clear state on logout, even if API call fails
      setState({
        user: null,
        loading: false,
        isAuthenticated: false
      })
      router.push('/auth/login')
    }
  }

  const refreshUser = async (userType?: UserType) => {
    try {
      const targetUserType = userType || state.user?.user_type
      if (!targetUserType) return

      if (isUserTypeLoggedIn(targetUserType)) {
        const token = localStorage.getItem(getTokenKey(targetUserType, 'access'))
        if (token) {
          // Set up API client for this user type
          apiClient['tokenStorage'].access_token = token
          apiClient['tokenStorage'].user_type = targetUserType
          
          try {
            const user = await apiClient.getCurrentUser()
            setState(prev => ({
              ...prev,
              user,
              isAuthenticated: true
            }))
          } catch (error) {
            console.error('[AuthContext] Token validation failed in refreshUser:', error)
            // Clear invalid tokens
            if (typeof window !== 'undefined') {
              localStorage.removeItem(getTokenKey(targetUserType, 'access'))
              localStorage.removeItem(getTokenKey(targetUserType, 'refresh'))
            }
            apiClient['tokenStorage'].access_token = null
            apiClient['tokenStorage'].refresh_token = null
            apiClient['tokenStorage'].user_type = null
            
            setState(prev => ({
              ...prev,
              user: null,
              isAuthenticated: false
            }))
          }
        }
      }
    } catch (error) {
      console.error('Failed to refresh user:', error)
      // If refresh fails, user might need to re-login
      setState(prev => ({
        ...prev,
        user: null,
        isAuthenticated: false
      }))
    }
  }

  const contextValue: AuthContextType = {
    ...state,
    login,
    setAuthenticatedUser,
    register,
    logout,
    refreshUser,
    getCurrentUserType,
    isUserTypeLoggedIn
  }

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth(): AuthContextType {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

// Helper hook for protected routes
export function useRequireAuth(allowedTypes?: UserType[]) {
  const auth = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!auth.loading) {
      if (!auth.isAuthenticated) {
        router.push('/auth/login')
        return
      }

      if (allowedTypes && auth.user && !allowedTypes.includes(auth.user.user_type)) {
        // Redirect unauthorized user types
        if (auth.user.user_type === 'admin') {
          router.push('/dashboard')
        } else {
          router.push('/portal')
        }
        return
      }
    }
  }, [auth.loading, auth.isAuthenticated, auth.user, allowedTypes, router])

  return auth
}