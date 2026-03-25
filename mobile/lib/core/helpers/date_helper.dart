class DateHelper {
  static String formatDate(String? isoDate) {
    if (isoDate == null || isoDate.isEmpty) return '';
    try {
      final date = DateTime.parse(isoDate);
      return '${date.day.toString().padLeft(2, '0')}/${date.month.toString().padLeft(2, '0')}/${date.year}';
    } catch (_) {
      return isoDate;
    }
  }

  static String relativeTime(String? isoTimestamp) {
    if (isoTimestamp == null || isoTimestamp.isEmpty) return '';
    try {
      final date = DateTime.parse(isoTimestamp);
      final now = DateTime.now();
      final diff = now.difference(date);

      if (diff.inMinutes < 1) return "à l'instant";
      if (diff.inMinutes < 60) return 'il y a ${diff.inMinutes}min';
      if (diff.inHours < 24) return 'il y a ${diff.inHours}h';
      if (diff.inDays == 1) return 'hier';
      if (diff.inDays < 7) return 'il y a ${diff.inDays}j';
      if (diff.inDays < 30) return 'il y a ${diff.inDays ~/ 7}sem';
      return formatDate(isoTimestamp);
    } catch (_) {
      return '';
    }
  }

  static String toIsoDate(DateTime date) {
    return '${date.year}-${date.month.toString().padLeft(2, '0')}-${date.day.toString().padLeft(2, '0')}';
  }

  static bool isValidDeliveryDate(DateTime date, String workType) {
    final minDays = _minDeliveryDays(workType);
    final earliest = DateTime.now().add(Duration(days: minDays));
    return date.isAfter(earliest) || date.isAtSameMomentAs(earliest);
  }

  static int _minDeliveryDays(String workType) {
    switch (workType) {
      case 'Simple': return 1;
      case 'Brode': return 3;
      case 'Perle': return 5;
      case 'Mixte': return 7;
      default: return 1;
    }
  }

  static int minDeliveryDays(String workType) => _minDeliveryDays(workType);
}
