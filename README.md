# TicketingLite

A lightweight client support & issue tracking web application built with ASP.NET Core MVC.

## Features
- Role-based access (Admin, Agent, Client)
- Ticket lifecycle: Open → In Progress → Resolved
- Agent assignment + management view
- Commenting with REST API + AJAX
- Validation and security-focused patterns

## Tech Stack
- ASP.NET Core MVC, C#
- ASP.NET Core Identity (auth + roles)
- Entity Framework Core
- SQLite (dev); provider can be swapped to SQL Server
- Razor Views, Bootstrap, JavaScript (fetch/AJAX)

## Local Setup
```bash
dotnet restore
dotnet ef database update
dotnet run
