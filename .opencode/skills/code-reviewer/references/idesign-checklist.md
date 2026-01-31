# iDesign Architecture Compliance Checklist

Use this checklist for every code review. Each item is a PASS/FAIL gate.

## 1. Layer Placement

### Is the code in the correct layer?

| Code Type | Correct Layer | Wrong Layer (common mistakes) |
|-----------|--------------|-------------------------------|
| HTTP request/response mapping | Client (Controller) | Manager |
| Workflow orchestration ("do A, then B") | Manager | Controller, Engine |
| Business rules, algorithms, validation logic | Engine | Manager, Accessor |
| Database queries, API calls, file I/O | Accessor | Engine, Manager |
| Logging config, serialization, caching helpers | Utility | Any other layer |

### Red Flags

- Controller with `if/else` business logic -> belongs in Engine
- Manager with `for` loops processing data -> belongs in Engine
- Manager making HTTP calls directly -> belongs in Accessor
- Accessor calculating prices or validating rules -> belongs in Engine
- Engine calling `DbContext` or `HttpClient` -> belongs in Accessor

## 2. Call Chain Integrity

### Allowed Calls (top-down only)

```
Client  ->  Manager  (always exactly ONE Manager per endpoint)
Manager ->  Engine   (one or more)
Manager ->  Accessor (one or more)
Engine  ->  Accessor (only when Engine needs data to compute)
Any     ->  Utility  (cross-cutting, always allowed)
```

### Forbidden Calls

| Violation | Why It's Wrong | Fix |
|-----------|---------------|-----|
| Manager -> Manager (sync) | Creates coupling between workflows | Use async messaging or create a higher-level Manager |
| Engine -> Engine | Engines are independent computation units | Manager should orchestrate both Engines |
| Accessor -> Engine | Upward call breaks isolation | Move logic to Manager/Engine layer |
| Accessor -> Manager | Upward call, circular dependency risk | Event-driven or restructure workflow |
| Engine -> Manager | Upward call breaks layering | Pass needed data as parameters |
| Controller -> multiple Managers | Controller becomes orchestrator | Single Manager should coordinate |

### How to Verify

1. Check constructor injection: what interfaces does each class depend on?
2. Manager should inject: `IEngine*`, `IAccessor*`, `ILogger`
3. Engine should inject: `IAccessor*` (if needed), `IOptions<>`, `ILogger`
4. Accessor should inject: `DbContext`, `HttpClient`, `ILogger`, `IOptions<>`
5. No layer should inject interfaces from layers above it

## 3. Manager Responsibilities

### Managers MUST

- [ ] Orchestrate workflow steps in sequence
- [ ] Coordinate between Engines and Accessors
- [ ] Handle cross-cutting concerns (transactions, compensation)
- [ ] Map between external DTOs and internal domain objects when needed
- [ ] Own the public API contract for their domain

### Managers MUST NOT

- [ ] Contain algorithms or business rules (delegate to Engine)
- [ ] Iterate over collections to process items (delegate to Engine)
- [ ] Make direct resource calls (delegate to Accessor)
- [ ] Call other Managers synchronously
- [ ] Contain complex conditional logic (simple null checks OK)

### Manager Code Smell Test

If a Manager method has:
- More than 1 level of nesting -> suspect business logic leak
- A `for`/`foreach` doing computation -> belongs in Engine
- LINQ with `.Where().Select().GroupBy()` -> belongs in Engine
- Direct `HttpClient` or `DbContext` usage -> belongs in Accessor
- Complex `if/else` trees -> rules belong in Engine

## 4. Engine Responsibilities

### Engines MUST

- [ ] Encapsulate business rules and algorithms
- [ ] Be stateless (no mutable instance state)
- [ ] Accept all inputs as parameters
- [ ] Return computed results (no side effects)
- [ ] Be independently testable without mocking I/O

### Engines MUST NOT

- [ ] Call other Engines (Manager orchestrates)
- [ ] Perform I/O directly (no `HttpClient`, no `DbContext`)
- [ ] Depend on request/response DTOs (use domain types)
- [ ] Throw exceptions for flow control (return result types when appropriate)
- [ ] Hold mutable state between calls

### Engine Purity Test

An Engine method should be testable with:
```csharp
// Pure input -> Pure output, no mocks needed for the core logic
var engine = new PricingEngine(Options.Create(new PricingOptions()));
var result = engine.CalculateEstimate(new PricingContext { ... });
Assert.Equal(expected, result.EstimatedCost);
```

If you need to mock databases or HTTP clients to test an Engine, it's doing too much.

## 5. Accessor Responsibilities

### Accessors MUST

- [ ] Encapsulate a single external resource (DB, API, filesystem)
- [ ] Provide atomic operations on domain concepts
- [ ] Handle connection management and resilience (retries, circuit breakers)
- [ ] Accept and propagate `CancellationToken`
- [ ] Log resource interactions at appropriate levels

### Accessors MUST NOT

- [ ] Contain business logic or validation rules
- [ ] Call Engines or Managers
- [ ] Expose implementation details (SQL, HTTP response codes) to callers
- [ ] Orchestrate multi-step workflows
- [ ] Make decisions about what to do next

### Accessor Isolation Test

If the underlying resource changed (PostgreSQL -> CosmosDB, REST -> gRPC):
- Would only the Accessor implementation change? **GOOD**
- Would Managers/Engines also change? **BAD** - leaky abstraction

## 6. Client (Controller) Responsibilities

### Controllers MUST

- [ ] Map HTTP requests to Manager calls (1 endpoint = 1 Manager method)
- [ ] Map Manager results to HTTP responses
- [ ] Handle authentication/authorization attributes
- [ ] Map exceptions to HTTP status codes
- [ ] Pass `CancellationToken` from the request

### Controllers MUST NOT

- [ ] Contain business logic (no `if (user.Credits < cost)`)
- [ ] Call multiple Managers per endpoint
- [ ] Transform data beyond HTTP mapping
- [ ] Catch generic `Exception` (use specific exception types)
- [ ] Access `DbContext`, `HttpClient`, or any Accessor directly

## 7. Volatility Encapsulation

For every new component, ask:

1. **What volatility does this encapsulate?** (What's likely to change?)
2. **Is the volatile thing isolated?** (Can it change without rippling?)
3. **Is the interface stable?** (Will callers need to change?)

| Good Encapsulation | Bad Encapsulation |
|-------------------|-------------------|
| `ITtsProviderAccessor` hides ElevenLabs API specifics | Manager knows ElevenLabs-specific request format |
| `IRoutingEngine` encapsulates provider selection logic | Controller decides which provider to use |
| `IPricingEngine` isolates pricing rules | Accessor calculates prices during save |
| `IStorageAccessor` abstracts blob storage | Engine writes directly to S3 |

## 8. Interface Design

### Interface Rules

- [ ] One interface per component (not per method)
- [ ] Interface lives in a `Contracts/` directory or alongside the component
- [ ] Naming: `I{Domain}{LayerType}` (e.g., `IGenerationManager`, `IRoutingEngine`)
- [ ] Methods are cohesive (all related to the same domain concept)
- [ ] No "god interfaces" with 10+ methods (split by domain)

### Interface Segregation

If a class implements an interface but leaves methods as `throw new NotImplementedException()`:
- The interface is too broad
- Split into focused interfaces
