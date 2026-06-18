using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonBlack.Gameplay.Core.Config
{
    /// <summary>
    /// Wraps InputActionAsset so each game adapter can supply its own bindings.
    /// </summary>
    [AuthoringContract(
        Capability = AuthoringCapability.Input,
        Relevance = "Defines participant-specific input overrides (e.g., custom controller bindings).",
        Axioms = AuthoringWorldAxiom.None,
        ProfileType = typeof(InputConfig),
        AssignmentFields = new[] { nameof(actions) },
        FirstProof = "Verify that the assigned InputActionAsset is loaded by the input system.",
        NativeSetup = new[] { "Assign your .inputactions asset." },
        ExpertAdvice = "Assign an InputActionAsset that defines the gameplay controls for this lane. Use unique InputConfigs for players vs AI if specific binding overrides are needed.",
        DocumentationURL = "https://docs.neonblack.com/pyralis/input"
    )]
[CreateAssetMenu(menuName = "NeonBlack/Core/Input Config", fileName = "InputConfig")]
public class InputConfig : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<PyralisRuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (actions == null)
                yield return PyralisRuntimeValidationIssue.Required(
                    "Input Action Asset is missing.",
                    nameof(actions),
                    nameof(InputConfig));
        }

        public InputActionAsset actions;
    }
}
