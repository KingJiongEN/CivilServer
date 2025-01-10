# CivilServer

A .NET 6.0 game server implementation with WebSocket support and various game-related functionalities.

## Overview

This project is a game server built with .NET 6.0 that provides backend services for a city/simulation game. It includes features for handling game logic, WebSocket communication, data persistence, and AI/LLM integration.

## Prerequisites

- .NET 6.0 SDK
- MySQL Server
- Redis Server
- Python (for pythonnet integration)

## Dependencies

The server uses several key packages:

- Microsoft.Extensions.Hosting (7.0.0)
- Microsoft.Extensions.Hosting.Systemd (7.0.0)
- MySql.Data (8.3.0)
- Newtonsoft.Json (13.0.3)
- pythonnet (3.0.3)
- StackExchange.Redis (2.7.33)
- YamlDotNet (15.1.2)

## Project Structure

- `/src/CityCommon/` - Core game logic and common functionality
  - `/Archvie/` - Archive management
  - `/Data/` - Data models and configuration
  - `/Msg/` - Message handling and protocols
  - `/NavCity/` - Navigation and pathfinding
  - `/Server/` - Server implementation
  
## Features

- WebSocket-based real-time communication
- City simulation and game state management
- Pathfinding and navigation systems
- User data persistence
- Message handling system
- LLM/AI integration
- Configuration management

## Building and Running

To build the project:

```bash
dotnet build
```

To run the project:

```bash
dotnet run
```

For development with hot reload:

```bash
dotnet watch run
```

## Configuration

The server uses the following configuration files:
- `appsettings.json` - Main configuration file
- `appsettings.Development.json` - Development environment settings

Key configuration settings include:
- Database connection strings
- Redis configuration
- WebSocket endpoints
- Logging settings

## Development

### VS Code Setup

The project includes VS Code configuration files for debugging and tasks. To get started:

1. Install the C# extension for VS Code
2. Open the project in VS Code
3. Use the included launch configurations for debugging

### Debugging

Launch configurations are available in `.vscode/launch.json` for:
- Debug with hot reload
- Attach to process
- Launch without debugging

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

[MIT License](LICENSE)

## Support

For support, please open an issue in the GitHub repository.