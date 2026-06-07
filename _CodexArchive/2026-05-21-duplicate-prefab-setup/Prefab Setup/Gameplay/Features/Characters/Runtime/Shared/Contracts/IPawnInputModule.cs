using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Characters;

namespace NeonBlack.Gameplay.Characters
{
    /// <summary>
    /// Applies participant or pawn-owned input authoring to a runtime input surface.
    /// </summary>
    public interface IPawnInputModule
    {
        void ApplyInputProfile(PawnProfileApplicationContext context, InputProfile inputProfile);
    }
}
