using SlaCore.Calculation;
using SlaCore.Models;
using System;

namespace SlaCore
{
    /// <summary>
    /// Core SLA calculation engine.
    /// All methods are pure functions, they require no state and are thread-safe.
    /// </summary>
    public static class SlaCalculator
    {
        /// <summary>
        /// Computes the SLA deadline given a start time and a policy.
        /// </summary>
        /// <param name="startedAt"> When the task started (timezone-aware).</param>
        /// <param name="policy"> The SLA policy to apply.</param>
        /// <returns> The deadline by which the task must be completed.</returns>
        public static DateTimeOffset CalculateSlaDeadline(DateTimeOffset startedAt, SlaPolicy policy)
        {
            if(policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            return policy.UseBusinessHoursOnly
                ? BusinessHoursCalculator.AddBusinessDuration(startedAt, policy.AllowedDuration, policy)
                : startedAt.Add(policy.AllowedDuration);
        }

        /// <summary>
        /// Evaluates the SLA status of an in-progress task at the current moment.
        /// </summary>
        /// <param name="startedAt"> When the task started.</param>
        /// <param name="policy"> The SLA policy to apply.</param>
        /// <returns>A full <see cref="SlaResult"/> snapshot.</returns>
        public static SlaResult Evaluate(DateTimeOffset startedAt, SlaPolicy policy)
            => Evaluate(startedAt, DateTimeOffset.UtcNow, policy);

        /// <summary>
        /// Evaluates the SLA status of a task at a specified evaluation time.
        /// Can be used for historical analysis or testing.
        /// </summary>
        /// <param name="startedAt"> When the task started.</param>
        /// <param name="evaluateAt"> The point in time at which to evaluate the SLA.</param>
        /// <param name="policy">The SLA policy to apply.</param>
        /// <returns>A full <see cref="SlaResult"/> snapshot.</returns>
        public static SlaResult Evaluate(DateTimeOffset startedAt, DateTimeOffset evaluateAt, SlaPolicy policy)
        {
            if (policy == null)
            {
                throw new ArgumentNullException(nameof(policy));
            }

            var deadline = CalculateSlaDeadline(startedAt, policy);

            var elapsed = policy.UseBusinessHoursOnly
                ? BusinessHoursCalculator.BusinessElapsed(startedAt, evaluateAt, policy)
                : (evaluateAt > startedAt ? evaluateAt - startedAt : TimeSpan.Zero);

            var fraction = policy.AllowedDuration.TotalSeconds > 0
                ? elapsed.TotalSeconds / policy.AllowedDuration.TotalSeconds
                : 1.0;

            var status = fraction >= 1.0
                ? SlaStatus.Breached
                : fraction >= policy.WarningThreshold
                    ? SlaStatus.AtRisk
                    : SlaStatus.OnTrack;

            return new SlaResult
            {
                Policy = policy,
                StartedAt = startedAt,
                Deadline = deadline,
                EvaluatedAt = evaluateAt,
                ElapsedTime = elapsed,
                Status = status
            };
        }
    }
}
