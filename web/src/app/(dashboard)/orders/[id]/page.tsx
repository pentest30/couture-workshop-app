'use client';
import { use, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { orders, finance, users, type Payment, type User, ROLE_LABELS } from '@/lib/api';
import { StatusBadge } from '@/components/ui/status-badge';
import { WorkTypeBadge } from '@/components/ui/work-type-badge';
import { formatDZD, formatDate, STATUS_COLORS } from '@/lib/helpers';
import Link from 'next/link';

export default function OrderDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const queryClient = useQueryClient();
  const { data: order, isLoading } = useQuery({ queryKey: ['order', id], queryFn: () => orders.get(id) });
  const { data: payments } = useQuery({ queryKey: ['payments', id], queryFn: () => finance.getPayments(id) });

  const [showStatusModal, setShowStatusModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);

  if (isLoading) return <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>;
  if (!order) return <div className="py-20 text-center text-muted-foreground">Commande introuvable</div>;

  const totalPaid = payments?.reduce((s, p) => s + p.amount, 0) ?? 0;
  const outstanding = order.totalPrice - totalPaid;

  return (
    <div className="space-y-6">
      {/* Breadcrumb + actions */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/orders" className="text-muted-foreground hover:text-primary transition-colors">Commandes</Link>
          <span className="text-muted-foreground">/</span>
          <span className="font-semibold">{order.code}</span>
        </div>
        <div className="flex items-center gap-2">
          {order.status !== 'Livree' && (
            <>
              <button onClick={() => setShowStatusModal(true)}
                className="h-9 px-4 rounded-full bg-primary text-white text-sm font-semibold hover:bg-primary transition-colors">
                Changer le statut
              </button>
              <Link href={`/orders/${id}/edit`}
                className="h-9 px-4 rounded-full border border-border text-sm font-semibold hover:bg-secondary transition-colors inline-flex items-center">
                Modifier
              </Link>
            </>
          )}
          <a href={`/api/orders/${id}/worksheet`} target="_blank" rel="noopener noreferrer"
            className="h-9 px-4 rounded-full border border-border text-sm font-semibold hover:bg-secondary transition-colors inline-flex items-center gap-1.5">
            Fiche artisan
          </a>
        </div>
      </div>

      {/* Late alert */}
      {order.isLate && (
        <div className="flex items-center gap-3 px-5 py-3 rounded-xl bg-destructive/8 border border-destructive/15">
          <svg className="w-5 h-5 text-destructive" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126z" />
          </svg>
          <span className="text-sm font-semibold text-destructive">En retard de {order.delayDays} jour(s)</span>
        </div>
      )}

      <div className="grid grid-cols-3 gap-6">
        {/* Main info — 2 cols */}
        <div className="col-span-2 space-y-6">
          {/* Header card */}
          <div className="bg-card rounded-2xl p-6 border border-border/20">
            <div className="flex items-start justify-between mb-4">
              <div>
                <h1 className="font-heading text-2xl font-bold">{order.clientName || 'Client'}</h1>
                <p className="text-sm text-muted-foreground">{order.code}</p>
              </div>
              <div className="flex items-center gap-2">
                <StatusBadge status={order.status} label={order.statusLabel} />
                <WorkTypeBadge workType={order.workType} label={order.workTypeLabel} />
              </div>
            </div>

            {/* Dates */}
            <div className="grid grid-cols-3 gap-4 mb-5">
              <InfoItem label="Réception" value={formatDate(order.receptionDate)} />
              <InfoItem label="Livraison prévue" value={formatDate(order.expectedDeliveryDate)} />
              {order.actualDeliveryDate && <InfoItem label="Livraison réelle" value={formatDate(order.actualDeliveryDate)} />}
            </div>

            {/* Description */}
            {order.description && (
              <div className="mb-4">
                <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Description</p>
                <p className="text-sm leading-relaxed">{order.description}</p>
              </div>
            )}
            {order.fabric && <InfoItem label="Tissu" value={order.fabric} />}
            {order.technicalNotes && (
              <div className="mt-3">
                <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Notes techniques</p>
                <p className="text-sm leading-relaxed whitespace-pre-wrap text-muted-foreground">{order.technicalNotes}</p>
              </div>
            )}
          </div>

          {/* Work type specifics */}
          {(order.embroideryStyle || order.threadColors || order.density || order.embroideryZone) && (
            <div className="bg-card rounded-2xl p-6 border border-border/20">
              <h3 className="font-heading text-base font-semibold mb-4">Détails broderie</h3>
              <div className="grid grid-cols-2 gap-3">
                {order.embroideryStyle && <InfoItem label="Style" value={order.embroideryStyle} />}
                {order.threadColors && <InfoItem label="Couleurs fils" value={order.threadColors} />}
                {order.density && <InfoItem label="Densité" value={order.density} />}
                {order.embroideryZone && <InfoItem label="Zone" value={order.embroideryZone} />}
              </div>
            </div>
          )}
          {(order.beadType || order.arrangement || order.affectedZones) && (
            <div className="bg-card rounded-2xl p-6 border border-border/20">
              <h3 className="font-heading text-base font-semibold mb-4">Détails perlage</h3>
              <div className="grid grid-cols-2 gap-3">
                {order.beadType && <InfoItem label="Type de perle" value={order.beadType} />}
                {order.arrangement && <InfoItem label="Arrangement" value={order.arrangement} />}
                {order.affectedZones && <InfoItem label="Zones" value={order.affectedZones} />}
              </div>
            </div>
          )}

          {/* Timeline */}
          <div className="bg-card rounded-2xl p-6 border border-border/20">
            <h3 className="font-heading text-base font-semibold mb-4">Historique</h3>
            <div className="space-y-0">
              {order.timeline?.map((t, i) => {
                const color = STATUS_COLORS[t.toStatus] || '#666';
                const isLast = i === order.timeline.length - 1;
                return (
                  <div key={i} className="flex gap-3">
                    <div className="flex flex-col items-center">
                      <div className="w-3 h-3 rounded-full shrink-0 mt-1" style={{ backgroundColor: color }} />
                      {!isLast && <div className="w-0.5 flex-1 my-1" style={{ backgroundColor: `${color}30` }} />}
                    </div>
                    <div className="pb-5">
                      <p className="text-sm font-semibold" style={{ color }}>{t.toStatusLabel}</p>
                      {t.reason && <p className="text-xs text-muted-foreground mt-0.5">{t.reason}</p>}
                      <p className="text-[11px] text-muted-foreground mt-0.5">{formatDate(t.transitionedAt)}</p>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Sidebar — 1 col */}
        <div className="space-y-6">
          {/* Pricing */}
          <div className="bg-card rounded-2xl p-6 border border-border/20">
            <h3 className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-3">Financier</h3>
            <div className="space-y-3">
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">Prix total</span>
                <span className="font-heading text-lg font-bold">{formatDZD(order.totalPrice)}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-sm text-muted-foreground">Total payé</span>
                <span className="text-sm font-semibold text-chart-1">{formatDZD(totalPaid)}</span>
              </div>
              <div className="h-px bg-border/30" />
              <div className="flex justify-between">
                <span className="text-sm font-semibold">Solde restant</span>
                <span className={`font-heading text-lg font-bold ${outstanding > 0 ? 'text-chart-2' : 'text-chart-1'}`}>
                  {formatDZD(outstanding)}
                </span>
              </div>
            </div>
            {outstanding > 0 && (
              <button onClick={() => setShowPaymentModal(true)}
                className="w-full h-10 mt-4 rounded-full bg-chart-2 text-white text-sm font-semibold hover:bg-chart-2/90 transition-colors">
                Encaisser
              </button>
            )}
          </div>

          {/* Payments */}
          {payments && payments.length > 0 && (
            <div className="bg-card rounded-2xl p-6 border border-border/20">
              <h3 className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-3">Paiements</h3>
              <div className="space-y-3">
                {payments.map((p) => (
                  <PaymentItem key={p.id} payment={p} />
                ))}
              </div>
            </div>
          )}

          {/* Artisans */}
          {(order.assignedTailorId || order.assignedEmbroidererId || order.assignedBeaderId) && (
            <div className="bg-card rounded-2xl p-6 border border-border/20">
              <h3 className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-3">Artisans</h3>
              <div className="space-y-2">
                {order.assignedTailorId && <ArtisanRow label="Couturier(e)" name={order.assignedTailorName} />}
                {order.assignedEmbroidererId && <ArtisanRow label="Brodeur(se)" name={order.assignedEmbroidererName} />}
                {order.assignedBeaderId && <ArtisanRow label="Perleur(se)" name={order.assignedBeaderName} />}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Status change modal */}
      {showStatusModal && (
        <StatusModal orderId={id} order={order} onClose={() => setShowStatusModal(false)}
          onDone={() => { setShowStatusModal(false); queryClient.invalidateQueries({ queryKey: ['order', id] }); }} />
      )}

      {/* Payment modal */}
      {showPaymentModal && (
        <PaymentModal orderId={id} orderCode={order.code} outstanding={outstanding}
          onClose={() => setShowPaymentModal(false)}
          onDone={() => { setShowPaymentModal(false); queryClient.invalidateQueries({ queryKey: ['payments', id] }); }} />
      )}
    </div>
  );
}

function InfoItem({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-0.5">{label}</p>
      <p className="text-sm">{value}</p>
    </div>
  );
}

function ArtisanRow({ label, name }: { label: string; name?: string }) {
  return (
    <div className="flex items-center gap-2">
      <div className="w-7 h-7 rounded-full bg-primary/20 flex items-center justify-center">
        <svg className="w-3.5 h-3.5 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
          <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 6a3.75 3.75 0 11-7.5 0 3.75 3.75 0 017.5 0zM4.501 20.118a7.5 7.5 0 0114.998 0" />
        </svg>
      </div>
      <div>
        <p className="text-xs text-muted-foreground">{label}</p>
        <p className="text-sm font-semibold">{name || 'Non renseigné'}</p>
      </div>
    </div>
  );
}

function PaymentItem({ payment: p }: { payment: Payment }) {
  return (
    <div className="p-3 rounded-xl bg-secondary">
      <div className="flex items-center justify-between mb-1">
        <span className="font-heading text-sm font-bold">{formatDZD(p.amount)}</span>
        <span className="text-[10px] font-bold px-2 py-0.5 rounded bg-primary/8 text-primary">{p.paymentMethodLabel}</span>
      </div>
      <p className="text-[11px] text-muted-foreground">{formatDate(p.paymentDate)}</p>
      {p.note && <p className="text-[11px] text-muted-foreground mt-1">{p.note}</p>}
      {p.receiptCode && (
        <a href={`/api/finance/receipts/${p.id}/pdf`} target="_blank" rel="noopener noreferrer"
          className="inline-flex items-center gap-1 mt-2 text-[11px] font-bold text-chart-2 hover:underline">
          <svg className="w-3 h-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5M16.5 12L12 16.5m0 0L7.5 12m4.5 4.5V3" />
          </svg>
          {p.receiptCode}
        </a>
      )}
    </div>
  );
}

function StatusModal({ orderId, order, onClose, onDone }: { orderId: string; order: { status: string; workType: string }; onClose: () => void; onDone: () => void }) {
  const [newStatus, setNewStatus] = useState('');
  const [reason, setReason] = useState('');
  const [assignedTailorId, setAssignedTailorId] = useState('');
  const [assignedEmbroidererId, setAssignedEmbroidererId] = useState('');
  const [assignedBeaderId, setAssignedBeaderId] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  // Fetch all active users once, filter by role client-side
  const { data: allUsers } = useQuery({ queryKey: ['users-active'], queryFn: () => users.list({ activeOnly: true }) });
  const tailors = { items: allUsers?.items?.filter(u => u.roles.includes('Tailor')) ?? [] };
  const embroiderers = { items: allUsers?.items?.filter(u => u.roles.includes('Embroiderer')) ?? [] };
  const beaders = { items: allUsers?.items?.filter(u => u.roles.includes('Beader')) ?? [] };

  const transitions: Record<string, string[]> = {
    Recue: ['EnAttente', 'EnCours'],
    EnAttente: ['EnCours'],
    EnCours: ['Broderie', 'Perlage', 'Retouche', 'Prete'].filter(s =>
      s === 'Broderie' ? ['Brode', 'Mixte'].includes(order.workType) :
      s === 'Perlage' ? ['Perle', 'Mixte'].includes(order.workType) : true),
    Broderie: [...(order.workType === 'Mixte' ? ['Perlage'] : []), 'Retouche', 'Prete'],
    Perlage: ['Retouche', 'Prete'],
    Retouche: ['EnCours', ...((['Brode', 'Mixte'].includes(order.workType)) ? ['Broderie'] : []),
      ...((['Perle', 'Mixte'].includes(order.workType)) ? ['Perlage'] : []), 'Prete'],
    Prete: ['Livree'],
  };

  const available = transitions[order.status] || [];

  // Determine which artisan picker to show based on target status
  const showTailor = newStatus === 'EnCours';
  const showEmbroiderer = newStatus === 'Broderie';
  const showBeader = newStatus === 'Perlage';

  const handleSubmit = async () => {
    if (!newStatus) return;
    setSaving(true); setError('');
    try {
      await orders.changeStatus(orderId, {
        newStatus,
        reason: reason || undefined,
        assignedTailorId: assignedTailorId || undefined,
        assignedEmbroidererId: assignedEmbroidererId || undefined,
        assignedBeaderId: assignedBeaderId || undefined,
      });
      onDone();
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false);
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50" onClick={onClose}>
      <div className="bg-card rounded-2xl p-6 w-full max-w-md shadow-xl" onClick={e => e.stopPropagation()}>
        <h3 className="font-heading text-lg font-semibold mb-4">Changer le statut</h3>
        <div className="space-y-3">
          {available.map(s => (
            <button key={s} onClick={() => setNewStatus(s)}
              className={`w-full px-4 py-3 rounded-xl border text-left text-sm font-medium transition-all
                ${newStatus === s ? 'border-primary bg-primary/5 text-primary' : 'border-border/30 hover:bg-secondary'}`}>
              <StatusBadge status={s} label={STATUS_LABELS_LOCAL[s] || s} />
            </button>
          ))}
        </div>

        {/* Artisan assignment (optional) */}
        {showTailor && (
          <ArtisanPicker label="Couturière" hint="Optionnel — si vide, vous serez assigné(e)"
            users={tailors?.items} value={assignedTailorId} onChange={setAssignedTailorId} />
        )}
        {showEmbroiderer && (
          <ArtisanPicker label="Brodeur(se)" hint="Optionnel — si vide, vous serez assigné(e)"
            users={embroiderers?.items} value={assignedEmbroidererId} onChange={setAssignedEmbroidererId} />
        )}
        {showBeader && (
          <ArtisanPicker label="Perleur(se)" hint="Optionnel — si vide, vous serez assigné(e)"
            users={beaders?.items} value={assignedBeaderId} onChange={setAssignedBeaderId} />
        )}

        {newStatus === 'Retouche' && (
          <textarea value={reason} onChange={e => setReason(e.target.value)} placeholder="Motif de la retouche..."
            className="w-full mt-3 p-3 rounded-xl border border-border text-sm resize-none h-20
              focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary" />
        )}
        {error && <p className="mt-2 text-sm text-destructive">{error}</p>}
        <div className="flex gap-2 mt-4">
          <button onClick={onClose} className="flex-1 h-10 rounded-full border border-border text-sm font-medium hover:bg-secondary transition-colors">Annuler</button>
          <button onClick={handleSubmit} disabled={!newStatus || saving}
            className="flex-1 h-10 rounded-full bg-primary text-white text-sm font-semibold hover:bg-primary disabled:opacity-40 transition-colors">
            {saving ? 'Enregistrement...' : 'Confirmer'}
          </button>
        </div>
      </div>
    </div>
  );
}

function ArtisanPicker({ label, hint, users: artisans, value, onChange }: {
  label: string; hint: string; users?: User[]; value: string; onChange: (v: string) => void;
}) {
  return (
    <div className="mt-3">
      <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">{label}</label>
      <select value={value} onChange={e => onChange(e.target.value)}
        className="w-full h-10 px-3 rounded-xl border border-border bg-card text-sm
          focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary appearance-none cursor-pointer"
        style={{ backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%234D4544' viewBox='0 0 16 16'%3E%3Cpath d='M4.5 6l3.5 4 3.5-4z'/%3E%3C/svg%3E")`, backgroundRepeat: 'no-repeat', backgroundPosition: 'right 8px center' }}>
        <option value="">— Non assigné —</option>
        {artisans?.map(u => (
          <option key={u.id} value={u.id}>{u.firstName} {u.lastName}</option>
        ))}
      </select>
      <p className="text-[11px] text-muted-foreground mt-1">
        {artisans === undefined ? 'Chargement...' : artisans.length === 0 ? 'Aucun artisan avec ce rôle' : hint}
      </p>
    </div>
  );
}

const STATUS_LABELS_LOCAL: Record<string, string> = {
  Recue: 'Reçue', EnAttente: 'En Attente', EnCours: 'En Cours',
  Broderie: 'Broderie', Perlage: 'Perlage', Retouche: 'Retouche', Prete: 'Prête', Livree: 'Livrée',
};

function PaymentModal({ orderId, orderCode, outstanding, onClose, onDone }: {
  orderId: string; orderCode: string; outstanding: number; onClose: () => void; onDone: () => void
}) {
  const [amount, setAmount] = useState('');
  const [method, setMethod] = useState('Especes');
  const [note, setNote] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const methods = [
    { key: 'Especes', label: 'Espèces' }, { key: 'Virement', label: 'Virement' },
    { key: 'Ccp', label: 'CCP' }, { key: 'BaridiMob', label: 'BaridiMob' }, { key: 'Dahabia', label: 'Dahabia' },
  ];

  const handleSubmit = async () => {
    const amt = parseFloat(amount);
    if (!amt || amt <= 0) { setError('Montant invalide'); return; }
    if (amt > outstanding) { setError(`Dépasse le solde (${formatDZD(outstanding)})`); return; }
    setSaving(true); setError('');
    try {
      const now = new Date();
      const dateStr = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
      await finance.recordPayment(orderId, { amount: amt, paymentMethod: method, paymentDate: dateStr, note: note || undefined });
      onDone();
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false);
  };

  return (
    <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50" onClick={onClose}>
      <div className="bg-card rounded-2xl p-6 w-full max-w-md shadow-xl" onClick={e => e.stopPropagation()}>
        <h3 className="font-heading text-lg font-semibold mb-1">Encaisser un paiement</h3>
        <p className="text-sm text-muted-foreground mb-4">{orderCode} — Solde: {formatDZD(outstanding)}</p>
        <div className="space-y-3">
          <input type="number" value={amount} onChange={e => setAmount(e.target.value)} placeholder="Montant (DZD)"
            className="w-full h-11 px-4 rounded-xl border border-border text-lg font-heading font-semibold
              focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary" />
          <div>
            <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2">Méthode</p>
            <div className="flex flex-wrap gap-2">
              {methods.map(m => (
                <button key={m.key} onClick={() => setMethod(m.key)}
                  className={`px-3 py-1.5 rounded-full text-xs font-semibold transition-all
                    ${method === m.key ? 'bg-primary text-white' : 'bg-secondary hover:bg-muted'}`}>
                  {m.label}
                </button>
              ))}
            </div>
          </div>
          <input value={note} onChange={e => setNote(e.target.value)} placeholder="Note (optionnel)"
            className="w-full h-10 px-4 rounded-xl border border-border text-sm
              focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary" />
        </div>
        {error && <p className="mt-2 text-sm text-destructive">{error}</p>}
        <div className="flex gap-2 mt-4">
          <button onClick={onClose} className="flex-1 h-10 rounded-full border border-border text-sm font-medium hover:bg-secondary transition-colors">Annuler</button>
          <button onClick={handleSubmit} disabled={saving}
            className="flex-1 h-10 rounded-full bg-chart-1 text-white text-sm font-semibold hover:bg-chart-1/90 disabled:opacity-40 transition-colors">
            {saving ? 'Enregistrement...' : 'Confirmer'}
          </button>
        </div>
      </div>
    </div>
  );
}
