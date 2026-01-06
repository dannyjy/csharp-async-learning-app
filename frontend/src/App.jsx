import { useState } from 'react';
import './App.css';
import Dashboard from './components/Dashboard';
import TaskManager from './components/TaskManager';
import FileProcessing from './components/FileProcessing';
import ExternalApiDemo from './components/ExternalApiDemo';
import Notifications from './components/Notifications';
import ParallelProcessing from './components/ParallelProcessing';

/**
 * Main App Component
 * 
 * This is the root component that manages navigation between different
 * learning modules. Each module demonstrates different async patterns.
 */
function App() {
  const [activeModule, setActiveModule] = useState('dashboard');

  const modules = {
    dashboard: { name: 'Dashboard', component: Dashboard },
    tasks: { name: 'Task Manager', component: TaskManager },
    fileProcessing: { name: 'File Processing', component: FileProcessing },
    externalApi: { name: 'External API', component: ExternalApiDemo },
    notifications: { name: 'Notifications', component: Notifications },
    parallel: { name: 'Parallel Processing', component: ParallelProcessing },
  };

  const ActiveComponent = modules[activeModule].component;

  return (
    <div className="app">
      <div className="header">
        <h1>C# Async Learning App</h1>
        <p>Master asynchronous programming patterns in C# and ASP.NET Core</p>
      </div>

      <nav className="navigation">
        {Object.entries(modules).map(([key, { name }]) => (
          <button
            key={key}
            className={`nav-button ${activeModule === key ? 'active' : ''}`}
            onClick={() => setActiveModule(key)}
          >
            {name}
          </button>
        ))}
      </nav>

      <main>
        <ActiveComponent />
      </main>

      <footer style={{ textAlign: 'center', padding: '20px', color: 'var(--text-secondary)' }}>
        <p>Built with ASP.NET Core 10.0 and React 18 • Educational Purpose</p>
      </footer>
    </div>
  );
}

export default App;
