using System;
using System.Collections.Generic;

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
        /// <summary>
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

        private static SlaPolicy Create(string id, string name, TimeSpan allowedDuration,
            bool businessHoursOnly, Action<SlaPolicy> configure)
        {
            var policy = new SlaPolicy
            {
                Id = id,
                Name = name,
                AllowedDuration = allowedDuration,
                UseBusinessHoursOnly = businessHoursOnly
            };

            configure?.Invoke(policy);

            return policy;
        }

        /// <summary>
        /// Creates a Critical SLA policy with a default allowed duration of 1 hour.
        /// </summary>
        /// <param name="businessHoursOnly">
        /// Specifies whether the SLA duration should be calculated using business hours only.
        /// Defaults to <c>false</c>.
        /// </param>
        /// <param name="configure">
        /// An optional callback that can be used to override the default policy settings.
        /// </param>
        /// <returns>A configured Critical SLA policy.</returns>
        public static SlaPolicy Critical(bool businessHoursOnly = false, Action<SlaPolicy> configure = null)
            => Create("P1", "Critical", TimeSpan.FromHours(1), businessHoursOnly, configure);

        /// <summary>
        /// Creates a High SLA policy with a default allowed duration of 4 hours.
        /// </summary>
        /// <param name="businessHoursOnly">
        /// Specifies whether the SLA duration should be calculated using business hours only.
        /// Defaults to <c>false</c>.
        /// </param>
        /// <param name="configure">
        /// An optional callback that can be used to override the default policy settings.
        /// </param>
        /// <returns>A configured High SLA policy.</returns>
        public static SlaPolicy High(bool businessHoursOnly = false, Action<SlaPolicy> configure = null)
            => Create("P2", "High", TimeSpan.FromHours(4), businessHoursOnly, configure);

        /// <summary>
        /// Creates a Medium SLA policy with a default allowed duration of 8 hours.
        /// </summary>
        /// <param name="businessHoursOnly">
        /// Specifies whether the SLA duration should be calculated using business hours only.
        /// Defaults to <c>false</c>.
        /// </param>
        /// <param name="configure">
        /// An optional callback that can be used to override the default policy settings.
        /// </param>
        /// <returns>A configured Medium SLA policy.</returns>
        public static SlaPolicy Medium(bool businessHoursOnly = false, Action<SlaPolicy> configure = null)
            => Create("P3", "Medium", TimeSpan.FromHours(8), businessHoursOnly, configure);

        /// <summary>
        /// Creates a Low SLA policy with a default allowed duration of 24 hours.
        /// </summary>
        /// <param name="businessHoursOnly">
        /// Specifies whether the SLA duration should be calculated using business hours only.
        /// Defaults to <c>false</c>.
        /// </param>
        /// <param name="configure">
        /// An optional callback that can be used to override the default policy settings.
        /// </param>
        /// <returns>A configured Low SLA policy.</returns>
        public static SlaPolicy Low(bool businessHoursOnly = false, Action<SlaPolicy> configure = null)
            => Create("P4", "Low", TimeSpan.FromHours(24), businessHoursOnly, configure);
    }
}
