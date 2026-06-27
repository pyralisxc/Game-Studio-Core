using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Modules.Character;

namespace NeonBlack.Gameplay.Modules.Character
{
    /// <summary>
    /// Applies participant or pawn-owned input authoring to a runtime input surface.
    /// </summary>
    public interface IPawnInputModule
    {
        void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile);
    }
}
