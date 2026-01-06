# C# Async Learning Application

A comprehensive teaching application for learning C# asynchronous operations using ASP.NET Core backend and React frontend.

## 🎯 Overview

This application provides hands-on examples of asynchronous programming patterns in C#, demonstrating real-world scenarios through interactive modules. Each module focuses on specific async concepts with detailed explanations and working code examples.

## 📚 Learning Modules

### 1. **Task Manager** - Basic Async CRUD
- Async database operations with Entity Framework Core
- Proper use of `async`/`await` in controllers
- `Task<T>` return types
- HTTP status codes and REST patterns

### 2. **File Processing** - Long-Running Operations
- `Task.Run` for CPU-bound operations
- Progress tracking and status updates
- `CancellationToken` support
- Polling patterns for job status

### 3. **External API** - Async I/O Operations
- `Task.WhenAll` for parallel operations
- `Task.WhenAny` for racing operations
- Sequential vs parallel performance comparison
- Retry logic with exponential backoff

### 4. **Notifications** - Real-Time Events
- Server-Sent Events (SSE)
- Long polling patterns
- Async event handling
- Background processing

### 5. **Parallel Processing** - Concurrency Patterns
- `Parallel.ForEachAsync` for concurrent processing
- Sync vs async performance comparison
- Concurrency limits and throttling
- Performance metrics and monitoring

## 🚀 Getting Started

### Prerequisites

- **.NET SDK 10.0 or higher** - [Download](https://dotnet.microsoft.com/download)
- **Node.js 18.0 or higher** - [Download](https://nodejs.org/)
- **npm** (comes with Node.js)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/dannyjy/csharp-async-learning-app.git
   cd csharp-async-learning-app
   ```

2. **Set up the Backend**
   ```bash
   cd src/AsyncLearningApp.Api
   dotnet restore
   dotnet build
   ```

3. **Set up the Frontend**
   ```bash
   cd ../../frontend
   npm install
   ```

### Running the Application

#### Start the Backend

```bash
cd src/AsyncLearningApp.Api
dotnet run
```

The API will be available at `http://localhost:5000`
Swagger documentation at `http://localhost:5000/swagger`

#### Start the Frontend

In a new terminal:

```bash
cd frontend
npm run dev
```

The React app will be available at `http://localhost:5173`

## 🏗️ Architecture

### Backend (ASP.NET Core)

```
src/AsyncLearningApp.Api/
├── Controllers/           # API controllers demonstrating async patterns
│   ├── TasksController.cs
│   ├── FileProcessingController.cs
│   ├── ExternalApiController.cs
│   ├── NotificationsController.cs
│   └── ParallelProcessingController.cs
├── Models/               # Data models and DTOs
├── Services/             # Business logic services
├── Data/                 # Database context
└── Program.cs            # Application configuration
```

### Frontend (React)

```
frontend/
├── src/
│   ├── components/       # React components for each module
│   ├── services/         # API client services
│   ├── App.jsx          # Main application component
│   └── App.css          # Styling
└── package.json
```

## 🎓 Key Concepts Demonstrated

### Async/Await Fundamentals
- When to use `async`/`await`
- Difference between synchronous and asynchronous code
- `Task<T>` and `Task` return types
- ConfigureAwait considerations

### Task-Based Asynchronous Pattern (TAP)
- Creating and consuming async methods
- Task composition with `Task.WhenAll` and `Task.WhenAny`
- Task continuation and chaining
- Exception handling in async code

### Cancellation
- `CancellationToken` usage
- Graceful cancellation of operations
- Cooperative cancellation patterns

### Parallel and Concurrent Operations
- `Parallel.ForEachAsync` for concurrent processing
- Thread pool usage
- Throttling and rate limiting
- Performance considerations

### Real-Time Communication
- Server-Sent Events (SSE)
- Long polling
- EventSource API in JavaScript

## 🔍 API Endpoints

### Tasks API
- `GET /api/tasks` - Get all tasks
- `GET /api/tasks/{id}` - Get task by ID
- `POST /api/tasks` - Create new task
- `PUT /api/tasks/{id}` - Update task
- `DELETE /api/tasks/{id}` - Delete task

### File Processing API
- `POST /api/fileprocessing/upload` - Start processing
- `GET /api/fileprocessing/status/{id}` - Check status
- `GET /api/fileprocessing/result/{id}` - Get results
- `POST /api/fileprocessing/cancel/{id}` - Cancel job

### External API
- `GET /api/externalapi/parallel` - Parallel fetch
- `GET /api/externalapi/sequential` - Sequential fetch
- `GET /api/externalapi/race` - Race fetch
- `GET /api/externalapi/compare` - Compare patterns

### Notifications API
- `GET /api/notifications/stream` - SSE stream
- `POST /api/notifications/trigger` - Create notification
- `GET /api/notifications/recent` - Get recent

### Parallel Processing API
- `POST /api/parallel/batch` - Process batch
- `GET /api/parallel/compare` - Compare sync vs async

## 📖 Learning Resources

- [Microsoft Docs: Async/Await](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/)
- [Task-based Asynchronous Pattern](https://docs.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [Async Best Practices](https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md)

## 🤝 Contributing

This is an educational project. Feel free to submit issues or pull requests with improvements or additional examples.

## 📄 License

This project is for educational purposes.

## 🎯 Next Steps

After exploring this application:

1. Read the detailed `LEARNING_GUIDE.md` for in-depth explanations
2. Experiment with modifying the code
3. Try creating your own async endpoints
4. Explore the code comments for detailed explanations
5. Use the browser developer tools to observe network requests

## 💡 Tips

- Open browser DevTools to see network requests in action
- Check the console for logged information
- Observe how async operations don't block the UI
- Compare execution times between different patterns
- Try cancelling long-running operations

## 🐛 Troubleshooting

**Backend won't start:**
- Ensure .NET 10.0 SDK is installed: `dotnet --version`
- Check if port 5000 is available
- Run `dotnet restore` to restore packages

**Frontend won't start:**
- Ensure Node.js is installed: `node --version`
- Delete `node_modules` and run `npm install` again
- Check if port 5173 is available

**CORS errors:**
- Ensure backend is running on port 5000
- Check CORS configuration in `Program.cs`
- Verify `VITE_API_BASE_URL` in frontend `.env`

## 📞 Support

For questions or issues, please open an issue on GitHub.