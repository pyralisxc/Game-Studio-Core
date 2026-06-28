using System;
using System.Linq;
using NeonBlack.Gameplay.Editor;
using NUnit.Framework;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public sealed class PyralisAuthoringHygieneScannerTests
    {
        [Test]
        public void Scanner_ClassifiesOwnershipResidueHidingSpots()
        {
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Evidence/LeakyReflection.cs",
                "class LeakyReflection { const string proof = \"proof.1p-pawn-movement\"; const string path = \"CapabilityPath\"; }",
                PyralisSourceDependencyPressureKind.ReflectionMeaningLeak,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Validation/LeakyValidator.cs",
                "class LeakyValidator { const string text = \"Open Guide and follow the proof route.\"; }",
                PyralisSourceDependencyPressureKind.ValidatorGuideLeak,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Inspectors/Pyralis/LeakyInspector.cs",
                "class LeakyInspector { const string text = \"Do Now: Route Proof through Overview.\"; }",
                PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Exports/LeakyJsonExporter.cs",
                "class LeakyJsonExporter { void Export() { PyralisAuthoringSetupGraphProjection.BuildRouteWorkingProjection(null); } }",
                PyralisSourceDependencyPressureKind.ExportTruthLeak,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Window/LeakyTabRenderer.cs",
                "class LeakyTabRenderer { void Render() { var state = PyralisAuthoringGraphEvidenceState.Missing; } }",
                PyralisSourceDependencyPressureKind.TabRendererLogicLeak,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Compatibility/LegacySetupBridge.cs",
                "class LegacySetupBridge { void Repair() { /* compatibility fallback auto-create quietly */ } }",
                PyralisSourceDependencyPressureKind.CompatibilityBridge,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/OLD.md",
                "This active guide describes the old setup and deprecated path.",
                PyralisSourceDependencyPressureKind.LegacyDocTruthLeak,
                cleanupFocus: false);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/OldOwnerName.cs",
                "class OldOwnerName { }",
                PyralisSourceDependencyPressureKind.OldOwnerName,
                cleanupFocus: false);
        }

        [Test]
        public void Scanner_SuppressesPolicyDocsAndInspectorHandoffNoise()
        {
            PyralisSourceDependencyHygieneRecord policyDoc =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/AUTHORING_BLUEPRINT.md",
                    "Fallback policy is strict. Do not recover by parsing display labels, must not auto-wire setup, and compatibility bridges are cleanup smells.");
            Assert.That(policyDoc.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.CompatibilityBridge));
            Assert.That(policyDoc.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.LegacyDocTruthLeak));

            PyralisSourceDependencyHygieneRecord currentOwnershipDoc =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/AUTHORING_BLUEPRINT.md",
                    "Hygiene pressure kinds are not all cleanup commands. Protective anti-fallback policy text and docs that define the current source-ownership audit should not be classified as ownership leaks.");
            Assert.That(currentOwnershipDoc.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.CompatibilityBridge));
            Assert.That(currentOwnershipDoc.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.LegacyDocTruthLeak));
            Assert.That(currentOwnershipDoc.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.OldOwnerName));

            PyralisSourceDependencyHygieneRecord scannerImplementation =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Hygiene/PyralisSourceDependencyHygieneScanner.cs",
                    "enum PyralisSourceDependencyPressureKind { OldOwnerName, CompatibilityBridge } class Scanner { string hint = \"CompatibilityBridge\"; }");
            Assert.That(scannerImplementation.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.ScannerImplementation));

            PyralisSourceDependencyHygieneRecord hygieneBridge =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Hygiene/PyralisCompatibilityBridge.cs",
                    "class PyralisCompatibilityBridge { void Repair() { /* compatibility fallback auto-create quietly */ } }");
            Assert.That(hygieneBridge.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.CompatibilityBridge));

            PyralisSourceDependencyHygieneRecord mixedDoc =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Docs/Authoring/MIXED.md",
                    "Fallback policy is strict, but this active guide also still points users at the legacy setup and deprecated path.");
            Assert.That(mixedDoc.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.LegacyDocTruthLeak));

            PyralisSourceDependencyHygieneRecord handoffInspector =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Enemies/Rigged3D/Editor/Inspectors/EnemyAIEditor.cs",
                    "class EnemyAIEditor { void OnInspectorGUI() { PyralisInspectorHandoff.DrawAuthoringButton(\"Enemy AI\", \"Use Pyralis Authoring for route setup and proof guidance.\"); } }");
            Assert.That(handoffInspector.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak));

            PyralisSourceDependencyHygieneRecord enemyRuntimeHandoffInspector =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Enemies/Rigged3D/Editor/Inspectors/EnemyFeatureRuntimeGuidedEditors.cs",
                    "class EnemyFeatureRuntimeGuidedEditors { void OnInspectorGUI() { PyralisInspectorHandoff.DrawAuthoringButton(\"Enemy Ambient Component\", null); PyralisInspectorHandoff.DrawAuthoringButton(\"Enemy Reaction Component\", null); } }");
            Assert.That(enemyRuntimeHandoffInspector.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak));

            PyralisSourceDependencyHygieneRecord routeOwnerInspector =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Inspectors/Pyralis/PyralisInspectorGuide.cs",
                    "class PyralisInspectorGuide { PyralisGuideContent content; void BuildChecklist() { const string proof = \"proof.1p-pawn-movement\"; } }");
            Assert.That(routeOwnerInspector.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.InspectorRouteGuideLeak));

            PyralisSourceDependencyHygieneRecord exportControlChrome =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Editor/Authoring/Window/PyralisAuthoringGraphJsonExportControl.cs",
                    "class PyralisAuthoringGraphJsonExportControl { void Draw() { if (GUILayout.Button(\"Export Map\")) PyralisAuthoringSetupGraphJsonExporter.ToMapJson(graph); } }");
            Assert.That(exportControlChrome.PressureKind, Is.Not.EqualTo(PyralisSourceDependencyPressureKind.ExportTruthLeak));
        }

        [Test]
        public void Scanner_ClassifiesNamespaceDependencyFanout()
        {
            const string BroadModuleSource =
                "using NeonBlack.Gameplay.Core.Contracts;\n"
                + "using NeonBlack.Gameplay.Data.Profiles;\n"
                + "using NeonBlack.Gameplay.Modules.Character;\n"
                + "using NeonBlack.Gameplay.Modules.Combat;\n"
                + "using NeonBlack.Gameplay.Modules.Feedback;\n"
                + "using NeonBlack.Gameplay.Glue.Session;\n"
                + "namespace NeonBlack.Gameplay.Modules.Combat { class BroadCombatRuntime { } }";
            PyralisSourceDependencyHygieneRecord broadModule =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Combat/Runtime/BroadCombatRuntime.cs",
                    BroadModuleSource);
            Assert.That(broadModule.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.NamespaceDependencyFanout));
            Assert.That(broadModule.Risk, Is.Not.EqualTo(PyralisSourceDependencyRisk.Low));
            Assert.That(broadModule.RiskScore, Is.GreaterThanOrEqualTo(8));
            Assert.That(broadModule.Reasons, Has.Some.Contains("budget is 3"));
            Assert.That(PyralisAuthoringHygieneProjection.IsCleanupFocus(broadModule.PressureKind), Is.True);

            const string AcceptedGlueSource =
                "using NeonBlack.Gameplay.Core.Contracts;\n"
                + "using NeonBlack.Gameplay.Data.Definitions;\n"
                + "using NeonBlack.Gameplay.Data.Participants;\n"
                + "using NeonBlack.Gameplay.Glue.Participants;\n"
                + "using NeonBlack.Gameplay.Glue.Session;\n"
                + "using NeonBlack.Gameplay.Glue.Spawning;\n"
                + "namespace NeonBlack.Gameplay.Glue.Bootstrap { class BootstrapWiring { } }";
            PyralisSourceDependencyHygieneRecord acceptedGlue =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Glue/Bootstrap/BootstrapWiring.cs",
                    AcceptedGlueSource);
            Assert.That(acceptedGlue.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.AcceptedComposition));

            const string ExtremeGlueSource =
                AcceptedGlueSource
                + "\nusing NeonBlack.Gameplay.Modules.Combat;"
                + "\nusing NeonBlack.Gameplay.Modules.Enemies;"
                + "\nusing NeonBlack.Gameplay.Modules.Traversal;"
                + "\nusing NeonBlack.Gameplay.Presentation.Camera;";
            PyralisSourceDependencyHygieneRecord extremeGlue =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(
                    "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Glue/Bootstrap/ExtremeBootstrapWiring.cs",
                    ExtremeGlueSource);
            Assert.That(extremeGlue.PressureKind, Is.EqualTo(PyralisSourceDependencyPressureKind.NamespaceDependencyFanout));
            Assert.That(extremeGlue.Risk, Is.Not.EqualTo(PyralisSourceDependencyRisk.Low));
            Assert.That(extremeGlue.ReviewHint, Does.Contain("imports too many Pyralis namespaces"));
        }

        [Test]
        public void Scanner_ClassifiesRuntimeCommunicationPressure()
        {
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Character/Runtime/Shared/LeakyCharacterRuntime.cs",
                "using NeonBlack.Gameplay.Modules.Hazards; namespace NeonBlack.Gameplay.Modules.Character { class LeakyCharacterRuntime { } }",
                PyralisSourceDependencyPressureKind.DirectModuleCommunication,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Character/Runtime/Shared/BoolClusterRuntime.cs",
                "class BoolClusterRuntime { bool isPlaying; bool hasJoined; bool canMove; bool isActive; }",
                PyralisSourceDependencyPressureKind.LifecycleBooleanCluster,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Character/Runtime/Shared/ImplicitMovementMode.cs",
                "class ImplicitMovementMode { private enum MovementState { Grounded, Airborne } private MovementState state = MovementState.Grounded; }",
                PyralisSourceDependencyPressureKind.StateMachineMissing,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Character/Runtime/Shared/EventHeavyRuntime.cs",
                "class EventHeavyRuntime { IGameplayEventChannel events; void Run() { events.Publish(a); events.Publish(b); events.Subscribe<A>(x); events.Subscribe<B>(x); } }",
                PyralisSourceDependencyPressureKind.EventChannelOveruse,
                cleanupFocus: true);
            AssertPressure(
                "Packages/com.neonblackinteractivellc.neonblackhub/Members/Pyralis/Gameplay/Modules/Character/Runtime/Shared/CharacterManager.cs",
                "class CharacterManager { void Awake() { UnityEngine.Object.FindAnyObjectByType<UnityEngine.Transform>(); } }",
                PyralisSourceDependencyPressureKind.ManagerBehaviorLeak,
                cleanupFocus: true);
        }

        private static void AssertPressure(
            string assetPath,
            string source,
            PyralisSourceDependencyPressureKind expectedKind,
            bool cleanupFocus)
        {
            PyralisSourceDependencyHygieneRecord record =
                PyralisSourceDependencyHygieneScanner.AnalyzeSource(assetPath, source);

            Assert.That(record.PressureKind, Is.EqualTo(expectedKind));
            Assert.That(record.Risk, Is.Not.EqualTo(PyralisSourceDependencyRisk.Low));
            Assert.That(record.Reasons.Any(reason => reason.Contains("appears", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("Ownership residue", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("legacy", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("Runtime communication", StringComparison.OrdinalIgnoreCase)), Is.True);
            Assert.That(PyralisAuthoringHygieneProjection.IsCleanupFocus(record.PressureKind), Is.EqualTo(cleanupFocus));
        }
    }
}
