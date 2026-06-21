using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Definitions.Rules;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor
{
    public enum PyralisSetupDependencyNodeKind
    {
        BootstrapRoot,
        SessionDefinition,
        GameModeDefinition,
        Participant,
        PawnDefinition,
        FeatureModule,
        Profile,
        Prefab,
        BoardDefinition,
        TurnOrderDefinition,
        ObjectReference
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

    public sealed class PyralisSetupAssignmentRecord
    {
        public PyralisSetupAssignmentRecord(
            UnityEngine.Object ownerObject,
            string ownerTypeName,
            string fieldPath,
            string expectedTypeName,
            UnityEngine.Object referencedObject,
            bool declaredByContract)
        {
            OwnerObject = ownerObject;
            OwnerTypeName = ownerTypeName ?? string.Empty;
            FieldPath = fieldPath ?? string.Empty;
            ExpectedTypeName = expectedTypeName ?? string.Empty;
            ReferencedObject = referencedObject;
            DeclaredByContract = declaredByContract;
        }

        public UnityEngine.Object OwnerObject { get; }
        public string OwnerTypeName { get; }
        public string FieldPath { get; }
        public string ExpectedTypeName { get; }
        public UnityEngine.Object ReferencedObject { get; }
        public bool DeclaredByContract { get; }
        public bool IsResolved => ReferencedObject != null;
        public string QualifiedFieldPath => string.IsNullOrWhiteSpace(OwnerTypeName)
            ? FieldPath
            : OwnerTypeName + "." + FieldPath;
    }

    public sealed class PyralisSetupDependencyTree
    {
        private readonly List<PyralisSetupDependencyNode> _nodes = new List<PyralisSetupDependencyNode>();
        private readonly List<PyralisSetupDependencyEdge> _edges = new List<PyralisSetupDependencyEdge>();
        private readonly List<PyralisSetupAssignmentRecord> _assignments = new List<PyralisSetupAssignmentRecord>();
        private readonly Dictionary<UnityEngine.Object, string> _objectNodeIds = new Dictionary<UnityEngine.Object, string>();

        private PyralisSetupDependencyTree(UnityEngine.Object source)
        {
            Source = source;
        }

        public UnityEngine.Object Source { get; }
        public GameplaySessionBootstrap Bootstrap { get; private set; }
        public SessionDefinition Session { get; private set; }
        public GameModeDefinition Mode { get; private set; }
        public ParticipantDefinition FirstParticipant { get; private set; }
        public PawnDefinition FirstPawn { get; private set; }
        public IReadOnlyList<ParticipantDefinition> Participants => _participants;
        public IReadOnlyList<PawnDefinition> Pawns => _pawns;
        public IReadOnlyList<FeatureModuleDefinition> FeatureModules => _featureModules;
        public IReadOnlyList<PyralisSetupDependencyNode> Nodes => _nodes;
        public IReadOnlyList<PyralisSetupDependencyEdge> Edges => _edges;
        public IReadOnlyList<PyralisSetupAssignmentRecord> AssignmentRecords => _assignments;

        private readonly List<ParticipantDefinition> _participants = new List<ParticipantDefinition>();
        private readonly List<PawnDefinition> _pawns = new List<PawnDefinition>();
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
            if (source is SessionDefinition selectedSession)
                Session = selectedSession;

            if (source is GameModeDefinition selectedMode)
                Mode = selectedMode;

            if (source is ParticipantDefinition selectedParticipant)
                FirstParticipant = selectedParticipant;

            if (source is PawnDefinition selectedPawn)
                FirstPawn = selectedPawn;

            DiscoverAssignments(source);

            Session ??= FindFirstAssigned<SessionDefinition>();
            Mode ??= FindFirstAssigned<GameModeDefinition>();
            AddAssignedObjects(_participants);
            AddAssignedObjects(_pawns);
            AddAssignedObjects(_featureModules);

            AddDistinct(_participants, FirstParticipant);
            AddDistinct(_pawns, FirstPawn);
            FirstParticipant = FirstParticipant != null ? FirstParticipant : _participants.Count > 0 ? _participants[0] : null;
            FirstPawn = _pawns.Count > 0 ? _pawns[0] : null;
        }

        private void BuildNodes()
        {
            AddNode("bootstrap.root", "Gameplay Root", PyralisSetupDependencyNodeKind.BootstrapRoot, Bootstrap, string.Empty);
            AddNode("session.definition", "Session Definition", PyralisSetupDependencyNodeKind.SessionDefinition, Session, "GameplaySessionBootstrap.sessionDefinition");
            AddNode("mode.definition", "Game Mode Definition", PyralisSetupDependencyNodeKind.GameModeDefinition, Mode, "SessionDefinition.defaultGameMode");
            AddNode("participant.default", "Participants", PyralisSetupDependencyNodeKind.Participant, FirstParticipant, "SessionDefinition.defaultParticipants");
            AddNode("pawn.definition", "Pawn Definition", PyralisSetupDependencyNodeKind.PawnDefinition, FirstPawn, "ParticipantDefinition.defaultPawn");
            AddReflectedReferenceNodes();

            AddEdge("bootstrap.root", "session.definition", "sessionDefinition", "reads");
            AddEdge("session.definition", "mode.definition", "defaultGameMode", "default mode");
            AddEdge("session.definition", "participant.default", "defaultParticipants", "default participants");
            AddEdge("participant.default", "pawn.definition", "defaultPawn", "pawn route");
        }

        private void AddReflectedReferenceNodes()
        {
            for (int i = 0; i < _assignments.Count; i++)
            {
                PyralisSetupAssignmentRecord assignment = _assignments[i];
                if (assignment == null || assignment.OwnerObject == null || assignment.ReferencedObject == null)
                    continue;

                string ownerNodeId = GetNodeIdForObject(assignment.OwnerObject);
                string targetNodeId = GetNodeIdForObject(assignment.ReferencedObject);
                AddNode(
                    targetNodeId,
                    GetObjectLabel(assignment.ReferencedObject),
                    ClassifyNodeKind(assignment.ReferencedObject),
                    assignment.ReferencedObject,
                    assignment.QualifiedFieldPath);
                AddEdge(ownerNodeId, targetNodeId, assignment.FieldPath, assignment.FieldPath);
            }
        }

        private void AddNode(
            string stableId,
            string label,
            PyralisSetupDependencyNodeKind kind,
            UnityEngine.Object sourceObject,
            string sourceFieldPath)
        {
            if (string.IsNullOrWhiteSpace(stableId))
                return;

            if (sourceObject != null && _objectNodeIds.ContainsKey(sourceObject))
                _objectNodeIds[sourceObject] = stableId;

            for (int i = 0; i < _nodes.Count; i++)
            {
                if (string.Equals(_nodes[i].StableId, stableId, StringComparison.Ordinal))
                    return;
            }

            _nodes.Add(new PyralisSetupDependencyNode(stableId, label, kind, sourceObject, sourceFieldPath));
            if (sourceObject != null && !_objectNodeIds.ContainsKey(sourceObject))
                _objectNodeIds[sourceObject] = stableId;
        }

        private void AddEdge(string fromNodeId, string toNodeId, string fieldPath, string label)
        {
            _edges.Add(new PyralisSetupDependencyEdge(fromNodeId, toNodeId, fieldPath, label));
        }

        private void DiscoverAssignments(UnityEngine.Object root)
        {
            if (root == null)
                return;

            Queue<UnityEngine.Object> queue = new Queue<UnityEngine.Object>();
            HashSet<UnityEngine.Object> visited = new HashSet<UnityEngine.Object>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                UnityEngine.Object owner = queue.Dequeue();
                if (owner == null || !visited.Add(owner) || !ShouldTraverse(owner))
                    continue;

                AddContractAssignmentRecords(owner, queue);
                AddResolvedSerializedReferenceRecords(owner, queue);
            }
        }

        private void AddContractAssignmentRecords(UnityEngine.Object owner, Queue<UnityEngine.Object> queue)
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByType(owner.GetType());
            if (contract == null || contract.AssignmentFields == null)
                return;

            for (int i = 0; i < contract.AssignmentFields.Length; i++)
                AddAssignmentRecordsForField(owner, contract.AssignmentFields[i], true, queue);
        }

        private void AddResolvedSerializedReferenceRecords(UnityEngine.Object owner, Queue<UnityEngine.Object> queue)
        {
            SerializedObject serializedObject = new SerializedObject(owner);
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (iterator.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                if (iterator.propertyPath == "m_Script")
                    continue;

                AddAssignmentRecord(
                    owner,
                    iterator.propertyPath,
                    iterator.objectReferenceValue != null ? iterator.objectReferenceValue.GetType().Name : iterator.type,
                    iterator.objectReferenceValue,
                    false,
                    queue);
            }
        }

        private void AddAssignmentRecordsForField(
            UnityEngine.Object owner,
            string fieldPath,
            bool declaredByContract,
            Queue<UnityEngine.Object> queue)
        {
            if (owner == null || string.IsNullOrWhiteSpace(fieldPath))
                return;

            SerializedObject serializedObject = new SerializedObject(owner);
            SerializedProperty property = serializedObject.FindProperty(fieldPath);
            if (property == null)
                return;

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                AddAssignmentRecord(owner, fieldPath, property.type, property.objectReferenceValue, declaredByContract, queue);
                return;
            }

            if (!property.isArray || property.propertyType == SerializedPropertyType.String)
                return;

            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                if (element == null || element.propertyType != SerializedPropertyType.ObjectReference)
                    continue;

                AddAssignmentRecord(
                    owner,
                    $"{fieldPath}[{i}]",
                    element.objectReferenceValue != null ? element.objectReferenceValue.GetType().Name : element.type,
                    element.objectReferenceValue,
                    declaredByContract,
                    queue);
            }
        }

        private void AddAssignmentRecord(
            UnityEngine.Object owner,
            string fieldPath,
            string expectedTypeName,
            UnityEngine.Object referencedObject,
            bool declaredByContract,
            Queue<UnityEngine.Object> queue)
        {
            if (owner == null || string.IsNullOrWhiteSpace(fieldPath))
                return;

            if (referencedObject != null && ShouldTraverse(referencedObject))
                queue.Enqueue(referencedObject);

            for (int i = 0; i < _assignments.Count; i++)
            {
                PyralisSetupAssignmentRecord record = _assignments[i];
                if (record.OwnerObject == owner
                    && string.Equals(record.FieldPath, fieldPath, StringComparison.Ordinal)
                    && record.ReferencedObject == referencedObject)
                {
                    return;
                }
            }

            _assignments.Add(new PyralisSetupAssignmentRecord(
                owner,
                owner.GetType().Name,
                fieldPath,
                expectedTypeName,
                referencedObject,
                declaredByContract));
        }

        private T FindFirstAssigned<T>() where T : UnityEngine.Object
        {
            for (int i = 0; i < _assignments.Count; i++)
            {
                if (_assignments[i].ReferencedObject is T value)
                    return value;
            }

            return null;
        }

        private void AddAssignedObjects<T>(List<T> target) where T : UnityEngine.Object
        {
            for (int i = 0; i < _assignments.Count; i++)
            {
                if (_assignments[i].ReferencedObject is T value)
                    AddDistinct(target, value);
            }
        }

        private string GetNodeIdForObject(UnityEngine.Object sourceObject)
        {
            if (sourceObject == null)
                return string.Empty;

            if (_objectNodeIds.TryGetValue(sourceObject, out string existingNodeId))
                return existingNodeId;

            string nodeId = "dependency." + NormalizeId(sourceObject.GetType().Name) + "." + _objectNodeIds.Count;
            _objectNodeIds[sourceObject] = nodeId;
            return nodeId;
        }

        private static bool ShouldTraverse(UnityEngine.Object sourceObject)
        {
            if (sourceObject == null)
                return false;

            return sourceObject is ScriptableObject
                || sourceObject is MonoBehaviour
                || sourceObject is GameObject;
        }

        private static PyralisSetupDependencyNodeKind ClassifyNodeKind(UnityEngine.Object sourceObject)
        {
            if (sourceObject is GameplaySessionBootstrap)
                return PyralisSetupDependencyNodeKind.BootstrapRoot;
            if (sourceObject is SessionDefinition)
                return PyralisSetupDependencyNodeKind.SessionDefinition;
            if (sourceObject is GameModeDefinition)
                return PyralisSetupDependencyNodeKind.GameModeDefinition;
            if (sourceObject is ParticipantDefinition)
                return PyralisSetupDependencyNodeKind.Participant;
            if (sourceObject is PawnDefinition)
                return PyralisSetupDependencyNodeKind.PawnDefinition;
            if (sourceObject is FeatureModuleDefinition)
                return PyralisSetupDependencyNodeKind.FeatureModule;
            if (sourceObject is BoardDefinition)
                return PyralisSetupDependencyNodeKind.BoardDefinition;
            if (sourceObject is TurnOrderDefinition)
                return PyralisSetupDependencyNodeKind.TurnOrderDefinition;
            if (sourceObject is GameObject)
                return PyralisSetupDependencyNodeKind.Prefab;
            if (sourceObject is ScriptableObject)
                return PyralisSetupDependencyNodeKind.Profile;

            return PyralisSetupDependencyNodeKind.ObjectReference;
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
