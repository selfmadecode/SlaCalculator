# Examples

## Helpdesk priority SLA

A helpdesk can use the predefined priority policies:

```
var critical = SlaPolicy.Critical();
var high = SlaPolicy.High();
var medium = SlaPolicy.Medium();
var low = SlaPolicy.Low();
```

Each priority has a different default SLA duration.

## Business-hours helpdesk

A helpdesk operating from 08:00 to 18:00 can configure:

```
var policy = SlaPolicy.High(
    businessHoursOnly: true,
    configure: p =>
    {
        p.BusinessHoursStart = TimeSpan.FromHours(8);
        p.BusinessHoursEnd = TimeSpan.FromHours(18);
    });
```

## Customer-specific SLA

Different customers can have different SLA rules:

```
var goldCustomerPolicy = new SlaPolicy
{
    Id = "GOLD",
    Name = "Gold Customer",
    AllowedDuration = TimeSpan.FromHours(2),
    UseBusinessHoursOnly = true,
    WarningThreshold = 0.75
};

var standardCustomerPolicy = new SlaPolicy
{
    Id = "STANDARD",
    Name = "Standard Customer",
    AllowedDuration = TimeSpan.FromHours(8),
    UseBusinessHoursOnly = true,
    WarningThreshold = 0.80
};
```

## Different support teams

Different teams can have different schedules. For example, an engineering team working Monday–Friday:

```
var engineeringPolicy = new SlaPolicy
{
    Id = "ENGINEERING",
    Name = "Engineering",
    AllowedDuration = TimeSpan.FromHours(8),
    UseBusinessHoursOnly = true,

    BusinessHoursStart = TimeSpan.FromHours(9),
    BusinessHoursEnd = TimeSpan.FromHours(17),

    BusinessDays = new List<DayOfWeek>
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    }
};
```

A support team operating Tuesday–Saturday can use:

```
var supportPolicy = new SlaPolicy
{
    Id = "SUPPORT",
    Name = "Support",
    AllowedDuration = TimeSpan.FromHours(4),
    UseBusinessHoursOnly = true,

    BusinessHoursStart = TimeSpan.FromHours(8),
    BusinessHoursEnd = TimeSpan.FromHours(20),

    BusinessDays = new List<DayOfWeek>
    {
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    }
};
```

## Incident management

For critical incidents, a 24/7 SLA may be appropriate:

```
var policy = SlaPolicy.Critical();
```

For a business-hours incident process:

```
var policy = SlaPolicy.Critical(
    businessHoursOnly: true,
    configure: p =>
    {
        p.BusinessHoursStart = TimeSpan.FromHours(8);
        p.BusinessHoursEnd = TimeSpan.FromHours(18);
    });
```

## Evaluating a ticket

Once the policy is created:

```
var startedAt = DateTimeOffset.UtcNow;
var result = SlaCalculator.Evaluate(startedAt, policy);

switch (result.Status)
{
    case SlaStatus.OnTrack:
        Console.WriteLine("Ticket is on track.");
        break;

    case SlaStatus.AtRisk:
        Console.WriteLine("Ticket is approaching its SLA deadline.");
        break;

    case SlaStatus.Breached:
        Console.WriteLine("Ticket has breached its SLA.");
        break;
}
```

## Historical evaluation

A specific evaluation time can be supplied (useful for reporting, testing, and auditing):

```
var startedAt = new DateTimeOffset(2026, 9, 1, 10, 0, 0, TimeSpan.Zero);
var evaluateAt = new DateTimeOffset(2026, 9, 1, 15, 0, 0, TimeSpan.Zero);
var result = SlaCalculator.Evaluate(startedAt, evaluateAt, policy);