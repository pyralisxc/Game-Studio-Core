using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Presentation.Animation;
using NUnit.Framework;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class AuthoringContractsContractTests
    {
        private static readonly string GameplayRoot = Path.Combine(
            Application.dataPath,
            "..",
            "Packages",
            "com.neonblackinteractivellc.neonblackhub",
            "Members",
            "Pyralis",
            "Gameplay");

        [Test]
        public void AuthoringContracts_DoNotContainDuplicateStableIds()
        {
            Assert.That(ResolvedAuthoringContractRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);
        }

        [Test]
        public void RouteIntentFacts_CoverProjectWideWorldUpChoices()
        {
            string[] expectedIntentIds =
            {
                "intent.2d-side-view-action",
                "intent.2d-top-down-plane",
                "intent.2_5d-lane-arena",
                "intent.3d-space-action",
                "intent.tabletop-board-card",
                "intent.ui-menu-first",
                "intent.hybrid-custom-project"
            };

            for (int i = 0; i < expectedIntentIds.Length; i++)
            {
                PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find(expectedIntentIds[i]);
                Assert.That(fact, Is.Not.Null, expectedIntentIds[i]);
                Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.RouteIntent), expectedIntentIds[i]);
                Assert.That(fact.FirstProof, Is.Not.Empty, expectedIntentIds[i]);
            }
        }

        [Test]
        public void IntentAdvisor_AxiomsAndGoals_InfluenceProjectIntentReading()
        {
            PyralisAuthoringIntentModel sideView = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringCapability.Movement | AuthoringCapability.Combat,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityVertical));

            PyralisAuthoringIntentModel topDown = PyralisAuthoringIntentAdvisor.Build(
                new PyralisAuthoringIntentSelection(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.Combat,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone));

            Assert.That(sideView.Summary, Does.Contain("Active focus"));
            Assert.That(sideView.Recommendations[0].Fact.StableId, Is.Not.EqualTo(topDown.Recommendations[0].Fact.StableId));
            Assert.That(topDown.Recommendations.Select(row => row.Fact.StableId), Does.Contain("intent.2d-top-down-plane"));
        }

        [Test]
        public void AuthoringContracts_CanFindContractByModuleId()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.StableId, Is.EqualTo("feature.actor.traversal.topdown-hop"));
        }

        [Test]
        public void TopDownHopContract_DeclaresProfileRuntimeLanesAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(TopDownHopProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Core.Contracts.IActorGameplayActionReceiver"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.IsExplicitlyUnsupported(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.AuthoringLane, Is.EqualTo("Traversal"));
            Assert.That(contract.ConsumedActionRoles, Does.Contain("Jump"));
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void TopDownHopContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.traversal.topdown-hop");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(TopDownHopProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorGameplayActionReceiver"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.UnsupportedLaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.GoalTags, Does.Contain("Traversal"));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));
            Assert.That(fact.FirstProof, Is.EqualTo("proof.1p-pawn-movement"));
        }

        [Test]
        public void AuthoringContracts_InferProofTargetsFromClearDependencies()
        {
            ResolvedAuthoringContract profileContract = ResolvedAuthoringContractRegistry.FindByType(typeof(TopDownHopProfile));

            Assert.That(profileContract, Is.Not.Null);
            Assert.That(profileContract.FirstProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));

            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find(profileContract.StableId);
            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.FirstProof, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.1p-pawn-movement"));
        }

        [Test]
        public void Traversal3DContract_DeclaresProfileRuntimeLanesAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.3d");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(PawnTraversalProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Traversal.IActorTraversalFeature"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.IsExplicitlyUnsupported(ActorPresentationMode.Sprite2D), Is.True);
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.npc-enemy-behavior"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void Traversal3DContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.traversal.3d");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(PawnTraversalProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorTraversalFeature"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.UnsupportedLaneTags, Does.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.npc-enemy-behavior"));
        }

        [Test]
        public void InteractionContract_DeclaresProfileRuntimeAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.interaction");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(InteractionFeatureProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Interaction.IActorInteractionFeature"));
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.action-selection"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void InteractionContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.interaction");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(InteractionFeatureProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorInteractionFeature"));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.action-selection"));
        }

        [Test]
        public void EnemyReactionContract_DeclaresProfileRuntimeAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("enemy.reaction");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(EnemyReactionProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Enemies.IEnemyReactionState"));
            Assert.That(contract.StableId, Is.EqualTo("feature.enemy.reaction"));
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.npc-enemy-behavior"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.False);
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void EnemyReactionContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.enemy.reaction");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(EnemyReactionProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IEnemyReactionState"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.LaneTags, Does.Not.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.npc-enemy-behavior"));
            Assert.That(fact.FirstProof, Is.EqualTo("proof.npc-enemy-behavior"));
        }

        [Test]
        public void EnemyAmbientContract_DeclaresProfileRuntimeAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("enemy.ambient");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(EnemyAmbientFeatureProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.StableId, Is.EqualTo("feature.enemy.ambient"));
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.npc-enemy-behavior"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.False);
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void EnemyAmbientContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.enemy.ambient");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(EnemyAmbientFeatureProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.LaneTags, Does.Not.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.npc-enemy-behavior"));
            Assert.That(fact.FirstProof, Is.EqualTo("proof.npc-enemy-behavior"));
        }

        [Test]
        public void Pickups2DContract_DeclaresProfileRuntimeLanesAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.pickups.2d");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(PickupFeatureProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IActorInteractionHandler"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.True);
            Assert.That(contract.IsExplicitlyUnsupported(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.IsExplicitlyUnsupported(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.custom-object-effect"));
            Assert.That(contract.ConsumedActionRoles, Does.Contain("Interact"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void Pickups2DContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.pickups.2d");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(PickupFeatureProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorInteractionHandler"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.UnsupportedLaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.UnsupportedLaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.custom-object-effect"));
            Assert.That(fact.FirstProof, Is.EqualTo("proof.custom-object-effect"));
        }

        [Test]
        public void Pickups3DContract_DeclaresProfileRuntimeLanesAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.pickups.3d");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(PickupFeatureProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IActorInteractionHandler"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.IsExplicitlyUnsupported(ActorPresentationMode.Sprite2D), Is.True);
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.custom-object-effect"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void Pickups3DContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.pickups.3d");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(PickupFeatureProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorInteractionHandler"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.UnsupportedLaneTags, Does.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.custom-object-effect"));
            Assert.That(fact.FirstProof, Is.EqualTo("proof.custom-object-effect"));
        }

        [Test]
        public void CombatReactionContract_DeclaresProfileRuntimeLanesAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.combat.reaction");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(ActorCombatReactionProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Combat.IActorGuardFeature"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Core.Contracts.IDamageModifier"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.ConsumedActionRoles, Does.Contain("Guard"));
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.npc-enemy-behavior"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void CombatReactionContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.combat.reaction");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(ActorCombatReactionProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorGuardFeature"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IDamageModifier"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.npc-enemy-behavior"));
        }

        [Test]
        public void StatusContract_DeclaresProfileRuntimeLanesAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.status");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(ActorStatusEffectProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Combat.IActorStatusEffectReceiver"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Core.Contracts.IDamageModifier"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.custom-object-effect"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void StatusContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.status");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(ActorStatusEffectProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorStatusEffectReceiver"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IDamageModifier"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.custom-object-effect"));
        }

        [Test]
        public void FeedbackContract_DeclaresProfileRuntimeLanesAndProof()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.feedback");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RequiredProfileType.FullName, Is.EqualTo(typeof(ActorFeedbackProfile).FullName));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
            Assert.That(contract.RequiredRuntimeInterfaceNames, Does.Contain("NeonBlack.Gameplay.Features.Composition.IActorFeedbackPublisher"));
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.Billboard2_5D), Is.True);
            Assert.That(contract.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.ui-hud-menu"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void FeedbackContract_ContributesAuthoringFact()
        {
            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find("feature.actor.feedback");

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.Kind, Is.EqualTo(PyralisAuthoringFactKind.FeatureContract));
            Assert.That(fact.RequiredProfiles, Does.Contain(nameof(ActorFeedbackProfile)));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(fact.RequiredUnitySurfaces, Does.Contain("IActorFeedbackPublisher"));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Sprite2D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.Billboard2_5D)));
            Assert.That(fact.LaneTags, Does.Contain(nameof(ActorPresentationMode.ThirdPerson3D)));
            Assert.That(fact.RelatedStableIds, Does.Contain("proof.ui-hud-menu"));
        }

        [Test]
        public void TopDownHopContractValidator_ReportsWrongProfileType()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.traversal.topdown-hop";
            definition.profileAsset = ScriptableObject.CreateInstance<InteractionFeatureProfile>();

            List<string> issues = NeonBlack.Gameplay.Editor.PyralisFeatureModuleContractValidator.GetValidationIssues(definition);

            Assert.That(issues.Exists(issue => issue.Contains("TopDownHopProfile")), Is.True);

            UnityEngine.Object.DestroyImmediate(definition.profileAsset);
            UnityEngine.Object.DestroyImmediate(definition);
        }

        [Test]
        public void TopDownHopContractValidator_ReportsUnsupportedRigged3DLane()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.traversal.topdown-hop";
            definition.supportedPresentationModes = new[] { ActorPresentationMode.ThirdPerson3D };

            List<string> issues = NeonBlack.Gameplay.Editor.PyralisFeatureModuleContractValidator.GetValidationIssues(definition);

            Assert.That(issues.Exists(issue => issue.Contains("Rigged3D actors should use the 3D traversal jump path")), Is.True);

            UnityEngine.Object.DestroyImmediate(definition);
        }

        [Test]
        public void AuthoringContracts_RegistryUsesReflectionDiscovery()
        {
            string registrySource = File.ReadAllText(
                Path.Combine(GameplayRoot, "Core", "Contracts", "ResolvedAuthoringContractRegistry.cs"));
            string factScannerSource = File.ReadAllText(
                Path.Combine(GameplayRoot, "Editor", "Authoring", "Spine", "Facts", "PyralisReflectiveFactScanner.cs"));
            string factTypesSource = File.ReadAllText(
                Path.Combine(GameplayRoot, "Editor", "Authoring", "Spine", "Facts", "PyralisAuthoringFactTypes.cs"));

            Assert.That(registrySource.Contains("GetTypesWithAttribute<AuthoringContractAttribute>()"), Is.True, "Registry should discover contracts via TypeCache attribute scanning.");
            Assert.That(registrySource.Contains("IAuthoringContractProvider"), Is.False, "Provider-based contracts were removed; keep one attribute-backed contract path.");
            Assert.That(registrySource.Contains("ModuleId"), Is.True, "Registry should filter and map via ModuleId metadata.");
            Assert.That(registrySource.Contains("SetupNodeId"), Is.True, "Contracts should explicitly map to resolved setup graph nodes when they enrich known setup concepts.");
            Assert.That(registrySource.Contains("FirstProofTargetId"), Is.True, "Proof routing should use explicit contract metadata, not infer from prose.");
            Assert.That(registrySource.Contains("ResolveFirstProofTargetId"), Is.False, "Central keyword proof-target inference should stay deleted.");
            Assert.That(registrySource.Contains("FirstProof.StartsWith(\"proof.\""), Is.False, "FirstProof is human guidance and must not be parsed as a proof route id.");
            Assert.That(registrySource.Contains("attr.ProfileType != null"), Is.False, "ProfileType should not imply feature-module runtime requirements. RequiredInterfaces owns runtime requirements explicitly.");
            Assert.That(registrySource.Contains("|| ContainsTypeName(contract.RequiredComponentNames, fullName)"), Is.False, "Physical component placement must not create dependency proof-target edges.");
            Assert.That(registrySource.Contains("PrettifyTypeName"), Is.True, "Registry should generate clean display names from reflected types.");
            Assert.That(factTypesSource.Contains("PrefabComponent"), Is.False, "Authoring fact kinds should use UnitySurface so requirements are not forced into prefab-only language.");
            Assert.That(factScannerSource.Contains("RequiredPrefabComponents"), Is.False, "Authoring facts should describe required Unity surfaces, not assume every requirement is prefab-owned.");
            Assert.That(factScannerSource.Contains("requiredPrefabComponents"), Is.False, "Authoring facts should describe required Unity surfaces, not assume every requirement is prefab-owned.");
            Assert.That(factScannerSource.Contains("|| ContainsTypeName(contract.RequiredComponentNames, fullName)"), Is.False, "Physical component placement should project into Unity surfaces, not dependency relationships.");
            Assert.That(factScannerSource.Contains("InferRelatedStableIds"), Is.False, "Reflection facts should relate through contracts and dependency metadata, not keyword proof-route inference.");
            Assert.That(factScannerSource.Contains("value.Contains(\"pawn\")"), Is.False, "Reflection facts should not keyword-map gameplay names to proof ids.");
            Assert.That(File.Exists(Path.Combine(GameplayRoot, "Editor", "Authoring", "Spine", "Facts", "PyralisAuthoringFactTypes.cs")), Is.True);
        }

        [Test]
        public void AuthoringContracts_ProfileTypeDoesNotImplyFeatureModuleRuntime()
        {
            ResolvedAuthoringContract inputProfile = ResolvedAuthoringContractRegistry.FindByType(typeof(InputProfile));

            Assert.That(inputProfile, Is.Not.Null);
            Assert.That(inputProfile.RequiredProfileType, Is.EqualTo(typeof(InputProfile)));
            Assert.That(inputProfile.RequiredRuntimeInterfaceNames, Does.Not.Contain("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"));
        }

        [Test]
        public void AuthoringContracts_ProjectPhysicalComponentRequirementsAsUnitySurfaces()
        {
            PyralisAuthoringFact feedback = PyralisAuthoringFactRegistry.Find("feature.actor.feedback");

            Assert.That(feedback, Is.Not.Null);
            Assert.That(feedback.RequiredUnitySurfaces, Does.Contain("IActorFeedbackPublisher"));
            Assert.That(feedback.RequiredUnitySurfaces, Does.Contain("HealthComponent"));
        }

        [Test]
        public void AuthoringSetupGraph_UsesStableProofIdsInsteadOfProofLabels()
        {
            string graphBuilderSource = File.ReadAllText(
                Path.Combine(GameplayRoot, "Editor", "Authoring", "Spine", "Graph", "PyralisAuthoringSetupGraphBuilder.cs"));

            Assert.That(graphBuilderSource.Contains("\"proof.\" + NormalizeId(card.ProofStepLabel)"), Is.False);
            Assert.That(graphBuilderSource.Contains("string.Equals(fact.DisplayName, proof.Label"), Is.False);
            Assert.That(graphBuilderSource.Contains("proof.StableId"), Is.True);
        }

        [Test]
        public void AuthoringContracts_PropagateSetupNodeMetadata()
        {
            ResolvedAuthoringContract sessionContract = ResolvedAuthoringContractRegistry.FindByType(typeof(SessionDefinition));
            ResolvedAuthoringContract pawnContract = ResolvedAuthoringContractRegistry.FindByType(typeof(PawnDefinition));
            ResolvedAuthoringContract pawnRootContract = ResolvedAuthoringContractRegistry.FindByType(typeof(PawnRoot));

            Assert.That(sessionContract, Is.Not.Null);
            Assert.That(sessionContract.SetupNodeId, Is.EqualTo("session.definition"));
            Assert.That(pawnContract, Is.Not.Null);
            Assert.That(pawnContract.SetupNodeId, Is.EqualTo("pawn.definition"));
            Assert.That(pawnRootContract, Is.Not.Null);
            Assert.That(pawnRootContract.SetupNodeId, Is.EqualTo("pawn.definition"));
        }

        [Test]
        public void AuthoringContracts_PreserveDeveloperFirstProofGuidanceSeparatelyFromRouteProofTarget()
        {
            ResolvedAuthoringContract contract = ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.FirstProofGuidance, Does.Contain("hop"));
            Assert.That(contract.FirstProofGuidance, Does.Not.StartWith("proof."));
            Assert.That(contract.FirstProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));
        }

        [Test]
        public void AuthoringContracts_FirstProofGuidanceNeverStoresRouteProofIds()
        {
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                Assert.That(contract.FirstProofGuidance, Does.Not.StartWith("proof."), contract.StableId);
            }
        }

        [Test]
        public void AuthoringSetupGraph_ReflectsFeatureContractsAsNodes()
        {
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null);

            Assert.That(graph.TryFindNode("contract.feature.actor.traversal.topdown-hop", out PyralisAuthoringGraphNode node), Is.True);
            Assert.That(node.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Contract));
            Assert.That(node.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.AuthoringContract));
            Assert.That(node.ProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(node.NativeSetup, Is.Not.Empty);
        }

        [Test]
        public void AuthoringSetupGraph_LinksContractsToResolvedSetupNodes()
        {
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null);
            ResolvedAuthoringContract sessionContract = ResolvedAuthoringContractRegistry.FindByType(typeof(SessionDefinition));
            ResolvedAuthoringContract pawnRootContract = ResolvedAuthoringContractRegistry.FindByType(typeof(PawnRoot));

            Assert.That(sessionContract, Is.Not.Null);
            Assert.That(pawnRootContract, Is.Not.Null);
            Assert.That(
                graph.Edges.Any(edge =>
                    edge.FromNodeId == "contract." + sessionContract.StableId
                    && edge.ToNodeId == "session.definition"
                    && edge.Kind == PyralisAuthoringGraphEdgeKind.RelatesTo),
                Is.True);
            Assert.That(
                graph.Edges.Any(edge =>
                    edge.FromNodeId == "contract." + pawnRootContract.StableId
                    && edge.ToNodeId == "pawn.definition"
                    && edge.Kind == PyralisAuthoringGraphEdgeKind.RelatesTo),
                Is.True);
        }

        [Test]
        public void AuthoringContractFacts_RelateFoundationalContractsToSetupNodes()
        {
            ResolvedAuthoringContract sessionContract = ResolvedAuthoringContractRegistry.FindByType(typeof(SessionDefinition));
            Assert.That(sessionContract, Is.Not.Null);

            PyralisAuthoringFact fact = PyralisAuthoringFactRegistry.Find(sessionContract.StableId);

            Assert.That(fact, Is.Not.Null);
            Assert.That(fact.RelatedStableIds, Does.Contain("session.definition"));
        }

        [Test]
        public void AuthoringSetupGraph_GameSetupProfileCreatesCapabilityAndProofNodes()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay },
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.Combat }
            };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);

            Assert.That(graph.TryFindNode("setup.profile", out PyralisAuthoringGraphNode setupNode), Is.True);
            Assert.That(setupNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(setupNode.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup));
            Assert.That(graph.TryFindNode("capability.selected", out PyralisAuthoringGraphNode capabilitySummaryNode), Is.True);
            Assert.That(capabilitySummaryNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Capability));
            Assert.That(capabilitySummaryNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(graph.TryFindNode("capability.2d-pawn-movement", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Capability));
            Assert.That(pawnNode.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.LegacyFact));
            Assert.That(graph.TryFindNode("proof.1p-pawn-movement", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));
            Assert.That(proofNode.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.SpineFallback));
            Assert.That(
                graph.Edges.Any(edge =>
                    edge.FromNodeId == "capability.2d-pawn-movement"
                    && edge.ToNodeId == "proof.1p-pawn-movement"
                    && edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof),
                Is.True);
            IReadOnlyList<PyralisAuthoringGraphConnectionRow> connectionRows = PyralisAuthoringSetupGraphProjection.BuildMapConnectionRows(graph);
            Assert.That(
                connectionRows.Any(row =>
                    row.Edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof
                    && row.From.StableId == "capability.2d-pawn-movement"
                    && row.To.StableId == "proof.1p-pawn-movement"
                    && row.Relationship == "supports proof"),
                Is.True);
            Assert.That(
                PyralisAuthoringSetupGraphProjection.BuildProofSupportRows(graph).Any(row =>
                    row.From.StableId == "capability.2d-pawn-movement"
                    && row.To.StableId == "proof.1p-pawn-movement"),
                Is.True);
            Assert.That(
                graph.Edges.Any(edge =>
                    edge.ToNodeId == "proof.1p-pawn-movement"
                    && edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy),
                Is.False,
                "Capability support must not be represented as a proof blocker.");
            Assert.That(PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph)?.StableId, Is.EqualTo("proof.1p-pawn-movement"));

            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringSetupGraph_TracksSourceOriginForMigrationReadiness()
        {
            GameObject root = new GameObject("Graph Origin Readiness");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);

            Assert.That(graph.TryFindNode("bootstrap.root", out PyralisAuthoringGraphNode bootstrapNode), Is.True);
            Assert.That(bootstrapNode.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.SpineGrammar));
            Assert.That(graph.TryFindNode("contract.feature.actor.traversal.topdown-hop", out PyralisAuthoringGraphNode contractNode), Is.True);
            Assert.That(contractNode.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.Contract));
            Assert.That(
                graph.Nodes.Any(node =>
                    node != null
                    && node.StableId.StartsWith("setupflow.", StringComparison.Ordinal)
                    && node.SourceOrigin == PyralisAuthoringGraphSourceOrigin.RuntimeEvidence),
                Is.True);
            Assert.That(
                PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Missing)
                    .Concat(PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Blocked))
                    .Any(row => !string.IsNullOrWhiteSpace(row.OriginLabel)),
                Is.True);

            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void AuthoringSetupGraph_ProjectsRuntimePatternsAsUserAuthoredSetupNodes()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            pattern.patternId = "pattern.editor-test";
            pattern.displayName = "Editor Test Pattern";
            pattern.description = "Test pattern description.";
            pattern.capabilityFamily = RuntimeCapabilityFamily.CameraInput;
            setupProfile.runtimePatterns = new[] { pattern };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);

            Assert.That(graph.TryFindNode("runtime-pattern.pattern-editor-test", out PyralisAuthoringGraphNode patternNode), Is.True);
            Assert.That(patternNode.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.RuntimePattern));
            Assert.That(patternNode.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.UserAuthoredSetup));
            Assert.That(patternNode.SourceObject, Is.SameAs(pattern));

            UnityEngine.Object.DestroyImmediate(pattern);
            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringOverviewModel_DoesNotRebuildVisibleProofGuidanceFromRouteProof()
        {
            string overviewSource = File.ReadAllText(
                Path.Combine(GameplayRoot, "Editor", "Authoring", "Spine", "Routes", "PyralisAuthoringOverviewModel.cs"));

            Assert.That(overviewSource.Contains("PyralisAuthoringRouteProof.Build"), Is.False);
        }

        [Test]
        public void AuthoringSetupGraph_TabletopRouteStaysNoPawnAndSupportsBoardProof()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.BoardCardTabletop }
            };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);

            Assert.That(graph.RouteAnalysis.RequiresPawn, Is.False);
            Assert.That(graph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement));
            Assert.That(pawnNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(pawnNode.Guidance, Does.Contain("No-pawn route"));
            Assert.That(graph.TryFindNode("proof.board-card-action", out PyralisAuthoringGraphNode proofNode), Is.True);
            Assert.That(proofNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.Proof));
            PyralisAuthoringGraphNode tabletopCapabilityNode = graph.Nodes.FirstOrDefault(node =>
                node.Kind == PyralisAuthoringGraphNodeKind.Capability
                && node.CapabilityFamily == RuntimeCapabilityFamily.BoardCardTabletop);
            Assert.That(tabletopCapabilityNode, Is.Not.Null);
            Assert.That(
                graph.Edges.Any(edge =>
                    edge.FromNodeId == tabletopCapabilityNode.StableId
                    && edge.ToNodeId == "proof.board-card-action"
                    && edge.Kind == PyralisAuthoringGraphEdgeKind.SupportsProof),
                Is.True);

            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringSetupGraph_SessionRouteCreatesParticipantPawnAndSurfaceNodes()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay }
            };
            mode.setupProfile = setupProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            participant.defaultPawn = pawn;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(graph.TryFindNode("participant.default", out PyralisAuthoringGraphNode participantNode), Is.True);
            Assert.That(participantNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(participantNode.SourceObject, Is.SameAs(participant));
            Assert.That(graph.TryFindNode("pawn.definition", out PyralisAuthoringGraphNode pawnNode), Is.True);
            Assert.That(pawnNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.UnitySurfaceRequirement));
            Assert.That(pawnNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(pawnNode.SourceObject, Is.SameAs(pawn));
            Assert.That(graph.TryFindNode("scene.surfaces", out PyralisAuthoringGraphNode surfaceSummaryNode), Is.True);
            Assert.That(surfaceSummaryNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.SceneSurface));

            UnityEngine.Object.DestroyImmediate(pawn);
            UnityEngine.Object.DestroyImmediate(participant);
            UnityEngine.Object.DestroyImmediate(session);
            UnityEngine.Object.DestroyImmediate(mode);
            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringSetupGraphProjection_BuildsMapRowsFromGraphNodes()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.Combat }
            };

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);
            IReadOnlyList<PyralisAuthoringSetupGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph);

            Assert.That(rows.Select(row => row.Label), Does.Contain("Setup Profile"));
            Assert.That(rows.Select(row => row.Label), Does.Contain("Capabilities"));
            Assert.That(rows.Select(row => row.Label), Does.Contain("Scene Surfaces"));
            Assert.That(PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph), Is.Not.Null);
            Assert.That(PyralisAuthoringSetupGraphProjection.FindFirstUnresolvedNode(graph), Is.Not.Null);

            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringSetupGraphProjection_ValidationRowsIncludeSetupFlowEvidence()
        {
            GameObject root = new GameObject("Bootstrap Validation Graph");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringValidationGraphRow[] rows = PyralisAuthoringSetupGraphProjection
                .BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Missing)
                .Concat(PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Blocked))
                .ToArray();

            Assert.That(
                rows.Any(row =>
                    row.Node != null
                    && row.Node.SourceKind == PyralisAuthoringGraphSourceKind.SetupFlow
                    && row.NodeId.StartsWith("setupflow.", StringComparison.Ordinal)),
                Is.True);

            UnityEngine.Object.DestroyImmediate(root);
        }

        [Test]
        public void AuthoringSetupGraphProjection_ValidationRowsIncludeSetupChainAndCapabilityReadiness()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);
            IReadOnlyList<PyralisAuthoringValidationGraphRow> missingRows =
                PyralisAuthoringSetupGraphProjection.BuildValidationRows(graph, PyralisAuthoringGraphEvidenceState.Missing);

            Assert.That(missingRows.Any(row => row.NodeId == "session.definition"), Is.True);
            Assert.That(missingRows.Any(row => row.NodeId == "mode.definition"), Is.True);
            Assert.That(missingRows.Any(row => row.NodeId == "capability.selected"), Is.True);

            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringSetupGraphProjection_BlankSourceNamesMissingGameplayRoot()
        {
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null);
            PyralisAuthoringCurrentStepGraphRow currentStep = PyralisAuthoringSetupGraphProjection.BuildCurrentStepRow(graph);

            Assert.That(graph.TryFindNode("bootstrap.root", out PyralisAuthoringGraphNode bootstrapNode), Is.True);
            Assert.That(bootstrapNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(bootstrapNode.NativeAction.HasValue, Is.True);
            Assert.That(currentStep.Label, Is.EqualTo("Gameplay Root"));
            Assert.That(currentStep.Message, Does.Contain("GameplaySessionBootstrap"));
        }

        [Test]
        public void AuthoringOverviewModel_UsesGraphReadinessForDoNow()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(setupProfile);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(
                setupProfile,
                graph);

            Assert.That(model.DoNow.Any(issue => issue.Label == "Session Definition"), Is.True);
            Assert.That(model.DoNow.Any(issue => issue.Label == "Game Mode Definition"), Is.True);
            Assert.That(model.DoNow.Any(issue => issue.Label == "Capabilities"), Is.True);

            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringSetupGraphProjection_ResolvesSelectedSetupAssetsToGraphContext()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay }
            };
            mode.setupProfile = setupProfile;
            session.defaultGameMode = mode;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);
            PyralisAuthoringSelectedContextGraphRow sessionContext = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, session);
            PyralisAuthoringSelectedContextGraphRow modeContext = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, mode);
            PyralisAuthoringSelectedContextGraphRow setupContext = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, setupProfile);

            Assert.That(sessionContext.NodeId, Is.EqualTo("session.definition"));
            Assert.That(modeContext.NodeId, Is.EqualTo("mode.definition"));
            Assert.That(setupContext.NodeId, Is.EqualTo("setup.profile"));
            Assert.That(setupContext.NextCheck, Does.Contain("Open Intent"));

            UnityEngine.Object.DestroyImmediate(session);
            UnityEngine.Object.DestroyImmediate(mode);
            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringSetupGraphProjection_ResolvesPawnRootSelectionToPawnNode()
        {
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Root Context");
            PawnRoot pawnRoot = prefab.AddComponent<PawnRoot>();
            setupProfile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection { capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay }
            };
            mode.setupProfile = setupProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            participant.defaultPawn = pawn;
            pawn.pawnPrefab = prefab;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);
            PyralisAuthoringSelectedContextGraphRow context = PyralisAuthoringSetupGraphProjection.BuildSelectedContextRow(graph, pawnRoot);

            Assert.That(context.NodeId, Is.EqualTo("pawn.definition"));
            Assert.That(context.RuntimeMeaning, Does.Contain("Pawn"));

            UnityEngine.Object.DestroyImmediate(prefab);
            UnityEngine.Object.DestroyImmediate(pawn);
            UnityEngine.Object.DestroyImmediate(participant);
            UnityEngine.Object.DestroyImmediate(session);
            UnityEngine.Object.DestroyImmediate(mode);
            UnityEngine.Object.DestroyImmediate(setupProfile);
        }

        [Test]
        public void AuthoringContracts_PropagatePriorityAndDeprecationMetadata()
        {
            ResolvedAuthoringContract movement = ResolvedAuthoringContractRegistry.FindByType(typeof(NeonBlack.Gameplay.Features.Characters.Pawn2DMovementComponent));

            Assert.That(movement, Is.Not.Null);
            Assert.That(movement.PriorityValueOverride, Is.EqualTo(50));
        }

        [Test]
        public void AuthoringContracts_RawAttributeScanningStaysInsideResolvedRegistry()
        {
            string registryPath = Path.Combine(GameplayRoot, "Core", "Contracts", "ResolvedAuthoringContractRegistry.cs");
            string editorRoot = Path.Combine(GameplayRoot, "Editor", "Authoring");
            string[] searchedFiles = Directory.GetFiles(editorRoot, "*.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Path.Combine(GameplayRoot, "Runtime"), "*.cs", SearchOption.AllDirectories))
                .Concat(Directory.GetFiles(Path.Combine(GameplayRoot, "Core"), "*.cs", SearchOption.AllDirectories))
                .Where(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(registryPath), StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string file in searchedFiles)
            {
                string source = File.ReadAllText(file);
                Assert.That(
                    source.Contains("GetTypesWithAttribute<AuthoringContractAttribute>()"),
                    Is.False,
                    $"{file} should consume ResolvedAuthoringContractRegistry instead of scanning raw AuthoringContractAttribute data.");
            }
        }

        [Test]
        public void AuthoringContracts_AllContractsCarrySetupProfileMetadata()
        {
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;

            Assert.That(contracts.Count, Is.GreaterThan(0));
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                Assert.That(contract.AuthoringCategory, Is.Not.Empty, contract.StableId);
                Assert.That(contract.NativeSetup.Length, Is.GreaterThan(0), contract.StableId);
                Assert.That(
                    contract.AssignmentFields.Length + contract.CustomizationMoments.Length + contract.RequiredRuntimeInterfaceNames.Length + contract.RequiredComponentNames.Length,
                    Is.GreaterThan(0),
                    contract.StableId);
            }
        }

        [Test]
        public void AuthoringContracts_RoutedFeatureModulesDeclareProofTargetsThatMapToRouteProofCards()
        {
            IReadOnlyList<ResolvedAuthoringContract> contracts = ResolvedAuthoringContractRegistry.All;

            Assert.That(contracts.Count, Is.GreaterThan(0));
            for (int i = 0; i < contracts.Count; i++)
            {
                ResolvedAuthoringContract contract = contracts[i];
                if (string.IsNullOrWhiteSpace(contract.ModuleId)
                    || !contract.RequiredRuntimeInterfaceNames.Contains("NeonBlack.Gameplay.Features.Composition.IFeatureModuleRuntime"))
                {
                    continue;
                }

                Assert.That(contract.FirstProofTargetId, Is.Not.Empty, contract.StableId);
                Assert.That(PyralisAuthoringRouteProof.FindProofFact(contract.FirstProofTargetId), Is.Not.Null, contract.StableId);
            }
        }

        [Test]
        public void ContractProofFactProjector_GeneratesContractOwnedProofFactsWhenNoBroadProofExists()
        {
            IReadOnlyCollection<string> broadProofIds = PyralisAuthoringRouteProof.GetAuthoringFacts()
                .Select(fact => fact.StableId)
                .ToArray();

            PyralisAuthoringFact proof = PyralisContractProofFactProjector.FindProofFact(
                "proof.contract-owned-editor-test",
                broadProofIds);

            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.Kind, Is.EqualTo(PyralisAuthoringFactKind.Proof));
            Assert.That(proof.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.FeatureContract));
            Assert.That(proof.FirstProof, Does.Contain("contract-owned proof"));
            Assert.That(proof.RelatedStableIds, Does.Contain("feature." + typeof(ContractOwnedProofFixture).FullName));
            Assert.That(PyralisAuthoringFactRegistry.Find("proof.contract-owned-editor-test"), Is.Not.Null);
        }

        [Test]
        public void ContractProofFactProjector_EnrichesBroadRouteProofsFromContractMetadata()
        {
            PyralisAuthoringFact proof = PyralisAuthoringFactRegistry.Find("proof.1p-pawn-movement");

            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.SourceKind, Is.EqualTo(PyralisAuthoringFactSourceKind.SetupFlow));
            Assert.That(proof.RequiredProfiles, Does.Contain(nameof(TopDownHopProfile)));
            Assert.That(proof.RequiredUnitySurfaces, Does.Contain("IFeatureModuleRuntime"));
            Assert.That(proof.GoalTags, Does.Contain("Traversal"));
            Assert.That(proof.RelatedStableIds, Does.Contain("feature.actor.traversal.topdown-hop"));
            Assert.That(proof.RouteRelevance, Does.Contain("Reflective contract inputs"));
        }

        [Test]
        public void FeatureModuleDefinition_DefinitionValidationNoLongerOwnsFeatureSpecificContracts()
        {
            string definitionSource = File.ReadAllText(
                Path.Combine(GameplayRoot, "Data", "Definitions", "FeatureModuleDefinition.cs"));

            Assert.That(definitionSource.Contains("AppendRuntimeContractIssues"), Is.False);
            Assert.That(definitionSource.Contains("ProfileMatches"), Is.False);
            Assert.That(definitionSource.Contains("profileAsset is not PawnTraversalProfile"), Is.False);
            Assert.That(definitionSource.Contains("expects an ActorCombatReactionProfile profile asset"), Is.False);
            Assert.That(definitionSource.Contains("runtime prefab should expose IActorGuardFeature"), Is.False);
        }

        [Test]
        public void DeprecatedContracts_EnforceHardDeletionDeadlines()
        {
            string path = Path.Combine(GameplayRoot, "..", "..", "..", "package.json");
            Assert.That(File.Exists(path), Is.True);
            
            string jsonRaw = File.ReadAllText(path);
            var packageDef = JsonUtility.FromJson<PackageJsonRaw>(jsonRaw);
            Version currentPackageVersion = new Version(packageDef.version);

            foreach (var fact in PyralisAuthoringFactRegistry.AllFacts)
            {
                if (fact.Priority == (int)AuthoringPriority.Deprecated)
                {
                    if (!string.IsNullOrEmpty(fact.RemovableInVersion))
                    {
                        Version expirationVersion = new Version(fact.RemovableInVersion);
                        
                        Assert.That(currentPackageVersion, Is.LessThan(expirationVersion),
                            $"DEPRECATION DEADLINE REACHED: '{fact.DisplayName}' was scheduled for physical deletion in version {expirationVersion}. " +
                            $"Active package version is {currentPackageVersion}. Physically delete this script to restore build compliance.");
                    }
                }
            }
        }

        [Serializable]
#pragma warning disable 0649
        private class PackageJsonRaw { public string version; }
#pragma warning restore 0649

        [AuthoringContract(
            Capability = AuthoringCapability.Puzzle,
            Relevance = "Test-only contract that proves feature contracts can own a proof target without a central route-proof entry.",
            Axioms = AuthoringWorldAxiom.None,
            FirstProofTargetId = "proof.contract-owned-editor-test",
            FirstProof = "Run one contract-owned proof generated from feature metadata.",
            NativeSetup = new[] { "Inspect the test-owned contract proof." },
            AssignmentFields = new[] { "ContractOwnedProofFixture.testField" })]
        private sealed class ContractOwnedProofFixture
        {
        }
    }
}
