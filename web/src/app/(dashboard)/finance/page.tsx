'use client';
import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { finance, dashboard, orders, type Payment } from '@/lib/api';
import { DataTable } from '@/components/ui/data-table';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/tabs';
import { currentQuarter, quarterDates, formatDZD, formatDate, type Quarter } from '@/lib/helpers';
import { Download, TrendingUp, AlertTriangle, Wallet, Users, Calendar } from 'lucide-react';
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer, Legend } from 'recharts';
import { type ColumnDef } from '@tanstack/react-table';

const METHOD_COLORS = ['#2E7D32', '#1565C0', '#E65100', '#6A1B9A', '#C62828'];

// Payment columns
const paymentColumns: ColumnDef<Payment>[] = [
  { accessorKey: 'receiptCode', header: 'Reçu', cell: ({ row }) => <span className="font-mono text-xs font-semibold text-muted-foreground">{row.original.receiptCode || '—'}</span> },
  { accessorKey: 'amount', header: 'Montant', cell: ({ row }) => <span className="font-semibold">{formatDZD(row.original.amount)}</span> },
  { accessorKey: 'paymentMethodLabel', header: 'Méthode', cell: ({ row }) => <Badge variant="secondary" className="text-[10px] font-bold">{row.original.paymentMethodLabel}</Badge> },
  { accessorKey: 'paymentDate', header: 'Date', cell: ({ row }) => <span className="text-sm">{formatDate(row.original.paymentDate)}</span> },
  { accessorKey: 'note', header: 'Note', cell: ({ row }) => <span className="text-xs text-muted-foreground truncate max-w-[200px] block">{row.original.note || '—'}</span> },
  { id: 'actions', header: '', cell: ({ row }) => row.original.receiptCode ? (
    <a href={`/api/finance/receipts/${row.original.id}/pdf`} target="_blank" rel="noopener noreferrer" className="inline-flex items-center gap-1 text-[11px] font-bold text-chart-2 hover:underline"><Download className="h-3 w-3" /> PDF</a>
  ) : null },
];

// Client balance type
interface ClientBalance {
  clientId: string;
  clientName: string;
  totalPrice: number;
  totalPaid: number;
  outstanding: number;
  orderCount: number;
}

const balanceColumns: ColumnDef<ClientBalance>[] = [
  {
    accessorKey: 'clientName',
    header: 'Client',
    cell: ({ row }) => <span className="font-semibold">{row.original.clientName}</span>,
  },
  {
    accessorKey: 'orderCount',
    header: 'Commandes',
    cell: ({ row }) => <span className="text-sm">{row.original.orderCount}</span>,
  },
  {
    accessorKey: 'totalPrice',
    header: 'Total commandes',
    cell: ({ row }) => <span className="text-sm">{formatDZD(row.original.totalPrice)}</span>,
  },
  {
    accessorKey: 'totalPaid',
    header: 'Payé',
    cell: ({ row }) => <span className="text-sm font-semibold text-chart-1">{formatDZD(row.original.totalPaid)}</span>,
  },
  {
    accessorKey: 'outstanding',
    header: 'Reste',
    cell: ({ row }) => (
      <span className={`text-sm font-bold ${row.original.outstanding > 0 ? 'text-chart-2' : 'text-chart-1'}`}>
        {formatDZD(row.original.outstanding)}
      </span>
    ),
  },
];

export default function FinancePage() {
  const { year: initY, quarter: initQ } = currentQuarter();
  const [period, setPeriod] = useState(`T${initQ} ${initY}`); // "all" or "T1 2026"

  const isAll = period === 'all';
  const year = isAll ? initY : parseInt(period.split(' ')[1]);
  const quarter = isAll ? 1 : (parseInt(period.split(' ')[0].substring(1)) as Quarter);

  // For "all", fetch all 4 quarters of current year; otherwise just the selected one
  const quartersToFetch: Quarter[] = isAll ? [1, 2, 3, 4] : [quarter];
  const dates = isAll ? undefined : quarterDates(year, quarter);

  const { data: kpi1 } = useQuery({ queryKey: ['kpis', year, quartersToFetch[0]], queryFn: () => dashboard.kpis(year, quartersToFetch[0]) });
  const { data: kpi2 } = useQuery({ queryKey: ['kpis', year, quartersToFetch[1] ?? 0], queryFn: () => dashboard.kpis(year, quartersToFetch[1] ?? quartersToFetch[0]), enabled: isAll });
  const { data: kpi3 } = useQuery({ queryKey: ['kpis', year, quartersToFetch[2] ?? 0], queryFn: () => dashboard.kpis(year, quartersToFetch[2] ?? quartersToFetch[0]), enabled: isAll });
  const { data: kpi4 } = useQuery({ queryKey: ['kpis', year, quartersToFetch[3] ?? 0], queryFn: () => dashboard.kpis(year, quartersToFetch[3] ?? quartersToFetch[0]), enabled: isAll });

  const revenue = (kpi1?.revenueCollected ?? 0) + (isAll ? (kpi2?.revenueCollected ?? 0) + (kpi3?.revenueCollected ?? 0) + (kpi4?.revenueCollected ?? 0) : 0);
  const outstanding = (kpi1?.outstandingBalances ?? 0) + (isAll ? (kpi2?.outstandingBalances ?? 0) + (kpi3?.outstandingBalances ?? 0) + (kpi4?.outstandingBalances ?? 0) : 0);

  const { data: summary1 } = useQuery({ queryKey: ['finance-summary', year, quartersToFetch[0]], queryFn: () => finance.summary(year, quartersToFetch[0]) });
  const { data: summary2 } = useQuery({ queryKey: ['finance-summary', year, quartersToFetch[1] ?? 0], queryFn: () => finance.summary(year, quartersToFetch[1] ?? quartersToFetch[0]), enabled: isAll });
  const { data: summary3 } = useQuery({ queryKey: ['finance-summary', year, quartersToFetch[2] ?? 0], queryFn: () => finance.summary(year, quartersToFetch[2] ?? quartersToFetch[0]), enabled: isAll });
  const { data: summary4 } = useQuery({ queryKey: ['finance-summary', year, quartersToFetch[3] ?? 0], queryFn: () => finance.summary(year, quartersToFetch[3] ?? quartersToFetch[0]), enabled: isAll });

  const recentPayments = [
    ...(summary1?.recentPayments ?? []),
    ...(isAll ? [...(summary2?.recentPayments ?? []), ...(summary3?.recentPayments ?? []), ...(summary4?.recentPayments ?? [])] : []),
  ].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  // Client balances from orders
  const { data: ordersData } = useQuery({
    queryKey: ['finance-orders', period],
    queryFn: () => orders.list({ pageSize: 500, ...(dates ? { dateFrom: dates.from, dateTo: dates.to } : {}) }),
  });

  const clientBalances = useMemo<ClientBalance[]>(() => {
    if (!ordersData?.items) return [];
    const map = new Map<string, ClientBalance>();
    for (const o of ordersData.items) {
      const existing = map.get(o.clientId);
      const paid = o.totalPrice - o.outstandingBalance;
      if (existing) {
        existing.totalPrice += o.totalPrice;
        existing.totalPaid += paid;
        existing.outstanding += o.outstandingBalance;
        existing.orderCount++;
      } else {
        map.set(o.clientId, {
          clientId: o.clientId,
          clientName: o.clientName || '—',
          totalPrice: o.totalPrice,
          totalPaid: paid,
          outstanding: o.outstandingBalance,
          orderCount: 1,
        });
      }
    }
    return Array.from(map.values()).sort((a, b) => b.outstanding - a.outstanding);
  }, [ordersData]);

  // Aggregate payment method distribution
  const paymentsByMethod = useMemo(() => {
    const all = [
      ...(summary1?.paymentsByMethod ?? []),
      ...(isAll ? [...(summary2?.paymentsByMethod ?? []), ...(summary3?.paymentsByMethod ?? []), ...(summary4?.paymentsByMethod ?? [])] : []),
    ];
    const map = new Map<string, { method: string; label: string; total: number }>();
    for (const p of all) {
      const existing = map.get(p.method);
      if (existing) existing.total += p.total;
      else map.set(p.method, { ...p });
    }
    return Array.from(map.values()).filter(p => p.total > 0);
  }, [summary1, summary2, summary3, summary4, isAll]);

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-1">Comptabilité</p>
          <h1 className="font-heading text-3xl font-bold">Finance</h1>
        </div>
        <PeriodSelect value={period} onChange={setPeriod} />
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-3 gap-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-xs font-bold tracking-wider uppercase text-muted-foreground">CA encaissé</CardTitle>
            <TrendingUp className="h-4 w-4 text-chart-1" />
          </CardHeader>
          <CardContent>
            <p className="font-heading text-3xl font-bold text-chart-1">{formatDZD(revenue)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-xs font-bold tracking-wider uppercase text-muted-foreground">Soldes impayés</CardTitle>
            <AlertTriangle className="h-4 w-4 text-chart-2" />
          </CardHeader>
          <CardContent>
            <p className="font-heading text-3xl font-bold text-chart-2">{formatDZD(outstanding)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between pb-2">
            <CardTitle className="text-xs font-bold tracking-wider uppercase text-muted-foreground">Total global</CardTitle>
            <Wallet className="h-4 w-4 text-primary" />
          </CardHeader>
          <CardContent>
            <p className="font-heading text-3xl font-bold text-primary">{formatDZD(revenue + outstanding)}</p>
          </CardContent>
        </Card>
      </div>

      {/* Payment method distribution chart */}
      {paymentsByMethod.length > 0 && (
        <div className="bg-card rounded-2xl p-6 border border-border/20">
          <h3 className="font-heading text-base font-semibold mb-4">Répartition par méthode de paiement</h3>
          <ResponsiveContainer width="100%" height={240}>
            <PieChart>
              <Pie data={paymentsByMethod} dataKey="total" nameKey="label" cx="50%" cy="50%"
                outerRadius={90} innerRadius={50} paddingAngle={2} strokeWidth={0}>
                {paymentsByMethod.map((_, i) => (
                  <Cell key={i} fill={METHOD_COLORS[i % METHOD_COLORS.length]} />
                ))}
              </Pie>
              <Tooltip contentStyle={{ borderRadius: 12, border: '1px solid #D2C2CF', fontSize: 12 }}
                formatter={(value) => [`${new Intl.NumberFormat('fr-DZ').format(value as number)} DZD`]} />
              <Legend wrapperStyle={{ fontSize: 11 }} />
            </PieChart>
          </ResponsiveContainer>
        </div>
      )}

      {/* Tabs: Balances par client + Derniers paiements */}
      <Tabs defaultValue="balances">
        <TabsList>
          <TabsTrigger value="balances" className="gap-1.5"><Users className="h-3.5 w-3.5" /> Balances par client</TabsTrigger>
          <TabsTrigger value="payments" className="gap-1.5"><Download className="h-3.5 w-3.5" /> Derniers paiements</TabsTrigger>
        </TabsList>

        <TabsContent value="balances" className="mt-4">
          <DataTable
            columns={balanceColumns}
            data={clientBalances}
            totalCount={clientBalances.length}
            emptyMessage="Aucune commande ce trimestre"
          />
        </TabsContent>

        <TabsContent value="payments" className="mt-4">
          <DataTable
            columns={paymentColumns}
            data={recentPayments}
            totalCount={recentPayments.length}
            emptyMessage="Aucun paiement ce trimestre"
          />
        </TabsContent>
      </Tabs>
    </div>
  );
}

function PeriodSelect({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const now = new Date();
  const options: { label: string; value: string }[] = [{ label: 'Tout', value: 'all' }];
  for (let y = now.getFullYear(); y >= now.getFullYear() - 2; y--) {
    for (const q of [1, 2, 3, 4]) {
      options.push({ label: `T${q} ${y}`, value: `T${q} ${y}` });
    }
  }
  return (
    <select value={value} onChange={e => onChange(e.target.value)}
      className="h-10 px-3 pr-8 rounded-xl border border-border bg-card text-sm font-semibold
        focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary appearance-none cursor-pointer"
      style={{ backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%234D4544' viewBox='0 0 16 16'%3E%3Cpath d='M4.5 6l3.5 4 3.5-4z'/%3E%3C/svg%3E")`, backgroundRepeat: 'no-repeat', backgroundPosition: 'right 8px center' }}>
      {options.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
    </select>
  );
}
