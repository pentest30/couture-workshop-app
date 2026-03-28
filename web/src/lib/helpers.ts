export const STATUS_COLORS: Record<string, string> = {
  Recue: '#1565C0', EnAttente: '#F9A825', EnCours: '#E65100',
  Broderie: '#6A1B9A', Perlage: '#880E4F', Retouche: '#C62828',
  Prete: '#2E7D32', Livree: '#424242',
};

export const STATUS_LABELS: Record<string, string> = {
  Recue: 'Reçue', EnAttente: 'En Attente', EnCours: 'En Cours',
  Broderie: 'Broderie', Perlage: 'Perlage', Retouche: 'Retouche',
  Prete: 'Prête', Livree: 'Livrée',
};

export const ALL_STATUSES = Object.keys(STATUS_LABELS);

export const WORK_TYPE_LABELS: Record<string, string> = {
  Simple: 'Simple', Brode: 'Brodé', Perle: 'Perlé', Mixte: 'Mixte',
};

export function formatDZD(amount: number): string {
  return new Intl.NumberFormat('fr-DZ', { style: 'decimal', maximumFractionDigits: 0 }).format(amount) + ' DZD';
}

export function formatDate(date?: string | null): string {
  if (!date) return '—';
  return new Date(date).toLocaleDateString('fr-FR', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export function formatRelative(date: string): string {
  const diff = Date.now() - new Date(date).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return "à l'instant";
  if (mins < 60) return `il y a ${mins}min`;
  const hrs = Math.floor(mins / 60);
  if (hrs < 24) return `il y a ${hrs}h`;
  const days = Math.floor(hrs / 24);
  if (days === 1) return 'hier';
  if (days < 7) return `il y a ${days}j`;
  return formatDate(date);
}

export type Quarter = 1 | 2 | 3 | 4;

export function quarterDates(year: number, quarter: Quarter): { from: string; to: string } {
  const starts: Record<Quarter, string> = { 1: '01-01', 2: '04-01', 3: '07-01', 4: '10-01' };
  const ends: Record<Quarter, string> = { 1: '03-31', 2: '06-30', 3: '09-30', 4: '12-31' };
  return { from: `${year}-${starts[quarter]}`, to: `${year}-${ends[quarter]}` };
}

export function currentQuarter(): { year: number; quarter: Quarter } {
  const now = new Date();
  const q = (Math.floor(now.getMonth() / 3) + 1) as Quarter;
  return { year: now.getFullYear(), quarter: q };
}

export function quarterLabel(quarter: Quarter): string {
  return `T${quarter}`;
}
