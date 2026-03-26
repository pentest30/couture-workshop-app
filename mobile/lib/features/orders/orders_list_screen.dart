import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/status_badge.dart';
import '../../core/widgets/work_type_badge.dart';

class OrdersListScreen extends ConsumerStatefulWidget {
  const OrdersListScreen({super.key});
  @override
  ConsumerState<OrdersListScreen> createState() => _OrdersListScreenState();
}

class _OrdersListScreenState extends ConsumerState<OrdersListScreen> {
  List<dynamic> _orders = [];
  bool _loading = true;
  String? _error;

  // Quarter filter
  late int _selectedYear;
  late int _selectedQuarter; // 1-4
  String? _selectedStatus; // null = all

  static const _allStatuses = [
    ('Recue', 'Recue'),
    ('EnAttente', 'En Attente'),
    ('EnCours', 'En Cours'),
    ('Broderie', 'Broderie'),
    ('Perlage', 'Perlage'),
    ('Retouche', 'Retouche'),
    ('Prete', 'Prete'),
    ('Livree', 'Livree'),
  ];

  @override
  void initState() {
    super.initState();
    final now = DateTime.now();
    _selectedYear = now.year;
    _selectedQuarter = ((now.month - 1) ~/ 3) + 1;
    _loadOrders();
  }

  static const _quarterStarts = {1: '01-01', 2: '04-01', 3: '07-01', 4: '10-01'};
  static const _quarterEnds = {1: '03-31', 2: '06-30', 3: '09-30', 4: '12-31'};

  (String, String) get _quarterDates {
    return ('$_selectedYear-${_quarterStarts[_selectedQuarter]}', '$_selectedYear-${_quarterEnds[_selectedQuarter]}');
  }

  List<String> get _quarterOptions {
    final now = DateTime.now();
    final options = <String>[];
    for (var y = now.year; y >= now.year - 2; y--) {
      for (var q = 1; q <= 4; q++) {
        options.add('T$q $y');
      }
    }
    return options;
  }

  String get _currentQuarterLabel => 'T$_selectedQuarter $_selectedYear';

  void _onQuarterChanged(String? value) {
    if (value == null) return;
    final parts = value.split(' ');
    setState(() {
      _selectedQuarter = int.parse(parts[0].substring(1));
      _selectedYear = int.parse(parts[1]);
    });
    _loadOrders();
  }

  Future<void> _loadOrders() async {
    setState(() { _loading = true; _error = null; });
    try {
      final api = ref.read(apiClientProvider);
      final dates = _quarterDates;
      final data = await api.getOrders(
        status: _selectedStatus,
        dateFrom: dates.$1,
        dateTo: dates.$2,
      );
      setState(() {
        _orders = data['items'] ?? [];
        _loading = false;
      });
    } catch (e) {
      setState(() { _loading = false; _error = e.toString(); });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('ARCHIVES DE L\'ATELIER', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
              const SizedBox(height: 4),
              Text('Commandes', style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
              const SizedBox(height: 12),

              // Quarter + Status filters row
              Row(children: [
                // Quarter dropdown
                Expanded(
                  child: Container(
                    height: 40,
                    padding: const EdgeInsets.symmetric(horizontal: 12),
                    decoration: BoxDecoration(
                      color: AppColors.surface,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppColors.outlineVariant),
                    ),
                    child: DropdownButtonHideUnderline(
                      child: DropdownButton<String>(
                        value: _currentQuarterLabel,
                        isExpanded: true,
                        icon: const Icon(Icons.calendar_month, size: 18, color: AppColors.primary),
                        style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: AppColors.onSurface),
                        items: _quarterOptions.map((s) => DropdownMenuItem(value: s, child: Text(s))).toList(),
                        onChanged: _onQuarterChanged,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 10),
                // Status dropdown
                Expanded(
                  child: Container(
                    height: 40,
                    padding: const EdgeInsets.symmetric(horizontal: 12),
                    decoration: BoxDecoration(
                      color: AppColors.surface,
                      borderRadius: BorderRadius.circular(12),
                      border: Border.all(color: AppColors.outlineVariant),
                    ),
                    child: DropdownButtonHideUnderline(
                      child: DropdownButton<String>(
                        value: _selectedStatus,
                        isExpanded: true,
                        hint: Text('Tous les statuts', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
                        icon: const Icon(Icons.filter_list, size: 18, color: AppColors.primary),
                        style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: AppColors.onSurface),
                        items: [
                          DropdownMenuItem<String>(value: null, child: Text('Tous les statuts', style: GoogleFonts.manrope(fontSize: 13))),
                          ..._allStatuses.map((s) => DropdownMenuItem(
                            value: s.$1,
                            child: Row(children: [
                              Container(width: 8, height: 8, decoration: BoxDecoration(color: AppColors.statusColor(s.$1), shape: BoxShape.circle)),
                              const SizedBox(width: 8),
                              Text(s.$2, style: GoogleFonts.manrope(fontSize: 13)),
                            ]),
                          )),
                        ],
                        onChanged: (v) {
                          setState(() => _selectedStatus = v);
                          _loadOrders();
                        },
                      ),
                    ),
                  ),
                ),
              ]),
              const SizedBox(height: 16),
            ]),
          ),

          // Orders list
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
                : _error != null
                    ? Center(
                        child: Column(mainAxisSize: MainAxisSize.min, children: [
                          Text('Erreur de chargement', style: GoogleFonts.manrope(color: AppColors.error, fontWeight: FontWeight.w600)),
                          const SizedBox(height: 8),
                          TextButton(onPressed: _loadOrders, child: const Text('Reessayer')),
                        ]),
                      )
                    : _orders.isEmpty
                        ? Center(
                            child: Column(mainAxisSize: MainAxisSize.min, children: [
                              Icon(Icons.inbox_outlined, size: 48, color: AppColors.onSurfaceVariant.withValues(alpha: 0.4)),
                              const SizedBox(height: 12),
                              Text('Aucune commande', style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant)),
                            ]),
                          )
                        : RefreshIndicator(
                            onRefresh: _loadOrders,
                            child: ListView.separated(
                              padding: const EdgeInsets.fromLTRB(20, 0, 20, 80),
                              itemCount: _orders.length,
                              separatorBuilder: (_, __) => const SizedBox(height: 10),
                              itemBuilder: (_, i) => _orderCard(_orders[i]),
                            ),
                          ),
          ),
        ]),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => context.go('/new-order'),
        child: const Icon(Icons.add, size: 28),
      ),
    );
  }

  Widget _orderCard(Map<String, dynamic> order) {
    final isLate = order['isLate'] == true;
    final clientName = order['clientName'] ?? _shortId(order['clientId']) ?? 'Client';
    return GestureDetector(
      onTap: () => context.push('/orders/${order['id']}'),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          border: isLate ? Border.all(color: AppColors.error.withValues(alpha: 0.3), width: 1.5) : null,
          boxShadow: [BoxShadow(color: AppColors.primary.withValues(alpha: 0.03), blurRadius: 12, offset: const Offset(0, 4))],
        ),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(children: [
            Text(order['code'] ?? '', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w700, color: AppColors.onSurfaceVariant)),
            const Spacer(),
            WorkTypeBadge(workType: order['workType'] ?? 'Simple'),
          ]),
          const SizedBox(height: 8),
          Text(clientName, style: GoogleFonts.notoSerif(fontSize: 16, fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          Row(children: [
            StatusBadge(status: order['status'] ?? '', label: order['statusLabel'] ?? order['status'] ?? ''),
            const Spacer(),
            if (isLate) ...[
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                decoration: BoxDecoration(color: AppColors.error, borderRadius: BorderRadius.circular(12)),
                child: Text('RETARD: ${order['delayDays']}J', style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w700, color: Colors.white)),
              ),
            ] else ...[
              const Icon(Icons.calendar_today_outlined, size: 14, color: AppColors.onSurfaceVariant),
              const SizedBox(width: 4),
              Text(_formatDate(order['expectedDeliveryDate']), style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
            ],
          ]),
        ]),
      ),
    );
  }

  String _formatDate(dynamic date) {
    if (date == null) return '';
    final str = date.toString();
    if (str.length >= 10) return str.substring(0, 10);
    return str;
  }

  String? _shortId(dynamic id) {
    if (id == null) return null;
    final str = id.toString();
    if (str.length >= 8) return 'Client #${str.substring(0, 8)}';
    return 'Client #$str';
  }
}
