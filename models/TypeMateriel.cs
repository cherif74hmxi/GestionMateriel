namespace GestionMateriel.Models;

public class TypeMateriel
{
    public int IdTypeMateriel { get; set; }

    public string LibelleTypeMateriel { get; set; } = string.Empty;

    public override string ToString()
    {
        return LibelleTypeMateriel;
    }
}
