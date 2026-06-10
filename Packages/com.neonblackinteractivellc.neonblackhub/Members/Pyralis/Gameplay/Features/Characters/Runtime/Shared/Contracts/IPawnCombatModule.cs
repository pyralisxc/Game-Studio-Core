using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Characters
{
    using NeonBlack.Gameplay.Core.Contracts;

    [AuthoringContract(
        Capability = AuthoringCapability.Combat, 
        Relevance = "Handles pawn-specific combat logic, weapon state, and targeting.", 
        Axioms = AuthoringWorldAxiom.None,
        FirstProof = "Verify that ApplyCombatProfile is called when the pawn is initialized.",
        NativeSetup = new[] { "Implement interface in a combat module" }
    )]
    public interface IPawnCombatModule
{
        void ApplyCombatProfile(PawnProfileApplicationContext context, PawnCombatProfile combatProfile);
    }
}
