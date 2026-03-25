import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/gold_divider.dart';

class ClientProfileScreen extends ConsumerStatefulWidget {
  final String clientId;
  const ClientProfileScreen({super.key, required this.clientId});
  @override
  ConsumerState<ClientProfileScreen> createState() => _ClientProfileScreenState();
}

class _ClientProfileScreenState extends ConsumerState<ClientProfileScreen> {
  Map<String, dynamic>? _client;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    try {
      final data = await ref.read(apiClientProvider).getClient(widget.clientId);
      setState(() { _client = data; _loading = false; });
    } catch (_) { setState(() => _loading = false); }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Scaffold(body: Center(child: CircularProgressIndicator(color: AppColors.primary)));
    if (_client == null) return const Scaffold(body: Center(child: Text('Client introuvable')));
    final c = _client!;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Fiche Client'), leading: const BackButton()),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 0, 20, 80),
        children: [
          // Avatar + Name
          Center(child: Column(children: [
            CircleAvatar(radius: 36, backgroundColor: AppColors.primaryContainer,
              child: Text('${c['firstName']?[0] ?? ''}${c['lastName']?[0] ?? ''}', style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.w600))),
            const SizedBox(height: 12),
            Text('${c['firstName']} ${c['lastName']}', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
            Text(c['code'] ?? '', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
          ])),
          const SizedBox(height: 24),

          // Stats row
          Row(children: [
            _stat('TOTAL COMMANDES', '${c['stats']?['totalOrders'] ?? 0}'),
            const SizedBox(width: 16),
            _stat('VOLUME CAPTURÉ', '${c['stats']?['totalAmountCollected'] ?? 0} DZD'),
          ]),
          const SizedBox(height: 24),
          const GoldDivider(),
          const SizedBox(height: 24),

          // Measurements
          Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            Text('Mesures & Tailles', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
            TextButton(onPressed: () {}, child: Text('MODIFIER', style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary))),
          ]),
          const SizedBox(height: 12),
          _measurementsGrid(c['currentMeasurements'] ?? []),
          const SizedBox(height: 24),
          const GoldDivider(),
          const SizedBox(height: 24),

          // Order history
          Text('Historique Commandes', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
          const SizedBox(height: 12),
          Text('Aucune commande pour l\'instant', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
        ],
      ),
    );
  }

  Widget _stat(String label, String value) {
    return Expanded(child: Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(14)),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text(label, style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
        const SizedBox(height: 6),
        Text(value, style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w700)),
      ]),
    ));
  }

  Widget _measurementsGrid(List<dynamic> measurements) {
    if (measurements.isEmpty) return Text('Aucune mesure enregistrée', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant));
    return Wrap(
      spacing: 10, runSpacing: 10,
      children: measurements.map<Widget>((m) => Container(
        width: (MediaQuery.of(context).size.width - 50) / 2,
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(10)),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text('${m['value']}${m['unit']}', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w700)),
          Text(m['fieldName'] ?? '', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
        ]),
      )).toList(),
    );
  }
}
