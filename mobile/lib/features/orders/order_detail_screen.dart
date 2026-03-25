import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/status_badge.dart';
import '../../core/widgets/work_type_badge.dart';
import '../../core/widgets/gold_divider.dart';
import 'change_status_sheet.dart';

class OrderDetailScreen extends ConsumerStatefulWidget {
  final String orderId;
  const OrderDetailScreen({super.key, required this.orderId});
  @override
  ConsumerState<OrderDetailScreen> createState() => _OrderDetailScreenState();
}

class _OrderDetailScreenState extends ConsumerState<OrderDetailScreen> {
  Map<String, dynamic>? _order;
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _loadOrder();
  }

  Future<void> _loadOrder() async {
    try {
      final data = await ref.read(apiClientProvider).getOrder(widget.orderId);
      setState(() { _order = data; _loading = false; });
    } catch (_) {
      setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Scaffold(body: Center(child: CircularProgressIndicator(color: AppColors.primary)));
    if (_order == null) return const Scaffold(body: Center(child: Text('Commande introuvable')));

    final o = _order!;
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Commande'), leading: const BackButton()),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 0, 20, 100),
        children: [
          // Status + Work type
          Row(children: [
            StatusBadge(status: o['status'], label: o['statusLabel']),
            const SizedBox(width: 8),
            WorkTypeBadge(workType: o['workType']),
          ]),
          const SizedBox(height: 16),

          // Client + Code
          Text(o['clientName'] ?? 'Client', style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
          const SizedBox(height: 4),
          Text(o['code'], style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
          const SizedBox(height: 20),

          // Dates row
          Row(children: [
            _infoChip(Icons.calendar_today_outlined, o['receptionDate'] ?? ''),
            const SizedBox(width: 12),
            _infoChip(Icons.flag_outlined, o['expectedDeliveryDate'] ?? ''),
          ]),
          const SizedBox(height: 20),
          const GoldDivider(),
          const SizedBox(height: 20),

          // Description
          if (o['description'] != null) ...[
            Text(o['description'], style: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurface, height: 1.5)),
            const SizedBox(height: 20),
          ],

          // Pricing
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: AppColors.surfaceContainer, borderRadius: BorderRadius.circular(12)),
            child: Row(children: [
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('PRIX TOTAL', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                Text('${o['totalPrice']} DZD', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w700)),
              ])),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
                Text('SOLDE RESTANT', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                Text('${o['outstandingBalance']} DZD', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w700, color: AppColors.secondary)),
              ])),
            ]),
          ),
          const SizedBox(height: 24),

          // Timeline
          Text('Historique', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
          const SizedBox(height: 12),
          ..._buildTimeline(o['timeline'] ?? []),
        ],
      ),
      bottomSheet: o['status'] != 'Livree'
          ? Container(
              padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
              decoration: BoxDecoration(color: AppColors.surface, boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.06), blurRadius: 20, offset: const Offset(0, -4))]),
              child: SizedBox(
                width: double.infinity, height: 52,
                child: ElevatedButton(
                  onPressed: () async {
                    await showModalBottomSheet(
                      context: context, isScrollControlled: true, backgroundColor: Colors.transparent,
                      builder: (_) => ChangeStatusSheet(order: o, api: ref.read(apiClientProvider)),
                    );
                    _loadOrder();
                  },
                  style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder()),
                  child: Text('Changer le Statut', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
                ),
              ),
            )
          : null,
    );
  }

  Widget _infoChip(IconData icon, String text) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(10)),
      child: Row(mainAxisSize: MainAxisSize.min, children: [
        Icon(icon, size: 14, color: AppColors.onSurfaceVariant),
        const SizedBox(width: 6),
        Text(text, style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurface)),
      ]),
    );
  }

  List<Widget> _buildTimeline(List<dynamic> timeline) {
    return timeline.asMap().entries.map((e) {
      final t = e.value;
      final isLast = e.key == timeline.length - 1;
      final color = AppColors.statusColor(t['toStatus'] ?? '');
      return Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Column(children: [
          Container(width: 12, height: 12, decoration: BoxDecoration(shape: BoxShape.circle, color: color)),
          if (!isLast) Container(width: 2, height: 40, color: AppColors.secondaryFixed.withOpacity(0.4)),
        ]),
        const SizedBox(width: 12),
        Expanded(child: Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(t['toStatusLabel'] ?? '', style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600, color: color)),
            if (t['reason'] != null) Text(t['reason'], style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
            Text(t['transitionedAt']?.toString().substring(0, 16) ?? '', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
          ]),
        )),
      ]);
    }).toList();
  }
}
