'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'

export default function RegisterPage() {
  const router = useRouter()
  const [selectedRole, setSelectedRole] = useState<'driver' | 'admin' | null>('driver')

  const handleRoleSelect = (role: 'driver' | 'admin') => {
    setSelectedRole(role)
  }

  const handleContinue = () => {
    if (selectedRole) {
      // Navigate to role-specific registration page
      router.push(`/auth/register/${selectedRole}`)
    }
  }

  return (
    <main className="min-h-screen flex flex-col md:flex-row">
      {/* Left Section - Content */}
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="max-w-md w-full">
          <div className="text-center mb-8">
            <div className="flex items-center justify-center space-x-3 mb-6">
              <span className="text-4xl">🚛</span>
              <span className="text-3xl font-bold text-amber-500">QTruck</span>
            </div>
            <h1 className="text-3xl font-bold text-gray-900 mb-2">Join QTruck</h1>
            <p className="text-gray-600">Choose how you'd like to use our platform</p>
          </div>

          <div className="space-y-4">
            <div
              className={`cursor-pointer hover:border-amber-500 transition-colors border-2 rounded-xl p-6 ${selectedRole === "driver" ? "border-amber-500 bg-amber-50" : "border-gray-200"}`}
              onClick={() => handleRoleSelect("driver")}
            >
              <div className="flex items-center">
                <div className="bg-amber-100 p-3 rounded-full mr-4">
                  <span className="text-2xl">📱</span>
                </div>
                <div className="flex-1">
                  <h3 className="font-bold text-gray-900">I'm a Driver</h3>
                  <p className="text-gray-600 text-sm">Manage trips, track expenses, and optimize routes</p>
                </div>
                {selectedRole === "driver" && <span className="text-amber-500 text-xl">→</span>}
              </div>
            </div>

            <div
              className={`cursor-pointer hover:border-slate-500 transition-colors border-2 rounded-xl p-6 ${selectedRole === "admin" ? "border-slate-500 bg-slate-50" : "border-gray-200"}`}
              onClick={() => handleRoleSelect("admin")}
            >
              <div className="flex items-center">
                <div className="bg-slate-100 p-3 rounded-full mr-4">
                  <span className="text-2xl">🖥️</span>
                </div>
                <div className="flex-1">
                  <h3 className="font-bold text-gray-900">I manage a Fleet</h3>
                  <p className="text-gray-600 text-sm">Oversee operations, manage drivers, and analyze performance</p>
                </div>
                {selectedRole === "admin" && <span className="text-slate-500 text-xl">→</span>}
              </div>
            </div>

            <button
              className={`w-full py-4 px-6 rounded-lg font-medium text-white transition-all duration-300 mt-6 ${
                selectedRole === 'driver' ? 'btn btn-primary' :
                selectedRole === 'admin' ? 'bg-gradient-to-r from-slate-500 to-slate-600 hover:from-slate-600 hover:to-slate-700' :
                'bg-gray-400 cursor-not-allowed'
              }`}
              disabled={!selectedRole}
              onClick={handleContinue}
            >
              Continue
            </button>

            <p className="text-center text-sm text-gray-500 mt-4">
              Already have an account?{" "}
              <Link href="/auth/login" className="text-amber-600 hover:text-amber-700 font-medium">
                Sign in
              </Link>
            </p>
          </div>
        </div>
      </div>

      {/* Right Section - Dynamic Features */}
      <div className={`hidden md:block md:w-1/2 relative transition-all duration-500 ${
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
                <h2 className="text-3xl font-bold mb-4">Join as a Driver</h2>
                <p className="text-xl mb-6">
                  Create your driver account and start managing trips efficiently with QTruck.
                </p>
                <div className="space-y-4">
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Easy trip management & tracking</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Expense recording with receipts</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Real-time communication with dispatch</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Performance bonuses & incentives</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Mobile-optimized interface</p>
                  </div>
                </div>
                
              </>
            ) : selectedRole === 'admin' ? (
              <>
                <h2 className="text-3xl font-bold mb-4">Create Fleet Account</h2>
                <p className="text-xl mb-6">
                  Set up your fleet management account and start optimizing your operations today.
                </p>
                <div className="space-y-4">
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Complete fleet management dashboard</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Real-time driver & vehicle tracking</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Advanced reporting & analytics</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Trip planning & optimization tools</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>30-day free trial included</p>
                  </div>
                </div>
                
              </>
            ) : (
              <>
                <h2 className="text-3xl font-bold mb-4">Transform Your Fleet Experience</h2>
                <p className="text-xl mb-6">
                  Whether you're driving or managing, QTruck provides the tools you need to succeed.
                </p>
                <div className="space-y-4">
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Real-time GPS tracking and route optimization</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Comprehensive expense and mileage tracking</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Performance analytics and reporting</p>
                  </div>
                  <div className="flex items-start">
                    <div className="bg-white/20 p-2 rounded-full mr-3 mt-1">
                      <span className="text-sm">✓</span>
                    </div>
                    <p>Mobile and web access for maximum flexibility</p>
                  </div>
                </div>
                
              </>
            )}
          </div>
        </div>
      </div>

      {/* Navigation */}
      <div className="absolute top-4 left-4">
        <Link href="/" className="flex items-center space-x-2 text-gray-600 hover:text-gray-900 transition-colors">
          <span>←</span>
          <span>Back to Home</span>
        </Link>
      </div>
    </main>
  )
}