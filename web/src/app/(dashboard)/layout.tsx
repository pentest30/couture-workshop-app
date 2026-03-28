'use client';
import Link from 'next/link';
import { usePathname, useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth-context';
import { useTheme } from '@/lib/theme-context';
import { useQuery } from '@tanstack/react-query';
import { notifications } from '@/lib/api';
import { useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';
import {
  LayoutDashboard, FileText, Users, Bell, Wallet, BookOpen,
  LogOut, Sun, Moon, Monitor, ChevronDown, Shield,
} from 'lucide-react';

// roles: which roles can see this nav item (empty = all authenticated users)
const NAV: { href: string; label: string; icon: typeof LayoutDashboard; roles?: string[] }[] = [
  { href: '/', label: 'Tableau de bord', icon: LayoutDashboard },
  { href: '/orders', label: 'Commandes', icon: FileText },
  { href: '/catalog', label: 'Catalogue', icon: BookOpen },
  { href: '/clients', label: 'Clients', icon: Users, roles: ['Manager', 'Tailor', 'Cashier'] },
  { href: '/notifications', label: 'Alertes', icon: Bell },
  { href: '/finance', label: 'Finance', icon: Wallet, roles: ['Manager', 'Cashier'] },
  { href: '/users', label: 'Utilisateurs', icon: Shield, roles: ['Manager'] },
];

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const pathname = usePathname();
  const router = useRouter();
  const { user, loading: authLoading, logout } = useAuth();
  const { theme, setTheme } = useTheme();

  const { data: unread } = useQuery({
    queryKey: ['unread-count'],
    queryFn: () => notifications.unreadCount(),
    refetchInterval: 30_000,
  });

  useEffect(() => {
    if (!authLoading && !user) router.push('/login');
  }, [authLoading, user, router]);

  if (authLoading || !user) {
    return <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="w-6 h-6 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
    </div>;
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Top header bar */}
      <header className="sticky top-0 z-30 border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="flex h-14 items-center px-4 gap-4">
          {/* Brand */}
          <Link href="/" className="flex items-center gap-2 mr-2">
            <div className="w-7 h-7 rounded-lg bg-primary flex items-center justify-center">
              <svg className="w-4 h-4 text-primary-foreground" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09z" />
              </svg>
            </div>
            <span className="font-heading text-sm font-bold hidden sm:inline">L&apos;Atelier Couture</span>
          </Link>

          <Separator orientation="vertical" className="h-6" />

          {/* Nav — filtered by role */}
          <nav className="flex items-center gap-1">
            {NAV.filter(item => !item.roles || item.roles.some(r => user.roles.includes(r))).map((item) => {
              const active = item.href === '/' ? pathname === '/' : pathname.startsWith(item.href);
              const Icon = item.icon;
              const isNotif = item.href === '/notifications';
              return (
                <Link key={item.href} href={item.href}>
                  <Button
                    variant={active ? 'secondary' : 'ghost'}
                    size="sm"
                    className="gap-1.5 text-xs font-medium relative"
                  >
                    <Icon className="h-4 w-4" />
                    <span className="hidden md:inline">{item.label}</span>
                    {isNotif && (unread?.count ?? 0) > 0 && (
                      <span className="absolute -top-1 -right-1 px-1 min-w-[16px] h-4 text-[10px] font-bold rounded-full bg-destructive text-white flex items-center justify-center">
                        {unread!.count}
                      </span>
                    )}
                  </Button>
                </Link>
              );
            })}
          </nav>

          <div className="ml-auto flex items-center gap-2">
            {/* Theme toggle */}
            <DropdownMenu>
              <DropdownMenuTrigger className="inline-flex items-center justify-center h-8 w-8 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring relative">
                <Sun className="h-4 w-4 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
                <Moon className="absolute h-4 w-4 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
                <span className="sr-only">Thème</span>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={() => setTheme('light')}>
                  <Sun className="mr-2 h-4 w-4" /> Clair
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setTheme('dark')}>
                  <Moon className="mr-2 h-4 w-4" /> Sombre
                </DropdownMenuItem>
                <DropdownMenuItem onClick={() => setTheme('system')}>
                  <Monitor className="mr-2 h-4 w-4" /> Système
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>

            <Separator orientation="vertical" className="h-6" />

            {/* User menu */}
            <DropdownMenu>
              <DropdownMenuTrigger className="inline-flex items-center gap-2 h-9 px-3 rounded-md text-sm font-medium ring-offset-background transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
                <div className="w-6 h-6 rounded-full bg-primary flex items-center justify-center text-primary-foreground text-[10px] font-bold">
                  {user.fullName?.[0] || '?'}
                </div>
                <span className="text-sm font-medium hidden sm:inline">{user.fullName}</span>
                <ChevronDown className="h-3 w-3 text-muted-foreground" />
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <div className="px-2 py-1.5">
                  <p className="text-sm font-medium">{user.fullName}</p>
                  <p className="text-xs text-muted-foreground">Gérant</p>
                </div>
                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={logout} className="text-destructive focus:text-destructive">
                  <LogOut className="mr-2 h-4 w-4" /> Déconnexion
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="flex-1">
        <div className="max-w-7xl mx-auto p-6">
          {children}
        </div>
      </main>
    </div>
  );
}
