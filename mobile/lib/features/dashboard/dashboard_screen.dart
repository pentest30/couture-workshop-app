import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/gold_divider.dart';

class DashboardScreen extends ConsumerStatefulWidget {
  const DashboardScreen({super.key});
  @override
  ConsumerState<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends ConsumerState<DashboardScreen> {
  Map<String, dynamic>? _kpis;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadKPIs();
  }

  Future<void> _loadKPIs() async {
    try {
      final api = ref.read(apiClientProvider);
      final now = DateTime.now();
      final quarter = ((now.month - 1) ~/ 3) + 1;
      final data = await api.getKPIs(now.year, quarter);
      setState(() { _kpis = data; _loading = false; });
    } catch (_) {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    final auth = ref.watch(authStateProvider);
    final name = auth?['fullName'] ?? 'Utilisateur';

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: _loading
            ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
            : RefreshIndicator(
                onRefresh: _loadKPIs,
                color: AppColors.primary,
                child: ListView(
                  padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
                  children: [
                    // Header
                    Row(
                      children: [
                        Expanded(
                          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                            Text("L'Atelier Couture", style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant, letterSpacing: 1)),
                            const SizedBox(height: 4),
                            Text('Bonjour, ${name.split(' ').first}', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
                          ]),
                        ),
                        CircleAvatar(backgroundColor: AppColors.primaryContainer, radius: 22, child: Text(name[0], style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600))),
                      ],
                    ),
                    const SizedBox(height: 24),

                    // Alert banner (late orders)
                    if ((_kpis?['lateOrders'] ?? 0) > 0) ...[
                      Container(
                        padding: const EdgeInsets.all(14),
                        decoration: BoxDecoration(
                          color: AppColors.error.withOpacity(0.08),
                          borderRadius: BorderRadius.circular(12),
                        ),
                        child: Row(children: [
                          const Icon(Icons.warning_amber_rounded, color: AppColors.error, size: 20),
                          const SizedBox(width: 10),
                          Text('${_kpis!['lateOrders']} commande(s) dépassent le délai', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: AppColors.error)),
                        ]),
                      ),
                      const SizedBox(height: 20),
                    ],

                    // KPI Grid
                    _buildKPIGrid(),
                    const SizedBox(height: 28),

                    // Section: Activity
                    Text('Activité Trimestrielle', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
                    const SizedBox(height: 12),
                    _buildActivityChart(),
                    const SizedBox(height: 28),

                    // Recent orders
                    Row(
                      mainAxisAlignment: MainAxisAlignment.spaceBetween,
                      children: [
                        Text('Commandes Récentes', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
                        TextButton(onPressed: () {}, child: Text('TOUT VOIR', style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary))),
                      ],
                    ),
                  ],
                ),
              ),
      ),
    );
  }

  Widget _buildKPIGrid() {
    return Row(
      children: [
        Expanded(child: _kpiCard('Commandes', '${_kpis?['totalOrders'] ?? 0}', AppColors.primary, Icons.receipt_long)),
        const SizedBox(width: 12),
        Expanded(child: _kpiCard('Livrées', '${_kpis?['deliveredOrders'] ?? 0}', AppColors.statusPrete, Icons.check_circle_outline)),
        const SizedBox(width: 12),
        Expanded(child: _kpiCard('En retard', '${_kpis?['lateOrders'] ?? 0}', AppColors.error, Icons.access_time)),
      ],
    );
  }

  Widget _kpiCard(String label, String value, Color color, IconData icon) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.04), blurRadius: 16, offset: const Offset(0, 4))],
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Icon(icon, color: color, size: 20),
        const SizedBox(height: 10),
        Text(value, style: GoogleFonts.notoSerif(fontSize: 28, fontWeight: FontWeight.w700, color: color)),
        const SizedBox(height: 2),
        Text(label, style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w500)),
      ]),
    );
  }

  Widget _buildActivityChart() {
    // Simplified bar chart placeholder matching design
    final revenue = _kpis?['revenueCollected'] ?? 0;
    final formatted = '${(revenue / 1000).toStringAsFixed(0)} 000';
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(16)),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Row(children: [
          const Icon(Icons.monetization_on_outlined, color: AppColors.secondary, size: 18),
          const SizedBox(width: 8),
          Text('$formatted DZD', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w700, color: AppColors.onSurface)),
        ]),
        const SizedBox(height: 4),
        Text('CA encaissé ce trimestre', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
        const SizedBox(height: 16),
        // Simple bars placeholder
        Row(
          crossAxisAlignment: CrossAxisAlignment.end,
          children: [
            _bar(0.3, AppColors.primary), const SizedBox(width: 6),
            _bar(0.5, AppColors.statusBroderie), const SizedBox(width: 6),
            _bar(0.8, AppColors.secondary), const SizedBox(width: 6),
            _bar(0.4, AppColors.statusPerlage),
          ],
        ),
      ]),
    );
  }

  Widget _bar(double ratio, Color color) {
    return Expanded(
      child: Container(
        height: 60 * ratio,
        decoration: BoxDecoration(color: color.withOpacity(0.8), borderRadius: BorderRadius.circular(4)),
      ),
    );
  }
}
