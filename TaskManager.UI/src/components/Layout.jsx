import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { LayoutDashboard, CheckSquare, PlusCircle, Users, User, LogOut, ShieldCheck } from 'lucide-react';

export default function Layout() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <div className="brand-icon">
            <CheckSquare size={20} strokeWidth={2.5} />
          </div>
          <h1>TaskManager</h1>
        </div>

        <nav className="sidebar-nav">
          <NavLink to="/dashboard" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            <LayoutDashboard size={18} className="nav-icon" />
            Dashboard
          </NavLink>
          <NavLink to="/tasks" end className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            <CheckSquare size={18} className="nav-icon" />
            Tasks
          </NavLink>
          <NavLink to="/tasks/new" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            <PlusCircle size={18} className="nav-icon" />
            New Task
          </NavLink>
          {user?.role === 'Admin' && (
            <NavLink to="/users" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
              <Users size={18} className="nav-icon" />
              Users
            </NavLink>
          )}
          <NavLink to="/profile" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>
            <User size={18} className="nav-icon" />
            Profile
          </NavLink>
        </nav>

        <div className="sidebar-footer">
          <div className="user-info">
            <div className="user-avatar">{user?.name?.charAt(0)?.toUpperCase() || 'U'}</div>
            <div className="user-details">
              <span className="user-name">{user?.name || 'User'}</span>
              <span className="user-role" style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                {user?.role === 'Admin' && <ShieldCheck size={12} color="#fca311" />}
                {user?.role || 'User'}
              </span>
            </div>
          </div>
          <button className="logout-btn" onClick={handleLogout} style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
            <LogOut size={16} /> Logout
          </button>
        </div>
      </aside>

      <main className="main-content">
        <Outlet />
      </main>
    </div>
  );
}
