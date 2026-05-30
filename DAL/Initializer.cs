using Microsoft.Data.Sqlite;

namespace GestionMateriel.DAL;

public static class Initializer
{
    public static void InitializeDatabase()
    {
        using var connection = Connection.GetInstance().GetConnection();
        EnableForeignKeys(connection);
        CreateSchema(connection);
        SeedTypeMateriel(connection);
        SeedNajeurs(connection);
        SeedMateriels(connection);
    }

    private static void EnableForeignKeys(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        command.ExecuteNonQuery();
    }

    private static void CreateSchema(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS TypeMateriel (
    IdTypeMateriel INTEGER PRIMARY KEY AUTOINCREMENT,
    LibelleTypeMateriel TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS Najeur (
    IdNajeur INTEGER PRIMARY KEY AUTOINCREMENT,
    Prenom TEXT NOT NULL,
    Nom TEXT NOT NULL,
    DateNaissance TEXT NULL,
    Telephone TEXT NULL,
    Email TEXT NULL UNIQUE,
    Login TEXT NULL UNIQUE,
    MotDePasse TEXT NULL,
    EstResponsable INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Materiel (
    IdMateriel INTEGER PRIMARY KEY AUTOINCREMENT,
    Code TEXT NOT NULL UNIQUE,
    Marque TEXT NOT NULL,
    IdTypeMateriel INTEGER NOT NULL,
    Taille TEXT NULL,
    Pointure INTEGER NULL,
    EstEnPret INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (IdTypeMateriel) REFERENCES TypeMateriel(IdTypeMateriel)
);

CREATE TABLE IF NOT EXISTS Pret (
    IdPret INTEGER PRIMARY KEY AUTOINCREMENT,
    IdMateriel INTEGER NOT NULL,
    IdNajeur INTEGER NOT NULL,
    DatePret TEXT NOT NULL,
    DateRetourPrevue TEXT NULL,
    DateRetourEffective TEXT NULL,
    FOREIGN KEY (IdMateriel) REFERENCES Materiel(IdMateriel),
    FOREIGN KEY (IdNajeur) REFERENCES Najeur(IdNajeur)
);

CREATE UNIQUE INDEX IF NOT EXISTS UX_Pret_Materiel_EnCours
ON Pret (IdMateriel)
WHERE DateRetourEffective IS NULL;
";
        command.ExecuteNonQuery();
    }

    private static void SeedTypeMateriel(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT OR IGNORE INTO TypeMateriel (LibelleTypeMateriel) VALUES
('Monopalme'),
('Tuba frontal'),
('Lunettes'),
('Combinaison');
";
        command.ExecuteNonQuery();
    }

    private static void SeedNajeurs(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT OR IGNORE INTO Najeur
(Prenom, Nom, DateNaissance, Telephone, Email, Login, MotDePasse, EstResponsable)
VALUES
('Admin', 'Materiel', '1988-01-01', '0600000001', 'admin@lyonpalme.local', 'admin', 'admin123', 1),
('Lina', 'Martin', '2001-04-13', '0600000002', 'lina.martin@lyonpalme.local', 'lina', 'demo', 0),
('Yanis', 'Roux', '1999-11-30', '0600000003', 'yanis.roux@lyonpalme.local', 'yanis', 'demo', 0),
('Ines', 'Bernard', '2003-07-18', '0600000004', 'ines.bernard@lyonpalme.local', 'ines', 'demo', 0),
('Nora', 'Bailly', '2002-02-21', '0600000005', 'nora.bailly@lyonpalme.local', 'nora', 'demo', 0);
";
        command.ExecuteNonQuery();
    }

    private static void SeedMateriels(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT OR IGNORE INTO Materiel (Code, Marque, IdTypeMateriel, Taille, Pointure, EstEnPret)
VALUES
('MONO-001', 'DMC', (SELECT IdTypeMateriel FROM TypeMateriel WHERE LibelleTypeMateriel = 'Monopalme'), NULL, 42, 0),
('MONO-002', 'Leaderfins', (SELECT IdTypeMateriel FROM TypeMateriel WHERE LibelleTypeMateriel = 'Monopalme'), NULL, 40, 0),
('TUBA-001', 'Arena', (SELECT IdTypeMateriel FROM TypeMateriel WHERE LibelleTypeMateriel = 'Tuba frontal'), NULL, NULL, 0),
('LUN-001', 'Speedo', (SELECT IdTypeMateriel FROM TypeMateriel WHERE LibelleTypeMateriel = 'Lunettes'), NULL, NULL, 0),
('COMB-001', 'Mako', (SELECT IdTypeMateriel FROM TypeMateriel WHERE LibelleTypeMateriel = 'Combinaison'), 'M', NULL, 0),
('COMB-002', 'Orca', (SELECT IdTypeMateriel FROM TypeMateriel WHERE LibelleTypeMateriel = 'Combinaison'), 'L', NULL, 0);
";
        command.ExecuteNonQuery();
    }
}
