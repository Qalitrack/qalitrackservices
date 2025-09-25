'use client'

import { useAuth } from '@/lib/auth/AuthContext'
import { PageLoading } from '@/components/ui/Loading'
import { useRouter } from 'next/navigation'
import { useEffect } from 'react'

interface ProtectedRouteProps {
  children: React.ReactNode
  allowedUserTypes?: ('admin' | 'driver' | 'tester')[]
  redirectTo?: string
}

export function ProtectedRoute({ 
  children, 
  allowedUserTypes = ['admin', 'driver', 'tester'],
  redirectTo = '/auth/login'
}: ProtectedRouteProps) {
  const { user, loading, isAuthenticated } = useAuth()
  const router = useRouter()

  useEffect(() => {
    console.log('[ProtectedRoute] Auth check - loading:', loading, 'isAuthenticated:', isAuthenticated, 'user:', user)
    
    if (!loading) {
      if (!isAuthenticated) {
        console.log('[ProtectedRoute] User not authenticated, redirecting to:', redirectTo)
        router.push(redirectTo)
        return
      }

      if (user && !allowedUserTypes.includes(user.user_type)) {
        console.log('[ProtectedRoute] User type', user.user_type, 'not in allowed types:', allowedUserTypes)
        // Redirect based on user type
        if (user.user_type === 'admin') {
          console.log('[ProtectedRoute] Redirecting admin to /dashboard')
          router.push('/dashboard')
        } else if (user.user_type === 'driver' || user.user_type === 'tester') {
          console.log('[ProtectedRoute] Redirecting driver/tester to /portal')
          router.push('/portal')
        } else {
          console.log('[ProtectedRoute] Unknown user type, redirecting to login')
          router.push('/auth/login')
        }
        return
      }

      if (user && user.status !== 'approved') {
        console.log('[ProtectedRoute] User status is', user.status, 'not approved, redirecting to pending')
        router.push('/auth/pending')
        return
      }
      
      console.log('[ProtectedRoute] All checks passed, allowing access')
    } else {
      console.log('[ProtectedRoute] Still loading...')
    }
  }, [user, loading, isAuthenticated, allowedUserTypes, redirectTo, router])

  if (loading) {
    return <PageLoading text="Verifying authentication..." />
  }

  if (!isAuthenticated) {
    return <PageLoading text="Redirecting to login..." />
  }

  if (user && !allowedUserTypes.includes(user.user_type)) {
    return <PageLoading text="Redirecting..." />
  }

  if (user && user.status !== 'approved') {
    return <PageLoading text="Redirecting to approval page..." />
  }

  return <>{children}</>
}

export function AdminRoute({ children }: { children: React.ReactNode }) {
  return (
    <ProtectedRoute allowedUserTypes={['admin']}>
      {children}
    </ProtectedRoute>
  )
}

export function DriverTestersRoute({ children }: { children: React.ReactNode }) {
  return (
    <ProtectedRoute allowedUserTypes={['driver', 'tester']}>
      {children}
    </ProtectedRoute>
  )
}