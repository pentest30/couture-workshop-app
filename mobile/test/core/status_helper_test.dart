import 'package:flutter_test/flutter_test.dart';
import 'package:couture_app/core/helpers/status_helper.dart';

void main() {
  group('StatusHelper', () {
    group('label', () {
      test('returns French label for known status', () {
        expect(StatusHelper.label('Recue'), 'Reçue');
        expect(StatusHelper.label('EnCours'), 'En Cours');
        expect(StatusHelper.label('Broderie'), 'En Broderie');
        expect(StatusHelper.label('Livree'), 'Livrée');
      });

      test('returns raw status for unknown status', () {
        expect(StatusHelper.label('Unknown'), 'Unknown');
      });
    });

    group('isTerminal', () {
      test('Livree is terminal', () {
        expect(StatusHelper.isTerminal('Livree'), true);
      });

      test('other statuses are not terminal', () {
        for (final s in ['Recue', 'EnCours', 'Broderie', 'Prete']) {
          expect(StatusHelper.isTerminal(s), false);
        }
      });
    });

    group('isLate', () {
      test('returns false for terminal status', () {
        expect(StatusHelper.isLate('Livree', '2020-01-01'), false);
      });

      test('returns true when past delivery date', () {
        expect(StatusHelper.isLate('EnCours', '2020-01-01'), true);
      });

      test('returns false when delivery date is in the future', () {
        final future = DateTime.now().add(const Duration(days: 30)).toIso8601String();
        expect(StatusHelper.isLate('EnCours', future), false);
      });

      test('returns false for null date', () {
        expect(StatusHelper.isLate('EnCours', null), false);
      });
    });

    group('delayDays', () {
      test('returns 0 for future date', () {
        final future = DateTime.now().add(const Duration(days: 5)).toIso8601String();
        expect(StatusHelper.delayDays(future), 0);
      });

      test('returns positive days for past date', () {
        final past = DateTime.now().subtract(const Duration(days: 3)).toIso8601String();
        expect(StatusHelper.delayDays(past), 3);
      });

      test('returns 0 for null', () {
        expect(StatusHelper.delayDays(null), 0);
      });
    });

    group('validTransitions', () {
      test('Recue can go to EnAttente or EnCours', () {
        expect(StatusHelper.validTransitions('Recue', 'Simple'), ['EnAttente', 'EnCours']);
      });

      test('EnCours for Simple cannot go to Broderie', () {
        final transitions = StatusHelper.validTransitions('EnCours', 'Simple');
        expect(transitions, isNot(contains('Broderie')));
        expect(transitions, contains('Retouche'));
        expect(transitions, contains('Prete'));
      });

      test('EnCours for Brode can go to Broderie', () {
        final transitions = StatusHelper.validTransitions('EnCours', 'Brode');
        expect(transitions, contains('Broderie'));
        expect(transitions, isNot(contains('Perlage')));
      });

      test('EnCours for Mixte can go to both Broderie and Perlage', () {
        final transitions = StatusHelper.validTransitions('EnCours', 'Mixte');
        expect(transitions, contains('Broderie'));
        expect(transitions, contains('Perlage'));
      });

      test('Broderie for Mixte can go to Perlage', () {
        final transitions = StatusHelper.validTransitions('Broderie', 'Mixte');
        expect(transitions, contains('Perlage'));
      });

      test('Broderie for Brode cannot go to Perlage', () {
        final transitions = StatusHelper.validTransitions('Broderie', 'Brode');
        expect(transitions, isNot(contains('Perlage')));
      });

      test('Prete can only go to Livree', () {
        expect(StatusHelper.validTransitions('Prete', 'Simple'), ['Livree']);
      });

      test('Livree has no transitions', () {
        expect(StatusHelper.validTransitions('Livree', 'Simple'), isEmpty);
      });

      test('Retouche can go back to EnCours and Prete', () {
        final transitions = StatusHelper.validTransitions('Retouche', 'Simple');
        expect(transitions, contains('EnCours'));
        expect(transitions, contains('Prete'));
      });
    });

    group('requirements', () {
      test('Retouche requires reason', () {
        expect(StatusHelper.requiresReason('Retouche'), true);
        expect(StatusHelper.requiresReason('EnCours'), false);
      });

      test('EnCours requires tailor', () {
        expect(StatusHelper.requiresTailor('EnCours'), true);
        expect(StatusHelper.requiresTailor('Prete'), false);
      });

      test('Broderie requires embroiderer', () {
        expect(StatusHelper.requiresEmbroiderer('Broderie'), true);
        expect(StatusHelper.requiresEmbroiderer('Perlage'), false);
      });

      test('Perlage requires beader', () {
        expect(StatusHelper.requiresBeader('Perlage'), true);
        expect(StatusHelper.requiresBeader('Broderie'), false);
      });
    });
  });
}
