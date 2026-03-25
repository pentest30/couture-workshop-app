import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../../core/theme/app_colors.dart';
import '../../core/api/api_client.dart';

class RecordPaymentSheet extends StatefulWidget {
  final String orderId;
  final String orderCode;
  final double outstandingBalance;
  final ApiClient api;

  const RecordPaymentSheet({
    super.key,
    required this.orderId,
    required this.orderCode,
    required this.outstandingBalance,
    required this.api,
  });

  @override
  State<RecordPaymentSheet> createState() => _RecordPaymentSheetState();
}

class _RecordPaymentSheetState extends State<RecordPaymentSheet> {
  final _amountCtrl = TextEditingController();
  final _noteCtrl = TextEditingController();
  String _method = 'Especes';
  bool _saving = false;
  String? _error;

  static const _methods = {
    'Especes': 'Espèces',
    'Virement': 'Virement',
    'Ccp': 'CCP',
    'BaridiMob': 'BaridiMob',
    'Dahabia': 'Dahabia',
  };

  @override
  void dispose() {
    _amountCtrl.dispose();
    _noteCtrl.dispose();
    super.dispose();
  }

  Future<void> _save() async {
    final amount = double.tryParse(_amountCtrl.text);
    if (amount == null || amount <= 0) {
      setState(() => _error = 'Le montant doit être supérieur à 0');
      return;
    }
    if (amount > widget.outstandingBalance) {
      setState(() => _error = 'Le montant dépasse le solde restant (${widget.outstandingBalance.toStringAsFixed(0)} DZD)');
      return;
    }

    setState(() { _saving = true; _error = null; });
    try {
      final now = DateTime.now();
      final dateStr = '${now.year}-${now.month.toString().padLeft(2, '0')}-${now.day.toString().padLeft(2, '0')}';

      final result = await widget.api.recordPayment(widget.orderId, {
        'amount': amount,
        'paymentMethod': _method,
        'paymentDate': dateStr,
        if (_noteCtrl.text.trim().isNotEmpty) 'note': _noteCtrl.text.trim(),
      });

      if (mounted) {
        Navigator.pop(context, result);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Paiement enregistré — Reçu ${result['receiptCode'] ?? ''}'),
            backgroundColor: AppColors.statusPrete,
          ),
        );
      }
    } catch (e) {
      setState(() => _error = e.toString());
    }
    if (mounted) setState(() => _saving = false);
  }

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: const BoxDecoration(
        color: AppColors.background,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      padding: EdgeInsets.fromLTRB(24, 16, 24, MediaQuery.of(context).viewInsets.bottom + 24),
      child: SingleChildScrollView(
      child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.start, children: [
        Center(child: Container(width: 40, height: 4, decoration: BoxDecoration(color: AppColors.outlineVariant, borderRadius: BorderRadius.circular(2)))),
        const SizedBox(height: 20),
        Text('Encaisser un Paiement', style: GoogleFonts.notoSerif(fontSize: 22, fontWeight: FontWeight.w600)),
        const SizedBox(height: 4),
        Text(widget.orderCode, style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
        const SizedBox(height: 8),
        Container(
          padding: const EdgeInsets.all(12),
          decoration: BoxDecoration(color: AppColors.secondary.withAlpha(15), borderRadius: BorderRadius.circular(10)),
          child: Row(children: [
            const Icon(Icons.account_balance_wallet_outlined, color: AppColors.secondary, size: 18),
            const SizedBox(width: 8),
            Text('Solde restant: ', style: GoogleFonts.manrope(fontSize: 13, color: AppColors.onSurfaceVariant)),
            Text('${widget.outstandingBalance.toStringAsFixed(0)} DZD', style: GoogleFonts.notoSerif(fontSize: 16, fontWeight: FontWeight.w700, color: AppColors.secondary)),
          ]),
        ),
        const SizedBox(height: 16),

        // Amount
        TextField(
          controller: _amountCtrl,
          keyboardType: const TextInputType.numberWithOptions(decimal: true),
          style: GoogleFonts.notoSerif(fontSize: 20, fontWeight: FontWeight.w600),
          decoration: InputDecoration(
            labelText: 'Montant (DZD)',
            labelStyle: GoogleFonts.manrope(fontSize: 13),
            prefixIcon: const Icon(Icons.monetization_on_outlined),
          ),
        ),
        const SizedBox(height: 12),

        // Payment method
        Text('MÉTHODE DE PAIEMENT', style: GoogleFonts.manrope(fontSize: 10, letterSpacing: 1.5, color: AppColors.onSurfaceVariant, fontWeight: FontWeight.w600)),
        const SizedBox(height: 8),
        Wrap(spacing: 8, runSpacing: 8, children: _methods.entries.map((e) {
          final selected = _method == e.key;
          return GestureDetector(
            onTap: () => setState(() => _method = e.key),
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
              decoration: BoxDecoration(
                color: selected ? AppColors.primary : AppColors.surfaceContainerLow,
                borderRadius: BorderRadius.circular(20),
              ),
              child: Text(e.value, style: GoogleFonts.manrope(fontSize: 13, fontWeight: FontWeight.w600, color: selected ? Colors.white : AppColors.onSurface)),
            ),
          );
        }).toList()),
        const SizedBox(height: 12),

        // Note
        TextField(
          controller: _noteCtrl,
          decoration: InputDecoration(labelText: 'Note (optionnel)', labelStyle: GoogleFonts.manrope(fontSize: 13)),
        ),

        // Error
        if (_error != null) ...[
          const SizedBox(height: 8),
          Text(_error!, style: GoogleFonts.manrope(fontSize: 13, color: AppColors.error)),
        ],

        const SizedBox(height: 20),
        SizedBox(
          width: double.infinity, height: 52,
          child: ElevatedButton(
            onPressed: _saving ? null : _save,
            style: ElevatedButton.styleFrom(backgroundColor: AppColors.statusPrete, foregroundColor: Colors.white, shape: const StadiumBorder()),
            child: _saving
                ? const SizedBox(width: 20, height: 20, child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                : Text('CONFIRMER LE PAIEMENT', style: GoogleFonts.manrope(fontSize: 15, fontWeight: FontWeight.w600)),
          ),
        ),
      ]),
      ),
    );
  }
}
