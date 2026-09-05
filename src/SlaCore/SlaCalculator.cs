using SlaCore.Calculation;
using SlaCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // ── Evaluation of SLA Status

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

        // ── Remaining time for SLA

        /// <summary>
        /// Returns how much time is remaining before the SLA deadline.
        /// Returns a negative <see cref="TimeSpan"/> if already breached.
        /// </summary>
        public static TimeSpan GetTimeRemaining(DateTimeOffset startedAt, SlaPolicy policy)
            => GetTimeRemaining(startedAt, DateTimeOffset.UtcNow, policy);

        /// <summary>
        /// Returns how much time is remaining before the SLA deadline at
        /// <paramref name="evaluateAt"/>.
        /// </summary>
        public static TimeSpan GetTimeRemaining(DateTimeOffset startedAt, DateTimeOffset evaluateAt, SlaPolicy policy)
        {
            var deadline = CalculateSlaDeadline(startedAt, policy);
            return deadline - evaluateAt;
        }

        // ── Breach check 

        /// <summary>
        /// Returns <c>true</c> when the task has exceeded its SLA deadline.
        /// </summary>
        /// 
        public static bool IsBreached(DateTimeOffset startedAt, SlaPolicy policy)
            => IsBreached(startedAt, DateTimeOffset.UtcNow, policy);

        /// <summary>
        /// Returns <c>true</c> when the task exceeded its SLA deadline at
        /// <paramref name="evaluateAt"/>.
        /// </summary>
        public static bool IsBreached(DateTimeOffset startedAt, DateTimeOffset evaluateAt, SlaPolicy policy)
            => Evaluate(startedAt, evaluateAt, policy).IsBreached;

        // ── Batch / reporting 

        /// <summary>
        /// Evaluates a collection of (startedAt, policy) pairs and returns
        /// aggregated compliance metrics.
        /// </summary>
        /// <param name="tasks">Sequence of start-time/policy tuples.</param>
        /// <param name="evaluateAt">
        /// Point in time to evaluate at. Defaults to <see cref="DateTimeOffset.UtcNow"/>.
        /// </param>
        public static SlaBatchMetrics EvaluateBatch(IEnumerable<(DateTimeOffset StartedAt, SlaPolicy Policy)> tasks, DateTimeOffset? evaluateAt = null)
        {
            if(tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            var at = evaluateAt ?? DateTimeOffset.UtcNow;
            var results = tasks.Select(t => Evaluate(t.StartedAt, at, t.Policy)).ToList();

            if (results.Count == 0)
                return new SlaBatchMetrics { TotalTasks = 0 };

            var met = results.Count(r => !r.IsBreached);
            var breached = results.Count(r => r.IsBreached);
            var atRisk = results.Count(r => r.IsAtRisk);
            var avgElapsed = TimeSpan.FromSeconds(results.Average(r => r.ElapsedTime.TotalSeconds));
            var worst = results.OrderByDescending(r => r.ElapsedFraction).First();

            return new SlaBatchMetrics
            {
                TotalTasks = results.Count,
                MetSla = met,
                BreachedSla = breached,
                AtRiskTasks = atRisk,
                AverageElapsedTime = avgElapsed,
                WorstPerformer = worst
            };
        }
    }
}
