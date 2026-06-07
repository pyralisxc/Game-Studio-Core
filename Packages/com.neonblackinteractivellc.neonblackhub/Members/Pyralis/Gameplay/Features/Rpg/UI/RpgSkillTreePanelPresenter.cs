using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NeonBlack.Gameplay.Features.Composition;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace NeonBlack.Gameplay.Features.Rpg.UI
{
    [AddComponentMenu("NeonBlack/Gameplay/RPG/UI/RPG Skill Tree Panel Presenter")]
    public sealed class RpgSkillTreePanelPresenter : MonoBehaviour, IRuntimeValidationProvider
    {
        [Header("Route")]
        [SerializeField] private RpgPanelRoutePresenter routePresenter;

        [Header("Definitions")]
        [SerializeField] private SkillTreeDefinition[] skillTrees = Array.Empty<SkillTreeDefinition>();

        [Header("Owner")]
        [SerializeField] private RpgOwnerKind ownerKind = RpgOwnerKind.Participant;
        [SerializeField] private string ownerStableId = "seat-1";

        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI treeLabel;
        [SerializeField] private TextMeshProUGUI skillPointLabel;
        [SerializeField] private TextMeshProUGUI nodeListLabel;
        [SerializeField] private TextMeshProUGUI selectedNodeLabel;
        [SerializeField] private TextMeshProUGUI issueLabel;

        [Header("Controls")]
        [SerializeField] private Button unlockButton;
        [SerializeField] private Button nextNodeButton;
        [SerializeField] private Button previousNodeButton;
        [SerializeField] private Button nextTreeButton;
        [SerializeField] private Button previousTreeButton;

        [Header("Copy")]
        [SerializeField] private string emptyTreeText = "No skill nodes available.";

        private ProgressionService _progressionService;
        private SkillTreeService _skillTreeService;
        private SkillTreeDefinition[] _runtimeTrees = Array.Empty<SkillTreeDefinition>();
        private RpgOwnerKey _runtimeOwner;
        private bool _hasRuntimeOwner;
        private int _selectedTreeIndex;
        private int _selectedNodeIndex;

        public RpgSkillTreeEntry[] Entries { get; private set; } = Array.Empty<RpgSkillTreeEntry>();
        public string LastIssue { get; private set; } = string.Empty;
        public string SkillPointText { get; private set; } = "Skill Points: 0";
        public int SelectedTreeIndex => _selectedTreeIndex;
        public int SelectedNodeIndex => _selectedNodeIndex;
        public RpgSkillTreeEntry SelectedEntry => Entries.Length > 0 && _selectedNodeIndex >= 0 && _selectedNodeIndex < Entries.Length ? Entries[_selectedNodeIndex] : default;

        [Inject]
        private void Construct(ProgressionService progression = null, SkillTreeService skills = null)
        {
            if (_progressionService == null)
                _progressionService = progression;

            if (_skillTreeService == null)
                _skillTreeService = skills ?? new SkillTreeService(_progressionService);
        }

        private void Awake()
        {
            ResolveReferences();
            EnsureServices();
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

        public void ConfigureForTests(RpgOwnerKey owner, ProgressionService progression, SkillTreeService service, SkillTreeDefinition[] trees)
        {
            _runtimeOwner = owner;
            _hasRuntimeOwner = true;
            _progressionService = progression;
            _skillTreeService = service ?? new SkillTreeService(_progressionService);
            _runtimeTrees = trees ?? Array.Empty<SkillTreeDefinition>();
        }

        public bool ShowInteractionResult(HubInteractionResult result)
        {
            if (result.Status != HubInteractionStatus.Selected || result.PanelRoute != PlayerPanelRoute.SkillTree && result.PanelRoute != PlayerPanelRoute.Trainer)
                return false;

            SelectTreeForResult(result);
            LastIssue = string.Empty;
            RefreshEntries();
            return true;
        }

        public void SelectNextNode()
        {
            if (Entries.Length == 0)
                return;

            _selectedNodeIndex = (_selectedNodeIndex + 1) % Entries.Length;
            Render();
        }

        public void SelectPreviousNode()
        {
            if (Entries.Length == 0)
                return;

            _selectedNodeIndex = (_selectedNodeIndex - 1 + Entries.Length) % Entries.Length;
            Render();
        }

        public void SelectNextTree()
        {
            SkillTreeDefinition[] trees = GetTrees();
            if (trees.Length == 0)
                return;

            _selectedTreeIndex = (_selectedTreeIndex + 1) % trees.Length;
            _selectedNodeIndex = 0;
            LastIssue = string.Empty;
            RefreshEntries();
        }

        public void SelectPreviousTree()
        {
            SkillTreeDefinition[] trees = GetTrees();
            if (trees.Length == 0)
                return;

            _selectedTreeIndex = (_selectedTreeIndex - 1 + trees.Length) % trees.Length;
            _selectedNodeIndex = 0;
            LastIssue = string.Empty;
            RefreshEntries();
        }

        public bool SelectNode(string nodeId)
        {
            string normalizedNodeId = Normalize(nodeId);
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].NodeId != normalizedNodeId)
                    continue;

                _selectedNodeIndex = i;
                Render();
                return true;
            }

            return Fail($"Skill node `{normalizedNodeId}` is not in this tree.");
        }

        public bool UnlockSelectedNode()
        {
            SkillTreeDefinition tree = ActiveTree;
            RpgSkillTreeEntry selected = SelectedEntry;
            if (tree == null)
                return Fail("No skill tree is selected.");

            if (string.IsNullOrEmpty(selected.NodeId))
                return Fail("No skill node is selected.");

            EnsureServices();
            if (!_skillTreeService.TryUnlock(ResolveOwner(), tree, selected.NodeId, out string issue))
                return Fail(issue);

            LastIssue = string.Empty;
            RefreshEntries();
            SelectNode(selected.NodeId);
            return true;
        }

        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            ResolveReferences();

            if (routePresenter == null)
                yield return "`RpgSkillTreePanelPresenter` should reference the SkillTree or Trainer route presenter or live below one.";

            if ((skillTrees == null || skillTrees.Length == 0) && (_runtimeTrees == null || _runtimeTrees.Length == 0))
                yield return "`RpgSkillTreePanelPresenter` should reference at least one Skill Tree Definition.";

            if (nodeListLabel == null && selectedNodeLabel == null)
                yield return "`RpgSkillTreePanelPresenter` should reference a node list or selected node label.";

            if (unlockButton == null)
                yield return "`RpgSkillTreePanelPresenter` needs Unlock Button or a project input bridge calling UnlockSelectedNode().";
        }

        private SkillTreeDefinition ActiveTree
        {
            get
            {
                SkillTreeDefinition[] trees = GetTrees();
                return trees.Length > 0 && _selectedTreeIndex >= 0 && _selectedTreeIndex < trees.Length ? trees[_selectedTreeIndex] : null;
            }
        }

        private void HandlePanelOpened(HubInteractionResult result)
        {
            ShowInteractionResult(result);
        }

        private void RefreshEntries()
        {
            SkillTreeDefinition tree = ActiveTree;
            if (tree == null)
            {
                Entries = Array.Empty<RpgSkillTreeEntry>();
                Render();
                return;
            }

            SkillNodeDefinition[] nodes = tree.nodes ?? Array.Empty<SkillNodeDefinition>();
            List<RpgSkillTreeEntry> entries = new List<RpgSkillTreeEntry>();
            for (int i = 0; i < nodes.Length; i++)
            {
                SkillNodeDefinition definition = nodes[i];
                if (string.IsNullOrWhiteSpace(definition.NodeId))
                    continue;

                int unlockCount = _skillTreeService != null ? _skillTreeService.GetUnlockCount(ResolveOwner(), definition.NodeId) : 0;
                entries.Add(new RpgSkillTreeEntry(
                    definition.NodeId,
                    definition.DisplayName,
                    definition.Cost,
                    unlockCount,
                    definition.repeatable,
                    BuildPrerequisiteSummary(definition),
                    CanUnlockNode(tree, definition, unlockCount)));
            }

            Entries = entries.ToArray();
            if (_selectedNodeIndex >= Entries.Length)
                _selectedNodeIndex = Math.Max(0, Entries.Length - 1);

            Render();
        }

        private void Render()
        {
            SkillTreeDefinition tree = ActiveTree;
            if (treeLabel != null)
                treeLabel.text = tree != null ? GetTreeTitle(tree) : string.Empty;

            SkillPointText = BuildSkillPointText();
            if (skillPointLabel != null)
                skillPointLabel.text = SkillPointText;

            if (nodeListLabel != null)
                nodeListLabel.text = BuildNodeListText();

            RpgSkillTreeEntry selected = SelectedEntry;
            if (selectedNodeLabel != null)
                selectedNodeLabel.text = BuildSelectedNodeText(selected);

            if (issueLabel != null)
                issueLabel.text = LastIssue;

            if (unlockButton != null)
                unlockButton.interactable = selected.CanUnlock;

            bool hasMultipleNodes = Entries.Length > 1;
            if (nextNodeButton != null)
                nextNodeButton.interactable = hasMultipleNodes;
            if (previousNodeButton != null)
                previousNodeButton.interactable = hasMultipleNodes;

            bool hasMultipleTrees = GetTrees().Length > 1;
            if (nextTreeButton != null)
                nextTreeButton.interactable = hasMultipleTrees;
            if (previousTreeButton != null)
                previousTreeButton.interactable = hasMultipleTrees;
        }

        private string BuildNodeListText()
        {
            if (Entries.Length == 0)
                return emptyTreeText;

            string[] lines = new string[Entries.Length];
            for (int i = 0; i < Entries.Length; i++)
            {
                string marker = i == _selectedNodeIndex ? "> " : "  ";
                string state = Entries[i].IsUnlocked ? "unlocked" : Entries[i].CanUnlock ? "available" : "locked";
                lines[i] = marker + Entries[i].Title + " - " + Entries[i].Cost + " SP - " + state;
            }

            return string.Join(System.Environment.NewLine, lines);
        }

        private string BuildSelectedNodeText(RpgSkillTreeEntry selected)
        {
            if (string.IsNullOrEmpty(selected.NodeId))
                return string.Empty;

            string repeatable = selected.Repeatable ? " repeatable" : string.Empty;
            string prerequisites = string.IsNullOrEmpty(selected.PrerequisiteSummary) ? string.Empty : " Requires: " + selected.PrerequisiteSummary;
            return selected.Title + " - Cost: " + selected.Cost + repeatable + prerequisites;
        }

        private string BuildSkillPointText()
        {
            int skillPoints = _progressionService != null ? _progressionService.GetState(ResolveOwner()).SkillPoints : 0;
            return "Skill Points: " + skillPoints;
        }

        private bool CanUnlockNode(SkillTreeDefinition tree, SkillNodeDefinition node, int unlockCount)
        {
            if (_skillTreeService == null)
                return false;

            if (unlockCount > 0 && !node.repeatable)
                return false;

            string[] prerequisites = node.PrerequisiteIds;
            for (int i = 0; i < prerequisites.Length; i++)
            {
                if (!_skillTreeService.IsUnlocked(ResolveOwner(), prerequisites[i]))
                    return false;
            }

            int skillPoints = _progressionService != null ? _progressionService.GetState(ResolveOwner()).SkillPoints : 0;
            return node.Cost <= 0 || skillPoints >= node.Cost;
        }

        private string BuildPrerequisiteSummary(SkillNodeDefinition node)
        {
            string[] prerequisites = node.PrerequisiteIds;
            if (prerequisites.Length == 0)
                return string.Empty;

            string[] titles = new string[prerequisites.Length];
            SkillTreeDefinition tree = ActiveTree;
            for (int i = 0; i < prerequisites.Length; i++)
            {
                titles[i] = tree != null && tree.TryGetNode(prerequisites[i], out SkillNodeDefinition prerequisite)
                    ? prerequisite.DisplayName
                    : prerequisites[i];
            }

            return string.Join(", ", titles);
        }

        private void SelectTreeForResult(HubInteractionResult result)
        {
            string desiredTreeId = !string.IsNullOrWhiteSpace(result.NpcId) ? result.NpcId : result.Prompt.InteractableId;
            if (string.IsNullOrWhiteSpace(desiredTreeId))
                return;

            SkillTreeDefinition[] trees = GetTrees();
            string normalizedTreeId = Normalize(desiredTreeId);
            for (int i = 0; i < trees.Length; i++)
            {
                if (trees[i] != null && Normalize(trees[i].treeId) == normalizedTreeId)
                {
                    _selectedTreeIndex = i;
                    _selectedNodeIndex = 0;
                    return;
                }
            }
        }

        private SkillTreeDefinition[] GetTrees()
        {
            if (_runtimeTrees != null && _runtimeTrees.Length > 0)
                return _runtimeTrees;

            return skillTrees ?? Array.Empty<SkillTreeDefinition>();
        }

        private void ResolveReferences()
        {
            if (routePresenter == null)
                routePresenter = GetComponentInParent<RpgPanelRoutePresenter>() ?? GetComponentInChildren<RpgPanelRoutePresenter>(true);
        }

        private void EnsureServices()
        {
            if (_skillTreeService == null)
                _skillTreeService = new SkillTreeService(_progressionService);
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
            unlockButton?.onClick.AddListener(UnlockSelectedNodeFromButton);
            nextNodeButton?.onClick.AddListener(SelectNextNode);
            previousNodeButton?.onClick.AddListener(SelectPreviousNode);
            nextTreeButton?.onClick.AddListener(SelectNextTree);
            previousTreeButton?.onClick.AddListener(SelectPreviousTree);
        }

        private void UnbindButtons()
        {
            unlockButton?.onClick.RemoveListener(UnlockSelectedNodeFromButton);
            nextNodeButton?.onClick.RemoveListener(SelectNextNode);
            previousNodeButton?.onClick.RemoveListener(SelectPreviousNode);
            nextTreeButton?.onClick.RemoveListener(SelectNextTree);
            previousTreeButton?.onClick.RemoveListener(SelectPreviousTree);
        }

        private void UnlockSelectedNodeFromButton()
        {
            UnlockSelectedNode();
        }

        private bool Fail(string issue)
        {
            LastIssue = issue ?? string.Empty;
            Render();
            return false;
        }

        private static string GetTreeTitle(SkillTreeDefinition tree)
        {
            if (tree != null && !string.IsNullOrWhiteSpace(tree.displayName))
                return tree.displayName.Trim();

            return tree != null ? tree.treeId : string.Empty;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
