'use client';
import { use, useState, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { catalog, uploadFile } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Trash2, Plus, LinkIcon, Upload } from 'lucide-react';
import { useRef } from 'react';
import { formatDZD } from '@/lib/helpers';

const CATEGORIES = ['Ceremonie', 'Quotidien', 'Mariee', 'Traditionnel', 'Moderne'];
const CAT_LABELS: Record<string, string> = { Ceremonie: 'Cérémonie', Quotidien: 'Quotidien', Mariee: 'Mariée', Traditionnel: 'Traditionnel', Moderne: 'Moderne' };
const WORK_TYPES = ['Simple', 'Brode', 'Perle', 'Mixte'];
const WT_LABELS: Record<string, string> = { Simple: 'Simple', Brode: 'Brodé', Perle: 'Perlé', Mixte: 'Mixte' };

export default function EditModelPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const queryClient = useQueryClient();
  const { data: model } = useQuery({ queryKey: ['catalog-model', id], queryFn: () => catalog.getModel(id) });

  const [name, setName] = useState('');
  const [category, setCategory] = useState('');
  const [workType, setWorkType] = useState('');
  const [basePrice, setBasePrice] = useState('');
  const [estimatedDays, setEstimatedDays] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Photo upload
  const fileRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);

  // Fabric link
  const [showFabricLink, setShowFabricLink] = useState(false);
  const [fabricSearch, setFabricSearch] = useState('');
  const { data: fabricResults } = useQuery({
    queryKey: ['fabric-search', fabricSearch],
    queryFn: () => catalog.listFabrics({ search: fabricSearch || undefined, pageSize: 10 }),
    enabled: showFabricLink && fabricSearch.length >= 1,
  });

  useEffect(() => {
    if (model) {
      setName(model.name);
      setCategory(model.category);
      setWorkType(model.workType);
      setBasePrice(String(model.basePrice));
      setEstimatedDays(String(model.estimatedDays));
      setDescription(model.description || '');
      setIsPublic(model.isPublic);
    }
  }, [model]);

  if (!model) return <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>;

  const handleSave = async () => {
    if (!name.trim()) { setError('Le nom est obligatoire'); return; }
    setSaving(true); setError(''); setSuccess('');
    try {
      await catalog.updateModel(id, {
        name: name.trim(), category, workType,
        basePrice: parseFloat(basePrice) || 0,
        estimatedDays: parseInt(estimatedDays) || 7,
        isPublic, description: description.trim() || undefined,
      });
      queryClient.invalidateQueries({ queryKey: ['catalog-model', id] });
      setSuccess('Modèle mis à jour');
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false);
  };

  const handleUploadPhotos = async (files: FileList | null) => {
    if (!files) return;
    setUploading(true);
    for (let i = 0; i < files.length; i++) {
      try {
        const uploaded = await uploadFile(files[i]);
        await catalog.addPhoto(id, { fileName: uploaded.fileName, storagePath: uploaded.storagePath, sortOrder: model.photos.length + i });
      } catch { /* continue */ }
    }
    queryClient.invalidateQueries({ queryKey: ['catalog-model', id] });
    setUploading(false);
  };

  const handleRemovePhoto = async (photoId: string) => {
    try {
      await catalog.removePhoto(id, photoId);
      queryClient.invalidateQueries({ queryKey: ['catalog-model', id] });
    } catch { /* ignore */ }
  };

  const handleLinkFabric = async (fabricId: string) => {
    try {
      await catalog.linkFabric(id, fabricId);
      queryClient.invalidateQueries({ queryKey: ['catalog-model', id] });
      setShowFabricLink(false); setFabricSearch('');
    } catch { /* ignore */ }
  };

  const handleUnlinkFabric = async (fabricId: string) => {
    try {
      await catalog.unlinkFabric(id, fabricId);
      queryClient.invalidateQueries({ queryKey: ['catalog-model', id] });
    } catch { /* ignore */ }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Link href={`/catalog/${id}`} className="p-2 rounded-lg hover:bg-muted transition-colors">
          <ArrowLeft className="h-5 w-5 text-muted-foreground" />
        </Link>
        <div className="flex-1">
          <p className="font-mono text-xs text-muted-foreground">{model.code}</p>
          <h1 className="font-heading text-2xl font-bold">Modifier le modèle</h1>
        </div>
        <Link href={`/api/catalog/${id}/pdf`} target="_blank">
          <Button variant="outline" size="sm" className="gap-1.5">Fiche PDF</Button>
        </Link>
      </div>

      <div className="grid grid-cols-3 gap-6">
        {/* Left: form — 2 cols */}
        <div className="col-span-2 space-y-6">
          <Card>
            <CardHeader><CardTitle>Informations</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div>
                <Label>Nom du modèle *</Label>
                <Input value={name} onChange={e => setName(e.target.value)} className="mt-1" />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <Label>Catégorie</Label>
                  <Select value={category} onValueChange={v => v && setCategory(v)}>
                    <SelectTrigger className="mt-1"><SelectValue /></SelectTrigger>
                    <SelectContent>{CATEGORIES.map(c => <SelectItem key={c} value={c}>{CAT_LABELS[c]}</SelectItem>)}</SelectContent>
                  </Select>
                </div>
                <div>
                  <Label>Type de travail</Label>
                  <Select value={workType} onValueChange={v => v && setWorkType(v)}>
                    <SelectTrigger className="mt-1"><SelectValue /></SelectTrigger>
                    <SelectContent>{WORK_TYPES.map(w => <SelectItem key={w} value={w}>{WT_LABELS[w]}</SelectItem>)}</SelectContent>
                  </Select>
                </div>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div><Label>Prix de base (DZD)</Label><Input type="number" value={basePrice} onChange={e => setBasePrice(e.target.value)} className="mt-1" /></div>
                <div><Label>Durée estimée (jours)</Label><Input type="number" value={estimatedDays} onChange={e => setEstimatedDays(e.target.value)} className="mt-1" /></div>
              </div>
              <div>
                <Label>Description</Label>
                <textarea value={description} onChange={e => setDescription(e.target.value)} rows={3}
                  className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm mt-1 resize-none ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring" />
              </div>
              <div className="flex items-center gap-3">
                <Switch checked={isPublic} onCheckedChange={setIsPublic} />
                <Label>Visible dans le catalogue public</Label>
              </div>

              {error && <p className="text-sm text-destructive">{error}</p>}
              {success && <p className="text-sm text-chart-1 font-medium">{success}</p>}

              <Button onClick={handleSave} disabled={saving} className="rounded-full px-8">
                {saving ? 'Enregistrement...' : 'Enregistrer'}
              </Button>
            </CardContent>
          </Card>

          {/* Photos */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between">
                Photos ({model.photos.length})
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              {model.photos.length > 0 && (
                <div className="grid grid-cols-4 gap-2">
                  {model.photos.map(p => (
                    <div key={p.id} className="relative group aspect-square rounded-lg bg-muted overflow-hidden">
                      <img src={p.storagePath} alt={p.fileName} className="w-full h-full object-cover" />
                      <button onClick={() => handleRemovePhoto(p.id)}
                        className="absolute top-1 right-1 p-1 rounded bg-destructive/80 text-white opacity-0 group-hover:opacity-100 transition-opacity">
                        <Trash2 className="h-3 w-3" />
                      </button>
                    </div>
                  ))}
                </div>
              )}
              <input ref={fileRef} type="file" accept="image/*" multiple className="hidden"
                onChange={e => handleUploadPhotos(e.target.files)} />
              <button onClick={() => fileRef.current?.click()} disabled={uploading}
                className="w-full border-2 border-dashed rounded-xl p-4 flex flex-col items-center gap-1.5 hover:bg-muted/50 hover:border-primary/30 transition-colors disabled:opacity-50">
                <Upload className="h-5 w-5 text-muted-foreground" />
                <p className="text-xs text-muted-foreground font-medium">
                  {uploading ? 'Upload en cours...' : 'Ajouter des photos'}
                </p>
              </button>
            </CardContent>
          </Card>
        </div>

        {/* Right sidebar: fabrics */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center justify-between text-base">
                Tissus liés
                <Button variant="ghost" size="icon" className="h-7 w-7" onClick={() => setShowFabricLink(!showFabricLink)}>
                  <LinkIcon className="h-4 w-4" />
                </Button>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-2">
              {showFabricLink && (
                <div className="mb-3 p-3 rounded-lg border bg-muted/50 space-y-2">
                  <Input value={fabricSearch} onChange={e => setFabricSearch(e.target.value)} placeholder="Rechercher un tissu..." className="h-8 text-xs" />
                  {fabricResults?.items?.filter(f => !model.fabrics.some(mf => mf.id === f.id)).map(f => (
                    <button key={f.id} onClick={() => handleLinkFabric(f.id)}
                      className="w-full flex items-center gap-2 p-2 rounded-md hover:bg-accent text-left text-xs">
                      <div className="w-4 h-4 rounded border" style={{ backgroundColor: f.color }} />
                      <span className="font-medium flex-1 truncate">{f.name}</span>
                      <Plus className="h-3 w-3 text-muted-foreground" />
                    </button>
                  ))}
                </div>
              )}
              {model.fabrics.map(f => (
                <div key={f.id} className="flex items-center gap-2 p-2 rounded-lg bg-muted">
                  <div className="w-6 h-6 rounded border" style={{ backgroundColor: f.color }} />
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-semibold truncate">{f.name}</p>
                    <p className="text-[10px] text-muted-foreground">{f.type} — {formatDZD(f.pricePerMeter)}/m</p>
                  </div>
                  <button onClick={() => handleUnlinkFabric(f.id)} className="p-1 rounded hover:bg-destructive/10">
                    <Trash2 className="h-3 w-3 text-destructive" />
                  </button>
                </div>
              ))}
              {model.fabrics.length === 0 && !showFabricLink && (
                <p className="text-xs text-muted-foreground text-center py-2">Aucun tissu lié</p>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
