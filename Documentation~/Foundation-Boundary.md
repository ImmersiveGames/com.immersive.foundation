# Foundation Boundary

`com.immersive.foundation` is a public package of generic primitives with an engine-independent core and optional Unity adapters. The [README](../README.md) is the consumer entry point and documents public usage procedures.

## Accepted surface

- `Immersive.Foundation.Validation`: argument, range, and invariant preconditions.
- `Immersive.Foundation.Events`: explicitly instantiated `EventBus<TEvent>` and disposable event bindings.
- `Immersive.Foundation.Fsm`: `StateMachine`, states, transitions, and predicates; no Unity lifecycle or automatic event integration.
- `Immersive.Foundation.Common`: runtime-only string normalization extensions.
- `Immersive.Foundation.Timing`: `Timer`, `CountdownTimer`, `StopwatchTimer`, `FrequencyTimer`, `TimerScheduler`, and scaled/unscaled time selection; no Unity or Framework references.
- `Immersive.Foundation.Timing.Unity`: optional `UnityTimerHost` frame adapter in its own engine-enabled assembly. It ticks only timers explicitly registered to that component and clears registrations on destruction.

## Ownership and exclusions

Consumers own bus, state-machine, state, predicate, timer, and scheduler instances. A timer can be registered with one scheduler at a time. `UnityTimerHost` owns its scheduler and registration lifetime only; consumers create it explicitly and choose its scene lifetime. The package provides no global bus or timer registry, singleton, service locator, or automatic bootstrap. There are no Unity authoring assets or external package dependencies.

Framework-specific timing integration and application/route/activity lifecycle ownership, scene/game lifecycle systems, bootstrap, configuration registry, pooling, logging, and game-specific concepts are outside this package. A Framework consumer may own and tick `TimerScheduler` in a later, separate integration. This boundary is architectural; for API contracts and examples follow the README.

## Deferred or excluded concerns

- Runtime mode and Strict/Release validation policy belong to Framework Core/Settings/Diagnostics.
- Scene composition and scene lifecycle belong to Framework Core; scene key/route/profile assets remain excluded pending public naming and Inspector UX design.
- Module-specific resolvers, runtime configuration registries, and degraded diagnostics do not belong here.
- Reflection utilities, filtered/global buses, legacy migration code, fallback compatibility rails, and lifecycle ownership are out of scope.
- Unity authoring, automatic cross-scene persistence, Framework lifecycle ownership, and global/PlayerLoop timer registration remain excluded; the explicit host lifetime remains consumer-owned.
