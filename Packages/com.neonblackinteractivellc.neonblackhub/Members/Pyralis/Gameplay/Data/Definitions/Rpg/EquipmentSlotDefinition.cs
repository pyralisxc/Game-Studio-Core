using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;

namespace NeonBlack.Gameplay.Data.Definitions.Rpg
{
    [AuthoringContract(
        ModuleId = "rpg.equipment.slot",
        Capability = AuthoringCapability.Inventory,
        Lane = "RPG",
        AssignmentFields = new[] { nameof(slotId), nameof(displayName), nameof(slotFamily) },
        FirstProof = "Proof that the equipment slot has a valid id and display properties."
    )]
    [CreateAssetMenu(menuName = "NeonBlack/RPG/Equipment Slot", fileName = "EquipmentSlotDefinition")]
    public class EquipmentSlotDefinition : ScriptableObject, IEquipmentSlot, IRuntimeValidationProvider
    {
        public IEnumerable<string> GetRuntimeValidationIssues()
        {
            return GetValidationIssues();
        }

        public string slotId = "slot.new";
        public string displayName = "New Slot";
        public string slotFamily = "General";
        public string SlotId => string.IsNullOrWhiteSpace(slotId) ? string.Empty : slotId.Trim();

        [TextArea(2, 5)]
        public string notes = string.Empty;

        public void Sanitize()
        {
            slotId = !string.IsNullOrWhiteSpace(slotId) ? slotId.Trim() : slotId;
            displayName = !string.IsNullOrWhiteSpace(displayName) ? displayName.Trim() : slotId;
            slotFamily = !string.IsNullOrWhiteSpace(slotFamily) ? slotFamily.Trim() : "General";
        }

        public List<string> GetValidationIssues()
        {
            List<string> issues = new List<string>();

            if (string.IsNullOrWhiteSpace(slotId))
                issues.Add("Equipment slot id is required.");

            if (string.IsNullOrWhiteSpace(displayName))
                issues.Add("Display name is required.");

            if (string.IsNullOrWhiteSpace(slotFamily))
                issues.Add("Slot family is required so equipment tools can group slots.");

            return issues;
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
