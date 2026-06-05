import React, { useEffect, useState } from 'react';
import Card from '../components/Card';
import Table from '../components/Table';
import Button from '../components/Button';
import { api } from '../services/api';

const Usuarios = () => {
  const [usuarios, setUsuarios] = useState([]);
  const [loading, setLoading] = useState(true);

  const carregarUsuarios = async () => {
    setLoading(true);
    try {
      const data = await api.getUsuarios();
      setUsuarios(data);
    } catch (error) {
      console.error(error);
      alert('Erro ao carregar usuários');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    carregarUsuarios();
  }, []);

  const columns = [
    { title: 'ID', dataIndex: 'codigoInterno' },
    { title: 'Nome Completo', dataIndex: 'nomeCompleto' },
    { title: 'Usuário', dataIndex: 'nomeUsuario' },
    { title: 'E-mail', dataIndex: 'email' },
    { title: 'Ações', render: (row) => (
      <Button variant="secondary" size="sm" onClick={() => alert('Edição em breve')}>Editar</Button>
    )}
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 className="text-2xl font-bold">Gerenciamento de Usuários</h1>
        <Button variant="primary" onClick={() => alert('Cadastro em breve')}>+ Novo Usuário</Button>
      </div>

      <Card>
        {loading ? <p>Carregando...</p> : <Table columns={columns} data={usuarios} />}
      </Card>
    </div>
  );
};

export default Usuarios;
