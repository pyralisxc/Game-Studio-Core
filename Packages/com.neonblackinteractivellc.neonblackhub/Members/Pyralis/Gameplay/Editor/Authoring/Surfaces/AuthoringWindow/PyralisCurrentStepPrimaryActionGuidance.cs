using NeonBlack.Gameplay.Characters;
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

            string fallback = currentStep != null
                && (currentStep.EvidenceState == PyralisAuthoringGraphEvidenceState.Missing
                    || currentStep.EvidenceState == PyralisAuthoringGraphEvidenceState.Blocked)
                ? "Review the current graph step and validation evidence next."
                : "No one-click setup action is needed for this selection.";
            return new PyralisPrimaryActionGuidance(string.Empty, MessageType.None, fallback);
        }

        private static bool IsSceneSupportObject(GameObject gameObject)
        {
            return gameObject.GetComponent<UnityEngine.Camera>() != null
                || gameObject.GetComponent<Light>() != null
                || gameObject.GetComponentInParent<UnityEngine.Camera>() != null;
        }
    }
}
