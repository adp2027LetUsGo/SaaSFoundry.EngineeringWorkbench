# .NET 10 Native AOT Guidelines

Observability components MUST support Native AOT.

Avoid:

- Reflection
- Runtime discovery
- Dynamic proxies

Prefer:

- Source generators
- Explicit registration
- Compile-time metadata
