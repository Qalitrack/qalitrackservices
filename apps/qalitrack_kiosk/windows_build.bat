@echo off
REM QaliTrack Kiosk - Windows Build Script
REM Run this script on a Windows machine with Flutter installed

echo ================================
echo QaliTrack Kiosk Windows Builder
echo ================================

REM Check if Flutter is installed
flutter --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Flutter is not installed or not in PATH
    echo Please install Flutter from: https://docs.flutter.dev/get-started/install/windows
    pause
    exit /b 1
)

echo Flutter version:
flutter --version

REM Check Flutter doctor
echo.
echo Checking Flutter environment...
flutter doctor

REM Enable Windows desktop support
echo.
echo Enabling Windows desktop support...
flutter config --enable-windows-desktop

REM Clean previous builds
echo.
echo Cleaning previous builds...
flutter clean

REM Get dependencies
echo.
echo Getting dependencies...
flutter pub get

REM Generate localization files
echo.
echo Generating localization files...
flutter gen-l10n

REM Build for Windows
echo.
echo Building for Windows (Release)...
flutter build windows --release

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo Check the error messages above for details.
    pause
    exit /b 1
)

echo.
echo ================================
echo BUILD SUCCESSFUL!
echo ================================
echo.
echo Executable location: build\windows\runner\Release\qalitrack_kiosk.exe
echo.
echo To run the application:
echo   cd build\windows\runner\Release
echo   qalitrack_kiosk.exe
echo.
echo For distribution, copy the entire Release folder contents.
echo.

REM Optional: Open build folder
set /p choice="Open build folder? (y/n): "
if /i "%choice%"=="y" (
    explorer build\windows\runner\Release
)

echo.
echo Build completed successfully!
pause