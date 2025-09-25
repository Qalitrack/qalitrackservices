'use client'

import { useEffect, useState } from 'react'
import { usePathname } from 'next/navigation'

export function NavigationLoader() {
  const [loading, setLoading] = useState(false)
  const pathname = usePathname()

  useEffect(() => {
    setLoading(true)
    const timer = setTimeout(() => {
      setLoading(false)
    }, 200) // Show loading for minimum 200ms

    return () => {
      clearTimeout(timer)
      setLoading(false)
    }
  }, [pathname])

  if (!loading) return null

  return (
    <div className="fixed top-0 left-0 right-0 z-50">
      <div className="h-1 bg-gradient-to-r from-amber-400 to-amber-600 animate-pulse"></div>
      <div className="h-1 bg-gradient-to-r from-amber-600 to-amber-400 animate-pulse delay-150"></div>
    </div>
  )
}

export function PageLoadingOverlay() {
  return (
    <div className="fixed inset-0 bg-white bg-opacity-75 flex items-center justify-center z-40">
      <div className="flex flex-col items-center space-y-4">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-600"></div>
        <p className="text-gray-600 font-medium">Loading...</p>
      </div>
    </div>
  )
}