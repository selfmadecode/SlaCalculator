# SlaCalculator

SlaCalculator is a .NET library for computing Service Level Agreement (SLA) deadlines
and evaluating SLA status (on-track, at-risk, breached). It supports both wall-clock and
business-hours-aware calculations, configurable business days, hours and holidays.

Prerequisites
- .NET 8 SDK

Quick start

1. Build the solution

```bash
dotnet build
```

2. Run the example runner project

```bash
dotnet run --project src/SlaCore.Runner
```

Using the library

Add a project reference to the `SlaCore` library (from your application project):

```bash
dotnet add reference src/SlaCore/SlaCore.csproj
```

Sample code

```csharp
using System;
using SlaCore;
using SlaCore.Models;

class Program
{
    static void Main()
    {
        var started = DateTimeOffset.UtcNow;

        // Use a predefined policy
        var policy = SlaPolicy.High(); // 4 hours

        // Compute deadline (wall-clock)
        var deadline = SlaCalculator.CalculateSlaDeadline(started, policy);
        Console.WriteLine($"Deadline: {deadline}");

        // Evaluate status now
        var result = SlaCalculator.Evaluate(started, policy);
        Console.WriteLine(result);

        // Business-hours example
        var businessPolicy = SlaPolicy.High(businessHoursOnly: true, configure: p =>
        {
            p.BusinessHoursStart = TimeSpan.FromHours(9);
            p.BusinessHoursEnd = TimeSpan.FromHours(17);
            p.Holidays = new System.Collections.Generic.List<DateTime> { new DateTime(2024, 12, 25) };
        });

        var businessDeadline = SlaCalculator.CalculateSlaDeadline(started, businessPolicy);
        Console.WriteLine($"Business-hours deadline: {businessDeadline}");
    }
}
```

API highlights
- `SlaPolicy` — create policies with `SlaPolicy.Critical()`, `High()`, `Medium()`, `Low()` and customize
- `SlaCalculator.CalculateSlaDeadline(startedAt, policy)` — compute deadline
- `SlaCalculator.Evaluate(startedAt, policy)` — get `SlaResult` with status, elapsed and remaining time

License
This repository is provided as-is.
