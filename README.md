# SlaCore

[SlaCore on NuGet](https://www.nuget.org/packages/SlaCore)

SlaCore is a lightweight .NET library for calculating SLA deadlines and evaluating SLA status. It supports wall-clock and business-hours SLAs with features like:

- Predefined priority levels (Critical, High, Medium, Low)
- Custom SLA durations
- Configurable business hours and business days
- Holiday exclusions
- Warning thresholds and SLA status evaluation
- Custom business rules

The library targets `netstandard2.0` and can be used from modern .NET applications.

## Installation

Install from NuGet:

```
dotnet add package SlaCore
```

Or reference the package in your project file:

```
<PackageReference Include="SlaCore" Version="1.0.0" />
```

## Quick start

Create a predefined policy and calculate a deadline:

```
using SlaCore;
using SlaCore.Models;

var startedAt = DateTimeOffset.UtcNow;
var policy = SlaPolicy.High();
var deadline = SlaCalculator.CalculateSlaDeadline(startedAt, policy);
Console.WriteLine($"SLA deadline: {deadline}");
```

`SlaPolicy.High()` defaults to a 4-hour allowed duration.

Predefined policies (defaults):

| Policy   | Default duration |
|---------:|-----------------:|
| Critical | 1 hour |
| High     | 4 hours |
| Medium   | 8 hours |
| Low      | 24 hours |

## Evaluating SLA status

```
var result = SlaCalculator.Evaluate(startedAt, policy);
Console.WriteLine($"Status: {result.Status}");
Console.WriteLine($"Elapsed: {result.ElapsedTime}");
Console.WriteLine($"Deadline: {result.Deadline}");
```

Status values:

- `SlaStatus.OnTrack`
- `SlaStatus.AtRisk`
- `SlaStatus.Breached`

By default, `AtRisk` is reached when `WarningThreshold` (0.80) of the allowed duration has elapsed.

## Business rules (wall-clock vs business time)

Set `UseBusinessHoursOnly = true` to apply business-days, business-hours, and holiday rules. When false, SLAs use continuous wall-clock time.

Examples and detailed explanations are available in the `docs/` folder:

- [Business rules](docs/business-rules.md)
- [Examples](docs/examples.md)
- [Policies](docs/policies.md)

## Real-life scenario: Helpdesk ticket SLA

Scenario:

- A helpdesk creates a ticket at `2026-09-02T16:00` (local time).
- The customer has a `High` SLA (4 hours) and the team operates Monday–Friday, 09:00–17:00.
- The next business day is Tuesday at 09:00 (because Monday is a holiday).

Calculation (business hours):

- Day 1: Thursday 16:00 → 17:00 = 1 hour
- Day 2: Friday is a holiday (0 hours)
- Weekend: 0 hours
- Monday: holiday (0 hours)
- Tuesday 09:00 → 12:00 = 3 hours
- Deadline: Tuesday 12:00

Code to configure the policy:

```
var policy = SlaPolicy.High(
    businessHoursOnly: true,
    configure: p =>
    {
        p.BusinessHoursStart = TimeSpan.FromHours(9);
        p.BusinessHoursEnd = TimeSpan.FromHours(17);
        p.BusinessDays = new List<DayOfWeek>
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        };
        p.Holidays = new List<DateTime> { new DateTime(2026, 9, 6), new DateTime(2026, 9, 7) };
    });
```

## API overview

Key types:

- `SlaPolicy` — defines allowed duration and business rules (`AllowedDuration`, `UseBusinessHoursOnly`, `BusinessHoursStart`, `BusinessHoursEnd`, `BusinessDays`, `Holidays`, `WarningThreshold`).
- `SlaCalculator.CalculateSlaDeadline(startedAt, policy)` — computes the deadline.
- `SlaCalculator.Evaluate(startedAt, policy)` — returns `SlaResult` with `Status`, `Deadline`, `ElapsedTime`.

## Use cases

- Helpdesk and support ticket SLAs
- Incident management
- Customer response-time monitoring
- Internal service requests and compliance tracking

## Development

Build the solution:

```
dotnet build
```

## License

MIT — see `LICENSE.txt`.
