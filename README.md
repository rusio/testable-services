Testable Services with Inverted Component Dependencies
======================================================

A service provides some functionality in a certain environment. But in distributed systems, a service rarely exists on its own. Usually thera are multiple services collaborating with each other and communicating via different protocols. Any service can be used by other services, or can use other services itself.

Achieving Service Testability at the Component Level
----------------------------------------------------

The key idea is to design the internal components within a service in a way, so the service becomes testable at the component level and to reduce the need for real collaborating services or a production-like environment. This is achieved by splitting up the logic into a core component and multiple peripherial components and by utilizing dependency inversion, so that coarse-grained API and integration tests can be employed on the component level and individual components be tested in isolation.

Examples of Component Design
----------------------------

![Component Design Examples](diagrams/Component_Design_Examples.png?raw=true)

The first example shows a monolythic application, where all the logic is put into a single deployable unit, including the main method as an entry point. Such internal architecture makes it hard to test the internal components in isolation from each other and might require a full-blown end-to-end test, including a setup of any collaborating services.

The second example illustrates a component-oriented design, where related logic is extracted into dedicated components. This a typical "intuitive" divide-and-conquer approach, where the main component is designated to be using and coordinating the subordinate components. Note that the main component also holds the main entry point.

The third example goes so far to split-up the logic into a core component, a main component and two infrastructure-related components. But more importantly, note the direction of the dependencies - they all point from the peripherial commponents to the core component.

The following text introduces the idea and combines known ideas introduced by Robert C. Martin (Uncle Bob) in his work on the SOLID design principles and the Clean Architecture model.


Intent
======

- Split-up the logic into a core component, and multiple peripherial components in order to test at the component level.
- Invert dependencies between the components in order to make the core component agnostic of the context in which it runs.
- Each test in the harness targets a specific component in isolation, by taking the role of the collaborating component.
- Tests excercise components only through the component's public API, staying focused at the component level.


Motivation
==========

When a services receives external requests or submits such requests itself, interactions with external dependencies are hard to test only at the unit test level. For example - the service uses a DBMS, or the service is being used via a RESTful API. This environment-specific logic is important and we want to test it. But we also want to test the core business logic inside the service.

A service has some typical units in order to interact with the external world.

- Entry points in order to be started or stopped. Examples: the main class, OnStart()/OnStop() callbacks, signal handlers.
- Input receiving units (ports). Examples: TCP server socket, HTTP request handlers, monitoring a spool directory, a message queue.
- Output producing units (adapters). Examples: a HTTP response writer, a database connection, a TCP connection to another service, a client for sending messages.

A component test must be able to interact with these units, in order to arrange, act and assert. And while these points might be well-known in advance, the service is not easily testable in isolation, without the presence of its external collaborators, and possibly their collaborators too.

Thus, in order to reduce dependencies on external infrastructure and environment, the dependencies between the components are inverted - all component dependencies point from the periphery to the core.


Structure
=========

The service code is split into one core component and one or more satellite components, so that each component can be tested in isolation and independently of the others.

The core component is fully agnostic of the environment in which it operates. Typically it will contain a domain model and business logic, but no deployment-specific logic.

The satellite components contain environment-specific logic and serve to provide input or output to the core component. Typically such logic would adapt an external protocol or connect to an external service.

Dependency inversion is achieved by placing all high-level interfaces into the core component, even though some of them might be implemented in a peripherial component.

One special satellite component is the main component, which is used to start and stop. This is also a suitable place to perform object wiring and to read static configuration.


Testing the Core Component
==========================

![Testing the Core Component](diagrams/Testing_the_Core_Component.png?raw=true)


Participants and Collaborations
-------------------------------

- Core Component - This is the Subject Under Test, which is excercised during the test.
- Satellite Component - This is the regular collaborator of the core component, which is not used in the test.
- Business Facade - This is the primary facade to the business logic inside the core component.
- Request Handler - The primary input interface of the core component for accepting commands from any clients. It is used by the satellite component and implemened in the core component.
- Response Handler - The primary output interface of the core component for emitting events to any observers. It is used by the core component and implemented in the satellite component.
- Core Component Test - The test initializes the core component and mimics the behavior of the real satellite component by invoking commands on the Request Handler and asserting on events from the Response Handler.


Testing the Satellite Component
===============================

![Testing the Satellite Component](diagrams/Testing_the_Satellite_Component.png?raw=true)


Participants and Collaborations
-------------------------------

- Satellite Component - This is the Subject Under Test, which is excercised during the test.
- Core Component - This is the regular collaborator of the satellite component, which is not used in the test.
- External Service - This is another regular collaborator of the satellite component, which might use or be used by the satellite component.
- Embedded Substitute - An in-memory substitute for the real External Service, for example a HTTP client or an embedded database.
- Satellite Component Test - The test initializes the Embedded Service and then the Satellite Component. It uses the Embedded Substitute in combination with the Request Handler or Response Handler in order to act and assert.


Consequences
============

Targeting a component in the test goes through a whole group of classes. In a way such tests are more coarse-grained than unit tests and cover longer paths. One advantage of this approach is that tests are not overly sensitive to refactorings within the component as long as the component's API remains stable. A disadvantage compared to fine-grained unit tests is the lack of precise control over the smaller units and over the exceptional paths.

A notable difference of the dependency-inverted design compared to a conventional split with straight dependencies manifests itself in the location of the interfaces RequestHandler and ResponseHandler. They are both defined in the core component. This is not quite intuitive at first sight, but eliminates the need for the core component to be dependent on the satellite components, making it easily testable without them.


Implementation
==============


All components are mapped one-to-one to physical artifacts, such as Jar files or .NET assemblies. The logical application boundary determines which code belongs to the core component and which code is placed into a satellite component.

Each test follows the typical AAAA phases introduced by Uncle Bob:
- Arrange: the component and any test resources and input data are prepared.
- Act: the component is excercised according to the test scenario.
- Assert: the result of the actions is checked.
- Annihilate: the component and all any test resources are destroyed.

Example-1: The satellite component wants to write into a database. The test uses an in-memory embedded database in order to accept SQL inserts from the target component.

Example-2: The satellite component wants to accept HTTP requests. The test uses a HTTP client in order to send requests to the target component.

The Arrange and Assert phases may be performed either directly through the component's API, or indirectly by pre-arranging some state or by checking some side-effect.

The Act and Annihilate phases go through the component's public API. For example if the component manages some resources, they should be freed by calling Dispose() or close().


When to Use
===========

The pattern can be used in situations, where we want to test the individual service components in isolation from each other, and where we want to minimize the need for external infrastructure or pre-existing real collaborators. Tests target the component level and business-logic tests are separated from environment-related tests, which are specific to the environment in which a service is deployed.

The component test comes in two main variations:

- Tests targeting the core component. They do not require any external dependencies. These tests are highly isoloated and can make use of the component's public API for all test phases. The target component is initialized in memory, tested and garbage collected. The test asserts by checking return values or by acting like an Observer/Listener. No TCP connections or other external interactions are required.

- Tests targeting one of the satellite components. They have properties similar to integration tests and may use embedded doubles of external depencies like an embedded database. These tests are integrative and may use another mechanism for the Arrange and Assert phases.


Related Patterns
================

- [Clean Architecture](https://blog.8thlight.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Ports and Adapters](http://alistair.cockburn.us/Hexagonal+architecture)


Conclusion
==========

Testing at the component level is facilitated by the component-oriented design with inverted component dependencies. This allows to test the core component without any of the satellite components and without any of the external dependencies, which would otherwise be required, if we attempted to test the service as a whole. Furthermore, it allows to test any of the satellite components integratively, without the need to launch the whole service in a production-like environment.
