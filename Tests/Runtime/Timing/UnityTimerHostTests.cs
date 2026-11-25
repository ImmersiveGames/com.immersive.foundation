using System.Collections;
using Immersive.Foundation.Timing;
using Immersive.Foundation.Timing.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Immersive.Foundation.Tests.Timing
{
    public sealed class UnityTimerHostTests
    {
        [UnityTest]
        public IEnumerator UnityTimerHostAdvancesScaledAndUnscaledTimersAndReleasesOnDestroy()
        {
            float previousTimeScale = Time.timeScale;
            GameObject hostObject = new GameObject("Foundation Timer Host Test");
            GameObject replacementObject = null;

            try
            {
                UnityTimerHost host = hostObject.AddComponent<UnityTimerHost>();
                var scaled = new StopwatchTimer(TimerTimeMode.Scaled);
                var unscaled = new StopwatchTimer(TimerTimeMode.Unscaled);
                Assert.That(host.Register(scaled), Is.True);
                Assert.That(host.Register(unscaled), Is.True);
                scaled.Start();
                unscaled.Start();

                Time.timeScale = 0f;
                yield return null;

                Assert.That(scaled.ElapsedSeconds, Is.EqualTo(0f));
                Assert.That(unscaled.ElapsedSeconds, Is.GreaterThan(0f));

                Object.Destroy(hostObject);
                yield return null;

                replacementObject = new GameObject("Foundation Timer Host Replacement Test");
                UnityTimerHost replacement = replacementObject.AddComponent<UnityTimerHost>();
                Assert.That(replacement.Register(scaled), Is.True);
                Assert.That(replacement.Register(unscaled), Is.True);
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                if (hostObject != null)
                {
                    Object.Destroy(hostObject);
                }

                if (replacementObject != null)
                {
                    Object.Destroy(replacementObject);
                }
            }
        }
    }
}
