using GestionMateriel.DAL;

namespace GestionMateriel.Forms;

public class FormHistorique : Form
{
    private readonly DataGridView _gridHistorique;
    private readonly TextBox _txtCodeMateriel;

    public FormHistorique()
    {
        FormUiHelper.ConfigureForm(this, "Historique des prets", 980, 620);

        Controls.Add(FormUiHelper.CreateTitle("Tracabilite du materiel", 20, 16));

        Controls.Add(new Label
        {
            Left = 20,
            Top = 62,
            Text = "Filtrer par code materiel",
            AutoSize = true
        });

        _txtCodeMateriel = FormUiHelper.CreateTextBox(20, 84, 220);
        Controls.Add(_txtCodeMateriel);

        var btnFiltrer = FormUiHelper.CreateButton("Filtrer", 255, 80, 110, 34);
        btnFiltrer.Click += (_, _) => ChargerHistorique();
        Controls.Add(btnFiltrer);

        var btnReinitialiser = FormUiHelper.CreateButton("Reinitialiser", 375, 80, 130, 34, true);
        btnReinitialiser.Click += (_, _) =>
        {
            _txtCodeMateriel.Clear();
            ChargerHistorique();
        };
        Controls.Add(btnReinitialiser);

        var btnFermer = FormUiHelper.CreateButton("Fermer", 515, 80, 100, 34, true);
        btnFermer.Click += (_, _) => Close();
        Controls.Add(btnFermer);

        _gridHistorique = new DataGridView
        {
            Left = 20,
            Top = 130,
            Width = 940,
            Height = 470
        };
        FormUiHelper.StyleGrid(_gridHistorique);
        Controls.Add(_gridHistorique);

        Shown += (_, _) => ChargerHistorique();
    }

    private void ChargerHistorique()
    {
        var data = DbInterface.GetHistoriquePrets(_txtCodeMateriel.Text)
            .Select(p => new
            {
                p.IdPret,
                p.CodeMateriel,
                p.NomNajeur,
                DatePret = p.DatePret.ToString("dd/MM/yyyy"),
                DateRetourPrevue = p.DateRetourPrevue?.ToString("dd/MM/yyyy") ?? "-",
                DateRetourEffective = p.DateRetourEffective?.ToString("dd/MM/yyyy") ?? "-",
                Statut = p.EnCours ? "En cours" : "Rendu"
            })
            .ToList();

        _gridHistorique.DataSource = data;
    }
}
