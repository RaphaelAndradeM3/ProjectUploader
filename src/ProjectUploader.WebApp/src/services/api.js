const BASE_URL = 'http://localhost:5206/api';

const request = async (endpoint, options = {}) => {
  const token = localStorage.getItem('token');
  
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  if (!response.ok) {
    if (response.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    const errorData = await response.text();
    throw new Error(errorData || 'Ocorreu um erro na requisição.');
  }

  // Se for 204 No Content, retorna true
  if (response.status === 204) return true;

  return response.json();
};

export const api = {
  login: (login, senha) => request('/Auth/login', {
    method: 'POST',
    body: JSON.stringify({ login, senha })
  }),
  getUsuarios: () => request('/Usuarios'),
  createUsuario: (data) => request('/Usuarios', {
    method: 'POST',
    body: JSON.stringify(data)
  }),
  updateUsuario: (id, data) => request(`/Usuarios/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data)
  }),
  deleteUsuario: (id) => request(`/Usuarios/${id}`, {
    method: 'DELETE'
  }),
  getLogs: () => request('/Logs'),
  getArquivos: () => request('/Arquivos')
};
