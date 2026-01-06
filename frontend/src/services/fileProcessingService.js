import apiClient from './apiClient';

/**
 * File Processing API service.
 */

export const fileProcessingService = {
  /**
   * Upload file and start processing.
   * @param {Object} fileData - {fileName, fileSizeKb}
   * @returns {Promise} Promise resolving to job info
   */
  async uploadFile(fileData) {
    const response = await apiClient.post('/api/fileprocessing/upload', fileData);
    return response.data;
  },

  /**
   * Get processing status.
   * @param {string} jobId - Job ID
   * @returns {Promise} Promise resolving to status info
   */
  async getStatus(jobId) {
    const response = await apiClient.get(`/api/fileprocessing/status/${jobId}`);
    return response.data;
  },

  /**
   * Get processing result.
   * @param {string} jobId - Job ID
   * @returns {Promise} Promise resolving to result
   */
  async getResult(jobId) {
    const response = await apiClient.get(`/api/fileprocessing/result/${jobId}`);
    return response.data;
  },

  /**
   * Cancel processing job.
   * @param {string} jobId - Job ID
   * @returns {Promise} Promise resolving when cancelled
   */
  async cancelJob(jobId) {
    await apiClient.post(`/api/fileprocessing/cancel/${jobId}`);
  },
};
