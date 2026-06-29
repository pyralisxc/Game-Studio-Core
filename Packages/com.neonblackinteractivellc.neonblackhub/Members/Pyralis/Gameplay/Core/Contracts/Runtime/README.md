# Runtime Contracts

This folder contains runtime gameplay interfaces and service seams.

Keep here:

- runtime interfaces implemented by gameplay systems
- service seams consumed by runtime code
- shared contract interfaces that describe how gameplay components talk to each other

Do not put authoring projection code here. Runtime scripts may declare PYS `AuthoringContract` metadata when they own gameplay meaning, but authoring scanning, vocabulary, graph building, projections, validation aggregation, hygiene, facts, exports, and UI live in the standalone `com.pys.authoring` package.

If a file only explains setup meaning for the Authoring Window, it belongs in authoring contracts or grammar instead of this runtime interface folder.
