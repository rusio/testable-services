Required Software for Development
=================================

- .NET Framework 4.6
- Visual Studio 2015 Community
- NuGet Package Manager
- NUnit Test Adapter


Software Architecture
=====================

The solution is aligned around Uncle Bob's Clean Architecture model.

* ApplicationCore: Application-Partition, business logic and domain objects.
* WindowsService: Main-Partition (Entry-Point) when started as a Windows Service.
* HttpConnector: UI-Partition for accepting commands over HTTP.
* SqlConnector: UI-Partition for writing events into a database.
* IntegrationTests: Examples of integration tests.
* InstallerScripts: Primitive installer scripts for running as a Windows Service.

![Solution Components](diagrams/SolutionComponents.png?raw=true)

There are following logical Boundaries:
* Boundary between the Application-Partition and the Main-Partition
* Boundary between the Application-Partition and the two UI-Partitions
