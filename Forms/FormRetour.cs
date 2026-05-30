using GestionMateriel.DAL;

namespace GestionMateriel.Forms;

public class FormRetour : Form
{
    private readonly DataGridView _gridPrets;
    private readonly DateTimePicker _dtRetour;
    private readonly Label _lblSelection;

    private int _idPretSelectionne;

    public FormRetour()
    {
        FormUiHelper.ConfigureForm(this, "Restitution materiel", 920, 600);

        Controls.Add(FormUiHelper.CreateTitle("Restitution de materiel", 20, 16));

        _gridPrets = new DataGridView
        {
            Left = 20,
            Top = 62,
            Width = 880,
            Height = 390
        };
        FormUiHelper.StyleGrid(_gridPrets);
        _gridPrets.SelectionChanged += (_, _) => OnSelectionChanged();
        Controls.Add(_gridPrets);

        Controls.Add(new Label { Left = 20, Top = 470, Text = "Date de retour", AutoSize = true });
        _dtRetour = new DateTimePicker
        {
            Left = 20,
            Top = 492,
            Width = 180,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today
        };
        Controls.Add(_dtRetour);

        var btnRestituer = FormUiHelper.CreateButton("Valider la restitution", 220, 488, 220, 38);
        btnRestituer.Click += OnRestituerClick;
        Controls.Add(btnRestituer);

        var btnFermer = FormUiHelper.CreateButton("Fermer", 460, 488, 120, 38, true);
        btnFermer.Click += (_, _) => Close();
        Controls.Add(btnFermer);

        _lblSelection = new Label
        {
            Left = 20,
            Top = 538,
            Width = 860,
            Text = "Selectionnez un pret en cours.",
            ForeColor = FormUiHelper.Secondary
        };
        Controls.Add(_lblSelection);

        Shown += (_, _) => ChargerPrets();
    }

    private void ChargerPrets()
    {
        var data = DbInterface.GetPretsEnCours()
            .Select(p => new
            {
                p.IdPret,
                p.CodeMateriel,
                p.NomNajeur,
                DatePret = p.DatePret.ToString("dd/MM/yyyy"),
                DateRetourPrevue = p.DateRetourPrevue?.ToString("dd/MM/yyyy") ?? "-"
            })
            .ToList();

        _gridPrets.DataSource = data;
        _idPretSelectionne = 0;
        _lblSelection.Text = "Selectionnez un pret en cours.";
    }

    private void OnSelectionChanged()
    {
        if (_gridPrets.CurrentRow == null)
        {
            return;
        }

        var cell = _gridPrets.CurrentRow.Cells[nameof(Models.Pret.IdPret)].Value;
        if (cell == null)
        {
            return;
        }

        _idPretSelectionne = Convert.ToInt32(cell);
        _lblSelection.Text = "Pret selectionne : " + _idPretSelectionne;
    }

    private void OnRestituerClick(object? sender, EventArgs e)
    {
        if (_idPretSelectionne <= 0)
        {
            MessageBox.Show(
                "Selectionnez un pret a restituer.",
                "Restitution",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        if (DbInterface.RestituerPret(_idPretSelectionne, _dtRetour.Value.Date, out var message))
        {
            MessageBox.Show(message, "Restitution", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ChargerPrets();
            return;
        }

        MessageBox.Show(message, "Restitution", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
