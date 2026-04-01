'use client';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { orders, finance, dashboard, type OrderSummary } from '@/lib/api';
import { QuarterSelect } from '@/components/ui/quarter-select';
import { StatusBadge } from '@/components/ui/status-badge';
import { WorkTypeBadge } from '@/components/ui/work-type-badge';
import { DataTable } from '@/components/ui/data-table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';
import { currentQuarter, quarterDates, formatDZD, formatDate, ALL_STATUSES, STATUS_LABELS, type Quarter } from '@/lib/helpers';
import { useRouter } from 'next/navigation';
import { useQueryClient } from '@tanstack/react-query';
import { Plus, Search, MoreHorizontal, Eye, Download, Pencil, Trash2, FileDown } from 'lucide-react';
import { type ColumnDef } from '@tanstack/react-table';

function OrderActions({ order }: { order: OrderSummary }) {
  const router = useRouter();
  const queryClient = useQueryClient();

  const handleDelete = async () => {
    if (!confirm('Supprimer cette commande ?')) return;
    try {
      await orders.deactivate(order.id);
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    } catch (e) { alert(e instanceof Error ? e.message : 'Erreur'); }
  };

  const handleDownload = async () => {
    try {
      const payments = await finance.getPayments(order.id);
      const withReceipt = payments.filter(p => p.receiptCode);
      if (withReceipt.length === 0) {
        alert('Aucun reçu disponible pour cette commande');
        return;
      }
      window.open(finance.receiptUrl(withReceipt[0].id), '_blank');
    } catch { /* ignore */ }
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger className="inline-flex items-center justify-center h-8 w-8 rounded-md hover:bg-accent transition-colors">
        <MoreHorizontal className="h-4 w-4 text-muted-foreground" />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => router.push(`/orders/${order.id}`)}>
          <Eye className="mr-2 h-4 w-4" /> Voir le détail
        </DropdownMenuItem>
        {order.status !== 'Livree' && (
          <DropdownMenuItem onClick={() => router.push(`/orders/${order.id}/edit`)}>
            <Pencil className="mr-2 h-4 w-4" /> Modifier
          </DropdownMenuItem>
        )}
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleDownload}>
          <Download className="mr-2 h-4 w-4" /> Télécharger le reçu
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => window.open(`/api/orders/${order.id}/worksheet`, '_blank')}>
          <Download className="mr-2 h-4 w-4" /> Fiche de travail (PDF)
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleDelete} className="text-destructive focus:text-destructive">
          <Trash2 className="mr-2 h-4 w-4" /> Annuler la commande
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

const columns: ColumnDef<OrderSummary>[] = [
  {
    accessorKey: 'code',
    header: 'Code',
    cell: ({ row }) => (
      <span className="font-mono text-xs font-semibold text-muted-foreground">{row.original.code}</span>
    ),
  },
  {
    accessorKey: 'clientName',
    header: 'Client',
    cell: ({ row }) => (
      <span className="font-semibold">{row.original.clientName || '—'}</span>
    ),
  },
  {
    accessorKey: 'status',
    header: 'Statut',
    cell: ({ row }) => <StatusBadge status={row.original.status} label={row.original.statusLabel} />,
  },
  {
    accessorKey: 'workType',
    header: 'Type',
    cell: ({ row }) => <WorkTypeBadge workType={row.original.workType} label={row.original.workTypeLabel} />,
  },
  {
    accessorKey: 'expectedDeliveryDate',
    header: 'Livraison',
    cell: ({ row }) => (
      <div>
        <span className="text-sm">{formatDate(row.original.expectedDeliveryDate)}</span>
        {row.original.isLate && (
          <span className="ml-2 px-1.5 py-0.5 rounded-full bg-destructive text-white text-[10px] font-bold">
            {row.original.delayDays}J
          </span>
        )}
      </div>
    ),
  },
  {
    accessorKey: 'totalPrice',
    header: 'Prix',
    cell: ({ row }) => (
      <div>
        <span className="font-semibold">{formatDZD(row.original.totalPrice)}</span>
        {row.original.outstandingBalance > 0 && (
          <p className="text-[11px] text-chart-2">{formatDZD(row.original.outstandingBalance)} dû</p>
        )}
      </div>
    ),
  },
  {
    id: 'actions',
    header: '',
    cell: ({ row }) => <OrderActions order={row.original} />,
  },
];

export default function OrdersPage() {
  const { year: initY, quarter: initQ } = currentQuarter();
  const [year, setYear] = useState(initY);
  const [quarter, setQuarter] = useState<Quarter>(initQ);
  const [status, setStatus] = useState('all');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const router = useRouter();

  const dates = quarterDates(year, quarter);
  const { data, isLoading } = useQuery({
    queryKey: ['orders', year, quarter, status, search, page],
    queryFn: () => orders.list({
      page, pageSize: 20,
      status: status === 'all' ? undefined : status,
      search: search || undefined,
      dateFrom: dates.from, dateTo: dates.to,
    }),
  });

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-end justify-between">
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-1">Archives de l&apos;atelier</p>
          <h1 className="font-heading text-3xl font-bold">Commandes</h1>
        </div>
        <div className="flex items-center gap-2">
          <a href={dashboard.exportUrl(year, quarter, 'csv')} download
            className="inline-flex items-center gap-1.5 h-10 px-3 rounded-xl border border-border bg-card text-sm font-semibold hover:bg-secondary/50 transition-colors">
            <FileDown className="h-4 w-4 text-muted-foreground" /> CSV
          </a>
          <Button onClick={() => router.push('/orders/new')} className="rounded-full gap-2">
            <Plus className="h-4 w-4" /> Nouvelle commande
          </Button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex items-center gap-3">
        <QuarterSelect year={year} quarter={quarter} onChange={(y, q) => { setYear(y); setQuarter(q); setPage(1); }} />

        <Select value={status} onValueChange={(v) => { setStatus(v ?? 'all'); setPage(1); }}>
          <SelectTrigger className="w-[180px] h-10 rounded-xl">
            <SelectValue placeholder="Tous les statuts" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Tous les statuts</SelectItem>
            {ALL_STATUSES.map(s => <SelectItem key={s} value={s}>{STATUS_LABELS[s]}</SelectItem>)}
          </SelectContent>
        </Select>

        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
            placeholder="Rechercher par code, nom ou client..."
            className="pl-9 h-10 rounded-xl"
          />
        </div>
      </div>

      {/* Data table — no onRowClick, use actions menu instead */}
      <DataTable
        columns={columns}
        data={data?.items ?? []}
        totalCount={data?.totalCount ?? 0}
        page={page}
        pageSize={20}
        onPageChange={setPage}
        isLoading={isLoading}
        emptyMessage="Aucune commande trouvée"
      />
    </div>
  );
}
