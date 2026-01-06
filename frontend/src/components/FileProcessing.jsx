import React, { useState } from 'react';
import { fileProcessingService } from '../services/fileProcessingService';

function FileProcessing() {
  const [fileName, setFileName] = useState('example-file.pdf');
  const [fileSize, setFileSize] = useState(500);
  const [jobId, setJobId] = useState(null);
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleUpload = async () => {
    try {
      setLoading(true);
      setError(null);
      const job = await fileProcessingService.uploadFile({
        fileName,
        fileSizeKb: parseInt(fileSize)
      });
      setJobId(job.id);
      setStatus(job);
      // Start polling for status
      pollStatus(job.id);
    } catch (err) {
      setError('Failed to start processing: ' + err.message);
    } finally {
      setLoading(false);
    }
  };

  const pollStatus = async (id) => {
    const interval = setInterval(async () => {
      try {
        const statusData = await fileProcessingService.getStatus(id);
        setStatus(statusData);
        
        if (statusData.status === 'Completed' || statusData.status === 'Failed' || statusData.status === 'Cancelled') {
          clearInterval(interval);
        }
      } catch (err) {
        console.error('Error polling status:', err);
        clearInterval(interval);
      }
    }, 1000);
  };

  const handleCancel = async () => {
    if (!jobId) return;
    try {
      await fileProcessingService.cancelJob(jobId);
      setStatus({ ...status, status: 'Cancelled' });
    } catch (err) {
      setError('Failed to cancel: ' + err.message);
    }
  };

  return (
    <div>
      <div className="card">
        <h2>File Processing - Long-Running Operations</h2>
        <p>Demonstrates Task.Run, progress tracking, and cancellation patterns.</p>
      </div>

      {error && <div className="error">{error}</div>}

      <div className="card">
        <h3>Upload File</h3>
        <div className="form-group">
          <label className="form-label">File Name</label>
          <input
            className="input"
            value={fileName}
            onChange={(e) => setFileName(e.target.value)}
            disabled={loading || (status && status.status === 'Processing')}
          />
        </div>
        <div className="form-group">
          <label className="form-label">File Size (KB)</label>
          <input
            className="input"
            type="number"
            value={fileSize}
            onChange={(e) => setFileSize(e.target.value)}
            min="10"
            max="5000"
            disabled={loading || (status && status.status === 'Processing')}
          />
        </div>
        <button
          className="button button-primary"
          onClick={handleUpload}
          disabled={loading || (status && status.status === 'Processing')}
        >
          Start Processing
        </button>
      </div>

      {status && (
        <div className="card">
          <h3>Processing Status</h3>
          <div style={{ marginBottom: '16px' }}>
            <strong>Job ID:</strong> {status.id}<br />
            <strong>File:</strong> {status.fileName}<br />
            <strong>Status:</strong> <span className={`badge badge-${status.status === 'Completed' ? 'success' : 'info'}`}>{status.status}</span><br />
            <strong>Elapsed Time:</strong> {status.elapsedTime ? `${status.elapsedTime.toFixed(2)}s` : 'N/A'}
          </div>

          {status.progress !== undefined && (
            <div className="progress-bar">
              <div className="progress-fill" style={{ width: `${status.progress}%` }}>
                {status.progress}%
              </div>
            </div>
          )}

          {status.result && (
            <div className="success">{status.result}</div>
          )}

          {status.errorMessage && (
            <div className="error">{status.errorMessage}</div>
          )}

          {status.status === 'Processing' && (
            <button className="button button-danger" onClick={handleCancel}>
              Cancel Processing
            </button>
          )}
        </div>
      )}

      <div className="card">
        <h3>🎓 Learning Points</h3>
        <ul style={{ textAlign: 'left', marginLeft: '20px' }}>
          <li><strong>Task.Run:</strong> Offloads CPU-bound work to thread pool</li>
          <li><strong>Progress Tracking:</strong> Incremental updates during processing</li>
          <li><strong>CancellationToken:</strong> Allows operations to be cancelled</li>
          <li><strong>Polling Pattern:</strong> Client checks status periodically</li>
        </ul>
      </div>
    </div>
  );
}

export default FileProcessing;
