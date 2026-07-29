import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import API from '../api/axiosInstance';

export default function TaskDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchTask();
  }, [id]);

  const fetchTask = async () => {
    try {
      const response = await API.get(`/tasks/${id}`);
      setTask(response.data);
    } catch (err) {
      setError('Task not found or access denied.');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this task?')) return;

    try {
      await API.delete(`/tasks/${id}`);
      navigate('/tasks');
    } catch (err) {
      setError('Failed to delete task.');
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
        <div className="loading-screen">
          <div className="spinner"></div>
          <p>Loading task details...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="page">
        <div className="alert alert-error">{error}</div>
        <Link to="/tasks" className="btn btn-secondary">← Back to Tasks</Link>
      </div>
    );
  }

  return (
    <div className="page">
      <div className="page-header">
        <Link to="/tasks" className="btn btn-secondary">← Back to Tasks</Link>
      </div>

      {task && (
        <div className="detail-card">
          <div className="detail-header">
            <h1>{task.title}</h1>
            <div className="task-badges">
              <span className={`badge ${getPriorityClass(task.priority)}`}>{task.priority}</span>
              <span className={`badge ${getStatusClass(task.status)}`}>{task.status}</span>
            </div>
          </div>

          <div className="detail-body">
            <div className="detail-row">
              <strong>Description</strong>
              <p>{task.description || 'No description provided.'}</p>
            </div>

            <div className="detail-grid">
              <div className="detail-row">
                <strong>Category</strong>
                <p>{task.category || 'None'}</p>
              </div>
              <div className="detail-row">
                <strong>Due Date</strong>
                <p>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'Not set'}</p>
              </div>
              <div className="detail-row">
                <strong>Assigned To</strong>
                <p>{task.assignedUserName || 'Unassigned'}</p>
              </div>
            </div>
          </div>

          <div className="detail-actions">
            <Link to={`/tasks/edit/${task.id}`} className="btn btn-primary">Edit Task</Link>
            <button onClick={handleDelete} className="btn btn-danger">Delete Task</button>
          </div>
        </div>
      )}
    </div>
  );
}
