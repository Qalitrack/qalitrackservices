'use client'

import Link from 'next/link'
import { useAuth } from '@/lib/auth/AuthContext'
import { useEffect, useState } from 'react'

export default function PendingApprovalPage() {
  const { user, logout } = useAuth()
  const [userStatus, setUserStatus] = useState<string>('')

  useEffect(() => {
    if (user) {
      setUserStatus(user.status)
    }
  }, [user])

  const handleLogout = async () => {
    await logout()
  }

  const getStatusMessage = () => {
    switch (userStatus) {
      case 'preapproval':
        return {
          title: 'Account Pending Approval',
          message: 'Your account registration is pending approval from an administrator. You will be notified once your account is approved.',
          icon: '⏳'
        }
      case 'rejected':
        return {
          title: 'Account Access Denied',
          message: 'Your account access has been rejected. Please contact an administrator for more information.',
          icon: '❌'
        }
      default:
        return {
          title: 'Account Status Unknown',
          message: 'There seems to be an issue with your account status. Please contact support.',
          icon: '❓'
        }
    }
  }

  const statusInfo = getStatusMessage()

  return (
    <main className="min-h-screen flex items-center justify-center bg-gray-50">
      <div className="max-w-md w-full mx-4">
        <div className="bg-white rounded-xl shadow-lg p-8 text-center">
          <div className="text-6xl mb-6">{statusInfo.icon}</div>
          
          <h1 className="text-2xl font-bold text-gray-900 mb-4">
            {statusInfo.title}
          </h1>
          
          <p className="text-gray-600 mb-8 leading-relaxed">
            {statusInfo.message}
          </p>

          {user && (
            <div className="bg-gray-50 rounded-lg p-4 mb-6">
              <div className="text-sm text-gray-700">
                <p><strong>Account:</strong> {user.full_name}</p>
                <p><strong>Email:</strong> {user.email}</p>
                <p><strong>User Type:</strong> {user.user_type}</p>
                <p><strong>Status:</strong> <span className={`font-semibold ${
                  userStatus === 'preapproval' ? 'text-yellow-600' :
                  userStatus === 'rejected' ? 'text-red-600' : 'text-gray-600'
                }`}>{userStatus}</span></p>
              </div>
            </div>
          )}

          <div className="space-y-3">
            {userStatus === 'preapproval' && (
              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <div className="flex items-start">
                  <div className="flex-shrink-0">
                    <svg className="h-5 w-5 text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <p className="text-sm text-blue-700">
                      <strong>What happens next?</strong><br/>
                      An administrator will review your account and approve or reject your access. This typically takes 1-2 business days.
                    </p>
                  </div>
                </div>
              </div>
            )}

            {userStatus === 'rejected' && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <div className="flex items-start">
                  <div className="flex-shrink-0">
                    <svg className="h-5 w-5 text-red-400" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a1 1 0 000 2v3a1 1 0 001 1h1a1 1 0 100-2v-3a1 1 0 00-1-1H9z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <p className="text-sm text-red-700">
                      <strong>Need help?</strong><br/>
                      If you believe this is an error, please contact your administrator or support team for assistance.
                    </p>
                  </div>
                </div>
              </div>
            )}

            <div className="flex space-x-3">
              <button
                onClick={handleLogout}
                className="flex-1 bg-gray-600 hover:bg-gray-700 text-white font-medium py-3 px-4 rounded-lg transition-colors"
              >
                Sign Out
              </button>
              <Link 
                href="/auth/login"
                className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-medium py-3 px-4 rounded-lg transition-colors text-center"
              >
                Back to Login
              </Link>
            </div>
          </div>

          <div className="mt-6 pt-4 border-t border-gray-200">
            <p className="text-xs text-gray-500">
              Need immediate assistance? Contact support at support@qtruck.com
            </p>
          </div>
        </div>
      </div>
    </main>
  )
}