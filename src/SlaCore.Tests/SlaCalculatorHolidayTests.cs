using SlaCore.Models;

namespace SlaCore.Tests
{
    public class SlaCalculatorHolidayTests
    {
        private static SlaPolicy MakePolicy(double hours, double warningThreshold = 0.80) => new()
        {
            Id = "BH",
            Name = "BusinessHours",
            AllowedDuration = TimeSpan.FromHours(hours),
            UseBusinessHoursOnly = true,
            WarningThreshold = warningThreshold
        };

        // ── Holiday and weekend tests

        [Fact]
        public void BusinessElapsed_SkipsHolidaysAndWeekend()
        {
            // The task started on Thursday at 09:00 and was evaluated on Tuesday at 14:00.
            // Friday and Monday are holidays, while Saturday and Sunday are non-business days.
            // Business time should be 8 hours on Thursday + 7 hours on Tuesday = 15 hours.
            
            var start = new DateTimeOffset(2024, 1, 11, 9, 0, 0, TimeSpan.Zero); // Thu 09:00
            var evaluateAt = new DateTimeOffset(2024, 1, 16, 14, 0, 0, TimeSpan.Zero); // Tue 14:00
            var policy = MakePolicy(15);

            policy.Holidays = new List<DateTime>
            {
                new DateTime(2024, 1, 12), // Fri
                new DateTime(2024, 1, 15)  // Mon
            };

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.FromHours(13), result.ElapsedTime);
        }

        [Fact]
        public void CalculateDeadline_SkipsHolidaysAndWeekend()
        {
            // The task started on Thursday at 09:00 with a 15-hour SLA.
            // Friday and Monday are holidays, while Saturday and Sunday are non-business days.
            // the dealine should be on Tuesday at 16:00, after 8 hours on Thursday and 7 hours on Tuesday.

            var start = new DateTimeOffset(2024, 1, 11, 9, 0, 0, TimeSpan.Zero); // Thu 09:00
            var policy = MakePolicy(15);

            policy.Holidays = new List<DateTime>
            {
                new DateTime(2024, 1, 12), // Fri
                new DateTime(2024, 1, 15)  // Mon
            };

            var deadline = SlaCalculator.CalculateSlaDeadline(start, policy);

            Assert.Equal(new DateTimeOffset(2024, 1, 16, 16, 0, 0, TimeSpan.Zero), deadline);
        }

        [Fact]
        public void BusinessElapsed_SkipsCustomWorkingDaysAndHolidays()
        {
            // The task started on Thursday at 09:00 and was evaluated on Tuesday at 14:00.
            // Friday is removed from the working days, Monday is a holiday,
            // and Saturday/Sunday are already non-business days.
            // Business time should be 8 hours on Thursday + 7 hours on Tuesday = 15 hours.

            var start = new DateTimeOffset(2024, 1, 11, 9, 0, 0, TimeSpan.Zero); // Thu 09:00
            var evaluateAt = new DateTimeOffset(2024, 1, 16, 14, 0, 0, TimeSpan.Zero); // Tue 14:00

            var policy = MakePolicy(15);

            policy.BusinessDays = new List<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                // Friday intentionally excluded
            };

            policy.Holidays = new List<DateTime>
            {
                new DateTime(2024, 1, 15) // Monday
            };

            var result = SlaCalculator.Evaluate(start, evaluateAt, policy);

            Assert.Equal(TimeSpan.FromHours(13), result.ElapsedTime); // 8 hours on Thursday + 5 hours on Tuesday = 13 hours
        }

        [Fact]
        public void BusinessElapsed_SkipsCustomWorkingDaysAndHolidays_ReturnsDate()
        {
            // The task started on Thursday at 09:00 with a 15-hour SLA.
            // Friday is removed from the working days, Monday is a holiday,
            // and Saturday/Sunday are non-business days.
            // The deadline should be Tuesday at 16:00.

            var start = new DateTimeOffset(2024, 1, 11, 9, 0, 0, TimeSpan.Zero); // Thu 09:00
            var policy = MakePolicy(15);

            policy.BusinessDays = new List<DayOfWeek>
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday
                // Friday intentionally excluded
            };

            policy.Holidays = new List<DateTime>
            {
                new DateTime(2024, 1, 15) // Monday
            };

            var deadline = SlaCalculator.CalculateSlaDeadline(start, policy);
            // 8 hours on Thursday + 7 hours on Tuesday = 15 hours, deadline on Tuesday at 16:00            
            Assert.Equal(new DateTimeOffset(2024, 1, 16, 16, 0, 0, TimeSpan.Zero), deadline);
        }
    }
}
