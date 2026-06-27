using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Presentation.Animation;
using NeonBlack.Gameplay.Presentation.Visuals;
using UnityEngine;

namespace NeonBlack.Gameplay.Modules.Enemies
{
    internal sealed class EnemyActorPresentationReferences
    {
        private EnemyActorPresentationReferences(
            ActorAnimationDriver animationDriver,
            BillboardFacing3D billboardFacing,
            SpriteRenderer spriteRenderer)
        {
            AnimationDriver = animationDriver;
            BillboardFacing = billboardFacing;
            SpriteRenderer = spriteRenderer;
        }

        public ActorAnimationDriver AnimationDriver { get; }
        public BillboardFacing3D BillboardFacing { get; }
        public SpriteRenderer SpriteRenderer { get; }
        public static ActorPresentationMode DefaultPresentationMode => ActorPresentationMode.Billboard2_5D;
        public ActorPresentationMode PresentationMode =>
            AnimationDriver != null ? AnimationDriver.PresentationMode : DefaultPresentationMode;

        public static EnemyActorPresentationReferences Resolve(GameObject owner)
        {
            return new EnemyActorPresentationReferences(
                owner != null ? owner.GetComponent<ActorAnimationDriver>() : null,
                owner != null ? owner.GetComponent<BillboardFacing3D>() : null,
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
    }
}
