using System;
using System.Collections.Generic;
using System.Linq;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Enums;
using NeonBlack.Gameplay.Data.Config;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Modules.Input;
using NeonBlack.Gameplay.Modules.Interaction;
using NeonBlack.Gameplay.Glue.InputRouting;
using NeonBlack.Gameplay.Glue.Spawning;
using NeonBlack.Gameplay.Presentation.Camera;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;
using NeonBlack.Gameplay.Glue.Bootstrap;
using NeonBlack.Gameplay.Data.Participants;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class PyralisAuthoringSmokeTests : PyralisEditorTestSupport
    {
        [Test]
        public void ContractRegistry_SmokeResolvesStableFeatureContract()
        {
            Assert.That(ResolvedAuthoringContractRegistry.HasDuplicateStableIds(out string duplicateStableId), Is.False, duplicateStableId);

            ResolvedAuthoringContract contract =
                ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.StableId, Is.EqualTo("feature.actor.traversal.topdown-hop"));
            Assert.That(contract.ProofTargetId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(contract.CapabilityPath, Is.EqualTo("Movement/Traversal/FakeGravityJump"));
            Assert.That(contract.RuntimeFamilies, Does.Contain(RuntimeCapabilityFamily.CharacterPawnGameplay));
            Assert.That(contract.RoleTags, Does.Contain("FakeGravityJump"));
            Assert.That(contract.Confidence, Is.EqualTo(PyralisAuthoringConfidence.Explicit));
        }

        [Test]
        public void ContractRegistry_SmokeRuntimeFamiliesAreContractOwned()
        {
            ResolvedAuthoringContract contract =
                ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop");

            Assert.That(contract, Is.Not.Null);
            Assert.That(contract.RuntimeFamilies, Is.EquivalentTo(new[] { RuntimeCapabilityFamily.CharacterPawnGameplay }));

            RuntimeCapabilityFamily[] families =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildRuntimeFamilies(
                    AuthoringCapability.RangedFlow,
                    RuntimeCapabilityLaneTag.Mixed,
                    AuthoringWorldAxiom.None);

            Assert.That(families.Contains(RuntimeCapabilityFamily.GunsProjectiles), Is.False);
            Assert.That(families.Contains(RuntimeCapabilityFamily.Combat), Is.False);
        }

        [Test]
        public void ContractRegistry_SmokeSurfaceSeparatesIntentIngredientsFromSetupGlue()
        {
            Assert.That(
                ResolvedAuthoringContractRegistry.FindByModuleId("actor.traversal.topdown-hop")?.Surface,
                Is.EqualTo(AuthoringContractSurface.GameplayIngredient));
            Assert.That(
                ResolvedAuthoringContractRegistry.FindByType(typeof(ParticipantInputRouter))?.Surface,
                Is.EqualTo(AuthoringContractSurface.RouteEssential));
            Assert.That(
                ResolvedAuthoringContractRegistry.FindByType(typeof(GameConfig))?.Surface,
                Is.EqualTo(AuthoringContractSurface.SetupOnly));
            Assert.That(
                ResolvedAuthoringContractRegistry.FindByType(typeof(TopDownHopProfile))?.Surface,
                Is.EqualTo(AuthoringContractSurface.Profile));
            Assert.That(
                ResolvedAuthoringContractRegistry.FindByType(typeof(PawnCameraTarget))?.Surface,
                Is.EqualTo(AuthoringContractSurface.RuntimeComponent));
        }

        [Test]
        public void IntentProjection_SmokeCapabilityIngredientTogglesAreUnique()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);
            PyralisAuthoringIntentProjection projection =
                PyralisAuthoringIntentProjection.Build(
                    new PyralisAuthoringIntentSelection(
                        RuntimeCapabilityLaneTag.Sprite2D,
                        AuthoringCapability.None,
                        AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime),
                    descriptors);
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> ingredientDescriptors =
                projection.GameplayIngredientDescriptors;

            string[] stableIds = ingredientDescriptors.Select(descriptor => descriptor.StableId).ToArray();
            Assert.That(stableIds.Distinct().Count(), Is.EqualTo(stableIds.Length));
            Assert.That(ingredientDescriptors.Any(descriptor =>
                descriptor.CapabilityPath.StartsWith("Core Setup/", System.StringComparison.Ordinal)), Is.False);
            Assert.That(ingredientDescriptors.Any(descriptor =>
                descriptor.Capability.HasFlag(AuthoringCapability.Movement)
                && descriptor.CapabilityPath.StartsWith("Movement", System.StringComparison.Ordinal)), Is.True);
            Assert.That(ingredientDescriptors.All(descriptor =>
                descriptor.Surface == AuthoringContractSurface.GameplayIngredient), Is.True);
        }

        [Test]
        public void IntentProjection_SmokeUsesReflectedContractDescriptorPaths()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringCapabilityDescriptor fakeGravityJump = descriptors.FirstOrDefault(descriptor =>
                descriptor != null
                && string.Equals(descriptor.StableId, "feature.actor.traversal.topdown-hop", System.StringComparison.Ordinal));

            Assert.That(fakeGravityJump, Is.Not.Null);
            Assert.That(fakeGravityJump.CapabilityPath, Is.EqualTo("Movement/Traversal/FakeGravityJump"));
            Assert.That(fakeGravityJump.RoleTags, Does.Contain("FakeGravityJump"));
            Assert.That(fakeGravityJump.SelectableIntent, Is.True);
        }

        [Test]
        public void IntentProjection_SmokeRouteEssentialsAreInferredNotSelected()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringCapabilityDescriptor movementIngredient = descriptors.FirstOrDefault(descriptor =>
                descriptor != null
                && !PyralisAuthoringCapabilityDescriptorRegistry.IsIntentRouteEssential(descriptor)
                && descriptor.CapabilityPath == "Movement/Traversal/FakeGravityJump");
            PyralisAuthoringCapabilityDescriptor routeEssential = descriptors.FirstOrDefault(descriptor =>
                descriptor != null
                && PyralisAuthoringCapabilityDescriptorRegistry.IsIntentRouteEssential(descriptor)
                && descriptor.DisplayName.Contains("Participant", System.StringComparison.OrdinalIgnoreCase));

            Assert.That(movementIngredient, Is.Not.Null);
            Assert.That(routeEssential, Is.Not.Null);

            string[] selectedIds =
            {
                movementIngredient.StableId,
                routeEssential.StableId
            };
            string[] filtered = PyralisAuthoringCapabilityDescriptorRegistry.FilterGameplayIntentDescriptorIds(selectedIds);

            Assert.That(filtered, Does.Contain(movementIngredient.StableId));
            Assert.That(filtered, Does.Not.Contain(routeEssential.StableId));

            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(filtered),
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                selectedIds,
                PyralisIntentParticipantRoute.TwoLocalPlayers);
            PyralisAuthoringIntentProjection projection =
                PyralisAuthoringIntentProjection.Build(intentSelection, descriptors);
            string intentJson = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(
                intentSelection,
                PyralisAuthoringSetupGraphProjection.BuildIntentModel(intentSelection),
                projection);

            Assert.That(intentJson, Does.Contain("\"selectedDescriptorIds\""));
            Assert.That(intentJson, Does.Contain(movementIngredient.StableId));
            int selectedIdsStart = intentJson.IndexOf("\"selectedDescriptorIds\"", System.StringComparison.Ordinal);
            int selectedIdsEnd = intentJson.IndexOf("]", selectedIdsStart, System.StringComparison.Ordinal);
            string selectedIdsBlock = intentJson.Substring(selectedIdsStart, selectedIdsEnd - selectedIdsStart);
            Assert.That(selectedIdsBlock, Does.Not.Contain(routeEssential.StableId));
            Assert.That(intentJson, Does.Contain("\"intentLayer\": \"RouteEssential\""));
            Assert.That(intentJson, Does.Contain("\"inferred\": true"));
            Assert.That(intentJson, Does.Contain("\"surface\": \"RouteEssential\""));
        }

        [Test]
        public void IntentProjection_SmokeExportsSelectedIntentStatusFromGraphProjection()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);
            PyralisAuthoringCapabilityDescriptor motorDescriptor = descriptors.FirstOrDefault(descriptor =>
                descriptor != null
                && string.Equals(descriptor.StableId, "feature.NeonBlack.Gameplay.Modules.Character.Motor2D", StringComparison.Ordinal));

            Assert.That(motorDescriptor, Is.Not.Null);

            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                new[] { motorDescriptor.StableId },
                PyralisIntentParticipantRoute.SoloLocal);
            PyralisAuthoringIntentProjection projection =
                PyralisAuthoringIntentProjection.Build(intentSelection, descriptors);
            PyralisAuthoringGraphNode motorNode = new PyralisAuthoringGraphNode(
                motorDescriptor.StableId,
                "Motor2D",
                PyralisAuthoringGraphNodeKind.Contract,
                PyralisAuthoringGraphSourceKind.AuthoringContract,
                PyralisAuthoringGraphEvidenceState.Ready,
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                AuthoringCapability.Movement | AuthoringCapability.KineticMotor2D,
                proofTargetId: "proof.1p-pawn-movement",
                guidance: "Motor2D supports the selected first movement proof.");
            PyralisAuthoringGraphNode proofNode = new PyralisAuthoringGraphNode(
                "proof.1p-pawn-movement",
                "1P Pawn Movement Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.AuthoringContract,
                PyralisAuthoringGraphEvidenceState.CandidateDetected,
                proofTargetId: "proof.1p-pawn-movement",
                guidance: "Move one local pawn.");
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                session,
                null,
                new[] { motorNode, proofNode },
                new[]
                {
                    new PyralisAuthoringGraphEdge(
                        motorNode.StableId,
                        proofNode.StableId,
                        PyralisAuthoringGraphEdgeKind.SupportsProof,
                        "supports proof")
                },
                intentSelection);

            IReadOnlyList<PyralisAuthoringSelectedIntentStatusRow> statusRows =
                PyralisAuthoringSetupGraphProjection.BuildSelectedIntentStatusRows(graph, projection);
            string intentJson = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(
                intentSelection,
                PyralisAuthoringSetupGraphProjection.BuildIntentModel(intentSelection),
                projection,
                graph);

            Assert.That(statusRows.Count, Is.EqualTo(1));
            Assert.That(statusRows[0].StableId, Is.EqualTo(motorDescriptor.StableId));
            Assert.That(statusRows[0].Status, Is.EqualTo(PyralisAuthoringSelectedIntentStatus.SupportsProof));
            Assert.That(statusRows[0].RouteEvidence, Is.EqualTo("ProofSupport"));
            Assert.That(PyralisAuthoringOverviewProjection.Build(session, graph).SelectedIntentStatus, Has.Count.EqualTo(1));
            Assert.That(PyralisAuthoringGuideProjection.Build(session, session, graph).SelectedIntentStatus, Has.Count.EqualTo(1));
            Assert.That(intentJson, Does.Not.Contain("\"selectedIntentStatus\""));
            Assert.That(intentJson, Does.Not.Contain("\"routeEvidence\": \"ProofSupport\""));
            Assert.That(intentJson, Does.Not.Contain("\"mapRows\""));
            Assert.That(intentJson, Does.Not.Contain("\"requiredSetup\""));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void IntentProjection_SmokeRouteEssentialsStayNarrowForLocalMovement()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Input | AuthoringCapability.Movement | AuthoringCapability.KineticMotor2D | AuthoringCapability.Traversal,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                participantRoute: PyralisIntentParticipantRoute.TwoLocalPlayers);

            string[] inferredEssentials = descriptors
                .Where(descriptor => PyralisAuthoringCapabilityDescriptorRegistry.IsIntentRouteEssentialExpected(descriptor, intentSelection))
                .Select(descriptor => descriptor.DisplayName)
                .ToArray();

            Assert.That(inferredEssentials, Does.Contain("Gameplay Session Bootstrap"));
            Assert.That(inferredEssentials, Does.Contain("Session Definition"));
            Assert.That(inferredEssentials, Does.Contain("Participant Definition"));
            Assert.That(inferredEssentials, Does.Contain("Participant Input Router"));
            Assert.That(inferredEssentials, Does.Contain("Participant Spawn Service"));
            Assert.That(inferredEssentials, Does.Contain("Pawn Root"));
            Assert.That(inferredEssentials, Does.Contain("2 D  Motor  Input  Adapter"));
            Assert.That(inferredEssentials, Does.Contain("Top Down Hop Component"));

            Assert.That(inferredEssentials, Does.Not.Contain("Networked Participant Spawn Service"));
            Assert.That(inferredEssentials, Does.Not.Contain("Networked Session State Service"));
            Assert.That(inferredEssentials, Does.Not.Contain("Main Menu Manager"));
            Assert.That(inferredEssentials, Does.Not.Contain("Loading Screen Controller"));
            Assert.That(inferredEssentials, Does.Not.Contain("Projectile Impact Definition"));
            Assert.That(inferredEssentials, Does.Not.Contain("Combat Action Definition"));
        }

        [Test]
        public void IntentProjection_SmokeRouteEssentialsRequireContractRoleTags()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Input | AuthoringCapability.Movement | AuthoringCapability.KineticMotor2D | AuthoringCapability.Traversal,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                participantRoute: PyralisIntentParticipantRoute.TwoLocalPlayers);

            PyralisAuthoringIntentProjection projection =
                PyralisAuthoringIntentProjection.Build(intentSelection, descriptors);
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> routeEssentials =
                projection.RouteEssentialDescriptors;

            Assert.That(routeEssentials, Is.Not.Empty);
            Assert.That(routeEssentials.All(descriptor =>
                descriptor.RoleTags.Contains(AuthoringContractRoleTags.IntentRouteEssential)), Is.True);
            Assert.That(routeEssentials.Any(descriptor =>
                descriptor.RoleTags.Contains(AuthoringContractRoleTags.CoreRouteAnchor)), Is.True);
            Assert.That(routeEssentials.Any(descriptor =>
                descriptor.RoleTags.Contains(AuthoringContractRoleTags.ParticipantRouteSupport)), Is.True);
            Assert.That(routeEssentials.Any(descriptor =>
                descriptor.RoleTags.Contains(AuthoringContractRoleTags.InputRouteSupport)), Is.True);
            Assert.That(routeEssentials.Any(descriptor =>
                descriptor.RoleTags.Contains(AuthoringContractRoleTags.NetworkRouteSupport)), Is.False);
        }

        [Test]
        public void IntentProjection_SmokeSharedProjectionFeedsJsonLenses()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);
            string[] selectedIds = descriptors
                .Where(descriptor => descriptor.CapabilityPath == "Movement/Traversal/FakeGravityJump"
                    || descriptor.CapabilityPath == "Movement/Sprite2D/Kinetic Motor")
                .Select(descriptor => descriptor.StableId)
                .ToArray();

            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(selectedIds),
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                selectedIds,
                PyralisIntentParticipantRoute.TwoLocalPlayers);
            PyralisAuthoringIntentProjection projection =
                PyralisAuthoringIntentProjection.Build(intentSelection, descriptors);

            string intentJson = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(
                intentSelection,
                PyralisAuthoringSetupGraphProjection.BuildIntentModel(intentSelection),
                projection);

            Assert.That(projection.SelectedDescriptors.Count, Is.EqualTo(selectedIds.Length));
            Assert.That(projection.RouteEssentialGroups.Sum(group => group.SelectedCount), Is.EqualTo(0));
            Assert.That(projection.RouteEssentialGroups.Sum(group => group.InferredCount), Is.GreaterThan(0));
            Assert.That(intentJson, Does.Contain("\"gameplayIngredientGroups\""));
            Assert.That(intentJson, Does.Contain("\"routeEssentialGroups\""));
            Assert.That(intentJson, Does.Contain("\"intentLayer\": \"GameplayIngredient\""));
            Assert.That(intentJson, Does.Contain("\"intentLayer\": \"RouteEssential\""));
        }

        [Test]
        public void IntentAdvisor_SmokeFiltersLooseReflectiveFactsForMovementIntent()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);
            string[] selectedIds = descriptors
                .Where(descriptor => descriptor.CapabilityPath == "Movement/Traversal/FakeGravityJump"
                    || descriptor.CapabilityPath == "Movement/Sprite2D/Kinetic Motor"
                    || descriptor.CapabilityPath == "Movement/Sprite2D/Movement Component")
                .Select(descriptor => descriptor.StableId)
                .ToArray();

            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(selectedIds),
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                selectedIds,
                PyralisIntentParticipantRoute.TwoLocalPlayers);

            PyralisAuthoringIntentModel model = PyralisAuthoringIntentAdvisor.Build(intentSelection);
            string[] recommendationIds = model.Recommendations
                .Select(row => row.Fact.StableId)
                .ToArray();

            Assert.That(recommendationIds, Does.Contain("proof.1p-pawn-movement"));
            Assert.That(recommendationIds.Any(id => id.Contains(".Core.Rpg.", System.StringComparison.Ordinal)), Is.False);
            Assert.That(recommendationIds.Any(id => id.Contains(".Features.Enemies.", System.StringComparison.Ordinal)), Is.False);
            Assert.That(model.Cautions.Any(row => row.Fact.StableId == "proof.npc-enemy-behavior"), Is.False);
        }

        [Test]
        public void IntentProjection_SmokeUsesOnlyContractSemanticPathsForGameplayIngredients()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);

            Assert.That(descriptors.Any(descriptor =>
                descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Vocabulary
                || descriptor.SourceOrigin == PyralisAuthoringGraphSourceOrigin.VocabularyFallback), Is.False);
            Assert.That(descriptors.Where(descriptor =>
                    !PyralisAuthoringCapabilityDescriptorRegistry.IsIntentRouteEssential(descriptor))
                .All(descriptor =>
                    descriptor.IsContractSemanticSource
                    && descriptor.SelectableIntent
                    && !string.IsNullOrWhiteSpace(descriptor.CapabilityPath)), Is.True);
            Assert.That(descriptors.Any(descriptor =>
                descriptor.DisplayName.Contains("IFeatureModuleRuntime", System.StringComparison.Ordinal)
                || descriptor.DisplayName.Contains("IModuleCapabilityRuntime", System.StringComparison.Ordinal)), Is.False);
        }

        [Test]
        public void SetupGraph_ClassifiesContractMetadataOwnershipBeforeRequestingContractFields()
        {
            ResolvedAuthoringContract inferred =
                ResolvedAuthoringContractRegistry.FindByModuleId("test.metadata.inferred-pawn");
            ResolvedAuthoringContract supportOnly =
                ResolvedAuthoringContractRegistry.FindByModuleId("test.metadata.support-interface");
            ResolvedAuthoringContract semantic =
                ResolvedAuthoringContractRegistry.FindByModuleId("test.metadata.semantic-combat");

            Assert.That(inferred, Is.Not.Null);
            Assert.That(supportOnly, Is.Not.Null);
            Assert.That(semantic, Is.Not.Null);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.BuildForContracts(
                null,
                default,
                new[] { inferred, supportOnly, semantic });

            Assert.That(graph.Nodes.Any(node =>
                node.SourceContract == inferred
                && node.IssueCode == "ContractMetadata.RuntimeFamiliesMissing"), Is.False);
            Assert.That(graph.Nodes.Any(node =>
                node.SourceContract == supportOnly
                && node.IssueCode == "ContractMetadata.RuntimeFamiliesMissing"), Is.False);

            PyralisAuthoringGraphNode semanticRuntimeFamily = graph.Nodes.FirstOrDefault(node =>
                node.SourceContract == semantic
                && node.IssueCode == "ContractMetadata.RuntimeFamiliesMissing");
            Assert.That(semanticRuntimeFamily, Is.Not.Null);

            PyralisAuthoringGraphAuditRow semanticRow = PyralisAuthoringSetupGraphProjection
                .BuildHygieneDetailRows(graph)
                .FirstOrDefault(row => row.Node == semanticRuntimeFamily);
            Assert.That(semanticRow, Is.Not.Null);
            Assert.That(semanticRow.OwnershipBucket, Is.EqualTo("ContractOwnedSemanticMetadata"));
            Assert.That(semanticRow.RepairOwner, Is.EqualTo("Contract"));
            Assert.That(semanticRow.OwnershipAdvice, Does.Contain("semantic runtime family"));

            string hygieneJson = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(
                PyralisAuthoringHygieneProjection.Build(null, graph, Array.Empty<PyralisSourceDependencyHygieneRecord>()));
            AssertJsonObjectContaining(
                hygieneJson,
                "\"nodeId\": \"contract-metadata.feature-test-metadata-semantic-combat.runtime-families\"",
                "\"ownershipBucket\": \"ContractOwnedSemanticMetadata\"",
                "\"repairOwner\": \"Contract\"",
                "\"ownershipAdvice\"");
            Assert.That(hygieneJson, Does.Not.Contain("contract-metadata.feature-test-metadata-inferred-pawn.runtime-families"));
            Assert.That(hygieneJson, Does.Not.Contain("contract-metadata.feature-test-metadata-support-interface.runtime-families"));
        }

        [Test]
        public void SetupGraph_SmokeDuplicateOwnershipClaimsAppearInHygiene()
        {
            ResolvedAuthoringContract first =
                ResolvedAuthoringContractRegistry.FindByModuleId("test.ownership.first");
            ResolvedAuthoringContract second =
                ResolvedAuthoringContractRegistry.FindByModuleId("test.ownership.second");

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(first.OwnershipClaims, Does.Contain("test.runtime.owner"));
            Assert.That(second.OwnershipClaims, Does.Contain("test.runtime.owner"));

            ResolvedAuthoringContract[] fixtureContracts = { first, second };
            PyralisAuthoringSetupGraph graph =
                PyralisAuthoringSetupGraphBuilder.BuildForContracts(null, default, fixtureContracts);
            PyralisAuthoringGraphNode duplicateOwner = graph.Nodes.FirstOrDefault(node =>
                node.IssueCode == "ContractMetadata.DuplicateOwnershipClaim"
                && node.Guidance.Contains("test.runtime.owner", System.StringComparison.Ordinal));

            Assert.That(duplicateOwner, Is.Not.Null);
            Assert.That(duplicateOwner.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.AuthoringContract));
            Assert.That(duplicateOwner.SourceOrigin, Is.EqualTo(PyralisAuthoringGraphSourceOrigin.Contract));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildHygieneSections(graph)
                .Any(section => section.Label == "Missing Contract Metadata"
                    && section.Rows.Any(row => row.NodeId == duplicateOwner.StableId)), Is.True);
            Assert.That(graph.Edges.Any(edge =>
                edge.FromNodeId == "contract.feature.test.ownership.first"
                && edge.ToNodeId == duplicateOwner.StableId
                && edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy), Is.True);
            Assert.That(graph.Edges.Any(edge =>
                edge.FromNodeId == "contract.feature.test.ownership.second"
                && edge.ToNodeId == duplicateOwner.StableId
                && edge.Kind == PyralisAuthoringGraphEdgeKind.BlockedBy), Is.True);
        }

        [Test]
        public void SetupGraph_SmokeProductGraphExcludesTestFixtureContracts()
        {
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null);
            string hygieneJson = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(
                PyralisAuthoringHygieneProjection.Build(null, graph, Array.Empty<PyralisSourceDependencyHygieneRecord>()));

            Assert.That(ResolvedAuthoringContractRegistry.ProductContracts.Any(
                ResolvedAuthoringContractRegistry.IsTestContract), Is.False);
            Assert.That(hygieneJson, Does.Not.Contain("test.runtime.owner"));
            Assert.That(hygieneJson, Does.Not.Contain("test.ownership.first"));
            Assert.That(hygieneJson, Does.Not.Contain("test.ownership.second"));
        }

        [Test]
        public void SetupGraph_SmokeSelectedContractDescriptorOwnsProofTarget()
        {
            string selectedId = "feature.actor.traversal.topdown-hop";
            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(new[] { selectedId }),
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                new[] { selectedId },
                PyralisIntentParticipantRoute.SoloLocal);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intentSelection);
            PyralisAuthoringGuideTraceProjection trace =
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph);
            string guideJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(trace);

            Assert.That(guideJson, Does.Contain("\"proofTargetId\": \"proof.1p-pawn-movement\""));
            Assert.That(graph.Nodes.Any(node =>
                node.ProofTargetId == "proof.1p-pawn-movement"
                && node.SourceOrigin == PyralisAuthoringGraphSourceOrigin.Contract), Is.True);
            Assert.That(graph.Nodes.Any(node =>
                node.IssueCode == "ContractMetadata.ProofTargetGenericTemplate"
                && node.ProofTargetId == "proof.1p-pawn-movement"), Is.False);
        }

        [Test]
        public void IntentProjection_SmokeParticipantRouteSteersGraphBeforeSetupExists()
        {
            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.Participants,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                participantRoute: PyralisIntentParticipantRoute.TwoLocalPlayers);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intent);
            string mapJson = PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
            PyralisAuthoringGuideTraceProjection guideTrace =
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph);
            string guideJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(guideTrace);

            Assert.That(mapJson, Does.Contain("\"participantTopology\": \"LocalJoin\""));
            Assert.That(mapJson, Does.Contain("\"expectedJoinPolicy\": \"PlayerInputJoin\""));
            Assert.That(mapJson, Does.Contain("\"assignedParticipantCount\": 2"));
            Assert.That(mapJson, Does.Contain("\"authoredParticipantCount\": 0"));
            Assert.That(mapJson, Does.Contain("\"desiredParticipantCount\": 2"));
            Assert.That(mapJson, Does.Contain("\"hasParticipants\": false"));
            Assert.That(mapJson, Does.Contain("\"participantSeats\""));
            Assert.That(guideJson, Does.Contain("\"label\": \"Participants\""));
            Assert.That(guideJson, Does.Contain("\"evidenceState\": \"Missing\""));
            Assert.That(guideJson, Does.Contain("2 previewed from Intent"));
            Assert.That(guideJson, Does.Contain("\"proofTargetId\": \"proof.local-pawn-join\""));
            Assert.That(guideJson, Does.Contain("Local Co-op Pawn Join Proof"));
            Assert.That(guideJson, Does.Not.Contain("Why is the route empty?"));
            Assert.That(guideTrace.Route.CriticalPath.First(row => row.StableId == "route.shape").NativeSetup, Is.Not.Empty);
            Assert.That(guideTrace.Route.CriticalPath.First(row => row.StableId == "pawn.definition").NativeSetup, Is.Not.Empty);
            Assert.That(graph.Nodes.Any(node =>
                node.IssueCode == "ContractMetadata.ProofTargetGenericTemplate"
                && node.SourceOrigin == PyralisAuthoringGraphSourceOrigin.VocabularyFallback), Is.True);
        }

        [Test]
        public void IntentProjection_SmokePrunesLaneHiddenSelectedDescriptors()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> spriteDescriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);
            string spriteOnlyId = spriteDescriptors
                .First(descriptor => descriptor.StableId == "feature.NeonBlack.Gameplay.Modules.Character.Pawn2DPresentationComponent")
                .StableId;

            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> billboardDescriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(
                    RuntimeCapabilityLaneTag.Billboard2_5D,
                    AuthoringWorldAxiom.Dimensions3D | AuthoringWorldAxiom.GravityVertical | AuthoringWorldAxiom.Realtime);
            PyralisAuthoringIntentSelection billboardIntent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Billboard2_5D,
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(new[] { spriteOnlyId }),
                AuthoringWorldAxiom.Dimensions3D | AuthoringWorldAxiom.GravityVertical | AuthoringWorldAxiom.Realtime,
                new[] { spriteOnlyId },
                PyralisIntentParticipantRoute.FourLocalPlayers);

            PyralisAuthoringIntentProjection projection =
                PyralisAuthoringIntentProjection.Build(billboardIntent, billboardDescriptors);
            string intentJson = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(
                billboardIntent,
                PyralisAuthoringSetupGraphProjection.BuildIntentModel(billboardIntent),
                projection);

            Assert.That(billboardDescriptors.Any(descriptor => descriptor.StableId == spriteOnlyId), Is.False);
            Assert.That(projection.SelectedDescriptorIds, Does.Not.Contain(spriteOnlyId));
            Assert.That(projection.SelectedDescriptors.Any(descriptor => descriptor.StableId == spriteOnlyId), Is.False);
            Assert.That(intentJson, Does.Not.Contain(spriteOnlyId));
        }

        [Test]
        public void IntentProjection_SmokePreservesAuthoredAndDesiredParticipantCounts()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultParticipants = new[]
            {
                ScriptableObject.CreateInstance<ParticipantDefinition>(),
                ScriptableObject.CreateInstance<ParticipantDefinition>(),
                ScriptableObject.CreateInstance<ParticipantDefinition>(),
                ScriptableObject.CreateInstance<ParticipantDefinition>()
            };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input | AuthoringCapability.Participants,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                participantRoute: PyralisIntentParticipantRoute.TwoLocalPlayers);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);
            string mapJson = PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
            string guideJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));

            Assert.That(mapJson, Does.Contain("\"routeName\": \"Local Co-op Pawn route\""));
            Assert.That(mapJson, Does.Contain("\"assignedParticipantCount\": 4"));
            Assert.That(mapJson, Does.Contain("\"authoredParticipantCount\": 4"));
            Assert.That(mapJson, Does.Contain("\"desiredParticipantCount\": 2"));
            Assert.That(guideJson, Does.Contain("4 authored in SessionDefinition, 2 requested by Intent"));
        }

        [Test]
        public void MapProjection_DoesNotLetSceneSurfaceIssuesContaminateGameplayRootRow()
        {
            PyralisAuthoringGraphNode gameplayRoot = new PyralisAuthoringGraphNode(
                "bootstrap.root",
                "Gameplay Root",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                PyralisAuthoringGraphEvidenceState.Ready,
                guidance: "Gameplay Root is assigned.");
            PyralisAuthoringGraphNode sceneSurfaces = new PyralisAuthoringGraphNode(
                "scene.surfaces",
                "Scene Surfaces",
                PyralisAuthoringGraphNodeKind.SceneSurface,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "One scene surface is missing.");
            PyralisAuthoringGraphNode missingUi = new PyralisAuthoringGraphNode(
                "scene.ui",
                "UI / HUD / Menus",
                PyralisAuthoringGraphNodeKind.SceneSurface,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "No Canvas");
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                null,
                null,
                new[] { gameplayRoot, sceneSurfaces, missingUi },
                new[]
                {
                    new PyralisAuthoringGraphEdge("bootstrap.root", "scene.ui", PyralisAuthoringGraphEdgeKind.RelatesTo, "scene surface"),
                    new PyralisAuthoringGraphEdge("bootstrap.root", "scene.surfaces", PyralisAuthoringGraphEdgeKind.RelatesTo, "scene surface summary")
                });

            IReadOnlyList<PyralisAuthoringSetupGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph);
            PyralisAuthoringSetupGraphRow rootRow = rows.First(row => row.Label == "Gameplay Root");
            PyralisAuthoringSetupGraphRow sceneRow = rows.First(row => row.Label == "Scene Surfaces");

            Assert.That(rootRow.EffectiveEvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(rootRow.Message, Does.Not.Contain("No Canvas"));
            Assert.That(sceneRow.EffectiveEvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(sceneRow.Message, Does.Contain("One scene surface"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph).Select(row => row.NodeId), Does.Contain("scene.ui"));
        }

        [Test]
        public void MapProjection_SmokeUsesTypedDomainInsteadOfSourceKindForLinkedIssues()
        {
            PyralisAuthoringGraphNode gameplayRoot = new PyralisAuthoringGraphNode(
                "bootstrap.root",
                "Gameplay Root",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                PyralisAuthoringGraphEvidenceState.Ready,
                setupDomain: PyralisAuthoringGraphSetupDomain.GameplayRoot,
                guidance: "Gameplay Root is assigned.");
            PyralisAuthoringGraphNode sceneSurfaces = new PyralisAuthoringGraphNode(
                "scene.surfaces",
                "Scene Surfaces",
                PyralisAuthoringGraphNodeKind.SceneSurface,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Ready,
                setupDomain: PyralisAuthoringGraphSetupDomain.SceneSurface,
                guidance: "Scene surfaces are present.");
            PyralisAuthoringGraphNode misleadingSceneIssue = new PyralisAuthoringGraphNode(
                "setup.fake-core-scene-surface",
                "Scene Surface",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                PyralisAuthoringGraphEvidenceState.Missing,
                setupDomain: PyralisAuthoringGraphSetupDomain.SceneSurface,
                workIntent: PyralisAuthoringGraphWorkIntent.RequiredSetup,
                guidance: "A scene surface is missing.");
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                null,
                null,
                new[] { gameplayRoot, sceneSurfaces, misleadingSceneIssue },
                new[]
                {
                    new PyralisAuthoringGraphEdge("bootstrap.root", misleadingSceneIssue.StableId, PyralisAuthoringGraphEdgeKind.RelatesTo, "misleading source"),
                    new PyralisAuthoringGraphEdge("scene.surfaces", misleadingSceneIssue.StableId, PyralisAuthoringGraphEdgeKind.RelatesTo, "typed scene domain")
                });

            IReadOnlyList<PyralisAuthoringSetupGraphRow> rows = PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph);
            PyralisAuthoringSetupGraphRow rootRow = rows.First(row => row.Label == "Gameplay Root");
            PyralisAuthoringSetupGraphRow sceneRow = rows.First(row => row.Label == "Scene Surfaces");

            Assert.That(rootRow.EffectiveEvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(rootRow.Message, Does.Not.Contain("scene surface is missing"));
            Assert.That(sceneRow.EffectiveEvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(sceneRow.Message, Does.Contain("scene surface is missing"));
        }

        [Test]
        public void RouteProjection_SmokeUsesTypedDomainInsteadOfSourceKindForRouteStepPhase()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            PyralisAuthoringGraphNode sessionNode = new PyralisAuthoringGraphNode(
                "setup.misleading-runtime-session",
                "Assign Session",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.RuntimeValidation,
                PyralisAuthoringGraphEvidenceState.Missing,
                setupDomain: PyralisAuthoringGraphSetupDomain.Session,
                workIntent: PyralisAuthoringGraphWorkIntent.RequiredSetup,
                guidance: "Assign a SessionDefinition.");
            PyralisAuthoringGraphNode proof = new PyralisAuthoringGraphNode(
                "proof.1p-pawn-movement",
                "Pawn Movement Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.ProofVocabulary,
                PyralisAuthoringGraphEvidenceState.Missing);
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                session,
                null,
                new[] { sessionNode, proof },
                System.Array.Empty<PyralisAuthoringGraphEdge>());

            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            PyralisAuthoringRouteStepRow row = route.CriticalPath.FirstOrDefault(step => step.StableId == sessionNode.StableId);

            Assert.That(row, Is.Not.Null);
            Assert.That(row.Phase, Is.EqualTo(PyralisAuthoringRouteStepPhase.SetupChain));
            Assert.That(row.Role, Is.EqualTo(PyralisAuthoringRouteStepRole.BlocksProof));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void RouteProjection_SmokeProofBlockerBecomesCurrentActionWhenCriticalPathIsClear()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            PyralisAuthoringGraphNode proof = new PyralisAuthoringGraphNode(
                "proof.1p-pawn-movement",
                "Pawn Movement Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.ProofVocabulary,
                PyralisAuthoringGraphEvidenceState.Blocked,
                guidance: "Move one pawn in Play Mode.");
            PyralisAuthoringGraphNode sceneBlocker = new PyralisAuthoringGraphNode(
                "scenereadiness.sceneroot.requiredbeforeplay.test",
                "SceneRoot",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Blocked,
                setupDomain: PyralisAuthoringGraphSetupDomain.SceneReadiness,
                workIntent: PyralisAuthoringGraphWorkIntent.RequiredSetup,
                guidance: "Gameplay Root is missing SessionStateService.");
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                session,
                null,
                new[] { proof, sceneBlocker },
                new[]
                {
                    new PyralisAuthoringGraphEdge(proof.StableId, sceneBlocker.StableId, PyralisAuthoringGraphEdgeKind.BlockedBy, "missing required setup")
                });

            PyralisAuthoringRouteWorkingProjection projection =
                PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));

            Assert.That(projection.ProofBlockers, Has.Count.EqualTo(1));
            Assert.That(projection.CurrentAction, Is.Not.Null);
            Assert.That(projection.CurrentAction.Node.StableId, Is.EqualTo(sceneBlocker.StableId));
            Assert.That(projection.CurrentAction.Role, Is.EqualTo(PyralisAuthoringRouteStepRole.BlocksProof));
            Assert.That(traceJson, Does.Contain("\"currentActionLabel\": \"SceneRoot\""));
            Assert.That(traceJson, Does.Contain("\"currentActionNodeId\": \"scenereadiness.sceneroot.requiredbeforeplay.test\""));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void RouteProjection_SmokeSceneReadinessBlockersResolveToConcreteSetupRequirements()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            PyralisAuthoringGraphNode proof = new PyralisAuthoringGraphNode(
                "proof.1p-pawn-movement",
                "Pawn Movement Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.ProofVocabulary,
                PyralisAuthoringGraphEvidenceState.Blocked,
                guidance: "Move one pawn in Play Mode.");
            PyralisAuthoringGraphNode sceneBlocker = new PyralisAuthoringGraphNode(
                "scenereadiness.sceneroot.requiredbeforeplay.session-state-service",
                "SceneRoot",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Blocked,
                setupDomain: PyralisAuthoringGraphSetupDomain.GameplayRoot,
                workIntent: PyralisAuthoringGraphWorkIntent.RequiredSetup,
                issueSeverity: PyralisAuthoringIssueSeverity.Required,
                issueCode: "SceneReadiness.SceneRoot",
                guidance: "Gameplay Root is missing authored core runtime service `SessionStateService`. Add a child GameObject with SessionStateService, or assign Bootstrap > Session State Service before Play Mode.",
                nativeAction: PyralisAuthoringNativeActionFactory.AddComponentAction(
                    "Gameplay Root",
                    "SessionStateService",
                    "Gameplay Root.SessionStateService is present before Play Mode."));
            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                session,
                null,
                new[] { proof, sceneBlocker },
                new[]
                {
                    new PyralisAuthoringGraphEdge(proof.StableId, sceneBlocker.StableId, PyralisAuthoringGraphEdgeKind.BlockedBy, "missing required setup")
                });

            IReadOnlyList<PyralisAuthoringResolvedSetupRequirement> requirements =
                PyralisAuthoringSetupGraphProjection.BuildResolvedSetupRequirements(graph);
            PyralisAuthoringRouteWorkingProjection route =
                PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            IReadOnlyList<PyralisAuthoringOverviewIssue> overviewIssues =
                PyralisAuthoringSetupGraphProjection.BuildOverviewIssues(graph, session);
            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));

            Assert.That(requirements, Has.Count.EqualTo(1));
            Assert.That(requirements[0].Label, Is.EqualTo("Add SessionStateService"));
            Assert.That(requirements[0].OwnerLabel, Is.EqualTo("Gameplay Root.SessionStateService"));
            Assert.That(route.CurrentAction, Is.Not.Null);
            Assert.That(route.CurrentAction.Label, Is.EqualTo("Add SessionStateService"));
            Assert.That(route.CurrentAction.OwnerLabel, Is.EqualTo("Gameplay Root.SessionStateService"));
            Assert.That(overviewIssues[0].Label, Is.EqualTo("Add SessionStateService"));
            Assert.That(traceJson, Does.Contain("\"currentActionLabel\": \"Add SessionStateService\""));
            Assert.That(traceJson, Does.Contain("\"owner\": \"Gameplay Root.SessionStateService\""));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void AuthoringLabelUtility_SmokeNormalizesNativeActionVocabulary()
        {
            PyralisAuthoringNativeAction addService = PyralisAuthoringNativeActionFactory.AddComponentAction(
                "Gameplay Root",
                "SessionStateService",
                "Gameplay Root.SessionStateService is present before Play Mode.");
            PyralisAuthoringNativeAction createAsset = PyralisAuthoringNativeActionFactory.CreateAssetAction(
                "PawnDefinition",
                "NeonBlack -> Definitions -> Pawn Definition",
                "ParticipantDefinition.defaultPawn references the asset.");
            PyralisAuthoringNativeAction createSceneObject = PyralisAuthoringNativeActionFactory.CreateSceneObjectAction(
                "Gameplay Root",
                "GameplaySessionBootstrap",
                "Overview can inspect the active setup route.",
                "name it Gameplay Root");

            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionFieldOrComponentName(addService), Is.EqualTo("SessionStateService"));
            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionDisplayLabel(addService), Is.EqualTo("Add SessionStateService"));
            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionOwnerLabel(addService), Is.EqualTo("Gameplay Root.SessionStateService"));
            Assert.That(
                PyralisAuthoringLabelUtility.FormatNativeAction(addService),
                Is.EqualTo("Add - Gameplay Root - SessionStateService - Gameplay Root.SessionStateService is present before Play Mode."));
            Assert.That(
                addService.ToGuidanceSentence(),
                Does.Contain("use Add Component > Session State Service"));

            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionFieldOrComponentName(createAsset), Is.EqualTo("Pawn Definition"));
            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionDisplayLabel(createAsset), Is.EqualTo("Create PawnDefinition"));
            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionOwnerLabel(createAsset), Is.EqualTo("Project.Pawn Definition"));
            Assert.That(
                PyralisAuthoringLabelUtility.FormatNativeAction(createAsset),
                Is.EqualTo("Create - PawnDefinition - Pawn Definition - ParticipantDefinition.defaultPawn references the asset."));

            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionFieldOrComponentName(createSceneObject), Is.EqualTo("GameplaySessionBootstrap"));
            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionDisplayLabel(createSceneObject), Is.EqualTo("Create or select Gameplay Root"));
            Assert.That(PyralisAuthoringLabelUtility.GetNativeActionOwnerLabel(createSceneObject), Is.EqualTo("Gameplay Root.GameplaySessionBootstrap"));
            Assert.That(
                createSceneObject.ToGuidanceSentence(),
                Does.Contain("use Create/select scene object + Add Component > Gameplay Session Bootstrap; name it Gameplay Root"));
        }

        [Test]
        public void SetupGraphJsonExport_SmokeSerializesGraphAndTabProjections()
        {
            PyralisAuthoringGraphNode missingSetup = new PyralisAuthoringGraphNode(
                "setup.session",
                "Session",
                PyralisAuthoringGraphNodeKind.SetupChain,
                PyralisAuthoringGraphSourceKind.CoreSetup,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "Assign a SessionDefinition.");
            PyralisAuthoringGraphNode proof = new PyralisAuthoringGraphNode(
                "proof.1p",
                "1P Proof",
                PyralisAuthoringGraphNodeKind.Proof,
                PyralisAuthoringGraphSourceKind.ProofVocabulary,
                PyralisAuthoringGraphEvidenceState.Missing);
            PyralisAuthoringGraphNode sceneIssue = new PyralisAuthoringGraphNode(
                "validation.input-profile",
                "Input Profile",
                PyralisAuthoringGraphNodeKind.ValidationEvidence,
                PyralisAuthoringGraphSourceKind.SceneReadiness,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "Assign the participant input profile.");
            PyralisAuthoringGraphNode graphIssue = new PyralisAuthoringGraphNode(
                "graph.assignment.coverage",
                "Graph Assignment Coverage",
                PyralisAuthoringGraphNodeKind.AssignmentField,
                PyralisAuthoringGraphSourceKind.VocabularyRegistry,
                PyralisAuthoringGraphEvidenceState.Missing,
                guidance: "Graph projection lacks assignment coverage.");

            PyralisAuthoringSetupGraph graph = new PyralisAuthoringSetupGraph(
                null,
                null,
                new[] { missingSetup, proof, sceneIssue, graphIssue },
                new[]
                {
                    new PyralisAuthoringGraphEdge("proof.1p", "setup.session", PyralisAuthoringGraphEdgeKind.BlockedBy, "missing setup"),
                    new PyralisAuthoringGraphEdge("proof.1p", "graph.assignment.coverage", PyralisAuthoringGraphEdgeKind.BlockedBy, "graph coverage")
                });

            string mapJson = PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
            var hygieneRecords = new[]
            {
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Platform/Session/HeavyRuntime.cs",
                    "using NeonBlack.Gameplay.Modules.Input; using NeonBlack.Gameplay.Modules.Combat; class HeavyRuntime { void Tick() { UnityEngine.Object.FindAnyObjectByType<UnityEngine.Transform>(); } }")
            };
            var noRouteHygieneRecords = new[]
            {
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Platform/Session/NoRoutePressure.cs",
                    "using NeonBlack.Gameplay.Modules.Input; using NeonBlack.Gameplay.Modules.Combat; class NoRoutePressure { }")
            };
            PyralisAuthoringHygieneProjection hygieneProjection =
                PyralisAuthoringHygieneProjection.Build(null, graph, hygieneRecords);
            string hygieneJson = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(hygieneProjection);
            string noRouteHygieneJson = PyralisAuthoringSetupGraphJsonExporter.ToHygieneJson(
                PyralisAuthoringHygieneProjection.Build(null, null, noRouteHygieneRecords));
            string guideJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));
            var intentSelection = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime,
                participantRoute: PyralisIntentParticipantRoute.TwoLocalPlayers);
            string intentJson = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(
                intentSelection,
                PyralisAuthoringSetupGraphProjection.BuildIntentModel(intentSelection),
                PyralisAuthoringIntentProjection.Build(
                    intentSelection,
                    PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(intentSelection.Lane, intentSelection.Axioms)));
            string factsJson = PyralisAuthoringSetupGraphJsonExporter.ToFactsJson(graph);
            IReadOnlyList<PyralisAuthoringFact> factsProjectionFacts =
                PyralisAuthoringSetupGraphProjection.BuildCookbookFacts(graph);

            Assert.That(mapJson, Does.Contain("pyralis.authoring.mapSnapshot.v1"));
            Assert.That(mapJson, Does.Contain("\"view\": \"Map\""));
            Assert.That(mapJson, Does.Contain("\"nodes\""));
            Assert.That(mapJson, Does.Contain("\"edges\""));
            Assert.That(mapJson, Does.Contain("\"summary\""));
            Assert.That(mapJson, Does.Contain("\"mapRowCount\""));
            Assert.That(mapJson, Does.Contain("\"sceneSetupIssueCount\""));
            Assert.That(mapJson, Does.Contain("\"currentRoute\""));
            Assert.That(mapJson, Does.Contain("\"mapRows\""));
            Assert.That(mapJson, Does.Contain("\"mapConnections\""));
            Assert.That(mapJson, Does.Contain("\"sceneSetupIssues\""));
            Assert.That(mapJson, Does.Contain("validation.input-profile"));
            Assert.That(mapJson, Does.Not.Contain("graph.assignment.coverage"));
            Assert.That(mapJson, Does.Not.Contain("\"hygieneSections\""));

            Assert.That(hygieneJson, Does.Contain("pyralis.authoring.hygieneSnapshot.v2"));
            Assert.That(hygieneJson, Does.Contain("\"view\": \"Hygiene\""));
            Assert.That(hygieneJson, Does.Contain("\"graphContext\""));
            Assert.That(hygieneJson, Does.Contain("\"graphSummary\""));
            Assert.That(hygieneJson, Does.Contain("\"summary\""));
            Assert.That(hygieneJson, Does.Contain("\"hygieneRowCount\""));
            Assert.That(hygieneJson, Does.Contain("\"cleanupFocusCount\""));
            Assert.That(hygieneJson, Does.Contain("\"hygieneSections\""));
            Assert.That(hygieneJson, Does.Contain("\"hygieneRows\""));
            Assert.That(hygieneJson, Does.Contain("\"proofBlockers\""));
            Assert.That(hygieneJson, Does.Contain("\"sourceOriginCounts\""));
            Assert.That(hygieneJson, Does.Contain("\"ownershipBucketCounts\""));
            Assert.That(hygieneJson, Does.Contain("\"dependencyPressureSummary\""));
            Assert.That(hygieneJson, Does.Contain("\"pressureKindCounts\""));
            Assert.That(hygieneJson, Does.Contain("\"omittedDependencyPressureCount\": 0"));
            Assert.That(hygieneJson, Does.Contain("\"omittedRecordCount\": 0"));
            Assert.That(hygieneJson, Does.Contain("\"cleanupFocus\""));
            Assert.That(hygieneJson, Does.Contain("\"watchList\""));
            Assert.That(hygieneJson, Does.Contain("\"dependencyPressure\""));
            Assert.That(hygieneJson, Does.Contain("\"pressureKind\""));
            Assert.That(hygieneJson, Does.Contain("\"reviewHint\""));
            Assert.That(hygieneJson, Does.Contain("\"issueCode\""));
            Assert.That(hygieneJson, Does.Contain("\"triageBucket\""));
            Assert.That(hygieneJson, Does.Contain("\"triageAdvice\""));
            Assert.That(hygieneJson, Does.Contain("\"ownershipBucket\""));
            Assert.That(hygieneJson, Does.Contain("\"repairOwner\""));
            Assert.That(hygieneJson, Does.Contain("\"ownershipAdvice\""));
            Assert.That(hygieneJson, Does.Contain("\"nativeAction\""));
            Assert.That(hygieneJson, Does.Contain("\"canInspectTarget\""));
            Assert.That(hygieneJson, Does.Contain("\"target\""));
            AssertJsonObjectContaining(
                hygieneJson,
                "\"nodeId\": \"graph.assignment.coverage\"",
                "\"issueCode\"",
                "\"triageBucket\"",
                "\"triageAdvice\"",
                "\"nativeAction\"",
                "\"canInspectTarget\"",
                "\"target\"");
            Assert.That(hygieneJson, Does.Contain("\"localComponentLookupCount\""));
            Assert.That(hygieneJson, Does.Contain("\"broadUnityDiscoveryCount\""));
            Assert.That(hygieneJson, Does.Contain("\"contractSourcePressure\""));
            Assert.That(hygieneJson, Does.Contain("\"declaredRuntimeFamilies\""));
            Assert.That(hygieneJson, Does.Contain("\"inferredRuntimeFamilies\""));
            Assert.That(hygieneJson, Does.Contain("\"supportOnly\""));
            Assert.That(hygieneJson, Does.Contain("HeavyRuntime.cs"));
            Assert.That(hygieneJson, Does.Not.Contain("NoRoutePressure.cs"));
            Assert.That(hygieneJson, Does.Not.Contain("validation.input-profile"));
            Assert.That(hygieneJson, Does.Not.Contain("setup.session"));
            Assert.That(hygieneJson, Does.Contain("graph.assignment.coverage"));
            Assert.That(hygieneJson, Does.Not.Contain("\"mapRows\""));

            Assert.That(noRouteHygieneJson, Does.Contain("pyralis.authoring.hygieneSnapshot.v2"));
            Assert.That(noRouteHygieneJson, Does.Contain("\"hasGraph\": false"));
            Assert.That(noRouteHygieneJson, Does.Contain("\"graphName\": \"No active setup graph\""));
            Assert.That(noRouteHygieneJson, Does.Not.Contain("\"routeName\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"dependencyPressureSummary\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"dependencyPressure\""));
            Assert.That(noRouteHygieneJson, Does.Contain("\"nodeCount\": 0"));
            Assert.That(noRouteHygieneJson, Does.Contain("NoRoutePressure.cs"));
            Assert.That(noRouteHygieneJson, Does.Not.Contain("HeavyRuntime.cs"));
            Assert.That(noRouteHygieneJson, Does.Not.Contain("\"mapRows\""));

            Assert.That(guideJson, Does.Contain("pyralis.authoring.guide.v1"));
            Assert.That(guideJson, Does.Contain("\"view\": \"Guide\""));
            Assert.That(guideJson, Does.Contain("\"summary\""));
            Assert.That(guideJson, Does.Contain("\"currentActionLabel\""));
            Assert.That(guideJson, Does.Contain("\"criticalPathCount\""));
            Assert.That(guideJson, Does.Contain("\"proof\""));
            Assert.That(guideJson, Does.Contain("\"hasProof\""));
            Assert.That(guideJson, Does.Contain("\"readyForAttempt\""));
            Assert.That(guideJson, Does.Contain("\"currentAction\""));
            Assert.That(guideJson, Does.Contain("\"hasAction\""));
            Assert.That(guideJson, Does.Contain("\"orderedSteps\""));
            Assert.That(guideJson, Does.Contain("\"criticalPath\""));
            Assert.That(guideJson, Does.Contain("\"proofEnhancers\""));
            Assert.That(guideJson, Does.Contain("\"canWait\""));
            Assert.That(guideJson, Does.Contain("\"proofBlockers\""));
            Assert.That(guideJson, Does.Contain("\"proofSupport\""));
            Assert.That(guideJson, Does.Contain("\"diagnosticQuestions\""));
            Assert.That(guideJson, Does.Contain("\"question\": \"What is the next route action?\""));
            Assert.That(guideJson, Does.Contain("\"question\": \"Where should incorrect guidance be fixed?\""));
            Assert.That(guideJson, Does.Contain("setup.session"));
            Assert.That(guideJson, Does.Not.Contain("validation.input-profile"));
            Assert.That(guideJson, Does.Not.Contain("\"mapRows\""));
            Assert.That(guideJson, Does.Not.Contain("\"hygieneSections\""));
            Assert.That(guideJson, Does.Not.Contain("\"blockingReason\""));

            Assert.That(intentJson, Does.Contain("pyralis.authoring.intentSnapshot.v1"));
            Assert.That(intentJson, Does.Contain("\"view\": \"Intent\""));
            Assert.That(intentJson, Does.Contain("\"selection\""));
            Assert.That(intentJson, Does.Contain("\"participantRoute\": \"TwoLocalPlayers\""));
            Assert.That(intentJson, Does.Contain("\"descriptorGroups\""));
            Assert.That(intentJson, Does.Contain("\"gameplayIngredientGroups\""));
            Assert.That(intentJson, Does.Contain("\"metadataBacklogGroups\""));
            Assert.That(intentJson, Does.Contain("\"metadataBacklogCount\""));
            Assert.That(intentJson, Does.Contain("\"routeEssentialGroups\""));
            Assert.That(intentJson, Does.Contain("\"shapeSummary\""));
            Assert.That(intentJson, Does.Contain("\"routeShapePreview\""));
            Assert.That(intentJson, Does.Contain("\"lensSummary\""));
            Assert.That(intentJson, Does.Contain("\"advisorSummary\""));
            Assert.That(intentJson, Does.Contain("\"targetProofFocus\""));
            Assert.That(intentJson, Does.Contain("\"targetProofSummary\""));
            Assert.That(intentJson, Does.Contain("\"Local Co-op Pawn Join Proof\""));
            Assert.That(intentJson, Does.Contain("Single-pawn movement recommendations are foundation pieces"));
            Assert.That(intentJson, Does.Contain("\"recommendations\""));
            Assert.That(intentJson, Does.Contain("\"runtimeFamilies\""));
            Assert.That(intentJson, Does.Not.Contain("\"requiredSetup\""));
            Assert.That(intentJson, Does.Not.Contain("\"assignmentFields\""));
            Assert.That(intentJson, Does.Not.Contain("\"nativeActions\""));
            Assert.That(intentJson, Does.Not.Contain("\"mapRows\""));
            Assert.That(intentJson, Does.Not.Contain("\"hygieneSections\""));

            Assert.That(factsJson, Does.Contain("pyralis.authoring.factsSnapshot.v1"));
            Assert.That(factsJson, Does.Contain("\"view\": \"Facts\""));
            Assert.That(factsJson, Does.Contain("\"factKindCounts\""));
            Assert.That(factsJson, Does.Contain("\"sourceKindCounts\""));
            Assert.That(factsJson, Does.Contain("\"graphContractCoverage\""));
            Assert.That(factsJson, Does.Contain("\"facts\""));
            Assert.That(factsJson, Does.Not.Contain("\"capabilityCounts\""));
            Assert.That(factsJson, Does.Not.Contain("\"graphProofCoverage\""));
            Assert.That(factsJson, Does.Not.Contain("\"proofSupportCount\""));
            Assert.That(factsJson, Does.Not.Contain("\"kind\": \"RouteIntent\""));
            Assert.That(factsJson, Does.Not.Contain("\"kind\": \"RuntimeCapability\""));
            Assert.That(factsJson, Does.Not.Contain("\"kind\": \"CustomizationMoment\""));
            Assert.That(factsJson, Does.Not.Contain("\"kind\": \"Proof\""));
            Assert.That(factsJson, Does.Not.Contain("\"capability\""));
            Assert.That(factsJson, Does.Not.Contain("\"axioms\""));
            Assert.That(factsJson, Does.Not.Contain("\"routeRelevance\""));
            Assert.That(factsJson, Does.Not.Contain("\"workIntent\""));
            Assert.That(factsJson, Does.Not.Contain("\"goalTags\""));
            Assert.That(factsJson, Does.Not.Contain("\"laneTags\""));
            Assert.That(factsJson, Does.Not.Contain("\"unsupportedLaneTags\""));
            Assert.That(factsJson, Does.Not.Contain("\"customizationMoments\""));
            Assert.That(factsJson, Does.Not.Contain("\"proof\""));
            Assert.That(factsJson, Does.Not.Contain("\"assignmentFields\""));
            Assert.That(factsJson, Does.Not.Contain("\"nativeActions\""));
            Assert.That(factsJson, Does.Not.Contain("\"fieldOrComponent\""));
            Assert.That(factsJson, Does.Not.Contain("\"mapRows\""));
            Assert.That(factsJson, Does.Not.Contain("\"hygieneSections\""));
            Assert.That(factsJson, Does.Not.Contain("\"InspectorGuide\""));
            Assert.That(factsJson, Does.Not.Contain("\"inspector."));
            Assert.That(factsProjectionFacts.Any(fact => fact.Kind == PyralisAuthoringFactKind.RouteIntent), Is.False);
            Assert.That(factsProjectionFacts.Any(fact => fact.Kind == PyralisAuthoringFactKind.RuntimeCapability), Is.False);
            Assert.That(factsProjectionFacts.Any(fact => fact.Kind == PyralisAuthoringFactKind.CustomizationMoment), Is.False);
            Assert.That(factsProjectionFacts.Any(fact => fact.Kind == PyralisAuthoringFactKind.Proof), Is.False);
        }

        [Test]
        public void ReflectiveFactScanner_SmokeDoesNotFallbackRepairCoreSetupRelations()
        {
            PyralisAuthoringFact[] reflectionFacts = PyralisReflectiveFactScanner.ScanProject()
                .Where(fact => fact.SourceKind == PyralisAuthoringFactSourceKind.Reflection)
                .ToArray();
            string[] reflectionRelatedStableIds = reflectionFacts
                .SelectMany(fact => fact.RelatedStableIds)
                .ToArray();

            Assert.That(reflectionRelatedStableIds, Does.Not.Contain("setup.assign-session-definition"));
            Assert.That(reflectionRelatedStableIds, Does.Not.Contain("setup.assign-default-game-mode"));
            Assert.That(reflectionRelatedStableIds, Does.Not.Contain("setup.assign-default-participants"));
        }

        [Test]
        public void SetupGraph_SmokePawnIntentBlocksWhenParticipantInputProfileIsMissing()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            CameraRigProfile cameraProfile = ScriptableObject.CreateInstance<CameraRigProfile>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<SmokePawnMotor>();
            prefab.AddComponent<SmokePawnPresentation>();
            prefab.AddComponent<SmokePawnInput>();
            pawn.pawnPrefab = prefab;
            participant.defaultPawn = pawn;
            session.defaultGameMode = mode;
            mode.cameraRigProfile = cameraProfile;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);

            Assert.That(graph.TryFindNode("route.participant-input-profile", out _), Is.False);
            Assert.That(graph.TryFindNode("dependency.participant.input-profile", out _), Is.False);
            Assert.That(graph.TryFindNode("participant.seat.0.input-profile", out PyralisAuthoringGraphNode inputNode), Is.True);
            Assert.That(inputNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(inputNode.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.AssignmentField));
            Assert.That(inputNode.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.Reflection));
            Assert.That(inputNode.Guidance, Does.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(inputNode.AssignmentFields, Does.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(inputNode.NativeAction.HasValue, Is.True);
            Assert.That(inputNode.NativeAction.Value.Surface, Is.EqualTo(PyralisAuthoringActionSurface.Inspector));
            Assert.That(string.Join(" ", inputNode.NativeSetup), Does.Not.Contain("add/remove Gameplay Action rows"));
            Assert.That(string.Join(" ", inputNode.NativeSetup), Does.Not.Contain("SessionDefinition or ParticipantDefinition"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph).CurrentAction.Node.StableId,
                Is.EqualTo("participant.seat.0.input-profile"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildOverviewIssues(graph, session)
                .Any(issue => issue.Label == "Assign Input Profile" && issue.Lane == PyralisAuthoringOverviewLane.DoNow), Is.True);
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph).CriticalPath
                .Any(row => row.Node != null && row.Node.StableId == "participant.seat.0.input-profile"), Is.True);
            Assert.That(graph.TryFindNode("route.camera-focus", out PyralisAuthoringGraphNode cameraFocusNode), Is.True);
            Assert.That(cameraFocusNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.CandidateDetected));
            Assert.That(cameraFocusNode.Guidance, Does.Contain("PawnCameraTarget"));

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(cameraProfile);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void RuntimeValidation_SmokeInputProfileOwnsActionMapReadiness()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            AddCoreSetupServices(root);

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            CameraRigProfile cameraProfile = ScriptableObject.CreateInstance<CameraRigProfile>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<SmokePawnMotor>();
            prefab.AddComponent<SmokePawnPresentation>();
            prefab.AddComponent<SmokePawnInput>();
            pawn.pawnPrefab = prefab;
            participant.defaultPawn = pawn;
            participant.inputProfile = inputProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            mode.cameraRigProfile = cameraProfile;
            SetPrivateField(bootstrap, "sessionDefinition", session);

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap, intent);
            PyralisSceneReadinessReport sceneReport = PyralisSceneReadinessValidator.BuildReport(bootstrap);

            PyralisAuthoringGraphNode inputProfileValidation = graph.Nodes.FirstOrDefault(node =>
                node.SourceObject == inputProfile
                && node.SourceKind == PyralisAuthoringGraphSourceKind.RuntimeValidation
                && string.Equals(node.IssueCode, "InputProfile.Actions.Missing", System.StringComparison.Ordinal));

            Assert.That(inputProfileValidation, Is.Not.Null);
            Assert.That(inputProfileValidation.Kind, Is.EqualTo(PyralisAuthoringGraphNodeKind.ValidationEvidence));
            Assert.That(inputProfileValidation.AssignmentFields, Does.Contain("InputProfile.actions"));
            Assert.That(inputProfileValidation.NativeSetup.FirstOrDefault(), Does.Contain("InputProfile.actions"));
            Assert.That(sceneReport.Issues.Any(issue =>
                issue.Message.Contains("effective InputProfile")
                || issue.Message.Contains("Primary Action Map")
                || issue.Message.Contains("Gameplay Actions")), Is.False);

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(cameraProfile);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SceneSurfaceSnapshot_SmokeUsesTypedDetectorsAndFlagsNameFallbacks()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            SetPrivateField(bootstrap, "sessionDefinition", session);

            GameObject pickup = new GameObject("Typed Pickup");
            Collectible2D collectible = pickup.AddComponent<Collectible2D>();
            GameObject nameOnly = new GameObject("Name Only Pickup Surface");
            PickupNameOnlySurface fallback = nameOnly.AddComponent<PickupNameOnlySurface>();

            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap);

            PyralisAuthoringSceneSurfaceRow encounter = snapshot.Rows.FirstOrDefault(row =>
                row.Surface == PyralisAuthoringSceneSurfaceGuidance.PickupsHazardsEnemies);
            PyralisAuthoringSceneSurfaceRow fallbackRow = snapshot.Rows.FirstOrDefault(row =>
                row.IssueCode == "SceneSurface.FallbackTypeName");

            Assert.That(encounter, Is.Not.Null);
            Assert.That(encounter.Present, Is.True);
            Assert.That(encounter.DetectorId, Is.EqualTo("scene.pickup-surface"));
            Assert.That(encounter.CandidateObject, Is.EqualTo(collectible));
            Assert.That(fallbackRow, Is.Not.Null);
            Assert.That(fallbackRow.CandidateObject, Is.EqualTo(fallback));
            Assert.That(fallbackRow.RouteRelevant, Is.False);

            Object.DestroyImmediate(nameOnly);
            Object.DestroyImmediate(pickup);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SceneSurfaceSnapshot_SmokeSpawnPointEnvironmentDoesNotRequireSideViewCollider()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            ParticipantSpawnService spawnService = root.AddComponent<ParticipantSpawnService>();
            GameObject spawnPoint = new GameObject("Spawn Point");
            SetPrivateField(spawnService, "spawnPoints", new[] { spawnPoint.transform });
            SetPrivateField(bootstrap, "participantSpawnService", spawnService);

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            PawnMovementProfile movementProfile = ScriptableObject.CreateInstance<PawnMovementProfile>();
            movementProfile.movementMode = MovementMode.TwoD;
            movementProfile.use2DPhysics = true;
            movementProfile.movementStyle = Pawn2DMovementStyle.SideViewGravity;
            pawn.movementProfile = movementProfile;
            participant.defaultPawn = pawn;
            session.defaultParticipants = new[] { participant };
            SetPrivateField(bootstrap, "sessionDefinition", session);

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);
            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.WithIntentFocus(
                PyralisSetupRouteAnalysis.Build(bootstrap),
                new[] { RuntimeCapabilityFamily.CharacterPawnGameplay },
                intent);
            PyralisAuthoringSceneSurfaceSnapshot snapshot = PyralisAuthoringSceneSurfaceSnapshot.Build(bootstrap, route);

            PyralisAuthoringSceneSurfaceRow environment = snapshot.Rows.FirstOrDefault(row =>
                row.Surface == PyralisAuthoringSceneSurfaceGuidance.EnvironmentPlayfield);

            Assert.That(environment, Is.Not.Null);
            Assert.That(environment.Recommended, Is.True);
            Assert.That(environment.Present, Is.True);
            Assert.That(environment.LinkedToActiveSetup, Is.True);
            Assert.That(environment.DetectorId, Is.EqualTo("scene.spawn-point"));
            Assert.That(environment.CandidateObject, Is.EqualTo(spawnPoint.transform));

            Object.DestroyImmediate(spawnPoint);
            Object.DestroyImmediate(movementProfile);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetupGraph_SmokeLocalJoinPolicyDetectsAutoRegisteredDefaultParticipants()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            AddCoreSetupServices(root);

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participantOne = ScriptableObject.CreateInstance<ParticipantDefinition>();
            ParticipantDefinition participantTwo = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<SmokePawnMotor>();
            prefab.AddComponent<SmokePawnPresentation>();
            prefab.AddComponent<SmokePawnInput>();
            pawn.pawnPrefab = prefab;
            participantOne.defaultPawn = pawn;
            participantTwo.defaultPawn = pawn;
            participantOne.inputProfile = inputProfile;
            participantTwo.inputProfile = inputProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participantOne, participantTwo };
            SetPrivateField(bootstrap, "sessionDefinition", session);

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Movement | AuthoringCapability.Input,
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap, intent);

            Assert.That(graph.TryFindNode("route.participant-topology", out PyralisAuthoringGraphNode topologyNode), Is.True);
            Assert.That(topologyNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(topologyNode.Guidance, Does.Contain("auto-register"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildSetupMapRows(graph)
                .Any(row => row.Label == "Join Policy" && row.EffectiveEvidenceState == PyralisAuthoringGraphEvidenceState.Missing), Is.True);
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            Assert.That(route.CurrentAction?.Node?.StableId, Is.EqualTo("route.participant-topology"));
            Assert.That(route.CriticalPath
                .Any(row => row.Node != null && row.Node.StableId == "route.participant-topology"), Is.True);

            string mapJson = PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph);
            Assert.That(mapJson, Does.Contain("\"participantTopology\": \"LocalJoin\""));
            Assert.That(mapJson, Does.Contain("\"expectedJoinPolicy\": \"PlayerInputJoin\""));
            Assert.That(mapJson, Does.Contain("\"hasLocalJoinPolicyConflict\": true"));
            string guideJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));
            Assert.That(guideJson, Does.Contain("\"label\": \"Participant Join Policy\""));
            Assert.That(guideJson, Does.Contain("\"currentActionLabel\": \"Participant Join Policy\""));
            Assert.That(guideJson, Does.Contain("\"proofTargetId\": \"proof.local-pawn-join\""));
            Assert.That(guideJson, Does.Contain("Local Co-op Pawn Join Proof"));

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participantTwo);
            Object.DestroyImmediate(participantOne);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetupGraph_SmokeLocalJoinPlayerPrefabMustContainPawnInitializer()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            AddCoreSetupServices(root);
            PlayerInputManager playerInputManager = root.AddComponent<PlayerInputManager>();
            GameObject inputOnlyPrefab = new GameObject("Input Only Prefab");
            inputOnlyPrefab.AddComponent<PlayerInput>();
            playerInputManager.playerPrefab = inputOnlyPrefab;
            SetPrivateField(bootstrap, "playerInputManager", playerInputManager);

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participantOne = ScriptableObject.CreateInstance<ParticipantDefinition>();
            ParticipantDefinition participantTwo = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject pawnPrefab = new GameObject("Pawn Prefab");
            pawnPrefab.AddComponent<PawnRoot>();
            pawnPrefab.AddComponent<SmokePawnMotor>();
            pawnPrefab.AddComponent<SmokePawnPresentation>();
            pawnPrefab.AddComponent<SmokePawnInput>();
            pawn.pawnPrefab = pawnPrefab;
            participantOne.defaultPawn = pawn;
            participantTwo.defaultPawn = pawn;
            participantOne.inputProfile = inputProfile;
            participantTwo.inputProfile = inputProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participantOne, participantTwo };
            SetPrivateField(bootstrap, "sessionDefinition", session);
            SetPrivateField(root.GetComponent<ParticipantInputRouter>(), "autoRegisterDefaultParticipantsWithoutPlayerInput", false);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);

            Assert.That(PyralisSetupRouteAnalysis.Build(bootstrap).ParticipantTopology, Is.EqualTo(PyralisParticipantTopology.LocalJoin));
            Assert.That(graph.TryFindNode("route.player-input-manager-prefab", out PyralisAuthoringGraphNode playerPrefabNode), Is.True);
            Assert.That(playerPrefabNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(playerPrefabNode.Guidance, Does.Contain("PawnRoot/IPawnParticipantInitializer"));
            Assert.That(playerPrefabNode.Guidance, Does.Contain("one action asset drive multiple pawns"));
            Assert.That(PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph).CriticalPath
                .Any(row => row.Node != null && row.Node.StableId == "route.player-input-manager-prefab"), Is.True);

            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(inputOnlyPrefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participantTwo);
            Object.DestroyImmediate(participantOne);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void CoreSetupGraph_SmokeLocalJoinKeepsPlayerInputManagerInGraphEvidence()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            ParticipantInputRouter inputRouter = AddCoreSetupServices(root);
            PlayerInputManager playerInputManager = root.AddComponent<PlayerInputManager>();
            SetPrivateField(bootstrap, "playerInputManager", playerInputManager);
            SetPrivateField(inputRouter, "autoRegisterDefaultParticipantsWithoutPlayerInput", false);

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participantOne = ScriptableObject.CreateInstance<ParticipantDefinition>();
            ParticipantDefinition participantTwo = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject pawnPrefab = new GameObject("Joined Pawn Prefab");
            pawnPrefab.AddComponent<PlayerInput>();
            pawnPrefab.AddComponent<PawnRoot>();
            pawnPrefab.AddComponent<SmokePawnMotor>();
            pawnPrefab.AddComponent<SmokePawnPresentation>();
            pawnPrefab.AddComponent<SmokePawnInput>();
            playerInputManager.playerPrefab = pawnPrefab;
            pawn.pawnPrefab = pawnPrefab;
            participantOne.defaultPawn = pawn;
            participantTwo.defaultPawn = pawn;
            participantOne.inputProfile = inputProfile;
            participantTwo.inputProfile = inputProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participantOne, participantTwo };
            SetPrivateField(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(bootstrap);

            Assert.That(route.HasPlayerInputManager, Is.True);
            Assert.That(route.ParticipantTopology, Is.EqualTo(PyralisParticipantTopology.LocalJoin));
            Assert.That(graph.TryFindNode("route.participant-topology", out PyralisAuthoringGraphNode topologyNode), Is.True);
            Assert.That(topologyNode.Guidance, Does.Contain("PlayerInputManager"));
            Assert.That(topologyNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));

            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participantTwo);
            Object.DestroyImmediate(participantOne);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void CoreSetupGraph_SmokeBuildsCoreRouteChainFromGraphNodes()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            session.defaultGameMode = mode;
            SetPrivateField(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);

            Assert.That(graph.TryFindNode("bootstrap.root", out PyralisAuthoringGraphNode bootstrapNode), Is.True);
            Assert.That(graph.TryFindNode("session.definition", out PyralisAuthoringGraphNode sessionNode), Is.True);
            Assert.That(graph.TryFindNode("mode.definition", out PyralisAuthoringGraphNode modeNode), Is.True);
            Assert.That(bootstrapNode.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.CoreSetup));
            Assert.That(sessionNode.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.CoreSetup));
            Assert.That(modeNode.SourceKind, Is.EqualTo(PyralisAuthoringGraphSourceKind.CoreSetup));

            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void AuthoringVocabulary_SmokeDoesNotReferenceRetiredSetupCards()
        {
            string[] retiredSetupIds =
            {
                "setup.assign-participant-pawn",
                "setup.assign-input-profile",
                "setup.assign-spawn-points",
                "setup.assign-camera-rig",
                "setup.assign-player-input-manager",
                "setup.tune-pawn-visuals-and-collision",
                "setup.tune-movement-and-input-feel",
                "setup.assign-playfield-profile",
                "setup.scene-prefab-readiness",
                "setup.assign-game-mode"
            };

            string[] facts = PyralisAuthoringVocabularyRegistry.AllFacts
                .SelectMany(fact => fact.RelatedStableIds ?? System.Array.Empty<string>())
                .Concat(PyralisAuthoringVocabularyRegistry.AllFacts.Select(fact => fact.StableId))
                .ToArray();

            for (int i = 0; i < retiredSetupIds.Length; i++)
                Assert.That(facts, Does.Not.Contain(retiredSetupIds[i]), retiredSetupIds[i]);
        }

        [Test]
        public void RouteProjection_SmokeLocalJoinPolicyConflictUsesSingleActionableCardWhenPlayerInputManagerExists()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();
            AddCoreSetupServices(root);
            PlayerInputManager playerInputManager = root.AddComponent<PlayerInputManager>();
            SetPrivateField(bootstrap, "playerInputManager", playerInputManager);

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participantOne = ScriptableObject.CreateInstance<ParticipantDefinition>();
            ParticipantDefinition participantTwo = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject pawnPrefab = new GameObject("Joined Pawn Prefab");
            pawnPrefab.AddComponent<PlayerInput>();
            pawnPrefab.AddComponent<PawnRoot>();
            pawnPrefab.AddComponent<SmokePawnMotor>();
            pawnPrefab.AddComponent<SmokePawnPresentation>();
            pawnPrefab.AddComponent<SmokePawnInput>();
            playerInputManager.playerPrefab = pawnPrefab;
            pawn.pawnPrefab = pawnPrefab;
            participantOne.defaultPawn = pawn;
            participantTwo.defaultPawn = pawn;
            participantOne.inputProfile = inputProfile;
            participantTwo.inputProfile = inputProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participantOne, participantTwo };
            SetPrivateField(bootstrap, "sessionDefinition", session);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            IReadOnlyList<PyralisAuthoringGraphAuditRow> mapIssues = PyralisAuthoringSetupGraphProjection.BuildMapSceneSetupIssueRows(graph);
            PyralisSetupRouteAnalysis routeAnalysis = PyralisSetupRouteAnalysis.Build(bootstrap);

            Assert.That(routeAnalysis.HasPlayerInputManager, Is.True);
            Assert.That(routeAnalysis.HasLocalJoinPolicyConflict(), Is.True);
            Assert.That(route.CurrentAction?.Node?.StableId, Is.EqualTo("setup.resolve-participant-join-policy"));
            Assert.That(route.CriticalPath.Any(row => row.Node != null && row.Node.StableId == "route.participant-topology"), Is.False);
            Assert.That(route.CriticalPath.Any(row => row.Node != null && row.Node.StableId == "setup.resolve-participant-join-policy"), Is.True);
            Assert.That(mapIssues.Any(row => row.NodeId == "route.participant-topology"), Is.False);
            Assert.That(mapIssues.Any(row => row.NodeId == "setup.resolve-participant-join-policy"), Is.True);

            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participantTwo);
            Object.DestroyImmediate(participantOne);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void SetupRoute_SmokeMultiSeatLocalRouteInfersLocalJoinWithoutPawns()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participantOne = ScriptableObject.CreateInstance<ParticipantDefinition>();
            ParticipantDefinition participantTwo = ScriptableObject.CreateInstance<ParticipantDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participantOne, participantTwo };

            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(session);

            Assert.That(route.RequiresPawn, Is.False);
            Assert.That(route.ParticipantTopology, Is.EqualTo(PyralisParticipantTopology.LocalJoin));
            Assert.That(route.ExpectedJoinPolicy, Is.EqualTo(PyralisParticipantJoinPolicy.PlayerInputJoin));

            Object.DestroyImmediate(participantTwo);
            Object.DestroyImmediate(participantOne);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupRoute_SmokeSoloLocalWithoutPlayerInputManagerExpectsAutoRegister()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();

            participant.autoJoin = true;
            participant.inputProfile = inputProfile;
            participant.defaultPawn = pawn;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisSetupRouteAnalysis route = PyralisSetupRouteAnalysis.Build(session);
            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(route.ParticipantTopology, Is.EqualTo(PyralisParticipantTopology.SoloLocal));
            Assert.That(route.ExpectedJoinPolicy, Is.EqualTo(PyralisParticipantJoinPolicy.AutoRegisterDefaults));
            Assert.That(route.HasParticipantInputRouter, Is.False);
            Assert.That(route.HasSoloLocalJoinPolicyGap(), Is.True);
            Assert.That(graph.TryFindNode("route.participant-topology", out PyralisAuthoringGraphNode topologyNode), Is.True);
            Assert.That(topologyNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Missing));
            Assert.That(topologyNode.IssueCode, Is.EqualTo("ParticipantTopology.SoloAutoRegisterMissing"));
            Assert.That(topologyNode.Guidance, Does.Contain("Enable auto-register defaults"));
            Assert.That(topologyNode.NativeAction?.ToGuidanceSentence(), Does.Contain("Add Component > Participant Input Router"));
            Assert.That(topologyNode.NativeAction?.ToGuidanceSentence(), Does.Not.Contain("autoRegisterDefaultParticipantsWithoutPlayerInput"));

            PyralisAuthoringRouteWorkingProjection projection =
                PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            Assert.That(projection.CurrentAction?.Node?.StableId, Is.EqualTo("route.participant-topology"));

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokePlayfieldCameraFocusDoesNotRequirePawnTarget()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            CameraRigProfile cameraProfile = ScriptableObject.CreateInstance<CameraRigProfile>();
            cameraProfile.focusMode = CameraRigProfile.CameraFocusMode.PlayfieldCenter;
            mode.cameraRigProfile = cameraProfile;
            session.defaultGameMode = mode;

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session);

            Assert.That(graph.TryFindNode("route.camera-focus", out PyralisAuthoringGraphNode cameraFocusNode), Is.True);
            Assert.That(cameraFocusNode.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(cameraFocusNode.Guidance, Does.Contain("Playfield Center"));
            Assert.That(cameraFocusNode.Guidance, Does.Not.Contain("PawnCameraTarget"));

            Object.DestroyImmediate(cameraProfile);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void Guide_SmokeTranslatesSprite2DPawnInterfacesIntoConcreteSetupCards()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            pawn.pawnPrefab = prefab;
            participant.defaultPawn = pawn;
            participant.inputProfile = inputProfile;
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Session
                | AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.Participants
                | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);

            Assert.That(route.CurrentAction, Is.Not.Null);
            Assert.That(route.CurrentAction.Label, Is.EqualTo("Add Motor2D"));
            Assert.That(route.CurrentAction.StableId, Is.EqualTo("pawn.definition"));
            Assert.That(route.CriticalPath.Any(row => row.Label == "Add Motor2D"), Is.True);
            Assert.That(route.CriticalPath.Any(row => row.UnityActionLabel.Contains("Motor2D")), Is.True);
            PyralisAuthoringRouteStepRow routeShape = route.CriticalPath
                .FirstOrDefault(row => row.StableId == "route.shape");
            Assert.That(routeShape, Is.Not.Null);
            Assert.That(routeShape.EvidenceState, Is.EqualTo(PyralisAuthoringGraphEvidenceState.Ready));
            Assert.That(routeShape.Message, Does.Not.Contain("Add Motor2D"));
            Assert.That(routeShape.NativeSetup.Any(step => step.Contains("Add Motor2D")), Is.False);
            Assert.That(routeShape.AssignmentFields, Does.Not.Contain("PawnDefinition.pawnPrefab"));
            Assert.That(routeShape.AssignmentFields, Does.Not.Contain("ParticipantDefinition.inputProfile"));
            Assert.That(route.CriticalPath.Any(row => row.Label == "Scene And Prefab Readiness"), Is.False);
            Assert.That(route.ProofEnhancers.Any(row => row.Node.SourceKind == PyralisAuthoringGraphSourceKind.AuthoringContract), Is.False);
            Assert.That(route.ProofEnhancers.Select(row => row.Label).Distinct().Count(), Is.EqualTo(route.ProofEnhancers.Count));
            Assert.That(route.Proof.NativeSetup.Length, Is.EqualTo(1));
            Assert.That(route.Proof.AssignmentFields.Length, Is.EqualTo(0));
            Assert.That(route.Proof.CustomizationMoments.Length, Is.EqualTo(0));
            Assert.That(route.ProofSupport.Any(row => row.FromLabel.Contains("Top Down Hop")), Is.False);
            Assert.That(route.ProofSupport.Any(row => row.FromLabel.Contains("Pawn Camera Target")), Is.False);

            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));
            Assert.That(traceJson, Does.Contain("Add Motor2D"));
            Assert.That(traceJson, Does.Contain("\"currentActionLabel\": \"Add Motor2D\""));
            Assert.That(traceJson.Contains("Motor2DInputAdapter") || traceJson.Contains("Pawn2DMovementComponent"), Is.True);
            Assert.That(traceJson, Does.Not.Contain("Scene And Prefab Readiness"));
            Assert.That(traceJson, Does.Not.Contain("Add Lane Motor"));
            Assert.That(traceJson, Does.Not.Contain("the lane motor component"));
            Assert.That(traceJson, Does.Not.Contain("implements IPawnMotor"));
            Assert.That(traceJson, Does.Not.Contain("TopDownHopProfile"));
            Assert.That(traceJson, Does.Not.Contain("Pawn Camera Target"));

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokeBeginnerMovementIntentUsesPawnMovementProof()
        {
            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Session
                | AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.Participants
                | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime
                | AuthoringWorldAxiom.BoundedSpace);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intent);

            PyralisAuthoringGraphNode proof = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.StableId, Is.EqualTo("proof.1p-pawn-movement"));
            Assert.That(proof.Label, Does.Contain("Pawn Movement"));
            Assert.That(proof.Label, Does.Not.Contain("Custom Object"));
            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.Combat), Is.False);
            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.GunsProjectiles), Is.False);
            Assert.That(graph.FindNodes(PyralisAuthoringGraphNodeKind.Capability)
                .Any(node => node.CapabilityFamily == RuntimeCapabilityFamily.CharacterPawnGameplay), Is.True);

            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("bootstrap.root"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("session.definition"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("mode.definition"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("participant.default"));
            Assert.That(route.OrderedSteps.Select(row => row.Node?.StableId), Does.Contain("pawn.definition"));
            Assert.That(route.CurrentAction, Is.Not.Null);
            Assert.That(route.CurrentAction.Node.StableId, Is.EqualTo("bootstrap.root"));

            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));
            Assert.That(traceJson, Does.Contain("\"orderedSteps\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"bootstrap.root\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"session.definition\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"participant.default\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"proof.1p-pawn-movement\""));
        }

        [Test]
        public void Guide_SmokeLocalJoinUsesLocalPawnJoinProofAndExcludesSceneSurfaceCards()
        {
            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Session
                | AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.Participants
                | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime,
                PyralisAuthoringCapabilityDescriptorRegistry.FilterGameplayIntentDescriptorIds(
                    new[] { "contract.NeonBlack.Gameplay.Modules.Character.Motor2D" }),
                PyralisIntentParticipantRoute.FourLocalPlayers);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intent);
            PyralisAuthoringGraphNode proof = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);

            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.StableId, Is.EqualTo("proof.local-pawn-join"));
            Assert.That(route.ProofEnhancers.Any(row => row.Node.SetupDomain == PyralisAuthoringGraphSetupDomain.SceneSurface), Is.False);
            Assert.That(route.ProofSupport.Any(row => row.From != null
                && row.From.SourceKind == PyralisAuthoringGraphSourceKind.AuthoringContract
                && row.From.ProofTargetId != "proof.local-pawn-join"), Is.False);

            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"proof.local-pawn-join\""));
            Assert.That(traceJson, Does.Not.Contain("\"nodeId\": \"proof.1p-pawn-movement\""));
            Assert.That(traceJson, Does.Not.Contain("SceneSurface.PickupsHazardsEnemies"));
            Assert.That(traceJson, Does.Not.Contain("SceneSurface.ScoringObjectives"));
            Assert.That(traceJson, Does.Not.Contain("SceneReadiness.CameraAudio"));
            Assert.That(traceJson, Does.Not.Contain("Fake Gravity Jump"));
            Assert.That(traceJson, Does.Not.Contain("Top Down Hop"));
        }

        [Test]
        public void Guide_SmokeSelectedIntentProofTargetWinsOverPawnFallback()
        {
            string interactionBridgeId = "feature.NeonBlack.Gameplay.Modules.Character.ActorInteractionInputBridge2D";
            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Session
                | AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.Participants
                | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime,
                new[] { interactionBridgeId },
                PyralisIntentParticipantRoute.SoloLocal);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intent);
            PyralisAuthoringGraphNode proof = PyralisAuthoringSetupGraphProjection.FindCurrentProofNode(graph);
            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));

            Assert.That(proof, Is.Not.Null);
            Assert.That(proof.StableId, Is.EqualTo("proof.custom-object-effect"));
            Assert.That(traceJson, Does.Contain("\"hasAction\": false"));
            Assert.That(traceJson, Does.Contain("\"status\": \"Required setup is clear for the selected proof.\""));
            Assert.That(traceJson, Does.Contain("\"hasProof\": true"));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"proof.custom-object-effect\""));
            Assert.That(traceJson, Does.Not.Contain("\"nodeId\": \"proof.1p-pawn-movement\""));
            Assert.That(traceJson, Does.Not.Contain("\"blockingReason\""));
        }

        [Test]
        public void IntentExport_SmokeUsesSameGraphProofAsGuideTrace()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.BoundedSpace);
            string[] selectedIds = descriptors
                .Where(descriptor => descriptor.CapabilityPath == "Movement/Sprite2D/Kinetic Motor"
                    || descriptor.CapabilityPath == "Movement/Sprite2D/Movement Component"
                    || descriptor.CapabilityPath == "Movement/Traversal/FakeGravityJump"
                    || descriptor.CapabilityPath == "Input/Pawn/Actor Interaction Input Bridge2D"
                    || descriptor.CapabilityPath == "Presentation/Feedback/Pawn2D Presentation Component"
                    || descriptor.CapabilityPath == "World & Meta/Camera/Cinemachine Rig")
                .Select(descriptor => descriptor.StableId)
                .ToArray();
            Assert.That(selectedIds.Length, Is.EqualTo(6));

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(selectedIds),
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.BoundedSpace,
                selectedIds,
                PyralisIntentParticipantRoute.FourLocalPlayers);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intent);
            PyralisAuthoringIntentProjection intentProjection = PyralisAuthoringIntentProjection.Build(intent, descriptors);
            string intentJson = PyralisAuthoringSetupGraphJsonExporter.ToIntentJson(
                intent,
                PyralisAuthoringSetupGraphProjection.BuildIntentModel(intent),
                intentProjection,
                graph);
            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(
                PyralisAuthoringSetupGraphProjection.BuildGuideTraceProjection(graph));

            Assert.That(intentJson, Does.Contain("\"targetProofFocus\": \"Camera Cursor World Proof\""));
            Assert.That(intentJson, Does.Contain("\"targetProofSummary\": \"Target proof: Camera Cursor World Proof."));
            Assert.That(intentJson, Does.Not.Contain("\"targetProofFocus\": \"1P Pawn Movement Proof\""));
            Assert.That(traceJson, Does.Contain("\"nodeId\": \"proof.camera-cursor-world\""));
            Assert.That(traceJson, Does.Contain("\"readyForProof\""));
        }

        [Test]
        public void Guide_SmokeGuideProjectionExportUsesSelectedIntentRouteFacts()
        {
            IReadOnlyList<PyralisAuthoringCapabilityDescriptor> descriptors =
                PyralisAuthoringCapabilityDescriptorRegistry.BuildIntentProjectionDescriptors(
                    RuntimeCapabilityLaneTag.Sprite2D,
                    AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.BoundedSpace);
            string[] selectedIds = descriptors
                .Where(descriptor => descriptor.CapabilityPath == "Movement/Sprite2D/Kinetic Motor"
                    || descriptor.CapabilityPath == "Movement/Sprite2D/Movement Component"
                    || descriptor.CapabilityPath == "Movement/Traversal/FakeGravityJump"
                    || descriptor.CapabilityPath == "Input/Pawn/Actor Interaction Input Bridge2D"
                    || descriptor.CapabilityPath == "Presentation/Feedback/Pawn2D Presentation Component"
                    || descriptor.CapabilityPath == "World & Meta/Camera/Cinemachine Rig")
                .Select(descriptor => descriptor.StableId)
                .ToArray();
            Assert.That(selectedIds.Length, Is.EqualTo(6));

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                PyralisAuthoringCapabilityDescriptorRegistry.BuildCapabilitiesForDescriptors(selectedIds),
                AuthoringWorldAxiom.Dimensions2D | AuthoringWorldAxiom.GravityNone | AuthoringWorldAxiom.Realtime | AuthoringWorldAxiom.BoundedSpace,
                selectedIds,
                PyralisIntentParticipantRoute.FourLocalPlayers);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(null, intent);
            PyralisAuthoringGuideProjection guide =
                PyralisAuthoringGuideProjection.Build(null, null, graph);
            string traceJson = PyralisAuthoringSetupGraphJsonExporter.ToGuideJson(guide.GuideTrace);

            Assert.That(guide.Route, Is.SameAs(guide.GuideTrace.Route));
            Assert.That(traceJson, Does.Contain("\"desiredParticipantCount\": 4"));
            Assert.That(traceJson, Does.Contain("\"capabilityFamilies\": ["));
            Assert.That(traceJson, Does.Not.Contain("\"Combat\""));
            Assert.That(traceJson, Does.Contain("Kinetic Motor"));
            Assert.That(traceJson, Does.Not.Contain("Kinetic  Motor"));
            Assert.That(traceJson, Does.Contain("\"family\": \"CharacterPawnGameplay\""));
            Assert.That(traceJson, Does.Contain("FakeGravityJump"));
            Assert.That(traceJson, Does.Contain("Pawn2D Movement Component"));
            Assert.That(traceJson, Does.Not.Contain("Pawn3D Presentation Component"));
            Assert.That(traceJson, Does.Not.Contain("Enemy Spawner"));
            Assert.That(traceJson, Does.Not.Contain("Enemy Reaction Component"));
        }

        [Test]
        public void OverviewProjection_SmokeReadsGraphNextAction()
        {
            GameObject root = new GameObject("Gameplay Root");
            GameplaySessionBootstrap bootstrap = root.AddComponent<GameplaySessionBootstrap>();

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(bootstrap);
            PyralisAuthoringRouteWorkingProjection route = PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(graph);
            PyralisAuthoringOverviewModel model = PyralisAuthoringOverviewModel.Build(bootstrap, graph);

            Assert.That(model, Is.Not.Null);
            Assert.That(model.DoNow.Count, Is.GreaterThan(0));
            Assert.That(model.DoNow.Count, Is.LessThanOrEqualTo(3));
            Assert.That(route.CurrentAction, Is.Not.Null);
            Assert.That(model.DoNow[0].Label, Is.EqualTo(route.CurrentAction.Label));
            Assert.That(model.BestNextAction, Is.Not.Empty);

            Object.DestroyImmediate(root);
        }

        [Test]
        public void InputProfileSync_SmokeMapsUnityInputActionsToGameplayRows()
        {
            InputActionAsset actions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap player = actions.AddActionMap("Player");
            player.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2");
            player.AddAction("Attack", InputActionType.Button, expectedControlLayout: "Button");
            player.AddAction("Emote", InputActionType.Button, expectedControlLayout: "Button");

            InputProfile profile = ScriptableObject.CreateInstance<InputProfile>();
            profile.actions = actions;
            profile.primaryActionMap = "Player";
            profile.actionBindings = System.Array.Empty<GameplayInputActionBinding>();

            bool changed = InputProfileInputActionSync.SyncFromAssignedActions(profile, includeCustomActions: true, out string summary);

            Assert.That(changed, Is.True, summary);
            Assert.That(profile.FindBinding(GameplayInputActionRole.Move)?.actionName, Is.EqualTo("Move"));
            Assert.That(profile.FindBinding(GameplayInputActionRole.Move)?.requiredForGameplay, Is.True);
            Assert.That(profile.FindBinding(GameplayInputActionRole.AttackPrimary)?.actionName, Is.EqualTo("Attack"));
            Assert.That(profile.FindCustomBinding("Emote")?.actionName, Is.EqualTo("Emote"));

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actions);
        }

        [Test]
        public void SetupDependencyTree_SmokeDiscoversContractAssignmentsReflectively()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            participant.defaultPawn = pawn;

            PyralisSetupDependencyTree tree = PyralisSetupDependencyTree.Build(session);

            Assert.That(tree.AssignmentRecords.Any(record =>
                record.DeclaredByContract
                && record.IsResolved
                && record.QualifiedFieldPath == "SessionDefinition.defaultGameMode"
                && record.ReferencedObject == mode), Is.True);
            Assert.That(tree.AssignmentRecords.Any(record =>
                record.DeclaredByContract
                && record.IsResolved
                && record.QualifiedFieldPath == "ParticipantDefinition.defaultPawn"
                && record.ReferencedObject == pawn), Is.True);
            Assert.That(tree.AssignmentRecords.Any(record =>
                record.DeclaredByContract
                && !record.IsResolved
                && record.QualifiedFieldPath == "PawnDefinition.movementProfile"), Is.True);
            Assert.That(tree.Edges.Any(edge =>
                edge.FieldPath == "defaultGameMode"
                && edge.FromNodeId == "session.definition"
                && edge.ToNodeId == "mode.definition"), Is.True);

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        [Test]
        public void SetupGraph_SmokeMissingAssignmentsComeFromReflectedRecords()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile inputProfile = ScriptableObject.CreateInstance<InputProfile>();
            PawnDefinition pawn = ScriptableObject.CreateInstance<PawnDefinition>();
            GameObject prefab = new GameObject("Pawn Prefab");
            prefab.AddComponent<PawnRoot>();
            prefab.AddComponent<SmokePawnMotor>();
            prefab.AddComponent<SmokePawnPresentation>();
            prefab.AddComponent<SmokePawnInput>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            participant.defaultPawn = pawn;
            participant.inputProfile = inputProfile;
            pawn.pawnPrefab = prefab;

            PyralisAuthoringIntentSelection intent = new PyralisAuthoringIntentSelection(
                RuntimeCapabilityLaneTag.Sprite2D,
                AuthoringCapability.Session
                | AuthoringCapability.Input
                | AuthoringCapability.Movement
                | AuthoringCapability.Participants
                | AuthoringCapability.KineticMotor2D,
                AuthoringWorldAxiom.Dimensions2D
                | AuthoringWorldAxiom.GravityNone
                | AuthoringWorldAxiom.Realtime);

            PyralisAuthoringSetupGraph graph = PyralisAuthoringSetupGraphBuilder.Build(session, intent);
            PyralisAuthoringGraphNode movementAssignment = graph.Nodes.FirstOrDefault(node =>
                node.SourceKind == PyralisAuthoringGraphSourceKind.Reflection
                && node.AssignmentFields.Contains("PawnDefinition.movementProfile"));

            Assert.That(movementAssignment, Is.Not.Null);
            Assert.That(movementAssignment.StableId, Is.EqualTo("dependency.assignment.pawndefinition-movementprofile"));
            Assert.That(movementAssignment.IssueCode, Is.EqualTo("Assignment.pawndefinition-movementprofile"));
            Assert.That(movementAssignment.SourceObject, Is.EqualTo(pawn));
            Assert.That(graph.Nodes.Any(node => node.StableId == "dependency.pawn.movement-profile"), Is.False);

            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(inputProfile);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(session);
        }

        private static ParticipantInputRouter AddCoreSetupServices(GameObject root)
        {
            ParticipantInputRouter inputRouter = root.AddComponent<ParticipantInputRouter>();
            root.AddComponent<ParticipantSpawnService>();
            return inputRouter;
        }

        private sealed class SmokePawnMotor : MonoBehaviour, IPawnMotor
        {
            public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile)
            {
            }
        }

        private sealed class SmokePawnPresentation : MonoBehaviour, IPawnPresentationModule
        {
            public void ApplyPresentationProfile(PawnProfileApplicationContext context, PawnPresentationProfile presentationProfile)
            {
            }
        }

        private sealed class SmokePawnInput : MonoBehaviour, IPawnInputModule
        {
            public void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile)
            {
            }
        }

        private sealed class PickupNameOnlySurface : MonoBehaviour
        {
        }

        private static void AssertJsonObjectContaining(string json, string objectNeedle, params string[] expectedFields)
        {
            int needleIndex = json.IndexOf(objectNeedle, System.StringComparison.Ordinal);
            Assert.That(needleIndex, Is.GreaterThanOrEqualTo(0), objectNeedle);

            int objectStart = json.LastIndexOf('{', needleIndex);
            int objectEnd = json.IndexOf('}', needleIndex);
            Assert.That(objectStart, Is.GreaterThanOrEqualTo(0), objectNeedle);
            Assert.That(objectEnd, Is.GreaterThan(objectStart), objectNeedle);

            string objectJson = json.Substring(objectStart, objectEnd - objectStart + 1);
            for (int i = 0; i < expectedFields.Length; i++)
                Assert.That(objectJson, Does.Contain(expectedFields[i]), expectedFields[i]);
        }

        [AuthoringContract(
            ModuleId = "test.ownership.first",
            SelectableIntent = false,
            OwnershipClaims = new[] { "test.runtime.owner" },
            RuntimeFamilies = new[] { RuntimeCapabilityFamily.Custom },
            Relevance = "Test-only contract owner for duplicate ownership hygiene.")]
        private sealed class TestDuplicateOwnershipFirst
        {
        }

        [AuthoringContract(
            ModuleId = "test.ownership.second",
            SelectableIntent = false,
            OwnershipClaims = new[] { "test.runtime.owner" },
            RuntimeFamilies = new[] { RuntimeCapabilityFamily.Custom },
            Relevance = "Test-only competing owner for duplicate ownership hygiene.")]
        private sealed class TestDuplicateOwnershipSecond
        {
        }

        [AuthoringContract(
            ModuleId = "test.metadata.inferred-pawn",
            Capability = AuthoringCapability.Movement,
            CapabilityPath = "Movement/Test/Inferred Pawn",
            Relevance = "Test-only pawn runtime family inference contract.")]
        private sealed class TestInferredPawnContract : MonoBehaviour, IPawnMotor
        {
            public void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile)
            {
            }
        }

        [AuthoringContract(
            ModuleId = "test.metadata.support-interface",
            Capability = AuthoringCapability.Setup,
            Relevance = "Test-only support contract that should not ask humans for route metadata.")]
        private interface ITestSupportOnlyContract
        {
        }

        [AuthoringContract(
            ModuleId = "test.metadata.semantic-combat",
            Capability = AuthoringCapability.Combat,
            CapabilityPath = "Combat/Test/Semantic Combat",
            Relevance = "Test-only semantic combat contract that needs explicit runtime family ownership.")]
        private sealed class TestSemanticCombatContract
        {
        }
    }
}
