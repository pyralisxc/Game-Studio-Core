using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    internal sealed class EnemyActorPresentationReferences
    {
        private EnemyActorPresentationReferences(
            IActorAnimationController animationDriver,
            IBillboardFacingController billboardFacing,
            SpriteRenderer spriteRenderer)
        {
            AnimationDriver = animationDriver;
            BillboardFacing = billboardFacing;
            SpriteRenderer = spriteRenderer;
        }

        public IActorAnimationController AnimationDriver { get; }
        public IBillboardFacingController BillboardFacing { get; }
        public SpriteRenderer SpriteRenderer { get; }
        public static ActorPresentationMode DefaultPresentationMode => ActorPresentationMode.Billboard2_5D;
        public ActorPresentationMode PresentationMode =>
            AnimationDriver != null ? AnimationDriver.PresentationMode : DefaultPresentationMode;

        public static EnemyActorPresentationReferences Resolve(GameObject owner)
        {
            return new EnemyActorPresentationReferences(
                owner != null ? owner.GetComponent<IActorAnimationController>() : null,
                owner != null ? owner.GetComponent<IBillboardFacingController>() : null,
                owner != null ? owner.GetComponentInChildren<SpriteRenderer>() : null);
        }

        public void ConfigureBillboard(
            Transform ownerTransform,
            Transform visualRoot,
            Camera presentationCamera,
            bool spriteDefaultFacesRight)
        {
            if (BillboardFacing == null)
                return;

            BillboardFacing.ConfigureBillboardFacing(
                visualRoot != null ? visualRoot : ownerTransform,
                visualRoot,
                SpriteRenderer,
                presentationCamera,
                spriteDefaultFacesRight);
        }

        public void SetPresentationCamera(Camera camera)
        {
            BillboardFacing?.SetCameraOverride(camera);
        }
    }
}
