using System;

namespace Immersive.Foundation.Timing
{
    /// <summary>
    /// Reports how many fixed intervals elapsed during each scheduler advance.
    /// </summary>
    public sealed class FrequencyTimer : Timer
    {
        private readonly double _intervalSeconds;
        private double _remainderSeconds;

        public FrequencyTimer(float intervalSeconds, TimerTimeMode timeMode = TimerTimeMode.Scaled)
            : base(timeMode)
        {
            if (float.IsNaN(intervalSeconds) || float.IsInfinity(intervalSeconds) || intervalSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), intervalSeconds, "Interval must be finite and greater than zero.");
            }

            IntervalSeconds = intervalSeconds;
            _intervalSeconds = intervalSeconds;
        }

        public float IntervalSeconds { get; }

        /// <summary>
        /// Reports elapsed interval counts once per scheduler advance. Very large counts saturate at <see cref="ulong.MaxValue"/>.
        /// </summary>
        public event Action<ulong> Tick;

        protected override void AdvanceTimer(double deltaTime)
        {
            AddElapsed(deltaTime);

            double total = _remainderSeconds + deltaTime;
            double elapsedIntervals = Math.Floor(total / _intervalSeconds);
            if (elapsedIntervals < 1d)
            {
                _remainderSeconds = total;
                return;
            }

            _remainderSeconds = total % _intervalSeconds;
            ulong intervalCount = elapsedIntervals >= ulong.MaxValue
                ? ulong.MaxValue
                : (ulong)elapsedIntervals;

            Action<ulong> tick = Tick;
            if (tick != null)
            {
                tick(intervalCount);
            }
        }

        protected override void ResetTimerState()
        {
            _remainderSeconds = 0d;
        }
    }
}
