using System;
using System.Collections.Generic;

namespace NeonBlack.Gameplay.Core.Rpg
{
    public sealed class DialogueService
    {
        private readonly ProgressionService _progression;
        private readonly InventoryService _inventory;
        private readonly QuestService _quests;
        private readonly SkillTreeService _skills;
        private readonly IDialogueConditionResolver _customConditionResolver;
        private readonly IDialogueEffectSink _customEffectSink;
        private readonly Dictionary<RpgOwnerKey, DialogueSessionRecord> _sessions = new Dictionary<RpgOwnerKey, DialogueSessionRecord>();
        private readonly Dictionary<RpgOwnerKey, HashSet<string>> _dialogueFlags = new Dictionary<RpgOwnerKey, HashSet<string>>();
        private readonly Dictionary<string, IQuestDefinition> _questRegistry = new Dictionary<string, IQuestDefinition>(StringComparer.Ordinal);

        public DialogueService(
            ProgressionService progression = null,
            InventoryService inventory = null,
            QuestService quests = null,
            SkillTreeService skills = null,
            IDialogueConditionResolver customConditionResolver = null,
            IDialogueEffectSink customEffectSink = null)
        {
            _progression = progression;
            _inventory = inventory;
            _quests = quests;
            _skills = skills;
            _customConditionResolver = customConditionResolver;
            _customEffectSink = customEffectSink;
        }

        public bool RegisterQuest(IQuestDefinition quest)
        {
            string questId = quest != null ? Normalize(quest.QuestId) : string.Empty;
            if (string.IsNullOrEmpty(questId))
                return false;

            _questRegistry[questId] = quest;
            return true;
        }

        public bool TryStartSession(
            RpgOwnerKey owner,
            INpcProfile npc,
            IDialogueGraph graph,
            out DialogueSessionState state,
            out string issue)
        {
            state = default;
            if (!ValidateOwnerNpcGraph(owner, npc, graph, out issue))
                return false;

            if (!graph.TryGetNode(graph.StartNodeId, out DialogueNode startNode))
            {
                issue = $"Dialogue graph `{graph.GraphId}` start node `{graph.StartNodeId}` could not be found.";
                return false;
            }

            DialogueSessionRecord record = new DialogueSessionRecord(npc.NpcId, graph.GraphId, startNode.NodeId, startNode.Kind == DialogueNodeKind.Terminal);
            _sessions[owner] = record;
            state = ToState(owner, record);
            issue = string.Empty;
            return true;
        }

        public DialogueChoice[] GetAvailableChoices(RpgOwnerKey owner, IDialogueGraph graph)
        {
            if (!owner.IsValid || graph == null || !_sessions.TryGetValue(owner, out DialogueSessionRecord record))
                return Array.Empty<DialogueChoice>();

            if (record.Ended || record.GraphId != Normalize(graph.GraphId) || !graph.TryGetNode(record.CurrentNodeId, out DialogueNode node))
                return Array.Empty<DialogueChoice>();

            List<DialogueChoice> choices = new List<DialogueChoice>();
            DialogueChoice[] nodeChoices = node.Choices ?? Array.Empty<DialogueChoice>();
            for (int i = 0; i < nodeChoices.Length; i++)
            {
                if (AreConditionsMet(owner, nodeChoices[i].Conditions))
                    choices.Add(nodeChoices[i]);
            }

            return choices.ToArray();
        }

        public bool TrySelectChoice(
            RpgOwnerKey owner,
            IDialogueGraph graph,
            string choiceId,
            out DialogueSessionState state,
            out string issue)
        {
            state = default;
            string normalizedChoiceId = Normalize(choiceId);
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (graph == null)
            {
                issue = "A valid dialogue graph is required.";
                return false;
            }

            if (string.IsNullOrEmpty(normalizedChoiceId))
            {
                issue = "Dialogue choice id is required.";
                return false;
            }

            if (!_sessions.TryGetValue(owner, out DialogueSessionRecord record))
            {
                issue = $"No active dialogue session exists for `{owner}`.";
                return false;
            }

            DialogueChoice[] choices = GetAvailableChoices(owner, graph);
            for (int i = 0; i < choices.Length; i++)
            {
                if (choices[i].ChoiceId != normalizedChoiceId)
                    continue;

                if (!ApplyEffects(owner, choices[i].Effects, out issue))
                    return false;

                string nextNodeId = Normalize(choices[i].NextNodeId);
                if (string.IsNullOrEmpty(nextNodeId))
                {
                    record.CurrentNodeId = string.Empty;
                    record.Ended = true;
                    state = ToState(owner, record);
                    issue = string.Empty;
                    return true;
                }

                if (!graph.TryGetNode(nextNodeId, out DialogueNode nextNode))
                {
                    issue = $"Dialogue next node `{nextNodeId}` could not be found.";
                    return false;
                }

                record.CurrentNodeId = nextNode.NodeId;
                record.Ended = nextNode.Kind == DialogueNodeKind.Terminal;
                state = ToState(owner, record);
                issue = string.Empty;
                return true;
            }

            issue = $"Dialogue choice `{normalizedChoiceId}` is not available.";
            state = ToState(owner, record);
            return false;
        }

        public bool TryContinue(
            RpgOwnerKey owner,
            IDialogueGraph graph,
            out DialogueSessionState state,
            out string issue)
        {
            state = default;
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (graph == null)
            {
                issue = "A valid dialogue graph is required.";
                return false;
            }

            if (!_sessions.TryGetValue(owner, out DialogueSessionRecord record))
            {
                issue = $"No active dialogue session exists for `{owner}`.";
                return false;
            }

            if (record.Ended)
            {
                state = ToState(owner, record);
                issue = "Dialogue session has already ended.";
                return false;
            }

            if (record.GraphId != Normalize(graph.GraphId))
            {
                issue = $"Active dialogue graph `{record.GraphId}` does not match `{graph.GraphId}`.";
                return false;
            }

            if (!graph.TryGetNode(record.CurrentNodeId, out DialogueNode node))
            {
                issue = $"Dialogue current node `{record.CurrentNodeId}` could not be found.";
                return false;
            }

            if (!ApplyEffects(owner, node.Effects, out issue))
                return false;

            string nextNodeId = Normalize(node.NextNodeId);
            if (string.IsNullOrEmpty(nextNodeId))
            {
                record.CurrentNodeId = string.Empty;
                record.Ended = true;
                state = ToState(owner, record);
                issue = string.Empty;
                return true;
            }

            if (!graph.TryGetNode(nextNodeId, out DialogueNode nextNode))
            {
                issue = $"Dialogue next node `{nextNodeId}` could not be found.";
                return false;
            }

            record.CurrentNodeId = nextNode.NodeId;
            record.Ended = nextNode.Kind == DialogueNodeKind.Terminal;
            state = ToState(owner, record);
            issue = string.Empty;
            return true;
        }

        public void SetDialogueFlag(RpgOwnerKey owner, string flagId, bool value)
        {
            string normalizedFlagId = Normalize(flagId);
            if (!owner.IsValid || string.IsNullOrEmpty(normalizedFlagId))
                return;

            HashSet<string> flags = GetOrCreateFlags(owner);
            if (value)
                flags.Add(normalizedFlagId);
            else
                flags.Remove(normalizedFlagId);
        }

        public bool HasDialogueFlag(RpgOwnerKey owner, string flagId)
        {
            string normalizedFlagId = Normalize(flagId);
            return owner.IsValid
                && !string.IsNullOrEmpty(normalizedFlagId)
                && _dialogueFlags.TryGetValue(owner, out HashSet<string> flags)
                && flags.Contains(normalizedFlagId);
        }

        public RpgDialogueSnapshot GetSnapshot(RpgOwnerKey owner)
        {
            return new RpgDialogueSnapshot(GetFlagSnapshot(owner), GetSessionSnapshot(owner));
        }

        public RpgDialogueSessionSnapshot GetSessionSnapshot(RpgOwnerKey owner)
        {
            return owner.IsValid && _sessions.TryGetValue(owner, out DialogueSessionRecord record)
                ? new RpgDialogueSessionSnapshot(record.NpcId, record.GraphId, record.CurrentNodeId, record.Ended)
                : default;
        }

        public void RestoreSnapshot(RpgOwnerKey owner, RpgDialogueSnapshot snapshot)
        {
            if (!owner.IsValid)
                return;

            _dialogueFlags.Remove(owner);
            string[] flags = snapshot.Flags ?? Array.Empty<string>();
            for (int i = 0; i < flags.Length; i++)
                SetDialogueFlag(owner, flags[i], true);

            _sessions.Remove(owner);
            if (snapshot.Session.IsValid)
            {
                _sessions[owner] = new DialogueSessionRecord(
                    snapshot.Session.NpcId,
                    snapshot.Session.GraphId,
                    snapshot.Session.CurrentNodeId,
                    snapshot.Session.Ended);
            }
        }

        private bool AreConditionsMet(RpgOwnerKey owner, DialogueCondition[] conditions)
        {
            DialogueCondition[] safeConditions = conditions ?? Array.Empty<DialogueCondition>();
            for (int i = 0; i < safeConditions.Length; i++)
            {
                if (!EvaluateCondition(owner, safeConditions[i]))
                    return false;
            }

            return true;
        }

        private bool EvaluateCondition(RpgOwnerKey owner, DialogueCondition condition)
        {
            bool actual;
            switch (condition.Kind)
            {
                case DialogueConditionKind.Always:
                    actual = true;
                    break;
                case DialogueConditionKind.ItemCount:
                    actual = _inventory != null && _inventory.GetItemCount(owner, condition.TargetId) >= condition.RequiredQuantity;
                    break;
                case DialogueConditionKind.DialogueFlag:
                    actual = HasDialogueFlag(owner, condition.TargetId);
                    break;
                case DialogueConditionKind.QuestStatus:
                    actual = EvaluateQuestStatus(owner, condition);
                    break;
                case DialogueConditionKind.Custom:
                    actual = _customConditionResolver != null && _customConditionResolver.Evaluate(owner, condition);
                    break;
                case DialogueConditionKind.SkillUnlocked:
                case DialogueConditionKind.ProjectFlag:
                case DialogueConditionKind.Faction:
                default:
                    actual = _customConditionResolver != null && _customConditionResolver.Evaluate(owner, condition);
                    break;
            }

            return actual == condition.Expected;
        }

        private bool EvaluateQuestStatus(RpgOwnerKey owner, DialogueCondition condition)
        {
            if (_quests == null)
                return false;

            QuestStatus expectedStatus;
            switch (Normalize(condition.ComparisonValue))
            {
                case "active":
                    expectedStatus = QuestStatus.Active;
                    break;
                case "completed":
                    expectedStatus = QuestStatus.Completed;
                    break;
                case "not-started":
                    expectedStatus = QuestStatus.NotStarted;
                    break;
                default:
                    expectedStatus = QuestStatus.Completed;
                    break;
            }

            return _quests.GetProgress(owner, condition.TargetId).Status == expectedStatus;
        }

        private bool ApplyEffects(RpgOwnerKey owner, DialogueEffect[] effects, out string issue)
        {
            DialogueEffect[] safeEffects = effects ?? Array.Empty<DialogueEffect>();
            for (int i = 0; i < safeEffects.Length; i++)
            {
                if (!ApplyEffect(owner, safeEffects[i], out issue))
                    return false;
            }

            issue = string.Empty;
            return true;
        }

        private bool ApplyEffect(RpgOwnerKey owner, DialogueEffect effect, out string issue)
        {
            switch (effect.Kind)
            {
                case DialogueEffectKind.SetDialogueFlag:
                    SetDialogueFlag(owner, effect.TargetId, effect.BoolValue);
                    issue = string.Empty;
                    return true;
                case DialogueEffectKind.StartQuest:
                    return TryStartQuestEffect(owner, effect, out issue);
                case DialogueEffectKind.ReportQuestObjective:
                    return TryReportQuestObjectiveEffect(owner, effect, out issue);
                case DialogueEffectKind.GrantItem:
                    if (_inventory == null)
                    {
                        issue = "Inventory service is required to grant dialogue item rewards.";
                        return false;
                    }

                    return _inventory.TryAddItem(owner, effect.TargetId, effect.Quantity, out issue);
                case DialogueEffectKind.GrantExperience:
                    _progression?.AddExperience(owner, effect.Quantity);
                    issue = string.Empty;
                    return true;
                case DialogueEffectKind.GrantSkillPoints:
                    _progression?.GrantSkillPoints(owner, effect.Quantity);
                    issue = string.Empty;
                    return true;
                case DialogueEffectKind.OpenVendor:
                case DialogueEffectKind.OpenTrainer:
                case DialogueEffectKind.OpenPortal:
                case DialogueEffectKind.CustomEvent:
                    return TrySendCustomEffect(owner, effect, out issue);
                default:
                    issue = string.Empty;
                    return true;
            }
        }

        private bool TryStartQuestEffect(RpgOwnerKey owner, DialogueEffect effect, out string issue)
        {
            if (_quests == null)
            {
                issue = "Quest service is required to start dialogue quests.";
                return false;
            }

            if (!_questRegistry.TryGetValue(effect.TargetId, out IQuestDefinition quest))
            {
                issue = $"Dialogue quest `{effect.TargetId}` is not registered.";
                return false;
            }

            return _quests.TryStartQuest(owner, quest, out issue);
        }

        private bool TryReportQuestObjectiveEffect(RpgOwnerKey owner, DialogueEffect effect, out string issue)
        {
            if (_quests == null)
            {
                issue = "Quest service is required to report dialogue quest progress.";
                return false;
            }

            if (!_questRegistry.TryGetValue(effect.TargetId, out IQuestDefinition quest))
            {
                issue = $"Dialogue quest `{effect.TargetId}` is not registered.";
                return false;
            }

            return _quests.ReportObjectiveProgress(owner, quest, effect.SecondaryTargetId, effect.Quantity, out _, out issue);
        }

        private bool TrySendCustomEffect(RpgOwnerKey owner, DialogueEffect effect, out string issue)
        {
            if (_customEffectSink == null)
            {
                issue = string.Empty;
                return true;
            }

            return _customEffectSink.TryApply(owner, effect, out issue);
        }

        private HashSet<string> GetOrCreateFlags(RpgOwnerKey owner)
        {
            if (_dialogueFlags.TryGetValue(owner, out HashSet<string> flags))
                return flags;

            flags = new HashSet<string>(StringComparer.Ordinal);
            _dialogueFlags[owner] = flags;
            return flags;
        }

        private string[] GetFlagSnapshot(RpgOwnerKey owner)
        {
            if (!owner.IsValid || !_dialogueFlags.TryGetValue(owner, out HashSet<string> flags))
                return Array.Empty<string>();

            string[] snapshot = new string[flags.Count];
            flags.CopyTo(snapshot);
            Array.Sort(snapshot, StringComparer.Ordinal);
            return snapshot;
        }

        private static DialogueSessionState ToState(RpgOwnerKey owner, DialogueSessionRecord record)
        {
            return new DialogueSessionState(owner, record.NpcId, record.GraphId, record.CurrentNodeId, record.Ended);
        }

        private static bool ValidateOwnerNpcGraph(RpgOwnerKey owner, INpcProfile npc, IDialogueGraph graph, out string issue)
        {
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (npc == null || string.IsNullOrWhiteSpace(npc.NpcId))
            {
                issue = "A valid NPC profile is required.";
                return false;
            }

            if (graph == null || string.IsNullOrWhiteSpace(graph.GraphId) || string.IsNullOrWhiteSpace(graph.StartNodeId))
            {
                issue = "A valid dialogue graph is required.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class DialogueSessionRecord
        {
            public DialogueSessionRecord(string npcId, string graphId, string currentNodeId, bool ended)
            {
                NpcId = npcId;
                GraphId = graphId;
                CurrentNodeId = currentNodeId;
                Ended = ended;
            }

            public string NpcId { get; }
            public string GraphId { get; }
            public string CurrentNodeId { get; set; }
            public bool Ended { get; set; }
        }
    }
}
