import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';
import '../../core/widgets/measurement_fields_widget.dart';

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
  final _measurementFieldsKey = GlobalKey<MeasurementFieldsWidgetState>();
  final Map<String, TextEditingController> _controllers = {};
  bool _saving = false;
  bool _loadingHistory = true;

  @override
  void initState() {
    super.initState();
    _loadHistory();
  }

  @override
  void dispose() {
    for (final c in _controllers.values) {
      c.dispose();
    }
    super.dispose();
  }

  Future<void> _loadHistory() async {
    try {
      final history = await ref.read(apiClientProvider).getMeasurementHistory(widget.clientId);
      final current = (history['current'] as List?) ?? [];

      // Pre-populate controllers from current measurements
      for (final m in current) {
        final name = m['fieldName']?.toString() ?? m['name']?.toString() ?? '';
        if (name.isNotEmpty) {
          _controllers[name] = TextEditingController(text: m['value']?.toString() ?? '');
        }
      }

      // Also populate from passed-in measurements if not already set
      for (final m in widget.currentMeasurements) {
        final name = m['fieldName']?.toString() ?? m['name']?.toString() ?? '';
        if (name.isNotEmpty && !_controllers.containsKey(name)) {
          _controllers[name] = TextEditingController(text: m['value']?.toString() ?? '');
        }
      }
    } catch (_) {
      // If history fetch fails, populate from passed-in measurements
      for (final m in widget.currentMeasurements) {
        final name = m['fieldName']?.toString() ?? m['name']?.toString() ?? '';
        if (name.isNotEmpty) {
          _controllers[name] = TextEditingController(text: m['value']?.toString() ?? '');
        }
      }
    }
    if (mounted) setState(() => _loadingHistory = false);
  }

  Future<void> _save() async {
    final measurements = _measurementFieldsKey.currentState?.getFilledMeasurements() ?? [];

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
      body: _loadingHistory
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
                  const Icon(Icons.straighten, color: AppColors.secondary),
                ]),
              ),
              const SizedBox(height: 8),

              // Measurement fields (dynamic, from API)
              Expanded(
                child: ListView(
                  padding: const EdgeInsets.fromLTRB(20, 8, 20, 100),
                  children: [
                    MeasurementFieldsWidget(
                      key: _measurementFieldsKey,
                      api: ref.read(apiClientProvider),
                      controllers: _controllers,
                      showAddRemove: true,
                    ),
                  ],
                ),
              ),
            ]),
      bottomSheet: !_loadingHistory
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
