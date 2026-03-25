class StatusHelper {
  static const statusLabels = {
    'Recue': 'Reçue',
    'EnAttente': 'En Attente',
    'EnCours': 'En Cours',
    'Broderie': 'En Broderie',
    'Perlage': 'En Perlage',
    'Retouche': 'En Retouche',
    'Prete': 'Prête',
    'Livree': 'Livrée',
  };

  static String label(String status) => statusLabels[status] ?? status;

  static bool isTerminal(String status) => status == 'Livree';

  static bool isLate(String status, String? expectedDeliveryDate) {
    if (isTerminal(status) || expectedDeliveryDate == null) return false;
    try {
      final expected = DateTime.parse(expectedDeliveryDate);
      return DateTime.now().isAfter(expected);
    } catch (_) {
      return false;
    }
  }

  static int delayDays(String? expectedDeliveryDate) {
    if (expectedDeliveryDate == null) return 0;
    try {
      final expected = DateTime.parse(expectedDeliveryDate);
      final now = DateTime.now();
      if (now.isAfter(expected)) {
        return now.difference(expected).inDays;
      }
      return 0;
    } catch (_) {
      return 0;
    }
  }

  /// Returns list of valid next statuses given current status and work type
  static List<String> validTransitions(String currentStatus, String workType) {
    switch (currentStatus) {
      case 'Recue':
        return ['EnAttente', 'EnCours'];
      case 'EnAttente':
        return ['EnCours'];
      case 'EnCours':
        final list = <String>[];
        if (workType == 'Brode' || workType == 'Mixte') list.add('Broderie');
        if (workType == 'Perle' || workType == 'Mixte') list.add('Perlage');
        list.addAll(['Retouche', 'Prete']);
        return list;
      case 'Broderie':
        final list = <String>[];
        if (workType == 'Mixte') list.add('Perlage');
        list.addAll(['Retouche', 'Prete']);
        return list;
      case 'Perlage':
        return ['Retouche', 'Prete'];
      case 'Retouche':
        final list = <String>['EnCours'];
        if (workType == 'Brode' || workType == 'Mixte') list.add('Broderie');
        if (workType == 'Perle' || workType == 'Mixte') list.add('Perlage');
        list.add('Prete');
        return list;
      case 'Prete':
        return ['Livree'];
      case 'Livree':
        return [];
      default:
        return [];
    }
  }

  static bool requiresReason(String targetStatus) => targetStatus == 'Retouche';

  static bool requiresTailor(String targetStatus) => targetStatus == 'EnCours';

  static bool requiresEmbroiderer(String targetStatus) => targetStatus == 'Broderie';

  static bool requiresBeader(String targetStatus) => targetStatus == 'Perlage';
}
