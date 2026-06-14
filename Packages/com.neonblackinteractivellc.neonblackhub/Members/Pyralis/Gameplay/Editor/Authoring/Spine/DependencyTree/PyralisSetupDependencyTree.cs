using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisSetupDependencyNodeKind
    {
        BootstrapRoot,
        SessionDefinition,
        GameModeDefinition,
        SetupProfile,
        Participant,
        PawnDefinition,
        RuntimeCapability,
        RuntimePattern,
        FeatureModule,
        Profile,
        Prefab,
        BoardDefinition,
        TurnOrderDefinition
    }

    public sealed class PyralisSetupDependencyNode
    {
        public PyralisSetupDependencyNode(
            string stableId,
            string label,
            PyralisSetupDependencyNodeKind kind,
            UnityEngine.Object sourceObject,
            string sourceFieldPath)
        {
            StableId = stableId ?? string.Empty;
            Label = label ?? string.Empty;
            Kind = kind;
            SourceObject = sourceObject;
            SourceFieldPath = sourceFieldPath ?? string.Empty;
        }

        public string StableId { get; }
        public string Label { get; }
        public PyralisSetupDependencyNodeKind Kind { get; }
        public UnityEngine.Object SourceObject { get; }
        public string SourceFieldPath { get; }
        public bool IsResolved => SourceObject != null;
    }

    public sealed class PyralisSetupDependencyEdge
    {
        public PyralisSetupDependencyEdge(string fromNodeId, string toNodeId, string fieldPath, string label)
        {
            FromNodeId = fromNodeId ?? string.Empty;
            ToNodeId = toNodeId ?? string.Empty;
            FieldPath = fieldPath ?? string.Empty;
            Label = label ?? string.Empty;
        }

        public string FromNodeId { get; }
        public string ToNodeId { get; }
        public string FieldPath { get; }
        public string Label { get; }
    }

    public sealed class PyralisSetupDependencyTree
    {
        private readonly List<PyralisSetupDependencyNode> _nodes = new List<PyralisSetupDependencyNode>();
        private readonly List<PyralisSetupDependencyEdge> _edges = new List<PyralisSetupDependencyEdge>();

        private PyralisSetupDependencyTree(UnityEngine.Object source)
        {
            Source = source;
        }

        public UnityEngine.Object Source { get; }
        public GameplaySessionBootstrap Bootstrap { get; private set; }
        public SessionDefinition Session { get; private set; }
        public GameModeDefinition Mode { get; private set; }
        public GameSetupProfile SetupProfile { get; private set; }
        public ParticipantDefinition FirstParticipant { get; private set; }
        public PawnDefinition FirstPawn { get; private set; }
        public IReadOnlyList<ParticipantDefinition> Participants => _participants;
        public IReadOnlyList<PawnDefinition> Pawns => _pawns;
        public IReadOnlyList<RuntimePatternDefinition> RuntimePatterns => _runtimePatterns;
        public IReadOnlyList<FeatureModuleDefinition> FeatureModules => _featureModules;
        public IReadOnlyList<PyralisSetupDependencyNode> Nodes => _nodes;
        public IReadOnlyList<PyralisSetupDependencyEdge> Edges => _edges;

        private readonly List<ParticipantDefinition> _participants = new List<ParticipantDefinition>();
        private readonly List<PawnDefinition> _pawns = new List<PawnDefinition>();
        private readonly List<RuntimePatternDefinition> _runtimePatterns = new List<RuntimePatternDefinition>();
        private readonly List<FeatureModuleDefinition> _featureModules = new List<FeatureModuleDefinition>();

        public static PyralisSetupDependencyTree Build(UnityEngine.Object source)
        {
            PyralisSetupDependencyTree tree = new PyralisSetupDependencyTree(source);
            tree.Resolve(source);
            tree.BuildNodes();
            return tree;
        }

        public bool TryFindNode(string stableId, out PyralisSetupDependencyNode node)
        {
            for (int i = 0; i < _nodes.Count; i++)
            {
                if (string.Equals(_nodes[i].StableId, stableId, StringComparison.Ordinal))
                {
                    node = _nodes[i];
                    return true;
                }
            }

            node = null;
            return false;
        }

        private void Resolve(UnityEngine.Object source)
        {
            Bootstrap = source as GameplaySessionBootstrap;
            if (Bootstrap != null)
                Session = GetObjectReference<SessionDefinition>(Bootstrap, "sessionDefinition");

            if (source is SessionDefinition selectedSession)
                Session = selectedSession;

            if (source is GameModeDefinition selectedMode)
                Mode = selectedMode;

            if (source is GameSetupProfile selectedSetup)
                SetupProfile = selectedSetup;

            if (source is ParticipantDefinition selectedParticipant)
                FirstParticipant = selectedParticipant;

            if (source is PawnDefinition selectedPawn)
                FirstPawn = selectedPawn;

            if (Session != null && Mode == null)
                Mode = GetObjectReference<GameModeDefinition>(Session, "defaultGameMode");

            if (Mode != null && SetupProfile == null)
                SetupProfile = GetObjectReference<GameSetupProfile>(Mode, "setupProfile");

            if (Session != null && FirstParticipant == null)
            {
                AddRangeDistinct(_participants, GetArrayReferences<ParticipantDefinition>(Session, "defaultParticipants"));
                FirstParticipant = _participants.Count > 0 ? _participants[0] : null;
            }
            else if (FirstParticipant != null)
            {
                AddDistinct(_participants, FirstParticipant);
            }

            if (FirstParticipant != null && FirstPawn == null)
                FirstPawn = GetObjectReference<PawnDefinition>(FirstParticipant, "defaultPawn");

            for (int i = 0; i < _participants.Count; i++)
            {
                PawnDefinition pawn = GetObjectReference<PawnDefinition>(_participants[i], "defaultPawn");
                AddDistinct(_pawns, pawn);
            }

            AddDistinct(_pawns, FirstPawn);
            FirstPawn = _pawns.Count > 0 ? _pawns[0] : null;

            if (Mode != null)
                AddRangeDistinct(_featureModules, GetArrayReferences<FeatureModuleDefinition>(Mode, "requiredFeatureModules"));

            if (SetupProfile != null)
            {
                AddRangeDistinct(_runtimePatterns, GetArrayReferences<RuntimePatternDefinition>(SetupProfile, "runtimePatterns"));
                AddRuntimeCapabilityPatternReferences(SetupProfile);
            }

            for (int i = 0; i < _pawns.Count; i++)
            {
                PawnDefinition pawn = _pawns[i];
                if (pawn == null)
                    continue;

                AddRangeDistinct(_featureModules, GetArrayReferences<FeatureModuleDefinition>(pawn, "featureModules"));
            }
        }

        private void BuildNodes()
        {
            AddNode("bootstrap.root", "Gameplay Root", PyralisSetupDependencyNodeKind.BootstrapRoot, Bootstrap, string.Empty);
            AddNode("session.definition", "Session Definition", PyralisSetupDependencyNodeKind.SessionDefinition, Session, "GameplaySessionBootstrap.sessionDefinition");
            AddNode("mode.definition", "Game Mode Definition", PyralisSetupDependencyNodeKind.GameModeDefinition, Mode, "SessionDefinition.defaultGameMode");
            AddNode("setup.profile", "Game Setup Profile", PyralisSetupDependencyNodeKind.SetupProfile, SetupProfile, "GameModeDefinition.setupProfile");
            AddNode("participant.default", "Participants", PyralisSetupDependencyNodeKind.Participant, FirstParticipant, "SessionDefinition.defaultParticipants");
            AddNode("pawn.definition", "Pawn Definition", PyralisSetupDependencyNodeKind.PawnDefinition, FirstPawn, "ParticipantDefinition.defaultPawn");
            AddSetupProfileDependencyNodes();
            AddModeDependencyNodes();
            AddParticipantDependencyNodes();
            AddPawnDependencyNodes();

            AddEdge("bootstrap.root", "session.definition", "sessionDefinition", "reads");
            AddEdge("session.definition", "mode.definition", "defaultGameMode", "default mode");
            AddEdge("mode.definition", "setup.profile", "setupProfile", "setup profile");
            AddEdge("session.definition", "participant.default", "defaultParticipants", "default participants");
            AddEdge("participant.default", "pawn.definition", "defaultPawn", "pawn route");
        }

        private void AddSetupProfileDependencyNodes()
        {
            if (SetupProfile == null)
                return;

            SerializedObject serializedSetup = new SerializedObject(SetupProfile);
            SerializedProperty capabilities = serializedSetup.FindProperty("runtimeCapabilities");
            if (capabilities != null && capabilities.isArray)
            {
                for (int i = 0; i < capabilities.arraySize; i++)
                {
                    SerializedProperty capability = capabilities.GetArrayElementAtIndex(i);
                    if (capability == null)
                        continue;

                    SerializedProperty family = capability.FindPropertyRelative("capabilityFamily");
                    SerializedProperty pattern = capability.FindPropertyRelative("patternDefinition");
                    string label = family != null
                        ? ((RuntimeCapabilityFamily)family.enumValueIndex).ToString()
                        : "Runtime Capability";
                    string nodeId = "setup.capability." + i;
                    AddNode(nodeId, label, PyralisSetupDependencyNodeKind.RuntimeCapability, SetupProfile, $"GameSetupProfile.runtimeCapabilities[{i}]");
                    AddEdge("setup.profile", nodeId, $"runtimeCapabilities[{i}]", "runtime capability");

                    if (pattern != null && pattern.objectReferenceValue is RuntimePatternDefinition patternDefinition)
                    {
                        string patternNodeId = "runtime-pattern." + i;
                        AddNode(patternNodeId, GetObjectLabel(patternDefinition), PyralisSetupDependencyNodeKind.RuntimePattern, patternDefinition, $"GameSetupProfile.runtimeCapabilities[{i}].patternDefinition");
                        AddEdge(nodeId, patternNodeId, "patternDefinition", "capability pattern");
                    }
                }
            }

            for (int i = 0; i < _runtimePatterns.Count; i++)
            {
                RuntimePatternDefinition pattern = _runtimePatterns[i];
                string nodeId = "setup.runtime-pattern." + i;
                AddNode(nodeId, GetObjectLabel(pattern), PyralisSetupDependencyNodeKind.RuntimePattern, pattern, $"GameSetupProfile.runtimePatterns[{i}]");
                AddEdge("setup.profile", nodeId, $"runtimePatterns[{i}]", "runtime pattern");
            }
        }

        private void AddModeDependencyNodes()
        {
            if (Mode == null)
                return;

            AddSingleObjectNode(
                "mode.board-definition",
                "Board Definition",
                PyralisSetupDependencyNodeKind.BoardDefinition,
                GetObjectReference<UnityEngine.Object>(Mode, "boardDefinition"),
                "mode.definition",
                "boardDefinition",
                "board rules");
            AddSingleObjectNode(
                "mode.turn-order-definition",
                "Turn Order Definition",
                PyralisSetupDependencyNodeKind.TurnOrderDefinition,
                GetObjectReference<UnityEngine.Object>(Mode, "turnOrderDefinition"),
                "mode.definition",
                "turnOrderDefinition",
                "turn order");

            for (int i = 0; i < _featureModules.Count; i++)
            {
                FeatureModuleDefinition module = _featureModules[i];
                if (module == null)
                    continue;

                string nodeId = "feature-module." + i;
                AddNode(nodeId, GetObjectLabel(module), PyralisSetupDependencyNodeKind.FeatureModule, module, $"FeatureModuleDefinition[{i}]");
            }

            FeatureModuleDefinition[] modeModules = GetArrayReferences<FeatureModuleDefinition>(Mode, "requiredFeatureModules");
            for (int i = 0; i < modeModules.Length; i++)
            {
                FeatureModuleDefinition module = modeModules[i];
                int moduleIndex = IndexOf(_featureModules, module);
                if (moduleIndex >= 0)
                    AddEdge("mode.definition", "feature-module." + moduleIndex, $"requiredFeatureModules[{i}]", "required feature module");
            }
        }

        private void AddParticipantDependencyNodes()
        {
            for (int i = 0; i < _participants.Count; i++)
            {
                ParticipantDefinition participant = _participants[i];
                if (participant == null)
                    continue;

                string participantNodeId = "participant.default." + i;
                AddNode(participantNodeId, GetObjectLabel(participant), PyralisSetupDependencyNodeKind.Participant, participant, $"SessionDefinition.defaultParticipants[{i}]");
                AddEdge("participant.default", participantNodeId, $"defaultParticipants[{i}]", "participant slot");

                PawnDefinition pawn = GetObjectReference<PawnDefinition>(participant, "defaultPawn");
                int pawnIndex = IndexOf(_pawns, pawn);
                if (pawnIndex >= 0)
                    AddEdge(participantNodeId, "pawn.definition." + pawnIndex, "defaultPawn", "default pawn");

                UnityEngine.Object inputProfile = GetObjectReference<UnityEngine.Object>(participant, "inputProfile");
                if (inputProfile != null)
                {
                    string inputNodeId = "participant.input-profile." + i;
                    AddNode(inputNodeId, GetObjectLabel(inputProfile), PyralisSetupDependencyNodeKind.Profile, inputProfile, $"ParticipantDefinition.inputProfile[{i}]");
                    AddEdge(participantNodeId, inputNodeId, "inputProfile", "input profile");
                }
            }
        }

        private void AddPawnDependencyNodes()
        {
            for (int i = 0; i < _pawns.Count; i++)
            {
                PawnDefinition pawn = _pawns[i];
                if (pawn == null)
                    continue;

                string pawnNodeId = "pawn.definition." + i;
                AddNode(pawnNodeId, GetObjectLabel(pawn), PyralisSetupDependencyNodeKind.PawnDefinition, pawn, $"PawnDefinition[{i}]");
                AddEdge("pawn.definition", pawnNodeId, $"PawnDefinition[{i}]", "pawn asset");

                AddSingleObjectNode("pawn.prefab." + i, "Pawn Prefab", PyralisSetupDependencyNodeKind.Prefab, GetObjectReference<UnityEngine.Object>(pawn, "pawnPrefab"), pawnNodeId, "pawnPrefab", "pawn prefab");
                AddPawnProfileNode(pawn, pawnNodeId, i, "defaultInputProfile");
                AddPawnProfileNode(pawn, pawnNodeId, i, "movementProfile");
                AddPawnProfileNode(pawn, pawnNodeId, i, "combatProfile");
                AddPawnProfileNode(pawn, pawnNodeId, i, "traversalProfile");
                AddPawnProfileNode(pawn, pawnNodeId, i, "presentationProfile");
                AddPawnProfileNode(pawn, pawnNodeId, i, "animationProfile");

                FeatureModuleDefinition[] pawnModules = GetArrayReferences<FeatureModuleDefinition>(pawn, "featureModules");
                for (int moduleSlot = 0; moduleSlot < pawnModules.Length; moduleSlot++)
                {
                    FeatureModuleDefinition module = pawnModules[moduleSlot];
                    int moduleIndex = IndexOf(_featureModules, module);
                    if (moduleIndex >= 0)
                        AddEdge(pawnNodeId, "feature-module." + moduleIndex, $"featureModules[{moduleSlot}]", "pawn feature module");
                }
            }
        }

        private void AddPawnProfileNode(PawnDefinition pawn, string pawnNodeId, int pawnIndex, string fieldPath)
        {
            UnityEngine.Object profile = GetObjectReference<UnityEngine.Object>(pawn, fieldPath);
            if (profile == null)
                return;

            string nodeId = $"pawn.profile.{pawnIndex}.{NormalizeId(fieldPath)}";
            AddNode(nodeId, GetObjectLabel(profile), PyralisSetupDependencyNodeKind.Profile, profile, $"PawnDefinition.{fieldPath}");
            AddEdge(pawnNodeId, nodeId, fieldPath, "profile");
        }

        private void AddSingleObjectNode(
            string nodeId,
            string fallbackLabel,
            PyralisSetupDependencyNodeKind kind,
            UnityEngine.Object sourceObject,
            string parentNodeId,
            string fieldPath,
            string edgeLabel)
        {
            if (sourceObject == null)
                return;

            AddNode(nodeId, GetObjectLabel(sourceObject, fallbackLabel), kind, sourceObject, fieldPath);
            AddEdge(parentNodeId, nodeId, fieldPath, edgeLabel);
        }

        private void AddNode(
            string stableId,
            string label,
            PyralisSetupDependencyNodeKind kind,
            UnityEngine.Object sourceObject,
            string sourceFieldPath)
        {
            _nodes.Add(new PyralisSetupDependencyNode(stableId, label, kind, sourceObject, sourceFieldPath));
        }

        private void AddEdge(string fromNodeId, string toNodeId, string fieldPath, string label)
        {
            _edges.Add(new PyralisSetupDependencyEdge(fromNodeId, toNodeId, fieldPath, label));
        }

        private void AddRuntimeCapabilityPatternReferences(GameSetupProfile setupProfile)
        {
            SerializedObject serializedSetup = new SerializedObject(setupProfile);
            SerializedProperty capabilities = serializedSetup.FindProperty("runtimeCapabilities");
            if (capabilities == null || !capabilities.isArray)
                return;

            for (int i = 0; i < capabilities.arraySize; i++)
            {
                SerializedProperty capability = capabilities.GetArrayElementAtIndex(i);
                SerializedProperty pattern = capability?.FindPropertyRelative("patternDefinition");
                AddDistinct(_runtimePatterns, pattern?.objectReferenceValue as RuntimePatternDefinition);
            }
        }

        private static T GetObjectReference<T>(UnityEngine.Object owner, string propertyPath) where T : UnityEngine.Object
        {
            if (owner == null || string.IsNullOrWhiteSpace(propertyPath))
                return null;

            SerializedObject serializedObject = new SerializedObject(owner);
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            return property != null ? property.objectReferenceValue as T : null;
        }

        private static T[] GetArrayReferences<T>(UnityEngine.Object owner, string propertyPath) where T : UnityEngine.Object
        {
            if (owner == null || string.IsNullOrWhiteSpace(propertyPath))
                return Array.Empty<T>();

            SerializedObject serializedObject = new SerializedObject(owner);
            SerializedProperty property = serializedObject.FindProperty(propertyPath);
            if (property == null || !property.isArray)
                return Array.Empty<T>();

            List<T> values = new List<T>();
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                if (element != null && element.objectReferenceValue is T value)
                    values.Add(value);
            }

            return values.ToArray();
        }

        private static void AddRangeDistinct<T>(List<T> target, T[] values) where T : UnityEngine.Object
        {
            if (target == null || values == null)
                return;

            for (int i = 0; i < values.Length; i++)
                AddDistinct(target, values[i]);
        }

        private static void AddDistinct<T>(List<T> target, T value) where T : UnityEngine.Object
        {
            if (target == null || value == null)
                return;

            if (IndexOf(target, value) < 0)
                target.Add(value);
        }

        private static int IndexOf<T>(List<T> values, T value) where T : UnityEngine.Object
        {
            if (values == null || value == null)
                return -1;

            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] == value)
                    return i;
            }

            return -1;
        }

        private static string GetObjectLabel(UnityEngine.Object value, string fallback = null)
        {
            return value != null && !string.IsNullOrWhiteSpace(value.name)
                ? value.name
                : fallback ?? "Dependency";
        }

        private static string NormalizeId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "unknown";

            return value.Trim()
                .Replace(" ", "-")
                .Replace("_", "-")
                .ToLowerInvariant();
        }
    }
}
