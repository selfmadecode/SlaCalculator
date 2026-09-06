# Business rules

SlaCalculator allows an SLA to follow the actual working rules of an organization.

Business rules are only applied when:

`UseBusinessHoursOnly = true`

Otherwise, the SLA uses wall-clock time.

## Business hours

Business hours define the portion of each business day that can contribute to the SLA.

For example:

```
BusinessHoursStart = TimeSpan.FromHours(9);
BusinessHoursEnd = TimeSpan.FromHours(17);
```

This gives 8 applicable hours per business day.

A task started at 16:00 with a 4-hour SLA would have:

- Day 1: 16:00 → 17:00    1 hour
- Day 2: 09:00 → 12:00    3 hours

The deadline is therefore 12:00 on the following business day.

## Business days

`BusinessDays` determines which days are eligible to contribute time.

For example, a Monday–Thursday schedule:

```
BusinessDays = new List<DayOfWeek>
{
    DayOfWeek.Monday,
    DayOfWeek.Tuesday,
    DayOfWeek.Wednesday,
    DayOfWeek.Thursday
};
```

Friday is not considered a business day even though it would normally be part of a Monday–Friday schedule.

## Holidays

Holidays are additional exclusions.

For example:

```
Holidays = new List<DateTime>
{
    new DateTime(2026, 9, 7)
};
```

If September 7 is a Monday, it will not contribute business time even if Monday is included in `BusinessDays`.

## Business Days and Holidays Together

The two settings are independent. For example:

```
BusinessDays = new List<DayOfWeek>
{
    DayOfWeek.Monday,
    DayOfWeek.Tuesday,
    DayOfWeek.Wednesday,
    DayOfWeek.Thursday
};

Holidays = new List<DateTime>
{
    new DateTime(2026, 9, 7)
};
```

This means:

- Monday is normally a business day, but September 7 is a holiday.
- Tuesday–Thursday are business days.
- Friday is not a business day.
- Saturday and Sunday are not business days.

### Example: SLA spanning a weekend and holiday

Consider:

- Business hours: 09:00–17:00
- Business days: Monday–Friday
- Friday: holiday
- Monday: holiday
- Saturday/Sunday: weekend
- SLA starts Thursday at 09:00
- SLA duration: 15 hours

The calculation is:

- Thursday    09:00 → 17:00    8 hours
- Friday                       holiday
- Saturday                     weekend
- Sunday                       weekend
- Monday                       holiday
- Tuesday     09:00 → 16:00    7 hours

Total: 15 hours → deadline Tuesday at 16:00.

## Custom working schedules

The library does not require a Monday–Friday schedule. For example, a business operating Tuesday–Saturday can configure:

```
BusinessDays = new List<DayOfWeek>
{
    DayOfWeek.Tuesday,
    DayOfWeek.Wednesday,
    DayOfWeek.Thursday,
    DayOfWeek.Friday,
    DayOfWeek.Saturday
};
```

Similarly, a support team operating 24 hours but only on weekdays could use:

```
BusinessHoursStart = TimeSpan.Zero;
BusinessHoursEnd = TimeSpan.FromHours(24);

BusinessDays = new List<DayOfWeek>
{
    DayOfWeek.Monday,
    DayOfWeek.Tuesday,
    DayOfWeek.Wednesday,
    DayOfWeek.Thursday,
    DayOfWeek.Friday
};
```

## Wall-clock vs business time

There are two fundamentally different SLA models.

Wall-clock (`UseBusinessHoursOnly = false`):

The SLA continuously counts elapsed time.

- Monday 16:00 + 4 hours = Monday 20:00

Business time (`UseBusinessHoursOnly = true`):

Only configured business time counts.

- Monday 16:00 → 17:00 = 1 hour
- Tuesday 09:00 → 12:00 = 3 hours

Deadline = Tuesday 12:00

This distinction allows the same calculator to support both 24/7 and business-hours-based service agreements.