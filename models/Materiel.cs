namespace GestionMateriel.Models;

public class Materiel
{
    public int IdMateriel { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Marque { get; set; } = string.Empty;

    public int IdTypeMateriel { get; set; }

    public string TypeMaterielLibelle { get; set; } = string.Empty;

    public string? Taille { get; set; }

    public int? Pointure { get; set; }

    public bool EstEnPret { get; set; }

    public bool EstEnStock
    {
        get { return !EstEnPret; }
    }
}
