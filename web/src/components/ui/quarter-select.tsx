'use client';

import type { Quarter } from '@/lib/helpers';

interface Props {
  year: number;
  quarter: Quarter;
  onChange: (year: number, quarter: Quarter) => void;
}

export function QuarterSelect({ year, quarter, onChange }: Props) {
  const now = new Date();
  const options: { label: string; year: number; quarter: Quarter }[] = [];
  for (let y = now.getFullYear(); y >= now.getFullYear() - 2; y--) {
    for (const q of [1, 2, 3, 4] as Quarter[]) {
      options.push({ label: `T${q} ${y}`, year: y, quarter: q });
    }
  }

  return (
    <select
      value={`T${quarter} ${year}`}
      onChange={(e) => {
        const [t, y] = e.target.value.split(' ');
        onChange(parseInt(y), parseInt(t.substring(1)) as Quarter);
      }}
      className="h-10 px-3 pr-8 rounded-xl border border-border bg-card text-sm font-semibold
        focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary
        appearance-none cursor-pointer"
      style={{ backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' fill='%234D4544' viewBox='0 0 16 16'%3E%3Cpath d='M4.5 6l3.5 4 3.5-4z'/%3E%3C/svg%3E")`, backgroundRepeat: 'no-repeat', backgroundPosition: 'right 8px center' }}
    >
      {options.map((o) => (
        <option key={o.label} value={o.label}>{o.label}</option>
      ))}
    </select>
  );
}