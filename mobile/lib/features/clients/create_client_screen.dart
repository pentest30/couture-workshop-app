import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/measurement_fields_widget.dart';

class CreateClientScreen extends ConsumerStatefulWidget {
  const CreateClientScreen({super.key});
  @override
  ConsumerState<CreateClientScreen> createState() => _CreateClientScreenState();
}

class _CreateClientScreenState extends ConsumerState<CreateClientScreen> {
  final _firstNameCtrl = TextEditingController();
  final _lastNameCtrl = TextEditingController();
  final _phoneCtrl = TextEditingController();
  final _phone2Ctrl = TextEditingController();
  final _addressCtrl = TextEditingController();
  final _notesCtrl = TextEditingController();
  bool _saving = false;
  String? _error;

  // Measurement widget
  final _measurementFieldsKey = GlobalKey<MeasurementFieldsWidgetState>();
  final _measurementCtrls = <String, TextEditingController>{};

  @override
  void dispose() {
    _firstNameCtrl.dispose();
    _lastNameCtrl.dispose();
    _phoneCtrl.dispose();
    _phone2Ctrl.dispose();
    _addressCtrl.dispose();
    _notesCtrl.dispose();
    for (final c in _measurementCtrls.values) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _save() async {
    // Validate
    if (_firstNameCtrl.text.trim().isEmpty || _lastNameCtrl.text.trim().isEmpty) {
      setState(() => _error = 'Prénom et nom sont obligatoires');
      return;
    }
    if (_phoneCtrl.text.trim().isEmpty) {
      setState(() => _error = 'Téléphone est obligatoire');
      return;
    }
    if (!RegExp(r'^0[567]\d{8}$').hasMatch(_phoneCtrl.text.trim())) {
      setState(() => _error = 'Format téléphone invalide (ex: 0550123456)');
      return;
    }

    setState(() { _saving = true; _error = null; });
    try {
      final api = ref.read(apiClientProvider);

      // 1. Create client
      final clientData = <String, dynamic>{
        'firstName': _firstNameCtrl.text.trim(),
        'lastName': _lastNameCtrl.text.trim(),
        'primaryPhone': _phoneCtrl.text.trim(),
      };
      if (_phone2Ctrl.text.trim().isNotEmpty) clientData['secondaryPhone'] = _phone2Ctrl.text.trim();
      if (_addressCtrl.text.trim().isNotEmpty) clientData['address'] = _addressCtrl.text.trim();
      if (_notesCtrl.text.trim().isNotEmpty) clientData['notes'] = _notesCtrl.text.trim();

      final result = await api.createClient(clientData);
      final clientId = result['id']?.toString();

      // 2. Record measurements if any filled
      if (clientId != null) {
        final measurements = _measurementFieldsKey.currentState?.getFilledMeasurements() ?? [];
        if (measurements.isNotEmpty) {
          try {
            await api.recordMeasurements(clientId, measurements);
          } catch (_) {
            // Non-blocking: client was created, measurements failed
          }
        }
      }

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Client ${result['code']} créé avec succès'), backgroundColor: AppColors.statusPrete),
        );
        context.go('/clients');
      }
    } catch (e) {
      setState(() => _error = e.toString());
    }
    if (mounted) setState(() => _saving = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text('Nouveau Client', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600)),
        leading: const BackButton(),
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 8, 20, 100),
        children: [
          // Section: Informations personnelles
          Text('INFORMATIONS PERSONNELLES', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
          const SizedBox(height: 12),
          Row(children: [
            Expanded(child: TextField(controller: _firstNameCtrl, decoration: InputDecoration(labelText: 'Prénom *', labelStyle: GoogleFonts.manrope(fontSize: 13)))),
            const SizedBox(width: 12),
            Expanded(child: TextField(controller: _lastNameCtrl, decoration: InputDecoration(labelText: 'Nom *', labelStyle: GoogleFonts.manrope(fontSize: 13)))),
          ]),
          const SizedBox(height: 12),
          TextField(controller: _phoneCtrl, keyboardType: TextInputType.phone,
            decoration: InputDecoration(labelText: 'Téléphone principal *', hintText: '0550123456', prefixIcon: const Icon(Icons.phone_outlined, size: 20), labelStyle: GoogleFonts.manrope(fontSize: 13))),
          const SizedBox(height: 12),
          TextField(controller: _phone2Ctrl, keyboardType: TextInputType.phone,
            decoration: InputDecoration(labelText: 'Téléphone secondaire', prefixIcon: const Icon(Icons.phone_outlined, size: 20), labelStyle: GoogleFonts.manrope(fontSize: 13))),
          const SizedBox(height: 12),
          TextField(controller: _addressCtrl,
            decoration: InputDecoration(labelText: 'Adresse', prefixIcon: const Icon(Icons.location_on_outlined, size: 20), labelStyle: GoogleFonts.manrope(fontSize: 13))),
          const SizedBox(height: 12),
          TextField(controller: _notesCtrl, maxLines: 2,
            decoration: InputDecoration(labelText: 'Notes / Préférences', labelStyle: GoogleFonts.manrope(fontSize: 13))),

          const SizedBox(height: 28),

          // Section: Mensurations
          Row(children: [
            const Icon(Icons.straighten, color: AppColors.secondary, size: 20),
            const SizedBox(width: 8),
            Text('MENSURATIONS', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
          ]),
          const SizedBox(height: 4),
          Text('Optionnel — vous pouvez les ajouter plus tard', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
          const SizedBox(height: 12),

          // Measurement fields (dynamic, from API)
          MeasurementFieldsWidget(
            key: _measurementFieldsKey,
            api: ref.read(apiClientProvider),
            controllers: _measurementCtrls,
            showAddRemove: true,
          ),

          // Error
          if (_error != null) ...[
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(color: AppColors.error.withAlpha(20), borderRadius: BorderRadius.circular(8)),
              child: Text(_error!, style: GoogleFonts.manrope(fontSize: 13, color: AppColors.error)),
            ),
          ],
        ],
      ),
      bottomSheet: Container(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        decoration: BoxDecoration(
          color: AppColors.surface,
          boxShadow: [BoxShadow(color: AppColors.primary.withAlpha(12), blurRadius: 20, offset: const Offset(0, -4))],
        ),
        child: SizedBox(
          width: double.infinity, height: 52,
          child: ElevatedButton(
            onPressed: _saving ? null : _save,
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder()),
            child: _saving
                ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                : Text('ENREGISTRER LE CLIENT', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
          ),
        ),
      ),
    );
  }
}
