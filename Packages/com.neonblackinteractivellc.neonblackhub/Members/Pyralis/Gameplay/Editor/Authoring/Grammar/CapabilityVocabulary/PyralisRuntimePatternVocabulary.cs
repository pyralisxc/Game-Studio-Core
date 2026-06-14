using NeonBlack.Gameplay.Data.Definitions;

namespace NeonBlack.Gameplay.Editor
{
    internal static class PyralisRuntimePatternVocabulary
    {
        public static string GetSuggestedDescription(RuntimePatternDefinition pattern)
        {
            return pattern.capabilityFamily switch
            {
                RuntimeCapabilityFamily.CharacterPawnGameplay => "Use this when the player, enemy, or AI needs a pawn body in the scene. Expect a ParticipantDefinition, PawnDefinition, pawn prefab, movement profile, presentation profile, and spawn point.",
                RuntimeCapabilityFamily.Combat => "Use this when gameplay needs attacks, hitboxes, hurtboxes, health, reactions, damage, brawler actions, fighter moves, or combat sequences.",
                RuntimeCapabilityFamily.GunsProjectiles => "Use this when gameplay needs bullets, hitscan shots, spells, traps, turrets, ammo, fire modes, projectile prefabs, or impact feedback.",
                RuntimeCapabilityFamily.ActionTargeting => "Use this when the player chooses actions through menus, turns, cards, commands, board selections, tactics targeting, or queued abilities.",
                RuntimeCapabilityFamily.BoardCardTabletop => "Use this when the game is driven by seats, boards, pieces, card hands, decks, zones, legal moves, turns, or tabletop-style state instead of a character controller.",
                RuntimeCapabilityFamily.CameraInput => "Use this when the participant controls a camera, cursor, selector, faction, commander view, or non-pawn input surface.",
                RuntimeCapabilityFamily.ProceduralGeneration => "Use this when the scene creates level chunks, rooms, lanes, waves, board layouts, encounters, or seeded content at edit time or runtime.",
                RuntimeCapabilityFamily.AnimationPresentation => "Use this when the setup needs shared Animator mapping for whichever controller the pawn equips, animation signals, 2D/2.5D/3D presentation, shadows, or feedback visuals.",
                RuntimeCapabilityFamily.ScoringObjectives => "Use this when the game tracks score, timers, lives, resources, objectives, victory points, round results, or win/loss state.",
                RuntimeCapabilityFamily.Networking => "Use this when participants, session state, or spawned gameplay need multiplayer ownership, host authority, or backend-facing synchronization.",
                _ => "Use this runtime pattern to describe one reusable setup expectation. Add it to a GameSetupProfile before wiring scene objects so the setup intent is explicit."
            };
        }

        public static string GetSuggestedSetupNotes(RuntimePatternDefinition pattern)
        {
            return pattern.capabilityFamily switch
            {
                RuntimeCapabilityFamily.CharacterPawnGameplay => "Create a ParticipantDefinition, assign a PawnDefinition for the default pawn, assign a pawn prefab with PawnRoot and movement/presentation components, add at least one Spawn Point to GameplaySessionBootstrap, then run Play Mode and verify one player pawn spawns and moves.",
                RuntimeCapabilityFamily.Combat => "Create CombatActionDefinition assets, group them in a CombatSequenceDefinition, assign the sequence through a PawnCombatProfile or action system, add health/hitbox components, then test one hit in Play Mode.",
                RuntimeCapabilityFamily.GunsProjectiles => "Create a ProjectileDefinition, ProjectileImpactDefinition, and FireModeDefinition. Decide whether the shot comes from a pawn, camera, card, trap, board piece, or AI, then add the matching launcher/runtime component.",
                RuntimeCapabilityFamily.ActionTargeting => "Create ActionDefinition assets first. Decide the selection surface: menu, cursor, board space, card hand, or pawn. Add UI/turn/action queue systems only after the basic action can be selected.",
                RuntimeCapabilityFamily.BoardCardTabletop => "Start without a PawnDefinition. Create participants as seats or players, add camera/cursor or UI control, create board/card zones, then add actions for legal moves or card choices.",
                RuntimeCapabilityFamily.CameraInput => "Create a camera or cursor control surface, assign an InputProfile, add UI or raycast selection if needed, and only add a PawnDefinition if the camera controls an actor body too. For 2D bounded views, make the Target Camera or CameraRigProfile orthographic.",
                RuntimeCapabilityFamily.ProceduralGeneration => "Author chunks, sockets, spawn rules, budgets, and seeds before runtime generation. Add validation so generated content can be inspected before it becomes required game flow.",
                RuntimeCapabilityFamily.AnimationPresentation => "Create an ActorAnimationDefinition, assign it to a PawnAnimationProfile, find the pawn visual's Animator Controller in your folderbase or package, assign it as Base Controller, map signals to that controller's parameters, then add ActorAnimationDriver to the pawn prefab.",
                RuntimeCapabilityFamily.ScoringObjectives => "Add ParticipantScoreService or the scoring runtime, decide what creates score events, wire HUD only after the score changes correctly in Play Mode.",
                RuntimeCapabilityFamily.Networking => "Start local-first. Confirm the session and participants work locally, then add ownership/authority rules and backend adapters around the same definitions.",
                _ => "Write the native Unity setup steps: assets to create, scene objects to add, fields to assign, what can stay empty, and what proves the setup works."
            };
        }

        public static string GetEmbodimentHelp(RuntimePatternDefinition pattern)
        {
            return pattern.participantEmbodiment switch
            {
                ParticipantEmbodimentRequirement.RequiredPawn => "Pawn-backed intent: each active participant should point to a PawnDefinition whose prefab has PawnRoot.",
                ParticipantEmbodimentRequirement.OptionalPawn => "Pawn optional: this pattern can be driven by a pawn, camera, cursor, card, board piece, menu, trap, or AI depending on the selected control surfaces.",
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired => "No character controller required: start with camera, cursor, UI, board, card, seat, or faction control instead of a PawnDefinition.",
                ParticipantEmbodimentRequirement.Custom => "Custom embodiment: explain the required participant surface in Description and Setup Notes before using this pattern in a setup profile.",
                _ => "No specific participant body is required unless another selected pattern asks for one."
            };
        }
    }
}
