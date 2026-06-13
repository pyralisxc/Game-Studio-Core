using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Editor
{
    public static class PyralisAuthoringCapabilityGuidance
    {
        public static PyralisAuthoringFeatureRow BuildSelectedRow(RuntimePatternDefinition pattern)
        {
            string source = GetPatternDisplayName(pattern);
            return BuildSelectedRow(pattern.capabilityFamily, source);
        }

        public static PyralisAuthoringFeatureRow BuildSelectedRow(RuntimeCapabilityFamily family)
        {
            return BuildSelectedRow(family, "Selected capability");
        }

        private static PyralisAuthoringFeatureRow BuildSelectedRow(RuntimeCapabilityFamily family, string source)
        {
            RuntimeCapabilityCard card = PyralisRuntimeCapabilityCatalog.FindPrimaryByFamily(family);
            if (card != null)
                return new PyralisAuthoringFeatureRow(
                    card.DisplayName,
                    source,
                    card.WhatItAdds,
                    BuildSetupSummary(card),
                    BuildCustomizationSummary(card));

            return family switch
            {
                RuntimeCapabilityFamily.PlatformCore => new PyralisAuthoringFeatureRow(
                    "Core Session Setup",
                    source,
                    "Declares the baseline Pyralis route: bootstrap, session, game mode, setup profile, participants, services, settings, scene flow, and optional scene roots.",
                    "Create Gameplay Root with GameplaySessionBootstrap and PyralisGameplayLifetimeScope, assign SessionDefinition > GameModeDefinition > GameSetupProfile, then add only the roots the selected capabilities need.",
                    "Customize session defaults, network mode, settings profile, scene loading, participant count, input defaults, and which optional roots are part of the first playable loop."),
                RuntimeCapabilityFamily.CharacterPawnGameplay => new PyralisAuthoringFeatureRow(
                    "Pawn Actor Control",
                    source,
                    "Participants own actor bodies that can move, collide, animate, fight, collect, or be spawned into the scene.",
                    "Create a pawn prefab with the lane stack, assign it through PawnDefinition, assign that pawn to ParticipantDefinition, then add Spawn Points to GameplaySessionBootstrap. For a 2D proof, use PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator on the prefab root.",
                    "Choose art and presentation in PawnPresentationProfile. Tune speed, acceleration, braking, jump, and feel in movement profiles and movement components."),
                RuntimeCapabilityFamily.Combat => new PyralisAuthoringFeatureRow(
                    "Combat / Brawler Actions",
                    source,
                    "Adds attacks, hitboxes, hurtboxes, damage, health, reactions, combos, or enemy moves to the route.",
                    "Create CombatActionDefinition and CombatSequenceDefinition assets, add health/hitbox components to the pawn, and prove one hit before building the full move list.",
                    "Tune timing, damage, knockback, cancel windows, reactions, and animation signals through combat/profile assets."),
                RuntimeCapabilityFamily.GunsProjectiles => new PyralisAuthoringFeatureRow(
                    "Projectiles / Shots",
                    source,
                    "Adds bullets, spells, traps, turrets, hitscan, fire modes, ammo, or impact feedback.",
                    "Decide the firing source first, then add ProjectileLauncher2D or ProjectileLauncher3D and assign ProjectileDefinition, ProjectileImpactDefinition, and FireModeDefinition assets.",
                    "Tune cadence, spread, clip/reload, payload, impact feedback, and whether the shot comes from a pawn, camera, card, trap, board piece, or AI."),
                RuntimeCapabilityFamily.ActionTargeting => new PyralisAuthoringFeatureRow(
                    "Action Selection / Menus",
                    source,
                    "Lets the player choose commands through buttons, turns, cards, board selections, tactical targeting, or queued actions.",
                    "Create one ActionDefinition, then create the selection surface: Canvas button, cursor, board space, card hand, or pawn action trigger. Prove one action before expanding the menu.",
                    "Customize targeting rules, costs, button layout, command text, turn timing, and which runtime owns action resolution."),
                RuntimeCapabilityFamily.BoardCardTabletop => new PyralisAuthoringFeatureRow(
                    "Board / Card / Tabletop",
                    source,
                    "Makes participants act as seats, hands, factions, pieces, or turn owners instead of requiring character controllers.",
                    "Start with no PawnDefinition. Create board/card zones, seats, turn order, legal move rules, and a UI/camera/cursor selection surface.",
                    "Customize board spaces, card zones, move legality, turn order, terminal conditions, scoring, and how players select pieces or cards."),
                RuntimeCapabilityFamily.CameraInput => new PyralisAuthoringFeatureRow(
                    "Camera / Cursor Control",
                    source,
                    "Lets a participant control the view, cursor, selector, commander camera, or other non-pawn input surface.",
                    "Create a Camera Root. Use CinemachineCameraRigController with a CameraRigProfile for shared/split camera, visible bounds, and 2D routes where the physical Target Camera or profile is orthographic.",
                    "Customize zoom, follow/framing, split behavior, board view, cursor rules, input profile, and whether camera selection also drives actions."),
                RuntimeCapabilityFamily.AnimationPresentation => new PyralisAuthoringFeatureRow(
                    "Animation / Presentation",
                    source,
                    "Connects gameplay signals to Sprite2D, Billboard2_5D, Rigged3D, whatever Animator Controller the pawn visual equips, shadows, and visual feedback.",
                    "Create ActorAnimationDefinition and PawnAnimationProfile, find the pawn visual's Animator Controller in your folderbase or package, assign it as Base Controller, map signals to that controller's parameters, then add ActorAnimationDriver or the lane-specific presentation component to the pawn prefab.",
                    "Customize the art pipeline, Animator controller, signal-to-parameter bindings, facing, shadows, 2D/2.5D/3D presentation mode, and feedback visuals."),
                RuntimeCapabilityFamily.ScoringObjectives => new PyralisAuthoringFeatureRow(
                    "Scoring / Objectives",
                    source,
                    "Tracks score, timers, lives, resources, objectives, win/loss state, round results, or victory points.",
                    "Enable scoring on GameModeDefinition, add ParticipantScoreService or another ISessionScoreService, then connect HUD labels after score changes work.",
                    "Customize score events, objective rules, timer text, victory conditions, score feedback, and whether scoring is participant or session scoped."),
                RuntimeCapabilityFamily.ProceduralGeneration => new PyralisAuthoringFeatureRow(
                    "Procedural Setup",
                    source,
                    "Creates level chunks, rooms, lanes, waves, board layouts, encounters, or seeded content at edit time or runtime.",
                    "Author chunks, sockets, seeds, spawn budgets, and validation first. Keep generated content inspectable before it becomes required game flow.",
                    "Customize generation rules, seed behavior, chunk libraries, encounter budgets, spawn density, and editor preview/validation."),
                RuntimeCapabilityFamily.Networking => new PyralisAuthoringFeatureRow(
                    "Networking / Authority",
                    source,
                    "Adds ownership, authority, host/client expectations, network prefab readiness, or backend-facing synchronization.",
                    "Confirm the route locally first, then add NetworkManager, UnityTransport, network prefab registration, NetworkObject setup, and authority metadata.",
                    "Customize ownership rules, host/client flow, prediction needs, replicated state, and which systems remain local-only."),
                _ => new PyralisAuthoringFeatureRow(
                    "Custom Capability",
                    source,
                    "Declares a project-specific setup expectation.",
                    "Explain concrete Unity objects, components, and fields in the pattern description/setup notes before using it in a guided route.",
                    "Customize only after the required scene objects and runtime systems are named clearly.")
            };
        }

        private static string BuildSetupSummary(RuntimeCapabilityCard card)
        {
            if (card == null)
                return string.Empty;

            List<string> parts = new List<string>();
            AddJoined(parts, card.RequiredDefinitions, "Definitions");
            AddJoined(parts, card.RequiredProfiles, "Profiles");
            AddJoined(parts, card.RequiredSceneComponents, "Scene");
            AddJoined(parts, card.RequiredUnitySurfaces, "Unity");

            if (parts.Count == 0)
                return card.FirstProof;

            return string.Join(" | ", parts);
        }

        private static string BuildCustomizationSummary(RuntimeCapabilityCard card)
        {
            if (card == null)
                return string.Empty;

            if (card.CustomizationMoments.Length > 0)
                return string.Join(" ", card.CustomizationMoments);

            return card.ExpertAdvice;
        }

        private static void AddJoined(List<string> parts, string[] values, string label)
        {
            if (values == null || values.Length == 0)
                return;

            parts.Add(label + ": " + string.Join(", ", values));
        }

        public static List<PyralisAuthoringFeatureRow> BuildRecommendedRows(PyralisAuthoringRouteDescriptor route)
        {
            List<PyralisAuthoringFeatureRow> rows = new List<PyralisAuthoringFeatureRow>();
            if (route == null)
                return rows;

            if (route.HasPawn && !route.HasAnimation)
                rows.Add(Row("Animation / Presentation", "Recommended next", "Pawn routes are easier to evaluate when the actor visibly faces, moves, reacts, or animates.", "Add an Animation/Presentation runtime pattern, then wire PawnPresentationProfile, PawnAnimationProfile, and ActorAnimationDriver or lane-specific presentation components. Find the pawn visual's Animator Controller in your folderbase or package, assign it as Base Controller, then map Pyralis signals to the parameters it already has.", "Use this when the prototype needs art mode clarity: Sprite2D, Billboard2_5D, Rigged3D, or a custom presentation lane."));

            if ((route.HasCombat || route.HasProjectiles) && !route.HasScoring)
                rows.Add(Row("Scoring / Objectives", "Optional next", "Combat and projectile loops usually need a reason to end, reward, count hits, award resources, or show success.", "Add a Scoring/Objectives pattern, enable scoring on the GameModeDefinition, then add ParticipantScoreService and HUD feedback when needed.", "Skip this for pure feel prototypes; add it when win/loss, timers, waves, resources, or rewards matter."));

            if ((route.HasCombat || route.HasProjectiles || route.HasTabletop) && !route.HasActions)
                rows.Add(Row("Action Selection / Menus", "Optional next", "Abilities, cards, board moves, commands, and special attacks become easier to author when the route has an explicit action selection surface.", "Add an ActionTargeting pattern, create one ActionDefinition, then connect a button, cursor, card, board space, or pawn input to that action.", "Use this for brawler specials, tactics commands, card choices, board moves, or menus."));

            if ((route.HasTabletop || route.HasActions) && !route.HasCamera)
                rows.Add(Row("Camera / Cursor Control", "Recommended next", "Board, card, and action-selection routes need a way for the player to inspect, point at, or choose things.", "Add a Camera/Input pattern, create Camera Root or cursor/select surface, then decide whether selection is UI, raycast, board grid, or card hand driven. For 2D bounded views, use an orthographic Target Camera or CameraRigProfile.", "Customize view framing, cursor rules, input map, and how the chosen object becomes an action target."));

            if ((route.HasPawn || route.HasActions || route.HasTabletop || route.HasScoring) && !route.HasNetworking)
                rows.Add(Row("HUD / Menus / Feedback", "Scene root reminder", "Most playable routes need visible state: health, score, prompts, card choices, board selection, pause/settings, or game-over flow.", "Create a UI Root with Canvas and EventSystem. Add ParticipantHealthHudBinder, ParticipantFeedbackHudPresenter, UIManager, or project-owned presenters that read Pyralis services.", "Customize layout, art, button text, labels, menu sections, and which services drive the displayed information."));

            if (route.HasProcedural && !route.HasScoring)
                rows.Add(Row("Validation / Objectives", "Recommended next", "Generated content needs a way to prove it created enough playable structure and a goal for players to complete.", "Add generation validation and optionally a Scoring/Objectives pattern after the generated layout is inspectable.", "Customize pass/fail checks, budgets, seeds, and what makes a generated route playable."));

            AddCurrentSystemRows(rows, route);
            return rows;
        }

        public static List<PyralisAuthoringFeatureRow> BuildEnvironmentRows(PyralisAuthoringRouteDescriptor route)
        {
            List<PyralisAuthoringFeatureRow> rows = new List<PyralisAuthoringFeatureRow>();
            if (route == null || !route.UsesWorld && !route.HasActions && !route.HasCamera && !route.HasScoring)
                return rows;

            rows.Add(Row("World / Ground / Environment", "Design first", "The ground, arena, board, rooms, platforms, props, backdrops, skyboxes, terrain, and boundaries may be plain Unity objects with no Pyralis script. Pyralis depends on their colliders, layers, bounds, zones, anchors, and selection surfaces only when gameplay needs those surfaces.", "Create an Environment or Playfield Root. Add Unity geometry, Tilemaps, flat sprite/PNG backdrops, meshes, terrain, skyboxes, Canvas backgrounds, colliders, trigger volumes, board/card anchors, or props. Use PlayfieldProfile for authored bounds, CinemachineCameraRigController for camera/spawn bounds, and TilemapGround/ArenaZone only when those helpers match the scene.", "Customize art, scale, collision layers, ground layer names, sorting/depth, lighting, camera framing, spawn-safe areas, board coordinates, hazard zones, pickup zones, and which surfaces are walkable, selectable, blocked, procedural, or only visual."));

            if (route.HasPawn)
                rows.Add(Row("Walkable Ground And Collision", "Pawn route", "Pawn movement is only believable when the level tells Unity what is ground, wall, ledge, platform, trigger, or hazard.", "Author colliders on the environment. For 2D use Collider2D/CompositeCollider2D/TilemapCollider2D as appropriate. For 3D use Collider, MeshCollider, CharacterController-compatible ground, or baked TilemapGround. Match pawn ground layers to the environment layers.", "Tune layers, physics materials, collider thickness, slopes, step height, ledges, one-way platforms, traversal surfaces, and spawn locations. Pyralis should guide these choices, not replace level design."));

            if (route.HasTabletop || route.HasActions)
                rows.Add(Row("Selectable Spaces", "Board/action route", "A board tile, card slot, menu row, or tactical square might be a visual object, UI element, collider, or data coordinate. It matters because actions need a target surface.", "Choose one selection path first: UI button, board presenter, Collider/OnMouseDown, raycast target, card hand presenter, or cursor bridge. Connect that surface to one ActionDefinition or board move before expanding.", "Customize legal spaces, highlight rules, disabled states, hover text, action costs, target filters, and how visual spaces map to rules data."));

            if (route.HasCamera || route.HasProcedural)
                rows.Add(Row("Bounds And Framing", "Camera/spawn route", "Spawners, hazards, pickups, generated chunks, and camera systems need to know the playable area even when the environment is mostly art.", "Use PlayfieldProfile for authored bounds and CinemachineCameraRigController for visible camera bounds. Add anchors or zones under Playfield Root for spawn, encounter, pickup, hazard, or generated content placement.", "Customize min/max bounds, camera margins, room sizes, spawn budgets, safe zones, offscreen margins, and how generated or placed content avoids blocked spaces."));

            return rows;
        }

        public static string GetRouteIntent(PyralisAuthoringRouteDescriptor route, int selectedCount)
        {
            if (route == null || selectedCount == 0)
                return "No capabilities selected yet. Choose capability ingredients that match the game surface before wiring scene objects.";
            if (route.HasPawn && route.HasCombat)
                return "This reads like a brawler, fighter, action character, or enemy-driven pawn route. Expect pawn, movement, combat, animation/presentation, input, camera, feedback, and optional scoring/HUD setup.";
            if (route.HasTabletop)
                return "This reads like a board, card, tactics, or tabletop route. Pawns can stay empty unless a selected capability later introduces actor bodies.";
            if (route.HasProjectiles && route.HasCamera && !route.HasPawn)
                return "This reads like a camera/cursor projectile, trap, turret, or command-shot prototype. Decide the firing source before building pawn assumptions.";
            if (route.HasPawn)
                return "This reads like a character-control route. Start with participant, pawn, movement, presentation, spawn points, camera/input, then add features.";
            if (route.HasActions)
                return "This reads like an action-selection route. Start with one command surface and one selectable action, then expand into menus, cards, turns, or board choices.";
            if (route.HasScoring)
                return "This reads like an objective or score-loop route. Prove score changes first, then connect HUD and win/loss flow.";
            if (route.HasProcedural)
                return "This reads like a generated-content route. Make generated output inspectable before relying on it for the playable loop.";
            if (route.HasNetworking)
                return "This reads like a network-aware setup route. Confirm the local route first, then add ownership, authority, and prefab registration.";
            if (route.HasAnimation)
                return "This reads like a presentation route. Connect it to pawn, action, feedback, or scene objects so visuals are driven by gameplay.";
            return "This setup has selected capabilities. Inspect each row below to see what gameplay it adds and what Unity objects to wire.";
        }

        private static void AddCurrentSystemRows(List<PyralisAuthoringFeatureRow> rows, PyralisAuthoringRouteDescriptor route)
        {
            if (route.HasPawn)
            {
                rows.Add(Row("Movement / Traversal / Respawn", "Current setup surface", "Pawn routes can use 2D movement, 3D/2.5D movement, traversal, spawn points, respawn rules, and playfield bounds.", "Wire PawnMovementProfile, optional PawnTraversalProfile, lane components such as Pawn2DMovementComponent or Pawn3DTraversalComponent, Spawn Points on GameplaySessionBootstrap, and PlayfieldProfile when bounds matter.", "Customize speed, acceleration, braking, jump, climb/hang/ledge behavior, spawn placement, respawn timing, safe zones, and lane-specific movement feel."));
                rows.Add(Row("Feature Modules / Pickups / Interaction", "Current setup surface", "Reusable actor capabilities live in FeatureModuleDefinition assets and can cover pickups, interaction prompts, feedback, status effects, and custom pawn modules.", "Add FeatureModuleDefinition assets to PawnDefinition.featureModules. For pickups, use ActorPickupCollectorFeature2D or ActorPickupCollectorFeature3D with Collectible2D, CollectibleSpawner2D, and pickup profiles. For interaction, add interaction feature/runtime pieces and prove one prompt.", "Customize feature runtime prefabs, profile assets, collection rules, prompt radius, cooldowns, status effects, pickup scoring, feedback messages, and supported presentation/network roles."));
            }

            if (route.HasCombat || route.HasProjectiles)
            {
                rows.Add(Row("Health / Hitboxes / Feedback", "Current setup surface", "Combat and projectile loops need visible cause and effect: hurtboxes, hitboxes, health, reactions, camera shake, impact effects, and participant feedback.", "Wire health/hitbox components on the pawn or target, assign PawnCombatProfile or projectile impact assets, add ActorFeedbackProfile or reaction profiles, then connect ParticipantFeedbackHudPresenter or ParticipantHealthHudBinder after one hit works.", "Customize hit windows, damage, knockback, invulnerability, hurt/stagger reactions, hit sparks, shake, popup text, health panels, and which feedback appears in HUD versus world space."));
                rows.Add(Row("Enemies / Hazards / Encounter Zones", "Current setup surface", "Enemy and hazard setups let the route add authored opponents, ambient behavior, damage zones, encounter triggers, difficulty, and environmental threats.", "Use EnemyFeatureProfile, EnemyCombatProfile, EnemyReactionProfile, EnemyAmbientFeatureProfile, HazardImpactProfile, HazardFeedbackProfile, HazardData, encounter zones, and hazard spawners only after the player-side pawn/combat proof works.", "Customize enemy attacks, AI cadence, reactions, patrol/ambient behavior, hazard damage, knockback, status effects, difficulty scaling, encounter boundaries, and warning/impact feedback."));
            }

            if (route.HasActions || route.HasTabletop || route.HasScoring)
                rows.Add(Row("Menus / Settings / Scene Flow", "Current setup surface", "Menus turn setup into a playable loop: pause, settings, restart, return to menu, scene transitions, action buttons, board/card choices, and game-over flow.", "Create UI Root with Canvas and EventSystem, add UIManager or project presenters, add SettingsManager when settings are editable, and add SceneFader or scene-flow components when buttons load or restart scenes.", "Customize menu sections, button text, navigation, settings profile, fade timing, scene names, pause behavior, game-over decisions, and which Pyralis service each UI action calls."));

            if (route.HasCamera || route.HasProcedural || route.HasProjectiles)
                rows.Add(Row("Camera / Bounds / Spawnable Content", "Current setup surface", "Camera-aware routes, projectiles, spawners, pickups, hazards, and procedural content depend on bounds, visible space, and safe placement.", "Use CameraRigProfile, CinemachineCameraRigController, PlayfieldProfile, spawn anchors, pickup/hazard spawners, and generated chunk/zone data where the route needs them.", "Customize camera follow/framing, orthographic size, split behavior, visible bounds, offscreen margins, spawn budgets, chunk sizes, safe placement, and whether camera or playfield owns the bounds."));

            if (route.HasNetworking)
                rows.Add(Row("Network Prefab Readiness", "Current setup surface", "Networked routes need the local setup to work first, then explicit ownership, transport, NetworkObject, and registered prefab setup.", "Set SessionDefinition.networkMode, add NetworkManager and UnityTransport, add NetworkObject to networked pawn prefabs, register them in Network Prefabs, and validate feature module network roles.", "Customize host/client flow, ownership transfer, authority checks, which features are local-only, and what state must replicate."));
        }

        private static PyralisAuthoringFeatureRow Row(string feature, string source, string gameplayEffect, string unitySetup, string customization)
        {
            return new PyralisAuthoringFeatureRow(feature, source, gameplayEffect, unitySetup, customization);
        }

        private static string GetPatternDisplayName(RuntimePatternDefinition pattern)
        {
            if (pattern == null)
                return "Missing pattern";
            if (!string.IsNullOrWhiteSpace(pattern.displayName))
                return pattern.displayName;
            if (!string.IsNullOrWhiteSpace(pattern.patternId))
                return pattern.patternId;
            return pattern.capabilityFamily.ToString();
        }
    }
}
