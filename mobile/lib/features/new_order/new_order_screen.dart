import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';

class NewOrderScreen extends ConsumerStatefulWidget {
  const NewOrderScreen({super.key});
  @override
  ConsumerState<NewOrderScreen> createState() => _NewOrderScreenState();
}

class _NewOrderScreenState extends ConsumerState<NewOrderScreen> {
  int _step = 0; // 0: client, 1: work, 2: planning
  final _searchController = TextEditingController();
  List<dynamic> _searchResults = [];
  Map<String, dynamic>? _selectedClient;
  String _workType = 'Simple';
  final _descController = TextEditingController();
  final _fabricController = TextEditingController();
  final _priceController = TextEditingController();
  final _depositController = TextEditingController();
  DateTime? _deliveryDate;
  bool _loading = false;

  Future<void> _searchClients(String query) async {
    if (query.length < 2) { setState(() => _searchResults = []); return; }
    try {
      final results = await ref.read(apiClientProvider).searchClients(query);
      setState(() => _searchResults = results);
    } catch (_) {}
  }

  Future<void> _createOrder() async {
    if (_selectedClient == null || _deliveryDate == null) return;
    setState(() => _loading = true);
    try {
      final api = ref.read(apiClientProvider);
      await api.createOrder({
        'clientId': _selectedClient!['id'],
        'workType': _workType,
        'expectedDeliveryDate': '${_deliveryDate!.year}-${_deliveryDate!.month.toString().padLeft(2, '0')}-${_deliveryDate!.day.toString().padLeft(2, '0')}',
        'totalPrice': double.tryParse(_priceController.text) ?? 0,
        'initialDeposit': double.tryParse(_depositController.text),
        'depositPaymentMethod': 'Especes',
        'description': _descController.text.isNotEmpty ? _descController.text : null,
        'fabric': _fabricController.text.isNotEmpty ? _fabricController.text : null,
      });
      if (mounted) { ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Commande créée !'))); context.go('/orders'); }
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erreur: $e')));
    }
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(children: [
          // Header
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
            child: Row(children: [
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text("L'Atelier Couture", style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                Text('Nouvelle Commande', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
              ])),
              Text('Étape ${_step + 1} / 3', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
            ]),
          ),
          const SizedBox(height: 8),
          // Step indicator
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: Row(children: List.generate(3, (i) => Expanded(
              child: Container(
                height: 3, margin: const EdgeInsets.symmetric(horizontal: 2),
                decoration: BoxDecoration(color: i <= _step ? AppColors.primary : AppColors.outlineVariant.withOpacity(0.3), borderRadius: BorderRadius.circular(2)),
              ),
            ))),
          ),
          const SizedBox(height: 20),
          // Step content
          Expanded(
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: switch (_step) {
                0 => _buildClientStep(),
                1 => _buildWorkStep(),
                2 => _buildPlanningStep(),
                _ => const SizedBox(),
              },
            ),
          ),
          // Navigation buttons
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 8, 20, 24),
            child: Row(children: [
              if (_step > 0) Expanded(child: OutlinedButton(
                onPressed: () => setState(() => _step--),
                style: OutlinedButton.styleFrom(foregroundColor: AppColors.primary, side: const BorderSide(color: AppColors.outlineVariant), shape: const StadiumBorder(), padding: const EdgeInsets.symmetric(vertical: 14)),
                child: Text('← PRÉCÉDENT', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
              )),
              if (_step > 0) const SizedBox(width: 12),
              Expanded(child: ElevatedButton(
                onPressed: _loading ? null : () { if (_step < 2) setState(() => _step++); else _createOrder(); },
                style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder(), padding: const EdgeInsets.symmetric(vertical: 14)),
                child: _loading
                    ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                    : Text(_step < 2 ? 'SUIVANT →' : 'CRÉER ✓', style: GoogleFonts.manrope(fontWeight: FontWeight.w700)),
              )),
            ]),
          ),
        ]),
      ),
    );
  }

  Widget _buildClientStep() {
    return ListView(children: [
      Text('Sélectionner le client', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
      const SizedBox(height: 12),
      TextField(
        controller: _searchController,
        onChanged: _searchClients,
        decoration: InputDecoration(hintText: 'Rechercher par nom ou N° client...', prefixIcon: const Icon(Icons.search), hintStyle: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurfaceVariant)),
      ),
      const SizedBox(height: 12),
      // + New client button
      OutlinedButton.icon(
        onPressed: () {}, icon: const Icon(Icons.add),
        label: Text('NOUVEAU CLIENT', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
        style: OutlinedButton.styleFrom(foregroundColor: AppColors.secondary, side: const BorderSide(color: AppColors.secondaryFixed), shape: const StadiumBorder(), padding: const EdgeInsets.symmetric(vertical: 12)),
      ),
      const SizedBox(height: 16),
      if (_searchResults.isNotEmpty) ...[
        Text('CLIENTS RÉCENTS', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
        const SizedBox(height: 8),
        ..._searchResults.map((c) => GestureDetector(
          onTap: () => setState(() { _selectedClient = c; _searchResults = []; }),
          child: Container(
            margin: const EdgeInsets.only(bottom: 8),
            padding: const EdgeInsets.all(14),
            decoration: BoxDecoration(
              color: _selectedClient?['id'] == c['id'] ? AppColors.secondaryFixed.withOpacity(0.15) : AppColors.surface,
              borderRadius: BorderRadius.circular(14),
              border: _selectedClient?['id'] == c['id'] ? Border.all(color: AppColors.secondaryFixed) : null,
            ),
            child: Row(children: [
              CircleAvatar(radius: 20, backgroundColor: AppColors.primaryContainer,
                child: Text('${c['firstName']?[0] ?? ''}${c['lastName']?[0] ?? ''}', style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w600))),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('${c['firstName']} ${c['lastName']}', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
                Text('${c['code']} · ${c['primaryPhone']}', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
              ])),
            ]),
          ),
        )),
      ],
      if (_selectedClient != null && _searchResults.isEmpty) ...[
        const SizedBox(height: 8),
        Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(color: AppColors.secondaryFixed.withOpacity(0.12), borderRadius: BorderRadius.circular(14), border: Border.all(color: AppColors.secondaryFixed)),
          child: Row(children: [
            CircleAvatar(radius: 20, backgroundColor: AppColors.primaryContainer,
              child: Text('${_selectedClient!['firstName']?[0]}${_selectedClient!['lastName']?[0]}', style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w600))),
            const SizedBox(width: 12),
            Expanded(child: Text('${_selectedClient!['firstName']} ${_selectedClient!['lastName']}', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600))),
            const Icon(Icons.check_circle, color: AppColors.statusPrete),
          ]),
        ),
      ],
    ]);
  }

  Widget _buildWorkStep() {
    final types = ['Simple', 'Brode', 'Perle', 'Mixte'];
    final labels = {'Simple': 'Simple', 'Brode': 'Brodé', 'Perle': 'Perlé', 'Mixte': 'Mixte'};
    final icons = {'Simple': Icons.checkroom, 'Brode': Icons.brush, 'Perle': Icons.diamond, 'Mixte': Icons.auto_awesome};

    return ListView(children: [
      Text('Détails du Travail', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600)),
      const SizedBox(height: 4),
      Text('TYPE DE CONFECTION', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
      const SizedBox(height: 12),
      Wrap(spacing: 10, runSpacing: 10, children: types.map((t) {
        final selected = _workType == t;
        return GestureDetector(
          onTap: () => setState(() => _workType = t),
          child: Container(
            width: (MediaQuery.of(context).size.width - 50) / 2,
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(
              color: selected ? AppColors.primary.withOpacity(0.08) : AppColors.surface,
              borderRadius: BorderRadius.circular(14),
              border: selected ? Border.all(color: AppColors.primary, width: 2) : Border.all(color: AppColors.outlineVariant.withOpacity(0.2)),
            ),
            child: Column(children: [
              Icon(icons[t], size: 28, color: selected ? AppColors.primary : AppColors.onSurfaceVariant),
              const SizedBox(height: 8),
              Text(labels[t]!, style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600, color: selected ? AppColors.primary : AppColors.onSurface)),
            ]),
          ),
        );
      }).toList()),
      const SizedBox(height: 20),
      TextField(controller: _descController, maxLines: 3, decoration: InputDecoration(labelText: 'Description du travail', labelStyle: GoogleFonts.manrope(fontSize: 13))),
      const SizedBox(height: 12),
      TextField(controller: _fabricController, decoration: InputDecoration(labelText: 'Tissu', labelStyle: GoogleFonts.manrope(fontSize: 13))),
    ]);
  }

  Widget _buildPlanningStep() {
    return ListView(children: [
      Text('Planning & Tarification', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600)),
      const SizedBox(height: 16),
      // Delivery date
      GestureDetector(
        onTap: () async {
          final date = await showDatePicker(context: context, initialDate: DateTime.now().add(const Duration(days: 7)), firstDate: DateTime.now(), lastDate: DateTime.now().add(const Duration(days: 365)));
          if (date != null) setState(() => _deliveryDate = date);
        },
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(12)),
          child: Row(children: [
            const Icon(Icons.calendar_today_outlined, color: AppColors.onSurfaceVariant, size: 20),
            const SizedBox(width: 12),
            Text(_deliveryDate != null ? '${_deliveryDate!.day}/${_deliveryDate!.month}/${_deliveryDate!.year}' : 'Date de livraison prévue', style: GoogleFonts.manrope(fontSize: 14, color: _deliveryDate != null ? AppColors.onSurface : AppColors.onSurfaceVariant)),
          ]),
        ),
      ),
      const SizedBox(height: 16),
      TextField(controller: _priceController, keyboardType: TextInputType.number, decoration: InputDecoration(labelText: 'Prix total (DZD)', prefixIcon: const Icon(Icons.monetization_on_outlined), labelStyle: GoogleFonts.manrope(fontSize: 13))),
      const SizedBox(height: 12),
      TextField(controller: _depositController, keyboardType: TextInputType.number, decoration: InputDecoration(labelText: 'Acompte (DZD)', prefixIcon: const Icon(Icons.payments_outlined), labelStyle: GoogleFonts.manrope(fontSize: 13))),
    ]);
  }
}
