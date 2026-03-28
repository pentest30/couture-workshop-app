'use client';
import { STATUS_COLORS } from '@/lib/helpers';

export function StatusBadge({ status, label }: { status: string; label: string }) {
  const color = STATUS_COLORS[status] || '#666';
  return (
    <span
      className="inline-flex items-center px-2.5 py-0.5 rounded-full text-[10px] font-bold tracking-wider uppercase"
      style={{ color, backgroundColor: `${color}18` }}
    >
      <span className="w-1.5 h-1.5 rounded-full mr-1.5" style={{ backgroundColor: color }} />
      {label}
    </span>
  );
}
