'use client';
import { use } from 'react';
import { useQuery } from '@tanstack/react-query';
import { clients, orders } from '@/lib/api';
import { StatusBadge } from '@/components/ui/status-badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatDate, formatDZD } from '@/lib/helpers';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Ruler } from 'lucide-react';

export default function ClientProfilePage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const { data: client } = useQuery({ queryKey: ['client', id], queryFn: () => clients.get(id) });
  const { data: clientOrders } = useQuery({ queryKey: ['client-orders', id], queryFn: () => orders.list({ clientId: id, pageSize: 50 }) });
  const { data: measurements } = useQuery({ queryKey: ['measurements', id], queryFn: () => clients.getMeasurements(id) });

  if (!client) return <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>;

  const totalOrders = clientOrders?.totalCount ?? 0;
  const totalRevenue = clientOrders?.items?.reduce((s, o) => s + o.totalPrice, 0) ?? 0;
  const totalPaid = clientOrders?.items?.reduce((s, o) => s + (o.totalPrice - o.outstandingBalance), 0) ?? 0;
  const totalOutstanding = totalRevenue - totalPaid;
  const activeOrders = clientOrders?.items?.filter(o => o.status !== 'Livree').length ?? 0;
  const lastOrderDate = clientOrders?.items?.[0]?.createdAt;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/clients" className="text-muted-foreground hover:text-primary transition-colors">Clients</Link>
          <span className="text-muted-foreground">/</span>
          <span className="font-semibold">{client.firstName} {client.lastName}</span>
        </div>
        <Link href={`/clients/${id}/edit`}>
          <button className="h-9 px-4 rounded-full border border-border text-sm font-semibold hover:bg-accent transition-colors">
            Modifier
          </button>
        </Link>
      </div>

      <div className="grid grid-cols-3 gap-6">
        {/* Profile card */}
        <div className="bg-card rounded-2xl p-6 border border-border/20">
          <div className="flex items-center gap-4 mb-5">
            <div className="w-14 h-14 rounded-full bg-primary flex items-center justify-center text-white font-bold text-lg">
              {client.firstName?.[0]}{client.lastName?.[0]}
            </div>
            <div>
              <h1 className="font-heading text-xl font-bold">{client.firstName} {client.lastName}</h1>
              <p className="text-sm text-muted-foreground">{client.code}</p>
            </div>
          </div>
          <div className="space-y-3">
            <div><p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-0.5">Téléphone</p><p className="text-sm">{client.primaryPhone}</p></div>
            {client.secondaryPhone && <div><p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-0.5">Tél. secondaire</p><p className="text-sm">{client.secondaryPhone}</p></div>}
            {client.address && <div><p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-0.5">Adresse</p><p className="text-sm">{client.address}</p></div>}
            {client.notes && <div><p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-0.5">Notes</p><p className="text-sm text-muted-foreground">{client.notes}</p></div>}
          </div>
          {/* Stats */}
          <div className="grid grid-cols-2 gap-3 mt-5 pt-5 border-t border-border/20">
            <div className="text-center"><p className="font-heading text-2xl font-bold text-primary">{totalOrders}</p><p className="text-xs text-muted-foreground">Commandes</p></div>
            <div className="text-center"><p className="font-heading text-2xl font-bold text-chart-1">{activeOrders}</p><p className="text-xs text-muted-foreground">En cours</p></div>
            <div className="text-center"><p className="font-heading text-lg font-bold text-chart-1">{formatDZD(totalPaid)}</p><p className="text-xs text-muted-foreground">Total payé</p></div>
            <div className="text-center"><p className="font-heading text-lg font-bold text-chart-2">{formatDZD(totalOutstanding)}</p><p className="text-xs text-muted-foreground">Solde dû</p></div>
          </div>
          {lastOrderDate && (
            <p className="text-xs text-muted-foreground mt-3 text-center">Dernière commande: {formatDate(lastOrderDate)}</p>
          )}
          <button onClick={() => router.push(`/orders/new?clientId=${id}`)}
            className="w-full h-10 mt-4 rounded-full bg-primary text-white text-sm font-semibold hover:bg-primary/90 transition-colors">
            Nouvelle commande
          </button>
        </div>

        {/* Orders + measurements */}
        <div className="col-span-2 space-y-6">
          {/* Measurements */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2 text-base">
                <Ruler className="h-4 w-4 text-primary" />
                Mensurations
              </CardTitle>
            </CardHeader>
            <CardContent>
              {measurements?.current && measurements.current.length > 0 ? (
                <div className="grid grid-cols-5 gap-3">
                  {measurements.current.map((m, i) => (
                    <div key={i} className="p-3 rounded-xl bg-muted text-center">
                      <p className="font-heading text-xl font-bold text-primary">{m.value}</p>
                      <p className="text-[10px] text-muted-foreground uppercase tracking-wider mt-0.5">{m.unit}</p>
                      <p className="text-[11px] font-medium text-foreground mt-1">{m.fieldName}</p>
                    </div>
                  ))}
                </div>
              ) : (
                <div className="py-6 text-center text-muted-foreground">
                  <Ruler className="h-8 w-8 mx-auto mb-2 opacity-30" />
                  <p className="text-sm">Aucune mensuration enregistrée</p>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Orders */}
          <div className="bg-card rounded-2xl p-6 border border-border/20">
            <h3 className="font-heading text-base font-semibold mb-4">Historique des commandes</h3>
            <div className="space-y-2">
              {clientOrders?.items?.map(o => (
                <button key={o.id} onClick={() => router.push(`/orders/${o.id}`)}
                  className="w-full flex items-center gap-4 p-3 rounded-xl hover:bg-secondary/50 transition-colors text-left">
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-semibold">{o.code}</p>
                    <p className="text-xs text-muted-foreground">{formatDate(o.expectedDeliveryDate)}</p>
                  </div>
                  <StatusBadge status={o.status} label={o.statusLabel} />
                  <span className="text-sm font-semibold">{formatDZD(o.totalPrice)}</span>
                </button>
              ))}
              {(!clientOrders?.items || clientOrders.items.length === 0) && (
                <p className="text-sm text-muted-foreground text-center py-4">Aucune commande</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
