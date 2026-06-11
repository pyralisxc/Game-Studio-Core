using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Editor;
using NeonBlack.Gameplay.Editor.Inspectors;
using NeonBlack.Gameplay.Core.Runtime;
using NUnit.Framework;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NeonBlack.Gameplay.Tests.Editor
{
    public class DefinitionValidationTests : PyralisEditorTestSupport
    {
        [Test]
        public void ActionDefinition_Sanitize_FillsLabelsAndClampsCosts()
        {
            ActionDefinition definition = ScriptableObject.CreateInstance<ActionDefinition>();
            definition.name = "FallbackActionName";
            definition.actionId = "";
            definition.displayName = "";
            definition.actionFamily = "";
            definition.cooldown = -3f;
            definition.resourceCost = -5;

            definition.Sanitize();

            Assert.That(definition.actionId, Is.EqualTo("FallbackActionName"));
            Assert.That(definition.displayName, Is.EqualTo("FallbackActionName"));
            Assert.That(definition.actionFamily, Is.EqualTo("General"));
            Assert.That(definition.cooldown, Is.EqualTo(0f));
            Assert.That(definition.resourceCost, Is.EqualTo(0));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ActionDefinition_GetValidationIssues_FlagsInvalidNoTargetCounts()
        {
            ActionDefinition definition = ScriptableObject.CreateInstance<ActionDefinition>();
            definition.actionId = "action.invalid";
            definition.displayName = "Invalid Action";
            definition.actionFamily = "Tests";
            definition.targetRule = new ActionTargetRule
            {
                targetKind = ActionTargetKind.None,
                minTargets = 1,
                maxTargets = 1
            };

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("No-target actions")), Is.True);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ActionDefinition_ValidateTargets_AllowsNoTargetMenuAction()
        {
            ActionDefinition definition = ScriptableObject.CreateInstance<ActionDefinition>();
            definition.actionId = "action.end-turn";
            definition.targetRule = ActionTargetRule.None();

            ActionValidationResult result = definition.ValidateTargets(new ActionExecutionContext("action.end-turn"));

            Assert.That(result.IsValid, Is.True);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void RuntimePatternDefinition_GetValidationIssues_AllowsProjectilePatternWithoutRequiredPawn()
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            pattern.patternId = "pattern.projectile-combat";
            pattern.displayName = "Projectile Combat";
            pattern.description = "Projectile test pattern.";
            pattern.setupNotes = "Create projectile assets and a launcher.";
            pattern.capabilityFamily = RuntimeCapabilityFamily.GunsProjectiles;
            pattern.participantEmbodiment = ParticipantEmbodimentRequirement.OptionalPawn;
            pattern.supportedControlSurfaces = new[]
            {
                RuntimeControlSurface.Pawn,
                RuntimeControlSurface.Camera,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.CardHand,
                RuntimeControlSurface.SystemAI
            };

            System.Collections.Generic.List<string> issues = pattern.GetValidationIssues();

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void RuntimePatternDefinition_GetValidationIssues_FlagsMissingIdentity()
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            pattern.patternId = "";
            pattern.displayName = "";
            pattern.supportedControlSurfaces = new[] { RuntimeControlSurface.Pawn };

            System.Collections.Generic.List<string> issues = pattern.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Pattern id")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Display name")), Is.True);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void RuntimePatternDefinition_GetValidationIssues_FlagsRequiredPawnWithoutPawnSurface()
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            pattern.patternId = "pattern.invalid-character";
            pattern.displayName = "Invalid Character";
            pattern.participantEmbodiment = ParticipantEmbodimentRequirement.RequiredPawn;
            pattern.supportedControlSurfaces = new[] { RuntimeControlSurface.Cursor };

            System.Collections.Generic.List<string> issues = pattern.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("requires a pawn")), Is.True);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void RuntimePatternDefinition_GetValidationIssues_FlagsNonPawnRequiredWithOnlyPawnSurface()
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            pattern.patternId = "pattern.invalid-tabletop";
            pattern.displayName = "Invalid Tabletop";
            pattern.participantEmbodiment = ParticipantEmbodimentRequirement.NonPawnSurfaceRequired;
            pattern.supportedControlSurfaces = new[] { RuntimeControlSurface.Pawn };

            System.Collections.Generic.List<string> issues = pattern.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("non-pawn")), Is.True);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void RuntimePatternDefinition_GetValidationIssues_FlagsPlaceholderAuthoringText()
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();

            System.Collections.Generic.List<string> issues = pattern.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("default placeholder")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Description is required")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Setup notes are required")), Is.True);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void RuntimePatternDefinition_GetValidationIssues_AcceptsManuallyAuthoredPawnRoute()
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();

            pattern.patternId = "pattern.local-brawler-pawn";
            pattern.displayName = "Local Brawler Pawn";
            pattern.description = "Participants own pawn actors for a local brawler route.";
            pattern.capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay;
            pattern.participantEmbodiment = ParticipantEmbodimentRequirement.RequiredPawn;
            pattern.supportedControlSurfaces = new[] { RuntimeControlSurface.Pawn };
            pattern.presentationLanes = new[] { RuntimePatternPresentationLane.Rigged3D };
            pattern.firstProofRequirements = RuntimePatternFirstProofRequirement.SpawnPoints
                | RuntimePatternFirstProofRequirement.CameraRig
                | RuntimePatternFirstProofRequirement.PlayerInputManager
                | RuntimePatternFirstProofRequirement.EnemyOrNpcSpawner;
            pattern.setupNotes = "Create participants, pawn definitions, rigged pawn prefabs, spawn points, camera rig evidence, and local join evidence.";
            pattern.Sanitize();

            Assert.That(pattern.capabilityFamily, Is.EqualTo(RuntimeCapabilityFamily.CharacterPawnGameplay));
            Assert.That(pattern.participantEmbodiment, Is.EqualTo(ParticipantEmbodimentRequirement.RequiredPawn));
            Assert.That(pattern.supportedControlSurfaces, Is.EquivalentTo(new[] { RuntimeControlSurface.Pawn }));
            Assert.That(pattern.presentationLanes, Is.EquivalentTo(new[] { RuntimePatternPresentationLane.Rigged3D }));
            Assert.That(pattern.RequiresFirstProof(RuntimePatternFirstProofRequirement.PlayerInputManager), Is.True);
            Assert.That(pattern.GetValidationIssues(), Is.Empty);

            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void GameSetupProfile_GetValidationIssues_AllowsCompatibleRuntimePatternOverlap()
        {
            RuntimePatternDefinition realtime = CreateRuntimePattern(
                "pattern.realtime-character",
                "Realtime Character",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);

            RuntimePatternDefinition projectile = CreateRuntimePattern(
                "pattern.projectile-combat",
                "Projectile Combat",
                RuntimeCapabilityFamily.GunsProjectiles,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn,
                RuntimeControlSurface.Cursor,
                RuntimeControlSurface.SystemAI);

            projectile.recommendedCompanionPatterns = new[] { realtime };

            GameSetupProfile profile = ScriptableObject.CreateInstance<GameSetupProfile>();
            profile.setupName = "Side-Scrolling Shooter";
            profile.runtimePatterns = new[] { realtime, projectile };

            System.Collections.Generic.List<string> issues = profile.GetValidationIssues();

            Assert.That(issues, Is.Empty);
            Assert.That(profile.HasPattern("pattern.projectile-combat"), Is.True);

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(projectile);
            Object.DestroyImmediate(realtime);
        }

        [Test]
        public void GameSetupProfile_GetValidationIssues_FlagsDuplicateRuntimePatternIds()
        {
            RuntimePatternDefinition first = CreateRuntimePattern(
                "pattern.projectile-combat",
                "Projectile Combat",
                RuntimeCapabilityFamily.GunsProjectiles,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn,
                RuntimeControlSurface.Cursor);

            RuntimePatternDefinition second = CreateRuntimePattern(
                "pattern.projectile-combat",
                "Projectile Combat Copy",
                RuntimeCapabilityFamily.GunsProjectiles,
                ParticipantEmbodimentRequirement.OptionalPawn,
                RuntimeControlSurface.Pawn);

            GameSetupProfile profile = ScriptableObject.CreateInstance<GameSetupProfile>();
            profile.setupName = "Duplicate Pattern Setup";
            profile.runtimePatterns = new[] { first, second };

            System.Collections.Generic.List<string> issues = profile.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("assigned more than once")), Is.True);

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(second);
            Object.DestroyImmediate(first);
        }

        [Test]
        public void GameSetupProfile_GetValidationIssues_FlagsConflictingRuntimePatterns()
        {
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);

            RuntimePatternDefinition pawnOnly = CreateRuntimePattern(
                "pattern.pawn-only",
                "Pawn Only",
                RuntimeCapabilityFamily.CharacterPawnGameplay,
                ParticipantEmbodimentRequirement.RequiredPawn,
                RuntimeControlSurface.Pawn);

            tabletop.cautionaryCompanionPatterns = new[] { pawnOnly };

            GameSetupProfile profile = ScriptableObject.CreateInstance<GameSetupProfile>();
            profile.setupName = "Conflicting Setup";
            profile.runtimePatterns = new[] { tabletop, pawnOnly };

            System.Collections.Generic.List<string> issues = profile.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("cautions against")), Is.True);

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(pawnOnly);
            Object.DestroyImmediate(tabletop);
        }

        [Test]
        public void SessionDefinition_Sanitize_ClampsParticipantCount()
        {
            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.maxParticipants = 0;

            session.Sanitize();

            Assert.That(session.maxParticipants, Is.EqualTo(1));

            Object.DestroyImmediate(session);
        }

        [Test]
        public void PlayfieldProfile_Sanitize_SwapsInvalidBounds()
        {
            PlayfieldProfile profile = ScriptableObject.CreateInstance<PlayfieldProfile>();
            profile.minBounds = new Vector2(5f, 4f);
            profile.maxBounds = new Vector2(-2f, -1f);
            profile.minDepth = 3f;
            profile.maxDepth = -3f;

            profile.Sanitize();

            Assert.That(profile.minBounds.x, Is.LessThanOrEqualTo(profile.maxBounds.x));
            Assert.That(profile.minBounds.y, Is.LessThanOrEqualTo(profile.maxBounds.y));
            Assert.That(profile.minDepth, Is.LessThanOrEqualTo(profile.maxDepth));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void CameraRigProfile_Sanitize_ClampsZoomRange()
        {
            CameraRigProfile profile = ScriptableObject.CreateInstance<CameraRigProfile>();
            profile.minZoom = 10f;
            profile.maxZoom = 3f;

            profile.Sanitize();

            Assert.That(profile.maxZoom, Is.GreaterThanOrEqualTo(profile.minZoom));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PawnMovementProfile_Sanitize_ClampsNegativeSpeeds()
        {
            PawnMovementProfile profile = ScriptableObject.CreateInstance<PawnMovementProfile>();
            profile.walkSpeed   = -5f;
            profile.sprintSpeed = -10f;
            profile.crouchSpeed = -2f;

            profile.Sanitize();

            Assert.That(profile.walkSpeed,   Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.sprintSpeed, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.crouchSpeed, Is.GreaterThanOrEqualTo(0f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PawnMovementProfile_Sanitize_ClampsDepthMultiplierToRange()
        {
            PawnMovementProfile profileLow  = ScriptableObject.CreateInstance<PawnMovementProfile>();
            PawnMovementProfile profileHigh = ScriptableObject.CreateInstance<PawnMovementProfile>();
            profileLow.depthSpeedMultiplier  = -1f;
            profileHigh.depthSpeedMultiplier = 5f;

            profileLow.Sanitize();
            profileHigh.Sanitize();

            Assert.That(profileLow.depthSpeedMultiplier,  Is.GreaterThanOrEqualTo(0.1f));
            Assert.That(profileHigh.depthSpeedMultiplier, Is.LessThanOrEqualTo(1f));

            Object.DestroyImmediate(profileLow);
            Object.DestroyImmediate(profileHigh);
        }

        [Test]
        public void PawnTraversalProfile_Sanitize_ClampsNegativeValues()
        {
            PawnTraversalProfile profile = ScriptableObject.CreateInstance<PawnTraversalProfile>();
            profile.jumpHeight    = -3f;
            profile.dodgeDistance = -5f;
            profile.dodgeDuration = 0f;
            profile.dodgeCooldown = -1f;
            profile.climbCooldown = -2f;

            profile.Sanitize();

            Assert.That(profile.jumpHeight,    Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.dodgeDistance, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.dodgeDuration, Is.GreaterThanOrEqualTo(0.01f));
            Assert.That(profile.dodgeCooldown, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.climbCooldown, Is.GreaterThanOrEqualTo(0f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PawnCombatProfile_Sanitize_ClampsBlockDamageReductionToRange()
        {
            PawnCombatProfile profileLow  = ScriptableObject.CreateInstance<PawnCombatProfile>();
            PawnCombatProfile profileHigh = ScriptableObject.CreateInstance<PawnCombatProfile>();
            profileLow.blockDamageReduction  = -0.5f;
            profileHigh.blockDamageReduction = 2f;

            profileLow.Sanitize();
            profileHigh.Sanitize();

            Assert.That(profileLow.blockDamageReduction,  Is.GreaterThanOrEqualTo(0f));
            Assert.That(profileHigh.blockDamageReduction, Is.LessThanOrEqualTo(1f));

            Object.DestroyImmediate(profileLow);
            Object.DestroyImmediate(profileHigh);
        }

        [Test]
        public void PawnCombatProfile_Sanitize_ClampsNegativeDamageAndCooldowns()
        {
            PawnCombatProfile profile = ScriptableObject.CreateInstance<PawnCombatProfile>();
            profile.baseDamage     = -10f;
            profile.baseKnockback  = -5f;
            profile.attackCooldown = -0.5f;
            profile.kickCooldown   = -0.8f;
            profile.maxAerialAttacks = -1;

            profile.Sanitize();

            Assert.That(profile.baseDamage,       Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.baseKnockback,    Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.attackCooldown,   Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.kickCooldown,     Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.maxAerialAttacks, Is.GreaterThanOrEqualTo(0));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void ActorAnimationDefinition_Sanitize_RemovesDuplicateSignals()
        {
            ActorAnimationDefinition definition = ScriptableObject.CreateInstance<ActorAnimationDefinition>();
            definition.supportedSignals = new[]
            {
                ActorAnimationSignal.Move,
                ActorAnimationSignal.Move,
                ActorAnimationSignal.Death
            };

            definition.Sanitize();

            Assert.That(definition.supportedSignals.Length, Is.EqualTo(2));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PawnAnimationProfile_Sanitize_CreatesBindingArray()
        {
            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.bindings = null;

            profile.Sanitize();

            Assert.That(profile.bindings, Is.Not.Null);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PawnAnimationProfileValidation_FlagsMissingAndMismatchedControllerParameters()
        {
            AnimatorController controller = CreateTestAnimatorController("AnimationValidationMismatch");
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

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
                    signal = ActorAnimationSignal.Sprint,
                    bindingType = ActorAnimationBindingType.Bool,
                    parameterName = "Speed"
                },
                new ActorAnimationBinding
                {
                    signal = ActorAnimationSignal.Jump,
                    bindingType = ActorAnimationBindingType.Trigger,
                    parameterName = "MissingJump"
                }
            };

            System.Collections.Generic.List<string> issues = NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.GetValidationIssues(profile);

            Assert.That(issues.Exists(issue => issue.Contains("IsMoving")), Is.False);
            Assert.That(issues.Exists(issue => issue.Contains("Speed") && issue.Contains("Float")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("MissingJump") && issue.Contains("missing")), Is.True);

            Object.DestroyImmediate(profile);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void PawnAnimationProfileValidation_AppendsSuggestedBindingsFromImportedController()
        {
            AnimatorController controller = CreateTestAnimatorController("AnimationValidationSuggestions");
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("ShimmySpeed", AnimatorControllerParameterType.Float);

            PawnAnimationProfile profile = ScriptableObject.CreateInstance<PawnAnimationProfile>();
            profile.baseController = controller;
            profile.bindings = System.Array.Empty<ActorAnimationBinding>();

            NeonBlack.Gameplay.Editor.PawnAnimationProfileValidation.AppendSuggestedBindings(profile);

            Assert.That(profile.bindings.Any(binding =>
                binding.signal == ActorAnimationSignal.Move
                && binding.parameterName == "IsMoving"
                && binding.bindingType == ActorAnimationBindingType.Bool), Is.True);
            Assert.That(profile.bindings.Any(binding =>
                binding.signal == ActorAnimationSignal.Jump
                && binding.parameterName == "Jump"
                && binding.bindingType == ActorAnimationBindingType.Trigger), Is.True);
            Assert.That(profile.bindings.Any(binding =>
                binding.signal == ActorAnimationSignal.Shimmy
                && binding.parameterName == "ShimmySpeed"
                && binding.bindingType == ActorAnimationBindingType.Float), Is.True);

            Object.DestroyImmediate(profile);
            DeleteTestAnimatorController(controller);
        }

        [Test]
        public void InputProfile_Sanitize_RemovesBlankAndDuplicateControlSchemes()
        {
            InputProfile profile = ScriptableObject.CreateInstance<InputProfile>();
            profile.primaryActionMap = "";
            profile.actionBindings = new[]
            {
                new GameplayInputActionBinding
                {
                    role = GameplayInputActionRole.Move,
                    actionName = "  Strafe  ",
                    valueType = GameplayInputValueType.Vector2,
                    requiredForProof = true
                },
                new GameplayInputActionBinding
                {
                    role = GameplayInputActionRole.Dash,
                    actionName = "  Dodge  ",
                    valueType = GameplayInputValueType.Button,
                    requiredForProof = false
                }
            };
            profile.preferredControlSchemes = new[] { "Gamepad", "", "Gamepad", "Keyboard&Mouse" };

            profile.Sanitize();

            Assert.That(profile.primaryActionMap, Is.EqualTo("Player"));
            Assert.That(profile.FindBinding(GameplayInputActionRole.Move).actionName, Is.EqualTo("Strafe"));
            Assert.That(profile.FindBinding(GameplayInputActionRole.Jump), Is.Null);
            Assert.That(profile.FindBinding(GameplayInputActionRole.Dash).actionName, Is.EqualTo("Dodge"));
            Assert.That(profile.preferredControlSchemes, Is.EqualTo(new[] { "Gamepad", "Keyboard&Mouse" }));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void GameSetupProfile_Sanitize_SyncsRuntimePatternsFromCapabilitySelections()
        {
            RuntimePatternDefinition pattern = ScriptableObject.CreateInstance<RuntimePatternDefinition>();
            pattern.patternId = "pattern.test.pawn";
            pattern.displayName = "Test Pawn Runtime";
            pattern.capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay;
            pattern.participantEmbodiment = ParticipantEmbodimentRequirement.RequiredPawn;
            pattern.supportedControlSurfaces = new[] { RuntimeControlSurface.Pawn };
            pattern.description = "Test pawn setup.";
            pattern.setupNotes = "Create a pawn proof setup.";

            GameSetupProfile profile = ScriptableObject.CreateInstance<GameSetupProfile>();
            profile.runtimeCapabilities = new[]
            {
                new RuntimeCapabilitySelection
                {
                    capabilityFamily = RuntimeCapabilityFamily.CharacterPawnGameplay,
                    patternDefinition = pattern,
                    requiredForFirstProof = true
                }
            };
            profile.runtimePatterns = System.Array.Empty<RuntimePatternDefinition>();

            profile.Sanitize();

            Assert.That(profile.runtimePatterns, Has.Length.EqualTo(1));
            Assert.That(profile.runtimePatterns[0], Is.SameAs(pattern));
            Assert.That(profile.GetValidationIssues(), Is.Empty);

            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(pattern);
        }

        [Test]
        public void CombatActionDefinition_Sanitize_ClampsInvalidValues()
        {
            CombatActionDefinition definition = ScriptableObject.CreateInstance<CombatActionDefinition>();
            definition.displayName = "";
            definition.comboStep = -2;
            definition.comboWindow = -1f;
            definition.cooldownOverride = -5f;
            definition.fallbackHitBoxZone = "";

            definition.Sanitize();

            Assert.That(definition.displayName, Is.Not.Empty);
            Assert.That(definition.comboStep, Is.EqualTo(1));
            Assert.That(definition.comboWindow, Is.EqualTo(0f));
            Assert.That(definition.cooldownOverride, Is.EqualTo(-1f));
            Assert.That(definition.fallbackHitBoxZone, Is.EqualTo("Punch"));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ProjectileDefinition_Sanitize_FillsLabelsAndClampsValues()
        {
            ProjectileDefinition definition = ScriptableObject.CreateInstance<ProjectileDefinition>();
            definition.name = "FallbackProjectileName";
            definition.projectileId = "";
            definition.displayName = "";
            definition.damage = -1f;
            definition.knockback = -2f;
            definition.speed = -3f;
            definition.maxDistance = -4f;
            definition.lifetime = -5f;

            definition.Sanitize();

            Assert.That(definition.projectileId, Is.EqualTo("FallbackProjectileName"));
            Assert.That(definition.displayName, Is.EqualTo("FallbackProjectileName"));
            Assert.That(definition.damage, Is.EqualTo(0f));
            Assert.That(definition.knockback, Is.EqualTo(0f));
            Assert.That(definition.speed, Is.EqualTo(0f));
            Assert.That(definition.maxDistance, Is.EqualTo(0f));
            Assert.That(definition.lifetime, Is.EqualTo(0.01f));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ProjectileDefinition_GetValidationIssues_FlagsInvalidDeliveryConfiguration()
        {
            ProjectileDefinition definition = ScriptableObject.CreateInstance<ProjectileDefinition>();
            definition.projectileId = "projectile.invalid";
            definition.displayName = "Invalid Projectile";
            definition.deliveryMode = ProjectileDeliveryMode.ProjectilePrefab;
            definition.projectilePrefab = null;
            definition.speed = 0f;

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("projectile prefab")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("speed greater than zero")), Is.True);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ProjectileDefinition_GetValidationIssues_AllowsHitscanWithoutPrefab()
        {
            ProjectileDefinition definition = ScriptableObject.CreateInstance<ProjectileDefinition>();
            definition.projectileId = "projectile.hitscan";
            definition.displayName = "Hitscan";
            definition.deliveryMode = ProjectileDeliveryMode.Hitscan;
            definition.projectilePrefab = null;
            definition.maxDistance = 20f;

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("projectile prefab")), Is.False);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ProjectileImpactDefinition_Sanitize_FillsLabelsAndClampsValues()
        {
            ProjectileImpactDefinition definition = ScriptableObject.CreateInstance<ProjectileImpactDefinition>();
            definition.name = "FallbackImpactName";
            definition.impactId = "";
            definition.displayName = "";
            definition.effectLifetime = -1f;
            definition.hitPauseDuration = -0.2f;
            definition.cameraShakeIntensity = -3f;
            definition.cameraShakeDuration = -4f;

            definition.Sanitize();

            Assert.That(definition.impactId, Is.EqualTo("FallbackImpactName"));
            Assert.That(definition.displayName, Is.EqualTo("FallbackImpactName"));
            Assert.That(definition.effectLifetime, Is.EqualTo(0f));
            Assert.That(definition.hitPauseDuration, Is.EqualTo(0f));
            Assert.That(definition.cameraShakeIntensity, Is.EqualTo(0f));
            Assert.That(definition.cameraShakeDuration, Is.EqualTo(0f));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ProjectileImpactDefinition_GetValidationIssues_FlagsEmptyIdentity()
        {
            ProjectileImpactDefinition definition = ScriptableObject.CreateInstance<ProjectileImpactDefinition>();
            definition.impactId = "";
            definition.displayName = "";

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Impact id")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Display name")), Is.True);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FireModeDefinition_Sanitize_ClampsInvalidValues()
        {
            FireModeDefinition definition = ScriptableObject.CreateInstance<FireModeDefinition>();
            definition.name = "FallbackFireModeName";
            definition.fireModeId = "";
            definition.displayName = "";
            definition.cooldown = -1f;
            definition.ammoPerShot = -2;
            definition.clipSize = -3;
            definition.reloadDuration = -4f;
            definition.burstCount = 0;
            definition.burstInterval = -5f;
            definition.projectilesPerShot = 0;
            definition.spreadAngle = -6f;

            definition.Sanitize();

            Assert.That(definition.fireModeId, Is.EqualTo("FallbackFireModeName"));
            Assert.That(definition.displayName, Is.EqualTo("FallbackFireModeName"));
            Assert.That(definition.cooldown, Is.EqualTo(0f));
            Assert.That(definition.ammoPerShot, Is.EqualTo(0));
            Assert.That(definition.clipSize, Is.EqualTo(0));
            Assert.That(definition.reloadDuration, Is.EqualTo(0f));
            Assert.That(definition.burstCount, Is.EqualTo(1));
            Assert.That(definition.burstInterval, Is.EqualTo(0f));
            Assert.That(definition.projectilesPerShot, Is.EqualTo(1));
            Assert.That(definition.spreadAngle, Is.EqualTo(0f));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FireModeDefinition_GetValidationIssues_FlagsInvalidClipConfiguration()
        {
            FireModeDefinition definition = ScriptableObject.CreateInstance<FireModeDefinition>();
            definition.fireModeId = "fire.invalid";
            definition.displayName = "Invalid Fire Mode";
            definition.clipSize = 6;
            definition.ammoPerShot = 0;
            definition.reloadDuration = 0f;

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Ammo per shot")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Reload duration")), Is.True);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void CombatSequenceDefinition_Sanitize_RemovesNullActions()
        {
            CombatSequenceDefinition sequence = ScriptableObject.CreateInstance<CombatSequenceDefinition>();
            CombatActionDefinition action = ScriptableObject.CreateInstance<CombatActionDefinition>();
            sequence.actions = new CombatActionDefinition[] { action, null };

            sequence.Sanitize();

            Assert.That(sequence.actions.Length, Is.EqualTo(1));

            Object.DestroyImmediate(action);
            Object.DestroyImmediate(sequence);
        }

        [Test]
        public void FeatureModuleDefinition_SupportsPresentationMode_WhenUnsetOrMatching()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            Assert.That(definition.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.True);

            definition.supportedPresentationModes = new[] { ActorPresentationMode.ThirdPerson3D };

            Assert.That(definition.SupportsPresentationMode(ActorPresentationMode.Sprite2D), Is.False);
            Assert.That(definition.SupportsPresentationMode(ActorPresentationMode.ThirdPerson3D), Is.True);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FeatureModuleDefinition_Sanitize_AssignsDefaultNetworkAndAuthoringMetadata()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "feature.test";
            definition.authoringCategory = string.Empty;
            definition.gizmoMode = FeatureAuthoringGizmoMode.None;
            definition.networkRole = FeatureNetworkRole.OfflineOnly;
            definition.replicationPolicyId = "policy.test";
            definition.requiresOwnership = true;
            definition.requiresAuthority = true;
            definition.requiresPrediction = true;
            definition.requiresServerExecution = true;

            definition.Sanitize();

            Assert.That(definition.authoringCategory, Is.EqualTo("General"));
            Assert.That(definition.gizmoMode, Is.EqualTo(FeatureAuthoringGizmoMode.Optional));
            Assert.That(definition.networkRole, Is.EqualTo(FeatureNetworkRole.OfflineOnly));
            Assert.That(definition.replicationPolicyId, Is.Empty);
            Assert.That(definition.requiresOwnership, Is.False);
            Assert.That(definition.requiresAuthority, Is.False);
            Assert.That(definition.requiresPrediction, Is.False);
            Assert.That(definition.requiresServerExecution, Is.False);

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FeatureModuleDefinition_GetValidationIssues_FlagsMissingNetworkMetadata()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "feature.test";
            definition.authoringCategory = "Tests";
            definition.networkRole = FeatureNetworkRole.Predicted;
            definition.runtimePrefab = new GameObject("Runtime");
            definition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("replication policy id")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("prediction support")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("require ownership")), Is.True);

            Object.DestroyImmediate(definition.runtimePrefab);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void EnemyReactionProfile_Sanitize_ClampsNegativeValues()
        {
            EnemyReactionProfile profile = ScriptableObject.CreateInstance<EnemyReactionProfile>();
            profile.hurtLockDuration = -1f;
            profile.staggerDamageThreshold = -10f;
            profile.staggerLockDuration = -0.5f;
            profile.hitPauseDuration = -0.1f;
            profile.cameraShakeIntensity = -1f;
            profile.cameraShakeDuration = -1f;

            profile.Sanitize();

            Assert.That(profile.hurtLockDuration, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.staggerDamageThreshold, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.staggerLockDuration, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.hitPauseDuration, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.cameraShakeIntensity, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.cameraShakeDuration, Is.GreaterThanOrEqualTo(0f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void InteractionFeatureProfile_Sanitize_ClampsNegativeCooldown()
        {
            InteractionFeatureProfile profile = ScriptableObject.CreateInstance<InteractionFeatureProfile>();
            profile.interactionCooldown = -3f;

            profile.Sanitize();

            Assert.That(profile.interactionCooldown, Is.GreaterThanOrEqualTo(0f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PickupFeatureProfile_Sanitize_ClampsNegativeInteractionRadius()
        {
            PickupFeatureProfile profile = ScriptableObject.CreateInstance<PickupFeatureProfile>();
            profile.interactionRadius = -2f;

            profile.Sanitize();

            Assert.That(profile.interactionRadius, Is.GreaterThanOrEqualTo(0f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void ActorStatusEffectProfile_Sanitize_ClampsShieldReduction()
        {
            ActorStatusEffectProfile profile = ScriptableObject.CreateInstance<ActorStatusEffectProfile>();
            profile.defaultShieldDamageReduction = 3f;

            profile.Sanitize();

            Assert.That(profile.defaultShieldDamageReduction, Is.LessThanOrEqualTo(1f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void HazardImpactProfile_Sanitize_ClampsValuesAndCreatesEffectId()
        {
            HazardImpactProfile profile = ScriptableObject.CreateInstance<HazardImpactProfile>();
            profile.effectId = "";
            profile.damagePerTick = -4f;
            profile.tickInterval = 0f;
            profile.knockbackForce = -2f;
            profile.statusEffects = null;

            profile.Sanitize();

            Assert.That(profile.effectId, Is.Not.Empty);
            Assert.That(profile.damagePerTick, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.tickInterval, Is.GreaterThanOrEqualTo(0.05f));
            Assert.That(profile.knockbackForce, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.statusEffects, Is.Not.Null);

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void HazardFeedbackProfile_Sanitize_ClampsPopupValuesAndFillsLabels()
        {
            HazardFeedbackProfile profile = ScriptableObject.CreateInstance<HazardFeedbackProfile>();
            profile.activationPopupText = "";
            profile.explosionPopupText = "";
            profile.collectiblePopupPrefix = "";
            profile.exitPopupText = "";
            profile.popupLifetime = 0f;
            profile.popupRiseSpeed = -1f;
            profile.popupFontSize = 0f;

            profile.Sanitize();

            Assert.That(profile.activationPopupText, Is.Not.Empty);
            Assert.That(profile.explosionPopupText, Is.Not.Empty);
            Assert.That(profile.collectiblePopupPrefix, Is.Not.Empty);
            Assert.That(profile.exitPopupText, Is.Not.Empty);
            Assert.That(profile.popupLifetime, Is.GreaterThanOrEqualTo(0.05f));
            Assert.That(profile.popupRiseSpeed, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.popupFontSize, Is.GreaterThan(0f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PyralisFeatureModuleContractValidator_FlagsFeedbackProfileMismatch()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.feedback";
            definition.profileAsset = ScriptableObject.CreateInstance<PickupFeatureProfile>();

            System.Collections.Generic.List<string> issues = PyralisFeatureModuleContractValidator.GetValidationIssues(definition);

            Assert.That(issues.Exists(issue => issue.Contains("ActorFeedbackProfile")), Is.True);

            Object.DestroyImmediate(definition.profileAsset);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FeatureModuleDefinition_GetValidationIssues_ValidatesTopDownHopRoute()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.traversal.topdown-hop";
            definition.authoringCategory = "Traversal";
            definition.profileAsset = ScriptableObject.CreateInstance<TopDownHopProfile>();
            definition.supportedPresentationModes = new[] { ActorPresentationMode.Sprite2D, ActorPresentationMode.Billboard2_5D };
            definition.runtimePrefab = new GameObject("TopDownHopRuntime");
            definition.runtimePrefab.AddComponent<TestGameplayActionFeatureRuntime>();

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();
            issues.AddRange(PyralisFeatureModuleContractValidator.GetValidationIssues(definition));

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(definition.runtimePrefab);
            Object.DestroyImmediate(definition.profileAsset);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FeatureModuleDefinition_GetValidationIssues_FlagsTopDownHopMismatches()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.traversal.topdown-hop";
            definition.authoringCategory = "Traversal";
            definition.profileAsset = ScriptableObject.CreateInstance<PickupFeatureProfile>();
            definition.supportedPresentationModes = new[] { ActorPresentationMode.ThirdPerson3D };
            definition.runtimePrefab = new GameObject("TopDownHopRuntime");
            definition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            System.Collections.Generic.List<string> issues = PyralisFeatureModuleContractValidator.GetValidationIssues(definition);

            Assert.That(issues.Exists(issue => issue.Contains("TopDownHopProfile")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Rigged3D actors should use the 3D traversal jump path")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("IActorGameplayActionReceiver")), Is.True);

            Object.DestroyImmediate(definition.runtimePrefab);
            Object.DestroyImmediate(definition.profileAsset);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FeatureModuleDefinition_GetActorCompatibilityIssues_FlagsMissingFeedbackReceiver()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.feedback";

            GameObject actor = new GameObject("Actor");
            actor.AddComponent<HealthComponent>();

            System.Collections.Generic.List<string> issues = definition.GetActorCompatibilityIssues(actor, ActorPresentationMode.Sprite2D);

            Assert.That(issues.Exists(issue => issue.Contains("IActorFeedbackReceiver")), Is.True);

            Object.DestroyImmediate(actor);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void StatusEffectDefinition_Sanitize_ClampsDurationsAndMagnitude()
        {
            StatusEffectDefinition definition = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            definition.effectId = "";
            definition.displayName = "";
            definition.duration = -1f;
            definition.magnitude = -2f;
            definition.tickInterval = 0f;
            definition.maxStacks = 0;

            definition.Sanitize();

            Assert.That(definition.effectId, Is.Not.Empty);
            Assert.That(definition.displayName, Is.Not.Empty);
            Assert.That(definition.duration, Is.GreaterThanOrEqualTo(0f));
            Assert.That(definition.magnitude, Is.GreaterThanOrEqualTo(0f));
            Assert.That(definition.tickInterval, Is.GreaterThanOrEqualTo(0.05f));
            Assert.That(definition.maxStacks, Is.GreaterThanOrEqualTo(1));

            Object.DestroyImmediate(definition);
        }

        [Test]
        public void ActorCombatReactionProfile_Sanitize_ClampsInvalidValues()
        {
            ActorCombatReactionProfile profile = ScriptableObject.CreateInstance<ActorCombatReactionProfile>();
            profile.blockDamageReduction = 2f;
            profile.blockFrontalAngle = 0f;
            profile.hurtLockDuration = -1f;
            profile.staggerDamageThreshold = -2f;
            profile.staggerLockDuration = -3f;

            profile.Sanitize();

            Assert.That(profile.blockDamageReduction, Is.LessThanOrEqualTo(1f));
            Assert.That(profile.blockFrontalAngle, Is.GreaterThanOrEqualTo(10f));
            Assert.That(profile.hurtLockDuration, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.staggerDamageThreshold, Is.GreaterThanOrEqualTo(0f));
            Assert.That(profile.staggerLockDuration, Is.GreaterThanOrEqualTo(0f));

            Object.DestroyImmediate(profile);
        }

        [Test]
        public void PyralisFeatureModuleContractValidator_FlagsEnemyReactionProfileMismatch()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "enemy.reaction";
            definition.profileAsset = ScriptableObject.CreateInstance<InteractionFeatureProfile>();

            System.Collections.Generic.List<string> issues = PyralisFeatureModuleContractValidator.GetValidationIssues(definition);

            Assert.That(issues.Exists(issue => issue.Contains("EnemyReactionProfile")), Is.True);

            Object.DestroyImmediate(definition.profileAsset);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PyralisFeatureModuleContractValidator_FlagsCombatReactionProfileMismatch()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.combat.reaction";
            definition.profileAsset = ScriptableObject.CreateInstance<PickupFeatureProfile>();

            System.Collections.Generic.List<string> issues = PyralisFeatureModuleContractValidator.GetValidationIssues(definition);

            Assert.That(issues.Exists(issue => issue.Contains("ActorCombatReactionProfile")), Is.True);

            Object.DestroyImmediate(definition.profileAsset);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PyralisFeatureModuleContractValidator_FlagsStatusProfileMismatch()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.status";
            definition.profileAsset = ScriptableObject.CreateInstance<PickupFeatureProfile>();

            System.Collections.Generic.List<string> issues = PyralisFeatureModuleContractValidator.GetValidationIssues(definition);

            Assert.That(issues.Exists(issue => issue.Contains("ActorStatusEffectProfile")), Is.True);

            Object.DestroyImmediate(definition.profileAsset);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FeatureModuleDefinition_GetValidationIssues_FlagsMutedFloatingFeedbackReceiver()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.feedback";
            definition.runtimePrefab = new GameObject("FeedbackRuntime");
            definition.runtimePrefab.AddComponent<ActorFeedbackFeatureRuntime>();
            ActorFloatingFeedbackReceiver receiver = definition.runtimePrefab.AddComponent<ActorFloatingFeedbackReceiver>();

            SetPrivateField(receiver, "showDamageNumbers", false);
            SetPrivateField(receiver, "showHealNumbers", false);
            SetPrivateField(receiver, "showScorePopups", false);
            SetPrivateField(receiver, "showComboPopups", false);
            SetPrivateField(receiver, "showStatusPopups", false);
            SetPrivateField(receiver, "showCombatAlertPopups", false);

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("hide every feedback category")), Is.True);

            Object.DestroyImmediate(definition.runtimePrefab);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PawnDefinition_GetValidationIssues_FlagsDuplicateAndIncompatibleFeatureModules()
        {
            PawnDefinition definition = ScriptableObject.CreateInstance<PawnDefinition>();
            PawnPresentationProfile presentation = ScriptableObject.CreateInstance<PawnPresentationProfile>();
            presentation.presentationMode = ActorPresentationMode.Sprite2D;

            FeatureModuleDefinition first = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            first.moduleId = "actor.pickups.2d";
            first.supportedPresentationModes = new[] { ActorPresentationMode.ThirdPerson3D };

            FeatureModuleDefinition second = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            second.moduleId = "actor.pickups.2d";

            definition.presentationProfile = presentation;
            definition.featureModules = new[] { first, second };

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("assigned more than once")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("does not support")), Is.True);

            Object.DestroyImmediate(second);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(presentation);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void PawnDefinition_GetValidationIssues_AllowsOptionalProfilesWhenPrefabExists()
        {
            PawnDefinition definition = ScriptableObject.CreateInstance<PawnDefinition>();
            definition.pawnPrefab = new GameObject("MinimalPawn");

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(definition.pawnPrefab);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void GameModeDefinition_GetValidationIssues_FlagsDuplicateRequiredFeatureModules()
        {
            GameModeDefinition definition = ScriptableObject.CreateInstance<GameModeDefinition>();

            FeatureModuleDefinition first = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            first.moduleId = "enemy.reaction";

            FeatureModuleDefinition second = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            second.moduleId = "enemy.reaction";

            definition.requiredFeatureModules = new[] { first, second };

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("assigned more than once")), Is.True);

            Object.DestroyImmediate(second);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void GameModeDefinition_GetValidationIssues_AllowsOptionalPlayfieldAndCameraProfiles()
        {
            GameModeDefinition definition = ScriptableObject.CreateInstance<GameModeDefinition>();
            RuntimePatternDefinition pattern = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Tabletop Setup";
            setupProfile.runtimePatterns = new[] { pattern };
            definition.setupProfile = setupProfile;

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(pattern);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void SessionDefinition_GetValidationIssues_AllowsMissingDefaultInputForNoPawnTabletop()
        {
            RuntimePatternDefinition tabletop = CreateRuntimePattern(
                "pattern.tabletop",
                "Board Card Tabletop",
                RuntimeCapabilityFamily.BoardCardTabletop,
                ParticipantEmbodimentRequirement.NonPawnSurfaceRequired,
                RuntimeControlSurface.BoardSeat,
                RuntimeControlSurface.CardHand);

            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Tabletop Setup";
            setupProfile.runtimePatterns = new[] { tabletop };

            GameModeDefinition mode = ScriptableObject.CreateInstance<GameModeDefinition>();
            mode.setupProfile = setupProfile;

            ParticipantDefinition participant = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participant.displayName = "Seat 1";

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.defaultGameMode = mode;
            session.defaultParticipants = new[] { participant };
            session.defaultInputProfile = null;

            System.Collections.Generic.List<string> issues = session.GetValidationIssues();

            Assert.That(issues.Any(issue => issue.Contains("input profile")), Is.False);

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(participant);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(tabletop);
        }

        [Test]
        public void GameModeDefinition_GetValidationIssues_IncludesSetupProfileIssues()
        {
            GameModeDefinition definition = ScriptableObject.CreateInstance<GameModeDefinition>();
            GameSetupProfile setupProfile = ScriptableObject.CreateInstance<GameSetupProfile>();
            setupProfile.setupName = "Invalid Setup";
            setupProfile.runtimePatterns = System.Array.Empty<RuntimePatternDefinition>();

            definition.setupProfile = setupProfile;

            System.Collections.Generic.List<string> issues = definition.GetValidationIssues();

            Assert.That(issues.Exists(issue => issue.Contains("Setup profile")), Is.True);
            Assert.That(issues.Exists(issue => issue.Contains("Choose a Runtime Capability family")), Is.True);

            Object.DestroyImmediate(setupProfile);
            Object.DestroyImmediate(definition);
        }

        [Test]
        public void FeatureModuleDefinition_GetActorCompatibilityIssues_FlagsHudPresenterWithoutPresentationSurface()
        {
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "actor.feedback";

            GameObject actor = new GameObject("Actor");
            actor.AddComponent<HealthComponent>();
            actor.AddComponent<ParticipantFeedbackHudPresenter>();

            System.Collections.Generic.List<string> issues = definition.GetActorCompatibilityIssues(actor, ActorPresentationMode.Sprite2D);

            Assert.That(issues.Exists(issue => issue.Contains("ParticipantFeedbackHudPresenter")), Is.True);

            Object.DestroyImmediate(actor);
            Object.DestroyImmediate(definition);
        }

        private sealed class TestGameplayActionFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime, IActorGameplayActionReceiver
        {
            public string ModuleId => "actor.traversal.topdown-hop";

            public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
            {
            }

            public void ShutdownFeature()
            {
            }

            public bool TryHandleGameplayAction(string actionKey)
            {
                return actionKey == "Jump";
            }
        }
    }
}
