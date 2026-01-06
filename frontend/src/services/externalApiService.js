import apiClient from './apiClient';

/**
 * External API service.
 */

export const externalApiService = {
  /**
   * Fetch from single source.
   */
  async fetchSingle(source = 'API-1', delayMs = 1000) {
    const response = await apiClient.get('/api/externalapi/fetch', {
      params: { source, delayMs }
    });
    return response.data;
  },

  /**
   * Fetch in parallel.
   */
  async fetchParallel() {
    const response = await apiClient.get('/api/externalapi/parallel');
    return response.data;
  },

  /**
   * Fetch sequentially.
   */
  async fetchSequential() {
    const response = await apiClient.get('/api/externalapi/sequential');
    return response.data;
  },

  /**
   * Fetch with race.
   */
  async fetchRace() {
    const response = await apiClient.get('/api/externalapi/race');
    return response.data;
  },

  /**
   * Fetch with retry.
   */
  async fetchWithRetry(source = 'Unreliable-API', maxRetries = 3) {
    const response = await apiClient.get('/api/externalapi/retry', {
      params: { source, maxRetries }
    });
    return response.data;
  },

  /**
   * Compare patterns.
   */
  async comparePatterns() {
    const response = await apiClient.get('/api/externalapi/compare');
    return response.data;
  },
};
