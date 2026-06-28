# Pyralis Canonical Setup

Canonical setup still follows the Pyralis runtime ownership model:

- scene root owns `GameplaySessionBootstrap`
- lifetime scope owns dependency registration
- session data lives in `SessionDefinition`
- mode data lives in `GameModeDefinition`
- participants live in `ParticipantDefinition`
- pawns live in `PawnDefinition`
- input lives in `InputProfile`
- camera routing lives in `CameraRigProfile`
- module capabilities live on explicit module-owned components and profiles

Use PYS Authoring to inspect the setup graph and projection guidance. Use the Inspector to make concrete Unity edits. Do not add runtime auto-repair, scene generators, or hidden compatibility bridges to make setup appear complete.
