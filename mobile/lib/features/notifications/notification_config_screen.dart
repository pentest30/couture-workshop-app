import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';

class NotificationConfigScreen extends ConsumerStatefulWidget {
  const NotificationConfigScreen({super.key});
  @override
  ConsumerState<NotificationConfigScreen> createState() => _NotificationConfigScreenState();
}

class _NotificationConfigScreenState extends ConsumerState<NotificationConfigScreen> {
  List<Map<String, dynamic>> _configs = [];
  bool _loading = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final api = ref.read(apiClientProvider);
      final data = await api.getNotificationConfigs();
      if (mounted) {
        setState(() {
          _configs = data.map((e) => Map<String, dynamic>.from(e as Map)).toList();
          _loading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() { _loading = false; _error = 'Erreur: $e'; });
    }
  }

  Future<void> _toggleEnabled(Map<String, dynamic> config, bool value) async {
    final typeValue = config['typeValue'] as int;
    try {
      await ref.read(apiClientProvider).updateNotificationConfig(typeValue, {'isEnabled': value});
      setState(() => config['isEnabled'] = value);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erreur: $e')));
      }
    }
  }

  Future<void> _toggleSms(Map<String, dynamic> config, bool value) async {
    final typeValue = config['typeValue'] as int;
    try {
      await ref.read(apiClientProvider).updateNotificationConfig(typeValue, {'smsEnabled': value});
      setState(() => config['smsEnabled'] = value);
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erreur: $e')));
      }
    }
  }

  Future<void> _editStallThresholds(Map<String, dynamic> config) async {
    final typeValue = config['typeValue'] as int;
    final simpleCtrl = TextEditingController(text: '${config['stallThresholdSimple'] ?? 3}');
    final brodeCtrl = TextEditingController(text: '${config['stallThresholdEmbroidered'] ?? 7}');
    final perleCtrl = TextEditingController(text: '${config['stallThresholdBeaded'] ?? 10}');
    final mixteCtrl = TextEditingController(text: '${config['stallThresholdMixed'] ?? 14}');

    final result = await showDialog<bool>(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text('Seuils de blocage', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text('Nombre de jours sans changement de statut avant notification N04.',
                style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
            const SizedBox(height: 16),
            _thresholdField('Simple', simpleCtrl),
            const SizedBox(height: 8),
            _thresholdField('Brode', brodeCtrl),
            const SizedBox(height: 8),
            _thresholdField('Perle', perleCtrl),
            const SizedBox(height: 8),
            _thresholdField('Mixte', mixteCtrl),
          ],
        ),
        actions: [
          TextButton(onPressed: () => Navigator.pop(ctx, false), child: const Text('Annuler')),
          FilledButton(onPressed: () => Navigator.pop(ctx, true), child: const Text('Enregistrer')),
        ],
      ),
    );

    if (result == true) {
      try {
        final data = {
          'stallThresholdSimple': int.tryParse(simpleCtrl.text) ?? 3,
          'stallThresholdEmbroidered': int.tryParse(brodeCtrl.text) ?? 7,
          'stallThresholdBeaded': int.tryParse(perleCtrl.text) ?? 10,
          'stallThresholdMixed': int.tryParse(mixteCtrl.text) ?? 14,
        };
        await ref.read(apiClientProvider).updateNotificationConfig(typeValue, data);
        setState(() {
          config['stallThresholdSimple'] = data['stallThresholdSimple'];
          config['stallThresholdEmbroidered'] = data['stallThresholdEmbroidered'];
          config['stallThresholdBeaded'] = data['stallThresholdBeaded'];
          config['stallThresholdMixed'] = data['stallThresholdMixed'];
        });
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            const SnackBar(content: Text('Seuils mis a jour'), duration: Duration(seconds: 2)),
          );
        }
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erreur: $e')));
        }
      }
    }
  }

  Widget _thresholdField(String label, TextEditingController ctrl) {
    return Row(
      children: [
        SizedBox(width: 80, child: Text(label, style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w500))),
        Expanded(
          child: TextField(
            controller: ctrl,
            keyboardType: TextInputType.number,
            decoration: InputDecoration(
              suffixText: 'jours',
              isDense: true,
              contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
              border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
            ),
          ),
        ),
      ],
    );
  }

  Color _priorityColor(String priority) => switch (priority) {
    'Critical' => AppColors.error,
    'High' => AppColors.secondary,
    _ => AppColors.onSurfaceVariant,
  };

  String _priorityLabel(String priority) => switch (priority) {
    'Critical' => 'CRITIQUE',
    'High' => 'IMPORTANT',
    _ => 'MOYEN',
  };

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text('Parametrage alertes', style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600)),
        backgroundColor: AppColors.background,
        surfaceTintColor: Colors.transparent,
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
          : _error != null
              ? Center(child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Text(_error!, style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant)),
                    const SizedBox(height: 16),
                    OutlinedButton(onPressed: _load, child: const Text('Reessayer')),
                  ],
                ))
              : RefreshIndicator(
                  onRefresh: _load,
                  child: ListView.separated(
                    padding: const EdgeInsets.fromLTRB(16, 8, 16, 24),
                    itemCount: _configs.length,
                    separatorBuilder: (_, __) => const SizedBox(height: 8),
                    itemBuilder: (_, i) => _configCard(_configs[i]),
                  ),
                ),
    );
  }

  Widget _configCard(Map<String, dynamic> config) {
    final isEnabled = config['isEnabled'] == true;
    final smsEnabled = config['smsEnabled'] == true;
    final typeName = config['typeName'] as String? ?? '';
    final typeLabel = config['typeLabel'] as String? ?? '';
    final priority = config['priority'] as String? ?? 'Medium';
    final isStalled = typeName == 'N04_Stalled';

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: isEnabled ? AppColors.outlineVariant : AppColors.surfaceContainer),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header row: type code + label + priority badge
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                decoration: BoxDecoration(
                  color: AppColors.primaryContainer,
                  borderRadius: BorderRadius.circular(6),
                ),
                child: Text(
                  typeName.split('_').first,
                  style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700, color: Colors.white, letterSpacing: 0.5),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: Text(typeLabel, style: GoogleFonts.manrope(fontSize: 14, fontWeight: FontWeight.w600)),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                decoration: BoxDecoration(
                  color: _priorityColor(priority).withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: Text(
                  _priorityLabel(priority),
                  style: GoogleFonts.manrope(fontSize: 9, fontWeight: FontWeight.w700, letterSpacing: 1, color: _priorityColor(priority)),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          // Toggle switches
          Row(
            children: [
              Expanded(
                child: Row(
                  children: [
                    Text('Actif', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w500)),
                    const SizedBox(width: 8),
                    SizedBox(
                      height: 28,
                      child: Switch(
                        value: isEnabled,
                        onChanged: (v) => _toggleEnabled(config, v),
                        activeColor: AppColors.primary,
                      ),
                    ),
                  ],
                ),
              ),
              Expanded(
                child: Row(
                  children: [
                    Text('SMS', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w500)),
                    const SizedBox(width: 8),
                    SizedBox(
                      height: 28,
                      child: Switch(
                        value: smsEnabled,
                        onChanged: isEnabled ? (v) => _toggleSms(config, v) : null,
                        activeColor: AppColors.secondary,
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          // Stall thresholds (only for N04)
          if (isStalled) ...[
            const SizedBox(height: 12),
            const Divider(height: 1),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Seuils de blocage', style: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w600)),
                      const SizedBox(height: 4),
                      Text(
                        'S:${config['stallThresholdSimple']}j  B:${config['stallThresholdEmbroidered']}j  P:${config['stallThresholdBeaded']}j  M:${config['stallThresholdMixed']}j',
                        style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant),
                      ),
                    ],
                  ),
                ),
                IconButton(
                  onPressed: isEnabled ? () => _editStallThresholds(config) : null,
                  icon: const Icon(Icons.edit_outlined, size: 20),
                  color: AppColors.primary,
                ),
              ],
            ),
          ],
          // SMS window info
          if (isEnabled && smsEnabled) ...[
            const SizedBox(height: 8),
            Row(
              children: [
                const Icon(Icons.schedule, size: 14, color: AppColors.onSurfaceVariant),
                const SizedBox(width: 4),
                Text(
                  'Plage SMS: ${config['smsWindowStart'] ?? '08:00'} - ${config['smsWindowEnd'] ?? '20:00'}',
                  style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant),
                ),
              ],
            ),
          ],
        ],
      ),
    );
  }
}
