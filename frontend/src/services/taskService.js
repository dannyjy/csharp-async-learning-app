import apiClient from './apiClient';

/**
 * Task API service.
 * Demonstrates async API calls in React applications.
 */

export const taskService = {
  /**
   * Get all tasks.
   * @returns {Promise} Promise resolving to array of tasks
   */
  async getAllTasks() {
    const response = await apiClient.get('/api/tasks');
    return response.data;
  },

  /**
   * Get task by ID.
   * @param {number} id - Task ID
   * @returns {Promise} Promise resolving to task object
   */
  async getTaskById(id) {
    const response = await apiClient.get(`/api/tasks/${id}`);
    return response.data;
  },

  /**
   * Create new task.
   * @param {Object} task - Task data {title, description}
   * @returns {Promise} Promise resolving to created task
   */
  async createTask(task) {
    const response = await apiClient.post('/api/tasks', task);
    return response.data;
  },

  /**
   * Update existing task.
   * @param {number} id - Task ID
   * @param {Object} updates - Updates {title?, description?, isCompleted?}
   * @returns {Promise} Promise resolving to updated task
   */
  async updateTask(id, updates) {
    const response = await apiClient.put(`/api/tasks/${id}`, updates);
    return response.data;
  },

  /**
   * Delete task.
   * @param {number} id - Task ID
   * @returns {Promise} Promise resolving when deleted
   */
  async deleteTask(id) {
    await apiClient.delete(`/api/tasks/${id}`);
  },

  /**
   * Get completed tasks.
   * @returns {Promise} Promise resolving to array of completed tasks
   */
  async getCompletedTasks() {
    const response = await apiClient.get('/api/tasks/completed');
    return response.data;
  },

  /**
   * Get pending tasks.
   * @returns {Promise} Promise resolving to array of pending tasks
   */
  async getPendingTasks() {
    const response = await apiClient.get('/api/tasks/pending');
    return response.data;
  },
};
