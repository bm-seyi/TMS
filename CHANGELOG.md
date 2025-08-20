## 0.0.1
First Development Version

## 0.0.2
### Added
- Implemented the [LinesHub](./API/Hubs/LinesHub.cs) to handle real-time notifications from the lines table in the database.
- Introduced the [HubsBackgroundService](./API/BackgroundServices/HubsBackgroundServices.cs) to automatically push data updates upon database changes.
- Developed the [LinesListeners](./API/Listeners/LinesListeners.cs) to track and detect changes within the database.
- Added the [DatabaseActions](./API/Utilties/DatabaseActions.cs) utility class for performing database operations.
- Integrated `DotNetEnv` into the project, enabling dynamic configuration loading based on the project's runtime mode. The .env variables are automatically loaded when running in debug mode.

### Fixed/Updated
- Renamed the API project folder from `TMS_API` to `API` and updated the [solution](TMS_API.sln) accordingly to reflect the new structure.

### Notes
Future commits will focus on enhancing functionality and writing unit tests for improved coverage and stability.

## 0.0.3
### Added
- Implemented a [dependency-review workflow](.github/workflows/dependency-review.yml) to ensure the project uses secure dependencies. 
- Developed [HubBackgroundServiceTests](./Tests/BackgroundServices/HubBackgroundServiceTests.cs) to provide unit test coverage for [HubBackgroundService](./API/BackgroundServices/HubsBackgroundServices.cs).
- Introduced [SqlDependencyManager](./API/Utilities/SqlDependencyManager.cs), a utility class created to decouple `HubBackgroundService` from direct interaction with `SqlDependency` and provide an abstraction layer, enabling unit testing of `HubBackgroundService`.
- Created [unit-tests](.github/workflows/unit-tests.yml) to automatically test the code on pull requests to the `master` branch.

### Fixed/Updated
- Updated [Program](./API/Program.cs) to register `ISqlDependencyManager` within the dependency injection container, enhancing modularity and testability.
- Refactored [HubBackgroundService](./API/BackgroundServices/HubsBackgroundServices.cs) to utilize `ISqlDependencyManager`. This change replaces direct use of `SqlDependency` (which required a live database connection) with an abstraction, improving testability and maintainability.
-Modified [TMS_API](TMS_API.sln) to support the addition of a new [Test](./Tests/Tests.csproj) project, which uses the `MsTest` framework for unit testing.


## 0.0.4
### Added
- Introduced a new unit test to verify the functionality of the `ExecuteAsync` method's catch block, ensuring exceptions are handled correctly.  
- Added a `Cleanup` method in [HubBackgroundServiceTests](./Tests/BackgroundServices/HubBackgroundServiceTests.cs) to reset initialized resources and improve test reliability.

### Fixed/Changed
- Updated `ExecuteAsync_Should_StartAndStopDependencies` to validate that `_mockSqlDependencyManager.Start` and `_mockSqlDependencyManager.Stop` are invoked, replacing the previous approach of checking specific text within the method.

## 0.0.5
### Added
- Integrated an `IDbConnection` registry into the dependency injection container in [Program](./API/Program.cs) to enhance database connection management.
- Added comprehensive unit testing for [LinesListener](./API/Listeners/LinesListeners.cs) with the creation of [LinesListenerTests](./Tests/Listeners/LinesListenersTests.cs).

### Fixed/Updated
- Refactored `StopListening` method to its asynchronous counterpart, `StopListeningAsync`, in [HubBackgroundServiceTests](./Tests/BackgroundServices/HubBackgroundServiceTests.cs).
- Enhanced [LinesListener](./API/Listeners/LinesListeners.cs) by adopting dependency injection for managing both the database connection and `SqlDependency` instance.
- Improved the [SqlDependencyManager](./API/Utilties/SqlDependencyManager.cs) to support initializing new instances of `SqlDependency`.

### Notes
- Several tests in [LinesListenerTests](./Tests/Listeners/LinesListenersTests.cs) have been temporarily commented out due to limitations in Moq handling explicit conversions used in [LinesListener](./API/Listeners/LinesListeners.cs).


## 0.1.0
### Added
- New `LinesBackgroundService` to replace the old `HubsBackgroundService`.
- New `DatabaseConnection` interface and implementation for managing database connections.
- New `LinesModel` class for strongly-typed line data.
- New test files:
  - `LinesBackgroundServiceTests.cs` for testing the new background service
  - `DatabaseActionsTests.cs` for testing database actions

### Fixed/Updated
- Refactored `DatabaseActions` to use new `DatabaseConnection` and implement queue message processing.
- Simplified `LinesHub` by removing unused `sendData` method.
- Updated `Program` to use new background service and remove old dependencies.

### Removed
- Removed `HubsBackgroundService` and related tests.
- Removed `LinesListener` implementation and related tests.
- Removed `SqlDependencyManager` and all SQL dependency-based functionality.
- Removed unused interfaces `ILinesListener` and `ISqlDependencyManager`.

## 0.1.1
### Added
- Added comprehensive unit tests for `LinesBackgroundService` covering various scenarios:
  - Processing `LinesModel` data and sending to clients
  - Processing deleted lines IDs and sending to clients
  - Handling null data from queue
  - Handling unknown data types from queue
  - Operation cancellation scenarios
  - Exception handling scenarios

### Fixed/Updated
- Updated `LinesBackgroundService` to log a warning message when stopping due to cancellation
- Improved test coverage and verification of logging behavior in background service

## 0.1.2
### Added
- Added `OnConnectedAsync` method to `LinesHub` that sends lines data to newly connected clients
- Added `IDatabaseActions` dependency to `LinesHub` constructor
- Added `ExcludeFromCodeCoverage` attribute to `DatabaseActions` and `DatabaseConnection` classes
- Added namespace `TMS_API.Utilities` to `DatabaseConnection` class
- Added comprehensive unit tests for `LinesHub` including:
  - Constructor null argument validation
  - `OnConnectedAsync` behavior verification
  - Client connection logging verification
  - Data transmission verification

### Fixed/Updated
- Updated `DatabaseActions` to use async versions of `IsDBNull` methods
- Updated `ProcessQueueMessageAsync` to explicitly open database connection
- Improved null checks in `DatabaseConnection` constructor
- Moved `DatabaseConnection` interface and class into `TMS_API.Utilities` namespace
- Updated database reader operations to be fully asynchronous

## 0.1.3
### Added
- Generic method `RetrieveModelAsync<T>` in `DatabaseActions` to replace specific data retrieval methods

### Fixed/Updated
- Updated `LinesHub.cs` to use the new generic `RetrieveModelAsync` method instead of the specific `RetrieveLinesDataAsync`
- Simplified lines data retrieval query in `LinesHub.cs`
- Renamed folder from `API/Utilties/` to `API/Utilities/` (corrected spelling)
- Updated `LinesHubTests.cs` to mock and test the new generic `RetrieveModelAsync` method instead of `RetrieveLinesDataAsync`

### Removed
- Removed specific `RetrieveLinesDataAsync` method in favor of generic solution
- Removed all queue message processing functionality previously in `DatabaseActions`


## 0.1.4
### Added
- Added support for stored procedure execution in `RetrieveModelAsync`
- Added `LinesQueueModel` for queue message processing
- Added nullable type handling in database value conversion
- Added early exit when no queue data is available

### Fixed/Updated
- Refactored `LinesBackgroundService` to use simplified queue processing
- Updated `DatabaseActions` to handle both queries and stored procedures
- Improved property mapping with proper null handling
- Simplified hub data retrieval logic
- Cleaned up model property formatting
- Updated tests to match new service behavior
- Improved logging messages for queue processing

### Removed
- Removed complex queue message processing logic from `DatabaseActions`
- Removed separate handling for different message types (delete/update)
- Removed redundant test cases for obsolete functionality
- Removed XML parsing dependencies

## 0.1.5
### Added
- Added Docker Compose configuration (`docker-compose.yml`) with:
  - Custom network configuration (`tms_network`)
  - Environment variable support
  - Port mapping (8080:80)
  - Explicit container naming
- Added parent directory traversal for `.env` file loading
- Added HTTPS (443) port exposure in Dockerfile
- Added explicit environment variable configuration in Docker Compose

### Fixed/Updated
- Updated Dockerfile to:
  - Correct project name references (`TMS_IDP` → `TMS_API`)
  - Simplify project structure (removed test project references)
  - Standardize port configuration
- Updated `.env` file loading to handle parent directory location
- Updated container entrypoint to match correct DLL name
- Improved environment variable handling between Compose and app

### Removed
- Removed test project references from Docker build process
- Removed redundant port exposure (kept only 80/443)

## 0.1.6
### Added
- Added `contents: read` permissions to GitHub workflows for:
  - Dependency Review workflow
  - Unit Tests workflow

## 0.2.0
### Added
- Redis support with StackExchange.Redis integration for SignalR backplane
- JWT Bearer authentication with comprehensive configuration options
- New configuration models (`RedisOptions`, `JwtOptions`) for structured app settings
- SignalR-specific authorization policies (`signalR.Read`, `signalR.Write`)
- Support for multiple appsettings files (environment-specific)
- Token extraction from both Authorization header and query string for WebSocket connections
- Enhanced Redis configuration with extensive connection options
- .NET 9.0 target framework support
- Additional SignalR Redis package dependency

### Fixed/Updated
- Upgraded from .NET 8.0 to .NET 9.0
- Updated package versions:
  - Microsoft.AspNetCore.Authentication.JwtBearer (8.0.11 → 9.0.5)
  - Microsoft.Data.SqlClient (5.2.2 → 6.0.2)
  - Swashbuckle.AspNetCore (6.6.2 → 8.1.1)
- Enhanced JWT authentication configuration with proper realm settings
- Improved token validation parameters with configurable clock skew
- Refactored authentication scheme configuration to use JwtBearerDefaults
- Updated appsettings.json with comprehensive Redis and JWT configuration sections
- Removed assembly info generation attributes from project file
- Improved error details inclusion in development environment

### Removed
- HISTORY.MD file (replaced with changelog.md)
- Old API scope policy ("ApiScope") in favor of SignalR-specific policies
- Hardcoded JWT authority and validation parameters
- Direct SQL connection string reference in Program.cs
- .NET 8.0 target framework support