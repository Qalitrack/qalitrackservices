# QaliTrack Kiosk Application

A Flutter-based self-service kiosk application for unmanned weighing operations with face detection authorization, designed to integrate seamlessly with the QaliTrack microservices ecosystem.

## 🚀 Features

### Core Functionality
- **Self-Service Weighing**: Unmanned weighing operations for improved efficiency
- **Face Detection Authentication**: Advanced biometric authentication using Google ML Kit
- **Multi-Language Support**: English and Swahili localization
- **Real-Time Weight Monitoring**: Live weight readings with stability detection
- **Offline Capability**: Continue operations during network interruptions
- **Receipt Printing**: Automatic transaction receipt generation

### Security & Administration
- **Secure Configuration Management**: Encrypted configuration storage
- **Admin Access Control**: Key sequence-based admin mode activation
- **Service Discovery**: Automatic network service detection
- **Audit Logging**: Comprehensive activity tracking
- **Role-Based Access**: Granular permission system

### Integration
- **QaliTrack Microservices**: Full integration with existing backend services
- **API Gateway**: Centralized authentication and routing
- **Real-Time Synchronization**: Live data sync with central services
- **Transaction Management**: Complete transaction lifecycle handling

## 🏗️ Architecture

### Project Structure
```
lib/
├── core/                   # Core system functionality
│   ├── api/               # API client and networking
│   ├── auth/              # Authentication services
│   ├── config/            # Configuration management
│   ├── constants/         # Application constants
│   └── utils/             # Utility functions
├── features/              # Feature-specific modules
│   ├── authentication/    # Auth UI and logic
│   └── weighing/          # Weighing process UI
├── shared/                # Shared components
│   ├── models/            # Data models
│   ├── services/          # Business services
│   └── widgets/           # Reusable UI components
└── l10n/                  # Localization files
```

## 🛠️ Configuration

### Environment Variables
```bash
# API Configuration
API_BASE_URL=http://localhost:7000
API_TIMEOUT=30000

# Service Endpoints
USER_SERVICE_URL=http://localhost:7001
DRIVER_SERVICE_URL=http://localhost:7004
TRANSACTION_SERVICE_URL=http://localhost:7100
WEIGHT_DATA_SERVICE_URL=http://localhost:7101

# Security
JWT_ISSUER=QaliTrack
JWT_AUDIENCE=QaliTrack.Kiosk

# Hardware
FACE_DETECTION_CONFIDENCE_THRESHOLD=0.8
CAMERA_QUALITY=high

# UI Settings
DEFAULT_LANGUAGE=en
UI_TIMEOUT_SECONDS=300
KIOSK_MODE=true
```

### Admin Configuration
Access admin settings using the key sequence: `Ctrl + Shift + Alt + A`

## 🚀 Getting Started

### Prerequisites
- Flutter SDK 3.5.4+
- Dart SDK 3.0+
- Camera permissions
- Network access to QaliTrack services

### Installation

1. **Install Dependencies**
   ```bash
   flutter pub get
   ```

2. **Generate Code**
   ```bash
   flutter packages pub run build_runner build
   ```

3. **Configure Environment**
   ```bash
   # Edit .env with your configuration
   ```

4. **Run Application**
   ```bash
   flutter run
   ```

## 🧪 Testing

### Unit Tests
```bash
flutter test test/unit_tests.dart
```

## 🔧 Deployment

### Production Build
```bash
flutter build linux --release
```

## 🔐 Security Considerations

### Authentication
- JWT-based authentication with the QaliTrack ecosystem
- Biometric authentication for driver identification
- Admin access through secure key sequences
- Session timeout and automatic logout

### Data Protection
- Configuration encryption using device fingerprinting
- Secure storage of sensitive data
- TLS 1.3 for all network communications
- Local data sanitization on logout

## 🤝 Integration with QaliTrack Services

### Microservices Integration
- **User Service**: Authentication and authorization
- **Driver Service**: Biometric data and driver management
- **Transaction Service**: Transaction lifecycle management
- **Weight Data Service**: Real-time weight monitoring

## 📄 License

This project is part of the QaliTrack ecosystem and follows the same licensing terms as the main QaliTrack system.

---

**Version**: 1.0.0  
**Last Updated**: July 2024  
**Compatibility**: QaliTrack Services v2.0+
