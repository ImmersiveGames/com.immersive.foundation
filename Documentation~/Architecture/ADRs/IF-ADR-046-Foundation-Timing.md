# ADR IF-ADR-046 — Foundation timing primitives and Unity host

- Status: Accepted
- Date: 2026-11-25

## Context

Foundation `0.2.3` provides generic validation, events, FSM, and common primitives. The project needs reusable Countdown, Stopwatch, and Frequency timers without a third-party package dependency. Timer state should be reusable in standalone Unity projects and remain independent of Immersive Framework types.

The Framework already owns pause and application lifecycle behavior. Foundation must not choose an application, route, or activity lifetime for consumers. Standalone Unity consumers still need an explicit component that advances timers automatically each frame.

## Decision

- Add `Timer`, `CountdownTimer`, `StopwatchTimer`, `FrequencyTimer`, `TimerTimeMode`, and `TimerScheduler` to the engine-independent `Immersive.Foundation.Runtime` assembly.
- Each timer is explicitly registered with one scheduler at a time. The scheduler receives scaled and unscaled elapsed seconds, validates both values before advancing any timer, and selects the value from each timer's time mode.
- Timer instances start stopped. `Start` resets and starts; `Pause` and `Resume` suspend and continue; `Stop` retains current values; `Reset` restores the stopped initial values.
- Countdown clamps to zero, stops, and invokes its synchronous completion event once. Frequency reports the number of elapsed intervals once per scheduler advance, preserves the fractional remainder, and saturates counts beyond `ulong.MaxValue` without an unbounded catch-up loop.
- Default to scaled time. A timer can opt into unscaled time for UI or menu behavior.
- Add `UnityTimerHost` in the separate `Immersive.Foundation.Timing.Unity` assembly. Consumers add it explicitly and register timers. It advances the scheduler with `Time.deltaTime` and `Time.unscaledDeltaTime`; destruction releases registrations without resetting timer state. It does not install PlayerLoop code, create a global registry, or persist across scenes.
- Keep Framework lifecycle integration and its Foundation dependency update in a later, separate Framework task. The Framework currently pins Foundation `0.2.3`; this change prepares Foundation `0.3.0` in this branch but does not change or publish the Framework package.
- Do not copy or depend on `Unity-Improved-Timers` source or package.

## Alternatives considered

- **Only consumer-driven `Tick(deltaTime)`:** keeps all consumers engine-independent but does not provide the requested automatic Unity frame updates by itself.
- **Static manager or automatic PlayerLoop insertion:** resembles the reference package but hides lifetime and cleanup in global state, so it conflicts with Foundation's ownership boundary.
- **Reference the Framework from Foundation:** would prevent standalone use and reverse the dependency direction.
- **Add a third-party timer package:** rejected because the requested capability must be owned by Foundation.

## Consequences

- `Immersive.Foundation.Runtime` remains `noEngineReferences: true`; Unity dependencies exist only in the optional host assembly.
- A standalone consumer owns a scene-bound host or a scheduler it ticks itself. The Framework may later own and advance the scheduler according to Framework lifecycle and pause behavior.
- Scaled timers pause naturally when Unity sets `Time.timeScale` to zero; unscaled timers keep advancing. Foundation does not alter `Time.timeScale`.
- The new public surface is versioned as Foundation `0.3.0` in this branch; tagging, publication, and Framework migration are separate actions.

## Verification and limits

- Unity 6000.6.10f1 resolved the Foundation package from the local worktree and compiled `Immersive.Foundation.Runtime`, `Immersive.Foundation.Timing.Unity`, and both test assemblies.
- Unity NUnit Edit Mode: 13 passed, 0 failed, 0 skipped. Results: `D:/Projetos/ImmersivePackages/_foundation-time-module-validation-20261125/TestResults/EditMode.xml`.
- Unity NUnit Play Mode: 1 passed, 0 failed, 0 skipped. `UnityTimerHostAdvancesScaledAndUnscaledTimersAndReleasesOnDestroy` exercised scaled/unscaled frame updates and host destruction. Results: `D:/Projetos/ImmersivePackages/_foundation-time-module-validation-20261125/TestResults/PlayMode.xml`.
- PowerShell-hosted C# contract harness: 13 core cases passed; Unity-host stub harness: 3 wiring/lifetime checks passed.
- Static checks confirmed assembly dependency direction, engine-independent core, unique Unity metadata GUIDs, and no Framework or PlayerLoop references in the timing source.
- No Player build was run. Framework integration, dependency update, tag, and publication remain separate actions.
