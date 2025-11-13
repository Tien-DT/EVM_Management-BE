# Development Guide

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server (or compatible database)
- Visual Studio 2022 or VS Code

### Setup Instructions
1. Clone the repository
2. Restore NuGet packages
3. Configure connection string in appsettings.json
4. Run database migrations
5. Start the application

## Project Structure

### EVMManagement.API
Contains the API controllers and configuration.

### EVMManagement.BLL
Business logic layer with services and DTOs.

### EVMManagement.DAL
Data access layer with repositories and entities.

## Development Workflow

1. Create feature branch
2. Make changes
3. Test locally
4. Commit changes
5. Push and create pull request

## Notes
- Follow coding standards defined in .editorconfig
- Write unit tests for new features
- Update documentation as needed

