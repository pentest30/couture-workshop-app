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
  String _selectedFilter = 'Tous';
  final _filters = ['Tous', 'En cours', 'En retard', 'Brodé'];

  @override
  void initState() {
    super.initState();
    _loadOrders();
  }

  Future<void> _loadOrders() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final api = ref.read(apiClientProvider);
      String? status;
      bool? lateOnly;
      String? workType;

      switch (_selectedFilter) {
        case 'En cours':
          status = 'EnCours';
          break;
        case 'En retard':
          lateOnly = true;
          break;
        case 'Brodé':
          workType = 'Brode';
          break;
      }

      final data = await api.getOrders(
        status: status,
        lateOnly: lateOnly,
        workType: workType,
      );
      setState(() {
        _orders = data['items'] ?? [];
        _loading = false;
      });
    } catch (e) {
      setState(() {
        _loading = false;
        _error = e.toString();
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final now = DateTime.now();
    final quarter = ((now.month - 1) ~/ 3) + 1;

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(children: [
          // Header
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('ARCHIVES DE L\'ATELIER', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
              const SizedBox(height: 4),
              Text('Commandes — T$quarter ${now.year}', style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
              const SizedBox(height: 16),

              // Filter chips
              SizedBox(
                height: 36,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  itemCount: _filters.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 8),
                  itemBuilder: (_, i) {
                    final selected = _filters[i] == _selectedFilter;
                    return ChoiceChip(
                      label: Text(_filters[i]),
                      selected: selected,
                      onSelected: (_) {
                        if (_selectedFilter != _filters[i]) {
                          setState(() => _selectedFilter = _filters[i]);
                          _loadOrders();
                        }
                      },
                      labelStyle: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w600, color: selected ? Colors.white : AppColors.onSurface),
                    );
                  },
                ),
              ),
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
                          TextButton(onPressed: _loadOrders, child: const Text('Réessayer')),
                        ]),
                      )
                    : _orders.isEmpty
                        ? Center(
                            child: Column(mainAxisSize: MainAxisSize.min, children: [
                              Icon(Icons.inbox_outlined, size: 48, color: AppColors.onSurfaceVariant.withOpacity(0.4)),
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
      onTap: () => context.go('/orders/${order['id']}'),
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          border: isLate ? Border.all(color: AppColors.error.withOpacity(0.3), width: 1.5) : null,
          boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.03), blurRadius: 12, offset: const Offset(0, 4))],
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
              Icon(Icons.calendar_today_outlined, size: 14, color: AppColors.onSurfaceVariant),
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
