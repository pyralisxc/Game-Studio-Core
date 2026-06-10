using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Contracts.Rpg;

namespace NeonBlack.Gameplay.Core.Rpg
{
    [AuthoringContract(
        Capability = AuthoringCapability.Stats | AuthoringCapability.Session,
        ModuleId = "rpg.skilltree",
        Lane = "RPG",
        RequiredInterfaces = new[] { typeof(ISkillTree) },
        FirstProof = "Unlock a skill and verify that its stat modifiers are applied to the character.",
        NativeSetup = new[]
        {
            "create SkillTree assets",
            "link SkillNodes to StatModifiers or Abilities",
            "assign skill tree to ProgressionService"
        }
    )]
    public sealed class SkillTreeService : ISkillTreeService
{
        private const string SkillModifierSourcePrefix = "skill:";
        private readonly IProgressionService _progression;
        private readonly Dictionary<RpgOwnerKey, Dictionary<string, int>> _unlockCounts =
            new Dictionary<RpgOwnerKey, Dictionary<string, int>>();

        public SkillTreeService(IProgressionService progression)
        {
            _progression = progression;
        }

        public bool TryUnlock(RpgOwnerKey owner, ISkillTree tree, string nodeId, out string issue)
        {
            string normalizedNodeId = Normalize(nodeId);
            if (!owner.IsValid)
            {
                issue = "A valid RPG owner is required.";
                return false;
            }

            if (tree == null || !tree.TryGetNode(normalizedNodeId, out SkillNode node))
            {
                issue = $"Skill node `{normalizedNodeId}` could not be found.";
                return false;
            }

            if (!node.Repeatable && IsUnlocked(owner, normalizedNodeId))
            {
                issue = $"Skill node `{normalizedNodeId}` is already unlocked.";
                return false;
            }

            string[] prerequisites = node.PrerequisiteNodeIds;
            for (int i = 0; i < prerequisites.Length; i++)
            {
                if (!IsUnlocked(owner, prerequisites[i]))
                {
                    issue = $"Skill node `{normalizedNodeId}` requires prerequisite `{prerequisites[i]}`.";
                    return false;
                }
            }

            int cost = node.Cost;
            if (cost > 0)
            {
                if (_progression == null)
                {
                    issue = "A progression service is required to spend skill points.";
                    return false;
                }

                if (!_progression.TrySpendSkillPoints(owner, cost, out issue))
                    return false;
            }

            Dictionary<string, int> ownerUnlocks = GetOrCreateUnlocks(owner);
            ownerUnlocks[normalizedNodeId] = ownerUnlocks.TryGetValue(normalizedNodeId, out int current) ? current + 1 : 1;
            issue = string.Empty;
            return true;
        }

        public bool IsUnlocked(RpgOwnerKey owner, string nodeId)
        {
            return GetUnlockCount(owner, nodeId) > 0;
        }

        public int GetUnlockCount(RpgOwnerKey owner, string nodeId)
        {
            string normalizedNodeId = Normalize(nodeId);
            return owner.IsValid
                && !string.IsNullOrEmpty(normalizedNodeId)
                && _unlockCounts.TryGetValue(owner, out Dictionary<string, int> ownerUnlocks)
                && ownerUnlocks.TryGetValue(normalizedNodeId, out int count)
                    ? count
                    : 0;
        }

        public RpgSkillUnlockSnapshot[] GetSnapshot(RpgOwnerKey owner)
        {
            if (!owner.IsValid || !_unlockCounts.TryGetValue(owner, out Dictionary<string, int> ownerUnlocks))
                return System.Array.Empty<RpgSkillUnlockSnapshot>();

            List<string> nodeIds = new List<string>(ownerUnlocks.Keys);
            nodeIds.Sort(System.StringComparer.Ordinal);
            List<RpgSkillUnlockSnapshot> snapshot = new List<RpgSkillUnlockSnapshot>(nodeIds.Count);
            for (int i = 0; i < nodeIds.Count; i++)
                snapshot.Add(new RpgSkillUnlockSnapshot(nodeIds[i], ownerUnlocks[nodeIds[i]]));

            return snapshot.ToArray();
        }

        public void RestoreSnapshot(RpgOwnerKey owner, RpgSkillUnlockSnapshot[] snapshot)
        {
            if (!owner.IsValid)
                return;

            Dictionary<string, int> ownerUnlocks = GetOrCreateUnlocks(owner);
            ownerUnlocks.Clear();

            RpgSkillUnlockSnapshot[] safeSnapshot = snapshot ?? System.Array.Empty<RpgSkillUnlockSnapshot>();
            for (int i = 0; i < safeSnapshot.Length; i++)
            {
                if (safeSnapshot[i].IsValid)
                    ownerUnlocks[safeSnapshot[i].NodeId] = safeSnapshot[i].Count;
            }
        }

        public void ApplySkillEffects(RpgOwnerKey owner, ISkillTree tree, StatSheet statSheet)
        {
            if (!owner.IsValid || tree == null || statSheet == null)
                return;

            statSheet.RemoveModifiersFromSource(SkillModifierSourcePrefix + owner);
            if (!_unlockCounts.TryGetValue(owner, out Dictionary<string, int> ownerUnlocks))
                return;

            string sourceId = SkillModifierSourcePrefix + owner;
            foreach (KeyValuePair<string, int> unlock in ownerUnlocks)
            {
                if (!tree.TryGetNode(unlock.Key, out SkillNode node))
                    continue;

                StatModifier[] modifiers = node.StatModifiers;
                for (int unlockIndex = 0; unlockIndex < unlock.Value; unlockIndex++)
                {
                    for (int i = 0; i < modifiers.Length; i++)
                    {
                        if (modifiers[i].IsValid)
                            statSheet.AddModifier(new StatModifier(modifiers[i].StatId, modifiers[i].Value, sourceId));
                    }
                }
            }
        }

        private Dictionary<string, int> GetOrCreateUnlocks(RpgOwnerKey owner)
        {
            if (_unlockCounts.TryGetValue(owner, out Dictionary<string, int> ownerUnlocks))
                return ownerUnlocks;

            ownerUnlocks = new Dictionary<string, int>();
            _unlockCounts[owner] = ownerUnlocks;
            return ownerUnlocks;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
