namespace Remotely.Console.Win;

internal sealed class ServerAddressDialog : Form
{
    private readonly TextBox _addressTextBox;

    public ServerAddressDialog(string currentAddress)
    {
        Text = "Configurar servidor";
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(480, 138);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;

        var explanationLabel = new Label
        {
            AutoSize = true,
            Location = new Point(12, 12),
            Text = "Endereço do computador principal (hub):"
        };

        _addressTextBox = new TextBox
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            Location = new Point(15, 38),
            Size = new Size(450, 23),
            Text = currentAddress
        };

        var exampleLabel = new Label
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Location = new Point(15, 65),
            Text = "Exemplo: https://meu-pc:5001 ou https://hub.minharede.ts.net"
        };

        var okButton = new Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            Location = new Point(309, 99),
            Size = new Size(75, 27),
            Text = "OK"
        };
        okButton.Click += HandleOkClicked;

        var cancelButton = new Button
        {
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
            DialogResult = DialogResult.Cancel,
            Location = new Point(390, 99),
            Size = new Size(75, 27),
            Text = "Cancelar"
        };

        AcceptButton = okButton;
        CancelButton = cancelButton;
        Controls.AddRange([
            explanationLabel,
            _addressTextBox,
            exampleLabel,
            okButton,
            cancelButton
        ]);
    }

    public string ServerAddress { get; private set; } = string.Empty;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _addressTextBox.SelectAll();
        _addressTextBox.Focus();
    }

    private void HandleOkClicked(object? sender, EventArgs e)
    {
        if (!ServerUrl.TryNormalize(_addressTextBox.Text, out var normalized))
        {
            MessageBox.Show(
                this,
                "Informe um endereço HTTP ou HTTPS válido.",
                "Endereço inválido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        ServerAddress = normalized;
        DialogResult = DialogResult.OK;
        Close();
    }
}
