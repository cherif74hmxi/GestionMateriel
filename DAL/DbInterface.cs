using System.Globalization;
using GestionMateriel.Models;
using Microsoft.Data.Sqlite;

namespace GestionMateriel.DAL;

public static class DbInterface
{
    private const string SqlDateFormat = "yyyy-MM-dd HH:mm:ss";

    public static bool VerifierConnexion(string login, string motDePasse, out Najeur? utilisateur)
    {
        utilisateur = null;

        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(motDePasse))
        {
            return false;
        }

        using var connection = Connection.GetInstance().GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT IdNajeur, Prenom, Nom, DateNaissance, Telephone, Email, Login, MotDePasse, EstResponsable
FROM Najeur
WHERE lower(Login) = lower($login)
  AND MotDePasse = $motDePasse
  AND EstResponsable = 1
LIMIT 1;";
        command.Parameters.AddWithValue("$login", login.Trim());
        command.Parameters.AddWithValue("$motDePasse", motDePasse);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return false;
        }

        utilisateur = MapNajeur(reader);
        return true;
    }

    public static List<TypeMateriel> GetTypesMateriel()
    {
        var types = new List<TypeMateriel>();

        using var connection = Connection.GetInstance().GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT IdTypeMateriel, LibelleTypeMateriel
FROM TypeMateriel
ORDER BY LibelleTypeMateriel;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            types.Add(new TypeMateriel
            {
                IdTypeMateriel = reader.GetInt32(0),
                LibelleTypeMateriel = reader.GetString(1)
            });
        }

        return types;
    }

    public static List<Materiel> GetMateriels(bool seulementDisponibles = false)
    {
        var materiels = new List<Materiel>();

        using var connection = Connection.GetInstance().GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT m.IdMateriel,
       m.Code,
       m.Marque,
       m.IdTypeMateriel,
       t.LibelleTypeMateriel,
       m.Taille,
       m.Pointure,
       m.EstEnPret
FROM Materiel m
INNER JOIN TypeMateriel t ON t.IdTypeMateriel = m.IdTypeMateriel
WHERE ($seulementDisponibles = 0 OR m.EstEnPret = 0)
ORDER BY m.Code;";
        command.Parameters.AddWithValue("$seulementDisponibles", seulementDisponibles ? 1 : 0);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            materiels.Add(MapMateriel(reader));
        }

        return materiels;
    }

    public static bool AjouterMateriel(Materiel materiel, out string message)
    {
        if (string.IsNullOrWhiteSpace(materiel.Code))
        {
            message = "Le code materiel est obligatoire.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(materiel.Marque))
        {
            message = "La marque est obligatoire.";
            return false;
        }

        if (materiel.IdTypeMateriel <= 0)
        {
            message = "Le type de materiel est obligatoire.";
            return false;
        }

        try
        {
            using var connection = Connection.GetInstance().GetConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO Materiel (Code, Marque, IdTypeMateriel, Taille, Pointure, EstEnPret)
VALUES ($code, $marque, $idType, $taille, $pointure, 0);";
            command.Parameters.AddWithValue("$code", materiel.Code.Trim());
            command.Parameters.AddWithValue("$marque", materiel.Marque.Trim());
            command.Parameters.AddWithValue("$idType", materiel.IdTypeMateriel);
            command.Parameters.AddWithValue("$taille", (object?)materiel.Taille?.Trim() ?? DBNull.Value);
            command.Parameters.AddWithValue("$pointure", (object?)materiel.Pointure ?? DBNull.Value);

            command.ExecuteNonQuery();
            message = "Materiel ajoute avec succes.";
            return true;
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            message = "Le code materiel existe deja.";
            return false;
        }
        catch (Exception ex)
        {
            message = "Erreur lors de l'ajout du materiel : " + ex.Message;
            return false;
        }
    }

    public static List<Najeur> GetNajeurs()
    {
        var najeurs = new List<Najeur>();

        using var connection = Connection.GetInstance().GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT IdNajeur, Prenom, Nom, DateNaissance, Telephone, Email, Login, MotDePasse, EstResponsable
FROM Najeur
ORDER BY Nom, Prenom;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            najeurs.Add(MapNajeur(reader));
        }

        return najeurs;
    }

    public static List<Materiel> GetMaterielsDisponibles()
    {
        return GetMateriels(true);
    }

    public static List<Pret> GetPretsEnCours()
    {
        return GetPretsInternes(true, null);
    }

    public static List<Pret> GetHistoriquePrets(string? codeMateriel)
    {
        return GetPretsInternes(false, codeMateriel);
    }

    public static bool CreerPret(
        int idMateriel,
        int idNajeur,
        DateTime datePret,
        DateTime? dateRetourPrevue,
        out string message)
    {
        if (dateRetourPrevue.HasValue && dateRetourPrevue.Value.Date < datePret.Date)
        {
            message = "La date de retour prevue ne peut pas etre avant la date de pret.";
            return false;
        }

        using var connection = Connection.GetInstance().GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            using (var checkMateriel = connection.CreateCommand())
            {
                checkMateriel.Transaction = transaction;
                checkMateriel.CommandText = @"
SELECT EstEnPret
FROM Materiel
WHERE IdMateriel = $idMateriel;";
                checkMateriel.Parameters.AddWithValue("$idMateriel", idMateriel);

                var materielState = checkMateriel.ExecuteScalar();
                if (materielState == null)
                {
                    message = "Materiel introuvable.";
                    transaction.Rollback();
                    return false;
                }

                var estEnPret = Convert.ToInt32(materielState) == 1;
                if (estEnPret)
                {
                    message = "Ce materiel est deja prete.";
                    transaction.Rollback();
                    return false;
                }
            }

            using (var checkNajeur = connection.CreateCommand())
            {
                checkNajeur.Transaction = transaction;
                checkNajeur.CommandText = @"
SELECT 1
FROM Najeur
WHERE IdNajeur = $idNajeur;";
                checkNajeur.Parameters.AddWithValue("$idNajeur", idNajeur);

                if (checkNajeur.ExecuteScalar() == null)
                {
                    message = "Adherent introuvable.";
                    transaction.Rollback();
                    return false;
                }
            }

            using (var insertPret = connection.CreateCommand())
            {
                insertPret.Transaction = transaction;
                insertPret.CommandText = @"
INSERT INTO Pret (IdMateriel, IdNajeur, DatePret, DateRetourPrevue, DateRetourEffective)
VALUES ($idMateriel, $idNajeur, $datePret, $dateRetourPrevue, NULL);";
                insertPret.Parameters.AddWithValue("$idMateriel", idMateriel);
                insertPret.Parameters.AddWithValue("$idNajeur", idNajeur);
                insertPret.Parameters.AddWithValue("$datePret", ToSqlDate(datePret));
                insertPret.Parameters.AddWithValue(
                    "$dateRetourPrevue",
                    dateRetourPrevue.HasValue ? ToSqlDate(dateRetourPrevue.Value) : DBNull.Value);
                insertPret.ExecuteNonQuery();
            }

            using (var updateMateriel = connection.CreateCommand())
            {
                updateMateriel.Transaction = transaction;
                updateMateriel.CommandText = @"
UPDATE Materiel
SET EstEnPret = 1
WHERE IdMateriel = $idMateriel;";
                updateMateriel.Parameters.AddWithValue("$idMateriel", idMateriel);
                updateMateriel.ExecuteNonQuery();
            }

            transaction.Commit();
            message = "Pret enregistre avec succes.";
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            message = "Erreur lors de la creation du pret : " + ex.Message;
            return false;
        }
    }

    public static bool RestituerPret(int idPret, DateTime dateRetour, out string message)
    {
        using var connection = Connection.GetInstance().GetConnection();
        using var transaction = connection.BeginTransaction();

        try
        {
            int idMateriel;
            DateTime datePret;

            using (var readPret = connection.CreateCommand())
            {
                readPret.Transaction = transaction;
                readPret.CommandText = @"
SELECT IdMateriel, DatePret, DateRetourEffective
FROM Pret
WHERE IdPret = $idPret;";
                readPret.Parameters.AddWithValue("$idPret", idPret);

                using var reader = readPret.ExecuteReader();
                if (!reader.Read())
                {
                    message = "Pret introuvable.";
                    transaction.Rollback();
                    return false;
                }

                if (!reader.IsDBNull(2))
                {
                    message = "Ce pret est deja restitue.";
                    transaction.Rollback();
                    return false;
                }

                idMateriel = reader.GetInt32(0);
                datePret = ParseSqlDate(reader.GetString(1));
            }

            if (dateRetour.Date < datePret.Date)
            {
                message = "La date de retour ne peut pas etre avant la date de pret.";
                transaction.Rollback();
                return false;
            }

            using (var updatePret = connection.CreateCommand())
            {
                updatePret.Transaction = transaction;
                updatePret.CommandText = @"
UPDATE Pret
SET DateRetourEffective = $dateRetour
WHERE IdPret = $idPret
  AND DateRetourEffective IS NULL;";
                updatePret.Parameters.AddWithValue("$dateRetour", ToSqlDate(dateRetour));
                updatePret.Parameters.AddWithValue("$idPret", idPret);

                if (updatePret.ExecuteNonQuery() != 1)
                {
                    message = "Impossible de mettre a jour le pret.";
                    transaction.Rollback();
                    return false;
                }
            }

            using (var updateMateriel = connection.CreateCommand())
            {
                updateMateriel.Transaction = transaction;
                updateMateriel.CommandText = @"
UPDATE Materiel
SET EstEnPret = 0
WHERE IdMateriel = $idMateriel;";
                updateMateriel.Parameters.AddWithValue("$idMateriel", idMateriel);
                updateMateriel.ExecuteNonQuery();
            }

            transaction.Commit();
            message = "Restitution enregistree avec succes.";
            return true;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            message = "Erreur lors de la restitution : " + ex.Message;
            return false;
        }
    }

    private static List<Pret> GetPretsInternes(bool uniquementEnCours, string? codeMateriel)
    {
        var prets = new List<Pret>();

        using var connection = Connection.GetInstance().GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT p.IdPret,
       p.IdMateriel,
       p.IdNajeur,
       m.Code,
       n.Prenom || ' ' || n.Nom AS NomNajeur,
       p.DatePret,
       p.DateRetourPrevue,
       p.DateRetourEffective
FROM Pret p
INNER JOIN Materiel m ON m.IdMateriel = p.IdMateriel
INNER JOIN Najeur n ON n.IdNajeur = p.IdNajeur
WHERE ($uniquementEnCours = 0 OR p.DateRetourEffective IS NULL)
  AND ($codeMateriel IS NULL OR m.Code LIKE '%' || $codeMateriel || '%')
ORDER BY p.DatePret DESC;";
        command.Parameters.AddWithValue("$uniquementEnCours", uniquementEnCours ? 1 : 0);
        command.Parameters.AddWithValue(
            "$codeMateriel",
            string.IsNullOrWhiteSpace(codeMateriel) ? DBNull.Value : codeMateriel.Trim());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            prets.Add(new Pret
            {
                IdPret = reader.GetInt32(0),
                IdMateriel = reader.GetInt32(1),
                IdNajeur = reader.GetInt32(2),
                CodeMateriel = reader.GetString(3),
                NomNajeur = reader.GetString(4),
                DatePret = ParseSqlDate(reader.GetString(5)),
                DateRetourPrevue = reader.IsDBNull(6) ? null : ParseSqlDate(reader.GetString(6)),
                DateRetourEffective = reader.IsDBNull(7) ? null : ParseSqlDate(reader.GetString(7))
            });
        }

        return prets;
    }

    private static Najeur MapNajeur(SqliteDataReader reader)
    {
        return new Najeur
        {
            IdNajeur = reader.GetInt32(0),
            Prenom = reader.GetString(1),
            Nom = reader.GetString(2),
            DateNaissance = reader.IsDBNull(3) ? null : ParseSqlDate(reader.GetString(3)),
            Telephone = reader.IsDBNull(4) ? null : reader.GetString(4),
            Email = reader.IsDBNull(5) ? null : reader.GetString(5),
            Login = reader.IsDBNull(6) ? null : reader.GetString(6),
            MotDePasse = reader.IsDBNull(7) ? null : reader.GetString(7),
            EstResponsable = !reader.IsDBNull(8) && reader.GetInt32(8) == 1
        };
    }

    private static Materiel MapMateriel(SqliteDataReader reader)
    {
        return new Materiel
        {
            IdMateriel = reader.GetInt32(0),
            Code = reader.GetString(1),
            Marque = reader.GetString(2),
            IdTypeMateriel = reader.GetInt32(3),
            TypeMaterielLibelle = reader.GetString(4),
            Taille = reader.IsDBNull(5) ? null : reader.GetString(5),
            Pointure = reader.IsDBNull(6) ? null : reader.GetInt32(6),
            EstEnPret = !reader.IsDBNull(7) && reader.GetInt32(7) == 1
        };
    }

    private static string ToSqlDate(DateTime date)
    {
        return date.ToString(SqlDateFormat, CultureInfo.InvariantCulture);
    }

    private static DateTime ParseSqlDate(string value)
    {
        if (DateTime.TryParseExact(
                value,
                SqlDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedExact))
        {
            return parsedExact;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed;
        }

        return DateTime.MinValue;
    }
}
