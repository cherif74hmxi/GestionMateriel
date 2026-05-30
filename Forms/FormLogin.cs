using GestionMateriel.DAL;

namespace GestionMateriel.Forms;

public class FormLogin : Form
{
    private readonly TextBox _txtLogin;
    private readonly TextBox _txtPassword;
    private readonly Label _lblMessage;

    public FormLogin()
    {
        FormUiHelper.ConfigureForm(this, "Connexion - Gestion Materiel", 520, 340);

        var title = FormUiHelper.CreateTitle("Connexion responsable materiel", 90, 30);
        Controls.Add(title);

        Controls.Add(new Label
        {
            Text = "Login",
            Left = 90,
            Top = 100,
            AutoSize = true
        });

        _txtLogin = FormUiHelper.CreateTextBox(90, 125, 330);
        Controls.Add(_txtLogin);

        Controls.Add(new Label
        {
            Text = "Mot de passe",
            Left = 90,
            Top = 165,
            AutoSize = true
        });

        _txtPassword = FormUiHelper.CreateTextBox(90, 190, 330, true);
        Controls.Add(_txtPassword);

        var btnConnexion = FormUiHelper.CreateButton("Se connecter", 90, 235, 160, 38);
        btnConnexion.Click += OnConnexionClick;
        Controls.Add(btnConnexion);

        var btnQuitter = FormUiHelper.CreateButton("Quitter", 260, 235, 160, 38, true);
        btnQuitter.Click += (_, _) => Close();
        Controls.Add(btnQuitter);

        _lblMessage = new Label
        {
            Left = 90,
            Top = 285,
            Width = 330,
            ForeColor = Color.Firebrick,
            Text = "Compte demo : admin / admin123"
        };
        Controls.Add(_lblMessage);

        AcceptButton = btnConnexion;
    }

    private void OnConnexionClick(object? sender, EventArgs e)
    {
        var login = _txtLogin.Text.Trim();
        var motDePasse = _txtPassword.Text;

        if (DbInterface.VerifierConnexion(login, motDePasse, out var utilisateur))
        {
            _lblMessage.Text = string.Empty;

            using var main = new FormMain(utilisateur!);
            Hide();
            main.ShowDialog(this);
            Show();

            _txtPassword.Clear();
            _txtPassword.Focus();
            return;
        }

        _lblMessage.Text = "Identifiants invalides ou utilisateur non responsable.";
    }
}
