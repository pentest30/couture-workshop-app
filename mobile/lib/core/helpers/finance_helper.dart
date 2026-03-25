class FinanceHelper {
  static double calculateOutstandingBalance(double totalPrice, List<dynamic> payments) {
    final totalPaid = payments.fold<double>(0, (sum, p) => sum + ((p['amount'] as num?)?.toDouble() ?? 0));
    return totalPrice - totalPaid;
  }

  static bool isFullyPaid(double totalPrice, List<dynamic> payments) {
    return calculateOutstandingBalance(totalPrice, payments) <= 0;
  }

  static String formatDZD(double amount) {
    if (amount >= 1000000) {
      return '${(amount / 1000000).toStringAsFixed(1)}M DZD';
    }
    if (amount >= 1000) {
      return '${(amount / 1000).toStringAsFixed(amount % 1000 == 0 ? 0 : 1)}K DZD';
    }
    return '${amount.toStringAsFixed(0)} DZD';
  }

  static String formatAmount(double amount) {
    if (amount == amount.roundToDouble()) {
      return amount.toStringAsFixed(0);
    }
    return amount.toStringAsFixed(2);
  }

  static bool isValidPayment(double amount, double outstandingBalance) {
    return amount > 0 && amount <= outstandingBalance;
  }

  static const paymentMethods = {
    'Especes': 'Espèces',
    'Virement': 'Virement',
    'Ccp': 'CCP',
    'BaridiMob': 'BaridiMob',
    'Dahabia': 'Dahabia',
  };
}
