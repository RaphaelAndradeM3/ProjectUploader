using System;
using System.Drawing;
using System.Windows.Forms;
using ProjectUploader.UploadApp.Servicos;

namespace ProjectUploader.UploadApp.Formularios;

public partial class FrmLogin : Form
{
    private readonly ApiClient _apiClient;
    private readonly FrmPrincipalUpload _frmPrincipal;

    private TextBox txtUsuario;
    private TextBox txtSenha;
    private Button btnEntrar;
    private Label lblErro;

    public FrmLogin(ApiClient apiClient, FrmPrincipalUpload frmPrincipal)
    {
        _apiClient = apiClient;
        _frmPrincipal = frmPrincipal;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Project Uploader - Login";
        this.Size = new Size(400, 300);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var lblUsuario = new Label { Text = "Usuário ou E-mail:", Location = new Point(50, 50), AutoSize = true };
        txtUsuario = new TextBox { Location = new Point(50, 70), Width = 280 };

        var lblSenha = new Label { Text = "Senha:", Location = new Point(50, 110), AutoSize = true };
        txtSenha = new TextBox { Location = new Point(50, 130), Width = 280, UseSystemPasswordChar = true };

        lblErro = new Label { Location = new Point(50, 170), Width = 280, ForeColor = Color.Red, Text = "" };

        btnEntrar = new Button { Text = "Entrar", Location = new Point(140, 210), Width = 100, Height = 35 };
        btnEntrar.Click += BtnEntrar_Click;

        this.Controls.Add(lblUsuario);
        this.Controls.Add(txtUsuario);
        this.Controls.Add(lblSenha);
        this.Controls.Add(txtSenha);
        this.Controls.Add(lblErro);
        this.Controls.Add(btnEntrar);
    }

    private async void BtnEntrar_Click(object? sender, EventArgs e)
    {
        btnEntrar.Enabled = false;
        lblErro.Text = "Autenticando...";

        bool sucesso = await _apiClient.AutenticarAsync(txtUsuario.Text, txtSenha.Text);

        if (sucesso)
        {
            this.Hide();
            _frmPrincipal.ShowDialog();
            this.Close(); // Fecha a aplicação ao sair do form principal
        }
        else
        {
            lblErro.Text = "Credenciais inválidas. Tente novamente.";
            btnEntrar.Enabled = true;
        }
    }
}
