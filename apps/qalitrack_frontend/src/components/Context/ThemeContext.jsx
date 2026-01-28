import React, { createContext, useContext, useState, useEffect } from 'react';

// Create Theme Context
const ThemeContext = createContext();

// Theme configuration
export const themes = {
  light: {
    // Background colors
    bgPrimary: 'bg-white',
    bgSecondary: 'bg-gray-50',
    bgTertiary: 'bg-gray-100',
    bgCard: 'bg-white',
    bgHover: 'hover:bg-gray-100',
    
    // Text colors
    textPrimary: 'text-gray-900',
    textSecondary: 'text-gray-600',
    textTertiary: 'text-gray-500',
    textMuted: 'text-gray-400',
    
    // Border colors
    borderPrimary: 'border-gray-200',
    borderSecondary: 'border-gray-300',
    
    // Accent colors (same for both themes)
    accentPrimary: 'bg-amber-500',
    accentHover: 'hover:bg-amber-600',
    accentText: 'text-amber-600',
    
    // Status colors
    success: 'text-green-600',
    successBg: 'bg-green-50',
    error: 'text-red-600',
    errorBg: 'bg-red-50',
    warning: 'text-yellow-600',
    warningBg: 'bg-yellow-50',
    info: 'text-blue-600',
    infoBg: 'bg-blue-50',
    
    // Gradients
    gradientBg: 'from-gray-50 via-gray-100 to-gray-200',
    
    // CSS variables (for use in style attributes)
    css: {
      bgPrimary: '#ffffff',
      bgSecondary: '#f9fafb',
      bgTertiary: '#f3f4f6',
      textPrimary: '#111827',
      textSecondary: '#4b5563',
      borderPrimary: '#e5e7eb',
    }
  },
  
  dark: {
    // Background colors
    bgPrimary: 'bg-gray-900',
    bgSecondary: 'bg-gray-800',
    bgTertiary: 'bg-gray-700',
    bgCard: 'bg-gray-800',
    bgHover: 'hover:bg-gray-700',
    
    // Text colors
    textPrimary: 'text-white',
    textSecondary: 'text-gray-300',
    textTertiary: 'text-gray-400',
    textMuted: 'text-gray-500',
    
    // Border colors
    borderPrimary: 'border-gray-700',
    borderSecondary: 'border-gray-600',
    
    // Accent colors (same for both themes)
    accentPrimary: 'bg-amber-500',
    accentHover: 'hover:bg-amber-600',
    accentText: 'text-amber-400',
    
    // Status colors
    success: 'text-green-400',
    successBg: 'bg-green-900',
    error: 'text-red-400',
    errorBg: 'bg-red-900',
    warning: 'text-yellow-400',
    warningBg: 'bg-yellow-900',
    info: 'text-blue-400',
    infoBg: 'bg-blue-900',
    
    // Gradients
    gradientBg: 'from-gray-900 via-gray-800 to-black',
    
    // CSS variables (for use in style attributes)
    css: {
      bgPrimary: '#111827',
      bgSecondary: '#1f2937',
      bgTertiary: '#374151',
      textPrimary: '#ffffff',
      textSecondary: '#d1d5db',
      borderPrimary: '#374151',
    }
  }
};

// Theme Provider Component
export function ThemeProvider({ children }) {
  // Get initial theme from localStorage or default to dark
  const [isDark, setIsDark] = useState(() => {
    const saved = localStorage.getItem('kiosk-theme');
    return saved ? saved === 'dark' : true; // Default to dark
  });

  // Update localStorage when theme changes
  useEffect(() => {
    localStorage.setItem('kiosk-theme', isDark ? 'dark' : 'light');
    
    // Update document class for global styles
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }, [isDark]);

  const theme = isDark ? themes.dark : themes.light;

  const toggleTheme = () => {
    setIsDark(prev => !prev);
  };

  const value = {
    isDark,
    theme,
    toggleTheme,
  };

  return (
    <ThemeContext.Provider value={value}>
      {children}
    </ThemeContext.Provider>
  );
}

// Custom hook to use theme
export function useTheme() {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useTheme must be used within ThemeProvider');
  }
  return context;
}

// Theme Toggle Button Component
export function ThemeToggle({ className = '' }) {
  const { isDark, toggleTheme, theme } = useTheme();

  return (
    <button
      onClick={toggleTheme}
      className={`
        px-4 py-2 rounded-lg transition-all
        ${theme.bgSecondary} ${theme.borderPrimary} border
        ${theme.textSecondary} ${theme.bgHover}
        flex items-center gap-2
        ${className}
      `}
      title={isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'}
    >
      {isDark ? (
        <>
          <span className="text-xl">☀️</span>
          <span className="font-medium">Light Mode</span>
        </>
      ) : (
        <>
          <span className="text-xl">🌙</span>
          <span className="font-medium">Dark Mode</span>
        </>
      )}
    </button>
  );
}

export default ThemeContext;