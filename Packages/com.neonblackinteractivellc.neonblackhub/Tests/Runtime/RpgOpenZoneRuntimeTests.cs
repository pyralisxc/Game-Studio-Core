using System.Linq;
using NeonBlack.Gameplay.Core.Rpg;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public sealed class RpgOpenZoneRuntimeTests
    {
        [Test]
        public void OpenZoneService_CaptureAndRestore_PreservesTravelFlagsAndPerZoneState()
        {
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOpenZoneService service = new RpgOpenZoneService();
            RpgZoneDefinition meadow = new RpgZoneDefinition(
                "zone.meadow",
                "Meadow",
                "scene.meadow",
                RpgZoneResetPolicy.CampaignPersistent,
                new[] { "entrance.hub" },
                new[] { "exit.cave" });

            Assert.That(service.RegisterZone(meadow), Is.True);
            Assert.That(service.EnterZone(owner, "zone.meadow", "entrance.hub", "hub.rpg-proof", out string issue), Is.True, issue);
            service.SetZoneFlag(owner, "zone.meadow", "flag.bridge-lowered", true);
            service.SetEncounterState(owner, "zone.meadow", "encounter.bandits", RpgZoneEntityStatus.Cleared);
            service.SetResourceState(owner, "zone.meadow", "resource.ore-01", 0, true);
            service.SetPickupState(owner, "zone.meadow", "pickup.chest-01", RpgZoneEntityStatus.Collected);
            service.SetNpcState(owner, "zone.meadow", "npc.elder", "spawn.market", false, "dialogue.after-quest");
            Assert.That(service.ExitZone(owner, "exit.cave", "zone.cave", out issue), Is.True, issue);

            RpgOpenZoneSnapshot snapshot = service.GetSnapshot(owner);
            RpgOpenZoneService restored = new RpgOpenZoneService();
            Assert.That(restored.RegisterZone(meadow), Is.True);
            restored.RestoreSnapshot(owner, snapshot);

            RpgZoneRuntimeState state = restored.GetZoneState(owner, "zone.meadow");
            Assert.That(restored.GetTravelSnapshot(owner).CurrentZoneId, Is.EqualTo("zone.cave"));
            Assert.That(restored.GetTravelSnapshot(owner).PreviousZoneId, Is.EqualTo("zone.meadow"));
            Assert.That(restored.GetTravelSnapshot(owner).LastExitId, Is.EqualTo("exit.cave"));
            Assert.That(restored.GetTravelSnapshot(owner).ReturnHubId, Is.EqualTo("hub.rpg-proof"));
            Assert.That(state.HasFlag("flag.bridge-lowered"), Is.True);
            Assert.That(state.GetEncounterStatus("encounter.bandits"), Is.EqualTo(RpgZoneEntityStatus.Cleared));
            Assert.That(state.GetResource("resource.ore-01").Quantity, Is.EqualTo(0));
            Assert.That(state.GetResource("resource.ore-01").Depleted, Is.True);
            Assert.That(state.GetPickupStatus("pickup.chest-01"), Is.EqualTo(RpgZoneEntityStatus.Collected));
            Assert.That(state.GetNpc("npc.elder").Active, Is.False);
            Assert.That(state.GetNpc("npc.elder").SpawnPointId, Is.EqualTo("spawn.market"));
            Assert.That(state.GetNpc("npc.elder").DialogueStateId, Is.EqualTo("dialogue.after-quest"));
        }

        [Test]
        public void OpenZoneService_ResetForNewRun_ClearsRunScopedZonesAndKeepsCampaignZones()
        {
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOpenZoneService service = new RpgOpenZoneService();
            Assert.That(service.RegisterZone(new RpgZoneDefinition("zone.campaign", "Campaign", "scene.campaign", RpgZoneResetPolicy.CampaignPersistent)), Is.True);
            Assert.That(service.RegisterZone(new RpgZoneDefinition("zone.run", "Run", "scene.run", RpgZoneResetPolicy.ResetOnRun)), Is.True);

            service.SetPickupState(owner, "zone.campaign", "pickup.story-cape", RpgZoneEntityStatus.Collected);
            service.SetPickupState(owner, "zone.run", "pickup.temp-cache", RpgZoneEntityStatus.Collected);

            service.Reset(owner, RpgZoneResetScope.NewRun);

            Assert.That(service.GetZoneState(owner, "zone.campaign").GetPickupStatus("pickup.story-cape"), Is.EqualTo(RpgZoneEntityStatus.Collected));
            Assert.That(service.GetZoneState(owner, "zone.run").GetPickupStatus("pickup.temp-cache"), Is.EqualTo(RpgZoneEntityStatus.Unknown));
        }

        [Test]
        public void RpgOwnerSaveData_CaptureAndApply_IncludesOpenZoneStateWhenProvided()
        {
            RpgOwnerKey owner = new RpgOwnerKey(RpgOwnerKind.Participant, "seat-1");
            RpgOpenZoneService zones = new RpgOpenZoneService();
            Assert.That(zones.RegisterZone(new RpgZoneDefinition("zone.meadow", "Meadow", "scene.meadow", RpgZoneResetPolicy.CampaignPersistent)), Is.True);
            Assert.That(zones.EnterZone(owner, "zone.meadow", "entrance.hub", "hub.rpg-proof", out _), Is.True);
            zones.SetEncounterState(owner, "zone.meadow", "encounter.bandits", RpgZoneEntityStatus.Cleared);

            RpgOwnerSaveData save = RpgOwnerSaveData.Capture(owner, null, null, null, null, null, null, default, zones);

            RpgOpenZoneService restored = new RpgOpenZoneService();
            Assert.That(restored.RegisterZone(new RpgZoneDefinition("zone.meadow", "Meadow", "scene.meadow", RpgZoneResetPolicy.CampaignPersistent)), Is.True);
            save.ApplyTo(null, null, null, null, null, null, null, restored);

            Assert.That(save.OpenZones.Zones.Select(zone => zone.ZoneId), Does.Contain("zone.meadow"));
            Assert.That(restored.GetZoneState(owner, "zone.meadow").GetEncounterStatus("encounter.bandits"), Is.EqualTo(RpgZoneEntityStatus.Cleared));
        }
    }
}
