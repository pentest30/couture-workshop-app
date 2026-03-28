'use client';
import { use, useState, useRef, useCallback } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { catalog } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { formatDZD } from '@/lib/helpers';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { Image as ImageIcon, Clock, Scissors, ShoppingBag, ChevronLeft, ChevronRight, Trash2, X, ZoomIn } from 'lucide-react';

export default function ModelDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const queryClient = useQueryClient();
  const { data: model, isLoading } = useQuery({ queryKey: ['catalog-model', id], queryFn: () => catalog.getModel(id) });
  const [photoIdx, setPhotoIdx] = useState(0);
  const [lightboxOpen, setLightboxOpen] = useState(false);

  if (isLoading) return <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>;
  if (!model) return <div className="py-20 text-center text-muted-foreground">Modèle introuvable</div>;

  const handleDelete = async () => {
    if (!confirm('Supprimer ce modèle du catalogue ?')) return;
    try {
      await catalog.deactivateModel(id);
      queryClient.invalidateQueries({ queryKey: ['catalog-models'] });
      router.push('/catalog');
    } catch (e) { alert(e instanceof Error ? e.message : 'Erreur'); }
  };

  const prevPhoto = () => setPhotoIdx(i => i <= 0 ? model.photos.length - 1 : i - 1);
  const nextPhoto = () => setPhotoIdx(i => i >= model.photos.length - 1 ? 0 : i + 1);

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm">
          <Link href="/catalog" className="text-muted-foreground hover:text-primary transition-colors">Catalogue</Link>
          <span className="text-muted-foreground">/</span>
          <span className="font-semibold">{model.name}</span>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handleDelete} className="rounded-full gap-1.5 text-destructive hover:text-destructive">
            <Trash2 className="h-3.5 w-3.5" /> Supprimer
          </Button>
          <Link href={`/catalog/${id}/edit`}>
            <Button variant="outline" className="rounded-full">Modifier</Button>
          </Link>
          <Button onClick={() => router.push(`/orders/new?workType=${model.workType}&description=${encodeURIComponent(model.name)}&price=${model.basePrice}`)}
            className="rounded-full gap-2">
            <ShoppingBag className="h-4 w-4" /> Créer une commande
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-3 gap-6">
        {/* Photos — 2 cols */}
        <div className="col-span-2 space-y-4">
          {/* Main photo with carousel + hover zoom */}
          {model.photos.length > 0 ? (
            <ZoomableImage
              src={model.photos[photoIdx]?.storagePath}
              alt={model.name}
              onPrev={model.photos.length > 1 ? prevPhoto : undefined}
              onNext={model.photos.length > 1 ? nextPhoto : undefined}
              onClickImage={() => setLightboxOpen(true)}
              dots={model.photos.length > 1 ? model.photos.map((_, i) => i) : undefined}
              activeIdx={photoIdx}
              onDotClick={setPhotoIdx}
            />
          ) : (
            <div className="aspect-[16/9] bg-muted rounded-2xl flex items-center justify-center">
              <ImageIcon className="w-16 h-16 text-muted-foreground/20" />
            </div>
          )}
          {/* Thumbnails */}
          {model.photos.length > 1 && (
            <div className="flex gap-2 overflow-x-auto">
              {model.photos.map((p, i) => (
                <button key={p.id} onClick={() => setPhotoIdx(i)}
                  className={`w-20 h-20 shrink-0 rounded-xl bg-muted overflow-hidden transition-all ${i === photoIdx ? 'ring-2 ring-primary' : 'opacity-60 hover:opacity-100'}`}>
                  <img src={p.storagePath} alt={p.fileName} className="w-full h-full object-cover" />
                </button>
              ))}
            </div>
          )}

          {/* Description */}
          {model.description && (
            <Card>
              <CardHeader><CardTitle className="text-base">Description</CardTitle></CardHeader>
              <CardContent><p className="text-sm leading-relaxed">{model.description}</p></CardContent>
            </Card>
          )}

          {/* Linked fabrics */}
          {model.fabrics.length > 0 && (
            <Card>
              <CardHeader><CardTitle className="text-base">Tissus recommandés</CardTitle></CardHeader>
              <CardContent>
                <div className="grid grid-cols-3 gap-3">
                  {model.fabrics.map(f => (
                    <div key={f.id} className="p-3 rounded-xl bg-muted flex items-center gap-3">
                      <div className="w-8 h-8 rounded-lg border" style={{ backgroundColor: f.color }} />
                      <div className="flex-1 min-w-0">
                        <p className="text-sm font-semibold truncate">{f.name}</p>
                        <p className="text-xs text-muted-foreground">{f.type} — {formatDZD(f.pricePerMeter)}/m</p>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </div>

        {/* Info sidebar */}
        <div className="space-y-4">
          <Card>
            <CardContent className="pt-6">
              <p className="font-mono text-xs text-muted-foreground mb-1">{model.code}</p>
              <h1 className="font-heading text-xl font-bold mb-2">{model.name}</h1>
              <div className="flex flex-wrap gap-2 mb-4">
                <Badge variant="secondary">{model.categoryLabel}</Badge>
                <Badge variant="outline">{model.workType}</Badge>
                {model.isPublic && <Badge>Public</Badge>}
              </div>
              <div className="space-y-3">
                <div className="flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-chart-2/10 flex items-center justify-center">
                    <Scissors className="h-4 w-4 text-chart-2" />
                  </div>
                  <div>
                    <p className="font-heading text-lg font-bold text-chart-2">{formatDZD(model.basePrice)}</p>
                    <p className="text-xs text-muted-foreground">Prix de base</p>
                  </div>
                </div>
                <div className="flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center">
                    <Clock className="h-4 w-4 text-primary" />
                  </div>
                  <div>
                    <p className="font-heading text-lg font-bold">{model.estimatedDays} jours</p>
                    <p className="text-xs text-muted-foreground">Durée estimée</p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardContent className="pt-6">
              <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2">Photos</p>
              <p className="text-sm">{model.photos.length} photo(s)</p>
              <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2 mt-4">Tissus liés</p>
              <p className="text-sm">{model.fabrics.length} tissu(s)</p>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Lightbox */}
      {lightboxOpen && model.photos.length > 0 && (
        <div className="fixed inset-0 z-50 bg-black/90 flex items-center justify-center" onClick={() => setLightboxOpen(false)}>
          <button className="absolute top-4 right-4 w-10 h-10 rounded-full bg-white/10 text-white flex items-center justify-center hover:bg-white/20 transition-colors z-10">
            <X className="h-5 w-5" />
          </button>
          {model.photos.length > 1 && (
            <>
              <button onClick={(e) => { e.stopPropagation(); prevPhoto(); }}
                className="absolute left-4 top-1/2 -translate-y-1/2 w-12 h-12 rounded-full bg-white/10 text-white flex items-center justify-center hover:bg-white/20 transition-colors">
                <ChevronLeft className="h-6 w-6" />
              </button>
              <button onClick={(e) => { e.stopPropagation(); nextPhoto(); }}
                className="absolute right-4 top-1/2 -translate-y-1/2 w-12 h-12 rounded-full bg-white/10 text-white flex items-center justify-center hover:bg-white/20 transition-colors">
                <ChevronRight className="h-6 w-6" />
              </button>
            </>
          )}
          <img src={model.photos[photoIdx]?.storagePath} alt={model.name}
            onClick={(e) => e.stopPropagation()}
            className="max-w-[85vw] max-h-[85vh] object-contain rounded-lg" />
          {model.photos.length > 1 && (
            <div className="absolute bottom-6 left-1/2 -translate-x-1/2 flex gap-2">
              {model.photos.map((_, i) => (
                <button key={i} onClick={(e) => { e.stopPropagation(); setPhotoIdx(i); }}
                  className={`w-2.5 h-2.5 rounded-full transition-all ${i === photoIdx ? 'bg-white scale-125' : 'bg-white/40'}`} />
              ))}
            </div>
          )}
          <p className="absolute bottom-6 right-6 text-white/50 text-sm">{photoIdx + 1} / {model.photos.length}</p>
        </div>
      )}
    </div>
  );
}

/** Image with hover-zoom lens effect + click to open lightbox */
function ZoomableImage({ src, alt, onPrev, onNext, onClickImage, dots, activeIdx, onDotClick }: {
  src: string; alt: string;
  onPrev?: () => void; onNext?: () => void;
  onClickImage: () => void;
  dots?: number[]; activeIdx?: number; onDotClick?: (i: number) => void;
}) {
  const containerRef = useRef<HTMLDivElement>(null);
  const [zoom, setZoom] = useState(false);
  const [pos, setPos] = useState({ x: 50, y: 50 });

  const handleMouseMove = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    const rect = containerRef.current?.getBoundingClientRect();
    if (!rect) return;
    const x = ((e.clientX - rect.left) / rect.width) * 100;
    const y = ((e.clientY - rect.top) / rect.height) * 100;
    setPos({ x, y });
  }, []);

  return (
    <div ref={containerRef}
      className="aspect-[16/9] bg-muted rounded-2xl overflow-hidden relative group cursor-zoom-in"
      onMouseEnter={() => setZoom(true)}
      onMouseLeave={() => setZoom(false)}
      onMouseMove={handleMouseMove}
      onClick={onClickImage}>
      {/* Normal image */}
      <img src={src} alt={alt} className="w-full h-full object-cover" draggable={false} />
      {/* Zoom overlay — shows magnified portion following cursor */}
      {zoom && (
        <div className="absolute inset-0 pointer-events-none"
          style={{
            backgroundImage: `url(${src})`,
            backgroundSize: '200%',
            backgroundPosition: `${pos.x}% ${pos.y}%`,
            opacity: 1,
          }} />
      )}
      {/* Zoom icon hint */}
      <div className="absolute top-3 right-3 w-8 h-8 rounded-full bg-black/30 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
        <ZoomIn className="h-4 w-4" />
      </div>
      {/* Nav arrows */}
      {onPrev && (
        <button onClick={(e) => { e.stopPropagation(); onPrev(); }}
          className="absolute left-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-black/40 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity hover:bg-black/60">
          <ChevronLeft className="h-5 w-5" />
        </button>
      )}
      {onNext && (
        <button onClick={(e) => { e.stopPropagation(); onNext(); }}
          className="absolute right-3 top-1/2 -translate-y-1/2 w-9 h-9 rounded-full bg-black/40 text-white flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity hover:bg-black/60">
          <ChevronRight className="h-5 w-5" />
        </button>
      )}
      {/* Dots */}
      {dots && (
        <div className="absolute bottom-3 left-1/2 -translate-x-1/2 flex gap-1.5">
          {dots.map(i => (
            <button key={i} onClick={(e) => { e.stopPropagation(); onDotClick?.(i); }}
              className={`w-2 h-2 rounded-full transition-all ${i === activeIdx ? 'bg-white scale-125' : 'bg-white/50'}`} />
          ))}
        </div>
      )}
    </div>
  );
}
