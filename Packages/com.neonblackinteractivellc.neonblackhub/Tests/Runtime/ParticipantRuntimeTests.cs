using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Visuals;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Enemies;
using NeonBlack.Gameplay.Features.Feedback;
using NeonBlack.Gameplay.Features.Feedback.UI;
using NeonBlack.Gameplay.Features.Hazards;
using NeonBlack.Gameplay.Features.Input;
using NeonBlack.Gameplay.Features.Interaction;
using NeonBlack.Gameplay.Features.Pickups;
using NeonBlack.Gameplay.Features.Traversal;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Runtime;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Characters;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Tests.Runtime
{
    public class ParticipantRuntimeTests
    {
        private sealed class TestFeatureRuntime : MonoBehaviour, IFeatureModuleRuntime
        {
            public static readonly System.Collections.Generic.List<string> InitializationOrder = new();

            [SerializeField] private string moduleId = "test.module";

            public string ModuleId => moduleId;

            public void InitializeFeature(FeatureRuntimeInitializationContext initializationContext)
            {
                FeatureModuleDefinition definition = initializationContext != null ? initializationContext.Definition : null;
                InitializationOrder.Add(definition != null ? definition.moduleId : moduleId);
            }

            public void ShutdownFeature()
            {
            }
        }

        private sealed class TestInputModule : MonoBehaviour, IPawnInputModule
        {
            public InputProfile AppliedProfile { get; private set; }
            public ParticipantHandle AppliedParticipant { get; private set; }

            public void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile)
            {
                AppliedParticipant = context.Participant;
                AppliedProfile = inputProfile;
            }
        }

        private sealed class TestInteractionHandler : MonoBehaviour, IActorInteractionHandler
        {
            public bool ShouldHandle = true;
            public int CallCount { get; private set; }

            public bool TryHandleInteraction(ActorFeatureContext context)
            {
                CallCount++;
                return ShouldHandle;
            }
        }

        private sealed class TestInteractionReceiver2D : MonoBehaviour, IActorInteractionInputReceiver2D
        {
            public int CallCount { get; private set; }

            public void HandleInteractionInput()
            {
                CallCount++;
            }
        }

        private sealed class TestMovementModifierReceiver : MonoBehaviour, IActorMovementModifierReceiver
        {
            public float SpeedMultiplier { get; private set; } = 1f;
            public bool ActionLocked { get; private set; }

            public void SetStatusMoveSpeedMultiplier(float multiplier)
            {
                SpeedMultiplier = multiplier;
            }

            public void SetStatusActionLock(bool locked)
            {
                ActionLocked = locked;
            }
        }

        private sealed class TestCombatModifierReceiver : MonoBehaviour, IActorCombatModifierReceiver
        {
            public float DamageMultiplier { get; private set; } = 1f;
            public float KnockbackMultiplier { get; private set; } = 1f;

            public void SetOutgoingDamageMultiplier(float multiplier)
            {
                DamageMultiplier = multiplier;
            }

            public void SetOutgoingKnockbackMultiplier(float multiplier)
            {
                KnockbackMultiplier = multiplier;
            }
        }

        private sealed class TestFeedbackReceiver : MonoBehaviour, IActorFeedbackReceiver
        {
            public readonly System.Collections.Generic.List<ActorFeedbackEvent> Events = new();

            public void HandleFeedbackEvent(ActorFeedbackEvent feedbackEvent)
            {
                Events.Add(feedbackEvent);
            }
        }

        private sealed class TestHudLabel : MonoBehaviour
        {
        }

        private sealed class TestPickupAwardSink : IPickupAwardSink
        {
            public int AwardCount { get; private set; }
            public PickupAwardPayload LastPayload { get; private set; }

            public void ApplyAward(in PickupAwardPayload payload)
            {
                AwardCount++;
                LastPayload = payload;
            }
        }

        [TearDown]
        public void TearDown()
        {
            GameplayPlatformContext.ClearCurrent();
            TestFeatureRuntime.InitializationOrder.Clear();
        }

        [Test]
        public void RegisterParticipant_AssignsSeatAndPrimaryParticipant()
        {
            GameObject go = new GameObject("Roster");
            ParticipantRosterService roster = go.AddComponent<ParticipantRosterService>();

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.maxParticipants = 3;
            roster.SetSessionDefinition(session);

            ParticipantDefinition definition = ScriptableObject.CreateInstance<ParticipantDefinition>();
            definition.displayName = "P1";

            ParticipantHandle participant = roster.RegisterParticipant(null, definition, 0);

            Assert.That(participant, Is.Not.Null);
            Assert.That(participant.SeatIndex, Is.EqualTo(0));
            Assert.That(participant.DisplayName, Is.EqualTo("P1"));
            Assert.That(roster.TryGetPrimaryParticipant(out ParticipantHandle primary), Is.True);
            Assert.That(primary, Is.EqualTo(participant));

            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(session);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RegisterParticipant_RespectsSessionCapacity()
        {
            GameObject go = new GameObject("Roster");
            ParticipantRosterService roster = go.AddComponent<ParticipantRosterService>();

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.maxParticipants = 1;
            roster.SetSessionDefinition(session);

            ParticipantHandle first = roster.RegisterParticipant(null);
            ParticipantHandle second = roster.RegisterParticipant(null);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Null);
            Assert.That(roster.Participants.Count, Is.EqualTo(1));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void RegisterParticipant_ReassignsDuplicatePreferredSeat()
        {
            GameObject go = new GameObject("Roster");
            ParticipantRosterService roster = go.AddComponent<ParticipantRosterService>();

            SessionDefinition session = ScriptableObject.CreateInstance<SessionDefinition>();
            session.maxParticipants = 3;
            roster.SetSessionDefinition(session);

            ParticipantHandle first = roster.RegisterParticipant(null, preferredSeatIndex: 0);
            ParticipantHandle second = roster.RegisterParticipant(null, preferredSeatIndex: 0);

            Assert.That(first, Is.Not.Null);
            Assert.That(second, Is.Not.Null);
            Assert.That(first.SeatIndex, Is.EqualTo(0));
            Assert.That(second.SeatIndex, Is.EqualTo(1));

            Object.DestroyImmediate(session);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ParticipantSpawnService_OverflowSeatUsesOffsetInsteadOfLastSpawnPoint()
        {
            GameObject root = new GameObject("Spawn Service");
            root.transform.position = new Vector3(10f, 0f, 0f);
            ParticipantSpawnService spawnService = root.AddComponent<ParticipantSpawnService>();

            GameObject spawnPointObject = new GameObject("Spawn 0");
            spawnPointObject.transform.position = new Vector3(100f, 0f, 0f);
            spawnService.SetSpawnPoints(new[] { spawnPointObject.transform });

            GameObject pawnPrefab = new GameObject("Pawn Prefab");
            PawnDefinition pawnDefinition = ScriptableObject.CreateInstance<PawnDefinition>();
            pawnDefinition.pawnPrefab = pawnPrefab;
            ParticipantDefinition participantDefinition = ScriptableObject.CreateInstance<ParticipantDefinition>();
            participantDefinition.defaultPawn = pawnDefinition;
            ParticipantHandle participant = new ParticipantHandle(
                new ParticipantId(1),
                seat: 2,
                team: 0,
                clientId: 0,
                local: true,
                name: "P3",
                playerInput: null,
                definition: participantDefinition);

            GameObject instance = spawnService.SpawnParticipantPawn(participant);

            Assert.That(instance.transform.position, Is.EqualTo(new Vector3(14f, 0f, 0f)));

            Object.DestroyImmediate(instance);
            Object.DestroyImmediate(participantDefinition);
            Object.DestroyImmediate(pawnDefinition);
            Object.DestroyImmediate(pawnPrefab);
            Object.DestroyImmediate(spawnPointObject);
            Object.DestroyImmediate(root);
        }

        [Test]
        public void ParticipantQueryUtility_ReturnsClosestParticipantTransform()
        {
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace();
            GameObject go = new GameObject("Roster");
            ParticipantRosterService roster = go.AddComponent<ParticipantRosterService>();
            platformContext.Services.Register<IParticipantRoster>(roster);

            ParticipantHandle first = roster.RegisterParticipant(null, preferredSeatIndex: 0);
            ParticipantHandle second = roster.RegisterParticipant(null, preferredSeatIndex: 1);

            GameObject pawnA = new GameObject("PawnA");
            pawnA.transform.position = new Vector3(-5f, 0f, 0f);
            first.AttachPawn(pawnA);

            GameObject pawnB = new GameObject("PawnB");
            pawnB.transform.position = new Vector3(3f, 0f, 0f);
            second.AttachPawn(pawnB);

            bool found = ParticipantQueryUtility.TryGetClosestParticipantTransform(Vector3.zero, out Transform closest, out float distance);

            Assert.That(found, Is.True);
            Assert.That(closest, Is.EqualTo(pawnB.transform));
            Assert.That(distance, Is.EqualTo(3f).Within(0.001f));

            Object.DestroyImmediate(pawnA);
            Object.DestroyImmediate(pawnB);
            Object.DestroyImmediate(go);
            GameplayPlatformContext.ClearCurrent();
        }

        [Test]
        public void ActorAnimationDriver_AppliesPresentationFacingForSpriteActors()
        {
            GameObject go = new GameObject("AnimatedPawn");
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            SpriteRenderer spriteRenderer = visual.AddComponent<SpriteRenderer>();
            ActorAnimationDriver driver = go.AddComponent<ActorAnimationDriver>();

            PawnPresentationProfile presentationProfile = ScriptableObject.CreateInstance<PawnPresentationProfile>();
            presentationProfile.presentationMode = ActorPresentationMode.Sprite2D;
            presentationProfile.spriteDefaultFacesRight = true;

            driver.ApplyProfiles(presentationProfile, ScriptableObject.CreateInstance<PawnAnimationProfile>());
            driver.SetFacing(true);

            Assert.That(spriteRenderer.flipX, Is.False);

            Object.DestroyImmediate(presentationProfile);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void PawnRoot_InitializeForParticipant_AppliesResolvedInputProfile()
        {
            GameObject pawnObject = new GameObject("Pawn");
            PawnRoot pawnRoot = pawnObject.AddComponent<PawnRoot>();
            TestInputModule inputModule = pawnObject.AddComponent<TestInputModule>();

            PawnDefinition pawnDefinition = ScriptableObject.CreateInstance<PawnDefinition>();
            InputProfile pawnInputProfile = ScriptableObject.CreateInstance<InputProfile>();

            ParticipantDefinition participantDefinition = ScriptableObject.CreateInstance<ParticipantDefinition>();
            InputProfile participantInputProfile = ScriptableObject.CreateInstance<InputProfile>();

            pawnDefinition.defaultInputProfile = pawnInputProfile;
            participantDefinition.defaultPawn = pawnDefinition;
            participantDefinition.inputProfile = participantInputProfile;

            typeof(PawnRoot)
                .GetField("pawnDefinition", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(pawnRoot, pawnDefinition);

            ParticipantHandle participant = new ParticipantHandle(
                new ParticipantId(1),
                0,
                0,
                0UL,
                true,
                "P1",
                null,
                participantDefinition);

            pawnRoot.InitializeForParticipant(participant, null);

            Assert.That(inputModule.AppliedParticipant, Is.EqualTo(participant));
            Assert.That(inputModule.AppliedProfile, Is.EqualTo(participantInputProfile));

            Object.DestroyImmediate(participantInputProfile);
            Object.DestroyImmediate(participantDefinition);
            Object.DestroyImmediate(pawnInputProfile);
            Object.DestroyImmediate(pawnDefinition);
            Object.DestroyImmediate(pawnObject);
        }

        [Test]
        public void ActorShadowDriver_CreatesRuntimeBlobShadowFromPresentationProfile()
        {
            GameObject go = new GameObject("ShadowedPawn");
            ActorShadowDriver shadowDriver = go.AddComponent<ActorShadowDriver>();
            PawnPresentationProfile profile = ScriptableObject.CreateInstance<PawnPresentationProfile>();
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();

            profile.shadowMode = ActorShadowMode.BlobSprite;
            profile.shadowSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f));

            shadowDriver.ApplyProfile(profile);

            Transform runtimeShadow = go.transform.Find("RuntimeShadow");
            Assert.That(runtimeShadow, Is.Not.Null);
            Assert.That(runtimeShadow.gameObject.activeSelf, Is.True);

            Object.DestroyImmediate(profile.shadowSprite);
            Object.DestroyImmediate(texture);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ActorFeatureHost_InitializesDefinitionsInDeterministicOrderAndFiltersPresentationMode()
        {
            GameObject hostObject = new GameObject("Actor");
            ActorFeatureHost host = hostObject.AddComponent<ActorFeatureHost>();

            FeatureModuleDefinition lateDefinition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            lateDefinition.moduleId = "late";
            lateDefinition.installOrder = 20;
            lateDefinition.supportedPresentationModes = new[] { ActorPresentationMode.Sprite2D };
            lateDefinition.runtimePrefab = new GameObject("LatePrefab");
            lateDefinition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            FeatureModuleDefinition filteredDefinition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            filteredDefinition.moduleId = "filtered";
            filteredDefinition.installOrder = 0;
            filteredDefinition.supportedPresentationModes = new[] { ActorPresentationMode.Rigged3D };
            filteredDefinition.runtimePrefab = new GameObject("FilteredPrefab");
            filteredDefinition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            FeatureModuleDefinition earlyDefinition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            earlyDefinition.moduleId = "early";
            earlyDefinition.installOrder = 5;
            earlyDefinition.supportedPresentationModes = new[] { ActorPresentationMode.Sprite2D };
            earlyDefinition.runtimePrefab = new GameObject("EarlyPrefab");
            earlyDefinition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            ActorFeatureContext context = new ActorFeatureContext(hostObject, presentationMode: ActorPresentationMode.Sprite2D);

            host.InitializeFeatures(context, new[] { lateDefinition, filteredDefinition, earlyDefinition });

            Assert.That(TestFeatureRuntime.InitializationOrder, Is.EqualTo(new[] { "early", "late" }));

            Object.DestroyImmediate(earlyDefinition.runtimePrefab);
            Object.DestroyImmediate(lateDefinition.runtimePrefab);
            Object.DestroyImmediate(filteredDefinition.runtimePrefab);
            Object.DestroyImmediate(earlyDefinition);
            Object.DestroyImmediate(lateDefinition);
            Object.DestroyImmediate(filteredDefinition);
            Object.DestroyImmediate(hostObject);
        }

        [Test]
        public void ActorFeatureHost_ReinitializeDestroysPreviouslyInstantiatedRuntimeObjects()
        {
            GameObject hostObject = new GameObject("Actor");
            ActorFeatureHost host = hostObject.AddComponent<ActorFeatureHost>();

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.moduleId = "runtime";
            definition.runtimePrefab = new GameObject("RuntimePrefab");
            definition.runtimePrefab.AddComponent<TestFeatureRuntime>();

            ActorFeatureContext context = new ActorFeatureContext(hostObject, presentationMode: ActorPresentationMode.Sprite2D);

            host.InitializeFeatures(context, new[] { definition });
            Transform firstInstance = hostObject.transform.GetChild(0);

            host.InitializeFeatures(context, new[] { definition });

            Assert.That(firstInstance.gameObject.activeSelf, Is.False);
            Assert.That(firstInstance.parent, Is.Null);
            Assert.That(hostObject.transform.childCount, Is.EqualTo(1));
            Assert.That(host.InstalledModules.Count, Is.EqualTo(1));

            Object.DestroyImmediate(definition.runtimePrefab);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(hostObject);
        }

        [Test]
        public void EnemyReactionFeatureRuntime_LocksReactionStateWhenDamageCrossesThreshold()
        {
            GameObject enemy = new GameObject("Enemy");
            HealthComponent health = enemy.AddComponent<HealthComponent>();
            EnemyReactionFeatureRuntime runtime = enemy.AddComponent<EnemyReactionFeatureRuntime>();
            enemy.AddComponent<CharacterController>();
            KnockbackReceiver knockback = enemy.AddComponent<KnockbackReceiver>();

            EnemyReactionProfile reactionProfile = ScriptableObject.CreateInstance<EnemyReactionProfile>();
            reactionProfile.staggerDamageThreshold = 10f;
            reactionProfile.staggerLockDuration = 0.25f;

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = reactionProfile;

            ActorFeatureContext context = new ActorFeatureContext(
                enemy,
                health: health,
                knockback: knockback,
                presentationMode: ActorPresentationMode.Billboard2_5D,
                authoredProfiles: new ScriptableObject[] { reactionProfile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));

            GameObject attacker = new GameObject("Attacker");
            attacker.AddComponent<HealthComponent>();
            health.TakeDamage(15f, Vector3.zero, attacker);

            Assert.That(runtime.IsReactionLocked, Is.True);

            runtime.ShutdownFeature();
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(reactionProfile);
            Object.DestroyImmediate(enemy);
        }

        [Test]
        public void ActorInteractionFeatureRuntime_UsesRegisteredHandlerBeforeFallbackAnimation()
        {
            GameObject actor = new GameObject("Interactor");
            ActorInteractionFeatureRuntime runtime = actor.AddComponent<ActorInteractionFeatureRuntime>();
            TestInteractionHandler handler = actor.AddComponent<TestInteractionHandler>();

            InteractionFeatureProfile profile = ScriptableObject.CreateInstance<InteractionFeatureProfile>();
            profile.enableInteraction = true;
            profile.triggerInteractAnimationWhenUnhandled = false;

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                presentationMode: ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));

            bool handled = runtime.TryHandleInteraction();

            Assert.That(handled, Is.True);
            Assert.That(handler.CallCount, Is.EqualTo(1));

            runtime.ShutdownFeature();
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorInteractionFeatureRuntime_DoesNotSpendCooldownWhenNoHandlerSucceeds()
        {
            GameObject actor = new GameObject("InteractorNoHit");
            ActorInteractionFeatureRuntime runtime = actor.AddComponent<ActorInteractionFeatureRuntime>();
            TestInteractionHandler handler = actor.AddComponent<TestInteractionHandler>();
            handler.ShouldHandle = false;

            InteractionFeatureProfile profile = ScriptableObject.CreateInstance<InteractionFeatureProfile>();
            profile.enableInteraction = true;
            profile.interactionCooldown = 10f;
            profile.triggerInteractAnimationWhenUnhandled = false;

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                presentationMode: ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));

            bool firstHandled = runtime.TryHandleInteraction();
            handler.ShouldHandle = true;
            bool secondHandled = runtime.TryHandleInteraction();

            Assert.That(firstHandled, Is.False);
            Assert.That(secondHandled, Is.True);
            Assert.That(handler.CallCount, Is.EqualTo(2));

            runtime.ShutdownFeature();
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void FeatureRuntimes_InitializeWithNullContextWithoutThrowing()
        {
            GameObject actor = new GameObject("LooseFeatureRuntime");
            ActorInteractionFeatureRuntime interaction = actor.AddComponent<ActorInteractionFeatureRuntime>();
            ActorPickupCollectorFeature2D pickup2D = actor.AddComponent<ActorPickupCollectorFeature2D>();
            ActorPickupCollectorFeature3D pickup3D = actor.AddComponent<ActorPickupCollectorFeature3D>();
            ActorFeedbackFeatureRuntime feedback = actor.AddComponent<ActorFeedbackFeatureRuntime>();
            PawnTraversalFeatureRuntime3D traversal3D = actor.AddComponent<PawnTraversalFeatureRuntime3D>();

            Assert.DoesNotThrow(() => interaction.InitializeFeature(null));
            Assert.DoesNotThrow(() => pickup2D.InitializeFeature(null));
            Assert.DoesNotThrow(() => pickup3D.InitializeFeature(null));
            Assert.DoesNotThrow(() => feedback.InitializeFeature(null));
            Assert.DoesNotThrow(() => traversal3D.InitializeFeature(null));

            interaction.ShutdownFeature();
            pickup2D.ShutdownFeature();
            pickup3D.ShutdownFeature();
            feedback.ShutdownFeature();
            traversal3D.ShutdownFeature();
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void PlayerInputHandler_BindsInteractActionWithoutThrowingWhenReceiverExists()
        {
            GameObject actor = new GameObject("Pawn2D");
            actor.SetActive(false);
            actor.AddComponent<Motor2D>();
            PlayerInputHandler inputHandler = actor.AddComponent<PlayerInputHandler>();
            TestInteractionReceiver2D receiver = actor.AddComponent<TestInteractionReceiver2D>();

            InputActionAsset asset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap map = new InputActionMap("Player");
            map.AddAction("Move", type: UnityEngine.InputSystem.InputActionType.Value);
            map.AddAction("Jump", type: UnityEngine.InputSystem.InputActionType.Button);
            map.AddAction("Attack", type: UnityEngine.InputSystem.InputActionType.Button);
            map.AddAction("Kick", type: UnityEngine.InputSystem.InputActionType.Button);
            map.AddAction("Interact", type: UnityEngine.InputSystem.InputActionType.Button);
            asset.AddActionMap(map);

            inputHandler.SetInputActions(asset);
            actor.SetActive(true);

            Assert.That(receiver.CallCount, Is.EqualTo(0));

            Object.DestroyImmediate(asset);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorPickupCollectorFeature2D_CollectsOverlappingPickupThroughFeatureRuntime()
        {
            GameObject actor = new GameObject("PickupActor");
            CircleCollider2D actorCollider = actor.AddComponent<CircleCollider2D>();
            actorCollider.isTrigger = true;
            ActorPickupCollectorFeature2D runtime = actor.AddComponent<ActorPickupCollectorFeature2D>();

            GameObject pickup = new GameObject("Pickup");
            CircleCollider2D pickupCollider = pickup.AddComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            pickup.transform.position = actor.transform.position;
            pickup.AddComponent<Collectible2D>();

            PickupFeatureProfile profile = ScriptableObject.CreateInstance<PickupFeatureProfile>();
            profile.enableAutoCollect = true;

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                presentationMode: ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));
            runtime.SendMessage("Update");

            Assert.That(pickup.activeSelf, Is.False);

            runtime.ShutdownFeature();
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(pickup);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void Collectible2D_ResolvesAwardSinkFromPlatformServices()
        {
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace();
            TestPickupAwardSink awardSink = new TestPickupAwardSink();
            platformContext.Services.Register<IPickupAwardSink>(awardSink);

            GameObject collector = new GameObject("Collector");
            GameObject pickup = new GameObject("ServicePickup2D");
            CircleCollider2D pickupCollider = pickup.AddComponent<CircleCollider2D>();
            pickupCollider.isTrigger = true;
            Collectible2D collectible = pickup.AddComponent<Collectible2D>();

            collectible.CollectBy(collector);

            Assert.That(awardSink.AwardCount, Is.EqualTo(1));
            Assert.That(awardSink.LastPayload.Collector, Is.EqualTo(collector));
            Assert.That(awardSink.LastPayload.Outcome, Is.EqualTo(PickupAwardOutcome.Collected));
            Assert.That(pickup.activeSelf, Is.False);

            Object.DestroyImmediate(pickup);
            Object.DestroyImmediate(collector);
        }

        [Test]
        public void ActorPickupCollectorFeature3D_CollectsOverlappingPickupThroughFeatureRuntime()
        {
            GameObject actor = new GameObject("PickupActor3D");
            SphereCollider actorCollider = actor.AddComponent<SphereCollider>();
            actorCollider.isTrigger = true;
            ActorPickupCollectorFeature3D runtime = actor.AddComponent<ActorPickupCollectorFeature3D>();

            GameObject pickup = new GameObject("Pickup3D");
            SphereCollider pickupCollider = pickup.AddComponent<SphereCollider>();
            pickupCollider.isTrigger = true;
            pickup.transform.position = actor.transform.position;
            pickup.AddComponent<Collectible3D>();

            PickupFeatureProfile profile = ScriptableObject.CreateInstance<PickupFeatureProfile>();
            profile.enableAutoCollect = true;
            profile.collectibleLayers3D = ~0;

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                presentationMode: ActorPresentationMode.Billboard2_5D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));
            runtime.SendMessage("Update");

            Assert.That(pickup.activeSelf, Is.False);

            runtime.ShutdownFeature();
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(pickup);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void Collectible3D_ResolvesAwardSinkFromPlatformServices()
        {
            GameplayPlatformContext platformContext = GameplayPlatformContext.CreateOrReplace();
            TestPickupAwardSink awardSink = new TestPickupAwardSink();
            platformContext.Services.Register<IPickupAwardSink>(awardSink);

            GameObject collector = new GameObject("Collector3D");
            GameObject pickup = new GameObject("ServicePickup3D");
            SphereCollider pickupCollider = pickup.AddComponent<SphereCollider>();
            pickupCollider.isTrigger = true;
            Collectible3D collectible = pickup.AddComponent<Collectible3D>();

            collectible.CollectBy(collector);

            Assert.That(awardSink.AwardCount, Is.EqualTo(1));
            Assert.That(awardSink.LastPayload.Collector, Is.EqualTo(collector));
            Assert.That(awardSink.LastPayload.Outcome, Is.EqualTo(PickupAwardOutcome.Collected));
            Assert.That(pickup.activeSelf, Is.False);

            Object.DestroyImmediate(pickup);
            Object.DestroyImmediate(collector);
        }

        [Test]
        public void ActorStatusEffectFeatureRuntime_AppliesAndResetsSharedModifiers()
        {
            GameObject actor = new GameObject("StatusActor");
            HealthComponent health = actor.AddComponent<HealthComponent>();
            TestMovementModifierReceiver movementReceiver = actor.AddComponent<TestMovementModifierReceiver>();
            TestCombatModifierReceiver combatReceiver = actor.AddComponent<TestCombatModifierReceiver>();
            ActorStatusEffectFeatureRuntime runtime = actor.AddComponent<ActorStatusEffectFeatureRuntime>();

            ActorStatusEffectProfile profile = ScriptableObject.CreateInstance<ActorStatusEffectProfile>();
            profile.defaultShieldDamageReduction = 0.1f;

            StatusEffectDefinition slow = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            slow.effectId = "slow";
            slow.effectKind = StatusEffectKind.Slow;
            slow.magnitude = 0.5f;

            StatusEffectDefinition stun = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            stun.effectId = "stun";
            stun.effectKind = StatusEffectKind.Stun;

            StatusEffectDefinition shield = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            shield.effectId = "shield";
            shield.effectKind = StatusEffectKind.Shield;
            shield.magnitude = 0.25f;

            StatusEffectDefinition damageBoost = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            damageBoost.effectId = "damage";
            damageBoost.effectKind = StatusEffectKind.DamageBoost;
            damageBoost.magnitude = 1.5f;

            StatusEffectDefinition knockbackBoost = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            knockbackBoost.effectId = "knockback";
            knockbackBoost.effectKind = StatusEffectKind.KnockbackBoost;
            knockbackBoost.magnitude = 2f;

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                health: health,
                presentationMode: ActorPresentationMode.Rigged3D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));
            runtime.ApplyStatusEffect(slow);
            runtime.ApplyStatusEffect(stun);
            runtime.ApplyStatusEffect(shield);
            runtime.ApplyStatusEffect(damageBoost);
            runtime.ApplyStatusEffect(knockbackBoost);

            float damage = 10f;
            bool modified = runtime.TryModifyIncomingDamage(actor, ref damage);

            Assert.That(movementReceiver.SpeedMultiplier, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(movementReceiver.ActionLocked, Is.True);
            Assert.That(combatReceiver.DamageMultiplier, Is.EqualTo(1.5f).Within(0.001f));
            Assert.That(combatReceiver.KnockbackMultiplier, Is.EqualTo(2f).Within(0.001f));
            Assert.That(modified, Is.True);
            Assert.That(damage, Is.EqualTo(7.5f).Within(0.001f));

            runtime.ShutdownFeature();

            Assert.That(movementReceiver.SpeedMultiplier, Is.EqualTo(1f).Within(0.001f));
            Assert.That(movementReceiver.ActionLocked, Is.False);
            Assert.That(combatReceiver.DamageMultiplier, Is.EqualTo(1f).Within(0.001f));
            Assert.That(combatReceiver.KnockbackMultiplier, Is.EqualTo(1f).Within(0.001f));

            Object.DestroyImmediate(knockbackBoost);
            Object.DestroyImmediate(damageBoost);
            Object.DestroyImmediate(shield);
            Object.DestroyImmediate(stun);
            Object.DestroyImmediate(slow);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorFeedbackFeatureRuntime_PublishesHealthAndManualEvents()
        {
            GameObject actor = new GameObject("FeedbackActor");
            HealthComponent health = actor.AddComponent<HealthComponent>();
            ActorFeedbackFeatureRuntime runtime = actor.AddComponent<ActorFeedbackFeatureRuntime>();
            TestFeedbackReceiver receiver = actor.AddComponent<TestFeedbackReceiver>();

            ActorFeedbackProfile profile = ScriptableObject.CreateInstance<ActorFeedbackProfile>();
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                health: health,
                presentationMode: ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));

            GameObject attacker = new GameObject("Attacker");
            attacker.AddComponent<HealthComponent>();
            health.TakeDamage(10f, Vector3.zero, attacker);
            health.Heal(5f);
            runtime.PublishCombo(2);
            runtime.PublishScore(1);

            Assert.That(receiver.Events.Exists(evt => evt.EventType == ActorFeedbackEventType.Damage), Is.True);
            Assert.That(receiver.Events.Exists(evt => evt.EventType == ActorFeedbackEventType.Heal), Is.True);
            Assert.That(receiver.Events.Exists(evt => evt.EventType == ActorFeedbackEventType.Combo && evt.IntValue == 2), Is.True);
            Assert.That(receiver.Events.Exists(evt => evt.EventType == ActorFeedbackEventType.Score && evt.IntValue == 1), Is.True);

            runtime.ShutdownFeature();
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorStatusEffectFeatureRuntime_SupportsBoostAndArmorBreakEffects()
        {
            GameObject actor = new GameObject("BoostActor");
            HealthComponent health = actor.AddComponent<HealthComponent>();
            TestMovementModifierReceiver movementReceiver = actor.AddComponent<TestMovementModifierReceiver>();
            TestCombatModifierReceiver combatReceiver = actor.AddComponent<TestCombatModifierReceiver>();
            ActorStatusEffectFeatureRuntime runtime = actor.AddComponent<ActorStatusEffectFeatureRuntime>();

            ActorStatusEffectProfile profile = ScriptableObject.CreateInstance<ActorStatusEffectProfile>();
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                health: health,
                presentationMode: ActorPresentationMode.Billboard2_5D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));

            StatusEffectDefinition speedBoost = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            speedBoost.effectId = "speed";
            speedBoost.effectKind = StatusEffectKind.SpeedBoost;
            speedBoost.magnitude = 1.75f;

            StatusEffectDefinition armorBreak = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            armorBreak.effectId = "break";
            armorBreak.effectKind = StatusEffectKind.ArmorBreak;
            armorBreak.magnitude = 1.5f;

            runtime.ApplyStatusEffect(speedBoost);
            runtime.ApplyStatusEffect(armorBreak);

            float damage = 10f;
            bool modified = runtime.TryModifyIncomingDamage(actor, ref damage);

            Assert.That(movementReceiver.SpeedMultiplier, Is.EqualTo(1.75f).Within(0.001f));
            Assert.That(modified, Is.True);
            Assert.That(damage, Is.EqualTo(15f).Within(0.001f));
            Assert.That(combatReceiver.DamageMultiplier, Is.EqualTo(1f).Within(0.001f));

            runtime.ShutdownFeature();
            Object.DestroyImmediate(speedBoost);
            Object.DestroyImmediate(armorBreak);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ParticipantFeedbackRelay_PublishesActorFeedbackToParticipantService()
        {
            GameObject serviceObject = new GameObject("ParticipantFeedbackService");
            ParticipantFeedbackService service = serviceObject.AddComponent<ParticipantFeedbackService>();
            GameplayPlatformContext context = GameplayPlatformContext.CreateOrReplace();
            context.Services.Register<IParticipantFeedbackPublisher>(service);

            ParticipantHandle capturedParticipant = null;
            int capturedScore = 0;
            int capturedCombo = 0;
            service.OnParticipantScorePopup.AddListener((participant, amount) =>
            {
                capturedParticipant = participant;
                capturedScore = amount;
            });
            service.OnParticipantComboPopup.AddListener((participant, amount) =>
            {
                capturedParticipant = participant;
                capturedCombo = amount;
            });

            GameObject rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
            context.Services.Register<IParticipantRoster>(roster);
            ParticipantHandle participant = roster.RegisterParticipant(null, preferredSeatIndex: 0);

            GameObject pawn = new GameObject("Pawn");
            participant.AttachPawn(pawn);
            ParticipantFeedbackRelay relay = pawn.AddComponent<ParticipantFeedbackRelay>();

            relay.HandleFeedbackEvent(new ActorFeedbackEvent(ActorFeedbackEventType.Score, intValue: 3));
            relay.HandleFeedbackEvent(new ActorFeedbackEvent(ActorFeedbackEventType.Combo, intValue: 2));

            Assert.That(capturedParticipant, Is.EqualTo(participant));
            Assert.That(capturedScore, Is.EqualTo(3));
            Assert.That(capturedCombo, Is.EqualTo(2));

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(rosterObject);
            Object.DestroyImmediate(serviceObject);
            GameplayPlatformContext.ClearCurrent();
        }

        [Test]
        public void ParticipantFeedbackRelay_PublishesCombatAlertsToParticipantService()
        {
            GameObject serviceObject = new GameObject("ParticipantFeedbackService");
            ParticipantFeedbackService service = serviceObject.AddComponent<ParticipantFeedbackService>();
            GameplayPlatformContext context = GameplayPlatformContext.CreateOrReplace();
            context.Services.Register<IParticipantFeedbackPublisher>(service);

            ParticipantHandle capturedParticipant = null;
            string capturedAlert = null;
            int capturedValue = -1;
            service.OnParticipantCombatAlert.AddListener((participant, alertKey, value) =>
            {
                capturedParticipant = participant;
                capturedAlert = alertKey;
                capturedValue = value;
            });

            GameObject rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
            context.Services.Register<IParticipantRoster>(roster);
            ParticipantHandle participant = roster.RegisterParticipant(null, preferredSeatIndex: 0);

            GameObject pawn = new GameObject("Pawn");
            participant.AttachPawn(pawn);
            ParticipantFeedbackRelay relay = pawn.AddComponent<ParticipantFeedbackRelay>();

            relay.HandleFeedbackEvent(new ActorFeedbackEvent(ActorFeedbackEventType.GuardBreak));
            Assert.That(capturedParticipant, Is.EqualTo(participant));
            Assert.That(capturedAlert, Is.EqualTo("GuardBreak"));

            relay.HandleFeedbackEvent(new ActorFeedbackEvent(ActorFeedbackEventType.Finisher, intValue: 3));
            Assert.That(capturedAlert, Is.EqualTo("Finisher"));
            Assert.That(capturedValue, Is.EqualTo(3));

            Object.DestroyImmediate(pawn);
            Object.DestroyImmediate(rosterObject);
            Object.DestroyImmediate(serviceObject);
            GameplayPlatformContext.ClearCurrent();
        }

        [Test]
        public void ParticipantInputRouter_RegisterAndUnregisterPlayerInput_UpdatesRosterWithoutPolling()
        {
            GameObject rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
            GameObject routerObject = new GameObject("Router");
            ParticipantInputRouter router = routerObject.AddComponent<ParticipantInputRouter>();
            GameObject playerObject = new GameObject("PlayerInput");
            PlayerInput playerInput = playerObject.AddComponent<PlayerInput>();

            router.SetRosterService(roster);
            router.RegisterPlayerInput(playerInput);

            Assert.That(roster.Participants.Count, Is.EqualTo(1));
            Assert.That(roster.Participants[0].PlayerInput, Is.SameAs(playerInput));

            router.UnregisterPlayerInput(playerInput);

            Assert.That(roster.Participants.Count, Is.EqualTo(0));

            Object.DestroyImmediate(playerObject);
            Object.DestroyImmediate(routerObject);
            Object.DestroyImmediate(rosterObject);
        }

        [Test]
        public void PlayerRegistry_StaticPlayer_PrefersParticipantProviderOverLocalRegistryFallback()
        {
            GameplayPlatformContext context = GameplayPlatformContext.CreateOrReplace();
            GameObject registryObject = new GameObject("Compatibility Player");
            registryObject.AddComponent<PlayerRegistry>();
            GameObject rosterObject = new GameObject("Roster");
            ParticipantRosterService roster = rosterObject.AddComponent<ParticipantRosterService>();
            GameObject participantPawn = new GameObject("Participant Pawn");
            ParticipantHandle participant = roster.RegisterParticipant(null, preferredSeatIndex: 0);
            participant.AttachPawn(participantPawn);
            context.Services.Register<IPlayerProvider>(roster);

            Assert.That(PlayerRegistry.Player, Is.SameAs(participantPawn.transform));

            Object.DestroyImmediate(participantPawn);
            Object.DestroyImmediate(rosterObject);
            Object.DestroyImmediate(registryObject);
            GameplayPlatformContext.ClearCurrent();
        }

        [Test]
        public void ActorCombatReactionFeatureRuntime_ReducesDamageAndAppliesReactionLock()
        {
            GameObject actor = new GameObject("CombatActor");
            actor.AddComponent<Rigidbody2D>();
            actor.AddComponent<PolygonCollider2D>();
            Motor2D motor = actor.AddComponent<Motor2D>();
            actor.AddComponent<ActorAnimationDriver>();
            HealthComponent health = actor.AddComponent<HealthComponent>();
            KnockbackReceiver knockback = actor.AddComponent<KnockbackReceiver>();
            ActorCombatReactionFeatureRuntime runtime = actor.AddComponent<ActorCombatReactionFeatureRuntime>();

            ActorCombatReactionProfile profile = ScriptableObject.CreateInstance<ActorCombatReactionProfile>();
            profile.enableParry = false;
            profile.blockDamageReduction = 0.5f;
            profile.hurtLockDuration = 0.2f;
            profile.staggerDamageThreshold = 100f;

            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                health: health,
                knockback: knockback,
                presentationMode: ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));
            runtime.BeginGuard();

            GameObject attacker = new GameObject("Attacker");
            attacker.transform.position = actor.transform.position + Vector3.right;
            attacker.AddComponent<HealthComponent>();

            health.TakeDamage(10f, Vector3.zero, attacker);

            Assert.That(health.CurrentHealth, Is.EqualTo(95f).Within(0.001f));
            Assert.That(motor.IsActionLocked, Is.True);

            runtime.ShutdownFeature();
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorCombatReactionFeatureRuntime_ParryNegatesDamageAndPublishesAlert()
        {
            GameObject actor = new GameObject("ParryActor");
            actor.AddComponent<Rigidbody2D>();
            actor.AddComponent<PolygonCollider2D>();
            Motor2D actorMotor = actor.AddComponent<Motor2D>();
            actor.AddComponent<ActorAnimationDriver>();
            HealthComponent actorHealth = actor.AddComponent<HealthComponent>();
            actor.AddComponent<KnockbackReceiver>();
            ActorCombatReactionFeatureRuntime reactionRuntime = actor.AddComponent<ActorCombatReactionFeatureRuntime>();
            ActorFeedbackFeatureRuntime feedbackRuntime = actor.AddComponent<ActorFeedbackFeatureRuntime>();
            TestFeedbackReceiver feedbackReceiver = actor.AddComponent<TestFeedbackReceiver>();

            FeatureModuleDefinition feedbackDefinition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            feedbackRuntime.InitializeFeature(new FeatureRuntimeInitializationContext(
                new ActorFeatureContext(actor, health: actorHealth, presentationMode: ActorPresentationMode.Sprite2D),
                feedbackDefinition,
                new PlatformServiceRegistry()));

            ActorCombatReactionProfile profile = ScriptableObject.CreateInstance<ActorCombatReactionProfile>();
            profile.enableParry = true;
            profile.parryWindowDuration = 0.5f;
            profile.parryReactionLockDuration = 0.25f;
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                health: actorHealth,
                presentationMode: ActorPresentationMode.Sprite2D,
                authoredProfiles: new ScriptableObject[] { profile });

            reactionRuntime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));
            reactionRuntime.BeginGuard();

            GameObject attacker = new GameObject("ParryAttacker");
            attacker.transform.position = actor.transform.position + Vector3.right;
            attacker.AddComponent<Rigidbody2D>();
            attacker.AddComponent<PolygonCollider2D>();
            Motor2D attackerMotor = attacker.AddComponent<Motor2D>();
            attacker.AddComponent<ActorAnimationDriver>();
            attacker.AddComponent<HealthComponent>();

            actorHealth.TakeDamage(10f, Vector3.zero, attacker);

            Assert.That(actorHealth.CurrentHealth, Is.EqualTo(100f).Within(0.001f));
            Assert.That(attackerMotor.IsActionLocked, Is.True);
            Assert.That(feedbackReceiver.Events.Exists(evt => evt.EventType == ActorFeedbackEventType.Parry), Is.True);

            reactionRuntime.ShutdownFeature();
            feedbackRuntime.ShutdownFeature();
            Object.DestroyImmediate(feedbackDefinition);
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorStatusEffectFeatureRuntime_ArmorReducesIncomingDamageThroughHealthModifier()
        {
            GameObject actor = new GameObject("ArmorActor");
            HealthComponent health = actor.AddComponent<HealthComponent>();
            actor.AddComponent<TestMovementModifierReceiver>();
            actor.AddComponent<TestCombatModifierReceiver>();
            ActorStatusEffectFeatureRuntime runtime = actor.AddComponent<ActorStatusEffectFeatureRuntime>();

            ActorStatusEffectProfile profile = ScriptableObject.CreateInstance<ActorStatusEffectProfile>();
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                health: health,
                presentationMode: ActorPresentationMode.Rigged3D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));

            StatusEffectDefinition armor = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            armor.effectId = "armor";
            armor.effectKind = StatusEffectKind.Armor;
            armor.magnitude = 0.5f;
            runtime.ApplyStatusEffect(armor);

            GameObject attacker = new GameObject("ArmorAttacker");
            attacker.AddComponent<HealthComponent>();
            health.TakeDamage(10f, Vector3.zero, attacker);

            Assert.That(health.CurrentHealth, Is.EqualTo(95f).Within(0.001f));

            runtime.ShutdownFeature();
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(armor);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void ActorStatusEffectFeatureRuntime_StackMagnitudeArmorCompoundsWithinMaxStacks()
        {
            GameObject actor = new GameObject("StackArmorActor");
            HealthComponent health = actor.AddComponent<HealthComponent>();
            actor.AddComponent<TestMovementModifierReceiver>();
            actor.AddComponent<TestCombatModifierReceiver>();
            ActorStatusEffectFeatureRuntime runtime = actor.AddComponent<ActorStatusEffectFeatureRuntime>();

            ActorStatusEffectProfile profile = ScriptableObject.CreateInstance<ActorStatusEffectProfile>();
            FeatureModuleDefinition definition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            definition.profileAsset = profile;

            ActorFeatureContext context = new ActorFeatureContext(
                actor,
                health: health,
                presentationMode: ActorPresentationMode.Rigged3D,
                authoredProfiles: new ScriptableObject[] { profile });

            runtime.InitializeFeature(new FeatureRuntimeInitializationContext(context, definition, new PlatformServiceRegistry()));

            StatusEffectDefinition armor = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            armor.effectId = "stacked.armor";
            armor.displayName = "Stacked Armor";
            armor.effectKind = StatusEffectKind.Armor;
            armor.stackMode = StatusEffectStackMode.StackMagnitude;
            armor.maxStacks = 3;
            armor.magnitude = 0.1f;

            runtime.ApplyStatusEffect(armor);
            runtime.ApplyStatusEffect(armor);

            GameObject attacker = new GameObject("StackArmorAttacker");
            attacker.AddComponent<HealthComponent>();
            health.TakeDamage(10f, Vector3.zero, attacker);

            Assert.That(health.CurrentHealth, Is.EqualTo(92f).Within(0.001f));

            runtime.ShutdownFeature();
            Object.DestroyImmediate(attacker);
            Object.DestroyImmediate(armor);
            Object.DestroyImmediate(definition);
            Object.DestroyImmediate(profile);
            Object.DestroyImmediate(actor);
        }

        [Test]
        public void HazardImpactUtility_AppliesDamageAndStatusEffectsThroughSharedRuntime()
        {
            GameObject actor = new GameObject("HazardTarget");
            HealthComponent health = actor.AddComponent<HealthComponent>();
            TestMovementModifierReceiver movementReceiver = actor.AddComponent<TestMovementModifierReceiver>();
            actor.AddComponent<TestCombatModifierReceiver>();
            ActorStatusEffectFeatureRuntime statusRuntime = actor.AddComponent<ActorStatusEffectFeatureRuntime>();

            ActorStatusEffectProfile statusProfile = ScriptableObject.CreateInstance<ActorStatusEffectProfile>();
            FeatureModuleDefinition statusDefinition = ScriptableObject.CreateInstance<FeatureModuleDefinition>();
            statusDefinition.profileAsset = statusProfile;
            statusRuntime.InitializeFeature(new FeatureRuntimeInitializationContext(
                new ActorFeatureContext(
                    actor,
                    health: health,
                    presentationMode: ActorPresentationMode.Sprite2D,
                    authoredProfiles: new ScriptableObject[] { statusProfile }),
                statusDefinition,
                new PlatformServiceRegistry()));

            StatusEffectDefinition slow = ScriptableObject.CreateInstance<StatusEffectDefinition>();
            slow.effectId = "hazard.slow";
            slow.displayName = "Hazard Slow";
            slow.effectKind = StatusEffectKind.Slow;
            slow.magnitude = 0.5f;

            HazardImpactProfile hazardProfile = ScriptableObject.CreateInstance<HazardImpactProfile>();
            hazardProfile.damagePerTick = 10f;
            hazardProfile.statusEffects = new[] { slow };
            hazardProfile.targeting = Features.Hazards.HazardTargetMode.All;

            bool applied = HazardImpactUtility.TryApplyImpact(actor, hazardProfile, actor, actor.transform.position);

            Assert.That(applied, Is.True);
            Assert.That(health.CurrentHealth, Is.EqualTo(90f).Within(0.001f));
            Assert.That(movementReceiver.SpeedMultiplier, Is.EqualTo(0.5f).Within(0.001f));

            statusRuntime.ShutdownFeature();
            Object.DestroyImmediate(hazardProfile);
            Object.DestroyImmediate(slow);
            Object.DestroyImmediate(statusDefinition);
            Object.DestroyImmediate(statusProfile);
            Object.DestroyImmediate(actor);
        }
    }
}
