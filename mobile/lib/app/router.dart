import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../features/auth/login_screen.dart';
import '../features/dashboard/dashboard_screen.dart';
import '../features/orders/orders_list_screen.dart';
import '../features/orders/order_detail_screen.dart';
import '../features/new_order/new_order_screen.dart';
import '../features/notifications/notifications_screen.dart';
import '../features/notifications/notification_config_screen.dart';
import '../features/catalog/catalog_screen.dart';
import '../features/catalog/catalog_detail_screen.dart';
import '../features/clients/client_profile_screen.dart';
import '../features/clients/clients_list_screen.dart';
import '../features/clients/measurements_screen.dart';
import '../features/clients/create_client_screen.dart';
import 'shell_screen.dart';

final router = GoRouter(
  initialLocation: '/login',
  routes: [
    GoRoute(path: '/login', builder: (_, __) => const LoginScreen()),
    ShellRoute(
      builder: (_, state, child) => ShellScreen(child: child),
      routes: [
        GoRoute(path: '/', builder: (_, __) => const DashboardScreen()),
        GoRoute(
          path: '/orders',
          builder: (_, __) => const OrdersListScreen(),
          routes: [
            GoRoute(path: ':id', builder: (_, state) => OrderDetailScreen(orderId: state.pathParameters['id']!)),
          ],
        ),
        GoRoute(path: '/new-order', builder: (_, __) => const NewOrderScreen()),
        GoRoute(
          path: '/catalog',
          builder: (_, __) => const CatalogScreen(),
          routes: [
            GoRoute(path: ':id', builder: (_, state) => CatalogDetailScreen(modelId: state.pathParameters['id']!)),
          ],
        ),
        GoRoute(
          path: '/notifications',
          builder: (_, __) => const NotificationsScreen(),
          routes: [
            GoRoute(path: 'config', builder: (_, __) => const NotificationConfigScreen()),
          ],
        ),
        GoRoute(
          path: '/clients',
          builder: (_, __) => const ClientsListScreen(),
          routes: [
            GoRoute(path: 'new', builder: (_, __) => const CreateClientScreen()),
            GoRoute(
              path: ':id',
              builder: (_, state) => ClientProfileScreen(clientId: state.pathParameters['id']!),
              routes: [
                GoRoute(path: 'measurements', builder: (_, state) => MeasurementsScreen(clientId: state.pathParameters['id']!, clientName: state.uri.queryParameters['name'] ?? '', currentMeasurements: const [])),
              ],
            ),
          ],
        ),
      ],
    ),
  ],
);
