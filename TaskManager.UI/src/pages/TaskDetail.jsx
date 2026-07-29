import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import API from '../api/axiosInstance';
import Loader from '../components/Loader';
import { useAuth } from '../context/AuthContext';

export default function TaskDetail() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();
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

  const handleToggleSubtask = async (subtaskId) => {
    if (!task) return;
    const updatedSubtasks = task.subTasks.map(st =>
      st.id === subtaskId ? { ...st, isCompleted: !st.isCompleted } : st
    );

    const payload = {
      title: task.title,
      description: task.description,
      dueDate: task.dueDate,
      priority: task.priority,
      status: task.status,
      category: task.category,
      subTasks: updatedSubtasks,
    };

    try {
      const response = await API.put(`/tasks/${id}`, payload);
      setTask(response.data);
    } catch (err) {
      setError('Failed to update subtask status.');
    }
  };

  const handleDelete = async () => {
    if (!window.confirm('Are you sure you want to delete this task?')) return;

    try {
      await API.delete(`/tasks/${id}`);
      navigate('/tasks');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete task.');
    }
  };

  const handleRestore = async () => {
    try {
      const response = await API.post(`/tasks/${id}/restore`);
      setTask(response.data);
    } catch (err) {
      setError('Failed to restore task.');
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

  const completedSubtasksCount = task?.subTasks?.filter(st => st.isCompleted).length || 0;
  const totalSubtasksCount = task?.subTasks?.length || 0;
  const progressPercent = totalSubtasksCount > 0 ? Math.round((completedSubtasksCount / totalSubtasksCount) * 100) : 0;

  if (loading) {
    return (
      <div className="page">
        <Loader text="Loading task details..." />
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
        <h1>Task Details</h1>
        <Link to="/tasks" className="btn btn-secondary">← Back to Tasks</Link>
      </div>

      {task && (
        <div className="detail-card">
          <div className="detail-header">
            <h2>{task.title}</h2>
            <div className="task-badges">
              <span className={`badge ${getPriorityClass(task.priority)}`}>{task.priority} Priority</span>
              <span className={`badge ${getStatusClass(task.status)}`}>{task.status}</span>
              {task.isAdminAssigned && <span className="badge badge-warning">🔒 Admin Assigned</span>}
            </div>
          </div>

          <div className="detail-body">
            <div className="detail-row">
              <strong>Assigned To:</strong>
              <span>👤 {task.assignedUserName || 'Unassigned'}</span>
            </div>

            <div className="detail-row">
              <strong>Description:</strong>
              <p>{task.description || 'No description provided.'}</p>
            </div>

            <div className="detail-grid">
              <div className="detail-item">
                <span className="detail-label">Category</span>
                <span className="detail-value">{task.category || 'Uncategorized'}</span>
              </div>

              <div className="detail-item">
                <span className="detail-label">Due Date</span>
                <span className="detail-value">
                  {task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No due date'}
                </span>
              </div>
            </div>

            {/* Subtasks Section */}
            {task.subTasks && task.subTasks.length > 0 && (
              <div style={{ marginTop: '24px', borderTop: '1px solid var(--border)', paddingTop: '20px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
                  <h3 style={{ fontSize: '16px', fontWeight: '600' }}>Checklist / Subtasks</h3>
                  <span style={{ fontSize: '13px', color: 'var(--text-secondary)' }}>
                    {completedSubtasksCount} of {totalSubtasksCount} completed ({progressPercent}%)
                  </span>
                </div>

                <div style={{ height: '8px', background: 'var(--bg-input)', borderRadius: '4px', overflow: 'hidden', marginBottom: '16px' }}>
                  <div style={{ width: `${progressPercent}%`, height: '100%', background: 'var(--accent)', transition: 'width 0.3s ease' }}></div>
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                  {task.subTasks.map(st => (
                    <label key={st.id} style={{ display: 'flex', alignItems: 'center', gap: '12px', background: 'var(--bg-input)', padding: '10px 14px', borderRadius: 'var(--radius-sm)', cursor: 'pointer' }}>
                      <input
                        type="checkbox"
                        checked={st.isCompleted}
                        onChange={() => handleToggleSubtask(st.id)}
                        style={{ width: '18px', height: '18px', cursor: 'pointer' }}
                      />
                      <span style={{ textDecoration: st.isCompleted ? 'line-through' : 'none', opacity: st.isCompleted ? 0.6 : 1, fontSize: '14px' }}>
                        {st.title}
                      </span>
                    </label>
                  ))}
                </div>
              </div>
            )}
          </div>

          <div className="detail-actions">
            {task.isDeleted ? (
              user?.role === 'Admin' && (
                <button onClick={handleRestore} className="btn btn-primary">
                  🔄 Restore Task
                </button>
              )
            ) : (
              <>
                <Link to={`/tasks/edit/${task.id}`} className="btn btn-primary">Edit Task</Link>
                {task.isAdminAssigned && user?.role !== 'Admin' ? (
                  <span className="badge badge-warning" style={{ fontSize: '12px', padding: '10px 16px' }}>
                    🔒 Admin Assigned (Deletion Restricted)
                  </span>
                ) : (
                  <button onClick={handleDelete} className="btn btn-danger">Delete Task</button>
                )}
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
