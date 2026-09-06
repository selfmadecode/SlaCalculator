# Policies

An `SlaPolicy` defines how an SLA should be calculated.

A policy controls the SLA duration and, when enabled, the business rules used to determine which periods count toward that duration.

## Creating a policy

There are two ways to create a policy.

### Predefined policies

SlaCalculator provides four factory methods:

```
SlaPolicy.Critical()
SlaPolicy.High()
SlaPolicy.Medium()
SlaPolicy.Low()
```

These provide sensible defaults:

| Policy | Default duration |
|------:|-----------------:|
| Critical | 1 hour |
| High     | 4 hours |
| Medium   | 8 hours |
| Low      | 24 hours |

Example:

```
var policy = SlaPolicy.High();
```

You can customize a predefined policy with the `configure` callback; only properties changed in the callback are overridden.

### Custom policies

You can instantiate `SlaPolicy` directly when you need custom behaviour:

```
var policy = new SlaPolicy
{
    Id = "P1",
    Name = "Critical Support",
    AllowedDuration = TimeSpan.FromHours(2),
    UseBusinessHoursOnly = true,
    WarningThreshold = 0.75
};
```

This approach is useful when your application's SLA configuration does not map to the predefined tiers.

## Customizing a predefined policy

Example of modifying the default Critical policy:

```
var policy = SlaPolicy.Critical(
    businessHoursOnly: true,
    configure: p =>
    {
        p.AllowedDuration = TimeSpan.FromHours(2);
        p.WarningThreshold = 0.75;
    });
```

## Policy properties

- `AllowedDuration` — amount of applicable time allowed for the SLA. When `UseBusinessHoursOnly` is false, this is wall-clock time; when true, only business time counts.
- `UseBusinessHoursOnly` — whether to apply business hours, business days, and holidays.
- `BusinessHoursStart` / `BusinessHoursEnd` — define the daily business-hours window.
- `BusinessDays` — which days of the week count as business days.
- `Holidays` — dates to exclude from counting toward the SLA.
- `WarningThreshold` — fraction of allowed duration after which status becomes `AtRisk` (default `0.80`).

## Choosing an approach

- Use a predefined policy for standard priority tiers.
- Customize a predefined policy when the priority is meaningful but rules differ.
- Create a policy directly when your application has its own SLA model.

Examples:

```
var policy = SlaPolicy.High(
    businessHoursOnly: true,
    configure: p => { p.AllowedDuration = TimeSpan.FromHours(6); });

var custom = new SlaPolicy
{
    Id = "CUSTOMER-GOLD",
    Name = "Gold Customer",
    AllowedDuration = TimeSpan.FromHours(2),
    UseBusinessHoursOnly = true
};