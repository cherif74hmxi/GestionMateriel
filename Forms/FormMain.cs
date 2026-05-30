using GestionMateriel.Models;

namespace GestionMateriel.Forms;

public class FormMain : Form
{
    private readonly Najeur _utilisateur;

    public FormMain(Najeur utilisateur)
    {
        _utilisateur = utilisateur;

        FormUiHelper.ConfigureForm(this, "Lyon Palme - Gestion et pret du materiel", 900, 520);

        Controls.Add(FormUiHelper.CreateTitle("Tableau de bord materiel", 30, 25));

        Controls.Add(new Label
        {
            Left = 30,
            Top = 68,
            Width = 700,
            Text = "Connecte en tant que : " + _utilisateur.NomComplet,
            ForeColor = FormUiHelper.Secondary
        });

        var btnStock = FormUiHelper.CreateButton("Afficher le stock", 30, 120, 260, 52);
        btnStock.Click += (_, _) => Forms.OpenDialog(this, new FormStock());
        Controls.Add(btnStock);

        var btnAjout = FormUiHelper.CreateButton("Ajouter du materiel", 310, 120, 260, 52);
        btnAjout.Click += (_, _) => Forms.OpenDialog(this, new FormAjoutMateriel());
        Controls.Add(btnAjout);

        var btnPret = FormUiHelper.CreateButton("Preter du materiel", 590, 120, 260, 52);
        btnPret.Click += (_, _) => Forms.OpenDialog(this, new FormPret());
        Controls.Add(btnPret);

        var btnRetour = FormUiHelper.CreateButton("Restituer du materiel", 30, 190, 260, 52);
        btnRetour.Click += (_, _) => Forms.OpenDialog(this, new FormRetour());
        Controls.Add(btnRetour);

        var btnHistorique = FormUiHelper.CreateButton("Tracer les prets", 310, 190, 260, 52);
        btnHistorique.Click += (_, _) => Forms.OpenDialog(this, new FormHistorique());
        Controls.Add(btnHistorique);

        var btnFermer = FormUiHelper.CreateButton("Deconnexion", 590, 190, 260, 52, true);
        btnFermer.Click += (_, _) => Close();
        Controls.Add(btnFermer);

        Controls.Add(new Label
        {
            Left = 30,
            Top = 285,
            Width = 830,
            Height = 130,
            BackColor = FormUiHelper.Card,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(16),
            Text =
                "Fonctionnalites disponibles :\n" +
                "- Consultation du stock complet et disponible\n" +
                "- Ajout de materiel (code unique)\n" +
                "- Selection des adherents et creation de prets\n" +
                "- Restitution avec date de retour\n" +
                "- Historique complet des prets pour la tracabilite"
        });
    }
}
