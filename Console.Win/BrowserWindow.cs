using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Remotely.Console.Win;

internal sealed class BrowserWindow : Form
{
    private readonly CoreWebView2Environment _environment;
    private readonly string _initialUrl;
    private readonly ToolStripStatusLabel _statusLabel;
    private readonly WebView2 _webView;

    public BrowserWindow(CoreWebView2Environment environment, string initialUrl)
    {
        _environment = environment;
        _initialUrl = initialUrl;

        Text = "Sessão remota — Remotely Console";
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1180, 740);
        MinimumSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterParent;

        var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (appIcon is not null)
        {
            Icon = appIcon;
        }

        var toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            RenderMode = ToolStripRenderMode.System
        };
        var backButton = new ToolStripButton("Voltar")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text
        };
        backButton.Click += (_, _) =>
        {
            if (_webView.CoreWebView2?.CanGoBack == true)
            {
                _webView.CoreWebView2.GoBack();
            }
        };
        var refreshButton = new ToolStripButton("Atualizar")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text
        };
        refreshButton.Click += (_, _) => _webView.CoreWebView2?.Reload();
        toolStrip.Items.AddRange([backButton, refreshButton]);

        var statusStrip = new StatusStrip
        {
            RenderMode = ToolStripRenderMode.System
        };
        _statusLabel = new ToolStripStatusLabel("Abrindo sessão...")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        statusStrip.Items.Add(_statusLabel);

        _webView = new WebView2
        {
            AllowExternalDrop = true,
            Dock = DockStyle.Fill
        };

        Controls.Add(_webView);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);
        Load += HandleLoad;
    }

    private async void HandleLoad(object? sender, EventArgs e)
    {
        try
        {
            await _webView.EnsureCoreWebView2Async(_environment);
            var core = _webView.CoreWebView2;
            core.Settings.AreDevToolsEnabled = false;
            core.Settings.IsStatusBarEnabled = false;

            core.NavigationStarting += (_, args) =>
            {
                _statusLabel.Text = $"Abrindo {args.Uri}...";
            };
            core.NavigationCompleted += (_, args) =>
            {
                _statusLabel.Text = args.IsSuccess
                    ? "Sessão carregada."
                    : $"Falha de navegação: {args.WebErrorStatus}";
            };
            core.DocumentTitleChanged += (_, _) =>
            {
                Text = string.IsNullOrWhiteSpace(core.DocumentTitle)
                    ? "Sessão remota — Remotely Console"
                    : $"{core.DocumentTitle} — Remotely Console";
            };
            core.NewWindowRequested += (_, args) =>
            {
                args.Handled = true;
                if (string.IsNullOrWhiteSpace(args.Uri))
                {
                    return;
                }

                new BrowserWindow(_environment, args.Uri).Show(this);
            };

            core.Navigate(_initialUrl);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Falha ao abrir sessão.";
            MessageBox.Show(
                this,
                $"Não foi possível abrir a janela.\n\n{ex.Message}",
                "Erro",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
