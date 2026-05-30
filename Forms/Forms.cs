namespace GestionMateriel.Forms;

internal static class Forms
{
    public static void OpenDialog(Form owner, Form dialog)
    {
        dialog.StartPosition = FormStartPosition.CenterParent;
        dialog.ShowDialog(owner);
    }
}
