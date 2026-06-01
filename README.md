# LyonPalme - Gestion Materiel

Application client lourd Windows Forms permettant au club LyonPalme de suivre son stock de materiel et les prets aux adherents.

Le projet initialise automatiquement une base SQLite locale au demarrage, avec des donnees de demonstration pour tester rapidement l'application.

## Fonctionnalites

- Connexion reservee aux responsables materiel.
- Consultation du stock complet ou uniquement du materiel disponible.
- Ajout de materiel avec code unique.
- Gestion des types de materiel : monopalme, tuba frontal, lunettes, combinaison.
- Creation d'un pret entre un adherent et un materiel disponible.
- Saisie d'une date de retour prevue.
- Restitution d'un pret en cours avec date de retour effective.
- Historique des prets avec filtre par code materiel.

## Use cases

Acteur principal : `Responsable materiel`.

Use cases principaux :

- Se connecter a l'application.
- Consulter le stock du club.
- Filtrer le stock disponible.
- Ajouter un nouveau materiel.
- Creer un pret pour un adherent.
- Restituer un materiel prete.
- Consulter l'historique des prets.
- Filtrer l'historique par code materiel.

```mermaid
flowchart LR
    Responsable[Responsable materiel]

    subgraph Systeme[Application Gestion Materiel]
        UC1([Se connecter])
        UC2([Consulter le stock])
        UC3([Filtrer le stock disponible])
        UC4([Ajouter du materiel])
        UC5([Creer un pret])
        UC6([Selectionner un adherent])
        UC7([Selectionner un materiel disponible])
        UC8([Saisir une date de retour prevue])
        UC9([Restituer du materiel])
        UC10([Consulter l'historique])
        UC11([Filtrer par code materiel])
    end

    Responsable --> UC1
    Responsable --> UC2
    Responsable --> UC4
    Responsable --> UC5
    Responsable --> UC9
    Responsable --> UC10

    UC2 -.->|include| UC3
    UC5 -.->|include| UC6
    UC5 -.->|include| UC7
    UC5 -.->|optionnel| UC8
    UC10 -.->|include| UC11
```

## Diagramme des classes

```mermaid
classDiagram
    class Najeur {
        +int IdNajeur
        +string Prenom
        +string Nom
        +DateTime DateNaissance
        +string Telephone
        +string Email
        +string Login
        +string MotDePasse
        +bool EstResponsable
        +string NomComplet
    }

    class Materiel {
        +int IdMateriel
        +string Code
        +string Marque
        +int IdTypeMateriel
        +string TypeMaterielLibelle
        +string Taille
        +int Pointure
        +bool EstEnPret
        +bool EstEnStock
    }

    class TypeMateriel {
        +int IdTypeMateriel
        +string LibelleTypeMateriel
        +ToString() string
    }

    class Pret {
        +int IdPret
        +int IdMateriel
        +int IdNajeur
        +string CodeMateriel
        +string NomNajeur
        +DateTime DatePret
        +DateTime DateRetourPrevue
        +DateTime DateRetourEffective
        +bool EnCours
    }

    class Connection {
        +string DbChemin
        +GetInstance() Connection
        +GetConnection() SqliteConnection
        +CloseConnection(connection) void
    }

    class Initializer {
        <<static>>
        +InitializeDatabase() void
    }

    class DbInterface {
        <<static>>
        +VerifierConnexion(login, motDePasse, utilisateur) bool
        +GetTypesMateriel() List~TypeMateriel~
        +GetMateriels(seulementDisponibles) List~Materiel~
        +AjouterMateriel(materiel, message) bool
        +GetNajeurs() List~Najeur~
        +GetMaterielsDisponibles() List~Materiel~
        +GetPretsEnCours() List~Pret~
        +GetHistoriquePrets(codeMateriel) List~Pret~
        +CreerPret(idMateriel, idNajeur, datePret, dateRetourPrevue, message) bool
        +RestituerPret(idPret, dateRetour, message) bool
    }

    class FormLogin
    class FormMain
    class FormStock
    class FormAjoutMateriel
    class FormPret
    class FormRetour
    class FormHistorique

    TypeMateriel "1" --> "0..*" Materiel : type
    Materiel "1" --> "0..*" Pret : concerne
    Najeur "1" --> "0..*" Pret : emprunte

    Initializer ..> Connection
    DbInterface ..> Connection
    DbInterface ..> Najeur
    DbInterface ..> Materiel
    DbInterface ..> TypeMateriel
    DbInterface ..> Pret

    FormLogin ..> DbInterface
    FormMain ..> FormStock
    FormMain ..> FormAjoutMateriel
    FormMain ..> FormPret
    FormMain ..> FormRetour
    FormMain ..> FormHistorique
    FormStock ..> DbInterface
    FormAjoutMateriel ..> DbInterface
    FormPret ..> DbInterface
    FormRetour ..> DbInterface
    FormHistorique ..> DbInterface
```

## Diagramme de sequence

Scenario : creation d'un pret de materiel.

```mermaid
sequenceDiagram
    actor Responsable
    participant Login as FormLogin
    participant Main as FormMain
    participant PretForm as FormPret
    participant Db as DbInterface
    participant Conn as Connection
    participant SQLite as Base SQLite

    Responsable->>Login: Saisit login et mot de passe
    Login->>Db: VerifierConnexion(login, motDePasse)
    Db->>Conn: GetConnection()
    Conn-->>Db: SqliteConnection
    Db->>SQLite: SELECT responsable
    SQLite-->>Db: Najeur responsable
    Db-->>Login: Connexion valide

    Login->>Main: Ouvre le tableau de bord
    Responsable->>Main: Clique sur Preter du materiel
    Main->>PretForm: Ouvre FormPret

    PretForm->>Db: GetNajeurs()
    Db->>SQLite: SELECT adherents
    SQLite-->>Db: Liste des adherents
    Db-->>PretForm: Adherents

    PretForm->>Db: GetMaterielsDisponibles()
    Db->>SQLite: SELECT materiels disponibles
    SQLite-->>Db: Liste des materiels
    Db-->>PretForm: Materiels disponibles

    Responsable->>PretForm: Selectionne adherent et materiel
    opt Date de retour prevue
        Responsable->>PretForm: Coche et saisit la date
    end

    Responsable->>PretForm: Valide le pret
    PretForm->>Db: CreerPret(idMateriel, idNajeur, datePret, dateRetourPrevue)
    Db->>Conn: GetConnection()
    Conn-->>Db: SqliteConnection
    Db->>SQLite: Verifie materiel disponible
    SQLite-->>Db: Materiel disponible
    Db->>SQLite: Verifie adherent
    SQLite-->>Db: Adherent existant
    Db->>SQLite: INSERT Pret
    Db->>SQLite: UPDATE Materiel EstEnPret = 1
    SQLite-->>Db: Transaction validee
    Db-->>PretForm: Pret enregistre avec succes
    PretForm-->>Responsable: Affiche le message de confirmation
```

## Technologies

- C#
- .NET `net10.0-windows`
- Windows Forms
- SQLite
- Package `Microsoft.Data.Sqlite`

## Prerequis

- Windows
- SDK .NET compatible avec `net10.0-windows`
- Visual Studio ou la CLI `dotnet`

## Installation

Cloner le projet :

```bash
git clone <url-du-repository>
cd LyonPalme-Gestion-Materiel
```

Restaurer les dependances :

```bash
dotnet restore
```

Compiler le projet :

```bash
dotnet build
```

Lancer l'application :

```bash
dotnet run --project GestionMateriel.csproj
```

## Connexion demo

Un compte responsable est cree automatiquement lors de l'initialisation de la base :

```text
Login : admin
Mot de passe : admin123
```

## Base de donnees

La base SQLite est creee automatiquement au premier lancement dans le dossier de sortie de l'application :

```text
GestionMateriel.db
```

Tables principales :

- `TypeMateriel`
- `Najeur`
- `Materiel`
- `Pret`

Des donnees de demonstration sont ajoutees avec `INSERT OR IGNORE`, ce qui permet de relancer l'application sans dupliquer les enregistrements.

## Structure du projet

```text
LyonPalme-Gestion-Materiel/
|-- DAL/
|   |-- Connection.cs
|   |-- DbInterface.cs
|   `-- Initializer.cs
|-- Forms/
|   |-- FormLogin.cs
|   |-- FormMain.cs
|   |-- FormStock.cs
|   |-- FormAjoutMateriel.cs
|   |-- FormPret.cs
|   |-- FormRetour.cs
|   `-- FormHistorique.cs
|-- models/
|   |-- Materiel.cs
|   |-- Najeur.cs
|   |-- Pret.cs
|   `-- TypeMateriel.cs
|-- GestionMateriel.csproj
|-- GestionMateriel-1.sln
`-- Program.cs
```

## Utilisation

1. Se connecter avec le compte responsable.
2. Consulter le stock depuis le tableau de bord.
3. Ajouter un nouveau materiel si besoin.
4. Creer un pret en selectionnant un adherent et un materiel disponible.
5. Restituer le materiel lorsqu'il est rendu.
6. Consulter l'historique pour suivre la tracabilite des prets.

## Notes

- Les mots de passe sont stockes en clair dans les donnees de demonstration.
- L'application est prevue pour un usage local.
- Le fichier SQLite est rattache au dossier de sortie, il peut donc etre recree apres un nettoyage du build.
