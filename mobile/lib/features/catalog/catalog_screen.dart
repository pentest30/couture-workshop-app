import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/api/api_client.dart';
import '../../core/providers/providers.dart';

class CatalogScreen extends ConsumerStatefulWidget {
  const CatalogScreen({super.key});
  @override
  ConsumerState<CatalogScreen> createState() => _CatalogScreenState();
}

class _CatalogScreenState extends ConsumerState<CatalogScreen> {
  List<dynamic> _models = [];
  bool _loading = true;
  String? _error;
  String _category = '';

  static const _categories = [
    {'key': '', 'label': 'Tous'},
    {'key': 'Ceremonie', 'label': 'Cérémonie'},
    {'key': 'Mariee', 'label': 'Mariée'},
    {'key': 'Traditionnel', 'label': 'Traditionnel'},
    {'key': 'Moderne', 'label': 'Moderne'},
    {'key': 'Quotidien', 'label': 'Quotidien'},
  ];

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final api = ref.read(apiClientProvider);
      final data = await api.getCatalogModels(category: _category.isNotEmpty ? _category : null);
      if (mounted) {
        setState(() {
          _models = data['items'] ?? [];
          _loading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() { _loading = false; _error = 'Erreur: $e'; });
    }
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
              Text('BIBLIOTHEQUE', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
              const SizedBox(height: 4),
              Text('Catalogue', style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
              const SizedBox(height: 12),
              // Category chips
              SizedBox(
                height: 36,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  itemCount: _categories.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 8),
                  itemBuilder: (_, i) {
                    final cat = _categories[i];
                    final selected = _category == cat['key'];
                    return GestureDetector(
                      onTap: () { setState(() => _category = cat['key']!); _load(); },
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 7),
                        decoration: BoxDecoration(
                          color: selected ? AppColors.primary : AppColors.surfaceContainerLow,
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: Text(cat['label']!, style: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w600, color: selected ? Colors.white : AppColors.onSurface)),
                      ),
                    );
                  },
                ),
              ),
              const SizedBox(height: 16),
            ]),
          ),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
                : _error != null
                    ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                        Text(_error!, style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant, fontSize: 13)),
                        const SizedBox(height: 12),
                        OutlinedButton(onPressed: _load, child: const Text('Reessayer')),
                      ]))
                    : _models.isEmpty
                        ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                            const Icon(Icons.auto_stories, size: 48, color: AppColors.onSurfaceVariant),
                            const SizedBox(height: 12),
                            Text('Aucun modele', style: GoogleFonts.manrope(color: AppColors.onSurfaceVariant)),
                          ]))
                        : RefreshIndicator(
                            onRefresh: _load,
                            child: GridView.builder(
                              padding: const EdgeInsets.fromLTRB(20, 0, 20, 24),
                              gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                                crossAxisCount: 2, mainAxisSpacing: 12, crossAxisSpacing: 12, childAspectRatio: 0.75,
                              ),
                              itemCount: _models.length,
                              itemBuilder: (_, i) => _modelCard(_models[i]),
                            ),
                          ),
          ),
        ]),
      ),
    );
  }

  Widget _modelCard(Map<String, dynamic> m) {
    return GestureDetector(
      onTap: () => context.push('/catalog/${m['id']}'),
      child: Container(
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          boxShadow: [BoxShadow(color: AppColors.primary.withValues(alpha: 0.04), blurRadius: 12)],
        ),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          // Photo area
          Expanded(
            child: Container(
              width: double.infinity,
              decoration: BoxDecoration(
                color: AppColors.surfaceContainerLow,
                borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
              ),
              clipBehavior: Clip.antiAlias,
              child: m['primaryPhotoPath'] != null
                  ? Image.network(
                      '${ApiClient.baseUrl}${m['primaryPhotoPath']}',
                      fit: BoxFit.cover,
                      width: double.infinity,
                      height: double.infinity,
                      errorBuilder: (_, __, ___) => const Center(child: Icon(Icons.auto_stories, size: 32, color: AppColors.onSurfaceVariant)),
                    )
                  : const Center(child: Icon(Icons.auto_stories, size: 32, color: AppColors.onSurfaceVariant)),
            ),
          ),
          // Info
          Padding(
            padding: const EdgeInsets.all(12),
            child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text(m['code'] ?? '', style: GoogleFonts.manrope(fontSize: 9, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
              const SizedBox(height: 2),
              Text(m['name'] ?? '', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600), maxLines: 1, overflow: TextOverflow.ellipsis),
              const SizedBox(height: 4),
              Row(children: [
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                  decoration: BoxDecoration(color: AppColors.primary.withValues(alpha: 0.08), borderRadius: BorderRadius.circular(6)),
                  child: Text(m['categoryLabel'] ?? '', style: GoogleFonts.manrope(fontSize: 9, fontWeight: FontWeight.w600, color: AppColors.primary)),
                ),
                const Spacer(),
                Text('${m['basePrice'] ?? 0} DZD', style: GoogleFonts.manrope(fontSize: 11, fontWeight: FontWeight.w700, color: AppColors.secondary)),
              ]),
            ]),
          ),
        ]),
      ),
    );
  }
}
