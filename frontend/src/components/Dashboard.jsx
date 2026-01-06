import React from 'react';

/**
 * Dashboard component - Overview of all learning modules.
 * 
 * This component provides an introduction to the application
 * and explains what each module teaches.
 */
function Dashboard() {
  const modules = [
    {
      title: 'Task Manager',
      description: 'Learn basic async CRUD operations with REST APIs',
      concepts: ['async/await', 'Task<T>', 'Database operations', 'HTTP status codes']
    },
    {
      title: 'File Processing',
      description: 'Understand long-running operations and progress tracking',
      concepts: ['Task.Run', 'Task.Delay', 'CancellationToken', 'Progress tracking']
    },
    {
      title: 'External API',
      description: 'Explore various async I/O patterns',
      concepts: ['Task.WhenAll', 'Task.WhenAny', 'Parallel vs Sequential', 'Retry logic']
    },
    {
      title: 'Notifications',
      description: 'Real-time event handling and Server-Sent Events',
      concepts: ['SSE', 'Long polling', 'Async events', 'Background services']
    },
    {
      title: 'Parallel Processing',
      description: 'Compare sync vs async performance',
      concepts: ['Parallel.ForEachAsync', 'Concurrency limits', 'Throttling', 'Performance metrics']
    }
  ];

  return (
    <div>
      <div className="card">
        <h2>Welcome to C# Async Learning App</h2>
        <p>
          This application demonstrates various asynchronous programming patterns in C# using ASP.NET Core.
          Each module showcases different aspects of async/await and provides hands-on examples.
        </p>
        <p>
          <strong>Learning Objectives:</strong>
        </p>
        <ul style={{ textAlign: 'left', marginLeft: '20px', marginBottom: '20px' }}>
          <li>Understand async/await fundamentals</li>
          <li>Learn Task-based Asynchronous Pattern (TAP)</li>
          <li>Master parallel and concurrent operations</li>
          <li>Implement cancellation and error handling</li>
          <li>Optimize performance with async patterns</li>
        </ul>
      </div>

      <div className="stats-grid">
        {modules.map((module, index) => (
          <div key={index} className="card" style={{ textAlign: 'left' }}>
            <h3>{module.title}</h3>
            <p>{module.description}</p>
            <div>
              <strong>Key Concepts:</strong>
              <ul style={{ marginTop: '8px' }}>
                {module.concepts.map((concept, idx) => (
                  <li key={idx}>{concept}</li>
                ))}
              </ul>
            </div>
          </div>
        ))}
      </div>

      <div className="card">
        <h3>How to Use This App</h3>
        <ol style={{ textAlign: 'left', marginLeft: '20px' }}>
          <li>Use the navigation buttons above to switch between modules</li>
          <li>Each module has interactive examples you can run</li>
          <li>Observe the async operations in action with loading states and results</li>
          <li>Check the browser console and network tab to see what's happening</li>
          <li>Review the code comments in both frontend and backend for explanations</li>
        </ol>
      </div>
    </div>
  );
}

export default Dashboard;
