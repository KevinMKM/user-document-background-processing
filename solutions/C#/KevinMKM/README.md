User Document Background Processing

This repository contains a comprehensive solution for a user management system with document upload, processing, and notification handling. The system leverages modern .NET technologies, including .NET 6, Entity Framework Core, Hangfire for background jobs, and Redis for performance optimization. The architecture follows best practices for scalability, maintainability, and fault tolerance.

Key Features

API using .NET 6: A RESTful API built with .NET 6, designed for user registration and document management.

Entity Framework Core (SQL Server): EF Core for data persistence and migrations; includes helpers for schema evolution.

Hangfire: Reliable background job processing using Hangfire for asynchronous processing (e.g., file conversion, email notifications) and support for retry mechanisms.

Redis Integration: Optional Redis support for Hangfire job storage (or use SQL Server by default).

Outbox Pattern: Ensures reliable message delivery (e.g., emails) after transactional consistency by storing messages in an Outbox table before dispatching.

Docker Support: Dockerfile for containerizing the API, and Docker Compose for managing dependencies like SQL Server, Redis, and the API.

GitHub Actions CI/CD: Continuous integration with automated build, test, and deploy pipelines.

Health Monitoring: Built-in health checks for API and services to ensure system stability.

Architecture Overview

This project is structured with a Clean Architecture approach, splitting the solution into several layers for better separation of concerns and testability.

Solution Layers

API Layer (UserDocs.API):

Handles HTTP requests and responses.

Contains controllers and job orchestration logic (e.g., Hangfire jobs).

The core responsibility of this layer is to expose API endpoints for user registration, document status, and background job monitoring.

Application Layer (UserDocs.Application):

Contains business logic interfaces (repositories, services).

Ensures the application's rules are separated from the persistence layer.

Interfaces define how the domain interacts with external dependencies (such as databases and file storage).

Domain Layer (UserDocs.Domain):

Contains domain models, such as User, UserDocument, and OutboxMessage.

Implements core business logic and entities that are independent of infrastructure.

Infrastructure Layer (UserDocs.Infrastructure):

Implements the actual file storage (e.g., local file system), email sender, background job handlers (using Hangfire), and EF Core repository implementations.

Includes the database context (ApplicationDbContext), background services (like OutboxDispatcher), and external integrations.

System Components
1. User Registration

UserController receives a POST /api/users/register request containing the user’s name, email, and document.

UserRepository persists the user data.

File Storage: The uploaded file is saved locally (or can be extended for cloud storage like AWS S3 or Azure Blob Storage).

Outbox Pattern: After successful registration, an OutboxMessage is stored in the database to ensure that the welcome email is sent after the transaction commits.

2. Background Job Processing (Hangfire)

Document Processing: After the user registers, a background job is scheduled to process the uploaded document (convert to PDF). This job is delayed by 30 seconds to simulate real-time file processing.

Retries: If the document conversion fails, Hangfire will retry the job up to two more times with delays of 5 and 10 minutes, respectively.

OutboxDispatcher: A background service that periodically checks for unprocessed Outbox messages and sends them (in this case, emails).

3. Health Monitoring

Health Checks: The system provides health endpoints to monitor the health of the API and essential services (e.g., database, Hangfire server).

Service Reliability: The application uses Polly for retry policies and fault tolerance, ensuring that transient issues are handled gracefully.

Deployment Options
1. Running with Docker

To run the application with Docker, you will need Docker and Docker Compose installed. The docker-compose.yml file will start the following services:

SQL Server: The database for storing user data and document status.

Redis (optional): For Hangfire job storage (useful for high-throughput systems).

API: The .NET API that exposes user registration and document management endpoints.

Steps to Run

Start Docker Services:

docker-compose up --build


API Health Check (optional):
To ensure everything is up, visit:

http://localhost:5000/health


Test API Endpoints:

User Registration:

curl -X POST "http://localhost:5000/api/users/register" -F "name=John" -F "email=john@example.com" -F "document=@/path/to/document.pdf"


Document Status:

curl "http://localhost:5000/api/users/documents/{documentId}/status"

2. Running Locally (Without Docker)

If you want to run the application locally without Docker:

Set Connection String:
In src/UserDocs.API/appsettings.json, update the ConnectionStrings__DefaultConnection to your local SQL Server instance.

Run Migrations:
Ensure the database schema is up to date:

./migrations.sh add Initial
./migrations.sh update


Run API:

dotnet run --project src/UserDocs.API

CI/CD with GitHub Actions

The project includes a GitHub Actions workflow to automate the build and testing process. The ci.yml file runs the build, restores dependencies, and publishes the API.

To test the pipeline:

Push your code to GitHub and open a PR.

The pipeline will run automatically and execute tests (if configured) or simply build the project.

The ci.yml file will:

Checkout the code

Set up the environment (install .NET, run tests, etc.)

Build the project and deploy to an appropriate environment.

Database Migrations

EF Core migrations are used to manage schema changes for the application.

Add Migration: To create a new migration, run:

./migrations.sh add <MigrationName>


Apply Migration: To update the database with the latest migration:

./migrations.sh update


These commands are helpful for tracking changes to the database schema and making sure the application and database are in sync.

Advanced Configuration
1. Redis Integration

If you prefer to use Redis for Hangfire's job storage:

Update docker-compose.yml to use Redis instead of SQL Server for Hangfire:

services:
  api:
    environment:
      - HANGFIRE_STORAGE=Redis
      - REDIS_HOST=redis


Modify Program.cs to configure Hangfire to use Redis.

2. Hangfire Dashboard Security

By default, the Hangfire dashboard is exposed at /hangfire. In a production scenario, you should secure this endpoint.

Basic Authentication: Add basic authentication middleware to protect the dashboard:

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new BasicAuthAuthorizationFilter() }
});


OAuth: Integrate OAuth or any other form of authorization based on your requirements.

Notes

Replace SA password: Ensure to change the SA_PASSWORD in docker-compose.yml before using the setup in production.

Secure Hangfire: Always secure Hangfire’s dashboard before exposing it in production environments.

Production Storage: For production environments, use a cloud file storage solution (e.g., AWS S3, Azure Blob Storage) instead of local file storage.

Performance: Redis and Hangfire offer excellent scalability options for handling a large number of background jobs.
