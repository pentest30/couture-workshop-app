'use client';
import { use, useState, useEffect } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { clients } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, User, Phone, MapPin, FileText, Ruler, Save } from 'lucide-react';

const DEFAULT_MEASUREMENTS = [
  'Tour de poitrine', 'Tour de taille', 'Tour de hanches',
  'Longueur robe (dos)', 'Longueur jupe', 'Longueur manche',
  'Tour de bras', 'Épaule', 'Carrure dos', 'Hauteur totale',
];

export default function EditClientPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const router = useRouter();
  const queryClient = useQueryClient();
  const { data: client } = useQuery({ queryKey: ['client', id], queryFn: () => clients.get(id) });
  const { data: measurements } = useQuery({ queryKey: ['measurements', id], queryFn: () => clients.getMeasurements(id) });

  // Client fields
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [phone2, setPhone2] = useState('');
  const [address, setAddress] = useState('');
  const [notes, setNotes] = useState('');

  // Measurements
  const [measValues, setMeasValues] = useState<Record<string, string>>({});

  const [saving, setSaving] = useState(false);
  const [savingMeas, setSavingMeas] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    if (client) {
      setFirstName(client.firstName || '');
      setLastName(client.lastName || '');
      setPhone(client.primaryPhone || '');
      setPhone2(client.secondaryPhone || '');
      setAddress(client.address || '');
      setNotes(client.notes || '');
    }
  }, [client]);

  useEffect(() => {
    if (measurements?.current) {
      const vals: Record<string, string> = {};
      measurements.current.forEach(m => { vals[m.fieldName] = String(m.value); });
      setMeasValues(vals);
    }
  }, [measurements]);

  if (!client) return <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>;

  const handleSaveClient = async () => {
    if (!firstName.trim() || !lastName.trim() || !phone.trim()) {
      setError('Prénom, nom et téléphone sont obligatoires'); return;
    }
    setSaving(true); setError(''); setSuccess('');
    try {
      await clients.update(id, {
        firstName: firstName.trim(), lastName: lastName.trim(),
        primaryPhone: phone.trim(),
        secondaryPhone: phone2.trim() || undefined,
        address: address.trim() || undefined,
        notes: notes.trim() || undefined,
      });
      queryClient.invalidateQueries({ queryKey: ['client', id] });
      setSuccess('Client mis à jour');
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSaving(false);
  };

  const handleSaveMeasurements = async () => {
    const filled = Object.entries(measValues)
      .filter(([, v]) => v && parseFloat(v) > 0)
      .map(([fieldName, value]) => ({ fieldName, value: parseFloat(value), unit: 'cm' }));
    if (filled.length === 0) return;
    setSavingMeas(true); setError(''); setSuccess('');
    try {
      await clients.saveMeasurements(id, filled);
      queryClient.invalidateQueries({ queryKey: ['measurements', id] });
      setSuccess('Mensurations enregistrées');
    } catch (e) { setError(e instanceof Error ? e.message : 'Erreur'); }
    setSavingMeas(false);
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href={`/clients/${id}`} className="p-2 rounded-lg hover:bg-muted transition-colors">
          <ArrowLeft className="h-5 w-5 text-muted-foreground" />
        </Link>
        <div className="flex-1">
          <p className="font-mono text-xs text-muted-foreground">{client.code}</p>
          <h1 className="font-heading text-2xl font-bold">Modifier {client.firstName} {client.lastName}</h1>
        </div>
      </div>

      {error && <div className="rounded-lg bg-destructive/10 border border-destructive/20 p-3"><p className="text-sm text-destructive font-medium">{error}</p></div>}
      {success && <div className="rounded-lg bg-chart-1/10 border border-chart-1/20 p-3"><p className="text-sm text-chart-1 font-medium">{success}</p></div>}

      <div className="grid grid-cols-2 gap-6">
        {/* Client info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <User className="h-4 w-4 text-primary" /> Informations personnelles
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <Label className="flex items-center gap-1">Prénom <span className="text-destructive">*</span></Label>
                <Input value={firstName} onChange={e => setFirstName(e.target.value)} className="mt-1" />
              </div>
              <div>
                <Label className="flex items-center gap-1">Nom <span className="text-destructive">*</span></Label>
                <Input value={lastName} onChange={e => setLastName(e.target.value)} className="mt-1" />
              </div>
            </div>
            <div>
              <Label className="flex items-center gap-1.5"><Phone className="h-3 w-3" /> Téléphone principal <span className="text-destructive">*</span></Label>
              <Input value={phone} onChange={e => setPhone(e.target.value)} className="mt-1" />
            </div>
            <div>
              <Label className="flex items-center gap-1.5"><Phone className="h-3 w-3" /> Téléphone secondaire</Label>
              <Input value={phone2} onChange={e => setPhone2(e.target.value)} className="mt-1" />
            </div>
            <div>
              <Label className="flex items-center gap-1.5"><MapPin className="h-3 w-3" /> Adresse</Label>
              <Input value={address} onChange={e => setAddress(e.target.value)} className="mt-1" />
            </div>
            <div>
              <Label className="flex items-center gap-1.5"><FileText className="h-3 w-3" /> Notes</Label>
              <textarea value={notes} onChange={e => setNotes(e.target.value)} rows={3}
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm mt-1 resize-none ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring" />
            </div>
            <Button onClick={handleSaveClient} disabled={saving} className="rounded-full gap-2">
              <Save className="h-4 w-4" /> {saving ? 'Enregistrement...' : 'Enregistrer le profil'}
            </Button>
          </CardContent>
        </Card>

        {/* Measurements */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Ruler className="h-4 w-4 text-primary" /> Mensurations
            </CardTitle>
            <p className="text-xs text-muted-foreground mt-1">Modifiez les mesures. Les champs vides seront ignorés.</p>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="grid grid-cols-2 gap-x-4 gap-y-3">
              {DEFAULT_MEASUREMENTS.map(name => (
                <div key={name}>
                  <Label className="text-xs">{name}</Label>
                  <div className="relative mt-1">
                    <Input type="number" value={measValues[name] || ''} placeholder="—"
                      onChange={e => setMeasValues(prev => ({ ...prev, [name]: e.target.value }))}
                      className="pr-10" />
                    <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-muted-foreground">cm</span>
                  </div>
                </div>
              ))}
            </div>
            <Button onClick={handleSaveMeasurements} disabled={savingMeas} variant="outline" className="rounded-full gap-2 mt-2">
              <Save className="h-4 w-4" /> {savingMeas ? 'Enregistrement...' : 'Enregistrer les mensurations'}
            </Button>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
