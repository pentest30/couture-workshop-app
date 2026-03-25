import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../theme/app_colors.dart';

class WorkTypeBadge extends StatelessWidget {
  final String workType;

  const WorkTypeBadge({super.key, required this.workType});

  Color get _color => switch (workType) {
    'Brode' => AppColors.statusBroderie,
    'Perle' => AppColors.statusPerlage,
    'Mixte' => AppColors.primary,
    _ => AppColors.onSurfaceVariant,
  };

  String get _label => switch (workType) {
    'Simple' => 'Couture',
    'Brode' => 'Embroidery',
    'Perle' => 'Beading',
    'Mixte' => 'Mixte',
    _ => workType,
  };

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(
        color: _color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(6),
      ),
      child: Text(
        _label.toUpperCase(),
        style: GoogleFonts.manrope(fontSize: 9, fontWeight: FontWeight.w700, letterSpacing: 1, color: _color),
      ),
    );
  }
}
