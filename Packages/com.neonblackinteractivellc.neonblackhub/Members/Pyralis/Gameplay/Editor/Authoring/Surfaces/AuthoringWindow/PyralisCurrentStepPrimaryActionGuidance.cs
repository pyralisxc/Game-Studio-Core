using NeonBlack.Gameplay.Characters;
using NeonBlack.Gameplay.Data.Definitions;
using NeonBlack.Gameplay.Data.Profiles;
using NeonBlack.Gameplay.Editor.Inspectors;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonBlack.Gameplay.Editor
{
    internal readonly struct PyralisPrimaryActionGuidance
    {
        public PyralisPrimaryActionGuidance(string message, MessageType messageType, string detail)
        {
            Message = message ?? string.Empty;
            MessageType = messageType;
            Detail = detail ?? string.Empty;
        }

        public string Message { get; }
        public MessageType MessageType { get; }
        public string Detail { get; }
    }

    internal static class PyralisCurrentStepPrimaryActionGuidance
    {
        public static PyralisPrimaryActionGuidance Build(
            Object selection,
            PyralisAuthoringCurrentStepGraphRow currentStep = null)
        {
            if (currentStep != null && currentStep.HasNode)
            {
                if (currentStep.NativeAction.HasValue)
                    return new PyralisPrimaryActionGuidance(string.Empty, MessageType.None, string.Empty);

                return new PyralisPrimaryActionGuidance(
                    string.Empty,
                    MessageType.None,
                    !string.IsNullOrWhiteSpace(currentStep.Detail)
                        ? currentStep.Detail
                        : "Use Map for topology, Validate for the full issue list, and the Inspector for field-level edits.");
            }

            if (selection == null)
            {
                return new PyralisPrimaryActionGuidance(
                    "Native first step: right-click in Hierarchy, choose Create Empty, name it Gameplay Root, then select it and use Inspector -> Add Component search for GameplaySessionBootstrap. Keep this window open while you do it.",
                    MessageType.Info,
                    "After that, select Gameplay Root so Overview can switch from route discovery to the setup map: SessionDefinition, participants, pawn prefab, spawn points, input, and camera bounds.");
            }

            if (selection is GameObject selectedGameObject && selectedGameObject.GetComponent<GameplaySessionBootstrap>() == null)
            {
                if (IsSceneSupportObject(selectedGameObject))
                {
                    return new PyralisPrimaryActionGuidance(
                        $"Native path: keep `{selectedGameObject.name}` as scene support. In the Hierarchy, right-click -> Create Empty, name it Gameplay Root, then select Gameplay Root and use Inspector -> Add Component search for GameplaySessionBootstrap.",
                        MessageType.Info,
                        "After Gameplay Root exists, return to camera, art, lights, and playfield objects as guided setup steps. The camera route should be wired through Camera Root with CinemachineCameraRigController, not by deleting Main Camera or turning it into the session root.");
                }

                return new PyralisPrimaryActionGuidance(
                    $"Native path: keep `{selectedGameObject.name}` selected, then use Inspector -> Add Component search for GameplaySessionBootstrap. Add PyralisGameplayLifetimeScope next if you want the composition root visible before Play Mode.",
                    MessageType.Info,
                    "This is still before asset wiring. Once GameplaySessionBootstrap is on the object, Overview will promote it to the active setup and guide SessionDefinition, participants, pawn prefab, spawn points, input, and camera bounds.");
            }

            GameplaySessionBootstrap bootstrap = PyralisAuthoringSetupContextResolver.GetSelectedBootstrap(selection);
            SessionDefinition session = PyralisAuthoringSetupContextResolver.GetSelectedSession(selection, bootstrap);
            GameModeDefinition mode = PyralisAuthoringSetupContextResolver.GetSelectedMode(selection, session);
            GameSetupProfile setupProfile = PyralisAuthoringSetupContextResolver.GetSelectedSetupProfile(selection, mode);
            PyralisAuthoringRouteDescriptor route = PyralisAuthoringRouteDescriptor.Build(setupProfile, session, mode);

            if (bootstrap != null && session == null)
            {
                return new PyralisPrimaryActionGuidance(
                    "Native path: in the Project window, choose or create a project-owned setup folder for this proof, separate from imported art folders, then right-click in that folder and choose Create -> NeonBlack -> Definitions -> Session Definition. Drag that asset into GameplaySessionBootstrap > Session Definition in the Inspector, or click the field's object picker circle and double-click the asset.",
                    MessageType.Info,
                    "This keeps folderbase and assignment ownership visible: check the Project content pane/breadcrumb first, because Unity creates the asset in the active Project folder. Keep imported art in its art folder, keep Pyralis setup definitions/profiles in the proof setup folder, and use the Inspector to show exactly which field owns the link.");
            }

            if (session != null && mode == null)
            {
                return new PyralisPrimaryActionGuidance(
                    "Native path: in the Project window, create a Game Mode Definition asset. Then select/open the SessionDefinition asset and assign its Default Game Mode field by dragging the asset there or using the field's object picker circle.",
                    MessageType.Info,
                    "The Authoring Window explains the next link; the SessionDefinition Inspector remains the field-level source of truth.");
            }

            if (mode != null && setupProfile == null)
            {
                return new PyralisPrimaryActionGuidance(
                    "Native path: in the Project window, create a Game Setup Profile asset. Then select/open the GameModeDefinition asset and assign its Setup Profile field by dragging the asset there or using the field's object picker circle.",
                    MessageType.Info,
                    "Create or choose the setup profile intentionally, then wire it through the GameModeDefinition Inspector.");
            }

            if (setupProfile != null && !route.HasAssignedPatterns)
            {
                return new PyralisPrimaryActionGuidance(
                    "Native path: select/open the GameSetupProfile asset and express the capability ingredients that match the Intent. Optional Runtime Pattern contracts can enrich validation; create a new Runtime Pattern Definition only when the existing capability language cannot describe the route.",
                    MessageType.Info,
                    "Runtime capabilities name the route intent before participant and pawn wiring becomes meaningful. For a 1P movement proof, start with Character Pawn Gameplay.");
            }

            if (setupProfile != null && !route.HasValidPatterns)
            {
                return new PyralisPrimaryActionGuidance(
                    "Native path: select the assigned Runtime Pattern Definition and clear its Inspector validation issues before adding participants. Fill Pattern Id, Display Name, Description, Setup Notes, Capability Family, Participant Embodiment, and Supported Control Surfaces.",
                    MessageType.Warning,
                    "A pattern slot is assigned, but Pyralis cannot trust it as the route source of truth until its metadata is real. Do this in the pattern Inspector, then return to the setup root.");
            }

            if (session != null && (session.defaultParticipants == null || session.defaultParticipants.Length == 0))
            {
                return new PyralisPrimaryActionGuidance(
                    "Native path: create a Participant Definition asset in the Project window, configure player/input fields in its Inspector, then select/open the SessionDefinition asset, add a Default Participants slot, and drag the asset there or use the slot's object picker circle.",
                    MessageType.Info,
                    "Participants are design-owned. Use the Inspector so player, seat, input, hand, faction, camera, cursor, or pawn intent stays explicit.");
            }

            if (route.RequiresPawn && !string.IsNullOrWhiteSpace(route.ParticipantPawnIssue))
            {
                return new PyralisPrimaryActionGuidance(
                    GetPawnIssuePrimaryAction(route.ParticipantPawnIssueKind),
                    MessageType.Info,
                    "The selected intent is pawn-backed. Select the participant, pawn definition, or pawn prefab named in Current Step when you need the exact Inspector field.");
            }

            PyralisSetupFlowStep nextStep = ResolveStep(bootstrap);
            PyralisPrimaryActionGuidance? stepGuidance = BuildStepGuidance(nextStep);
            if (stepGuidance.HasValue)
                return stepGuidance.Value;

            if (selection is ParticipantDefinition participant && participant.defaultPawn == null)
            {
                if (route.RequiresPawn)
                {
                    return new PyralisPrimaryActionGuidance(
                        "Native path: create a Pawn Definition asset in the Project window, then make the pawn prefab as a normal Unity prefab. For a 2D movement proof, start from a Hierarchy GameObject and add PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator. Motor2D adds the required movement and presentation siblings. Add Unity PlayerInput when you want explicit local keyboard/gamepad ownership, then assign the Input Actions asset there. Drag the finished prefab into PawnDefinition > Pawn Prefab, then assign the Pawn Definition into ParticipantDefinition > Default Pawn by drag/drop or the object picker circle.",
                        MessageType.Info,
                        "Keep pawn definition, prefab, art, input, movement, and presentation choices explicit in Unity. Do not add a separate 2D Player Input Handler when Motor2DInputAdapter is already present.");
                }

                return new PyralisPrimaryActionGuidance(
                    string.Empty,
                    MessageType.None,
                    "Pawn is optional for this route; leave it empty for seats, hands, factions, camera, cursor, menu, or board-driven participants.");
            }

            string fallback = currentStep != null
                && (currentStep.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing
                    || currentStep.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                ? "Review the current graph step and validation evidence next."
                : "No one-click setup action is needed for this selection.";
            return new PyralisPrimaryActionGuidance(string.Empty, MessageType.None, fallback);
        }

        private static PyralisSetupFlowStep ResolveStep(GameplaySessionBootstrap bootstrap)
        {
            if (bootstrap == null)
                return null;

            PyralisSetupFlowReport report = PyralisSetupFlowValidator.BuildReport(bootstrap);
            return report != null ? report.FirstBlockingStep : null;
        }

        private static PyralisPrimaryActionGuidance? BuildStepGuidance(PyralisSetupFlowStep step)
        {
            if (step == null)
                return null;

            switch (step.StepId)
            {
                case PyralisSetupFlowStepId.AssignSpawnPoints:
                    return new PyralisPrimaryActionGuidance(
                        "Native path: right-click in Hierarchy -> Create Empty, name it SpawnPoint_1, position it, select Gameplay Root, expand GameplaySessionBootstrap > Spawn Points, click + to create Element 0, then drag SpawnPoint_1 from the Hierarchy into that Transform slot.",
                        MessageType.Info,
                        "Unity list fields usually need an element slot before a drag can land. The guide should keep this explicit so beginners do not bounce off an empty list.");
                case PyralisSetupFlowStepId.AssignPlayerInputManager:
                    return new PyralisPrimaryActionGuidance(
                        "Native path: only add Unity PlayerInputManager for local join. For a 1P proof, select/open the SessionDefinition asset, set Max Participants to 1, and leave Bootstrap > Player Input Manager empty. For local join, add PlayerInputManager, assign a dedicated PlayerInput prefab, configure Join Behavior/Input Actions, then drag it into Bootstrap > Player Input Manager.",
                        MessageType.Warning,
                        "PlayerInputManager is not the pawn spawner for ordinary 1P proofs. It is a local-join input surface, and Unity requires a Player Prefab when joining is enabled.");
                case PyralisSetupFlowStepId.AssignCameraRig:
                    return new PyralisPrimaryActionGuidance(
                        "Native path:\n1. Keep or create exactly one enabled physical Unity Camera for this shared proof, usually the default Main Camera.\n2. Hierarchy right-click -> Create Empty, name it Camera Root.\n3. Add CinemachineCameraRigController.\n4. Create GameObject -> Cinemachine -> Cinemachine Camera; this creates a separate Cinemachine Camera GameObject and usually adds Cinemachine Brain to the physical Main Camera.\n5. Verify Main Camera still has the MainCamera tag and Cinemachine Brain. Add the Brain manually only if it is missing.\n6. Assign the Cinemachine Camera component to Shared Camera Behaviour and the physical Main Camera to Target Camera.\n7. Disable or remove accidental extra physical Camera objects only when they were created by mistake; keep intentional overlay, split-screen, minimap, or render-texture cameras.\n8. Drag Camera Root into GameplaySessionBootstrap > Camera Rig Controller.",
                        MessageType.Warning,
                        "For a 2D camera/bounds proof, set Target Camera > Camera > Projection to Orthographic or use an orthographic CameraRigProfile. Treat this as a Proof Enhancer unless the selected intent is explicitly testing framing, bounds, hazards, pickups, or camera-aware spawning.");
                default:
                    return null;
            }
        }

        private static string GetPawnIssuePrimaryAction(PyralisParticipantPawnIssueKind issueKind)
        {
            switch (issueKind)
            {
                case PyralisParticipantPawnIssueKind.MissingPawnDefinition:
                    return "Native path: create a Pawn Definition asset in the Project window, create or choose a pawn prefab with the lane stack it needs, then assign the Pawn Definition to the participant named in Current Step.";
                case PyralisParticipantPawnIssueKind.MissingPawnPrefab:
                    return "Native path: create or choose a pawn prefab, then build its lane stack on the prefab root. For a 2D pawn, add PawnRoot, Motor2D, Motor2DInputAdapter, SpriteRenderer, and Animator; Motor2D adds Pawn2DMovementComponent and Pawn2DPresentationComponent. Add Unity PlayerInput when you want explicit local keyboard/gamepad ownership and assign the same Input Actions asset used by the InputProfile. Assign SpriteRenderer > Sprite before Play Mode, keep one input adapter, then drag the prefab into PawnDefinition > Pawn Prefab.";
                case PyralisParticipantPawnIssueKind.MissingPawnRoot:
                    return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add PawnRoot on the root GameObject. PawnRoot is the composition root that applies PawnDefinition profiles at runtime.";
                case PyralisParticipantPawnIssueKind.MissingMotor:
                    return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add the lane motor. For a Sprite2D proof, add Motor2D; it supplies the IPawnMotor surface and adds required 2D movement/presentation siblings.";
                case PyralisParticipantPawnIssueKind.MissingPresentation:
                    return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add the lane presentation component. For a Sprite2D proof, Motor2D adds Pawn2DPresentationComponent; add SpriteRenderer and Animator on the same root, then drag the sprite or Aseprite asset from the Project window onto SpriteRenderer > Sprite.";
                case PyralisParticipantPawnIssueKind.MissingInputModule:
                    return "Native path: select the pawn prefab named in Current Step and use Inspector -> Add Component to add the input adapter for the lane. For a Sprite2D proof, add Motor2DInputAdapter so InputProfile actions reach Motor2D. Add Unity PlayerInput only when you want explicit local keyboard/gamepad ownership; do not add a separate 2D Player Input Handler next to Motor2DInputAdapter.";
                default:
                    return "Native path: inspect the pawn route item named in Current Step and clear its Inspector validation issue before entering Play Mode.";
            }
        }

        private static bool IsSceneSupportObject(GameObject gameObject)
        {
            return gameObject.GetComponent<UnityEngine.Camera>() != null
                || gameObject.GetComponent<Light>() != null
                || gameObject.GetComponentInParent<UnityEngine.Camera>() != null;
        }
    }
}
