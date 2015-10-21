Testable Services with Inverted Component Dependencies
======================================================

Achieving Service Testability at the Component Level
----------------------------------------------------

Design patterns describe solutions to common, ever-recurring problems. This pattern focuses on the internal component within a service and describes how component dependency inversion can be employed in order to achieve a higher grade of isolation at the component level.

The pattern combines known ideas introduced by Robert C. Martin (Uncle Bob) in his work on the SOLID design principles and the Clean Architecture model. In fact, the pattern is a simplified variant of Uncle Bob's Clean Architecture model.


Intent
======

- Split-up the service into one core component and multiple satellite components, so that each component can be tested independently.
- Utilize component dependency inversion at the application boundary, in order to improve testability and make the core component agnostic of the context.
- Each test in the harness targets a specific component in isolation, by taking the role of the collaborating component.
- Tests excercise components only through the component's public API, staying focused on the component level.


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

Testing the Core Component
--------------------------

![Testing the Core Component](diagrams/Testing_the_Core_Component.png?raw=true)

Testing the Satellite Component
-------------------------------

![Testing the Satellite Component](diagrams/Testing_the_Satellite_Component.png?raw=true)


Participants
============

Core Component: Environment-agnostic logic. High-level abstraction, domain model, business logic.

Satellite Component: Environment-specific logic. Low-level implementation, protocol adapters, external connectors.

Component Test: Targets the component's public API. Each test follows the typical AAAA phases introduced by Uncle Bob:
- Arrange: the component and any test resources and input data are prepared.
- Act: the component is excercised according to the test scenario.
- Assert: the result of the actions is checked.
- Annihilate: the component and all any test resources are destroyed.

Collaborations
==============

TODO

Consequences
============

The Component Test targets a component, that is a whole group of classes. In this sense, the test is more coarse-grained than unit tests and covers longer paths. One advantage of this approach are that tests are not overly sensitive to refactorings within the component as long as the component's API does not change. A disadvantage compared to fine-grained unit tests is the lack of precise control over the smaller units.

The component test comes in two main variations:

- Tests targeting the core component. They unit test nature, and do not require any external dependencies. These tests are highly isoloated and can make use of the component's public API for all test phases. The target component is initialized in memory, tested and garbage collected. The test asserts by checking return values or by acting like an Observer/Listener. No TCP connections or other external interactions are required.

- Tests targeting one of the satellite components. They have properties similar to integration tests and may use embedded doubles of external depencies like an embedded database. These tests are integrative and may use another mechanism for the Arrange and Assert phases.


Implementation
==============

Perhaps the most notable difference of the dependency-inverted design compared to a conventional split with straight dependencies manifests itself in the location of the interfaces RequestHandler and ResponseHandler. They are both defined in the core component. This is not quite intuitive at first sight, but eliminates the need for the core component to be dependent on the satellite components, making it easily testable without them.

Testing Satellite Components
----------------------------

Example-1: The satellite component wants to write into a database. The test uses an in-memory embedded database in order to accept SQL inserts from the target component.

Example-2: The satellite component wants to accept HTTP requests. The test uses a HTTP client in order to send requests to the target component.

The Arrange and Assert phases may be performed either directly through the component's API, or indirectly by pre-arranging some state or by checking some side-effect.

The Act and Annihilate phases go through the component's public API. For example if the component manages some resources, they should be freed by calling Dispose() or close().


Sample Code
===========

TODO

Known Uses
==========

TODO

Related Patterns
================

TODO: abgrenzen von anderen häufigen nicht-invertierten Strukturmustern

TODO verlinken:

- Clean Architecture
- Hexagoal Architecture (Ports and Adapters)

Conclusion
==========

Testing at the component level is facilitated by the component-oriented structure of the service with inverted component dependencies, all pointing to the core. This allow to test the core component without any of the satellite components and without any of the external dependencies, which would otherwise be required, if we attempted to test the service as a whole. Thus, the tests are fast, isolated, reproducible, which are some good properties of unit-tests. Furthermore, it allows to test any of the satellite components integratively, without the need to launch the whole service in a production-like environment.
