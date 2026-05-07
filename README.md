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
