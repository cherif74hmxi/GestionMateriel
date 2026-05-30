using GestionMateriel.DAL;
using GestionMateriel.Models;

namespace GestionMateriel.Forms;

public class FormAjoutMateriel : Form
{
    private readonly TextBox _txtCode;
    private readonly TextBox _txtMarque;
    private readonly ComboBox _cbType;
    private readonly TextBox _txtTaille;
    private readonly NumericUpDown _numPointure;
    private readonly Label _lblInfo;

    public FormAjoutMateriel()
    {
        FormUiHelper.ConfigureForm(this, "Ajout materiel", 700, 460);

        Controls.Add(FormUiHelper.CreateTitle("Ajouter un materiel", 20, 18));

        Controls.Add(new Label { Left = 20, Top = 72, Text = "Code materiel", AutoSize = true });
        _txtCode = FormUiHelper.CreateTextBox(20, 94, 300);
        Controls.Add(_txtCode);

        Controls.Add(new Label { Left = 350, Top = 72, Text = "Marque", AutoSize = true });
        _txtMarque = FormUiHelper.CreateTextBox(350, 94, 300);
        Controls.Add(_txtMarque);

        Controls.Add(new Label { Left = 20, Top = 140, Text = "Type", AutoSize = true });
        _cbType = new ComboBox
        {
            Left = 20,
            Top = 162,
            Width = 300
        };
        FormUiHelper.StyleComboBox(_cbType);
        _cbType.SelectedIndexChanged += (_, _) => AjusterChampsSpecifiques();
        Controls.Add(_cbType);

        Controls.Add(new Label { Left = 350, Top = 140, Text = "Taille (combinaison)", AutoSize = true });
        _txtTaille = FormUiHelper.CreateTextBox(350, 162, 300);
        Controls.Add(_txtTaille);

        Controls.Add(new Label { Left = 20, Top = 208, Text = "Pointure (monopalme)", AutoSize = true });
        _numPointure = new NumericUpDown
        {
            Left = 20,
            Top = 230,
            Width = 300,
            Minimum = 20,
            Maximum = 60,
            Value = 40
        };
        Controls.Add(_numPointure);

        var btnAjouter = FormUiHelper.CreateButton("Ajouter", 20, 292, 145, 40);
        btnAjouter.Click += OnAjouterClick;
        Controls.Add(btnAjouter);

        var btnFermer = FormUiHelper.CreateButton("Fermer", 180, 292, 145, 40, true);
        btnFermer.Click += (_, _) => Close();
        Controls.Add(btnFermer);

        _lblInfo = new Label
        {
            Left = 20,
            Top = 352,
            Width = 630,
            Height = 50,
            ForeColor = Color.Firebrick
        };
        Controls.Add(_lblInfo);

        Shown += (_, _) => ChargerTypes();
    }

    private void ChargerTypes()
    {
        var types = DbInterface.GetTypesMateriel();
        _cbType.DataSource = types;
        _cbType.DisplayMember = nameof(TypeMateriel.LibelleTypeMateriel);
        _cbType.ValueMember = nameof(TypeMateriel.IdTypeMateriel);
        AjusterChampsSpecifiques();
    }

    private void AjusterChampsSpecifiques()
    {
        var libelle = (_cbType.SelectedItem as TypeMateriel)?.LibelleTypeMateriel.ToLowerInvariant() ?? string.Empty;

        var isCombinaison = libelle.Contains("combinaison");
        var isMonopalme = libelle.Contains("monopalme");

        _txtTaille.Enabled = isCombinaison;
        if (!isCombinaison)
        {
            _txtTaille.Clear();
        }

        _numPointure.Enabled = isMonopalme;
        if (!isMonopalme)
        {
            _numPointure.Value = 40;
        }
    }

    private void OnAjouterClick(object? sender, EventArgs e)
    {
        _lblInfo.ForeColor = Color.Firebrick;
        _lblInfo.Text = string.Empty;

        var type = _cbType.SelectedItem as TypeMateriel;
        if (type == null)
        {
            _lblInfo.Text = "Veuillez selectionner un type.";
            return;
        }

        var libelleType = type.LibelleTypeMateriel.ToLowerInvariant();

        var taille = _txtTaille.Enabled ? _txtTaille.Text.Trim() : null;
        var pointure = _numPointure.Enabled ? (int?)_numPointure.Value : null;

        if (_txtTaille.Enabled && string.IsNullOrWhiteSpace(taille))
        {
            _lblInfo.Text = "La taille est obligatoire pour une combinaison.";
            return;
        }

        if (libelleType.Contains("monopalme") && pointure == null)
        {
            _lblInfo.Text = "La pointure est obligatoire pour une monopalme.";
            return;
        }

        var materiel = new Materiel
        {
            Code = _txtCode.Text.Trim(),
            Marque = _txtMarque.Text.Trim(),
            IdTypeMateriel = type.IdTypeMateriel,
            Taille = string.IsNullOrWhiteSpace(taille) ? null : taille,
            Pointure = pointure
        };

        if (DbInterface.AjouterMateriel(materiel, out var message))
        {
            _lblInfo.ForeColor = Color.FromArgb(21, 128, 61);
            _lblInfo.Text = message;
            _txtCode.Clear();
            _txtMarque.Clear();
            _txtTaille.Clear();
            _numPointure.Value = 40;
            _txtCode.Focus();
            return;
        }

        _lblInfo.Text = message;
    }
}
