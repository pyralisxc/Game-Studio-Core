using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Features.Combat
{
    public interface IProjectileRuntimeBody
    {
        void Launch(ProjectileSpawnCommand command, IHitPauseSink hitPauseSink = null, ICameraShakeSink cameraShakeSink = null);
    }
}
