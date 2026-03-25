import 'package:flutter_test/flutter_test.dart';
import 'package:couture_app/core/helpers/date_helper.dart';

void main() {
  group('DateHelper', () {
    group('formatDate', () {
      test('formats ISO date to dd/MM/yyyy', () {
        expect(DateHelper.formatDate('2026-03-25'), '25/03/2026');
        expect(DateHelper.formatDate('2026-01-05'), '05/01/2026');
      });

      test('handles ISO datetime', () {
        expect(DateHelper.formatDate('2026-03-25T14:30:00Z'), '25/03/2026');
      });

      test('returns empty for null', () {
        expect(DateHelper.formatDate(null), '');
      });

      test('returns raw string for invalid date', () {
        expect(DateHelper.formatDate('not-a-date'), 'not-a-date');
      });
    });

    group('relativeTime', () {
      test('returns empty for null', () {
        expect(DateHelper.relativeTime(null), '');
      });

      test('returns "hier" for yesterday', () {
        final yesterday = DateTime.now().subtract(const Duration(hours: 25)).toIso8601String();
        expect(DateHelper.relativeTime(yesterday), 'hier');
      });

      test('returns hours for today', () {
        final hoursAgo = DateTime.now().subtract(const Duration(hours: 3)).toIso8601String();
        expect(DateHelper.relativeTime(hoursAgo), 'il y a 3h');
      });

      test('returns days for this week', () {
        final daysAgo = DateTime.now().subtract(const Duration(days: 4)).toIso8601String();
        expect(DateHelper.relativeTime(daysAgo), 'il y a 4j');
      });

      test('returns minutes for recent', () {
        final minutesAgo = DateTime.now().subtract(const Duration(minutes: 15)).toIso8601String();
        expect(DateHelper.relativeTime(minutesAgo), 'il y a 15min');
      });
    });

    group('toIsoDate', () {
      test('formats DateTime to ISO date string', () {
        expect(DateHelper.toIsoDate(DateTime(2026, 3, 25)), '2026-03-25');
        expect(DateHelper.toIsoDate(DateTime(2026, 1, 5)), '2026-01-05');
      });
    });

    group('minDeliveryDays', () {
      test('Simple requires 1 day', () {
        expect(DateHelper.minDeliveryDays('Simple'), 1);
      });

      test('Brode requires 3 days', () {
        expect(DateHelper.minDeliveryDays('Brode'), 3);
      });

      test('Perle requires 5 days', () {
        expect(DateHelper.minDeliveryDays('Perle'), 5);
      });

      test('Mixte requires 7 days', () {
        expect(DateHelper.minDeliveryDays('Mixte'), 7);
      });
    });
  });
}
