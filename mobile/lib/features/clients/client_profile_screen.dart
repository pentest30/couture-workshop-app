import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/gold_divider.dart';
import '../../core/widgets/status_badge.dart';
import 'measurements_screen.dart';

class ClientProfileScreen extends ConsumerStatefulWidget {
  final String clientId;
  const ClientProfileScreen({super.key, required this.clientId});
  @override
  ConsumerState<ClientProfileScreen> createState() => _ClientProfileScreenState();
}

class _ClientProfileScreenState extends ConsumerState<ClientProfileScreen> {
  Map<String, dynamic>? _client;
  List<dynamic> _orders = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final data = await ref.read(apiClientProvider).getClient(widget.clientId);
      if (mounted) setState(() { _client = data; _loading = false; });

      // Load client orders
      try {
        final ordersData = await ref.read(apiClientProvider).getOrders(clientId: widget.clientId);
        if (mounted) setState(() { _orders = (ordersData['items'] as List?) ?? []; });
      } catch (_) {}
    } catch (e) {
      if (mounted) setState(() { _loading = false; _error = 'Impossible de charger le client: $e'; });
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(title: const Text('Fiche Client'), leading: const BackButton()),
        body: const Center(child: CircularProgressIndicator(color: AppColors.primary)),
      );
    }
    if (_error != null || _client == null) {
      return Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(title: const Text('Fiche Client'), leading: const BackButton()),
        body: Center(child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            const Icon(Icons.error_outline, size: 48, color: AppColors.onSurfaceVariant),
            const SizedBox(height: 16),
            Text(_error ?? 'Client introuvable', style: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurfaceVariant), textAlign: TextAlign.center),
            const SizedBox(height: 16),
            OutlinedButton(onPressed: _load, child: const Text('Reessayer')),
          ]),
        )),
      );
    }
    final c = _client!;
    final measurements = (c['currentMeasurements'] as List<dynamic>?) ?? [];
    final totalOrders = c['stats']?['totalOrders'] ?? 0;
    final totalAmount = c['stats']?['totalAmountCollected'] ?? 0;

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: const Text('Fiche Client'),
        leading: const BackButton(),
        backgroundColor: AppColors.background,
        surfaceTintColor: Colors.transparent,
      ),
      body: RefreshIndicator(
        onRefresh: _load,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 0, 20, 100),
          children: [
            // Avatar + Name
            Center(child: Column(children: [
              CircleAvatar(
                radius: 36, backgroundColor: AppColors.primaryContainer,
                child: Text(
                  '${(c['firstName'] as String?)?.isNotEmpty == true ? c['firstName'][0] : ''}${(c['lastName'] as String?)?.isNotEmpty == true ? c['lastName'][0] : ''}',
                  style: const TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.w600),
                ),
              ),
              const SizedBox(height: 12),
              Text('${c['firstName'] ?? ''} ${c['lastName'] ?? ''}', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
              const SizedBox(height: 4),
              Text(c['code'] ?? '', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
              if (c['primaryPhone'] != null) ...[
                const SizedBox(height: 2),
                Text(c['primaryPhone'], style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
              ],
            ])),
            const SizedBox(height: 24),

            // Stats row
            Row(children: [
              _stat('TOTAL COMMANDES', '$totalOrders'),
              const SizedBox(width: 16),
              _stat('VOLUME CAPTURE', '${_formatAmount(totalAmount)} DZD'),
            ]),
            const SizedBox(height: 24),
            const GoldDivider(),
            const SizedBox(height: 24),

            // Measurements
            Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
              Text('Mesures & Tailles', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
              TextButton(
                onPressed: () async {
                  final result = await Navigator.push(context, MaterialPageRoute(
                    builder: (_) => MeasurementsScreen(
                      clientId: widget.clientId,
                      clientName: '${c['firstName']} ${c['lastName']}',
                      currentMeasurements: c['currentMeasurements'] ?? [],
                    ),
                  ));
                  if (result == true) _load(); // Reload after saving
                },
                child: Text('MODIFIER', style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary)),
              ),
            ]),
            const SizedBox(height: 12),
            _measurementsGrid(measurements),
            const SizedBox(height: 24),
            const GoldDivider(),
            const SizedBox(height: 24),

            // Order history with balance
            Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
              Text('Historique Commandes (${_orders.length})', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
              if (_orders.isNotEmpty)
                TextButton(onPressed: () => context.go('/orders'), child: Text('VOIR TOUT', style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary))),
            ]),
            const SizedBox(height: 12),
            if (_orders.isEmpty)
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(12)),
                child: Column(children: [
                  Icon(Icons.receipt_long_outlined, size: 36, color: AppColors.onSurfaceVariant.withAlpha(80)),
                  const SizedBox(height: 8),
                  Text('Aucune commande', style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant)),
                ]),
              )
            else
              ..._orders.take(10).map((o) {
                final isLate = o['isLate'] == true;
                final balance = ((o['outstandingBalance'] as num?) ?? 0).toDouble();
                return GestureDetector(
                  onTap: () => context.go('/orders/${o['id']}'),
                  child: Container(
                    margin: const EdgeInsets.only(bottom: 8),
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: AppColors.surface,
                      borderRadius: BorderRadius.circular(12),
                      border: isLate ? Border.all(color: AppColors.error.withAlpha(40)) : null,
                    ),
                    child: Row(children: [
                      Container(
                        width: 4, height: 36,
                        decoration: BoxDecoration(
                          color: AppColors.statusColor(o['status'] ?? ''),
                          borderRadius: BorderRadius.circular(2),
                        ),
                      ),
                      const SizedBox(width: 10),
                      Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                        Row(children: [
                          Text(o['code'] ?? '', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600)),
                          const SizedBox(width: 8),
                          StatusBadge(status: o['status'] ?? '', label: o['statusLabel'] ?? ''),
                        ]),
                        const SizedBox(height: 2),
                        Text('${((o['totalPrice'] as num?) ?? 0).toStringAsFixed(0)} DZD', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
                      ])),
                      Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
                        if (balance > 0)
                          Text('${balance.toStringAsFixed(0)} DZD', style: GoogleFonts.notoSerif(fontSize: 14, fontWeight: FontWeight.w700, color: AppColors.secondary))
                        else
                          Text('Soldé', style: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w600, color: AppColors.statusPrete)),
                        if (isLate)
                          Text('${o['delayDays']}j retard', style: GoogleFonts.manrope(fontSize: 10, color: AppColors.error, fontWeight: FontWeight.w600)),
                      ]),
                    ]),
                  ),
                );
              }),
          ],
        ),
      ),
      bottomNavigationBar: SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 8, 20, 16),
          child: ElevatedButton.icon(
            onPressed: () => context.push('/new-order'),
            icon: const Icon(Icons.add),
            label: Text('NOUVELLE COMMANDE', style: GoogleFonts.manrope(fontWeight: FontWeight.w700)),
            style: ElevatedButton.styleFrom(
              backgroundColor: AppColors.primary,
              foregroundColor: Colors.white,
              shape: const StadiumBorder(),
              padding: const EdgeInsets.symmetric(vertical: 14),
            ),
          ),
        ),
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
    if (measurements.isEmpty) {
      return Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(14)),
        child: Column(children: [
          const Icon(Icons.straighten, size: 32, color: AppColors.onSurfaceVariant),
          const SizedBox(height: 8),
          Text('Aucune mesure enregistree', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
          const SizedBox(height: 4),
          Text('Les mesures seront ajoutees lors de la premiere commande.', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant), textAlign: TextAlign.center),
        ]),
      );
    }
    return Wrap(
      spacing: 10, runSpacing: 10,
      children: measurements.map<Widget>((m) => Container(
        width: (MediaQuery.of(context).size.width - 50) / 2,
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(10)),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Text('${m['value'] ?? '-'}${m['unit'] ?? 'cm'}', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w700)),
          const SizedBox(height: 2),
          Text(m['fieldName'] ?? '', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
        ]),
      )).toList(),
    );
  }

  Widget _orderTile(Map<String, dynamic> o) {
    final status = o['status'] ?? '';
    return GestureDetector(
      onTap: () {
        final orderId = o['id'];
        if (orderId != null) context.push('/orders/$orderId');
      },
      child: Container(
        margin: const EdgeInsets.only(bottom: 8),
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(12)),
        child: Row(children: [
          Container(
            width: 4, height: 36,
            decoration: BoxDecoration(color: AppColors.statusColor(status), borderRadius: BorderRadius.circular(2)),
          ),
          const SizedBox(width: 12),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(o['orderNumber'] ?? '', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600)),
            Text(o['workType'] ?? '', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
          ])),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
            decoration: BoxDecoration(color: AppColors.statusBgColor(status), borderRadius: BorderRadius.circular(8)),
            child: Text(status, style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w600, color: AppColors.statusColor(status))),
          ),
        ]),
      ),
    );
  }

  String _formatAmount(dynamic amount) {
    if (amount == null) return '0';
    final num n = amount is num ? amount : num.tryParse(amount.toString()) ?? 0;
    if (n >= 1000) {
      return '${(n / 1000).toStringAsFixed(n % 1000 == 0 ? 0 : 1)}k';
    }
    return n.toStringAsFixed(0);
  }
}
