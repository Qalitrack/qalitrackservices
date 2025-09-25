'use client'

import { ReactNode, useState } from 'react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { DriverTestersRoute } from '@/components/auth/ProtectedRoute'
import { useAuth } from '@/lib/auth/AuthContext'
import { NavigationLoader } from '@/components/ui/NavigationLoader'

const driverNavItems = [
  { href: '/portal', icon: '📊', label: 'Dashboard' },
  { href: '/portal/trips', icon: '🗺️', label: 'My Trips' },
  { href: '/portal/expenses', icon: '💰', label: 'Expenses' },
  { href: '/portal/mileage', icon: '📏', label: 'Mileage' },
  { href: '/portal/profile', icon: '👤', label: 'Profile' },
  { href: '/portal/feedback', icon: '💬', label: 'Feedback' },
]

export default function PortalLayout({
  children,
}: {
  children: ReactNode
}) {
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(true)
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState(false)
  const pathname = usePathname()
  const { user, logout } = useAuth()

  const handleLogout = () => {
    logout()
  }

  return (
    <DriverTestersRoute>
      <NavigationLoader />
      <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 shadow-sm fixed top-0 w-full z-30">
        <div className="flex items-center justify-between px-4 py-3">
          <div className="flex items-center space-x-4">
            <button
              onClick={() => setIsMobileSidebarOpen(!isMobileSidebarOpen)}
              className="p-2 rounded-md text-gray-600 hover:text-gray-900 hover:bg-gray-100 lg:hidden"
            >
              <div className="space-y-1">
                <div className="w-6 h-0.5 bg-current"></div>
                <div className="w-6 h-0.5 bg-current"></div>
                <div className="w-6 h-0.5 bg-current"></div>
              </div>
            </button>
            <div className="flex items-center space-x-2">
              <span className="text-2xl">🚛</span>
              <span className="text-xl font-bold text-gray-900">QTruck</span>
              <span className="text-sm text-gray-500">Driver Portal</span>
            </div>
          </div>
          
          <div className="flex items-center space-x-4">
            <div className="text-sm text-gray-600">
              Welcome, <span className="font-semibold text-gray-900">{user?.first_name || user?.username || 'User'}</span>
            </div>
            <button onClick={handleLogout} className="bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white font-medium py-2 px-3 rounded-lg transition-all duration-200 text-sm">
              Logout
            </button>
          </div>
        </div>
      </header>

      <div className="flex pt-16 h-screen">
        {/* Desktop Sidebar */}
        <aside className={`hidden lg:flex lg:flex-col bg-white border-r border-gray-200 shadow-lg transition-all duration-300 ease-in-out ${
          isSidebarCollapsed ? 'w-16' : 'w-64'
        }`}>
          
          <div className="flex-1 flex flex-col h-full">
            <div className="p-4 border-b border-gray-200">
              <div className="flex items-center justify-between">
                {!isSidebarCollapsed ? (
                  <div className="flex items-center space-x-3">
                    <div className="w-10 h-10 bg-gradient-to-r from-amber-500 to-orange-500 rounded-lg flex items-center justify-center">
                      <span className="text-white text-lg font-bold">🚗</span>
                    </div>
                    <div>
                      <h3 className="text-lg font-semibold text-gray-900">Driver Portal</h3>
                      <p className="text-gray-500 text-sm">Fleet Management</p>
                    </div>
                  </div>
                ) : (
                  <div className="w-10 h-10 bg-gradient-to-r from-amber-500 to-orange-500 rounded-lg flex items-center justify-center mx-auto">
                    <span className="text-white text-lg font-bold">🚗</span>
                  </div>
                )}
                
                <button
                  onClick={() => setIsSidebarCollapsed(!isSidebarCollapsed)}
                  className="p-1.5 rounded-md text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors"
                >
                  <svg 
                    className="w-4 h-4" 
                    fill="none" 
                    stroke="currentColor" 
                    viewBox="0 0 24 24"
                  >
                    {isSidebarCollapsed ? (
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 5l7 7-7 7M5 5l7 7-7 7" />
                    ) : (
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M11 19l-7-7 7-7M19 19l-7-7 7-7" />
                    )}
                  </svg>
                </button>
              </div>
            </div>
            
            <nav className="flex-1 p-4 space-y-1">
              {driverNavItems.map((item) => {
                const isActive = pathname === item.href || (item.href !== '/portal' && pathname.startsWith(item.href))
                return (
                  <div key={item.href} className="group relative">
                    <Link
                      href={item.href}
                      className={`flex items-center px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                        isActive 
                          ? 'bg-amber-50 text-amber-700' 
                          : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                      } ${isSidebarCollapsed ? 'justify-center' : ''}`}
                    >
                      <span className={`text-lg ${isSidebarCollapsed ? '' : 'mr-3'}`}>{item.icon}</span>
                      {!isSidebarCollapsed && <span>{item.label}</span>}
                    </Link>
                    
                    {/* Tooltip for collapsed state */}
                    {isSidebarCollapsed && (
                      <div className="absolute left-full ml-2 px-2 py-1 bg-gray-900 text-white text-sm rounded opacity-0 pointer-events-none group-hover:opacity-100 transition-opacity duration-200 whitespace-nowrap z-50 top-1/2 transform -translate-y-1/2">
                        {item.label}
                        <div className="absolute left-0 top-1/2 transform -translate-y-1/2 -translate-x-1 w-2 h-2 bg-gray-900 rotate-45"></div>
                      </div>
                    )}
                  </div>
                )
              })}
            </nav>
          </div>
        </aside>

        {/* Mobile Sidebar */}
        <aside className={`${
          isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full'
        } lg:hidden fixed inset-y-0 left-0 z-20 w-64 bg-white border-r border-gray-200 shadow-lg transform transition-transform duration-300 ease-in-out pt-16`}>
          
          <div className="h-full flex flex-col">
            <div className="p-6 border-b border-gray-200">
              <div className="flex items-center space-x-3">
                <div className="w-10 h-10 bg-gradient-to-r from-amber-500 to-orange-500 rounded-lg flex items-center justify-center">
                  <span className="text-white text-lg font-bold">🚗</span>
                </div>
                <div>
                  <h3 className="text-lg font-semibold text-gray-900">Driver Portal</h3>
                  <p className="text-gray-500 text-sm">Fleet Management</p>
                </div>
              </div>
            </div>
            
            <nav className="flex-1 p-4 space-y-1">
              {driverNavItems.map((item) => {
                const isActive = pathname === item.href || (item.href !== '/portal' && pathname.startsWith(item.href))
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    className={`flex items-center px-3 py-2 rounded-lg text-sm font-medium transition-colors ${
                      isActive 
                        ? 'bg-amber-50 text-amber-700' 
                        : 'text-gray-600 hover:text-gray-900 hover:bg-gray-50'
                    }`}
                    onClick={() => setIsMobileSidebarOpen(false)}
                  >
                    <span className="text-lg mr-3">{item.icon}</span>
                    <span>{item.label}</span>
                  </Link>
                )
              })}
            </nav>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 overflow-y-auto">
          <div className="p-6">
            {children}
          </div>
        </main>

        {/* Overlay for mobile */}
        {isMobileSidebarOpen && (
          <div 
            className="fixed inset-0 bg-black bg-opacity-50 z-10 lg:hidden"
            onClick={() => setIsMobileSidebarOpen(false)}
          />
        )}
      </div>
      </div>
    </DriverTestersRoute>
  )
}