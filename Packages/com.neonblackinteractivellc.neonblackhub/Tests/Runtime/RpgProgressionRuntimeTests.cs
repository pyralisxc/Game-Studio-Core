using NeonBlack.Gameplay.Core.Rpg;
using NeonBlack.Gameplay.Data.Definitions.Rpg;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgProgressionRuntimeTests
    {
        [Test]
        public void RpgOwnerKey_UsesKindAndStableIdForEquality()
        {
            RpgOwnerKey first = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOwnerKey second = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOwnerKey different = new RpgOwnerKey(RpgOwnerKind.Actor, "seat-1");

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Is.Not.EqualTo(different));
            Assert.That(first.IsValid, Is.True);
        }

        [Test]
        public void StatSheet_ReturnsBaseValuePlusMatchingModifiers()
        {
            StatSheet sheet = new StatSheet();
            sheet.SetBaseValue("wisdom", 5f);
            sheet.AddModifier(new StatModifier("wisdom", 2f, "cape"));
            sheet.AddModifier(new StatModifier("strength", 10f, "sword"));

            Assert.That(sheet.GetValue("wisdom"), Is.EqualTo(7f));
            Assert.That(sheet.GetValue("strength"), Is.EqualTo(10f));
            Assert.That(sheet.GetValue("missing"), Is.EqualTo(0f));
        }

        [Test]
        public void StatSheet_RemoveModifiersFromSource_OnlyRemovesMatchingSource()
        {
            StatSheet sheet = new StatSheet();
            sheet.SetBaseValue("speed", 3f);
            sheet.AddModifier(new StatModifier("speed", 2f, "boots"));
            sheet.AddModifier(new StatModifier("speed", 5f, "spell"));

            int removed = sheet.RemoveModifiersFromSource("boots");

            Assert.That(removed, Is.EqualTo(1));
            Assert.That(sheet.GetValue("speed"), Is.EqualTo(8f));
        }

        [Test]
        public void ProgressionService_LevelsOwnerAndGrantsSkillPoints()
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 0, 100, 250 });
            curve.SetTestSkillPointGrants(new[] { 0, 1, 2 });

            ProgressionService service = new ProgressionService(curve);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            service.AddExperience(owner, 260);
            ProgressionState state = service.GetState(owner);

            Assert.That(state.Level, Is.EqualTo(3));
            Assert.That(state.Experience, Is.EqualTo(260));
            Assert.That(state.SkillPoints, Is.EqualTo(3));

            Object.DestroyImmediate(curve);
        }

        [Test]
        public void ProgressionService_DoesNotGrantSameLevelSkillPointsTwice()
        {
            ProgressionCurveDefinition curve = ScriptableObject.CreateInstance<ProgressionCurveDefinition>();
            curve.SetTestThresholds(new[] { 0, 100, 250 });
            curve.SetTestSkillPointGrants(new[] { 0, 1, 2 });
            ProgressionService service = new ProgressionService(curve);
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");

            service.AddExperience(owner, 120);
            service.AddExperience(owner, 10);
            ProgressionState state = service.GetState(owner);

            Assert.That(state.Level, Is.EqualTo(2));
            Assert.That(state.SkillPoints, Is.EqualTo(1));

            Object.DestroyImmediate(curve);
        }
    }
}
