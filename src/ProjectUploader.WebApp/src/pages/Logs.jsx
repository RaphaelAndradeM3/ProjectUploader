import React, { useEffect, useState } from 'react';
import Card from '../components/Card';
import Table from '../components/Table';
import { api } from '../services/api';

const Logs = () => {
  const [logs, setLogs] = useState([]);
  const [loading, setLoading] = useState(true);

  const fetchLogs = async () => {
    try {
      const data = await api.getLogs();
      setLogs(data);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchLogs();
    const interval = setInterval(fetchLogs, 5000); // Polling 5s
    return () => clearInterval(interval);
  }, []);

  const columns = [
    { title: 'Data/Hora', render: (row) => new Date(row.dataHora).toLocaleString() },
    { title: 'Nível', render: (row) => (
      <span style={{ 
        padding: '0.25rem 0.5rem', 
        borderRadius: '4px',
        fontSize: '0.75rem',
        fontWeight: 'bold',
        backgroundColor: row.nivel === 'Error' ? 'rgba(239, 68, 68, 0.2)' : 'rgba(16, 185, 129, 0.2)',
        color: row.nivel === 'Error' ? 'var(--error)' : 'var(--success)'
      }}>
        {row.nivel}
      </span>
    )},
    { title: 'Operação', dataIndex: 'operacao' },
    { title: 'Detalhes', dataIndex: 'detalhes' },
    { title: 'IP Origem', dataIndex: 'ipOrigem' }
  ];

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Logs de Eventos (Auditoria)</h1>
      <Card>
        {loading && logs.length === 0 ? <p>Carregando...</p> : <Table columns={columns} data={logs} keyField="id" />}
      </Card>
    </div>
  );
};

export default Logs;
