'use client';
import { use, useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { orders } from '@/lib/api';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function EditOrderPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const { data: order } = useQuery({ queryKey: ['order', id], queryFn: () => orders.get(id) });

  const [deliveryDate, setDeliveryDate] = useState('');
  const [totalPrice, setTotalPrice] = useState('');
  const [technicalNotes, setTechnicalNotes] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (order) {
      setDeliveryDate(order.expectedDeliveryDate);
      setTotalPrice(String(order.totalPrice));
      setTechnicalNotes(order.technicalNotes || '');
    }
  }, [order]);

  if (!order) return <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>;

  if (order.status === 'Livree') {
    return (
      <div className="max-w-2xl mx-auto py-20 text-center">
        <p className="text-muted-foreground mb-4">Cette commande est livrée et ne peut plus être modifiée.</p>
        <Link href={`/orders/${id}`} className="text-sm font-semibold text-primary hover:underline">Retour à la commande</Link>
      </div>
    );
  }

  const handleSave = async () => {
    setSaving(true); setError('');
    try {
      // The API currently doesn't have a dedicated PUT endpoint for order editing.
      // This would need to be added to the backend.
      // For now, we show the form but note the limitation.
      setError('Fonctionnalité en cours de développement côté serveur.');
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false);
  };

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div className="flex items-center gap-2 text-sm">
        <Link href={`/orders/${id}`} className="text-muted-foreground hover:text-primary transition-colors">← Retour</Link>
      </div>

      <div>
        <p className="text-xs font-semibold tracking-widest uppercase text-muted-foreground mb-1">Modifier</p>
        <h1 className="font-heading text-3xl font-bold">{order.code}</h1>
      </div>

      <div className="bg-card rounded-2xl p-6 border border-border/20 space-y-5">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Date de livraison prévue</label>
            <input type="date" value={deliveryDate} onChange={e => setDeliveryDate(e.target.value)}
              className="w-full h-11 px-3 rounded-xl border border-border text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary" />
          </div>
          <div>
            <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Prix total (DZD)</label>
            <input type="number" value={totalPrice} onChange={e => setTotalPrice(e.target.value)}
              className="w-full h-11 px-3 rounded-xl border border-border text-sm font-heading font-semibold focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary" />
          </div>
        </div>

        <div>
          <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Notes techniques</label>
          <textarea value={technicalNotes} onChange={e => setTechnicalNotes(e.target.value)} rows={4}
            className="w-full p-3 rounded-xl border border-border text-sm resize-none focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary" />
        </div>

        {error && <p className="text-sm text-destructive font-medium">{error}</p>}

        <div className="flex gap-2 pt-4 border-t border-border/20">
          <Link href={`/orders/${id}`}
            className="h-10 px-6 rounded-full border border-border text-sm font-medium hover:bg-secondary transition-colors inline-flex items-center">
            Annuler
          </Link>
          <button onClick={handleSave} disabled={saving}
            className="h-10 px-8 rounded-full bg-primary text-white text-sm font-semibold hover:bg-primary disabled:opacity-40 transition-colors">
            {saving ? 'Enregistrement...' : 'Enregistrer'}
          </button>
        </div>
      </div>
    </div>
  );
}
