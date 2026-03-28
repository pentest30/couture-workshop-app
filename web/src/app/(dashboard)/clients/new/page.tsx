'use client';
import { useState } from 'react';
import { clients } from '@/lib/api';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { ArrowLeft, User, Phone, MapPin, FileText, Ruler } from 'lucide-react';

const DEFAULT_MEASUREMENTS = [
  { name: 'Tour de poitrine', unit: 'cm' },
  { name: 'Tour de taille', unit: 'cm' },
  { name: 'Tour de hanches', unit: 'cm' },
  { name: 'Longueur robe (dos)', unit: 'cm' },
  { name: 'Longueur jupe', unit: 'cm' },
  { name: 'Longueur manche', unit: 'cm' },
  { name: 'Tour de bras', unit: 'cm' },
  { name: 'Épaule', unit: 'cm' },
  { name: 'Carrure dos', unit: 'cm' },
  { name: 'Hauteur totale', unit: 'cm' },
];

export default function NewClientPage() {
  const router = useRouter();

  // Client info
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [phone2, setPhone2] = useState('');
  const [address, setAddress] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState('');
  const [notes, setNotes] = useState('');

  // Measurements
  const [measurements, setMeasurements] = useState<Record<string, string>>({});

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [duplicateWarning, setDuplicateWarning] = useState<{
    existingClientId: string; existingCode: string; existingName: string;
  } | null>(null);

  const updateMeasurement = (name: string, value: string) => {
    setMeasurements(prev => ({ ...prev, [name]: value }));
  };

  const submitClient = async (confirmDuplicate = false) => {
    if (!firstName.trim() || !lastName.trim() || !phone.trim()) {
      setError('Prénom, nom et téléphone sont obligatoires');
      return;
    }

    setSaving(true);
    setError('');
    if (!confirmDuplicate) setDuplicateWarning(null);

    try {
      const result = await clients.create({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        primaryPhone: phone.trim(),
        secondaryPhone: phone2.trim() || undefined,
        address: address.trim() || undefined,
        dateOfBirth: dateOfBirth || undefined,
        notes: notes.trim() || undefined,
        confirmDuplicate,
      });

      // Save measurements if any are filled
      const filledMeasurements = Object.entries(measurements)
        .filter(([, v]) => v && parseFloat(v) > 0)
        .map(([fieldName, value]) => ({
          fieldName,
          value: parseFloat(value),
          unit: 'cm',
        }));

      if (filledMeasurements.length > 0) {
        try {
          await clients.saveMeasurements(result.id, filledMeasurements);
        } catch {
          // Non-blocking — client is already created
        }
      }

      router.push(`/clients/${result.id}`);
    } catch (e: unknown) {
      const err = e as { duplicate?: boolean; existingClientId?: string; existingCode?: string; existingName?: string; message?: string };
      if (err.duplicate && err.existingClientId) {
        setDuplicateWarning({ existingClientId: err.existingClientId, existingCode: err.existingCode!, existingName: err.existingName! });
      } else {
        setError(err.message || 'Erreur lors de la création');
      }
    }

    setSaving(false);
  };

  const handleSubmit = () => submitClient(false);

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Link href="/clients" className="p-2 rounded-lg hover:bg-muted transition-colors">
          <ArrowLeft className="h-5 w-5 text-muted-foreground" />
        </Link>
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-0.5">Nouveau</p>
          <h1 className="font-heading text-2xl font-bold">Créer un client</h1>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-6">
        {/* Left: Client info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <User className="h-4 w-4 text-primary" />
              Informations personnelles
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1.5">
                  Prénom <span className="text-destructive">*</span>
                </label>
                <Input value={firstName} onChange={e => setFirstName(e.target.value)} placeholder="Sara" />
              </div>
              <div>
                <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1.5">
                  Nom <span className="text-destructive">*</span>
                </label>
                <Input value={lastName} onChange={e => setLastName(e.target.value)} placeholder="Benali" />
              </div>
            </div>

            <div>
              <label className="flex items-center gap-1.5 text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1.5">
                <Phone className="h-3 w-3" /> Téléphone principal <span className="text-destructive">*</span>
              </label>
              <Input value={phone} onChange={e => setPhone(e.target.value)} placeholder="0550123456" />
            </div>

            <div>
              <label className="flex items-center gap-1.5 text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1.5">
                <Phone className="h-3 w-3" /> Téléphone secondaire
              </label>
              <Input value={phone2} onChange={e => setPhone2(e.target.value)} placeholder="Optionnel" />
            </div>

            <div>
              <label className="flex items-center gap-1.5 text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1.5">
                <MapPin className="h-3 w-3" /> Adresse
              </label>
              <Input value={address} onChange={e => setAddress(e.target.value)} placeholder="Optionnel" />
            </div>

            <div>
              <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1.5">
                Date de naissance
              </label>
              <Input type="date" value={dateOfBirth} onChange={e => setDateOfBirth(e.target.value)} />
            </div>

            <div>
              <label className="flex items-center gap-1.5 text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1.5">
                <FileText className="h-3 w-3" /> Notes
              </label>
              <textarea
                value={notes}
                onChange={e => setNotes(e.target.value)}
                rows={3}
                placeholder="Préférences, habitudes..."
                className="flex w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 resize-none"
              />
            </div>
          </CardContent>
        </Card>

        {/* Right: Measurements */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2 text-base">
              <Ruler className="h-4 w-4 text-primary" />
              Mensurations
            </CardTitle>
            <p className="text-xs text-muted-foreground mt-1">Renseignez les mesures disponibles. Les champs vides seront ignorés.</p>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 gap-x-4 gap-y-3">
              {DEFAULT_MEASUREMENTS.map((m) => (
                <div key={m.name}>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">
                    {m.name}
                  </label>
                  <div className="relative">
                    <Input
                      type="number"
                      value={measurements[m.name] || ''}
                      onChange={e => updateMeasurement(m.name, e.target.value)}
                      placeholder="—"
                      className="pr-10"
                    />
                    <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-muted-foreground">
                      {m.unit}
                    </span>
                  </div>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Duplicate warning */}
      {duplicateWarning && (
        <div className="rounded-lg bg-amber-50 dark:bg-amber-950/30 border border-amber-300 dark:border-amber-700 p-4 space-y-3">
          <p className="text-sm font-semibold text-amber-800 dark:text-amber-200">
            Un client avec ce numéro de téléphone existe déjà :
          </p>
          <div className="flex items-center gap-3 bg-white dark:bg-card rounded-lg p-3 border border-amber-200 dark:border-amber-800">
            <div className="w-10 h-10 rounded-full bg-amber-100 dark:bg-amber-900 flex items-center justify-center">
              <User className="h-5 w-5 text-amber-600" />
            </div>
            <div className="flex-1">
              <p className="text-sm font-semibold">{duplicateWarning.existingName}</p>
              <p className="text-xs text-muted-foreground">{duplicateWarning.existingCode}</p>
            </div>
            <Button variant="outline" size="sm" className="rounded-full" onClick={() => router.push(`/clients/${duplicateWarning.existingClientId}`)}>
              Voir le client
            </Button>
          </div>
          <div className="flex items-center gap-2">
            <Button variant="outline" size="sm" className="rounded-full" onClick={() => setDuplicateWarning(null)}>
              Corriger le numéro
            </Button>
            <Button size="sm" className="rounded-full" onClick={() => submitClient(true)} disabled={saving}>
              {saving ? 'Création...' : 'Créer quand même'}
            </Button>
          </div>
        </div>
      )}

      {/* Error */}
      {error && (
        <div className="rounded-lg bg-destructive/10 border border-destructive/20 p-3">
          <p className="text-sm text-destructive font-medium">{error}</p>
        </div>
      )}

      <div className="flex items-center justify-between pt-2 pb-8">
        <Link href="/clients">
          <Button variant="outline" className="rounded-full">Annuler</Button>
        </Link>
        <Button onClick={handleSubmit} disabled={saving} className="rounded-full px-8">
          {saving ? 'Création en cours...' : 'Créer le client'}
        </Button>
      </div>
    </div>
  );
}
