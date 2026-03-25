import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import '../features/auth/login_screen.dart';
import '../features/dashboard/dashboard_screen.dart';
import '../features/orders/orders_list_screen.dart';
import '../features/orders/order_detail_screen.dart';
import '../features/new_order/new_order_screen.dart';
import '../features/notifications/notifications_screen.dart';
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
        GoRoute(path: '/orders', builder: (_, __) => const OrdersListScreen()),
        GoRoute(path: '/orders/:id', builder: (_, state) => OrderDetailScreen(orderId: state.pathParameters['id']!)),
        GoRoute(path: '/new-order', builder: (_, __) => const NewOrderScreen()),
        GoRoute(path: '/notifications', builder: (_, __) => const NotificationsScreen()),
        GoRoute(path: '/clients', builder: (_, __) => const ClientsListScreen()),
        GoRoute(path: '/clients/new', builder: (_, __) => const CreateClientScreen()),
        GoRoute(path: '/clients/:id', builder: (_, state) => ClientProfileScreen(clientId: state.pathParameters['id']!)),
        GoRoute(path: '/clients/:id/measurements', builder: (_, state) => MeasurementsScreen(clientId: state.pathParameters['id']!, clientName: state.uri.queryParameters['name'] ?? '', currentMeasurements: const [])),
      ],
    ),
  ],
);
