using System.Collections.Generic;
using NeonBlack.Gameplay.Core.Contracts;
using UnityEngine;
using Pys.Authoring.Contracts;

namespace NeonBlack.Gameplay.Data.Profiles
{
    /// <summary>
    /// Shared traversal authoring profile for jumps, dodge, and climb-like features.
    /// </summary>
    [AuthoringContract(
        Category = "Movement",
        CapabilityPath = "Movement/Traversal/Pawn Traversal Profile",
        Surface = AuthoringSurface.Profile,
        Summary = "Defines the jumping, dodging, and climbing capabilities of a pawn.",
        DocumentationUrl = "https://docs.neonblack.com/pyralis/traversal",
        SetupSteps = new[] { "Assign to a PawnDefinition.", "Enable desired traversal features." },
        SuccessChecks = new[] { "Verify the pawn can jump and crouch correctly in-game." },
        Tags = new[] { "capability:Movement", "runtime:CharacterPawnGameplay" },
        Selectable = false
    )]
[CreateAssetMenu(menuName = "NeonBlack/Profiles/Pawn Traversal Profile", fileName = "PawnTraversalProfile", order = -50)]
    public class PawnTraversalProfile : ScriptableObject, IRuntimeValidationProvider
    {
        public IEnumerable<RuntimeValidationIssue> GetRuntimeValidationIssues()
        {
            if (allowJump && jumpHeight <= 0f) yield return RuntimeValidationIssue.Required("Jump is allowed but Jump Height is <= 0.");
            if (allowDodge && dodgeDistance <= 0f) yield return RuntimeValidationIssue.Required("Dodge is allowed but Dodge Distance is <= 0.");
        }

        public bool allowJump = true;
        public bool allowClimb = false;
        public bool allowHang = false;
        public bool allowDodge = false;
        public bool allowCrouch = true;
        public float jumpHeight = 3f;
        public float gravity = -20f;
        public float dodgeDistance = 3f;
        public float dodgeDuration = 0.4f;
        public float dodgeCooldown = 0.8f;
        public float climbCooldown = 1.2f;

        public void Sanitize()
        {
            jumpHeight = Mathf.Max(0f, jumpHeight);
            dodgeDistance = Mathf.Max(0f, dodgeDistance);
            dodgeDuration = Mathf.Max(0.01f, dodgeDuration);
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown);
            climbCooldown = Mathf.Max(0f, climbCooldown);
        }

        private void OnValidate()
        {
            Sanitize();
        }
    }
}
