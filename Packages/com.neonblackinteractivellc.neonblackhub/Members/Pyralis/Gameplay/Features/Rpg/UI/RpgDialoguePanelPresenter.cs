using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AuthoringContract(
        ModuleId = "rpg.dialogue.ui",
        Capability = AuthoringCapability.Dialogue,
        Lane = "RPG",
        RequiredInterfaces = new[] { typeof(IRuntimeValidationProvider) },
        RequiredComponentNames = new[] { "TMPro.TextMeshProUGUI" },
        FirstProof = "Verify that the dialogue panel displays the speaker name and line text correctly."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/RPG Dialogue Panel Presenter")]
    public sealed class RpgDialoguePanelPresenter : MonoBehaviour, IRuntimeValidationProvider
{
        [Header("Route")]
        [SerializeField] private RpgPanelRoutePresenter routePresenter;

        [Header("Definitions")]
        [SerializeField] private DialogueGraphDefinition[] dialogueGraphs = Array.Empty<DialogueGraphDefinition>();
        [SerializeField] private NpcDefinition[] npcProfiles = Array.Empty<NpcDefinition>();

        [Header("Owner")]
        [SerializeField] private RpgOwnerKind ownerKind = RpgOwnerKind.Participant;
        [SerializeField] private string ownerStableId = "seat-1";

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI speakerLabel;
        [SerializeField] private TextMeshProUGUI lineLabel;
        [SerializeField] private TextMeshProUGUI choiceSummaryLabel;
        [SerializeField] private TextMeshProUGUI issueLabel;

        [Header("Controls")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button[] choiceButtons = Array.Empty<Button>();

        [Header("Copy")]
        [SerializeField] private string endedText = "Conversation ended.";
        [SerializeField] private string emptyChoiceText = string.Empty;

        private readonly List<UnityAction> _choiceButtonHandlers = new List<UnityAction>();
        private DialogueService _dialogueService;
        private IDialogueGraph[] _runtimeGraphs = Array.Empty<IDialogueGraph>();
        private INpcProfile[] _runtimeNpcs = Array.Empty<INpcProfile>();
        private RpgOwnerKey _runtimeOwner;
        private bool _hasRuntimeOwner;
        private IDialogueGraph _activeGraph;

        public DialogueSessionState CurrentState { get; private set; }
        public DialogueNode CurrentNode { get; private set; }
        public DialogueChoice[] AvailableChoices { get; private set; } = Array.Empty<DialogueChoice>();
        public string LastIssue { get; private set; } = string.Empty;

        [Inject]
        private void Construct(DialogueService dialogue)
        {
            _dialogueService = dialogue;
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            BindRoutePresenter();
            BindButtons();
            Render();
        }

        private void OnDisable()
        {
            UnbindRoutePresenter();
            UnbindButtons();
        }

        public void ConfigureForTests(RpgOwnerKey owner, DialogueService service, IDialogueGraph[] graphs, INpcProfile[] npcs)
        {
            _runtimeOwner = owner;
            _hasRuntimeOwner = true;
            _dialogueService = service;
            _runtimeGraphs = graphs ?? Array.Empty<IDialogueGraph>();
            _runtimeNpcs = npcs ?? Array.Empty<INpcProfile>();
        }

        public bool ShowInteractionResult(HubInteractionResult result)
        {
            if (result.Status != HubInteractionStatus.Selected || result.PanelRoute != PlayerPanelRoute.Dialogue)
                return false;

            if (!TryResolveGraph(result.DialogueGraphId, out IDialogueGraph graph))
                return Fail($"Dialogue graph `{result.DialogueGraphId}` could not be found.");

            if (!TryResolveNpc(result.NpcId, graph, out INpcProfile npc))
                return Fail($"Dialogue NPC `{result.NpcId}` could not be found.");

            RpgOwnerKey owner = ResolveOwner();
            if (!_dialogueService.TryStartSession(owner, npc, graph, out DialogueSessionState state, out string issue))
                return Fail(issue);

            _activeGraph = graph;
            CurrentState = state;
            LastIssue = string.Empty;
            RefreshCurrentNode();
            return true;
        }

        public bool Continue()
        {
            if (_activeGraph == null)
                return Fail("No active dialogue graph is open.");

            if (!_dialogueService.TryContinue(ResolveOwner(), _activeGraph, out DialogueSessionState state, out string issue))
                return Fail(issue);

            CurrentState = state;
            LastIssue = string.Empty;
            RefreshCurrentNode();
            return true;
        }

        public bool SelectChoice(string choiceId)
        {
            if (_activeGraph == null)
                return Fail("No active dialogue graph is open.");

            if (!_dialogueService.TrySelectChoice(ResolveOwner(), _activeGraph, choiceId, out DialogueSessionState state, out string issue))
                return Fail(issue);

            CurrentState = state;
            LastIssue = string.Empty;
            RefreshCurrentNode();
            return true;
        }

        public bool SelectChoiceByIndex(int index)
        {
            DialogueChoice[] choices = AvailableChoices ?? Array.Empty<DialogueChoice>();
            if (index < 0 || index >= choices.Length)
                return Fail($"Dialogue choice index `{index}` is not available.");

            return SelectChoice(choices[index].ChoiceId);
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            ResolveReferences();

            if (routePresenter == null)
                yield return "`RpgDialoguePanelPresenter` should reference the Dialogue route presenter or live below one.";

            if ((dialogueGraphs == null || dialogueGraphs.Length == 0) && (_runtimeGraphs == null || _runtimeGraphs.Length == 0))
                yield return "`RpgDialoguePanelPresenter` should reference at least one Dialogue Graph Definition.";

            if (lineLabel == null)
                yield return "`RpgDialoguePanelPresenter` should reference a line label so the current dialogue node is visible.";

            if (continueButton == null && (choiceButtons == null || choiceButtons.Length == 0))
                yield return "`RpgDialoguePanelPresenter` needs Continue Button or Choice Buttons for player input.";
        }

        private void HandlePanelOpened(HubInteractionResult result)
        {
            ShowInteractionResult(result);
        }

        private void RefreshCurrentNode()
        {
            CurrentNode = default;
            AvailableChoices = Array.Empty<DialogueChoice>();

            if (_activeGraph == null || CurrentState.Ended || !_activeGraph.TryGetNode(CurrentState.CurrentNodeId, out DialogueNode node))
            {
                Render();
                return;
            }

            CurrentNode = node;
            AvailableChoices = _dialogueService.GetAvailableChoices(ResolveOwner(), _activeGraph);
            Render();
        }

        private void Render()
        {
            bool ended = CurrentState.Ended || _activeGraph == null;
            if (speakerLabel != null)
                speakerLabel.text = ended ? string.Empty : CurrentNode.SpeakerId;

            if (lineLabel != null)
                lineLabel.text = ended ? endedText : CurrentNode.LineText;

            if (choiceSummaryLabel != null)
                choiceSummaryLabel.text = BuildChoiceSummary();

            if (issueLabel != null)
                issueLabel.text = LastIssue;

            if (continueButton != null)
                continueButton.interactable = !ended && AvailableChoices.Length == 0 && !string.IsNullOrWhiteSpace(CurrentNode.NextNodeId);

            RenderChoiceButtons();
        }

        private void RenderChoiceButtons()
        {
            Button[] buttons = choiceButtons ?? Array.Empty<Button>();
            DialogueChoice[] choices = AvailableChoices ?? Array.Empty<DialogueChoice>();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                    continue;

                bool hasChoice = i < choices.Length;
                buttons[i].gameObject.SetActive(hasChoice || !string.IsNullOrWhiteSpace(emptyChoiceText));
                buttons[i].interactable = hasChoice;

                TextMeshProUGUI label = buttons[i].GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                    label.text = hasChoice ? choices[i].Text : emptyChoiceText;
            }
        }

        private string BuildChoiceSummary()
        {
            DialogueChoice[] choices = AvailableChoices ?? Array.Empty<DialogueChoice>();
            if (choices.Length == 0)
                return emptyChoiceText;

            string[] labels = new string[choices.Length];
            for (int i = 0; i < choices.Length; i++)
                labels[i] = choices[i].Text;

            return string.Join(System.Environment.NewLine, labels);
        }

        private bool TryResolveGraph(string graphId, out IDialogueGraph graph)
        {
            string normalizedGraphId = Normalize(graphId);
            IDialogueGraph[] runtimeGraphs = _runtimeGraphs ?? Array.Empty<IDialogueGraph>();
            for (int i = 0; i < runtimeGraphs.Length; i++)
            {
                if (runtimeGraphs[i] != null && runtimeGraphs[i].GraphId == normalizedGraphId)
                {
                    graph = runtimeGraphs[i];
                    return true;
                }
            }

            DialogueGraphDefinition[] definitions = dialogueGraphs ?? Array.Empty<DialogueGraphDefinition>();
            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i] != null && definitions[i].GraphId == normalizedGraphId)
                {
                    graph = definitions[i];
                    return true;
                }
            }

            graph = null;
            return false;
        }

        private bool TryResolveNpc(string npcId, IDialogueGraph graph, out INpcProfile npc)
        {
            string normalizedNpcId = Normalize(npcId);
            INpcProfile[] runtimeNpcs = _runtimeNpcs ?? Array.Empty<INpcProfile>();
            for (int i = 0; i < runtimeNpcs.Length; i++)
            {
                if (runtimeNpcs[i] != null && runtimeNpcs[i].NpcId == normalizedNpcId)
                {
                    npc = runtimeNpcs[i];
                    return true;
                }
            }

            NpcDefinition[] definitions = npcProfiles ?? Array.Empty<NpcDefinition>();
            for (int i = 0; i < definitions.Length; i++)
            {
                if (definitions[i] != null && definitions[i].NpcId == normalizedNpcId)
                {
                    npc = definitions[i];
                    return true;
                }
            }

            if (string.IsNullOrEmpty(normalizedNpcId) && graph != null && graph.TryGetNode(graph.StartNodeId, out DialogueNode startNode))
                normalizedNpcId = startNode.SpeakerId;

            if (!string.IsNullOrEmpty(normalizedNpcId))
            {
                npc = new NpcProfile(normalizedNpcId, normalizedNpcId, "npc", Array.Empty<string>(), string.Empty, string.Empty);
                return true;
            }

            npc = null;
            return false;
        }

        private void ResolveReferences()
        {
            if (routePresenter == null)
                routePresenter = GetComponentInParent<RpgPanelRoutePresenter>() ?? GetComponentInChildren<RpgPanelRoutePresenter>(true);
        }

        private void EnsureService()
        {
            if (_dialogueService == null)
                _dialogueService = new DialogueService();
        }

        private RpgOwnerKey ResolveOwner()
        {
            if (_hasRuntimeOwner)
                return _runtimeOwner;

            return new RpgOwnerKey(ownerKind, ownerStableId);
        }

        private void BindRoutePresenter()
        {
            ResolveReferences();
            if (routePresenter != null)
                routePresenter.PanelOpened += HandlePanelOpened;
        }

        private void UnbindRoutePresenter()
        {
            if (routePresenter != null)
                routePresenter.PanelOpened -= HandlePanelOpened;
        }

        private void BindButtons()
        {
            continueButton?.onClick.AddListener(ContinueFromButton);
            Button[] buttons = choiceButtons ?? Array.Empty<Button>();
            for (int i = 0; i < buttons.Length; i++)
            {
                int choiceIndex = i;
                UnityAction handler = () => SelectChoiceByIndex(choiceIndex);
                _choiceButtonHandlers.Add(handler);
                buttons[i]?.onClick.AddListener(handler);
            }
        }

        private void UnbindButtons()
        {
            continueButton?.onClick.RemoveListener(ContinueFromButton);
            Button[] buttons = choiceButtons ?? Array.Empty<Button>();
            for (int i = 0; i < buttons.Length && i < _choiceButtonHandlers.Count; i++)
                buttons[i]?.onClick.RemoveListener(_choiceButtonHandlers[i]);

            _choiceButtonHandlers.Clear();
        }

        private bool Fail(string issue)
        {
            LastIssue = issue ?? string.Empty;
            Render();
            return false;
        }

        private void ContinueFromButton()
        {
            Continue();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
