Testable Services: A Pattern for Achieving Service Testability on the Component Level
==========================================================================================

Design patterns describe solutions to common, ever-recurring problems. This pattern focuses on the internal component design within a service and describes how component dependency inversion can be employed in order to achieve testability on the component and integration levels. The pattern combines known ideas introduced by Robert C. Martin (Uncle Bob) in his work on the SOLID design principles and the Clean Architecture model. In fact, the pattern is a simplified variant of Uncle Bob's Clean Architecture.


Intent
======

- Split up the service into one core component and multiple satellite components, so that each component can be tested independently.
- Utilize dependency inversion at the application boundary, in order to improve testability and make the core component independent of the context.
- Each test in the harness targets a specific component in isolation, by taking the role of the collaborating component.
- Tests excercise each component only through the component's public API, staying focused on the component level.


Motivation
==========

A service is a long-process that provides some functionality in a certain environment. When services collaborate with other services, interactions with external dependencies are hard to test sufficiently only at the unit level. Such interactions can happen when for example the service uses a DBMS, or when the service is being used via its REST API. In order to test such scenarios, we want to employ more coarse-grained tests, such as component tests and integration tests.



Applicability
=============

Usually we can interact with a service from outside through three types of externally-accessible points.

- A service has entry points in order to be started or stopped. Examples: the main() method, Start()/Stop() callbacks, signal handlers.
- A service has some input accepting units in order to accept input. Examples: a TCP server socket, HTTP request handlers, a monitor of a spool directory, a callback for accepting messages from a broker.
- A service has some output producing units in order to produce output. Examples: a HTTP response writer, a database connection, a connector to another service, a library for sending messages to a message broker.

Regardless of the specific nature of the input and output points, a test must be able to interact with these, in order to arrange, act and assert. So while every service has some known input and output points, it is not automatically testable in isolation, without requiring the presence of its external dependencies, and possibly their dependencies too.

The pattern can be used in situations, where we want to test the individual service components in isolation from each other, and where we want to minimize the need for external infrastructure or pre-existing real collaborators.


Structure
=========

TODO: diagramm mit abstrakten bezeichnern, eventuell mehrere diagramme

TODO: abgrenzen von anderen häufigen und schlechteren strukturrierungsmustern


Participants
============

TODO:

Core Component: Environment-agnostic code. High-level abstraction, domain model, business logic.

Satellite Component: Environment-specific code. Low-level implementation, protocol adapters, collaborator connectors.

Component Test: Targets the component's public API. Each test follows the typical AAAA phases introduced by Uncle Bob:
- Arrange: the component and any test resources and input data are prepared.
- Act: the component is excercised according to the test scenario.
- Assert: the result of the actions is checked.
- Annihilate: the component and all any test resources are destroyed.

The Arrange and Assert phases may be performed either directly through the component's API, or indirectly by pre-arranging some state or by checking some side-effect.

The Act and Annihilate should go through the component's public API. For example if the component manages some resources, they should be freed by calling Dispose() or close().

The Component Test comes in two variations:

Isolated: Usually targets the core component. Usually uses the component's public API for all test phases phases. Has similar properties to a unit test. Example: the target component is initialized in memory, tested and garbage collected. The test asserts by checking return values or by acting like an Observer/Listener. No TCP connections are opened.

Integrative: Usually targets a satellite component. Usually uses another mechanism for the Arrange and Assert phases. Has similar properties to an integration test.

Example-1: The target component wants to write into a database. The test uses an in-memory database in order to accept SQL inserts from the target component.

Example-2: The target component wants to accept HTTP requests. The test uses a HTTP client in order to send requests to the target component.


Consequences
============

TODO

