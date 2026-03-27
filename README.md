# .NET Microservices Take-Home Assignment

## Overview
This project is a small microservices-based Order Processing System built in **.NET**, using **event-driven architecture**. 

In this system, a client is able to initiate an "order creation", which triggers payment processing and notification delivery. This is achieved with 3 separate microservices: 
- `OrderService` for order creation
- `PaymentService` for payment processing
- `NotificationService` for sending notifications. 

Because of the architecture, services are independently deployable, and communicate asynchronously via events. I specifically avoided direct API calls between services, to avoid coupling.

The tech stack for this project includes:
- `C#/.NET 9` for the services
- `xUnit` + `Moq` + `FluentAssertions` for Unit tests. 
- `RabbitMQ` for the message broker
- `Serilog` with `Seq` for structured logging
- `YARP` for the API Gateway
- `Docker` to containerize each application
- `docker-compose` to orchestrate deployment

Throughout this assignment, I prioritised clean layering, clear service boundaries, and maintainable code, with responsibilities separated across API, application, domain, and infrastructure layers. 

On top of that, I added safety rails on the service level to accommodate for the event of network failures such as retries, idempotency and the outbox pattern - more details in [Design Decisions](#design-decisions). I prioritised these features over additional "production hardening" features such as authentication middleware, rate limiting, and including integration tests.

The rest of this README consists of a quick start guide, service URLs, event flow, design decisions, testing, limitations and future improvements.

## Architecture Diagram

![Architecture diagram](ArchitectureDiagram.png)

## Quick Start
The easiest way to get this project running is using Docker. Please ensure you have it installed: [Docker Engine](https://docs.docker.com/engine/install/) / [Docker Desktop](https://docs.docker.com/desktop/). Run all steps from the repo root.

1. Run `docker compose up --build` to build the images and run the containers. If it's your first time running it, it may take a while to pull all the relevant images.
2. Run the following health check scripts to check all the containers and services are running and healthy:

    Linux/macOS/Git Bash:
    ```
    bash scripts/health-check.sh
    ```
    Windows PowerShell:
    ```
    .\scripts\health-check.ps1
    ```
    Expected result: all checks print `PASS`.

    Alternatively, check container status via Docker Desktop and ping `localhost:5000/health` (also check ports `5211`, `5104` and `5030`).

3. Access the services through the API gateway (default is `localhost:5000`)
4. A Postman collection is provided in `OrderProcessingSystem.postman_collection.json`, which can be imported directly into Postman to test the endpoints. 
5. If you have a dotnet SDK installed, you can run `dotnet test` in the root of the project to run the unit tests.

### Exiting containers
To stop running the Order Processing System, you can run `docker compose down`, or `docker compose down -v` to remove volumes.

## Service URLs
The available services can be accessed at the following URLs:

- RabbitMQ UI:
    - `localhost:15672/` (username: guest, password: guest)
- Seq:
    - `localhost:8081/` (username: admin, password: AdminPassword1!)
    - Note: you may need to update the password on first init
- API Gateway:
    - `localhost:5000`
- OrderService:
    - `localhost:5000/api/orders`: through API gateway
    - `localhost:5211/api/orders`: directly
    - `localhost:5211/scalar/v1`: Scalar OpenAPI documentation
- PaymentService:
    - `localhost:5000/api/payments`: through API gateway
    - `localhost:5104/api/payments`: directly
    - `localhost:5104/scalar/v1`: Scalar OpenAPI documentation
- NotificationService:
    - `localhost:5000/api/notifications`: through API gateway
    - `localhost:5030/api/notifications`: directly
    - `localhost:5030/scalar/v1`: Scalar OpenAPI documentation

## Event Flow
![Event Flow Diagram](EventFlowDiagram.png)

### Flow
1. When a client sends a POST Order request (create new order) through the API gateway, the reverse proxy will route it to the OrderService.
2. The OrderService will store the Order in the OrderDb, and publish an event `OrderCreatedEvent` to the message broker, in this case RabbitMQ.
3. The PaymentService listens for `OrderCreatedEvent`, and when it receives one it will simulate making a payment. Then, it will publish a `PaymentSucceededEvent` to RabbitMQ and store the payment in the PaymentDb.
4. The NotificationService listens for `PaymentSucceededEvent` and logs a message to Seq, simulating sending a notification, and stores the notification in the NotificationDb.

This event flow is designed to be reliable and observable (outbox, retries, idempotency, and correlation IDs). Details are documented in [Design Decisions](#design-decisions).

### Verifying end-to-end
- Send a `POST /api/orders` request via Postman (it's provided)
- Check in Seq logs for "Received OrderCreatedEvent". The CorrelationId associated with this can be pasted in the search bar to verify:
    - the PaymentService receives the OrderCreatedEvent
    - the PaymentService processes the payment
    - the NotificationService receives the PaymentSucceededEvent
- Call
    - `GET /api/payments` to verify the payment is stored
    - `GET /api/notifications` to verify the notification is stored

## Design Decisions
### Architecture
I structured each service with API, Application, Domain, and Infrastructure layers to keep responsibilities explicit and make the codebase easier to maintain. This keeps HTTP concerns out of business logic and makes each layer easier to test and evolve independently. I also kept each service independently deployable by isolating persistence per service and keeping communication event-driven.

There is also a shared BuildingBlocks class library, which allows contracts to be shared. This ensures that the microservices sharing `OrderCreatedEvent` and `PaymentSucceededEvent` do not encounter routing mismatches. 

The NotificationsService notably has outbox disabled, because it is only a consumer of events, and never publishes.

The ApiGateway's `appsettings` points to the container names, and to localhost in the `appsettings.Development`.

I also chose to use Scalar with .NET 9 OpenAPI, as I believe this to be the more modern API documentation choice over Swagger. Although I miss Swagger a bit.

### Messaging stack choice
I chose to use RabbitMQ in a docker container, along with MassTransit, to demonstrate my fluency in using it. On top of this, I created a `BuildingBlocks` Extension for the MassTransit setup, to allow all 3 services to use it. Using RabbitMQ is more maintainable, and more accurately reflects a real development environment. It also allows me to independently deploy each service, which was important for the docker-compose.

### Reliability
I implemented a few reliability patterns to make the event-driven flow resilient to transient failures and duplicate deliveries:

- Outbox pattern (OrderService and PaymentService): The services that publish integration events use MassTransit's EF outbox, which stores outgoing messages in the same transaction as the database write. A background dispatcher then publishes from the outbox to RabbitMQ. This prevents data inconsistency if RabbitMQ is temporarily unavailable.
- Message retries (consumer side): I configured MassTransit retry intervals (500ms → 1s → 5s) in `MassTransitExtensions`, so transient failures in consumers are retried automatically.
- Consumer-side idempotency: If a duplicate event is delivered, repository checks + unique DB constraints prevent duplicate records (`OrderId` unique in PaymentService, `PaymentId` unique in NotificationService).
- Consumer-side validation policy: Invalid event payloads are logged and skipped (instead of thrown) to avoid repeated retries of poison messages that will never succeed.

### Observability
I added correlation IDs to make it easy to trace a single request across services:

- The API gateway and services use `CorrelationIdMiddleware` to read or generate `X-Correlation-ID`, echo it back on responses, and enrich logs with it.
- Since logs are shipped to Seq, I can search by CorrelationId to follow a transaction end-to-end across OrderService, PaymentService, and NotificationService.

### Developer Experience
For ease of review, I chose to use Docker Compose, and ensure the containers start up in the correct order using `depends_on`. A Postman collection was also provided containing all of the relevant endpoints. A healthcheck script was also provided for the reviewer.

### Trade-offs
Due to time constraints, I did not include authentication, rate limiting, and integration tests. In a real production environment, these would be important considerations. 

This trade-off is aligned with the assignment guidance: I prioritized clarity, correctness, and a working end-to-end event flow over production hardening features.

- Authentication middleware is important for endpoints that would trigger events, such as a `POST` endpoint
- Rate limiting is usually the first step in preventing DDoS attacks, and helps prevent abuse of the application.
- Integration tests (i.e. testing calling the endpoint directly) are usually critical for ensuring the functionality of an application. However, implementing a good set of integration tests in this case would require either mocking the RabbitMQ and DB instances, or depends on real ones running (e.g. setting up temporary ones for each test). Due to time constraints this was omitted.

## Testing Strategy

I implemented unit tests across all three services using `xUnit`, `Moq`, and `FluentAssertions`.  
`Moq` is used to isolate repositories and message publishing dependencies, while `FluentAssertions` improves readability of assertions.

- `OrderAppService`
    - Happy path for creating an order
    - Happy path for getting all orders
    - Invalid input cases that throw `ArgumentException`
- `PaymentAppService`
    - Happy path when payment does not yet exist
    - Happy path for getting all payments
    - Duplicate `OrderCreatedEvent` scenario does not create a second payment
    - `OrderCreatedConsumer` validation guard: invalid event payload does not call `ProcessOrderCreatedAsync`
- `NotificationAppService`
    - Happy path when notification does not yet exist
    - Happy path for getting all notifications
    - Duplicate `PaymentSucceededEvent` scenario does not create a second notification
    - `PaymentSucceededConsumer` validation guard: invalid event payload does not call `ProcessPaymentSucceededAsync`

Consumer validation in this project intentionally logs-and-skips invalid events instead of throwing. The reason is to avoid retrying permanently invalid "poison" messages via MassTransit retry middleware, which would add noise and unnecessary processing while never succeeding. For this assignment scope, rejecting invalid messages at the consumer boundary and preserving service availability was the more practical trade-off.

Run tests from the repository root with `dotnet test`.

As noted in Trade-offs, integration tests were omitted due to time constraints because they require temporary RabbitMQ and database dependencies (or equivalent test containers). In a production setting, I would prioritize adding integration tests for the full event flow.

## Limitations and Future Improvements

This implementation focuses on clean architecture and a reliable end-to-end event flow for the assignment scope. The main gaps for a production-ready version are:

- Add authentication/authorization for API endpoints.
- Add request rate limiting and broader abuse protection.
- Add integration tests covering API + RabbitMQ + database boundaries end-to-end.