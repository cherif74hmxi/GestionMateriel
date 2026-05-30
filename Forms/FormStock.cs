using GestionMateriel.DAL;

namespace GestionMateriel.Forms;

public class FormStock : Form
{
    private readonly DataGridView _gridStock;
    private readonly CheckBox _chkDisponibles;

    public FormStock()
    {
        FormUiHelper.ConfigureForm(this, "Stock materiel", 980, 560);

        Controls.Add(FormUiHelper.CreateTitle("Stock du club", 20, 18));

        _chkDisponibles = new CheckBox
        {
            Left = 20,
            Top = 62,
            Width = 240,
            Text = "Afficher seulement le stock disponible"
        };
        _chkDisponibles.CheckedChanged += (_, _) => ChargerStock();
        Controls.Add(_chkDisponibles);

        var btnActualiser = FormUiHelper.CreateButton("Actualiser", 280, 54, 120, 34);
        btnActualiser.Click += (_, _) => ChargerStock();
        Controls.Add(btnActualiser);

        var btnFermer = FormUiHelper.CreateButton("Fermer", 410, 54, 120, 34, true);
        btnFermer.Click += (_, _) => Close();
        Controls.Add(btnFermer);

        _gridStock = new DataGridView
        {
            Left = 20,
            Top = 100,
            Width = 940,
            Height = 430
        };
        FormUiHelper.StyleGrid(_gridStock);
        Controls.Add(_gridStock);

        Shown += (_, _) => ChargerStock();
    }

    private void ChargerStock()
    {
        var data = DbInterface
            .GetMateriels(_chkDisponibles.Checked)
            .Select(m => new
            {
                m.IdMateriel,
                m.Code,
                m.Marque,
                Type = m.TypeMaterielLibelle,
                m.Taille,
                m.Pointure,
                Statut = m.EstEnPret ? "Prete" : "Disponible"
            })
            .ToList();

        _gridStock.DataSource = data;
    }
}
