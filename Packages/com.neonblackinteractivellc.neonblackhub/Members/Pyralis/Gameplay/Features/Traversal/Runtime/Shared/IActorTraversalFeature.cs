using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Features.Characters;
using NeonBlack.Gameplay.Features.Composition;
using NeonBlack.Gameplay.Presentation.Animation;

namespace NeonBlack.Gameplay.Features.Traversal
{
    [AuthoringContract(
        ModuleId = "actor.traversal.3d",
        Capability = AuthoringCapability.Movement,
        Lane = "Traversal",
        ProfileType = typeof(PawnTraversalProfile),
        RequiredInterfaces = new[] { typeof(IFeatureModuleRuntime), typeof(IActorTraversalFeature) },
        SupportedLanes = new[] { ActorPresentationMode.Billboard2_5D, ActorPresentationMode.Rigged3D },
        UnsupportedLanes = new[] { ActorPresentationMode.Sprite2D },
        UnsupportedLaneMessage = "Sprite2D actors should use the 2D movement or top-down hop traversal path instead of the 3D traversal module.",
        ConsumedRoles = new[] { "Jump", "Interact" },
        NativeSetup = new[]
        {
            "create PawnTraversalProfile",
            "create FeatureModuleDefinition",
            "assign runtime prefab with PawnTraversalFeatureRuntime3D",
            "assign profile asset",
            "add module to PawnDefinition.featureModules",
            "bind Jump or Interact in InputProfile"
        },
        FirstProof = "proof.npc-enemy-behavior",
        AssignmentFields = new[]
        {
            "FeatureModuleDefinition.moduleId",
            "FeatureModuleDefinition.runtimePrefab",
            "FeatureModuleDefinition.profileAsset",
            "PawnDefinition.featureModules",
            "InputProfile.gameplayActions"
        },
        CustomizationMoments = new[]
        {
            "PawnTraversalProfile.allowClimb",
            "PawnTraversalProfile.allowHang",
            "PawnTraversalProfile.allowDodge",
            "PawnTraversalProfile.jumpHeight",
            "Pawn3DTraversalComponent traversal probes and climb zones"
        }
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
