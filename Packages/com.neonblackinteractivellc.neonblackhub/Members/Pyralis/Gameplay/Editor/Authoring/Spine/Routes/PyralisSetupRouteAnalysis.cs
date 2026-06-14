using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
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

    public enum PyralisParticipantPawnIssueKind
    {
        None,
        MissingParticipants,
        EmptyParticipantSlot,
        MissingPawnDefinition,
        MissingPawnPrefab,
        MissingPawnRoot,
        MissingMotor,
        MissingPresentation,
        MissingInputModule,
        PawnValidation
    }

    public sealed class PyralisSetupRouteAnalysis
    {
        private PyralisSetupRouteAnalysis(
            SessionDefinition session,
            GameModeDefinition mode,
            RuntimeCapabilityFamily[] capabilityFamilies,
            bool requiresPawn,
            bool hasParticipants,
            bool hasAnyDefaultPawn,
            string participantPawnIssue,
            PyralisParticipantPawnIssueKind participantPawnIssueKind,
            PyralisAuthoringRouteFact[] routeFacts)
        {
            Session = session;
            Mode = mode;
            CapabilityFamilies = capabilityFamilies ?? System.Array.Empty<RuntimeCapabilityFamily>();
            HasSelectedCapabilities = CapabilityFamilies.Length > 0;
            RequiresPawn = requiresPawn;
            HasParticipants = hasParticipants;
            HasAnyDefaultPawn = hasAnyDefaultPawn;
            ParticipantPawnIssue = participantPawnIssue;
            ParticipantPawnIssueKind = participantPawnIssueKind;
            RouteFacts = routeFacts ?? System.Array.Empty<PyralisAuthoringRouteFact>();
        }

        public SessionDefinition Session { get; }
        public GameModeDefinition Mode { get; }
        public RuntimeCapabilityFamily[] CapabilityFamilies { get; }
        public bool HasSelectedCapabilities { get; }
        public bool RequiresPawn { get; }
        public bool HasParticipants { get; }
        public bool HasAnyDefaultPawn { get; }
        public string ParticipantPawnIssue { get; }
        public PyralisParticipantPawnIssueKind ParticipantPawnIssueKind { get; }
        public PyralisAuthoringRouteFact[] RouteFacts { get; }
        public PyralisAuthoringRouteFact PrimaryRouteFact => RouteFacts.Length > 0 ? RouteFacts[0] : null;

        public string RouteName
        {
            get
            {
                if (!HasSelectedCapabilities)
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
            PyralisSetupDependencyTree dependencyTree = PyralisSetupDependencyTree.Build(bootstrap);
            return BuildResolved(dependencyTree.Session, dependencyTree.Mode);
        }

        public static PyralisSetupRouteAnalysis Build(SessionDefinition session)
        {
            PyralisSetupDependencyTree dependencyTree = PyralisSetupDependencyTree.Build(session);
            return BuildResolved(dependencyTree.Session, dependencyTree.Mode);
        }

        public static PyralisSetupRouteAnalysis Build(GameModeDefinition mode, SessionDefinition session = null)
        {
            PyralisSetupDependencyTree dependencyTree = PyralisSetupDependencyTree.Build(session != null ? session : mode);
            GameModeDefinition resolvedMode = mode != null ? mode : dependencyTree.Mode;
            return BuildResolved(session != null ? session : dependencyTree.Session, resolvedMode);
        }

        public static PyralisSetupRouteAnalysis Build(UnityEngine.Object source)
        {
            if (source is GameplaySessionBootstrap bootstrap)
                return Build(bootstrap);
            if (source is SessionDefinition session)
                return Build(session);
            if (source is GameModeDefinition mode)
                return Build(mode);

            return BuildResolved(null, null);
        }

        private static PyralisSetupRouteAnalysis BuildResolved(SessionDefinition session, GameModeDefinition mode)
        {
            RuntimeCapabilityFamily[] capabilityFamilies = CollectCapabilityFamilies(session, mode);
            bool requiresPawn = ContainsFamily(capabilityFamilies, RuntimeCapabilityFamily.CharacterPawnGameplay);
            bool hasParticipants = CheckHasParticipants(session);
            bool hasAnyDefaultPawn = CheckHasAnyDefaultPawn(session);
            string participantPawnIssue = GetParticipantPawnIssue(session, out PyralisParticipantPawnIssueKind participantPawnIssueKind);
            PyralisAuthoringRouteFact[] routeFacts = BuildRouteFacts(capabilityFamilies);

            return new PyralisSetupRouteAnalysis(
                session,
                mode,
                capabilityFamilies,
                requiresPawn,
                hasParticipants,
                hasAnyDefaultPawn,
                participantPawnIssue,
                participantPawnIssueKind,
                routeFacts);
        }

        public bool UsesCamera()
        {
            return HasFamily(RuntimeCapabilityFamily.CameraInput);
        }

        public bool LikelyUsesInputManager()
        {
            return Session != null
                && Session.networkMode == GameplayNetworkMode.LocalOnly
                && Session.GetEffectiveMaxParticipants() > 1
                && UsesPawnGameplay();
        }

        public bool UsesPlayfield()
        {
            return HasFamily(RuntimeCapabilityFamily.CharacterPawnGameplay)
                || HasFamily(RuntimeCapabilityFamily.BoardCardTabletop)
                || HasFamily(RuntimeCapabilityFamily.ProceduralGeneration);
        }

        public bool UsesScoring()
        {
            return HasFamily(RuntimeCapabilityFamily.ScoringObjectives);
        }

        public bool UsesPawnGameplay()
        {
            return HasFamily(RuntimeCapabilityFamily.CharacterPawnGameplay);
        }

        public bool Requires2DCameraBounds()
        {
            return UsesPawnGameplay() && Mode != null && Mode.cameraRigProfile != null;
        }

        public bool UsesProjectileCombat()
        {
            return HasFamily(RuntimeCapabilityFamily.GunsProjectiles);
        }

        public bool UsesTabletopContract()
        {
            return HasFamily(RuntimeCapabilityFamily.BoardCardTabletop);
        }

        public bool UsesActionSelection()
        {
            return HasFamily(RuntimeCapabilityFamily.ActionTargeting);
        }

        private bool HasFamily(RuntimeCapabilityFamily family)
        {
            return ContainsFamily(CapabilityFamilies, family);
        }

        private static bool ContainsIgnoreCase(string value, string token)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(token)
                && value.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static RuntimeCapabilityFamily[] CollectCapabilityFamilies(SessionDefinition session, GameModeDefinition mode)
        {
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            AddFamiliesFromMode(families, mode);
            AddFamiliesFromParticipants(families, session);

            return families.ToArray();
        }

        private static void AddFamiliesFromMode(List<RuntimeCapabilityFamily> families, GameModeDefinition mode)
        {
            if (mode == null)
                return;

            AddFamily(families, RuntimeCapabilityFamily.PlatformCore);

            if (mode.requiredFeatureModules != null && mode.requiredFeatureModules.Length > 0)
            {
                for (int i = 0; i < mode.requiredFeatureModules.Length; i++)
                    AddFamiliesFromFeatureModule(families, mode.requiredFeatureModules[i]);
            }

            if (mode.enableCombat)
                AddFamily(families, RuntimeCapabilityFamily.Combat);
            if (mode.enableScore)
                AddFamily(families, RuntimeCapabilityFamily.ScoringObjectives);
            if (mode.boardDefinition != null || mode.turnOrderDefinition != null || mode.boardTerminalConditions != null && mode.boardTerminalConditions.Length > 0)
                AddFamily(families, RuntimeCapabilityFamily.BoardCardTabletop);
            if (mode.cameraRigProfile != null)
                AddFamily(families, RuntimeCapabilityFamily.CameraInput);
            if (mode.playfieldProfile != null)
                AddFamily(families, RuntimeCapabilityFamily.CharacterPawnGameplay);
        }

        private static void AddFamiliesFromParticipants(List<RuntimeCapabilityFamily> families, SessionDefinition session)
        {
            if (session == null)
                return;

            if (session.networkMode != GameplayNetworkMode.LocalOnly)
                AddFamily(families, RuntimeCapabilityFamily.Networking);

            if (session.defaultInputProfile != null)
                AddFamily(families, RuntimeCapabilityFamily.CameraInput);

            if (session.defaultParticipants == null)
                return;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                if (participant.inputProfile != null)
                    AddFamily(families, RuntimeCapabilityFamily.CameraInput);

                PawnDefinition pawn = participant.defaultPawn;
                if (pawn == null)
                    continue;

                AddFamily(families, RuntimeCapabilityFamily.CharacterPawnGameplay);

                if (pawn.combatProfile != null)
                    AddFamily(families, RuntimeCapabilityFamily.Combat);
                if (pawn.presentationProfile != null || pawn.animationProfile != null)
                    AddFamily(families, RuntimeCapabilityFamily.AnimationPresentation);
                if (pawn.defaultInputProfile != null)
                    AddFamily(families, RuntimeCapabilityFamily.CameraInput);
                if (pawn.featureModules != null)
                {
                    for (int moduleIndex = 0; moduleIndex < pawn.featureModules.Length; moduleIndex++)
                        AddFamiliesFromFeatureModule(families, pawn.featureModules[moduleIndex]);
                }
            }
        }

        private static void AddFamiliesFromFeatureModule(List<RuntimeCapabilityFamily> families, FeatureModuleDefinition module)
        {
            if (module == null)
                return;

            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId(module.moduleId);
            if (contract != null)
            {
                RuntimeCapabilityFamily[] reflectedFamilies = PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                    contract.Capability,
                    RuntimeCapabilityLaneTag.Mixed,
                    contract.Axioms);
                for (int i = 0; i < reflectedFamilies.Length; i++)
                    AddFamily(families, reflectedFamilies[i]);
            }

            string haystack = $"{module.moduleId} {module.displayName} {module.authoringCategory}";
            if (ContainsIgnoreCase(haystack, "combat") || ContainsIgnoreCase(haystack, "damage"))
                AddFamily(families, RuntimeCapabilityFamily.Combat);
            if (ContainsIgnoreCase(haystack, "projectile") || ContainsIgnoreCase(haystack, "gun"))
                AddFamily(families, RuntimeCapabilityFamily.GunsProjectiles);
            if (ContainsIgnoreCase(haystack, "score") || ContainsIgnoreCase(haystack, "objective"))
                AddFamily(families, RuntimeCapabilityFamily.ScoringObjectives);
            if (ContainsIgnoreCase(haystack, "input") || ContainsIgnoreCase(haystack, "camera"))
                AddFamily(families, RuntimeCapabilityFamily.CameraInput);
            if (ContainsIgnoreCase(haystack, "animation") || ContainsIgnoreCase(haystack, "presentation") || ContainsIgnoreCase(haystack, "feedback"))
                AddFamily(families, RuntimeCapabilityFamily.AnimationPresentation);
        }

        private static void AddFamily(List<RuntimeCapabilityFamily> families, RuntimeCapabilityFamily family)
        {
            if (!ContainsFamily(families, family))
                families.Add(family);
        }

        private static bool ContainsFamily(List<RuntimeCapabilityFamily> families, RuntimeCapabilityFamily family)
        {
            for (int i = 0; i < families.Count; i++)
            {
                if (families[i] == family)
                    return true;
            }

            return false;
        }

        private static bool ContainsFamily(RuntimeCapabilityFamily[] families, RuntimeCapabilityFamily family)
        {
            if (families == null)
                return false;

            for (int i = 0; i < families.Length; i++)
            {
                if (families[i] == family)
                    return true;
            }

            return false;
        }

        private static PyralisAuthoringRouteFact[] BuildRouteFacts(RuntimeCapabilityFamily[] families)
        {
            if (families == null)
                return System.Array.Empty<PyralisAuthoringRouteFact>();

            List<PyralisAuthoringRouteFact> facts = new List<PyralisAuthoringRouteFact>();
            for (int i = 0; i < families.Length; i++)
                AddFact(facts, families[i]);

            return facts.ToArray();
        }

        private static void AddFact(List<PyralisAuthoringRouteFact> facts, RuntimeCapabilityFamily family)
        {
            PyralisAuthoringCapabilityDescriptor descriptor = PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(family);
            if (TryGetRouteCapability(family, out PyralisAuthoringRouteCapability capability))
            {
                string label = descriptor != null && !string.IsNullOrWhiteSpace(descriptor.DisplayName)
                    ? descriptor.DisplayName
                    : family.ToString();
                bool primaryProofCandidate = descriptor != null && !string.IsNullOrWhiteSpace(descriptor.ProofTargetId);
                facts.Add(new PyralisAuthoringRouteFact(capability, label, family, primaryProofCandidate));
            }
        }

        private static bool TryGetRouteCapability(
            RuntimeCapabilityFamily family,
            out PyralisAuthoringRouteCapability capability)
        {
            capability = PyralisAuthoringRouteCapability.PlatformCore;

            switch (family)
            {
                case RuntimeCapabilityFamily.CharacterPawnGameplay:
                    capability = PyralisAuthoringRouteCapability.PawnAction;
                    return true;
                case RuntimeCapabilityFamily.Combat:
                    capability = PyralisAuthoringRouteCapability.Combat;
                    return true;
                case RuntimeCapabilityFamily.ActionTargeting:
                    capability = PyralisAuthoringRouteCapability.ActionSelection;
                    return true;
                case RuntimeCapabilityFamily.CameraInput:
                    capability = PyralisAuthoringRouteCapability.CameraCursor;
                    return true;
                case RuntimeCapabilityFamily.ScoringObjectives:
                    capability = PyralisAuthoringRouteCapability.Scoring;
                    return true;
                case RuntimeCapabilityFamily.BoardCardTabletop:
                    capability = PyralisAuthoringRouteCapability.Tabletop;
                    return true;
                case RuntimeCapabilityFamily.GunsProjectiles:
                    capability = PyralisAuthoringRouteCapability.Projectile;
                    return true;
                case RuntimeCapabilityFamily.ProceduralGeneration:
                    capability = PyralisAuthoringRouteCapability.Procedural;
                    return true;
                case RuntimeCapabilityFamily.Networking:
                    capability = PyralisAuthoringRouteCapability.Networking;
                    return true;
                case RuntimeCapabilityFamily.AnimationPresentation:
                    capability = PyralisAuthoringRouteCapability.AnimationPresentation;
                    return true;
                case RuntimeCapabilityFamily.PlatformCore:
                    capability = PyralisAuthoringRouteCapability.PlatformCore;
                    return true;
                default:
                    return false;
            }
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

        private static string GetParticipantPawnIssue(SessionDefinition session, out PyralisParticipantPawnIssueKind issueKind)
        {
            if (session == null || session.defaultParticipants == null || session.defaultParticipants.Length == 0)
            {
                issueKind = PyralisParticipantPawnIssueKind.MissingParticipants;
                return "Assign default participants before checking pawn readiness.";
            }

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                {
                    issueKind = PyralisParticipantPawnIssueKind.EmptyParticipantSlot;
                    return $"Default participant slot {i} is empty.";
                }

                if (participant.defaultPawn == null)
                {
                    issueKind = PyralisParticipantPawnIssueKind.MissingPawnDefinition;
                    return $"Selected pawn-backed intent asks participant `{participant.displayName}` to use a PawnDefinition before participants can spawn.";
                }

                PawnDefinition pawn = participant.defaultPawn;
                if (pawn.pawnPrefab == null)
                {
                    issueKind = PyralisParticipantPawnIssueKind.MissingPawnPrefab;
                    return $"Selected pawn-backed intent asks PawnDefinition `{pawn.name}` to point at a pawn prefab before participants can spawn.";
                }

                if (pawn.pawnPrefab.GetComponent<PawnRoot>() == null)
                {
                    issueKind = PyralisParticipantPawnIssueKind.MissingPawnRoot;
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` is missing PawnRoot on its root GameObject.";
                }

                if (!PrefabHasComponent<IPawnMotor>(pawn.pawnPrefab))
                {
                    issueKind = PyralisParticipantPawnIssueKind.MissingMotor;
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` is missing a lane motor component that implements IPawnMotor.";
                }

                if (!PrefabHasComponent<IPawnPresentationModule>(pawn.pawnPrefab))
                {
                    issueKind = PyralisParticipantPawnIssueKind.MissingPresentation;
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` is missing a presentation component that implements IPawnPresentationModule.";
                }

                if (!PrefabHasComponent<IPawnInputModule>(pawn.pawnPrefab))
                {
                    issueKind = PyralisParticipantPawnIssueKind.MissingInputModule;
                    return $"Pawn prefab `{pawn.pawnPrefab.name}` is missing an input adapter that implements IPawnInputModule so the selected InputProfile can reach movement.";
                }

                List<string> pawnIssues = PyralisPawnPrefabReadinessAnalysis.BuildIssues(pawn);
                if (pawnIssues.Count > 0)
                {
                    issueKind = PyralisParticipantPawnIssueKind.PawnValidation;
                    return $"PawnDefinition `{pawn.name}`: {pawnIssues[0]}";
                }
            }

            issueKind = PyralisParticipantPawnIssueKind.None;
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
