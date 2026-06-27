using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;

namespace NeonBlack.Gameplay.Modules.Character
{
    public interface IPawnMotor
    {
        void ApplyMovementProfile(PawnProfileApplicationContext context, PawnMovementProfile movementProfile);
    }
}
