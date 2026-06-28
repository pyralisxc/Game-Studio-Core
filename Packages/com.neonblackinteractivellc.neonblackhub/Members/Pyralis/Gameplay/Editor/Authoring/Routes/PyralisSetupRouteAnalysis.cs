using System.Collections.Generic;
using NeonBlack.Gameplay.Glue.Bootstrap;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Glue.Session;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Glue.InputRouting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public enum PyralisParticipantTopology
    {
        Unknown,
        NoParticipants,
        SoloLocal,
        LocalJoin,
        Networked,
        HybridLocalNetworked
    }

    public enum PyralisParticipantJoinPolicy
    {
        Unknown,
        NoParticipants,
        AutoRegisterDefaults,
        PlayerInputJoin,
        NetworkAuthority,
        HybridPlayerInputAndNetwork
    }

    public enum PyralisParticipantSpawnPolicy
    {
        Unknown,
        NoPawnSpawn,
        SpawnOnRegister,
        ManualSpawn
    }

    public sealed class PyralisParticipantSeatReadiness
    {
        public PyralisParticipantSeatReadiness(
            int slotIndex,
            int seatIndex,
            ParticipantDefinition participant,
            PawnDefinition pawn,
            InputProfile inputProfile,
            bool requiresPawn,
            string inputIssue,
            string pawnIssue,
            PyralisParticipantPawnIssueKind pawnIssueKind)
        {
            SlotIndex = slotIndex;
            SeatIndex = seatIndex;
            Participant = participant;
            Pawn = pawn;
            InputProfile = inputProfile;
            RequiresPawn = requiresPawn;
            InputIssue = inputIssue ?? string.Empty;
            PawnIssue = pawnIssue ?? string.Empty;
            PawnIssueKind = pawnIssueKind;
        }

        public int SlotIndex { get; }
        public int SeatIndex { get; }
        public ParticipantDefinition Participant { get; }
        public PawnDefinition Pawn { get; }
        public InputProfile InputProfile { get; }
        public bool RequiresPawn { get; }
        public string InputIssue { get; }
        public string PawnIssue { get; }
        public PyralisParticipantPawnIssueKind PawnIssueKind { get; }
        public bool HasParticipant => Participant != null;
        public bool HasInputProfile => InputProfile != null;
        public bool HasPawnDefinition => Pawn != null;
        public bool HasPawnPrefab => Pawn != null && Pawn.pawnPrefab != null;
        public bool IsInputReady => !RequiresPawn || HasInputProfile && string.IsNullOrWhiteSpace(InputIssue);
        public bool IsPawnReady => !RequiresPawn || string.IsNullOrWhiteSpace(PawnIssue);
        public bool IsReady => HasParticipant && IsInputReady && IsPawnReady;
        public string StableIdSuffix => SlotIndex >= 0 ? SlotIndex.ToString() : "standalone";
        public string DisplayName
        {
            get
            {
                if (Participant != null && !string.IsNullOrWhiteSpace(Participant.displayName))
                    return Participant.displayName;

                return SlotIndex >= 0 ? $"Participant Slot {SlotIndex}" : "Participant";
            }
        }
    }

    public sealed class PyralisSetupRouteAnalysis
    {
        private PyralisSetupRouteAnalysis(
            GameplaySessionBootstrap bootstrap,
            SessionDefinition session,
            GameModeDefinition mode,
            ParticipantDefinition participant,
            PawnDefinition pawn,
            RuntimeCapabilityFamily[] capabilityFamilies,
            bool requiresPawn,
            bool hasParticipants,
            bool hasAnyDefaultPawn,
            string participantPawnIssue,
            PyralisParticipantPawnIssueKind participantPawnIssueKind,
            PyralisAuthoringRouteFact[] routeFacts,
            PyralisParticipantTopology participantTopology,
            PyralisParticipantJoinPolicy expectedJoinPolicy,
            PyralisParticipantSpawnPolicy spawnPolicy,
            int assignedParticipantCount,
            int authoredParticipantCount,
            int desiredParticipantCount,
            int autoJoinParticipantCount,
            bool hasParticipantInputRouter,
            bool autoRegisterDefaultsWithoutPlayerInput,
            bool hasPlayerInputManager,
            bool spawnOnRegister,
            PyralisParticipantSeatReadiness[] participantSeats,
            string playerInputManagerIssue)
        {
            Bootstrap = bootstrap;
            Session = session;
            Mode = mode;
            Participant = participant;
            Pawn = pawn;
            CapabilityFamilies = capabilityFamilies ?? System.Array.Empty<RuntimeCapabilityFamily>();
            HasSelectedCapabilities = CapabilityFamilies.Length > 0;
            RequiresPawn = requiresPawn;
            HasParticipants = hasParticipants;
            HasAnyDefaultPawn = hasAnyDefaultPawn;
            ParticipantPawnIssue = participantPawnIssue;
            ParticipantPawnIssueKind = participantPawnIssueKind;
            RouteFacts = routeFacts ?? System.Array.Empty<PyralisAuthoringRouteFact>();
            ParticipantTopology = participantTopology;
            ExpectedJoinPolicy = expectedJoinPolicy;
            SpawnPolicy = spawnPolicy;
            AssignedParticipantCount = assignedParticipantCount;
            AuthoredParticipantCount = authoredParticipantCount;
            DesiredParticipantCount = desiredParticipantCount;
            AutoJoinParticipantCount = autoJoinParticipantCount;
            HasParticipantInputRouter = hasParticipantInputRouter;
            AutoRegisterDefaultsWithoutPlayerInput = autoRegisterDefaultsWithoutPlayerInput;
            HasPlayerInputManager = hasPlayerInputManager;
            SpawnOnRegister = spawnOnRegister;
            ParticipantSeats = participantSeats ?? System.Array.Empty<PyralisParticipantSeatReadiness>();
            PlayerInputManagerIssue = playerInputManagerIssue ?? string.Empty;
        }

        public GameplaySessionBootstrap Bootstrap { get; }
        public SessionDefinition Session { get; }
        public GameModeDefinition Mode { get; }
        public ParticipantDefinition Participant { get; }
        public PawnDefinition Pawn { get; }
        public RuntimeCapabilityFamily[] CapabilityFamilies { get; }
        public bool HasSelectedCapabilities { get; }
        public bool RequiresPawn { get; }
        public bool HasParticipants { get; }
        public bool HasAnyDefaultPawn { get; }
        public string ParticipantPawnIssue { get; }
        public PyralisParticipantPawnIssueKind ParticipantPawnIssueKind { get; }
        public PyralisAuthoringRouteFact[] RouteFacts { get; }
        public PyralisParticipantTopology ParticipantTopology { get; }
        public PyralisParticipantJoinPolicy ExpectedJoinPolicy { get; }
        public PyralisParticipantSpawnPolicy SpawnPolicy { get; }
        public int AssignedParticipantCount { get; }
        public int AuthoredParticipantCount { get; }
        public int DesiredParticipantCount { get; }
        public int AutoJoinParticipantCount { get; }
        public bool HasParticipantInputRouter { get; }
        public bool AutoRegisterDefaultsWithoutPlayerInput { get; }
        public bool HasPlayerInputManager { get; }
        public bool SpawnOnRegister { get; }
        public PyralisParticipantSeatReadiness[] ParticipantSeats { get; }
        public string PlayerInputManagerIssue { get; }
        public PyralisAuthoringRouteFact PrimaryRouteFact => RouteFacts.Length > 0 ? RouteFacts[0] : null;

        public string RouteName
        {
            get
            {
                if (!HasSelectedCapabilities)
                    return "No setup route selected";

                if (RequiresPawn && ParticipantTopology == PyralisParticipantTopology.LocalJoin)
                    return "Local Co-op Pawn route";
                if (RequiresPawn && ParticipantTopology == PyralisParticipantTopology.HybridLocalNetworked)
                    return "Hybrid Local/Network Pawn route";
                if (RequiresPawn && ParticipantTopology == PyralisParticipantTopology.Networked)
                    return "Networked Pawn route";
                if (RequiresPawn && ParticipantTopology == PyralisParticipantTopology.SoloLocal)
                    return "1P Pawn route";

                if (RouteFacts.Length == 0)
                    return RequiresPawn ? "Pawn-backed route" : "No-pawn-capable route";

                PyralisAuthoringRouteFact primary = FindRouteNameFact(RouteFacts);
                return RouteFacts.Length == 1
                    ? $"{primary.Label} route"
                    : $"{primary.Label} + {RouteFacts.Length - 1} capability route";
            }
        }

        public static PyralisSetupRouteAnalysis Build(GameplaySessionBootstrap bootstrap)
        {
            PyralisSetupDependencyTree dependencyTree = PyralisSetupDependencyTree.Build(bootstrap);
            return BuildResolved(dependencyTree);
        }

        public static PyralisSetupRouteAnalysis Build(SessionDefinition session)
        {
            PyralisSetupDependencyTree dependencyTree = PyralisSetupDependencyTree.Build(session);
            return BuildResolved(dependencyTree);
        }

        public static PyralisSetupRouteAnalysis Build(GameModeDefinition mode, SessionDefinition session = null)
        {
            PyralisSetupDependencyTree dependencyTree = PyralisSetupDependencyTree.Build(session != null ? session : mode);
            GameModeDefinition assignedMode = session != null && session.defaultGameMode != mode
                ? null
                : mode;
            return BuildResolved(dependencyTree, assignedMode);
        }

        public static PyralisSetupRouteAnalysis Build(UnityEngine.Object source)
        {
            if (source is GameplaySessionBootstrap bootstrap)
                return Build(bootstrap);
            if (source is SessionDefinition session)
                return Build(session);
            if (source is GameModeDefinition mode)
                return Build(mode);

            PyralisSetupDependencyTree dependencyTree = PyralisSetupDependencyTree.Build(source);
            return BuildResolved(dependencyTree);
        }

        public static PyralisSetupRouteAnalysis Build(
            UnityEngine.Object source,
            RuntimeCapabilityFamily[] focusedCapabilityFamilies)
        {
            return WithAdditionalCapabilityFamilies(Build(source), focusedCapabilityFamilies);
        }

        public static PyralisSetupRouteAnalysis WithAdditionalCapabilityFamilies(
            PyralisSetupRouteAnalysis route,
            RuntimeCapabilityFamily[] additionalFamilies)
        {
            return WithIntentFocus(route, additionalFamilies, null);
        }

        public static PyralisSetupRouteAnalysis WithIntentFocus(
            PyralisSetupRouteAnalysis route,
            RuntimeCapabilityFamily[] additionalFamilies,
            PyralisAuthoringIntentSelection intentSelection)
        {
            if (route == null)
                route = BuildResolved(null, null);

            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            for (int i = 0; i < route.CapabilityFamilies.Length; i++)
                AddFamily(families, route.CapabilityFamilies[i]);
            if (additionalFamilies != null)
            {
                for (int i = 0; i < additionalFamilies.Length; i++)
                    AddFamily(families, additionalFamilies[i]);
            }

            if (intentSelection == null
                && families.Count == route.CapabilityFamilies.Length)
            {
                return route;
            }

            RuntimeCapabilityFamily[] mergedFamilies = families.ToArray();
            bool mergedRequiresPawn = ContainsFamily(mergedFamilies, RuntimeCapabilityFamily.CharacterPawnGameplay);
            int desiredParticipantCount = GetIntentParticipantCount(intentSelection);
            PyralisParticipantTopology desiredTopology = GetIntentParticipantTopology(intentSelection, route.Session?.networkMode ?? GameplayNetworkMode.LocalOnly);
            int authoredParticipantCount = route.AuthoredParticipantCount;
            int assignedParticipantCount = System.Math.Max(authoredParticipantCount, desiredParticipantCount);
            bool hasParticipants = route.HasParticipants;
            PyralisParticipantTopology topology = desiredTopology != PyralisParticipantTopology.Unknown
                ? desiredTopology
                : InferParticipantTopology(
                    route.Session,
                    mergedRequiresPawn,
                    assignedParticipantCount,
                    hasParticipants);
            PyralisParticipantJoinPolicy joinPolicy = InferExpectedJoinPolicy(
                topology,
                route.AutoRegisterDefaultsWithoutPlayerInput);
            PyralisParticipantSpawnPolicy spawnPolicy = InferSpawnPolicy(
                mergedRequiresPawn,
                route.SpawnOnRegister);
            PyralisParticipantSeatReadiness[] participantSeats = BuildParticipantSeatReadiness(
                route.Session,
                null,
                route.Participant,
                route.Pawn,
                mergedRequiresPawn,
                assignedParticipantCount);
            string playerInputManagerIssue = GetPlayerInputManagerIssue(route.Bootstrap, topology);
            return new PyralisSetupRouteAnalysis(
                route.Bootstrap,
                route.Session,
                route.Mode,
                route.Participant,
                route.Pawn,
                mergedFamilies,
                mergedRequiresPawn,
                hasParticipants,
                route.HasAnyDefaultPawn,
                route.ParticipantPawnIssue,
                route.ParticipantPawnIssueKind,
                BuildRouteFacts(mergedFamilies, intentSelection),
                topology,
                joinPolicy,
                spawnPolicy,
                assignedParticipantCount,
                authoredParticipantCount,
                desiredParticipantCount,
                route.AutoJoinParticipantCount,
                route.HasParticipantInputRouter,
                route.AutoRegisterDefaultsWithoutPlayerInput,
                route.HasPlayerInputManager,
                route.SpawnOnRegister,
                participantSeats,
                playerInputManagerIssue);
        }

        private static PyralisSetupRouteAnalysis BuildResolved(
            PyralisSetupDependencyTree dependencyTree,
            GameModeDefinition modeOverride = null)
        {
            SessionDefinition session = dependencyTree?.Session;
            GameplaySessionBootstrap bootstrap = dependencyTree?.Bootstrap;
            GameModeDefinition mode = modeOverride != null ? modeOverride : dependencyTree?.Mode;
            ParticipantDefinition participant = dependencyTree?.FirstParticipant;
            PawnDefinition pawn = dependencyTree?.FirstPawn;
            RuntimeCapabilityFamily[] capabilityFamilies = CollectCapabilityFamilies(
                session,
                mode,
                dependencyTree?.Participants,
                pawn);
            bool requiresPawn = ContainsFamily(capabilityFamilies, RuntimeCapabilityFamily.CharacterPawnGameplay);
            bool hasParticipants = CheckHasParticipants(session, dependencyTree?.Participants, participant);
            int assignedParticipantCount = session != null ? CountAssignedParticipants(session) : hasParticipants ? 1 : 0;
            int authoredParticipantCount = assignedParticipantCount;
            int autoJoinParticipantCount = CountAutoJoinParticipants(session);
            bool hasAnyDefaultPawn = CheckHasAnyDefaultPawn(session, dependencyTree?.Participants, pawn);
            string participantPawnIssue = GetParticipantPawnIssue(
                session,
                dependencyTree?.Participants,
                participant,
                pawn,
                out PyralisParticipantPawnIssueKind participantPawnIssueKind);
            PyralisAuthoringRouteFact[] routeFacts = BuildRouteFacts(capabilityFamilies);
            bool hasPlayerInputManager = ResolveHasPlayerInputManager(bootstrap);
            bool hasParticipantInputRouter = ResolveHasParticipantInputRouter(bootstrap);
            bool autoRegisterDefaultsWithoutPlayerInput = GetAutoRegisterDefaultsWithoutPlayerInput(bootstrap);
            bool spawnOnRegister = GetSpawnOnRegister(bootstrap);
            PyralisParticipantTopology topology = InferParticipantTopology(
                session,
                requiresPawn,
                assignedParticipantCount,
                hasParticipants);
            PyralisParticipantJoinPolicy joinPolicy = InferExpectedJoinPolicy(topology, autoRegisterDefaultsWithoutPlayerInput);
            PyralisParticipantSpawnPolicy spawnPolicy = InferSpawnPolicy(requiresPawn, spawnOnRegister);
            PyralisParticipantSeatReadiness[] participantSeats = BuildParticipantSeatReadiness(
                session,
                dependencyTree?.Participants,
                participant,
                pawn,
                requiresPawn,
                assignedParticipantCount);
            string playerInputManagerIssue = GetPlayerInputManagerIssue(bootstrap, topology);

            return new PyralisSetupRouteAnalysis(
                bootstrap,
                session,
                mode,
                participant,
                pawn,
                capabilityFamilies,
                requiresPawn,
                hasParticipants,
                hasAnyDefaultPawn,
                participantPawnIssue,
                participantPawnIssueKind,
                routeFacts,
                topology,
                joinPolicy,
                spawnPolicy,
                assignedParticipantCount,
                authoredParticipantCount,
                0,
                autoJoinParticipantCount,
                hasParticipantInputRouter,
                autoRegisterDefaultsWithoutPlayerInput,
                hasPlayerInputManager,
                spawnOnRegister,
                participantSeats,
                playerInputManagerIssue);
        }

        public bool UsesCamera()
        {
            return HasFamily(RuntimeCapabilityFamily.CameraInput);
        }

        public bool LikelyUsesInputManager()
        {
            return ParticipantTopology == PyralisParticipantTopology.LocalJoin;
        }

        public int LocalParticipantCount()
        {
            return AssignedParticipantCount;
        }

        public bool HasLocalJoinPolicyConflict()
        {
            return ParticipantTopology == PyralisParticipantTopology.LocalJoin
                && AutoRegisterDefaultsWithoutPlayerInput
                && AutoJoinParticipantCount > 0;
        }

        public bool HasSoloLocalJoinPolicyGap()
        {
            return ParticipantTopology == PyralisParticipantTopology.SoloLocal
                && ExpectedJoinPolicy == PyralisParticipantJoinPolicy.AutoRegisterDefaults
                && !AutoRegisterDefaultsWithoutPlayerInput
                && !HasPlayerInputManager
                && AutoJoinParticipantCount > 0;
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

        private static RuntimeCapabilityFamily[] CollectCapabilityFamilies(
            SessionDefinition session,
            GameModeDefinition mode,
            IReadOnlyList<ParticipantDefinition> reflectedParticipants,
            PawnDefinition standalonePawn)
        {
            List<RuntimeCapabilityFamily> families = new List<RuntimeCapabilityFamily>();
            AddFamiliesFromMode(families, mode);
            AddFamiliesFromParticipants(families, session);
            AddFamiliesFromReflectedParticipants(families, reflectedParticipants);
            AddFamiliesFromPawn(families, standalonePawn);

            return families.ToArray();
        }

        private static void AddFamiliesFromMode(List<RuntimeCapabilityFamily> families, GameModeDefinition mode)
        {
            if (mode == null)
                return;

            AddFamily(families, RuntimeCapabilityFamily.PlatformCore);

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

            if (session.defaultParticipants == null)
                return;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant == null)
                    continue;

                AddFamiliesFromPawn(families, participant.defaultPawn);
            }
        }

        private static void AddFamiliesFromReflectedParticipants(
            List<RuntimeCapabilityFamily> families,
            IReadOnlyList<ParticipantDefinition> participants)
        {
            if (participants == null)
                return;

            for (int i = 0; i < participants.Count; i++)
            {
                ParticipantDefinition participant = participants[i];
                if (participant == null)
                    continue;

                AddFamiliesFromPawn(families, participant.defaultPawn);
            }
        }

        private static void AddFamiliesFromPawn(List<RuntimeCapabilityFamily> families, PawnDefinition pawn)
        {
            if (pawn == null)
                return;

            AddFamily(families, RuntimeCapabilityFamily.CharacterPawnGameplay);

            if (pawn.combatProfile != null)
                AddFamily(families, RuntimeCapabilityFamily.Combat);
            if (pawn.presentationProfile != null || pawn.animationProfile != null)
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

        private static int CountAssignedParticipants(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return 0;

            int count = 0;
            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                if (session.defaultParticipants[i] != null)
                    count++;
            }

            return count;
        }

        private static int CountAutoJoinParticipants(SessionDefinition session)
        {
            if (session == null || session.defaultParticipants == null)
                return 0;

            int count = 0;
            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.autoJoin)
                    count++;
            }

            return count;
        }

        private static PyralisParticipantTopology InferParticipantTopology(
            SessionDefinition session,
            bool requiresPawn,
            int assignedParticipantCount,
            bool hasParticipants)
        {
            if (!hasParticipants)
                return PyralisParticipantTopology.NoParticipants;

            if (session == null)
                return assignedParticipantCount > 1
                    ? PyralisParticipantTopology.LocalJoin
                    : PyralisParticipantTopology.SoloLocal;

            if (session.networkMode != GameplayNetworkMode.LocalOnly)
            {
                return assignedParticipantCount > 1
                    ? PyralisParticipantTopology.HybridLocalNetworked
                    : PyralisParticipantTopology.Networked;
            }

            if (assignedParticipantCount > 1)
                return PyralisParticipantTopology.LocalJoin;

            return PyralisParticipantTopology.SoloLocal;
        }

        private static int GetIntentParticipantCount(PyralisAuthoringIntentSelection selection)
        {
            if (selection == null)
                return 0;

            switch (selection.ParticipantRoute)
            {
                case PyralisIntentParticipantRoute.SoloLocal:
                case PyralisIntentParticipantRoute.Networked:
                    return 1;
                case PyralisIntentParticipantRoute.TwoLocalPlayers:
                case PyralisIntentParticipantRoute.HybridLocalNetworked:
                    return 2;
                case PyralisIntentParticipantRoute.ThreeLocalPlayers:
                    return 3;
                case PyralisIntentParticipantRoute.FourLocalPlayers:
                    return 4;
                default:
                    return 0;
            }
        }

        private static PyralisParticipantTopology GetIntentParticipantTopology(
            PyralisAuthoringIntentSelection selection,
            GameplayNetworkMode sessionNetworkMode)
        {
            if (selection == null)
                return PyralisParticipantTopology.Unknown;

            switch (selection.ParticipantRoute)
            {
                case PyralisIntentParticipantRoute.SoloLocal:
                    return sessionNetworkMode == GameplayNetworkMode.LocalOnly
                        ? PyralisParticipantTopology.SoloLocal
                        : PyralisParticipantTopology.Networked;
                case PyralisIntentParticipantRoute.TwoLocalPlayers:
                case PyralisIntentParticipantRoute.ThreeLocalPlayers:
                case PyralisIntentParticipantRoute.FourLocalPlayers:
                    return sessionNetworkMode == GameplayNetworkMode.LocalOnly
                        ? PyralisParticipantTopology.LocalJoin
                        : PyralisParticipantTopology.HybridLocalNetworked;
                case PyralisIntentParticipantRoute.Networked:
                    return PyralisParticipantTopology.Networked;
                case PyralisIntentParticipantRoute.HybridLocalNetworked:
                    return PyralisParticipantTopology.HybridLocalNetworked;
                default:
                    return PyralisParticipantTopology.Unknown;
            }
        }

        private static PyralisParticipantJoinPolicy InferExpectedJoinPolicy(
            PyralisParticipantTopology topology,
            bool autoRegisterDefaultsWithoutPlayerInput)
        {
            switch (topology)
            {
                case PyralisParticipantTopology.NoParticipants:
                    return PyralisParticipantJoinPolicy.NoParticipants;
                case PyralisParticipantTopology.LocalJoin:
                    return PyralisParticipantJoinPolicy.PlayerInputJoin;
                case PyralisParticipantTopology.Networked:
                    return PyralisParticipantJoinPolicy.NetworkAuthority;
                case PyralisParticipantTopology.HybridLocalNetworked:
                    return PyralisParticipantJoinPolicy.HybridPlayerInputAndNetwork;
                case PyralisParticipantTopology.SoloLocal:
                    return PyralisParticipantJoinPolicy.AutoRegisterDefaults;
                default:
                    return PyralisParticipantJoinPolicy.Unknown;
            }
        }

        private static PyralisParticipantSpawnPolicy InferSpawnPolicy(bool requiresPawn, bool spawnOnRegister)
        {
            if (!requiresPawn)
                return PyralisParticipantSpawnPolicy.NoPawnSpawn;

            return spawnOnRegister
                ? PyralisParticipantSpawnPolicy.SpawnOnRegister
                : PyralisParticipantSpawnPolicy.ManualSpawn;
        }

        private static bool ResolveHasPlayerInputManager(GameplaySessionBootstrap bootstrap)
        {
            return GetObjectReference<PlayerInputManager>(bootstrap, "playerInputManager") != null;
        }

        private static bool ResolveHasParticipantInputRouter(GameplaySessionBootstrap bootstrap)
        {
            return bootstrap != null && bootstrap.GetComponentInChildren<ParticipantInputRouter>(true) != null;
        }

        private static bool GetAutoRegisterDefaultsWithoutPlayerInput(GameplaySessionBootstrap bootstrap)
        {
            ParticipantInputRouter router = bootstrap != null
                ? bootstrap.GetComponentInChildren<ParticipantInputRouter>(true)
                : null;
            if (router == null)
                return false;

            return GetBool(router, "autoRegisterDefaultParticipantsWithoutPlayerInput");
        }

        private static bool GetSpawnOnRegister(GameplaySessionBootstrap bootstrap)
        {
            ParticipantSpawnService spawnService = bootstrap != null
                ? bootstrap.GetComponentInChildren<ParticipantSpawnService>(true)
                : null;
            if (spawnService == null)
                return true;

            return GetBool(spawnService, "spawnOnRegister", true);
        }

        private static bool GetBool(UnityEngine.Object source, string propertyPath, bool fallback = false)
        {
            if (source == null)
                return fallback;

            SerializedObject serialized = new SerializedObject(source);
            SerializedProperty property = serialized.FindProperty(propertyPath);
            return property != null ? property.boolValue : fallback;
        }

        private static T GetObjectReference<T>(UnityEngine.Object source, string propertyPath)
            where T : UnityEngine.Object
        {
            if (source == null || string.IsNullOrWhiteSpace(propertyPath))
                return null;

            SerializedObject serialized = new SerializedObject(source);
            SerializedProperty property = serialized.FindProperty(propertyPath);
            return property != null ? property.objectReferenceValue as T : null;
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

        private static PyralisAuthoringRouteFact[] BuildRouteFacts(
            RuntimeCapabilityFamily[] families,
            PyralisAuthoringIntentSelection intentSelection = null)
        {
            if (families == null)
                return System.Array.Empty<PyralisAuthoringRouteFact>();

            List<PyralisAuthoringRouteFact> facts = new List<PyralisAuthoringRouteFact>();
            for (int i = 0; i < families.Length; i++)
                AddFact(facts, families[i], intentSelection);

            return facts.ToArray();
        }

        private static void AddFact(
            List<PyralisAuthoringRouteFact> facts,
            RuntimeCapabilityFamily family,
            PyralisAuthoringIntentSelection intentSelection)
        {
            PyralisAuthoringCapabilityDescriptor descriptor =
                FindIntentSelectedDescriptorForFamily(family, intentSelection)
                ?? PyralisAuthoringCapabilityDescriptorRegistry.FindPrimaryByFamily(family);
            if (TryGetRouteCapability(family, out PyralisAuthoringRouteCapability capability))
            {
                string label = descriptor != null && !string.IsNullOrWhiteSpace(descriptor.DisplayName)
                    ? descriptor.DisplayName
                    : family.ToString();
                bool primaryProofCandidate = descriptor != null && !string.IsNullOrWhiteSpace(descriptor.ProofTargetId);
                facts.Add(new PyralisAuthoringRouteFact(capability, label, family, primaryProofCandidate));
            }
        }

        private static PyralisAuthoringCapabilityDescriptor FindIntentSelectedDescriptorForFamily(
            RuntimeCapabilityFamily family,
            PyralisAuthoringIntentSelection intentSelection)
        {
            if (intentSelection?.DescriptorIds == null || intentSelection.DescriptorIds.Length == 0)
                return null;

            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.All;
            for (int selectedIndex = 0; selectedIndex < intentSelection.DescriptorIds.Length; selectedIndex++)
            {
                string selectedId = intentSelection.DescriptorIds[selectedIndex];
                if (string.IsNullOrWhiteSpace(selectedId))
                    continue;

                for (int descriptorIndex = 0; descriptorIndex < descriptors.Count; descriptorIndex++)
                {
                    PyralisAuthoringCapabilityDescriptor descriptor = descriptors[descriptorIndex];
                    if (descriptor != null
                        && descriptor.Family == family
                        && descriptor.IsContractSemanticSource
                        && PyralisAuthoringCapabilityDescriptorRegistry.IsGameplayIngredientDescriptor(descriptor)
                        && string.Equals(descriptor.StableId, selectedId, System.StringComparison.Ordinal))
                    {
                        return descriptor;
                    }
                }
            }

            return null;
        }

        private static PyralisAuthoringRouteFact FindRouteNameFact(PyralisAuthoringRouteFact[] facts)
        {
            if (facts == null || facts.Length == 0)
                return null;

            for (int i = 0; i < facts.Length; i++)
            {
                if (facts[i] != null
                    && facts[i].Family != RuntimeCapabilityFamily.PlatformCore)
                {
                    return facts[i];
                }
            }

            return facts[0];
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

        private static bool CheckHasParticipants(
            SessionDefinition session,
            IReadOnlyList<ParticipantDefinition> reflectedParticipants,
            ParticipantDefinition standaloneParticipant)
        {
            if (session == null)
                return standaloneParticipant != null || HasAnyParticipant(reflectedParticipants);

            if (session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                if (session.defaultParticipants[i] == null)
                    return false;
            }

            return true;
        }

        private static bool CheckHasAnyDefaultPawn(
            SessionDefinition session,
            IReadOnlyList<ParticipantDefinition> reflectedParticipants,
            PawnDefinition standalonePawn)
        {
            if (session == null)
            {
                if (standalonePawn != null)
                    return true;

                return HasAnyReflectedDefaultPawn(reflectedParticipants);
            }

            if (session.defaultParticipants == null || session.defaultParticipants.Length == 0)
                return false;

            for (int i = 0; i < session.defaultParticipants.Length; i++)
            {
                ParticipantDefinition participant = session.defaultParticipants[i];
                if (participant != null && participant.defaultPawn != null)
                    return true;
            }

            return false;
        }

        private static string GetParticipantPawnIssue(
            SessionDefinition session,
            IReadOnlyList<ParticipantDefinition> reflectedParticipants,
            ParticipantDefinition standaloneParticipant,
            PawnDefinition standalonePawn,
            out PyralisParticipantPawnIssueKind issueKind)
        {
            if (session == null)
            {
                if (standaloneParticipant != null)
                    return GetParticipantPawnIssue(new[] { standaloneParticipant }, out issueKind);

                if (HasAnyParticipant(reflectedParticipants))
                    return GetParticipantPawnIssue(reflectedParticipants, out issueKind);

                if (standalonePawn != null)
                {
                    string pawnIssue = GetPawnIssue(standalonePawn, out issueKind);
                    return string.IsNullOrWhiteSpace(pawnIssue)
                        ? "Assign this PawnDefinition to ParticipantDefinition.defaultPawn so a participant can spawn it."
                        : pawnIssue;
                }

                issueKind = PyralisParticipantPawnIssueKind.MissingParticipants;
                return "Assign default participants before checking pawn readiness.";
            }

            if (session.defaultParticipants == null || session.defaultParticipants.Length == 0)
            {
                issueKind = PyralisParticipantPawnIssueKind.MissingParticipants;
                if (standaloneParticipant != null)
                    return $"Assign ParticipantDefinition `{standaloneParticipant.name}` to SessionDefinition.defaultParticipants before Play Mode can spawn a pawn.";

                if (HasAnyParticipant(reflectedParticipants))
                    return "Assign the reflected ParticipantDefinition to SessionDefinition.defaultParticipants before Play Mode can spawn a pawn.";

                return "Assign default participants before checking pawn readiness.";
            }

            return GetParticipantPawnIssue(session.defaultParticipants, out issueKind);
        }

        private static PyralisParticipantSeatReadiness[] BuildParticipantSeatReadiness(
            SessionDefinition session,
            IReadOnlyList<ParticipantDefinition> reflectedParticipants,
            ParticipantDefinition standaloneParticipant,
            PawnDefinition standalonePawn,
            bool requiresPawn,
            int desiredSeatCount = 0)
        {
            List<PyralisParticipantSeatReadiness> seats = new List<PyralisParticipantSeatReadiness>();

            if (session != null && session.defaultParticipants != null && (session.defaultParticipants.Length > 0 || desiredSeatCount > 0))
            {
                int seatCount = System.Math.Max(session.defaultParticipants.Length, desiredSeatCount);
                for (int i = 0; i < seatCount; i++)
                {
                    ParticipantDefinition participant = i < session.defaultParticipants.Length
                        ? session.defaultParticipants[i]
                        : null;
                    seats.Add(BuildParticipantSeatReadiness(i, participant, requiresPawn));
                }

                return seats.ToArray();
            }

            if (standaloneParticipant != null)
            {
                seats.Add(BuildParticipantSeatReadiness(0, standaloneParticipant, requiresPawn));
                return seats.ToArray();
            }

            if (HasAnyParticipant(reflectedParticipants))
            {
                for (int i = 0; i < reflectedParticipants.Count; i++)
                {
                    if (reflectedParticipants[i] != null)
                        seats.Add(BuildParticipantSeatReadiness(i, reflectedParticipants[i], requiresPawn));
                }

                return seats.ToArray();
            }

            if (standalonePawn != null)
            {
                string pawnIssue = GetPawnIssue(standalonePawn, out PyralisParticipantPawnIssueKind pawnIssueKind);
                seats.Add(new PyralisParticipantSeatReadiness(
                    0,
                    0,
                    null,
                    standalonePawn,
                    null,
                    requiresPawn,
                    requiresPawn ? "Assign a ParticipantDefinition.inputProfile once this PawnDefinition is attached to a participant." : string.Empty,
                    string.IsNullOrWhiteSpace(pawnIssue)
                        ? "Assign this PawnDefinition to ParticipantDefinition.defaultPawn so a participant can spawn it."
                        : pawnIssue,
                    string.IsNullOrWhiteSpace(pawnIssue)
                        ? PyralisParticipantPawnIssueKind.MissingParticipants
                        : pawnIssueKind));
            }

            if (seats.Count == 0 && desiredSeatCount > 0)
            {
                for (int i = 0; i < desiredSeatCount; i++)
                    seats.Add(BuildParticipantSeatReadiness(i, null, requiresPawn));
            }

            return seats.ToArray();
        }

        private static PyralisParticipantSeatReadiness BuildParticipantSeatReadiness(
            int slotIndex,
            ParticipantDefinition participant,
            bool requiresPawn)
        {
            if (participant == null)
            {
                return new PyralisParticipantSeatReadiness(
                    slotIndex,
                    slotIndex,
                    null,
                    null,
                    null,
                    requiresPawn,
                    requiresPawn ? $"Participant slot {slotIndex} is empty, so no InputProfile can be resolved." : string.Empty,
                    $"Default participant slot {slotIndex} is empty.",
                    PyralisParticipantPawnIssueKind.EmptyParticipantSlot);
            }

            PawnDefinition pawn = participant.defaultPawn;
            InputProfile inputProfile = ParticipantInputProfileUtility.ResolveEffectiveInputProfile(participant);
            string inputIssue = requiresPawn && inputProfile == null
                ? $"Participant `{GetParticipantDisplayName(participant, slotIndex)}` needs ParticipantDefinition.inputProfile so its joined PlayerInput can drive only this participant's pawn."
                : string.Empty;
            PyralisParticipantPawnIssueKind pawnIssueKind = PyralisParticipantPawnIssueKind.None;
            string pawnIssue = requiresPawn
                ? GetPawnIssue(pawn, out pawnIssueKind)
                : string.Empty;

            return new PyralisParticipantSeatReadiness(
                slotIndex,
                participant.preferredSeatIndex >= 0 ? participant.preferredSeatIndex : slotIndex,
                participant,
                pawn,
                inputProfile,
                requiresPawn,
                inputIssue,
                pawnIssue,
                requiresPawn ? pawnIssueKind : PyralisParticipantPawnIssueKind.None);
        }

        private static string GetParticipantDisplayName(ParticipantDefinition participant, int slotIndex)
        {
            if (participant != null && !string.IsNullOrWhiteSpace(participant.displayName))
                return participant.displayName;

            return $"Participant Slot {slotIndex}";
        }

        private static string GetParticipantPawnIssue(
            IReadOnlyList<ParticipantDefinition> participants,
            out PyralisParticipantPawnIssueKind issueKind)
        {
            if (participants == null || participants.Count == 0)
            {
                issueKind = PyralisParticipantPawnIssueKind.MissingParticipants;
                return "Assign default participants before checking pawn readiness.";
            }

            for (int i = 0; i < participants.Count; i++)
            {
                ParticipantDefinition participant = participants[i];
                if (participant == null)
                {
                    issueKind = PyralisParticipantPawnIssueKind.EmptyParticipantSlot;
                    return $"Default participant slot {i} is empty.";
                }

                if (participant.defaultPawn == null)
                {
                    issueKind = PyralisParticipantPawnIssueKind.MissingPawnDefinition;
                    return $"Selected pawn-backed intent asks participant `{participant.displayName}` to use a PawnDefinition. Assign it in ParticipantDefinition.defaultPawn before participants can spawn.";
                }

                string pawnIssue = GetPawnIssue(participant.defaultPawn, out issueKind);
                if (!string.IsNullOrWhiteSpace(pawnIssue))
                    return pawnIssue;
            }

            issueKind = PyralisParticipantPawnIssueKind.None;
            return null;
        }

        private static string GetPawnIssue(PawnDefinition pawn, out PyralisParticipantPawnIssueKind issueKind)
        {
            if (pawn == null)
            {
                issueKind = PyralisParticipantPawnIssueKind.MissingPawnDefinition;
                return "Selected pawn-backed intent needs a PawnDefinition before participants can spawn.";
            }

            if (pawn.pawnPrefab == null)
            {
                issueKind = PyralisParticipantPawnIssueKind.MissingPawnPrefab;
                return $"Selected pawn-backed intent asks PawnDefinition `{pawn.name}` to point at a pawn prefab before participants can spawn.";
            }

            PyralisRuntimeValidationIssue pawnIssue = GetFirstRequiredPawnIssue(pawn);
            if (pawnIssue != null)
            {
                issueKind = ClassifyPawnValidationIssueCode(pawnIssue.IssueCode);
                return $"PawnDefinition `{pawn.name}`: {pawnIssue.Message}";
            }

            issueKind = PyralisParticipantPawnIssueKind.None;
            return null;
        }

        private static PyralisRuntimeValidationIssue GetFirstRequiredPawnIssue(PawnDefinition pawn)
        {
            if (pawn == null)
                return null;

            foreach (PyralisRuntimeValidationIssue issue in pawn.GetRuntimeValidationIssues())
            {
                if (issue != null && issue.Severity == PyralisRuntimeValidationSeverity.Required)
                    return issue;
            }

            return null;
        }

        private static PyralisParticipantPawnIssueKind ClassifyPawnValidationIssueCode(string issueCode)
        {
            if (string.IsNullOrWhiteSpace(issueCode))
                return PyralisParticipantPawnIssueKind.PawnValidation;

            if (issueCode.Contains(".PawnRoot.", System.StringComparison.Ordinal))
                return PyralisParticipantPawnIssueKind.MissingPawnRoot;
            if (issueCode.Contains(".PawnMotor.", System.StringComparison.Ordinal))
                return PyralisParticipantPawnIssueKind.MissingMotor;
            if (issueCode.Contains(".PawnInput.", System.StringComparison.Ordinal))
                return PyralisParticipantPawnIssueKind.MissingInputModule;
            if (issueCode.Contains(".PawnPresentation.", System.StringComparison.Ordinal))
                return PyralisParticipantPawnIssueKind.MissingPresentation;

            return PyralisParticipantPawnIssueKind.PawnValidation;
        }

        private static string GetPlayerInputManagerIssue(
            GameplaySessionBootstrap bootstrap,
            PyralisParticipantTopology topology)
        {
            if (topology != PyralisParticipantTopology.LocalJoin
                && topology != PyralisParticipantTopology.HybridLocalNetworked)
            {
                return string.Empty;
            }

            PlayerInputManager playerInputManager = GetObjectReference<PlayerInputManager>(bootstrap, "playerInputManager");
            if (playerInputManager == null)
                return "Local join routes need GameplaySessionBootstrap.playerInputManager assigned so Unity can pair each controller with one participant.";

            if (playerInputManager.playerPrefab == null)
                return "PlayerInputManager.playerPrefab is empty. Assign the joined pawn prefab shape that contains PlayerInput and PawnRoot/IPawnParticipantInitializer.";

            GameObject playerPrefab = playerInputManager.playerPrefab;
            if (playerPrefab.GetComponent<PlayerInput>() == null)
                return $"PlayerInputManager.playerPrefab `{playerPrefab.name}` needs a PlayerInput component so Unity can pair a device with this joined participant.";

            if (!PrefabHasComponent<IPawnParticipantInitializer>(playerPrefab))
                return $"PlayerInputManager.playerPrefab `{playerPrefab.name}` must contain PawnRoot/IPawnParticipantInitializer. Otherwise Unity joins an input object while ParticipantSpawnService instantiates a separate pawn, which can make one action asset drive multiple pawns.";

            return string.Empty;
        }

        private static bool HasAnyParticipant(IReadOnlyList<ParticipantDefinition> participants)
        {
            if (participants == null)
                return false;

            for (int i = 0; i < participants.Count; i++)
            {
                if (participants[i] != null)
                    return true;
            }

            return false;
        }

        private static bool HasAnyReflectedDefaultPawn(IReadOnlyList<ParticipantDefinition> participants)
        {
            if (participants == null)
                return false;

            for (int i = 0; i < participants.Count; i++)
            {
                ParticipantDefinition participant = participants[i];
                if (participant != null && participant.defaultPawn != null)
                    return true;
            }

            return false;
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
