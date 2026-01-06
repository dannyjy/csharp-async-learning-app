import apiClient from './apiClient';

/**
 * Parallel Processing API service.
 */

export const parallelProcessingService = {
  /**
   * Process batch.
   */
  async processBatch(items, maxDegreeOfParallelism = 4) {
    const response = await apiClient.post('/api/parallel/batch', {
      items,
      maxDegreeOfParallelism
    });
    return response.data;
  },

  /**
   * Compare sync vs async performance.
   */
  async comparePerformance(itemCount = 10, parallelism = 4) {
    const response = await apiClient.get('/api/parallel/compare', {
      params: { itemCount, parallelism }
    });
    return response.data;
  },

  /**
   * Process with throttling.
   */
  async processThrottled(items, requestsPerSecond = 5) {
    const response = await apiClient.post('/api/parallel/throttled', {
      items,
      maxDegreeOfParallelism: 4
    }, {
      params: { requestsPerSecond }
    });
    return response.data;
  },
};
