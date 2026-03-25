import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/api/api_client.dart';
import '../../core/providers/providers.dart';

class CatalogDetailScreen extends ConsumerStatefulWidget {
  final String modelId;
  const CatalogDetailScreen({super.key, required this.modelId});
  @override
  ConsumerState<CatalogDetailScreen> createState() => _CatalogDetailScreenState();
}

class _CatalogDetailScreenState extends ConsumerState<CatalogDetailScreen> {
  Map<String, dynamic>? _model;
  bool _loading = true;
  int _photoIndex = 0;
  final PageController _pageController = PageController();

  @override
  void initState() {
    super.initState();
    _load();
  }

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  Future<void> _load() async {
    try {
      final data = await ref.read(apiClientProvider).getCatalogModel(widget.modelId);
      if (mounted) setState(() { _model = data; _loading = false; });
    } catch (_) {
      if (mounted) setState(() => _loading = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const Scaffold(body: Center(child: CircularProgressIndicator(color: AppColors.primary)));
    if (_model == null) return Scaffold(appBar: AppBar(), body: const Center(child: Text('Modele introuvable')));

    final m = _model!;
    final photos = (m['photos'] as List<dynamic>?) ?? [];
    final fabrics = (m['fabrics'] as List<dynamic>?) ?? [];

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        title: Text(m['name'] ?? '', style: GoogleFonts.notoSerif(fontSize: 18, fontWeight: FontWeight.w600)),
        backgroundColor: AppColors.background,
        surfaceTintColor: Colors.transparent,
      ),
      body: ListView(
        padding: const EdgeInsets.fromLTRB(20, 0, 20, 100),
        children: [
          // Photo carousel
          if (photos.isNotEmpty) ...[
            ClipRRect(
              borderRadius: BorderRadius.circular(16),
              child: SizedBox(
                height: 240,
                child: Stack(children: [
                  PageView.builder(
                    controller: _pageController,
                    itemCount: photos.length,
                    onPageChanged: (i) => setState(() => _photoIndex = i),
                    itemBuilder: (_, i) {
                      final path = (photos[i] as Map)['storagePath'] ?? '';
                      return Image.network(
                        '${ApiClient.baseUrl}$path',
                        fit: BoxFit.cover,
                        width: double.infinity,
                        errorBuilder: (_, __, ___) => Container(
                          color: AppColors.surfaceContainerLow,
                          child: const Center(child: Icon(Icons.image_not_supported, size: 40, color: AppColors.onSurfaceVariant)),
                        ),
                      );
                    },
                  ),
                  if (photos.length > 1)
                    Positioned(
                      bottom: 10, left: 0, right: 0,
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: List.generate(photos.length, (i) => Container(
                          width: i == _photoIndex ? 8 : 6,
                          height: i == _photoIndex ? 8 : 6,
                          margin: const EdgeInsets.symmetric(horizontal: 3),
                          decoration: BoxDecoration(
                            shape: BoxShape.circle,
                            color: i == _photoIndex ? Colors.white : Colors.white54,
                          ),
                        )),
                      ),
                    ),
                ]),
              ),
            ),
            // Thumbnail strip
            if (photos.length > 1) ...[
              const SizedBox(height: 8),
              SizedBox(
                height: 56,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  itemCount: photos.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 6),
                  itemBuilder: (_, i) {
                    final path = (photos[i] as Map)['storagePath'] ?? '';
                    return GestureDetector(
                      onTap: () { _pageController.animateToPage(i, duration: const Duration(milliseconds: 300), curve: Curves.ease); },
                      child: Container(
                        width: 56,
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: i == _photoIndex ? AppColors.primary : Colors.transparent, width: 2),
                        ),
                        clipBehavior: Clip.antiAlias,
                        child: Image.network('${ApiClient.baseUrl}$path', fit: BoxFit.cover,
                          errorBuilder: (_, __, ___) => Container(color: AppColors.surfaceContainerLow)),
                      ),
                    );
                  },
                ),
              ),
            ],
            const SizedBox(height: 16),
          ],

          // Code + badges
          Text(m['code'] ?? '', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
          const SizedBox(height: 8),
          Row(children: [
            _badge(m['categoryLabel'] ?? '', AppColors.primary),
            const SizedBox(width: 8),
            _badge(m['workType'] ?? '', AppColors.secondary),
            if (m['isPublic'] == true) ...[const SizedBox(width: 8), _badge('Public', AppColors.statusPrete)],
          ]),
          const SizedBox(height: 20),

          // Price + duration
          Container(
            padding: const EdgeInsets.all(16),
            decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(16)),
            child: Row(children: [
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                Text('PRIX DE BASE', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                Text('${m['basePrice'] ?? 0} DZD', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w700, color: AppColors.secondary)),
              ])),
              Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.end, children: [
                Text('DUREE ESTIMEE', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                Text('${m['estimatedDays'] ?? 0} jours', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w700)),
              ])),
            ]),
          ),
          const SizedBox(height: 16),

          // Description
          if (m['description'] != null && (m['description'] as String).isNotEmpty) ...[
            Text('Description', style: GoogleFonts.notoSerif(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            Text(m['description'], style: GoogleFonts.manrope(fontSize: 14, color: AppColors.onSurface, height: 1.5)),
            const SizedBox(height: 20),
          ],

          // Fabrics
          if (fabrics.isNotEmpty) ...[
            Text('Tissus recommandes', style: GoogleFonts.notoSerif(fontSize: 16, fontWeight: FontWeight.w600)),
            const SizedBox(height: 8),
            ...fabrics.map((f) {
              final fabric = f as Map<String, dynamic>;
              return Container(
                margin: const EdgeInsets.only(bottom: 8),
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(color: AppColors.surface, borderRadius: BorderRadius.circular(12)),
                child: Row(children: [
                  Container(width: 32, height: 32, decoration: BoxDecoration(
                    color: _parseColor(fabric['color']),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: AppColors.outlineVariant),
                  )),
                  const SizedBox(width: 12),
                  Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                    Text(fabric['name'] ?? '', style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600)),
                    Text('${fabric['type'] ?? ''} — ${fabric['pricePerMeter'] ?? 0} DZD/m', style: GoogleFonts.manrope(fontSize: 11, color: AppColors.onSurfaceVariant)),
                  ])),
                ]),
              );
            }),
          ],
        ],
      ),
      bottomSheet: Container(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
        decoration: BoxDecoration(color: AppColors.surface, boxShadow: [BoxShadow(color: AppColors.primary.withOpacity(0.06), blurRadius: 20, offset: const Offset(0, -4))]),
        child: SizedBox(
          width: double.infinity, height: 52,
          child: ElevatedButton(
            onPressed: () => context.push('/new-order'),
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.primary, foregroundColor: Colors.white, shape: const StadiumBorder()),
            child: Text('Creer une commande', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
          ),
        ),
      ),
    );
  }

  Widget _badge(String text, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(color: color.withValues(alpha: 0.1), borderRadius: BorderRadius.circular(8)),
      child: Text(text, style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w700, color: color)),
    );
  }

  Color _parseColor(String? hex) {
    if (hex == null || hex.isEmpty) return AppColors.onSurfaceVariant;
    try { return Color(int.parse(hex.replaceFirst('#', '0xFF'))); }
    catch (_) { return AppColors.onSurfaceVariant; }
  }
}
