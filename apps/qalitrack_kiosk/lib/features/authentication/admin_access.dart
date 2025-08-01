import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:logger/logger.dart';
import '../../core/config/kiosk_config.dart';
import '../../shared/models/auth_models.dart';
import '../../core/auth/auth_service.dart';

class AdminAccessProvider extends ChangeNotifier {
  final Logger _logger = Logger();
  final KioskConfig _config = KioskConfig();
  final AuthService _authService = AuthService();
  
  bool _isAdminMode = false;
  bool _isListeningForKeySequence = false;
  List<String> _currentKeySequence = [];
  DateTime? _adminSessionStart;
  
  bool get isAdminMode => _isAdminMode;
  bool get isAdminSessionValid {
    if (_adminSessionStart == null) return false;
    final timeout = _config.getValue<int>('security.admin_session_timeout', 300);
    return DateTime.now().difference(_adminSessionStart!).inSeconds < (timeout ?? 300);
  }

  void startKeySequenceListener() {
    _isListeningForKeySequence = true;
    _currentKeySequence.clear();
    _logger.d('Started listening for admin key sequence');
  }

  void stopKeySequenceListener() {
    _isListeningForKeySequence = false;
    _currentKeySequence.clear();
    _logger.d('Stopped listening for admin key sequence');
  }

  bool handleKeyPress(RawKeyEvent event) {
    if (!_isListeningForKeySequence) return false;
    
    if (event is RawKeyDownEvent) {
      final expectedSequence = _config.getValue<List<dynamic>>(
        'ui.admin_access_key_sequence', 
        ['ctrl', 'shift', 'alt', 'a']
      ) ?? ['ctrl', 'shift', 'alt', 'a'];
      
      String? keyPressed;
      
      if (event.isControlPressed) keyPressed = 'ctrl';
      else if (event.isShiftPressed) keyPressed = 'shift';
      else if (event.isAltPressed) keyPressed = 'alt';
      else if (event.logicalKey == LogicalKeyboardKey.keyA) keyPressed = 'a';
      
      if (keyPressed != null) {
        _currentKeySequence.add(keyPressed);
        
        // Check if sequence matches
        if (_currentKeySequence.length == expectedSequence.length) {
          bool matches = true;
          for (int i = 0; i < expectedSequence.length; i++) {
            if (_currentKeySequence[i] != expectedSequence[i]) {
              matches = false;
              break;
            }
          }
          
          if (matches) {
            _logger.i('Admin key sequence detected');
            _showAdminLoginDialog();
            return true;
          } else {
            _currentKeySequence.clear();
          }
        } else if (_currentKeySequence.length > expectedSequence.length) {
          _currentKeySequence.clear();
        }
      }
    }
    
    return false;
  }

  Future<void> _showAdminLoginDialog() async {
    // This would be called from the UI layer
    _logger.d('Admin login dialog requested');
  }

  Future<bool> authenticateAdmin(String username, String password) async {
    try {
      _logger.i('Attempting admin authentication for: $username');
      
      // Try local default credentials first
      if (_authenticateLocalAdmin(username, password)) {
        _isAdminMode = true;
        _adminSessionStart = DateTime.now();
        _logger.i('Local admin authentication successful');
        notifyListeners();
        return true;
      }
      
      // Try remote authentication if local fails
      try {
        final loginRequest = LoginRequest(
          username: username,
          password: password,
          deviceId: 'kiosk-admin',
          deviceType: 'kiosk_admin',
        );
        
        final response = await _authService.login(loginRequest);
        
        if (response != null && response.roles.contains('Admin')) {
          _isAdminMode = true;
          _adminSessionStart = DateTime.now();
          _logger.i('Remote admin authentication successful');
          notifyListeners();
          return true;
        }
      } catch (e) {
        _logger.w('Remote admin authentication failed: $e');
      }
      
      _logger.w('Admin authentication failed');
      return false;
    } catch (e) {
      _logger.e('Admin authentication error: $e');
      return false;
    }
  }
  
  bool _authenticateLocalAdmin(String username, String password) {
    final defaultUsername = _config.getValue<String>(
      'security.default_admin_username', 
      'admin'
    ) ?? 'admin';
    
    final defaultPassword = _config.getValue<String>(
      'security.default_admin_password', 
      'admin123'
    ) ?? 'admin123';
    
    return username == defaultUsername && password == defaultPassword;
  }
  
  Future<bool> updateAdminCredentials(String currentPassword, String newUsername, String newPassword) async {
    try {
      // Verify current password
      final currentUsername = _config.getValue<String>(
        'security.default_admin_username', 
        'admin'
      ) ?? 'admin';
      
      if (!_authenticateLocalAdmin(currentUsername, currentPassword)) {
        return false;
      }
      
      // Update credentials
      await _config.setValue('security.default_admin_username', newUsername);
      await _config.setValue('security.default_admin_password', newPassword);
      
      _logger.i('Admin credentials updated successfully');
      return true;
    } catch (e) {
      _logger.e('Failed to update admin credentials: $e');
      return false;
    }
  }

  void exitAdminMode() {
    _isAdminMode = false;
    _adminSessionStart = null;
    _authService.logout();
    _logger.i('Exited admin mode');
    notifyListeners();
  }

  void refreshAdminSession() {
    if (_isAdminMode) {
      _adminSessionStart = DateTime.now();
      _logger.d('Admin session refreshed');
    }
  }
}

class AdminKeySequenceListener extends StatefulWidget {
  final Widget child;
  final AdminAccessProvider adminProvider;

  const AdminKeySequenceListener({
    Key? key,
    required this.child,
    required this.adminProvider,
  }) : super(key: key);

  @override
  State<AdminKeySequenceListener> createState() => _AdminKeySequenceListenerState();
}

class _AdminKeySequenceListenerState extends State<AdminKeySequenceListener> {
  @override
  void initState() {
    super.initState();
    widget.adminProvider.startKeySequenceListener();
  }

  @override
  void dispose() {
    widget.adminProvider.stopKeySequenceListener();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return RawKeyboardListener(
      focusNode: FocusNode(),
      autofocus: true,
      onKey: (RawKeyEvent event) {
        widget.adminProvider.handleKeyPress(event);
      },
      child: widget.child,
    );
  }
}

class AdminLoginDialog extends StatefulWidget {
  final AdminAccessProvider adminProvider;

  const AdminLoginDialog({Key? key, required this.adminProvider}) : super(key: key);

  @override
  State<AdminLoginDialog> createState() => _AdminLoginDialogState();
}

class _AdminLoginDialogState extends State<AdminLoginDialog> {
  final _usernameController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isLoading = false;
  String? _errorMessage;

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: const Text('Administrator Access'),
      content: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          TextField(
            controller: _usernameController,
            decoration: const InputDecoration(
              labelText: 'Username',
              border: OutlineInputBorder(),
            ),
            enabled: !_isLoading,
          ),
          const SizedBox(height: 16),
          TextField(
            controller: _passwordController,
            decoration: const InputDecoration(
              labelText: 'Password',
              border: OutlineInputBorder(),
            ),
            obscureText: true,
            enabled: !_isLoading,
            onSubmitted: (_) => _handleLogin(),
          ),
          if (_errorMessage != null) ...[
            const SizedBox(height: 16),
            Text(
              _errorMessage!,
              style: TextStyle(color: Theme.of(context).colorScheme.error),
            ),
          ],
        ],
      ),
      actions: [
        TextButton(
          onPressed: _isLoading ? null : () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        ElevatedButton(
          onPressed: _isLoading ? null : _handleLogin,
          child: _isLoading
              ? const SizedBox(
                  width: 20,
                  height: 20,
                  child: CircularProgressIndicator(strokeWidth: 2),
                )
              : const Text('Login'),
        ),
      ],
    );
  }

  Future<void> _handleLogin() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    final success = await widget.adminProvider.authenticateAdmin(
      _usernameController.text,
      _passwordController.text,
    );

    if (success) {
      if (mounted) Navigator.of(context).pop(true);
    } else {
      setState(() {
        _errorMessage = 'Invalid credentials or insufficient privileges';
        _isLoading = false;
      });
    }
  }

  @override
  void dispose() {
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }
}