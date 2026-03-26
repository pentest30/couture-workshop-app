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

  // Artisan assignment state
  String? _selectedTailorId;
  String? _selectedEmbroidererId;
  String? _selectedBeaderId;
  List<Map<String, dynamic>> _tailors = [];
  List<Map<String, dynamic>> _embroiderers = [];
  List<Map<String, dynamic>> _beaders = [];
  bool _artisansLoaded = false;

  // Expanded transition (to show artisan picker inline)
  String? _expandedStatus;

  @override
  void initState() {
    super.initState();
    _loadArtisans();
  }

  @override
  void dispose() {
    _reasonController.dispose();
    super.dispose();
  }

  Future<void> _loadArtisans() async {
    try {
      final results = await Future.wait([
        widget.api.listUsersByRole('Tailor'),
        widget.api.listUsersByRole('Embroiderer'),
        widget.api.listUsersByRole('Beader'),
      ]);
      if (mounted) {
        setState(() {
          _tailors = results[0];
          _embroiderers = results[1];
          _beaders = results[2];
          _artisansLoaded = true;
        });
      }
    } catch (_) {
      if (mounted) setState(() => _artisansLoaded = true);
    }
  }

  List<Map<String, dynamic>> get _transitions {
    final status = widget.order['status'] as String;
    final workType = widget.order['workType'] as String? ?? 'Simple';
    final transitions = <Map<String, dynamic>>[];

    if (status == 'Recue') {
      transitions.addAll([
        {'to': 'EnAttente', 'label': 'Mettre en Attente', 'icon': Icons.hourglass_empty, 'color': AppColors.statusEnAttente},
        {'to': 'EnCours', 'label': 'D\u00e9marrer le Travail', 'icon': Icons.play_arrow, 'color': AppColors.statusEnCours, 'artisanRole': 'tailor'},
      ]);
    } else if (status == 'EnAttente') {
      transitions.add({'to': 'EnCours', 'label': 'D\u00e9marrer le Travail', 'icon': Icons.play_arrow, 'color': AppColors.statusEnCours, 'artisanRole': 'tailor'});
    } else if (status == 'EnCours') {
      if (workType == 'Brode' || workType == 'Mixte') {
        transitions.add({'to': 'Broderie', 'label': 'Passer en Broderie', 'icon': Icons.brush, 'color': AppColors.statusBroderie, 'artisanRole': 'embroiderer'});
      }
      if (workType == 'Perle' || workType == 'Mixte') {
        transitions.add({'to': 'Perlage', 'label': 'Passer en Perlage', 'icon': Icons.diamond, 'color': AppColors.statusPerlage, 'artisanRole': 'beader'});
      }
      transitions.addAll([
        {'to': 'Retouche', 'label': 'Signaler Retouche', 'icon': Icons.content_cut, 'color': AppColors.statusRetouche, 'needsReason': true},
        {'to': 'Prete', 'label': 'Marquer Pr\u00eate', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Broderie') {
      if (workType == 'Mixte') {
        transitions.add({'to': 'Perlage', 'label': 'Passer en Perlage', 'icon': Icons.diamond, 'color': AppColors.statusPerlage, 'artisanRole': 'beader'});
      }
      transitions.addAll([
        {'to': 'Retouche', 'label': 'Signaler Retouche', 'icon': Icons.content_cut, 'color': AppColors.statusRetouche, 'needsReason': true},
        {'to': 'Prete', 'label': 'Marquer Pr\u00eate', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Perlage') {
      transitions.addAll([
        {'to': 'Retouche', 'label': 'Signaler Retouche', 'icon': Icons.content_cut, 'color': AppColors.statusRetouche, 'needsReason': true},
        {'to': 'Prete', 'label': 'Marquer Pr\u00eate', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Retouche') {
      transitions.addAll([
        {'to': 'EnCours', 'label': 'Reprendre le Travail', 'icon': Icons.play_arrow, 'color': AppColors.statusEnCours},
        {'to': 'Prete', 'label': 'Marquer Pr\u00eate', 'icon': Icons.check, 'color': AppColors.statusPrete},
      ]);
    } else if (status == 'Prete') {
      transitions.add({'to': 'Livree', 'label': 'Marquer Livr\u00e9e', 'icon': Icons.local_shipping, 'color': AppColors.statusLivree});
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
      await widget.api.changeStatus(
        widget.order['id'],
        newStatus,
        reason: needsReason ? _reasonController.text.trim() : null,
        tailorId: _selectedTailorId,
        embroidererId: _selectedEmbroidererId,
        beaderId: _selectedBeaderId,
        actualDeliveryDate: newStatus == 'Livree'
            ? DateTime.now().toIso8601String().substring(0, 10)
            : null,
      );
      if (mounted) Navigator.pop(context, true);
    } catch (e) {
      if (mounted) {
        setState(() => _loading = false);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(
          content: Text('Erreur: $e'),
          backgroundColor: AppColors.error,
        ));
      }
    }
  }

  List<Map<String, dynamic>> _getArtisansForRole(String? role) {
    switch (role) {
      case 'tailor': return _tailors;
      case 'embroiderer': return _embroiderers;
      case 'beader': return _beaders;
      default: return [];
    }
  }

  String _roleLabel(String? role) {
    switch (role) {
      case 'tailor': return 'Couturi\u00e8re';
      case 'embroiderer': return 'Brodeur(se)';
      case 'beader': return 'Perleur(se)';
      default: return '';
    }
  }

  void _onArtisanSelected(String? role, String? id) {
    setState(() {
      switch (role) {
        case 'tailor': _selectedTailorId = id; break;
        case 'embroiderer': _selectedEmbroidererId = id; break;
        case 'beader': _selectedBeaderId = id; break;
      }
    });
  }

  String? _selectedForRole(String? role) {
    switch (role) {
      case 'tailor': return _selectedTailorId;
      case 'embroiderer': return _selectedEmbroidererId;
      case 'beader': return _selectedBeaderId;
      default: return null;
    }
  }

  @override
  Widget build(BuildContext context) {
    final currentLabel = widget.order['statusLabel'] ?? widget.order['status'] ?? '';
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
        if (_loading)
          const Center(child: Padding(
            padding: EdgeInsets.symmetric(vertical: 24),
            child: CircularProgressIndicator(color: AppColors.primary),
          ))
        else
          ..._transitions.map((t) {
            final needsReason = t['needsReason'] == true;
            final artisanRole = t['artisanRole'] as String?;
            final isExpanded = _expandedStatus == t['to'];
            final hasArtisanPicker = artisanRole != null && _artisansLoaded && _getArtisansForRole(artisanRole).isNotEmpty;

            return Padding(
              padding: const EdgeInsets.only(bottom: 10),
              child: Column(children: [
                GestureDetector(
                  onTap: _loading ? null : () {
                    if (needsReason) {
                      setState(() => _expandedStatus = isExpanded ? null : t['to']);
                    } else if (hasArtisanPicker) {
                      // Toggle expand to show artisan picker
                      setState(() => _expandedStatus = isExpanded ? null : t['to']);
                    } else {
                      _changeStatus(t['to']);
                    }
                  },
                  child: Container(
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: (t['color'] as Color).withOpacity(isExpanded ? 0.15 : 0.08),
                      borderRadius: BorderRadius.circular(16),
                      border: isExpanded ? Border.all(color: (t['color'] as Color).withOpacity(0.3)) : null,
                    ),
                    child: Row(children: [
                      Icon(t['icon'] as IconData, color: t['color'] as Color, size: 22),
                      const SizedBox(width: 12),
                      Expanded(child: Text(t['label'], style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600, color: AppColors.onSurface))),
                      Icon(isExpanded ? Icons.expand_less : Icons.chevron_right, color: AppColors.onSurfaceVariant),
                    ]),
                  ),
                ),
                // Expanded section: artisan picker and/or reason
                if (isExpanded) ...[
                  if (hasArtisanPicker) ...[
                    const SizedBox(height: 10),
                    _buildArtisanDropdown(artisanRole),
                  ],
                  if (needsReason) ...[
                    const SizedBox(height: 8),
                    TextField(
                      controller: _reasonController,
                      decoration: InputDecoration(
                        hintText: 'Ex: Ajustement de l\'encolure n\u00e9cessaire...',
                        hintStyle: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant),
                      ),
                      maxLines: 2,
                    ),
                  ],
                  const SizedBox(height: 10),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _loading ? null : () => _changeStatus(t['to'], needsReason: needsReason),
                      style: ElevatedButton.styleFrom(
                        backgroundColor: t['color'] as Color,
                        foregroundColor: Colors.white,
                        shape: const StadiumBorder(),
                        padding: const EdgeInsets.symmetric(vertical: 14),
                      ),
                      child: Text('CONFIRMER', style: GoogleFonts.manrope(fontWeight: FontWeight.w700, letterSpacing: 0.5)),
                    ),
                  ),
                ],
              ]),
            );
          }),
        const SizedBox(height: 8),
        Center(child: TextButton(onPressed: _loading ? null : () => Navigator.pop(context, false), child: Text('ANNULER', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, letterSpacing: 1, color: AppColors.onSurfaceVariant)))),
      ]),
    );
  }

  Widget _buildArtisanDropdown(String role) {
    final artisans = _getArtisansForRole(role);
    final selected = _selectedForRole(role) ?? '';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: AppColors.outlineVariant),
      ),
      child: DropdownButtonHideUnderline(
        child: DropdownButton<String>(
          isExpanded: true,
          value: selected,
          icon: const Icon(Icons.person_outline, size: 20),
          style: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurface, fontWeight: FontWeight.w500),
          items: [
            DropdownMenuItem<String>(
              value: '',
              child: Text('${_roleLabel(role)} (optionnel)', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
            ),
            ...artisans.map((u) => DropdownMenuItem<String>(
              value: u['id'] as String,
              child: Text('${u['firstName']} ${u['lastName']}', style: GoogleFonts.manrope(fontSize: 14)),
            )),
          ],
          onChanged: (val) => _onArtisanSelected(role, val == '' ? null : val),
        ),
      ),
    );
  }
}