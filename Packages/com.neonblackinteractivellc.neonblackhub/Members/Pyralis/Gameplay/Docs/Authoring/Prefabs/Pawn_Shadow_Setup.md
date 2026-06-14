# Pawn Shadow Setup

Use `PawnPresentationProfile` plus `ActorShadowDriver` to author shared shadows for `2D`, `2.5D`, and `rigged 3D` pawns.

## Before You Wire This

Start with a `SessionDefinition` assigned to `GameplaySessionBootstrap.sessionDefinition` and a `GameModeDefinition` assigned to `SessionDefinition.defaultGameMode`.

Recommended route capabilities:

- Animation/Presentation
- Realtime Character when shadows belong to pawn-controlled actors
- Board/Card/Tabletop only when board pieces or tabletop actors need pawn-style visual presentation

Resolve route validation before adding shadow drivers or presentation profile shadow settings.

## Core Idea

Shadows are a presentation feature, not controller logic.

That means:

- author shadow behavior on `PawnPresentationProfile`
- let `ActorAnimationDriver` forward the profile to `ActorShadowDriver`
- reuse the same profile shape across sprite, billboard, and rigged actors

## Supported Modes

- `Auto`
  - chooses blob shadows for sprite or billboard actors when a shadow sprite or prefab is provided
  - chooses renderer shadows for rigged actors when no blob asset is assigned
- `None`
  - disables shared shadow output
- `BlobSprite`
  - creates or uses a flat sprite-based shadow under the actor
- `RendererShadows`
  - uses renderer cast/receive shadow settings for rigged or mesh actors

## Required Components

On the pawn root or visual root:

- `ActorAnimationDriver`
- `ActorShadowDriver`

The shadow driver is profile-driven. It does not need controller-specific wiring.

## Blob Shadow Setup

Use this for:

- top-down or side-on `2D`
- `2.5D` billboard characters
- stylized `3D` when you want a consistent arcade blob shadow

On `PawnPresentationProfile`:

- set `Shadow Mode` to `BlobSprite` or `Auto`
- assign `Shadow Sprite` for a generated runtime sprite shadow
- or assign `Shadow Prefab` if you want a custom prefab-based shadow
- tune:
  - `Shadow Local Offset`
  - `Shadow Scale`
  - `Shadow Color`
  - `Shadow Sorting Layer Name`
  - `Shadow Sorting Order`
  - `Shadow Height Scale Response`

Recommended defaults:

- `Shadow Color`: semi-transparent black
- `Shadow Sorting Order`: below the pawn sprite
- `Shadow Height Scale Response`: low but non-zero so jumps feel readable

## Rigged 3D Shadow Setup

Use this for:

- `Generic` rigged models
- `Humanoid` rigged models

On `PawnPresentationProfile`:

- set `Shadow Mode` to `RendererShadows` or leave it on `Auto`
- set `Cast Model Shadows`
- set `Receive Model Shadows`

This mode uses renderer shadow settings instead of sprite blobs.

## Style Guidance

Best default choices by lane:

- `2D`: blob sprite shadow
- `2.5D`: blob sprite shadow
- `rigged 3D`: renderer shadows when lighting supports them, blob shadow when you want cleaner stylization or more consistent readability

## Common Mistakes

| Problem | Likely cause |
|---|---|
| No shadow appears on a 2D pawn | `Shadow Mode` is `None`, or no `Shadow Sprite` / `Shadow Prefab` is assigned |
| Rigged actor has no shadow | lighting does not support model shadows, or `Cast Model Shadows` is off |
| Shadow renders above the pawn | `Shadow Sorting Order` is too high |
| Jumping pawn shadow never changes feel | `Shadow Height Scale Response` is `0` |
