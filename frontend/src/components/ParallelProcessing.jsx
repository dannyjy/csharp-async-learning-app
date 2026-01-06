import React, { useState } from 'react';
import { parallelProcessingService } from '../services/parallelProcessingService';

function ParallelProcessing() {
  const [itemCount, setItemCount] = useState(10);
  const [parallelism, setParallelism] = useState(4);
  const [results, setResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleBatchProcess = async () => {
    try {
      setLoading(true);
      setError(null);
      const items = Array.from({ length: itemCount }, (_, i) => i + 1);
      const data = await parallelProcessingService.processBatch(items, parallelism);
      setResults({ type: 'batch', data });
    } catch (err) {
      setError('Failed to process batch: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleCompare = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await parallelProcessingService.comparePerformance(itemCount, parallelism);
      setResults({ type: 'compare', data });
    } catch (err) {
      setError('Failed to compare: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="card">
        <h2>Parallel Processing - Concurrency Patterns</h2>
        <p>Compare synchronous vs asynchronous processing with Parallel.ForEachAsync.</p>
      </div>

      {error && <div className="error">{error}</div>}

      <div className="card">
        <h3>Configuration</h3>
        <div className="grid-2">
          <div className="form-group">
            <label className="form-label">Number of Items (1-20)</label>
            <input
              className="input"
              type="number"
              value={itemCount}
              onChange={(e) => setItemCount(parseInt(e.target.value) || 1)}
              min="1"
              max="20"
              disabled={loading}
            />
          </div>
          <div className="form-group">
            <label className="form-label">Max Parallelism (1-10)</label>
            <input
              className="input"
              type="number"
              value={parallelism}
              onChange={(e) => setParallelism(parseInt(e.target.value) || 1)}
              min="1"
              max="10"
              disabled={loading}
            />
          </div>
        </div>
        <button
          className="button button-primary"
          onClick={handleBatchProcess}
          disabled={loading}
        >
          Process Batch
        </button>
        <button
          className="button button-success"
          onClick={handleCompare}
          disabled={loading}
        >
          Compare Sync vs Async
        </button>
      </div>

      {loading && (
        <div className="card">
          <div className="loading">
            <div className="spinner"></div>
            Processing...
          </div>
        </div>
      )}

      {results && !loading && results.type === 'batch' && (
        <div className="card">
          <h3>Batch Processing Results</h3>
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-value">{results.data.totalItems}</div>
              <div className="stat-label">Items Processed</div>
            </div>
            <div className="stat-card">
              <div className="stat-value">{results.data.processingTime.toFixed(2)}s</div>
              <div className="stat-label">Total Time</div>
            </div>
          </div>
          <p style={{ marginTop: '12px' }}>
            <strong>Processing Type:</strong> {results.data.processingType}
          </p>
        </div>
      )}

      {results && !loading && results.type === 'compare' && (
        <div>
          <div className="card">
            <h3>Performance Comparison</h3>
            <div className="comparison-result">
              <div className="metric-card">
                <div className="metric-title">Synchronous</div>
                <div className="metric-value">{results.data.syncResult.processingTime.toFixed(2)}s</div>
                <div>{results.data.syncResult.totalItems} items</div>
              </div>
              <div className="metric-card">
                <div className="metric-title">Asynchronous</div>
                <div className="metric-value">{results.data.asyncResult.processingTime.toFixed(2)}s</div>
                <div>{results.data.asyncResult.totalItems} items</div>
              </div>
            </div>
            <div className="success" style={{ marginTop: '16px' }}>
              <strong>Speedup Factor:</strong> {results.data.speedupFactor.toFixed(2)}x<br />
              {results.data.recommendation}
            </div>
          </div>
        </div>
      )}

      <div className="card">
        <h3>🎓 Learning Points</h3>
        <ul style={{ textAlign: 'left', marginLeft: '20px' }}>
          <li><strong>Parallel.ForEachAsync:</strong> Process collections concurrently</li>
          <li><strong>MaxDegreeOfParallelism:</strong> Control concurrency level</li>
          <li><strong>Sync vs Async:</strong> Dramatic performance differences for I/O</li>
          <li><strong>Thread Pool:</strong> Efficient reuse of threads</li>
        </ul>
      </div>
    </div>
  );
}

export default ParallelProcessing;
