using System.Collections.Generic;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Editor
{
    public sealed class PyralisAuthoringFeatureRow
    {
        public PyralisAuthoringFeatureRow(
            string feature,
            string source,
            string gameplayEffect,
            string unitySetup,
            string customization)
        {
            Feature = feature;
            Source = source;
            GameplayEffect = gameplayEffect;
            UnitySetup = unitySetup;
            Customization = customization;
        }

        public string Feature { get; }
        public string Source { get; }
        public string GameplayEffect { get; }
        public string UnitySetup { get; }
        public string Customization { get; }
    }

    public sealed class PyralisAuthoringDesignPrompt
    {
        public PyralisAuthoringDesignPrompt(string question, string options, string whyItMatters, string setupImpact)
        {
            Question = question;
            Options = options;
            WhyItMatters = whyItMatters;
            SetupImpact = setupImpact;
        }

        public string Question { get; }
        public string Options { get; }
        public string WhyItMatters { get; }
        public string SetupImpact { get; }
    }

    public sealed class PyralisAuthoringFeatureAdvisor
    {
        private readonly List<PyralisAuthoringFeatureRow> _selectedFeatures;
        private readonly List<PyralisAuthoringFeatureRow> _recommendedFeatures;
        private readonly List<PyralisAuthoringFeatureRow> _environmentGuidance;
        private readonly List<PyralisAuthoringDesignPrompt> _designPrompts;

        private PyralisAuthoringFeatureAdvisor(
            string routeIntent,
            string firstProofLabel,
            string firstProofGuidance,
            string firstUnityFocus,
            List<PyralisAuthoringFeatureRow> selectedFeatures,
            List<PyralisAuthoringFeatureRow> recommendedFeatures,
            List<PyralisAuthoringFeatureRow> environmentGuidance,
            List<PyralisAuthoringDesignPrompt> designPrompts)
        {
            RouteIntent = routeIntent;
            FirstProofLabel = firstProofLabel;
            FirstProofGuidance = firstProofGuidance;
            FirstUnityFocus = firstUnityFocus;
            _selectedFeatures = selectedFeatures ?? new List<PyralisAuthoringFeatureRow>();
            _recommendedFeatures = recommendedFeatures ?? new List<PyralisAuthoringFeatureRow>();
            _environmentGuidance = environmentGuidance ?? new List<PyralisAuthoringFeatureRow>();
            _designPrompts = designPrompts ?? new List<PyralisAuthoringDesignPrompt>();
        }

        public string RouteIntent { get; }
        public string FirstProofLabel { get; }
        public string FirstProofGuidance { get; }
        public string FirstUnityFocus { get; }
        public IReadOnlyList<PyralisAuthoringFeatureRow> SelectedFeatures => _selectedFeatures;
        public IReadOnlyList<PyralisAuthoringFeatureRow> RecommendedFeatures => _recommendedFeatures;
        public IReadOnlyList<PyralisAuthoringFeatureRow> EnvironmentGuidance => _environmentGuidance;
        public IReadOnlyList<PyralisAuthoringDesignPrompt> DesignPrompts => _designPrompts;

        public static PyralisAuthoringFeatureAdvisor Build(GameSetupProfile setupProfile)
        {
            return Build(PyralisAuthoringRouteDescriptor.Build(setupProfile));
        }

        public static PyralisAuthoringFeatureAdvisor Build(PyralisAuthoringRouteDescriptor route)
        {
            if (route == null || route.SetupProfile == null)
            {
                return new PyralisAuthoringFeatureAdvisor(
                    "No setup profile is selected yet.",
                    "Choose Capability Ingredients",
                    "Assign a GameSetupProfile before choosing feature wiring.",
                    "First Unity focus: assign a GameSetupProfile before choosing feature wiring.",
                    new List<PyralisAuthoringFeatureRow>(),
                    new List<PyralisAuthoringFeatureRow>(),
                    new List<PyralisAuthoringFeatureRow>(),
                    BuildDesignPrompts(false, false, false, false, false, false, false, false, false));
            }

            RuntimePatternDefinition[] patterns = route.Patterns;
            List<PyralisAuthoringFeatureRow> selected = new List<PyralisAuthoringFeatureRow>();
            bool hasPawn = route.HasPawn;
            bool hasCombat = route.HasCombat;
            bool hasProjectiles = route.HasProjectiles;
            bool hasActions = route.HasActions;
            bool hasTabletop = route.HasTabletop;
            bool hasCamera = route.HasCamera;
            bool hasAnimation = route.HasAnimation;
            bool hasScoring = route.HasScoring;
            bool hasProcedural = route.HasProcedural;
            bool hasNetworking = route.HasNetworking;
            PyralisAuthoringRouteProof proof = PyralisAuthoringRouteProof.Build(route);

            AddSelectedRows(route, patterns, selected);

            List<PyralisAuthoringFeatureRow> recommended = PyralisAuthoringCapabilityGuidance.BuildRecommendedRows(route);
            List<PyralisAuthoringFeatureRow> environmentGuidance = PyralisAuthoringCapabilityGuidance.BuildEnvironmentRows(route);
            List<PyralisAuthoringDesignPrompt> designPrompts = BuildDesignPrompts(
                hasPawn,
                hasCombat,
                hasProjectiles,
                hasActions,
                hasTabletop,
                hasCamera,
                hasScoring,
                hasProcedural,
                selected.Count > 0);

            return new PyralisAuthoringFeatureAdvisor(
                PyralisAuthoringCapabilityGuidance.GetRouteIntent(route, selected.Count),
                proof.Label,
                proof.Guidance,
                proof.FirstUnityFocus,
                selected,
                recommended,
                environmentGuidance,
                designPrompts);
        }

        private static void AddSelectedRows(PyralisAuthoringRouteDescriptor route, RuntimePatternDefinition[] patterns, List<PyralisAuthoringFeatureRow> selected)
        {
            HashSet<RuntimeCapabilityFamily> emitted = new HashSet<RuntimeCapabilityFamily>();
            if (patterns != null)
            {
                for (int i = 0; i < patterns.Length; i++)
                {
                    RuntimePatternDefinition pattern = patterns[i];
                    if (pattern == null || emitted.Contains(pattern.capabilityFamily))
                        continue;

                    emitted.Add(pattern.capabilityFamily);
                    selected.Add(PyralisAuthoringCapabilityGuidance.BuildSelectedRow(pattern));
                }
            }

            RuntimeCapabilityFamily[] families = route?.CapabilityFamilies ?? System.Array.Empty<RuntimeCapabilityFamily>();
            for (int i = 0; i < families.Length; i++)
            {
                RuntimeCapabilityFamily family = families[i];
                if (emitted.Contains(family))
                    continue;

                emitted.Add(family);
                selected.Add(PyralisAuthoringCapabilityGuidance.BuildSelectedRow(family));
            }
        }

        private static List<PyralisAuthoringDesignPrompt> BuildDesignPrompts(
            bool hasPawn,
            bool hasCombat,
            bool hasProjectiles,
            bool hasActions,
            bool hasTabletop,
            bool hasCamera,
            bool hasScoring,
            bool hasProcedural,
            bool hasSelectedPatterns)
        {
            List<PyralisAuthoringDesignPrompt> prompts = new List<PyralisAuthoringDesignPrompt>();

            prompts.Add(new PyralisAuthoringDesignPrompt(
                "What is the player actually controlling?",
                hasTabletop
                    ? "Seat, hand, board piece, cursor, camera, menu command, or optional pawn."
                    : hasPawn
                        ? "Pawn actor first, with optional camera, cursor, menu, or action command surfaces."
                        : "Camera, cursor, board/card surface, menu command, faction, AI director, or later a pawn.",
                "This decides whether the selected intent asks ParticipantDefinition for a PawnDefinition or whether empty pawn fields are correct.",
                "Pick the control surface before creating pawn prefabs, PlayerInputManager, board presenters, or action UI."));

            prompts.Add(new PyralisAuthoringDesignPrompt(
                "What kind of space does the game happen in?",
                hasTabletop
                    ? "Board grid, card table, menu-driven arena, map, or hybrid board plus 3D/2D scene."
                    : hasProcedural
                        ? "Generated rooms, chunks, lanes, waves, encounters, or seeded board layouts."
                        : "2D platform, top-down arena, 2.5D lane, 3D room, open zone, menu screen, board, or card table.",
                "Environment can be plain Unity art, but Pyralis systems read the consequences: colliders, layers, bounds, zones, anchors, and selectable surfaces.",
                "Create an Environment/Playfield Root early, then decide which parts are visual only and which parts are walkable, bounded, spawnable, hazardous, selectable, or camera-framing surfaces."));

            if (hasCombat || hasProjectiles || hasActions)
            {
                prompts.Add(new PyralisAuthoringDesignPrompt(
                    "What is the first proof of interaction?",
                    hasCombat
                        ? "One hit, one hurt reaction, one blocked hit, one projectile impact, or one selected action."
                        : "One selected action, one projectile shot, one card/board choice, or one UI command.",
                    "A small interaction proof keeps the authoring path from turning into a giant menu or combat tree before the runtime surface works.",
                    "Wire one action source to one target surface, then add HUD/feedback after the runtime event is visible in Play Mode."));
            }

            if (hasScoring)
            {
                prompts.Add(new PyralisAuthoringDesignPrompt(
                    "What makes progress visible?",
                    "Score label, timer, objective text, lives/resources, victory points, win/loss screen, combat feedback, or board state.",
                    "HUD should explain the game state the player needs now, not every system Pyralis can support.",
                    "Prove the service changes first, then connect Canvas labels, feedback presenters, and game-over/menu flow."));
            }

            if (!hasSelectedPatterns)
            {
                prompts.Add(new PyralisAuthoringDesignPrompt(
                    "Which capability should be selected first?",
                    "Pawn action, board/card/tabletop, camera/cursor, action selection, combat, projectiles, scoring, animation/presentation, procedural, or networking.",
                    "Capability ingredients are the authoring contract that turns design intent into route-aware setup guidance.",
                    "Choose capability families before wiring scene roots. Add optional route contracts only when the existing capability language cannot describe the game."));
            }

            return prompts;
        }

    }
}
