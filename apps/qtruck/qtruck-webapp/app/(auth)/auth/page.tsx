'use client'

import Link from 'next/link'

export default function AuthPage() {
  return (
    <main className="min-h-screen flex flex-col md:flex-row">
      {/* Left Section - Login */}
      <div className="flex-1 flex items-center justify-center p-8 bg-gradient-to-br from-amber-50 to-green-50 relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-amber-100/30 to-green-100/20"></div>
        <div className="relative z-10 max-w-md w-full text-center space-y-8">
          <div className="space-y-4">
            <div className="text-6xl mb-4">🔑</div>
            <h2 className="text-3xl font-bold text-gray-900">Welcome Back</h2>
            <p className="text-lg text-gray-600">
              Sign in to access your QTruck dashboard and manage your fleet operations.
            </p>
          </div>
          
          <div className="space-y-4">
            <div className="bg-white/80 backdrop-blur-sm rounded-lg p-4 border border-amber-200">
              <h3 className="font-semibold text-amber-700 mb-2">Quick Access:</h3>
              <ul className="text-sm text-gray-600 space-y-1 text-left">
                <li>✓ Real-time fleet monitoring</li>
                <li>✓ Trip management & tracking</li>
                <li>✓ Performance analytics</li>
                <li>✓ Expense reporting</li>
              </ul>
            </div>
            
            <Link 
              href="/auth/login"
              className="w-full btn btn-primary text-lg py-4 shadow-xl hover:shadow-2xl transform hover:scale-105 transition-all duration-300 block"
            >
              Sign In
            </Link>
            
            <p className="text-sm text-gray-500">
              Choose your role and access your personalized dashboard
            </p>
          </div>
        </div>
      </div>

      {/* Right Section - Register */}
      <div className="flex-1 flex items-center justify-center p-8 bg-gradient-to-br from-blue-50 to-purple-50 relative overflow-hidden">
        <div className="absolute inset-0 bg-gradient-to-br from-blue-100/30 to-purple-100/20"></div>
        <div className="relative z-10 max-w-md w-full text-center space-y-8">
          <div className="space-y-4">
            <div className="text-6xl mb-4">🚛</div>
            <h2 className="text-3xl font-bold text-gray-900">Join QTruck</h2>
            <p className="text-lg text-gray-600">
              Start your journey with the most comprehensive fleet management platform.
            </p>
          </div>
          
          <div className="space-y-4">
            <div className="bg-white/80 backdrop-blur-sm rounded-lg p-4 border border-blue-200">
              <h3 className="font-semibold text-blue-700 mb-2">Get Started With:</h3>
              <ul className="text-sm text-gray-600 space-y-1 text-left">
                <li>✓ Free trial period</li>
                <li>✓ Complete setup assistance</li>
                <li>✓ Training & onboarding</li>
                <li>✓ 24/7 customer support</li>
              </ul>
            </div>
            
            <Link 
              href="/auth/register"
              className="w-full bg-gradient-to-r from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 text-white font-medium py-4 px-6 rounded-lg transition-all duration-300 shadow-xl hover:shadow-2xl transform hover:scale-105 block"
            >
              Create Account
            </Link>
            
            <p className="text-sm text-gray-500">
              Choose between driver or fleet manager account
            </p>
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

      {/* Logo */}
      <div className="absolute top-4 right-4">
        <div className="flex items-center space-x-2">
          <span className="text-2xl">🚛</span>
          <span className="text-xl font-bold text-amber-500">QTruck</span>
        </div>
      </div>
    </main>
  )
}