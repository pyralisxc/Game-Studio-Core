using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    public sealed partial class Pawn3DTraversalComponent
    {
        public void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile profile)
        {
            if (profile == null)
                return;

            allowClimb = profile.allowClimb;
            allowHang = profile.allowHang;
            climbCooldown = profile.climbCooldown;
            if (EnsureDependencies())
                _movement.ApplyTraversalProfile(profile);
        }
    }
}
