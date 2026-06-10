using System;
using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Rpg;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.hub",
        Capability = AuthoringCapability.Dialogue | AuthoringCapability.Puzzle,
        Relevance = "Handles RPG hub interactions, including NPC dialogue, quest triggers, and scene navigation.",
        Lane = "RPG",
        RequiredInterfaces = new[] { typeof(IHubDefinition), typeof(IHubConditionResolver), typeof(IHubEffectSink) },
        NativeSetup = new[]
        {
            "create HubDefinition assets",
            "configure HubInteractables",
            "place HubInteractionSceneController in scene"
        },
        FirstProof = "Interact with an NPC in the hub and verify the dialogue or interaction flow begins."
    )]
    public sealed class HubInteractionService : IHubInteractionService
{
        private readonly IInventoryService _inventory;
        private readonly IQuestService _quests;
        private readonly ISkillTreeService _skills;
        private readonly IDialogueService _dialogue;
        private readonly IHubConditionResolver _customConditionResolver;
        private readonly IHubEffectSink _customEffectSink;
        private readonly Dictionary<string, IQuestDefinition> _questRegistry = new Dictionary<string, IQuestDefinition>(StringComparer.Ordinal);
        private readonly Dictionary<string, ISkillTree> _skillTreeRegistry = new Dictionary<string, ISkillTree>(StringComparer.Ordinal);

        public HubInteractionService(
            IInventoryService inventory = null,
            IQuestService quests = null,
            ISkillTreeService skills = null,
            IDialogueService dialogue = null,
            IHubConditionResolver customConditionResolver = null,
            IHubEffectSink customEffectSink = null)
        {
            _inventory = inventory;
            _quests = quests;
            _skills = skills;
            _dialogue = dialogue;
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

        public bool RegisterSkillTree(string treeId, ISkillTree tree)
        {
            string normalizedTreeId = Normalize(treeId);
            if (string.IsNullOrEmpty(normalizedTreeId) || tree == null)
                return false;

            _skillTreeRegistry[normalizedTreeId] = tree;
            return true;
        }

        public HubInteractionResult[] GetAvailableInteractions(RpgOwnerKey owner, IHubDefinition hub)
        {
            if (!ValidateOwnerHub(owner, hub, out string issue))
                return new[] { HubInteractionResult.Invalid(issue) };

            List<HubInteractionResult> results = new List<HubInteractionResult>();
            HubInteractable[] interactables = hub.Interactables ?? Array.Empty<HubInteractable>();
            for (int i = 0; i < interactables.Length; i++)
            {
                HubInteractionResult result = BuildAvailabilityResult(owner, hub, interactables[i]);
                if (result.Status != HubInteractionStatus.Hidden)
                    results.Add(result);
            }

            results.Sort((left, right) => left.Prompt.InteractableId.CompareTo(right.Prompt.InteractableId));
            return results.ToArray();
        }

        public HubInteractionResult SelectInteraction(RpgOwnerKey owner, IHubDefinition hub, string interactableId)
        {
            if (!ValidateOwnerHub(owner, hub, out string issue))
                return HubInteractionResult.Invalid(issue);

            if (!hub.TryGetInteractable(interactableId, out HubInteractable interactable))
                return HubInteractionResult.Invalid($"Hub interactable `{Normalize(interactableId)}` could not be found.");

            HubInteractionResult availability = BuildAvailabilityResult(owner, hub, interactable);
            if (availability.Status == HubInteractionStatus.Hidden || availability.Status == HubInteractionStatus.Locked || availability.Status == HubInteractionStatus.Invalid)
                return availability;

            if (!ApplyEffects(owner, interactable, out issue))
                return HubInteractionResult.Invalid(issue);

            PlayerPanelRoute panelRoute = ResolvePanelRoute(interactable);
            string sceneId = ResolveSceneId(interactable);
            string dialogueGraphId = ResolveDialogueGraphId(interactable);
            string npcId = ResolveNpcId(interactable);
            HubNotificationPayload[] notifications = string.IsNullOrWhiteSpace(interactable.NotificationText)
                ? Array.Empty<HubNotificationPayload>()
                : new[] { new HubNotificationPayload(interactable.DisplayName, interactable.NotificationText, interactable.IconId, "info", 2.5f) };

            return new HubInteractionResult(
                HubInteractionStatus.Selected,
                string.Empty,
                BuildPrompt(owner, hub, interactable, locked: false),
                panelRoute,
                sceneId,
                dialogueGraphId,
                npcId,
                notifications);
        }

        private HubInteractionResult BuildAvailabilityResult(RpgOwnerKey owner, IHubDefinition hub, HubInteractable interactable)
        {
            bool available = AreConditionsMet(owner, interactable.Conditions);
            if (available)
            {
                return new HubInteractionResult(
                    HubInteractionStatus.Available,
                    string.Empty,
                    BuildPrompt(owner, hub, interactable, locked: false),
                    ResolvePanelRoute(interactable),
                    ResolveSceneId(interactable),
                    ResolveDialogueGraphId(interactable),
                    ResolveNpcId(interactable),
                    Array.Empty<HubNotificationPayload>());
            }

            if (interactable.Availability == HubInteractionAvailability.HiddenUntilAvailable)
            {
                return new HubInteractionResult(
                    HubInteractionStatus.Hidden,
                    string.Empty,
                    BuildPrompt(owner, hub, interactable, locked: true),
                    PlayerPanelRoute.None,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    Array.Empty<HubNotificationPayload>());
            }

            return new HubInteractionResult(
                HubInteractionStatus.Locked,
                string.Empty,
                BuildPrompt(owner, hub, interactable, locked: true),
                PlayerPanelRoute.None,
                string.Empty,
                string.Empty,
                string.Empty,
                Array.Empty<HubNotificationPayload>());
        }

        private bool AreConditionsMet(RpgOwnerKey owner, HubInteractionCondition[] conditions)
        {
            HubInteractionCondition[] safeConditions = conditions ?? Array.Empty<HubInteractionCondition>();
            for (int i = 0; i < safeConditions.Length; i++)
            {
                if (!EvaluateCondition(owner, safeConditions[i]))
                    return false;
            }

            return true;
        }

        private bool EvaluateCondition(RpgOwnerKey owner, HubInteractionCondition condition)
        {
            bool actual;
            switch (condition.Kind)
            {
                case HubConditionKind.Always:
                    actual = true;
                    break;
                case HubConditionKind.ItemCount:
                    actual = _inventory != null && _inventory.GetItemCount(owner, condition.TargetId) >= condition.RequiredQuantity;
                    break;
                case HubConditionKind.QuestStatus:
                    actual = _quests != null && _quests.GetProgress(owner, condition.TargetId).Status == ParseQuestStatus(condition.ComparisonValue);
                    break;
                case HubConditionKind.SkillUnlocked:
                    actual = EvaluateSkillUnlocked(owner, condition);
                    break;
                case HubConditionKind.DialogueFlag:
                    actual = _dialogue != null && _dialogue.HasDialogueFlag(owner, condition.TargetId);
                    break;
                case HubConditionKind.Custom:
                case HubConditionKind.ProjectFlag:
                case HubConditionKind.Faction:
                default:
                    actual = _customConditionResolver != null && _customConditionResolver.Evaluate(owner, condition);
                    break;
            }

            return actual == condition.Expected;
        }

        private bool EvaluateSkillUnlocked(RpgOwnerKey owner, HubInteractionCondition condition)
        {
            if (_skills == null)
                return false;

            if (string.IsNullOrEmpty(condition.ComparisonValue))
                return _skills.GetUnlockCount(owner, condition.TargetId) >= condition.RequiredQuantity;

            return _skillTreeRegistry.TryGetValue(condition.ComparisonValue, out ISkillTree tree)
                && tree.TryGetNode(condition.TargetId, out _)
                && _skills.GetUnlockCount(owner, condition.TargetId) >= condition.RequiredQuantity;
        }

        private bool ApplyEffects(RpgOwnerKey owner, HubInteractable interactable, out string issue)
        {
            HubInteractionEffect[] effects = interactable.Effects ?? Array.Empty<HubInteractionEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                if (!ApplyEffect(owner, effects[i], out issue))
                    return false;
            }

            issue = string.Empty;
            return true;
        }

        private bool ApplyEffect(RpgOwnerKey owner, HubInteractionEffect effect, out string issue)
        {
            switch (effect.Kind)
            {
                case HubEffectKind.None:
                case HubEffectKind.OpenPanel:
                case HubEffectKind.StartDialogue:
                case HubEffectKind.NavigateScene:
                    issue = string.Empty;
                    return true;
                case HubEffectKind.SetDialogueFlag:
                    _dialogue?.SetDialogueFlag(owner, effect.TargetId, effect.BoolValue);
                    issue = string.Empty;
                    return true;
                case HubEffectKind.StartQuest:
                    return TryStartQuest(owner, effect, out issue);
                case HubEffectKind.ReportQuestObjective:
                    return TryReportQuestObjective(owner, effect, out issue);
                case HubEffectKind.GrantItem:
                    if (_inventory == null)
                    {
                        issue = "Inventory service is required to grant hub item rewards.";
                        return false;
                    }

                    return _inventory.TryAddItem(owner, effect.TargetId, effect.Quantity, out issue);
                case HubEffectKind.GrantExperience:
                case HubEffectKind.GrantSkillPoints:
                case HubEffectKind.CustomEvent:
                default:
                    if (_customEffectSink == null)
                    {
                        issue = string.Empty;
                        return true;
                    }

                    return _customEffectSink.TryApply(owner, effect, out issue);
            }
        }

        private bool TryStartQuest(RpgOwnerKey owner, HubInteractionEffect effect, out string issue)
        {
            if (_quests == null)
            {
                issue = "Quest service is required to start hub quests.";
                return false;
            }

            if (!_questRegistry.TryGetValue(effect.TargetId, out IQuestDefinition quest))
            {
                issue = $"Hub quest `{effect.TargetId}` is not registered.";
                return false;
            }

            return _quests.TryStartQuest(owner, quest, out issue);
        }

        private bool TryReportQuestObjective(RpgOwnerKey owner, HubInteractionEffect effect, out string issue)
        {
            if (_quests == null)
            {
                issue = "Quest service is required to report hub quest progress.";
                return false;
            }

            if (!_questRegistry.TryGetValue(effect.TargetId, out IQuestDefinition quest))
            {
                issue = $"Hub quest `{effect.TargetId}` is not registered.";
                return false;
            }

            return _quests.ReportObjectiveProgress(owner, quest, effect.SecondaryTargetId, effect.Quantity, out _, out issue);
        }

        private static PlayerPanelRoute ResolvePanelRoute(HubInteractable interactable)
        {
            if (interactable.PanelRoute != PlayerPanelRoute.None)
                return interactable.PanelRoute;

            HubInteractionEffect[] effects = interactable.Effects ?? Array.Empty<HubInteractionEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].Kind == HubEffectKind.OpenPanel && effects[i].PanelRoute != PlayerPanelRoute.None)
                    return effects[i].PanelRoute;
            }

            return interactable.Kind == HubInteractionKind.NPCDialogue ? PlayerPanelRoute.Dialogue : PlayerPanelRoute.None;
        }

        private static string ResolveSceneId(HubInteractable interactable)
        {
            if (!string.IsNullOrEmpty(interactable.SceneId))
                return interactable.SceneId;

            HubInteractionEffect[] effects = interactable.Effects ?? Array.Empty<HubInteractionEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].Kind == HubEffectKind.NavigateScene && !string.IsNullOrEmpty(effects[i].TargetId))
                    return effects[i].TargetId;
            }

            return string.Empty;
        }

        private static string ResolveDialogueGraphId(HubInteractable interactable)
        {
            if (!string.IsNullOrEmpty(interactable.DialogueGraphId))
                return interactable.DialogueGraphId;

            HubInteractionEffect[] effects = interactable.Effects ?? Array.Empty<HubInteractionEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].Kind == HubEffectKind.StartDialogue && !string.IsNullOrEmpty(effects[i].TargetId))
                    return effects[i].TargetId;
            }

            return string.Empty;
        }

        private static string ResolveNpcId(HubInteractable interactable)
        {
            if (!string.IsNullOrEmpty(interactable.NpcId))
                return interactable.NpcId;

            HubInteractionEffect[] effects = interactable.Effects ?? Array.Empty<HubInteractionEffect>();
            for (int i = 0; i < effects.Length; i++)
            {
                if (effects[i].Kind == HubEffectKind.StartDialogue && !string.IsNullOrEmpty(effects[i].SecondaryTargetId))
                    return effects[i].SecondaryTargetId;
            }

            return string.Empty;
        }

        private static HubPromptPayload BuildPrompt(RpgOwnerKey owner, IHubDefinition hub, HubInteractable interactable, bool locked)
        {
            return new HubPromptPayload(
                owner,
                hub.HubId,
                interactable.InteractableId,
                locked ? interactable.LockedPromptText : interactable.PromptText,
                interactable.IconId,
                locked,
                interactable.Priority);
        }

        private static bool ValidateOwnerHub(RpgOwnerKey owner, IHubDefinition hub, out string issue)
        {
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (hub == null || string.IsNullOrWhiteSpace(hub.HubId))
            {
                issue = "A valid hub definition is required.";
                return false;
            }

            issue = string.Empty;
            return true;
        }

        private static QuestStatus ParseQuestStatus(string value)
        {
            switch (Normalize(value))
            {
                case "active":
                    return QuestStatus.Active;
                case "completed":
                    return QuestStatus.Completed;
                case "not-started":
                    return QuestStatus.NotStarted;
                default:
                    return QuestStatus.Completed;
            }
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
