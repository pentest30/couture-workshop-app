'use client';
import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { catalog, type CatalogModel } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useRouter } from 'next/navigation';
import { Plus, Search, Image as ImageIcon } from 'lucide-react';
import { formatDZD } from '@/lib/helpers';

const CATEGORIES = [
  { key: '', label: 'Tous' },
  { key: 'Ceremonie', label: 'Cérémonie' },
  { key: 'Quotidien', label: 'Quotidien' },
  { key: 'Mariee', label: 'Mariée' },
  { key: 'Traditionnel', label: 'Traditionnel' },
  { key: 'Moderne', label: 'Moderne' },
];

export default function CatalogPage() {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [category, setCategory] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['catalog-models', search, category, page],
    queryFn: () => catalog.listModels({ search: search || undefined, category: category || undefined, page, pageSize: 12 }),
  });

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-1">Bibliothèque</p>
          <h1 className="font-heading text-3xl font-bold">Catalogue</h1>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => router.push('/catalog/fabrics')} className="rounded-full gap-2">
            Tissus
          </Button>
          <Button onClick={() => router.push('/catalog/new')} className="rounded-full gap-2">
            <Plus className="h-4 w-4" /> Nouveau modèle
          </Button>
        </div>
      </div>

      {/* Category tabs + search */}
      <div className="flex items-center gap-4">
        <Tabs value={category} onValueChange={(v) => { setCategory(v); setPage(1); }}>
          <TabsList>
            {CATEGORIES.map(c => (
              <TabsTrigger key={c.key} value={c.key}>{c.label}</TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
        <div className="relative flex-1 max-w-xs ml-auto">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input value={search} onChange={e => { setSearch(e.target.value); setPage(1); }}
            placeholder="Rechercher..." className="pl-9 h-9 rounded-lg" />
        </div>
      </div>

      {/* Grid */}
      {isLoading ? (
        <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>
      ) : (
        <>
          <div className="grid grid-cols-4 gap-4">
            {data?.items?.map((m) => (
              <ModelCard key={m.id} model={m} onClick={() => router.push(`/catalog/${m.id}`)} />
            ))}
          </div>
          {data?.items?.length === 0 && (
            <div className="py-16 text-center text-muted-foreground">
              <ImageIcon className="w-12 h-12 mx-auto mb-3 opacity-30" />
              <p className="text-sm">Aucun modèle trouvé</p>
            </div>
          )}
          {data && data.totalCount > 12 && (
            <div className="flex items-center justify-center gap-2 pt-4">
              <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Précédent</Button>
              <span className="text-sm text-muted-foreground px-3">Page {page} / {Math.ceil(data.totalCount / 12)}</span>
              <Button variant="outline" size="sm" disabled={page * 12 >= data.totalCount} onClick={() => setPage(p => p + 1)}>Suivant</Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

function ModelCard({ model: m, onClick }: { model: CatalogModel; onClick: () => void }) {
  return (
    <button onClick={onClick} className="bg-card rounded-xl border overflow-hidden hover:shadow-lg hover:shadow-primary/5 transition-all text-left group">
      {/* Photo area */}
      <div className="aspect-[4/3] bg-muted flex items-center justify-center overflow-hidden">
        {m.primaryPhotoPath ? (
          <img src={m.primaryPhotoPath} alt={m.name} className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300" />
        ) : (
          <ImageIcon className="w-10 h-10 text-muted-foreground/30" />
        )}
      </div>
      {/* Info */}
      <div className="p-4">
        <div className="flex items-center gap-2 mb-1.5">
          <Badge variant="secondary" className="text-[10px]">{m.categoryLabel}</Badge>
          {m.isPublic && <Badge variant="outline" className="text-[10px]">Public</Badge>}
        </div>
        <p className="font-mono text-[10px] text-muted-foreground mb-0.5">{m.code}</p>
        <p className="font-heading text-sm font-semibold truncate">{m.name}</p>
        <div className="flex items-center justify-between mt-2">
          <span className="text-sm font-semibold text-chart-2">{formatDZD(m.basePrice)}</span>
          <span className="text-xs text-muted-foreground">{m.estimatedDays}j</span>
        </div>
      </div>
    </button>
  );
}
