import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:logger/logger.dart';
import 'package:virtual_keyboard_multi_language/virtual_keyboard_multi_language.dart';
import '../../core/config/kiosk_config.dart';
import '../../shared/models/auth_models.dart';
import '../../core/auth/auth_service.dart';
import 'admin_setup_dialog.dart';

class AdminAccessProvider extends ChangeNotifier {
  final Logger _logger = Logger();
  final KioskConfig _config = KioskConfig();
  final AuthService _authService = AuthService();
  
  bool _isAdminMode = false;
  bool _isListeningForKeySequence = false;
  List<String> _currentKeySequence = [];
  DateTime? _adminSessionStart;
  
  bool get isAdminMode => _isAdminMode;
  KioskConfig get config => _config;
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
      
      // Check if first-time setup is required
      if (_config.isFirstTimeSetup) {
        _logger.w('First-time setup required');
        return false;
      }
      
      // Use stored credentials for authentication (local only)
      if (_authenticateSetupAdmin(username, password)) {
        _isAdminMode = true;
        _adminSessionStart = DateTime.now();
        _logger.i('Admin authentication successful (stored credentials)');
        notifyListeners();
        return true;
      }
      
      // Fallback to default credentials if stored credentials don't exist
      if (!_config.areCredentialsSet && _authenticateLocalAdmin(username, password)) {
        _isAdminMode = true;
        _adminSessionStart = DateTime.now();
        _logger.i('Admin authentication successful (default credentials)');
        notifyListeners();
        return true;
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

  Future<bool> setupAdminCredentials(String newUsername, String newPassword) async {
    try {
      // Store encrypted credentials
      await _config.setValue('security.admin_username', newUsername);
      await _config.setValue('security.admin_password', newPassword);
      
      // Mark first-time setup as complete
      await _config.markFirstTimeSetupComplete();
      
      _logger.i('Admin credentials setup completed successfully');
      return true;
    } catch (e) {
      _logger.e('Failed to setup admin credentials: $e');
      return false;
    }
  }

  bool _authenticateSetupAdmin(String username, String password) {
    // After setup, use the stored credentials
    final storedUsername = _config.getValue<String>('security.admin_username');
    final storedPassword = _config.getValue<String>('security.admin_password');
    
    if (storedUsername != null && storedPassword != null) {
      return username == storedUsername && password == storedPassword;
    }
    
    return false;
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
  bool _showPassword = false;
  late FocusNode _usernameFocus;
  late FocusNode _passwordFocus;
  late ScrollController _scrollController;
  final GlobalKey _usernameKey = GlobalKey();
  final GlobalKey _passwordKey = GlobalKey();

  @override
  void initState() {
    super.initState();
    _scrollController = ScrollController();
    _usernameFocus = FocusNode();
    _passwordFocus = FocusNode();
    
    _usernameFocus.addListener(() {
      if (_usernameFocus.hasFocus) {
        setState(() {
          _showKeyboard = true;
          _isPasswordField = false;
        });
        _scrollToActiveField();
      }
    });
    
    _passwordFocus.addListener(() {
      if (_passwordFocus.hasFocus) {
        setState(() {
          _showKeyboard = true;
          _isPasswordField = true;
        });
        _scrollToActiveField();
      }
    });
    
    // Add listeners for Enter key detection
    _usernameController.addListener(_handleUsernameChange);
    _passwordController.addListener(_handlePasswordChange);
  }

  @override
  void dispose() {
    _usernameController.removeListener(_handleUsernameChange);
    _passwordController.removeListener(_handlePasswordChange);
    _scrollController.dispose();
    _usernameFocus.dispose();
    _passwordFocus.dispose();
    _usernameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: Container(
        width: MediaQuery.of(context).size.width,
        height: MediaQuery.of(context).size.height,
        padding: const EdgeInsets.all(32),
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
                    color: Colors.black87,
                  ),
                ),
                IconButton(
                  onPressed: () => Navigator.of(context).pop(),
                  icon: const Icon(Icons.close, color: Colors.black87),
                ),
              ],
            ),
            const SizedBox(height: 16),
            
            // Content area with proper layout
            Expanded(
              child: Stack(
                children: [
                  // Form layer (can scroll behind keyboard)
                  _buildFormLayout(),
                  // Keyboard overlay (fixed at bottom when visible)
                  if (_showKeyboard) _buildKeyboardOverlay(),
                ],
              ),
            ),
            
            // Action buttons (simplified - only show cancel when no keyboard)
            if (!_showKeyboard)
              Row(
                mainAxisAlignment: MainAxisAlignment.end,
                children: [
                  TextButton(
                    onPressed: _isLoading ? null : () => Navigator.of(context).pop(),
                    child: const Text('Cancel'),
                  ),
                ],
              ),
          ],
        ),
      ),
    );
  }

  Widget _buildFormLayout() {
    return SingleChildScrollView(
      controller: _scrollController,
      padding: EdgeInsets.only(bottom: _showKeyboard ? 370 : 0), // Add padding when keyboard is visible
      child: Column(
        children: [
          // Input fields
          Container(
            key: _usernameKey,
            child: TextField(
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
          ),
          const SizedBox(height: 16),
          Container(
            key: _passwordKey,
            child: TextField(
              controller: _passwordController,
              focusNode: _passwordFocus,
            decoration: InputDecoration(
              labelText: 'Password',
              border: const OutlineInputBorder(),
              prefixIcon: const Icon(Icons.lock),
              suffixIcon: IconButton(
                icon: Icon(
                  _showPassword ? Icons.visibility : Icons.visibility_off,
                ),
                onPressed: () {
                  setState(() {
                    _showPassword = !_showPassword;
                  });
                },
              ),
            ),
            style: const TextStyle(fontSize: 18),
            obscureText: !_showPassword,
            enabled: !_isLoading,
            readOnly: true, // Prevent system keyboard
            onTap: () {
              _passwordFocus.requestFocus();
            },
            ),
          ),
          const SizedBox(height: 16),
          
          // Default credentials hint - only shown when credentials not set
          if (!widget.adminProvider.config.areCredentialsSet) ...[
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
            const SizedBox(height: 16),
          ],
          
          // Error message
          if (_errorMessage != null) ...[
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
            const SizedBox(height: 16),
          ],
        ],
      ),
    );
  }

  Widget _buildKeyboardOverlay() {
    return Positioned(
      bottom: 0,
      left: 0,
      right: 0,
      child: Container(
        decoration: const BoxDecoration(
          color: Colors.white,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            // Keyboard header with field info and controls
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
              decoration: BoxDecoration(
                color: Colors.grey.shade100,
                border: Border(bottom: BorderSide(color: Colors.grey.shade300)),
              ),
              child: Row(
                children: [
                  // Field indicator
                  Expanded(
                    child: Text(
                      'Field ${_isPasswordField ? 2 : 1} of 2: ${_isPasswordField ? 'Password' : 'Username'}',
                      style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                    ),
                  ),
                  // Navigation buttons
                  IconButton(
                    onPressed: _isPasswordField ? () {
                      _usernameFocus.requestFocus();
                    } : null,
                    icon: const Icon(Icons.keyboard_arrow_up),
                    tooltip: 'Previous field',
                  ),
                  IconButton(
                    onPressed: !_isPasswordField ? () {
                      _passwordFocus.requestFocus();
                    } : null,
                    icon: const Icon(Icons.keyboard_arrow_down),
                    tooltip: 'Next field',
                  ),
                  IconButton(
                    onPressed: () {
                      setState(() {
                        _showKeyboard = false;
                      });
                      FocusScope.of(context).unfocus();
                    },
                    icon: const Icon(Icons.keyboard_hide),
                    tooltip: 'Hide keyboard',
                  ),
                ],
              ),
            ),
            // Virtual Keyboard - full width, no margins or borders
            SizedBox(
              height: 300,
              width: double.infinity,
              child: VirtualKeyboard(
                height: 300,
                textColor: Colors.black,
                textController: _isPasswordField ? _passwordController : _usernameController,
                defaultLayouts: const [VirtualKeyboardDefaultLayouts.English],
                type: VirtualKeyboardType.Alphanumeric,
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _scrollToActiveField() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      final GlobalKey activeKey = _isPasswordField ? _passwordKey : _usernameKey;
      if (activeKey.currentContext != null) {
        final RenderBox renderBox = activeKey.currentContext!.findRenderObject() as RenderBox;
        final position = renderBox.localToGlobal(Offset.zero);
        
        // Scroll to make the field visible above the keyboard
        final keyboardHeight = 350.0; // Keyboard height
        final screenHeight = MediaQuery.of(context).size.height;
        final availableHeight = screenHeight - keyboardHeight;
        final targetPosition = position.dy - 100; // Add some padding above the field
        
        if (targetPosition > availableHeight || position.dy > availableHeight) {
          _scrollController.animateTo(
            _scrollController.offset + (targetPosition - availableHeight + 100),
            duration: const Duration(milliseconds: 300),
            curve: Curves.easeInOut,
          );
        }
      }
    });
  }

  void _handleUsernameChange() {
    final text = _usernameController.text;
    if (text.contains('\n')) {
      // Remove newline and move to next field
      _usernameController.text = text.replaceAll('\n', '');
      _passwordFocus.requestFocus();
    }
  }

  void _handlePasswordChange() {
    final text = _passwordController.text;
    if (text.contains('\n')) {
      // Remove newline and submit form
      _passwordController.text = text.replaceAll('\n', '');
      _handleLogin();
    }
  }

  Future<void> _handleLogin() async {
    // Validate input first
    if (_usernameController.text.trim().isEmpty || _passwordController.text.trim().isEmpty) {
      setState(() {
        _errorMessage = 'Please enter both username and password';
      });
      return;
    }

    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    // Check if first-time setup is required
    if (widget.adminProvider.config.isFirstTimeSetup) {
      // Validate default credentials before allowing setup
      final defaultUsername = widget.adminProvider.config.getValue<String>(
        'security.default_admin_username', 
        'admin'
      ) ?? 'admin';
      
      final defaultPassword = widget.adminProvider.config.getValue<String>(
        'security.default_admin_password', 
        'admin123'
      ) ?? 'admin123';

      if (_usernameController.text != defaultUsername || _passwordController.text != defaultPassword) {
        setState(() {
          _errorMessage = 'Invalid default credentials';
          _isLoading = false;
        });
        return;
      }

      // Credentials are valid, proceed to setup
      setState(() {
        _isLoading = false;
      });
      
      Navigator.of(context).pop();
      final setupResult = await Navigator.of(context).push<bool>(
        MaterialPageRoute(
          builder: (context) => AdminSetupDialog(
            adminProvider: widget.adminProvider,
            isFromSettings: false, // This is first-time setup, not from settings
          ),
          fullscreenDialog: true,
        ),
      );
      if (setupResult == true && mounted) {
        Navigator.of(context).pop(true);
      }
      return;
    }

    // Normal authentication
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