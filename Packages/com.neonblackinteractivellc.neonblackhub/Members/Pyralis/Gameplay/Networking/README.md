# Pyralis Networking

This domain owns the Unity Netcode for GameObjects lane for Pyralis. Core gameplay stays transport-agnostic; networked scenes opt in through `SessionDefinition.networkMode`.

## Build Or Buy Boundary

Pyralis should not write its own low-level transport, packet replication, host/client connection lifecycle, or socket stack for the MVP. Those are mature engine problems and belong to Unity Netcode for GameObjects plus Unity Transport in the supported MVP lane.

Pyralis does write the game-development layer above that backend:

- beginner-facing authoring semantics through `SessionDefinition.networkMode`
- setup guidance that separates local multiplayer from networked sessions
- scene and prefab validation for `NetworkManager`, `UnityTransport`, `NetworkObject`, and Network Prefab registration
- participant ownership, seat ownership, authority checks, roster state, session state, and spawn service adapters
- game-rule seams that future lobby, relay, rollback, prediction, reconciliation, and `.io`-style systems can plug into without putting NGO types into core gameplay features

This keeps the platform commercially realistic: use proven networking infrastructure, but own the Pyralis authoring, validation, and game-rule chain that creators actually touch.

## MVP Contract

Supported now:

- `GameplayNetworkMode.LocalOnly` uses local/offline services.
- `GameplayNetworkMode.NetcodeHost`, `NetcodeClient`, and `NetcodeServer` select networked participant/session services.
- `GameplaySessionBootstrap` creates `NetworkedSessionStateService`, `NetworkedParticipantRosterService`, and `NetworkedParticipantSpawnService` when a session selects an NGO mode.
- networked sessions register NGO-backed `ISessionOwnershipService` and `IParticipantAuthorityService` implementations.
- `PyralisNetworkSetupValidator` checks the required MVP scene wiring: `NetworkManager`, `UnityTransport`, pawn prefab `NetworkObject`, and Network Prefab registration.
- `NetworkedParticipantSpawnService` calls `NetworkObject.Spawn`, `SpawnWithOwnership`, and `Despawn` on the server path.
- local authority is resolved against the participant owner client id, so host/client status alone does not make every participant local.

Not claimed yet:

- rollback or client-side prediction
- movement reconciliation
- replicated animation state
- projectile reconciliation
- remote input command streams
- lobby/matchmaking/session browser flows

Those features should build on the participant/session/authority seams instead of adding direct NGO dependencies to gameplay feature code. Networking is MVP-ready for prefab/scene setup, not feature-complete for competitive online action.

## Scene Requirements

For a networked pawn-backed scene:

1. Set `SessionDefinition.networkMode` to `NetcodeHost`, `NetcodeClient`, or `NetcodeServer`.
2. Add one `NetworkManager` to the scene.
3. Add/configure `UnityTransport` and assign it to `NetworkManager.NetworkConfig.NetworkTransport`.
4. Add `NetworkObject` to every pawn prefab spawned by networked participants.
5. Register those pawn prefabs in `NetworkManager.NetworkConfig.Prefabs`.
6. Keep feature modules using Pyralis network metadata (`networkRole`, `replicationPolicyId`, ownership/server flags) rather than direct NGO types.

Local multiplayer remains a separate PlayerInputManager/participant-spawn route. Installing NGO and Transport does not automatically make a scene networked.
