'use client';
import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { measurementFields as fieldsApi, type MeasurementField } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Ruler, Plus, Trash2, GripVertical } from 'lucide-react';

export default function SettingsPage() {
  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div>
        <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-1">Configuration</p>
        <h1 className="font-heading text-3xl font-bold">Paramètres</h1>
      </div>
      <MeasurementFieldsSection />
    </div>
  );
}

function MeasurementFieldsSection() {
  const queryClient = useQueryClient();
  const { data: fields, isLoading } = useQuery({
    queryKey: ['measurement-fields'],
    queryFn: () => fieldsApi.list(),
  });

  const [newName, setNewName] = useState('');
  const [newUnit, setNewUnit] = useState('cm');
  const [adding, setAdding] = useState(false);
  const [error, setError] = useState('');

  const handleAdd = async () => {
    if (!newName.trim()) return;
    setAdding(true); setError('');
    try {
      const nextOrder = (fields?.length ?? 0) + 1;
      await fieldsApi.create({ name: newName.trim(), unit: newUnit.trim() || 'cm', displayOrder: nextOrder });
      queryClient.invalidateQueries({ queryKey: ['measurement-fields'] });
      setNewName(''); setNewUnit('cm');
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erreur');
    }
    setAdding(false);
  };

  const handleRemove = async (field: MeasurementField) => {
    if (!confirm(`Supprimer la mensuration "${field.name}" ?`)) return;
    try {
      await fieldsApi.remove(field.id);
      queryClient.invalidateQueries({ queryKey: ['measurement-fields'] });
    } catch (e) {
      alert(e instanceof Error ? e.message : 'Erreur');
    }
  };

  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-10 text-center">
          <div className="w-5 h-5 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" />
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <Ruler className="h-4 w-4 text-primary" />
          Champs de mensurations
        </CardTitle>
        <p className="text-xs text-muted-foreground mt-1">
          Gérez les mensurations disponibles lors de la création de clients. Vous pouvez ajouter des champs personnalisés ou supprimer ceux qui ne sont pas utilisés.
        </p>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Existing fields */}
        <div className="space-y-1">
          {fields?.map((f) => (
            <div key={f.id} className="flex items-center gap-3 px-3 py-2.5 rounded-lg hover:bg-muted/50 group transition-colors">
              <GripVertical className="h-4 w-4 text-muted-foreground/40" />
              <div className="flex-1 min-w-0">
                <span className="text-sm font-medium">{f.name}</span>
                {f.isDefault && (
                  <span className="ml-2 text-[10px] font-bold tracking-wider uppercase text-muted-foreground bg-muted px-1.5 py-0.5 rounded">
                    par défaut
                  </span>
                )}
              </div>
              <span className="text-xs text-muted-foreground w-10">{f.unit}</span>
              <button onClick={() => handleRemove(f)}
                className="opacity-0 group-hover:opacity-100 p-1.5 rounded-md hover:bg-destructive/10 text-destructive transition-all"
                title="Supprimer">
                <Trash2 className="h-3.5 w-3.5" />
              </button>
            </div>
          ))}
          {(!fields || fields.length === 0) && (
            <p className="text-sm text-muted-foreground text-center py-4">Aucun champ de mensuration configuré</p>
          )}
        </div>

        {/* Add new field */}
        <div className="border-t pt-4">
          <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2">
            <Plus className="h-3 w-3 inline mr-1" />
            Ajouter un champ
          </p>
          <div className="flex items-center gap-2">
            <Input value={newName} onChange={e => setNewName(e.target.value)}
              placeholder="Nom (ex: Tour de cou)" className="flex-1 h-9"
              onKeyDown={e => e.key === 'Enter' && handleAdd()} />
            <Input value={newUnit} onChange={e => setNewUnit(e.target.value)}
              placeholder="Unité" className="w-20 h-9" />
            <Button onClick={handleAdd} disabled={adding || !newName.trim()} size="sm" className="rounded-full h-9 px-4">
              {adding ? '...' : 'Ajouter'}
            </Button>
          </div>
          {error && <p className="text-xs text-destructive mt-1">{error}</p>}
        </div>
      </CardContent>
    </Card>
  );
}
