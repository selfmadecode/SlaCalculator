using System;
using System.Collections.Generic;
using System.Text;

namespace SlaCore.Models
{
    /// <summary>
    /// Defines the rules for calculating the Service Level Agreement (SLA) for a given process or task.
    /// </summary>
    public class SlaPolicy
    {
        private static readonly IReadOnlyList<DayOfWeek> DefaultBusinessDays =
        new List<DayOfWeek>
        {
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday
        };
        /// <summary> Unique identifier for this policy (e.g. "P1", "Gold").</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary> Policy name.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the allowed duration for the SLA.
        /// </summary>
        public TimeSpan AllowedDuration { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether only business hours should be considered for the SLA.
        /// </summary>
        public bool UseBusinessHoursOnly { get; set; }
        // <summary>
        /// Gets or sets the holidays that should be excluded from the SLA calculation.
        /// </summary>
        public IReadOnlyList<DateTime> Holidays { get; set; } = new List<DateTime>();
        /// <summary>
        /// Gets or sets the days considered business days.
        /// If not specified, Monday through Friday are used.
        /// </summary>
        public IReadOnlyList<DayOfWeek> BusinessDays { get; set; }
        /// <summary>
        /// Start of the business day (local time). Default 09:00.
        /// </summary>
        public TimeSpan BusinessHoursStart { get; set; } = new TimeSpan(9, 0, 0);

        /// <summary>
        /// End of the business day (local time). Default 17:00.
        /// </summary>
        public TimeSpan BusinessHoursEnd { get; set; } = new TimeSpan(17, 0, 0);

        /// <summary>
        /// Gets or sets the fraction of the allowed SLA duration at which a warning is raised.
        /// For example, 0.80 raises a warning when 80% of the allowed duration has elapsed,
        /// leaving 20% of the SLA duration remaining. Default is 0.80 (80%).
        /// </summary>
        public double WarningThreshold { get; set; } = 0.80;

        public SlaPolicy()
        {
            BusinessDays = DefaultBusinessDays;
        }
    }
}
