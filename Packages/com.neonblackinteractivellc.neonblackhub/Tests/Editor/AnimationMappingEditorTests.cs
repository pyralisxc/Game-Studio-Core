using System.IO;
using System.Linq;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Presentation.Animation;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public class AnimationMappingEditorTests : PyralisEditorTestSupport
    {
        [Test]
        public void ActorAnimationDriver_AppliesValidatedBoolFloatAndIntBindings()
        {
            AnimatorController controller = CreateTestAnimatorController("RuntimeAnimationMapping");
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("ShimmySpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("ComboStep", AnimatorControllerParameterType.Int);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);

            GameObject actor = new GameObject("AnimatedActor");
            Animator animator = actor.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            ActorAnimationDriver driver = actor.AddComponent<ActorAnimationDriver>();

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.baseController = controller;
            profile.bindings = new[]
            {
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Move,
                    bindingType = ActorAnimationBindingType.Bool,
                    parameterName = "IsMoving"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Shimmy,
                    bindingType = ActorAnimationBindingType.Float,
                    parameterName = "ShimmySpeed"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.AttackPrimary,
                    bindingType = ActorAnimationBindingType.Int,
                    parameterName = "ComboStep"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = "Speed",
                    bindingType = ActorAnimationBindingType.Float,
                    parameterName = "Speed"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = "MoveX",
                    bindingType = ActorAnimationBindingType.Float,
                    parameterName = "MoveX"
                }
            };

            driver.ApplyProfiles(null, profile);
            driver.SetBoolSignal(ActorAnimationSignal.Move, true);
            driver.SetFloatSignal(ActorAnimationSignal.Shimmy, 0.75f);
            driver.SetIntSignal(ActorAnimationSignal.AttackPrimary, 2);
            driver.SetFloatCustom("Speed", 4.5f);
            driver.SetFloatCustom("MoveX", -1f);

            Assert.That(animator.GetBool("IsMoving"), Is.True);
            Assert.That(animator.GetFloat("ShimmySpeed"), Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(animator.GetInteger("ComboStep"), Is.EqualTo(2));
            Assert.That(animator.GetFloat("Speed"), Is.EqualTo(4.5f).Within(0.001f));
            Assert.That(animator.GetFloat("MoveX"), Is.EqualTo(-1f).Within(0.001f));

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void PawnAnimationProfileValidation_AppendsBlendTreeFloatChannelsFromController()
        {
            AnimatorController controller = CreateTestAnimatorController("BlendTreeFloatMapping");
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("VerticalVelocity", AnimatorControllerParameterType.Float);

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.baseController = controller;
            profile.bindings = System.Array.Empty<ActorAnimationBinding>();

            PawnAnimationProfileValidation.AppendSuggestedBindings(profile);
            System.Collections.Generic.List<string> issues = PawnAnimationProfileValidation.GetValidationIssues(profile);

            Assert.That(issues.Exists(issue => issue.Contains("Speed")), Is.False);
            AssertMapped(profile, ActorAnimationSignal.Custom, "Speed", "Speed", ActorAnimationBindingType.Float);
            AssertMapped(profile, ActorAnimationSignal.Custom, "MoveX", "MoveX", ActorAnimationBindingType.Float);
            AssertMapped(profile, ActorAnimationSignal.Custom, "MoveY", "MoveY", ActorAnimationBindingType.Float);
            AssertMapped(profile, ActorAnimationSignal.Custom, "VelocityY", "VerticalVelocity", ActorAnimationBindingType.Float);

            Object.DestroyImmediate(profile);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void PawnAnimationProfileValidation_DetectsCustomKeysAndDuplicateBindings()
        {
            AnimatorController controller = CreateTestAnimatorController("AnimationMappingDuplicates");
            controller.AddParameter("ComboConfirm", AnimatorControllerParameterType.Trigger);

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.baseController = controller;
            profile.bindings = new[]
            {
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "ComboConfirm"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = "ComboConfirm",
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "ComboConfirm"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = "ComboConfirm",
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "ComboConfirm"
                }
            };

            System.Collections.Generic.List<string> issues = PawnAnimationProfileValidation.GetValidationIssues(profile);

            Assert.That(issues.Exists(issue => issue.Contains("Custom") && issue.Contains("no custom key")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("duplicated")), Is.True);

            Object.DestroyImmediate(profile);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void PawnAnimationProfileValidation_ExposesTypedParameterChoicesAndMappingSummary()
        {
            AnimatorController controller = CreateTestAnimatorController("AnimationMappingWizardSummary");
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("ComboStep", AnimatorControllerParameterType.Int);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            ActorAnimationDefinition definition = ScriptableObject.CreateInstance<ActorAnimationDefinition>();
            definition.supportedSignals = new[]
            {
                ActorAnimationSignal.Move,
                ActorAnimationSignal.Jump
            };

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.animationDefinition = definition;
            profile.baseController = controller;
            profile.bindings = new[]
            {
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Move,
                    bindingType = ActorAnimationBindingType.Bool,
                    parameterName = "IsMoving"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Jump,
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "Jump"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = "Speed",
                    bindingType = ActorAnimationBindingType.Float,
                    parameterName = "Speed"
                }
            };

            System.Collections.Generic.IReadOnlyList<PawnAnimationParameterInfo> boolParameters =
                PawnAnimationProfileValidation.GetCompatibleParameters(profile, ActorAnimationBindingType.Bool);
            System.Collections.Generic.IReadOnlyList<PawnAnimationParameterInfo> floatParameters =
                PawnAnimationProfileValidation.GetCompatibleParameters(profile, ActorAnimationBindingType.Float);
            PawnAnimationMappingSummary summary = PawnAnimationProfileValidation.GetMappingSummary(profile);

            Assert.That(boolParameters.Any(parameter => parameter.Name == "IsMoving"), Is.True);
            Assert.That(boolParameters.Any(parameter => parameter.Name == "Speed"), Is.False);
            Assert.That(floatParameters.Any(parameter => parameter.Name == "Speed"), Is.True);
            Assert.That(summary.ControllerParameterCount, Is.EqualTo(4));
            Assert.That(summary.BindingCount, Is.EqualTo(3));
            Assert.That(summary.MappedSignalCount, Is.EqualTo(2));
            Assert.That(summary.SupportedSignalCount, Is.EqualTo(2));
            Assert.That(summary.CustomChannelCount, Is.EqualTo(1));
            Assert.That(summary.IssueCount, Is.EqualTo(0));
            Assert.That(summary.ReadinessLabel, Is.EqualTo("Ready"));

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(definition);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void PawnAnimationProfileValidation_GroupsIssuesForGuidedAuthoring()
        {
            AnimatorController controller = CreateTestAnimatorController("AnimationMappingWizardIssues");
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("ComboConfirm", AnimatorControllerParameterType.Trigger);

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.baseController = controller;
            profile.bindings = new[]
            {
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Sprint,
                    bindingType = ActorAnimationBindingType.Bool,
                    parameterName = "Speed"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "ComboConfirm"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = "ComboConfirm",
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "ComboConfirm"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Custom,
                    customKey = "ComboConfirm",
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "ComboConfirm"
                }
            };

            System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> groups =
                PawnAnimationProfileValidation.GetValidationIssueGroups(profile);

            Assert.That(groups.ContainsKey("Setup"), Is.True);
            Assert.That(groups.ContainsKey("Animator Parameter Mismatch"), Is.True);
            Assert.That(groups.ContainsKey("Custom Channels"), Is.True);
            Assert.That(groups.ContainsKey("Duplicate Bindings"), Is.True);

            Object.DestroyImmediate(profile);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void PawnAnimationProfileValidation_ReplaceWithSuggestedBindingsClearsManualMappings()
        {
            AnimatorController controller = CreateTestAnimatorController("AnimationMappingWizardReplace");
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.baseController = controller;
            profile.bindings = new[]
            {
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Death,
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "LegacyDeath"
                }
            };

            PawnAnimationProfileValidation.ReplaceWithSuggestedBindings(profile);

            Assert.That(profile.bindings.Any(binding => binding.parameterName == "LegacyDeath"), Is.False);
            AssertMapped(profile, ActorAnimationSignal.Move, "IsMoving", ActorAnimationBindingType.Bool);
            AssertMapped(profile, ActorAnimationSignal.Custom, "Speed", "Speed", ActorAnimationBindingType.Float);

            Object.DestroyImmediate(profile);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void ImportedAnimatorControllerFixture_CanGenerateAndUseValidGenericSignalMappings()
        {
            const string controllerPath = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Public/Apocalyptia/Animations/Player/Player.controller";
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            Assert.That(controller, Is.Not.Null, "The fixture should behave like an imported third-party Animator Controller.");

            ActorAnimationDefinition definition = ScriptableObject.CreateInstance<ActorAnimationDefinition>();
            definition.supportedSignals = System.Array.Empty<ActorAnimationSignal>();

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.animationDefinition = definition;
            profile.baseController = controller;
            profile.bindings = System.Array.Empty<ActorAnimationBinding>();

            PawnAnimationProfileValidation.AppendSuggestedBindings(profile);
            System.Collections.Generic.List<string> issues = PawnAnimationProfileValidation.GetValidationIssues(profile);

            Assert.That(ContainsOnlyMissingSpriteReferenceIssues(issues), Is.True, string.Join("\n", issues));
            AssertMapped(profile, ActorAnimationSignal.Move, "IsMoving", ActorAnimationBindingType.Bool);
            AssertMapped(profile, ActorAnimationSignal.Sprint, "IsSprinting", ActorAnimationBindingType.Bool);
            AssertMapped(profile, ActorAnimationSignal.Crouch, "IsCrouching", ActorAnimationBindingType.Bool);
            AssertMapped(profile, ActorAnimationSignal.Jump, "Jump", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.Dash, "DodgeFwd", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.Slide, "Slide", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.Hang, "IsHanging", ActorAnimationBindingType.Bool);
            AssertMapped(profile, ActorAnimationSignal.Shimmy, "ShimmySpeed", ActorAnimationBindingType.Float);
            AssertMapped(profile, ActorAnimationSignal.AttackPrimary, "RightPunch", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.AttackSecondary, "RightKick", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.BlockLoop, "Block", ActorAnimationBindingType.Bool);
            AssertMapped(profile, ActorAnimationSignal.Hurt, "KnockedBack", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.Interact, "Interact", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.SideClimb, "SideClimb", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.ForwardClimb, "FwdClimb", ActorAnimationBindingType.Trigger);
            AssertMapped(profile, ActorAnimationSignal.LedgeDrop, "LedgeDrop", ActorAnimationBindingType.Trigger);

            GameObject ricoActor = new GameObject("Rico Animator Controller Proof");
            Animator animator = ricoActor.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            ActorAnimationDriver driver = ricoActor.AddComponent<ActorAnimationDriver>();

            driver.ApplyProfiles(null, profile);
            driver.SetBoolSignal(ActorAnimationSignal.Move, true);
            driver.SetBoolSignal(ActorAnimationSignal.Sprint, true);
            driver.SetBoolSignal(ActorAnimationSignal.Crouch, true);
            driver.SetBoolSignal(ActorAnimationSignal.BlockLoop, true);
            driver.SetFloatSignal(ActorAnimationSignal.Shimmy, 0.6f);
            driver.TriggerSignal(ActorAnimationSignal.Jump);
            driver.TriggerSignal(ActorAnimationSignal.AttackPrimary);
            driver.TriggerSignal(ActorAnimationSignal.AttackSecondary);

            Assert.That(animator.runtimeAnimatorController, Is.SameAs(controller));
            Assert.That(animator.GetBool("IsMoving"), Is.True);
            Assert.That(animator.GetBool("IsSprinting"), Is.True);
            Assert.That(animator.GetBool("IsCrouching"), Is.True);
            Assert.That(animator.GetBool("Block"), Is.True);
            Assert.That(animator.GetFloat("ShimmySpeed"), Is.EqualTo(0.6f).Within(0.001f));

            Object.DestroyImmediate(ricoActor);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PawnStarterPackAuthoring_CanAssignRicoControllerThroughMappingWizard()
        {
            const string testFolder = "Assets/Temp/PyralisAuthoringStarterPackTest";
            const string starterPackRoot = testFolder + "/PawnStarterPack";
            const string controllerPath = "Packages/com.neonblackinteractivellc.neonblackhub/Members/Public/Apocalyptia/Animations/Player/Player.controller";

            AssetDatabase.DeleteAsset(testFolder);
            if (!AssetDatabase.IsValidFolder("Assets/Temp"))
                AssetDatabase.CreateFolder("Assets", "Temp");
            AssetDatabase.CreateFolder("Assets/Temp", "PyralisAuthoringStarterPackTest");

            Object previousSelection = Selection.activeObject;
            try
            {
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(testFolder);

                GameplayStarterPackFactory.CreatePawnStarterPack();

                Assert.That(AssetDatabase.IsValidFolder(starterPackRoot), Is.True, "The internal starter-pack factory should still create the expected authoring folder for coverage.");

                PawnAnimationProfile profile = AssetDatabase.LoadAssetAtPath<PawnAnimationProfile>($"{starterPackRoot}/Profiles/AnimationProfile.asset");
                ActorAnimationDefinition definition = AssetDatabase.LoadAssetAtPath<ActorAnimationDefinition>($"{starterPackRoot}/Definitions/DefaultActorAnimationDefinition.asset");
                PawnDefinition pawn = AssetDatabase.LoadAssetAtPath<PawnDefinition>($"{starterPackRoot}/Definitions/Sprite2DPawnDefinition.asset");
                AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

                Assert.That(profile, Is.Not.Null);
                Assert.That(definition, Is.Not.Null);
                Assert.That(pawn, Is.Not.Null);
                Assert.That(controller, Is.Not.Null, "The Rico controller fixture should be available for a real imported-controller authoring test.");
                Assert.That(profile.animationDefinition, Is.SameAs(definition));
                Assert.That(pawn.animationProfile, Is.SameAs(profile));
                Assert.That(pawn.pawnPrefab, Is.Not.Null);

                Animator prefabAnimator = pawn.pawnPrefab.GetComponentInChildren<Animator>(true);
                ActorAnimationDriver prefabDriver = pawn.pawnPrefab.GetComponent<ActorAnimationDriver>();
                Pawn2DPresentationComponent presentation = pawn.pawnPrefab.GetComponent<Pawn2DPresentationComponent>();
                Assert.That(prefabAnimator, Is.Not.Null, "The generated 2D starter pawn should expose a visual Animator for imported controllers.");
                Assert.That(prefabDriver, Is.Not.Null);
                Assert.That(presentation, Is.Not.Null);

                SerializedObject driverObject = new SerializedObject(prefabDriver);
                Assert.That(driverObject.FindProperty("animator")?.objectReferenceValue, Is.SameAs(prefabAnimator));
                SerializedObject presentationObject = new SerializedObject(presentation);
                Assert.That(presentationObject.FindProperty("animator")?.objectReferenceValue, Is.SameAs(prefabAnimator));

                profile.baseController = controller;
                PawnAnimationProfileValidation.ReplaceWithSuggestedBindings(profile);
                System.Collections.Generic.List<string> issues = PawnAnimationProfileValidation.GetValidationIssues(profile);

                Assert.That(ContainsOnlyMissingSpriteReferenceIssues(issues), Is.True, string.Join("\n", issues));
                Assert.That(issues.Any(issue => issue.Contains("missing SpriteRenderer sprite frame reference")), Is.True);
                AssertMapped(profile, ActorAnimationSignal.Move, "IsMoving", ActorAnimationBindingType.Bool);
                AssertMapped(profile, ActorAnimationSignal.Dash, "DodgeFwd", ActorAnimationBindingType.Trigger);
                AssertMapped(profile, ActorAnimationSignal.AttackPrimary, "RightPunch", ActorAnimationBindingType.Trigger);
                AssertMapped(profile, ActorAnimationSignal.AttackSecondary, "RightKick", ActorAnimationBindingType.Trigger);
                AssertMapped(profile, ActorAnimationSignal.BlockLoop, "Block", ActorAnimationBindingType.Bool);
                Assert.That(profile.bindings.Any(binding => binding.signal == ActorAnimationSignal.SideClimb), Is.False, "Suggestions should not add unsupported starter definition signals.");

                GameObject instance = PrefabUtility.InstantiatePrefab(pawn.pawnPrefab) as GameObject;
                try
                {
                    Assert.That(instance, Is.Not.Null);
                    Animator runtimeAnimator = instance.GetComponentInChildren<Animator>(true);
                    ActorAnimationDriver runtimeDriver = instance.GetComponent<ActorAnimationDriver>();

                    runtimeDriver.ApplyProfiles(pawn.presentationProfile, profile);
                    runtimeDriver.SetBoolSignal(ActorAnimationSignal.Move, true);

                    Assert.That(runtimeAnimator.runtimeAnimatorController, Is.SameAs(controller));
                    Assert.That(runtimeAnimator.GetBool("IsMoving"), Is.True);
                }
                finally
                {
                    if (instance != null)
                        Object.DestroyImmediate(instance);
                }
            }
            finally
            {
                Selection.activeObject = previousSelection;
                AssetDatabase.DeleteAsset(testFolder);
                DeleteFolderIfEmpty("Assets/Temp");
            }
        }

        [Test]
        public void AnimationMappingSource_DoesNotDependOnApocalyptiaFixture()
        {
            string gameplayRoot = Path.Combine(
                Application.dataPath,
                "..",
                "Packages",
                "com.neonblackinteractivellc.neonblackhub",
                "Members",
                "Pyralis",
                "Gameplay");

            string[] genericMappingSources =
            {
                Path.Combine(gameplayRoot, "Editor", "PawnAnimationProfileEditor.cs"),
                Path.Combine(gameplayRoot, "Presentation", "Animation", "ActorAnimationDriver.cs")
            };

            for (int i = 0; i < genericMappingSources.Length; i++)
            {
                string source = File.ReadAllText(genericMappingSources[i]);
                Assert.That(source.Contains("Apocalyptia"), Is.False, genericMappingSources[i]);
                Assert.That(source.Contains("Members/Public"), Is.False, genericMappingSources[i]);
            }
        }

        private static void AssertMapped(
            PawnAnimationProfile profile,
            ActorAnimationSignal signal,
            string parameterName,
            ActorAnimationBindingType bindingType)
        {
            Assert.That(profile.bindings.Any(binding =>
                binding.signal == signal
                && binding.parameterName == parameterName
                && binding.bindingType == bindingType), Is.True, $"{signal} should map to {parameterName} ({bindingType}).");
        }

        private static void AssertMapped(
            PawnAnimationProfile profile,
            ActorAnimationSignal signal,
            string customKey,
            string parameterName,
            ActorAnimationBindingType bindingType)
        {
            Assert.That(profile.bindings.Any(binding =>
                binding.signal == signal
                && binding.customKey == customKey
                && binding.parameterName == parameterName
                && binding.bindingType == bindingType), Is.True, $"{signal}:{customKey} should map to {parameterName} ({bindingType}).");
        }

        private static bool ContainsOnlyMissingSpriteReferenceIssues(System.Collections.Generic.IReadOnlyList<string> issues)
        {
            if (issues == null || issues.Count == 0)
                return true;

            return issues.All(issue => issue.Contains("missing SpriteRenderer sprite frame reference"));
        }
    }
}
