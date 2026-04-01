import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/status_badge.dart';

class DashboardScreen extends ConsumerStatefulWidget {
  const DashboardScreen({super.key});
  @override
  ConsumerState<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends ConsumerState<DashboardScreen> {
  Map<String, dynamic>? _kpis;
  List<dynamic> _recentOrders = [];
  bool _loading = true;
  String? _error;

  late int _selectedYear;
  late int _selectedQuarter; // 1-4

  @override
  void initState() {
    super.initState();
    final now = DateTime.now();
    _selectedYear = now.year;
    _selectedQuarter = ((now.month - 1) ~/ 3) + 1;
    _loadData();
  }

  String get _currentQuarterLabel => 'T$_selectedQuarter $_selectedYear';

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

  void _onQuarterChanged(String? value) {
    if (value == null) return;
    final parts = value.split(' ');
    setState(() {
      _selectedQuarter = int.parse(parts[0].substring(1));
      _selectedYear = int.parse(parts[1]);
    });
    _loadData();
  }

  static const _quarterStarts = {1: '01-01', 2: '04-01', 3: '07-01', 4: '10-01'};
  static const _quarterEnds = {1: '03-31', 2: '06-30', 3: '09-30', 4: '12-31'};

  Future<void> _loadData() async {
    setState(() { _loading = true; _error = null; });
    try {
      final api = ref.read(apiClientProvider);
      final dateFrom = '$_selectedYear-${_quarterStarts[_selectedQuarter]}';
      final dateTo = '$_selectedYear-${_quarterEnds[_selectedQuarter]}';

      Map<String, dynamic>? kpiData;
      try {
        kpiData = await api.getKPIs(_selectedYear, _selectedQuarter).timeout(const Duration(seconds: 10));
      } catch (_) { kpiData = null; }

      // Load recent orders
      List<dynamic> recentItems = [];
      try {
        final ordersData = await api.getOrders(page: 1, dateFrom: dateFrom, dateTo: dateTo).timeout(const Duration(seconds: 10));
        recentItems = (ordersData['items'] as List? ?? []).take(5).toList();
      } catch (_) {}

      if (!mounted) return;
      setState(() {
        _kpis = kpiData;
        _recentOrders = recentItems;
        _loading = false;
      });
    } catch (e) {
      setState(() { _error = e.toString(); _loading = false; });
    }
  }

  double _avgRate(dynamic a, dynamic b) {
    final va = (a ?? 0 as num).toDouble();
    final vb = (b ?? 0 as num).toDouble();
    if (va == 0 && vb == 0) return 0;
    return (va + vb) / 2;
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
            : _error != null
                ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                    Text('Erreur de chargement', style: GoogleFonts.manrope(fontSize: 16, color: AppColors.error)),
                    const SizedBox(height: 8),
                    Text(_error!, style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant), textAlign: TextAlign.center),
                    const SizedBox(height: 16),
                    ElevatedButton(onPressed: _loadData, child: const Text('Reessayer')),
                  ]))
                : RefreshIndicator(
                    onRefresh: _loadData,
                    color: AppColors.primary,
                    child: ListView(
                      padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
                      children: [
                        _buildHeader(name),
                        const SizedBox(height: 16),
                        _buildQuarterSelector(),
                        const SizedBox(height: 16),
                        if ((_kpis?['lateOrders'] ?? 0) > 0) ...[_buildLateAlert(), const SizedBox(height: 16)],
                        _buildKPIGrid(),
                        const SizedBox(height: 16),
                        _buildRevenueCard(),
                        const SizedBox(height: 24),
                        _buildRecentOrdersHeader(),
                        const SizedBox(height: 12),
                        ..._recentOrders.map(_buildRecentOrderCard),
                        if (_recentOrders.isEmpty)
                          Padding(
                            padding: const EdgeInsets.symmetric(vertical: 20),
                            child: Center(child: Text('Aucune commande', style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant))),
                          ),
                      ],
                    ),
                  ),
      ),
    );
  }

  void _logout() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Deconnexion', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
        content: Text('Voulez-vous vous deconnecter ?', style: GoogleFonts.manrope(fontSize: 14)),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: Text('Annuler', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: TextButton.styleFrom(foregroundColor: AppColors.error),
            child: Text('Deconnecter', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );
    if (confirmed != true || !mounted) return;

    ref.read(apiClientProvider).clearToken();
    ref.read(signalRServiceProvider).dispose();
    ref.read(authStateProvider.notifier).state = null;
    ref.read(unreadCountProvider.notifier).state = 0;
    context.go('/login');
  }

  Widget _buildHeader(String name) {
    return Row(children: [
      Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text("L'Atelier Couture", style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant, letterSpacing: 1.5, fontWeight: FontWeight.w600)),
        const SizedBox(height: 4),
        Text('Bonjour, ${name.split(' ').first}', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
      ])),
      GestureDetector(
        onTap: _logout,
        child: CircleAvatar(backgroundColor: AppColors.primaryContainer, radius: 22, child: Text(name.isNotEmpty ? name[0] : '?', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600, fontSize: 18))),
      ),
    ]);
  }

  Widget _buildQuarterSelector() {
    return Container(
      height: 44,
      padding: const EdgeInsets.symmetric(horizontal: 14),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.outlineVariant),
      ),
      child: DropdownButtonHideUnderline(
        child: DropdownButton<String>(
          value: _currentQuarterLabel,
          isExpanded: true,
          icon: const Icon(Icons.calendar_month, size: 20, color: AppColors.primary),
          style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600, color: AppColors.onSurface),
          items: _quarterOptions.map((s) => DropdownMenuItem(value: s, child: Text(s))).toList(),
          onChanged: _onQuarterChanged,
        ),
      ),
    );
  }

  Widget _buildLateAlert() {
    return GestureDetector(
      onTap: () => context.go('/orders'),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(color: AppColors.error.withAlpha(20), borderRadius: BorderRadius.circular(12)),
        child: Row(children: [
          const Icon(Icons.warning_amber_rounded, color: AppColors.error, size: 20),
          const SizedBox(width: 10),
          Expanded(child: Text('${_kpis!['lateOrders']} commande(s) depassent le delai', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: AppColors.error))),
          const Icon(Icons.chevron_right, color: AppColors.error, size: 18),
        ]),
      ),
    );
  }

  Widget _buildKPIGrid() {
    return Row(children: [
      Expanded(child: _kpiCard('Commandes', '${_kpis?['totalOrders'] ?? 0}', AppColors.primary, Icons.receipt_long)),
      const SizedBox(width: 10),
      Expanded(child: _kpiCard('Livrees', '${_kpis?['deliveredOrders'] ?? 0}', AppColors.statusPrete, Icons.check_circle_outline)),
      const SizedBox(width: 10),
      Expanded(child: _kpiCard('En retard', '${_kpis?['lateOrders'] ?? 0}', AppColors.error, Icons.schedule)),
    ]);
  }

  Widget _kpiCard(String label, String value, Color color, IconData icon) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [BoxShadow(color: AppColors.primary.withAlpha(10), blurRadius: 16, offset: const Offset(0, 4))],
      ),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Icon(icon, color: color, size: 20),
        const SizedBox(height: 8),
        Text(value, style: GoogleFonts.notoSerif(fontSize: 26, fontWeight: FontWeight.w700, color: color)),
        const SizedBox(height: 2),
        Text(label, style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
      ]),
    );
  }

  Widget _buildRevenueCard() {
    final revenue = (_kpis?['revenueCollected'] ?? 0).toDouble();
    final outstanding = (_kpis?['outstandingBalances'] ?? 0).toDouble();
    final embroidered = _kpis?['embroideredOrders'] ?? 0;
    final beaded = _kpis?['beadedOrders'] ?? 0;
    final onTimeRate = (_kpis?['onTimeDeliveryRate'] ?? 0).toDouble();

    return Container(
      padding: const EdgeInsets.all(18),
      decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(16)),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Row(children: [
          const Icon(Icons.monetization_on_outlined, color: AppColors.secondary, size: 18),
          const SizedBox(width: 8),
          Text(_formatDZD(revenue), style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w700)),
          const Spacer(),
          Container(
            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
            decoration: BoxDecoration(color: AppColors.statusPrete.withAlpha(25), borderRadius: BorderRadius.circular(10)),
            child: Text('${onTimeRate.toStringAsFixed(0)}% a temps', style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w600, color: AppColors.statusPrete)),
          ),
        ]),
        Text('CA encaisse ce trimestre', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
        const SizedBox(height: 12),
        Row(children: [
          _miniStat('Soldes dus', _formatDZD(outstanding), AppColors.secondary),
          const SizedBox(width: 16),
          _miniStat('Brodees', '$embroidered', AppColors.statusBroderie),
          const SizedBox(width: 16),
          _miniStat('Perlees', '$beaded', AppColors.statusPerlage),
        ]),
      ]),
    );
  }

  Widget _miniStat(String label, String value, Color color) {
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text(value, style: GoogleFonts.notoSerif(fontSize: 15, fontWeight: FontWeight.w700, color: color)),
      Text(label, style: GoogleFonts.manrope(fontSize: 10, color: AppColors.onSurfaceVariant)),
    ]);
  }

  Widget _buildRecentOrdersHeader() {
    return Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
      Text('Commandes Recentes', style: GoogleFonts.notoSerif(fontSize: 17, fontWeight: FontWeight.w600)),
      GestureDetector(
        onTap: () => context.go('/orders'),
        child: Text('TOUT VOIR', style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary)),
      ),
    ]);
  }

  Widget _buildRecentOrderCard(dynamic order) {
    final o = order as Map<String, dynamic>;
    final isLate = o['isLate'] == true;
    return GestureDetector(
      onTap: () => context.push('/orders/${o['id']}'),
      child: Container(
        margin: const EdgeInsets.only(bottom: 8),
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(12),
          border: isLate ? Border.all(color: AppColors.error.withAlpha(50)) : null,
        ),
        child: Row(children: [
          CircleAvatar(radius: 18, backgroundColor: AppColors.statusColor(o['status'] ?? '').withAlpha(30),
            child: Icon(Icons.checkroom, size: 18, color: AppColors.statusColor(o['status'] ?? ''))),
          const SizedBox(width: 12),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text(o['code'] ?? '', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600)),
            Text(o['clientName'] ?? 'Client', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
          ])),
          Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
            StatusBadge(status: o['status'] ?? '', label: o['statusLabel'] ?? ''),
            if (isLate) Padding(
              padding: const EdgeInsets.only(top: 4),
              child: Text('${o['delayDays']}j retard', style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w600, color: AppColors.error)),
            ),
          ]),
        ]),
      ),
    );
  }

  String _formatDZD(double amount) {
    if (amount >= 1000) {
      return '${(amount / 1000).toStringAsFixed(amount % 1000 == 0 ? 0 : 1)}K DZD';
    }
    return '${amount.toStringAsFixed(0)} DZD';
  }
}
