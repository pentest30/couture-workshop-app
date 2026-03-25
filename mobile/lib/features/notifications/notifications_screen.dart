import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/providers/providers.dart';

class NotificationsScreen extends ConsumerStatefulWidget {
  const NotificationsScreen({super.key});
  @override
  ConsumerState<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends ConsumerState<NotificationsScreen> {
  List<dynamic> _notifications = [];
  int _unreadCount = 0;
  bool _loading = true;
  String _filter = 'all';

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final api = ref.read(apiClientProvider);
      final data = await api.getNotifications(filter: _filter);
      setState(() {
        _notifications = data['items'] ?? [];
        _unreadCount = data['unreadCount'] ?? 0;
        _loading = false;
      });
    } catch (_) { setState(() => _loading = false); }
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
              Row(children: [
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text('Notifications', style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
                  Text('CENTRE DE GESTION', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                ])),
                TextButton(
                  onPressed: () async { await ref.read(apiClientProvider).markAllRead(); _load(); },
                  child: Text('Tout marquer lu', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.secondary, fontWeight: FontWeight.w600)),
                ),
              ]),
              const SizedBox(height: 16),
              // Filter tabs
              Row(children: [
                _filterChip('Toutes', 'all'),
                const SizedBox(width: 8),
                _filterChip('Non lues', 'unread', badge: _unreadCount),
                const SizedBox(width: 8),
                _filterChip('Critiques', 'critical'),
              ]),
              const SizedBox(height: 16),
            ]),
          ),
          Expanded(
            child: _loading
                ? const Center(child: CircularProgressIndicator(color: AppColors.primary))
                : _notifications.isEmpty
                    ? Center(child: Column(mainAxisSize: MainAxisSize.min, children: [
                        Text('"La précision est l\'âme de la couture."', style: GoogleFonts.notoSerif(fontSize: 16, fontStyle: FontStyle.italic, color: AppColors.onSurfaceVariant)),
                        const SizedBox(height: 8),
                        Text('Aucune notification', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
                      ]))
                    : RefreshIndicator(
                        onRefresh: _load,
                        child: ListView.separated(
                          padding: const EdgeInsets.fromLTRB(20, 0, 20, 24),
                          itemCount: _notifications.length,
                          separatorBuilder: (_, __) => const SizedBox(height: 8),
                          itemBuilder: (_, i) => _notifCard(_notifications[i]),
                        ),
                      ),
          ),
        ]),
      ),
    );
  }

  Widget _filterChip(String label, String value, {int badge = 0}) {
    final selected = _filter == value;
    return GestureDetector(
      onTap: () { setState(() => _filter = value); _load(); },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 7),
        decoration: BoxDecoration(
          color: selected ? AppColors.primary : AppColors.surfaceContainerLow,
          borderRadius: BorderRadius.circular(20),
        ),
        child: Row(mainAxisSize: MainAxisSize.min, children: [
          Text(label, style: GoogleFonts.manrope(fontSize: 12, fontWeight: FontWeight.w600, color: selected ? Colors.white : AppColors.onSurface)),
          if (badge > 0) ...[
            const SizedBox(width: 6),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
              decoration: BoxDecoration(color: selected ? Colors.white : AppColors.error, borderRadius: BorderRadius.circular(10)),
              child: Text('$badge', style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w700, color: selected ? AppColors.primary : Colors.white)),
            ),
          ],
        ]),
      ),
    );
  }

  Widget _notifCard(Map<String, dynamic> n) {
    final isRead = n['isRead'] == true;
    final priority = n['priority'] ?? '';
    final isCritical = priority == 'Critical';

    return GestureDetector(
      onTap: () async {
        if (!isRead) await ref.read(apiClientProvider).markRead(n['id']);
        _load();
      },
      child: Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(
          color: isRead ? AppColors.surface : AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          border: isCritical ? Border.all(color: AppColors.error.withOpacity(0.2)) : null,
          boxShadow: isRead ? null : [BoxShadow(color: AppColors.primary.withOpacity(0.04), blurRadius: 12)],
        ),
        child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Container(
            width: 4, height: 40,
            decoration: BoxDecoration(
              color: isCritical ? AppColors.error : isRead ? Colors.transparent : AppColors.secondary,
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            if (isCritical) Text('⚠ CRITIQUE', style: GoogleFonts.manrope(fontSize: 9, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.error)),
            Text(n['title'] ?? '', style: GoogleFonts.manrope(fontSize: 14, fontWeight: isRead ? FontWeight.w400 : FontWeight.w600)),
            const SizedBox(height: 4),
            Text(n['message'] ?? '', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant), maxLines: 2, overflow: TextOverflow.ellipsis),
          ])),
          Text('VOIR', style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary)),
        ]),
      ),
    );
  }
}
