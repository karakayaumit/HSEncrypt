using System.Data;
using System.Text;
using System.Windows.Forms;

namespace BankServiceViewer;

public class SettingDetailsDialog : Form
{
    private readonly DataRow _row;

    public SettingDetailsDialog(DataRow row)
    {
        _row = row;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Seçilen Satır Detayı";
        Width = 500;
        Height = 400;
        var textBox = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };

        textBox.Text = BuildDetailsText();
        Controls.Add(textBox);
    }

    private string BuildDetailsText()
    {
        var builder = new StringBuilder();
        foreach (DataColumn column in _row.Table.Columns)
        {
            builder.AppendLine($"{column.ColumnName}: {_row[column]}".Trim());
        }

        return builder.ToString();
    }
}
