import React, { useState, useEffect } from 'react';
import { taskService } from '../services/taskService';

/**
 * TaskManager Component - Demonstrates async CRUD operations.
 * 
 * Learning Points:
 * - async/await in React event handlers
 * - useEffect for fetching data on component mount
 * - Loading states during async operations
 * - Error handling in async operations
 */
function TaskManager() {
  const [tasks, setTasks] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [newTask, setNewTask] = useState({ title: '', description: '' });
  const [editingTask, setEditingTask] = useState(null);

  /**
   * useEffect with async function to load tasks on mount.
   * Note: useEffect callback itself cannot be async, so we define an async function inside.
   */
  useEffect(() => {
    loadTasks();
  }, []); // Empty dependency array means this runs once on mount

  /**
   * Fetch all tasks from the API.
   * Demonstrates async data fetching with loading and error states.
   */
  const loadTasks = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await taskService.getAllTasks();
      setTasks(data);
    } catch (err) {
      setError('Failed to load tasks: ' + err.message);
      console.error('Error loading tasks:', err);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Create a new task.
   * Demonstrates async POST request with form data.
   */
  const handleCreateTask = async (e) => {
    e.preventDefault();
    if (!newTask.title.trim()) {
      setError('Task title is required');
      return;
    }

    try {
      setLoading(true);
      setError(null);
      await taskService.createTask(newTask);
      setNewTask({ title: '', description: '' });
      await loadTasks(); // Reload tasks to show the new one
    } catch (err) {
      setError('Failed to create task: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Toggle task completion status.
   * Demonstrates async PUT request.
   */
  const handleToggleComplete = async (task) => {
    try {
      setLoading(true);
      setError(null);
      await taskService.updateTask(task.id, { isCompleted: !task.isCompleted });
      await loadTasks();
    } catch (err) {
      setError('Failed to update task: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Delete a task.
   * Demonstrates async DELETE request.
   */
  const handleDeleteTask = async (taskId) => {
    if (!window.confirm('Are you sure you want to delete this task?')) {
      return;
    }

    try {
      setLoading(true);
      setError(null);
      await taskService.deleteTask(taskId);
      await loadTasks();
    } catch (err) {
      setError('Failed to delete task: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="card">
        <h2>Task Manager - Async CRUD Operations</h2>
        <p>
          This module demonstrates basic Create, Read, Update, Delete (CRUD) operations
          using async/await patterns. Watch the loading states and observe how operations
          complete asynchronously.
        </p>
      </div>

      {error && <div className="error">{error}</div>}

      <div className="card">
        <h3>Create New Task</h3>
        <form onSubmit={handleCreateTask}>
          <div className="form-group">
            <label className="form-label">Title *</label>
            <input
              className="input"
              type="text"
              value={newTask.title}
              onChange={(e) => setNewTask({ ...newTask, title: e.target.value })}
              placeholder="Enter task title"
              disabled={loading}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Description</label>
            <textarea
              className="textarea"
              value={newTask.description}
              onChange={(e) => setNewTask({ ...newTask, description: e.target.value })}
              placeholder="Enter task description"
              disabled={loading}
            />
          </div>
          <button type="submit" className="button button-primary" disabled={loading}>
            {loading ? 'Creating...' : 'Create Task'}
          </button>
        </form>
      </div>

      <div className="card">
        <h3>Task List ({tasks.length})</h3>
        {loading && <div className="loading"><div className="spinner"></div> Loading tasks...</div>}
        
        {!loading && tasks.length === 0 && (
          <p style={{ color: 'var(--text-secondary)' }}>No tasks yet. Create one above!</p>
        )}

        {!loading && tasks.length > 0 && (
          <div className="task-list">
            {tasks.map((task) => (
              <div key={task.id} className={`task-item ${task.isCompleted ? 'completed' : ''}`}>
                <div className="task-info">
                  <div className="task-title">{task.title}</div>
                  <div className="task-description">{task.description}</div>
                  <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginTop: '4px' }}>
                    Created: {new Date(task.createdAt).toLocaleString()}
                    {task.completedAt && ` • Completed: ${new Date(task.completedAt).toLocaleString()}`}
                  </div>
                </div>
                <div className="task-actions">
                  <button
                    className={`button ${task.isCompleted ? 'button-secondary' : 'button-success'}`}
                    onClick={() => handleToggleComplete(task)}
                    disabled={loading}
                  >
                    {task.isCompleted ? '↩️ Undo' : '✓ Complete'}
                  </button>
                  <button
                    className="button button-danger"
                    onClick={() => handleDeleteTask(task.id)}
                    disabled={loading}
                  >
                    🗑️ Delete
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="card">
        <h3>🎓 Learning Points</h3>
        <ul style={{ textAlign: 'left', marginLeft: '20px' }}>
          <li><strong>async/await in React:</strong> Event handlers use async/await for API calls</li>
          <li><strong>Loading States:</strong> UI shows feedback during async operations</li>
          <li><strong>Error Handling:</strong> try/catch blocks handle API errors gracefully</li>
          <li><strong>Task&lt;T&gt; Return Types:</strong> Backend methods return Task or Task&lt;T&gt;</li>
          <li><strong>HTTP Methods:</strong> GET (fetch), POST (create), PUT (update), DELETE (remove)</li>
        </ul>
      </div>
    </div>
  );
}

export default TaskManager;
