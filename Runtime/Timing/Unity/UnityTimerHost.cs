using Immersive.Foundation.Timing;
using UnityEngine;

namespace Immersive.Foundation.Timing.Unity
{
    /// <summary>
    /// Advances explicitly registered Foundation timers from Unity frame time.
    /// </summary>
    public sealed class UnityTimerHost : MonoBehaviour
    {
        private readonly TimerScheduler _scheduler = new TimerScheduler();

        public bool Register(Timer timer)
        {
            return _scheduler.Register(timer);
        }

        public bool Unregister(Timer timer)
        {
            return _scheduler.Unregister(timer);
        }

        private void Update()
        {
            _scheduler.Tick(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            _scheduler.Clear();
        }
    }
}
