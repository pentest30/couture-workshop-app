import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:url_launcher/url_launcher.dart';
import '../../core/theme/app_colors.dart';
import 'package:flutter/services.dart';
import '../../core/api/api_client.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/status_badge.dart';
import '../../core/widgets/work_type_badge.dart';
import '../../core/widgets/gold_divider.dart';
import 'change_status_sheet.dart';
import 'record_payment_sheet.dart';

class OrderDetailScreen extends ConsumerStatefulWidget {
  final String orderId;
  const OrderDetailScreen({super.key, required this.orderId});
  @override
  ConsumerState<OrderDetailScreen> createState() => _OrderDetailScreenState();
}

class _OrderDetailScreenState extends ConsumerState<OrderDetailScreen> {
  Map<String, dynamic>? _order;
  List<dynamic> _payments = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final api = ref.read(apiClientProvider);
      final results = await Future.wait([
        api.getOrder(widget.orderId),
        api.getPayments(widget.orderId),
      ]);
      setState(() {
        _order = results[0] as Map<String, dynamic>;
        _payments = results[1] as List<dynamic>;
        _loading = false;
      });
    } catch (e) {
      setState(() {
        _loading = false;
        _error = e.toString();
      });
    }
  }

  double get _outstandingBalance {
    if (_order == null) return 0;
    final total = (_order!['totalPrice'] as num?)?.toDouble() ?? 0;
    final paid = _payments.fold<double>(0, (sum, p) => sum + (((p as Map)['amount'] as num?)?.toDouble() ?? 0));
    return total - paid;
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Scaffold(body: Center(child: CircularProgressIndicator(color: AppColors.primary)));
    if (_error != null || _order == null) {
      return Scaffold(
        backgroundColor: AppColors.background,
        appBar: AppBar(title: const Text('Commande'), leading: const BackButton()),
        body: Center(
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            Text(_order == null ? 'Commande introuvable' : 'Erreur de chargement', style: GoogleFonts.manrope(color: AppColors.error)),
            const SizedBox(height: 8),
            TextButton(onPressed: _loadData, child: const Text('Réessayer')),
          ]),
        ),
      );
    }

    final o = _order!;
    final clientName = o['clientName'] ?? _shortId(o['clientId']) ?? 'Client';
    final balance = _outstandingBalance;
    final photos = o['photos'] as List<dynamic>? ?? [];

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(title: const Text('Commande'), leading: const BackButton()),
      body: RefreshIndicator(
        onRefresh: _loadData,
        child: ListView(
          padding: const EdgeInsets.fromLTRB(20, 0, 20, 100),
          children: [
            // Status + Work type
            Row(children: [
              StatusBadge(status: o['status'] ?? '', label: o['statusLabel'] ?? o['status'] ?? ''),
              const SizedBox(width: 8),
              WorkTypeBadge(workType: o['workType'] ?? 'Simple'),
            ]),
            const SizedBox(height: 16),

            // Client + Code
            Text(clientName, style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
            const SizedBox(height: 4),
            Text(o['code'] ?? '', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
            const SizedBox(height: 20),

            // Late alert
            if (o['isLate'] == true) ...[
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(color: AppColors.error.withAlpha(20), borderRadius: BorderRadius.circular(10)),
                child: Row(children: [
                  const Icon(Icons.warning_amber_rounded, color: AppColors.error, size: 18),
                  const SizedBox(width: 8),
                  Text('En retard de ${o['delayDays'] ?? 0} jour(s)', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: AppColors.error)),
                ]),
              ),
              const SizedBox(height: 16),
            ],

            // Dates section
            _sectionTitle('Dates'),
            const SizedBox(height: 8),
            _detailRow('Reception', _formatDate(o['receptionDate'])),
            _detailRow('Livraison prevue', _formatDate(o['expectedDeliveryDate'])),
            if (o['actualDeliveryDate'] != null)
              _detailRow('Livraison reelle', _formatDate(o['actualDeliveryDate'])),
            const SizedBox(height: 16),

            // Artisan info
            if (o['assignedTailorId'] != null || o['assignedEmbroidererId'] != null || o['assignedBeaderId'] != null) ...[
              _sectionTitle('Artisans assignes'),
              const SizedBox(height: 8),
              if (o['assignedTailorId'] != null)
                _detailRow('Couturier(e)', _shortId(o['assignedTailorId']) ?? ''),
              if (o['assignedEmbroidererId'] != null)
                _detailRow('Brodeur(se)', _shortId(o['assignedEmbroidererId']) ?? ''),
              if (o['assignedBeaderId'] != null)
                _detailRow('Perleur(se)', _shortId(o['assignedBeaderId']) ?? ''),
              const SizedBox(height: 16),
            ],

            const GoldDivider(),
            const SizedBox(height: 16),

            // Description & fabric
            _sectionTitle('Details de la commande'),
            const SizedBox(height: 8),
            _detailRow('Type de travail', o['workTypeLabel'] ?? o['workType'] ?? ''),
            if (o['description'] != null && (o['description'] as String).isNotEmpty)
              _detailRow('Description', o['description']),
            if (o['fabric'] != null && (o['fabric'] as String).isNotEmpty)
              _detailRow('Tissu', o['fabric']),
            if (o['technicalNotes'] != null && (o['technicalNotes'] as String).isNotEmpty)
              _detailRow('Notes techniques', o['technicalNotes']),
            const SizedBox(height: 12),

            // Embroidery details
            if (_hasEmbroideryDetails(o)) ...[
              _sectionTitle('Details broderie'),
              const SizedBox(height: 8),
              if (o['embroideryStyle'] != null)
                _detailRow('Style', o['embroideryStyle']),
              if (o['threadColors'] != null)
                _detailRow('Couleurs fils', o['threadColors']),
              if (o['density'] != null)
                _detailRow('Densite', o['density']),
              if (o['embroideryZone'] != null)
                _detailRow('Zone', o['embroideryZone']),
              const SizedBox(height: 12),
            ],

            // Beading details
            if (_hasBeadingDetails(o)) ...[
              _sectionTitle('Details perlage'),
              const SizedBox(height: 8),
              if (o['beadType'] != null)
                _detailRow('Type de perle', o['beadType']),
              if (o['arrangement'] != null)
                _detailRow('Arrangement', o['arrangement']),
              if (o['affectedZones'] != null)
                _detailRow('Zones concernees', o['affectedZones']),
              const SizedBox(height: 12),
            ],

            // Photos
            if (photos.isNotEmpty) ...[
              Row(children: [
                const Icon(Icons.photo_library_outlined, size: 16, color: AppColors.onSurfaceVariant),
                const SizedBox(width: 6),
                Text('${photos.length} photo${photos.length > 1 ? 's' : ''}', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
              ]),
              const SizedBox(height: 16),
            ],

            // Pricing
            Container(
              padding: const EdgeInsets.all(16),
              decoration: BoxDecoration(color: AppColors.surfaceContainer, borderRadius: BorderRadius.circular(12)),
              child: Row(children: [
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text('PRIX TOTAL', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                  Text('${o['totalPrice'] ?? 0} DZD', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w700)),
                ])),
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
                  Text('SOLDE RESTANT', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                  Text('${balance.toStringAsFixed(0)} DZD', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w700, color: balance > 0 ? AppColors.secondary : AppColors.statusPrete)),
                ])),
              ]),
            ),

            // Payments section (F08)
            if (balance > 0) ...[
              const SizedBox(height: 16),
              SizedBox(
                width: double.infinity, height: 44,
                child: ElevatedButton.icon(
                  onPressed: () async {
                    final result = await showModalBottomSheet(
                      context: context, isScrollControlled: true, backgroundColor: Colors.transparent,
                      builder: (_) => RecordPaymentSheet(
                        orderId: widget.orderId,
                        orderCode: o['code'] ?? '',
                        outstandingBalance: balance,
                        api: ref.read(apiClientProvider),
                      ),
                    );
                    if (result != null) _loadData();
                  },
                  icon: const Icon(Icons.payments_outlined, size: 18),
                  label: Text('ENCAISSER', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
                  style: ElevatedButton.styleFrom(backgroundColor: AppColors.secondary, foregroundColor: Colors.white, shape: const StadiumBorder()),
                ),
              ),
            ],

            // Payment history
            if (_payments.isNotEmpty) ...[
              const SizedBox(height: 20),
              Text('Paiements', style: GoogleFonts.notoSerif(fontSize: 16, fontWeight: FontWeight.w600)),
              const SizedBox(height: 8),
              ..._payments.map((p) {
                final pm = p as Map;
                final receiptCode = pm['receiptCode']?.toString();
                final receiptId = pm['id']?.toString();
                return Container(
                  margin: const EdgeInsets.only(bottom: 10),
                  padding: const EdgeInsets.all(14),
                  decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(12)),
                  child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Row(children: [
                      const Icon(Icons.payments_outlined, color: AppColors.statusPrete, size: 18),
                      const SizedBox(width: 8),
                      Text('${(pm['amount'] as num?)?.toStringAsFixed(0) ?? '0'} DZD', style: GoogleFonts.notoSerif(fontSize: 16, fontWeight: FontWeight.w700)),
                      const SizedBox(width: 8),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                        decoration: BoxDecoration(color: AppColors.primary.withAlpha(15), borderRadius: BorderRadius.circular(8)),
                        child: Text(pm['paymentMethodLabel'] ?? pm['paymentMethod'] ?? '', style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w600, color: AppColors.primary)),
                      ),
                      const Spacer(),
                      Text(pm['paymentDate'] ?? '', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
                    ]),
                    if (pm['note'] != null && pm['note'].toString().isNotEmpty) ...[
                      const SizedBox(height: 4),
                      Text(pm['note'].toString(), style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
                    ],
                    if (receiptCode != null) ...[
                      const SizedBox(height: 8),
                      SizedBox(
                        width: double.infinity, height: 36,
                        child: OutlinedButton.icon(
                          onPressed: () async {
                            final url = '${ApiClient.baseUrl}/api/finance/receipts/$receiptId/pdf';
                            try {
                              await launchUrl(Uri.parse(url), mode: LaunchMode.externalApplication);
                            } catch (_) {
                              if (context.mounted) {
                                await Clipboard.setData(ClipboardData(text: url));
                                ScaffoldMessenger.of(context).showSnackBar(
                                  SnackBar(content: Text('Lien copié: $url')),
                                );
                              }
                            }
                          },
                          icon: const Icon(Icons.download_outlined, size: 16),
                          label: Text('TÉLÉCHARGER REÇU $receiptCode', style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700)),
                          style: OutlinedButton.styleFrom(
                            foregroundColor: AppColors.secondary,
                            side: const BorderSide(color: AppColors.secondaryFixed),
                            shape: const StadiumBorder(),
                            padding: const EdgeInsets.symmetric(horizontal: 12),
                          ),
                        ),
                      ),
                    ],
                  ]),
                );
              }),
            ],
            const SizedBox(height: 20),

            // Timeline
            Text('Historique', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
            const SizedBox(height: 12),
            ..._buildTimeline(o['timeline'] ?? []),
          ],
        ),
      ),
      bottomSheet: o['status'] != 'Livree'
          ? Container(
              padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
              decoration: BoxDecoration(color: AppColors.surface, boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.06), blurRadius: 20, offset: const Offset(0, -4))]),
              child: SizedBox(
                width: double.infinity, height: 52,
                child: ElevatedButton(
                  onPressed: () async {
                    final changed = await showModalBottomSheet<bool>(
                      context: context, isScrollControlled: true, backgroundColor: Colors.transparent,
                      builder: (_) => ChangeStatusSheet(order: o, api: ref.read(apiClientProvider)),
                    );
                    if (changed == true) {
                      _loadData();
                    }
                  },
                  style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder()),
                  child: Text('Changer le Statut', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
                ),
              ),
            )
          : null,
    );
  }

  bool _hasEmbroideryDetails(Map<String, dynamic> o) {
    return o['embroideryStyle'] != null || o['threadColors'] != null
        || o['density'] != null || o['embroideryZone'] != null;
  }

  bool _hasBeadingDetails(Map<String, dynamic> o) {
    return o['beadType'] != null || o['arrangement'] != null || o['affectedZones'] != null;
  }

  Widget _sectionTitle(String title) {
    return Text(title.toUpperCase(), style: GoogleFonts.manrope(fontSize: 11, letterSpacing: 1.2, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w700));
  }

  Widget _detailRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 6),
      child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
        SizedBox(
          width: 120,
          child: Text(label, style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
        ),
        Expanded(child: Text(value, style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurface))),
      ]),
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
    if (timeline.isEmpty) {
      return [Text('Aucun historique', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant))];
    }
    return timeline.asMap().entries.map((e) {
      final t = e.value as Map<String, dynamic>;
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
            Text(t['toStatusLabel'] ?? t['toStatus'] ?? '', style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600, color: color)),
            if (t['reason'] != null && (t['reason'] as String).isNotEmpty)
              Text(t['reason'], style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
            Text(_formatDateTime(t['transitionedAt']), style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
          ]),
        )),
      ]);
    }).toList();
  }

  String _formatDate(dynamic date) {
    if (date == null) return '';
    final str = date.toString();
    if (str.length >= 10) return str.substring(0, 10);
    return str;
  }

  String _formatDateTime(dynamic date) {
    if (date == null) return '';
    final str = date.toString();
    if (str.length >= 16) return str.substring(0, 16).replaceFirst('T', ' ');
    return str;
  }

  String? _shortId(dynamic id) {
    if (id == null) return null;
    final str = id.toString();
    if (str.length >= 8) return str.substring(0, 8);
    return str;
  }
}
