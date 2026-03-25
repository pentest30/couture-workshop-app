import 'dart:async';
import 'package:signalr_netcore/signalr_client.dart';
import 'api_client.dart';

/// Manages the SignalR connection to /hubs/notifications.
/// Exposes a stream of incoming notification payloads.
class SignalRService {
  HubConnection? _hub;
  final _controller = StreamController<Map<String, dynamic>>.broadcast();
  bool _disposed = false;

  /// Stream of new notification payloads pushed by the server.
  Stream<Map<String, dynamic>> get onNotificationReceived => _controller.stream;

  bool get isConnected =>
      _hub?.state == HubConnectionState.Connected;

  /// Connect to the SignalR hub with the given JWT token.
  Future<void> connect(String token) async {
    if (_disposed) return;
    await disconnect();

    final hubUrl = '${ApiClient.baseUrl}/hubs/notifications';

    _hub = HubConnectionBuilder()
        .withUrl(
          hubUrl,
          options: HttpConnectionOptions(
            accessTokenFactory: () async => token,
            skipNegotiation: true,
            transport: HttpTransportType.WebSockets,
          ),
        )
        .withAutomaticReconnect()
        .build();

    _hub!.on('NotificationReceived', (args) {
      if (args != null && args.isNotEmpty && args[0] is Map) {
        _controller.add(Map<String, dynamic>.from(args[0] as Map));
      }
    });

    _hub!.onclose(({error}) {
      // Connection closed — will auto-reconnect via withAutomaticReconnect
    });

    try {
      await _hub!.start();
    } catch (_) {
      // Silently fail — will retry on next reconnect cycle
    }
  }

  Future<void> disconnect() async {
    if (_hub != null) {
      try {
        await _hub!.stop();
      } catch (_) {}
      _hub = null;
    }
  }

  void dispose() {
    _disposed = true;
    disconnect();
    _controller.close();
  }
}
