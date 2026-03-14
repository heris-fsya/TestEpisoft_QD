# TestEpisoft_QD
# Mini moteur de rapprochement bancaire / comptable

## Présentation du projet

Ce projet consiste à construire un **mini moteur de rapprochement** entre un flux bancaire et un flux comptable à partir de deux fichiers CSV.

L’objectif du programme est d’identifier les transactions qui correspondent entre :

* un fichier **bank.csv** représentant le flux bancaire
* un fichier **accounting.csv** représentant le flux comptable

Le moteur compare les transactions selon plusieurs critères simples (principalement **la date et le montant**) afin de trouver les correspondances les plus probables.

Chaque correspondance reçoit un **score** en fonction de la règle utilisée.

Le programme produit ensuite deux fichiers de sortie :

* **matches.csv** : la liste des correspondances trouvées
* **report.txt** : un rapport récapitulatif contenant différentes statistiques sur le rapprochement

Le but était de construire quelque chose de **fonctionnel, lisible et facilement testable**, plutôt qu’un moteur extrêmement complexe.

---

# Prérequis

Pour exécuter le projet il faut :

* **.NET Framework 4.6 ou supérieur**
* un environnement permettant de compiler du C# (Visual Studio, Rider ou VS Code)

Les fichiers de données doivent être placés dans le dossier :

```
Data/
```

avec :

```
Data/bank.csv
Data/accounting.csv
```

---

# Comment exécuter le programme

Depuis le dossier du projet :

```bash
dotnet run
```

Le programme va :

1. charger les transactions bancaires et comptables depuis les fichiers CSV
2. appliquer les règles de rapprochement
3. afficher les correspondances trouvées dans la console
4. générer deux fichiers dans le dossier du projet :

```
matches.csv
report.txt
```

---

# Règles de rapprochement

Les règles utilisées sont volontairement simples et basées sur la date et le montant.

| Règle                                    | Condition                       | Score |
| ---------------------------------------- | ------------------------------- | ----- |
| Correspondance exacte                    | même date et même montant       | 100   |
| Montant identique avec tolérance de date | montant identique, date ±1 jour | 85    |
| Date identique avec tolérance de montant | même date, montant ±5           | 70    |
| Tolérance date et montant                | date ±2 jours et montant ±5     | 55    |

Lorsqu'il existe **plusieurs correspondances possibles avec exactement les mêmes critères**, le cas est marqué comme **ambigu**.

Dans ce cas, la liste des transactions comptables candidates est conservée afin de pouvoir les analyser dans le rapport.

---

# Les hypothèses & choix

Le temps estimé pour cet exercice étant d’environ **6 heures**, certains choix ont été faits pour garder une solution simple et claire :

* la comparaison se base uniquement sur **la date et le montant**
* la **description n’est pas utilisée dans le calcul du score**
* une transaction comptable ne peut être utilisée **qu’une seule fois**
* le moteur génère **toutes les correspondances possibles**, puis sélectionne les meilleures selon le score et certains critères de tri

J’ai également découpé la logique en plusieurs méthodes afin de rendre le code **plus lisible et plus facile à tester**.

---

# Dans un contexte où cet outil serait utilisé en production

Si cet outil devait être utilisé dans un contexte réel, plusieurs éléments devraient être ajoutés :

* une **gestion d’erreurs plus robuste** lors de la lecture des fichiers
* un système de **journalisation (logs)**
* une **configuration des règles de rapprochement**
* des **tests unitaires plus complets**
* une gestion plus avancée des **cas ambigus**

Cela permettrait de rendre l’outil plus fiable et plus maintenable.

---

# Les limites

Certaines choses ne sont pas gérées actuellement ou seulement de manière simplifiée :

* la **similarité des descriptions** (par exemple AMAZON vs AMAZON MARKETPLACE)
* les **transactions fractionnées ou regroupées**
* les différences de **format ou de devise**
* la gestion de **volumes de données très importants**
* une analyse plus poussée des **cas ambigus**

Ces points pourraient être améliorés dans une version plus avancée du moteur.

---

# Les améliorations possibles (par ordre de priorité)

1. Ajouter une **comparaison des descriptions** pour améliorer la qualité des correspondances.
2. Rendre les **règles de rapprochement configurables** (par exemple via un fichier de configuration).
3. Améliorer la gestion et l’analyse des **cas ambigus**.
4. Optimiser l’algorithme pour gérer des **volumes de transactions plus importants**.
5. Ajouter une **interface en ligne de commande plus complète**.
6. Ajouter une **interface graphique ou un outil de visualisation** pour analyser les résultats.

---

# Ce que je testerais en plus et pourquoi

Pour un projet réel, je mettrais en place plusieurs types de tests :

### Tests des règles de rapprochement

Vérifier que chaque règle fonctionne correctement :

* correspondance exacte
* tolérance de date
* tolérance de montant

---

### Tests des cas ambigus

Vérifier que les situations où plusieurs transactions peuvent correspondre :

* sont correctement détectées
* apparaissent bien dans le rapport

---

### Tests des transactions non rapprochées

Vérifier que les statistiques du **report.txt** sont correctes :

* nombre total de transactions
* nombre de transactions rapprochées
* nombre de transactions restantes

---

### Tests de cas limites

Par exemple :

* plusieurs transactions identiques
* petites différences de montant (arrondis)
* transactions très proches dans le temps

Ces tests permettent de s’assurer que le moteur reste fiable dans des situations réalistes.

---

# Comment lancer les tests

Les tests unitaires peuvent être lancés avec :

```bash
dotnet test
```

Les tests ciblent principalement la classe **ReconciliationEngine**, qui contient la logique principale du moteur de rapprochement.

Le moteur a été structuré en plusieurs méthodes afin de faciliter l’écriture de tests unitaires.

---

# Remarque personnelle

C’est mon premier projet de ce type en dehors d’un cadre scolaire.
J’ai essayé de privilégier un code **simple, lisible et compréhensible**, afin qu’il soit facile à relire et à améliorer.

Mon objectif principal était de proposer une solution **fonctionnelle et structurée**, tout en restant dans le temps estimé pour l’exercice.
