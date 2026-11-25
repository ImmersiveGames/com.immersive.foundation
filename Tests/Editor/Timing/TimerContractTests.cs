using System;
using Immersive.Foundation.Timing;
using NUnit.Framework;

namespace Immersive.Foundation.Tests.Timing
{
    public sealed class TimerContractTests
    {
        [Test]
        public void CountdownCompletesOnceAtZero()
        {
            var timer = new CountdownTimer(1f);
            var scheduler = new TimerScheduler();
            int completedCount = 0;
            timer.Completed += () => completedCount++;
            scheduler.Register(timer);
            timer.Start();

            scheduler.Tick(2f, 2f);
            scheduler.Tick(1f, 1f);

            Assert.That(timer.RemainingSeconds, Is.EqualTo(0f));
            Assert.That(timer.IsCompleted, Is.True);
            Assert.That(timer.IsRunning, Is.False);
            Assert.That(completedCount, Is.EqualTo(1));
        }

        [Test]
        public void CountdownPauseResumeReset()
        {
            var timer = new CountdownTimer(1f);
            var scheduler = new TimerScheduler();
            scheduler.Register(timer);
            timer.Start();
            scheduler.Tick(0.25f, 0.25f);
            timer.Pause();
            scheduler.Tick(0.5f, 0.5f);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(0.75f).Within(0.0001f));

            timer.Resume();
            scheduler.Tick(0.25f, 0.25f);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(0.5f).Within(0.0001f));
            timer.Stop();
            scheduler.Tick(0.25f, 0.25f);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(0.5f).Within(0.0001f));

            timer.Reset();
            Assert.That(timer.RemainingSeconds, Is.EqualTo(1f));
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0f));
            Assert.That(timer.IsRunning, Is.False);
            Assert.That(timer.IsCompleted, Is.False);
        }

        [Test]
        public void CountdownAccumulatesSubUlpSteps()
        {
            var timer = new CountdownTimer(1f);
            var scheduler = new TimerScheduler();
            scheduler.Register(timer);
            timer.Start();

            for (int i = 0; i < 20; i++)
            {
                scheduler.Tick(1e-8f, 1e-8f);
            }

            Assert.That(timer.ElapsedSeconds, Is.GreaterThan(0f));
            Assert.That(timer.RemainingSeconds, Is.LessThan(1f));
        }

        [Test]
        public void StopwatchAccumulatesOnlyWhileRunning()
        {
            var timer = new StopwatchTimer();
            var scheduler = new TimerScheduler();
            scheduler.Register(timer);
            timer.Start();
            scheduler.Tick(0.25f, 0.25f);
            timer.Pause();
            scheduler.Tick(0.5f, 0.5f);
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0.25f).Within(0.0001f));

            timer.Resume();
            scheduler.Tick(0.25f, 0.25f);
            timer.Stop();
            scheduler.Tick(0.5f, 0.5f);
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0.5f).Within(0.0001f));

            timer.Start();
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0f));
            Assert.That(timer.IsRunning, Is.True);
        }

        [Test]
        public void FrequencyReportsIntervalsAndKeepsRemainder()
        {
            var timer = new FrequencyTimer(0.5f);
            var scheduler = new TimerScheduler();
            ulong totalIntervals = 0;
            int notifications = 0;
            timer.Tick += intervals =>
            {
                totalIntervals += intervals;
                notifications++;
            };
            scheduler.Register(timer);
            timer.Start();

            scheduler.Tick(1.2f, 1.2f);
            scheduler.Tick(0.3f, 0.3f);

            Assert.That(notifications, Is.EqualTo(2));
            Assert.That(totalIntervals, Is.EqualTo(3UL));
        }

        [Test]
        public void FrequencyHandlesHugeDeltaWithoutLooping()
        {
            var timer = new FrequencyTimer(1f);
            var scheduler = new TimerScheduler();
            int notifications = 0;
            ulong elapsedIntervals = 0;
            timer.Tick += intervals =>
            {
                notifications++;
                elapsedIntervals = intervals;
            };
            scheduler.Register(timer);
            timer.Start();

            scheduler.Tick(float.MaxValue, float.MaxValue);

            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(elapsedIntervals, Is.EqualTo(ulong.MaxValue));
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(float.MaxValue));
        }

        [Test]
        public void ConstructorsRejectInvalidDurationsAndModes()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CountdownTimer(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FrequencyTimer(float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => new StopwatchTimer((TimerTimeMode)99));

            Assert.That(new CountdownTimer(1f).TimeMode, Is.EqualTo(TimerTimeMode.Scaled));
            Assert.That(new StopwatchTimer().TimeMode, Is.EqualTo(TimerTimeMode.Scaled));
            Assert.That(new FrequencyTimer(1f).TimeMode, Is.EqualTo(TimerTimeMode.Scaled));
        }

        [Test]
        public void SchedulerSelectsScaledOrUnscaledDelta()
        {
            var scheduler = new TimerScheduler();
            var scaled = new CountdownTimer(2f, TimerTimeMode.Scaled);
            var unscaled = new CountdownTimer(2f, TimerTimeMode.Unscaled);
            scheduler.Register(scaled);
            scheduler.Register(unscaled);
            scaled.Start();
            unscaled.Start();

            scheduler.Tick(0.5f, 1f);

            Assert.That(scaled.RemainingSeconds, Is.EqualTo(1.5f));
            Assert.That(unscaled.RemainingSeconds, Is.EqualTo(1f));
        }

        [Test]
        public void SchedulerRejectsInvalidDeltaBeforeAdvancingAnyTimer()
        {
            var scheduler = new TimerScheduler();
            var scaled = new StopwatchTimer(TimerTimeMode.Scaled);
            var unscaled = new StopwatchTimer(TimerTimeMode.Unscaled);
            scheduler.Register(scaled);
            scheduler.Register(unscaled);
            scaled.Start();
            unscaled.Start();

            Assert.Throws<ArgumentOutOfRangeException>(() => scheduler.Tick(0.5f, float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => scheduler.Tick(-0.5f, 0.5f));

            Assert.That(scaled.ElapsedSeconds, Is.EqualTo(0f));
            Assert.That(unscaled.ElapsedSeconds, Is.EqualTo(0f));
        }

        [Test]
        public void SchedulerClearsOwnershipWithoutResettingTimerState()
        {
            var timer = new StopwatchTimer();
            var first = new TimerScheduler();
            first.Register(timer);
            timer.Start();
            first.Tick(0.5f, 0.5f);
            first.Clear();

            Assert.That(timer.ElapsedSeconds, Is.EqualTo(0.5f));
            Assert.That(timer.IsRunning, Is.True);

            var second = new TimerScheduler();
            Assert.That(second.Register(timer), Is.True);
            second.Tick(0.5f, 0.5f);
            Assert.That(timer.ElapsedSeconds, Is.EqualTo(1f));
        }

        [Test]
        public void SchedulerRejectsASecondOwner()
        {
            var timer = new StopwatchTimer();
            var first = new TimerScheduler();
            var second = new TimerScheduler();

            Assert.That(first.Register(timer), Is.True);
            Assert.That(first.Register(timer), Is.False);
            Assert.Throws<InvalidOperationException>(() => second.Register(timer));
            Assert.That(first.Unregister(timer), Is.True);
            Assert.That(second.Register(timer), Is.True);
        }

        [Test]
        public void SchedulerSkipsAnUnregisteredTimerDuringTheSameTick()
        {
            var scheduler = new TimerScheduler();
            var frequency = new FrequencyTimer(0.25f);
            var stopwatch = new StopwatchTimer();
            frequency.Tick += _ => scheduler.Unregister(stopwatch);
            scheduler.Register(frequency);
            scheduler.Register(stopwatch);
            frequency.Start();
            stopwatch.Start();

            scheduler.Tick(0.5f, 0.5f);

            Assert.That(stopwatch.ElapsedSeconds, Is.EqualTo(0f));
            Assert.That(scheduler.Unregister(stopwatch), Is.False);
        }

        [Test]
        public void SchedulerPropagatesSubscriberExceptions()
        {
            var timer = new CountdownTimer(1f);
            var scheduler = new TimerScheduler();
            timer.Completed += () => throw new InvalidOperationException("subscriber failure");
            scheduler.Register(timer);
            timer.Start();

            Assert.Throws<InvalidOperationException>(() => scheduler.Tick(1f, 1f));
            Assert.That(timer.IsCompleted, Is.True);
            Assert.That(timer.RemainingSeconds, Is.EqualTo(0f));
        }
    }
}
