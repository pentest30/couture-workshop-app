import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'app/router.dart';
import 'core/theme/app_theme.dart';

void main() {
  runApp(const ProviderScope(child: CoutureApp()));
}

class CoutureApp extends StatelessWidget {
  const CoutureApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: "L'Atelier Couture",
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      routerConfig: router,
    );
  }
}
