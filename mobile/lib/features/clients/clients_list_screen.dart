import 'dart:async';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';

class ClientsListScreen extends ConsumerStatefulWidget {
  const ClientsListScreen({super.key});
  @override
  ConsumerState<ClientsListScreen> createState() => _ClientsListScreenState();
}

class _ClientsListScreenState extends ConsumerState<ClientsListScreen> {
  List<dynamic> _clients = [];
  bool _loading = true;
  final _searchController = TextEditingController();
  Timer? _debounce;

  @override
  void initState() {
    super.initState();
    _loadClients();
  }

  @override
  void dispose() {
    _debounce?.cancel();
    _searchController.dispose();
    super.dispose();
  }

  Future<void> _loadClients([String? search]) async {
    setState(() => _loading = true);
    try {
      final api = ref.read(apiClientProvider);
      final data = await api.getClients(search: search);
      setState(() { _clients = data['items'] ?? []; _loading = false; });
    } catch (_) {
      setState(() => _loading = false);
    }
  }

  void _onSearch(String value) {
    _debounce?.cancel();
    _debounce = Timer(const Duration(milliseconds: 400), () {
      _loadClients(value.isEmpty ? null : value);
    });
  }

  void _showNewClientSheet() {
    final firstNameCtrl = TextEditingController();
    final lastNameCtrl = TextEditingController();
    final phoneCtrl = TextEditingController();
    bool creating = false;

    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (ctx) => StatefulBuilder(builder: (ctx, setSheetState) {
        return Container(
          padding: EdgeInsets.fromLTRB(24, 16, 24, MediaQuery.of(ctx).viewInsets.bottom + 24),
          decoration: const BoxDecoration(
            color: AppColors.background,
            borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
          ),
          child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
            Center(child: Container(width: 40, height: 4, decoration: BoxDecoration(color: AppColors.outlineVariant, borderRadius: BorderRadius.circular(2)))),
            const SizedBox(height: 20),
            Text('Nouveau Client', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
            const SizedBox(height: 16),
            TextField(controller: firstNameCtrl, decoration: InputDecoration(labelText: 'Prenom', labelStyle: GoogleFonts.manrope(fontSize: 13))),
            const SizedBox(height: 12),
            TextField(controller: lastNameCtrl, decoration: InputDecoration(labelText: 'Nom', labelStyle: GoogleFonts.manrope(fontSize: 13))),
            const SizedBox(height: 12),
            TextField(controller: phoneCtrl, keyboardType: TextInputType.phone, decoration: InputDecoration(labelText: 'Telephone (ex: 0550123456)', labelStyle: GoogleFonts.manrope(fontSize: 13))),
            const SizedBox(height: 20),
            SizedBox(
              width: double.infinity, height: 50,
              child: ElevatedButton(
                onPressed: creating ? null : () async {
                  if (firstNameCtrl.text.trim().isEmpty || lastNameCtrl.text.trim().isEmpty || phoneCtrl.text.trim().isEmpty) {
                    ScaffoldMessenger.of(ctx).showSnackBar(const SnackBar(content: Text('Tous les champs sont obligatoires')));
                    return;
                  }
                  setSheetState(() => creating = true);
                  try {
                    final result = await ref.read(apiClientProvider).createClient({
                      'firstName': firstNameCtrl.text.trim(),
                      'lastName': lastNameCtrl.text.trim(),
                      'primaryPhone': phoneCtrl.text.trim(),
                    });
                    if (ctx.mounted) Navigator.pop(ctx);
                    if (mounted) {
                      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text('Client ${result['code']} cree'), backgroundColor: AppColors.statusPrete));
                      _loadClients();
                    }
                  } catch (e) {
                    if (ctx.mounted) ScaffoldMessenger.of(ctx).showSnackBar(SnackBar(content: Text('$e'), backgroundColor: AppColors.error));
                    setSheetState(() => creating = false);
                  }
                },
                style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder()),
                child: creating
                    ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                    : Text('CREER LE CLIENT', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
              ),
            ),
          ]),
        );
      }),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('REPERTOIRE', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
              const SizedBox(height: 4),
              Text('Mes Clientes', style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
              const SizedBox(height: 16),
              TextField(
                controller: _searchController,
                onChanged: _onSearch,
                decoration: InputDecoration(
                  hintText: 'Rechercher par nom, code ou telephone...',
                  hintStyle: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurfaceVariant),
                  prefixIcon: const Icon(Icons.search, color: AppColors.onSurfaceVariant),
                  suffixIcon: _searchController.text.isNotEmpty
                      ? IconButton(icon: const Icon(Icons.clear, size: 18), onPressed: () { _searchController.clear(); _loadClients(); })
                      : null,
                ),
              ),
              const SizedBox(height: 16),
            ]),
          ),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
                : _clients.isEmpty
                    ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                        Icon(Icons.people_outline, size: 48, color: AppColors.onSurfaceVariant.withAlpha(80)),
                        const SizedBox(height: 12),
                        Text('Aucune cliente trouvee', style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant)),
                      ]))
                    : RefreshIndicator(
                        onRefresh: () => _loadClients(_searchController.text.isEmpty ? null : _searchController.text),
                        child: ListView.separated(
                          padding: const EdgeInsets.fromLTRB(20, 0, 20, 80),
                          itemCount: _clients.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 8),
                          itemBuilder: (_, i) => _clientCard(_clients[i]),
                        ),
                      ),
          ),
        ]),
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () async {
          await context.push('/clients/new');
          _loadClients(); // Reload after returning
        },
        backgroundColor: AppColors.primary,
        foregroundColor: Colors.white,
        icon: const Icon(Icons.person_add_outlined),
        label: Text('NOUVELLE', style: GoogleFonts.manrope(fontWeight: FontWeight.w600)),
      ),
    );
  }

  Widget _clientCard(Map<String, dynamic> c) {
    final initials = '${(c['firstName'] ?? '')[0]}${(c['lastName'] ?? '')[0]}'.toUpperCase();
    return GestureDetector(
      onTap: () => context.go('/clients/${c['id']}'),
      child: Container(
        padding: const EdgeInsets.all(14),
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(14),
          boxShadow: [BoxShadow(color: AppColors.primary.withAlpha(8), blurRadius: 12, offset: const Offset(0, 3))],
        ),
        child: Row(children: [
          CircleAvatar(
            radius: 24,
            backgroundColor: AppColors.primaryContainer,
            child: Text(initials, style: const TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.w600)),
          ),
          const SizedBox(width: 14),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('${c['firstName']} ${c['lastName']}', style: GoogleFonts.notoSerif(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 2),
            Row(children: [
              Text(c['code'] ?? '', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.secondary, fontWeight: FontWeight.w600)),
              const SizedBox(width: 8),
              Text('·', style: TextStyle(color: AppColors.onSurfaceVariant)),
              const SizedBox(width: 8),
              Text(c['primaryPhone'] ?? '', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant)),
            ]),
          ])),
          Column(children: [
            Icon(Icons.chevron_right, color: AppColors.onSurfaceVariant.withAlpha(120)),
          ]),
        ]),
      ),
    );
  }
}
