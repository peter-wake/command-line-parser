using System;

// ReSharper disable once CheckNamespace
namespace CommandLine
{
    public class PrematureMatchTerminationException : Exception
    {
        public PrematureMatchTerminationException(string message) : base(message) { }
    }
}
