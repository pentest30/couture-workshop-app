import 'package:flutter_test/flutter_test.dart';
import 'package:couture_app/core/helpers/finance_helper.dart';

void main() {
  group('FinanceHelper', () {
    group('calculateOutstandingBalance', () {
      test('full price when no payments', () {
        expect(FinanceHelper.calculateOutstandingBalance(20000, []), 20000);
      });

      test('subtracts single payment', () {
        expect(FinanceHelper.calculateOutstandingBalance(20000, [{'amount': 5000}]), 15000);
      });

      test('subtracts multiple payments', () {
        final payments = [{'amount': 5000}, {'amount': 3000}, {'amount': 2000}];
        expect(FinanceHelper.calculateOutstandingBalance(20000, payments), 10000);
      });

      test('zero when fully paid', () {
        expect(FinanceHelper.calculateOutstandingBalance(10000, [{'amount': 10000}]), 0);
      });

      test('handles null amount in payment', () {
        expect(FinanceHelper.calculateOutstandingBalance(10000, [{'amount': null}]), 10000);
      });
    });

    group('isFullyPaid', () {
      test('true when balance is zero', () {
        expect(FinanceHelper.isFullyPaid(10000, [{'amount': 10000}]), true);
      });

      test('false when balance remains', () {
        expect(FinanceHelper.isFullyPaid(10000, [{'amount': 5000}]), false);
      });
    });

    group('formatDZD', () {
      test('formats thousands with K', () {
        expect(FinanceHelper.formatDZD(5000), '5K DZD');
        expect(FinanceHelper.formatDZD(25000), '25K DZD');
      });

      test('formats millions with M', () {
        expect(FinanceHelper.formatDZD(1500000), '1.5M DZD');
      });

      test('formats small amounts without suffix', () {
        expect(FinanceHelper.formatDZD(500), '500 DZD');
        expect(FinanceHelper.formatDZD(0), '0 DZD');
      });

      test('formats non-round thousands with decimal', () {
        expect(FinanceHelper.formatDZD(5500), '5.5K DZD');
      });
    });

    group('isValidPayment', () {
      test('valid when positive and within balance', () {
        expect(FinanceHelper.isValidPayment(5000, 10000), true);
      });

      test('invalid when zero', () {
        expect(FinanceHelper.isValidPayment(0, 10000), false);
      });

      test('invalid when negative', () {
        expect(FinanceHelper.isValidPayment(-100, 10000), false);
      });

      test('invalid when exceeds balance', () {
        expect(FinanceHelper.isValidPayment(15000, 10000), false);
      });

      test('valid when equals balance (full settlement)', () {
        expect(FinanceHelper.isValidPayment(10000, 10000), true);
      });
    });
  });
}
