using SlaCore.Models;

namespace SlaCore.Tests
{
    public class BusinessHoursEvaluationTests
    {
        private static SlaPolicy MakePolicy(double hours, double warningThreshold = 0.80) => new()
        {
            Id = "BH",
            Name = "BusinessHours",
            AllowedDuration = TimeSpan.FromHours(hours),
            UseBusinessHoursOnly = true,
            WarningThreshold = warningThreshold
        };

        // ── Elapsed time tests

        [Fact]
        public void BusinessElapsed_WithinSameBusinessDay()
        {
            // The task started on Monday at 10:00 and was evaluated two hours later.
            // For an 8-hour task, the elapsed business time should be 2 hours.

            var start = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero); // Mon 10:00
            var evaluateAt = start.AddHours(2);                                     // Mon 12:00
            var policy = MakePolicy(8);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.FromHours(2), result.ElapsedTime);
        }

        [Fact]
        public void BusinessElapsed_StopsAtEndOfDay()
        {
            // The task started on Monday at 16:00 and was evaluated at 20:00.
            // Only one business hour elapsed because business hours end at 17:00.
            // For an 8-hour task, the elapsed business time should be 1 hour.

            var start = new DateTimeOffset(2024, 1, 15, 16, 0, 0, TimeSpan.Zero); // Mon 16:00                                                                                  
            var evaluateAt = new DateTimeOffset(2024, 1, 15, 20, 0, 0, TimeSpan.Zero); // evaluateAt = Monday 20:00 (after business hours) – only 1h of business time elapsed
            var policy = MakePolicy(8);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.FromHours(1), result.ElapsedTime);
        }

        [Fact]
        public void BusinessElapsed_SpansWeekend_OnlyCountsWeekdays()
        {
            // Friday 16:00 → Monday 10:00
            // Business time: 1h Friday + 1h Monday = 2h
            // The task started on Friday at 16:00 and was evaluated on Monday at 10:00.
            // Only business hours are counted: 1 hour on Friday and 1 hour on Monday.

            var start = new DateTimeOffset(2024, 1, 12, 16, 0, 0, TimeSpan.Zero); // Fri 16:00
            var evaluateAt = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero); // Mon 10:00
            var policy = MakePolicy(8);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.FromHours(2), result.ElapsedTime);
        }

        [Fact]
        public void BusinessElapsed_WhenEvaluatedBeforeStart_IsZero()
        {
            // The task started on Monday at 10:00 and was evaluated one hour earlier.
            // The elapsed business time should be zero, as the evaluation time is before the start time.
            var start = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
            var evaluateAt = start.AddHours(-1);
            var policy = MakePolicy(4);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.Zero, result.ElapsedTime);
        }

        [Fact]
        public void BusinessElapsed_OnNonBusinessDay_IsZero()
        {
            // Start and evaluate on a Saturday
            // The elapsed business time should be zero, as Saturday is not a business day.
            // The task started on Saturday at 10:00 and was evaluated on Saturday at 12:00.
            // For an 8-hour task, the elapsed business time should be 0 hours.
            var start = new DateTimeOffset(2024, 1, 13, 10, 0, 0, TimeSpan.Zero); // Sat
            var evaluateAt = new DateTimeOffset(2024, 1, 13, 12, 0, 0, TimeSpan.Zero); // Sat 12:00
            var policy = MakePolicy(8);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.Zero, result.ElapsedTime);
        }

        // ── Status with business hours

        [Fact]
        public void BusinessHours_Status_OnTrack_EarlyInDay()
        {
            // The task started on Monday at 09:00 and was evaluated two hours later.
            // For an 8-hour task, the elapsed business time is 2 hours, which is 25% of the allowed duration.
            var start = new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.Zero); // Mon 09:00
            var evaluateAt = start.AddHours(2);                                    // Mon 11:00 (25% of 8h)
            var policy = MakePolicy(8);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(SlaStatus.OnTrack, result.Status);
        }

        [Fact]
        public void BusinessHours_Status_Breached_NextDayAfterDeadline()
        {
            // The task started on Monday at 09:00 and was evaluated at 14:00.
            // For a 4-hour task, the elapsed business time is 5 hours, which exceeds the allowed duration.
            // The status should be Breached.
            var start = new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.Zero);
            var evaluateAt = new DateTimeOffset(2024, 1, 15, 14, 0, 0, TimeSpan.Zero);
            var policy = MakePolicy(4);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(SlaStatus.Breached, result.Status);
        }

        // ── Holiday exclusion

        [Fact]
        public void BusinessElapsed_HolidayNotCounted()
        {
            // Mon 09:00 → Wed 09:00 (Tue is holiday)
            // Expected: 8h Mon only (Tue excluded, Wed not yet reached at 09:00)

            // The task started on Monday at 09:00 and was evaluated on Wednesday at 09:00.
            // Tuesday is a holiday, so only Monday's business hours should be counted.
            // For an 8-hour task, the elapsed business time should be 8 hours.
            // The policy includes a holiday on Tuesday, January 16, 2024.

            var start = new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.Zero); // Mon
            var holiday = new DateTime(2024, 1, 16); // Tuesday
            var evaluateAt = new DateTimeOffset(2024, 1, 17, 9, 0, 0, TimeSpan.Zero); // Wed 09:00
            var policy = new SlaPolicy
            {
                Id = "BH",
                Name = "BH",
                AllowedDuration = TimeSpan.FromHours(16),
                UseBusinessHoursOnly = true,
                Holidays = new[] { holiday }
            };

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            // Only Monday's 8 hours count (Tuesday skipped, Wednesday not started yet at 09:00)
            Assert.Equal(TimeSpan.FromHours(8), result.ElapsedTime);
        }

        // ── Custom business hours

        [Fact]
        public void CustomBusinessHours_8amTo6pm()
        {
            // The task started on Monday at 08:00 and was evaluated at 12:00.
            // For a 4-hour task, the elapsed business time is 4 hours, which is exactly the allowed duration.
            var policy = new SlaPolicy
            {
                Id = "EH",
                Name = "ExtendedHours",
                AllowedDuration = TimeSpan.FromHours(4),
                UseBusinessHoursOnly = true,
                BusinessHoursStart = new TimeSpan(8, 0, 0),
                BusinessHoursEnd = new TimeSpan(18, 0, 0)
            };

            var start = new DateTimeOffset(2024, 1, 15, 8, 0, 0, TimeSpan.Zero);
            var evaluateAt = start.AddHours(4);

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.FromHours(4), result.ElapsedTime);
            Assert.Equal(SlaStatus.Breached, result.Status);
        }
    }
}