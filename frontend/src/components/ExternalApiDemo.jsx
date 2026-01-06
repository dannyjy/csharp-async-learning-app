import React, { useState } from 'react';
import { externalApiService } from '../services/externalApiService';

function ExternalApiDemo() {
  const [results, setResults] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const runOperation = async (operation, operationFn) => {
    try {
      setLoading(true);
      setError(null);
      setResults(null);
      const data = await operationFn();
      setResults({ operation, data });
    } catch (err) {
      setError(`Failed to ${operation}: ` + err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <div className="card">
        <h2>External API - Async I/O Patterns</h2>
        <p>Explore Task.WhenAll, Task.WhenAny, and different async execution patterns.</p>
      </div>

      {error && <div className="error">{error}</div>}

      <div className="card">
        <h3>Try Different Patterns</h3>
        <div style={{ display: 'flex', gap: '10px', flexWrap: 'wrap' }}>
          <button
            className="button button-primary"
            onClick={() => runOperation('Sequential Fetch', externalApiService.fetchSequential)}
            disabled={loading}
          >
            Sequential (Slow)
          </button>
          <button
            className="button button-success"
            onClick={() => runOperation('Parallel Fetch', externalApiService.fetchParallel)}
            disabled={loading}
          >
            Parallel (Fast)
          </button>
          <button
            className="button button-secondary"
            onClick={() => runOperation('Race', externalApiService.fetchRace)}
            disabled={loading}
          >
            Race (First Wins)
          </button>
          <button
            className="button button-primary"
            onClick={() => runOperation('With Retry', () => externalApiService.fetchWithRetry())}
            disabled={loading}
          >
            With Retry Logic
          </button>
          <button
            className="button button-success"
            onClick={() => runOperation('Compare Patterns', externalApiService.comparePatterns)}
            disabled={loading}
          >
            Compare All
          </button>
        </div>
      </div>

      {loading && (
        <div className="card">
          <div className="loading">
            <div className="spinner"></div>
            Running async operation...
          </div>
        </div>
      )}

      {results && !loading && (
        <div className="card">
          <h3>Results: {results.operation}</h3>
          {results.data.totalTime && (
            <div style={{ marginBottom: '16px' }}>
              <strong>Total Time:</strong> {results.data.totalTime.toFixed(2)}s<br />
              <strong>Operation Type:</strong> {results.data.operationType}
            </div>
          )}

          {results.data.results && (
            <div>
              <h4>Fetched Data ({results.data.results.length} sources):</h4>
              {results.data.results.map((item, idx) => (
                <div key={idx} style={{ padding: '8px', margin: '8px 0', backgroundColor: 'var(--bg-color)', borderRadius: '4px' }}>
                  <strong>{item.source}:</strong> {item.data} (Delay: {item.delayMs}ms)
                </div>
              ))}
            </div>
          )}

          {results.data.winner && (
            <div>
              <h4>Winner:</h4>
              <div style={{ padding: '12px', backgroundColor: '#d1fae5', borderRadius: '8px' }}>
                <strong>{results.data.winner.source}</strong> completed first!<br />
                Data: {results.data.winner.data}<br />
                Delay: {results.data.winner.delayMs}ms
              </div>
            </div>
          )}

          {results.data.sequential && (
            <div className="comparison-result">
              <div className="metric-card">
                <div className="metric-title">Sequential</div>
                <div className="metric-value">{results.data.sequential.totalTimeSeconds.toFixed(2)}s</div>
                <div>{results.data.sequential.resultCount} results</div>
              </div>
              <div className="metric-card">
                <div className="metric-title">Parallel</div>
                <div className="metric-value">{results.data.parallel.totalTimeSeconds.toFixed(2)}s</div>
                <div>{results.data.parallel.resultCount} results</div>
              </div>
            </div>
          )}

          {results.data.speedupFactor && (
            <div className="success" style={{ marginTop: '16px' }}>
              <strong>Speedup Factor:</strong> {results.data.speedupFactor.toFixed(2)}x<br />
              {results.data.recommendation}
            </div>
          )}
        </div>
      )}

      <div className="card">
        <h3>🎓 Learning Points</h3>
        <ul style={{ textAlign: 'left', marginLeft: '20px' }}>
          <li><strong>Task.WhenAll:</strong> Runs multiple async operations concurrently</li>
          <li><strong>Task.WhenAny:</strong> Returns when first operation completes</li>
          <li><strong>Sequential vs Parallel:</strong> Dramatic performance differences</li>
          <li><strong>Retry Logic:</strong> Handle transient failures with exponential backoff</li>
        </ul>
      </div>
    </div>
  );
}

export default ExternalApiDemo;
