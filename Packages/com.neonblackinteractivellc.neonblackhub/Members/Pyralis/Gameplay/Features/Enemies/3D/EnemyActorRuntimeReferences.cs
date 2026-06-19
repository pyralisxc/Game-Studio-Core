using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Combat;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Enemies
{
    internal sealed class EnemyActorRuntimeReferences
    {
        private readonly GameObject _owner;

        private EnemyActorRuntimeReferences(
            GameObject owner,
            CharacterController controller,
            EnemyMovementModule movementModule,
            EnemyDetectionModule detectionModule,
            EnemyCombatModule combatModule,
            EnemyAnimationModule animationModule,
            HealthComponent healthComponent,
            ActorFeatureHost featureHost,
            ActorAnimationDriver animationDriver,
            KnockbackReceiver knockbackReceiver,
            BillboardFacing3D billboardFacing,
            SpriteRenderer spriteRenderer)
        {
            _owner = owner;
            Controller = controller;
            MovementModule = movementModule;
            DetectionModule = detectionModule;
            CombatModule = combatModule;
            AnimationModule = animationModule;
            HealthComponent = healthComponent;
            FeatureHost = featureHost;
            AnimationDriver = animationDriver;
            KnockbackReceiver = knockbackReceiver;
            BillboardFacing = billboardFacing;
            SpriteRenderer = spriteRenderer;
        }

        public CharacterController Controller { get; }
        public EnemyMovementModule MovementModule { get; }
        public EnemyDetectionModule DetectionModule { get; }
        public EnemyCombatModule CombatModule { get; }
        public EnemyAnimationModule AnimationModule { get; }
        public HealthComponent HealthComponent { get; }
        public ActorFeatureHost FeatureHost { get; private set; }
        public ActorAnimationDriver AnimationDriver { get; }
        public KnockbackReceiver KnockbackReceiver { get; }
        public BillboardFacing3D BillboardFacing { get; }
        public SpriteRenderer SpriteRenderer { get; }

        public static EnemyActorRuntimeReferences Resolve(GameObject owner)
        {
            return new EnemyActorRuntimeReferences(
                owner,
                owner.GetComponent<CharacterController>(),
                owner.GetComponent<EnemyMovementModule>(),
                owner.GetComponent<EnemyDetectionModule>(),
                owner.GetComponent<EnemyCombatModule>(),
                owner.GetComponent<EnemyAnimationModule>(),
                owner.GetComponent<HealthComponent>(),
                owner.GetComponent<ActorFeatureHost>(),
                owner.GetComponent<ActorAnimationDriver>(),
                owner.GetComponent<KnockbackReceiver>(),
                owner.GetComponent<BillboardFacing3D>(),
                owner.GetComponentInChildren<SpriteRenderer>());
        }

        public void ConfigureBillboard(
            Transform ownerTransform,
            Transform visualRoot,
            Camera presentationCamera,
            bool spriteDefaultFacesRight)
        {
            if (BillboardFacing == null)
                return;

            BillboardFacing.Configure(
                visualRoot != null ? visualRoot : ownerTransform,
                visualRoot,
                SpriteRenderer,
                presentationCamera,
                BillboardFacing3D.FacingMode.YAxisOnly,
                spriteDefaultFacesRight);
        }

        public void SetPresentationCamera(Camera camera)
        {
            BillboardFacing?.SetCameraOverride(camera);
        }

        public ActorFeatureContext BuildFeatureContext(
            EnemyFeatureProfile enemyFeatureProfile,
            IEnemyActorState enemyActorState)
        {
            return new ActorFeatureContext(
                _owner,
                health: HealthComponent,
                animation: AnimationDriver,
                knockback: KnockbackReceiver,
                enemyActorState: enemyActorState,
                presentationMode: AnimationDriver != null ? AnimationDriver.PresentationMode : ActorPresentationMode.Billboard2_5D,
                authoredProfiles: new ScriptableObject[] { enemyFeatureProfile });
        }
    }
}
