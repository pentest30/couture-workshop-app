# 📋 Spécifications Fonctionnelles — Atelier Couture App
**Version** : 1.0.0 | **Date** : Mars 2026 | **Statut** : Draft

---

## Table des Matières

1. [Présentation du Projet](#1-présentation-du-projet)
2. [Périmètre Fonctionnel](#2-périmètre-fonctionnel)
3. [Spécifications Fonctionnelles](#3-spécifications-fonctionnelles)
4. [F01 — Gestion des Commandes](#f01--gestion-des-commandes)
5. [F02 — Statuts & Cycle de Vie](#f02--statuts--cycle-de-vie)
6. [F03 — Types de Travaux (Brodé, Perlé)](#f03--types-de-travaux-brodé-perlé)
7. [F04 — Tableau de Bord Trimestriel](#f04--tableau-de-bord-trimestriel)
8. [F05 — Système de Notifications](#f05--système-de-notifications)
9. [F06 — Gestion des Clients](#f06--gestion-des-clients)
10. [F07 — Gestion des Utilisateurs & Rôles](#f07--gestion-des-utilisateurs--rôles)
11. [F08 — Gestion Financière](#f08--gestion-financière)
12. [Annexes — Cas d'Utilisation](#annexes--cas-dutilisation)
13. [Contraintes & Règles Métier](#8-contraintes--règles-métier)
14. [Interfaces Utilisateur](#9-interfaces-utilisateur)
15. [Exigences Non-Fonctionnelles](#10-exigences-non-fonctionnelles)
16. [Annexes — Matrice Features / Rôles](#annexes--matrice-features--rôles)

---

## 1. Présentation du Projet

### 1.1 Contexte
L'application vise à digitaliser la gestion opérationnelle d'un atelier de couture artisanal ou semi-industriel, en intégrant les workflows spécifiques de la couture haut de gamme (broderie, perlerie) et les contraintes usuelles de suivi (délais, encaissements, traçabilité).

### 1.2 Objectifs principaux
- Centraliser le suivi des commandes du devis à la livraison
- Gérer les types de travaux : simple / brodé / perlé / mixte
- Offrir un tableau de bord analytique par trimestre
- Automatiser les alertes et notifications en cas de risque ou dépassement de délai
- Améliorer la traçabilité client et la qualité de service

### 1.3 Utilisateurs cibles
| Profil | Rôle | Accès |
|---|---|---|
| Gérant / Patron | Administration complète | Toutes fonctionnalités |
| Couturier(ère) | Suivi de ses commandes | Commandes assignées + notifications |
| Brodeur / Perleur | Travaux spéciaux | Commandes brodées / perlées selon affectation |
| Caissier | Encaissements | Module financier + reçus |

---

## 2. Périmètre Fonctionnel

### 2.1 Modules inclus / exclus (v1)
| Code | Module | Priorité | Statut | Correspondance (features) |
|---|---|---:|---|---|
| M01 | Gestion des Commandes & Statuts | P0 | In Scope | F01, F02 |
| M02 | Types de Travaux (Brodé / Perlé / Mixte) | P0 | In Scope | F03 |
| M03 | Tableau de Bord Trimestriel | P0 | In Scope | F04 |
| M04 | Système de Notifications & Alertes | P0 | In Scope | F05 |
| M05 | Gestion Clients (N° + fiche + mensurations) | P1 | In Scope | F06 |
| M06 | Gestion des Couturiers / utilisateurs & rôles | P1 | In Scope | F07 |
| M07 | Suivi des Paiements / Acomptes | P1 | In Scope | F08 |
| M08 | Génération de Reçus / Factures | P2 | In Scope | F09 |
| M09 | Importation données Excel | P3 | Out of Scope v1 | — |

---

## 3. Spécifications Fonctionnelles

| ID | Feature | Priorité | Module | Statut |
|----|---------|----------|--------|--------|
| F01 | Création & suivi des commandes | P0 | Orders | ✅ In Scope |
| F02 | Cycle de vie des statuts | P0 | Orders | ✅ In Scope |
| F03 | Types de travaux : Brodé / Perlé / Mixte | P0 | Orders | ✅ In Scope |
| F04 | Tableau de bord trimestriel | P0 | Dashboard | ✅ In Scope |
| F05 | Notifications & alertes dépassement délai | P0 | Notifications | ✅ In Scope |
| F06 | Gestion clients & mensurations | P1 | Clients | ✅ In Scope |
| F07 | Gestion utilisateurs & rôles | P1 | Admin | ✅ In Scope |
| F08 | Gestion financière (acomptes, soldes) | P1 | Finance | ✅ In Scope |
| F09 | Génération de reçus / factures PDF | P2 | Finance | ✅ In Scope |
| F10 | Catalogue références visuelles | P2 | Catalog | ✅ In Scope |
| F11 | Rapports & exports CSV/Excel | P2 | Reports | ✅ In Scope |
| F12 | Mode hors-ligne (offline sync) | P1 | Core | ✅ In Scope |

---

## F01 — Gestion des Commandes

### Description
Fonctionnalité centrale de l'application. Permet la création, le suivi et la clôture complète d'une commande de couture, du premier contact client jusqu'à la livraison finale.

### Feature Breakdown

#### F01.1 — Création d'une commande
- Formulaire de création en **3 étapes** :
  1. **Étape 1 — Client** : sélection d'un client existant (par N° ou nom) ou création rapide d'un nouveau client
  2. **Étape 2 — Travail** : type de travail (Simple/Brodé/Perlé/Mixte), description, tissu, photos de référence, notes techniques
  3. **Étape 3 — Planning** : date de livraison prévue, acompte versé, prix total, couturier assigné
- Génération automatique du **N° de commande** au format `CMD-YYYY-NNNN` (ex: `CMD-2026-0042`)
- Statut initial automatique : `REÇUE`
- Calcul automatique du **solde restant** = Prix total - Acompte

#### F01.2 — Consultation d'une commande
- Fiche commande complète affichant :
  - En-tête : N° commande, N° client, nom client, statut (badge coloré), type de travail
  - Bloc planning : date de réception, date livraison prévue, date livraison réelle, jours de retard si applicable
  - Bloc financier : prix total, acompte versé, solde restant
  - Bloc travail : description, tissu, instructions, photos
  - **Timeline des statuts** : historique horodaté de chaque changement de statut (qui / quand / depuis)
  - Bloc notifications : liste des alertes envoyées sur cette commande

#### F01.3 — Modification d'une commande
- Editable tant que statut ≠ `LIVRÉE`
- Champs modifiables : date livraison prévue, prix total, acompte, notes, couturier assigné, photos
- Chaque modification est tracée dans l'historique d'audit

#### F01.4 — Changement de statut
- Bouton d'action contextuel selon le statut courant (ex: si `EN_COURS` → boutons "Passer en Broderie", "Marquer Prête", "Signaler Retouche")
- Transitions validées côté serveur
- Chaque transition déclenche les notifications associées

#### F01.5 — Recherche & Filtrage
- Recherche par : N° commande, N° client, nom client
- Filtres combinables : statut, type de travail, couturier, plage de dates
- Tri : date de livraison (ASC/DESC), date de création, statut
- Pagination : 20 commandes par page

#### F01.6 — Clôture d'une commande
- Passage au statut `LIVRÉE` uniquement si :
  - Solde = 0 (soldé) **OU** validation explicite du gérant avec motif
  - Date de livraison réelle saisie
- Génération automatique du reçu de livraison PDF

---

## F02 — Statuts & Cycle de Vie

### Description
Définit les états possibles d'une commande et les règles de transition entre ces états. Chaque statut a une sémantique précise et déclenche des comportements spécifiques.

### Référentiel des Statuts

| Code | Libellé | Couleur | Icône | Description métier |
|------|---------|---------|-------|-------------------|
| `REÇUE` | Reçue | 🔵 `#1565C0` | 📥 | Commande enregistrée, pas encore assignée ou démarrée |
| `EN_ATTENTE` | En Attente | 🟡 `#F9A825` | ⏳ | Blocage : attente tissu, modèle, validation client |
| `EN_COURS` | En Cours | 🟠 `#E65100` | 🪡 | Travail de couture actif |
| `BRODERIE` | En Broderie | 🟣 `#6A1B9A` | 🧵 | Phase de broderie en cours |
| `PERLAGE` | En Perlage | 💜 `#880E4F` | 💎 | Phase de perlage / strass en cours |
| `RETOUCHE` | En Retouche | 🔴 `#C62828` | ✂️ | Retour client pour corrections |
| `PRÊTE` | Prête | 🟢 `#2E7D32` | ✅ | Commande terminée, en attente de récupération |
| `LIVRÉE` | Livrée | ⚫ `#424242` | 📦 | Commande remise au client, dossier clos |

### Matrice des Transitions Autorisées

```
REÇUE ──────────► EN_ATTENTE
  │                    │
  └────────────────► EN_COURS ──────► BRODERIE ──► PERLAGE
                       │                  │            │
                       ▼                  ▼            ▼
                    RETOUCHE ◄─────────────────────────┘
                       │
                       ▼
                    PRÊTE ──────────► LIVRÉE (état final)
```

### Règles de Transition

| Transition | Condition requise | Notification déclenchée |
|-----------|-------------------|------------------------|
| `* → EN_COURS` | Couturier assigné obligatoire | N07 — Alerte couturier |
| `EN_COURS → BRODERIE` | Type = Brodé ou Mixte | N07 — Alerte brodeur |
| `EN_COURS → PERLAGE` | Type = Perlé ou Mixte | N07 — Alerte perleur |
| `* → RETOUCHE` | Motif de retouche obligatoire | N05 — Alerte gérant + couturier |
| `* → PRÊTE` | Aucune | N06 — Alerte gérant |
| `PRÊTE → LIVRÉE` | Solde soldé OU validation gérant + date livraison réelle | N08 si solde impayé |

### Feature F02.1 — Timeline Visuelle des Statuts
- Affichage chronologique sur la fiche commande
- Chaque entrée : statut précédent → statut suivant | utilisateur | date/heure | éventuel motif
- Durée passée dans chaque statut calculée et affichée
- Identification visuelle des statuts dépassés ou anormalement longs

---

## F03 — Types de Travaux (Brodé, Perlé)

### Description
Le système distingue 4 types de travaux avec des workflows, délais et acteurs spécifiques. Les types Brodé, Perlé et Mixte activent des phases supplémentaires dans le cycle de vie.

### Feature F03.1 — Définition des Types

#### Type SIMPLE
- Couture standard sans ornements spéciaux
- Workflow : `EN_COURS → PRÊTE`
- Délai standard configurable : **3–7 jours**
- Acteur : Couturier(ère)

#### Type BRODÉ
- Broderie à la main ou machine (motifs floraux, géométriques, traditionnels)
- Workflow : `EN_COURS → BRODERIE → PRÊTE`
- Délai standard configurable : **7–21 jours**
- Acteurs : Couturier(ère) + Brodeur(euse)
- Champs supplémentaires : style de broderie, couleurs fils, densité, zone à broder

#### Type PERLÉ
- Application de perles, sequins, cristaux, strass, paillettes
- Workflow : `EN_COURS → PERLAGE → PRÊTE`
- Délai standard configurable : **10–30 jours**
- Acteurs : Couturier(ère) + Perleur(euse)
- Champs supplémentaires : type de perles, disposition, zones concernées

#### Type MIXTE
- Combinaison de broderie et perlage sur le même vêtement
- Workflow : `EN_COURS → BRODERIE → PERLAGE → PRÊTE`
- Délai standard configurable : **14–45 jours**
- Acteurs : Couturier(ère) + Brodeur(euse) + Perleur(euse)
- Les deux phases peuvent être séquentielles ou partiellement parallèles

### Feature F03.2 — Affectation Multi-Artisans
- Une commande MIXTE peut avoir jusqu'à **3 artisans assignés** simultanément :
  - `assigned_tailor_id` → Couturier principal
  - `assigned_embroiderer_id` → Brodeur (Brodé/Mixte)
  - `assigned_beader_id` → Perleur (Perlé/Mixte)
- Chaque artisan ne voit que les commandes dans **sa phase active**

### Feature F03.3 — Seuils d'Alerte par Type
Délais paramétrables déclenchant la notification N04 (commande bloquée) :

| Type | Seuil défaut sans changement de statut |
|------|---------------------------------------|
| SIMPLE | 3 jours |
| BRODÉ | 7 jours |
| PERLÉ | 10 jours |
| MIXTE | 14 jours |

---

## F04 — Tableau de Bord Trimestriel

### Description
Vue analytique principale accessible dès la connexion. Agrège les données des commandes sur un trimestre sélectionnable et expose KPIs, liste filtrée et graphiques.

### Feature F04.1 — Sélecteur de Trimestre
- Navigation : `< T1 2026 >` avec flèches précédent/suivant
- Dropdown rapide : T1 / T2 / T3 / T4 + sélecteur d'année
- Trimestre courant sélectionné par défaut au chargement
- Définition : T1 = Jan–Mar | T2 = Avr–Jun | T3 = Jul–Sep | T4 = Oct–Déc

### Feature F04.2 — Cartes KPI

| KPI | Formule | Format affiché |
|-----|---------|----------------|
| Total commandes | `COUNT(commandes WHERE date_reception IN trimestre)` | Nombre + Δ% vs trimestre précédent |
| Commandes livrées | `COUNT(status = LIVRÉE AND date_livraison_reelle IN trimestre)` | Nombre + badge vert |
| Commandes en retard | `COUNT(status != LIVRÉE AND date_livraison_prevue < today)` | Nombre + badge rouge si > 0 |
| Taux livraison à temps | `(livrées_dans_délai / total_livrées) * 100` | Pourcentage + jauge colorée |
| CA encaissé | `SUM(montants_encaissés WHERE date IN trimestre)` | Montant DZD |
| Soldes en attente | `SUM(solde_restant WHERE status != LIVRÉE)` | Montant DZD + alerte si > seuil |
| Commandes brodées | `COUNT(type IN [BRODÉ, MIXTE])` | Nombre + icône 🧵 |
| Commandes perlées | `COUNT(type IN [PERLÉ, MIXTE])` | Nombre + icône 💎 |

### Feature F04.3 — Liste des Commandes du Trimestre

Colonnes affichées (toutes triables) :

| Colonne | Source | Comportement |
|---------|--------|-------------|
| N° Commande | `order.code` | Lien cliquable vers fiche |
| N° Client | `client.code` | Ex: C-0042 |
| Nom Client | `client.full_name` | Recherche |
| Type Travail | `order.work_type` | Badge coloré (Simple/Brodé/Perlé/Mixte) |
| Date Livraison | `order.expected_delivery_date` | Rouge si dépassée |
| Statut | `order.status` | Badge coloré |
| Retard | Calculé : `today - expected_delivery_date` si en retard | Affiché en jours (rouge) ou `—` |
| Couturier | `tailor.name` | |
| Actions | — | Voir / Changer statut |

Comportements :
- Surbrillance rouge sur toute la ligne si commande en retard
- Pagination 20 éléments/page avec info "N–M sur Total"
- Barre de recherche globale (N° commande, nom client)
- Filtres combinables : statut, type, couturier, retard seulement

### Feature F04.4 — Graphiques Analytiques

1. **Histogramme mensuel** : Nb commandes par mois du trimestre, barres empilées par type (Simple/Brodé/Perlé/Mixte)
2. **Donut Statuts** : Répartition en % des commandes par statut courant
3. **Donut Types** : Répartition Simple / Brodé / Perlé / Mixte
4. **Courbe CA** : Évolution du chiffre d'affaires sur les 4 derniers trimestres (comparaison)
5. **Bar Chart retards** : Nb de jours de retard moyen par couturier (top 5)

### Feature F04.5 — Export
- Export **CSV** de la liste complète du trimestre
- Export **Excel (.xlsx)** avec mise en forme (en-têtes, couleurs statuts)
- Export **PDF** du rapport trimestriel complet (KPIs + graphiques + liste)

---

## F05 — Système de Notifications

### Description
Moteur d'alertes proactif fonctionnant en tâche de fond. Surveille les commandes en temps réel et notifie les acteurs concernés selon des règles métier paramétrables.

### Feature F05.1 — Référentiel des Notifications

| ID | Déclencheur | Destinataires | Canal | Priorité |
|----|------------|---------------|-------|----------|
| N01 | Date livraison dépassée (J+1) | Couturier assigné + Gérant | In-App + SMS | 🔴 CRITIQUE |
| N02 | Date livraison dans 24h | Couturier assigné | In-App + SMS | 🟠 HAUTE |
| N03 | Date livraison dans 48h | Couturier assigné | In-App | 🟡 MOYENNE |
| N04 | Commande bloquée sans changement de statut > N jours | Gérant | In-App | 🟠 HAUTE |
| N05 | Statut changé → RETOUCHE | Couturier assigné + Gérant | In-App + SMS | 🟠 HAUTE |
| N06 | Statut changé → PRÊTE | Gérant | In-App | 🟡 MOYENNE |
| N07 | Nouvelle commande assignée à un artisan | Artisan concerné | In-App + SMS | 🟡 MOYENNE |
| N08 | Tentative de livraison avec solde impayé | Gérant | In-App | 🔴 CRITIQUE |

### Feature F05.2 — Centre de Notifications In-App
- Icône cloche dans la navbar avec **compteur badge** (non-lues)
- Panneau latéral coulissant listant les notifications :
  - Icône de priorité + titre + résumé + heure relative (ex: "il y a 2h")
  - Lien direct vers la commande concernée
  - Actions : **Marquer comme lu** | **Voir la commande** | **Ignorer**
- Onglets : Toutes | Non lues | Critiques
- Marquer tout comme lu en un clic
- **Persistence** : notifications conservées 30 jours

### Feature F05.3 — Notifications SMS
- Envoi via passerelle SMS configurable (Twilio, Vonage, ou passerelle locale)
- Template de message paramétrable par type de notification
- Templates par défaut :

```
N01 (Retard) :
[ATELIER] ⚠️ Commande {order_code} — {client_name}
Délai dépassé de {days} jour(s). Statut actuel : {status}.
Merci d'agir rapidement.

N07 (Assignation) :
[ATELIER] Bonjour {tailor_name}, une nouvelle commande
{order_code} vous a été assignée. Livraison le {delivery_date}.
```

- Confirmation d'envoi trackée (sent / delivered / failed)
- Logs d'envoi consultables par le gérant
- Plage horaire d'envoi configurable (ex: 8h00 – 20h00 uniquement)

### Feature F05.4 — Paramétrage des Alertes
Interface dédiée pour le gérant :
- Activer / désactiver chaque notification (N01 → N08) indépendamment
- Modifier les **seuils de délai** par type de travail pour N04 :
  - Simple : `__` jours (défaut : 3)
  - Brodé : `__` jours (défaut : 7)
  - Perlé : `__` jours (défaut : 10)
  - Mixte : `__` jours (défaut : 14)
- Choisir les canaux actifs par notification (In-App seul / In-App + SMS)
- Configurer la plage horaire SMS
- Tester l'envoi d'un SMS de test depuis l'interface

---

## F06 — Gestion des Clients

### Description
Gestion du référentiel clients avec fiche complète, mensurations personnalisables et historique des commandes.

### Feature F06.1 — Fiche Client

| Champ | Type | Obligatoire | Remarque |
|-------|------|-------------|---------|
| N° Client | Auto `C-NNNN` | Auto | Généré à la création |
| Nom | String | ✅ | |
| Prénom | String | ✅ | |
| Téléphone principal | String | ✅ | Format algérien : 05/06/07XXXXXXXX |
| Téléphone secondaire | String | ❌ | |
| Adresse | Text | ❌ | |
| Date de naissance | Date | ❌ | Pour les VIP anniversaire |
| Notes | Text | ❌ | Préférences, habitudes |

### Feature F06.2 — Mensurations Personnalisables
- Grille de mensurations **entièrement configurable** par le gérant
- Champs par défaut fournis :

| Mensuration | Unité |
|------------|-------|
| Tour de poitrine | cm |
| Tour de taille | cm |
| Tour de hanches | cm |
| Longueur robe (dos) | cm |
| Longueur jupe | cm |
| Longueur manche | cm |
| Tour de bras | cm |
| Épaule | cm |
| Carrure dos | cm |
| Hauteur totale | cm |

- Possibilité d'**ajouter des champs personnalisés** (ex: profondeur d'encolure, longueur haïk, tour de tête...)
- Historique des mensurations : chaque modification est datée (détection des changements de morphologie)

### Feature F06.3 — Historique & Statistiques Client
Depuis la fiche client :
- Liste de toutes ses commandes (toutes dates confondues)
- Statistiques : nb total commandes | montant total encaissé | dernière visite | commandes en cours
- Bouton "Nouvelle commande" pré-remplissant le client

### Feature F06.4 — Recherche Client
- Par N° client (exact)
- Par nom / prénom (partiel, insensible casse et diacritiques)
- Par numéro de téléphone (partiel)
- Résultats en temps réel (debounced 300ms)
- Affichage : N° client | Nom | Téléphone | Nb commandes en cours

---

## F07 — Gestion des Utilisateurs & Rôles

### Description
Gestion des comptes utilisateurs de l'atelier avec système de rôles basé sur les responsabilités métier (RBAC).

### Feature F07.1 — Rôles

| Rôle | Code | Description |
|------|------|-------------|
| Gérant | `MANAGER` | Accès total, paramétrage, rapports, validation livraison |
| Couturier | `TAILOR` | Voir et gérer ses commandes assignées uniquement |
| Brodeur | `EMBROIDERER` | Voir les commandes en phase BRODERIE assignées |
| Perleur | `BEADER` | Voir les commandes en phase PERLAGE assignées |
| Caissier | `CASHIER` | Module financier : encaissements, reçus |

### Feature F07.2 — Profil Utilisateur
- Nom, prénom, téléphone (obligatoire pour SMS)
- Rôle(s) assignés (multi-rôle possible, ex: TAILOR + EMBROIDERER)
- Statut : Actif / Inactif
- Historique de connexion

### Feature F07.3 — Authentification
- Login par identifiant + mot de passe
- OTP SMS optionnel (2FA)
- Session persistante configurable (durée : 8h / 24h / 7 jours)
- Déconnexion automatique après inactivité

---

## F08 — Gestion Financière

### Description
Suivi des flux financiers liés aux commandes : acomptes, paiements, soldes, méthodes de paiement.

### Feature F08.1 — Suivi par Commande
- Prix total | Acompte versé à la prise de commande | Solde restant (calculé)
- Possibilité de saisir des **paiements partiels intermédiaires** (ex: 2ème acompte)
- Méthodes de paiement supportées : Espèces | Virement | CCP | BaridiMob | Dahabia
- Chaque paiement : montant + méthode + date + note

### Feature F08.2 — Vue Financière du Tableau de Bord
- CA encaissé du trimestre (par méthode de paiement)
- Soldes en attente (commandes non soldées)
- Commandes livrées avec solde impayé (alerte)
- Graphique : répartition par méthode de paiement

### Feature F08.3 — Génération de Reçus
- Reçu PDF généré automatiquement à chaque paiement
- En-tête atelier (nom, adresse, logo, téléphone)
- Corps : N° reçu, date, N° commande, client, montant, méthode, cumul payé, solde restant
- Signature numérique ou zone de signature manuelle

---

## Annexes — Cas d'Utilisation

### UC01 — Créer une nouvelle commande brodée

**Acteur principal** : Gérant ou Couturier senior  
**Pré-conditions** : Client existant ou nouveau à créer  
**Déclencheur** : Client se présente à l'atelier avec un tissu à broder

**Scénario nominal** :
1. L'utilisateur clique sur **"Nouvelle Commande"**
2. Étape 1 — Il recherche le client par nom "Benali" → sélectionne "Mme Sara Benali (C-0042)"
3. Étape 2 — Il sélectionne le type de travail : **BRODÉ**
   - Les champs spécifiques broderie apparaissent : style de broderie, couleurs fils, zone concernée
   - Il saisit : "Broderie florale sur corsage et bas de robe, fils dorés et bordeaux"
   - Il uploade 2 photos de référence
4. Étape 3 — Il saisit :
   - Date de livraison prévue : 15/04/2026
   - Acompte : 3 000 DZD
   - Prix total : 8 500 DZD
   - Couturier assigné : Mme Fatima (sélection dans la liste)
5. Il clique sur **"Créer la Commande"**
6. Le système génère le N° `CMD-2026-0058`, statut = `REÇUE`
7. Une notification N07 est envoyée à Mme Fatima (In-App + SMS)
8. Un reçu d'acompte PDF est généré automatiquement

**Résultat** : Commande créée, couturier notifié, acompte tracé

---

### UC02 — Changer le statut d'une commande vers "En Broderie"

**Acteur principal** : Couturier (Mme Fatima)  
**Pré-conditions** : Commande `CMD-2026-0058` en statut `EN_COURS`  
**Déclencheur** : La couturière a terminé la base de la robe et passe la commande au brodeur

**Scénario nominal** :
1. Mme Fatima ouvre son **espace "Mes Commandes"**
2. Elle voit `CMD-2026-0058` — Mme Benali — statut `EN_COURS`
3. Elle clique sur **"Passer en Broderie"** (bouton contextuel)
4. Le système vérifie : type = BRODÉ ✅, brodeur disponible ?
5. Mme Fatima sélectionne le brodeur : M. Karim
6. Elle confirme le changement
7. Statut → `BRODERIE`
8. L'entrée timeline est créée : `EN_COURS → BRODERIE | Mme Fatima | 25/03/2026 14:32`
9. Notification N07 envoyée à M. Karim : "Commande CMD-2026-0058 vous a été assignée (broderie)"

**Variante (A) — Brodeur non disponible** :
- 4a. Aucun brodeur libre dans la liste
- 4b. Le gérant est alerté In-App : "Commande CMD-2026-0058 en attente d'un brodeur disponible"

---

### UC03 — Consulter le tableau de bord du T1 2026

**Acteur principal** : Gérant  
**Pré-conditions** : Des commandes existent dans la période  
**Déclencheur** : Bilan de fin de trimestre

**Scénario nominal** :
1. Le gérant se connecte → le dashboard s'ouvre par défaut sur le trimestre courant
2. Il voit les KPIs :
   - 47 commandes ce trimestre (+12% vs T4 2025)
   - 3 commandes en retard (badge rouge)
   - Taux livraison à temps : 87%
   - CA encaissé : 412 000 DZD
   - 18 commandes brodées | 9 commandes perlées
3. Il clique sur le badge rouge "3 en retard" → la liste est automatiquement filtrée sur les commandes en retard
4. Il voit les 3 commandes avec la colonne "Retard" affichant 2j, 5j, 1j
5. Il clique sur la commande à 5j de retard pour ouvrir la fiche
6. Il change le statut vers `RETOUCHE` avec motif "Ajustement taille demandé par cliente"
7. Retour au dashboard — les KPIs se mettent à jour

---

### UC04 — Recevoir une alerte de dépassement de délai

**Acteur principal** : Système (automatique) → notifie le Couturier + Gérant  
**Pré-conditions** : Commande `CMD-2026-0043` type PERLÉ, date livraison `22/03/2026`, statut `PERLAGE` inchangé depuis 12 jours (seuil PERLÉ = 10 jours)  
**Déclencheur** : Job CRON quotidien (02h00)

**Scénario nominal** :
1. Le job nocturne évalue toutes les commandes actives
2. `CMD-2026-0043` : dernière transition de statut = 13/03/2026, durée dans statut = 12 jours > seuil 10 jours
3. Le système crée la notification N04 :
   - **In-App** pour le Gérant : "⚠️ CMD-2026-0043 (Perlé) bloquée depuis 12 jours. Statut : PERLAGE."
4. Le lendemain matin (`22/03/2026`), la date de livraison est dépassée → N01 déclenché :
   - **In-App + SMS** pour le perleur M. Rachid : "ATELIER RANIA — Commande CMD-2026-0043 — Mme Kaci : délai dépassé de 1 jour. Veuillez contacter le gérant."
   - **In-App + SMS** pour le Gérant
5. Le gérant voit les notifications à sa connexion
6. Il ouvre la commande, appelle M. Rachid, met à jour la date de livraison prévue au `02/04/2026`
7. Le système re-calcule — plus de retard — les alertes sont résolues

---

### UC05 — Livrer une commande et clôturer le dossier

**Acteur principal** : Gérant ou Caissier  
**Pré-conditions** : Commande `CMD-2026-0058` en statut `PRÊTE`  
**Déclencheur** : Mme Benali vient récupérer sa robe

**Scénario nominal** :
1. Le gérant ouvre la fiche de `CMD-2026-0058`
2. Le statut est `PRÊTE` — solde restant : **5 500 DZD**
3. Mme Benali règle le solde en espèces
4. Le gérant clique sur **"Encaisser Solde"** → saisit 5 500 DZD en espèces
5. Solde restant = 0 DZD ✅
6. Il clique sur **"Marquer comme Livrée"**
7. Il saisit la date de livraison réelle : 28/03/2026
8. Le système passe le statut à `LIVRÉE` (irréversible)
9. Un **reçu de livraison final** PDF est généré automatiquement
10. La commande disparaît des listes actives et est archivée

**Variante (B) — Livraison avec solde impayé** :
- 5b. Mme Benali ne peut pas payer le solde aujourd'hui
- 6b. Le gérant clique sur "Livrer sans solde" → popup de confirmation avec champ motif obligatoire
- 7b. Il saisit le motif : "Cliente absente, accord verbal remboursement semaine prochaine"
- 8b. Notification N08 créée pour le gérant : solde 5 500 DZD impayé sur CMD-2026-0058
- 9b. La commande passe à LIVRÉE avec flag `solde_impayé = true` visible dans les rapports

---

### UC06 — Ajouter un client avec mensurations

**Acteur principal** : Gérant ou Couturier  
**Déclencheur** : Nouveau client qui passe une commande pour la première fois

**Scénario nominal** :
1. Depuis "Nouvelle Commande" → Étape 1, l'utilisateur clique sur **"+ Nouveau Client"**
2. Il saisit : Prénom = Nadia, Nom = Hamidi, Tel = 0550 123 456
3. Il clique sur **"Prendre les Mensurations"**
4. La grille de mensurations s'affiche avec tous les champs configurés pour l'atelier
5. Il saisit les mesures (poitrine = 92cm, taille = 74cm, hanches = 98cm...)
6. Il clique sur **"Enregistrer le client"**
7. Le client reçoit le code `C-0087`
8. Le formulaire de commande est automatiquement rempli avec ce client

---

## 8. Contraintes & Règles Métier

### RG01 — Numérotation automatique
- N° Commande : `CMD-{YYYY}-{NNNN}` (réinitialisation annuelle, paddé à 4 chiffres)
- N° Client : `C-{NNNN}` (séquentiel global, jamais réinitialisé)
- N° Reçu : `REC-{YYYY}-{NNNN}` (séquentiel annuel)

### RG02 — Date de livraison minimale
La date de livraison prévue doit respecter : `date_reception + délai_minimum_type`

| Type | Délai minimum imposé |
|------|---------------------|
| SIMPLE | 1 jour ouvrable |
| BRODÉ | 3 jours ouvrables |
| PERLÉ | 5 jours ouvrables |
| MIXTE | 7 jours ouvrables |

### RG03 — Capacité couturier
- Un couturier ne peut avoir qu'un maximum configurable de commandes actives simultanément (défaut : **10**)
- Si ce plafond est atteint, une alerte apparaît lors de l'assignation (non bloquante, confirmation requise)

### RG04 — Immutabilité de LIVRÉE
- Une commande au statut `LIVRÉE` ne peut plus être modifiée
- Exception : le gérant peut ajouter une note mais ne peut pas changer le statut

### RG05 — Audit Trail complet
Chaque action critique est tracée dans une table d'audit :
- `who` (user_id) | `what` (action) | `when` (timestamp UTC) | `on_what` (entity + id) | `before` | `after`
- Actions tracées : création commande, changement statut, modification prix, encaissement, création client

### RG06 — Calcul du retard
```
retard_jours = MAX(0, (today - expected_delivery_date).jours_ouvrables)
is_late = status != LIVRÉE AND expected_delivery_date < today
```

### RG07 — Dédoublonnage client
- Alerte si un client est créé avec un numéro de téléphone identique à un client existant
- L'utilisateur peut confirmer la création ou sélectionner le client existant

---

## 9. Interfaces Utilisateur

### 9.1 Plateformes cibles
- Application mobile : iOS & Android
- Application web : navigateur moderne
- Mode hors-ligne avec synchronisation différée

### 9.2 Écrans principaux
| Écran | Contenu |
|---|---|
| Accueil / Dashboard | KPIs trimestriels, liste commandes, alertes actives |
| Liste commandes | Tableau filtrable + recherche + export |
| Fiche commande | Détail commande, timeline statuts, photos, paiements |
| Nouvelle commande | Formulaire de création en 3 étapes |
| Fiche client | Infos + mensurations + historique commandes |
| Notifications | Centre de notifications In-App |
| Administration | Paramétrage alertes, délais, utilisateurs |
| Rapports | Dashboard trimestriel avec graphiques exportables |

---

## 10. Exigences Non-Fonctionnelles

### 10.1 Performance
- Chargement du dashboard < 2 secondes
- Recherche client < 500ms
- Synchronisation hors-ligne < 5 secondes au retour du réseau

### 10.2 Sécurité
- Authentification par identifiant + mot de passe, option OTP SMS
- Rôles et permissions (RBAC)
- Chiffrement en transit (HTTPS/TLS) et au repos
- Sauvegarde automatique quotidienne des données

### 10.3 Disponibilité
- SLA cible : 99.5% (hors maintenance programmée)
- Fonctionnement offline complet avec sync différée

### 10.4 Internationalisation
- Langue principale : Français (fr-DZ)
- Devise : Dinar Algérien (DZD)
- Format de date : JJ/MM/AAAA
- Futur : support Arabe (ar-DZ)

### 10.5 Stack technique recommandée (indicatif)
| Couche | Technologie recommandée |
|---|---|
| Backend API | ASP.NET Core Web API |
| Base de données | PostgreSQL ou SQL Server |
| ORM | Entity Framework Core |
| Frontend web | Blazor Server / React |
| Mobile | Flutter (iOS + Android) |
| Notifications SMS | Twilio / Vonage / passerelle locale |
| Hébergement | Azure App Service / VPS |
| CI/CD | GitHub Actions / Azure DevOps |

### 10.6 Contraintes de déploiement
- Support du déploiement on-premise (atelier sans accès cloud fiable)
- Évolution future : mode multi-tenant (plusieurs ateliers)

---

## Annexes — Matrice Features / Rôles

| Feature | MANAGER | TAILOR | EMBROIDERER | BEADER | CASHIER |
|---------|---------|--------|-------------|--------|---------|
| Créer une commande | ✅ | ✅ | ❌ | ❌ | ❌ |
| Voir toutes les commandes | ✅ | ❌ | ❌ | ❌ | ✅ (vue limitée) |
| Voir ses commandes assignées | ✅ | ✅ | ✅ | ✅ | ❌ |
| Changer le statut | ✅ | ✅ (limité) | ✅ (limité) | ✅ (limité) | ❌ |
| Valider livraison (LIVRÉE) | ✅ | ❌ | ❌ | ❌ | ✅ |
| Voir dashboard trimestriel | ✅ | ❌ | ❌ | ❌ | ✅ (finance) |
| Paramétrer les notifications | ✅ | ❌ | ❌ | ❌ | ❌ |
| Gérer les clients | ✅ | ✅ (lecture + création) | ❌ | ❌ | ✅ |
| Gérer les utilisateurs | ✅ | ❌ | ❌ | ❌ | ❌ |
| Encaisser un paiement | ✅ | ❌ | ❌ | ❌ | ✅ |
| Générer reçus / factures | ✅ | ❌ | ❌ | ❌ | ✅ |
| Exporter rapports | ✅ | ❌ | ❌ | ❌ | ✅ (finance) |
| Paramétrage application | ✅ | ❌ | ❌ | ❌ | ❌ |

---

*Document généré le 23 Mars 2026 — Atelier Couture App v1.0*  
*Référence : SPEC-FUNC-COUTURE-v1.0*
