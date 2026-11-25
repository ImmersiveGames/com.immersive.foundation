using System;
using System.Collections.Generic;

namespace Immersive.Foundation.Timing
{
    /// <summary>
    /// Advances explicitly registered timers using scaled and unscaled elapsed time.
    /// </summary>
    public sealed class TimerScheduler
    {
        private readonly List<Registration> _registrations = new List<Registration>();

        public bool Register(Timer timer)
        {
            if (timer == null)
            {
                throw new ArgumentNullException(nameof(timer));
            }

            if (FindRegistration(timer) != null)
            {
                return false;
            }

            if (timer.SchedulerOwner != null && !ReferenceEquals(timer.SchedulerOwner, this))
            {
                throw new InvalidOperationException("The timer is already owned by another scheduler.");
            }

            timer.SetSchedulerOwner(this);
            _registrations.Add(new Registration(timer));
            return true;
        }

        public bool Unregister(Timer timer)
        {
            if (timer == null)
            {
                throw new ArgumentNullException(nameof(timer));
            }

            Registration registration = FindRegistration(timer);
            if (registration == null)
            {
                return false;
            }

            _registrations.Remove(registration);
            timer.SetSchedulerOwner(null);
            return true;
        }

        /// <summary>
        /// Releases all timer registrations without resetting timer state.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _registrations.Count; i++)
            {
                _registrations[i].Timer.SetSchedulerOwner(null);
            }

            _registrations.Clear();
        }

        /// <summary>
        /// Advances running timers by their selected time domain.
        /// </summary>
        public void Tick(float scaledDeltaTime, float unscaledDeltaTime)
        {
            ValidateDelta(scaledDeltaTime, nameof(scaledDeltaTime));
            ValidateDelta(unscaledDeltaTime, nameof(unscaledDeltaTime));

            Registration[] snapshot = _registrations.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
            {
                Registration registration = snapshot[i];
                if (!ContainsRegistration(registration))
                {
                    continue;
                }

                float deltaTime = registration.Timer.TimeMode == TimerTimeMode.Scaled
                    ? scaledDeltaTime
                    : unscaledDeltaTime;
                registration.Timer.Advance(deltaTime);
            }
        }

        private static void ValidateDelta(float deltaTime, string parameterName)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(parameterName, deltaTime, "Elapsed time must be finite and non-negative.");
            }
        }

        private Registration FindRegistration(Timer timer)
        {
            for (int i = 0; i < _registrations.Count; i++)
            {
                if (ReferenceEquals(_registrations[i].Timer, timer))
                {
                    return _registrations[i];
                }
            }

            return null;
        }

        private bool ContainsRegistration(Registration registration)
        {
            for (int i = 0; i < _registrations.Count; i++)
            {
                if (ReferenceEquals(_registrations[i], registration))
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class Registration
        {
            public Registration(Timer timer)
            {
                Timer = timer;
            }

            public Timer Timer { get; }
        }
    }
}
