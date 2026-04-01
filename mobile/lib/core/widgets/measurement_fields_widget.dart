import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../api/api_client.dart';
import '../theme/app_colors.dart';

class MeasurementFieldsWidget extends StatefulWidget {
  final ApiClient api;
  final Map<String, TextEditingController> controllers;
  final bool showAddRemove;

  const MeasurementFieldsWidget({
    super.key,
    required this.api,
    required this.controllers,
    this.showAddRemove = true,
  });

  @override
  State<MeasurementFieldsWidget> createState() => MeasurementFieldsWidgetState();
}

class MeasurementFieldsWidgetState extends State<MeasurementFieldsWidget> {
  List<Map<String, dynamic>> _fields = [];
  bool _loading = true;
  final _newFieldCtrl = TextEditingController();
  bool _addingField = false;

  static const _defaultFields = [
    {'name': 'Tour de poitrine', 'unit': 'cm'},
    {'name': 'Tour de taille', 'unit': 'cm'},
    {'name': 'Tour de hanches', 'unit': 'cm'},
    {'name': 'Longueur robe (dos)', 'unit': 'cm'},
    {'name': 'Longueur jupe', 'unit': 'cm'},
    {'name': 'Longueur manche', 'unit': 'cm'},
    {'name': 'Tour de bras', 'unit': 'cm'},
    {'name': 'Epaule', 'unit': 'cm'},
    {'name': 'Carrure dos', 'unit': 'cm'},
    {'name': 'Hauteur totale', 'unit': 'cm'},
  ];

  @override
  void initState() {
    super.initState();
    _loadFields();
  }

  @override
  void dispose() {
    _newFieldCtrl.dispose();
    super.dispose();
  }

  Future<void> _loadFields() async {
    try {
      final apiFields = await widget.api.getMeasurementFields();
      if (apiFields.isNotEmpty) {
        _fields = apiFields.map((f) => Map<String, dynamic>.from(f as Map)).toList();
      } else {
        _fields = _defaultFields.map((f) => Map<String, dynamic>.from(f)).toList();
      }
    } catch (_) {
      _fields = _defaultFields.map((f) => Map<String, dynamic>.from(f)).toList();
    }

    // Create controllers for fields that don't have one yet
    for (final f in _fields) {
      final key = f['name']?.toString() ?? '';
      if (key.isNotEmpty && !widget.controllers.containsKey(key)) {
        widget.controllers[key] = TextEditingController();
      }
    }

    if (mounted) setState(() => _loading = false);
  }

  /// Returns filled measurements ready for the API, with both fieldId and fieldName.
  List<Map<String, dynamic>> getFilledMeasurements() {
    final result = <Map<String, dynamic>>[];
    for (final f in _fields) {
      final name = f['name']?.toString() ?? '';
      final ctrl = widget.controllers[name];
      final value = double.tryParse(ctrl?.text ?? '');
      if (value != null && value > 0) {
        result.add({
          'fieldId': f['id']?.toString() ?? '',
          'fieldName': name,
          'value': value,
        });
      }
    }
    return result;
  }

  Future<void> _addField() async {
    final name = _newFieldCtrl.text.trim();
    if (name.isEmpty) return;

    // Check duplicate
    if (_fields.any((f) => f['name']?.toString().toLowerCase() == name.toLowerCase())) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Le champ "$name" existe deja'), backgroundColor: AppColors.error),
        );
      }
      return;
    }

    setState(() => _addingField = true);
    try {
      final result = await widget.api.createMeasurementField(name, 'cm', _fields.length + 1);
      final newField = <String, dynamic>{
        'id': result['id'],
        'name': name,
        'unit': 'cm',
      };
      widget.controllers[name] = TextEditingController();
      setState(() {
        _fields.add(newField);
        _newFieldCtrl.clear();
      });
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erreur: $e'), backgroundColor: AppColors.error),
        );
      }
    }
    if (mounted) setState(() => _addingField = false);
  }

  Future<void> _removeField(int index) async {
    final field = _fields[index];
    final name = field['name']?.toString() ?? '';
    final fieldId = field['id']?.toString();

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Supprimer le champ', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
        content: Text("Supprimer le champ '$name' ?", style: GoogleFonts.manrope(fontSize: 14)),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx, false),
            child: Text('Annuler', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
          ),
          TextButton(
            onPressed: () => Navigator.pop(ctx, true),
            style: TextButton.styleFrom(foregroundColor: AppColors.error),
            child: Text('Supprimer', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    try {
      if (fieldId != null && fieldId.isNotEmpty) {
        await widget.api.deleteMeasurementField(fieldId);
      }
      widget.controllers[name]?.dispose();
      widget.controllers.remove(name);
      setState(() => _fields.removeAt(index));
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erreur: $e'), backgroundColor: AppColors.error),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) {
      return const Padding(
        padding: EdgeInsets.symmetric(vertical: 24),
        child: Center(child: CircularProgressIndicator(color: AppColors.primary)),
      );
    }

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Field rows
        ...List.generate(_fields.length, (i) {
          final field = _fields[i];
          final name = field['name']?.toString() ?? 'Mesure ${i + 1}';
          final unit = field['unit']?.toString() ?? 'cm';
          final ctrl = widget.controllers[name];

          return Padding(
            padding: const EdgeInsets.only(bottom: 10),
            child: Row(children: [
              Expanded(
                flex: 3,
                child: Row(children: [
                  Expanded(
                    child: Text(name, style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w500)),
                  ),
                  if (widget.showAddRemove)
                    GestureDetector(
                      onTap: () => _removeField(i),
                      child: Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 4),
                        child: Icon(Icons.remove_circle_outline, size: 18, color: AppColors.error.withValues(alpha: 0.6)),
                      ),
                    ),
                ]),
              ),
              const SizedBox(width: 12),
              Expanded(
                flex: 2,
                child: TextField(
                  controller: ctrl,
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
        }),

        // Add field row
        if (widget.showAddRemove) ...[
          const SizedBox(height: 8),
          Row(children: [
            Expanded(
              child: TextField(
                controller: _newFieldCtrl,
                decoration: InputDecoration(
                  hintText: 'Ajouter un champ...',
                  hintStyle: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant),
                  contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
                  isDense: true,
                ),
                style: GoogleFonts.manrope(fontSize: 14),
                onSubmitted: (_) => _addField(),
              ),
            ),
            const SizedBox(width: 8),
            SizedBox(
              width: 40,
              height: 40,
              child: _addingField
                  ? const Padding(
                      padding: EdgeInsets.all(8),
                      child: CircularProgressIndicator(strokeWidth: 2, color: AppColors.primary),
                    )
                  : IconButton(
                      onPressed: _addField,
                      icon: const Icon(Icons.add_circle, color: AppColors.primary),
                      padding: EdgeInsets.zero,
                    ),
            ),
          ]),
        ],
      ],
    );
  }
}
