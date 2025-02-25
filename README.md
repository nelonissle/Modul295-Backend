# Modul295PraxisArbeit Backend Projekt Documentation

This document describes the complete backend project "Modul295PraxisArbeit" (also referred to as Praxisarbeit_M295 in some parts). It explains the overall architecture, including controllers, models, data context, services, testing, configuration, and deployment. Use this guide to understand, maintain, and deploy my project.

---

## Overview of this Projekt 

This projekt is the backend to my Webpage (See in Github WebpageReact25) it was a School projekt but i have Decided to Keep working on it time by time and now i have a Funktioning Backend system. I keep working on it and this is more a "Testing ground" for me. I will Keep working on it and Improving it the main goal for me is to Automate everything and getting in touch with real world Software Development.

#### Nice to know 

- The Projekt is Designed to run local but mainly in Docker (see Docker Below)
- It is in .NET 8.0 
- it has Git Secrets so if you want to Deploy it just Contact me for the Environment Setup  for the keys you need. More Infos About this are under [Docker \& Deployment](#docker--deployment)
- It is Still in work so not everything is Perfekt or Not Implementet yet


#### TOFO for me 

- I want to Analyse Data with Cookies or analytics Tool (matomo or google analytics)
- Strukture it more so its more viewable and looking better 
- make Git Actions for more automations 
- make Logs more advanced and better Struktured so it is simpler to view and analyse the Logs 
---

## Table of Contents

- [Modul295PraxisArbeit Backend Projekt Documentation](#modul295praxisarbeit-backend-projekt-documentation)
  - [Overview of this Projekt](#overview-of-this-projekt)
      - [Nice to know](#nice-to-know)
      - [TOFO for me](#tofo-for-me)
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
  - [Program \& Startup Configuration](#program--startup-configuration)
    - [Program.cs](#programcs)
  - [Docker \& Deployment](#docker--deployment)
    - [Making Docker setup](#making-docker-setup)
    - [Docker-Compose.yml](#docker-composeyml)
    - [Dockerfile](#dockerfile)
    - [Install Script (Install.md)](#install-script-installmd)
  - [Command Gallery](#command-gallery)
  - [Required Packages](#required-packages)

---

## Overview

The "Modul295PraxisArbeit" backend project is built with ASP.NET Core and uses MongoDB as its primary database. It supports cookie management, service order processing, translation via DeepL, user authentication with JWT and two-factor authentication (2FA), and background testing. The project is containerized using Docker for easier deployment.

---

## Controllers

### CookiesController
- **Purpose:**  
  Manages cookie consent—logging user details, storing consent data, and clearing cookies.
- **Key Endpoints:**
  - **POST `/api/cookies/accept`:**  
    Accepts cookie consent, logs details (IP, User-Agent, referrer, session ID), and stores the consent JSON as a cookie (expires in one year).
  - **GET `/api/cookies/status`:**  
    Checks if the user has already accepted cookies by reading the stored cookie.
  - **POST `/api/cookies/clear`:**  
    Clears the cookie consent (typically on user logout).
- **Dependencies:**  
  ASP.NET Core MVC, logging, d Newtonsoft.Json for serialization.

### ServiceOrdersController
- **Purpose:**  an
  Provides CRUD operations for service orders.
- **Key Endpoints:**
  - **GET `/api/serviceorders`:** Retrieves all service orders.
  - **GET `/api/serviceorders/{id}`:** Retrieves a specific order by ID.
  - **GET `/api/serviceorders/User/{id}`:** Retrieves orders filtered by the assigned user.
  - **POST `/api/serviceorders`:** Creates a new service order (assigning the order to a user if available).
  - **PUT `/api/serviceorders/{id}`:** Updates an existing order.
  - **DELETE `/api/serviceorders/{id}`:** Deletes a service order.
- **Security:**  
  Most endpoints use the `[Authorize]` attribute to ensure proper authentication.
- **Dependencies:**  
  MongoDB.Driver and Microsoft.Extensions.Logging.

### TranslationController
- **Purpose:**  
  Provides translation functionality using the DeepL API.
- **Key Endpoint:**
  - **POST `/api/translate`:**  
    Accepts a translation request (with text and target language), calls DeepL’s API using an HTTP client, and returns the translated text.
- **Dependencies:**  
  System.Net.Http, Microsoft.Extensions.Configuration, and logging.

### UsersController
- **Purpose:**  
  Manages user registration, login, two-factor authentication (2FA), password reset, and administrative tasks.
- **Key Endpoints:**
  - **POST `/api/users/Register`:**  
    Registers a new user (with password strength validation and BCrypt hashing) and issues a JWT token.
  - **POST `/api/users/Login`:**  
    Logs in a user, checking credentials and handling 2FA if enabled.
  - **2FA Endpoints:**  
    - **POST `/api/users/2fa/enable-email`:** Enables email-based 2FA.
    - **POST `/api/users/2fa/send-email`:** Sends a 2FA code to the user's email.
    - **POST `/api/users/2fa/verify-email`:** Verifies the provided 2FA code.
    - **GET `/api/users/2fa/status/{username}`:** Checks if 2FA is enabled for the user.
    - **POST `/api/users/2fa/disable/{username}`:** Disables 2FA for the user.
  - **Password Reset Endpoints:**  
    - **POST `/api/users/forgot-password/request`:** Initiates a password reset by sending a code via email.
    - **POST `/api/users/forgot-password/reset`:** Resets the password after verifying the code.
  - **Admin Functions:**  
    - **PUT `/api/users/{id}`:** Updates a user's role.
    - **DELETE `/api/users/Delete/{username}`:** Deletes a user.
- **Dependencies:**  
  MongoDB.Driver, BCrypt.Net for password hashing, OtpNet, QRCoder, SkiaSharp for 2FA, and System.Net.Mail for emailing.

---

## Models

### IOrderService
- **Description:**  
  An interface defining the CRUD operations for service orders.
- **Methods:**
  - `Task<List<OrderService>> GetAllOrdersAsync()`
  - `Task<OrderService> GetOrderByIdAsync(string id)`
  - `Task CreateOrderAsync(OrderService newOrder)`
  - `Task UpdateOrderAsync(string id, OrderService updatedOrder)`
  - `Task DeleteOrderAsync(string id)`

### IUserService
- **Description:**  
  An interface for user-related operations.
- **Methods:**
  - `Task<bool> UserExists(string username)`
  - `Task CreateUser(string username, string password, string role)`
  - `Task<OrderUser> GetUserByUsername(string username)`
  - `Task UpdateUserRole(string username, string role)`

### OrderService
- **Description:**  
  Represents a service order.
- **Properties:**
  - `OrderId`: A unique identifier (generated using MongoDB’s ObjectId).
  - `Name`, `Email`, `Phone`, `Priority`, `Service`, `Status`
  - `AssignedUserId`: The ID of the assigned user.
  - `AssignedUser`: Optionally contains the full user details.

### OrderUser
- **Description:**  
  Represents a user in the system.
- **Properties:**
  - `Id` and `UserId`: Unique identifiers (as strings using MongoDB ObjectId).
  - `Username` and `PasswordHash`: For authentication.
  - `Role`: User role (default is `"user"`, can be updated to `"Kunde"`, `"Admin"`, etc.).
  - **Two-Factor Authentication Fields:**
    - `TwoFactorEnabled`
    - `TwoFactorSecret`
    - `TwoFactorRecoveryCodes`
  - `CreatedAt`: Timestamp when the user was created.

---

## Data Context

### MongoDbContext (OrderServiceContext)
- **Purpose:**  
  Provides MongoDB connectivity and exposes collections.
- **Features:**
  - **Constructor:**  
    - Validates that both the connection string and database name are provided.
    - Creates a MongoClient and retrieves the specified database.
  - **Property:**  
    - `OrderServices`: Exposes the MongoDB collection for service orders.
- **Dependency:**  
  MongoDB.Driver

---

## Services

### JwtService
- **Purpose:**  
  Generates and validates JWT tokens.
- **Key Methods:**
  - `GenerateToken(string username, string role)`:  
    Creates a JWT that expires in 10 days.
  - `ValidateToken(string token)`:  
    Validates the token and extracts the claims.
- **Dependencies:**  
  System.IdentityModel.Tokens.Jwt, Microsoft.IdentityModel.Tokens

### OrderServiceService
- **Purpose:**  
  Implements the `IOrderService` interface for managing service orders.
- **Operations:**  
  Uses MongoDB to get, create, update, and delete orders.
- **Dependency:**  
  MongoDB.Driver

### TestRunnerService
- **Purpose:**  
  A background service that periodically runs unit tests (every 30 minutes) using a `dotnet test` process.
- **Behavior:**  
  Logs the status (success or failure) of test runs.
- **Dependencies:**  
  Microsoft.Extensions.Hosting, Microsoft.Extensions.Logging

### UserService
- **Purpose:**  
  Implements the `IUserService` interface for user management.
- **Operations:**  
  Checks if a user exists, creates new users (with BCrypt password hashing), retrieves users, and updates user roles.
- **Dependencies:**  
  MongoDB.Driver, BCrypt.Net

---

## Testing

### OrderServiceControllerTests & OrderServiceMongoDBTests
- **Purpose:**  
  Contains unit tests for controller endpoints and MongoDB operations.
- **Highlights:**  
  - Tests successful and failure scenarios for retrieving, creating, updating, and deleting orders.
  - Uses NUnit and Moq for mocking dependencies.
- **Namespace:**  
  `Modul295PraxisArbeit.Tests`

---

## Configuration Files

### appsettings.json
- **Purpose:**  
  Holds configuration settings for logging, connection strings, JWT, DeepL API, and MongoDB.
- **Key Sections:**
  - **Logging:** Configures log levels.
  - **ConnectionStrings:** (For SQL Server if used.)
  - **JwtSettings:** Contains the JWT secret key.
  - **DeepL:** Contains the API key for translation services.
  - **MongoDbSettings:** Contains the connection string and database name for MongoDB.

### launchSettings.json (Properties)
- **Purpose:**  
  Configures local development settings such as application URLs, launch profiles, and environment variables.
- **Key Settings:**
  - `launchUrl` set to Swagger.
  - Profiles for HTTP, HTTPS, and IIS Express with the environment set to "Development".

---

## Program & Startup Configuration

### Program.cs
- **Purpose:**  
  Configures middleware, dependency injection, logging, authentication, and routing.
- **Key Configurations:**
  - **Logging:**  
    - Uses Serilog to log to both console and file (located in a Logs directory).
  - **Authentication:**  
    - Configured using JWT Bearer authentication with custom token validation parameters.
  - **MongoDB:**  
    - Reads connection details from configuration/environment and registers `IMongoDatabase` as a singleton.
  - **Service Registration:**  
    - Registers `IOrderService`, `IJwtService`, and background services (like `TestRunnerService`).
  - **CORS:**  
    - Configured to allow requests from the frontend (e.g., `http://localhost:5173`).
  - **Swagger:**  
    - Enabled in development mode.

---

## Docker & Deployment

### Making Docker setup 
  Here i will go more into the Detailed Deployment for the Docker and what you need for it to work properly. For the Docker GUI i use Docker Desktop Because it offers me the perfekt Environment to Manage my Dockers.

  1. Install Docker Desktop if you havent already Here is the Download link for it 
     [Docker Desktop](https://www.docker.com/products/docker-desktop/ "Docker Desktop download")
  2. After you Downloaded Docker Desktop Login or creat a Account
  3. if you have done this you can go back into Visual Stuido Code and run the install script
   - for Windows (`./Install.ps1`) 
   - for linux and MacOS run (`./Install.sh`)
  4. If you have done these Steps you should be Ready to go and my projekt should run on your Local System too 
   
  **Important:**
  If you are running it Localy you need to set your Environment Variables with the keys i have setup for 2fa to work Or else you cant use it for these keys and Secret infos you can Contact me. 

### Docker-Compose.yml
- **Purpose:**  
  Defines Docker services for MongoDB and the web server.
- **Services:**
  - **db:**  
    - Uses the latest MongoDB image.
    - Sets up root username and password.
    - Exposes port 27017 and mounts a volume for data persistence.
  - **web:**  
    - Uses your built image (`neloserver:latest`).
    - Sets environment variables such as SMTP credentials.
    - Maps container ports to host ports and connects to the MongoDB network.
- **Networks:**  
  - A custom network (`mymongonet`) ensures container communication.

### Dockerfile
- **Purpose:**  
  Builds and packages the backend application.
- **Stages:**
  - **Build Stage:**  
    - Uses the .NET SDK image to restore, build, and publish the application (and test data inserter).
  - **Runtime Stage:**  
    - Uses the .NET ASP.NET Core runtime image.
    - Copies published files from the build stage.
    - Exposes ports 443 and 8080.
    - Sets the MongoDB connection string environment variable.
- **Usage:**  
  Build the image with `docker build -t neloserver .` and run using Docker Compose.

### Install Script (Install.md)
- **Purpose:**  
  Provides step-by-step instructions to set up and deploy the project.
- **Steps Include:**
  1. **Environment Setup:**  
     - Set SMTPKEY and SMTPUSER.
  2. **Execution:**  
     - Run the provided PowerShell script (`Install.ps1`) to build and deploy Docker containers.
  3. **Test Data:**  
     - Create an admin user via TestDataInserter.
  4. **MongoDB Commands:**  
     - Commands to connect to the MongoDB container, use the proper database, and inspect users.
  5. **MongoDB Connection:**  
     - Examples for containerized or local MongoDB usage.

---

## Command Gallery 
here are some Important Commands you Could come Encounter. Those Commands are for Windows so some of them ight not work on MacOS or Linux

**General Commands for the Projekt**
  - `dotnet run` (Runs the projekt Localy)
  - `dotnet restore` (restore a .NET project’s dependencies and tools)
  - `dotnet build` (Builds the Projekt and checks for errors)
  - `dotnet Clean` (properly cleans the projekt)
  - `cd .\Modul295PraxisArbeit\` (moves into the main projekt Directory)
  - `cd ..` (goes one Directory Back)
  - `dir` (displays the contents of a directory)
  - `Docker Compose up` (It reads the docker-compose.yml file and builds it with its Instruktions)
  - `Docker compose down` (cleans up the containers and networks created)
  - `docker compose up db -d` (Launches only the mongodb Docker use it for local development with dotnet run)
  - `TestDataInserter` (Creats Test Data for the MongoDB)

**Commands for Mongo shell**
- `use Modul295Db` ()
- `show collections`
- `db.Users.find().pretty()` (Displays the current data in Users Collection)

## Required Packages

Make sure to install the following NuGet packages:

- **ASP.NET Core & Logging:**
  - `Microsoft.AspNetCore.App`
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
  - `Serilog` and `Serilog.Sinks.File`
  - `System.Drawing.Common`

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
