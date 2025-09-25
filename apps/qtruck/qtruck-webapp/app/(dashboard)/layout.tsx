'use client'

import { ReactNode, useState } from 'react'
import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { AdminRoute } from '@/components/auth/ProtectedRoute'
import { useAuth } from '@/lib/auth/AuthContext'
import { NavigationLoader } from '@/components/ui/NavigationLoader'

const adminNavItems = [
  { href: '/dashboard', icon: '📊', label: 'Dashboard' },
  { 
    section: 'User Management',
    items: [
      { href: '/dashboard/users', icon: '👥', label: 'User Management' },
      { href: '/dashboard/drivers', icon: '🚗', label: 'Driver Profiles' },
      { href: '/dashboard/feedback', icon: '💬', label: 'Feedback Management' },
    ]
  },
  {
    section: 'Fleet Management', 
    items: [
      { href: '/dashboard/vehicles', icon: '🚛', label: 'Vehicles' },
      { href: '/dashboard/materials', icon: '📦', label: 'Materials' },
    ]
  },
  {
    section: 'Operations',
    items: [
      { href: '/dashboard/trips', icon: '🗺️', label: 'Trips' },
      { href: '/dashboard/expenses', icon: '💰', label: 'Expenses' },
    ]
  },
  {
    section: 'Analytics',
    items: [
      { href: '/dashboard/reports', icon: '📈', label: 'Reports' },
    ]
  },
  { href: '/dashboard/settings', icon: '⚙️', label: 'System Settings' },
]

export default function DashboardLayout({
  children,
}: {
  children: ReactNode
}) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false)
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(true)
  const pathname = usePathname()
  const { user, logout, loading } = useAuth()
  

  const handleLogout = () => {
    logout()
  }

  return (
    <AdminRoute>
      <NavigationLoader />
      <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200 fixed top-0 w-full z-30">
        <div className="flex items-center justify-between px-4 py-3">
          <div className="flex items-center space-x-4">
            <button
              onClick={() => setIsSidebarOpen(!isSidebarOpen)}
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
              <span className="text-xl font-bold text-slate-700">QTruck Admin</span>
            </div>
          </div>
          
          <div className="flex items-center space-x-4">
            <div className="text-sm text-gray-600">
              {loading ? (
                <span>Loading...</span>
              ) : (
                <>
                  Welcome back, <span className="font-semibold">{user?.first_name || user?.last_name || user?.email || 'Admin'}</span>
                </>
              )}
            </div>
            <button onClick={handleLogout} className="bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white font-medium py-2 px-4 rounded-lg transition-all duration-200 text-sm">
              Logout
            </button>
          </div>
        </div>
      </header>

      <div className="flex pt-16 h-screen">
        {/* Sidebar */}
        <aside className={`${
          isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
        } lg:translate-x-0 fixed lg:static inset-y-0 left-0 z-20 ${
          isSidebarCollapsed ? 'w-16' : 'w-64'
        } bg-white shadow-lg transform transition-all duration-300 ease-in-out lg:transform-none pt-16 lg:pt-0 h-full`}>
          
          <div className="h-full flex flex-col">
            <div className="flex-shrink-0 p-4 border-b border-gray-200 bg-gradient-to-r from-slate-700 to-slate-800 text-white">
              <div className="flex items-center justify-between">
                {!isSidebarCollapsed && (
                  <div>
                    <h3 className="text-lg font-semibold">Admin Panel</h3>
                    <p className="text-slate-200 text-sm">Fleet Management</p>
                  </div>
                )}
                <button
                  onClick={() => setIsSidebarCollapsed(!isSidebarCollapsed)}
                  className="p-2 hover:bg-slate-600 rounded-lg transition-colors hidden lg:block"
                  title={isSidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
                >
                  <span className="text-lg">
                    {isSidebarCollapsed ? '→' : '←'}
                  </span>
                </button>
              </div>
            </div>
            
            <nav className="flex-1 overflow-y-auto p-2 space-y-1">
              {adminNavItems.map((item, index) => {
                if (item.section) {
                  return (
                    <div key={index} className="space-y-1">
                      {!isSidebarCollapsed && (
                        <div className="text-xs font-semibold text-gray-500 uppercase tracking-wider px-3 py-1 mt-2">
                          {item.section}
                        </div>
                      )}
                      {item.items?.map((subItem) => {
                        const isActive = pathname === subItem.href || (subItem.href !== '/dashboard' && pathname.startsWith(subItem.href))
                        return (
                          <div key={subItem.href} className="relative group">
                            <Link
                              href={subItem.href}
                              className={`flex items-center px-3 py-2 text-gray-600 hover:bg-slate-50 hover:text-slate-700 transition-colors border-l-4 border-transparent hover:border-slate-500 rounded-r-lg ${
                                isActive ? 'bg-slate-50 text-slate-700 border-slate-500 font-semibold' : ''
                              } ${isSidebarCollapsed ? 'justify-center' : ''}`}
                              onClick={() => setIsSidebarOpen(false)}
                              title={isSidebarCollapsed ? subItem.label : ''}
                            >
                              <span className={`text-lg ${isSidebarCollapsed ? '' : 'mr-3'}`}>{subItem.icon}</span>
                              {!isSidebarCollapsed && <span className="text-sm">{subItem.label}</span>}
                            </Link>
                            {isSidebarCollapsed && (
                              <div className="absolute left-full top-1/2 transform -translate-y-1/2 ml-2 px-2 py-1 bg-gray-800 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-50">
                                {subItem.label}
                              </div>
                            )}
                          </div>
                        )
                      })}
                    </div>
                  )
                } else {
                  const isActive = pathname === item.href || (item.href !== '/dashboard' && pathname.startsWith(item.href))
                  return (
                    <div key={item.href} className="relative group">
                      <Link
                        href={item.href}
                        className={`flex items-center px-3 py-2 text-gray-600 hover:bg-slate-50 hover:text-slate-700 transition-colors border-l-4 border-transparent hover:border-slate-500 rounded-r-lg ${
                          isActive ? 'bg-slate-50 text-slate-700 border-slate-500 font-semibold' : ''
                        } ${isSidebarCollapsed ? 'justify-center' : ''}`}
                        onClick={() => setIsSidebarOpen(false)}
                        title={isSidebarCollapsed ? item.label : ''}
                      >
                        <span className={`text-lg ${isSidebarCollapsed ? '' : 'mr-3'}`}>{item.icon}</span>
                        {!isSidebarCollapsed && <span className="text-sm">{item.label}</span>}
                      </Link>
                      {isSidebarCollapsed && (
                        <div className="absolute left-full top-1/2 transform -translate-y-1/2 ml-2 px-2 py-1 bg-gray-800 text-white text-xs rounded opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none whitespace-nowrap z-50">
                          {item.label}
                        </div>
                      )}
                    </div>
                  )
                }
              })}
            </nav>
          </div>
        </aside>

        {/* Main Content */}
        <main className="flex-1 lg:ml-0 overflow-y-auto">
          <div className="p-6">
            {children}
          </div>
        </main>

        {/* Overlay for mobile */}
        {isSidebarOpen && (
          <div 
            className="fixed inset-0 bg-black bg-opacity-50 z-10 lg:hidden"
            onClick={() => setIsSidebarOpen(false)}
          />
        )}
      </div>
      </div>
    </AdminRoute>
  )
}