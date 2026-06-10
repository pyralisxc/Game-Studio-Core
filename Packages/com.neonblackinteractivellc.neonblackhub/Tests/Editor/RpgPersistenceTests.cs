using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using NeonBlack.Gameplay.Core.Rpg;

namespace NeonBlack.Gameplay.Tests.Editor
{
    [TestFixture]
    public class RpgPersistenceTests
    {
        private string _testPath;
        private RpgOwnerKey _testOwner;

        [SetUp]
        public void SetUp()
        {
            _testPath = Path.Combine(Application.persistentDataPath, "Saves", "RpgTests");
            if (Directory.Exists(_testPath))
                Directory.Delete(_testPath, true);
            Directory.CreateDirectory(_testPath);
            
            _testOwner = new RpgOwnerKey(RpgOwnerKind.Pawn, "test-pawn-001");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testPath))
                Directory.Delete(_testPath, true);
        }

        [Test]
        public void LocalRpgPersistence_SavesAndLoadsCorrectly()
        {
            var service = new LocalRpgPersistenceService();
            var data = CreateTestData(_testOwner);

            service.Save(data);
            Assert.IsTrue(service.HasSaveData(_testOwner), "Save file should exist.");

            var loaded = service.Load(_testOwner);
            Assert.IsNotNull(loaded, "Loaded data should not be null.");
            Assert.AreEqual(data.Owner.StableId, loaded.Owner.StableId, "Owner ID should match.");
            Assert.AreEqual(data.Progression.Level, loaded.Progression.Level, "Level should match.");
            Assert.AreEqual(data.Inventory.Length, loaded.Inventory.Length, "Inventory size should match.");
        }

        [Test]
        public void LocalRpgPersistence_DeletesCorrectly()
        {
            var service = new LocalRpgPersistenceService();
            var data = CreateTestData(_testOwner);

            service.Save(data);
            Assert.IsTrue(service.HasSaveData(_testOwner));

            service.DeleteSaveData(_testOwner);
            Assert.IsFalse(service.HasSaveData(_testOwner), "Save file should be deleted.");
        }

        [Test]
        public void BinaryRpgPersistence_SavesAndLoadsCorrectly()
        {
            var service = new BinaryRpgPersistenceService();
            var data = CreateTestData(_testOwner);

            service.Save(data);
            Assert.IsTrue(service.HasSaveData(_testOwner), "Binary save file should exist.");

            var loaded = service.Load(_testOwner);
            Assert.IsNotNull(loaded, "Loaded binary data should not be null.");
            Assert.AreEqual(data.Owner.StableId, loaded.Owner.StableId, "Owner ID should match.");
            Assert.AreEqual(data.Progression.Level, loaded.Progression.Level, "Level should match.");
            Assert.AreEqual(data.Inventory.Length, loaded.Inventory.Length, "Inventory size should match.");
            Assert.AreEqual(data.Quests[0].QuestId, loaded.Quests[0].QuestId, "Quest ID should match.");
            Assert.AreEqual(data.Dialogue.Flags[0], loaded.Dialogue.Flags[0], "Dialogue flag should match.");
        }

        [Test]
        public void BinaryRpgPersistence_DeletesCorrectly()
        {
            var service = new BinaryRpgPersistenceService();
            var data = CreateTestData(_testOwner);

            service.Save(data);
            Assert.IsTrue(service.HasSaveData(_testOwner));

            service.DeleteSaveData(_testOwner);
            Assert.IsFalse(service.HasSaveData(_testOwner), "Binary save file should be deleted.");
        }

        private RpgOwnerSaveData CreateTestData(RpgOwnerKey owner)
        {
            return new RpgOwnerSaveData(
                owner,
                RpgOwnerSaveData.CurrentSchemaVersion,
                new RpgProgressionSnapshot(100, 5, 2),
                new[] { new RpgInventoryItemSnapshot("potion_hp", 3) },
                new[] { new RpgEquipmentSnapshot("main_hand", "iron_sword") },
                new[] { new RpgQuestSnapshot("tutorial_01", QuestStatus.Active, false, null) },
                new[] { new RpgSkillUnlockSnapshot("slash_upgrade", 1) },
                new RpgDialogueSnapshot(new[] { "met_elder" }, default),
                new RpgHubReturnSnapshot("hub_city", "Scene_City", "spawn_main", "", ""));
        }
    }

    // Stub for QuestStatus if missing in context, but I should check where it is.
}