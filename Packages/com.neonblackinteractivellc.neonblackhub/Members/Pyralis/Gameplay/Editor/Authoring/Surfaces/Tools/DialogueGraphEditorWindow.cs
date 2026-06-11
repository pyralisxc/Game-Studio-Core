using System;
using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Editor.Rpg
{
    public sealed class DialogueGraphEditorWindow : EditorWindow
    {
        private DialogueGraphDefinition _graph;
        private string _selectedNodeId;
        private Vector2 _nodeScroll;
        private Vector2 _inspectorScroll;
        private Vector2 _previewScroll;
        private DialogueService _previewService;
        private DialogueSessionState _previewState;
        private string _previewIssue;

        [MenuItem("NeonBlack/Gameplay/RPG Narrative Editor")]
        public static void Open()
        {
            DialogueGraphEditorWindow window = GetWindow<DialogueGraphEditorWindow>("RPG Narrative");
            if (Selection.activeObject is DialogueGraphDefinition graph)
                window.SetGraph(graph);
        }

        public void SetGraph(DialogueGraphDefinition graph)
        {
            _graph = graph;
            _selectedNodeId = graph != null && graph.NodeDefinitions.Length > 0 ? graph.NodeDefinitions[0].NodeId : string.Empty;
            ResetPreview();
            Repaint();
        }

        private void OnEnable()
        {
            if (_graph == null && Selection.activeObject is DialogueGraphDefinition graph)
                SetGraph(graph);
        }

        private void OnGUI()
        {
            DrawHeader();
            if (_graph == null)
            {
                EditorGUILayout.HelpBox("Assign a DialogueGraphDefinition asset to edit nodes, links, validation, and preview.", MessageType.Info);
                return;
            }

            _graph.Sanitize();
            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawNodeMap();
                DrawSelectedNodeInspector();
            }

            DrawValidationAndPreview();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                DialogueGraphDefinition graph = (DialogueGraphDefinition)EditorGUILayout.ObjectField(_graph, typeof(DialogueGraphDefinition), false, GUILayout.MinWidth(260f));
                if (EditorGUI.EndChangeCheck())
                    SetGraph(graph);

                GUILayout.FlexibleSpace();
                if (_graph != null && GUILayout.Button("Ping", EditorStyles.toolbarButton, GUILayout.Width(54f)))
                    EditorGUIUtility.PingObject(_graph);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(_graph.displayName, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Line", GUILayout.Width(88f)))
                    AddNode(DialogueNodeKind.Line);

                if (GUILayout.Button("Add Choice", GUILayout.Width(96f)))
                    AddNode(DialogueNodeKind.ChoiceHub);

                if (GUILayout.Button("Add End", GUILayout.Width(82f)))
                    AddNode(DialogueNodeKind.Terminal);

                if (GUILayout.Button("Preview Start", GUILayout.Width(112f)))
                    StartPreview();
            }
        }

        private void DrawNodeMap()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(300f)))
            {
                EditorGUILayout.LabelField("Graph Map", EditorStyles.boldLabel);
                _nodeScroll = EditorGUILayout.BeginScrollView(_nodeScroll, EditorStyles.helpBox, GUILayout.MinHeight(260f));

                DialogueNodeDefinition[] nodes = _graph.NodeDefinitions;
                for (int i = 0; i < nodes.Length; i++)
                {
                    DialogueNodeDefinition node = nodes[i];
                    bool selected = node.NodeId == _selectedNodeId;
                    GUIStyle style = selected ? EditorStyles.toolbarButton : GUI.skin.button;
                    if (GUILayout.Button($"{node.NodeId}  [{node.kind}]", style))
                        _selectedNodeId = node.NodeId;

                    string next = string.IsNullOrWhiteSpace(node.NextNodeId) ? "end" : node.NextNodeId;
                    EditorGUILayout.LabelField($"  -> {next}", EditorStyles.miniLabel);

                    DialogueChoiceDefinition[] choices = node.Choices;
                    for (int choiceIndex = 0; choiceIndex < choices.Length; choiceIndex++)
                    {
                        string target = string.IsNullOrWhiteSpace(choices[choiceIndex].NextNodeId) ? "end" : choices[choiceIndex].NextNodeId;
                        EditorGUILayout.LabelField($"  [{choices[choiceIndex].ChoiceId}] -> {target}", EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawSelectedNodeInspector()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Node Inspector", EditorStyles.boldLabel);
                int nodeIndex = GetSelectedNodeIndex();
                if (nodeIndex < 0)
                {
                    EditorGUILayout.HelpBox("Select a node from the graph map.", MessageType.Info);
                    return;
                }

                _inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll, EditorStyles.helpBox, GUILayout.MinHeight(260f));
                DialogueNodeDefinition[] nodes = _graph.NodeDefinitions;
                DialogueNodeDefinition node = nodes[nodeIndex];

                EditorGUI.BeginChangeCheck();
                string nodeId = EditorGUILayout.TextField("Node Id", node.nodeId);
                DialogueNodeKind kind = (DialogueNodeKind)EditorGUILayout.EnumPopup("Kind", node.kind);
                string speakerId = EditorGUILayout.TextField("Speaker Id", node.speakerId);
                string lineText = EditorGUILayout.TextField("Line Text", node.lineText);
                string nextNodeId = DrawNodeIdPopup("Next Node Id", node.nextNodeId, allowEmpty: true);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_graph, "Edit Dialogue Node");
                    node.nodeId = nodeId;
                    node.kind = kind;
                    node.speakerId = speakerId;
                    node.lineText = lineText;
                    node.nextNodeId = nextNodeId;
                    nodes[nodeIndex] = node;
                    _graph.nodes = nodes;
                    _graph.Sanitize();
                    _selectedNodeId = node.NodeId;
                    EditorUtility.SetDirty(_graph);
                    ResetPreview();
                }

                DrawChoices(nodeIndex);
                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawChoices(int nodeIndex)
        {
            DialogueNodeDefinition[] nodes = _graph.NodeDefinitions;
            DialogueNodeDefinition node = nodes[nodeIndex];
            DialogueChoiceDefinition[] choices = node.Choices;

            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Choices", EditorStyles.boldLabel);
                if (GUILayout.Button("+", GUILayout.Width(28f)))
                {
                    string id = $"choice.{choices.Length + 1}";
                    DialogueGraphEditorModel.AddChoice(_graph, node.NodeId, id, "New choice", string.Empty);
                    ResetPreview();
                    return;
                }
            }

            for (int i = 0; i < choices.Length; i++)
            {
                DialogueChoiceDefinition choice = choices[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUI.BeginChangeCheck();
                    string choiceId = EditorGUILayout.TextField("Choice Id", choice.choiceId);
                    string text = EditorGUILayout.TextField("Text", choice.text);
                    string nextNodeId = DrawNodeIdPopup("Next Node Id", choice.nextNodeId, allowEmpty: true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_graph, "Edit Dialogue Choice");
                        choice.choiceId = choiceId;
                        choice.text = text;
                        choice.nextNodeId = nextNodeId;
                        choices[i] = choice;
                        node.choices = choices;
                        nodes[nodeIndex] = node;
                        _graph.nodes = nodes;
                        _graph.Sanitize();
                        EditorUtility.SetDirty(_graph);
                        ResetPreview();
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Conditions: {choice.Conditions.Length}  Effects: {choice.Effects.Length}", EditorStyles.miniLabel);
                        if (GUILayout.Button("Remove", GUILayout.Width(72f)))
                        {
                            Undo.RecordObject(_graph, "Remove Dialogue Choice");
                            node.choices = choices.Where((_, index) => index != i).ToArray();
                            nodes[nodeIndex] = node;
                            _graph.nodes = nodes;
                            _graph.Sanitize();
                            EditorUtility.SetDirty(_graph);
                            ResetPreview();
                            return;
                        }
                    }
                }
            }

            if (GUILayout.Button("Remove Selected Node"))
            {
                DialogueGraphEditorModel.RemoveNode(_graph, node.NodeId);
                _selectedNodeId = _graph.NodeDefinitions.Length > 0 ? _graph.NodeDefinitions[0].NodeId : string.Empty;
                ResetPreview();
            }
        }

        private void DrawValidationAndPreview()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
                    var issues = _graph.GetValidationIssues();
                    if (issues.Count == 0)
                        EditorGUILayout.HelpBox("Dialogue graph is ready for runtime preview.", MessageType.Info);
                    else
                    {
                        for (int i = 0; i < issues.Count; i++)
                            EditorGUILayout.HelpBox(issues[i], MessageType.Warning);
                    }
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.MinWidth(340f)))
                {
                    EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
                    _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, GUILayout.MinHeight(140f));
                    DrawPreviewState();
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        private void DrawPreviewState()
        {
            if (!string.IsNullOrWhiteSpace(_previewIssue))
                EditorGUILayout.HelpBox(_previewIssue, MessageType.Warning);

            if (_previewService == null)
            {
                EditorGUILayout.HelpBox("Press Preview Start to walk the authored graph with DialogueService.", MessageType.Info);
                return;
            }

            if (_previewState.Ended)
            {
                EditorGUILayout.HelpBox("Preview session ended.", MessageType.Info);
                return;
            }

            if (!_graph.TryGetNodeDefinition(_previewState.CurrentNodeId, out DialogueNodeDefinition node))
            {
                EditorGUILayout.HelpBox($"Preview node `{_previewState.CurrentNodeId}` could not be found.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField(node.SpeakerId, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(node.lineText, EditorStyles.wordWrappedLabel);

            DialogueChoice[] choices = _previewService.GetAvailableChoices(_previewState.Owner, _graph.CreateRuntimeGraph());
            if (choices.Length == 0)
            {
                if (!string.IsNullOrWhiteSpace(node.NextNodeId) && GUILayout.Button($"Continue -> {node.NextNodeId}"))
                    JumpPreview(node.NextNodeId);
                return;
            }

            for (int i = 0; i < choices.Length; i++)
            {
                if (GUILayout.Button(choices[i].Text))
                    SelectPreviewChoice(choices[i].ChoiceId);
            }
        }

        private void AddNode(DialogueNodeKind kind)
        {
            string nodeId = $"node.{_graph.NodeDefinitions.Length + 1}";
            string speakerId = kind == DialogueNodeKind.Terminal ? string.Empty : "npc.new";
            string lineText = kind == DialogueNodeKind.Terminal ? string.Empty : "New line.";
            if (DialogueGraphEditorModel.AddNode(_graph, nodeId, kind, speakerId, lineText))
                _selectedNodeId = nodeId;

            ResetPreview();
        }

        private void StartPreview()
        {
            ResetPreview();
            if (!DialogueGraphEditorModel.CanPreview(_graph, out _previewIssue))
                return;

            _previewService = new DialogueService();
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "editor-preview");
            NpcProfile npc = new NpcProfile("npc.preview", "Preview NPC", "preview", Array.Empty<string>(), string.Empty, string.Empty);
            if (!_previewService.TryStartSession(owner, npc, _graph.CreateRuntimeGraph(), out _previewState, out _previewIssue))
                _previewService = null;
        }

        private void SelectPreviewChoice(string choiceId)
        {
            if (_previewService == null)
                return;

            if (!_previewService.TrySelectChoice(_previewState.Owner, _graph.CreateRuntimeGraph(), choiceId, out _previewState, out _previewIssue))
                return;

            Repaint();
        }

        private void JumpPreview(string nextNodeId)
        {
            if (!_graph.TryGetNodeDefinition(nextNodeId, out DialogueNodeDefinition nextNode))
            {
                _previewIssue = $"Preview node `{nextNodeId}` could not be found.";
                return;
            }

            _previewState = new DialogueSessionState(_previewState.Owner, _previewState.NpcId, _previewState.GraphId, nextNode.NodeId, nextNode.kind == DialogueNodeKind.Terminal);
            _previewIssue = string.Empty;
        }

        private void ResetPreview()
        {
            _previewService = null;
            _previewState = default;
            _previewIssue = string.Empty;
        }

        private int GetSelectedNodeIndex()
        {
            DialogueNodeDefinition[] nodes = _graph.NodeDefinitions;
            int index = Array.FindIndex(nodes, node => node.NodeId == _selectedNodeId);
            if (index >= 0)
                return index;

            if (nodes.Length == 0)
                return -1;

            _selectedNodeId = nodes[0].NodeId;
            return 0;
        }

        private string DrawNodeIdPopup(string label, string currentValue, bool allowEmpty)
        {
            string[] nodeIds = DialogueGraphEditorModel.GetNodeIds(_graph);
            string[] options = allowEmpty ? new[] { string.Empty }.Concat(nodeIds).ToArray() : nodeIds;
            int currentIndex = Array.IndexOf(options, currentValue ?? string.Empty);
            if (currentIndex < 0)
                currentIndex = 0;

            int nextIndex = EditorGUILayout.Popup(label, currentIndex, options.Select(option => string.IsNullOrWhiteSpace(option) ? "(end)" : option).ToArray());
            return options.Length == 0 ? string.Empty : options[nextIndex];
        }
    }
}
