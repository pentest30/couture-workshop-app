import 'package:flutter/material.dart';

class AppColors {
  static const primary = Color(0xFF410050);
  static const primaryContainer = Color(0xFF5C1A6B);
  static const secondary = Color(0xFF7F5700);
  static const secondaryFixed = Color(0xFFF6BD5E);
  static const background = Color(0xFFFCF9F4);
  static const surface = Color(0xFFFFFFFF);
  static const surfaceContainer = Color(0xFFF0EDE9);
  static const surfaceContainerLow = Color(0xFFF6F3EF);
  static const onSurface = Color(0xFF1C1C19);
  static const onSurfaceVariant = Color(0xFF4D4544);
  static const outlineVariant = Color(0xFFD2C2CF);
  static const error = Color(0xFFC62828);

  // Status colors
  static const statusRecue = Color(0xFF1565C0);
  static const statusEnAttente = Color(0xFFF9A825);
  static const statusEnCours = Color(0xFFE65100);
  static const statusBroderie = Color(0xFF6A1B9A);
  static const statusPerlage = Color(0xFF880E4F);
  static const statusRetouche = Color(0xFFC62828);
  static const statusPrete = Color(0xFF2E7D32);
  static const statusLivree = Color(0xFF424242);

  static Color statusColor(String status) => switch (status) {
    'Recue' => statusRecue,
    'EnAttente' => statusEnAttente,
    'EnCours' => statusEnCours,
    'Broderie' => statusBroderie,
    'Perlage' => statusPerlage,
    'Retouche' => statusRetouche,
    'Prete' => statusPrete,
    'Livree' => statusLivree,
    _ => onSurfaceVariant,
  };

  static Color statusBgColor(String status) => statusColor(status).withOpacity(0.12);
}
