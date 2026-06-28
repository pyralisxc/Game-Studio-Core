using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Participants;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Modules.Traversal
{
    public sealed partial class Pawn3DTraversalComponent
    {
        private void ApplySerializedTraversalProfile()
        {
            if (traversalProfile == null)
                return;

            traversalProfile.Sanitize();
            ApplyTraversalProfile(
                new PawnProfileApplicationContext(gameObject, ResolvePawnDefinition(), ResolveParticipant()),
                traversalProfile);
        }

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

        private ParticipantHandle ResolveParticipant()
        {
            return GetComponent<IPawnParticipantStateReader>()?.Participant;
        }

        private PawnDefinition ResolvePawnDefinition()
        {
            return ResolveParticipant()?.PawnDefinition;
        }
    }
}
