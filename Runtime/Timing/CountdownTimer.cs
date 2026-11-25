using System;

namespace Immersive.Foundation.Timing
{
    /// <summary>
    /// Counts down a fixed duration and notifies once when it reaches zero.
    /// </summary>
    public sealed class CountdownTimer : Timer
    {
        private readonly float _durationSeconds;
        private double _remainingSeconds;

        public CountdownTimer(float durationSeconds, TimerTimeMode timeMode = TimerTimeMode.Scaled)
            : base(timeMode)
        {
            if (float.IsNaN(durationSeconds) || float.IsInfinity(durationSeconds) || durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), durationSeconds, "Duration must be finite and greater than zero.");
            }

            _durationSeconds = durationSeconds;
            _remainingSeconds = durationSeconds;
        }

        public float DurationSeconds
        {
            get { return _durationSeconds; }
        }

        public float RemainingSeconds
        {
            get { return (float)_remainingSeconds; }
        }

        public float Progress
        {
            get
            {
                double progress = (_durationSeconds - _remainingSeconds) / _durationSeconds;
                if (progress < 0d)
                {
                    return 0f;
                }

                return progress > 1d ? 1f : (float)progress;
            }
        }

        public event Action Completed;

        protected override void AdvanceTimer(double deltaTime)
        {
            double remaining = _remainingSeconds - deltaTime;
            if (remaining <= 0d)
            {
                _remainingSeconds = 0f;
                SetElapsed(_durationSeconds);
                MarkCompleted();

                Action completed = Completed;
                if (completed != null)
                {
                    completed();
                }

                return;
            }

            _remainingSeconds = remaining;
            AddElapsed(deltaTime);
        }

        protected override void ResetTimerState()
        {
            _remainingSeconds = _durationSeconds;
        }
    }
}
