using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Modules.Actor.Composition;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
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
            IActorHealthState health,
            ActorFeatureHost featureHost,
            IActorKnockbackController knockback,
            EnemyActorPresentationReferences presentation)
        {
            _owner = owner;
            Controller = controller;
            MovementModule = movementModule;
            DetectionModule = detectionModule;
            CombatModule = combatModule;
            AnimationModule = animationModule;
            Health = health;
            FeatureHost = featureHost;
            Knockback = knockback;
            Presentation = presentation;
        }

        public CharacterController Controller { get; }
        public EnemyMovementModule MovementModule { get; }
        public EnemyDetectionModule DetectionModule { get; }
        public EnemyCombatModule CombatModule { get; }
        public EnemyAnimationModule AnimationModule { get; }
        public IActorHealthState Health { get; }
        public ActorFeatureHost FeatureHost { get; private set; }
        public IActorKnockbackController Knockback { get; }
        public EnemyActorPresentationReferences Presentation { get; }

        public static EnemyActorRuntimeReferences Resolve(GameObject owner)
        {
            return new EnemyActorRuntimeReferences(
                owner,
                owner.GetComponent<CharacterController>(),
                owner.GetComponent<EnemyMovementModule>(),
                owner.GetComponent<EnemyDetectionModule>(),
                owner.GetComponent<EnemyCombatModule>(),
                owner.GetComponent<EnemyAnimationModule>(),
                owner.GetComponent<IActorHealthState>(),
                owner.GetComponent<ActorFeatureHost>(),
                owner.GetComponent<IActorKnockbackController>(),
                EnemyActorPresentationReferences.Resolve(owner));
        }

        public void ConfigureBillboard(
            Transform ownerTransform,
            Transform visualRoot,
            Camera presentationCamera,
            bool spriteDefaultFacesRight)
        {
            Presentation?.ConfigureBillboard(ownerTransform, visualRoot, presentationCamera, spriteDefaultFacesRight);
        }

        public void SetPresentationCamera(Camera camera)
        {
            Presentation?.SetPresentationCamera(camera);
        }

        public ActorFeatureContext BuildFeatureContext(
            EnemyFeatureProfile enemyFeatureProfile,
            IEnemyActorState enemyActorState)
        {
            return new ActorFeatureContext(
                _owner,
                health: Health,
                animation: Presentation?.AnimationDriver,
                knockback: Knockback,
                enemyActorState: enemyActorState,
                presentationMode: Presentation != null ? Presentation.PresentationMode : EnemyActorPresentationReferences.DefaultPresentationMode,
                authoredProfiles: new ScriptableObject[] { enemyFeatureProfile });
        }
    }
}
