interface RefreshButtonProps {
  onRefresh: () => void
  loading: boolean
  size?: 'sm' | 'md' | 'lg'
  variant?: 'primary' | 'secondary'
  theme?: 'admin' | 'driver'
}

export function RefreshButton({ 
  onRefresh, 
  loading, 
  size = 'md',
  variant = 'primary',
  theme = 'admin'
}: RefreshButtonProps) {
  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2',
    lg: 'px-6 py-3 text-lg'
  }

  const getVariantClasses = (theme: 'admin' | 'driver') => ({
    primary: theme === 'admin' 
      ? 'bg-gradient-to-r from-slate-600 to-slate-700 hover:from-slate-700 hover:to-slate-800 text-white'
      : 'bg-gradient-to-r from-amber-600 to-orange-600 hover:from-amber-700 hover:to-orange-700 text-white',
    secondary: 'bg-gray-200 text-gray-700 hover:bg-gray-300'
  })
  
  const variantClasses = getVariantClasses(theme)

  const iconSizes = {
    sm: 'w-3 h-3',
    md: 'w-4 h-4',
    lg: 'w-5 h-5'
  }

  return (
    <button
      onClick={onRefresh}
      disabled={loading}
      className={`
        flex items-center space-x-2 rounded-lg transition-colors
        disabled:opacity-50 disabled:cursor-not-allowed
        ${sizeClasses[size]}
        ${variantClasses[variant]}
      `}
    >
      <svg 
        className={`${iconSizes[size]} ${loading ? 'animate-spin' : ''}`} 
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path 
          strokeLinecap="round" 
          strokeLinejoin="round" 
          strokeWidth={2} 
          d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" 
        />
      </svg>
      <span>{loading ? 'Refreshing...' : 'Refresh'}</span>
    </button>
  )
}