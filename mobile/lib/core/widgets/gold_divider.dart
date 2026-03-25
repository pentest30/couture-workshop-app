import 'package:flutter/material.dart';
import '../theme/app_colors.dart';

class GoldDivider extends StatelessWidget {
  const GoldDivider({super.key});

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24),
      child: Container(height: 1, color: AppColors.secondaryFixed.withOpacity(0.4)),
    );
  }
}
