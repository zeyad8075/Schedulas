import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../providers/auth_providers.dart';

class RegisterScreen extends ConsumerStatefulWidget {
  const RegisterScreen({super.key});

  @override
  ConsumerState<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends ConsumerState<RegisterScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _fullNameController = TextEditingController();
  final _institutionIdController = TextEditingController();
  final _departmentIdController = TextEditingController();

  // Defaulting to Student(1) for this example form; would typically use a dropdown
  int _selectedRole = 1;

  bool _isSubmitting = false;
  bool _submitted = false;
  String? _errorMessage;

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    _fullNameController.dispose();
    _institutionIdController.dispose();
    _departmentIdController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isSubmitting = true;
      _errorMessage = null;
    });

    final result = await ref.read(authRepositoryProvider).register(
          _emailController.text.trim(),
          _passwordController.text,
          _fullNameController.text.trim(),
          _selectedRole,
          _institutionIdController.text.trim(),
          _departmentIdController.text.trim().isEmpty
              ? null
              : _departmentIdController.text.trim(),
        );

    if (mounted) {
      setState(() {
        _isSubmitting = false;
      });
    }

    result.fold(
      (failure) {
        if (mounted) {
          setState(() {
            _errorMessage = failure.message;
          });
        }
      },
      (_) {
        if (mounted) {
          setState(() => _submitted = true);
        }
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: const Text('تسجيل مستخدم جديد')),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: _submitted
                  ? _buildSuccessState(theme)
                  : _buildFormState(theme),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildSuccessState(ThemeData theme) {
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        Icon(Icons.check_circle_outline,
            size: 64, color: theme.colorScheme.primary),
        const SizedBox(height: 16),
        Text(
          'تم إرسال رابط تأكيد البريد الإلكتروني',
          textAlign: TextAlign.center,
          style: theme.textTheme.titleMedium,
        ),
      ],
    );
  }

  Widget _buildFormState(ThemeData theme) {
    return Form(
      key: _formKey,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          if (_errorMessage != null) ...[
            Text(_errorMessage!,
                style: TextStyle(color: theme.colorScheme.error)),
            const SizedBox(height: 12),
          ],
          TextFormField(
            controller: _fullNameController,
            decoration: const InputDecoration(
                labelText: 'الاسم الكامل',
                prefixIcon: Icon(Icons.person_outline)),
            validator: (value) =>
                (value == null || value.trim().isEmpty) ? 'الاسم مطلوب' : null,
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _emailController,
            keyboardType: TextInputType.emailAddress,
            decoration: const InputDecoration(
                labelText: 'البريد الإلكتروني',
                prefixIcon: Icon(Icons.email_outlined)),
            validator: (value) => (value == null || value.trim().isEmpty)
                ? 'البريد الإلكتروني مطلوب'
                : null,
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _passwordController,
            obscureText: true,
            decoration: const InputDecoration(
                labelText: 'كلمة المرور', prefixIcon: Icon(Icons.lock_outline)),
            validator: (value) =>
                (value == null || value.isEmpty) ? 'كلمة المرور مطلوبة' : null,
          ),
          const SizedBox(height: 16),
          TextFormField(
            controller: _institutionIdController,
            decoration: const InputDecoration(
                labelText: 'معرف المؤسسة (UUID)',
                prefixIcon: Icon(Icons.account_balance_outlined)),
            validator: (value) => (value == null || value.trim().isEmpty)
                ? 'معرف المؤسسة مطلوب'
                : null,
          ),
          const SizedBox(height: 16),
          DropdownButtonFormField<int>(
            initialValue: _selectedRole,
            decoration: const InputDecoration(
                labelText: 'الدور', prefixIcon: Icon(Icons.badge_outlined)),
            items: const [
              DropdownMenuItem(value: 1, child: Text('طالب')),
              DropdownMenuItem(value: 2, child: Text('معلم')),
              DropdownMenuItem(value: 3, child: Text('ولي أمر')),
              DropdownMenuItem(value: 4, child: Text('مدير قسم')),
              DropdownMenuItem(value: 5, child: Text('مدير مؤسسة')),
            ],
            onChanged: (value) {
              if (value != null) {
                setState(() {
                  _selectedRole = value;
                });
              }
            },
          ),
          const SizedBox(height: 24),
          ElevatedButton(
            onPressed: _isSubmitting ? null : _submit,
            child: _isSubmitting
                ? const SizedBox(
                    height: 22,
                    width: 22,
                    child: CircularProgressIndicator(
                        strokeWidth: 2.5, color: Colors.white))
                : const Text('تسجيل'),
          ),
        ],
      ),
    );
  }
}
