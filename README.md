# Immersive Foundation

`com.immersive.foundation` is a publicly distributed Unity package of reusable, engine-independent validation, event, and finite-state-machine primitives. The runtime has no Unity lifecycle, global registry, or Framework-specific semantics.

## Find a capability

| Intent | Start here | Contract and validation |
|---|---|---|
| Reject invalid arguments or state | [Preconditions](#validate-inputs-and-state) | `Immersive.Foundation.Validation.Preconditions`; exceptions described below |
| Normalize optional text | `FoundationStringExtensions.NormalizeText` / `NormalizeTextOrFallback` in `Immersive.Foundation.Common` | Runtime-only string normalization; no Unity dependencies |
| Publish an event to locally owned subscribers | [Events](#publish-and-subscribe-to-events) | `EventBus<TEvent>`, `IEventBinding`; unit-test the consumer using a local bus |
| Move an object through states and predicate-driven transitions | [FSM](#run-a-finite-state-machine) | `StateMachine`, `IState`, `IPredicate`; unit-test lifecycle callbacks and predicates |
| Understand what belongs in this package | [Boundary](Documentation~/Foundation-Boundary.md) | Package boundary and exclusions |

## Validate inputs and state

`Preconditions` returns validated values where applicable and throws on invalid input. It does not log, recover, or select a fallback.

```csharp
using Immersive.Foundation.Validation;

public sealed class Inventory
{
    public void SetCapacity(int capacity)
    {
        Capacity = Preconditions.CheckInRange(capacity, 1, 100, nameof(capacity));
    }

    public int Capacity { get; private set; }
}
```

`NotNull` throws `ArgumentNullException`; `NotNullOrWhiteSpace` throws `ArgumentNullException` for null and `ArgumentException` for empty/whitespace; `Check` throws `InvalidOperationException`; `CheckArgument` throws `ArgumentException`; range and non-negative checks throw `ArgumentOutOfRangeException`. The caller owns error handling appropriate to its boundary.

## Publish and subscribe to events

Create a bus at the composition/owner boundary and pass it explicitly. The bus is a local object, not a singleton or service locator. Events must be reference types implementing `IEvent`.

```csharp
using Immersive.Foundation.Events;

public sealed class DoorOpened : IEvent { }

public sealed class DoorSignals
{
    private readonly EventBus<DoorOpened> _events = new EventBus<DoorOpened>();

    public IEventBinding Subscribe(System.Action<DoorOpened> handler)
    {
        return _events.Subscribe(handler);
    }

    public int PublishOpened()
    {
        return _events.Publish(new DoorOpened());
    }

    public bool Unsubscribe(IEventBinding binding)
    {
        return _events.Unsubscribe(binding);
    }
}

public sealed class DoorSubscriber
{
    private IEventBinding _binding;
    private DoorSignals _signals;

    public void Attach(DoorSignals signals)
    {
        _signals = signals;
        _binding = signals.Subscribe(OnDoorOpened);
    }

    public void Detach()
    {
        if (_binding != null)
        {
            _signals.Unsubscribe(_binding);
        }

        _binding = null;
        _signals = null;
    }

    private void OnDoorOpened(DoorOpened evt) { /* react to the event */ }
}

// The publisher calls DoorSignals.PublishOpened(); its return value is the delivery count.
```

`Subscribe` rejects a null handler. `Publish` rejects a null event, invokes the snapshot of active bindings and returns the number invoked. Handlers execute synchronously; a handler exception propagates to the publisher and stops that publish call. The bus does not isolate or log handler failures. A subscription is owned by its subscriber: retain the returned `IEventBinding` and call `Unsubscribe(binding)` when participation ends. `Dispose()` deactivates the binding but leaves it in the bus list until `Unsubscribe` or `Clear`; `Unsubscribe` removes and disposes it and returns false for a binding not present on this bus. `Clear()` disposes and removes all subscriptions. The publisher owns the bus lifetime and should clear it when that composition ends; Foundation does not infer Unity or game lifecycle.

## Run a finite-state machine

Implement state behavior and predicates in consumer-owned types. `StateMachine` calls `OnExit` on the previous state and `OnEnter` on the next state; each `Tick` checks any-state transitions first, then transitions registered for the current state, then ticks the current state if no transition fired.

```csharp
using Immersive.Foundation.Fsm;

public sealed class IdleState : IState
{
    public void OnEnter() { }
    public void Tick() { }
    public void OnExit() { }
}

public sealed class MovingState : IState
{
    public void OnEnter() { }
    public void Tick() { }
    public void OnExit() { }
}

public sealed class StartRequested : IPredicate
{
    public bool Evaluate() => true; // Replace with the consumer-owned condition.
}

public sealed class DoorFlow
{
    private readonly StateMachine _machine = new StateMachine();

    public DoorFlow()
    {
        var idle = new IdleState();
        var moving = new MovingState();
        _machine.SetState(idle);
        _machine.AddTransition(idle, moving, new StartRequested());
    }

    public void Tick()
    {
        _machine.Tick(); // any-state transitions, then current-state transitions, then state Tick
    }
}
```

`SetState`, `AddTransition`, and `AddAnyTransition` reject null required arguments. Setting the same state instance is a no-op. Exceptions from predicates or state callbacks propagate; the machine does not recover or roll back partially completed callbacks. The machine does not own, dispose, or construct states and predicates, and does not integrate automatically with `EventBus` or Unity. Keep state and predicate instances alive for as long as the machine references them.

## Dependencies, authoring, and validation

There are no package dependencies or Unity authoring assets. Runtime is engine-independent; consumers provide the composition and call `StateMachine.Tick()` from their own update loop when using FSM. Validate behavior with consumer-side unit tests for exception cases, event delivery/unsubscription/clear, and FSM transition ordering/lifecycle callbacks. This repository currently contains no package test source or shipped `Samples~`; no automated test execution is claimed here.

## Boundary

Foundation v0 is frozen to generic Validation, Events, and FSM primitives. Framework lifecycle, Unity authoring, global buses, service registration, bootstrap, pooling, logging, and game-specific concepts belong elsewhere. See [Foundation Boundary](Documentation~/Foundation-Boundary.md) for the architectural exclusions.

## Installation

Use the project's configured package registry or its existing Git/package source. Resolve the package version from the consuming project's `Packages/manifest.json` and `packages-lock.json`; this README does not prescribe a version for discovery.

## License

Licensed under the [MIT License](LICENSE.md).
