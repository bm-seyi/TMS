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




