'use client';
import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { notifications } from '@/lib/api';
import { formatRelative } from '@/lib/helpers';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth-context';

const PRIORITY_COLORS: Record<string, string> = { Critical: '#C62828', High: '#E65100', Medium: '#4D4544' };

export default function NotificationsPage() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const isManager = user?.roles.includes('Manager') ?? false;
  const [filter, setFilter] = useState('unread');

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['notifications', filter],
    queryFn: () => notifications.list(filter),
  });

  const handleMarkRead = async (id: string) => {
    await notifications.markRead(id);
    refetch();
    queryClient.invalidateQueries({ queryKey: ['unread-count'] });
  };

  const handleMarkAllRead = async () => {
    await notifications.markAllRead();
    refetch();
    queryClient.invalidateQueries({ queryKey: ['unread-count'] });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <p className="text-xs font-semibold tracking-widest uppercase text-muted-foreground mb-1">Centre de gestion</p>
          <h1 className="font-heading text-3xl font-bold">Notifications</h1>
        </div>
        <div className="flex items-center gap-3">
          {isManager && (
            <button onClick={() => router.push('/notifications/config')}
              className="h-9 px-4 rounded-full border border-border text-sm font-semibold hover:bg-secondary transition-colors inline-flex items-center gap-1.5">
              Configuration
            </button>
          )}
          <button onClick={handleMarkAllRead}
            className="text-xs font-bold text-chart-2 hover:text-chart-2/80 transition-colors">
            Tout marquer lu
          </button>
        </div>
      </div>

      {/* Filter tabs */}
      <div className="flex gap-2">
        {[{ key: 'unread', label: 'Non lues' }, { key: 'all', label: 'Toutes' }, { key: 'critical', label: 'Critiques' }].map(f => (
          <button key={f.key} onClick={() => setFilter(f.key)}
            className={`px-4 py-2 rounded-full text-sm font-semibold transition-all
              ${filter === f.key ? 'bg-primary text-white' : 'bg-secondary text-foreground hover:bg-muted'}`}>
            {f.label}
            {f.key === 'unread' && data?.unreadCount ? (
              <span className="ml-1.5 px-1.5 py-0.5 rounded-full bg-white/20 text-[10px]">{data.unreadCount}</span>
            ) : null}
          </button>
        ))}
      </div>

      {/* List */}
      {isLoading ? (
        <div className="py-20 text-center"><div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin mx-auto" /></div>
      ) : (
        <div className="space-y-2">
          {data?.items?.map(n => {
            const pColor = PRIORITY_COLORS[n.priority] || '#666';
            return (
              <div key={n.id}
                className={`bg-card rounded-2xl p-5 border transition-all
                  ${n.isRead ? 'border-border/15 opacity-60' : 'border-border/20'}
                  ${n.priority === 'Critical' ? 'border-destructive/20' : ''}`}>
                <div className="flex items-start gap-4">
                  <div className="w-1 h-12 rounded-full shrink-0" style={{ backgroundColor: pColor }} />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      {n.priority === 'Critical' && (
                        <span className="px-1.5 py-0.5 rounded text-[9px] font-bold tracking-wider bg-destructive/10 text-destructive">CRITIQUE</span>
                      )}
                      {n.priority === 'High' && (
                        <span className="px-1.5 py-0.5 rounded text-[9px] font-bold tracking-wider bg-chart-2/10 text-chart-2">IMPORTANT</span>
                      )}
                      <span className="text-[11px] text-muted-foreground ml-auto">{formatRelative(n.createdAt)}</span>
                    </div>
                    <p className={`text-sm ${n.isRead ? 'font-normal' : 'font-semibold'}`}>{n.title}</p>
                    <p className="text-xs text-muted-foreground mt-0.5 line-clamp-2">{n.message}</p>
                  </div>
                  <div className="flex items-center gap-2 shrink-0">
                    {n.orderId && (
                      <button onClick={() => router.push(`/orders/${n.orderId}`)}
                        className="px-2.5 py-1 rounded-lg bg-chart-2/10 text-[10px] font-bold text-chart-2 hover:bg-chart-2/20 transition-colors">
                        VOIR
                      </button>
                    )}
                    {!n.isRead && (
                      <button onClick={() => handleMarkRead(n.id)}
                        className="p-1.5 rounded-lg hover:bg-secondary transition-colors" title="Marquer lu">
                        <svg className="w-4 h-4 text-muted-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                          <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                        </svg>
                      </button>
                    )}
                  </div>
                </div>
              </div>
            );
          })}
          {(!data?.items || data.items.length === 0) && (
            <div className="py-16 text-center text-muted-foreground">
              <svg className="w-12 h-12 mx-auto mb-3 opacity-30" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M14.857 17.082a23.848 23.848 0 005.454-1.31A8.967 8.967 0 0118 9.75v-.7V9A6 6 0 006 9v.75a8.967 8.967 0 01-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 01-5.714 0m5.714 0a3 3 0 11-5.714 0" />
              </svg>
              <p className="text-sm">Aucune notification</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
