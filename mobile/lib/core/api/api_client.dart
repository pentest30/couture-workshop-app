import 'package:dio/dio.dart';

class ApiClient {
  static const baseUrl = 'http://10.0.2.2:5050'; // Android emulator -> localhost

  late final Dio dio;
  String? _token;

  ApiClient() {
    dio = Dio(BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 10),
      headers: {'Content-Type': 'application/json'},
    ));

    dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) {
        if (_token != null) {
          options.headers['Authorization'] = 'Bearer $_token';
        }
        handler.next(options);
      },
      onError: (error, handler) {
        if (error.response?.statusCode == 401) {
          _token = null;
        }
        handler.next(error);
      },
    ));
  }

  void setToken(String token) => _token = token;
  void clearToken() => _token = null;
  bool get isAuthenticated => _token != null;

  // Auth
  Future<Map<String, dynamic>> login(String email, String password) async {
    final response = await dio.post('/api/auth/login', data: {'email': email, 'password': password});
    final data = response.data as Map<String, dynamic>;
    setToken(data['accessToken']);
    return data;
  }

  // Orders
  Future<Map<String, dynamic>> getOrders({int page = 1, String? status, String? search, bool? lateOnly}) async {
    final params = <String, dynamic>{'page': page, 'pageSize': 20};
    if (status != null) params['status'] = status;
    if (search != null) params['search'] = search;
    if (lateOnly == true) params['lateOnly'] = true;
    final response = await dio.get('/api/orders', queryParameters: params);
    return response.data;
  }

  Future<Map<String, dynamic>> getOrder(String id) async {
    final response = await dio.get('/api/orders/$id');
    return response.data;
  }

  Future<Map<String, dynamic>> changeStatus(String orderId, String newStatus, {String? reason, String? embroidererId, String? beaderId, String? actualDeliveryDate}) async {
    final response = await dio.post('/api/orders/$orderId/status', data: {
      'newStatus': newStatus,
      if (reason != null) 'reason': reason,
      if (embroidererId != null) 'assignedEmbroidererId': embroidererId,
      if (beaderId != null) 'assignedBeaderId': beaderId,
      if (actualDeliveryDate != null) 'actualDeliveryDate': actualDeliveryDate,
    });
    return response.data;
  }

  Future<Map<String, dynamic>> createOrder(Map<String, dynamic> data) async {
    final response = await dio.post('/api/orders', data: data);
    return response.data;
  }

  // Clients
  Future<List<dynamic>> searchClients(String query) async {
    final response = await dio.get('/api/clients/search', queryParameters: {'q': query});
    return response.data;
  }

  Future<Map<String, dynamic>> getClient(String id) async {
    final response = await dio.get('/api/clients/$id');
    return response.data;
  }

  Future<Map<String, dynamic>> createClient(Map<String, dynamic> data) async {
    final response = await dio.post('/api/clients', data: data);
    return response.data;
  }

  // Dashboard
  Future<Map<String, dynamic>> getKPIs(int year, int quarter) async {
    final response = await dio.get('/api/dashboard/kpis', queryParameters: {'year': year, 'quarter': quarter});
    return response.data;
  }

  // Notifications
  Future<Map<String, dynamic>> getNotifications({String filter = 'all', int page = 1}) async {
    final response = await dio.get('/api/notifications', queryParameters: {'filter': filter, 'page': page});
    return response.data;
  }

  Future<int> getUnreadCount() async {
    final response = await dio.get('/api/notifications/unread-count');
    return response.data['count'];
  }

  Future<void> markRead(String id) async {
    await dio.post('/api/notifications/$id/read');
  }

  Future<void> markAllRead() async {
    await dio.post('/api/notifications/read-all');
  }

  // Finance
  Future<Map<String, dynamic>> recordPayment(String orderId, Map<String, dynamic> data) async {
    final response = await dio.post('/api/orders/$orderId/payments', data: data);
    return response.data;
  }

  Future<List<dynamic>> getPayments(String orderId) async {
    final response = await dio.get('/api/orders/$orderId/payments');
    return response.data;
  }
}
