namespace GestionMateriel.Models;

public class Najeur
{
    public int IdNajeur { get; set; }

    public string Prenom { get; set; } = string.Empty;

    public string Nom { get; set; } = string.Empty;

    public DateTime? DateNaissance { get; set; }

    public string? Telephone { get; set; }

    public string? Email { get; set; }

    public string? Login { get; set; }

    public string? MotDePasse { get; set; }

    public bool EstResponsable { get; set; }

    public string NomComplet
    {
        get
        {
            return (Prenom + " " + Nom).Trim();
        }
    }
}
