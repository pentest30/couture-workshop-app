'use client';
import { useState, useEffect, useCallback } from 'react';
import { clients as clientsApi, orders, catalog, type Client, type CatalogModel } from '@/lib/api';
import { useRouter, useSearchParams } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { useQuery } from '@tanstack/react-query';
import { formatDZD } from '@/lib/helpers';
import { BookOpen, Image as ImageIcon, X } from 'lucide-react';

const WORK_TYPES = [
  { key: 'Simple', label: 'Simple', desc: 'Couture standard' },
  { key: 'Brode', label: 'Brodé', desc: 'Avec broderie' },
  { key: 'Perle', label: 'Perlé', desc: 'Avec perlage' },
  { key: 'Mixte', label: 'Mixte', desc: 'Broderie + perlage' },
];

export default function NewOrderPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [step, setStep] = useState(1);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  // Step 1: Client
  const [clientSearch, setClientSearch] = useState('');
  const [searchResults, setSearchResults] = useState<Client[]>([]);
  const [selectedClient, setSelectedClient] = useState<Client | null>(null);

  // Catalog model (optional)
  const [selectedModel, setSelectedModel] = useState<CatalogModel | null>(null);
  const [showCatalog, setShowCatalog] = useState(false);

  // Step 2: Work details
  const [workType, setWorkType] = useState('Simple');
  const [description, setDescription] = useState('');
  const [fabric, setFabric] = useState('');
  const [technicalNotes, setTechnicalNotes] = useState('');
  const [embroideryStyle, setEmbroideryStyle] = useState('');
  const [threadColors, setThreadColors] = useState('');
  const [beadType, setBeadType] = useState('');

  // Step 3: Planning
  const [deliveryDate, setDeliveryDate] = useState('');
  const [totalPrice, setTotalPrice] = useState('');
  const [deposit, setDeposit] = useState('');

  // Pre-fill from URL params (when coming from catalog detail "Créer commande")
  useEffect(() => {
    const wt = searchParams.get('workType');
    const desc = searchParams.get('description');
    const price = searchParams.get('price');
    const modelId = searchParams.get('modelId');
    if (wt) setWorkType(wt);
    if (desc) setDescription(desc);
    if (price) setTotalPrice(price);
    if (modelId) {
      catalog.listModels({ pageSize: 1, search: '' }).then(data => {
        // We'll just store the ID, the form is already pre-filled
      }).catch(() => {});
    }
  }, [searchParams]);

  const applyModel = (model: CatalogModel) => {
    setSelectedModel(model);
    setWorkType(model.workType);
    setDescription(model.name);
    setTotalPrice(String(model.basePrice));
    setShowCatalog(false);
  };

  const clearModel = () => {
    setSelectedModel(null);
    setWorkType('Simple');
    setDescription('');
    setTotalPrice('');
  };

  const searchClients = useCallback(async (q: string) => {
    if (q.length < 2) { setSearchResults([]); return; }
    try {
      const results = await clientsApi.search(q);
      setSearchResults(results);
    } catch { setSearchResults([]); }
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => searchClients(clientSearch), 300);
    return () => clearTimeout(timer);
  }, [clientSearch, searchClients]);

  const handleSubmit = async () => {
    setSaving(true); setError('');
    try {
      const data: Record<string, unknown> = {
        clientId: selectedClient!.id, workType, description, fabric, technicalNotes,
        expectedDeliveryDate: deliveryDate, totalPrice: parseFloat(totalPrice),
        catalogModelId: selectedModel?.id || undefined,
      };
      if (deposit) data.initialDeposit = parseFloat(deposit);
      if (['Brode', 'Mixte'].includes(workType)) {
        data.embroideryStyle = embroideryStyle;
        data.threadColors = threadColors;
      }
      if (['Perle', 'Mixte'].includes(workType)) data.beadType = beadType;
      const result = await orders.create(data);
      router.push(`/orders/${result.orderId}`);
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false);
  };

  const canNext = step === 1 ? !!selectedClient : step === 2 ? !!workType : !!deliveryDate && !!totalPrice;

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <p className="text-xs font-semibold tracking-widest uppercase text-muted-foreground mb-1">Nouvelle commande</p>
        <h1 className="font-heading text-3xl font-bold">Créer une commande</h1>
      </div>

      {/* Steps */}
      <div className="flex items-center gap-3">
        {['Client', 'Modèle & Détails', 'Planification'].map((s, i) => (
          <div key={s} className="flex items-center gap-2">
            <div className={`w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold
              ${step > i + 1 ? 'bg-chart-1 text-white' : step === i + 1 ? 'bg-primary text-white' : 'bg-muted text-muted-foreground'}`}>
              {step > i + 1 ? '✓' : i + 1}
            </div>
            <span className={`text-sm ${step === i + 1 ? 'font-semibold' : 'text-muted-foreground'}`}>{s}</span>
            {i < 2 && <div className="w-12 h-px bg-border" />}
          </div>
        ))}
      </div>

      <div className="bg-card rounded-2xl p-6 border border-border/20">
        {/* Step 1: Client */}
        {step === 1 && (
          <div className="space-y-4">
            <h3 className="font-heading text-lg font-semibold">Sélectionner un client</h3>
            {selectedClient ? (
              <div className="flex items-center gap-3 p-4 rounded-xl bg-primary/5 border border-primary/15">
                <div className="w-10 h-10 rounded-full bg-primary flex items-center justify-center text-white font-semibold text-sm">
                  {selectedClient.firstName[0]}{selectedClient.lastName[0]}
                </div>
                <div className="flex-1">
                  <p className="font-semibold">{selectedClient.firstName} {selectedClient.lastName}</p>
                  <p className="text-xs text-muted-foreground">{selectedClient.code} — {selectedClient.primaryPhone}</p>
                </div>
                <button onClick={() => setSelectedClient(null)} className="text-xs text-destructive font-semibold">Changer</button>
              </div>
            ) : (
              <>
                <Input value={clientSearch} onChange={e => setClientSearch(e.target.value)} placeholder="Rechercher par nom ou téléphone..." />
                {searchResults.length > 0 && (
                  <div className="space-y-1 max-h-60 overflow-y-auto">
                    {searchResults.map(c => (
                      <button key={c.id} onClick={() => { setSelectedClient(c); setSearchResults([]); setClientSearch(''); }}
                        className="w-full flex items-center gap-3 p-3 rounded-xl hover:bg-secondary transition-colors text-left">
                        <div className="w-8 h-8 rounded-full bg-primary/30 flex items-center justify-center text-primary text-xs font-bold">{c.firstName[0]}{c.lastName[0]}</div>
                        <div><p className="text-sm font-medium">{c.firstName} {c.lastName}</p><p className="text-xs text-muted-foreground">{c.code}</p></div>
                      </button>
                    ))}
                  </div>
                )}
              </>
            )}
          </div>
        )}

        {/* Step 2: Model + Work details */}
        {step === 2 && (
          <div className="space-y-5">
            {/* Catalog model selector */}
            <div>
              <h3 className="font-heading text-lg font-semibold mb-3">Modèle du catalogue</h3>
              {selectedModel ? (
                <div className="flex items-center gap-3 p-4 rounded-xl bg-chart-2/5 border border-chart-2/15">
                  <div className="w-12 h-12 rounded-lg bg-muted flex items-center justify-center overflow-hidden">
                    {selectedModel.primaryPhotoPath ? (
                      <img src={selectedModel.primaryPhotoPath} alt="" className="w-full h-full object-cover" />
                    ) : (
                      <ImageIcon className="w-5 h-5 text-muted-foreground" />
                    )}
                  </div>
                  <div className="flex-1">
                    <p className="font-semibold">{selectedModel.name}</p>
                    <p className="text-xs text-muted-foreground">{selectedModel.categoryLabel} — {formatDZD(selectedModel.basePrice)}</p>
                  </div>
                  <button onClick={clearModel} className="p-1 rounded hover:bg-muted"><X className="h-4 w-4" /></button>
                </div>
              ) : (
                <Button variant="outline" onClick={() => setShowCatalog(true)} className="w-full gap-2 h-12">
                  <BookOpen className="h-4 w-4" /> Choisir un modèle du catalogue
                </Button>
              )}

              {showCatalog && <CatalogPicker onSelect={applyModel} onClose={() => setShowCatalog(false)} />}
            </div>

            <div className="border-t pt-4">
              <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2">
                {selectedModel ? 'Personnaliser les détails' : 'Ou saisir manuellement'}
              </p>
            </div>

            <div>
              <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2">Type de travail</p>
              <div className="grid grid-cols-2 gap-2">
                {WORK_TYPES.map(wt => (
                  <button key={wt.key} onClick={() => setWorkType(wt.key)}
                    className={`p-3 rounded-xl border text-left transition-all
                      ${workType === wt.key ? 'border-primary bg-primary/5' : 'border-border/30 hover:bg-secondary'}`}>
                    <p className={`text-sm font-semibold ${workType === wt.key ? 'text-primary' : ''}`}>{wt.label}</p>
                    <p className="text-xs text-muted-foreground">{wt.desc}</p>
                  </button>
                ))}
              </div>
            </div>
            <div>
              <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Description</label>
              <textarea value={description} onChange={e => setDescription(e.target.value)} rows={3}
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm resize-none
                  ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                placeholder="Robe de soirée, caftan..." />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div><label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Tissu</label>
                <Input value={fabric} onChange={e => setFabric(e.target.value)} placeholder="Satin, velours..." /></div>
              <div><label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Notes techniques</label>
                <Input value={technicalNotes} onChange={e => setTechnicalNotes(e.target.value)} /></div>
            </div>
            {['Brode', 'Mixte'].includes(workType) && (
              <div className="grid grid-cols-2 gap-4 pt-2 border-t">
                <div><label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Style broderie</label>
                  <Input value={embroideryStyle} onChange={e => setEmbroideryStyle(e.target.value)} /></div>
                <div><label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Couleurs fils</label>
                  <Input value={threadColors} onChange={e => setThreadColors(e.target.value)} /></div>
              </div>
            )}
            {['Perle', 'Mixte'].includes(workType) && (
              <div className="pt-2 border-t">
                <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Type de perle</label>
                <Input value={beadType} onChange={e => setBeadType(e.target.value)} />
              </div>
            )}
          </div>
        )}

        {/* Step 3: Planning */}
        {step === 3 && (
          <div className="space-y-5">
            <h3 className="font-heading text-lg font-semibold">Planification & prix</h3>
            <div className="grid grid-cols-2 gap-4">
              <div><label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Date de livraison</label>
                <Input type="date" value={deliveryDate} onChange={e => setDeliveryDate(e.target.value)} /></div>
              <div><label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Prix total (DZD)</label>
                <Input type="number" value={totalPrice} onChange={e => setTotalPrice(e.target.value)} className="font-heading font-semibold" /></div>
            </div>
            <div><label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Acompte initial (DZD, optionnel)</label>
              <Input type="number" value={deposit} onChange={e => setDeposit(e.target.value)} /></div>
          </div>
        )}

        {error && <p className="mt-3 text-sm text-destructive font-medium">{error}</p>}

        {/* Navigation */}
        <div className="flex gap-2 mt-6 pt-4 border-t">
          {step > 1 && <Button variant="outline" onClick={() => setStep(s => s - 1)} className="rounded-full">Retour</Button>}
          <div className="flex-1" />
          {step < 3 ? (
            <Button onClick={() => setStep(s => s + 1)} disabled={!canNext} className="rounded-full">Suivant</Button>
          ) : (
            <Button onClick={handleSubmit} disabled={!canNext || saving} className="rounded-full px-8">
              {saving ? 'Création...' : 'Créer la commande'}
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}

function CatalogPicker({ onSelect, onClose }: { onSelect: (m: CatalogModel) => void; onClose: () => void }) {
  const [search, setSearch] = useState('');
  const { data } = useQuery({
    queryKey: ['catalog-picker', search],
    queryFn: () => catalog.listModels({ search: search || undefined, pageSize: 8 }),
  });

  return (
    <div className="mt-3 p-4 rounded-xl border bg-card space-y-3">
      <div className="flex items-center justify-between">
        <p className="text-sm font-semibold">Catalogue de modèles</p>
        <button onClick={onClose} className="p-1 rounded hover:bg-muted"><X className="h-4 w-4" /></button>
      </div>
      <Input value={search} onChange={e => setSearch(e.target.value)} placeholder="Rechercher un modèle..." className="h-9" />
      <div className="grid grid-cols-2 gap-2 max-h-64 overflow-y-auto">
        {data?.items?.map(m => (
          <button key={m.id} onClick={() => onSelect(m)}
            className="flex items-center gap-3 p-3 rounded-lg border hover:bg-accent transition-colors text-left">
            <div className="w-10 h-10 rounded-lg bg-muted flex items-center justify-center overflow-hidden shrink-0">
              {m.primaryPhotoPath ? (
                <img src={m.primaryPhotoPath} alt="" className="w-full h-full object-cover" />
              ) : (
                <ImageIcon className="w-4 h-4 text-muted-foreground" />
              )}
            </div>
            <div className="flex-1 min-w-0">
              <p className="text-sm font-semibold truncate">{m.name}</p>
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="text-[9px]">{m.categoryLabel}</Badge>
                <span className="text-xs text-chart-2 font-semibold">{formatDZD(m.basePrice)}</span>
              </div>
            </div>
          </button>
        ))}
        {data?.items?.length === 0 && <p className="col-span-2 text-center text-sm text-muted-foreground py-4">Aucun modèle</p>}
      </div>
    </div>
  );
}
