import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/api/api_client.dart';
import '../../core/providers/providers.dart';
import '../../core/theme/app_colors.dart';

class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});
  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _emailController = TextEditingController(text: 'admin@couture.local');
  final _passwordController = TextEditingController(text: 'Admin123!');
  bool _loading = false;
  String? _error;

  Future<void> _login() async {
    setState(() { _loading = true; _error = null; });
    try {
      final api = ref.read(apiClientProvider);
      final data = await api.login(_emailController.text, _passwordController.text);
      ref.read(authStateProvider.notifier).state = data;
      if (mounted) context.go('/');
    } catch (e) {
      final msg = e.toString();
      if (msg.contains('401') || msg.contains('Unauthorized')) {
        setState(() => _error = 'Identifiants invalides');
      } else {
        setState(() => _error = 'Erreur de connexion: $msg');
      }
    } finally {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text("L'Atelier", style: GoogleFonts.notoSerif(fontSize: 36, fontWeight: FontWeight.w700, color: AppColors.primary)),
              Text('Couture', style: GoogleFonts.notoSerif(fontSize: 36, fontWeight: FontWeight.w300, color: AppColors.secondary)),
              const SizedBox(height: 48),
              TextField(controller: _emailController, decoration: const InputDecoration(hintText: 'Email', prefixIcon: Icon(Icons.email_outlined))),
              const SizedBox(height: 16),
              TextField(controller: _passwordController, obscureText: true, decoration: const InputDecoration(hintText: 'Mot de passe', prefixIcon: Icon(Icons.lock_outline))),
              if (_error != null) ...[const SizedBox(height: 12), Text(_error!, style: TextStyle(color: AppColors.error, fontSize: 13))],
              const SizedBox(height: 32),
              SizedBox(
                width: double.infinity, height: 52,
                child: ElevatedButton(
                  onPressed: _loading ? null : _login,
                  style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder()),
                  child: _loading ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2)) : Text('Se connecter', style: GoogleFonts.manrope(fontSize: 16, fontWeight: FontWeight.w600)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
