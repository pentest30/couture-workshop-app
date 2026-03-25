import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'app_colors.dart';

class AppTheme {
  static ThemeData get light => ThemeData(
    useMaterial3: true,
    brightness: Brightness.light,
    scaffoldBackgroundColor: AppColors.background,
    colorScheme: const ColorScheme.light(
      primary: AppColors.primary,
      primaryContainer: AppColors.primaryContainer,
      secondary: AppColors.secondary,
      surface: AppColors.surface,
      error: AppColors.error,
      onPrimary: Colors.white,
      onSurface: AppColors.onSurface,
      onSurfaceVariant: AppColors.onSurfaceVariant,
      outlineVariant: AppColors.outlineVariant,
    ),
    textTheme: _textTheme,
    cardTheme: CardThemeData(
      color: AppColors.surface,
      elevation: 0,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      margin: EdgeInsets.zero,
    ),
    appBarTheme: AppBarTheme(
      backgroundColor: Colors.transparent,
      elevation: 0,
      scrolledUnderElevation: 0,
      titleTextStyle: GoogleFonts.notoSerif(
        fontSize: 20, fontWeight: FontWeight.w600, color: AppColors.onSurface,
      ),
      iconTheme: const IconThemeData(color: AppColors.onSurface),
    ),
    bottomNavigationBarTheme: const BottomNavigationBarThemeData(
      backgroundColor: AppColors.surface,
      selectedItemColor: AppColors.primary,
      unselectedItemColor: AppColors.onSurfaceVariant,
      type: BottomNavigationBarType.fixed,
      elevation: 0,
    ),
    floatingActionButtonTheme: const FloatingActionButtonThemeData(
      backgroundColor: AppColors.primary,
      foregroundColor: Colors.white,
      shape: CircleBorder(),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: AppColors.surfaceContainerLow,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(12),
        borderSide: BorderSide.none,
      ),
      contentPadding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
    ),
    chipTheme: ChipThemeData(
      backgroundColor: AppColors.surfaceContainerLow,
      selectedColor: AppColors.primary,
      labelStyle: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w500),
      shape: const StadiumBorder(),
      side: BorderSide.none,
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
    ),
  );

  static TextTheme get _textTheme => TextTheme(
    displayLarge: GoogleFonts.notoSerif(fontSize: 34, fontWeight: FontWeight.w700, letterSpacing: -0.5),
    displayMedium: GoogleFonts.notoSerif(fontSize: 28, fontWeight: FontWeight.w600),
    headlineLarge: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600),
    headlineMedium: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600),
    headlineSmall: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w500),
    titleLarge: GoogleFonts.manrope(fontSize: 18, fontWeight: FontWeight.w600),
    titleMedium: GoogleFonts.manrope(fontSize: 16, fontWeight: FontWeight.w600),
    titleSmall: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600),
    bodyLarge: GoogleFonts.manrope(fontSize: 16, fontWeight: FontWeight.w400),
    bodyMedium: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w400),
    bodySmall: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w400),
    labelLarge: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600, letterSpacing: 0.5),
    labelMedium: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w500, letterSpacing: 0.8),
    labelSmall: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w500, letterSpacing: 1.0),
  );
}
