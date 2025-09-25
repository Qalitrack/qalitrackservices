'use client'

import { useState, useEffect } from 'react'
import Link from 'next/link'
import { useAuth } from '@/lib/auth/AuthContext'

export default function LoginRoleSelectionPage() {
  const [selectedRole, setSelectedRole] = useState<'driver' | 'admin' | null>('driver')
  const { user, isAuthenticated, logout, isUserTypeLoggedIn } = useAuth()
  const [loggedInUsers, setLoggedInUsers] = useState<{driver?: boolean, admin?: boolean}>({})

  useEffect(() => {
    // Check which user types are logged in
    const driverLoggedIn = isUserTypeLoggedIn('driver')
    const adminLoggedIn = isUserTypeLoggedIn('admin')
    setLoggedInUsers({ driver: driverLoggedIn, admin: adminLoggedIn })
  }, [isUserTypeLoggedIn])

  const handleRoleSelect = (role: 'driver' | 'admin') => {
    setSelectedRole(role)
  }

  const handleContinue = () => {
    if (selectedRole) {
      // Navigate to role-specific login page
      window.location.href = `/auth/login/${selectedRole}`
    }
  }

  const handleGoToPortal = (userType: 'driver' | 'admin') => {
    if (userType === 'driver') {
      window.location.href = '/portal'
    } else {
      window.location.href = '/dashboard'
    }
  }

  const handleLogout = async (userType: 'driver' | 'admin') => {
    await logout(userType)
    setLoggedInUsers(prev => ({ ...prev, [userType]: false }))
  }

  return (
    <main className="min-h-screen flex">
      {/* Left Side - Role Selection */}
      <div className="flex-1 flex items-center justify-center p-8 bg-white">
        <div className="max-w-md w-full space-y-8">
          <div className="text-center">
            <div className="flex items-center justify-center space-x-3 mb-6">
              <span className="text-4xl">🚛</span>
              <span className="text-3xl font-bold text-amber-500">QTruck</span>
            </div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Welcome Back</h1>
            <p className="text-gray-600">Choose how you'd like to sign in</p>
          </div>

          <div className="space-y-4">
            {/* Driver Card */}
            {loggedInUsers.driver ? (
              <div className="border-2 border-green-500 bg-green-50 rounded-xl p-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className="bg-green-100 p-3 rounded-full mr-4">
                      <span className="text-2xl">✅</span>
                    </div>
                    <div className="flex-1">
                      <h3 className="font-bold text-gray-900">You're signed in as Driver</h3>
                      <p className="text-gray-600 text-sm">Ready to manage your trips</p>
                    </div>
                  </div>
                  <div className="flex space-x-2">
                    <button
                      onClick={() => handleGoToPortal('driver')}
                      className="btn btn-sm btn-primary"
                    >
                      Go to Portal
                    </button>
                    <button
                      onClick={() => handleLogout('driver')}
                      className="btn btn-sm btn-outline text-gray-600"
                    >
                      Sign Out
                    </button>
                  </div>
                </div>
              </div>
            ) : (
              <div
                className={`cursor-pointer hover:border-amber-500 transition-all border-2 rounded-xl p-6 ${
                  selectedRole === "driver" ? "border-amber-500 bg-amber-50" : "border-gray-200"
                }`}
                onClick={() => handleRoleSelect("driver")}
              >
                <div className="flex items-center">
                  <div className="bg-amber-100 p-3 rounded-full mr-4">
                    <span className="text-2xl">📱</span>
                  </div>
                  <div className="flex-1">
                    <h3 className="font-bold text-gray-900">I'm a Driver</h3>
                    <p className="text-gray-600 text-sm">Access my trips and manage expenses</p>
                  </div>
                  {selectedRole === "driver" && <span className="text-amber-500 text-xl">✓</span>}
                </div>
              </div>
            )}

            {/* Admin Card */}
            {loggedInUsers.admin ? (
              <div className="border-2 border-green-500 bg-green-50 rounded-xl p-6">
                <div className="flex items-center justify-between">
                  <div className="flex items-center">
                    <div className="bg-green-100 p-3 rounded-full mr-4">
                      <span className="text-2xl">✅</span>
                    </div>
                    <div className="flex-1">
                      <h3 className="font-bold text-gray-900">You're signed in as Admin</h3>
                      <p className="text-gray-600 text-sm">Ready to manage your fleet</p>
                    </div>
                  </div>
                  <div className="flex space-x-2">
                    <button
                      onClick={() => handleGoToPortal('admin')}
                      className="btn btn-sm bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700 text-white"
                    >
                      Go to Dashboard
                    </button>
                    <button
                      onClick={() => handleLogout('admin')}
                      className="btn btn-sm btn-outline text-gray-600"
                    >
                      Sign Out
                    </button>
                  </div>
                </div>
              </div>
            ) : (
              <div
                className={`cursor-pointer hover:border-slate-500 transition-all border-2 rounded-xl p-6 ${
                  selectedRole === "admin" ? "border-slate-500 bg-slate-50" : "border-gray-200"
                }`}
                onClick={() => handleRoleSelect("admin")}
              >
                <div className="flex items-center">
                  <div className="bg-slate-100 p-3 rounded-full mr-4">
                    <span className="text-2xl">🖥️</span>
                  </div>
                  <div className="flex-1">
                    <h3 className="font-bold text-gray-900">I'm an Admin</h3>
                    <p className="text-gray-600 text-sm">Manage fleet and operations</p>
                  </div>
                  {selectedRole === "admin" && <span className="text-slate-500 text-xl">✓</span>}
                </div>
              </div>
            )}

            {/* Only show continue button if not all roles are logged in */}
            {!(loggedInUsers.driver && loggedInUsers.admin) && (
              <button
                className={`w-full py-4 px-6 rounded-lg font-medium text-white transition-all duration-300 mt-6 ${
                  (selectedRole === 'driver' && !loggedInUsers.driver) ? 'btn btn-primary' :
                  (selectedRole === 'admin' && !loggedInUsers.admin) ? 'bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700' :
                  'bg-gray-400 cursor-not-allowed'
                }`}
                disabled={!selectedRole || (selectedRole === 'driver' && loggedInUsers.driver) || (selectedRole === 'admin' && loggedInUsers.admin)}
                onClick={handleContinue}
              >
                Continue to Sign In
              </button>
            )}

            <p className="text-center text-sm text-gray-500 mt-4">
              Need an account?{" "}
              <Link href="/auth/register" className="text-amber-600 hover:text-amber-700 font-medium">
                Register here
              </Link>
            </p>
          </div>
        </div>
      </div>

      {/* Right Side - Dynamic Content */}
      <div className={`flex-1 relative transition-all duration-500 ${
        selectedRole === 'driver' 
          ? 'bg-gradient-to-br from-amber-500 to-teal-600' 
          : selectedRole === 'admin'
          ? 'bg-gradient-to-br from-slate-600 to-slate-800'
          : 'bg-gradient-to-br from-gray-600 to-gray-800'
      }`}>
        <div className="absolute inset-0 flex items-center justify-center p-12">
          <div className="text-white max-w-md">
            {selectedRole === 'driver' ? (
              <>
                <h2 className="text-3xl font-bold mb-4">Driver Portal Access</h2>
                <p className="text-xl mb-6 opacity-90">
                  Manage your trips, track expenses, and optimize your daily operations with QTruck.
                </p>
                <div className="space-y-4">
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Trip management & real-time tracking</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Expense recording with receipt upload</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Vehicle mileage & fuel logging</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Performance metrics & ratings</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Mobile-optimized interface</p>
                  </div>
                </div>
                
              </>
            ) : selectedRole === 'admin' ? (
              <>
                <h2 className="text-3xl font-bold mb-4">Admin Dashboard Access</h2>
                <p className="text-xl mb-6 opacity-90">
                  Oversee your entire fleet, manage drivers, and optimize operations with comprehensive tools.
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
                
              </>
            ) : (
              <>
                <h2 className="text-3xl font-bold mb-4">Choose Your Access Type</h2>
                <p className="text-xl mb-6 opacity-90">
                  Select a role to see specific features and demo credentials.
                </p>
                <div className="space-y-4">
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Real-time fleet monitoring</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Comprehensive trip management</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Advanced analytics & reporting</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">→</span>
                    </div>
                    <p>Mobile & web accessibility</p>
                  </div>
                </div>
              </>
            )}
          </div>
        </div>
      </div>

      {/* Navigation */}
      <div className="absolute top-4 left-4">
        <Link href="/auth" className="flex items-center space-x-2 text-gray-600 hover:text-gray-900 transition-colors">
          <span>←</span>
          <span>Back</span>
        </Link>
      </div>
    </main>
  )
}