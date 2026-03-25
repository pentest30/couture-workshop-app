import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../api/api_client.dart';
import '../api/signalr_service.dart';

final apiClientProvider = Provider<ApiClient>((ref) => ApiClient());

final authStateProvider = StateProvider<Map<String, dynamic>?>((ref) => null);

final signalRServiceProvider = Provider<SignalRService>((ref) {
  final service = SignalRService();
  ref.onDispose(() => service.dispose());
  return service;
});

/// Global unread notification count, updated by SignalR pushes.
final unreadCountProvider = StateProvider<int>((ref) => 0);
