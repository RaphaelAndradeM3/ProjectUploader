using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProjectUploader.UploadApp.Servicos;

namespace ProjectUploader.UploadApp.Formularios;

public partial class FrmPrincipalUpload : Form
{
    private readonly ApiClient _apiClient;
    private Button btnSelecionarPasta;
    private Button btnIniciarUpload;
    private DataGridView dgvArquivos;
    private Label lblStatusGlobal;
    private ProgressBar pbGlobal;

    private List<string> _arquivosPendentes = new();
    private CancellationTokenSource _cts = new();

    public FrmPrincipalUpload(ApiClient apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Project Uploader - Cliente de Envio (Multi-Thread)";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        btnSelecionarPasta = new Button { Text = "Selecionar Pasta", Location = new Point(20, 20), Width = 150 };
        btnSelecionarPasta.Click += BtnSelecionarPasta_Click;

        btnIniciarUpload = new Button { Text = "Iniciar Upload", Location = new Point(190, 20), Width = 150, Enabled = false };
        btnIniciarUpload.Click += BtnIniciarUpload_Click;

        lblStatusGlobal = new Label { Location = new Point(360, 25), Width = 400, Text = "Nenhum arquivo selecionado." };

        pbGlobal = new ProgressBar { Location = new Point(20, 60), Width = 740, Height = 20 };

        dgvArquivos = new DataGridView
        {
            Location = new Point(20, 100),
            Width = 740,
            Height = 440,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        dgvArquivos.Columns.Add("Caminho", "Caminho do Arquivo");
        dgvArquivos.Columns.Add("Tamanho", "Tamanho");
        dgvArquivos.Columns.Add("Progresso", "Progresso");
        dgvArquivos.Columns.Add("Status", "Status");

        this.Controls.Add(btnSelecionarPasta);
        this.Controls.Add(btnIniciarUpload);
        this.Controls.Add(lblStatusGlobal);
        this.Controls.Add(pbGlobal);
        this.Controls.Add(dgvArquivos);
    }

    private void BtnSelecionarPasta_Click(object? sender, EventArgs e)
    {
        using var fbd = new FolderBrowserDialog();
        if (fbd.ShowDialog() == DialogResult.OK)
        {
            _arquivosPendentes = Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.AllDirectories).ToList();

            if (_arquivosPendentes.Count > 999)
            {
                MessageBox.Show("Limite máximo de 999 arquivos simultâneos excedido.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _arquivosPendentes = _arquivosPendentes.Take(999).ToList();
            }

            dgvArquivos.Rows.Clear();
            foreach (var arq in _arquivosPendentes)
            {
                var fileInfo = new FileInfo(arq);
                dgvArquivos.Rows.Add(arq, $"{fileInfo.Length / 1024.0 / 1024.0:F2} MB", "0%", "Pendente");
            }

            lblStatusGlobal.Text = $"{_arquivosPendentes.Count} arquivos selecionados.";
            btnIniciarUpload.Enabled = _arquivosPendentes.Any();
        }
    }

    private async void BtnIniciarUpload_Click(object? sender, EventArgs e)
    {
        btnSelecionarPasta.Enabled = false;
        btnIniciarUpload.Enabled = false;
        _cts = new CancellationTokenSource();

        using var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2); // Controla threads ativas
        var tasks = new List<Task>();
        int concluidos = 0;

        pbGlobal.Maximum = _arquivosPendentes.Count;
        pbGlobal.Value = 0;

        for (int i = 0; i < _arquivosPendentes.Count; i++)
        {
            int rowIndex = i;
            var caminhoArquivo = _arquivosPendentes[i];
            
            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(_cts.Token);
                try
                {
                    AtualizarGrid(rowIndex, "Em Andamento", "0%");
                    var fileInfo = new FileInfo(caminhoArquivo);
                    
                    bool sucesso = await _apiClient.FazerUploadArquivoAsync(caminhoArquivo, (bytesEnviados) => 
                    {
                        var pct = (bytesEnviados * 100) / fileInfo.Length;
                        AtualizarGrid(rowIndex, "Em Andamento", $"{pct}%");
                    }, _cts.Token);

                    AtualizarGrid(rowIndex, sucesso ? "Concluído" : "Erro", sucesso ? "100%" : "Falhou");
                }
                catch (Exception)
                {
                    AtualizarGrid(rowIndex, "Erro", "Falhou");
                }
                finally
                {
                    Interlocked.Increment(ref concluidos);
                    Invoke((Action)(() =>
                    {
                        pbGlobal.Value = concluidos;
                        lblStatusGlobal.Text = $"Enviados {concluidos} de {_arquivosPendentes.Count} arquivos.";
                    }));
                    semaphore.Release();
                }
            }, _cts.Token));
        }

        await Task.WhenAll(tasks);
        MessageBox.Show("Transferência Concluída!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        btnSelecionarPasta.Enabled = true;
    }

    private void AtualizarGrid(int rowIndex, string status, string progresso)
    {
        if (InvokeRequired)
        {
            Invoke(new Action(() => AtualizarGrid(rowIndex, status, progresso)));
            return;
        }

        dgvArquivos.Rows[rowIndex].Cells["Status"].Value = status;
        dgvArquivos.Rows[rowIndex].Cells["Progresso"].Value = progresso;
    }
}
