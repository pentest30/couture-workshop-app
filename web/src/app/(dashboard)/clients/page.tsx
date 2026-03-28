'use client';
import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { clients, type Client } from '@/lib/api';
import { DataTable } from '@/components/ui/data-table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';
import { useRouter } from 'next/navigation';
import { Search, Phone, Plus, MoreHorizontal, Eye, Pencil, Trash2 } from 'lucide-react';
import { type ColumnDef } from '@tanstack/react-table';

function ClientActions({ client }: { client: Client }) {
  const router = useRouter();
  const queryClient = useQueryClient();

  const handleDelete = async () => {
    if (!confirm(`Supprimer le client ${client.firstName} ${client.lastName} ?`)) return;
    try {
      await clients.deactivate(client.id);
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    } catch (e) { alert(e instanceof Error ? e.message : 'Erreur'); }
  };

  return (
    <DropdownMenu>
      <DropdownMenuTrigger className="inline-flex items-center justify-center h-8 w-8 rounded-md hover:bg-accent transition-colors">
        <MoreHorizontal className="h-4 w-4 text-muted-foreground" />
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => router.push(`/clients/${client.id}`)}>
          <Eye className="mr-2 h-4 w-4" /> Voir le profil
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => router.push(`/clients/${client.id}/edit`)}>
          <Pencil className="mr-2 h-4 w-4" /> Modifier
        </DropdownMenuItem>
        <DropdownMenuSeparator />
        <DropdownMenuItem onClick={handleDelete} className="text-destructive focus:text-destructive">
          <Trash2 className="mr-2 h-4 w-4" /> Supprimer
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

const columns: ColumnDef<Client>[] = [
  {
    accessorKey: 'code',
    header: 'Code',
    cell: ({ row }) => <span className="font-mono text-xs font-semibold text-muted-foreground">{row.original.code}</span>,
  },
  {
    id: 'name',
    header: 'Nom',
    cell: ({ row }) => (
      <div className="flex items-center gap-3">
        <div className="w-8 h-8 rounded-full bg-primary flex items-center justify-center text-primary-foreground text-xs font-bold shrink-0">
          {row.original.firstName?.[0]}{row.original.lastName?.[0]}
        </div>
        <span className="font-semibold">{row.original.firstName} {row.original.lastName}</span>
      </div>
    ),
  },
  {
    accessorKey: 'primaryPhone',
    header: 'Téléphone',
    cell: ({ row }) => (
      <div className="flex items-center gap-1.5 text-sm text-muted-foreground">
        <Phone className="h-3.5 w-3.5" />
        {row.original.primaryPhone}
      </div>
    ),
  },
  {
    accessorKey: 'address',
    header: 'Adresse',
    cell: ({ row }) => (
      <span className="text-sm text-muted-foreground truncate max-w-[200px] block">
        {row.original.address || '—'}
      </span>
    ),
  },
  {
    accessorKey: 'notes',
    header: 'Notes',
    cell: ({ row }) => (
      <span className="text-xs text-muted-foreground truncate max-w-[150px] block">
        {row.original.notes || '—'}
      </span>
    ),
  },
  {
    id: 'actions',
    header: '',
    cell: ({ row }) => <ClientActions client={row.original} />,
  },
];

export default function ClientsPage() {
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const router = useRouter();

  const { data, isLoading } = useQuery({
    queryKey: ['clients', search, page],
    queryFn: () => clients.list({ page, search: search || undefined }),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-1">Répertoire</p>
          <h1 className="font-heading text-3xl font-bold">Clients</h1>
        </div>
        <Button onClick={() => router.push('/clients/new')} className="rounded-full gap-2">
          <Plus className="h-4 w-4" /> Nouveau client
        </Button>
      </div>

      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
          placeholder="Rechercher par nom, code ou téléphone..."
          className="pl-9 h-10 rounded-xl" />
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        totalCount={data?.totalCount ?? 0}
        page={page}
        pageSize={20}
        onPageChange={setPage}
        isLoading={isLoading}
        emptyMessage="Aucun client trouvé"
      />
    </div>
  );
}
