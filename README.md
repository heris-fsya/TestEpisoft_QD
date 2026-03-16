# Mini moteur de rapprochement bancaire / comptable

## Présentation du projet

Ce projet consiste à développer un **mini moteur de rapprochement entre un flux bancaire et un flux comptable** à partir de deux fichiers CSV.

Le programme lit deux sources de données :

* **bank.csv** : flux des transactions bancaires
* **accounting.csv** : flux des transactions comptables

L’objectif est d’identifier les transactions qui correspondent entre ces deux flux en appliquant plusieurs **règles de rapprochement** basées principalement sur :

* la **date**
* le **montant**

Chaque correspondance reçoit un **score** qui indique le niveau de confiance du rapprochement.

À la fin de l'exécution, le programme génère deux fichiers :

* **matches.csv** : liste des rapprochements trouvés
* **report.txt** : rapport contenant des statistiques sur le rapprochement (nombre de transactions, correspondances trouvées, cas ambigus, etc.)

Le moteur fonctionne en générant toutes les correspondances possibles, puis en sélectionnant les meilleures en fonction du score et de différents critères de tri.
Les règles de tolérance utilisées pour déterminer les correspondances peuvent être paramétrées via un fichier de configuration, ce qui permet d’adapter le comportement du moteur sans modifier le code.

---

# Prérequis

Pour exécuter ce projet il faut :

* **.NET Framework 4.6 ou supérieur**
* un environnement de développement compatible C# (Visual Studio recommandé)

Les fichiers de données doivent être placés dans le dossier :

```
Data/
rules.config
```

avec les deux fichiers suivants :

```
Data/bank.csv
Data/accounting.csv
```

---

# Comment exécuter le programme

Le programme peut être exécuté sans environnement de développement une fois l’exécutable généré.

1. Aller dans le dossier : bin/Debug/
2. Lancer le fichier : TestEpisoft_QD.exe

Le programme va :

1. lire les transactions bancaires et comptables
2. appliquer les règles de rapprochement
3. afficher les correspondances dans la console
4. générer les fichiers de sortie.

Les fichiers produits sont :

```
matches.csv
report.txt
```

---

# Règles de rapprochement

Le moteur utilise plusieurs règles simples basées sur la date et le montant.

| Règle                                    | Condition                       | Score |
| ---------------------------------------- | ------------------------------- | ----- |
| Correspondance exacte                    | même date et même montant       | 100   |
| Montant identique avec tolérance de date | montant identique, date ±1 jour | 85    |
| Date identique avec tolérance de montant | même date, montant ±5           | 70    |
| Tolérance date et montant                | date ±2 jours et montant ±5     | 55    |

Les seuils de tolérance utilisés par ces règles sont configurables via le fichier rules.config.
Cela permet d’ajuster facilement le comportement du moteur sans avoir à modifier ou recompiler le code.
Lorsqu'il existe **plusieurs correspondances possibles avec exactement les mêmes critères**, le cas est marqué comme **ambigu**.

Dans ce cas, le moteur conserve la liste des transactions comptables candidates afin de pouvoir les analyser dans le rapport.

---

# Les hypothèses & choix

Le temps estimé pour réaliser cet exercice étant d’environ **6 heures**, certains choix ont été faits pour garder une solution simple et compréhensible :

* la comparaison repose uniquement sur **la date et le montant**
* la **description de la transaction n'est pas utilisée dans le calcul du score**
* une transaction comptable ne peut être associée **qu'à une seule transaction bancaire**
* le moteur génère toutes les correspondances possibles avant de sélectionner les meilleures

La logique du moteur a également été découpée en plusieurs méthodes afin de rendre le code **plus lisible et plus facilement testable**.

---

# Tests unitaires

Des **tests unitaires** ont été ajoutés afin de vérifier les principales parties du programme.

Les tests couvrent notamment :

### Le moteur de rapprochement

* vérification des règles de score
* génération des correspondances possibles
* tri des correspondances
* détection des cas ambigus
* fonctionnement global du moteur

### La lecture des fichiers CSV

Deux scénarios principaux sont testés :

1. **Parsing nominal**

   Vérifie qu’un fichier CSV valide est correctement lu :

   * nombre de lignes correct
   * valeurs correctement interprétées (date, description, montant)

2. **Parsing avec erreur**

   Vérifie qu'une ligne invalide ne provoque pas de crash du programme (par exemple :

   * date invalide
   * montant invalide
   * colonnes manquantes)

Ces tests permettent de sécuriser les parties principales du projet.

---

# Dans un contexte où cet outil serait utilisé en production

Dans un contexte réel, plusieurs améliorations seraient nécessaires :

* ajouter un **système de journalisation (logs)**
* améliorer la **gestion des erreurs de lecture des fichiers**
* améliorer la **gestion et la validation des paramètres de configuration**
* améliorer la gestion des **cas ambigus**
* ajouter davantage de **tests automatisés**

Cela permettrait de rendre l’outil plus robuste et plus facile à maintenir.

---

# Les limites

Certaines fonctionnalités ne sont pas gérées ou seulement de manière simplifiée :

* la comparaison des **descriptions de transactions**
* les **transactions fractionnées ou regroupées**
* les différences de **formats bancaires**
* la gestion de **très gros volumes de transactions**
* l’analyse avancée des cas ambigus

Ces limitations sont principalement liées au temps limité pour la réalisation de l'exercice.

---

# Les améliorations possibles (par ordre de priorité)

### 1. Score progressif basé sur l’écart des données

Actuellement, le score fonctionne par **paliers fixes** selon la règle appliquée.

Une amélioration importante serait de remplacer ce système par un **score progressif calculé automatiquement**.

Le score pourrait être calculé à partir :

* de l’écart de **jours entre les dates**
* de l’écart de **montant**

Par exemple avec une formule mathématique où :

* plus l'écart de date est grand → plus le score diminue
* plus l'écart de montant est important → plus le score diminue

Cela permettrait d’obtenir un moteur de rapprochement **plus précis et plus flexible**.

---

### 2. Analyse des descriptions de transactions

Comparer les descriptions pour détecter des correspondances même si les libellés sont légèrement différents
(ex : *AMAZON PAYMENT* vs *CARD AMAZON MARKETPLACE*).

---

### 3. Amélioration des performances

Optimiser l’algorithme pour traiter des volumes de transactions beaucoup plus importants.

---

### 4. Outil d’analyse des résultats

Ajouter une interface ou un outil permettant d’analyser plus facilement les correspondances et les cas ambigus.

---

# Ce que je testerais en plus et pourquoi

Pour un projet utilisé en production, j’ajouterais notamment :

* des tests avec **de gros volumes de données**
* des tests avec **transactions très proches mais différentes**
* des tests avec **fichiers partiellement corrompus**
* des tests avec **transactions dupliquées**

Ces cas sont fréquents dans les données financières réelles.

---

# Comment lancer les tests

Les tests unitaires peuvent être exécutés via Visual Studio ou en ligne de commande :

```
dotnet test
```

Les tests ciblent principalement :

* le moteur de rapprochement
* la lecture et le parsing des fichiers CSV

---

# Remarque personnelle

Ce projet a été réalisé dans le cadre d’un **exercice technique estimé à environ 6 heures**.

C’est également l’un de mes premiers projets réalisés dans un contexte proche d’un environnement professionnel.
J’ai donc essayé de privilégier une approche **simple, lisible et structurée**, afin que le code reste facile à comprendre et à faire évoluer.
