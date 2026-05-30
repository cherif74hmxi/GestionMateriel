namespace GestionMateriel.Models;

public class Pret
{
    public int IdPret { get; set; }

    public int IdMateriel { get; set; }

    public int IdNajeur { get; set; }

    public string CodeMateriel { get; set; } = string.Empty;

    public string NomNajeur { get; set; } = string.Empty;

    public DateTime DatePret { get; set; }

    public DateTime? DateRetourPrevue { get; set; }

    public DateTime? DateRetourEffective { get; set; }

    public bool EnCours
    {
        get { return DateRetourEffective == null; }
    }
}
