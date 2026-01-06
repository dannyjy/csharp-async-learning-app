import React, { useState, useEffect } from 'react';
import { notificationService } from '../services/notificationService';

function Notifications() {
  const [notifications, setNotifications] = useState([]);
  const [connected, setConnected] = useState(false);
  const [newNotification, setNewNotification] = useState({
    title: 'Test Notification',
    message: 'This is a test message',
    type: 0 // Info
  });
  const [error, setError] = useState(null);

  useEffect(() => {
    loadRecentNotifications();
    
    // Setup Server-Sent Events
    const eventSource = notificationService.createEventSource();
    
    eventSource.onopen = () => {
      setConnected(true);
      console.log('SSE Connection opened');
    };

    eventSource.addEventListener('notification', (event) => {
      try {
        const notification = JSON.parse(event.data);
        setNotifications(prev => [notification, ...prev].slice(0, 20));
      } catch (err) {
        console.error('Error parsing notification:', err);
      }
    });

    eventSource.addEventListener('connection', (event) => {
      console.log('SSE:', event.data);
    });

    eventSource.onerror = (err) => {
      console.error('SSE Error:', err);
      setConnected(false);
    };

    return () => {
      eventSource.close();
      setConnected(false);
    };
  }, []);

  const loadRecentNotifications = async () => {
    try {
      const data = await notificationService.getRecent(10);
      setNotifications(data);
    } catch (err) {
      console.error('Failed to load notifications:', err);
    }
  };

  const handleTrigger = async (e) => {
    e.preventDefault();
    try {
      setError(null);
      await notificationService.triggerNotification(newNotification);
    } catch (err) {
      setError('Failed to trigger notification: ' + err.message);
    }
  };

  const typeMap = ['info', 'success', 'warning', 'error'];

  return (
    <div>
      <div className="card">
        <h2>Notifications - Real-Time Events</h2>
        <p>Demonstrates Server-Sent Events (SSE) and async event handling.</p>
        <div>
          <strong>Connection Status:</strong>
          <span className={`badge ${connected ? 'badge-success' : 'badge-danger'}`}>
            {connected ? '● Connected' : '○ Disconnected'}
          </span>
        </div>
      </div>

      {error && <div className="error">{error}</div>}

      <div className="card">
        <h3>Trigger Notification</h3>
        <form onSubmit={handleTrigger}>
          <div className="form-group">
            <label className="form-label">Title</label>
            <input
              className="input"
              value={newNotification.title}
              onChange={(e) => setNewNotification({ ...newNotification, title: e.target.value })}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Message</label>
            <textarea
              className="textarea"
              value={newNotification.message}
              onChange={(e) => setNewNotification({ ...newNotification, message: e.target.value })}
              style={{ minHeight: '80px' }}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Type</label>
            <select
              className="input"
              value={newNotification.type}
              onChange={(e) => setNewNotification({ ...newNotification, type: parseInt(e.target.value) })}
            >
              <option value="0">Info</option>
              <option value="1">Success</option>
              <option value="2">Warning</option>
              <option value="3">Error</option>
            </select>
          </div>
          <button type="submit" className="button button-primary">
            Send Notification
          </button>
        </form>
      </div>

      <div className="card">
        <h3>Recent Notifications ({notifications.length})</h3>
        {notifications.length === 0 && (
          <p style={{ color: 'var(--text-secondary)' }}>No notifications yet. Trigger one above!</p>
        )}
        <div>
          {notifications.map((notif) => (
            <div key={notif.id} className={`notification-item ${typeMap[notif.type]}`}>
              <div className="notification-title">{notif.title}</div>
              <div>{notif.message}</div>
              <div className="notification-time">
                {new Date(notif.createdAt).toLocaleString()}
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="card">
        <h3>🎓 Learning Points</h3>
        <ul style={{ textAlign: 'left', marginLeft: '20px' }}>
          <li><strong>Server-Sent Events:</strong> Real-time push from server to client</li>
          <li><strong>EventSource API:</strong> Native browser support for SSE</li>
          <li><strong>Async Event Handlers:</strong> Handle events asynchronously</li>
          <li><strong>Long-Running Connections:</strong> Keeps connection alive for real-time updates</li>
        </ul>
      </div>
    </div>
  );
}

export default Notifications;
