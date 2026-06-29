using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Modules.Combat;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Combat
{
    internal sealed class PawnCombat2DRuntimeReferences
    {
        public IActorCombatMovementState Motor { get; private set; }
        public IActorAnimationController AnimationDriver { get; private set; }
        public IActorCombatResultReceiver[] CombatResultReceivers { get; private set; }
        public HealthComponent Health { get; private set; }
        public IActorFeedbackPublisher FeedbackPublisher { get; private set; }
        public ProjectileLauncher2D ProjectileLauncher { get; private set; }

        public static PawnCombat2DRuntimeReferences Resolve(GameObject owner, ProjectileLauncher2D authoredProjectileLauncher)
        {
            PawnCombat2DRuntimeReferences references = new PawnCombat2DRuntimeReferences();
            if (owner == null)
                return references;

            references.Motor = owner.GetComponent<IActorCombatMovementState>();
            references.AnimationDriver = owner.GetComponent<IActorAnimationController>();
            references.CombatResultReceivers = owner.GetComponents<IActorCombatResultReceiver>();
            references.Health = owner.GetComponent<HealthComponent>();
            references.FeedbackPublisher = owner.GetComponent<IActorFeedbackPublisher>();
            references.ProjectileLauncher = ResolveProjectileLauncherInternal(owner.transform, authoredProjectileLauncher);
            return references;
        }

        public ProjectileLauncher2D ResolveProjectileLauncher(Transform ownerTransform, ProjectileLauncher2D authoredProjectileLauncher)
        {
            if (authoredProjectileLauncher != null)
            {
                ProjectileLauncher = authoredProjectileLauncher;
                return ProjectileLauncher;
            }

            if (ProjectileLauncher != null)
                return ProjectileLauncher;

            ProjectileLauncher = ResolveProjectileLauncherInternal(ownerTransform, null);
            return ProjectileLauncher;
        }

        private static ProjectileLauncher2D ResolveProjectileLauncherInternal(Transform ownerTransform, ProjectileLauncher2D authoredProjectileLauncher)
        {
            if (authoredProjectileLauncher != null)
                return authoredProjectileLauncher;

            if (ownerTransform == null)
                return null;

            ProjectileLauncher2D launcher = ownerTransform.GetComponentInParent<ProjectileLauncher2D>();
            if (launcher == null)
                launcher = ownerTransform.GetComponentInChildren<ProjectileLauncher2D>();

            return launcher;
        }
    }
}
