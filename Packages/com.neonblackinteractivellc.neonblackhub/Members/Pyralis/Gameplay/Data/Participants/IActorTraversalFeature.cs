using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Types.Input;
using NeonBlack.Gameplay.Data.Profiles;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Participants
{
    [AuthoringContract(
        StableId = "feature.actor.traversal.3d",
        Category = "Traversal",
        Surface = AuthoringSurface.Profile,
        Summary = "Optional traversal capability contract for specialized world movement like climbing and hanging.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/traversal",
        RequiredFields = new[]
        {
            "Pawn3DTraversalComponent.traversalProfile",
            "InputProfile.gameplayActions"
        },
        RequiredInterfaces = new[] { typeof(IActorTraversalFeature) },
        SetupSteps = new[]
        {
            "create PawnTraversalProfile",
            "add Pawn3DTraversalComponent to the pawn root",
            "assign PawnTraversalProfile",
            "keep Motor3D, Pawn3DMovementComponent, and Pawn3DTraversalComponent as explicit pawn siblings",
            "bind Jump or Interact in InputProfile"
        },
        SuccessChecks = new[] { "Press Jump or Interact when near a valid ClimbZone and verify the actor transition." },
        Tags = new[] { "capability:Traversal", "lane:Traversal" },
        Selectable = false
    )]
    public interface IActorTraversalFeature
    {
        float ShimmyVelocityX { get; }
        void ProbeTraversal();
        bool HandleHangFrame(FrameInput frameInput);
        void TriggerClimbUp();
        void TryLedgeGrab(IClimbZone zone, float maxVelocityY = 0f);
        void SetClimbZone(IClimbZone zone);
        void ClearClimbZone();
    }
}
