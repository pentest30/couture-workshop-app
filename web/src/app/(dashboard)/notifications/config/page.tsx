'use client';
import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { notifications, type NotifConfig } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Switch } from '@/components/ui/switch';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { ArrowLeft, Bell, Settings } from 'lucide-react';

const PRIORITY_COLORS: Record<string, string> = {
  Critical: 'bg-destructive text-white',
  High: 'bg-orange-500 text-white',
  Medium: 'bg-blue-500 text-white',
};

export default function NotificationConfigPage() {
  const queryClient = useQueryClient();
  const { data: configs, isLoading } = useQuery({
    queryKey: ['notif-configs'],
    queryFn: () => notifications.getConfigs(),
  });

  if (isLoading) return <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>;

  return (
    <div className="max-w-3xl mx-auto space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/notifications" className="p-2 rounded-lg hover:bg-muted transition-colors">
          <ArrowLeft className="h-5 w-5 text-muted-foreground" />
        </Link>
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-0.5">Administration</p>
          <h1 className="font-heading text-2xl font-bold">Configuration des alertes</h1>
        </div>
      </div>

      <div className="space-y-4">
        {configs?.map(config => (
          <ConfigCard key={config.typeValue} config={config}
            onSaved={() => queryClient.invalidateQueries({ queryKey: ['notif-configs'] })} />
        ))}
      </div>
    </div>
  );
}

function ConfigCard({ config, onSaved }: { config: NotifConfig; onSaved: () => void }) {
  const [isEnabled, setIsEnabled] = useState(config.isEnabled);
  const [smsEnabled, setSmsEnabled] = useState(config.smsEnabled);
  const [stallSimple, setStallSimple] = useState(config.stallThresholdSimple);
  const [stallEmbroidered, setStallEmbroidered] = useState(config.stallThresholdEmbroidered);
  const [stallBeaded, setStallBeaded] = useState(config.stallThresholdBeaded);
  const [stallMixed, setStallMixed] = useState(config.stallThresholdMixed);
  const [saving, setSaving] = useState(false);
  const [dirty, setDirty] = useState(false);

  const isStallType = config.typeName === 'N04_Stalled';

  const handleSave = async () => {
    setSaving(true);
    try {
      await notifications.updateConfig(config.typeValue, {
        isEnabled,
        smsEnabled,
        ...(isStallType ? {
          stallThresholdSimple: stallSimple,
          stallThresholdEmbroidered: stallEmbroidered,
          stallThresholdBeaded: stallBeaded,
          stallThresholdMixed: stallMixed,
        } : {}),
      });
      setDirty(false);
      onSaved();
    } catch { /* ignore */ }
    setSaving(false);
  };

  const markDirty = () => setDirty(true);

  return (
    <Card className={!isEnabled ? 'opacity-60' : ''}>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-lg bg-primary/10 flex items-center justify-center">
              <Bell className="h-4 w-4 text-primary" />
            </div>
            <div>
              <CardTitle className="text-sm font-semibold">{config.typeLabel}</CardTitle>
              <p className="text-xs text-muted-foreground">{config.typeName}</p>
            </div>
          </div>
          <div className="flex items-center gap-3">
            <Badge className={`text-[10px] ${PRIORITY_COLORS[config.priority] || 'bg-muted'}`}>
              {config.priority}
            </Badge>
            <Switch checked={isEnabled} onCheckedChange={(v) => { setIsEnabled(v); markDirty(); }} />
          </div>
        </div>
      </CardHeader>
      {isEnabled && (
        <CardContent className="space-y-4 pt-0">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-xs font-semibold">SMS</p>
              <p className="text-[11px] text-muted-foreground">Envoyer un SMS en plus de la notification in-app</p>
            </div>
            <Switch checked={smsEnabled} onCheckedChange={(v) => { setSmsEnabled(v); markDirty(); }} />
          </div>

          {isStallType && (
            <div>
              <p className="text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2">Seuils de blocage (jours)</p>
              <div className="grid grid-cols-4 gap-3">
                <ThresholdInput label="Simple" value={stallSimple} onChange={(v) => { setStallSimple(v); markDirty(); }} />
                <ThresholdInput label="Brode" value={stallEmbroidered} onChange={(v) => { setStallEmbroidered(v); markDirty(); }} />
                <ThresholdInput label="Perle" value={stallBeaded} onChange={(v) => { setStallBeaded(v); markDirty(); }} />
                <ThresholdInput label="Mixte" value={stallMixed} onChange={(v) => { setStallMixed(v); markDirty(); }} />
              </div>
            </div>
          )}

          {dirty && (
            <div className="flex justify-end">
              <Button onClick={handleSave} disabled={saving} size="sm" className="rounded-full">
                {saving ? 'Enregistrement...' : 'Enregistrer'}
              </Button>
            </div>
          )}
        </CardContent>
      )}
    </Card>
  );
}

function ThresholdInput({ label, value, onChange }: { label: string; value: number; onChange: (v: number) => void }) {
  return (
    <div>
      <label className="block text-[11px] text-muted-foreground mb-1">{label}</label>
      <Input type="number" value={value} onChange={e => onChange(parseInt(e.target.value) || 0)}
        className="h-8 text-center text-sm" min={1} max={90} />
    </div>
  );
}