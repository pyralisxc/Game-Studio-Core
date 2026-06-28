using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Core.Types.Animation;
using NeonBlack.Gameplay.Core.Types.Input;
using NeonBlack.Gameplay.Data.Profiles;

namespace NeonBlack.Gameplay.Data.Participants
{
    [AuthoringContract(
        ModuleId = "actor.traversal.3d",
        Capability = AuthoringCapability.Traversal,
        Lane = "Traversal",
        Relevance = "Optional traversal capability contract for specialized world movement like climbing and hanging.",
        ProfileType = typeof(PawnTraversalProfile),
        RequiredInterfaces = new[] { typeof(IActorTraversalFeature) },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.ThirdPerson3D },
        UnsupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        UnsupportedLaneMessage = "Sprite2D actors should use the 2D movement or top-down hop traversal path instead of the 3D traversal module.",
        ConsumedRoles = new[] { "Jump", "Interact" },
        NativeSetup = new[]
        {
            "create PawnTraversalProfile",
            "add Pawn3DTraversalComponent to the pawn root",
            "assign PawnTraversalProfile",
            "keep Motor3D, Pawn3DMovementComponent, and Pawn3DTraversalComponent as explicit pawn siblings",
            "bind Jump or Interact in InputProfile"
        },
        Proof = "Press Jump or Interact when near a valid ClimbZone and verify the actor transition.",
        ProofTargetId = "proof.npc-enemy-behavior",
        AssignmentFields = new[]
        {
            "Pawn3DTraversalComponent.traversalProfile",
            "InputProfile.gameplayActions"
        },
        ExpertAdvice = "Use this direct pawn sibling seam to let Motor3D coordinate optional shimmy, climb, or hang behavior without importing the concrete traversal implementation.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/traversal"
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
