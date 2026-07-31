# Antigravity 2.0 Observability Instructions

When generating a new Cell:

1. Add structured logging.

2. Register ActivitySource.

3. Register metrics.

4. Propagate ExecutionContext.

5. Add audit events when required.


Forbidden:

- Console.WriteLine
- Reflection based telemetry
- Custom telemetry frameworks


Generated code MUST comply with AHS Observability Standards.
