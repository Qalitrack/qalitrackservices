'use client'

import React, { useState, useRef, useCallback, useEffect } from 'react'

interface EnhancedPhotoCaptureProps {
  isOpen: boolean
  onClose: () => void
  onCapture: (file: File) => void
  title?: string
  preferredCamera?: 'user' | 'environment'
}

interface CameraDevice {
  deviceId: string
  label: string
  facingMode?: string
}

export function EnhancedPhotoCapture({ 
  isOpen, 
  onClose, 
  onCapture, 
  title = "Take Photo",
  preferredCamera = 'environment'
}: EnhancedPhotoCaptureProps) {
  const [stream, setStream] = useState<MediaStream | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [devices, setDevices] = useState<CameraDevice[]>([])
  const [currentDeviceId, setCurrentDeviceId] = useState<string>('')
  const [facingMode, setFacingMode] = useState<'user' | 'environment'>(preferredCamera)
  const [isFlashOn, setIsFlashOn] = useState(false)
  const [capturedImage, setCapturedImage] = useState<string | null>(null)
  
  const videoRef = useRef<HTMLVideoElement>(null)
  const canvasRef = useRef<HTMLCanvasElement>(null)

  // Check if getUserMedia is supported
  const isSupported = useCallback(() => {
    return !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia)
  }, [])

  // Enumerate available camera devices
  const enumerateDevices = useCallback(async () => {
    if (!isSupported()) return []
    
    try {
      const devices = await navigator.mediaDevices.enumerateDevices()
      const videoDevices = devices
        .filter(device => device.kind === 'videoinput')
        .map(device => ({
          deviceId: device.deviceId,
          label: device.label || `Camera ${device.deviceId.slice(0, 8)}`,
          facingMode: device.label.toLowerCase().includes('front') || device.label.toLowerCase().includes('user') ? 'user' : 'environment'
        }))
      
      setDevices(videoDevices)
      return videoDevices
    } catch (err) {
      console.error('Error enumerating devices:', err)
      return []
    }
  }, [isSupported])

  // Get optimal constraints for the camera
  const getConstraints = useCallback((deviceId?: string, facing?: 'user' | 'environment') => {
    const constraints: MediaStreamConstraints = {
      video: {
        width: { ideal: 1920, max: 1920 },
        height: { ideal: 1080, max: 1080 },
        frameRate: { ideal: 30, max: 30 }
      },
      audio: false
    }

    if (deviceId) {
      // Use specific device ID if available
      constraints.video = {
        ...constraints.video,
        deviceId: { exact: deviceId }
      }
    } else if (facing) {
      // Use facing mode as fallback
      constraints.video = {
        ...constraints.video,
        facingMode: { ideal: facing }
      }
    }

    return constraints
  }, [])

  // Start camera with proper error handling and fallbacks
  const startCamera = useCallback(async (deviceId?: string, facing: 'user' | 'environment' = facingMode) => {
    if (!isSupported()) {
      setError('Camera API not supported in this browser. Please use a modern browser or update your current one.')
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      // Stop existing stream
      if (stream) {
        stream.getTracks().forEach(track => track.stop())
        setStream(null)
      }

      let constraints = getConstraints(deviceId, facing)
      let mediaStream: MediaStream

      try {
        // Try with specific constraints first
        mediaStream = await navigator.mediaDevices.getUserMedia(constraints)
      } catch (err: any) {
        console.warn('Failed with specific constraints, trying fallback:', err)
        
        // Fallback to basic constraints
        constraints = {
          video: {
            facingMode: facing,
            width: { ideal: 1280 },
            height: { ideal: 720 }
          },
          audio: false
        }
        
        try {
          mediaStream = await navigator.mediaDevices.getUserMedia(constraints)
        } catch (fallbackErr: any) {
          console.warn('Fallback failed, trying basic video:', fallbackErr)
          
          // Final fallback - just request video
          mediaStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false })
        }
      }

      setStream(mediaStream)
      setCurrentDeviceId(deviceId || '')
      setFacingMode(facing)

      if (videoRef.current) {
        videoRef.current.srcObject = mediaStream
        
        // Wait for video to load
        videoRef.current.onloadedmetadata = () => {
          videoRef.current?.play()
        }
      }

    } catch (err: any) {
      console.error('Camera access error:', err)
      
      let errorMessage = 'Unable to access camera.'
      
      if (err.name === 'NotAllowedError') {
        errorMessage = 'Camera access denied. Please allow camera permissions in your browser settings and try again.'
      } else if (err.name === 'NotFoundError') {
        errorMessage = 'No camera found on this device.'
      } else if (err.name === 'NotSupportedError') {
        errorMessage = 'Camera not supported in this browser.'
      } else if (err.name === 'OverconstrainedError') {
        errorMessage = 'Camera constraints not supported. Trying with different settings...'
        // Try again with basic constraints
        setTimeout(() => startCamera(undefined, facing), 1000)
        return
      } else if (err.name === 'SecurityError') {
        errorMessage = 'Camera access blocked for security reasons. Please ensure you\'re using HTTPS.'
      }
      
      setError(errorMessage)
    } finally {
      setIsLoading(false)
    }
  }, [isSupported, stream, getConstraints, facingMode])

  // Stop camera and cleanup
  const stopCamera = useCallback(() => {
    if (stream) {
      stream.getTracks().forEach(track => track.stop())
      setStream(null)
    }
    if (videoRef.current) {
      videoRef.current.srcObject = null
    }
  }, [stream])

  // Switch between cameras
  const switchCamera = useCallback(async () => {
    const newFacing = facingMode === 'user' ? 'environment' : 'user'
    
    // Find device with the desired facing mode
    const targetDevice = devices.find(device => device.facingMode === newFacing)
    
    if (targetDevice) {
      await startCamera(targetDevice.deviceId, newFacing)
    } else {
      await startCamera(undefined, newFacing)
    }
  }, [facingMode, devices, startCamera])

  // Capture photo with high quality
  const capturePhoto = useCallback(() => {
    if (!videoRef.current || !canvasRef.current) return

    const video = videoRef.current
    const canvas = canvasRef.current
    const context = canvas.getContext('2d')

    if (!context) return

    // Set canvas size to video dimensions for best quality
    canvas.width = video.videoWidth || 1920
    canvas.height = video.videoHeight || 1080

    // Apply flash effect if enabled
    if (isFlashOn) {
      document.body.style.backgroundColor = 'white'
      setTimeout(() => {
        document.body.style.backgroundColor = ''
      }, 100)
    }

    // Draw the video frame to canvas
    context.drawImage(video, 0, 0, canvas.width, canvas.height)

    // Convert to blob with high quality
    canvas.toBlob((blob) => {
      if (blob) {
        const file = new File([blob], `photo-${Date.now()}.jpg`, { 
          type: 'image/jpeg',
          lastModified: Date.now()
        })
        
        // Show preview
        const url = URL.createObjectURL(blob)
        setCapturedImage(url)
        
        // Call the callback with the file
        setTimeout(() => {
          onCapture(file)
          handleClose()
        }, 1500) // Show preview for 1.5 seconds
      }
    }, 'image/jpeg', 0.95) // High quality JPEG
  }, [isFlashOn, onCapture])

  // Handle close with cleanup
  const handleClose = useCallback(() => {
    stopCamera()
    setError(null)
    setCapturedImage(null)
    onClose()
  }, [stopCamera, onClose])

  // Initialize camera when dialog opens
  useEffect(() => {
    if (isOpen) {
      enumerateDevices().then(devices => {
        if (devices.length > 0) {
          // Try to find a camera with the preferred facing mode
          const preferredDevice = devices.find(device => device.facingMode === preferredCamera)
          startCamera(preferredDevice?.deviceId, preferredCamera)
        } else {
          startCamera(undefined, preferredCamera)
        }
      })
    }
  }, [isOpen, enumerateDevices, startCamera, preferredCamera])

  // Cleanup on unmount
  useEffect(() => {
    return () => {
      stopCamera()
    }
  }, [stopCamera])

  if (!isOpen) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-90 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg max-w-4xl w-full mx-4 max-h-[95vh] overflow-hidden">
        {/* Header */}
        <div className="flex items-center justify-between p-4 border-b">
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

        {/* Camera View */}
        <div className="relative bg-black">
          {error ? (
            <div className="flex flex-col items-center justify-center py-16 px-8">
              <div className="text-red-500 mb-4">
                <svg className="w-16 h-16 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
                </svg>
              </div>
              <p className="text-red-600 mb-6 text-center max-w-md">{error}</p>
              <div className="space-x-3">
                <button onClick={() => startCamera()} className="btn btn-primary">
                  Try Again
                </button>
                <button onClick={handleClose} className="btn btn-outline">
                  Cancel
                </button>
              </div>
            </div>
          ) : (
            <>
              {/* Video Element */}
              <video
                ref={videoRef}
                autoPlay
                playsInline
                muted
                className="w-full h-96 object-cover"
                style={{ maxHeight: '60vh' }}
              />
              
              {/* Loading Overlay */}
              {(isLoading || !stream) && (
                <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-50">
                  <div className="text-white text-center">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-white mx-auto mb-4"></div>
                    <p>Starting camera...</p>
                  </div>
                </div>
              )}

              {/* Captured Image Preview */}
              {capturedImage && (
                <div className="absolute inset-0 flex items-center justify-center bg-black">
                  <img src={capturedImage} alt="Captured" className="max-w-full max-h-full object-contain" />
                  <div className="absolute top-4 left-1/2 transform -translate-x-1/2 bg-green-500 text-white px-4 py-2 rounded-full">
                    ✓ Photo Captured!
                  </div>
                </div>
              )}

              {/* Camera Controls Overlay */}
              {stream && !capturedImage && (
                <div className="absolute bottom-4 left-0 right-0 flex items-center justify-center space-x-6">
                  {/* Flash Toggle */}
                  <button
                    onClick={() => setIsFlashOn(!isFlashOn)}
                    className={`p-3 rounded-full transition-colors ${
                      isFlashOn ? 'bg-yellow-500 text-white' : 'bg-white bg-opacity-20 text-white hover:bg-opacity-30'
                    }`}
                  >
                    <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                  </button>

                  {/* Capture Button */}
                  <button
                    onClick={capturePhoto}
                    className="w-16 h-16 bg-white rounded-full border-4 border-gray-300 hover:border-gray-400 transition-colors flex items-center justify-center"
                  >
                    <div className="w-12 h-12 bg-gray-200 rounded-full"></div>
                  </button>

                  {/* Camera Switch */}
                  {devices.length > 1 && (
                    <button
                      onClick={switchCamera}
                      className="p-3 rounded-full bg-white bg-opacity-20 text-white hover:bg-opacity-30 transition-colors"
                    >
                      <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
                      </svg>
                    </button>
                  )}
                </div>
              )}
            </>
          )}
        </div>

        {/* Hidden canvas for capture */}
        <canvas ref={canvasRef} className="hidden" />

        {/* Footer Controls */}
        {!error && (
          <div className="p-4 bg-gray-50 border-t">
            <div className="flex justify-between items-center">
              <div className="text-sm text-gray-600">
                {devices.length > 0 && (
                  <span>Camera: {facingMode === 'user' ? 'Front' : 'Back'}</span>
                )}
              </div>
              
              <div className="flex space-x-3">
                <button onClick={handleClose} className="btn btn-outline">
                  Cancel
                </button>
                {stream && !capturedImage && (
                  <button onClick={capturePhoto} className="btn btn-primary">
                    Capture Photo
                  </button>
                )}
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}