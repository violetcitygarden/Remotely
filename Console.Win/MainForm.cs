using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Remotely.Console.Win;

internal sealed class MainForm : Form
{
    private readonly ToolStripButton _backButton;
    private readonly ToolStripStatusLabel _addressLabel;
    private readonly ToolStripStatusLabel _connectionStatusLabel;
    private readonly SettingsStore _settingsStore;
    private readonly WebView2 _webView;
    private CoreWebView2Environment? _webViewEnvironment;
    private ServerSettings _settings;

    public MainForm(SettingsStore settingsStore)
    {
        _settingsStore = settingsStore;
        _settings = settingsStore.Load();

        Text = "Remotely Console";
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1240, 760);
        MinimumSize = new Size(900, 560);
        StartPosition = FormStartPosition.CenterScreen;

        var appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (appIcon is not null)
        {
            Icon = appIcon;
        }

        var menuStrip = BuildMenuStrip();
        var toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            RenderMode = ToolStripRenderMode.System
        };

        _backButton = new ToolStripButton("Voltar")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text,
            Enabled = false
        };
        _backButton.Click += (_, _) => GoBack();

        var homeButton = new ToolStripButton("Início")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text
        };
        homeButton.Click += (_, _) => NavigateHome();

        var refreshButton = new ToolStripButton("Atualizar")
        {
            DisplayStyle = ToolStripItemDisplayStyle.Text
        };
        refreshButton.Click += (_, _) => Reload();

        var configureButton = new ToolStripButton("Servidor...")
        {
            Alignment = ToolStripItemAlignment.Right,
            DisplayStyle = ToolStripItemDisplayStyle.Text
        };
        configureButton.Click += (_, _) => ConfigureServer();

        _addressLabel = new ToolStripStatusLabel(_settings.ServerUrl)
        {
            AutoSize = false,
            BorderSides = ToolStripStatusLabelBorderSides.All,
            BorderStyle = Border3DStyle.SunkenOuter,
            TextAlign = ContentAlignment.MiddleLeft,
            Width = 430
        };

        toolStrip.Items.AddRange([
            _backButton,
            homeButton,
            refreshButton,
            new ToolStripSeparator(),
            new ToolStripLabel("Servidor:"),
            _addressLabel,
            configureButton
        ]);

        var statusStrip = new StatusStrip
        {
            RenderMode = ToolStripRenderMode.System
        };
        _connectionStatusLabel = new ToolStripStatusLabel("Inicializando...")
        {
            Spring = true,
            TextAlign = ContentAlignment.MiddleLeft
        };
        statusStrip.Items.Add(_connectionStatusLabel);
        statusStrip.Items.Add(new ToolStripStatusLabel("Hub remoto"));

        _webView = new WebView2
        {
            AllowExternalDrop = true,
            CreationProperties = null,
            Dock = DockStyle.Fill
        };

        Controls.Add(_webView);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        Controls.Add(statusStrip);
        MainMenuStrip = menuStrip;

        Load += HandleLoad;
    }

    private MenuStrip BuildMenuStrip()
    {
        var menuStrip = new MenuStrip
        {
            RenderMode = ToolStripRenderMode.System
        };

        var fileMenu = new ToolStripMenuItem("&Arquivo");
        var configureItem = new ToolStripMenuItem("&Configurar servidor...");
        configureItem.Click += (_, _) => ConfigureServer();
        var exitItem = new ToolStripMenuItem("Sai&r");
        exitItem.Click += (_, _) => Close();
        fileMenu.DropDownItems.AddRange([
            configureItem,
            new ToolStripSeparator(),
            exitItem
        ]);

        var navigationMenu = new ToolStripMenuItem("&Navegação");
        var backItem = new ToolStripMenuItem("&Voltar");
        backItem.Click += (_, _) => GoBack();
        var homeItem = new ToolStripMenuItem("&Início");
        homeItem.Click += (_, _) => NavigateHome();
        var refreshItem = new ToolStripMenuItem("&Atualizar");
        refreshItem.ShortcutKeys = Keys.F5;
        refreshItem.Click += (_, _) => Reload();
        navigationMenu.DropDownItems.AddRange([
            backItem,
            homeItem,
            refreshItem
        ]);

        var helpMenu = new ToolStripMenuItem("A&juda");
        var aboutItem = new ToolStripMenuItem("&Sobre");
        aboutItem.Click += (_, _) => MessageBox.Show(
            this,
            "Remotely Console\nControlador desktop para o hub Remotely.",
            "Sobre",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        helpMenu.DropDownItems.Add(aboutItem);

        menuStrip.Items.AddRange([fileMenu, navigationMenu, helpMenu]);
        return menuStrip;
    }

    private void ConfigureServer()
    {
        using var dialog = new ServerAddressDialog(_settings.ServerUrl);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _settings = new ServerSettings(dialog.ServerAddress);

        try
        {
            _settingsStore.Save(_settings);
        }
        catch (IOException ex)
        {
            MessageBox.Show(
                this,
                $"Não foi possível salvar a configuração.\n\n{ex.Message}",
                "Erro ao salvar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        _addressLabel.Text = _settings.ServerUrl;
        NavigateHome();
    }

    private void GoBack()
    {
        if (_webView.CoreWebView2?.CanGoBack == true)
        {
            _webView.CoreWebView2.GoBack();
        }
    }

    private async void HandleLoad(object? sender, EventArgs e)
    {
        try
        {
            var profileDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Remotely Console",
                "WebView2");

            _webViewEnvironment = await CoreWebView2Environment.CreateAsync(
                userDataFolder: profileDirectory);

            await _webView.EnsureCoreWebView2Async(_webViewEnvironment);
            ConfigureWebView();
            NavigateHome();
        }
        catch (WebView2RuntimeNotFoundException)
        {
            _connectionStatusLabel.Text = "WebView2 Runtime não encontrado.";
            MessageBox.Show(
                this,
                "O Microsoft Edge WebView2 Runtime precisa estar instalado para abrir o console.",
                "Componente ausente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            _connectionStatusLabel.Text = "Falha ao inicializar.";
            MessageBox.Show(
                this,
                $"Não foi possível iniciar o console.\n\n{ex.Message}",
                "Erro de inicialização",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ConfigureWebView()
    {
        var core = _webView.CoreWebView2;
        core.Settings.AreDefaultScriptDialogsEnabled = true;
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.IsZoomControlEnabled = true;

        core.NavigationStarting += (_, args) =>
        {
            _connectionStatusLabel.Text = $"Conectando a {args.Uri}...";
        };

        core.NavigationCompleted += (_, args) =>
        {
            _connectionStatusLabel.Text = args.IsSuccess
                ? "Conectado."
                : $"Falha de navegação: {args.WebErrorStatus}";
            _backButton.Enabled = core.CanGoBack;
        };

        core.HistoryChanged += (_, _) =>
        {
            _backButton.Enabled = core.CanGoBack;
        };

        core.SourceChanged += (_, _) =>
        {
            if (Uri.TryCreate(core.Source, UriKind.Absolute, out var uri))
            {
                _connectionStatusLabel.Text = uri.Host;
            }
        };

        core.DocumentTitleChanged += (_, _) =>
        {
            Text = string.IsNullOrWhiteSpace(core.DocumentTitle)
                ? "Remotely Console"
                : $"{core.DocumentTitle} — Remotely Console";
        };

        core.NewWindowRequested += (_, args) =>
        {
            args.Handled = true;
            if (_webViewEnvironment is null || string.IsNullOrWhiteSpace(args.Uri))
            {
                return;
            }

            var window = new BrowserWindow(_webViewEnvironment, args.Uri);
            window.Show(this);
        };

        core.ProcessFailed += (_, args) =>
        {
            _connectionStatusLabel.Text = $"Processo do navegador interrompido: {args.ProcessFailedKind}";
        };
    }

    private void NavigateHome()
    {
        if (_webView.CoreWebView2 is null)
        {
            return;
        }

        _webView.CoreWebView2.Navigate(_settings.ServerUrl);
    }

    private void Reload()
    {
        _webView.CoreWebView2?.Reload();
    }
}
