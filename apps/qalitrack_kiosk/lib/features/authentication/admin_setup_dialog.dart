import 'package:flutter/material.dart';
import 'package:virtual_keyboard_multi_language/virtual_keyboard_multi_language.dart';
import '../../core/config/kiosk_config.dart';
import 'admin_access.dart';

class AdminSetupDialog extends StatefulWidget {
  final AdminAccessProvider adminProvider;
  final bool isFromSettings;

  const AdminSetupDialog({Key? key, required this.adminProvider, this.isFromSettings = false}) : super(key: key);

  @override
  State<AdminSetupDialog> createState() => _AdminSetupDialogState();
}

class _AdminSetupDialogState extends State<AdminSetupDialog> {
  final _passwordController = TextEditingController();
  final _confirmPasswordController = TextEditingController();
  final _currentPasswordController = TextEditingController();
  bool _isLoading = false;
  String? _errorMessage;
  bool _showKeyboard = false;
  int _focusedFieldIndex = 0; // 0=current, 1=password, 2=confirm
  bool _showCurrentPassword = false;
  bool _showNewPassword = false;
  bool _showConfirmPassword = false;
  late List<FocusNode> _focusNodes;
  late List<TextEditingController> _controllers;
  late ScrollController _scrollController;
  final List<GlobalKey> _fieldKeys = [];

  @override
  void initState() {
    super.initState();
    _scrollController = ScrollController();
    _focusNodes = List.generate(3, (index) => FocusNode());
    _controllers = [
      _currentPasswordController,
      _passwordController,
      _confirmPasswordController
    ];
    
    // Create keys for each field
    for (int i = 0; i < 3; i++) {
      _fieldKeys.add(GlobalKey());
    }
    
    // Set default current password
    _currentPasswordController.text = 'admin123';
    
    for (int i = 0; i < _focusNodes.length; i++) {
      _focusNodes[i].addListener(() {
        if (_focusNodes[i].hasFocus) {
          setState(() {
            _showKeyboard = true;
            _focusedFieldIndex = i;
          });
          _scrollToField(i);
        }
      });
    }
    
    // Add listeners for Enter key detection
    for (int i = 0; i < _controllers.length; i++) {
      _controllers[i].addListener(() => _handleTextChange(i));
    }
  }

  @override
  void dispose() {
    for (int i = 0; i < _controllers.length; i++) {
      _controllers[i].removeListener(() => _handleTextChange(i));
    }
    _scrollController.dispose();
    for (final node in _focusNodes) {
      node.dispose();
    }
    for (final controller in _controllers) {
      controller.dispose();
    }
    super.dispose();
  }

  String? _validatePassword(String password) {
    if (password.length < 8) {
      return 'Password must be at least 8 characters long';
    }
    if (!password.contains(RegExp(r'[A-Z]'))) {
      return 'Password must contain at least one uppercase letter';
    }
    if (!password.contains(RegExp(r'[a-z]'))) {
      return 'Password must contain at least one lowercase letter';
    }
    if (!password.contains(RegExp(r'[0-9]'))) {
      return 'Password must contain at least one number';
    }
    if (!password.contains(RegExp(r'[!@#$%^&*(),.?":{}|<>]'))) {
      return 'Password must contain at least one special character';
    }
    return null;
  }

  String? _validateUsername(String username) {
    if (username.length < 3) {
      return 'Username must be at least 3 characters long';
    }
    if (username.contains(' ')) {
      return 'Username cannot contain spaces';
    }
    if (!username.contains(RegExp(r'^[a-zA-Z0-9_]+$'))) {
      return 'Username can only contain letters, numbers, and underscores';
    }
    return null;
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
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      widget.isFromSettings ? 'Change Admin Credentials' : 'Change Admin Password',
                      style: const TextStyle(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                        color: Colors.black87,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      widget.isFromSettings 
                        ? 'Update your admin credentials'
                        : 'Please change your password for security',
                      style: const TextStyle(
                        fontSize: 16,
                        color: Colors.black54,
                      ),
                    ),
                  ],
                ),
                Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: Colors.orange.shade100,
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(color: Colors.orange),
                      ),
                      child: const Text(
                        'REQUIRED',
                        style: TextStyle(
                          color: Colors.orange,
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                    const SizedBox(width: 8),
                    IconButton(
                      onPressed: () => Navigator.of(context).pop(),
                      icon: const Icon(Icons.close, color: Colors.black87),
                    ),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 24),
            
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
            
            // No action buttons needed - using Enter key for form submission
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
          // Current password field
          Container(
            key: _fieldKeys[0],
            child: TextField(
              controller: _currentPasswordController,
              focusNode: _focusNodes[0],
            decoration: InputDecoration(
              labelText: 'Current Password',
              border: const OutlineInputBorder(),
              prefixIcon: const Icon(Icons.lock_outline),
              suffixIcon: IconButton(
                icon: Icon(
                  _showCurrentPassword ? Icons.visibility : Icons.visibility_off,
                ),
                onPressed: () {
                  setState(() {
                    _showCurrentPassword = !_showCurrentPassword;
                  });
                },
              ),
              helperText: 'Enter the default password to verify',
            ),
            style: const TextStyle(fontSize: 18),
            obscureText: !_showCurrentPassword,
            enabled: !_isLoading,
            readOnly: true,
            onTap: () {
              _focusNodes[0].requestFocus();
            },
            ),
          ),
          const SizedBox(height: 16),
          
          // New password field
          Container(
            key: _fieldKeys[1],
            child: TextField(
              controller: _passwordController,
              focusNode: _focusNodes[1],
            decoration: InputDecoration(
              labelText: 'New Password',
              border: const OutlineInputBorder(),
              prefixIcon: const Icon(Icons.lock),
              suffixIcon: IconButton(
                icon: Icon(
                  _showNewPassword ? Icons.visibility : Icons.visibility_off,
                ),
                onPressed: () {
                  setState(() {
                    _showNewPassword = !_showNewPassword;
                  });
                },
              ),
              helperText: '8+ chars, upper/lower case, number, special char',
            ),
            style: const TextStyle(fontSize: 18),
            obscureText: !_showNewPassword,
            enabled: !_isLoading,
            readOnly: true,
            onTap: () {
              _focusNodes[1].requestFocus();
            },
            ),
          ),
          const SizedBox(height: 16),
          
          // Confirm password field
          Container(
            key: _fieldKeys[2],
            child: TextField(
              controller: _confirmPasswordController,
              focusNode: _focusNodes[2],
            decoration: InputDecoration(
              labelText: 'Confirm New Password',
              border: const OutlineInputBorder(),
              prefixIcon: const Icon(Icons.lock_reset),
              suffixIcon: IconButton(
                icon: Icon(
                  _showConfirmPassword ? Icons.visibility : Icons.visibility_off,
                ),
                onPressed: () {
                  setState(() {
                    _showConfirmPassword = !_showConfirmPassword;
                  });
                },
              ),
              helperText: 'Re-enter your new password',
            ),
            style: const TextStyle(fontSize: 18),
            obscureText: !_showConfirmPassword,
            enabled: !_isLoading,
            readOnly: true,
            onTap: () {
              _focusNodes[2].requestFocus();
            },
            ),
          ),
          const SizedBox(height: 16),
          
          // Security notice
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: Colors.amber.shade50,
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: Colors.amber.shade200),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  children: [
                    Icon(Icons.security, color: Colors.amber.shade700),
                    const SizedBox(width: 8),
                    Text(
                      'Security Requirements',
                      style: TextStyle(
                        fontWeight: FontWeight.bold,
                        color: Colors.amber.shade700,
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                const Text(
                  '• Password must be at least 8 characters\n'
                  '• Include uppercase and lowercase letters\n'
                  '• Include at least one number\n'
                  '• Include at least one special character\n'
                  '• Username must be unique and 3+ characters',
                  style: TextStyle(fontSize: 14),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          
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
                      'Field ${_focusedFieldIndex + 1} of 3: ${_getFieldLabel(_focusedFieldIndex)}',
                      style: const TextStyle(fontSize: 14, fontWeight: FontWeight.w500),
                    ),
                  ),
                  // Navigation buttons
                  IconButton(
                    onPressed: _focusedFieldIndex > 0 ? () {
                      setState(() {
                        _focusedFieldIndex--;
                      });
                      _focusNodes[_focusedFieldIndex].requestFocus();
                      _scrollToField(_focusedFieldIndex);
                    } : null,
                    icon: const Icon(Icons.keyboard_arrow_up),
                    tooltip: 'Previous field',
                  ),
                  IconButton(
                    onPressed: _focusedFieldIndex < 2 ? () {
                      setState(() {
                        _focusedFieldIndex++;
                      });
                      _focusNodes[_focusedFieldIndex].requestFocus();
                      _scrollToField(_focusedFieldIndex);
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
                textController: _controllers[_focusedFieldIndex],
                defaultLayouts: const [VirtualKeyboardDefaultLayouts.English],
                type: VirtualKeyboardType.Alphanumeric,
              ),
            ),
          ],
        ),
      ),
    );
  }

  void _scrollToField(int fieldIndex) {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (_fieldKeys[fieldIndex].currentContext != null) {
        final RenderBox renderBox = _fieldKeys[fieldIndex].currentContext!.findRenderObject() as RenderBox;
        final position = renderBox.localToGlobal(Offset.zero);
        
        // Scroll to make the field visible above the keyboard
        final keyboardHeight = 370.0; // Header + keyboard height
        final screenHeight = MediaQuery.of(context).size.height;
        final availableHeight = screenHeight - keyboardHeight;
        final fieldHeight = renderBox.size.height;
        
        // Calculate scroll position to center the field in the available space
        final fieldBottomPosition = position.dy + fieldHeight;
        final targetScrollPosition = fieldBottomPosition - availableHeight + 150; // Extra padding
        
        if (targetScrollPosition > 0) {
          _scrollController.animateTo(
            _scrollController.offset + targetScrollPosition,
            duration: const Duration(milliseconds: 400),
            curve: Curves.easeOutCubic,
          );
        }
      }
    });
  }

  void _handleTextChange(int fieldIndex) {
    final text = _controllers[fieldIndex].text;
    if (text.contains('\n')) {
      // Remove newline
      _controllers[fieldIndex].text = text.replaceAll('\n', '');
      
      if (fieldIndex < 2) {
        // Move to next field
        setState(() {
          _focusedFieldIndex = fieldIndex + 1;
        });
        _focusNodes[_focusedFieldIndex].requestFocus();
        _scrollToField(_focusedFieldIndex);
      } else {
        // Submit form if on last field
        _handleSetup();
      }
    }
  }

  String _getFieldLabel(int index) {
    switch (index) {
      case 0: return 'Current Password';
      case 1: return 'New Password';
      case 2: return 'Confirm Password';
      default: return 'Field';
    }
  }

  IconData _getFieldIcon(int index) {
    switch (index) {
      case 0: return Icons.lock_outline;
      case 1: return Icons.lock;
      case 2: return Icons.lock_reset;
      default: return Icons.text_fields;
    }
  }

  Future<void> _handleSetup() async {
    setState(() {
      _isLoading = true;
      _errorMessage = null;
    });

    // Validate current password
    if (_currentPasswordController.text != 'admin123') {
      setState(() {
        _errorMessage = 'Current password is incorrect';
        _isLoading = false;
      });
      return;
    }

    // Get current admin username from config
    final currentUsername = widget.adminProvider.config.getValue<String>(
      'security.default_admin_username', 
      'admin'
    ) ?? 'admin';

    // Validate new password
    final passwordError = _validatePassword(_passwordController.text);
    if (passwordError != null) {
      setState(() {
        _errorMessage = passwordError;
        _isLoading = false;
      });
      return;
    }

    // Validate password confirmation
    if (_passwordController.text != _confirmPasswordController.text) {
      setState(() {
        _errorMessage = 'Passwords do not match';
        _isLoading = false;
      });
      return;
    }

    // Update credentials (keep same username, just change password)
    final success = await widget.adminProvider.setupAdminCredentials(
      currentUsername,
      _passwordController.text,
    );

    if (success) {
      if (mounted) {
        if (widget.isFromSettings) {
          // Just pop back to settings - return success
          Navigator.of(context).pop(true);
        } else {
          // Pop back to home after first-time setup
          Navigator.of(context).pop(true);
        }
      }
    } else {
      setState(() {
        _errorMessage = 'Failed to update credentials. Please try again.';
        _isLoading = false;
      });
    }
  }
}