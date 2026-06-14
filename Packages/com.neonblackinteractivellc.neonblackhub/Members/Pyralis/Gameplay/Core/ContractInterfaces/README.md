# Pyralis Contract Interfaces

This folder contains runtime gameplay interfaces and service seams.

Keep here:

- runtime interfaces implemented by gameplay systems
- service seams consumed by runtime code
- shared contract interfaces that describe how gameplay components talk to each other

Do not put authoring metadata here. Authoring contract attributes, resolved authoring contracts, and authoring spine vocabulary live in `Core/Authoring`.

If a file only explains setup meaning for the Authoring Window, it belongs in authoring contracts or grammar instead of this runtime interface folder.
