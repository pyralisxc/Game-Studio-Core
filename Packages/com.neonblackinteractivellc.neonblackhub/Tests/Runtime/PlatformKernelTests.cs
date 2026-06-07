using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Core.Actions;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Characters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class PlatformKernelTests
    {
        private sealed class TestFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime
        {
            public FeatureRuntimeInitializationContext InitializationContext { get; private set; }
            public string ModuleId => "feature.runtime.test";

            public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
            {
                InitializationContext = initializationContext;
            }

            public void ShutdownFeature()
            {
            }
        }

        private sealed class InjectedSceneComponent : MonoBehaviour
        {
            public ParticipantRosterService InjectedRoster { get; private set; }

            [Inject]
            private void Construct(ParticipantRosterService roster)
            {
                InjectedRoster = roster;
            }
        }

        private sealed class InjectedScoreConsumer : MonoBehaviour
        {
            public ISessionScoreService InjectedScoreService { get; private set; }

            [Inject]
            private void Construct(ISessionScoreService scoreService)
            {
                InjectedScoreService = scoreService;
            }
        }

        private sealed class TestSessionScoreService : ISessionScoreService
        {
            public int PointsCollected => 0;
            public float SurvivalTime => 0f;
            public int HighScorePoints => 0;
            public float HighScoreTime => 0f;
            public float HighScoreBestTime => 0f;

            public void AddPoints(int amount = 1)
            {
            }

            public void AddPointsChangedListener(UnityAction<int> listener)
            {
            }

            public void RemovePointsChangedListener(UnityAction<int> listener)
            {
            }
        }

        [Test]
        public void PlatformServiceRegistry_RegistersAndResolvesServiceContracts()
        {
            PlatformServiceRegistry registry = new PlatformServiceRegistry();
            ParticipantRosterService roster = new GameObject("Roster").AddComponent<ParticipantRosterService>();

            registry.Register<IParticipantRoster>(roster);

            Assert.That(registry.TryResolve(out IParticipantRoster resolved), Is.True);
            Assert.That(resolved, Is.SameAs(roster));

            Object.DestroyImmediate(roster.gameObject);
        }

        [Test]
        public void GameplayPlatformContext_TryResolve_CentralizesActiveServiceLookup()
        {
            GameplayPlatformContext context = GameplayPlatformContext.CreateOrReplace();
            ParticipantRosterService roster = new GameObject("Roster").AddComponent<ParticipantRosterService>();
            context.Services.Register<IParticipantRoster>(roster);

            bool resolved = GameplayPlatformContext.TryResolve(out IParticipantRoster service);

            Assert.That(resolved, Is.True);
            Assert.That(service, Is.SameAs(roster));

            Object.DestroyImmediate(roster.gameObject);
            GameplayPlatformContext.ClearCurrent();
        }

        [Test]
        public void ActionTargetRule_SelfTarget_ValidatesWithoutPawnRoot()
        {
            GameObject source = new GameObject("SourceActor");
            ActionTargetRule rule = ActionTargetRule.Single(ActionTargetKind.Self);
            ActionExecutionContext context = new ActionExecutionContext(
                "action.guard",
                source,
                source,
                sourceFaction: Faction.Player,
                targets: new[] { ActionTargetDescriptor.Self(source, Faction.Player) });

            ActionValidationResult result = rule.ValidateTargets(context);

            Assert.That(result.IsValid, Is.True);

            Object.DestroyImmediate(source);
        }

        [Test]
        public void ActionTargetRule_RejectsWrongTargetKind()
        {
            ActionTargetRule rule = ActionTargetRule.Single(ActionTargetKind.Actor);
            ActionExecutionContext context = new ActionExecutionContext(
                "action.fire",
                targets: new[] { ActionTargetDescriptor.WorldPoint(Vector3.one) });

            ActionValidationResult result = rule.ValidateTargets(context);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Message, Does.Contain("Actor"));
        }

        [Test]
        public void ActionTargetRule_NoTargetAction_ValidatesForCameraOrMenuControl()
        {
            ActionTargetRule rule = ActionTargetRule.None();
            ActionExecutionContext context = new ActionExecutionContext(
                "action.end-turn",
                participant: "SeatA",
                sourceFaction: Faction.Neutral);

            ActionValidationResult result = rule.ValidateTargets(context);

            Assert.That(result.IsValid, Is.True);
        }

        [Test]
        public void ProjectileFirePlanner_ReturnsEmptyCommandsWithoutProjectileDefinition()
        {
            ProjectileFireRequest request = new ProjectileFireRequest(
                null,
                null,
                Vector3.zero,
                Vector3.forward);

            ProjectileSpawnCommand[] commands = ProjectileFirePlanner.BuildCommands(request);

            Assert.That(commands, Is.Empty);
        }

        [Test]
        public void ProjectileFirePlanner_BuildCommands_CreatesBurstAndSpreadCommands()
        {
            ProjectileDefinition projectile = ScriptableObject.CreateInstance<ProjectileDefinition>();
            projectile.deliveryMode = ProjectileDeliveryMode.Hitscan;
            projectile.damage = 12f;
            projectile.knockback = 4f;
            projectile.speed = 45f;
            projectile.maxDistance = 60f;
            projectile.lifetime = 2f;
            projectile.allowFriendlyFire = true;

            FireModeDefinition fireMode = ScriptableObject.CreateInstance<FireModeDefinition>();
            fireMode.burstCount = 2;
            fireMode.burstInterval = 0.15f;
            fireMode.projectilesPerShot = 3;
            fireMode.spreadAngle = 30f;

            ProjectileFireRequest request = new ProjectileFireRequest(
                projectile,
                fireMode,
                Vector3.one,
                Vector3.forward,
                sourceFaction: Faction.Player);

            ProjectileSpawnCommand[] commands = ProjectileFirePlanner.BuildCommands(request);

            Assert.That(commands.Length, Is.EqualTo(6));
            Assert.That(commands[0].Damage, Is.EqualTo(12f));
            Assert.That(commands[0].Knockback, Is.EqualTo(4f));
            Assert.That(commands[0].DeliveryMode, Is.EqualTo(ProjectileDeliveryMode.Hitscan));
            Assert.That(commands[0].AllowFriendlyFire, Is.True);
            Assert.That(commands[0].Delay, Is.EqualTo(0f));
            Assert.That(commands[3].Delay, Is.EqualTo(0.15f));
            Assert.That(Vector3.Angle(commands[0].Direction, Quaternion.AngleAxis(-15f, Vector3.up) * Vector3.forward), Is.LessThan(0.01f));
            Assert.That(Vector3.Angle(commands[1].Direction, Vector3.forward), Is.LessThan(0.01f));
            Assert.That(Vector3.Angle(commands[2].Direction, Quaternion.AngleAxis(15f, Vector3.up) * Vector3.forward), Is.LessThan(0.01f));

            Object.DestroyImmediate(fireMode);
            Object.DestroyImmediate(projectile);
        }

        [Test]
        public void ProjectileFirePlanner_UsesActionDirectionTargetWhenPresent()
        {
            ProjectileDefinition projectile = ScriptableObject.CreateInstance<ProjectileDefinition>();
            projectile.deliveryMode = ProjectileDeliveryMode.Hitscan;

            ActionExecutionContext context = new ActionExecutionContext(
                "action.fire",
                targets: new[] { ActionTargetDescriptor.Direction(Vector3.right) });

            ProjectileFireRequest request = new ProjectileFireRequest(
                projectile,
                null,
                Vector3.zero,
                Vector3.forward,
                actionContext: context);

            ProjectileSpawnCommand[] commands = ProjectileFirePlanner.BuildCommands(request);

            Assert.That(commands.Length, Is.EqualTo(1));
            Assert.That(Vector3.Angle(commands[0].Direction, Vector3.right), Is.LessThan(0.01f));

            Object.DestroyImmediate(projectile);
        }

        [Test]
        public void ProjectileFirePlanner_AppliesRequestDamageAndKnockbackMultipliers()
        {
            ProjectileDefinition projectile = ScriptableObject.CreateInstance<ProjectileDefinition>();
            projectile.deliveryMode = ProjectileDeliveryMode.Hitscan;
            projectile.damage = 12f;
            projectile.knockback = 4f;

            ProjectileFireRequest request = new ProjectileFireRequest(
                projectile,
                null,
                Vector3.zero,
                Vector3.forward,
                damageMultiplier: 2f,
                knockbackMultiplier: 0.5f);

            ProjectileSpawnCommand[] commands = ProjectileFirePlanner.BuildCommands(request);

            Assert.That(commands.Length, Is.EqualTo(1));
            Assert.That(commands[0].Damage, Is.EqualTo(24f));
            Assert.That(commands[0].Knockback, Is.EqualTo(2f));

            Object.DestroyImmediate(projectile);
        }

        [Test]
        public void BrawlerMovementModel_TraversalCapabilityFlags_BlockJumpDodgeAndCrouch()
        {
            BrawlerMovementModel model = new BrawlerMovementModel();
            model.Configure(new MovementConfig
            {
                AllowJump = false,
                AllowDodge = false,
                AllowCrouch = false,
                MovementMode = Core.Enums.MovementMode.ThreeD,
                Gravity = -20f,
                JumpHeight = 3f,
                MaxJumps = 2,
                CoyoteTime = 0.1f,
                JumpBufferTime = 0.1f,
                JumpCutMultiplier = 0.4f,
                WalkSpeed = 5f,
                SprintSpeed = 8f,
                CrouchSpeed = 2f,
                AccelerationTime = 0.1f,
                DecelerationTime = 0.1f,
                DepthSpeedMultiplier = 1f,
                DodgeDistance = 3f,
                DodgeDuration = 0.4f,
                DodgeCooldown = 0.8f,
                RollCooldown = 1f,
                PowerSlideDistance = 4f,
                PowerSlideDuration = 0.4f,
                PowerSlideCooldown = 1f,
                LandSlowMultiplier = 1f
            });

            MovementPhysicsFrame grounded = MovementPhysicsFrame.Default;
            grounded.GroundedByCollision = true;
            model.Tick(new MovementInput(), grounded, 0.016f);

            Vector3 velocity = model.Tick(new MovementInput { JumpPressed = true }, grounded, 0.016f);
            bool dodgeStarted = model.TryStartDodge(Vector2.right);
            model.SetCrouching(true);

            Assert.That(model.State.TriggerJump, Is.False);
            Assert.That(velocity.y, Is.LessThanOrEqualTo(0f));
            Assert.That(dodgeStarted, Is.False);
            Assert.That(model.State.IsCrouching, Is.False);
        }

        [Test]
        public void BrawlerMovementModel_UsesResolvedWorldPlanarMove()
        {
            BrawlerMovementModel model = new BrawlerMovementModel();
            model.Configure(new MovementConfig
            {
                AllowJump = true,
                AllowDodge = true,
                AllowCrouch = true,
                AllowPowerSlide = true,
                MovementMode = Core.Enums.MovementMode.ThreeD,
                Gravity = -20f,
                WalkSpeed = 10f,
                SprintSpeed = 10f,
                CrouchSpeed = 5f,
                AccelerationTime = 0.1f,
                DecelerationTime = 0.1f,
                DepthSpeedMultiplier = 1f,
                JumpCutMultiplier = 0.4f,
                LandSlowMultiplier = 1f
            });

            MovementPhysicsFrame grounded = MovementPhysicsFrame.Default;
            grounded.GroundedByCollision = true;
            Vector3 velocity = model.Tick(new MovementInput
            {
                Move = Vector2.right,
                MoveWorld = Vector3.forward,
                CameraRight = Vector3.forward
            }, grounded, 0.1f);

            Assert.That(Mathf.Abs(velocity.x), Is.LessThan(0.001f));
            Assert.That(velocity.z, Is.GreaterThan(9.9f));
            Assert.That(model.State.FacingRight, Is.True);
        }

        [Test]
        public void HealthComponent_TryTakeDamage_ReturnsFalseWhenIFramesBlockHit()
        {
            GameObject target = new GameObject("Target");
            HealthComponent health = target.AddComponent<HealthComponent>();

            bool firstHit = health.TryTakeDamage(10f, Vector3.zero);
            bool secondHit = health.TryTakeDamage(10f, Vector3.zero);

            Assert.That(firstHit, Is.True);
            Assert.That(secondHit, Is.False);
            Assert.That(health.CurrentHealth, Is.EqualTo(90f));

            Object.DestroyImmediate(target);
        }

        [Test]
        public void HealthComponent_TryTakeDamage_ReturnsFalseForFriendlyFire()
        {
            GameObject attacker = new GameObject("Attacker");
            HealthComponent attackerHealth = attacker.AddComponent<HealthComponent>();
            attackerHealth.faction = Faction.Player;

            GameObject target = new GameObject("Target");
            HealthComponent targetHealth = target.AddComponent<HealthComponent>();
            targetHealth.faction = Faction.Player;

            bool damaged = targetHealth.TryTakeDamage(10f, Vector3.zero, attacker);

            Assert.That(damaged, Is.False);
            Assert.That(targetHealth.CurrentHealth, Is.EqualTo(100f));

            Object.DestroyImmediate(target);
            Object.DestroyImmediate(attacker);
        }

        [Test]
        public void ProjectileMagazineState_ConsumesReloadsAndTracksReserveAmmo()
        {
            FireModeDefinition fireMode = ScriptableObject.CreateInstance<FireModeDefinition>();
            fireMode.clipSize = 3;
            fireMode.ammoPerShot = 1;
            fireMode.reloadDuration = 0.5f;

            ProjectileMagazineState state = new ProjectileMagazineState(fireMode, reserveAmmo: 5);

            Assert.That(state.CurrentClipAmmo, Is.EqualTo(3));
            Assert.That(state.ReserveAmmo, Is.EqualTo(5));
            Assert.That(state.TryConsumeShot(), Is.True);
            Assert.That(state.TryConsumeShot(), Is.True);
            Assert.That(state.TryConsumeShot(), Is.True);
            Assert.That(state.TryConsumeShot(), Is.False);
            Assert.That(state.CurrentClipAmmo, Is.EqualTo(0));

            Assert.That(state.TryReload(), Is.True);
            Assert.That(state.CurrentClipAmmo, Is.EqualTo(3));
            Assert.That(state.ReserveAmmo, Is.EqualTo(2));

            Object.DestroyImmediate(fireMode);
        }

        [Test]
        public void ProjectileMagazineState_AllowsUnlimitedFireWhenClipIsUnset()
        {
            FireModeDefinition fireMode = ScriptableObject.CreateInstance<FireModeDefinition>();
            fireMode.clipSize = 0;
            fireMode.ammoPerShot = 0;

            ProjectileMagazineState state = new ProjectileMagazineState(fireMode);

            Assert.That(state.IsUnlimited, Is.True);
            Assert.That(state.TryConsumeShot(), Is.True);
            Assert.That(state.TryConsumeShot(), Is.True);
            Assert.That(state.CurrentClipAmmo, Is.EqualTo(0));

            Object.DestroyImmediate(fireMode);
        }

        [Test]
        public void ProjectileLauncher3D_ExecuteHitscan_DamagesEnemyTarget()
        {
            GameObject launcherObject = new GameObject("Launcher3D");
            ProjectileLauncher3D launcher = launcherObject.AddComponent<ProjectileLauncher3D>();

            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Target3D";
            target.transform.position = Vector3.forward * 4f;
            HealthComponent health = target.AddComponent<HealthComponent>();
            health.faction = Faction.Enemy;
            Physics.SyncTransforms();

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.Hitscan,
                null,
                Vector3.zero,
                Vector3.forward,
                25f,
                0f,
                0f,
                10f,
                1f,
                Faction.Player,
                launcherObject,
                0f,
                false);

            ProjectileSpawnResult result = launcher.Execute(command);

            Assert.That(result.Status, Is.EqualTo(ProjectileSpawnStatus.Hit));
            Assert.That(result.HitObject, Is.SameAs(target));
            Assert.That(health.CurrentHealth, Is.EqualTo(75f));

            Object.DestroyImmediate(target);
            Object.DestroyImmediate(launcherObject);
        }

        [Test]
        public void ProjectileLauncher3D_ExecuteHitscan_SpawnsImpactEffectFromCommand()
        {
            GameObject launcherObject = new GameObject("Launcher3D");
            ProjectileLauncher3D launcher = launcherObject.AddComponent<ProjectileLauncher3D>();

            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Target3D";
            target.transform.position = Vector3.forward * 4f;
            HealthComponent health = target.AddComponent<HealthComponent>();
            health.faction = Faction.Enemy;

            GameObject hitEffectPrefab = new GameObject("HitEffectPrefab");
            ProjectileImpactDefinition impactDefinition = ScriptableObject.CreateInstance<ProjectileImpactDefinition>();
            impactDefinition.hitEffectPrefab = hitEffectPrefab;
            impactDefinition.effectLifetime = 0f;
            Physics.SyncTransforms();

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.Hitscan,
                null,
                Vector3.zero,
                Vector3.forward,
                25f,
                0f,
                0f,
                10f,
                1f,
                Faction.Player,
                launcherObject,
                0f,
                false,
                impactDefinition);

            ProjectileSpawnResult result = launcher.Execute(command);

            Assert.That(result.Status, Is.EqualTo(ProjectileSpawnStatus.Hit));
            Assert.That(result.ImpactEffectObject, Is.Not.Null);
            Assert.That(result.ImpactEffectObject.name, Does.StartWith("HitEffectPrefab"));

            Object.DestroyImmediate(result.ImpactEffectObject);
            Object.DestroyImmediate(hitEffectPrefab);
            Object.DestroyImmediate(impactDefinition);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(launcherObject);
        }

        [Test]
        public void ProjectileLauncher2D_ExecuteHitscan_DamagesEnemyTarget()
        {
            GameObject launcherObject = new GameObject("Launcher2D");
            ProjectileLauncher2D launcher = launcherObject.AddComponent<ProjectileLauncher2D>();

            GameObject target = new GameObject("Target2D");
            target.transform.position = Vector3.right * 4f;
            BoxCollider2D collider = target.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            HealthComponent health = target.AddComponent<HealthComponent>();
            health.faction = Faction.Enemy;
            Physics2D.SyncTransforms();

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.Hitscan,
                null,
                Vector3.zero,
                Vector3.right,
                30f,
                0f,
                0f,
                10f,
                1f,
                Faction.Player,
                launcherObject,
                0f,
                false);

            ProjectileSpawnResult result = launcher.Execute(command);

            Assert.That(result.Status, Is.EqualTo(ProjectileSpawnStatus.Hit));
            Assert.That(result.HitObject, Is.SameAs(target));
            Assert.That(health.CurrentHealth, Is.EqualTo(70f));

            Object.DestroyImmediate(target);
            Object.DestroyImmediate(launcherObject);
        }

        [Test]
        public void ProjectileLauncher3D_ExecuteProjectilePrefab_SpawnsAndLaunchesRigidbody()
        {
            GameObject launcherObject = new GameObject("Launcher3D");
            ProjectileLauncher3D launcher = launcherObject.AddComponent<ProjectileLauncher3D>();

            GameObject prefab = new GameObject("ProjectilePrefab");
            Rigidbody body = prefab.AddComponent<Rigidbody>();
            body.useGravity = false;

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.one,
                Vector3.forward,
                5f,
                0f,
                12f,
                0f,
                0f,
                Faction.Player,
                launcherObject,
                0f,
                false);

            ProjectileSpawnResult result = launcher.Execute(command);

            Assert.That(result.Status, Is.EqualTo(ProjectileSpawnStatus.Spawned));
            Assert.That(result.SpawnedObject, Is.Not.Null);
            Assert.That(result.SpawnedObject.transform.position, Is.EqualTo(Vector3.one));
            Assert.That(result.SpawnedObject.GetComponent<Rigidbody>().linearVelocity, Is.EqualTo(Vector3.forward * 12f));

            Object.DestroyImmediate(result.SpawnedObject);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(launcherObject);
        }

        [Test]
        public void ProjectileLauncher2D_ExecuteProjectilePrefab_InitializesRuntimeProjectileBody()
        {
            GameObject launcherObject = new GameObject("Launcher2D");
            ProjectileLauncher2D launcher = launcherObject.AddComponent<ProjectileLauncher2D>();

            GameObject prefab = new GameObject("Projectile2DPrefab");
            Rigidbody2D body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            BoxCollider2D collider = prefab.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            prefab.AddComponent<Projectile2D>();

            ProjectileSpawnCommand command = new ProjectileSpawnCommand(
                ProjectileDeliveryMode.ProjectilePrefab,
                prefab,
                Vector3.one,
                Vector3.right,
                5f,
                0f,
                9f,
                20f,
                2f,
                Faction.Player,
                launcherObject,
                0f,
                true);

            ProjectileSpawnResult result = launcher.Execute(command);

            Assert.That(result.Status, Is.EqualTo(ProjectileSpawnStatus.Spawned));
            Assert.That(result.SpawnedObject, Is.Not.Null);
            Assert.That(result.SpawnedObject.GetComponent<Projectile2D>(), Is.Not.Null);
            Assert.That(result.SpawnedObject.GetComponent<Rigidbody2D>().linearVelocity, Is.EqualTo(Vector2.right * 9f));

            Object.DestroyImmediate(result.SpawnedObject);
            Object.DestroyImmediate(prefab);
            Object.DestroyImmediate(launcherObject);
        }

        [Test]
        public void ActorFeatureHost_InitializeFeatures_PassesRegistryAndDefinitionToRuntime()
        {
            GameObject actor = new GameObject("Actor");
            ActorFeatureHost host = actor.AddComponent<ActorFeatureHost>();

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "feature.runtime.test";
            definition.authoringCategory = "Tests";
            definition.gizmoMode = FeatureAuthoringGizmoMode.Optional;
            definition.runtimePrefab = new GameObject("RuntimePrefab");
            definition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            PlatformServiceRegistry registry = new PlatformServiceRegistry();
            registry.Register<ActorFeatureHost>(host);
            ActorFeatureContext context = new ActorFeatureContext(actor);

            host.InitializeFeatures(new FeatureHostInitializationContext(context, registry), new[] { definition });

            Assert.That(host.TryGetInstalledFeature(out TestFeatureRuntime runtime), Is.True);
            Assert.That(runtime.InitializationContext, Is.Not.Null);
            Assert.That(runtime.InitializationContext.Definition, Is.SameAs(definition));
            Assert.That(runtime.InitializationContext.Services, Is.SameAs(registry));

            Object.DestroyImmediate(definition.runtimePrefab);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorFeatureHost_ContextOverload_DoesNotCreateAnonymousServiceRegistry()
        {
            GameplayPlatformContext.ClearCurrent();
            GameObject actor = new GameObject("Actor");
            ActorFeatureHost host = actor.AddComponent<ActorFeatureHost>();

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "feature.runtime.test";
            definition.authoringCategory = "Tests";
            definition.gizmoMode = FeatureAuthoringGizmoMode.Optional;
            definition.runtimePrefab = new GameObject("RuntimePrefab");
            definition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            host.InitializeFeatures(new ActorFeatureContext(actor), new[] { definition });

            Assert.That(host.TryGetInstalledFeature(out TestFeatureRuntime runtime), Is.True);
            Assert.That(runtime.InitializationContext, Is.Not.Null);
            Assert.That(runtime.InitializationContext.Services, Is.Null,
                "Feature hosts should leave missing platform composition visible instead of manufacturing a throwaway registry.");

            Object.DestroyImmediate(definition.runtimePrefab);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void FeatureRuntimeInitializationContext_ExposesPawnDefinitionWithoutPawnRoot()
        {
            GameObject actor = new GameObject("Actor");
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            PawnDefinition pawnDefinition = ScriptableObject.CreateInstance<PawnDefinition>();

            FeatureRuntimeInitializationContext initializationContext = new FeatureRuntimeInitializationContext(
                new ActorFeatureContext(actor, pawnDefinition: pawnDefinition),
                definition,
                new PlatformServiceRegistry());

            Assert.That(initializationContext.PawnDefinition, Is.SameAs(pawnDefinition));

            Object.DestroyImmediate(pawnDefinition);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void LifetimeScope_Build_InjectsSceneComponentsAndExportsResolver()
        {
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace();
            GameObject scopeObject = new GameObject("Scope");
            PyralisGameplayLifetimeScope scope = scopeObject.AddComponent<PyralisGameplayLifetimeScope>();

            GameObject rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();

            GameObject consumerObject = new GameObject("Consumer");
            InjectedSceneComponent consumer = consumerObject.AddComponent<InjectedSceneComponent>();

            scope.ConfigureRuntime(
                platformContext,
                null,
                null,
                roster,
                null,
                null,
                null,
                null,
                null,
                null,
                new LocalSessionOwnershipService(),
                new LocalParticipantAuthorityService());

            scope.Build();

            Assert.That(platformContext.Services.TryResolve(out IObjectResolver resolver), Is.True);
            Assert.That(resolver, Is.Not.Null);
            Assert.That(consumer.InjectedRoster, Is.SameAs(roster));

            GameplayPlatformContext.ClearCurrent();
            Object.DestroyImmediate(consumerObject);
            Object.DestroyImmediate(rosterObject);
            Object.DestroyImmediate(scopeObject);
        }

        [Test]
        public void LifetimeScope_Build_RegistersPlatformRegistryServicesWithContainer()
        {
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace();
            GameObject scopeObject = new GameObject("Scope");
            PyralisGameplayLifetimeScope scope = scopeObject.AddComponent<PyralisGameplayLifetimeScope>();

            TestSessionScoreService scoreService = new TestSessionScoreService();
            platformContext.Services.Register<ISessionScoreService>(scoreService);

            GameObject consumerObject = new GameObject("ScoreConsumer");
            InjectedScoreConsumer consumer = consumerObject.AddComponent<InjectedScoreConsumer>();

            scope.ConfigureRuntime(
                platformContext,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new LocalSessionOwnershipService(),
                new LocalParticipantAuthorityService());

            scope.Build();

            Assert.That(consumer.InjectedScoreService, Is.SameAs(scoreService));
            Assert.That(platformContext.Services.TryResolve(out IObjectResolver resolver), Is.True);
            Assert.That(resolver.Resolve<ISessionScoreService>(), Is.SameAs(scoreService));

            GameplayPlatformContext.ClearCurrent();
            Object.DestroyImmediate(consumerObject);
            Object.DestroyImmediate(scopeObject);
        }

        [Test]
        public void LifetimeScope_Destroy_ClearsActivePlatformContext()
        {
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace();
            GameObject scopeObject = new GameObject("Scope");
            PyralisGameplayLifetimeScope scope = scopeObject.AddComponent<PyralisGameplayLifetimeScope>();

            scope.ConfigureRuntime(
                platformContext,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new LocalSessionOwnershipService(),
                new LocalParticipantAuthorityService());

            scope.Build();

            Assert.That(GameplayPlatformContext.TryGetCurrent(out _), Is.True);
            Assert.That(platformContext.Services.TryResolve(out IObjectResolver _), Is.True);

            Object.DestroyImmediate(scopeObject);

            Assert.That(GameplayPlatformContext.TryGetCurrent(out _), Is.False);
        }
    }
}
