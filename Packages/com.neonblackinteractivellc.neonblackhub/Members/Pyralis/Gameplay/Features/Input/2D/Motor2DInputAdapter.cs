using NeonBlack.Gameplay.Core.Contracts;
using NeonBlack.Gameplay.Features.Characters;
using UnityEngine;

namespace NeonBlack.Gameplay.Features.Input
{
/// <summary>
/// Supported neutral input adapter for <see cref="Motor2D"/>.
/// Use this component on player-controlled 2D pawns.
/// It translates Unity Input System actions into movement direction for the 2D motor.
/// </summary>
[AuthoringContract(
    Capability = AuthoringCapability.Input | AuthoringCapability.Movement, 
    CapabilityPath = "Core Setup/Input/2D Motor Input Adapter",
    Relevance = "Primary input module for 2D characters. Translates participant input into Motor2D movement.",
    Axioms = AuthoringWorldAxiom.Dimensions2D,
    RequiredComponents = new[] { typeof(Motor2D) },
    RoleTags = new[] { "ParticipantInputConsumer", "Motor2DInput", "PawnInputAdapter" },
    FirstProof = "Verify that player input moves the pawn in 2D space and respects the active InputProfile."
)]
[AddComponentMenu("NeonBlack/Gameplay/Input/2D Motor Input Adapter")]
public class Motor2DInputAdapter : PlayerInputHandler
{
    // Motor2DInputAdapter inherits the Participant-ready flow from PlayerInputHandler.
    // It provides the concrete component identity for 2D pawn composition.
}
}
