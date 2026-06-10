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
        NativeSetup = new[] { "Create Asset" }
    )]
    [CreateAssetMenu(menuName = "NeonBlack/Core/Input Config", fileName = "InputConfig")]
public class InputConfig : ScriptableObject
    {
        public InputActionAsset actions;
    }
}
