using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;

namespace NeonBlack.Gameplay.Characters
{
    public interface IPawnMotor
    {
        void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile);
    }
}
