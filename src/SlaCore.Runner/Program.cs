using SlaCore;
using SlaCore.Models;
using SlaCore.Runner;

var passed = 0;
var failed = 0;
var errors = new List<string>();
var Start = new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.Zero); // Monday
void Run(string name, Action test)
{
    try
    {
        test();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {name}");
        Console.ResetColor();
        passed++;
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {name}");
        Console.WriteLine($"    → {ex.Message}");
        Console.ResetColor();
        errors.Add($"{name}: {ex.Message}");
        failed++;
    }
}

Console.WriteLine("\n─── Deadline Calculation Tests ───────────────────────────────────────────────");

Run("WallClock: deadline = start + duration", () =>
{
    var policy = SlaPolicy.High();
    var deadline = SlaCalculator.CalculateSlaDeadline(Start, policy);
    TestAssert.Equal(Start.AddHours(4), deadline);
});

Run("Custom Critical policy overrides defaults", () =>
{
    var policy = SlaPolicy.Critical(
        configure: p =>
        {
            p.AllowedDuration = TimeSpan.FromHours(2);
            p.UseBusinessHoursOnly = true;
            p.BusinessHoursStart = TimeSpan.FromHours(8);
            p.BusinessHoursEnd = TimeSpan.FromHours(18);
            p.WarningThreshold = 0.75;
        });

    TestAssert.Equal("P1", policy.Id);
    TestAssert.Equal("Critical", policy.Name);
    TestAssert.Equal(TimeSpan.FromHours(2), policy.AllowedDuration);
    TestAssert.True(policy.UseBusinessHoursOnly);
    TestAssert.Equal(TimeSpan.FromHours(8), policy.BusinessHoursStart);
    TestAssert.Equal(TimeSpan.FromHours(18), policy.BusinessHoursEnd);
    TestAssert.EqualApprox(0.75, policy.WarningThreshold);
});

Run("WallClock: deadline spans weekend (wall-clock ignores weekend)", () =>
{
    var friday = new DateTimeOffset(2024, 1, 12, 22, 0, 0, TimeSpan.Zero);
    var policy = SlaPolicy.High();
    var deadline = SlaCalculator.CalculateSlaDeadline(friday, policy);
    TestAssert.Equal(new DateTimeOffset(2024, 1, 13, 2, 0, 0, TimeSpan.Zero), deadline);
});

Run("BusinessHours: skips holidays and weekend", () =>
{
    // The task starts on Thursday at 09:00 with a 15-hour SLA.
    // Friday and Monday are holidays, while Saturday and Sunday are non-business days.
    // With business hours from 08:00 to 18:00, 9 hours are available on Thursday
    // and the remaining 6 hours are completed on Tuesday, resulting in a deadline
    // of Tuesday at 14:00.
    var start = new DateTimeOffset(2024, 1, 11, 9, 0, 0, TimeSpan.Zero); // Thursday 09:00
    var evaluateAt = new DateTimeOffset(2024, 1, 16, 14, 0, 0, TimeSpan.Zero); // Tuesday 14:00

    var policy = SlaPolicy.Medium(
        businessHoursOnly: true,
        configure: p =>
        {
            p.AllowedDuration = TimeSpan.FromHours(15);
            p.BusinessHoursStart = TimeSpan.FromHours(8);
            p.BusinessHoursEnd = TimeSpan.FromHours(18);
            p.Holidays = new List<DateTime>
            {
                new DateTime(2024, 1, 12), // Friday
                new DateTime(2024, 1, 15)  // Monday
            };
        });

    var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

    TestAssert.Equal(new DateTimeOffset(2024, 1, 16, 14, 0, 0, TimeSpan.Zero), result.Deadline);
    TestAssert.EqualTimeSpan(TimeSpan.FromHours(15), result.ElapsedTime);
    TestAssert.Equal(SlaStatus.Breached, result.Status);
});
