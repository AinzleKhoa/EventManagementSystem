# OpenCode Development Guide

## Project Overview & Structure
- **Architecture**: .NET 8 microservices architecture with Ocelot API Gateway and ASP.NET Core MVC Frontend (`WebClient`).
- **Solution Location**: `EventManagement/EventManagement.sln` containing 12 projects in `EventManagement/`:
  - `GatewayAPI`: Port `7158` (Ocelot API Gateway)
  - `WebClient`: Port `7104` (ASP.NET Core MVC Frontend)
  - Microservices: `AuthenticationAPI` (`7243`), `CategoryAPI` (`7020`), `EventAPI` (`7289`), `BookingAPI` (`7108`), `SocialAPI` (`7182`), `VoucherAPI` (`7227`), `WithdrawalAPI` (`7192`), `ChatAPI` (`7148`), `NotificationAPI` (`7197`), `ChatbotAPI` (`7299`).
- **Database Scripts**: SQL Server setup scripts (`1. AuthDB.sql` through `9. NotificationDB.sql`) reside in the root repository folder.

## Build & Package Management
- **Restore Dependencies**: If package restore fails (e.g. `NU1101` missing sources), explicitly pass the official NuGet feed:
  ```powershell
  dotnet restore EventManagement/EventManagement.sln --source https://api.nuget.org/v3/index.json
  ```
- **Build Solution**:
  ```powershell
  dotnet build EventManagement/EventManagement.sln
  ```

## Service Execution
- **Run Individual Service**:
  ```powershell
  dotnet run --project EventManagement/<ProjectDirectory> --launch-profile https
  ```
- **Start All 12 Microservices**:
  - PowerShell: `.\EventManagement\start_all.ps1`
  - Command Prompt: `.\EventManagement\start_all.bat`
  - VS Code Task: "Run All Microservices" (parallel launch)
- **Stop All Running Services**:
  - PowerShell: `.\EventManagement\stop_all.ps1`
  - Command Prompt: `.\EventManagement\stop_all.bat`

## Database Setup & Reset
- **Reset Databases**: Drop and recreate all 9 databases using `sqlcmd` from root SQL scripts:
  - SQL Authentication:
    ```cmd
    .\EventManagement\reset_databases.bat localhost sql sa 123456
    ```
  - Windows Authentication:
    ```cmd
    .\EventManagement\reset_databases.bat localhost windows
    ```
