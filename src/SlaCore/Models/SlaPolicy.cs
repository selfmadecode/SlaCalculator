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
        /// <summary>
        /// Gets or sets the allowed duration for the SLA.
        /// </summary>
        public TimeSpan AllowedDuration { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether only business hours should be considered for the SLA.
        /// </summary>
        public bool UseBusinessHoursOnly { get; set; }
    }
}
