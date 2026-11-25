using System;

namespace Immersive.Foundation.Timing
{
    /// <summary>
    /// Base state and lifecycle shared by Foundation timers.
    /// </summary>
    public abstract class Timer
    {
        private double _elapsedSeconds;
        private bool _isRunning;
        private bool _isPaused;
        private bool _isCompleted;
        private TimerScheduler _schedulerOwner;

        protected Timer(TimerTimeMode timeMode = TimerTimeMode.Scaled)
        {
            if (!Enum.IsDefined(typeof(TimerTimeMode), timeMode))
            {
                throw new ArgumentOutOfRangeException(nameof(timeMode), timeMode, "The timer time mode is not defined.");
            }

            TimeMode = timeMode;
        }

        public TimerTimeMode TimeMode { get; }

        public bool IsRunning
        {
            get { return _isRunning; }
        }

        public bool IsPaused
        {
            get { return _isPaused; }
        }

        public virtual bool IsCompleted
        {
            get { return _isCompleted; }
        }

        /// <summary>
        /// Gets total elapsed seconds, clamped to the largest finite float value.
        /// </summary>
        public float ElapsedSeconds
        {
            get { return (float)_elapsedSeconds; }
        }

        /// <summary>
        /// Starts a new run from the timer's initial state.
        /// </summary>
        public void Start()
        {
            Reset();
            _isRunning = true;
        }

        public void Pause()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            _isPaused = true;
        }

        public void Resume()
        {
            if (!_isPaused)
            {
                return;
            }

            _isPaused = false;
            _isRunning = true;
        }

        /// <summary>
        /// Stops advancing while preserving the current elapsed and remaining values.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
        }

        /// <summary>
        /// Stops the timer and restores its initial values.
        /// </summary>
        public void Reset()
        {
            _elapsedSeconds = 0d;
            _isRunning = false;
            _isPaused = false;
            _isCompleted = false;
            ResetTimerState();
        }

        internal TimerScheduler SchedulerOwner
        {
            get { return _schedulerOwner; }
        }

        internal void SetSchedulerOwner(TimerScheduler schedulerOwner)
        {
            _schedulerOwner = schedulerOwner;
        }

        internal void Advance(float deltaTime)
        {
            if (!_isRunning || deltaTime <= 0f)
            {
                return;
            }

            AdvanceTimer(deltaTime);
        }

        protected abstract void AdvanceTimer(double deltaTime);

        protected virtual void ResetTimerState()
        {
        }

        protected void AddElapsed(double deltaTime)
        {
            double nextElapsed = _elapsedSeconds + deltaTime;
            _elapsedSeconds = nextElapsed >= float.MaxValue ? float.MaxValue : nextElapsed;
        }

        protected void SetElapsed(double elapsedSeconds)
        {
            _elapsedSeconds = elapsedSeconds >= float.MaxValue ? float.MaxValue : elapsedSeconds;
        }

        protected void MarkCompleted()
        {
            _isRunning = false;
            _isPaused = false;
            _isCompleted = true;
        }
    }
}
