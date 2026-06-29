using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
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
            IActorCombatRequestReceiver combatRequestReceiver,
            IActorCombatTacticalState combatTacticalState,
            IActorCombatModifierReceiver combatModifierReceiver,
            IEnemyCombatProfileReceiver combatProfileReceiver,
            EnemyAnimationModule animationModule,
            IActorHealthState health,
            IActorKnockbackController knockback,
            EnemyActorPresentationReferences presentation)
        {
            _owner = owner;
            Controller = controller;
            MovementModule = movementModule;
            DetectionModule = detectionModule;
            CombatRequestReceiver = combatRequestReceiver;
            CombatTacticalState = combatTacticalState;
            CombatModifierReceiver = combatModifierReceiver;
            CombatProfileReceiver = combatProfileReceiver;
            AnimationModule = animationModule;
            Health = health;
            Knockback = knockback;
            Presentation = presentation;
        }

        public CharacterController Controller { get; }
        public EnemyMovementModule MovementModule { get; }
        public EnemyDetectionModule DetectionModule { get; }
        public IActorCombatRequestReceiver CombatRequestReceiver { get; }
        public IActorCombatTacticalState CombatTacticalState { get; }
        public IActorCombatModifierReceiver CombatModifierReceiver { get; }
        public IEnemyCombatProfileReceiver CombatProfileReceiver { get; }
        public EnemyAnimationModule AnimationModule { get; }
        public IActorHealthState Health { get; }
        public IActorKnockbackController Knockback { get; }
        public EnemyActorPresentationReferences Presentation { get; }

        public static EnemyActorRuntimeReferences Resolve(GameObject owner)
        {
            return new EnemyActorRuntimeReferences(
                owner,
                owner.GetComponent<CharacterController>(),
                owner.GetComponent<EnemyMovementModule>(),
                owner.GetComponent<EnemyDetectionModule>(),
                owner.GetComponent<IActorCombatRequestReceiver>(),
                owner.GetComponent<IActorCombatTacticalState>(),
                owner.GetComponent<IActorCombatModifierReceiver>(),
                owner.GetComponent<IEnemyCombatProfileReceiver>(),
                owner.GetComponent<EnemyAnimationModule>(),
                owner.GetComponent<IActorHealthState>(),
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

    }
}
