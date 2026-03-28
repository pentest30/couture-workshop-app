'use client';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { dashboard, orders } from '@/lib/api';
import { QuarterSelect } from '@/components/ui/quarter-select';
import { StatusBadge } from '@/components/ui/status-badge';
import { currentQuarter, quarterDates, formatDZD, formatDate, type Quarter } from '@/lib/helpers';
import { useRouter } from 'next/navigation';
import { PieChart, Pie, Cell, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend, LineChart, Line } from 'recharts';

const STATUS_CHART_COLORS = ['#1565C0', '#F9A825', '#E65100', '#6A1B9A', '#880E4F', '#C62828', '#2E7D32', '#424242'];

export default function DashboardPage() {
  const router = useRouter();
  const { year: initY, quarter: initQ } = currentQuarter();
  const [year, setYear] = useState(initY);
  const [quarter, setQuarter] = useState<Quarter>(initQ);

  const dates = quarterDates(year, quarter);

  const { data: kpis } = useQuery({ queryKey: ['kpis', year, quarter], queryFn: () => dashboard.kpis(year, quarter) });

  const { data: histogram } = useQuery({ queryKey: ['histogram', year, quarter], queryFn: () => dashboard.monthlyHistogram(year, quarter) });
  const { data: statusDist } = useQuery({ queryKey: ['status-dist', year, quarter], queryFn: () => dashboard.statusDistribution(year, quarter) });
  const { data: revenueTrend } = useQuery({ queryKey: ['revenue-trend', year, quarter], queryFn: () => dashboard.revenueTrend(year, quarter) });
  const { data: delayData } = useQuery({ queryKey: ['delay-artisan', year, quarter], queryFn: () => dashboard.delayByArtisan(year, quarter) });
  const { data: recentOrders } = useQuery({
    queryKey: ['recent-orders', year, quarter],
    queryFn: () => orders.list({ page: 1, pageSize: 5, dateFrom: dates.from, dateTo: dates.to }),
  });

  return (
    <div className="space-y-8">
      {/* Header */}
      <div className="flex items-end justify-between">
        <div>
          <p className="text-xs font-semibold tracking-widest uppercase text-muted-foreground mb-1">Tableau de bord</p>
          <h1 className="font-heading text-3xl font-bold">Vue d&apos;ensemble</h1>
        </div>
        <div className="flex items-center gap-2">
          <a href={dashboard.exportUrl(year, quarter, 'csv')} download
            className="inline-flex items-center gap-1.5 h-10 px-3 rounded-xl border border-border bg-card text-sm font-semibold hover:bg-secondary/50 transition-colors">
            <svg className="w-4 h-4 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5M16.5 12L12 16.5m0 0L7.5 12m4.5 4.5V3" />
            </svg>
            CSV
          </a>
          <a href={dashboard.exportUrl(year, quarter, 'pdf')} download
            className="inline-flex items-center gap-1.5 h-10 px-3 rounded-xl border border-border bg-card text-sm font-semibold hover:bg-secondary/50 transition-colors">
            <svg className="w-4 h-4 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5M16.5 12L12 16.5m0 0L7.5 12m4.5 4.5V3" />
            </svg>
            PDF
          </a>
          <QuarterSelect year={year} quarter={quarter} onChange={(y, q) => { setYear(y); setQuarter(q); }} />
        </div>
      </div>

      {/* Late alert */}
      {kpis && kpis.lateOrders > 0 && (
        <button onClick={() => router.push('/orders')}
          className="w-full flex items-center gap-3 px-5 py-3.5 rounded-xl bg-destructive/8 border border-destructive/15 hover:bg-destructive/12 transition-colors text-left">
          <svg className="w-5 h-5 text-destructive shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
          </svg>
          <span className="text-sm font-semibold text-destructive">{kpis.lateOrders} commande(s) dépassent le délai de livraison</span>
          <svg className="w-4 h-4 text-destructive ml-auto" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M8.25 4.5l7.5 7.5-7.5 7.5" />
          </svg>
        </button>
      )}

      {/* KPI Cards */}
      {kpis && (
        <div className="grid grid-cols-5 gap-4">
          <KPICard label="Commandes" value={kpis.totalOrders} icon="receipt" color="var(--primary)" />
          <KPICard label="Livrées" value={kpis.deliveredOrders} icon="check" color="var(--color-status-prete)" />
          <KPICard label="En retard" value={kpis.lateOrders} icon="clock" color="var(--destructive)" />
          <KPICard label="CA encaissé" value={formatDZD(kpis.revenueCollected)} icon="money" color="var(--chart-2)" />
          <KPICard label="Soldes dus" value={formatDZD(kpis.outstandingBalances)} icon="wallet" color="var(--chart-2)" />
        </div>
      )}

      {/* Charts row */}
      <div className="grid grid-cols-2 gap-6">
        {/* Monthly histogram */}
        <div className="bg-card rounded-2xl p-6 border border-border/20">
          <h3 className="font-heading text-base font-semibold mb-4">Commandes par mois</h3>
          {histogram?.months ? (
            <ResponsiveContainer width="100%" height={240}>
              <BarChart data={histogram.months}>
                <CartesianGrid strokeDasharray="3 3" stroke="#E8E4E0" />
                <XAxis dataKey="month" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} />
                <Tooltip contentStyle={{ borderRadius: 12, border: '1px solid #D2C2CF', fontSize: 12 }} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
                <Bar dataKey="simple" name="Simple" fill="#7F5700" radius={[4, 4, 0, 0]} />
                <Bar dataKey="brode" name="Brodé" fill="#6A1B9A" radius={[4, 4, 0, 0]} />
                <Bar dataKey="perle" name="Perlé" fill="#880E4F" radius={[4, 4, 0, 0]} />
                <Bar dataKey="mixte" name="Mixte" fill="#410050" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          ) : <div className="h-60 flex items-center justify-center text-sm text-muted-foreground">Chargement...</div>}
        </div>

        {/* Status donut */}
        <div className="bg-card rounded-2xl p-6 border border-border/20">
          <h3 className="font-heading text-base font-semibold mb-4">Répartition par statut</h3>
          {statusDist?.statuses ? (
            <ResponsiveContainer width="100%" height={240}>
              <PieChart>
                <Pie data={statusDist.statuses} dataKey="count" nameKey="label" cx="50%" cy="50%"
                  outerRadius={90} innerRadius={50} paddingAngle={2} strokeWidth={0}>
                  {statusDist.statuses.map((s, i) => (
                    <Cell key={s.status} fill={s.color || STATUS_CHART_COLORS[i % STATUS_CHART_COLORS.length]} />
                  ))}
                </Pie>
                <Tooltip contentStyle={{ borderRadius: 12, border: '1px solid #D2C2CF', fontSize: 12 }} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          ) : <div className="h-60 flex items-center justify-center text-sm text-muted-foreground">Chargement...</div>}
        </div>
      </div>

      {/* Charts row 2: Types donut + Revenue curve */}
      <div className="grid grid-cols-2 gap-6">
        {/* Work type donut */}
        <div className="bg-card rounded-2xl p-6 border border-border/20">
          <h3 className="font-heading text-base font-semibold mb-4">Répartition par type</h3>
          {kpis ? (
            <ResponsiveContainer width="100%" height={240}>
              <PieChart>
                <Pie
                  data={[
                    { name: 'Simple', value: Math.max(0, kpis.totalOrders - kpis.embroideredOrders - kpis.beadedOrders) },
                    { name: 'Brodé', value: kpis.embroideredOrders },
                    { name: 'Perlé', value: kpis.beadedOrders },
                  ].filter(d => d.value > 0)}
                  dataKey="value" nameKey="name" cx="50%" cy="50%"
                  outerRadius={90} innerRadius={50} paddingAngle={2} strokeWidth={0}>
                  <Cell fill="#7F5700" />
                  <Cell fill="#6A1B9A" />
                  <Cell fill="#880E4F" />
                </Pie>
                <Tooltip contentStyle={{ borderRadius: 12, border: '1px solid #D2C2CF', fontSize: 12 }} />
                <Legend wrapperStyle={{ fontSize: 11 }} />
              </PieChart>
            </ResponsiveContainer>
          ) : <div className="h-60 flex items-center justify-center text-sm text-muted-foreground">Chargement...</div>}
        </div>

        {/* Revenue trend curve */}
        <div className="bg-card rounded-2xl p-6 border border-border/20">
          <h3 className="font-heading text-base font-semibold mb-4">Évolution du CA (4 trimestres)</h3>
          {revenueTrend?.quarters ? (
            <ResponsiveContainer width="100%" height={240}>
              <LineChart data={revenueTrend.quarters}>
                <CartesianGrid strokeDasharray="3 3" stroke="#E8E4E0" />
                <XAxis dataKey="label" tick={{ fontSize: 11 }} />
                <YAxis tick={{ fontSize: 11 }} tickFormatter={(v) => `${(v / 1000).toFixed(0)}k`} />
                <Tooltip contentStyle={{ borderRadius: 12, border: '1px solid #D2C2CF', fontSize: 12 }}
                  formatter={(value) => [`${new Intl.NumberFormat('fr-DZ').format(value as number)} DZD`, 'CA']} />
                <Line type="monotone" dataKey="revenue" name="CA" stroke="var(--primary)" strokeWidth={2.5} dot={{ r: 5, fill: 'var(--primary)' }} />
              </LineChart>
            </ResponsiveContainer>
          ) : <div className="h-60 flex items-center justify-center text-sm text-muted-foreground">Chargement...</div>}
        </div>
      </div>

      {/* Delays by artisan bar chart */}
      <div className="bg-card rounded-2xl p-6 border border-border/20">
        <h3 className="font-heading text-base font-semibold mb-4">Retard moyen par artisan (top 5)</h3>
        {delayData?.artisans && delayData.artisans.length > 0 ? (
          <ResponsiveContainer width="100%" height={240}>
            <BarChart data={delayData.artisans} layout="vertical">
              <CartesianGrid strokeDasharray="3 3" stroke="#E8E4E0" />
              <XAxis type="number" tick={{ fontSize: 11 }} unit="j" />
              <YAxis type="category" dataKey="name" tick={{ fontSize: 11 }} width={120} />
              <Tooltip contentStyle={{ borderRadius: 12, border: '1px solid #D2C2CF', fontSize: 12 }}
                formatter={(value) => [`${(value as number).toFixed(1)} jours`, 'Retard moyen']} />
              <Bar dataKey="avgDelayDays" name="Retard moyen" fill="#C62828" radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        ) : <div className="h-60 flex items-center justify-center text-sm text-muted-foreground">
          {delayData ? 'Aucun retard ce trimestre' : 'Chargement...'}
        </div>}
      </div>

      {/* Stats row */}
      {kpis && (
        <div className="grid grid-cols-3 gap-4">
          <div className="bg-card rounded-2xl p-5 border border-border/20 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-chart-3/10 flex items-center justify-center">
              <svg className="w-6 h-6 text-chart-3" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M9.53 16.122a3 3 0 00-5.78 1.128 2.25 2.25 0 01-2.4 2.245 4.5 4.5 0 008.4-2.245c0-.399-.078-.78-.22-1.128zm0 0a15.998 15.998 0 003.388-1.62m-5.043-.025a15.994 15.994 0 011.622-3.395m3.42 3.42a15.995 15.995 0 004.764-4.648l3.876-5.814a1.151 1.151 0 00-1.597-1.597L14.146 6.32a15.996 15.996 0 00-4.649 4.763m3.42 3.42a6.776 6.776 0 00-3.42-3.42" />
              </svg>
            </div>
            <div>
              <p className="font-heading text-2xl font-bold text-chart-3">{kpis.embroideredOrders}</p>
              <p className="text-xs text-muted-foreground">Brodées</p>
            </div>
          </div>
          <div className="bg-card rounded-2xl p-5 border border-border/20 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-chart-4/10 flex items-center justify-center">
              <svg className="w-6 h-6 text-chart-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M21 11.25v8.25a1.5 1.5 0 01-1.5 1.5H5.25a1.5 1.5 0 01-1.5-1.5v-8.25M12 4.875A2.625 2.625 0 109.375 7.5H12m0-2.625V7.5m0-2.625A2.625 2.625 0 1114.625 7.5H12m0 0V21m-8.625-9.75h18c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125h-18c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125z" />
              </svg>
            </div>
            <div>
              <p className="font-heading text-2xl font-bold text-chart-4">{kpis.beadedOrders}</p>
              <p className="text-xs text-muted-foreground">Perlées</p>
            </div>
          </div>
          <div className="bg-card rounded-2xl p-5 border border-border/20 flex items-center gap-4">
            <div className="w-12 h-12 rounded-xl bg-chart-1/10 flex items-center justify-center">
              <svg className="w-6 h-6 text-chart-1" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
            </div>
            <div>
              <p className="font-heading text-2xl font-bold text-chart-1">{kpis.onTimeDeliveryRate.toFixed(0)}%</p>
              <p className="text-xs text-muted-foreground">Livraison à temps</p>
            </div>
          </div>
        </div>
      )}

      {/* Recent orders */}
      <div>
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-heading text-lg font-semibold">Commandes récentes</h2>
          <button onClick={() => router.push('/orders')} className="text-xs font-bold tracking-wider uppercase text-chart-2 hover:text-chart-2/80 transition-colors">
            Tout voir
          </button>
        </div>
        <div className="bg-card rounded-2xl border border-border/20 divide-y divide-border/20">
          {recentOrders?.items?.map((o) => (
            <button key={o.id} onClick={() => router.push(`/orders/${o.id}`)}
              className="w-full flex items-center gap-4 px-5 py-3.5 hover:bg-secondary/50 transition-colors text-left">
              <div className="w-10 h-10 rounded-xl flex items-center justify-center" style={{ backgroundColor: `${o.statusColor}15` }}>
                <svg className="w-5 h-5" style={{ color: o.statusColor }} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M15.75 10.5V6a3.75 3.75 0 10-7.5 0v4.5m11.356-1.993l1.263 12c.07.665-.45 1.243-1.119 1.243H4.25a1.125 1.125 0 01-1.12-1.243l1.264-12A1.125 1.125 0 015.513 7.5h12.974c.576 0 1.059.435 1.119 1.007zM8.625 10.5a.375.375 0 11-.75 0 .375.375 0 01.75 0zm7.5 0a.375.375 0 11-.75 0 .375.375 0 01.75 0z" />
                </svg>
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-semibold truncate">{o.clientName || 'Client'}</p>
                <p className="text-xs text-muted-foreground">{o.code}</p>
              </div>
              <StatusBadge status={o.status} label={o.statusLabel} />
              <div className="text-right ml-2">
                <p className="text-xs text-muted-foreground">{formatDate(o.expectedDeliveryDate)}</p>
                {o.isLate && <p className="text-[10px] font-bold text-destructive">{o.delayDays}j retard</p>}
              </div>
            </button>
          ))}
          {recentOrders?.items?.length === 0 && (
            <p className="py-8 text-center text-sm text-muted-foreground">Aucune commande ce trimestre</p>
          )}
        </div>
      </div>
    </div>
  );
}

function KPICard({ label, value, icon, color }: { label: string; value: string | number; icon: string; color: string }) {
  const icons: Record<string, string> = {
    receipt: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z',
    check: 'M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
    clock: 'M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z',
    money: 'M12 6v12m-3-2.818l.879.659c1.171.879 3.07.879 4.242 0 1.172-.879 1.172-2.303 0-3.182C13.536 12.219 12.768 12 12 12c-.725 0-1.45-.22-2.003-.659-1.106-.879-1.106-2.303 0-3.182s2.9-.879 4.006 0l.415.33M21 12a9 9 0 11-18 0 9 9 0 0118 0z',
    wallet: 'M21 12a2.25 2.25 0 00-2.25-2.25H15a3 3 0 110-6h5.25A2.25 2.25 0 0121 6v6zm-3 0a.75.75 0 11-1.5 0 .75.75 0 011.5 0zm-12-3.75h.008v.008H6v-.008z',
  };
  return (
    <div className="bg-card rounded-2xl p-5 border border-border/20">
      <div className="w-9 h-9 rounded-lg mb-3 flex items-center justify-center" style={{ backgroundColor: `${color}15` }}>
        <svg className="w-5 h-5" style={{ color }} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
          <path strokeLinecap="round" strokeLinejoin="round" d={icons[icon] || icons.receipt} />
        </svg>
      </div>
      <p className="font-heading text-2xl font-bold" style={{ color }}>{value}</p>
      <p className="text-xs text-muted-foreground mt-0.5">{label}</p>
    </div>
  );
}
