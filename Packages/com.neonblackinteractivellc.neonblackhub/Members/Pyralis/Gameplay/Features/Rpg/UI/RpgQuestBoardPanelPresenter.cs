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
        ModuleId = "rpg.quest.ui",
        Capability = AuthoringCapability.Dialogue,
        Lane = "RPG",
        RequiredInterfaces = new[] { typeof(IRuntimeValidationProvider) },
        RequiredComponentNames = new[] { "TMPro.TextMeshProUGUI" },
        FirstProof = "Verify that the quest board displays the list of quests and correctly handles quest selection."
    )]
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/RPG Quest Board Panel Presenter")]
    public sealed class RpgQuestBoardPanelPresenter : MonoBehaviour, IRuntimeValidationProvider
{
        [Header("Route")]
        [SerializeField] private RpgPanelRoutePresenter routePresenter;

        [Header("Definitions")]
        [SerializeField] private QuestDefinition[] quests = Array.Empty<QuestDefinition>();

        [Header("Owner")]
        [SerializeField] private RpgOwnerKind ownerKind = RpgOwnerKind.Participant;
        [SerializeField] private string ownerStableId = "seat-1";

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI boardLabel;
        [SerializeField] private TextMeshProUGUI selectedQuestLabel;
        [SerializeField] private TextMeshProUGUI selectedQuestStatusLabel;
        [SerializeField] private TextMeshProUGUI issueLabel;

        [Header("Controls")]
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        [Header("Copy")]
        [SerializeField] private string emptyBoardText = "No quests available.";

        private QuestService _questService;
        private IQuestDefinition[] _runtimeQuests = Array.Empty<IQuestDefinition>();
        private RpgOwnerKey _runtimeOwner;
        private bool _hasRuntimeOwner;
        private int _selectedIndex;

        public RpgQuestBoardEntry[] Entries { get; private set; } = Array.Empty<RpgQuestBoardEntry>();
        public int SelectedIndex => _selectedIndex;
        public RpgQuestBoardEntry SelectedEntry => Entries.Length > 0 && _selectedIndex >= 0 && _selectedIndex < Entries.Length ? Entries[_selectedIndex] : default;
        public string LastIssue { get; private set; } = string.Empty;

        [Inject]
        private void Construct(QuestService questsService = null)
        {
            if (_questService == null)
                _questService = questsService ?? new QuestService();
        }

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            BindRoutePresenter();
            BindButtons();
            RefreshEntries();
        }

        private void OnDisable()
        {
            UnbindRoutePresenter();
            UnbindButtons();
        }

        public void ConfigureForTests(RpgOwnerKey owner, QuestService service, IQuestDefinition[] questDefinitions)
        {
            _runtimeOwner = owner;
            _hasRuntimeOwner = true;
            _questService = service ?? new QuestService();
            _runtimeQuests = questDefinitions ?? Array.Empty<IQuestDefinition>();
        }

        public bool ShowInteractionResult(HubInteractionResult result)
        {
            if (result.Status != HubInteractionStatus.Selected || result.PanelRoute != PlayerPanelRoute.QuestBoard)
                return false;

            LastIssue = string.Empty;
            RefreshEntries();
            return true;
        }

        public void SelectNextQuest()
        {
            if (Entries.Length == 0)
                return;

            _selectedIndex = (_selectedIndex + 1) % Entries.Length;
            Render();
        }

        public void SelectPreviousQuest()
        {
            if (Entries.Length == 0)
                return;

            _selectedIndex = (_selectedIndex - 1 + Entries.Length) % Entries.Length;
            Render();
        }

        public bool SelectQuest(string questId)
        {
            string normalizedQuestId = Normalize(questId);
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].QuestId != normalizedQuestId)
                    continue;

                _selectedIndex = i;
                Render();
                return true;
            }

            return Fail($"Quest `{normalizedQuestId}` is not on this board.");
        }

        public bool StartSelectedQuest()
        {
            RpgQuestBoardEntry selected = SelectedEntry;
            if (string.IsNullOrEmpty(selected.QuestId))
                return Fail("No quest is selected.");

            if (!TryGetQuest(selected.QuestId, out IQuestDefinition quest))
                return Fail($"Quest `{selected.QuestId}` could not be found.");

            if (!_questService.TryStartQuest(ResolveOwner(), quest, out string issue))
                return Fail(issue);

            LastIssue = string.Empty;
            RefreshEntries();
            SelectQuest(selected.QuestId);
            return true;
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            ResolveReferences();

            if (routePresenter == null)
                yield return "`RpgQuestBoardPanelPresenter` should reference the QuestBoard route presenter or live below one.";

            if ((quests == null || quests.Length == 0) && (_runtimeQuests == null || _runtimeQuests.Length == 0))
                yield return "`RpgQuestBoardPanelPresenter` should reference at least one Quest Definition.";

            if (boardLabel == null && selectedQuestLabel == null)
                yield return "`RpgQuestBoardPanelPresenter` should reference a board or selected quest label.";

            if (acceptButton == null)
                yield return "`RpgQuestBoardPanelPresenter` can show quests without Accept Button, but players need a button or input bridge calling StartSelectedQuest().";
        }

        private void HandlePanelOpened(HubInteractionResult result)
        {
            ShowInteractionResult(result);
        }

        private void RefreshEntries()
        {
            IQuestDefinition[] questDefinitions = GetQuests();
            List<RpgQuestBoardEntry> entries = new List<RpgQuestBoardEntry>();
            RpgOwnerKey owner = ResolveOwner();
            for (int i = 0; i < questDefinitions.Length; i++)
            {
                IQuestDefinition quest = questDefinitions[i];
                if (quest == null || string.IsNullOrWhiteSpace(quest.QuestId))
                    continue;

                QuestProgressState progress = _questService != null
                    ? _questService.GetProgress(owner, quest.QuestId)
                    : new QuestProgressState(quest.QuestId, QuestStatus.NotStarted, null);
                entries.Add(new RpgQuestBoardEntry(quest.QuestId, GetQuestTitle(quest), progress.Status, quest.Repeatable));
            }

            Entries = entries.ToArray();
            if (_selectedIndex >= Entries.Length)
                _selectedIndex = Math.Max(0, Entries.Length - 1);

            Render();
        }

        private void Render()
        {
            if (boardLabel != null)
                boardLabel.text = BuildBoardText();

            RpgQuestBoardEntry selected = SelectedEntry;
            if (selectedQuestLabel != null)
                selectedQuestLabel.text = string.IsNullOrEmpty(selected.QuestId) ? string.Empty : selected.Title;

            if (selectedQuestStatusLabel != null)
                selectedQuestStatusLabel.text = string.IsNullOrEmpty(selected.QuestId) ? string.Empty : selected.Status.ToString();

            if (issueLabel != null)
                issueLabel.text = LastIssue;

            if (acceptButton != null)
                acceptButton.interactable = selected.CanStart;

            bool hasMultiple = Entries.Length > 1;
            if (nextButton != null)
                nextButton.interactable = hasMultiple;
            if (previousButton != null)
                previousButton.interactable = hasMultiple;
        }

        private string BuildBoardText()
        {
            if (Entries.Length == 0)
                return emptyBoardText;

            string[] lines = new string[Entries.Length];
            for (int i = 0; i < Entries.Length; i++)
            {
                string marker = i == _selectedIndex ? "> " : "  ";
                lines[i] = marker + Entries[i].Title + " - " + Entries[i].Status;
            }

            return string.Join(System.Environment.NewLine, lines);
        }

        private bool TryGetQuest(string questId, out IQuestDefinition quest)
        {
            string normalizedQuestId = Normalize(questId);
            IQuestDefinition[] questDefinitions = GetQuests();
            for (int i = 0; i < questDefinitions.Length; i++)
            {
                if (questDefinitions[i] != null && questDefinitions[i].QuestId == normalizedQuestId)
                {
                    quest = questDefinitions[i];
                    return true;
                }
            }

            quest = null;
            return false;
        }

        private IQuestDefinition[] GetQuests()
        {
            if (_runtimeQuests != null && _runtimeQuests.Length > 0)
                return _runtimeQuests;

            QuestDefinition[] definitions = quests ?? Array.Empty<QuestDefinition>();
            IQuestDefinition[] result = new IQuestDefinition[definitions.Length];
            for (int i = 0; i < definitions.Length; i++)
                result[i] = definitions[i];

            return result;
        }

        private void ResolveReferences()
        {
            if (routePresenter == null)
                routePresenter = GetComponentInParent<RpgPanelRoutePresenter>() ?? GetComponentInChildren<RpgPanelRoutePresenter>(true);
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
            acceptButton?.onClick.AddListener(StartSelectedQuestFromButton);
            nextButton?.onClick.AddListener(SelectNextQuest);
            previousButton?.onClick.AddListener(SelectPreviousQuest);
        }

        private void UnbindButtons()
        {
            acceptButton?.onClick.RemoveListener(StartSelectedQuestFromButton);
            nextButton?.onClick.RemoveListener(SelectNextQuest);
            previousButton?.onClick.RemoveListener(SelectPreviousQuest);
        }

        private void StartSelectedQuestFromButton()
        {
            StartSelectedQuest();
        }

        private bool Fail(string issue)
        {
            LastIssue = issue ?? string.Empty;
            Render();
            return false;
        }

        private static string GetQuestTitle(IQuestDefinition quest)
        {
            QuestDefinition definition = quest as QuestDefinition;
            if (definition != null && !string.IsNullOrWhiteSpace(definition.displayName))
                return definition.displayName.Trim();

            return quest != null ? quest.QuestId : string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
