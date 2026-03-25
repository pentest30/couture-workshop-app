import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/api/api_client.dart';

class ChangeStatusSheet extends StatefulWidget {
  final Map<String, dynamic> order;
  final ApiClient api;
  const ChangeStatusSheet({super.key, required this.order, required this.api});
  @override
  State<ChangeStatusSheet> createState() => _ChangeStatusSheetState();
}

class _ChangeStatusSheetState extends State<ChangeStatusSheet> {
  final _reasonController = TextEditingController();
  bool _loading = false;

  List<Map<String, dynamic>> get _transitions {
    final status = widget.order['status'] as String;
    final workType = widget.order['workType'] as String;
    final transitions = <Map<String, dynamic>>[];

    if (status == 'Recue') {
      transitions.addAll([
        {'to': 'EnAttente', 'label': 'Mettre en Attente', 'icon': Icons.hourglass_empty, 'color': AppColors.statusEnAttente},
        {'to': 'EnCours', 'label': 'Démarrer le Travail', 'icon': Icons.play_arrow, 'color': AppColors.statusEnCours},
      ]);
    } else if (status == 'EnAttente') {
      transitions.add({'to': 'EnCours', 'label': 'Démarrer le Travail', 'icon': Icons.play_arrow, 'color': AppColors.statusEnCours});
    } else if (status == 'EnCours') {
      if (workType == 'Brode' || workType == 'Mixte') {
        transitions.add({'to': 'Broderie', 'label': 'Passer en Broderie', 'icon': Icons.brush, 'color': AppColors.statusBroderie});
      }
      if (workType == 'Perle' || workType == 'Mixte') {
        transitions.add({'to': 'Perlage', 'label': 'Passer en Perlage', 'icon': Icons.diamond, 'color': AppColors.statusPerlage});
      }
      transitions.addAll([
        {'to': 'Retouche', 'label': 'Signaler Retouche', 'icon': Icons.content_cut, 'color': AppColors.statusRetouche, 'needsReason': true},
        {'to': 'Prete', 'label': 'Marquer Prête', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Broderie') {
      if (workType == 'Mixte') {
        transitions.add({'to': 'Perlage', 'label': 'Passer en Perlage', 'icon': Icons.diamond, 'color': AppColors.statusPerlage});
      }
      transitions.addAll([
        {'to': 'Retouche', 'label': 'Signaler Retouche', 'icon': Icons.content_cut, 'color': AppColors.statusRetouche, 'needsReason': true},
        {'to': 'Prete', 'label': 'Marquer Prête', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Perlage') {
      transitions.addAll([
        {'to': 'Retouche', 'label': 'Signaler Retouche', 'icon': Icons.content_cut, 'color': AppColors.statusRetouche, 'needsReason': true},
        {'to': 'Prete', 'label': 'Marquer Prête', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Retouche') {
      transitions.addAll([
        {'to': 'EnCours', 'label': 'Reprendre le Travail', 'icon': Icons.play_arrow, 'color': AppColors.statusEnCours},
        {'to': 'Prete', 'label': 'Marquer Prête', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Prete') {
      transitions.add({'to': 'Livree', 'label': 'Marquer Livrée', 'icon': Icons.local_shipping, 'color': AppColors.statusLivree});
    }
    return transitions;
  }

  Future<void> _changeStatus(String newStatus, {bool needsReason = false}) async {
    if (needsReason && _reasonController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Motif de retouche obligatoire')));
      return;
    }
    setState(() => _loading = true);
    try {
      await widget.api.changeStatus(widget.order['id'], newStatus, reason: needsReason ? _reasonController.text : null);
      if (mounted) Navigator.pop(context);
    } catch (e) {
      if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Erreur: $e')));
    }
    setState(() => _loading = false);
  }

  @override
  Widget build(BuildContext context) {
    final currentLabel = widget.order['statusLabel'] ?? '';
    return Container(
      decoration: const BoxDecoration(color: AppColors.background, borderRadius: BorderRadius.vertical(top: Radius.circular(24))),
      padding: const EdgeInsets.fromLTRB(24, 16, 24, 32),
      child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
        Center(child: Container(width: 40, height: 4, decoration: BoxDecoration(color: AppColors.outlineVariant, borderRadius: BorderRadius.circular(2)))),
        const SizedBox(height: 20),
        Text('Changer le Statut', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
        const SizedBox(height: 4),
        Text('Actuellement: $currentLabel', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
        const SizedBox(height: 20),
        ..._transitions.map((t) {
          final needsReason = t['needsReason'] == true;
          return Padding(
            padding: const EdgeInsets.only(bottom: 10),
            child: Column(children: [
              GestureDetector(
                onTap: _loading ? null : () => needsReason ? null : _changeStatus(t['to']),
                child: Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: (t['color'] as Color).withOpacity(0.08),
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: Row(children: [
                    Icon(t['icon'] as IconData, color: t['color'] as Color, size: 22),
                    const SizedBox(width: 12),
                    Expanded(child: Text(t['label'], style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600, color: AppColors.onSurface))),
                    Icon(Icons.chevron_right, color: AppColors.onSurfaceVariant),
                  ]),
                ),
              ),
              if (needsReason) ...[
                const SizedBox(height: 8),
                TextField(
                  controller: _reasonController,
                  decoration: InputDecoration(
                    hintText: 'Ex: Ajustement de l\'encolure nécessaire...',
                    hintStyle: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant),
                  ),
                  maxLines: 2,
                ),
                const SizedBox(height: 8),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed: _loading ? null : () => _changeStatus(t['to'], needsReason: true),
                    style: ElevatedButton.styleFrom(backgroundColor: AppColors.statusRetouche, foregroundColor: Colors.white, shape: const StadiumBorder()),
                    child: Text('CONFIRMER RETOUCHE', style: GoogleFonts.manrope(fontWeight: FontWeight.w700, letterSpacing: 0.5)),
                  ),
                ),
              ],
            ]),
          );
        }),
        const SizedBox(height: 8),
        Center(child: TextButton(onPressed: () => Navigator.pop(context), child: Text('ANNULER', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, letterSpacing: 1, color: AppColors.onSurfaceVariant)))),
      ]),
    );
  }
}
