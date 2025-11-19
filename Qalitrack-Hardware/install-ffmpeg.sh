#!/bin/bash

echo "================================================"
echo "FFmpeg Installation Script for Qalitrack Camera"
echo "================================================"
echo ""

# Check if already installed
if command -v ffmpeg &> /dev/null; then
    echo "✅ FFmpeg is already installed!"
    ffmpeg -version | head -3
    exit 0
fi

echo "Installing FFmpeg..."
echo ""

# Update package list
sudo apt update

# Install FFmpeg
sudo apt install -y ffmpeg

# Verify installation
echo ""
echo "================================================"
if command -v ffmpeg &> /dev/null; then
    echo "✅ FFmpeg installed successfully!"
    echo ""
    ffmpeg -version | head -3
    echo ""
    echo "📹 You can now restart your Qalitrack service"
    echo "   The camera stream will work automatically"
else
    echo "❌ FFmpeg installation failed"
    echo "   Please try manually: sudo apt install ffmpeg"
fi
echo "================================================"
