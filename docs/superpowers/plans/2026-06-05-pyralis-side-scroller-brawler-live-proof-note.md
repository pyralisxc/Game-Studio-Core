# Pyralis Authoring 2.0 Side-Scroller Brawler Live Proof Note

Date: 2026-06-05

Status: Checkpoint reached

## Route

First proving route for a local 2D side-scroller brawler using imported Jim/Rico art as the creator-provided asset pool.

This is still a user-authored Unity route, not a preset profile:

- Authoring Window Intent read the selected path as a 2D side-view pawn brawler.
- `GameplaySessionBootstrap` was added through Inspector Add Component search.
- `BrawlerSession` was assigned to the bootstrap through Inspector drag/drop.
- A `Spawn Point` scene object was assigned to the bootstrap `spawnPoints` array through Inspector drag/drop.
- `Jim Brawler Pawn` was built from a native Hierarchy object using Add Component search.
- Adding `Motor 2D` pulled the expected `Rigidbody2D`, `PolygonCollider2D`, `Pawn2DMovementComponent`, `ActorAnimationDriver`, and `Pawn2DPresentationComponent` support stack into the prefab workflow.
- The Inspector guide surfaced missing `SpriteRenderer` and missing input adapter guidance; adding `Sprite Renderer` and `Motor 2D Input Adapter` updated the visible readiness messages.
- The configured pawn was dragged from Hierarchy into `Assets/PyralisProof` to create `Jim Brawler Pawn.prefab`.
- The temporary scene pawn copy was removed so Play Mode uses the authored participant/pawn prefab chain.

## Created Proof Assets

All route proof assets live under `Assets/PyralisProof`:

- `AuthoringLiveProof.unity`
- `BrawlerSession.asset`
- `BrawlerGameMode.asset`
- `BrawlerSetupProfile.asset`
- `BrawlerCharacterPawnPattern.asset`
- `BrawlerParticipant.asset`
- `JimBrawlerPawnDefinition.asset`
- `BrawlerInputProfile.asset`
- `BrawlerMovementProfile.asset`
- `BrawlerPresentationProfile.asset`
- `BrawlerAnimationProfile.asset`
- `Jim Brawler Pawn.prefab`

The data chain is:

```text
AuthoringLiveProof.unity
  -> Pyralis Gameplay Root (GameplaySessionBootstrap)
      -> BrawlerSession
          -> BrawlerGameMode
              -> BrawlerSetupProfile
                  -> BrawlerCharacterPawnPattern
          -> BrawlerParticipant
              -> JimBrawlerPawnDefinition
                  -> Jim Brawler Pawn.prefab
                  -> BrawlerInputProfile
                  -> BrawlerMovementProfile
                  -> BrawlerPresentationProfile
                  -> BrawlerAnimationProfile
```

## Live Proof Result

Play Mode entered successfully after Unity domain reload.

Observed runtime evidence:

- `Jim Brawler Pawn(Clone)` appeared in the Hierarchy during Play Mode.
- The Editor log scan after the proof showed no Pyralis setup exceptions, no null-reference exceptions, and no compiler errors related to the proof route.
- The visible Game view remained empty because the prefab intentionally has no final sprite or Animator Controller assigned yet.

Observed non-route noise:

- Unity AI Assistant service noise continued to appear in the status/log stream (`ApiNoLongerSupported`, relay/port noise). This was already known project noise and was not caused by the Pyralis route.

## Remaining Cameron/User Choices

These should stay as creator choices rather than hidden starter defaults:

- Assign the actual Rico sprite or sliced sprite subasset to `Jim Brawler Pawn.prefab` `SpriteRenderer.m_Sprite`.
- Assign the actual Animator component/controller path:
  - add/configure an `Animator` if the controller route needs it,
  - assign the controller to `BrawlerAnimationProfile.baseController`,
  - wire clips/parameters/bindings to the real attack/jump/run/idle controller.
- Assign a real Unity Input Action Asset to `BrawlerInputProfile.actions` and/or `Motor2DInputAdapter._inputActions` when moving beyond editor keyboard proof.
- Tune `BrawlerMovementProfile` and prefab collider/ground-check fit for brawler feel.
- Decide camera framing and bounds for the side-scroller proof.
- Decide whether combat should first prove only attack animation signaling or also hitboxes/damage through `PawnCombatBehaviour2D` and combat profiles.

## Authoring System Observations

Helpful:

- Intent capacity shelves and route reading were noticeably useful for confirming "2D side-view + pawn brawler" without applying a preset.
- Add Component menu facts are valuable in practice: the user can find core Pyralis components by native Unity search.
- Inspector field guides worked well for the 2D pawn stack, especially the missing renderer/input-adapter messages.
- Selection reactivity worked: selecting `GameplaySessionBootstrap` made the Authoring Window follow the scene root as active setup.

Pain points to monitor in later slices:

- Dragging a raw sprite sheet into `SpriteRenderer` was not enough to produce a visible placeholder. The authoring system should make asset-prep expectations clearer: sliced sprite subasset, Animator Controller, and visual-root/renderer assignment are distinct steps.
- The route can spawn an invisible pawn if the prefab has no sprite/controller. Validate/Overview should keep that distinction obvious: structural route proof can pass while visual proof is still waiting for creator assets.
- Input has two layers: editor keyboard input can help a quick proof, but export-ready input should still require a proper Input Action Asset and should be called out separately.

## Export Footprint Note

This route used runtime assets and scene references only under the route proof folder. No editor authoring provider, fact registry, validator, or Authoring Window object was intentionally referenced by the runtime prefab/session chain. A later route-promotion build-report gate should confirm that player builds include only route-relevant runtime code/assets and do not pull editor authoring assemblies into exports.
