import React, { useEffect, useState } from 'react';
import Card from '../components/Card';
import Table from '../components/Table';
import { api } from '../services/api';

const Dashboard = () => {
  const [arquivos, setArquivos] = useState([]);
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchData = async () => {
    try {
      const [arquivosData, logsData] = await Promise.all([
        api.getArquivos(),
        api.getLogs()
      ]);
      setArquivos(arquivosData);
      setLogs(logsData.slice(0, 5)); // Últimos 5 logs
    } catch (error) {
      console.error("Erro ao buscar dados do dashboard:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    // Polling a cada 5 segundos
    const interval = setInterval(fetchData, 5000);
    return () => clearInterval(interval);
  }, []);

  const arquivosColumns = [
    { title: 'Nome do Arquivo', dataIndex: 'nomeOriginal' },
    { title: 'Tamanho (MB)', render: (row) => `${(row.totalBytes / 1024 / 1024).toFixed(2)} MB` },
    { title: 'Status', render: (row) => (
      <span style={{ color: row.status === 1 ? 'var(--success)' : 'var(--warning)' }}>
        {row.status === 1 ? 'Concluído' : 'Processando'}
      </span>
    )}
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Dashboard (Tempo Real)</h1>
      
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem', marginBottom: '2rem' }}>
        <Card>
          <h3 style={{ color: 'var(--text-secondary)', fontSize: '0.875rem' }}>Total de Uploads</h3>
          <p style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--accent-primary)' }}>{arquivos.length}</p>
        </Card>
        <Card>
          <h3 style={{ color: 'var(--text-secondary)', fontSize: '0.875rem' }}>Armazenamento Utilizado</h3>
          <p style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--success)' }}>
            {(arquivos.reduce((acc, curr) => acc + curr.totalBytes, 0) / 1024 / 1024).toFixed(2)} MB
          </p>
        </Card>
      </div>

      <Card title="Últimos Arquivos Recebidos">
        {loading && arquivos.length === 0 ? (
          <p>Carregando...</p>
        ) : (
          <Table columns={arquivosColumns} data={arquivos.slice(0, 10)} />
        )}
      </Card>
    </div>
  );
};

export default Dashboard;
