using GestionMateriel.DAL;
using GestionMateriel.Forms;

namespace GestionMateriel;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            Initializer.InitializeDatabase();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Impossible d'initialiser la base locale : " + ex.Message,
                "Erreur",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new FormLogin());
    }
}
