using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Rules.TurnPhase;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Core.Runtime;
using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public class ArchitectureContractTests : PyralisEditorTestSupport
    {
        [Test]
        public void ActiveAssetsRoot_DoesNotContainArchivedGeneratedOrTestScratchContent()
        {
            string assetsRoot = Application.dataPath;
            string[] forbiddenPaths =
            {
                Path.Combine(assetsRoot, "GameplayExamplePack"),
                Path.Combine(assetsRoot, "Temp")
            };

            string[] offenders = forbiddenPaths.Where(Directory.Exists).ToArray();
            Assert.That(offenders, Is.Empty,
                "The active Assets root should stay clear for scene/prefab launch-pad work. Archive generated starter packs or test scratch folders outside Unity import paths: " + string.Join(", ", offenders));

            string[] initTestScenes = Directory.GetFiles(assetsRoot, "InitTestScene*.unity", SearchOption.TopDirectoryOnly);
            Assert.That(initTestScenes, Is.Empty,
                "Unity test scratch scenes should not remain in the active Assets root. Offenders: " + string.Join(", ", initTestScenes));
        }

        [Test]
        public void GameplayPackage_ContainsDataAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Data",
                "NeonBlack.Gameplay.Data.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Data asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayPackage_ContainsCoreAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Core",
                "NeonBlack.Gameplay.Core.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Core asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayPackage_ContainsPresentationAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Presentation",
                "NeonBlack.Gameplay.Presentation.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Presentation asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayAssembly_ReferencesPresentationAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("\"NeonBlack.Gameplay.Presentation\""), Is.True,
                "Expected NeonBlack.Gameplay.asmdef to reference the Presentation assembly.");
        }

        [Test]
        public void GameplayAssembly_DoesNotReferenceOptionalNetworkingAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("NeonBlack.Gameplay.Networking"), Is.False,
                "Core gameplay startup should stay local-first; backend networking belongs behind the optional Networking assembly.");
        }

        [Test]
        public void PresentationDomain_ContainsMovedRuntimeFiles()
        {
            string presentationPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Presentation");

            Assert.That(File.Exists(Path.Combine(presentationPath, "Animation", "ActorAnimationDriver.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(presentationPath, "Camera", "CinemachineCameraRigController.cs")), Is.True);
            Assert.That(Directory.Exists(Path.Combine(presentationPath, "Camera", "2D")), Is.False);
            Assert.That(File.Exists(Path.Combine(presentationPath, "Camera", "3D", "CameraOcclusionFader.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(presentationPath, "Visuals", "ActorShadowDriver.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(presentationPath, "Visuals", "CameraShake.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(presentationPath, "Visuals", "SpriteFlasher.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(presentationPath, "Visuals", "TextFlasher.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(presentationPath, "Visuals", "3D", "BillboardFacing3D.cs")), Is.True);
        }

        [Test]
        public void LegacyPresentationFeatureFolders_DoNotContainRuntimeCode()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features");

            string[] legacyFolders =
            {
                Path.Combine(gameplayRoot, "Animation"),
                Path.Combine(gameplayRoot, "Camera"),
                Path.Combine(gameplayRoot, "Visuals")
            };

            foreach (string legacyFolder in legacyFolders)
            {
                if (!Directory.Exists(legacyFolder))
                    continue;

                string[] runtimeFiles = Directory.GetFiles(legacyFolder, "*.cs", SearchOption.AllDirectories);
                Assert.That(runtimeFiles, Is.Empty,
                    "Expected legacy presentation folders to be free of runtime code after the Presentation cut. Offenders: " + string.Join(", ", runtimeFiles));
            }
        }

        [Test]
        public void GameplayPackage_ContainsCharactersAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "Runtime",
                "NeonBlack.Gameplay.Characters.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Characters asmdef at: {asmdefPath}");
        }

        [Test]
        public void CharactersAssembly_ContainsSharedContractsAndMovementModels()
        {
            string runtimeSharedPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "Runtime",
                "Shared");

            string[] expectedFiles =
            {
                Path.Combine("Components", "3D", "Pawn3DMovementComponent.cs"),
                Path.Combine("Contracts", "ICharacterMotorState.cs"),
                Path.Combine("Contracts", "IMovementModule.cs"),
                Path.Combine("Contracts", "IPawnCombatModule.cs"),
                Path.Combine("Contracts", "IPawnInputModule.cs"),
                Path.Combine("Contracts", "IPawnMotor.cs"),
                Path.Combine("Contracts", "IPawnPresentationModule.cs"),
                Path.Combine("Contracts", "IPawnTraversalModule.cs"),
                Path.Combine("Queries", "ParticipantQueryUtility.cs"),
                Path.Combine("Movement", "2D", "Motor2DConfig.cs"),
                Path.Combine("Movement", "2D", "Motor2DInput.cs"),
                Path.Combine("Movement", "2D", "Motor2DModel.cs"),
                Path.Combine("Movement", "2D", "Motor2DState.cs"),
                Path.Combine("Movement", "3D", "BrawlerMovementModel.cs"),
                Path.Combine("Movement", "3D", "FrameInput.cs"),
                Path.Combine("Movement", "3D", "MovementConfig.cs"),
                Path.Combine("Movement", "3D", "MovementInput.cs"),
                Path.Combine("Movement", "3D", "MovementPhysicsFrame.cs"),
                Path.Combine("Movement", "3D", "MovementState.cs")
            };

            foreach (string relativePath in expectedFiles)
                Assert.That(File.Exists(Path.Combine(runtimeSharedPath, relativePath)), Is.True,
                    $"Expected Characters shared runtime file at: {Path.Combine(runtimeSharedPath, relativePath)}");
        }

        [Test]
        public void Pawn3DMovement_Source_UsesExplicitCameraAndOptionalKnockback()
        {
            string motorPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "3D",
                "Motor3D.cs");
            string movementPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "Runtime",
                "Shared",
                "Components",
                "3D",
                "Pawn3DMovementComponent.cs");

            string motorSource = File.ReadAllText(motorPath);
            string source = File.ReadAllText(movementPath);
            string modelSource = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "Runtime",
                "Shared",
                "Movement",
                "3D",
                "BrawlerMovementModel.cs"));
            string traversalSource = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Traversal",
                "Runtime",
                "3D",
                "Pawn3DTraversalComponent.cs"));

            Assert.That(source.Contains("Camera.main"), Is.False,
                "3D movement should use an explicit movement camera or world-axis fallback, not the global MainCamera lookup.");
            Assert.That(source.Contains("[SerializeField] private Camera movementCamera"), Is.True);
            Assert.That(source.Contains("public void SetMovementCamera(Camera camera)"), Is.True);
            Assert.That(source.Contains("MoveWorld                  = ResolvePlanarMove(fi.Move)"), Is.True);
            Assert.That(source.Contains("Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up)"), Is.True);
            Assert.That(motorSource.Contains("new Vector3(_movement.State.VelocityX, _movement.State.VelocityY, _movement.State.VelocityZ)"), Is.True,
                "Motor3D.CurrentVelocity should expose full X/Y/Z movement state.");
            Assert.That(source.Contains("if (_knockback != null)"), Is.True,
                "Non-combat pawns should be able to move without an IActorKnockbackController.");
            Assert.That(source.Contains("_knockback.Tick(Time.deltaTime);\r\n            CollisionFlags"), Is.False,
                "Knockback must not be ticked unguarded immediately before movement.");
            Assert.That(motorSource.Contains("ResetPhysicsFrame();"), Is.False,
                "Motor3D should not clear previous physics before BrawlerMovementModel.Tick consumes grounding.");
            Assert.That(source.Contains("public void ApplyMovement(Vector3 modelVelocity)"), Is.True);
            Assert.That(source.Contains("ResetPhysicsFrame();"), Is.True,
                "Pawn3DMovementComponent.ApplyMovement should clear the accumulator immediately before recording fresh CharacterController results.");
            Assert.That(source.Contains("allowJump = profile.allowJump"), Is.True);
            Assert.That(source.Contains("allowDodge = profile.allowDodge"), Is.True);
            Assert.That(source.Contains("allowCrouch = profile.allowCrouch"), Is.True);
            Assert.That(modelSource.Contains("_config.AllowJump"), Is.True);
            Assert.That(modelSource.Contains("_config.AllowDodge"), Is.True);
            Assert.That(modelSource.Contains("_config.AllowCrouch"), Is.True);
            Assert.That(modelSource.Contains("input.MoveWorld"), Is.True);
            Assert.That(traversalSource.Contains("allowClimb = profile.allowClimb"), Is.True);
            Assert.That(traversalSource.Contains("allowHang = profile.allowHang"), Is.True);
            Assert.That(traversalSource.Contains("!allowClimb"), Is.True);
            Assert.That(traversalSource.Contains("!allowHang"), Is.True);
        }

        [Test]
        public void LegacyCharactersRoot_DoesNotRetainMovedSharedRuntimeFiles()
        {
            string charactersRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters");

            string[] legacyPaths =
            {
                "ICharacterMotorState.cs",
                "IMovementModule.cs",
                "IPawnCombatModule.cs",
                "IPawnInputModule.cs",
                "IPawnMotor.cs",
                "IPawnPresentationModule.cs",
                "IPawnTraversalModule.cs",
                "ParticipantQueryUtility.cs",
                Path.Combine("Runtime", "3D", "Pawn3DMovementComponent.cs"),
                Path.Combine("2D", "Movement", "Motor2DConfig.cs"),
                Path.Combine("2D", "Movement", "Motor2DInput.cs"),
                Path.Combine("2D", "Movement", "Motor2DModel.cs"),
                Path.Combine("2D", "Movement", "Motor2DState.cs"),
                Path.Combine("3D", "Movement", "BrawlerMovementModel.cs"),
                Path.Combine("3D", "Movement", "FrameInput.cs"),
                Path.Combine("3D", "Movement", "MovementConfig.cs"),
                Path.Combine("3D", "Movement", "MovementInput.cs"),
                Path.Combine("3D", "Movement", "MovementPhysicsFrame.cs"),
                Path.Combine("3D", "Movement", "MovementState.cs")
            };

            string[] offenders = legacyPaths
                .Select(relativePath => Path.Combine(charactersRoot, relativePath))
                .Where(File.Exists)
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Expected widened Characters shared runtime to move out of the legacy root folders. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void GameplayPackage_ContainsCompositionAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Composition",
                "NeonBlack.Gameplay.Features.Composition.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Composition asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayPackage_ContainsClimbZoneCoreContract()
        {
            string contractPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Core",
                "Contracts",
                "IClimbZone.cs");

            Assert.That(File.Exists(contractPath), Is.True, $"Expected climb-zone contract at: {contractPath}");
        }

        [Test]
        public void CompositionDomain_ContainsInteractionHandlerContract()
        {
            string contractPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Composition",
                "IActorInteractionHandler.cs");

            Assert.That(File.Exists(contractPath), Is.True, $"Expected interaction handler contract at: {contractPath}");
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(contractPath)!, "..", "Interaction", "IActorInteractionHandler.cs")), Is.False,
                "Interaction handler contract should no longer live under the Interaction feature.");
        }

        [Test]
        public void PlatformCompositionFiles_DoNotLiveUnderCharacters()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string coreRuntimeRoot = Path.Combine(gameplayRoot, "Core", "Runtime");
            string platformCompositionRoot = Path.Combine(gameplayRoot, "Features", "Platform", "Composition");
            string charactersCompositionRoot = Path.Combine(gameplayRoot, "Features", "Characters", "Composition");
            string charactersSharedCompositionRoot = Path.Combine(gameplayRoot, "Features", "Characters", "Runtime", "Shared", "Composition");

            
            
            Assert.That(File.Exists(Path.Combine(platformCompositionRoot, "GameplayRuntimeContext.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(platformCompositionRoot, "PyralisGameplayLifetimeScope.cs")), Is.True);

            Assert.That(Directory.Exists(charactersCompositionRoot), Is.False,
                "Bootstrap-owned platform composition helpers should not live under the Characters feature.");
            Assert.That(Directory.Exists(charactersSharedCompositionRoot), Is.False,
                "GameplayPlatformContext is core service-composition state, not Characters shared runtime.");
        }

        [Test]
        public void GameplayPackage_ContainsCombatAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Combat",
                "NeonBlack.Gameplay.Feature.Combat.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Combat asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayAssembly_ReferencesCombatAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("\"NeonBlack.Gameplay.Feature.Combat\""), Is.True,
                "Expected NeonBlack.Gameplay.asmdef to reference the Combat assembly.");
        }

        [Test]
        public void GameplayAssembly_ReferencesTraversalAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("\"NeonBlack.Gameplay.Feature.Traversal\""), Is.True,
                "Expected NeonBlack.Gameplay.asmdef to reference the Traversal assembly.");
        }

        [Test]
        public void CombatDomain_ContainsMovedAuthoredFiles()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");
            string combatProfilePath = Path.Combine(gameplayRoot, "Data", "Profiles", "Combat");
            string combatDefinitionPath = Path.Combine(gameplayRoot, "Data", "Definitions", "Combat");

            Assert.That(File.Exists(Path.Combine(combatProfilePath, "PawnCombatProfile.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(combatProfilePath, "EnemyCombatProfile.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(combatProfilePath, "ActorStatusEffectProfile.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(combatProfilePath, "ActorCombatReactionProfile.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(combatDefinitionPath, "StatusEffectDefinition.cs")), Is.True);
        }

        [Test]
        public void LegacyDataFolders_DoNotContainMovedCombatAuthoring()
        {
            string dataRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Data");

            string[] legacyPaths =
            {
                Path.Combine(dataRoot, "Profiles", "PawnCombatProfile.cs"),
                Path.Combine(dataRoot, "Profiles", "EnemyCombatProfile.cs"),
                Path.Combine(dataRoot, "Profiles", "ActorStatusEffectProfile.cs"),
                Path.Combine(dataRoot, "Profiles", "ActorCombatReactionProfile.cs"),
                Path.Combine(dataRoot, "Definitions", "StatusEffectDefinition.cs")
            };

            string[] offenders = legacyPaths.Where(File.Exists).ToArray();

            Assert.That(offenders, Is.Empty,
                "Expected combat-owned authoring to move out of Gameplay/Data. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void CombatAssembly_Source_UsesOnlyExpectedCrossDomainImports()
        {
            string combatRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Combat");

            string[] combatFiles = Directory.GetFiles(combatRoot, "*.cs", SearchOption.AllDirectories);
            string[] forbiddenImports =
            {
                "using NeonBlack.Gameplay.Features.Feedback;",
                "using NeonBlack.Gameplay.Features.Camera;",
                "using NeonBlack.Gameplay.Features.GameFlow;",
                "using NeonBlack.Gameplay.Features.Zones;"
            };

            string[] offenders = combatFiles
                .Where(path =>
                {
                    string source = File.ReadAllText(path);
                    return forbiddenImports.Any(source.Contains);
                })
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Combat should not depend on unrelated feature namespaces after the assembly cut. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void TraversalSources_UseCoreAndCompositionSeams()
        {
            string traversalRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Traversal",
                "Runtime");

            string interfaceSource = File.ReadAllText(Path.Combine(traversalRoot, "Shared", "IActorTraversalFeature.cs"));
            string runtimeSource = File.ReadAllText(Path.Combine(traversalRoot, "3D", "PawnTraversalFeatureRuntime3D.cs"));

            Assert.That(interfaceSource.Contains("using NeonBlack.Gameplay.Features.Zones;"), Is.False,
                "Traversal contract should not depend directly on Zones.");
            Assert.That(interfaceSource.Contains("IClimbZone"), Is.True);
            Assert.That(runtimeSource.Contains("using NeonBlack.Gameplay.Features.Interaction;"), Is.False,
                "Traversal runtime should consume the shared interaction seam, not the Interaction feature namespace.");
            Assert.That(runtimeSource.Contains("using NeonBlack.Gameplay.Features.Zones;"), Is.False,
                "Traversal runtime should not depend directly on Zones.");
            Assert.That(runtimeSource.Contains("using NeonBlack.Gameplay.Core.Contracts;"), Is.True);
            Assert.That(runtimeSource.Contains("IActorInteractionHandler"), Is.True);
        }

        [Test]
        public void TraversalDomain_OwnsMovedRuntimeFiles_And_Presentation_DropsConcreteTraversalCache()
        {
            string traversalRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Traversal",
                "Runtime");

            string presentationPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "3D",
                "Pawn3DPresentationComponent.cs");

            Assert.That(File.Exists(Path.Combine(traversalRoot, "NeonBlack.Gameplay.Feature.Traversal.asmdef")), Is.True);
            Assert.That(File.Exists(Path.Combine(traversalRoot, "3D", "LedgeProbe3D.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(traversalRoot, "3D", "GrabDetector.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(traversalRoot, "3D", "ClimbZone.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(traversalRoot, "3D", "Pawn3DTraversalComponent.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(traversalRoot)!, "Editor", "ClimbZoneEditor.cs")), Is.True);

            string ledgeProbeSource = File.ReadAllText(Path.Combine(traversalRoot, "3D", "LedgeProbe3D.cs"));
            string traversalComponentSource = File.ReadAllText(Path.Combine(traversalRoot, "3D", "Pawn3DTraversalComponent.cs"));
            string climbZoneSource = File.ReadAllText(Path.Combine(traversalRoot, "3D", "ClimbZone.cs"));
            string grabDetectorSource = File.ReadAllText(Path.Combine(traversalRoot, "3D", "GrabDetector.cs"));
            string presentationSource = File.ReadAllText(presentationPath);

            Assert.That(ledgeProbeSource.Contains("using NeonBlack.Gameplay.Core.Contracts;"), Is.True);
            Assert.That(ledgeProbeSource.Contains("public IClimbZone FindClimbZone"), Is.True);
            Assert.That(traversalComponentSource.Contains("namespace NeonBlack.Gameplay.Features.Traversal"), Is.True);
            Assert.That(climbZoneSource.Contains("IClimbTraversalActor"), Is.True,
                "ClimbZone should depend on the climb traversal actor contract instead of a concrete character motor.");
            Assert.That(grabDetectorSource.Contains("IClimbTraversalActor"), Is.True,
                "GrabDetector should depend on the climb traversal actor contract instead of a concrete character motor.");
            Assert.That(presentationSource.Contains("private Pawn3DTraversalComponent _traversal;"), Is.False,
                "Presentation should not cache the concrete traversal component when it is unused.");
            Assert.That(presentationSource.Contains("_traversal = GetComponent<Pawn3DTraversalComponent>();"), Is.False,
                "Presentation should not resolve the concrete traversal component when it is unused.");
        }

        [Test]
        public void LegacyZonesFolders_DoNotRetainTraversalOwnedRuntimeFiles()
        {
            string zonesRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Zones");

            string[] legacyPaths =
            {
                Path.Combine(zonesRoot, "3D", "LedgeProbe3D.cs"),
                Path.Combine(zonesRoot, "3D", "GrabDetector.cs"),
                Path.Combine(zonesRoot, "3D", "ClimbZone.cs"),
                Path.Combine(zonesRoot, "3D", "Editor", "ClimbZoneEditor.cs")
            };

            string[] offenders = legacyPaths.Where(File.Exists).ToArray();

            Assert.That(offenders, Is.Empty,
                "Traversal-owned climb runtime should not remain under Zones after the ownership move. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void GameplayPackage_ContainsInteractionAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Interaction",
                "Runtime",
                "NeonBlack.Gameplay.Feature.Interaction.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Interaction asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayAssembly_ReferencesInteractionAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("\"NeonBlack.Gameplay.Feature.Interaction\""), Is.True,
                "Expected NeonBlack.Gameplay.asmdef to reference the Interaction assembly.");
        }

        [Test]
        public void InteractionDomain_ContainsMovedRuntimeFiles_And_Relocates2DBridge()
        {
            string interactionRuntimeRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Interaction",
                "Runtime",
                "Shared");

            string characters2DPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "2D",
                "ActorInteractionInputBridge2D.cs");

            Assert.That(File.Exists(Path.Combine(interactionRuntimeRoot, "IActorInteractionFeature.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(interactionRuntimeRoot, "ActorInteractionFeatureRuntime.cs")), Is.True);
            Assert.That(File.Exists(characters2DPath), Is.True);
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(interactionRuntimeRoot)!, "..", "2D", "ActorInteractionInputBridge2D.cs")), Is.False,
                "The 2D interaction input bridge should no longer live under the Interaction domain.");
        }

        [Test]
        public void GameplayPackage_ContainsFeedbackAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Feedback",
                "Runtime",
                "NeonBlack.Gameplay.Feature.Feedback.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Feedback asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayAssembly_ReferencesFeedbackAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("\"NeonBlack.Gameplay.Feature.Feedback\""), Is.True,
                "Expected NeonBlack.Gameplay.asmdef to reference the Feedback assembly.");
        }

        [Test]
        public void FeedbackDeferredSlice_ContainsTypedParticipantFeedbackStream()
        {
            string feedbackRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Feedback");

            Assert.That(File.Exists(Path.Combine(feedbackRoot, "IParticipantFeedbackStream.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "IParticipantFeedbackPublisher.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "ParticipantFeedbackKind.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "ParticipantFeedbackMessage.cs")), Is.True);
        }

        [Test]
        public void FeedbackDomain_ContainsActorFeedbackCore_And_DefersParticipantHudRuntime()
        {
            string feedbackRuntimeRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Feedback",
                "Runtime",
                "Shared");

            string feedbackRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Feedback");

            Assert.That(File.Exists(Path.Combine(feedbackRuntimeRoot, "ActorFeedbackFeatureRuntime.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRuntimeRoot, "ActorFeedbackEvent.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRuntimeRoot, "ActorFeedbackEventType.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRuntimeRoot, "IActorFeedbackReceiver.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRuntimeRoot, "ActorFloatingFeedbackReceiver.cs")), Is.True);

            Assert.That(File.Exists(Path.Combine(feedbackRoot, "ParticipantFeedbackRelay.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "ParticipantFeedbackService.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "UI", "ParticipantFeedbackHudPresenter.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "UI", "ParticipantHealthHudBinder.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "UI", "ParticipantHudTargetBinding.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(feedbackRoot, "UI", "ParticipantHudFeedbackReceiver.cs")), Is.False);
        }

        [Test]
        public void FloatingFeedback_Source_UsesExplicitCameraAndDamageNumberSink()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string damageNumberPath = Path.Combine(gameplayRoot, "Features", "Combat", "DamageNumber.cs");
            string damageNumberSpawnerPath = Path.Combine(gameplayRoot, "Features", "Combat", "DamageNumberSpawner.cs");
            string worldHealthBarPath = Path.Combine(gameplayRoot, "Features", "Combat", "UI", "WorldHealthBar.cs");
            string floatingFeedbackPath = Path.Combine(gameplayRoot, "Features", "Feedback", "Runtime", "Shared", "ActorFloatingFeedbackReceiver.cs");

            Assert.That(File.ReadAllText(damageNumberSpawnerPath).Contains("IDamageNumberSink"), Is.True);
            Assert.That(File.ReadAllText(damageNumberSpawnerPath).Contains("static DamageNumberSpawner Instance"), Is.False);

            string[] explicitFeedbackConsumers =
            {
                damageNumberPath,
                worldHealthBarPath,
                floatingFeedbackPath
            };

            foreach (string path in explicitFeedbackConsumers)
            {
                string source = File.ReadAllText(path);
                Assert.That(source.Contains("Camera.main"), Is.False, $"{Path.GetFileName(path)} should not resolve the global main camera.");
                Assert.That(source.Contains("DamageNumberSpawner.Instance"), Is.False, $"{Path.GetFileName(path)} should not resolve the damage-number singleton.");
            }

            Assert.That(File.ReadAllText(worldHealthBarPath).Contains("IDamageNumberSink"), Is.True);
            Assert.That(File.ReadAllText(floatingFeedbackPath).Contains("IDamageNumberSink"), Is.True);
        }

        [Test]
        public void PresentationCameraFacing_Source_UsesExplicitCameraReferences()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string[] runtimeFiles =
            {
                Path.Combine(gameplayRoot, "Presentation", "Camera", "CinemachineCameraRigController.cs"),
                Path.Combine(gameplayRoot, "Presentation", "Visuals", "CameraShake.cs"),
                Path.Combine(gameplayRoot, "Presentation", "Visuals", "3D", "BillboardFacing3D.cs"),
                Path.Combine(gameplayRoot, "Presentation", "Animation", "ActorAnimationDriver.cs"),
                Path.Combine(gameplayRoot, "Features", "Hazards", "HazardFeedbackRuntime.cs"),
                Path.Combine(gameplayRoot, "Features", "Enemies", "3D", "EnemyAI.cs")
            };

            foreach (string path in runtimeFiles)
            {
                string source = File.ReadAllText(path);
                Assert.That(source.Contains("Camera.main"), Is.False, $"{Path.GetFileName(path)} should use explicit camera fields or local hierarchy references.");
            }

            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Presentation", "Camera", "CinemachineCameraRigController.cs")).Contains("SetTargetCamera"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Presentation", "Visuals", "3D", "BillboardFacing3D.cs")).Contains("SetCameraOverride"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Presentation", "Animation", "ActorAnimationDriver.cs")).Contains("SetCameraOverride"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Hazards", "HazardFeedbackRuntime.cs")).Contains("SetPopupCamera"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Enemies", "3D", "EnemyAI.cs")).Contains("SetPresentationCamera"), Is.True);
        }

        [Test]
        public void CombatImpactFeedback_Source_UsesExplicitSinksInsteadOfSingletonServices()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string contractsSource = File.ReadAllText(Path.Combine(gameplayRoot, "Core", "Contracts", "IGameService.cs"));
            Assert.That(contractsSource.Contains("interface IHitPauseSink"), Is.True);
            Assert.That(contractsSource.Contains("interface ICameraShakeSink"), Is.True);

            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Core", "TimeManager.cs")).Contains("IHitPauseSink"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Presentation", "Visuals", "CameraShake.cs")).Contains("ICameraShakeSink"), Is.True);

            string[] impactFeedbackConsumers =
            {
                Path.Combine(gameplayRoot, "Features", "Combat", "HitBox.cs"),
                Path.Combine(gameplayRoot, "Features", "Combat", "2D", "HitBox2D.cs"),
                Path.Combine(gameplayRoot, "Features", "Combat", "Projectile.cs"),
                Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileLauncherBase.cs"),
                Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileImpactEffectPlayer.cs"),
                Path.Combine(gameplayRoot, "Features", "Enemies", "EnemyReactionFeatureRuntime.cs"),
                Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.cs"),
                Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.SharedBehaviours.cs")
            };

            foreach (string path in impactFeedbackConsumers)
            {
                string source = File.ReadAllText(path);
                Assert.That(source.Contains("TimeManager.Instance"), Is.False, $"{Path.GetFileName(path)} should not resolve hit pause through TimeManager.Instance.");
                Assert.That(source.Contains("CameraShake.Instance"), Is.False, $"{Path.GetFileName(path)} should not resolve shake through CameraShake.Instance.");
            }

            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "HitBox.cs")).Contains("SetHitPauseSink"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "HitBox.cs")).Contains("SetCameraShakeSink"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileLauncherBase.cs")).Contains("SetImpactFeedbackSinks"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Enemies", "EnemyReactionFeatureRuntime.cs")).Contains("SetImpactFeedbackSinks"), Is.True);
            Assert.That(File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.cs")).Contains("SetCameraShakeSink"), Is.True);
        }

        [Test]
        public void ProjectileAndDamageZones_Source_UseSafeRuntimeLifecycles()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string projectileSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "Projectile.cs"));
            string projectile2DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "2D", "Projectile2D.cs"));
            string launcherBaseSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileLauncherBase.cs"));
            string launcher3DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileLauncher3D.cs"));
            string launcher2DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "ProjectileLauncher2D.cs"));
            string poolHandleSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Combat", "ProjectilePoolHandle.cs"));
            string weaponSource = File.ReadAllText(Path.Combine(gameplayRoot, "Data", "Definitions", "Combat", "WeaponData.cs"));
            string pawnCombatSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "PawnCombatBehaviour.cs"));
            string pawnCombat2DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Characters", "2D", "PawnCombatBehaviour2D.cs"));
            string damageZone2DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Zones", "2D", "DamageZone2D.cs"));
            string damageZone3DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Zones", "3D", "DamageZone.cs"));

            Assert.That(projectileSource.Contains("ProjectilePoolHandle"), Is.True);
            Assert.That(projectileSource.Contains("IProjectileRuntimeBody"), Is.True);
            Assert.That(projectileSource.Contains("Launch(GameObject owner"), Is.False);
            Assert.That(projectileSource.Contains("ProjectileSpawnCommand command"), Is.True);
            Assert.That(projectile2DSource.Contains("IProjectileRuntimeBody"), Is.True);
            Assert.That(projectile2DSource.Contains("ProjectileSpawnCommand command"), Is.True);
            Assert.That(launcherBaseSource.Contains("IProjectileRuntimeBody"), Is.True);
            Assert.That(launcherBaseSource.Contains("runtimeBody?.Launch(command"), Is.True);
            Assert.That(launcher3DSource.Contains("projectile.Launch(command.Owner"), Is.False);
            Assert.That(launcher2DSource.Contains("Rigidbody2D body"), Is.True);
            Assert.That(projectileSource.Contains("ReleaseToPool()"), Is.True);
            Assert.That(projectileSource.Contains("private void Retire()"), Is.True);
            Assert.That(poolHandleSource.Contains("public bool ReleaseToPool()"), Is.True);
            Assert.That(weaponSource.Contains("public ProjectileDefinition projectileDefinition"), Is.True);
            Assert.That(weaponSource.Contains("CreateProjectileDefinitionSnapshot"), Is.False);
            Assert.That(weaponSource.Contains("public GameObject projectilePrefab"), Is.False);
            Assert.That(weaponSource.Contains("projectileSpeed"), Is.False);
            Assert.That(weaponSource.Contains("projectileMaxDistance"), Is.False);
            Assert.That(weaponSource.Contains("projectileLifetime"), Is.False);
            Assert.That(weaponSource.Contains("projectileImpactDefinition"), Is.False);
            Assert.That(pawnCombatSource.Contains("ProjectileFireRequest"), Is.True);
            Assert.That(pawnCombatSource.Contains("weapon.projectileDefinition"), Is.True);
            Assert.That(pawnCombatSource.Contains("damageMultiplier: _outgoingDamageMultiplier"), Is.True);
            Assert.That(pawnCombatSource.Contains("knockbackMultiplier: _outgoingKnockbackMultiplier"), Is.True);
            Assert.That(pawnCombatSource.Contains("launcher.Fire(request)"), Is.True);
            Assert.That(pawnCombatSource.Contains("Instantiate(weapon.projectilePrefab"), Is.False);
            Assert.That(pawnCombat2DSource.Contains("ProjectileFireRequest"), Is.True);
            Assert.That(pawnCombat2DSource.Contains("weapon.projectileDefinition"), Is.True);
            Assert.That(pawnCombat2DSource.Contains("damageMultiplier: _outgoingDamageMultiplier"), Is.True);
            Assert.That(pawnCombat2DSource.Contains("knockbackMultiplier: _outgoingKnockbackMultiplier"), Is.True);
            Assert.That(pawnCombat2DSource.Contains("launcher.Fire(request)"), Is.True);
            Assert.That(pawnCombat2DSource.Contains("Instantiate(weapon.projectilePrefab"), Is.False);

            Assert.That(damageZone2DSource.Contains("_targetSnapshot.Add(health)"), Is.True);
            Assert.That(damageZone2DSource.Contains("for (int i = 0; i < _targetSnapshot.Count; i++)"), Is.True);
            Assert.That(damageZone3DSource.Contains("_targetSnapshot.Add(health)"), Is.True);
            Assert.That(damageZone3DSource.Contains("for (int i = 0; i < _targetSnapshot.Count; i++)"), Is.True);
        }

        [Test]
        public void GameplayPackage_ContainsScoringAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Scoring",
                "Runtime",
                "NeonBlack.Gameplay.Feature.Scoring.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Scoring asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayAssembly_ReferencesScoringAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("\"NeonBlack.Gameplay.Feature.Scoring\""), Is.True,
                "Expected NeonBlack.Gameplay.asmdef to reference the Scoring assembly.");
            Assert.That(System.Text.RegularExpressions.Regex.Matches(source, "\"NeonBlack\\.Gameplay\\.Feature\\.Scoring\"").Count, Is.EqualTo(1),
                "Expected NeonBlack.Gameplay.asmdef to reference the Scoring assembly exactly once.");
        }

        [Test]
        public void ScoringDomain_RuntimeShared_OwnsParticipantScoreService()
        {
            string scoringRuntimeRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Scoring",
                "Runtime",
                "Shared");

            string scoringRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Scoring");

            Assert.That(File.Exists(Path.Combine(scoringRuntimeRoot, "ParticipantScoreService.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(scoringRoot, "ParticipantScoreService.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(scoringRoot, "Shared", "ParticipantScoreService.cs")), Is.False);

            string participantScorePath = Path.Combine(scoringRuntimeRoot, "ParticipantScoreService.cs");
            string[] participantScoreCopies = Directory.GetFiles(scoringRoot, "ParticipantScoreService.cs", SearchOption.AllDirectories)
                .Where(path => path != participantScorePath)
                .ToArray();

            Assert.That(participantScoreCopies, Is.Empty,
                "Expected ParticipantScoreService.cs to be owned only by Features/Scoring/Runtime/Shared. Offenders: " + string.Join(", ", participantScoreCopies));
        }

        [Test]
        public void ScoringDeferredSlice_UsesLeaderboardServiceAndGameplayStateReader()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string leaderboardServicePath = Path.Combine(gameplayRoot, "Features", "Scoring", "Runtime", "Shared", "ILeaderboardService.cs");
            string leaderboardEntryPath = Path.Combine(gameplayRoot, "Features", "Scoring", "Runtime", "Shared", "LeaderboardEntry.cs");
            string gameplayStateReaderPath = Path.Combine(gameplayRoot, "Core", "Contracts", "IGameplayStateReader.cs");
            string leaderboardScreenPath = Path.Combine(gameplayRoot, "Features", "Scoring", "UI", "LeaderboardScreen.cs");
            string stillnessBonusPath = Path.Combine(gameplayRoot, "Features", "Scoring", "2D", "StillnessBonus2D.cs");

            Assert.That(File.Exists(leaderboardServicePath), Is.True);
            Assert.That(File.Exists(leaderboardEntryPath), Is.True);
            Assert.That(File.Exists(gameplayStateReaderPath), Is.True);

            string leaderboardScreenSource = File.ReadAllText(leaderboardScreenPath);
            Assert.That(leaderboardScreenSource.Contains("ILeaderboardService"), Is.True);
            Assert.That(leaderboardScreenSource.Contains("LeaderboardManager.Instance"), Is.False,
                "LeaderboardScreen should consume the leaderboard service seam instead of the singleton manager.");

            string stillnessBonusSource = File.ReadAllText(stillnessBonusPath);
            Assert.That(stillnessBonusSource.Contains("IGameplayStateReader"), Is.True);
            Assert.That(stillnessBonusSource.Contains("GameManager.Instance"), Is.False,
                "StillnessBonus2D should read gameplay state through IGameplayStateReader instead of the GameManager singleton.");
        }

        [Test]
        public void Arcade2D_Runtime_UsesExplicitServicesInsteadOfSingletonGlobals()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string contractsRoot = Path.Combine(gameplayRoot, "Core", "Contracts");
            string movementPath = Path.Combine(gameplayRoot, "Features", "Characters", "2D", "Pawn2DMovementComponent.cs");
            string collectibleSpawnerPath = Path.Combine(gameplayRoot, "Features", "Pickups", "2D", "CollectibleSpawner2D.cs");
            string hazardPath = Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.cs");
            string hazardSpawnerPath = Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "HazardSpawner.cs");
            string hazardSharedPath = Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.SharedBehaviours.cs");
            string hazardCrossingPath = Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.CrossingSequence.cs");
            string playerInputPath = Path.Combine(gameplayRoot, "Features", "Input", "2D", "PlayerInputHandler.cs");
            string stillnessBonusPath = Path.Combine(gameplayRoot, "Features", "Scoring", "2D", "StillnessBonus2D.cs");
            string uiManagerPath = Path.Combine(gameplayRoot, "Features", "GameFlow", "2D", "UI", "UIManager.cs");
            string settingsScreenPath = Path.Combine(gameplayRoot, "Features", "Settings", "UI", "SettingsScreen.cs");
            string settingsMenuPath = Path.Combine(gameplayRoot, "Features", "Settings", "UI", "SettingsMenu.cs");

            Assert.That(File.Exists(Path.Combine(contractsRoot, "ICameraBoundsProvider.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(contractsRoot, "IHazardOutcomeSink.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(contractsRoot, "IPickupBurstSpawnSurface.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(contractsRoot, "ISessionScoreService.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(contractsRoot, "IGameplaySettingsApplier.cs")), Is.True);

            string[] runtimeFiles =
            {
                movementPath,
                collectibleSpawnerPath,
                hazardPath,
                hazardSpawnerPath,
                hazardSharedPath,
                hazardCrossingPath,
                playerInputPath,
                stillnessBonusPath,
                uiManagerPath
            };

            foreach (string path in runtimeFiles)
            {
                string source = File.ReadAllText(path);
                Assert.That(source.Contains("GameManager.Instance"), Is.False, $"{Path.GetFileName(path)} should not read the GameManager singleton.");
                Assert.That(source.Contains("Camera.main"), Is.False, $"{Path.GetFileName(path)} should not search for Camera.main at runtime.");
                Assert.That(source.Contains("CollectibleSpawner2D.Instance"), Is.False, $"{Path.GetFileName(path)} should not reach through the collectible spawner singleton.");
            }

            string[] settingsUiFiles =
            {
                settingsScreenPath,
                settingsMenuPath
            };

            foreach (string path in settingsUiFiles)
            {
                string source = File.ReadAllText(path);
                Assert.That(source.Contains("GameManager.Instance"), Is.False, $"{Path.GetFileName(path)} should not read the GameManager singleton.");
                Assert.That(source.Contains("SettingsManager.Instance"), Is.False, $"{Path.GetFileName(path)} should not read the SettingsManager singleton.");
                Assert.That(source.Contains("IGameplaySettingsApplier"), Is.True, $"{Path.GetFileName(path)} should consume the settings service contract.");
            }

            Assert.That(File.ReadAllText(movementPath).Contains("IGameplayStateReader"), Is.True);
            Assert.That(File.ReadAllText(movementPath).Contains("ICameraBoundsProvider"), Is.True);
            Assert.That(File.ReadAllText(hazardPath).Contains("IHazardOutcomeSink"), Is.True);
            Assert.That(File.ReadAllText(hazardSpawnerPath).Contains("IPickupBurstSpawnSurface"), Is.True);
            Assert.That(File.ReadAllText(playerInputPath).Contains("IGameplayStateReader"), Is.True);
            Assert.That(File.ReadAllText(stillnessBonusPath).Contains("ISessionScoreAwardSink"), Is.True);
            Assert.That(File.ReadAllText(uiManagerPath).Contains("IGameplaySessionFlow"), Is.True);
            Assert.That(File.ReadAllText(settingsScreenPath).Contains("_musicVolumeSlider"), Is.True);
            Assert.That(File.ReadAllText(playerInputPath).Contains("PlayerInputHandler Instance"), Is.False);
            Assert.That(File.ReadAllText(uiManagerPath).Contains("UIManager Instance"), Is.False);
        }

        [Test]
        public void ScoringDomain_FirstCut_ExplicitlyDefersLeaderboardAndGameflowUiSlices()
        {
            string scoringRuntimeRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Scoring",
                "Runtime",
                "Shared");

            string scoringRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Scoring");

            Assert.That(File.Exists(Path.Combine(scoringRoot, "LeaderboardManager.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(scoringRoot, "2D", "StillnessBonus2D.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(scoringRoot, "UI", "LeaderboardScreen.cs")), Is.True);

            Assert.That(File.Exists(Path.Combine(scoringRuntimeRoot, "LeaderboardManager.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(scoringRuntimeRoot, "StillnessBonus2D.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(scoringRuntimeRoot, "LeaderboardScreen.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(scoringRuntimeRoot)!, "2D", "StillnessBonus2D.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(scoringRuntimeRoot)!, "UI", "LeaderboardScreen.cs")), Is.False);

            string leaderboardManagerPath = Path.Combine(scoringRoot, "LeaderboardManager.cs");
            string[] leaderboardManagerCopies = Directory.GetFiles(scoringRoot, "LeaderboardManager.cs", SearchOption.AllDirectories)
                .Where(path => path != leaderboardManagerPath)
                .ToArray();

            Assert.That(leaderboardManagerCopies, Is.Empty,
                "Expected LeaderboardManager.cs to stay outside the first Scoring runtime cut. Offenders: " + string.Join(", ", leaderboardManagerCopies));
        }

        [Test]
        public void GameplayPackage_ContainsPickupsAssemblyDefinition()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups",
                "Runtime",
                "NeonBlack.Gameplay.Feature.Pickups.asmdef");

            Assert.That(File.Exists(asmdefPath), Is.True, $"Expected Pickups asmdef at: {asmdefPath}");
        }

        [Test]
        public void GameplayAssembly_ReferencesPickupsAssembly()
        {
            string asmdefPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "NeonBlack.Gameplay.asmdef");

            string source = File.ReadAllText(asmdefPath);

            Assert.That(source.Contains("\"NeonBlack.Gameplay.Feature.Pickups\""), Is.True,
                "Expected NeonBlack.Gameplay.asmdef to reference the Pickups assembly.");
            Assert.That(System.Text.RegularExpressions.Regex.Matches(source, "\"NeonBlack\\.Gameplay\\.Feature\\.Pickups\"").Count, Is.EqualTo(1),
                "Expected NeonBlack.Gameplay.asmdef to reference the Pickups assembly exactly once.");
        }

        [Test]
        public void PickupsDomain_FirstCut_OwnsCollectorCore()
        {
            string pickupsRuntimeRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups",
                "Runtime");

            string pickupsRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups");

            Assert.That(File.Exists(Path.Combine(pickupsRuntimeRoot, "Shared", "IPickupCollectible.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(pickupsRuntimeRoot, "2D", "ActorPickupCollectorFeature2D.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(pickupsRuntimeRoot, "3D", "ActorPickupCollectorFeature3D.cs")), Is.True);

            Assert.That(File.Exists(Path.Combine(pickupsRoot, "IPickupCollectible.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(pickupsRoot, "2D", "ActorPickupCollectorFeature2D.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(pickupsRoot, "3D", "ActorPickupCollectorFeature3D.cs")), Is.False);

            string pickupContractPath = Path.Combine(pickupsRuntimeRoot, "Shared", "IPickupCollectible.cs");
            string[] pickupContractCopies = Directory.GetFiles(pickupsRoot, "IPickupCollectible.cs", SearchOption.AllDirectories)
                .Where(path => path != pickupContractPath)
                .ToArray();
            Assert.That(pickupContractCopies, Is.Empty,
                "Expected IPickupCollectible.cs to be owned only by Features/Pickups/Runtime/Shared. Offenders: " + string.Join(", ", pickupContractCopies));

            string collector2DPath = Path.Combine(pickupsRuntimeRoot, "2D", "ActorPickupCollectorFeature2D.cs");
            string[] collector2DCopies = Directory.GetFiles(pickupsRoot, "ActorPickupCollectorFeature2D.cs", SearchOption.AllDirectories)
                .Where(path => path != collector2DPath)
                .ToArray();
            Assert.That(collector2DCopies, Is.Empty,
                "Expected ActorPickupCollectorFeature2D.cs to be owned only by Features/Pickups/Runtime/2D. Offenders: " + string.Join(", ", collector2DCopies));

            string collector3DPath = Path.Combine(pickupsRuntimeRoot, "3D", "ActorPickupCollectorFeature3D.cs");
            string[] collector3DCopies = Directory.GetFiles(pickupsRoot, "ActorPickupCollectorFeature3D.cs", SearchOption.AllDirectories)
                .Where(path => path != collector3DPath)
                .ToArray();
            Assert.That(collector3DCopies, Is.Empty,
                "Expected ActorPickupCollectorFeature3D.cs to be owned only by Features/Pickups/Runtime/3D. Offenders: " + string.Join(", ", collector3DCopies));
        }

        [Test]
        public void PickupsDomain_FirstCut_ExplicitlyDefersCollectiblesSpawnersAndFeedbackHelpers()
        {
            string pickupsRuntimeRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups",
                "Runtime");

            string pickupsRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups");

            Assert.That(File.Exists(Path.Combine(pickupsRoot, "2D", "Collectible2D.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(pickupsRoot, "2D", "CollectibleSpawner2D.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(pickupsRoot, "2D", "CollectibleFeedback2D.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(pickupsRoot, "3D", "Collectible3D.cs")), Is.True);

            Assert.That(File.Exists(Path.Combine(pickupsRuntimeRoot, "2D", "Collectible2D.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(pickupsRuntimeRoot, "2D", "CollectibleSpawner2D.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(pickupsRuntimeRoot, "2D", "CollectibleFeedback2D.cs")), Is.False);
            Assert.That(File.Exists(Path.Combine(pickupsRuntimeRoot, "3D", "Collectible3D.cs")), Is.False);
        }

        [Test]
        public void PickupsContract_SplitsCollectionFromRemoval()
        {
            string pickupContractPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups",
                "Runtime",
                "Shared",
                "IPickupCollectible.cs");

            string source = File.ReadAllText(pickupContractPath);

            Assert.That(source.Contains("void CollectBy(GameObject collector);"), Is.True);
            Assert.That(source.Contains("bool RemoveFromPlay();"), Is.True,
                "Expected the pickup contract to separate collector-driven pickup from removal.");
        }

        [Test]
        public void HazardImpactUtility_UsesPickupRemovalPath_InsteadOfNullCollection()
        {
            string hazardUtilityPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Hazards",
                "HazardImpactUtility.cs");

            string source = File.ReadAllText(hazardUtilityPath);

            Assert.That(source.Contains("collectible?.RemoveFromPlay();"), Is.True);
            Assert.That(source.Contains("collectible?.CollectBy(null);"), Is.False,
                "Hazards should remove pickups from play instead of routing through null-collector collection.");
        }

        [Test]
        public void PickupsDomain_DeferredCleanup_ContainsCoreSeamContracts()
        {
            string contractsRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Core",
                "Contracts");

            string awardSinkPath = Path.Combine(contractsRoot, "IPickupAwardSink.cs");
            string spawnSurfacePath = Path.Combine(contractsRoot, "IPickupSpawnSurface.cs");

            Assert.That(File.Exists(awardSinkPath), Is.True, $"Expected pickup award seam contract at: {awardSinkPath}");
            Assert.That(File.Exists(spawnSurfacePath), Is.True, $"Expected pickup spawn seam contract at: {spawnSurfacePath}");

            string pickupsRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups");

            string[] featureCopies = Directory.GetFiles(pickupsRoot, "IPickupAwardSink.cs", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(pickupsRoot, "IPickupSpawnSurface.cs", SearchOption.AllDirectories))
                .ToArray();

            Assert.That(featureCopies, Is.Empty,
                "Pickup cleanup seams should live under Gameplay/Core/Contracts, not inside the Pickups feature. Offenders: " + string.Join(", ", featureCopies));
        }

        [Test]
        public void PickupsDomain_DeferredCleanup_UsesCoreAwardAndSpawnSeams()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string collectible2DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Pickups", "2D", "Collectible2D.cs"));
            string collectible3DSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Pickups", "3D", "Collectible3D.cs"));
            string spawnerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Pickups", "2D", "CollectibleSpawner2D.cs"));
            string feedbackSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Pickups", "2D", "CollectibleFeedback2D.cs"));

            Assert.That(collectible2DSource.Contains("IPickupAwardSink"), Is.True,
                "Collectible2D should resolve pickup side effects through IPickupAwardSink.");
            Assert.That(collectible3DSource.Contains("IPickupAwardSink"), Is.True,
                "Collectible3D should resolve pickup side effects through IPickupAwardSink.");
            Assert.That(collectible2DSource.Contains("ParticipantScoreService"), Is.False,
                "Collectible2D should not depend directly on ParticipantScoreService after deferred cleanup.");
            Assert.That(collectible3DSource.Contains("ParticipantScoreService"), Is.False,
                "Collectible3D should not depend directly on ParticipantScoreService after deferred cleanup.");
            Assert.That(collectible2DSource.Contains("CollectibleFeedback2D.Instance"), Is.False,
                "Collectible2D should not reach directly into CollectibleFeedback2D.Instance after deferred cleanup.");

            Assert.That(spawnerSource.Contains("IPickupSpawnSurface"), Is.True,
                "CollectibleSpawner2D should use the IPickupSpawnSurface seam to group spawn rules.");
            Assert.That(feedbackSource.Contains("IPickupAwardSink"), Is.True,
                "CollectibleFeedback2D should act as the deferred pickup award sink implementation.");
            Assert.That(feedbackSource.Contains("GameManager.Instance"), Is.False,
                "CollectibleFeedback2D should use an explicit score award service instead of the GameManager singleton.");
            Assert.That(feedbackSource.Contains("CollectibleFeedback2D Instance"), Is.False,
                "CollectibleFeedback2D should not expose a scene singleton for new content.");
            Assert.That(feedbackSource.Contains("ISessionScoreAwardSink"), Is.True,
                "CollectibleFeedback2D should award points through the shared session score award contract.");
        }

        [Test]
        public void PickupsDomain_RemovesThinPointPickupAliases()
        {
            string pickupsRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Pickups",
                "2D");

            Assert.That(File.Exists(Path.Combine(pickupsRoot, "PointPickup.cs")), Is.False,
                "PointPickup alias should be removed in favor of the canonical Collectible2D path.");
            Assert.That(File.Exists(Path.Combine(pickupsRoot, "PointPickupSpawner.cs")), Is.False,
                "PointPickupSpawner alias should be removed in favor of the canonical CollectibleSpawner2D path.");
            Assert.That(File.Exists(Path.Combine(pickupsRoot, "PointPickupFeedback.cs")), Is.False,
                "PointPickupFeedback alias should be removed in favor of the canonical CollectibleFeedback2D path.");
        }

        [Test]
        public void GameplayPackage_ContainsPawn2DComponentFiles()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "2D");

            Assert.That(File.Exists(Path.Combine(gameplayRoot, "Pawn2DMovementComponent.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(gameplayRoot, "Pawn2DPresentationComponent.cs")), Is.True);
        }

        [Test]
        public void PawnStarterPack_DoesNotShipEmptyPlayerTwoPrefab()
        {
            string playerTwoPrefabPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "PawnStarterPack",
                "Prefabs",
                "Players",
                "Player 2.prefab");

            Assert.That(File.Exists(playerTwoPrefabPath), Is.False,
                "Player Two intentionally uses SharedPawnDefinition; do not ship an empty Player 2 prefab that looks playable but has no runtime stack.");
        }

        [Test]
        public void CoreNetworkingContracts_UseCoreNamespace()
        {
            string contractsPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Core",
                "Contracts",
                "Networking");

            string[] contractFiles = Directory.GetFiles(contractsPath, "*.cs", SearchOption.TopDirectoryOnly);
            foreach (string contractFile in contractFiles)
            {
                string source = File.ReadAllText(contractFile);
                Assert.That(source.Contains("namespace NeonBlack.Gameplay.Networking.Contracts"), Is.False,
                    $"Networking contracts under Core should not keep the old namespace: {contractFile}");
            }
        }

        [Test]
        public void GameplayFeatures_DoNotUseSceneWideRuntimeDiscovery()
        {
            string featuresPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features");

            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            string[] forbiddenPatterns =
            {
                "FindAnyObjectByType<",
                "FindObjectOfType<",
                "FindObjectsByType<",
                "FindObjectsOfType<",
                "GameObject.Find(",
                "GameObject.FindGameObjectWithTag",
                "GameObject.FindGameObjectsWithTag"
            };

            string[] featureFiles = Directory.GetFiles(featuresPath, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(editorDirectorySegment))
                .ToArray();
            string[] offenders = featureFiles
                .Where(path =>
                {
                    string source = File.ReadAllText(path);
                    return forbiddenPatterns.Any(source.Contains);
                })
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Runtime feature code should resolve through authored references, hierarchy ownership, or platform services instead of scene-wide discovery. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void GameplayRuntime_Source_DoesNotConsumeLegacySingletonInstances()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            string[] forbiddenPatterns =
            {
                "CameraShake.Instance",
                "CollectibleFeedback2D.Instance",
                "DamageNumberSpawner.Instance",
                "GameManager.Instance",
                "LeaderboardManager.Instance",
                "PlayerRegistry.Motor2D",
                "PlayerRegistry.Player",
                "SceneFader.Instance",
                "SceneLoader.Instance",
                "SettingsManager.Instance",
                "TimeManager.Instance"
            };

            string[] offenders = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(editorDirectorySegment))
                .SelectMany(path =>
                {
                    string source = File.ReadAllText(path);
                    return forbiddenPatterns
                        .Where(source.Contains)
                        .Select(pattern => $"{path} uses {pattern}");
                })
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Runtime code should consume explicit services, authored references, or injected contracts instead of legacy singleton entry points. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void GameplayRuntime_Source_DoesNotReadGameplayPlatformContextCurrentOutsideContextOwner()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            string contextOwnerPath = Path.Combine(gameplayRoot, "Core", "Runtime", "GameplayPlatformContext.cs");
            string[] offenders = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(editorDirectorySegment))
                .Where(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(contextOwnerPath), System.StringComparison.OrdinalIgnoreCase))
                .Where(path => File.ReadAllText(path).Contains("GameplayPlatformContext.Current"))
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Runtime code should use GameplayPlatformContext.TryResolve, TryGetServices, or TryGetCurrent instead of reading Current directly. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void GameplayRuntime_Source_DoesNotCreateAnonymousPlatformServiceRegistries()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            string contextOwnerPath = Path.Combine(gameplayRoot, "Core", "Runtime", "GameplayPlatformContext.cs");
            string[] offenders = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(editorDirectorySegment))
                .Where(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(contextOwnerPath), System.StringComparison.OrdinalIgnoreCase))
                .Where(path => File.ReadAllText(path).Contains("GameplayPlatformContext.GetServicesOrEmpty"))
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Runtime code should use TryGetServices and handle missing platform setup explicitly instead of creating anonymous PlatformServiceRegistry fallbacks. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void GameplayRuntime_Source_RestrictsGlobalSceneDiscoveryToSceneGuard()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string editorDirectorySegment = Path.DirectorySeparatorChar + "Editor" + Path.DirectorySeparatorChar;
            string allowedSceneGuardPath = Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "SceneGuard.cs");
            string[] forbiddenPatterns =
            {
                "Camera.main",
                "FindAnyObjectByType<",
                "FindObjectOfType<",
                "FindObjectsByType<",
                "FindObjectsOfType<",
                "GameObject.Find(",
                "GameObject.FindGameObjectWithTag",
                "GameObject.FindGameObjectsWithTag"
            };

            string[] offenders = Directory.GetFiles(gameplayRoot, "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(editorDirectorySegment))
                .Where(path => !Path.GetFullPath(path).Equals(Path.GetFullPath(allowedSceneGuardPath), System.StringComparison.OrdinalIgnoreCase))
                .SelectMany(path =>
                {
                    string source = File.ReadAllText(path);
                    return forbiddenPatterns
                        .Where(source.Contains)
                        .Select(pattern => $"{path} uses {pattern}");
                })
                .ToArray();

            Assert.That(offenders, Is.Empty,
                "Runtime code should avoid broad scene discovery. SceneGuard is the only current exception because it cleans duplicate EventSystems and AudioListeners during scene transitions. Offenders: " + string.Join(", ", offenders));
        }

        [Test]
        public void ActorFeatureContext_Source_UsesNarrowRuntimeCapabilities()
        {
            string contextPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Composition",
                "ActorFeatureContext.cs");

            string source = File.ReadAllText(contextPath);

            Assert.That(source.Contains("public PawnRoot PawnRoot"), Is.False, "ActorFeatureContext should not expose PawnRoot directly.");
            Assert.That(source.Contains("public EnemyAI EnemyAI"), Is.False, "ActorFeatureContext should not expose EnemyAI directly.");
            Assert.That(source.Contains("public ActorAnimationDriver AnimationDriver"), Is.False, "ActorFeatureContext should expose animation through a contract.");
            Assert.That(source.Contains("public KnockbackReceiver KnockbackReceiver"), Is.False, "ActorFeatureContext should expose knockback through a contract.");
            Assert.That(source.Contains("using NeonBlack.Gameplay.Features.Combat;"), Is.False, "ActorFeatureContext should not depend on Combat namespaces.");
            Assert.That(source.Contains("public HealthComponent Health"), Is.False, "ActorFeatureContext should expose health through a core contract.");
            Assert.That(source.Contains("public PawnDefinition PawnDefinition"), Is.True);
            Assert.That(source.Contains("public IActorHealthState Health"), Is.True);
            Assert.That(source.Contains("public IActorAnimationController Animation"), Is.True);
            Assert.That(source.Contains("public IActorKnockbackController Knockback"), Is.True);
            Assert.That(source.Contains("public IEnemyActorState EnemyActorState"), Is.True);
        }

        [Test]
        public void Motor2D_Source_IsFacadeOverDedicated2DComponents()
        {
            string motorPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Features",
                "Characters",
                "2D",
                "Motor2D.cs");

            string source = File.ReadAllText(motorPath);

            Assert.That(source.Contains("RequireComponent(typeof(Pawn2DMovementComponent))"), Is.True);
            Assert.That(source.Contains("RequireComponent(typeof(Pawn2DPresentationComponent))"), Is.True);
            Assert.That(source.Contains("IPawnMotor"), Is.False,
                "Motor2D should stay the shared 2D motor surface, not a second movement-profile owner beside Pawn2DMovementComponent.");
            Assert.That(source.Contains("IPawnPresentationModule"), Is.False,
                "Motor2D should not double-apply presentation profiles beside Pawn2DPresentationComponent.");
            Assert.That(source.Contains("private Pawn2DMovementComponent movement;"), Is.True);
            Assert.That(source.Contains("private readonly Motor2DModel"), Is.False, "Motor2D should no longer own the movement model directly.");
        }

        [Test]
        public void GameplayStarterPackFactory_Source_CreatesProjectileAuthoringAssets()
        {
            string factoryPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Editor",
                "GameplayStarterPackFactory.cs");

            string source = File.ReadAllText(factoryPath);

            Assert.That(source.Contains("SampleHitscanFireMode"), Is.True);
            Assert.That(source.Contains("SampleProjectileImpact"), Is.True);
            Assert.That(source.Contains("SampleHitscanProjectile"), Is.True);
            Assert.That(source.Contains("ProjectileDeliveryMode.Hitscan"), Is.True);
            Assert.That(source.Contains("SaveAsPrefabAsset"), Is.True);
            Assert.That(source.Contains("Pawn3DMovementComponent"), Is.True);
            Assert.That(source.Contains("Pawn3DPresentationComponent"), Is.True);
        }

        [Test]
        public void GameplayStarterPackFactory_Source_CreatesRuntimePatternSetupAssets()
        {
            string factoryPath = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay",
                "Editor",
                "GameplayStarterPackFactory.cs");

            string source = File.ReadAllText(factoryPath);

            Assert.That(source.Contains("RuntimePatternDefinition"), Is.True);
            Assert.That(source.Contains("GameSetupProfile"), Is.True);
            Assert.That(source.Contains("PatternRealtimeCharacter"), Is.True);
            Assert.That(source.Contains("PatternProjectileCombat"), Is.True);
            Assert.That(source.Contains("PatternBoardCardTabletop"), Is.True);
            Assert.That(source.Contains("SetupBrawlerWithProjectiles"), Is.True);
        }

        [Test]
        public void SceneAndInputFlow_Source_UsesExplicitServicesInsteadOfRuntimeSingletons()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string sceneNavigatorContract = File.ReadAllText(Path.Combine(gameplayRoot, "Core", "Contracts", "ISceneNavigator.cs"));
            string inputSettingsContract = File.ReadAllText(Path.Combine(gameplayRoot, "Core", "Contracts", "IInputSettingsReceiver.cs"));
            string sceneFaderSource = File.ReadAllText(Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "SceneFader.cs"));
            string sceneLoaderSource = File.ReadAllText(Path.Combine(gameplayRoot, "Core", "SceneLoader.cs"));
            string sceneNavigatorSource = File.ReadAllText(Path.Combine(gameplayRoot, "Core", "SceneNavigator.cs"));
            string mainMenuSource = File.ReadAllText(Path.Combine(gameplayRoot, "Core", "Navigation", "UI", "MainMenuManager.cs"));
            string gameManagerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "GameFlow", "2D", "GameManager.cs"));
            string inputHandlerSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Input", "2D", "PlayerInputHandler.cs"));
            string inputZoneEditorSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Input", "2D", "Editor", "InputZoneSetEditor.cs"));
            string hazardSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.cs"));
            string hazardSlamSource = File.ReadAllText(Path.Combine(gameplayRoot, "Features", "Hazards", "2D", "Hazard.SlamSequence.cs"));

            Assert.That(sceneNavigatorContract.Contains("void QuitGame();"), Is.True);
            Assert.That(inputSettingsContract.Contains("interface IInputSettingsRegistrar"), Is.True);
            Assert.That(sceneFaderSource.Contains("ISceneNavigator"), Is.True);
            Assert.That(sceneLoaderSource.Contains("ISceneNavigator"), Is.True);
            Assert.That(sceneNavigatorSource.Contains("SceneLoader.Instance"), Is.False);

            Assert.That(mainMenuSource.Contains("sceneNavigatorSource"), Is.True);
            Assert.That(mainMenuSource.Contains("SceneLoader.Instance"), Is.False);
            Assert.That(gameManagerSource.Contains("sceneNavigatorSource"), Is.True);
            Assert.That(gameManagerSource.Contains("settingsSource"), Is.True);
            Assert.That(gameManagerSource.Contains("SceneFader.Instance"), Is.False);
            Assert.That(gameManagerSource.Contains("SettingsManager.Instance"), Is.False);

            Assert.That(inputHandlerSource.Contains("_settingsRegistrarSource"), Is.True);
            Assert.That(inputHandlerSource.Contains("ResolveInputSettingsRegistrar"), Is.True);
            Assert.That(inputHandlerSource.Contains("SettingsManager.Instance"), Is.False);
            Assert.That(inputZoneEditorSource.Contains("Camera.main"), Is.False);
            Assert.That(hazardSource.Contains("_settingsSource"), Is.True);
            Assert.That(hazardSource.Contains("SetSettings(IGameplaySettingsApplier"), Is.True);
            Assert.That(hazardSlamSource.Contains("SettingsManager.Instance"), Is.False);
            Assert.That(hazardSlamSource.Contains("ResolveSfxVolume()"), Is.True);
        }

        [Test]
        public void ReflectiveAuthoring_KeyInterfacesAreTagged()
        {
            var keyInterfaces = new[]
            {
                typeof(IActionResolver),
                typeof(ITurnOrderService),
                typeof(IActorAnimationController),
                typeof(ICameraBoundsProvider),
                typeof(IFeatureModuleRuntime),
                typeof(ICharacterMotorState),
                typeof(IMovementModule),
                typeof(IPawnCombatModule)
            };

            foreach (var type in keyInterfaces)
            {
                var attr = type.GetCustomAttributes(typeof(AuthoringContractAttribute), true);
                Assert.That(attr.Length, Is.GreaterThan(0), $"Expected interface {type.Name} to be tagged with [AuthoringContract] for reflective discovery.");
            }
        }
    }
}
