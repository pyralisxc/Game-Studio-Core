using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Characters
{
    public interface IPawnTraversalModule
    {
        void ApplyTraversalProfile(PawnProfileApplicationContext context, PawnTraversalProfile traversalProfile);
    }
}
