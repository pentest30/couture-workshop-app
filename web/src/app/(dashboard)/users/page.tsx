'use client';
import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { users, type User, AVAILABLE_ROLES, ROLE_LABELS } from '@/lib/api';
import { DataTable } from '@/components/ui/data-table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { formatDate } from '@/lib/helpers';
import { Plus, Search, Pencil, Trash2, Shield } from 'lucide-react';
import { type ColumnDef } from '@tanstack/react-table';

export default function UsersPage() {
  const [search, setSearch] = useState('');
  const [showCreate, setShowCreate] = useState(false);
  const [editUser, setEditUser] = useState<User | null>(null);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['users', search],
    queryFn: () => users.list({ search: search || undefined }),
  });

  const handleDelete = async (user: User) => {
    if (!confirm(`Désactiver ${user.firstName} ${user.lastName} ?`)) return;
    try {
      await users.deactivate(user.id);
      queryClient.invalidateQueries({ queryKey: ['users'] });
    } catch (e) { alert(e instanceof Error ? e.message : 'Erreur'); }
  };

  const columns: ColumnDef<User>[] = [
    {
      accessorKey: 'firstName',
      header: 'Nom',
      cell: ({ row }) => (
        <div>
          <span className="font-semibold">{row.original.firstName} {row.original.lastName}</span>
          {!row.original.isActive && <Badge variant="secondary" className="ml-2 text-[10px]">Inactif</Badge>}
        </div>
      ),
    },
    {
      accessorKey: 'email',
      header: 'Email',
      cell: ({ row }) => <span className="text-sm text-muted-foreground">{row.original.email}</span>,
    },
    {
      accessorKey: 'phone',
      header: 'Téléphone',
      cell: ({ row }) => <span className="text-sm">{row.original.phone || '—'}</span>,
    },
    {
      accessorKey: 'roles',
      header: 'Rôles',
      cell: ({ row }) => (
        <div className="flex flex-wrap gap-1">
          {row.original.roles.map(r => (
            <Badge key={r} variant="outline" className="text-[10px] font-bold">
              {ROLE_LABELS[r] || r}
            </Badge>
          ))}
        </div>
      ),
    },
    {
      accessorKey: 'lastLoginAt',
      header: 'Dernière connexion',
      cell: ({ row }) => <span className="text-sm text-muted-foreground">{formatDate(row.original.lastLoginAt)}</span>,
    },
    {
      id: 'actions',
      header: '',
      cell: ({ row }) => (
        <div className="flex items-center gap-1">
          <button onClick={() => setEditUser(row.original)} className="p-1.5 rounded-md hover:bg-accent transition-colors">
            <Pencil className="h-3.5 w-3.5 text-muted-foreground" />
          </button>
          {row.original.isActive && (
            <button onClick={() => handleDelete(row.original)} className="p-1.5 rounded-md hover:bg-accent transition-colors">
              <Trash2 className="h-3.5 w-3.5 text-muted-foreground" />
            </button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-end justify-between">
        <div>
          <p className="text-[11px] font-bold tracking-widest uppercase text-muted-foreground mb-1">Administration</p>
          <h1 className="font-heading text-3xl font-bold">Utilisateurs</h1>
        </div>
        <Button onClick={() => setShowCreate(true)} className="rounded-full gap-2">
          <Plus className="h-4 w-4" /> Nouvel utilisateur
        </Button>
      </div>

      <div className="relative max-w-sm">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input value={search} onChange={e => setSearch(e.target.value)}
          placeholder="Rechercher par nom ou email..." className="pl-9 h-10 rounded-xl" />
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        totalCount={data?.totalCount ?? 0}
        isLoading={isLoading}
        emptyMessage="Aucun utilisateur trouvé"
      />

      {/* Create dialog */}
      <UserFormDialog
        open={showCreate}
        onClose={() => setShowCreate(false)}
        onSaved={() => { setShowCreate(false); queryClient.invalidateQueries({ queryKey: ['users'] }); }}
      />

      {/* Edit dialog */}
      {editUser && (
        <UserFormDialog
          open={true}
          user={editUser}
          onClose={() => setEditUser(null)}
          onSaved={() => { setEditUser(null); queryClient.invalidateQueries({ queryKey: ['users'] }); }}
        />
      )}
    </div>
  );
}

function UserFormDialog({ open, onClose, onSaved, user }: {
  open: boolean; onClose: () => void; onSaved: () => void; user?: User;
}) {
  const isEdit = !!user;
  const [firstName, setFirstName] = useState(user?.firstName ?? '');
  const [lastName, setLastName] = useState(user?.lastName ?? '');
  const [email, setEmail] = useState(user?.email ?? '');
  const [phone, setPhone] = useState(user?.phone ?? '');
  const [password, setPassword] = useState('');
  const [selectedRoles, setSelectedRoles] = useState<string[]>(user?.roles ?? []);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const toggleRole = (role: string) => {
    setSelectedRoles(prev => prev.includes(role) ? prev.filter(r => r !== role) : [...prev, role]);
  };

  const handleSave = async () => {
    if (!firstName.trim() || !lastName.trim()) { setError('Prénom et nom obligatoires'); return; }
    if (!isEdit && !email.trim()) { setError('Email obligatoire'); return; }
    if (!isEdit && !password) { setError('Mot de passe obligatoire'); return; }
    if (selectedRoles.length === 0) { setError('Sélectionnez au moins un rôle'); return; }

    setSaving(true);
    setError('');

    try {
      if (isEdit) {
        await users.update(user!.id, {
          firstName: firstName.trim(),
          lastName: lastName.trim(),
          phone: phone.trim() || null,
          roles: selectedRoles,
          ...(password ? { newPassword: password } : {}),
        });
      } else {
        await users.create({
          firstName: firstName.trim(),
          lastName: lastName.trim(),
          email: email.trim(),
          password,
          phone: phone.trim() || null,
          roles: selectedRoles,
        });
      }
      onSaved();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Erreur');
    }
    setSaving(false);
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) onClose(); }}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5 text-primary" />
            {isEdit ? 'Modifier l\'utilisateur' : 'Nouvel utilisateur'}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4 pt-2">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Prénom</label>
              <Input value={firstName} onChange={e => setFirstName(e.target.value)} />
            </div>
            <div>
              <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Nom</label>
              <Input value={lastName} onChange={e => setLastName(e.target.value)} />
            </div>
          </div>

          {!isEdit && (
            <div>
              <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Email</label>
              <Input type="email" value={email} onChange={e => setEmail(e.target.value)} placeholder="prenom@couture.local" />
            </div>
          )}

          <div>
            <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">Téléphone</label>
            <Input value={phone} onChange={e => setPhone(e.target.value)} placeholder="0550123456" />
          </div>

          <div>
            <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-1">
              {isEdit ? 'Nouveau mot de passe (laisser vide pour garder)' : 'Mot de passe'}
            </label>
            <Input type="password" value={password} onChange={e => setPassword(e.target.value)} />
          </div>

          <div>
            <label className="block text-xs font-bold tracking-wider uppercase text-muted-foreground mb-2">Rôles</label>
            <div className="flex flex-wrap gap-2">
              {AVAILABLE_ROLES.map(role => (
                <button key={role} type="button" onClick={() => toggleRole(role)}
                  className={`px-3 py-1.5 rounded-full text-xs font-bold border transition-colors ${
                    selectedRoles.includes(role)
                      ? 'bg-primary text-primary-foreground border-primary'
                      : 'bg-card border-border text-muted-foreground hover:border-primary/50'
                  }`}>
                  {ROLE_LABELS[role] || role}
                </button>
              ))}
            </div>
          </div>

          {error && (
            <div className="rounded-lg bg-destructive/10 border border-destructive/20 p-2">
              <p className="text-xs text-destructive font-medium">{error}</p>
            </div>
          )}

          <div className="flex justify-end gap-2 pt-2">
            <Button variant="outline" onClick={onClose} className="rounded-full">Annuler</Button>
            <Button onClick={handleSave} disabled={saving} className="rounded-full">
              {saving ? 'En cours...' : isEdit ? 'Enregistrer' : 'Créer'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}