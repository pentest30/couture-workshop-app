'use client';
import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { catalog, type CatalogFabric } from '@/lib/api';
import { DataTable } from '@/components/ui/data-table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { formatDZD } from '@/lib/helpers';
import Link from 'next/link';
import { Search, Plus, ArrowLeft } from 'lucide-react';
import { type ColumnDef } from '@tanstack/react-table';

const columns: ColumnDef<CatalogFabric>[] = [
  {
    id: 'swatch',
    header: '',
    cell: ({ row }) => (
      <div className="w-8 h-8 rounded-lg border" style={{ backgroundColor: row.original.color }} />
    ),
  },
  { accessorKey: 'name', header: 'Nom', cell: ({ row }) => <span className="font-semibold">{row.original.name}</span> },
  { accessorKey: 'type', header: 'Type', cell: ({ row }) => <span className="text-sm text-muted-foreground">{row.original.type}</span> },
  { accessorKey: 'color', header: 'Couleur' },
  { accessorKey: 'supplier', header: 'Fournisseur', cell: ({ row }) => <span className="text-sm text-muted-foreground">{row.original.supplier || '—'}</span> },
  { accessorKey: 'pricePerMeter', header: 'Prix/m', cell: ({ row }) => <span className="font-semibold">{formatDZD(row.original.pricePerMeter)}</span> },
  { accessorKey: 'stockMeters', header: 'Stock', cell: ({ row }) => <span>{row.original.stockMeters}m</span> },
];

export default function FabricsPage() {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ['catalog-fabrics', search, page],
    queryFn: () => catalog.listFabrics({ search: search || undefined, page, pageSize: 20 }),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/catalog" className="p-2 rounded-lg hover:bg-muted transition-colors">
          <ArrowLeft className="h-5 w-5 text-muted-foreground" />
        </Link>
        <div className="flex-1">
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-0.5">Catalogue</p>
          <h1 className="font-heading text-2xl font-bold">Tissus</h1>
        </div>
        <Button onClick={() => setShowCreate(true)} className="rounded-full gap-2">
          <Plus className="h-4 w-4" /> Nouveau tissu
        </Button>
      </div>

      <div className="relative max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
          placeholder="Rechercher par nom, type ou couleur..." className="pl-9 h-10 rounded-xl" />
      </div>

      <DataTable columns={columns} data={data?.items ?? []} totalCount={data?.totalCount ?? 0}
        page={page} pageSize={20} onPageChange={setPage} isLoading={isLoading}
        emptyMessage="Aucun tissu trouvé" />

      {showCreate && (
        <CreateFabricDialog open={showCreate} onClose={() => setShowCreate(false)}
          onCreated={() => { setShowCreate(false); queryClient.invalidateQueries({ queryKey: ['catalog-fabrics'] }); }} />
      )}
    </div>
  );
}

function CreateFabricDialog({ open, onClose, onCreated }: { open: boolean; onClose: () => void; onCreated: () => void }) {
  const [name, setName] = useState('');
  const [type, setType] = useState('');
  const [color, setColor] = useState('#000000');
  const [price, setPrice] = useState('');
  const [stock, setStock] = useState('');
  const [supplier, setSupplier] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async () => {
    if (!name.trim() || !type.trim() || !price) { setError('Nom, type et prix sont obligatoires'); return; }
    setSaving(true); setError('');
    try {
      await catalog.createFabric({
        name: name.trim(), type: type.trim(), color,
        pricePerMeter: parseFloat(price), stockMeters: parseFloat(stock) || 0,
        supplier: supplier.trim() || undefined,
      });
      onCreated();
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false);
  };

  return (
    <Dialog open={open} onOpenChange={v => { if (!v) onClose(); }}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader><DialogTitle className="font-heading">Nouveau tissu</DialogTitle></DialogHeader>
        <div className="space-y-3 mt-2">
          <div className="grid grid-cols-2 gap-3">
            <div><Label>Nom *</Label><Input value={name} onChange={e => setName(e.target.value)} placeholder="Satin" className="mt-1" /></div>
            <div><Label>Type *</Label><Input value={type} onChange={e => setType(e.target.value)} placeholder="Soie, Coton..." className="mt-1" /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><Label>Couleur</Label><Input type="color" value={color} onChange={e => setColor(e.target.value)} className="mt-1 h-10" /></div>
            <div><Label>Fournisseur</Label><Input value={supplier} onChange={e => setSupplier(e.target.value)} className="mt-1" /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><Label>Prix/mètre (DZD) *</Label><Input type="number" value={price} onChange={e => setPrice(e.target.value)} className="mt-1" /></div>
            <div><Label>Stock (mètres)</Label><Input type="number" value={stock} onChange={e => setStock(e.target.value)} className="mt-1" /></div>
          </div>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="flex gap-2 pt-2">
            <Button variant="outline" onClick={onClose} className="flex-1">Annuler</Button>
            <Button onClick={handleSubmit} disabled={saving} className="flex-1">{saving ? 'Création...' : 'Créer'}</Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
