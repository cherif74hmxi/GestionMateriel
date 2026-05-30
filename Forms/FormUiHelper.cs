using System.Drawing;

namespace GestionMateriel.Forms;

internal static class FormUiHelper
{
    public static readonly Color Background = Color.FromArgb(244, 247, 251);
    public static readonly Color Card = Color.White;
    public static readonly Color Primary = Color.FromArgb(15, 118, 110);
    public static readonly Color Secondary = Color.FromArgb(71, 85, 105);
    public static readonly Color Text = Color.FromArgb(17, 24, 39);

    public static void ConfigureForm(Form form, string title, int width, int height)
    {
        form.Text = title;
        form.BackColor = Background;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.ClientSize = new Size(width, height);
        form.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
    }

    public static Label CreateTitle(string text, int x, int y)
    {
        return new Label
        {
            Text = text,
            Left = x,
            Top = y,
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Text
        };
    }

    public static Button CreateButton(string text, int x, int y, int width, int height, bool secondary = false)
    {
        return new Button
        {
            Text = text,
            Left = x,
            Top = y,
            Width = width,
            Height = height,
            FlatStyle = FlatStyle.Flat,
            BackColor = secondary ? Secondary : Primary,
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };
    }

    public static TextBox CreateTextBox(int x, int y, int width, bool password = false)
    {
        return new TextBox
        {
            Left = x,
            Top = y,
            Width = width,
            PasswordChar = password ? '*' : '\0'
        };
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.MultiSelect = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Card;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(226, 232, 240);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Text;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.FlatStyle = FlatStyle.Flat;
    }
}
