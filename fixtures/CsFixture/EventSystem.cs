using System;
using System.Collections.Generic;

namespace Fixtures
{
    // Covers: class inheriting EventArgs
    public class ThresholdReachedEventArgs : EventArgs
    {
        public int Threshold { get; }
        public DateTime TimeReached { get; }

        public ThresholdReachedEventArgs(int threshold)
        {
            Threshold = threshold;
            TimeReached = DateTime.UtcNow;
        }
    }

    // Covers: event declaration, event invocation, protected virtual raise pattern
    public class Counter
    {
        public event EventHandler<ThresholdReachedEventArgs>? ThresholdReached;

        private int _count;
        private readonly int _threshold;

        public Counter(int threshold)
        {
            _threshold = threshold;
        }

        public void Increment()
        {
            _count++;
            if (_count >= _threshold)
                OnThresholdReached(new ThresholdReachedEventArgs(_threshold));
        }

        protected virtual void OnThresholdReached(ThresholdReachedEventArgs e)
        {
            ThresholdReached?.Invoke(this, e);
        }

        public int Count => _count;

        public void Reset() => _count = 0;
    }

    // Covers: event subscription via += in constructor, handler method, cross-method call
    public class Alarm
    {
        private readonly Counter _counter;

        public Alarm(Counter counter)
        {
            _counter = counter;
            _counter.ThresholdReached += Counter_ThresholdReached;
        }

        private void Counter_ThresholdReached(object? sender, ThresholdReachedEventArgs e)
        {
            TriggerAlarm(e.Threshold);
        }

        private void TriggerAlarm(int threshold)
        {
            Console.WriteLine($"Alarm triggered at threshold {threshold}");
        }

        public void Detach()
        {
            _counter.ThresholdReached -= Counter_ThresholdReached;
        }
    }

    // Covers: second subscriber to same event source
    public class Logger
    {
        private readonly Counter _counter;
        private readonly List<string> _log = new();

        public Logger(Counter counter)
        {
            _counter = counter;
            _counter.ThresholdReached += Counter_ThresholdReached;
        }

        private void Counter_ThresholdReached(object? sender, ThresholdReachedEventArgs e)
        {
            RecordEvent(e.Threshold);
        }

        private void RecordEvent(int threshold)
        {
            _log.Add($"Threshold {threshold} at {DateTime.UtcNow}");
        }

        public IReadOnlyList<string> Log => _log;
    }
}
