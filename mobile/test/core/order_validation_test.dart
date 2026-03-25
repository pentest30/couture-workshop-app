import 'package:flutter_test/flutter_test.dart';
import 'package:couture_app/core/helpers/order_validation.dart';

void main() {
  group('OrderValidation', () {
    group('validateStep1', () {
      test('returns error when no client selected', () {
        expect(OrderValidation.validateStep1(null), isNotNull);
      });

      test('returns null when client selected', () {
        expect(OrderValidation.validateStep1({'id': '123', 'firstName': 'Sara'}), isNull);
      });
    });

    group('validateStep2', () {
      test('returns error when work type empty', () {
        expect(OrderValidation.validateStep2(''), isNotNull);
      });

      test('returns null when work type selected', () {
        expect(OrderValidation.validateStep2('Brode'), isNull);
      });
    });

    group('validateStep3', () {
      test('returns error when no delivery date', () {
        expect(OrderValidation.validateStep3(
          deliveryDate: null, priceText: '10000', depositText: '0', workType: 'Simple',
        ), isNotNull);
      });

      test('returns error when price is 0', () {
        expect(OrderValidation.validateStep3(
          deliveryDate: DateTime(2026, 4, 1), priceText: '0', depositText: '0', workType: 'Simple',
        ), isNotNull);
      });

      test('returns error when price is empty', () {
        expect(OrderValidation.validateStep3(
          deliveryDate: DateTime(2026, 4, 1), priceText: '', depositText: '0', workType: 'Simple',
        ), isNotNull);
      });

      test('returns error when deposit exceeds price', () {
        expect(OrderValidation.validateStep3(
          deliveryDate: DateTime(2026, 4, 1), priceText: '10000', depositText: '15000', workType: 'Simple',
        ), contains('dépasser'));
      });

      test('returns null for valid input', () {
        expect(OrderValidation.validateStep3(
          deliveryDate: DateTime(2026, 4, 1), priceText: '15000', depositText: '5000', workType: 'Brode',
        ), isNull);
      });

      test('deposit defaults to 0 when empty', () {
        expect(OrderValidation.validateStep3(
          deliveryDate: DateTime(2026, 4, 1), priceText: '15000', depositText: '', workType: 'Simple',
        ), isNull);
      });
    });

    group('validateClientForm', () {
      test('valid Algerian phone number', () {
        expect(OrderValidation.validateClientForm(
          firstName: 'Sara', lastName: 'Benali', phone: '0550123456',
        ), isNull);
      });

      test('rejects empty first name', () {
        expect(OrderValidation.validateClientForm(
          firstName: '', lastName: 'Benali', phone: '0550123456',
        ), isNotNull);
      });

      test('rejects invalid phone format', () {
        expect(OrderValidation.validateClientForm(
          firstName: 'Sara', lastName: 'Benali', phone: '123456',
        ), contains('invalide'));
      });

      test('accepts 06 and 07 prefixes', () {
        expect(OrderValidation.validateClientForm(
          firstName: 'Sara', lastName: 'Benali', phone: '0661234567',
        ), isNull);
        expect(OrderValidation.validateClientForm(
          firstName: 'Sara', lastName: 'Benali', phone: '0770123456',
        ), isNull);
      });

      test('rejects 08 prefix', () {
        expect(OrderValidation.validateClientForm(
          firstName: 'Sara', lastName: 'Benali', phone: '0812345678',
        ), contains('invalide'));
      });
    });

    group('workType helpers', () {
      test('embroidery fields required for Brode and Mixte', () {
        expect(OrderValidation.requiresEmbroideryFields('Brode'), true);
        expect(OrderValidation.requiresEmbroideryFields('Mixte'), true);
        expect(OrderValidation.requiresEmbroideryFields('Simple'), false);
        expect(OrderValidation.requiresEmbroideryFields('Perle'), false);
      });

      test('beading fields required for Perle and Mixte', () {
        expect(OrderValidation.requiresBeadingFields('Perle'), true);
        expect(OrderValidation.requiresBeadingFields('Mixte'), true);
        expect(OrderValidation.requiresBeadingFields('Simple'), false);
        expect(OrderValidation.requiresBeadingFields('Brode'), false);
      });
    });
  });
}
