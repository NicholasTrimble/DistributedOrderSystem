Distributed Order System

A lightweight distributed order-processing API built with .NET 10, Entity Framework Core, SQLite, and background processing.
I built this project to practice real-world API design with controllers, rate-limiting, middleware, and automated order-processing.


Features

REST API with Products + Orders

SQLite database with EF Core

Background worker that processes orders

Custom exception middleware

Debug endpoint listing all routes

Health check endpoint (/health)

Swagger UI enabled in development

Clean folder structure across Domain, Infrastructure, and API


What I used

ASP.NET Core 10

Entity Framework Core

SQLite

Background HostedService Workers

Rate Limiting Middleware

Swagger / OpenAPI




This is part of my learning path to build more realistic, backend-focused applications. 
I wanted something more meaningful than CRUD but still approachable. This API simulates 
how distributed order systems work in real companies without getting overly complex.