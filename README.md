Testable Services with Inverted Component Dependencies
======================================================

A service provides some functionality in a certain environment. It can be implemented in any programming language and is usually deployed as a long running OS-level process.

But in a distributed system, a service rarely exists on its own. Usually there are multiple services collaborating with each other and communicating via different protocols. The services might be wildly interconnected and dependent on each other in order to function.

Similar to testing small units like classes, testing whole services faces the same difficulties when it comes to dependencies: A services receives and submits requests from and to other services, which must be up and running.

The following text is inspired by ideas introduced by Robert C. Martin (Uncle Bob) in his work on the [SOLID design principles](http://butunclebob.com/ArticleS.UncleBob.PrinciplesOfOod) and the [Clean Architecture](https://blog.8thlight.com/uncle-bob/2012/08/13/the-clean-architecture.html) model.


Testing Objectives - What to Test?
==================================

There are three main objectives when testing a service:

- Test the core business logic inside the service.
- Test interactions between the service and its collaborators.
- Test the integration of the service with the OS.


Testing Possibilities - What is Exposed?
========================================

A service has some typical interfaces to the external world:

- A main entry point for starting and stopping. Examples: the `main()` class, `OnStart()/OnStop()` callbacks, signal handlers.
- Input ports. Examples: TCP server socket, HTTP request handlers, monitoring a spool directory, a message queue.
- Output adapters. Examples: a HTTP response writer, a database connection, a TCP connection to another service, a client for sending messages.

A test must be able to interact with these interfaces, in order to arrange, act and assert. And while these interfaces might be well-known in advance, services are often not easily testable in isolation.


Achieving Testability at the Component Level
============================================

The key idea is to design the internal components within a service in such a way, that the service becomes testable at component level.

This is achieved by splitting up the logic into a core component and multiple peripheral components and by utilizing dependency inversion.

Coarse-grained API and integration tests target individual components, and test them in isolation, thus reducing the need for real collaborating services or a production-like environment.

Examples of Component Design
----------------------------

Monolythic: all the code is located in a single component. Dependencies do not cross components.

![Component Design Example 1](diagrams/Component_Design_Example_1.png?raw=true)

In a monolithic application, all the logic is put into a single deployable unit. Such internal architecture makes it hard to test the internal components in isolation and might require a full-blown end-to-end test, including setup of any collaborating services.

Cohesive: components hold related code. Root component uses subordnate components.

![Component Design Example 2](diagrams/Component_Design_Example_2.png?raw=true)

An typical divide-and-conquer component design, where related logic is extracted into dedicated components. The main component is designated to coordinate the subordinate components. Note that the dependencies follow the "intuitive" top-down direction and the root component also holds the main entry point.

Inverted: components hold related code. But component dependencies point bottom-up into the core component.

![Component Design Example 3](diagrams/Component_Design_Example_3.png?raw=true)

Takes the second approach further and extracts the main entry point in a dedicated component for object wiring and configuration. More importantly, component dependencies are inverted: peripheral components depend on the core component, making the core component environment-agnostic.


Intent
======

- Split-up the logic into one core component, and multiple peripheral components in order to test at the component level.
- Invert dependencies between components in order to make the core component agnostic of the context it runs in.
- Each test in the harness targets a specific component in isolation, by taking the role of the collaborating components.
- Each test exercises a single component, using only the component's public API (in contrast to unit tests, which may access non-public API).


Structure
=========

The core component is fully agnostic of the environment in which it operates. It contains a domain model and business logic, but no deployment-specific logic.

Satellite components contain environment-specific logic and serve to provide input and output to the core component. Typically such logic would adapt an external protocol or connect to an external service.

Dependency inversion is achieved by placing high-level interfaces into the core component, even though some of them might be implemented in a peripheral component.

One special satellite component is the main component, which is used to start and stop. This is also a suitable place to perform object wiring and to read static configuration.


Testing Business Logic
======================

The test targets the Core Component through its API and fakes the external interactions.

![Testing the Core Component](diagrams/Testing_the_Core_Component.png?raw=true)


Participants
------------

- Core Component - This is the Subject Under Test, which is exercised during the test.
- Satellite Component - A the regular collaborator of the core component, which is not used in the test.
- Business Logic - Business rules, domain model, interactors.
- CommandHandler - The primary input interface of the core component for accepting commands from any client. It is used by the satellite component and implemented in the core component.
- EventHandler - The primary output interface of the core component for emitting events to any observers. It is used by the core component and implemented in the satellite component.
- Core Component Test - Initializes the core component and mimics the behavior of the real satellite component by invoking commands on the CommandHandler and asserting on events from the EventHandler.


Testing External Interactions
=============================

The test targets a Satellite Component through its external protocol and fakes the business logic.

![Testing the Satellite Component](diagrams/Testing_the_Satellite_Component.png?raw=true)


Participants and Collaborations
-------------------------------

- Satellite Component - This is the Subject Under Test, which is exercised during the test.
- Core Component - The regular collaborator of the satellite component, which is not used in the test.
- External Service - Another regular collaborator of the satellite component, which might use or be used by the satellite component.
- Embedded Substitute - An in-memory substitute for the real External Service, for example an embedded database.
- Satellite Component Test - The test initializes the Embedded Substitute and then the Satellite Component. It uses the Embedded Substitute in combination with the CommandHandler or EventHandler in order to act and assert.


Consequences
============

Targeting a component in the test goes through a whole group of classes. In a way such tests are more coarse-grained than unit tests and cover longer paths. One advantage of this approach is that tests are not overly sensitive to refactorings within the component as long as the component's API remains stable. A disadvantage compared to fine-grained unit tests is the lack of precise control over the smaller units and over the exceptional paths.

An important difference dependency inversion compared to a conventional split with straight dependencies manifests itself in the location of the interfaces CommandHandler and EventHandler. They are both defined and deployed with the core component. This is not quite intuitive at first sight, but it is the key factor to make the core component independent of the satellite components, making it testable without them.


Implementation
==============

All components are mapped one-to-one to physical artifacts, such as Jar files or .NET assemblies. The logical application boundary determines which code belongs to the core component and which code is placed into a satellite component.

Each test follows the typical AAAA phases introduced by Uncle Bob:
- Arrange: the component and any test resources and input data are prepared.
- Act: the component is exercised according to the test scenario.
- Assert: the result of the actions is checked.
- Annihilate: the component and all any test resources are destroyed.

Example-1: The satellite component wants to write into a database. The test uses an in-memory embedded database in order to accept SQL inserts from the target component.

Example-2: The satellite component wants to accept HTTP requests. The test uses an embedded HTTP client in order to send requests to the target component.

The Arrange and Assert phases may be performed either directly through the component's API, or indirectly by pre-arranging some state or by checking some side-effect.

The Act and Annihilate phases go through the component's public API. For example if the component manages some resources, they should be freed by calling Dispose() or close().


When to Use
===========

The pattern can be used in situations, where we want to test the individual service components in isolation from each other, and where we want to minimize the need for external infrastructure or pre-existing real collaborators. Tests target the component level and business-logic tests are separated from environment-related tests, which are specific to the environment in which a service is deployed.

The component test comes in two main variations:

- Tests targeting the core component. They do not require any external dependencies. These tests are highly isoloated and can make use of the component's public API for all test phases. The target component is initialized in memory, tested and garbage collected. The test asserts by checking return values or by acting like an Observer/Listener. No TCP connections or other external interactions are required.

- Tests targeting one of the satellite components. They have properties similar to integration tests and may use embedded doubles of external dependencies like an embedded database. These tests are integrative and may use another mechanism for the Arrange and Assert phases.


Conclusion
==========

Testing at the component level is facilitated by the component-oriented design with inverted component dependencies. This allows to test the core component without any of the satellite components and without any of the external dependencies, which would otherwise be required, if we attempted to test the service as a whole. Furthermore, it allows integrative testing of any peripheral component, without the need to launch the whole service in a production-like environment.
