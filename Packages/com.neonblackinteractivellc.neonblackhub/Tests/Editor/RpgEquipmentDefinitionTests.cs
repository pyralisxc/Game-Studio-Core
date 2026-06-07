using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class RpgEquipmentDefinitionTests
    {
        [Test]
        public void EquipmentSlotDefinition_GetValidationIssues_RequiresStableId()
        {
            EquipmentSlotDefinition slot = ScriptableObject.CreateInstance<EquipmentSlotDefinition>();
            slot.slotId = string.Empty;
            slot.displayName = "Weapon";

            Assert.That(slot.GetValidationIssues().Any(issue => issue.Contains("Equipment slot id")), Is.True);

            Object.DestroyImmediate(slot);
        }

        [Test]
        public void EquippableItemDefinition_GetValidationIssues_RequiresAllowedSlot()
        {
            EquippableItemDefinition item = ScriptableObject.CreateInstance<EquippableItemDefinition>();
            item.itemId = "item.sword";
            item.displayName = "Sword";
            item.allowedSlotIds = System.Array.Empty<string>();

            Assert.That(item.GetValidationIssues().Any(issue => issue.Contains("allowed slot")), Is.True);

            Object.DestroyImmediate(item);
        }

        [Test]
        public void EquippableItemDefinition_GetValidationIssues_RejectsInvalidStatModifier()
        {
            EquippableItemDefinition item = ScriptableObject.CreateInstance<EquippableItemDefinition>();
            item.itemId = "item.cape";
            item.displayName = "Cape";
            item.allowedSlotIds = new[] { "slot.cape" };
            item.statModifiers = new[] { new StatModifierDefinition("", 2f) };

            Assert.That(item.GetValidationIssues().Any(issue => issue.Contains("Stat id")), Is.True);

            Object.DestroyImmediate(item);
        }

        [Test]
        public void EquippableItemDefinition_Sanitize_TrimsSlotsAndRemovesDuplicates()
        {
            EquippableItemDefinition item = ScriptableObject.CreateInstance<EquippableItemDefinition>();
            item.itemId = " item.cape ";
            item.displayName = " Cape ";
            item.allowedSlotIds = new[] { " slot.cape ", "", "slot.cape", "slot.back" };

            item.Sanitize();

            Assert.That(item.itemId, Is.EqualTo("item.cape"));
            Assert.That(item.displayName, Is.EqualTo("Cape"));
            Assert.That(item.allowedSlotIds, Is.EqualTo(new[] { "slot.cape", "slot.back" }));

            Object.DestroyImmediate(item);
        }
    }
}
