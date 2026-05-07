# Take Home Challenge Scenario

Event driven architecture is an important concept to keep systems scalable and decoupled.

In this scenario you will demonstrate the use of events to build an application providing a summary of analytics.

Mr. Green is the owner of a large stadium. He would like to build a dashboard for the number of people entering and leaving each gate over a period of time, in order to optimise resources in managing people flow.

Each gate has a sensor which accurately detects the number of people entering and leaving the stadium for the last minute.

An example JSON format of people entering:

```
{
  gate: 'Gate A',
  timestamp: '2023-04-01T08:00:00Z',
  numberOfPeople: 10,
  type: 'enter'
}
```

# Take Home Challenge Requirements

Your goal is to build an API that allows Mr. Green to build his dashboard. Here are the requirements:

- Minimum .NET 8 web API
- Storage in a relational database (for e.g. SQL, SQLite). Feel free to use any libraries you wish including Entity Framework.
- Expose a web endpoint that returns a list of sensor results grouped by gate and type as per below:
```
{
  gate: 'Gate A',
  type: 'enter',
  numberOfPeople: 100
}
```

- The web endpoint should be able to search by gate or type or time range (optional start time and end time)
- You will need to simulate events to be consumed asynchronously by the application
- Please create a solution that reflects production quality and is suitable for deploying to a customer
- Send us all source files so we can compile, build and give the program a go


## Developer Setup & Running Multiple Startup Projects

1. **Restore dependencies**  
   Open a terminal in the project root and run:
   
2. **Build the solution**  

3. **Run the API and Simulator projects**  
   - If using Visual Studio:
     - Right-click on the solution in Solution Explorer and select "Set Startup Projects..."
     - In the "Solution Property Pages" window, select "Multiple startup projects"
     - Set both the API project and the Simulator (or background worker) project to "Start" action
     - Click "OK" to save the changes
     - Start debugging (F5) or run without debugging (Ctrl + F5)
   - If using command-line:
     - Navigate to the API project folder and run `dotnet run`
     - Open a new terminal tab or window, navigate to the Simulator (or background worker) project folder and run `dotnet run`
     - Ensure both projects are running simultaneously

4. **Access the API**  
   - The API will be available at the URL shown in the output (e.g., `https://localhost:5001`).
   https://localhost:7009;  http://localhost:5115

## API Documentation & Testing

This project uses [Swagger UI (Scalar)](https://github.com/RicoSuter/NSwag/wiki/Scalar) for interactive API documentation and testing.

- Once the API is running, open your browser and navigate to:
- Replace the port if your API runs on a different port; check the console output when running the API.)

- Use the Scalar UI to explore, test, and interact with the API endpoints.

---

## Recent Changes

- **Infrastructure Layer:**
  - Moved `AppDbContext`, `ISensorEventRepository`, and `SensorEventRepository` to `StadiumAnalytics.Infrastructure`.
  - Database file `stadium.db` should now be placed in `StadiumAnalytics.Infrastructure/Database/` (update connection string if needed).
  - `SensorEventChannel` should be implemented in the infrastructure layer.

- **Application Layer:**
  - `AnalyticsService` and `EventConsumerWorker` remain in the application layer (`StadiumAnalytics.Api`).

- **Health Checks:**
  - Added ASP.NET Core health checks, including a database check.
  - Health endpoint available at `/health`.

## Project Structure

- `StadiumAnalytics.Api` – Application layer (controllers, services, workers)
- `StadiumAnalytics.Infrastructure` – Infrastructure layer (data access, messaging, database)
- `StadiumAnalytics.Shared` – Shared models and interfaces
- `EventSimulator` – Event simulation utilities
- `StadiumAnalytics.Tests` – Unit and integration tests

## How to Run

1. Ensure `stadium.db` is in `StadiumAnalytics.Infrastructure/Database/`.
2. Update the connection string in `appsettings.json` or `Program.cs` if needed.
3. Build and run the solution.
4. Access health check at `/health`.

---

## Running Tests

To run alltests, execute:

---

## Notes

- Ensure your database connection string is configured correctly in `appsettings.json` or user secrets.
- For production deployment, review security, logging, and error handling best practices