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
  String? _error;
  String _filter = 'all';
  bool _markingAllRead = false;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() { _loading = true; _error = null; });
    try {
      final api = ref.read(apiClientProvider);
      final results = await Future.wait([
        api.getNotifications(filter: _filter),
        api.getUnreadCount(),
      ]);
      final data = results[0] as Map<String, dynamic>;
      final unread = results[1] as int;
      if (mounted) {
        setState(() {
          _notifications = data['items'] ?? [];
          _unreadCount = unread;
          _loading = false;
        });
      }
    } catch (e) {
      if (mounted) setState(() { _loading = false; _error = 'Impossible de charger les notifications: $e'; });
    }
  }

  Future<void> _markAllRead() async {
    setState(() => _markingAllRead = true);
    try {
      await ref.read(apiClientProvider).markAllRead();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Toutes les notifications marquees comme lues'), duration: Duration(seconds: 2)),
        );
      }
      await _load();
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Erreur: $e')),
        );
      }
    }
    if (mounted) setState(() => _markingAllRead = false);
  }

  Future<void> _markRead(Map<String, dynamic> n) async {
    final id = n['id'];
    if (id == null) return;
    try {
      await ref.read(apiClientProvider).markRead(id.toString());
      // Update locally for instant feedback
      if (mounted) {
        setState(() {
          n['isRead'] = true;
          _unreadCount = (_unreadCount - 1).clamp(0, _unreadCount);
        });
      }
    } catch (_) {}
  }

  void _onFilterChanged(String value) {
    if (_filter == value) return;
    setState(() => _filter = value);
    _load();
  }

  String _formatTimestamp(String? timestamp) {
    if (timestamp == null) return '';
    try {
      final date = DateTime.parse(timestamp);
      final now = DateTime.now();
      final diff = now.difference(date);
      if (diff.inMinutes < 1) return 'a l\'instant';
      if (diff.inMinutes < 60) return 'il y a ${diff.inMinutes}min';
      if (diff.inHours < 24) return 'il y a ${diff.inHours}h';
      if (diff.inDays == 1) return 'hier';
      if (diff.inDays < 7) return 'il y a ${diff.inDays}j';
      return '${date.day.toString().padLeft(2, '0')}/${date.month.toString().padLeft(2, '0')}/${date.year}';
    } catch (_) {
      return '';
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
              Row(children: [
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Text('Notifications', style: GoogleFonts.notoSerif(fontSize: 24, fontWeight: FontWeight.w600)),
                  Text('CENTRE DE GESTION', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
                ])),
                TextButton(
                  onPressed: _markingAllRead || _loading ? null : _markAllRead,
                  child: _markingAllRead
                      ? const SizedBox(width: 16, height: 16, child: CircularProgressIndicator(strokeWidth: 2, color: AppColors.secondary))
                      : Text('Tout marquer lu', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.secondary, fontWeight: FontWeight.w600)),
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
                : _error != null
                    ? Center(child: Padding(
                        padding: const EdgeInsets.all(32),
                        child: Column(mainAxisSize: MainAxisSize.min, children: [
                          const Icon(Icons.cloud_off, size: 48, color: AppColors.onSurfaceVariant),
                          const SizedBox(height: 16),
                          Text(_error!, style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant), textAlign: TextAlign.center),
                          const SizedBox(height: 16),
                          OutlinedButton(onPressed: _load, child: const Text('Reessayer')),
                        ]),
                      ))
                    : _notifications.isEmpty
                        ? Center(child: Padding(
                            padding: const EdgeInsets.all(40),
                            child: Column(mainAxisSize: MainAxisSize.min, children: [
                              const Icon(Icons.notifications_none, size: 48, color: AppColors.onSurfaceVariant),
                              const SizedBox(height: 16),
                              Text(
                                '"La precision est l\'ame de la couture."',
                                style: GoogleFonts.notoSerif(fontSize: 16, fontStyle: FontStyle.italic, color: AppColors.onSurfaceVariant),
                                textAlign: TextAlign.center,
                              ),
                              const SizedBox(height: 8),
                              Text('Aucune notification', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
                            ]),
                          ))
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
      onTap: () => _onFilterChanged(value),
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
    final isHigh = priority == 'High';
    final timestamp = _formatTimestamp(n['createdAt'] as String?);

    // Priority bar color
    Color barColor;
    if (isCritical) {
      barColor = AppColors.error;
    } else if (isHigh) {
      barColor = AppColors.secondaryFixed;
    } else if (!isRead) {
      barColor = AppColors.primary.withOpacity(0.4);
    } else {
      barColor = AppColors.onSurfaceVariant.withOpacity(0.2);
    }

    return GestureDetector(
      onTap: () async {
        if (!isRead) await _markRead(n);
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
          // Priority indicator bar
          Container(
            width: 4, height: 52,
            decoration: BoxDecoration(
              color: barColor,
              borderRadius: BorderRadius.circular(2),
            ),
          ),
          const SizedBox(width: 12),
          Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            // Priority label + timestamp row
            Row(children: [
              if (isCritical)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
                  margin: const EdgeInsets.only(right: 8),
                  decoration: BoxDecoration(color: AppColors.error.withOpacity(0.1), borderRadius: BorderRadius.circular(4)),
                  child: Text('CRITIQUE', style: GoogleFonts.manrope(fontSize: 9, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.error)),
                ),
              if (isHigh)
                Container(
                  padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
                  margin: const EdgeInsets.only(right: 8),
                  decoration: BoxDecoration(color: AppColors.secondaryFixed.withOpacity(0.2), borderRadius: BorderRadius.circular(4)),
                  child: Text('IMPORTANT', style: GoogleFonts.manrope(fontSize: 9, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary)),
                ),
              const Spacer(),
              if (timestamp.isNotEmpty)
                Text(timestamp, style: GoogleFonts.manrope(fontSize: 10, color: AppColors.onSurfaceVariant)),
            ]),
            const SizedBox(height: 4),
            // Title
            Text(n['title'] ?? '', style: GoogleFonts.manrope(fontSize: 14, fontWeight: isRead ? FontWeight.w400 : FontWeight.w600)),
            const SizedBox(height: 4),
            // Message
            Text(n['message'] ?? '', style: GoogleFonts.manrope(fontSize: 12, color: AppColors.onSurfaceVariant), maxLines: 2, overflow: TextOverflow.ellipsis),
            // Unread dot
            if (!isRead) ...[
              const SizedBox(height: 4),
              Row(children: [
                Container(width: 6, height: 6, decoration: const BoxDecoration(color: AppColors.primary, shape: BoxShape.circle)),
                const SizedBox(width: 4),
                Text('Non lue', style: GoogleFonts.manrope(fontSize: 10, color: AppColors.primary, fontWeight: FontWeight.w500)),
              ]),
            ],
          ])),
          const SizedBox(width: 8),
          // VOIR button
          if (n['orderId'] != null)
            GestureDetector(
              onTap: () async {
                if (!isRead) await _markRead(n);
                if (mounted) context.push('/orders/${n['orderId']}');
              },
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
                decoration: BoxDecoration(color: AppColors.secondary.withOpacity(0.08), borderRadius: BorderRadius.circular(8)),
                child: Text('VOIR', style: GoogleFonts.manrope(fontSize: 10, fontWeight: FontWeight.w700, letterSpacing: 1, color: AppColors.secondary)),
              ),
            ),
        ]),
      ),
    );
  }
}
