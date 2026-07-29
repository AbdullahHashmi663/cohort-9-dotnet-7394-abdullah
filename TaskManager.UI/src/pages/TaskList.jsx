import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import API from '../api/axiosInstance';
import Loader from '../components/Loader';
import { useAuth } from '../context/AuthContext';
import { Download, Upload, Plus, Search, Folder, Calendar, User, Lock, Trash2, RotateCcw, Edit3 } from 'lucide-react';

export default function TaskList() {
  const { user } = useAuth();
  const [tasks, setTasks] = useState([]);
  const [filteredTasks, setFilteredTasks] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [successMsg, setSuccessMsg] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('All');
  const [priorityFilter, setPriorityFilter] = useState('All');
  const fileInputRef = useRef(null);

  useEffect(() => {
    fetchTasks();
  }, []);

  useEffect(() => {
    applyFilters();
  }, [tasks, searchTerm, statusFilter, priorityFilter]);

  const fetchTasks = async () => {
    try {
      const response = await API.get('/tasks');
      setTasks(response.data);
    } catch (err) {
      setError('Failed to load tasks.');
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    let result = [...tasks];

    if (statusFilter === 'Deleted') {
      result = result.filter(t => t.isDeleted);
    } else {
      result = result.filter(t => !t.isDeleted);
      if (statusFilter !== 'All') {
        result = result.filter(t => t.status === statusFilter);
      }
    }

    if (searchTerm) {
      result = result.filter(t =>
        t.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        t.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        t.category.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    if (priorityFilter !== 'All') {
      result = result.filter(t => t.priority === priorityFilter);
    }

    setFilteredTasks(result);
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this task?')) return;

    try {
      await API.delete(`/tasks/${id}`);
      setSuccessMsg('Task deleted successfully!');
      setTimeout(() => setSuccessMsg(''), 3000);
      fetchTasks();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete task.');
    }
  };

  const handleRestore = async (id) => {
    try {
      await API.post(`/tasks/${id}/restore`);
      setSuccessMsg('Task restored successfully!');
      setTimeout(() => setSuccessMsg(''), 3000);
      fetchTasks();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to restore task.');
    }
  };

  const handleExport = async () => {
    try {
      const response = await API.get('/tasks/export', { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', `tasks-export-${new Date().toISOString().slice(0,10)}.json`);
      document.body.appendChild(link);
      link.click();
      link.remove();
    } catch (err) {
      setError('Failed to export tasks.');
    }
  };

  const handleImportClick = () => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleImportFileChange = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    try {
      const text = await file.text();
      const jsonData = JSON.parse(text);
      const itemsToImport = Array.isArray(jsonData) ? jsonData : [jsonData];

      const response = await API.post('/tasks/import', itemsToImport);
      setSuccessMsg(response.data.message || 'Tasks imported successfully!');
      setTimeout(() => setSuccessMsg(''), 3000);
      fetchTasks();
    } catch (err) {
      setError('Failed to import tasks. Ensure valid JSON format.');
    } finally {
      if (fileInputRef.current) fileInputRef.current.value = '';
    }
  };

  const getPriorityClass = (priority) => {
    switch (priority) {
      case 'High': return 'badge-danger';
      case 'Medium': return 'badge-warning';
      case 'Low': return 'badge-info';
      default: return 'badge-default';
    }
  };

  const getStatusClass = (status) => {
    switch (status) {
      case 'Completed': return 'badge-success';
      case 'InProgress': return 'badge-warning';
      case 'Pending': return 'badge-default';
      default: return 'badge-default';
    }
  };

  if (loading) {
    return (
      <div className="page">
        <Loader text="Loading tasks..." />
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <h1>Tasks</h1>
        <div style={{ display: 'flex', gap: '10px' }}>
          <button onClick={handleExport} className="btn btn-secondary">
            <Download size={16} /> Export
          </button>
          <button onClick={handleImportClick} className="btn btn-secondary">
            <Upload size={16} /> Import
          </button>
          <input
            type="file"
            ref={fileInputRef}
            style={{ display: 'none' }}
            accept=".json"
            onChange={handleImportFileChange}
          />
          <Link to="/tasks/new" className="btn btn-primary">
            <Plus size={16} /> New Task
          </Link>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {successMsg && <div className="alert alert-success">{successMsg}</div>}

      <div className="filters">
        <div style={{ position: 'relative', flex: 1 }}>
          <Search size={16} style={{ position: 'absolute', left: '12px', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
          <input
            type="text"
            className="search-input"
            style={{ paddingLeft: '36px', width: '100%' }}
            placeholder="Search tasks..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="filter-select">
          <option value="All">All Status</option>
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
          {user?.role === 'Admin' && <option value="Deleted">Deleted Tasks</option>}
        </select>
        <select value={priorityFilter} onChange={(e) => setPriorityFilter(e.target.value)} className="filter-select">
          <option value="All">All Priority</option>
          <option value="High">High</option>
          <option value="Medium">Medium</option>
          <option value="Low">Low</option>
        </select>
      </div>

      {filteredTasks.length === 0 ? (
        <div className="empty-state">
          <p>No tasks found.</p>
          <Link to="/tasks/new" className="btn btn-primary">
            <Plus size={16} /> Create your first task
          </Link>
        </div>
      ) : (
        <div className="task-list">
          {filteredTasks.map((task) => (
            <div key={task.id} className="task-card" style={task.isDeleted ? { opacity: 0.75, borderColor: '#d90429' } : {}}>
              <div className="task-card-header">
                <h3>
                  <Link to={`/tasks/${task.id}`}>{task.title}</Link>
                </h3>
                <div className="task-badges">
                  {task.isDeleted ? (
                    <span className="badge badge-danger" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                      <Trash2 size={12} /> Deleted
                    </span>
                  ) : (
                    <>
                      <span className={`badge ${getPriorityClass(task.priority)}`}>{task.priority}</span>
                      <span className={`badge ${getStatusClass(task.status)}`}>{task.status}</span>
                    </>
                  )}
                </div>
              </div>
              <p className="task-description">{task.description || 'No description'}</p>
              <div className="task-card-footer">
                <div className="task-meta">
                  {task.assignedUserName && (
                    <span className="task-assigned" style={{ fontSize: '12px', opacity: 0.85, display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                      <User size={12} /> {task.assignedUserName}
                    </span>
                  )}
                  {task.category && (
                    <span className="task-category" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                      <Folder size={12} /> {task.category}
                    </span>
                  )}
                  {task.dueDate && (
                    <span className="task-due-date" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                      <Calendar size={12} /> {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                  )}
                </div>
                <div className="task-actions">
                  {task.isDeleted ? (
                    user?.role === 'Admin' && (
                      <button onClick={() => handleRestore(task.id)} className="btn btn-sm btn-primary" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                        <RotateCcw size={14} /> Restore
                      </button>
                    )
                  ) : (
                    <>
                      <Link to={`/tasks/edit/${task.id}`} className="btn btn-sm btn-secondary" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                        <Edit3 size={14} /> Edit
                      </Link>
                      {task.isAdminAssigned && user?.role !== 'Admin' ? (
                        <span className="badge badge-warning" style={{ fontSize: '11px', display: 'inline-flex', alignItems: 'center', gap: '4px' }} title="Task assigned by Admin cannot be deleted by user">
                          <Lock size={12} /> Admin Assigned
                        </span>
                      ) : (
                        <button onClick={() => handleDelete(task.id)} className="btn btn-sm btn-danger" style={{ display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                          <Trash2 size={14} /> Delete
                        </button>
                      )}
                    </>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
