using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Rpg;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        Category = "Inventory, Stats",
        CapabilityPath = "RPG/Inventory/Definitions/Equippable Item Definition",
        Surface = AuthoringSurface.Goal,
        Summary = "Extends standard items with equipment slots and stat modifiers.",
        RequiredFields = new[] { nameof(allowedSlotIds) },
        SetupSteps = new[] { "Add Allowed Slot Ids.", "Add Stat Modifiers." },
        SuccessChecks = new[] { "Verify the item can be equipped into the specified slots and correctly modifies stats." },
        Tags = new[] { "capability:Inventory", "capability:Stats", "runtime:CharacterPawnGameplay" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Equippable Item", fileName = "EquippableItemDefinition")]
    public class EquippableItemDefinition : ItemDefinition, IEquippableItem, IRuntimeValidationProvider
    {
        public new IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            return PyralisRuntimeValidationIssueUtility.FromLocalValidationMessages(GetValidationIssues(), this);
        }

        public string[] allowedSlotIds = System.Array.Empty<string>();
        public StatModifierDefinition[] statModifiers = System.Array.Empty<StatModifierDefinition>();
        public string ItemId => string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();

        public new void Sanitize()
        {
            base.Sanitize();

            allowedSlotIds = allowedSlotIds == null
                ? System.Array.Empty<string>()
                : allowedSlotIds.Select(slotId => string.IsNullOrWhiteSpace(slotId) ? string.Empty : slotId.Trim())
                    .Where(slotId => !string.IsNullOrWhiteSpace(slotId))
                    .Distinct()
                    .ToArray();

            if (statModifiers == null)
            {
                statModifiers = System.Array.Empty<StatModifierDefinition>();
                return;
            }

            for (int i = 0; i < statModifiers.Length; i++)
                statModifiers[i].Sanitize();
        }

        public new List<string> GetValidationIssues()
        {
            List<string> issues = base.GetValidationIssues();

            if (allowedSlotIds == null || allowedSlotIds.Length == 0 || allowedSlotIds.All(string.IsNullOrWhiteSpace))
                issues.Add("At least one allowed slot id is required for equippable items.");

            if (statModifiers == null)
                return issues;

            for (int i = 0; i < statModifiers.Length; i++)
            {
                if (!statModifiers[i].IsValid)
                    issues.Add($"Stat Modifiers[{i}] Stat id is required.");
            }

            return issues;
        }

        public bool CanEquipInSlot(string slotId)
        {
            string normalizedSlotId = string.IsNullOrWhiteSpace(slotId) ? string.Empty : slotId.Trim();
            if (string.IsNullOrEmpty(normalizedSlotId) || allowedSlotIds == null)
                return false;

            for (int i = 0; i < allowedSlotIds.Length; i++)
            {
                if (allowedSlotIds[i] == normalizedSlotId)
                    return true;
            }

            return false;
        }

        public StatModifier[] CreateStatModifiers(string sourceId)
        {
            if (statModifiers == null)
                return System.Array.Empty<StatModifier>();

            return statModifiers
                .Where(modifier => modifier.IsValid)
                .Select(modifier => new StatModifier(modifier.StatId, modifier.Value, sourceId))
                .ToArray();
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
