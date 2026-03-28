'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/lib/auth-context';

export default function LoginPage() {
  const { login } = useAuth();
  const router = useRouter();
  const [email, setEmail] = useState('admin@couture.local');
  const [password, setPassword] = useState('Admin123!');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      await login(email, password);
      router.push('/');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur de connexion');
    }
    setLoading(false);
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background relative overflow-hidden">
      {/* Decorative background elements */}
      <div className="absolute top-0 right-0 w-[600px] h-[600px] rounded-full bg-primary/[0.03] -translate-y-1/2 translate-x-1/3" />
      <div className="absolute bottom-0 left-0 w-[400px] h-[400px] rounded-full bg-chart-2/10 translate-y-1/2 -translate-x-1/3" />

      <div className="w-full max-w-md mx-4 relative">
        {/* Logo area */}
        <div className="text-center mb-10">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-primary mb-4">
            <svg className="w-8 h-8 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M9.813 15.904L9 18.75l-.813-2.846a4.5 4.5 0 00-3.09-3.09L2.25 12l2.846-.813a4.5 4.5 0 003.09-3.09L9 5.25l.813 2.846a4.5 4.5 0 003.09 3.09L15.75 12l-2.846.813a4.5 4.5 0 00-3.09 3.09zM18.259 8.715L18 9.75l-.259-1.035a3.375 3.375 0 00-2.455-2.456L14.25 6l1.036-.259a3.375 3.375 0 002.455-2.456L18 2.25l.259 1.035a3.375 3.375 0 002.455 2.456L21.75 6l-1.036.259a3.375 3.375 0 00-2.455 2.456z" />
            </svg>
          </div>
          <h1 className="font-heading text-3xl font-bold text-primary">L&apos;Atelier</h1>
          <p className="font-heading text-2xl font-light text-chart-2">Couture</p>
        </div>

        {/* Login form */}
        <form onSubmit={handleSubmit} className="bg-card rounded-2xl shadow-lg shadow-primary/5 p-8 border border-border/30">
          <h2 className="font-heading text-lg font-semibold mb-6 text-foreground">Connexion</h2>

          <div className="space-y-4">
            <div>
              <label className="block text-xs font-semibold tracking-wider uppercase text-muted-foreground mb-1.5">Email</label>
              <input
                type="email" value={email} onChange={e => setEmail(e.target.value)}
                className="w-full h-11 px-4 rounded-xl border border-border bg-secondary
                  text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                placeholder="admin@couture.local"
              />
            </div>
            <div>
              <label className="block text-xs font-semibold tracking-wider uppercase text-muted-foreground mb-1.5">Mot de passe</label>
              <input
                type="password" value={password} onChange={e => setPassword(e.target.value)}
                className="w-full h-11 px-4 rounded-xl border border-border bg-secondary
                  text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
              />
            </div>
          </div>

          {error && (
            <p className="mt-3 text-sm text-destructive font-medium">{error}</p>
          )}

          <button
            type="submit" disabled={loading}
            className="w-full h-12 mt-6 rounded-full bg-primary text-white font-semibold text-sm
              hover:bg-primary active:scale-[0.98] transition-all
              disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {loading ? (
              <span className="inline-flex items-center gap-2">
                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                Connexion...
              </span>
            ) : 'Se connecter'}
          </button>
        </form>
      </div>
    </div>
  );
}
