using SlaCore.Models;

namespace SlaCore.Tests
{
    public class DeadlineCalculationTests
    {
        // ── Wall-clock (no business hours)

        [Fact]
        public void WallClock_Deadline_IsStartPlusDuration()
        {
            // Arrange
            var start = new DateTimeOffset(2024, 1, 15, 9, 0, 0, TimeSpan.Zero);
            var policy = SlaPolicy.High(); // 4 hours, wall-clock

            var deadline = SlaCalculator.CalculateSlaDeadline(start, policy);

            Assert.Equal(start.AddHours(4), deadline);
        }
    }
}
