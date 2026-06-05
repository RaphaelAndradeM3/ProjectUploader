import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../services/api';
import Card from '../components/Card';
import Input from '../components/Input';
import Button from '../components/Button';
import './Login.css';

const Login = () => {
  const [login, setLogin] = useState('');
  const [senha, setSenha] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await api.login(login, senha);
      if (response && response.token) {
        localStorage.setItem('token', response.token);
        navigate('/');
      } else {
        setError('Token inválido ou não recebido.');
      }
    } catch (err) {
      setError(err.message || 'Erro ao realizar login.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-background"></div>
      <Card className="login-card">
        <div className="login-header">
          <h2>Bem-vindo de volta</h2>
          <p>Acesse o painel administrativo</p>
        </div>
        <form onSubmit={handleLogin} className="login-form">
          <Input 
            label="Usuário ou E-mail" 
            placeholder="admin" 
            value={login}
            onChange={(e) => setLogin(e.target.value)}
            required 
          />
          <Input 
            label="Senha" 
            type="password" 
            placeholder="••••••••" 
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            required 
          />
          {error && <div className="login-error">{error}</div>}
          <Button type="submit" variant="primary" className="login-btn" disabled={loading}>
            {loading ? 'Entrando...' : 'Entrar no Sistema'}
          </Button>
        </form>
      </Card>
    </div>
  );
};

export default Login;
