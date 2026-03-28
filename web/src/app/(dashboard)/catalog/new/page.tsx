'use client';
import { useState, useRef } from 'react';
import { catalog, uploadFile } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { Switch } from '@/components/ui/switch';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, Upload, X, Image as ImageIcon } from 'lucide-react';

const CATEGORIES = ['Ceremonie', 'Quotidien', 'Mariee', 'Traditionnel', 'Moderne'];
const CAT_LABELS: Record<string, string> = { Ceremonie: 'Cérémonie', Quotidien: 'Quotidien', Mariee: 'Mariée', Traditionnel: 'Traditionnel', Moderne: 'Moderne' };
const WORK_TYPES = ['Simple', 'Brode', 'Perle', 'Mixte'];
const WT_LABELS: Record<string, string> = { Simple: 'Simple', Brode: 'Brodé', Perle: 'Perlé', Mixte: 'Mixte' };

interface PendingPhoto { file: File; preview: string; }

export default function NewModelPage() {
  const router = useRouter();
  const fileRef = useRef<HTMLInputElement>(null);
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [category, setCategory] = useState('Ceremonie');
  const [workType, setWorkType] = useState('Simple');
  const [basePrice, setBasePrice] = useState('');
  const [estimatedDays, setEstimatedDays] = useState('');
  const [description, setDescription] = useState('');
  const [isPublic, setIsPublic] = useState(true);
  const [photos, setPhotos] = useState<PendingPhoto[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [uploadProgress, setUploadProgress] = useState('');

  const handleFiles = (files: FileList | null) => {
    if (!files) return;
    const newPhotos = Array.from(files).map(file => ({
      file,
      preview: URL.createObjectURL(file),
    }));
    setPhotos(prev => [...prev, ...newPhotos]);
  };

  const removePhoto = (index: number) => {
    setPhotos(prev => {
      URL.revokeObjectURL(prev[index].preview);
      return prev.filter((_, i) => i !== index);
    });
  };

  const handleSubmit = async () => {
    if (!name.trim() || !basePrice) { setError('Nom et prix sont obligatoires'); return; }
    setSaving(true); setError('');

    try {
      // 1. Create model
      const result = await catalog.createModel({
        name: name.trim(), category, workType,
        basePrice: parseFloat(basePrice),
        estimatedDays: parseInt(estimatedDays) || 7,
        isPublic, description: description.trim() || undefined,
        code: code.trim() || undefined,
      });

      // 2. Upload photos and attach
      for (let i = 0; i < photos.length; i++) {
        setUploadProgress(`Upload photo ${i + 1}/${photos.length}...`);
        try {
          const uploaded = await uploadFile(photos[i].file);
          await catalog.addPhoto(result.id, {
            fileName: uploaded.fileName,
            storagePath: uploaded.storagePath,
            sortOrder: i,
          });
        } catch { /* continue with other photos */ }
      }

      router.push(`/catalog/${result.id}`);
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false); setUploadProgress('');
  };

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/catalog" className="p-2 rounded-lg hover:bg-muted transition-colors">
          <ArrowLeft className="h-5 w-5 text-muted-foreground" />
        </Link>
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-0.5">Catalogue</p>
          <h1 className="font-heading text-2xl font-bold">Nouveau modèle</h1>
        </div>
      </div>

      <div className="grid grid-cols-5 gap-6">
        {/* Left: form — 3 cols */}
        <div className="col-span-3">
          <Card>
            <CardHeader><CardTitle>Informations</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div>
                <Label>Code (optionnel)</Label>
                <Input value={code} onChange={e => setCode(e.target.value)} placeholder="Laissez vide pour auto-générer (MOD-2026-0001)" className="mt-1 font-mono" />
                <p className="text-[11px] text-muted-foreground mt-1">Votre propre référence, ex: CAF-001, RDS-2026-A</p>
              </div>
              <div>
                <Label>Nom du modèle *</Label>
                <Input value={name} onChange={e => setName(e.target.value)} placeholder="Caftan cérémonie, Robe de soirée..." className="mt-1" />
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
                <div><Label>Prix de base (DZD) *</Label><Input type="number" value={basePrice} onChange={e => setBasePrice(e.target.value)} className="mt-1" /></div>
                <div><Label>Durée estimée (jours)</Label><Input type="number" value={estimatedDays} onChange={e => setEstimatedDays(e.target.value)} placeholder="7" className="mt-1" /></div>
              </div>
              <div>
                <Label>Description</Label>
                <textarea value={description} onChange={e => setDescription(e.target.value)} rows={3} placeholder="Description du modèle..."
                  className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm mt-1 resize-none ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring" />
              </div>
              <div className="flex items-center gap-3">
                <Switch checked={isPublic} onCheckedChange={setIsPublic} />
                <Label>Visible dans le catalogue public</Label>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Right: photos — 2 cols */}
        <div className="col-span-2">
          <Card>
            <CardHeader><CardTitle>Photos</CardTitle></CardHeader>
            <CardContent className="space-y-3">
              {/* Photo grid */}
              {photos.length > 0 && (
                <div className="grid grid-cols-2 gap-2">
                  {photos.map((p, i) => (
                    <div key={i} className="relative aspect-square rounded-lg bg-muted overflow-hidden group">
                      <img src={p.preview} alt="" className="w-full h-full object-cover" />
                      <button onClick={() => removePhoto(i)}
                        className="absolute top-1 right-1 p-1 rounded-full bg-destructive/80 text-white opacity-0 group-hover:opacity-100 transition-opacity">
                        <X className="h-3 w-3" />
                      </button>
                    </div>
                  ))}
                </div>
              )}

              {/* Upload zone */}
              <input ref={fileRef} type="file" accept="image/*" multiple className="hidden"
                onChange={e => handleFiles(e.target.files)} />
              <button onClick={() => fileRef.current?.click()}
                className="w-full border-2 border-dashed rounded-xl p-6 flex flex-col items-center gap-2 hover:bg-muted/50 hover:border-primary/30 transition-colors">
                <Upload className="h-6 w-6 text-muted-foreground" />
                <p className="text-xs text-muted-foreground font-medium">Cliquer pour ajouter des photos</p>
                <p className="text-[10px] text-muted-foreground">JPG, PNG, WebP — Max 5MB</p>
              </button>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Actions */}
      {error && (
        <div className="rounded-lg bg-destructive/10 border border-destructive/20 p-3">
          <p className="text-sm text-destructive font-medium">{error}</p>
        </div>
      )}

      <div className="flex items-center justify-between pb-8">
        <Link href="/catalog"><Button variant="outline" className="rounded-full">Annuler</Button></Link>
        <Button onClick={handleSubmit} disabled={saving} className="rounded-full px-8">
          {saving ? (uploadProgress || 'Création...') : 'Créer le modèle'}
        </Button>
      </div>
    </div>
  );
}
