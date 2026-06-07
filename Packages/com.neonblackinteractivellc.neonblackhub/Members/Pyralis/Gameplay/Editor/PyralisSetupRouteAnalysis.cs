using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisAuthoringRouteCapability
    {
        PawnAction,
        Combat,
        Projectile,
        ActionSelection,
        Tabletop,
        CameraCursor,
        AnimationPresentation,
        Scoring,
        Procedural,
        Networking,
        PlatformCore
    }

    public sealed class PyralisAuthoringRouteFact
    {
        public PyralisAuthoringRouteFact(PyralisAuthoringRouteCapability capability, string label, RuntimeCapabilityFamily family, bool primaryProofCandidate)
        {
            Capability = capability;
            Label = label ?? string.Empty;
            Family = family;
            PrimaryProofCandidate = primaryProofCandidate;
        }

        public PyralisAuthoringRouteCapability Capability { get; }
        public string Label { get; }
        public RuntimeCapabilityFamily Family { get; }
        public bool PrimaryProofCandidate { get; }
    }

    public sealed class PyralisSetupRouteAnalysis
    {
        private PyralisSetupRouteAnalysis(
            SessionDefinition session,
            GameModeDefinition mode,
            GameSetupProfile setupProfile,
            RuntimePatternDefinition[] patterns,
            string[] requiredRuntimeSystems,
            bool hasAssignedPatterns,
            bool hasValidPatterns,
            bool requiresPawn,
            bool hasParticipants,
            bool hasAnyDefaultPawn,
            string participantPawnIssue,
            PyralisAuthoringRouteFact[] routeFacts)
        {
            Session = session;
            Mode = mode;
            SetupProfile = setupProfile;
            Patterns = patterns;
            RequiredRuntimeSystems = requiredRuntimeSystems;
            HasAssignedPatterns = hasAssignedPatterns;
            HasValidPatterns = hasValidPatterns;
            RequiresPawn = requiresPawn;
            HasParticipants = hasParticipants;
            HasAnyDefaultPawn = hasAnyDefaultPawn;
            ParticipantPawnIssue = participantPawnIssue;
            RouteFacts = routeFacts ?? System.Array.Empty<PyralisAuthoringRouteFact>();
        }

        public SessionDefinition Session { get; }
        public GameModeDefinition Mode { get; }
        public GameSetupProfile SetupProfile { get; }
        public RuntimePatternDefinition[] Patterns { get; }
        public string[] RequiredRuntimeSystems { get; }
        public bool HasAssignedPatterns { get; }
        public bool HasValidPatterns { get; }
        public bool RequiresPawn { get; }
        public bool HasParticipants { get; }
        public bool HasAnyDefaultPawn { get; }
        public string ParticipantPawnIssue { get; }
        public PyralisAuthoringRouteFact[] RouteFacts { get; }
        public PyralisAuthoringRouteFact PrimaryRouteFact => RouteFacts.Length > 0 ? RouteFacts[0] : null;

        public string RouteName
        {
            get
            {
                if (SetupProfile == null || !HasValidPatterns)
                    return "No setup route selected";

                if (RouteFacts.Length == 0)
                    return RequiresPawn ? "Pawn-backed route" : "No-pawn-capable route";

                return RouteFacts.Length == 1
                    ? $"{RouteFacts[0].Label} route"
                    : $"{RouteFacts[0].Label} + {RouteFacts.Length - 1} capability route";
            }
        }

        public static PyralisSetupRouteAnalysis Build(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return Build((SessionDefinition)null);

            SerializedObject serializedBootstrap = new SerializedObject(bootstrap);
            SessionDefinition session = serializedBootstrap.FindProperty("sessionDefinition")?.objectReferenceValue as SessionDefinition;
            return Build(session);
        }

        public static PyralisSetupRouteAnalysis Build(SessionDefinition session)
        {
            GameModeDefinition mode = session != null ? session.defaultGameMode : null;
            return Build(mode, session);
        }

        public static PyralisSetupRouteAnalysis Build(GameModeDefinition mode, SessionDefinition session = null)
        {
            GameSetupProfile setupProfile = mode != null ? mode.setupProfile : null;
            return Build(setupProfile, session, mode);
        }

        public static PyralisSetupRouteAnalysis Build(GameSetupProfile setupProfile, SessionDefinition session = null, GameModeDefinition mode = null)
        {
            RuntimePatternDefinition[] patterns = setupProfile != null ? setupProfile.runtimePatterns : null;
            string[] requiredRuntimeSystems = CollectRequiredRuntimeSystems(patterns);
            bool hasAssignedPatterns = CheckHasAssignedPatterns(patterns);
            bool hasValidPatterns = CheckHasValidPatterns(patterns);
            bool requiresPawn = RequiresPawnPattern(patterns);
            bool hasParticipants = CheckHasParticipants(session);
            bool hasAnyDefaultPawn = CheckHasAnyDefaultPawn(session);
            string participantPawnIssue = GetParticipantPawnIssue(session);
            PyralisAuthoringRouteFact[] routeFacts = BuildRouteFacts(patterns);

            return new PyralisSetupRouteAnalysis(
                session,
                mode,
                setupProfile,
                patterns,
                requiredRuntimeSystems,
                hasAssignedPatterns,
                hasValidPatterns,
                requiresPawn,
                hasParticipants,
                hasAnyDefaultPawn,
                participantPawnIssue,
                routeFacts);
        }

        public bool UsesCamera()
        {
            return RequiresAnyFirstProof(
                    RuntimePatternFirstProofRequirement.CameraRig,
                    RuntimePatternFirstProofRequirement.CameraBounds2D,
                    RuntimePatternFirstProofRequirement.CameraBoundsService)
                || HasFamily(RuntimeCapabilityFamily.CameraInput)
                || SupportsSurface(RuntimeControlSurface.Camera)
                || SupportsSurface(RuntimeControlSurface.Cursor);
        }

        public bool LikelyUsesInputManager()
        {
            return RequiresFirstProof(RuntimePatternFirstProofRequirement.PlayerInputManager)
                || Session != null
                && Session.networkMode == GameplayNetworkMode.LocalOnly
                && Session.GetEffectiveMaxParticipants() > 1
                && SupportsSurface(RuntimeControlSurface.Pawn);
        }

        public bool UsesPlayfield()
        {
            return HasFamily(RuntimeCapabilityFamily.CharacterPawnGameplay)
                || HasFamily(RuntimeCapabilityFamily.BoardCardTabletop)
                || HasFamily(RuntimeCapabilityFamily.ProceduralGeneration);
        }

        public bool UsesScoring()
        {
            return RequiresFirstProof(RuntimePatternFirstProofRequirement.ScoreService)
                || HasFamily(RuntimeCapabilityFamily.ScoringObjectives);
        }

        public bool UsesPawnGameplay()
        {
            return HasFamily(RuntimeCapabilityFamily.CharacterPawnGameplay)
                || SupportsSurface(RuntimeControlSurface.Pawn);
        }

        public bool Requires2DCameraBounds()
        {
            if (!UsesPawnGameplay() || Patterns == null)
                return false;

            bool hasExplicitFirstProofRequirements = false;
            for (int i = 0; i < Patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = Patterns[i];
                if (!IsValidPattern(pattern))
                    continue;

                if (pattern.firstProofRequirements != RuntimePatternFirstProofRequirement.None)
                {
                    hasExplicitFirstProofRequirements = true;
                    if (pattern.RequiresFirstProof(RuntimePatternFirstProofRequirement.CameraBounds2D))
                        return true;
                }
            }

            if (hasExplicitFirstProofRequirements)
                return false;

            for (int i = 0; i < Patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = Patterns[i];
                if (!IsValidPattern(pattern))
                    continue;

                string patternText = $"{pattern.patternId} {pattern.displayName} {pattern.description} {pattern.setupNotes}";
                if (ContainsIgnoreCase(patternText, "3d")
                    || ContainsIgnoreCase(patternText, "3D")
                    || ContainsIgnoreCase(patternText, "billboard")
                    || ContainsIgnoreCase(patternText, "rigged"))
                {
                    continue;
                }

                if (ContainsIgnoreCase(patternText, "2d")
                    || ContainsIgnoreCase(patternText, "2D")
                    || ContainsIgnoreCase(patternText, "sprite"))
                {
                    return true;
                }
            }

            return false;
        }

        public bool UsesProjectileCombat()
        {
            return RequiresFirstProof(RuntimePatternFirstProofRequirement.ProjectileOrHitboxSource)
                || HasFamily(RuntimeCapabilityFamily.GunsProjectiles);
        }

        public bool UsesTabletopContract()
        {
            return RequiresFirstProof(RuntimePatternFirstProofRequirement.TabletopRuntimeContract)
                || HasFamily(RuntimeCapabilityFamily.BoardCardTabletop)
                || SupportsSurface(RuntimeControlSurface.BoardSeat)
                || SupportsSurface(RuntimeControlSurface.BoardPiece)
                || SupportsSurface(RuntimeControlSurface.CardHand);
        }

        public bool UsesActionSelection()
        {
            return RequiresFirstProof(RuntimePatternFirstProofRequirement.SelectionSurface)
                || HasFamily(RuntimeCapabilityFamily.ActionTargeting)
                || SupportsSurface(RuntimeControlSurface.MenuSelection)
                || SupportsSurface(RuntimeControlSurface.Cursor)
                || SupportsSurface(RuntimeControlSurface.CardHand);
        }

        public bool RequiresFirstProof(RuntimePatternFirstProofRequirement requirement)
        {
            if (Patterns == null || requirement == RuntimePatternFirstProofRequirement.None)
                return false;

            for (int i = 0; i < Patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = Patterns[i];
                if (IsValidPattern(pattern) && pattern.RequiresFirstProof(requirement))
                    return true;
            }

            return false;
        }

        private bool RequiresAnyFirstProof(params RuntimePatternFirstProofRequirement[] requirements)
        {
            if (requirements == null)
                return false;

            for (int i = 0; i < requirements.Length; i++)
            {
                if (RequiresFirstProof(requirements[i]))
                    return true;
            }

            return false;
        }

        public bool RequiresRuntimeSystem(string token)
        {
            if (RequiredRuntimeSystems == null || string.IsNullOrWhiteSpace(token))
                return false;

            for (int i = 0; i < RequiredRuntimeSystems.Length; i++)
            {
                string requiredSystem = RequiredRuntimeSystems[i];
                if (!string.IsNullOrWhiteSpace(requiredSystem)
                    && requiredSystem.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasFamily(RuntimeCapabilityFamily family)
        {
            if (Patterns == null)
                return false;

            for (int i = 0; i < Patterns.Length; i++)
            {
                if (IsValidPattern(Patterns[i]) && Patterns[i].capabilityFamily == family)
                    return true;
            }

            return false;
        }

        private bool SupportsSurface(RuntimeControlSurface surface)
        {
            if (Patterns == null)
                return false;

            for (int i = 0; i < Patterns.Length; i++)
            {
                if (IsValidPattern(Patterns[i]) && Patterns[i].SupportsControlSurface(surface))
                    return true;
            }

            return false;
        }

        private static bool CheckHasAssignedPatterns(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return false;

            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i] != null)
                    return true;
            }

            return false;
        }

        private static bool ContainsIgnoreCase(string value, string token)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(token)
                && value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool CheckHasValidPatterns(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return false;

            for (int i = 0; i < patterns.Length; i++)
            {
                if (IsValidPattern(patterns[i]))
                    return true;
            }

            return false;
        }

        private static string[] CollectRequiredRuntimeSystems(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return System.Array.Empty<string>();

            List<string> requiredSystems = new List<string>();
            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = patterns[i];
                if (!IsValidPattern(pattern) || pattern.requiredRuntimeSystems == null)
                    continue;

                for (int systemIndex = 0; systemIndex < pattern.requiredRuntimeSystems.Length; systemIndex++)
                {
                    string requiredSystem = pattern.requiredRuntimeSystems[systemIndex];
                    if (string.IsNullOrWhiteSpace(requiredSystem) || Contains(requiredSystems, requiredSystem))
                        continue;

                    requiredSystems.Add(requiredSystem.Trim());
                }
            }

            return requiredSystems.ToArray();
        }

        private static bool Contains(List<string> values, string candidate)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], candidate, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static bool RequiresPawnPattern(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return false;

            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = patterns[i];
                if (IsValidPattern(pattern) && pattern.participantEmbodiment == ParticipantEmbodimentRequirement.RequiredPawn)
                    return true;
            }

            return false;
        }

        private static PyralisAuthoringRouteFact[] BuildRouteFacts(RuntimePatternDefinition[] patterns)
        {
            if (patterns == null)
                return System.Array.Empty<PyralisAuthoringRouteFact>();

            List<PyralisAuthoringRouteFact> facts = new List<PyralisAuthoringRouteFact>();
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.CharacterPawnGameplay, PyralisAuthoringRouteCapability.PawnAction, "Pawn Action", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.BoardCardTabletop, PyralisAuthoringRouteCapability.Tabletop, "Tabletop", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.ActionTargeting, PyralisAuthoringRouteCapability.ActionSelection, "Action Selection", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.GunsProjectiles, PyralisAuthoringRouteCapability.Projectile, "Projectile", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.Combat, PyralisAuthoringRouteCapability.Combat, "Combat", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.ScoringObjectives, PyralisAuthoringRouteCapability.Scoring, "Scoring", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.CameraInput, PyralisAuthoringRouteCapability.CameraCursor, "Camera / Cursor", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.ProceduralGeneration, PyralisAuthoringRouteCapability.Procedural, "Procedural", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.Networking, PyralisAuthoringRouteCapability.Networking, "Networking", true);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.AnimationPresentation, PyralisAuthoringRouteCapability.AnimationPresentation, "Animation / Presentation", false);
            AddFactIfFamily(facts, patterns, RuntimeCapabilityFamily.PlatformCore, PyralisAuthoringRouteCapability.PlatformCore, "Platform Core", false);
            return facts.ToArray();
        }

        private static void AddFactIfFamily(
            List<PyralisAuthoringRouteFact> facts,
            RuntimePatternDefinition[] patterns,
            RuntimeCapabilityFamily family,
            PyralisAuthoringRouteCapability capability,
            string label,
            bool primaryProofCandidate)
        {
            if (!ContainsFamily(patterns, family))
                return;

            facts.Add(new PyralisAuthoringRouteFact(capability, label, family, primaryProofCandidate));
        }

        private static bool ContainsFamily(RuntimePatternDefinition[] patterns, RuntimeCapabilityFamily family)
        {
            for (int i = 0; i < patterns.Length; i++)
            {
                RuntimePatternDefinition pattern = patterns[i];
                if (IsValidPattern(pattern) && pattern.capabilityFamily == family)
                    return true;
            }

            return false;
        }

        private static bool IsValidPattern(RuntimePatternDefinition pattern)
        {
            return pattern != null && pattern.GetValidationIssues().Count == 0;
        }

        private static bool CheckHasParticipants(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                if (session.defaultParticipants[i] == null)
                    return false;
            }

            return true;
        }

        private static bool CheckHasAnyDefaultPawn(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.defaultPawn != null)
                    return true;
            }

            return false;
        }

        private static string GetParticipantPawnIssue(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return "Assign default participants before checking pawn readiness.";

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    return $"Default participant slot {i} is empty.";

                if (participant.defaultPawn == null)
                    return $"Participant `{participant.displayName}` needs a PawnDefinition for pawn-backed setup.";

                PawnDefinition pawn = participant.defaultPawn;
                if (pawn.pawnPrefab == null)
                    return $"PawnDefinition `{pawn.name}` needs a pawn prefab before this setup can spawn participants.";

                if (pawn.pawnPrefab.GetComponent<PawnRoot>() == null)
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` needs PawnRoot on its root GameObject.";

                if (!PrefabHasComponent<IPawnMotor>(pawn.pawnPrefab))
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` needs a component that implements IPawnMotor.";

                if (!PrefabHasComponent<IPawnPresentationModule>(pawn.pawnPrefab))
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` needs a component that implements IPawnPresentationModule.";

                if (!PrefabHasComponent<IPawnInputModule>(pawn.pawnPrefab))
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` needs a component that implements IPawnInputModule so InputProfile actions can reach movement.";

                List<string> pawnIssues = PyralisAuthoringValidationModel.BuildPawnRouteValidationIssues(pawn);
                if (pawnIssues.Count > 0)
                    return $"PawnDefinition `{pawn.name}`: {pawnIssues[0]}";
            }

            return null;
        }

        private static bool PrefabHasComponent<T>(GameObject prefab) where T : class
        {
            if (prefab == null)
                return false;

            MonoBehaviour[] behaviours = prefab.GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is T)
                    return true;
            }

            return false;
        }
    }
}
