# Foundation Boundary

`com.immersive.foundation` is a public package of generic, engine-independent primitives. The [README](../README.md) is the consumer entry point and documents the public usage procedures.

## Accepted surface

- `Immersive.Foundation.Validation`: argument, range, and invariant preconditions.
- `Immersive.Foundation.Events`: explicitly instantiated `EventBus<TEvent>` and disposable event bindings.
- `Immersive.Foundation.Fsm`: `StateMachine`, states, transitions, and predicates; no Unity lifecycle or automatic event integration.
- `Immersive.Foundation.Common`: runtime-only string normalization extensions.

## Ownership and exclusions

Consumers own bus, state-machine, state, and predicate instances. The package provides no global bus, singleton, service locator, composition root, or lifecycle ownership. It has no Unity authoring surface and no package dependencies.

Framework-specific behavior, scene/game lifecycle, bootstrap, configuration registry, pooling, logging, and game-specific concepts are outside this package. This boundary is architectural; for API contracts and examples follow the README.

## Deferred or excluded concerns

- Runtime mode and Strict/Release validation policy belong to Framework Core/Settings/Diagnostics.
- Scene composition and scene lifecycle belong to Framework Core; scene key/route/profile assets remain excluded pending public naming and Inspector UX design.
- Module-specific resolvers, runtime configuration registries, and degraded diagnostics do not belong here.
- Reflection utilities, filtered/global buses, legacy migration code, fallback compatibility rails, and lifecycle ownership are out of scope.
- Specialized reusable technical capabilities may be separate packages; framework behavior remains in Framework Core.
