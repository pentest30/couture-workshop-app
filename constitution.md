# Atelier Couture App Constitution

## Core Principles

### I. Domain-First & Traceability

The product digitalizes a real atelier workflow (devis → livraison). Business rules for commandes, statuts, types de travaux (Simple / Brodé / Perlé / Mixte), finances et notifications MUST live in explicit domain logic—not only in UI or ad hoc SQL.

- Command identity and lifecycle are auditable (historique des statuts, qui / quand / motif si requis).
- Financial integrity: acomptes, soldes, clôture `LIVRÉE` suivent les règles métier du CDC fonctionnel.
- Statut transitions are validated server-side; matrice autorisée (F02) is enforced in one place.

### II. Role-Based Access (NON-NEGOTIABLE)

Gérant, couturier(ère), brodeur/perleur, caissier ont des périmètres différents. Chaque action sensible (changement de statut, validation livraison avec solde, paramètres) MUST être contrôlée par rôle et traçable.

### III. Test-Driven Quality

- Automated tests for statut transitions, calculs financiers (solde, clôture), et règles par type de travail.
- Integration tests for flux critiques (création commande 3 étapes, transition vers BRODERIE/PERLAGE, dashboard trimestriel).
- Edge cases from the functional spec (retard, retouche avec motif, solde non nul à la livraison avec validation gérant) MUST have explicit tests.

### IV. Explicit Errors & UX Clarity

Expected failures (transition interdite, champs obligatoires, solde incompatible) MUST être renvoyés de façon structurée pour l’UI (messages clairs, pas d’exceptions silencieuses pour le métier).

### V. Simplicity & YAGNI (v1)

- Pas de microservices ni bus d’événements pour la v1 sauf justification documentée.
- Hors périmètre v1 (SPECS) : import Excel massif (M09) — ne pas implémenter tant que non re-priorisé.
- Notifications : privilégier une implémentation simple et fiable (ex. file + envoi ou in-app selon stack) avant les optimisations complexes.

## Technology Stack & Constraints

Stack technique : **à figer dans `specs/.../plan.md`** (pas imposé dans cette constitution). Toute décision (web stack, mobile, base de données) MUST rester compatible avec :

- Données relationnelles pour commandes, clients, paiements, historiques.
- Pagination et listes performantes (ex. 20 commandes par page, filtres F01.5).
- Génération PDF pour reçus / factures (F01.6, F09) — objectif perf raisonnable (cf. SPECS).

## Development Workflow & Quality Gates

- Branches de fonctionnalité alignées sur les dossiers `specs/NNN-nom-feature/`.
- Definition of Done : critères d’acceptation de `spec.md` couverts par tests ou vérification manuelle traçable ; pas de régression sur transitions et finances.
- Commits : messages clairs ; PR liée au numéro de feature / spec quand applicable.

## Governance

Cette constitution complète `SPECS_FONCTIONNELLES.md` pour les choix d’architecture et de processus. En cas de conflit sur le métier, **SPECS_FONCTIONNELLES.md** fait foi.

- Amendements : versionner ce fichier ; noter la date et le motif.
- Toute complexité ajoutée au-delà de ces principes MUST être justifiée (risque, contrainte légale, perf mesurée).

**Version**: 1.0.0 | **Ratified**: 2026-03-24 | **Last Amended**: 2026-03-24
