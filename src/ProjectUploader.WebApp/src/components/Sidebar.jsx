import React from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import './Sidebar.css';

const Sidebar = () => {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem('token');
    navigate('/login');
  };

  return (
    <div className="sidebar">
      <div className="sidebar-header">
        <h2>ProjectUploader</h2>
        <p>Admin Dashboard</p>
      </div>
      
      <nav className="sidebar-nav">
        <NavLink to="/" className={({isActive}) => isActive ? "nav-item active" : "nav-item"}>
          <span>Dashboard</span>
        </NavLink>
        <NavLink to="/usuarios" className={({isActive}) => isActive ? "nav-item active" : "nav-item"}>
          <span>Usuários</span>
        </NavLink>
        <NavLink to="/logs" className={({isActive}) => isActive ? "nav-item active" : "nav-item"}>
          <span>Logs de Eventos</span>
        </NavLink>
      </nav>

      <div className="sidebar-footer">
        <button onClick={handleLogout} className="logout-btn">
          Sair do Sistema
        </button>
      </div>
    </div>
  );
};

export default Sidebar;
