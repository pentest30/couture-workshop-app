class OrderValidation {
  static String? validateStep1(Map<String, dynamic>? selectedClient) {
    if (selectedClient == null) return 'Veuillez sélectionner un client';
    return null;
  }

  static String? validateStep2(String workType) {
    if (workType.isEmpty) return 'Veuillez sélectionner un type de travail';
    return null;
  }

  static String? validateStep3({
    required DateTime? deliveryDate,
    required String? priceText,
    required String? depositText,
    required String workType,
  }) {
    if (deliveryDate == null) return 'Veuillez sélectionner une date de livraison';

    final price = double.tryParse(priceText ?? '');
    if (price == null || price <= 0) return 'Le prix doit être supérieur à 0';

    final deposit = double.tryParse(depositText ?? '') ?? 0;
    if (deposit < 0) return "L'acompte ne peut pas être négatif";
    if (deposit > price) return "L'acompte ne peut pas dépasser le prix total";

    return null;
  }

  static String? validateClientForm({
    required String firstName,
    required String lastName,
    required String phone,
  }) {
    if (firstName.trim().isEmpty) return 'Le prénom est obligatoire';
    if (lastName.trim().isEmpty) return 'Le nom est obligatoire';
    if (phone.trim().isEmpty) return 'Le téléphone est obligatoire';
    if (!RegExp(r'^0[567]\d{8}$').hasMatch(phone.trim())) {
      return 'Format téléphone invalide (ex: 0550123456)';
    }
    return null;
  }

  static const workTypes = ['Simple', 'Brode', 'Perle', 'Mixte'];

  static const workTypeLabels = {
    'Simple': 'Simple',
    'Brode': 'Brodé',
    'Perle': 'Perlé',
    'Mixte': 'Mixte',
  };

  static bool requiresEmbroideryFields(String workType) => workType == 'Brode' || workType == 'Mixte';
  static bool requiresBeadingFields(String workType) => workType == 'Perle' || workType == 'Mixte';
}
