'use client'

import React, { useState, useRef, useCallback } from 'react'

interface PhotoCaptureProps {
  isOpen: boolean
  onClose: () => void
  onCapture: (file: File) => void
  title?: string
}

// Check if we're in a secure context (HTTPS)
const isSecureContext = typeof window !== 'undefined' && (window.isSecureContext || window.location.protocol === 'https:')

export function PhotoCapture({ isOpen, onClose, onCapture, title = "Take Photo" }: PhotoCaptureProps) {
  const [stream, setStream] = useState<MediaStream | null>(null)
  const [isCapturing, setIsCapturing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [showFileUpload, setShowFileUpload] = useState(false)
  const videoRef = useRef<HTMLVideoElement>(null)
  const canvasRef = useRef<HTMLCanvasElement>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const startCamera = useCallback(async () => {
    try {
      setError(null)
      
      // Check for secure context first
      if (!isSecureContext) {
        throw new Error('Camera access requires HTTPS. Please access this site over HTTPS or use "Choose File" instead.')
      }
      
      // Check if camera API is available
      if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
        throw new Error('Camera API not supported in this browser')
      }
      
      // Check permissions first
      try {
        const permissionStatus = await navigator.permissions.query({ name: 'camera' as PermissionName })
        if (permissionStatus.state === 'denied') {
          throw new Error('Camera permission denied. Please check your browser settings and allow camera access.')
        }
      } catch (permError) {
        // Permissions API not supported, continue with getUserMedia
        console.log('Permissions API not supported, trying direct camera access')
      }
      
      // Try to use the back camera first (better for documents)
      let constraints = {
        video: { 
          width: { ideal: 1280, min: 640 },
          height: { ideal: 720, min: 480 },
          facingMode: 'environment' // Use back camera on mobile
        }
      }
      
      // Detect mobile devices
      const isMobile = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)
      
      let mediaStream
      try {
        mediaStream = await navigator.mediaDevices.getUserMedia(constraints)
      } catch (err) {
        // If back camera fails, try front camera or any available camera
        console.log('Back camera not available, trying front camera or any camera')
        constraints = {
          video: {
            width: { ideal: 1280, min: 640 },
            height: { ideal: 720, min: 480 },
            facingMode: isMobile ? 'user' : undefined // Front camera on mobile, any on desktop
          }
        }
        mediaStream = await navigator.mediaDevices.getUserMedia(constraints)
      }
      setStream(mediaStream)
      
      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream
      }
    } catch (err) {
      console.error('Error accessing camera:', err)
      let errorMessage = 'Unable to access camera.'
      
      if (err instanceof Error) {
        if (err.name === 'NotAllowedError') {
          errorMessage = 'Camera access denied. Please allow camera permissions and try again.'
        } else if (err.name === 'NotFoundError') {
          errorMessage = 'No camera found on this device.'
        } else if (err.name === 'NotSupportedError') {
          errorMessage = 'Camera not supported in this browser.'
        } else if (err.message.includes('not supported')) {
          errorMessage = 'Camera API not supported in this browser. Please use "Choose File" instead.'
        }
      }
      
      setError(errorMessage)
    }
  }, [])

  const startCameraBasic = useCallback(async () => {
    try {
      setError(null)
      // Try with very basic constraints as fallback
      const mediaStream = await navigator.mediaDevices.getUserMedia({
        video: true // Just basic video, no specific constraints
      })
      setStream(mediaStream)
      
      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream
      }
    } catch (err) {
      console.error('Error accessing camera with basic settings:', err)
      setError('Unable to access camera. Please use "Choose File" instead.')
      setShowFileUpload(true)
    }
  }, [])

  const handleFileSelect = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (file) {
      // Validate file type
      if (!file.type.startsWith('image/')) {
        setError('Please select an image file.')
        return
      }
      
      // Validate file size (max 10MB)
      if (file.size > 10 * 1024 * 1024) {
        setError('Image is too large. Please select an image smaller than 10MB.')
        return
      }
      
      onCapture(file)
      handleClose()
    }
  }, [onCapture])

  const handleFileButtonClick = useCallback(() => {
    console.log('File button clicked, fileInputRef:', fileInputRef.current)
    if (fileInputRef.current) {
      fileInputRef.current.click()
    } else {
      console.error('File input ref is null')
    }
  }, [])

  // Diagnostic function for troubleshooting
  const runDiagnostics = useCallback(async () => {
    console.log('=== Camera Diagnostics ===')
    console.log('User Agent:', navigator.userAgent)
    console.log('Is Secure Context:', isSecureContext)
    console.log('Protocol:', window.location.protocol)
    console.log('MediaDevices supported:', !!navigator.mediaDevices)
    console.log('getUserMedia supported:', !!navigator.mediaDevices?.getUserMedia)
    
    try {
      const devices = await navigator.mediaDevices.enumerateDevices()
      const videoDevices = devices.filter(device => device.kind === 'videoinput')
      console.log('Video devices found:', videoDevices.length)
      videoDevices.forEach((device, i) => {
        console.log(`  Device ${i + 1}:`, device.label || `Camera ${i + 1}`, device.deviceId)
      })
    } catch (err) {
      console.log('Could not enumerate devices:', err)
    }
    
    try {
      const permissionStatus = await navigator.permissions.query({ name: 'camera' as PermissionName })
      console.log('Camera permission status:', permissionStatus.state)
    } catch (err) {
      console.log('Permissions API not supported')
    }
    
    console.log('===========================')
    alert('Diagnostic information has been logged to the browser console. Please open Developer Tools (F12) and check the Console tab.')
  }, [])

  const stopCamera = useCallback(() => {
    if (stream) {
      stream.getTracks().forEach(track => track.stop())
      setStream(null)
    }
  }, [stream])

  const capturePhoto = useCallback(() => {
    if (!videoRef.current || !canvasRef.current) return

    const video = videoRef.current
    const canvas = canvasRef.current
    const context = canvas.getContext('2d')

    if (!context) return

    // Set canvas dimensions to match video
    canvas.width = video.videoWidth
    canvas.height = video.videoHeight

    // Draw the video frame to canvas
    context.drawImage(video, 0, 0, canvas.width, canvas.height)

    // Convert canvas to blob and then to file
    canvas.toBlob((blob) => {
      if (blob) {
        const file = new File([blob], `photo-${Date.now()}.jpg`, { type: 'image/jpeg' })
        onCapture(file)
        handleClose()
      }
    }, 'image/jpeg', 0.9)
  }, [onCapture])

  const handleClose = useCallback(() => {
    stopCamera()
    setError(null)
    onClose()
  }, [stopCamera, onClose])

  // Start camera when dialog opens
  React.useEffect(() => {
    if (isOpen && !stream) {
      startCamera()
    }
  }, [isOpen, startCamera, stream])

  // Cleanup when component unmounts
  React.useEffect(() => {
    return () => {
      stopCamera()
    }
  }, [stopCamera])

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg p-6 max-w-2xl w-full mx-4 max-h-[90vh] overflow-auto">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-lg font-semibold text-gray-900">{title}</h3>
          <button
            onClick={handleClose}
            className="text-gray-400 hover:text-gray-600 transition-colors"
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {error ? (
          <div className="text-center py-8">
            <div className="text-red-500 mb-4">
              <svg className="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
            </div>
            <p className="text-red-600 mb-4">{error}</p>
            
            {/* Troubleshooting info */}
            <div className="bg-blue-50 border border-blue-200 rounded-md p-4 mb-4 text-left">
              <h4 className="font-medium text-blue-900 mb-2">Troubleshooting Tips:</h4>
              <ul className="text-sm text-blue-800 space-y-1">
                <li>• Make sure you're using HTTPS (secure connection)</li>
                <li>• Allow camera permissions when prompted</li>
                <li>• Close other apps that might be using the camera</li>
                <li>• Try refreshing the page</li>
                <li>• Use "Choose File" if camera doesn't work</li>
                <li>• Use the Diagnostics button for detailed info</li>
              </ul>
            </div>
            <div className="space-x-3">
              <button
                onClick={startCamera}
                className="btn btn-primary"
              >
                Try Again
              </button>
              {(showFileUpload || !isSecureContext) && (
                <button
                  onClick={handleFileButtonClick}
                  className="btn btn-secondary"
                >
                  Choose File Instead
                </button>
              )}
              <button
                onClick={runDiagnostics}
                className="btn btn-outline text-xs"
                title="Run camera diagnostics"
              >
                Diagnostics
              </button>
              <button
                onClick={handleClose}
                className="btn btn-outline"
              >
                Cancel
              </button>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            {/* Helpful info banner for camera usage */}
            {!stream && (
              <div className="bg-green-50 border border-green-200 rounded-md p-3">
                <div className="flex items-center">
                  <div className="flex-shrink-0">
                    <svg className="w-5 h-5 text-green-400" fill="currentColor" viewBox="0 0 20 20">
                      <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7-4a1 1 0 11-2 0 1 1 0 012 0zM9 9a.75.75 0 000 1.5v3a.75.75 0 001.5 0v-3A.75.75 0 009 9z" clipRule="evenodd" />
                    </svg>
                  </div>
                  <div className="ml-3">
                    <p className="text-sm text-green-700">
                      Click "Allow" when your browser asks for camera permission. Position the document clearly in the frame for best results.
                    </p>
                  </div>
                </div>
              </div>
            )}
            {/* Camera Preview */}
            <div className="relative bg-black rounded-lg overflow-hidden">
              <video
                ref={videoRef}
                autoPlay
                playsInline
                muted
                className="w-full h-64 object-cover"
              />
              {!stream && (
                <div className="absolute inset-0 flex items-center justify-center">
                  <div className="text-white text-center">
                    <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-white mx-auto mb-2"></div>
                    <p>Starting camera...</p>
                  </div>
                </div>
              )}
            </div>

            {/* Hidden canvas for capturing */}
            <canvas ref={canvasRef} className="hidden" />

            {/* Camera Controls */}
            <div className="flex justify-center space-x-4">
              <button
                onClick={capturePhoto}
                disabled={!stream || isCapturing}
                className="btn btn-primary flex items-center space-x-2"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 9a2 2 0 012-2h.93a2 2 0 001.664-.89l.812-1.22A2 2 0 0110.07 4h3.86a2 2 0 011.664.89l.812 1.22A2 2 0 0018.07 7H19a2 2 0 012 2v9a2 2 0 01-2 2H5a2 2 0 01-2-2V9z" />
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 13a3 3 0 11-6 0 3 3 0 016 0z" />
                </svg>
                <span>Capture Photo</span>
              </button>
              <button
                onClick={handleFileButtonClick}
                className="btn btn-secondary flex items-center space-x-2"
              >
                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12" />
                </svg>
                <span>Choose File</span>
              </button>
              <button
                onClick={handleClose}
                className="btn btn-outline"
              >
                Cancel
              </button>
            </div>

            {/* Instructions */}
            <div className="text-center text-sm text-gray-600">
              <p>Position the document or yourself in the frame and click "Capture Photo", or use "Choose File" to select from your device</p>
            </div>
            
          </div>
        )}
        
        {/* Hidden file input - always available regardless of camera state */}
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          onChange={handleFileSelect}
          className="hidden"
        />
      </div>
    </div>
  )
}