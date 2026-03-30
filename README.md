# HotelSync Solution 🏨⚡️

A robust integration ecosystem built with **.NET 8** and **C#** designed to synchronize high-volume hospitality data between **Oracle Opera Cloud** and **HubSpot CRM**.

## 🏗 System Architecture
The solution follows a distributed architecture to ensure high availability and separation of concerns:

1.  **HotelSyncApi (Web API):** Handles incoming webhooks, manual triggers, and provides an interface for data management.
2.  **HotelSyncWorker (Worker Service):** A background process that performs constant polling to Opera Cloud and processes synchronization queues asynchronously.
3.  **SQL Server Layer:** Ensures data persistence, tracking every synchronization state (`PENDING`, `SUCCESS`, `ERROR`).

---

## 🚀 Project Progress

### Phase 1 & 2: Core API & Persistence
- Established a **Repository Pattern** for database operations.
- Implemented **SQL Server** integration using `Microsoft.Data.SqlClient`.
- Created the `SyncRecords` table to log and track every reservation movement.

### Phase 3: HubSpot CRM Integration
- Developed a dedicated `HubSpotService` using `HttpClient`.
- Integrated **HubSpot Private Apps** with OAuth 2.0 (Bearer Tokens).
- Automated the creation and association of **Contacts** and **Deals** via HubSpot's CRM API.

### Phase 4: Background Processing (Worker Services)
- Added a **Worker Service** project to handle long-running background tasks.
- Implemented an `OperaPollingWorker` that simulates real-time data fetching from Opera Cloud.
- Configured a resilient polling loop that recovers from API errors without stopping the service.

---

## 🛠 Tech Stack
- **Backend:** .NET 8 (Web API & Worker Service)
- **Language:** C# 12
- **Database:** SQL Server Express
- **Integrations:** HubSpot CRM API (REST)
- **Tools:** Visual Studio 2022, Postman, Git

## 🔄 Integration Flow

### 1. Opera to HubSpot (Polling)
- A **Background Worker** polls the Opera Cloud API every 10 seconds for new reservations.
- New reservations are synced as **Deals** in HubSpot.

### 2. HubSpot to Opera (Webhooks)
- The system exposes a **RESTful Webhook endpoint** using **ngrok** for local development.
- When a Deal is moved to **'Closed Won'** in HubSpot, a webhook triggers an event.
- The **HotelSyncApi** receives the payload and queues a **PENDING** job in **SQL Server**.
- This decoupled architecture ensures high availability and fault tolerance.

---

## ⚙️ How to Run
1.  **Database:** Execute the `database_setup.sql` script in your SQL Server instance.
2.  **Configuration:** Update `appsettings.json` with your `ConnectionStrings` and HubSpot `AccessToken`.
3.  **Startup:** - Right-click the **Solution** > **Properties**.
    - Set **Multiple Startup Projects** to "Start" for both `HotelSyncApi` and `HotelSyncWorker`.
    - Press **F5**.

    Deployment Note: For production environments, this solution is designed to be deployed as a Windows Service (Worker) and hosted on Azure App Services or IIS (API), utilizing a CI/CD pipeline for automated deployments.