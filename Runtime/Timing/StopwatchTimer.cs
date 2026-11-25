namespace Immersive.Foundation.Timing
{
    /// <summary>
    /// Measures elapsed time while running.
    /// </summary>
    public sealed class StopwatchTimer : Timer
    {
        public StopwatchTimer(TimerTimeMode timeMode = TimerTimeMode.Scaled)
            : base(timeMode)
        {
        }

        protected override void AdvanceTimer(double deltaTime)
        {
            AddElapsed(deltaTime);
        }
    }
}
