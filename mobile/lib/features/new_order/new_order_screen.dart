import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/measurement_fields_widget.dart';

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
  bool _searching = false;
  String? _stepError;
  Timer? _debounce;

  // Catalog model
  Map<String, dynamic>? _selectedModel;
  List<dynamic> _catalogModels = [];
  bool _loadingCatalog = false;

  // Embroidery / beading fields
  final _embroideryDescController = TextEditingController();
  final _beadingDescController = TextEditingController();

  // Inline client creation
  bool _createMode = false;
  final _newFirstNameCtrl = TextEditingController();
  final _newLastNameCtrl = TextEditingController();
  final _newPhoneCtrl = TextEditingController();
  final _newPhone2Ctrl = TextEditingController();
  final _newAddressCtrl = TextEditingController();
  final _newNotesCtrl = TextEditingController();
  final _newMeasurementCtrls = <String, TextEditingController>{};
  final _measurementFieldsKey = GlobalKey<MeasurementFieldsWidgetState>();
  bool _creatingClient = false;
  String? _createError;

  @override
  void dispose() {
    _debounce?.cancel();
    _searchController.dispose();
    _descController.dispose();
    _fabricController.dispose();
    _priceController.dispose();
    _depositController.dispose();
    _embroideryDescController.dispose();
    _beadingDescController.dispose();
    _newFirstNameCtrl.dispose();
    _newLastNameCtrl.dispose();
    _newPhoneCtrl.dispose();
    _newPhone2Ctrl.dispose();
    _newAddressCtrl.dispose();
    _newNotesCtrl.dispose();
    for (final c in _newMeasurementCtrls.values) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _loadCatalogModels() async {
    setState(() => _loadingCatalog = true);
    try {
      final api = ref.read(apiClientProvider);
      final data = await api.getCatalogModels();
      setState(() { _catalogModels = data['items'] ?? []; _loadingCatalog = false; });
    } catch (_) {
      setState(() => _loadingCatalog = false);
    }
  }

  void _applyModel(Map<String, dynamic> model) {
    setState(() {
      _selectedModel = model;
      _workType = model['workType'] ?? 'Simple';
      _descController.text = model['name'] ?? '';
      _priceController.text = (model['basePrice'] ?? 0).toString();
    });
  }

  void _clearModel() {
    setState(() {
      _selectedModel = null;
      _workType = 'Simple';
      _descController.clear();
      _priceController.clear();
    });
  }

  Future<void> _showCatalogPicker() async {
    await _loadCatalogModels();
    if (!mounted) return;
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => Container(
        height: MediaQuery.of(context).size.height * 0.6,
        decoration: const BoxDecoration(
          color: AppColors.background,
          borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
        ),
        padding: const EdgeInsets.all(20),
        child: Column(children: [
          Center(child: Container(width: 40, height: 4, decoration: BoxDecoration(color: AppColors.outlineVariant, borderRadius: BorderRadius.circular(2)))),
          const SizedBox(height: 16),
          Text('Choisir un modele', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
          const SizedBox(height: 12),
          Expanded(
            child: _catalogModels.isEmpty
                ? Center(child: Text('Aucun modele dans le catalogue', style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant)))
                : ListView.separated(
                    itemCount: _catalogModels.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 8),
                    itemBuilder: (_, i) {
                      final m = _catalogModels[i] as Map<String, dynamic>;
                      return GestureDetector(
                        onTap: () {
                          _applyModel(m);
                          Navigator.pop(context);
                        },
                        child: Container(
                          padding: const EdgeInsets.all(12),
                          decoration: BoxDecoration(
                            color: AppColors.surface,
                            borderRadius: BorderRadius.circular(12),
                            border: Border.all(color: AppColors.outlineVariant.withValues(alpha: 0.3)),
                          ),
                          child: Row(children: [
                            Container(
                              width: 48, height: 48,
                              decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(8)),
                              child: const Icon(Icons.auto_stories, color: AppColors.onSurfaceVariant, size: 20),
                            ),
                            const SizedBox(width: 12),
                            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                              Text(m['name'] ?? '', style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600)),
                              Text('${m['categoryLabel'] ?? ''} — ${m['workType'] ?? ''}', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
                            ])),
                            Text('${m['basePrice'] ?? 0} DZD', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w700, color: AppColors.secondary)),
                          ]),
                        ),
                      );
                    },
                  ),
          ),
        ]),
      ),
    );
  }

  void _onSearchChanged(String query) {
    _debounce?.cancel();
    if (query.length < 2) {
      setState(() {
        _searchResults = [];
        _searching = false;
      });
      return;
    }
    setState(() => _searching = true);
    _debounce = Timer(const Duration(milliseconds: 400), () => _searchClients(query));
  }

  Future<void> _searchClients(String query) async {
    try {
      final results = await ref.read(apiClientProvider).searchClients(query);
      if (mounted) setState(() { _searchResults = results; _searching = false; });
    } catch (e) {
      if (mounted) setState(() => _searching = false);
    }
  }

  Future<void> _createClientInline() async {
    if (_newFirstNameCtrl.text.trim().isEmpty || _newLastNameCtrl.text.trim().isEmpty) {
      setState(() => _createError = 'Prenom et nom sont obligatoires');
      return;
    }
    if (_newPhoneCtrl.text.trim().isEmpty) {
      setState(() => _createError = 'Telephone est obligatoire');
      return;
    }

    setState(() { _creatingClient = true; _createError = null; });
    try {
      final api = ref.read(apiClientProvider);
      final clientData = <String, dynamic>{
        'firstName': _newFirstNameCtrl.text.trim(),
        'lastName': _newLastNameCtrl.text.trim(),
        'primaryPhone': _newPhoneCtrl.text.trim(),
      };
      if (_newPhone2Ctrl.text.trim().isNotEmpty) clientData['secondaryPhone'] = _newPhone2Ctrl.text.trim();
      if (_newAddressCtrl.text.trim().isNotEmpty) clientData['address'] = _newAddressCtrl.text.trim();
      if (_newNotesCtrl.text.trim().isNotEmpty) clientData['notes'] = _newNotesCtrl.text.trim();

      final result = await api.createClient(clientData);
      final clientId = result['id']?.toString();

      // Save measurements if any filled
      if (clientId != null) {
        final measurements = _measurementFieldsKey.currentState?.getFilledMeasurements() ?? [];
        if (measurements.isNotEmpty) {
          try {
            await api.recordMeasurements(clientId, measurements);
          } catch (_) {}
        }
      }

      // Auto-select and advance to step 2
      setState(() {
        _selectedClient = {
          'id': result['id'],
          'code': result['code'],
          'firstName': _newFirstNameCtrl.text.trim(),
          'lastName': _newLastNameCtrl.text.trim(),
          'primaryPhone': _newPhoneCtrl.text.trim(),
        };
        _createMode = false;
        _step = 1;
        _stepError = null;
      });

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Client ${result['code']} cree'), backgroundColor: AppColors.statusPrete),
        );
      }
    } catch (e) {
      setState(() => _createError = e.toString());
    }
    if (mounted) setState(() => _creatingClient = false);
  }

  bool _validateStep() {
    setState(() => _stepError = null);
    switch (_step) {
      case 0:
        if (_selectedClient == null) {
          setState(() => _stepError = 'Veuillez selectionner un client.');
          return false;
        }
        return true;
      case 1:
        if (_workType.isEmpty) {
          setState(() => _stepError = 'Veuillez selectionner un type de confection.');
          return false;
        }
        return true;
      case 2:
        final price = double.tryParse(_priceController.text) ?? 0;
        final deposit = double.tryParse(_depositController.text) ?? 0;
        if (_deliveryDate == null) {
          setState(() => _stepError = 'Veuillez selectionner une date de livraison.');
          return false;
        }
        if (price <= 0) {
          setState(() => _stepError = 'Le prix doit etre superieur a 0.');
          return false;
        }
        if (deposit < 0 || deposit > price) {
          setState(() => _stepError = 'L\'acompte doit etre entre 0 et le prix total.');
          return false;
        }
        return true;
      default:
        return true;
    }
  }

  void _onNext() {
    if (!_validateStep()) return;
    if (_step < 2) {
      setState(() => _step++);
    } else {
      _createOrder();
    }
  }

  Future<void> _createOrder() async {
    if (_selectedClient == null || _deliveryDate == null) return;
    setState(() { _loading = true; _stepError = null; });
    try {
      final api = ref.read(apiClientProvider);
      final data = <String, dynamic>{
        'clientId': _selectedClient!['id'],
        'workType': _workType,
        'expectedDeliveryDate': '${_deliveryDate!.year}-${_deliveryDate!.month.toString().padLeft(2, '0')}-${_deliveryDate!.day.toString().padLeft(2, '0')}',
        'totalPrice': double.tryParse(_priceController.text) ?? 0,
        'depositPaymentMethod': 'Especes',
      };
      final deposit = double.tryParse(_depositController.text);
      if (deposit != null && deposit > 0) data['initialDeposit'] = deposit;
      if (_descController.text.isNotEmpty) data['description'] = _descController.text;
      if (_fabricController.text.isNotEmpty) data['fabric'] = _fabricController.text;
      if (_embroideryDescController.text.isNotEmpty) data['embroideryDescription'] = _embroideryDescController.text;
      if (_beadingDescController.text.isNotEmpty) data['beadingDescription'] = _beadingDescController.text;
      if (_selectedModel != null) data['catalogModelId'] = _selectedModel!['id'];

      await api.createOrder(data);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Commande creee avec succes !'), backgroundColor: AppColors.statusPrete),
        );
        context.go('/orders');
      }
    } catch (e) {
      if (mounted) {
        setState(() => _stepError = 'Erreur lors de la creation: $e');
      }
    }
    if (mounted) setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    final hideNextButton = _createMode && _step == 0;

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(children: [
          // Header
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
            child: Row(children: [
              GestureDetector(
                onTap: () => context.pop(),
                child: const Icon(Icons.arrow_back, size: 22),
              ),
              const SizedBox(width: 12),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text("L'Atelier Couture", style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                Text('Nouvelle Commande', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
              ])),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(color: AppColors.primary.withOpacity(0.08), borderRadius: BorderRadius.circular(12)),
                child: Text('${_step + 1} / 3', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w700, color: AppColors.primary)),
              ),
            ]),
          ),
          const SizedBox(height: 8),
          // Step indicator
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 20),
            child: Row(children: List.generate(3, (i) => Expanded(
              child: Container(
                height: 3, margin: const EdgeInsets.symmetric(horizontal: 2),
                decoration: BoxDecoration(
                  color: i <= _step ? AppColors.primary : AppColors.outlineVariant.withOpacity(0.3),
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
            ))),
          ),
          // Step label
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 10, 20, 0),
            child: Align(
              alignment: Alignment.centerLeft,
              child: Text(
                ['Client', 'Travail', 'Planning'][_step],
                style: GoogleFonts.manrope(fontSize: 11, letterSpacing: 1.2, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600),
              ),
            ),
          ),
          const SizedBox(height: 8),
          // Error message
          if (_stepError != null)
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20),
              child: Container(
                width: double.infinity,
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(color: AppColors.error.withOpacity(0.08), borderRadius: BorderRadius.circular(10)),
                child: Row(children: [
                  const Icon(Icons.error_outline, color: AppColors.error, size: 18),
                  const SizedBox(width: 8),
                  Expanded(child: Text(_stepError!, style: GoogleFonts.manrope(fontSize: 12, color: AppColors.error, fontWeight: FontWeight.w500))),
                ]),
              ),
            ),
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
          // Navigation buttons (hidden when in create mode on step 0)
          if (!hideNextButton)
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 8, 20, 24),
              child: Row(children: [
                if (_step > 0) Expanded(child: OutlinedButton(
                  onPressed: _loading ? null : () => setState(() { _step--; _stepError = null; }),
                  style: OutlinedButton.styleFrom(foregroundColor: AppColors.primary, side: const BorderSide(color: AppColors.outlineVariant), shape: const StadiumBorder(), padding: const EdgeInsets.symmetric(vertical: 14)),
                  child: Text('PRECEDENT', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
                )),
                if (_step > 0) const SizedBox(width: 12),
                Expanded(child: ElevatedButton(
                  onPressed: _loading ? null : _onNext,
                  style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder(), padding: const EdgeInsets.symmetric(vertical: 14)),
                  child: _loading
                      ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                      : Text(_step < 2 ? 'SUIVANT' : 'CREER', style: GoogleFonts.manrope(fontWeight: FontWeight.w700)),
                )),
              ]),
            ),
        ]),
      ),
    );
  }

  Widget _buildClientStep() {
    // If a client is already selected, show it
    if (_selectedClient != null && !_createMode) {
      return ListView(children: [
        const SizedBox(height: 4),
        Text('CLIENT SELECTIONNE', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
        const SizedBox(height: 8),
        _clientTile(_selectedClient!, selected: true),
        const SizedBox(height: 8),
        Align(
          alignment: Alignment.centerLeft,
          child: TextButton.icon(
            onPressed: () => setState(() { _selectedClient = null; _searchController.clear(); }),
            icon: const Icon(Icons.swap_horiz, size: 16),
            label: Text('Changer de client', style: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w600)),
            style: TextButton.styleFrom(foregroundColor: AppColors.onSurfaceVariant),
          ),
        ),
      ]);
    }

    return ListView(children: [
      const SizedBox(height: 4),
      // Mode toggle: search / create
      Row(children: [
        Expanded(child: GestureDetector(
          onTap: () => setState(() => _createMode = false),
          child: Container(
            padding: const EdgeInsets.symmetric(vertical: 12),
            decoration: BoxDecoration(
              color: !_createMode ? AppColors.primary.withOpacity(0.08) : AppColors.surface,
              borderRadius: BorderRadius.circular(12),
              border: !_createMode ? Border.all(color: AppColors.primary) : Border.all(color: AppColors.outlineVariant.withOpacity(0.3)),
            ),
            child: Row(mainAxisAlignment: MainAxisAlignment.center, children: [
              Icon(Icons.search, size: 18, color: !_createMode ? AppColors.primary : AppColors.onSurfaceVariant),
              const SizedBox(width: 6),
              Text('Client existant', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: !_createMode ? AppColors.primary : AppColors.onSurfaceVariant)),
            ]),
          ),
        )),
        const SizedBox(width: 10),
        Expanded(child: GestureDetector(
          onTap: () => setState(() => _createMode = true),
          child: Container(
            padding: const EdgeInsets.symmetric(vertical: 12),
            decoration: BoxDecoration(
              color: _createMode ? AppColors.primary.withOpacity(0.08) : AppColors.surface,
              borderRadius: BorderRadius.circular(12),
              border: _createMode ? Border.all(color: AppColors.primary) : Border.all(color: AppColors.outlineVariant.withOpacity(0.3)),
            ),
            child: Row(mainAxisAlignment: MainAxisAlignment.center, children: [
              Icon(Icons.person_add, size: 18, color: _createMode ? AppColors.primary : AppColors.onSurfaceVariant),
              const SizedBox(width: 6),
              Text('Nouveau client', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: _createMode ? AppColors.primary : AppColors.onSurfaceVariant)),
            ]),
          ),
        )),
      ]),
      const SizedBox(height: 16),

      if (!_createMode) ...[
        // Search mode
        TextField(
          controller: _searchController,
          onChanged: _onSearchChanged,
          decoration: InputDecoration(
            hintText: 'Rechercher par nom ou telephone...',
            prefixIcon: const Icon(Icons.search),
            suffixIcon: _searching ? const Padding(padding: EdgeInsets.all(12), child: SizedBox(width: 18, height: 18, child: CircularProgressIndicator(strokeWidth: 2, color: AppColors.primary))) : null,
            hintStyle: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurfaceVariant),
          ),
        ),
        const SizedBox(height: 16),
        if (_searchResults.isNotEmpty) ...[
          Text('RESULTATS', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          ..._searchResults.map((c) => _clientTile(c, selectable: true)),
        ],
        // No results hint
        if (_searchController.text.length >= 2 && _searchResults.isEmpty && !_searching) ...[
          const SizedBox(height: 24),
          Center(child: Column(children: [
            Icon(Icons.search_off, size: 40, color: AppColors.onSurfaceVariant.withOpacity(0.4)),
            const SizedBox(height: 8),
            Text('Aucun resultat', style: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurfaceVariant)),
            const SizedBox(height: 4),
            TextButton(
              onPressed: () => setState(() => _createMode = true),
              child: Text('Creer un nouveau client', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: AppColors.primary)),
            ),
          ])),
        ],
      ] else ...[
        // Create mode: inline client form
        _buildInlineClientForm(),
      ],
    ]);
  }

  Widget _buildInlineClientForm() {
    return Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
      Text('INFORMATIONS', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
      const SizedBox(height: 12),
      Row(children: [
        Expanded(child: TextField(controller: _newFirstNameCtrl, decoration: InputDecoration(labelText: 'Prenom *', labelStyle: GoogleFonts.manrope(fontSize: 13)))),
        const SizedBox(width: 12),
        Expanded(child: TextField(controller: _newLastNameCtrl, decoration: InputDecoration(labelText: 'Nom *', labelStyle: GoogleFonts.manrope(fontSize: 13)))),
      ]),
      const SizedBox(height: 12),
      TextField(
        controller: _newPhoneCtrl,
        keyboardType: TextInputType.phone,
        decoration: InputDecoration(labelText: 'Telephone *', hintText: '0550123456 ou +213550123456', prefixIcon: const Icon(Icons.phone_outlined, size: 20), labelStyle: GoogleFonts.manrope(fontSize: 13)),
      ),
      const SizedBox(height: 12),
      TextField(
        controller: _newPhone2Ctrl,
        keyboardType: TextInputType.phone,
        decoration: InputDecoration(labelText: 'Tel. secondaire', hintText: '0550123456 ou +213...', prefixIcon: const Icon(Icons.phone_outlined, size: 20), labelStyle: GoogleFonts.manrope(fontSize: 13)),
      ),
      const SizedBox(height: 12),
      TextField(
        controller: _newAddressCtrl,
        decoration: InputDecoration(labelText: 'Adresse', prefixIcon: const Icon(Icons.location_on_outlined, size: 20), labelStyle: GoogleFonts.manrope(fontSize: 13)),
      ),
      const SizedBox(height: 12),
      TextField(
        controller: _newNotesCtrl,
        maxLines: 2,
        decoration: InputDecoration(labelText: 'Notes', labelStyle: GoogleFonts.manrope(fontSize: 13)),
      ),
      const SizedBox(height: 20),

      // Measurements section
      Row(children: [
        const Icon(Icons.straighten, color: AppColors.secondary, size: 20),
        const SizedBox(width: 8),
        Text('MENSURATIONS', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
      ]),
      const SizedBox(height: 4),
      Text('Optionnel — vous pouvez les ajouter plus tard', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
      const SizedBox(height: 12),
      MeasurementFieldsWidget(
        key: _measurementFieldsKey,
        api: ref.read(apiClientProvider),
        controllers: _newMeasurementCtrls,
        showAddRemove: true,
      ),

      // Error message
      if (_createError != null) ...[
        const SizedBox(height: 12),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(color: AppColors.error.withOpacity(0.08), borderRadius: BorderRadius.circular(10)),
          child: Row(children: [
            const Icon(Icons.error_outline, color: AppColors.error, size: 18),
            const SizedBox(width: 8),
            Expanded(child: Text(_createError!, style: GoogleFonts.manrope(fontSize: 12, color: AppColors.error, fontWeight: FontWeight.w500))),
          ]),
        ),
      ],

      const SizedBox(height: 16),
      SizedBox(
        width: double.infinity,
        child: ElevatedButton(
          onPressed: _creatingClient ? null : _createClientInline,
          style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder(), padding: const EdgeInsets.symmetric(vertical: 14)),
          child: _creatingClient
              ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
              : Text('CREER ET CONTINUER', style: GoogleFonts.manrope(fontWeight: FontWeight.w700)),
        ),
      ),
      const SizedBox(height: 16),
    ]);
  }

  Widget _clientTile(Map<String, dynamic> c, {bool selectable = false, bool selected = false}) {
    final isSelected = selected || _selectedClient?['id'] == c['id'];
    return GestureDetector(
      onTap: selectable ? () => setState(() { _selectedClient = c; _searchResults = []; _searchController.clear(); _stepError = null; }) : null,
      child: Container(
        margin: const EdgeInsets.only(bottom: 8),
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: isSelected ? AppColors.secondaryFixed.withOpacity(0.15) : AppColors.surface,
          borderRadius: BorderRadius.circular(14),
          border: isSelected ? Border.all(color: AppColors.secondaryFixed) : null,
        ),
        child: Row(children: [
          CircleAvatar(
            radius: 20, backgroundColor: AppColors.primaryContainer,
            child: Text(
              '${(c['firstName'] as String?)?.isNotEmpty == true ? c['firstName'][0] : ''}${(c['lastName'] as String?)?.isNotEmpty == true ? c['lastName'][0] : ''}',
              style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w600),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('${c['firstName'] ?? ''} ${c['lastName'] ?? ''}', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
            Text('${c['code'] ?? ''} ${c['primaryPhone'] != null ? '- ${c['primaryPhone']}' : ''}', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
          ])),
          if (isSelected) const Icon(Icons.check_circle, color: AppColors.statusPrete, size: 22),
        ]),
      ),
    );
  }

  Widget _buildWorkStep() {
    final types = ['Simple', 'Brode', 'Perle', 'Mixte'];
    final labels = {'Simple': 'Simple', 'Brode': 'Brode', 'Perle': 'Perle', 'Mixte': 'Mixte'};
    final icons = {'Simple': Icons.checkroom, 'Brode': Icons.brush, 'Perle': Icons.diamond, 'Mixte': Icons.auto_awesome};
    final showEmbroidery = _workType == 'Brode' || _workType == 'Mixte';
    final showBeading = _workType == 'Perle' || _workType == 'Mixte';

    return ListView(children: [
      const SizedBox(height: 4),
      Text('Details du Travail', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600)),
      const SizedBox(height: 12),

      // Catalog model picker
      if (_selectedModel != null)
        Container(
          padding: const EdgeInsets.all(12),
          margin: const EdgeInsets.only(bottom: 16),
          decoration: BoxDecoration(color: AppColors.secondary.withValues(alpha: 0.08), borderRadius: BorderRadius.circular(12), border: Border.all(color: AppColors.secondary.withValues(alpha: 0.2))),
          child: Row(children: [
            const Icon(Icons.auto_stories, size: 20, color: AppColors.secondary),
            const SizedBox(width: 10),
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(_selectedModel!['name'] ?? '', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600)),
              Text('${_selectedModel!['categoryLabel'] ?? ''} — ${_selectedModel!['basePrice'] ?? 0} DZD', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
            ])),
            GestureDetector(onTap: _clearModel, child: const Icon(Icons.close, size: 18, color: AppColors.onSurfaceVariant)),
          ]),
        )
      else
        Padding(
          padding: const EdgeInsets.only(bottom: 16),
          child: OutlinedButton.icon(
            onPressed: () => _showCatalogPicker(),
            icon: const Icon(Icons.auto_stories, size: 18),
            label: Text('Choisir un modele du catalogue', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600)),
            style: OutlinedButton.styleFrom(
              minimumSize: const Size(double.infinity, 48),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
          ),
        ),

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
      // Embroidery fields
      if (showEmbroidery) ...[
        const SizedBox(height: 20),
        Text('BRODERIE', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.statusBroderie, fontWeight: FontWeight.w700)),
        const SizedBox(height: 8),
        TextField(
          controller: _embroideryDescController,
          maxLines: 2,
          decoration: InputDecoration(
            labelText: 'Description de la broderie',
            labelStyle: GoogleFonts.manrope(fontSize: 13),
            prefixIcon: const Icon(Icons.brush, size: 18),
          ),
        ),
      ],
      // Beading fields
      if (showBeading) ...[
        const SizedBox(height: 20),
        Text('PERLAGE', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.statusPerlage, fontWeight: FontWeight.w700)),
        const SizedBox(height: 8),
        TextField(
          controller: _beadingDescController,
          maxLines: 2,
          decoration: InputDecoration(
            labelText: 'Description du perlage',
            labelStyle: GoogleFonts.manrope(fontSize: 13),
            prefixIcon: const Icon(Icons.diamond, size: 18),
          ),
        ),
      ],
    ]);
  }

  Widget _buildPlanningStep() {
    final price = double.tryParse(_priceController.text) ?? 0;
    final deposit = double.tryParse(_depositController.text) ?? 0;
    final balance = (price - deposit).clamp(0, double.infinity);

    return ListView(children: [
      const SizedBox(height: 4),
      Text('Planning & Tarification', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600)),
      const SizedBox(height: 16),
      // Delivery date
      GestureDetector(
        onTap: () async {
          final date = await showDatePicker(
            context: context,
            initialDate: _deliveryDate ?? DateTime.now().add(const Duration(days: 7)),
            firstDate: DateTime.now(),
            lastDate: DateTime.now().add(const Duration(days: 365)),
          );
          if (date != null) setState(() => _deliveryDate = date);
        },
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: _deliveryDate != null ? AppColors.primary.withOpacity(0.06) : AppColors.surfaceContainerLow,
            borderRadius: BorderRadius.circular(12),
            border: _deliveryDate != null ? Border.all(color: AppColors.primary.withOpacity(0.3)) : null,
          ),
          child: Row(children: [
            Icon(Icons.calendar_today_outlined, color: _deliveryDate != null ? AppColors.primary : AppColors.onSurfaceVariant, size: 20),
            const SizedBox(width: 12),
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              if (_deliveryDate != null) Text('DATE DE LIVRAISON', style: GoogleFonts.manrope(fontSize: 9, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
              Text(
                _deliveryDate != null
                    ? '${_deliveryDate!.day.toString().padLeft(2, '0')}/${_deliveryDate!.month.toString().padLeft(2, '0')}/${_deliveryDate!.year}'
                    : 'Date de livraison prevue *',
                style: GoogleFonts.manrope(fontSize: 14, fontWeight: _deliveryDate != null ? FontWeight.w600 : FontWeight.w400, color: _deliveryDate != null ? AppColors.onSurface : AppColors.onSurfaceVariant),
              ),
            ])),
            if (_deliveryDate != null) const Icon(Icons.check_circle_outline, color: AppColors.statusPrete, size: 18),
          ]),
        ),
      ),
      const SizedBox(height: 16),
      TextField(
        controller: _priceController,
        keyboardType: TextInputType.number,
        onChanged: (_) => setState(() {}),
        decoration: InputDecoration(
          labelText: 'Prix total (DZD) *',
          prefixIcon: const Icon(Icons.monetization_on_outlined),
          labelStyle: GoogleFonts.manrope(fontSize: 13),
        ),
      ),
      const SizedBox(height: 12),
      TextField(
        controller: _depositController,
        keyboardType: TextInputType.number,
        onChanged: (_) => setState(() {}),
        decoration: InputDecoration(
          labelText: 'Acompte (DZD)',
          prefixIcon: const Icon(Icons.payments_outlined),
          labelStyle: GoogleFonts.manrope(fontSize: 13),
        ),
      ),
      const SizedBox(height: 16),
      // Balance display
      if (price > 0) Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(color: AppColors.surfaceContainerLow, borderRadius: BorderRadius.circular(12)),
        child: Column(children: [
          _summaryRow('Prix total', '${price.toStringAsFixed(0)} DZD'),
          if (deposit > 0) ...[
            const SizedBox(height: 6),
            _summaryRow('Acompte', '- ${deposit.toStringAsFixed(0)} DZD'),
          ],
          const SizedBox(height: 6),
          const Divider(height: 1),
          const SizedBox(height: 6),
          _summaryRow('Reste a payer', '${balance.toStringAsFixed(0)} DZD', bold: true, color: balance > 0 ? AppColors.secondary : AppColors.statusPrete),
        ]),
      ),
      // Order summary
      const SizedBox(height: 20),
      if (_selectedClient != null) ...[
        Text('RESUME', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
        const SizedBox(height: 8),
        Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(12)),
          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            _summaryRow('Client', '${_selectedClient!['firstName']} ${_selectedClient!['lastName']}'),
            const SizedBox(height: 4),
            _summaryRow('Type', _workType),
            if (_descController.text.isNotEmpty) ...[
              const SizedBox(height: 4),
              _summaryRow('Description', _descController.text),
            ],
          ]),
        ),
      ],
    ]);
  }

  Widget _summaryRow(String label, String value, {bool bold = false, Color? color}) {
    return Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
      Text(label, style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
      Flexible(child: Text(value, style: GoogleFonts.manrope(fontSize: 13, fontWeight: bold ? FontWeight.w700 : FontWeight.w500, color: color ?? AppColors.onSurface), textAlign: TextAlign.end)),
    ]);
  }
}
