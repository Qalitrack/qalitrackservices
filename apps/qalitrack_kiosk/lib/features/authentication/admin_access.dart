import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:logger/logger.dart';
import 'package:virtual_keyboard_multi_language/virtual_keyboard_multi_language.dart';
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
  bool _showKeyboard = false;
  bool _isPasswordField = false;
  late FocusNode _usernameFocus;
  late FocusNode _passwordFocus;

  @override
  void initState() {
    super.initState();
    _usernameFocus = FocusNode();
    _passwordFocus = FocusNode();
    
    _usernameFocus.addListener(() {
      if (_usernameFocus.hasFocus) {
        setState(() {
          _showKeyboard = true;
          _isPasswordField = false;
        });
      }
    });
    
    _passwordFocus.addListener(() {
      if (_passwordFocus.hasFocus) {
        setState(() {
          _showKeyboard = true;
          _isPasswordField = true;
        });
      }
    });
  }

  @override
  void dispose() {
    _usernameFocus.dispose();
    _passwordFocus.dispose();
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Dialog(
      child: Container(
        width: MediaQuery.of(context).size.width * 0.9,
        height: MediaQuery.of(context).size.height * 0.8,
        padding: const EdgeInsets.all(24),
        child: Column(
          children: [
            // Header
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Administrator Access',
                  style: TextStyle(
                    fontSize: 24,
                    fontWeight: FontWeight.bold,
                  ),
                ),
                IconButton(
                  onPressed: () => Navigator.of(context).pop(),
                  icon: const Icon(Icons.close),
                ),
              ],
            ),
            const SizedBox(height: 24),
            
            // Input fields
            Expanded(
              flex: 2,
              child: Column(
                children: [
                  TextField(
                    controller: _usernameController,
                    focusNode: _usernameFocus,
                    decoration: const InputDecoration(
                      labelText: 'Username',
                      border: OutlineInputBorder(),
                      prefixIcon: Icon(Icons.person),
                    ),
                    style: const TextStyle(fontSize: 18),
                    enabled: !_isLoading,
                    readOnly: true, // Prevent system keyboard
                    onTap: () {
                      _usernameFocus.requestFocus();
                    },
                  ),
                  const SizedBox(height: 16),
                  TextField(
                    controller: _passwordController,
                    focusNode: _passwordFocus,
                    decoration: const InputDecoration(
                      labelText: 'Password',
                      border: OutlineInputBorder(),
                      prefixIcon: Icon(Icons.lock),
                    ),
                    style: const TextStyle(fontSize: 18),
                    obscureText: true,
                    enabled: !_isLoading,
                    readOnly: true, // Prevent system keyboard
                    onTap: () {
                      _passwordFocus.requestFocus();
                    },
                  ),
                  const SizedBox(height: 16),
                  
                  // Default credentials hint
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: Colors.blue.shade50,
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: Colors.blue.shade200),
                    ),
                    child: const Row(
                      children: [
                        Icon(Icons.info_outline, color: Colors.blue),
                        SizedBox(width: 8),
                        Text(
                          'Default: admin / admin123',
                          style: TextStyle(color: Colors.blue),
                        ),
                      ],
                    ),
                  ),
                  
                  if (_errorMessage != null) ...[
                    const SizedBox(height: 16),
                    Container(
                      padding: const EdgeInsets.all(12),
                      decoration: BoxDecoration(
                        color: Colors.red.shade50,
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: Colors.red.shade200),
                      ),
                      child: Row(
                        children: [
                          const Icon(Icons.error_outline, color: Colors.red),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              _errorMessage!,
                              style: const TextStyle(color: Colors.red),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ],
              ),
            ),
            
            // Virtual Keyboard
            if (_showKeyboard) ...[
              const Divider(),
              Expanded(
                flex: 3,
                child: VirtualKeyboard(
                  height: 300,
                  textColor: Colors.black,
                  textController: _isPasswordField ? _passwordController : _usernameController,
                  defaultLayouts: const [VirtualKeyboardDefaultLayouts.English],
                  type: VirtualKeyboardType.Alphanumeric,
                ),
              ),
            ],
            
            // Action buttons
            const SizedBox(height: 16),
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                if (_showKeyboard)
                  TextButton(
                    onPressed: () {
                      setState(() {
                        _showKeyboard = false;
                      });
                      FocusScope.of(context).unfocus();
                    },
                    child: const Text('Hide Keyboard'),
                  ),
                const SizedBox(width: 16),
                TextButton(
                  onPressed: _isLoading ? null : () => Navigator.of(context).pop(),
                  child: const Text('Cancel'),
                ),
                const SizedBox(width: 16),
                ElevatedButton(
                  onPressed: _isLoading ? null : _handleLogin,
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 16),
                  ),
                  child: _isLoading
                      ? const SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Text('Login', style: TextStyle(fontSize: 16)),
                ),
              ],
            ),
          ],
        ),
      ),
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

}