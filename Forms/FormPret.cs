using GestionMateriel.DAL;
using GestionMateriel.Models;

namespace GestionMateriel.Forms;

public class FormPret : Form
{
    private sealed class NajeurRow
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Email { get; set; }
    }

    private sealed class MaterielRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Marque { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Taille { get; set; }
        public int? Pointure { get; set; }
    }

    private readonly DataGridView _gridAdherents;
    private readonly DataGridView _gridMateriels;
    private readonly DataGridView _gridPrets;
    private readonly DateTimePicker _dtPret;
    private readonly DateTimePicker _dtRetourPrevu;
    private readonly CheckBox _chkRetourPrevu;
    private readonly Label _lblSelection;

    private int _idNajeurSelectionne;
    private int _idMaterielSelectionne;

    public FormPret()
    {
        FormUiHelper.ConfigureForm(this, "Pret materiel", 1020, 700);

        Controls.Add(FormUiHelper.CreateTitle("Nouveau pret", 20, 14));

        Controls.Add(new Label { Left = 20, Top = 56, Text = "Adherents", AutoSize = true });
        Controls.Add(new Label { Left = 510, Top = 56, Text = "Materiels disponibles", AutoSize = true });

        _gridAdherents = new DataGridView
        {
            Left = 20,
            Top = 78,
            Width = 470,
            Height = 220
        };
        FormUiHelper.StyleGrid(_gridAdherents);
        _gridAdherents.SelectionChanged += (_, _) => OnAdherentSelectionChanged();
        Controls.Add(_gridAdherents);

        _gridMateriels = new DataGridView
        {
            Left = 510,
            Top = 78,
            Width = 490,
            Height = 220
        };
        FormUiHelper.StyleGrid(_gridMateriels);
        _gridMateriels.SelectionChanged += (_, _) => OnMaterielSelectionChanged();
        Controls.Add(_gridMateriels);

        Controls.Add(new Label { Left = 20, Top = 317, Text = "Date de pret", AutoSize = true });
        _dtPret = new DateTimePicker
        {
            Left = 20,
            Top = 338,
            Width = 220,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today
        };
        Controls.Add(_dtPret);

        _dtRetourPrevu = new DateTimePicker
        {
            Left = 510,
            Top = 338,
            Width = 220,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today.AddDays(7),
            Enabled = false
        };
        Controls.Add(_dtRetourPrevu);

        _chkRetourPrevu = new CheckBox
        {
            Left = 270,
            Top = 340,
            Width = 220,
            Text = "Saisir une date de retour prevue"
        };
        _chkRetourPrevu.CheckedChanged += (_, _) => _dtRetourPrevu.Enabled = _chkRetourPrevu.Checked;
        Controls.Add(_chkRetourPrevu);

        var btnPret = FormUiHelper.CreateButton("Enregistrer le pret", 750, 334, 170, 38);
        btnPret.Click += OnPretClick;
        Controls.Add(btnPret);

        var btnFermer = FormUiHelper.CreateButton("Fermer", 930, 334, 70, 38, true);
        btnFermer.Click += (_, _) => Close();
        Controls.Add(btnFermer);

        _lblSelection = new Label
        {
            Left = 20,
            Top = 383,
            Width = 980,
            Height = 22,
            ForeColor = FormUiHelper.Secondary,
            Text = "Selectionnez un adherent et un materiel disponibles."
        };
        Controls.Add(_lblSelection);

        Controls.Add(new Label { Left = 20, Top = 420, Text = "Prets en cours", AutoSize = true });
        _gridPrets = new DataGridView
        {
            Left = 20,
            Top = 442,
            Width = 980,
            Height = 230
        };
        FormUiHelper.StyleGrid(_gridPrets);
        Controls.Add(_gridPrets);

        Shown += (_, _) => ChargerDonnees();
    }

    private void ChargerDonnees()
    {
        var adherents = DbInterface.GetNajeurs()
            .Select(a => new NajeurRow
            {
                Id = a.IdNajeur,
                Nom = a.Nom,
                Prenom = a.Prenom,
                Telephone = a.Telephone,
                Email = a.Email
            })
            .ToList();

        _gridAdherents.DataSource = adherents;

        var materiels = DbInterface.GetMaterielsDisponibles()
            .Select(m => new MaterielRow
            {
                Id = m.IdMateriel,
                Code = m.Code,
                Marque = m.Marque,
                Type = m.TypeMaterielLibelle,
                Taille = m.Taille,
                Pointure = m.Pointure
            })
            .ToList();

        _gridMateriels.DataSource = materiels;

        var prets = DbInterface.GetPretsEnCours()
            .Select(p => new
            {
                p.IdPret,
                p.CodeMateriel,
                p.NomNajeur,
                DatePret = p.DatePret.ToString("dd/MM/yyyy"),
                DateRetourPrevue = p.DateRetourPrevue?.ToString("dd/MM/yyyy") ?? "-"
            })
            .ToList();

        _gridPrets.DataSource = prets;

        _idNajeurSelectionne = 0;
        _idMaterielSelectionne = 0;
        _lblSelection.Text = "Selectionnez un adherent et un materiel disponibles.";
    }

    private void OnAdherentSelectionChanged()
    {
        if (_gridAdherents.CurrentRow?.DataBoundItem is not NajeurRow row)
        {
            return;
        }

        _idNajeurSelectionne = row.Id;
        RafraichirTexteSelection();
    }

    private void OnMaterielSelectionChanged()
    {
        if (_gridMateriels.CurrentRow?.DataBoundItem is not MaterielRow row)
        {
            return;
        }

        _idMaterielSelectionne = row.Id;
        RafraichirTexteSelection();
    }

    private void RafraichirTexteSelection()
    {
        _lblSelection.Text =
            "Adherent selectionne : " + (_idNajeurSelectionne == 0 ? "aucun" : _idNajeurSelectionne) +
            " | Materiel selectionne : " + (_idMaterielSelectionne == 0 ? "aucun" : _idMaterielSelectionne);
    }

    private void OnPretClick(object? sender, EventArgs e)
    {
        if (_idNajeurSelectionne == 0 || _idMaterielSelectionne == 0)
        {
            MessageBox.Show(
                "Selectionnez un adherent et un materiel avant de valider.",
                "Pret",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        DateTime? dateRetourPrevue = _chkRetourPrevu.Checked ? _dtRetourPrevu.Value.Date : null;

        if (DbInterface.CreerPret(
                _idMaterielSelectionne,
                _idNajeurSelectionne,
                _dtPret.Value.Date,
                dateRetourPrevue,
                out var message))
        {
            MessageBox.Show(message, "Pret", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ChargerDonnees();
            return;
        }

        MessageBox.Show(message, "Pret", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
