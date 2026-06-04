using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProjectUploader.DownloadApp.Servicos;

namespace ProjectUploader.DownloadApp.Formularios;

public partial class FrmPrincipalDownload : Form
{
    private readonly ApiClientDownloader _apiClient;
    private Button btnAtualizar;
    private Button btnSelecionarDestino;
    private Button btnIniciarDownload;
    private DataGridView dgvArquivos;
    private Label lblStatusGlobal;
    private ProgressBar pbGlobal;
    
    private string _pastaDestino = string.Empty;
    private List<ApiClientDownloader.ArquivoResponse> _arquivosDisponiveis = new();
    private CancellationTokenSource _cts = new();

    public FrmPrincipalDownload(ApiClientDownloader apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Project Uploader Server - Recepção de Arquivos (Multi-Thread)";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;

        btnAtualizar = new Button { Text = "Atualizar Lista", Location = new Point(20, 20), Width = 120 };
        btnAtualizar.Click += BtnAtualizar_Click;

        btnSelecionarDestino = new Button { Text = "Destino...", Location = new Point(150, 20), Width = 120 };
        btnSelecionarDestino.Click += BtnSelecionarDestino_Click;

        btnIniciarDownload = new Button { Text = "Baixar Lote", Location = new Point(280, 20), Width = 120, Enabled = false };
        btnIniciarDownload.Click += BtnIniciarDownload_Click;

        lblStatusGlobal = new Label { Location = new Point(410, 25), Width = 350, Text = "Pasta destino não selecionada." };

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

        dgvArquivos.Columns.Add("Id", "Id");
        dgvArquivos.Columns["Id"].Visible = false;
        dgvArquivos.Columns.Add("Nome", "Nome do Arquivo");
        dgvArquivos.Columns.Add("Tamanho", "Tamanho");
        dgvArquivos.Columns.Add("Progresso", "Progresso");
        dgvArquivos.Columns.Add("Status", "Status Servidor");

        this.Controls.Add(btnAtualizar);
        this.Controls.Add(btnSelecionarDestino);
        this.Controls.Add(btnIniciarDownload);
        this.Controls.Add(lblStatusGlobal);
        this.Controls.Add(pbGlobal);
        this.Controls.Add(dgvArquivos);
        
        this.Load += FrmPrincipalDownload_Load;
    }

    private void FrmPrincipalDownload_Load(object? sender, EventArgs e)
    {
        BtnAtualizar_Click(sender, e);
    }

    private async void BtnAtualizar_Click(object? sender, EventArgs e)
    {
        try
        {
            btnAtualizar.Enabled = false;
            _arquivosDisponiveis = await _apiClient.ListarArquivosAsync();
            
            dgvArquivos.Rows.Clear();
            foreach (var arq in _arquivosDisponiveis)
            {
                // Se status == 2 é Concluído
                dgvArquivos.Rows.Add(arq.Id.ToString(), arq.NomeOriginal, $"{arq.TotalBytes / 1024.0 / 1024.0:F2} MB", "0%", arq.Status == 2 ? "Disponível" : "Pendente/Erro");
            }
            
            ValidarBotaoDownload();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao atualizar lista: {ex.Message}");
        }
        finally
        {
            btnAtualizar.Enabled = true;
        }
    }

    private void BtnSelecionarDestino_Click(object? sender, EventArgs e)
    {
        using var fbd = new FolderBrowserDialog();
        if (fbd.ShowDialog() == DialogResult.OK)
        {
            _pastaDestino = fbd.SelectedPath;
            lblStatusGlobal.Text = $"Destino: {_pastaDestino}";
            ValidarBotaoDownload();
        }
    }

    private void ValidarBotaoDownload()
    {
        btnIniciarDownload.Enabled = !string.IsNullOrEmpty(_pastaDestino) && _arquivosDisponiveis.Any(a => a.Status == 2);
    }

    private async void BtnIniciarDownload_Click(object? sender, EventArgs e)
    {
        btnIniciarDownload.Enabled = false;
        btnSelecionarDestino.Enabled = false;
        btnAtualizar.Enabled = false;
        _cts = new CancellationTokenSource();

        using var semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
        var tasks = new List<Task>();
        var arquivosParaBaixar = _arquivosDisponiveis.Where(a => a.Status == 2).ToList(); // Somente concluídos
        int concluidos = 0;

        pbGlobal.Maximum = arquivosParaBaixar.Count;
        pbGlobal.Value = 0;

        for (int i = 0; i < arquivosParaBaixar.Count; i++)
        {
            int rowIndex = i; // Supondo que a ordem é mantida para os disponíveis, mas ideal buscar a row correta.
            // Para ser preciso:
            var row = dgvArquivos.Rows.Cast<DataGridViewRow>().FirstOrDefault(r => r.Cells["Id"].Value.ToString() == arquivosParaBaixar[i].Id.ToString());
            if (row == null) continue;
            
            int realRowIndex = row.Index;
            var arquivoInfo = arquivosParaBaixar[i];

            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync(_cts.Token);
                try
                {
                    AtualizarGrid(realRowIndex, "Baixando", "0%");
                    
                    bool sucesso = await _apiClient.FazerDownloadArquivoAsync(arquivoInfo.Id, _pastaDestino, arquivoInfo.NomeOriginal, (pct) => 
                    {
                        AtualizarGrid(realRowIndex, "Baixando", $"{pct}%");
                    }, _cts.Token);

                    AtualizarGrid(realRowIndex, sucesso ? "Concluído" : "Erro", sucesso ? "100%" : "Falhou");
                }
                catch (Exception)
                {
                    AtualizarGrid(realRowIndex, "Erro", "Falhou");
                }
                finally
                {
                    Interlocked.Increment(ref concluidos);
                    Invoke((Action)(() =>
                    {
                        pbGlobal.Value = concluidos;
                        lblStatusGlobal.Text = $"Baixados {concluidos} de {arquivosParaBaixar.Count} arquivos.";
                    }));
                    semaphore.Release();
                }
            }, _cts.Token));
        }

        await Task.WhenAll(tasks);
        MessageBox.Show("Downloads Concluídos!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
        btnSelecionarDestino.Enabled = true;
        btnAtualizar.Enabled = true;
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
