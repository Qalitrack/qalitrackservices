'use client'

import { ReactNode } from 'react'

interface FormFieldProps {
  label: string
  children: ReactNode
  errors?: string[]
  required?: boolean
  hint?: string
}

export function FormField({ label, children, errors, required, hint }: FormFieldProps) {
  const hasErrors = errors && errors.length > 0

  return (
    <div className="space-y-1">
      <label className={`label ${hasErrors ? 'text-red-700' : ''}`}>
        {label}
        {required && <span className="text-red-500 ml-1">*</span>}
      </label>
      {children}
      {hasErrors && (
        <div className="space-y-1">
          {errors.map((error, index) => (
            <p key={index} className="text-sm text-red-600 flex items-center">
              <span className="mr-1">⚠️</span>
              {error}
            </p>
          ))}
        </div>
      )}
      {hint && !hasErrors && (
        <p className="text-sm text-gray-500">{hint}</p>
      )}
    </div>
  )
}

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
  errors?: string[]
}

export function Input({ errors, className = '', ...props }: InputProps) {
  const hasErrors = errors && errors.length > 0
  const baseClass = hasErrors 
    ? 'input border-red-300 focus:ring-red-500 focus:border-red-500' 
    : 'input'
  
  return (
    <input 
      className={`${baseClass} ${className}`}
      {...props}
    />
  )
}

interface TextareaProps extends React.TextareaHTMLAttributes<HTMLTextAreaElement> {
  errors?: string[]
}

export function Textarea({ errors, className = '', ...props }: TextareaProps) {
  const hasErrors = errors && errors.length > 0
  const baseClass = hasErrors 
    ? 'input border-red-300 focus:ring-red-500 focus:border-red-500' 
    : 'input'
  
  return (
    <textarea 
      className={`${baseClass} ${className}`}
      {...props}
    />
  )
}

interface SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  errors?: string[]
  children: ReactNode
}

export function Select({ errors, className = '', children, ...props }: SelectProps) {
  const hasErrors = errors && errors.length > 0
  const baseClass = hasErrors 
    ? 'input border-red-300 focus:ring-red-500 focus:border-red-500' 
    : 'input'
  
  return (
    <select 
      className={`${baseClass} ${className}`}
      {...props}
    >
      {children}
    </select>
  )
}

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger'
  size?: 'sm' | 'md' | 'lg'
}

export function Button({ 
  variant = 'primary', 
  size = 'md', 
  className = '', 
  children, 
  ...props 
}: ButtonProps) {
  const baseClass = 'inline-flex items-center justify-center font-medium rounded-md focus:outline-none focus:ring-2 focus:ring-offset-2 transition-colors'
  
  const variantClasses = {
    primary: 'bg-blue-600 hover:bg-blue-700 text-white focus:ring-blue-500',
    secondary: 'bg-gray-100 hover:bg-gray-200 text-gray-900 focus:ring-gray-500',
    danger: 'bg-red-600 hover:bg-red-700 text-white focus:ring-red-500'
  }
  
  const sizeClasses = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-sm',
    lg: 'px-6 py-3 text-base'
  }
  
  return (
    <button 
      className={`${baseClass} ${variantClasses[variant]} ${sizeClasses[size]} ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}