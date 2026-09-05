using SlaCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlaCore.Calculation
{
    /// <summary>
    /// Handles all business-hours elapsed-time and deadline arithmetic.
    /// </summary>
    internal static class BusinessHoursCalculator
    {
        /// <summary>
        /// Calculates the deadline by adding <paramref name="duration"/> of business time
        /// to <paramref name="start"/>, skipping non-business days and hours.
        /// </summary>
        internal static DateTimeOffset AddBusinessDuration(DateTimeOffset start, TimeSpan duration, SlaPolicy policy)
        {
            if (duration <= TimeSpan.Zero)
                return start;

            var current = MoveToNextBusinessDateStartTime(start, policy);
            var remaining = duration;

            while (remaining > TimeSpan.Zero)
            {
                var dayEnd = new DateTimeOffset(current.Date + policy.BusinessHoursEnd, current.Offset);

                if (current >= dayEnd)
                {
                    current = NextBusinessDayStart(current, policy);
                    continue;
                }

                var availableToday = dayEnd - current;

                if (remaining <= availableToday)
                {
                    current = current.Add(remaining);
                    remaining = TimeSpan.Zero;
                }
                else
                {
                    remaining -= availableToday;
                    current = NextBusinessDayStart(current, policy);
                }
            }

            return current;
        }


        // Helper methods to determine business days and hours
        private static DateTimeOffset MoveToNextBusinessDateStartTime(DateTimeOffset dt, SlaPolicy policy)
        {
            if (IsWithinBusinessHours(dt, policy))
                return dt;

            if (IsBusinessDay(dt, policy))
            {
                var startToday = new DateTimeOffset(dt.Date + policy.BusinessHoursStart, dt.Offset);

                if (dt < startToday)
                    return startToday;
            }

            return NextBusinessDayStart(dt, policy);
        }

        private static bool IsWithinBusinessHours(DateTimeOffset dt, SlaPolicy policy)
        {
            if (!IsBusinessDay(dt, policy))
                return false;

            var time = dt.TimeOfDay;
            return time >= policy.BusinessHoursStart && time < policy.BusinessHoursEnd;
        }
  
        private static bool IsBusinessDay(DateTimeOffset dt, SlaPolicy policy)
        {
            if (!policy.BusinessDays.Contains(dt.DayOfWeek))
                return false;

            return !policy.Holidays.Contains(dt.Date);
        }

        private static DateTimeOffset NextBusinessDayStart(DateTimeOffset dt, SlaPolicy policy)
        {
            // start checking from the day after the specified date.
            var nextDay = dt.Date.AddDays(1);
            for (var i = 0; i < 14; i++)
            {
                var candidate = new DateTimeOffset(nextDay + policy.BusinessHoursStart, dt.Offset);
                if (IsBusinessDay(candidate, policy))
                    return candidate;

                nextDay = nextDay.AddDays(1);
            }
            throw new InvalidOperationException("Could not find a valid business day within 14 days. " +
                "Check your BusinessDays and Holidays configuration.");
        }
    }
}
