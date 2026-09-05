using System;
using System.Collections.Generic;
using System.Text;

namespace SlaCore.Models
{
    /// <summary>
    /// Snapshot of an SLA evaluation at a given point in time.
    /// </summary>
    public sealed class SlaResult
    {
        /// <summary> The policy that was evaluated.</summary>
        public SlaPolicy Policy { get; set; }

        /// <summary> When the task started.</summary>
        public DateTimeOffset StartedAt { get; set; }

        /// <summary> The computed deadline by which the task must complete.</summary>
        public DateTimeOffset Deadline { get; set; }

        /// <summary>Time at which this result was evaluated.</summary>
        public DateTimeOffset EvaluatedAt { get; set; }

        /// <summary>
        /// Elapsed business-time (or wall-clock time when not using business hours).
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }

        /// <summary>
        /// Time remaining before the deadline. Negative when breached.
        /// </summary>
        public TimeSpan TimeRemaining => Deadline - EvaluatedAt;

        /// <summary>
        /// Fraction of the allowed duration that has been consumed (0.0 – 1.0+).
        /// Values above 1.0 indicate a breach.
        /// </summary>
        public double ElapsedFraction =>
            Policy.AllowedDuration.TotalSeconds > 0
                ? ElapsedTime.TotalSeconds / Policy.AllowedDuration.TotalSeconds
                : 1.0;

        /// <summary>Current SLA compliance status.</summary>
        public SlaStatus Status { get; set; }

        /// <summary>Whether the SLA deadline has been passed.</summary>
        public bool IsBreached => Status == SlaStatus.Breached;

        /// <summary>Whether the task is in the warning zone but not yet breached.</summary>
        public bool IsAtRisk => Status == SlaStatus.AtRisk;

        /// <summary>Human-readable summary of this result.</summary>
        public override string ToString()
        {

            switch (Status)
            {
                case SlaStatus.Breached:
                    return $"[{Policy.Name}] BREACHED – overdue by {-TimeRemaining:hh\\:mm\\:ss}";
                case SlaStatus.AtRisk:
                    return $"[{Policy.Name}] AT RISK – {TimeRemaining:hh\\:mm\\:ss} remaining ({ElapsedFraction:P0} consumed)";
                default:
                    return $"[{Policy.Name}] ON TRACK – {TimeRemaining:hh\\:mm\\:ss} remaining ({ElapsedFraction:P0} consumed)";
            }
        }
    }
}
