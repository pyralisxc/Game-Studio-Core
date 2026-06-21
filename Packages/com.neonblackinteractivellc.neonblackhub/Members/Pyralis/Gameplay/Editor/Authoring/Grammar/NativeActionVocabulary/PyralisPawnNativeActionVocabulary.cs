using NeonBlack.Gameplay.Core.Contracts;

namespace NeonBlack.Gameplay.Editor.Inspectors
{
    public static class PyralisPawnNativeActionVocabulary
    {
        public static PyralisAuthoringNativeAction GetNativeAction(PyralisParticipantPawnIssueKind issueKind)
        {
            return GetNativeAction(issueKind, RuntimeCapabilityLaneTag.Mixed);
        }

        public static PyralisAuthoringNativeAction GetNativeAction(
            PyralisParticipantPawnIssueKind issueKind,
            RuntimeCapabilityLaneTag laneTag)
        {
            switch (issueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return PyralisAuthoringNativeActionFactory.CreateAssetAction(
                        "PawnDefinition",
                        "NeonBlack -> Definitions -> Pawn Definition",
                        "the participant points at a PawnDefinition",
                        "assign it to ParticipantDefinition.defaultPawn");
                case PyralisParticipantPawnIssueKind.MissingPawnPrefab:
                    return new PyralisAuthoringNativeAction(
                        "Create or select",
                        PyralisAuthoringActionSurface.Hierarchy,
                        "the pawn prefab root",
                        GetPawnPrefabSetupInstruction(laneTag),
                        "the PawnDefinition has a prefab");
                case PyralisParticipantPawnIssueKind.MissingPawnRoot:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root",
                        "PawnRoot",
                        "Pyralis recognizes the prefab as a pawn actor");
                case PyralisParticipantPawnIssueKind.MissingMotor:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root",
                        GetPawnMotorComponentLabel(laneTag),
                        "movement profiles have a runtime motor to drive");
                case PyralisParticipantPawnIssueKind.MissingPresentation:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root or visual child",
                        GetPawnPresentationComponentLabel(laneTag),
                        "the pawn has visible presentation",
                        "assign a project-owned sprite, prefab visual, or renderer in the presentation fields");
                case PyralisParticipantPawnIssueKind.MissingInputModule:
                    return PyralisAuthoringNativeActionFactory.AddComponentAction(
                        "the pawn prefab root",
                        GetPawnInputComponentLabel(laneTag),
                        "InputProfile actions can reach movement");
                default:
                    return new PyralisAuthoringNativeAction(
                        "Inspect",
                        PyralisAuthoringActionSurface.Inspector,
                        "the participant, PawnDefinition, or pawn prefab",
                        "the field or component named by the validation message",
                        "Assign Participant Pawn is ready");
            }
        }

        private static string GetPawnPrefabSetupInstruction(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "name the GameObject, add PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator, save it as a prefab, then drag the prefab into PawnDefinition > Pawn Prefab. Motor2D adds the required Pawn2DMovementComponent and Pawn2DPresentationComponent siblings. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the InputProfile";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "name the GameObject, add PawnRoot, Pawn3DMovementComponent, Pawn3DInputModule, Pawn3DPresentationComponent, and CharacterController, save it as a prefab, then drag the prefab into PawnDefinition > Pawn Prefab. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the InputProfile";
                default:
                    return "name the GameObject, add PawnRoot plus the lane motor, input, and presentation components, save it as a prefab, then drag the prefab into PawnDefinition > Pawn Prefab. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership, and assign the same Input Actions asset used by the InputProfile";
            }
        }

        private static string GetPawnMotorComponentLabel(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "Motor2D";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "Pawn3DMovementComponent";
                default:
                    return "the lane motor component";
            }
        }

        private static string GetPawnInputComponentLabel(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "Motor2DInputAdapter";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "Pawn3DInputModule";
                default:
                    return "the lane input module";
            }
        }

        private static string GetPawnPresentationComponentLabel(RuntimeCapabilityLaneTag laneTag)
        {
            switch (laneTag)
            {
                case RuntimeCapabilityLaneTag.Sprite2D:
                    return "Pawn2DPresentationComponent";
                case RuntimeCapabilityLaneTag.ThirdPerson3D:
                case RuntimeCapabilityLaneTag.Billboard2_5D:
                    return "Pawn3DPresentationComponent";
                default:
                    return "the lane presentation module";
            }
        }
    }
}
