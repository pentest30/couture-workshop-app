import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';

class MeasurementsScreen extends ConsumerStatefulWidget {
  final String clientId;
  final String clientName;
  final List<dynamic> currentMeasurements;

  const MeasurementsScreen({
    super.key,
    required this.clientId,
    required this.clientName,
    required this.currentMeasurements,
  });

  @override
  ConsumerState<MeasurementsScreen> createState() => _MeasurementsScreenState();
}

class _MeasurementsScreenState extends ConsumerState<MeasurementsScreen> {
  final Map<String, TextEditingController> _controllers = {};
  bool _saving = false;
  bool _loading = true;
  List<dynamic> _fields = [];

  // Default fields if API doesn't return them
  static const _defaultFields = [
    {'name': 'Tour de poitrine', 'unit': 'cm', 'id': 'poitrine'},
    {'name': 'Tour de taille', 'unit': 'cm', 'id': 'taille'},
    {'name': 'Tour de hanches', 'unit': 'cm', 'id': 'hanches'},
    {'name': 'Longueur robe (dos)', 'unit': 'cm', 'id': 'longueur_robe'},
    {'name': 'Longueur jupe', 'unit': 'cm', 'id': 'longueur_jupe'},
    {'name': 'Longueur manche', 'unit': 'cm', 'id': 'longueur_manche'},
    {'name': 'Tour de bras', 'unit': 'cm', 'id': 'tour_bras'},
    {'name': 'Epaule', 'unit': 'cm', 'id': 'epaule'},
    {'name': 'Carrure dos', 'unit': 'cm', 'id': 'carrure_dos'},
    {'name': 'Hauteur totale', 'unit': 'cm', 'id': 'hauteur'},
  ];

  @override
  void initState() {
    super.initState();
    _loadFields();
  }

  @override
  void dispose() {
    for (final c in _controllers.values) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _loadFields() async {
    try {
      final history = await ref.read(apiClientProvider).getMeasurementHistory(widget.clientId);
      final current = (history['current'] as List?) ?? [];

      // Use current measurements as fields, or defaults
      if (current.isNotEmpty) {
        _fields = current;
        for (final m in current) {
          final key = m['fieldId']?.toString() ?? m['fieldName']?.toString() ?? '';
          _controllers[key] = TextEditingController(text: m['value']?.toString() ?? '');
        }
      } else {
        // Use known measurement names from currentMeasurements passed in
        if (widget.currentMeasurements.isNotEmpty) {
          _fields = widget.currentMeasurements;
          for (final m in widget.currentMeasurements) {
            final key = m['fieldId']?.toString() ?? m['fieldName']?.toString() ?? '';
            _controllers[key] = TextEditingController(text: m['value']?.toString() ?? '');
          }
        } else {
          _fields = _defaultFields;
          for (final f in _defaultFields) {
            _controllers[f['id']!] = TextEditingController();
          }
        }
      }
    } catch (_) {
      _fields = _defaultFields;
      for (final f in _defaultFields) {
        _controllers[f['id']!] = TextEditingController();
      }
    }
    setState(() => _loading = false);
  }

  Future<void> _save() async {
    final measurements = <Map<String, dynamic>>[];

    for (final field in _fields) {
      final key = field['fieldId']?.toString() ?? field['id']?.toString() ?? '';
      final value = double.tryParse(_controllers[key]?.text ?? '');
      if (value != null && value > 0) {
        final fieldId = field['fieldId']?.toString() ?? field['id']?.toString();
        if (fieldId != null) {
          measurements.add({'fieldId': fieldId, 'value': value});
        }
      }
    }

    if (measurements.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Veuillez saisir au moins une mesure')),
      );
      return;
    }

    setState(() => _saving = true);
    try {
      await ref.read(apiClientProvider).recordMeasurements(widget.clientId, measurements);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Mesures enregistrees'), backgroundColor: AppColors.statusPrete),
        );
        Navigator.pop(context, true);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erreur: $e'), backgroundColor: AppColors.error),
        );
      }
    }
    setState(() => _saving = false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text('Mesures', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600)),
        leading: const BackButton(),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
          : Column(children: [
              // Client header
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                color: AppColors.surfaceContainerLow,
                child: Row(children: [
                  CircleAvatar(
                    radius: 20,
                    backgroundColor: AppColors.primaryContainer,
                    child: Text(
                      widget.clientName.isNotEmpty ? widget.clientName[0] : '?',
                      style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(widget.clientName, style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
                    Text('Saisie des mensurations', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
                  ])),
                  Icon(Icons.straighten, color: AppColors.secondary),
                ]),
              ),
              const SizedBox(height: 8),

              // Measurement fields
              Expanded(
                child: ListView.builder(
                  padding: const EdgeInsets.fromLTRB(20, 8, 20, 100),
                  itemCount: _fields.length,
                  itemBuilder: (_, i) {
                    final field = _fields[i];
                    final name = field['fieldName']?.toString() ?? field['name']?.toString() ?? 'Mesure ${i + 1}';
                    final unit = field['unit']?.toString() ?? 'cm';
                    final key = field['fieldId']?.toString() ?? field['id']?.toString() ?? '$i';

                    return Padding(
                      padding: const EdgeInsets.only(bottom: 12),
                      child: Row(children: [
                        Expanded(
                          flex: 3,
                          child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                            Text(name, style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w500)),
                          ]),
                        ),
                        const SizedBox(width: 12),
                        Expanded(
                          flex: 2,
                          child: TextField(
                            controller: _controllers[key],
                            keyboardType: const TextInputType.numberWithOptions(decimal: true),
                            textAlign: TextAlign.center,
                            style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600),
                            decoration: InputDecoration(
                              suffixText: unit,
                              suffixStyle: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant),
                              contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                            ),
                          ),
                        ),
                      ]),
                    );
                  },
                ),
              ),
            ]),
      bottomSheet: !_loading
          ? Container(
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
                      : Text('ENREGISTRER LES MESURES', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
                ),
              ),
            )
          : null,
    );
  }
}
