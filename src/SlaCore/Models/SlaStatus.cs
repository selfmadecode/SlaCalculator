using System;
using System.Collections.Generic;
using System.Text;

namespace SlaCore.Models
{
    /// <summary>
    /// Represents the compliance state of a task against its SLA policy.
    /// </summary>
    public enum SlaStatus
    {
        /// <summary>The task is within the allowed duration with no warning yet.</summary>
        OnTrack,

        /// <summary>
        /// The task has consumed more than <see cref="SlaPolicy.WarningThreshold"/> of its
        /// allowed duration but has not yet breached.
        /// </summary>
        AtRisk,

        /// <summary>The allowed duration has been exceeded and the SLA is breached.</summary>
        Breached
    }
}
