using System;
using System.Collections.Generic;
using System.Text;

namespace SlaCore.Models
{
    /// <summary>
    /// Aggregate SLA compliance metrics calculated across a collection of tasks.
    /// </summary>
    public sealed class SlaBatchMetrics
    {
        /// <summary> Total number of tasks evaluated.</summary>
        public int TotalTasks { get; set; }

        /// <summary> Number of tasks that met their SLA.</summary>
        public int MetSla { get; set; }

        /// <summary> Number of tasks that breached their SLA.</summary>
        public int BreachedSla { get; set; }

        /// <summary>Number of tasks currently in the warning zone.</summary>
        public int AtRiskTasks { get; set; }

        /// <summary> Percentage of tasks that met their SLA (0–100).</summary>
        public double ComplianceRate =>
            TotalTasks > 0 ? (double)MetSla / TotalTasks * 100 : 0;

        /// <summary> Average elapsed time across all evaluated tasks.</summary>
        public TimeSpan AverageElapsedTime { get; set; }

        /// <summary> The single task with the greatest elapsed time.</summary>
        public SlaResult WorstPerformer { get; set; }
    }
}
