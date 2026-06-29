using UnityEngine;

namespace NeonBlack.Gameplay.Core.Contracts
{
    public interface IBillboardFacingController
    {
        void ConfigureBillboardFacing(
            Transform target,
            Transform mirroredVisualRoot,
            SpriteRenderer spriteRenderer,
            Camera camera,
            bool spriteDefaultFacesRight);

        void ApplyFacing(bool facingRight);

        void SetCameraOverride(Camera camera);
    }
}
