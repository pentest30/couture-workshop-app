import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../core/theme/app_colors.dart';

class ShellScreen extends StatelessWidget {
  final Widget child;
  const ShellScreen({super.key, required this.child});

  int _currentIndex(BuildContext context) {
    final location = GoRouterState.of(context).uri.toString();
    if (location == '/') return 0;
    if (location.startsWith('/orders')) return 1;
    if (location.startsWith('/new-order')) return 2;
    if (location.startsWith('/notifications')) return 3;
    return 0;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: child,
      bottomNavigationBar: Container(
        decoration: BoxDecoration(
          color: AppColors.surface,
          boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.04), blurRadius: 20, offset: const Offset(0, -4))],
        ),
        child: BottomNavigationBar(
          currentIndex: _currentIndex(context),
          onTap: (i) => switch (i) {
            0 => context.go('/'),
            1 => context.go('/orders'),
            2 => context.go('/new-order'),
            3 => context.go('/notifications'),
            _ => null,
          },
          items: const [
            BottomNavigationBarItem(icon: Icon(Icons.home_outlined), activeIcon: Icon(Icons.home), label: 'Accueil'),
            BottomNavigationBarItem(icon: Icon(Icons.receipt_long_outlined), activeIcon: Icon(Icons.receipt_long), label: 'Commandes'),
            BottomNavigationBarItem(icon: Icon(Icons.add_circle_outline, size: 32), activeIcon: Icon(Icons.add_circle, size: 32), label: 'Nouvelle'),
            BottomNavigationBarItem(icon: Icon(Icons.notifications_outlined), activeIcon: Icon(Icons.notifications), label: 'Alertes'),
          ],
        ),
      ),
    );
  }
}
