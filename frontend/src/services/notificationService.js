import apiClient from './apiClient';

/**
 * Notification API service.
 */

export const notificationService = {
  /**
   * Trigger a notification.
   */
  async triggerNotification(notification) {
    const response = await apiClient.post('/api/notifications/trigger', notification);
    return response.data;
  },

  /**
   * Get recent notifications.
   */
  async getRecent(count = 10) {
    const response = await apiClient.get('/api/notifications/recent', {
      params: { count }
    });
    return response.data;
  },

  /**
   * Create EventSource for Server-Sent Events.
   * Note: SSE uses native EventSource API, not axios.
   */
  createEventSource() {
    const baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000';
    return new EventSource(`${baseURL}/api/notifications/stream`);
  },
};
