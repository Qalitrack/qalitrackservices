'use client'

import React, { useState } from 'react'
import { EnhancedPhotoCapture } from './EnhancedPhotoCapture'

interface PhotoUploadProps {
  label: string
  onFileSelect: (file: File) => void
  onRemoveImage?: (index: number) => void
  currentImage?: string
  currentImages?: string[]  // For multiple images mode
  accept?: string
  required?: boolean
  maxImages?: number  // Maximum number of images allowed
  mode?: 'single' | 'multiple'  // Upload mode
}

export function PhotoUpload({ 
  label, 
  onFileSelect,
  onRemoveImage,
  currentImage, 
  currentImages = [],
  accept = "image/*",
  required = false,
  maxImages = 2,
  mode = 'single'
}: PhotoUploadProps) {
  const [showOptions, setShowOptions] = useState(false)
  const [showCamera, setShowCamera] = useState(false)
  const [cameraSupported, setCameraSupported] = useState(false)

  // Check camera support on component mount
  React.useEffect(() => {
    const checkCameraSupport = () => {
      setCameraSupported(!!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia))
    }
    checkCameraSupport()
  }, [])

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (file) {
      onFileSelect(file)
    }
    setShowOptions(false)
  }

  const handleCameraCapture = (file: File) => {
    onFileSelect(file)
    setShowCamera(false)
    setShowOptions(false)
  }

  // Helper functions for multiple mode
  const currentImagesArray = mode === 'multiple' ? currentImages : (currentImage ? [currentImage] : [])
  const canAddMore = mode === 'multiple' ? currentImagesArray.length < maxImages : !currentImage
  const showAddButton = canAddMore && !showOptions

  return (
    <div className="space-y-3">
      <label className="block text-sm font-medium text-gray-700">
        {label} {required && <span className="text-red-500">*</span>}
        {mode === 'multiple' && (
          <span className="text-xs text-gray-500 ml-2">
            (Max {maxImages} images)
          </span>
        )}
      </label>
      
      {/* Image Thumbnails Grid */}
      <div className="grid grid-cols-3 gap-3">
        {/* Existing Images */}
        {currentImagesArray.map((image, index) => (
          <div key={index} className="relative group">
            <div className="w-24 h-24 border-2 border-gray-200 rounded-lg overflow-hidden bg-gray-50">
              <img 
                src={image} 
                alt={`${label} ${index + 1}`}
                className="w-full h-full object-cover"
              />
            </div>
            {/* Remove Button */}
            {onRemoveImage && (
              <button
                type="button"
                onClick={() => onRemoveImage(index)}
                className="absolute -top-2 -right-2 w-6 h-6 bg-red-500 hover:bg-red-600 text-white rounded-full flex items-center justify-center text-xs transition-colors shadow-md"
              >
                ×
              </button>
            )}
            <p className="text-xs text-gray-500 mt-1 text-center truncate">
              {mode === 'multiple' ? `Image ${index + 1}` : 'Current'}
            </p>
          </div>
        ))}

        {/* Add Photo Button */}
        {showAddButton && (
          <div className="w-24 h-24">
            <button
              type="button"
              onClick={() => setShowOptions(true)}
              className="w-full h-full border-2 border-dashed border-gray-300 rounded-lg flex flex-col items-center justify-center hover:border-amber-400 hover:bg-amber-50 transition-colors group"
            >
              <svg className="w-6 h-6 text-gray-400 group-hover:text-amber-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
              </svg>
              <span className="text-xs text-gray-500 group-hover:text-amber-600 mt-1">Add</span>
            </button>
            <p className="text-xs text-gray-500 mt-1 text-center">
              {currentImagesArray.length === 0 ? 'Add photo' : 'Add more'}
            </p>
          </div>
        )}
      </div>

      {/* Upload Options Modal */}
      {showOptions && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-40">
          <div className="bg-white rounded-lg p-6 max-w-sm w-full mx-4">
            <h3 className="text-lg font-semibold text-gray-900 mb-4">Add {label}</h3>
            
            <div className="space-y-3">
              {/* Camera Option - only show if supported */}
              {cameraSupported && (
                <button
                  type="button"
                  onClick={() => setShowCamera(true)}
                  className="w-full flex items-center px-4 py-3 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 transition-colors"
                >
                  <svg className="w-5 h-5 mr-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
                  </svg>
                  Take Photo
                </button>
              )}

              {/* File Upload Option */}
              <label className="w-full flex items-center px-4 py-3 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 transition-colors cursor-pointer">
                <svg className="w-5 h-5 mr-3 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                Choose File
                <input
                  type="file"
                  accept={accept}
                  onChange={handleFileUpload}
                  className="hidden"
                />
              </label>

              {/* Cancel */}
              <button
                type="button"
                onClick={() => setShowOptions(false)}
                className="w-full px-4 py-3 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 transition-colors"
              >
                Cancel
              </button>
            </div>
            
            {/* Camera not supported message */}
            {!cameraSupported && (
              <p className="text-xs text-gray-500 mt-3 text-center">
                📷 Camera not available in this browser. Use "Choose File" to upload from your device.
              </p>
            )}
          </div>
        </div>
      )}

      {/* Camera Modal */}
      <EnhancedPhotoCapture
        isOpen={showCamera}
        onClose={() => setShowCamera(false)}
        onCapture={handleCameraCapture}
        title={`Take ${label}`}
        preferredCamera="environment"
      />
    </div>
  )
}