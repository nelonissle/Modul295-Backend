# Backend Project Documentation

This document provides an in-depth explanation of your backend code. It covers the structure, functionality, and dependencies of your application. Sections include controllers, models, data context, services, testing, configuration, and deployment (Docker).

---

## Table of Contents

- [Backend Project Documentation](#backend-project-documentation)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Controllers](#controllers)
    - [CookiesController](#cookiescontroller)
    - [ServiceOrdersController](#serviceorderscontroller)
    - [TranslationController](#translationcontroller)
    - [UsersController](#userscontroller)
  - [Models](#models)
    - [IOrderService](#iorderservice)
    - [IUserService](#iuserservice)
    - [OrderService](#orderservice)
    - [OrderUser](#orderuser)
  - [Data Context](#data-context)
    - [MongoDbContext (OrderServiceContext)](#mongodbcontext-orderservicecontext)
  - [Services](#services)
    - [JwtService](#jwtservice)
    - [OrderServiceService](#orderserviceservice)
    - [TestRunnerService](#testrunnerservice)
    - [UserService](#userservice)
  - [Testing](#testing)
    - [OrderServiceControllerTests \& OrderServiceMongoDBTests](#orderservicecontrollertests--orderservicemongodbtests)
  - [Configuration Files](#configuration-files)
    - [appsettings.json](#appsettingsjson)
    - [launchSettings.json (Properties)](#launchsettingsjson-properties)
  - [Program and Startup Configuration](#program-and-startup-configuration)
    - [Program.cs](#programcs)
  - [Docker and Deployment](#docker-and-deployment)
    - [Docker-Compose.yml](#docker-composeyml)
    - [Dockerfile](#dockerfile)
    - [Install Script (Install.md)](#install-script-installmd)
  - [Required Packages](#required-packages)

---

## Overview

The backend project is built with ASP.NET Core and uses MongoDB as its primary database. It includes various controllers for handling user actions (cookie consent, service orders, translation, user management), models representing orders and users, and services for JWT authentication, order management, and background test execution. Docker is used for containerized deployment.

---

## Controllers

### CookiesController
- **Purpose:** Manages cookie consent actions.
- **Endpoints:**
  - **POST api/cookies/accept:** Accepts and logs cookie consent details, storing the data in a cookie (1-year expiry).
  - **GET api/cookies/status:** Checks if a user has accepted cookies by reading the stored cookie.
  - **POST api/cookies/clear:** Clears the cookie consent on logout.
- **Dependencies:**  
  - `Microsoft.AspNetCore.Mvc`, `Newtonsoft.Json`, and ASP.NET Core logging.

### ServiceOrdersController
- **Purpose:** Provides CRUD operations for service orders.
- **Endpoints:**
  - **GET api/serviceorders:** Retrieves all service orders.
  - **GET api/serviceorders/{id}:** Retrieves a specific order by its ID.
  - **GET api/serviceorders/User/{id}:** Retrieves orders assigned to a specific user.
  - **POST api/serviceorders:** Creates a new service order.
  - **PUT api/serviceorders/{id}:** Updates an existing order.
  - **DELETE api/serviceorders/{id}:** Deletes an order.
- **Security:**  
  - Uses `[Authorize]` to secure endpoints.
- **Dependencies:**  
  - `MongoDB.Driver` for database interactions.

### TranslationController
- **Purpose:** Provides translation functionality using the DeepL API.
- **Endpoint:**
  - **POST api/translate:** Accepts a translation request (text and target language) and returns the translated text.
- **Key Points:**
  - Retrieves the DeepL API key from configuration.
  - Uses `HttpClient` to call the external API.
- **Dependencies:**  
  - `System.Net.Http`, `Microsoft.Extensions.Configuration`.

### UsersController
- **Purpose:** Handles user registration, login, two-factor authentication (2FA), password resets, and administrative user management.
- **Endpoints:**
  - **POST api/users/Register:** Registers a new user with password strength validation.
  - **POST api/users/Login:** Logs in a user, issuing a JWT token or requiring 2FA.
  - **POST api/users/2fa/enable-email:** Enables email-based 2FA.
  - **POST api/users/2fa/send-email:** Sends a 2FA code via email.
  - **POST api/users/2fa/verify-email:** Verifies the email-based 2FA code.
  - **GET api/users/2fa/status/{username}:** Checks if 2FA is enabled.
  - **POST api/users/2fa/disable/{username}:** Disables 2FA.
  - **PUT api/users/{id}:** (Admin) Updates a user's role.
  - **DELETE api/users/Delete/{username}:** (Admin) Deletes a user.
  - **POST api/users/forgot-password/request:** Initiates a password reset.
  - **POST api/users/forgot-password/reset:** Resets the user's password after code verification.
- **Security:**  
  - JWT-based authentication and role-based authorization for admin functions.
- **Dependencies:**  
  - `MongoDB.Driver`, `BCrypt.Net` for password hashing, `OtpNet`, `QRCoder`, `SkiaSharp` for 2FA, `System.Net.Mail` for email.

---

## Models

### IOrderService
- **Description:**  
  Interface defining the CRUD operations for service orders.
- **Methods:**
  - `GetAllOrdersAsync()`
  - `GetOrderByIdAsync(string id)`
  - `CreateOrderAsync(OrderService newOrder)`
  - `UpdateOrderAsync(string id, OrderService updatedOrder)`
  - `DeleteOrderAsync(string id)`

### IUserService
- **Description:**  
  Interface for user-related operations.
- **Methods:**
  - `UserExists(string username)`
  - `CreateUser(string username, string password, string role)`
  - `GetUserByUsername(string username)`
  - `UpdateUserRole(string username, string role)`

### OrderService
- **Description:**  
  Represents a service order.
- **Properties:**
  - `OrderId`: Unique identifier generated using MongoDB’s ObjectId.
  - `Name`, `Email`, `Phone`, `Priority`, `Service`, `Status`
  - `AssignedUserId`: References the ID of the assigned user.
  - `AssignedUser`: Optionally contains the user details.

### OrderUser
- **Description:**  
  Represents a user within the system.
- **Properties:**
  - `Id` and `UserId`: Unique identifiers.
  - `Username` and `PasswordHash`: For authentication.
  - `Role`: User role (default is `"user"`).
  - **Two-Factor Authentication Fields:**
    - `TwoFactorEnabled`
    - `TwoFactorSecret`
    - `TwoFactorRecoveryCodes`
  - `CreatedAt`: Timestamp for user creation.

---

## Data Context

### MongoDbContext (OrderServiceContext)
- **Purpose:**  
  Provides access to the MongoDB collections.
- **Features:**
  - **Constructor:** Validates the connection string and database name.
  - **Property:**  
    - `OrderServices`: Exposes the collection for service orders.
- **Dependencies:**  
  - `MongoDB.Driver`

---

## Services

### JwtService
- **Purpose:**  
  Handles JWT token generation and validation.
- **Key Methods:**
  - `GenerateToken(string username, string role)`: Creates a token that expires in 10 days.
  - `ValidateToken(string token)`: Validates a token and returns the associated claims.
- **Dependencies:**  
  - `System.IdentityModel.Tokens.Jwt`, `Microsoft.IdentityModel.Tokens`

### OrderServiceService
- **Purpose:**  
  Implements the `IOrderService` interface to provide CRUD operations for service orders.
- **Operations:**
  - Get, create, update, and delete orders using MongoDB.
- **Dependencies:**  
  - `MongoDB.Driver`

### TestRunnerService
- **Purpose:**  
  A background service that runs unit tests periodically (every 30 minutes).
- **Key Points:**
  - Uses a `Process` to execute `dotnet test`.
  - Logs the results (success or failure).
- **Dependencies:**  
  - `Microsoft.Extensions.Hosting`, `Microsoft.Extensions.Logging`

### UserService
- **Purpose:**  
  Implements the `IUserService` interface for managing user data.
- **Operations:**
  - Checks for existing users, creates new users (with password hashing), retrieves users, and updates user roles.
- **Dependencies:**  
  - `MongoDB.Driver`, `BCrypt.Net`

---

## Testing

### OrderServiceControllerTests & OrderServiceMongoDBTests
- **Purpose:**  
  Contains unit tests for controller endpoints and MongoDB operations.
- **Highlights:**
  - Tests for successful retrieval, creation, update, and deletion of orders.
  - Uses NUnit, Moq, and MongoDB.Driver for testing.
- **Location:**  
  - Under the `Modul295PraxisArbeit.Tests` namespace.
  
---

## Configuration Files

### appsettings.json
- **Purpose:**  
  Holds configuration settings such as logging, connection strings, JWT settings, DeepL API key, and MongoDB settings.
- **Example Settings:**
  - **Logging:** Configured to show information for the default and ASP.NET Core namespaces.
  - **ConnectionStrings:** Contains SQL Server connection details (if applicable).
  - **JwtSettings:** Includes the secret key for JWT token generation.
  - **DeepL:** Contains the API key for translation services.
  - **MongoDbSettings:** Provides the connection string and database name for MongoDB.

### launchSettings.json (Properties)
- **Purpose:**  
  Configures local development settings including application URLs, environment variables, and profiles (HTTP, HTTPS, IIS Express).
- **Highlights:**
  - `launchUrl` is set to Swagger for API testing.
  - Environment is set to `"Development"`.

---

## Program and Startup Configuration

### Program.cs
- **Purpose:**  
  Configures the application’s middleware, dependency injection, logging, authentication, and routing.
- **Key Configurations:**
  - **Logging:** Uses Serilog to log both to the console and to a file.
  - **Authentication:** Configured using JWT with custom token validation parameters.
  - **MongoDB:** Reads configuration for the connection string and database name and registers `IMongoDatabase` as a singleton.
  - **Services Registration:**  
    - Registers `IOrderService` and its implementation.
    - Registers `IJwtService` with the provided secret key.
    - Adds CORS policies to allow the frontend (e.g., a React app at `http://localhost:5173`).
  - **Swagger:** Enabled for development.
  - **Middleware:** Configures HTTPS redirection, authentication, authorization, and Serilog request logging.

---

## Docker and Deployment

### Docker-Compose.yml
- **Purpose:**  
  Defines the Docker services for both the MongoDB database and the web server.
- **Key Points:**
  - **Services:**
    - **db:** Uses the latest MongoDB image, sets up authentication, maps port 27017, and mounts a volume for persistence.
    - **web:** Runs the backend application image (e.g., `neloserver:latest`), sets environment variables, maps ports, and connects to the MongoDB network.
  - **Networks:**  
    - A custom network (`mymongonet`) ensures the containers can communicate.

### Dockerfile
- **Purpose:**  
  Builds the backend application.
- **Stages:**
  - **Build Stage:**  
    - Uses the .NET SDK image to restore, build, and publish the application (including a test data inserter if needed).
  - **Runtime Stage:**  
    - Uses the .NET ASP.NET Core runtime image.
    - Copies published files from the build stage.
    - Exposes ports (443 and 8080) and sets environment variables.
- **Commands:**  
  - Build the image with `docker build -t neloserver .`
  - Run the container using Docker Compose or `docker run`.

### Install Script (Install.md)
- **Purpose:**  
  Provides step-by-step instructions to install and deploy the backend.
- **Steps Include:**
  1. **Environment Setup:**  
     - Set environment variables such as `SMTPKEY` and `SMTPUSER`.
  2. **Execution:**  
     - Run the provided PowerShell script (`Install.ps1`) to build and deploy the Docker containers.
  3. **Test Data:**  
     - Create an admin user via the TestDataInserter.
  4. **MongoDB Commands:**  
     - Instructions to connect to the MongoDB container, switch databases, and inspect users.
  5. **MongoDB Connection String Examples:**  
     - How to use the connection string for either containerized MongoDB or a local instance.

---

## Required Packages

Ensure the following NuGet packages are installed for the project to build and run correctly:

- **ASP.NET Core & Logging:**
  - `Microsoft.AspNetCore.App` (includes MVC, HTTP, etc.)
  - `Microsoft.Extensions.Logging`
- **MongoDB:**
  - `MongoDB.Driver`
- **JSON Serialization:**
  - `Newtonsoft.Json`
- **Security & Hashing:**
  - `BCrypt.Net-Next`
- **JWT Authentication:**
  - `System.IdentityModel.Tokens.Jwt`
  - `Microsoft.IdentityModel.Tokens`
- **Two-Factor Authentication & QR Codes:**
  - `OtpNet`
  - `QRCoder`
  - `SkiaSharp`
- **Networking & Email:**
  - `System.Net.Http`
  - `System.Net.Mail`
- **Unit Testing:**
  - `NUnit`
  - `Moq`
- **Others:**
  - `Serilog` (and related sinks)
  - `System.Drawing.Common` (if using System.Drawing for image processing)

Example .NET CLI commands:

```bash
dotnet add package MongoDB.Driver
dotnet add package Newtonsoft.Json
dotnet add package BCrypt.Net-Next
dotnet add package OtpNet
dotnet add package QRCoder
dotnet add package SkiaSharp
dotnet add package Serilog
dotnet add package Serilog.Sinks.File
dotnet add package NUnit
dotnet add package Moq
dotnet add package System.Drawing.Common
