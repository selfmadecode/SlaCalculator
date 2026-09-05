using SlaCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlaCore.Runner
{
    public static class TestAssert
    {
        public static void Equal<T>(T expected, T actual, string? msg = null)
        {
            if (!Equals(expected, actual))
                throw new Exception(msg ?? $"Expected {expected}, got {actual}");
        }

        public static void True(bool condition, string? msg = null)
        {
            if (!condition)
                throw new Exception(msg ?? "Expected true");
        }

        public static void False(bool condition, string? msg = null)
        {
            if (condition)
                throw new Exception(msg ?? "Expected false");
        }

        public static void Null(object? obj, string? msg = null)
        {
            if (obj is not null)
                throw new Exception(msg ?? "Expected null");
        }

        public static void NotNull(object? obj, string? msg = null)
        {
            if (obj is null)
                throw new Exception(msg ?? "Expected non-null");
        }

        public static void Throws<TEx>(Action action, string? msg = null) where TEx : Exception
        {
            try { action();
                throw new Exception(msg ?? $"Expected {typeof(TEx).Name} but no exception thrown"); }
            catch (TEx) { /* expected */ }
        }

        public static void EqualApprox(double expected, double actual, double tolerance = 0.0001, string? msg = null)
        {
            if (Math.Abs(expected - actual) > tolerance)
                throw new Exception(msg ?? $"Expected ~{expected}, got {actual}");
        }

        public static void EqualTimeSpan(TimeSpan expected, TimeSpan actual, string? msg = null)
        {
            if (Math.Abs((expected - actual).TotalSeconds) > 1)
                throw new Exception(msg ?? $"Expected {expected}, got {actual}");
        }
    }
}
