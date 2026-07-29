import { useState, useEffect, useRef } from 'react';
import { Link } from 'react-router-dom';
import API from '../api/axiosInstance';
import Loader from '../components/Loader';

export default function TaskList() {
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

    if (searchTerm) {
      result = result.filter(t =>
        t.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
        t.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
        t.category.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    if (statusFilter !== 'All') {
      result = result.filter(t => t.status === statusFilter);
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
      setTasks(tasks.filter(t => t.id !== id));
      setSuccessMsg('Task deleted successfully.');
      setTimeout(() => setSuccessMsg(''), 3000);
    } catch (err) {
      setError('Failed to delete task.');
    }
  };

  const handleExport = async () => {
    try {
      const response = await API.get('/tasks/export', { responseType: 'blob' });
      const blob = new Blob([response.data], { type: 'application/json' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'tasks.json';
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
      setSuccessMsg('Tasks exported successfully!');
      setTimeout(() => setSuccessMsg(''), 3000);
    } catch (err) {
      setError('Failed to export tasks.');
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
        <div className="header-actions" style={{ display: 'flex', gap: '8px' }}>
          <button onClick={handleExport} className="btn btn-secondary">
            📥 Export
          </button>
          <button onClick={() => fileInputRef.current?.click()} className="btn btn-secondary">
            📤 Import
          </button>
          <input
            type="file"
            ref={fileInputRef}
            style={{ display: 'none' }}
            accept=".json"
            onChange={handleImportFileChange}
          />
          <Link to="/tasks/new" className="btn btn-primary">
            ➕ New Task
          </Link>
        </div>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {successMsg && <div className="alert alert-success">{successMsg}</div>}

      <div className="filters">
        <input
          type="text"
          className="search-input"
          placeholder="Search tasks..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className="filter-select">
          <option value="All">All Status</option>
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Completed">Completed</option>
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
          <Link to="/tasks/new" className="btn btn-primary">Create your first task</Link>
        </div>
      ) : (
        <div className="task-list">
          {filteredTasks.map((task) => (
            <div key={task.id} className="task-card">
              <div className="task-card-header">
                <h3>
                  <Link to={`/tasks/${task.id}`}>{task.title}</Link>
                </h3>
                <div className="task-badges">
                  <span className={`badge ${getPriorityClass(task.priority)}`}>{task.priority}</span>
                  <span className={`badge ${getStatusClass(task.status)}`}>{task.status}</span>
                </div>
              </div>
              <p className="task-description">{task.description || 'No description'}</p>
              <div className="task-card-footer">
                <div className="task-meta">
                  {task.category && <span className="task-category">📁 {task.category}</span>}
                  {task.dueDate && (
                    <span className="task-due-date">
                      📅 {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                  )}
                </div>
                <div className="task-actions">
                  <Link to={`/tasks/edit/${task.id}`} className="btn btn-sm btn-secondary">Edit</Link>
                  <button onClick={() => handleDelete(task.id)} className="btn btn-sm btn-danger">Delete</button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
