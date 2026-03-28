'use client';

const COLORS: Record<string, string> = {
  Simple: '#7F5700', Brode: '#6A1B9A', Perle: '#880E4F', Mixte: '#410050',
};

export function WorkTypeBadge({ workType, label }: { workType: string; label?: string }) {
  const color = COLORS[workType] || '#666';
  return (
    <span
      className="inline-flex items-center px-2 py-0.5 rounded text-[10px] font-bold tracking-wide uppercase"
      style={{ color, backgroundColor: `${color}12`, border: `1px solid ${color}25` }}
    >
      {label || workType}
    </span>
  );
}
